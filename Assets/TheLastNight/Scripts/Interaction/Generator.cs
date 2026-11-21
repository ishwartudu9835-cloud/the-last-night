// Pump-house generator: insert fuses (hold), then pull-start (hold). Every pull is LOUD and
// the first pull may fail — a deliberate risk/reward moment that draws the monster.
using System.Collections;
using UnityEngine;

namespace TheLastNight
{
    public class Generator : MonoBehaviour, IInteractable
    {
        public float insertHold = 1.2f;
        public float pullHold = 3.5f;
        [Range(0f, 1f)] public float firstPullSuccessChance = 0.4f;
        public Light statusLight;
        public Light[] poweredLights;       // street lamps that come on
        public EscapeGate gate;
        public Transform shakeRoot;         // visual vibration when running

        int pulls;
        bool running;
        AudioSource loop;

        public string Prompt(PlayerController p)
        {
            var gm = GameManager.Instance;
            if (running || gm == null) return null;
            if (gm.FusesInserted < gm.fusesRequired)
                return p.Inventory != null && p.Inventory.Fuses > 0 ? "Fuse lagao (hold)" : string.Format("Generator: {0}/{1} fuse chahiye", gm.FusesInserted, gm.fusesRequired);
            return "Generator start karo (hold) — AWAAZ HOGI";
        }

        public float HoldTime(PlayerController p)
        {
            var gm = GameManager.Instance;
            if (gm == null) return 0f;
            if (gm.FusesInserted < gm.fusesRequired) return p.Inventory != null && p.Inventory.Fuses > 0 ? insertHold : 0f;
            return pullHold;
        }

        public void Interact(PlayerController p)
        {
            var gm = GameManager.Instance;
            if (gm == null || running) return;
            if (gm.FusesInserted < gm.fusesRequired)
            {
                if (p.Inventory == null || !p.Inventory.TakeFuse()) { gm.Say("Pehle fuse dhoondo", 2f); return; }
                gm.OnFuseInserted();
                Play(ProceduralAudio.MetalClunk, 0.8f);
                NoiseSystem.Emit(transform.position, 7f, NoiseKind.Object, p.gameObject);
                SetStatus(new Color(1f, 0.6f, 0.1f));
                return;
            }
            pulls++;
            bool success = pulls >= 2 || Random.value < firstPullSuccessChance;
            NoiseSystem.Emit(transform.position, NoiseSystem.DefaultRadius(NoiseKind.Generator), NoiseKind.Generator, p.gameObject);
            if (!success)
            {
                Play(ProceduralAudio.GeneratorSputter, 1f);
                gm.Say("Chala nahi! Phir se khicho... jaldi!", 2.5f);
                return;
            }
            StartCoroutine(StartUp());
        }

        IEnumerator StartUp()
        {
            running = true;
            Play(ProceduralAudio.GeneratorSputter, 1f);
            yield return new WaitForSeconds(1.1f);
            if (AudioManager.Instance != null) loop = AudioManager.Instance.PlayLoopAt(ProceduralAudio.GeneratorLoop, transform, 0.8f, 22f);
            SetStatus(Color.green);
            if (poweredLights != null)
                foreach (var l in poweredLights) { if (l != null) { l.enabled = true; var fl = l.GetComponent<FlickerLight>(); if (fl != null) fl.Powered = true; } }
            GameManager.Instance.OnGeneratorStarted();
            if (gate != null) gate.Open();
            // keep "shouting" to the monster for a while
            for (int i = 0; i < 6; i++) { NoiseSystem.Emit(transform.position, 20f, NoiseKind.Generator, null); yield return new WaitForSeconds(4f); }
        }

        void Update()
        {
            if (running && shakeRoot != null)
                shakeRoot.localPosition = new Vector3(Mathf.Sin(Time.time * 60f) * 0.004f, 0f, Mathf.Cos(Time.time * 53f) * 0.004f);
        }

        void SetStatus(Color c) { if (statusLight != null) { statusLight.enabled = true; statusLight.color = c; } }
        void Play(AudioClip c, float v) { if (AudioManager.Instance != null) AudioManager.Instance.PlayAt(c, transform.position + Vector3.up, v); }
        void OnDisable() { if (loop != null) loop.Stop(); }
    }
}
