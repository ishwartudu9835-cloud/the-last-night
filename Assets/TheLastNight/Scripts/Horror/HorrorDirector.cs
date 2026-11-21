// HorrorDirector: paces scripted-random scares from the current tension (player fear, monster state).
// Rules: global gap between events, per-event cooldowns, no repeats from the recent history,
// close jumpscare max once per 3 minutes, nothing big during a chase.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheLastNight
{
    public class HorrorDirector : MonoBehaviour
    {
        public GhostApparition ghost;
        public Vector2 gapSeconds = new Vector2(20f, 45f);
        public int historySize = 3;
        public bool debugLog;

        class Ev
        {
            public string name; public float weight, cooldown, minTension, maxTension; public float readyAt;
            public Func<bool> run;   // returns true if it actually happened
        }

        readonly List<Ev> events = new List<Ev>();
        readonly Queue<string> history = new Queue<string>();
        float nextAt;

        PlayerController P { get { return GameManager.Instance != null ? GameManager.Instance.Player : null; } }

        void Start()
        {
            Add("LightFlicker", 3f, 25f, 0f, 1f, LightFlicker);
            Add("PowerFailure", 1.2f, 120f, 0.15f, 0.8f, PowerFailure);
            Add("DistantScream", 1.5f, 70f, 0f, 0.6f, DistantScream);
            Add("Whisper", 1.5f, 60f, 0.2f, 0.9f, Whisper);
            Add("DistantCrying", 1.2f, 90f, 0f, 0.5f, DistantCrying);
            Add("DoorSlam", 1.5f, 60f, 0.1f, 0.8f, DoorSlam);
            Add("ObjectFall", 1.5f, 50f, 0f, 0.8f, ObjectFall);
            Add("RadioStatic", 1f, 80f, 0f, 0.7f, RadioStatic);
            Add("GhostGlimpse", 1.3f, 90f, 0.1f, 0.6f, GhostGlimpse);
            Add("Jumpscare", 0.8f, 180f, 0.3f, 0.65f, Jumpscare);
            nextAt = 35f; // first scare after you've settled in
        }

        void Add(string n, float w, float cd, float minT, float maxT, Func<bool> run)
        {
            events.Add(new Ev { name = n, weight = w, cooldown = cd, minTension = minT, maxTension = maxT, run = run, readyAt = cd * 0.3f });
        }

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || !gm.IsPlaying || P == null || P.Health.IsDead) return;
            if (gm.PlayTime < nextAt) return;

            float tension = P.Health.Fear;
            bool chase = gm.Monster != null && (gm.Monster.State == MonsterState.Chase || gm.Monster.State == MonsterState.Attack);

            // weighted pick among eligible events
            float total = 0f;
            var eligible = new List<Ev>(events.Count);
            foreach (var e in events)
            {
                if (gm.PlayTime < e.readyAt || tension < e.minTension || tension > e.maxTension) continue;
                if (history.Contains(e.name)) continue;
                if (chase && e.name != "LightFlicker") continue;
                eligible.Add(e); total += e.weight;
            }
            nextAt = gm.PlayTime + UnityEngine.Random.Range(gapSeconds.x, gapSeconds.y) * Mathf.Lerp(1.2f, 0.7f, tension);
            if (eligible.Count == 0) return;
            float r = UnityEngine.Random.value * total;
            foreach (var e in eligible)
            {
                r -= e.weight;
                if (r > 0f) continue;
                if (e.run())
                {
                    e.readyAt = gm.PlayTime + e.cooldown;
                    history.Enqueue(e.name);
                    while (history.Count > historySize) history.Dequeue();
                    if (debugLog) Debug.Log("[HorrorDirector] " + e.name + " (tension " + tension.ToString("0.00") + ")");
                }
                break;
            }
        }

        // ------------------------------------------------------------ events
        Transform Cam { get { return P.cam.transform; } }
        AudioManager AM { get { return AudioManager.Instance; } }

        bool LightFlicker()
        {
            bool any = false;
            foreach (var l in FlickerLight.All)
                if (Vector3.Distance(l.transform.position, P.transform.position) < 20f) { l.Burst(UnityEngine.Random.Range(1.5f, 3.5f)); any = true; }
            if (P.Flashlight != null) { P.Flashlight.ScareFlicker(1.2f); any = true; }
            return any;
        }

        bool PowerFailure()
        {
            if (GameManager.Instance.GeneratorRunning || FlickerLight.All.Count == 0) return false;
            float s = UnityEngine.Random.Range(6f, 12f);
            foreach (var l in FlickerLight.All) l.PowerCut(s);
            if (AM != null) AM.PlayUI(ProceduralAudio.MetalClunk, 0.5f);
            P.Health.AddScare(0.15f);
            return true;
        }

        bool DistantScream()
        {
            if (AM == null) return false;
            AM.PlayAt(ProceduralAudio.DistantScream, RandomPointAround(25f, 40f), 1f, UnityEngine.Random.Range(0.8f, 1.1f), 60f);
            P.Health.AddScare(0.1f);
            return true;
        }

        bool Whisper() { if (AM == null) return false; AM.Whisper(Cam); P.Health.AddScare(0.15f); return true; }

        bool DistantCrying()
        {
            if (AM == null) return false;
            AM.PlayAt(ProceduralAudio.Crying, RandomPointAround(12f, 22f), 0.8f, UnityEngine.Random.Range(0.9f, 1.1f), 30f);
            return true;
        }

        bool DoorSlam()
        {
            Door best = null;
            foreach (var d in FindObjectsByType<Door>(FindObjectsSortMode.None))
            {
                float dist = Vector3.Distance(d.transform.position, P.transform.position);
                if (d.IsOpen && dist > 4f && dist < 16f) { best = d; break; }
            }
            if (best == null) return false;
            best.Slam();
            P.Health.AddScare(0.2f);
            return true;
        }

        bool ObjectFall()
        {
            HorrorProp pick = null;
            foreach (var h in HorrorProp.All)
            {
                if (h.IsRadio) continue;
                float d = Vector3.Distance(h.transform.position, P.transform.position);
                if (d > 4f && d < 15f && !InView(h.transform.position)) { pick = h; break; }
            }
            if (pick == null) return false;
            pick.Disturb();
            return true;
        }

        bool RadioStatic()
        {
            foreach (var h in HorrorProp.All)
                if (h.IsRadio && Vector3.Distance(h.transform.position, P.transform.position) < 18f) { h.Disturb(); return true; }
            return false;
        }

        bool GhostGlimpse()
        {
            if (ghost == null || ghost.Visible) return false;
            // at the edge of vision, 10–16 m away, on open ground
            for (int i = 0; i < 6; i++)
            {
                float side = UnityEngine.Random.value < 0.5f ? -1f : 1f;
                Vector3 dir = Quaternion.Euler(0f, side * UnityEngine.Random.Range(28f, 40f), 0f) * Flat(Cam.forward);
                Vector3 pos = P.transform.position + dir * UnityEngine.Random.Range(10f, 16f);
                if (OnGround(ref pos) && Visible(pos + Vector3.up * 1.5f)) { ghost.Show(pos, Cam, 2.5f, false); return true; }
            }
            return false;
        }

        bool Jumpscare()
        {
            if (ghost == null || ghost.Visible || P.IsHidden) return false;
            Vector3 pos = P.transform.position + Flat(Cam.forward) * 1.3f;
            if (!OnGround(ref pos) || !Visible(pos + Vector3.up * 1.5f)) return false;
            ghost.Show(pos, Cam, 0.45f, true);
            if (AM != null) AM.PlayUI(ProceduralAudio.Jumpscare, 1f);
            P.Health.AddScare(0.6f);
            var fx = FindFirstObjectByType<FearPostFX>();
            if (fx != null) fx.Shake(0.5f);
            return true;
        }

        // ------------------------------------------------------------ helpers
        static Vector3 Flat(Vector3 v) { v.y = 0f; return v.sqrMagnitude > 0.0001f ? v.normalized : Vector3.forward; }

        Vector3 RandomPointAround(float min, float max)
        {
            Vector2 c = UnityEngine.Random.insideUnitCircle.normalized * UnityEngine.Random.Range(min, max);
            return P.transform.position + new Vector3(c.x, 1.5f, c.y);
        }

        bool InView(Vector3 p) { return Vector3.Angle(Cam.forward, p - Cam.position) < 40f; }

        bool Visible(Vector3 p)
        {
            Vector3 d = p - Cam.position;
            return !Physics.Raycast(Cam.position, d.normalized, d.magnitude - 0.3f, ~0, QueryTriggerInteraction.Ignore);
        }

        static bool OnGround(ref Vector3 p)
        {
            RaycastHit hit;
            if (Physics.Raycast(p + Vector3.up * 3f, Vector3.down, out hit, 6f, ~0, QueryTriggerInteraction.Ignore)) { p = hit.point; return true; }
            return false;
        }
    }
}
