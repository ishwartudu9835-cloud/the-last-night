// ProceduralAudio: synthesises every Phase-1 sound at load time (no audio files needed).
// Phase 2 swaps these for recorded/licensed samples: just assign AudioClips with the same names
// (AudioManager.Override) — gameplay code never changes.
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheLastNight
{
    public static class ProceduralAudio
    {
        public const int Rate = 22050;   // mobile-friendly
        public static bool Ready { get; private set; }
        public static float Progress { get; private set; }

        public static AudioClip Click, Pickup, Paper, DoorCreak, DoorSlam, DoorRattle, MetalClunk, MetalDrag, MetalDragLoop,
            GeneratorSputter, GeneratorLoop, MonsterBreath, MonsterStep, Scream, Growl, Sniff, Hit,
            Heartbeat, PlayerBreath, Crying, RadioStatic, DistantScream, Wind, Rain, Drone, ChasePulse, Stinger,
            ObjectFall, Jumpscare;
        public static AudioClip[] Footsteps, Whispers;

        static System.Random rng = new System.Random(1234);
        static float N() { return (float)(rng.NextDouble() * 2.0 - 1.0); }

        /// <summary>Generates everything, one clip per frame so the loading screen stays responsive.</summary>
        public static IEnumerator GenerateAll()
        {
            if (Ready) { Progress = 1f; yield break; }
            var jobs = new List<Action>
            {
                () => Click = Make("click", 0.05f, (t, i) => N() * Env(t, 0.001f, 0.04f) * 0.6f),
                () => Pickup = Make("pickup", 0.35f, (t, i) => (Sin(660f, t) + Sin(990f, t) * 0.6f) * Env(t, 0.005f, 0.3f) * 0.35f),
                () => Paper = Filtered("paper", 0.5f, 3000f, 0.2f, (t) => Env(t, 0.02f, 0.45f) * (0.5f + 0.5f * Mathf.Sin(t * 60f))),
                () => DoorCreak = Creak("creak", 1.4f),
                () => DoorSlam = Make("slam", 0.9f, (t, i) => (LowNoise(i, 0.08f) * 2.2f + Sin(70f, t) * 0.8f) * Env(t, 0.002f, 0.8f)),
                () => DoorRattle = Make("rattle", 0.6f, (t, i) => N() * (Mathf.Sin(t * 70f) > 0.6f ? 0.6f : 0.05f) * Env(t, 0.01f, 0.55f)),
                () => MetalClunk = Make("clunk", 0.5f, (t, i) => (Sin(180f, t) + Sin(437f, t) * 0.6f + Sin(1210f, t) * 0.3f) * Env(t, 0.001f, 0.45f) * 0.5f),
                () => MetalDrag = Drag("drag", 4f, false),
                () => MetalDragLoop = Drag("dragloop", 3f, true),
                () => GeneratorSputter = Make("sputter", 2.2f, (t, i) => (Saw(28f + 18f * t, t) * 0.5f + LowNoise(i, 0.2f)) * (Mathf.Sin(t * 22f) > -0.2f ? 1f : 0.2f) * Env(t, 0.02f, 2.1f) * 0.8f),
                () => GeneratorLoop = Loop(Make("genloop", 2f, (t, i) => (Saw(49f, t) * 0.35f + Sin(98f, t) * 0.25f + LowNoise(i, 0.15f) * 0.5f) * (0.85f + 0.15f * Mathf.Sin(t * 2f * Mathf.PI * 12.5f)))),
                () => MonsterBreath = Loop(Breath("mbreath", 3.2f, 140f, 0.9f)),
                () => MonsterStep = Make("mstep", 0.4f, (t, i) => (LowNoise(i, 0.05f) * 2.5f + Sin(55f, t)) * Env(t, 0.003f, 0.35f) * 0.9f),
                () => Scream = ScreamClip("scream", 1.6f, 900f, 240f),
                () => Growl = Make("growl", 1.2f, (t, i) => (Saw(70f + Mathf.Sin(t * 40f) * 12f, t) * 0.5f + LowNoise(i, 0.12f)) * Env(t, 0.08f, 1.1f) * 0.8f),
                () => Sniff = Make("sniff", 1.3f, (t, i) => HiNoise(i) * (Mathf.Repeat(t, 0.32f) < 0.12f ? 0.7f : 0f) * Env(t, 0.01f, 1.25f)),
                () => Hit = Make("hit", 0.6f, (t, i) => (LowNoise(i, 0.1f) * 2f + Sin(90f - t * 80f, t)) * Env(t, 0.001f, 0.55f)),
                () => Heartbeat = Make("heartbeat", 0.9f, (t, i) => Sin(52f, t) * (Env(t, 0.005f, 0.14f) + Env(t - 0.22f, 0.005f, 0.14f) * 0.75f)),
                () => PlayerBreath = Loop(Breath("pbreath", 2.4f, 900f, 0.5f)),
                () => Crying = Make("crying", 3.5f, (t, i) => Sin(520f + Mathf.Sin(t * 5f) * 40f - t * 30f, t) * 0.15f * (0.5f + 0.5f * Mathf.Sin(t * 3.1f)) * Env(t, 0.5f, 3f) + HiNoise(i) * 0.03f),
                () => RadioStatic = Make("static", 2f, (t, i) => N() * 0.4f * (0.6f + 0.4f * Mathf.Sin(t * 9f)) + Sin(1000f, t) * 0.05f * (Mathf.Sin(t * 3f) > 0.7f ? 1 : 0)),
                () => DistantScream = ScreamClip("dscream", 2.4f, 650f, 300f, 0.35f),
                () => Wind = Loop(Filtered("wind", 6f, 500f, 0.5f, (t) => 0.4f + 0.35f * Mathf.Sin(t * 0.9f) + 0.15f * Mathf.Sin(t * 2.3f))),
                () => Rain = Loop(Filtered("rain", 4f, 6000f, 0.35f, (t) => 0.8f)),
                () => Drone = Loop(Make("drone", 8f, (t, i) => (Sin(55f, t) * 0.3f + Sin(55.7f, t) * 0.3f + Sin(82.4f, t) * 0.15f + Sin(110.3f, t) * 0.08f * Mathf.Sin(t * 0.785f)) * 0.7f)),
                () => ChasePulse = Loop(Make("chase", 4f, (t, i) => (Saw(41f, t) * 0.4f + LowNoise(i, 0.2f) * 0.6f) * Env(Mathf.Repeat(t, 0.5f), 0.005f, 0.35f) + Sin(220f, t) * 0.08f * (Mathf.Repeat(t, 1f) < 0.5f ? 1 : 0))),
                () => Stinger = Make("stinger", 1.8f, (t, i) => (Saw(110f, t) + Saw(116.5f, t) + Saw(155f, t)) * 0.18f * Env(t, 0.01f, 1.7f) + HiNoise(i) * Env(t, 0.001f, 0.6f) * 0.4f),
                () => ObjectFall = Make("fall", 1f, (t, i) => (LowNoise(i, 0.1f) * 1.6f + Sin(300f, t) * 0.4f) * (Env(t, 0.001f, 0.3f) + Env(t - 0.35f, 0.001f, 0.25f) * 0.6f)),
                () => Jumpscare = Make("jumpscare", 1.5f, (t, i) => (Saw(180f - t * 60f, t) + Saw(240f, t) * 0.7f + HiNoise(i) * 1.5f) * 0.35f * Env(t, 0.001f, 1.4f)),
                () => Footsteps = new[] { Step("step0", 0.9f), Step("step1", 1.0f), Step("step2", 1.1f), Step("step3", 0.95f) },
                () => Whispers = new[] { WhisperClip("whisper0", 2.2f), WhisperClip("whisper1", 2.8f) },
            };
            for (int k = 0; k < jobs.Count; k++) { jobs[k](); Progress = (k + 1f) / jobs.Count; yield return null; }
            Ready = true;
        }

        // ------------------------------------------------------------------ DSP helpers
        static float lp1, hpPrev;
        static float Sin(float f, float t) { return Mathf.Sin(2f * Mathf.PI * f * t); }
        static float Saw(float f, float t) { return 2f * Mathf.Repeat(f * t, 1f) - 1f; }
        static float Env(float t, float attack, float length)
        {
            if (t < 0f || t > length) return 0f;
            if (t < attack) return t / attack;
            float r = (t - attack) / Mathf.Max(0.0001f, length - attack);
            return Mathf.Pow(1f - r, 2f);
        }
        // one-pole low-pass noise; `amount` ~ cutoff (0..1, lower = darker)
        static float LowNoise(int i, float amount) { if (i == 0) lp1 = 0f; lp1 += (N() - lp1) * amount; return lp1 * 2.5f; }
        static float HiNoise(int i) { float n = N(); if (i == 0) hpPrev = 0f; float o = n - hpPrev; hpPrev = n; return o * 0.5f; }

        static AudioClip Make(string name, float seconds, Func<float, int, float> f)
        {
            int n = Mathf.CeilToInt(seconds * Rate);
            var data = new float[n];
            for (int i = 0; i < n; i++) data[i] = Mathf.Clamp(f(i / (float)Rate, i), -1f, 1f);
            return ToClip(name, data);
        }

        static AudioClip ToClip(string name, float[] data)
        {
            var clip = AudioClip.Create("tln_" + name, data.Length, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>Crossfades the tail into the head so the clip loops without a click.</summary>
        static AudioClip Loop(AudioClip c)
        {
            string nm = c.name.Replace("tln_", "") + "_loop";
            var d = new float[c.samples];
            c.GetData(d, 0);
            int fade = Mathf.Min(Rate / 4, d.Length / 4);
            int n = d.Length - fade;
            var o = new float[n];
            for (int i = 0; i < n; i++) o[i] = d[i];
            for (int i = 0; i < fade; i++) { float a = i / (float)fade; o[i] = d[i] * a + d[n + i] * (1f - a); }
            UnityEngine.Object.Destroy(c);
            return ToClip(nm, o);
        }

        // band-limited noise via biquad band-pass, with amplitude envelope fn
        static AudioClip Filtered(string name, float seconds, float freq, float q, Func<float, float> amp)
        {
            int n = Mathf.CeilToInt(seconds * Rate);
            var d = new float[n];
            float w0 = 2f * Mathf.PI * freq / Rate, alpha = Mathf.Sin(w0) / (2f * q), cs = Mathf.Cos(w0);
            float b0 = alpha, b2 = -alpha, a0 = 1f + alpha, a1 = -2f * cs, a2 = 1f - alpha;
            float x1 = 0, x2 = 0, y1 = 0, y2 = 0;
            for (int i = 0; i < n; i++)
            {
                float x = N();
                float y = (b0 * x + b2 * x2 - a1 * y1 - a2 * y2) / a0;
                x2 = x1; x1 = x; y2 = y1; y1 = y;
                d[i] = Mathf.Clamp(y * 3f * amp(i / (float)Rate), -1f, 1f);
            }
            return ToClip(name, d);
        }

        static AudioClip Step(string name, float pitch)
        {
            return Make(name, 0.22f, (t, i) => (LowNoise(i, 0.35f * pitch) + HiNoise(i) * 0.4f) * Env(t, 0.002f, 0.2f) * 0.8f);
        }

        static AudioClip Creak(string name, float seconds)
        {
            // stick-slip friction: impulse train with wandering rate through a resonance
            int n = Mathf.CeilToInt(seconds * Rate);
            var d = new float[n];
            float phase = 0f, y1 = 0, y2 = 0;
            float f0 = 700f, r = 0.995f;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Rate;
                float rate = 35f + 25f * Mathf.Sin(t * 3.3f) + 15f * Mathf.Sin(t * 7.1f);
                phase += rate / Rate;
                float imp = 0f;
                if (phase >= 1f) { phase -= 1f; imp = 1f + N() * 0.3f; }
                float w = 2f * Mathf.PI * (f0 + 200f * Mathf.Sin(t * 1.7f)) / Rate;
                float y = imp + 2f * r * Mathf.Cos(w) * y1 - r * r * y2;
                y2 = y1; y1 = y;
                d[i] = Mathf.Clamp(y * 0.08f * Env(t, 0.05f, seconds), -1f, 1f);
            }
            return ToClip(name, d);
        }

        static AudioClip Drag(string name, float seconds, bool loop)
        {
            AudioClip c = Make(name, seconds, (t, i) =>
            {
                float scrape = HiNoise(i) * (0.6f + 0.4f * Mathf.Sin(t * 13f + Mathf.Sin(t * 3f) * 4f));
                float ring = Sin(1340f, t) * 0.08f + Sin(2210f, t) * 0.05f;
                float env = loop ? (0.7f + 0.3f * Mathf.Sin(t * 2f * Mathf.PI / seconds)) : Env(t, 0.3f, seconds);
                return (scrape + ring) * env * 0.8f;
            });
            return loop ? Loop(c) : c;
        }

        static AudioClip Breath(string name, float seconds, float colour, float depth)
        {
            // inhale/exhale cycle of filtered noise
            return Filtered(name, seconds, colour, 0.7f, (t) =>
            {
                float c = Mathf.Repeat(t / seconds, 1f);
                float inh = c < 0.4f ? Mathf.Sin(c / 0.4f * Mathf.PI) : 0f;
                float exh = c >= 0.45f && c < 0.95f ? Mathf.Sin((c - 0.45f) / 0.5f * Mathf.PI) * 1.2f : 0f;
                return (inh * 0.6f + exh) * depth;
            });
        }

        static AudioClip ScreamClip(string name, float seconds, float f0, float f1, float gain = 1f)
        {
            float ph1 = 0, ph2 = 0;
            return Make(name, seconds, (t, i) =>
            {
                if (i == 0) { ph1 = 0; ph2 = 0; }
                float k = t / seconds;
                float f = Mathf.Lerp(f0, f1, k) * (1f + 0.04f * Mathf.Sin(t * 38f));
                ph1 += f / Rate; ph2 += f * 1.49f / Rate;
                float v = (2f * Mathf.Repeat(ph1, 1f) - 1f) * 0.5f + (2f * Mathf.Repeat(ph2, 1f) - 1f) * 0.3f + HiNoise(i) * 0.6f;
                return v * Env(t, 0.03f, seconds) * 0.6f * gain;
            });
        }

        static AudioClip WhisperClip(string name, float seconds)
        {
            // syllable-like bursts of band-passed noise, panned later by AudioManager
            return Filtered(name, seconds, 2600f, 1.8f, (t) =>
            {
                float syl = Mathf.Max(0f, Mathf.Sin(t * 2f * Mathf.PI * (3.2f + Mathf.Sin(t * 1.3f))));
                return syl * syl * Env(t, 0.2f, seconds) * 0.9f;
            });
        }
    }
}
