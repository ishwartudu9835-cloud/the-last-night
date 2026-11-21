// AudioManager: pooled 3D one-shots, attached loops, 2D ambience (wind/rain), dynamic music
// (calm drone <-> chase pulse) and the player's fear audio (heartbeat, heavy breathing).
using System.Collections.Generic;
using UnityEngine;

namespace TheLastNight
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Range(0f, 1f)] public float ambienceVolume = 0.55f;
        [Range(0f, 1f)] public float musicVolume = 0.5f;

        ComponentPool<AudioSource> pool;
        readonly List<AudioSource> active = new List<AudioSource>();
        readonly List<float> activeEnd = new List<float>();
        AudioSource wind, rain, drone, chase, heart, breath, ui;
        float heartTimer, chaseLevel, stepIdx;
        bool ambienceOn;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            pool = new ComponentPool<AudioSource>(CreatePooled, 16);
            ui = Make2D("UI");
        }

        void OnDestroy() { if (Instance == this) Instance = null; }

        AudioSource CreatePooled()
        {
            var go = new GameObject("sfx");
            go.transform.SetParent(transform);
            var s = go.AddComponent<AudioSource>();
            s.playOnAwake = false;
            s.spatialBlend = 1f;
            s.rolloffMode = AudioRolloffMode.Logarithmic;
            s.minDistance = 1.5f;
            s.dopplerLevel = 0f;
            return s;
        }

        AudioSource Make2D(string n)
        {
            var go = new GameObject(n);
            go.transform.SetParent(transform);
            var s = go.AddComponent<AudioSource>();
            s.playOnAwake = false; s.spatialBlend = 0f;
            return s;
        }

        // ------------------------------------------------------------ one-shots
        public AudioSource PlayAt(AudioClip clip, Vector3 pos, float volume = 1f, float pitch = 1f, float maxDistance = 30f)
        {
            if (clip == null) return null;
            var s = pool.Get();
            s.transform.position = pos;
            s.clip = clip; s.volume = volume; s.pitch = pitch;
            s.maxDistance = maxDistance; s.spatialBlend = 1f; s.loop = false;
            s.Play();
            active.Add(s);
            activeEnd.Add(Time.unscaledTime + clip.length / Mathf.Max(0.1f, pitch) + 0.1f);
            return s;
        }

        public void PlayUI(AudioClip clip, float volume = 1f) { if (clip != null) ui.PlayOneShot(clip, volume); }

        public void PlayFootstep(Vector3 pos, float volume)
        {
            var arr = ProceduralAudio.Footsteps;
            if (arr == null || arr.Length == 0) return;
            stepIdx = (stepIdx + 1 + Random.Range(0, arr.Length - 1)) % arr.Length;
            var s = PlayAt(arr[(int)stepIdx], pos, volume, Random.Range(0.9f, 1.1f), 15f);
            if (s != null) s.spatialBlend = 0.4f; // own footsteps sit "under" the listener
        }

        /// <summary>Whisper right next to one ear of the listener.</summary>
        public void Whisper(Transform listener)
        {
            var w = ProceduralAudio.Whispers;
            if (w == null || listener == null) return;
            Vector3 side = listener.right * (Random.value < 0.5f ? -0.6f : 0.6f) - listener.forward * 0.2f;
            PlayAt(w[Random.Range(0, w.Length)], listener.position + side, 0.9f, Random.Range(0.85f, 1.05f), 4f);
        }

        /// <summary>Looping 3D source parented to a transform (monster breath, generator).</summary>
        public AudioSource PlayLoopAt(AudioClip clip, Transform parent, float volume, float maxDistance)
        {
            if (clip == null) return null;
            var go = new GameObject("loop_" + clip.name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = Vector3.up * 1.5f;
            var s = go.AddComponent<AudioSource>();
            s.clip = clip; s.loop = true; s.volume = volume; s.spatialBlend = 1f;
            s.rolloffMode = AudioRolloffMode.Logarithmic; s.minDistance = 1.5f; s.maxDistance = maxDistance; s.dopplerLevel = 0f;
            s.time = Random.Range(0f, clip.length * 0.9f);
            s.Play();
            return s;
        }

        // ------------------------------------------------------------ ambience & music
        public void StartAmbience()
        {
            if (ambienceOn) return;
            ambienceOn = true;
            wind = Loop2D(ProceduralAudio.Wind, ambienceVolume * 0.7f);
            rain = Loop2D(ProceduralAudio.Rain, ambienceVolume * 0.45f);
            drone = Loop2D(ProceduralAudio.Drone, 0f);
            chase = Loop2D(ProceduralAudio.ChasePulse, 0f);
            heart = Make2D("heart");
            breath = Loop2D(ProceduralAudio.PlayerBreath, 0f);
        }

        AudioSource Loop2D(AudioClip c, float vol)
        {
            var s = Make2D(c != null ? c.name : "loop");
            s.clip = c; s.loop = true; s.volume = vol;
            if (c != null) s.Play();
            return s;
        }

        public void SetChase(bool on) { if (on) PlayUI(ProceduralAudio.Stinger, 0.6f * GameSettings.MusicVolume); }

        void Update()
        {
            // release finished one-shots back to the pool
            for (int i = active.Count - 1; i >= 0; i--)
            {
                if (Time.unscaledTime >= activeEnd[i] || active[i] == null) { pool.Release(active[i]); active.RemoveAt(i); activeEnd.RemoveAt(i); }
            }
            if (!ambienceOn) return;
            var gm = GameManager.Instance;
            bool playing = gm != null && (gm.IsPlaying || gm.State == GameState.Paused);
            // duck the weather while paused / on end screens
            float duck = gm != null && gm.IsPlaying ? 1f : 0.35f;
            wind.volume = Mathf.MoveTowards(wind.volume, ambienceVolume * 0.7f * duck, Time.unscaledDeltaTime);
            rain.volume = Mathf.MoveTowards(rain.volume, ambienceVolume * 0.45f * duck, Time.unscaledDeltaTime);
            float mv = musicVolume * GameSettings.MusicVolume;

            // dynamic music: chase layer follows monster state
            bool chasing = playing && gm.Monster != null && (gm.Monster.State == MonsterState.Chase || gm.Monster.State == MonsterState.Attack);
            chaseLevel = Mathf.MoveTowards(chaseLevel, chasing ? 1f : 0f, Time.deltaTime * (chasing ? 2f : 0.25f));
            drone.volume = playing ? mv * Mathf.Lerp(0.8f, 0.25f, chaseLevel) : 0f;
            chase.volume = playing ? mv * chaseLevel : 0f;

            // fear audio
            var p = gm != null ? gm.Player : null;
            if (p != null && p.Health != null && gm.IsPlaying)
            {
                float fear = p.Health.Fear;
                if (fear > 0.3f)
                {
                    heartTimer -= Time.deltaTime;
                    if (heartTimer <= 0f) { heartTimer = Mathf.Lerp(1.1f, 0.42f, fear); heart.PlayOneShot(ProceduralAudio.Heartbeat, Mathf.Lerp(0.2f, 0.9f, fear)); }
                }
                float breathTarget = Mathf.Max(fear > 0.5f ? (fear - 0.5f) * 1.4f : 0f, p.Tired ? 0.7f : 0f);
                breath.volume = Mathf.MoveTowards(breath.volume, breathTarget * 0.8f, Time.deltaTime);
                breath.pitch = p.Tired ? 1.25f : 1f;
            }
            else if (breath != null) breath.volume = 0f;
        }
    }
}
