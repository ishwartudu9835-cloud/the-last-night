// MonsterAudio: all of the creature's 3D sound "tells" so players can locate it by ear.
using UnityEngine;
using UnityEngine.AI;

namespace TheLastNight
{
    public class MonsterAudio : MonoBehaviour
    {
        public float stepInterval = 0.62f;
        AudioSource breath, drag;
        MonsterAI ai;
        NavMeshAgent agent;
        float stepT, lastScream = -99f;

        void Start()
        {
            ai = GetComponent<MonsterAI>();
            agent = GetComponent<NavMeshAgent>();
        }

        void EnsureLoops()
        {
            if (breath != null || AudioManager.Instance == null || ProceduralAudio.MonsterBreath == null) return;
            breath = AudioManager.Instance.PlayLoopAt(ProceduralAudio.MonsterBreath, transform, 0.7f, 16f);
            drag = AudioManager.Instance.PlayLoopAt(ProceduralAudio.MetalDragLoop, transform, 0f, 26f);
        }

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            EnsureLoops();
            bool playing = gm.IsPlaying;
            if (breath != null) breath.volume = playing ? (ai.State == MonsterState.Chase ? 1f : 0.6f) : 0f;
            if (drag != null)
            {
                // metal dragging = "it's patrolling somewhere near"
                float target = playing && (ai.State == MonsterState.Patrol || ai.State == MonsterState.ReturnToPatrol) && agent.velocity.sqrMagnitude > 0.2f ? 0.55f : 0f;
                drag.volume = Mathf.MoveTowards(drag.volume, target, Time.deltaTime * 0.8f);
            }
            if (!playing) return;
            float spd = agent.velocity.magnitude;
            if (spd > 0.3f)
            {
                stepT -= Time.deltaTime * Mathf.Lerp(0.8f, 1.9f, spd / 4.6f);
                if (stepT <= 0f) { stepT = stepInterval; Play(ProceduralAudio.MonsterStep, 0.9f, Random.Range(0.85f, 1.05f)); }
            }
        }

        public void Scream()
        {
            if (Time.time - lastScream < 8f) return;
            lastScream = Time.time;
            Play(ProceduralAudio.Scream, 1f, Random.Range(0.8f, 0.95f));
            if (AudioManager.Instance != null) AudioManager.Instance.SetChase(true);
        }

        public void Alert() { Play(ProceduralAudio.Growl, 0.6f, Random.Range(0.9f, 1.1f)); }
        public void Sniff() { Play(ProceduralAudio.Sniff, 0.8f, 1f); if (AudioManager.Instance != null) AudioManager.Instance.SetChase(false); }
        public void AttackWindup() { Play(ProceduralAudio.Growl, 1f, 1.3f); }
        public void Hit() { Play(ProceduralAudio.Hit, 1f, 1f); }
        public void DistantHowl() { Play(ProceduralAudio.Scream, 0.5f, 0.6f); }

        void Play(AudioClip c, float v, float pitch)
        {
            if (AudioManager.Instance != null && c != null) AudioManager.Instance.PlayAt(c, transform.position + Vector3.up * 2f, v, pitch, 40f);
        }
    }
}
