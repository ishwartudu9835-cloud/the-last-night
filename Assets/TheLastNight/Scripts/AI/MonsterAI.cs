// MonsterAI — "Nishachar". NavMesh-driven finite state machine with sight + hearing.
// States: Idle, Patrol, InvestigateNoise, Search, Chase, Attack, LoseTarget, ReturnToPatrol.
// Design goals: unpredictable (random idles, patrol picks, search points, hiding checks)
// but FAIR (grace period, perception build-up, audible tells, attack wind-up, post-hit stagger).
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace TheLastNight
{
    public enum MonsterState { Idle, Patrol, InvestigateNoise, Search, Chase, Attack, LoseTarget, ReturnToPatrol }

    [RequireComponent(typeof(NavMeshAgent))]
    public class MonsterAI : MonoBehaviour
    {
        [Header("References")]
        public Transform eyes;
        public Transform[] patrolPoints;
        public LayerMask sightBlockers = ~0;

        [Header("Speeds")]
        public float patrolSpeed = 1.6f;
        public float investigateSpeed = 2.8f;
        public float searchSpeed = 2.2f;
        public float chaseSpeed = 4.6f;
        public float returnSpeed = 2.0f;

        [Header("Senses")]
        public float sightRange = 14f;
        public float sightAngle = 110f;
        public float instantSpotDistance = 4.5f;
        public float awarenessToChase = 0.4f;     // seconds of continuous sight needed (fairness)
        public float hearingMultiplier = 1f;

        [Header("Combat")]
        public float attackRange = 2.0f;
        public float attackWindup = 0.45f;
        public float attackDamage = 40f;
        public float staggerAfterHit = 1.5f;

        [Header("Search")]
        public float searchRadius = 8f;
        public Vector2Int searchPointCount = new Vector2Int(3, 5);
        [Range(0f, 1f)] public float hidingCheckChance = 0.25f;

        public MonsterState State { get; private set; }
        public Vector3 LastKnownPosition { get; private set; }
        public float Awareness { get; private set; }   // 0..1, UI/debug

        NavMeshAgent agent;
        MonsterAudio sfx;
        float stateTimer, senseTimer, doorTimer, stuckTimer, seeTime, lastSawPlayerTime = -99f, nextHuntPulse;
        int searchLeft, lastPatrol = -1, prevPatrol = -1;
        bool attackHit, lookingAround;
        HidingSpot suspectSpot;           // spot the player was SEEN entering
        HidingSpot checkingSpot;
        bool hasNoise;
        NoiseEvent pendingNoise;
        readonly Collider[] overlap = new Collider[8];

        PlayerController Target { get { return GameManager.Instance != null ? GameManager.Instance.Player : null; } }
        // Phase 4 (multiplayer): Target becomes "closest perceived player" from a list; noise already carries Source.

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            sfx = GetComponent<MonsterAudio>();
            nextHuntPulse = Random.Range(60f, 100f);
        }

        void OnEnable() { NoiseSystem.OnNoise += OnNoise; }
        void OnDisable() { NoiseSystem.OnNoise -= OnNoise; }

        void Start()
        {
            if (GameManager.Instance != null) GameManager.Instance.Register(this);
            Enter(MonsterState.Idle);
        }

        // ============================================================ update
        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || !gm.IsPlaying) { if (agent.isOnNavMesh) agent.isStopped = true; return; }
            if (agent.isOnNavMesh) agent.isStopped = State == MonsterState.Idle || State == MonsterState.Attack || State == MonsterState.LoseTarget;

            stateTimer -= Time.deltaTime;
            Sense(gm);
            OpenDoorsInTheWay();
            Unstick();

            switch (State)
            {
                case MonsterState.Idle:
                    if (stateTimer <= 0f) Enter(MonsterState.Patrol);
                    break;

                case MonsterState.Patrol:
                    if (Arrived())
                    {
                        if (Random.value < 0.3f) Enter(MonsterState.Idle);
                        else GoToNextPatrol();
                    }
                    if (gm.PlayTime > nextHuntPulse) HuntPulse(gm);
                    break;

                case MonsterState.InvestigateNoise:
                    if (Arrived()) { LastKnownPosition = transform.position; Enter(MonsterState.Search); }
                    break;

                case MonsterState.Search:
                    SearchUpdate();
                    break;

                case MonsterState.Chase:
                    ChaseUpdate();
                    break;

                case MonsterState.Attack:
                    AttackUpdate();
                    break;

                case MonsterState.LoseTarget:
                    if (stateTimer <= 0f) Enter(MonsterState.Search);
                    break;

                case MonsterState.ReturnToPatrol:
                    if (Arrived()) Enter(MonsterState.Patrol);
                    break;
            }
        }

        // ============================================================ states
        void Enter(MonsterState s)
        {
            MonsterState prev = State;
            State = s;
            float variance = Random.Range(0.92f, 1.08f);
            switch (s)
            {
                case MonsterState.Idle:
                    stateTimer = Random.Range(2f, 5f);
                    break;
                case MonsterState.Patrol:
                    agent.speed = patrolSpeed * variance;
                    GoToNextPatrol();
                    break;
                case MonsterState.InvestigateNoise:
                    agent.speed = investigateSpeed * variance;
                    SetDest(pendingNoise.Position);
                    if (sfx != null) sfx.Alert();
                    break;
                case MonsterState.Search:
                    agent.speed = searchSpeed * variance;
                    searchLeft = Random.Range(searchPointCount.x, searchPointCount.y + 1);
                    checkingSpot = null;
                    lookingAround = false;
                    NextSearchPoint();
                    break;
                case MonsterState.Chase:
                    agent.speed = chaseSpeed * variance;
                    if (prev != MonsterState.Attack && sfx != null) sfx.Scream();
                    break;
                case MonsterState.Attack:
                    stateTimer = attackWindup;
                    attackHit = false;
                    if (sfx != null) sfx.AttackWindup();
                    break;
                case MonsterState.LoseTarget:
                    stateTimer = 1.5f;
                    if (sfx != null) sfx.Sniff();
                    break;
                case MonsterState.ReturnToPatrol:
                    agent.speed = returnSpeed;
                    SetDest(NearestPatrolPoint());
                    break;
            }
        }

        void ChaseUpdate()
        {
            var p = Target;
            if (p == null || p.Health.IsDead) { Enter(MonsterState.ReturnToPatrol); return; }

            if (p.IsHidden)
            {
                // Did we see them get in? Then we know exactly where they are.
                if (suspectSpot != null && suspectSpot == p.Hiding) { GoCheckSpot(suspectSpot); return; }
                Enter(MonsterState.LoseTarget);
                return;
            }

            bool seen = Time.time - lastSawPlayerTime < 0.25f;
            if (seen) LastKnownPosition = p.transform.position;
            SetDest(LastKnownPosition);

            float d = Vector3.Distance(transform.position, p.transform.position);
            if (seen && d <= attackRange) { Enter(MonsterState.Attack); return; }
            // lost sight for a while and reached last known position
            if (!seen && Time.time - lastSawPlayerTime > 2.5f && Arrived()) Enter(MonsterState.LoseTarget);
        }

        void AttackUpdate()
        {
            var p = Target;
            if (p == null) { Enter(MonsterState.Idle); return; }
            FaceTowards(p.transform.position, 10f);
            if (!attackHit && stateTimer <= 0f)
            {
                attackHit = true;
                float d = Vector3.Distance(transform.position, p.transform.position);
                if (d <= attackRange + 0.4f && !p.IsHidden)
                {
                    p.Health.TakeDamage(attackDamage);
                    if (sfx != null) sfx.Hit();
                }
                stateTimer = staggerAfterHit;      // recovery window = the player's chance to run
            }
            else if (attackHit && stateTimer <= 0f)
            {
                if (p.Health.IsDead) { Enter(MonsterState.Idle); return; }
                Enter(MonsterState.Chase);
            }
        }

        void SearchUpdate()
        {
            if (checkingSpot != null)
            {
                if (Arrived())
                {
                    FaceTowards(checkingSpot.transform.position, 8f);
                    var pulled = checkingSpot.MonsterSearch();
                    checkingSpot = null;
                    suspectSpot = null;
                    if (pulled != null)
                    {
                        LastKnownPosition = pulled.transform.position;
                        lastSawPlayerTime = Time.time;
                        Enter(MonsterState.Attack);
                        return;
                    }
                    lookingAround = false;
                }
                return;
            }
            if (!Arrived()) return;
            // arrived at a search point: stand and look around before moving on
            if (!lookingAround) { lookingAround = true; stateTimer = Random.Range(1f, 2.2f); if (sfx != null && Random.value < 0.4f) sfx.Sniff(); }
            if (stateTimer > 0f) { transform.Rotate(0f, 70f * Time.deltaTime, 0f); return; }
            lookingAround = false;
            if (searchLeft <= 0) { Enter(MonsterState.ReturnToPatrol); return; }

            // sometimes check a nearby hiding spot instead (not 100% safe!)
            if (Random.value < hidingCheckChance)
            {
                var spot = NearestHidingSpot(transform.position, 6f);
                if (spot != null) { GoCheckSpot(spot); searchLeft--; return; }
            }
            NextSearchPoint();
        }

        void GoCheckSpot(HidingSpot spot)
        {
            if (State != MonsterState.Search) Enter(MonsterState.Search);
            checkingSpot = spot;
            agent.speed = State == MonsterState.Search ? searchSpeed * 1.3f : agent.speed;
            SetDest(spot.monsterCheckPoint != null ? spot.monsterCheckPoint.position : spot.transform.position);
        }

        void NextSearchPoint()
        {
            searchLeft--;
            lookingAround = false;
            for (int i = 0; i < 8; i++)
            {
                Vector3 r = LastKnownPosition + Random.insideUnitSphere * searchRadius;
                NavMeshHit hit;
                if (NavMesh.SamplePosition(r, out hit, 2.5f, NavMesh.AllAreas)) { SetDest(hit.position); return; }
            }
            SetDest(LastKnownPosition);
        }

        // ============================================================ senses
        void Sense(GameManager gm)
        {
            senseTimer -= Time.deltaTime;
            if (senseTimer > 0f) return;
            float dt = 0.15f - senseTimer;
            senseTimer = 0.15f;

            var p = Target;
            if (p == null || p.Health.IsDead) return;

            bool sees = !gm.InGrace && CanSee(p);
            if (sees)
            {
                lastSawPlayerTime = Time.time;
                LastKnownPosition = p.transform.position;
                float d = Vector3.Distance(transform.position, p.transform.position);
                seeTime += dt * (d < instantSpotDistance ? 4f : 1f);
            }
            else seeTime = Mathf.Max(0f, seeTime - dt * 0.5f);
            Awareness = Mathf.Clamp01(seeTime / awarenessToChase);

            // remember the spot if we watched them hide
            if (p.IsHidden && p.Hiding != null && Time.time - lastSawPlayerTime < 1.0f && Time.time - p.Hiding.EnteredAt < 1.2f)
                suspectSpot = p.Hiding;

            if (Awareness >= 1f && State != MonsterState.Chase && State != MonsterState.Attack)
                Enter(MonsterState.Chase);

            if (hasNoise)
            {
                hasNoise = false;
                if (!gm.InGrace && State != MonsterState.Chase && State != MonsterState.Attack)
                    Enter(MonsterState.InvestigateNoise);
            }
        }

        bool CanSee(PlayerController p)
        {
            float vis = p.VisibilityFactor();
            if (vis <= 0f) return false;
            Vector3 from = eyes != null ? eyes.position : transform.position + Vector3.up * 2f;
            Vector3 to = p.HeadPosition;
            Vector3 dir = to - from;
            float dist = dir.magnitude;
            float range = sightRange * vis;
            if (dist > range) return false;
            if (dist > instantSpotDistance && Vector3.Angle(transform.forward, dir) > sightAngle * 0.5f) return false;
            RaycastHit hit;
            if (Physics.Raycast(from, dir / dist, out hit, dist, sightBlockers, QueryTriggerInteraction.Ignore))
                return hit.collider.GetComponentInParent<PlayerController>() == p;
            return true;
        }

        void OnNoise(NoiseEvent e)
        {
            float d = Vector3.Distance(transform.position, e.Position);
            if (d > e.Radius * hearingMultiplier) return;
            // prefer louder / newer noises
            if (hasNoise && e.Radius < pendingNoise.Radius * 0.6f) return;
            pendingNoise = e;
            hasNoise = true;
        }

        // ============================================================ helpers
        void GoToNextPatrol()
        {
            if (patrolPoints == null || patrolPoints.Length == 0) { Enter(MonsterState.Idle); return; }
            int i, guard = 0;
            do { i = Random.Range(0, patrolPoints.Length); guard++; }
            while ((i == lastPatrol || i == prevPatrol) && patrolPoints.Length > 2 && guard < 20);
            prevPatrol = lastPatrol; lastPatrol = i;
            SetDest(patrolPoints[i].position);
        }

        void HuntPulse(GameManager gm)
        {
            // Every 1–2 minutes it drifts toward the player's area (keeps pressure up). Audible, so fair.
            nextHuntPulse = gm.PlayTime + Random.Range(60f, 120f);
            var p = Target;
            if (p == null || patrolPoints == null || patrolPoints.Length == 0) return;
            Transform best = null; float bd = float.MaxValue;
            foreach (var t in patrolPoints) { float d = Vector3.Distance(t.position, p.transform.position); if (d < bd) { bd = d; best = t; } }
            if (best != null) { agent.speed = patrolSpeed * 1.3f; SetDest(best.position); if (sfx != null) sfx.DistantHowl(); }
        }

        Vector3 NearestPatrolPoint()
        {
            if (patrolPoints == null || patrolPoints.Length == 0) return transform.position;
            Transform best = patrolPoints[0]; float bd = float.MaxValue;
            foreach (var t in patrolPoints) { float d = (t.position - transform.position).sqrMagnitude; if (d < bd) { bd = d; best = t; } }
            return best.position;
        }

        static HidingSpot NearestHidingSpot(Vector3 pos, float maxDist)
        {
            HidingSpot best = null; float bd = maxDist;
            foreach (var h in HidingSpot.All) { float d = Vector3.Distance(h.transform.position, pos); if (d < bd) { bd = d; best = h; } }
            return best;
        }

        void SetDest(Vector3 p)
        {
            if (!agent.isOnNavMesh) return;
            agent.SetDestination(p);
            stuckTimer = 0f;
        }

        bool Arrived()
        {
            if (!agent.isOnNavMesh || agent.pathPending) return false;
            return agent.remainingDistance <= Mathf.Max(agent.stoppingDistance, 0.6f);
        }

        void FaceTowards(Vector3 p, float speed)
        {
            Vector3 d = p - transform.position; d.y = 0f;
            if (d.sqrMagnitude < 0.001f) return;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(d), Time.deltaTime * speed);
        }

        void OpenDoorsInTheWay()
        {
            doorTimer -= Time.deltaTime;
            if (doorTimer > 0f) return;
            doorTimer = 0.25f;
            int n = Physics.OverlapSphereNonAlloc(transform.position + transform.forward * 0.8f + Vector3.up, 1.2f, overlap, ~0, QueryTriggerInteraction.Collide);
            for (int i = 0; i < n; i++)
            {
                var door = overlap[i].GetComponentInParent<Door>();
                if (door != null && !door.IsOpen) door.ForceOpen(transform.position);
            }
        }

        void Unstick()
        {
            if (!agent.isOnNavMesh || agent.isStopped || agent.pathPending) return;
            if (agent.remainingDistance > 1f && agent.velocity.sqrMagnitude < 0.04f) stuckTimer += Time.deltaTime;
            else stuckTimer = 0f;
            if (stuckTimer > 3f)
            {
                stuckTimer = 0f;
                if (State == MonsterState.Chase || State == MonsterState.InvestigateNoise) Enter(MonsterState.Search);
                else Enter(MonsterState.ReturnToPatrol);
            }
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, sightRange);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(LastKnownPosition, 0.5f);
        }
#endif
    }
}
