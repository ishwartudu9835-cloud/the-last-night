// FearPostFX: drives URP post-processing from the player's fear & damage —
// vignette (red pulse on hit), chromatic aberration + lens distortion (screen distortion),
// film grain, desaturation; plus camera shake. Disabled automatically on the Low preset.
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace TheLastNight
{
    public class FearPostFX : MonoBehaviour
    {
        public Volume volume;              // global volume with the overrides below
        public Camera cam;                 // player camera (child of the head pivot, local pos 0)
        public PlayerHealth health;

        Vignette vignette;
        ChromaticAberration chroma;
        FilmGrain grain;
        LensDistortion lens;
        ColorAdjustments color;
        float hurt, shake;
        Color baseVignette = new Color(0.02f, 0f, 0.03f);

        void Start()
        {
            if (volume != null && volume.profile != null)
            {
                volume.profile.TryGet(out vignette);
                volume.profile.TryGet(out chroma);
                volume.profile.TryGet(out grain);
                volume.profile.TryGet(out lens);
                volume.profile.TryGet(out color);
            }
            if (health != null) health.Damaged += OnDamaged;
            GameSettings.Changed += ApplyQuality;
            ApplyQuality();
        }

        void OnDestroy() { GameSettings.Changed -= ApplyQuality; if (health != null) health.Damaged -= OnDamaged; }

        void ApplyQuality()
        {
            if (cam == null) return;
            var data = cam.GetUniversalAdditionalCameraData();
            if (data != null) data.renderPostProcessing = GameSettings.Preset != GraphicsPreset.Low;
        }

        void OnDamaged(float amount) { hurt = 1f; Shake(0.35f); }
        public void Shake(float amount) { shake = Mathf.Max(shake, amount); }

        void Update()
        {
            float fear = health != null ? health.Fear : 0f;
            float hp = health != null ? health.Health01 : 1f;
            float pulse = Mathf.Sin(Time.time * Mathf.Lerp(4f, 9f, fear)) * 0.5f + 0.5f;
            hurt = Mathf.MoveTowards(hurt, 0f, Time.deltaTime * 1.5f);

            if (vignette != null)
            {
                vignette.intensity.Override(Mathf.Clamp01(0.32f + fear * 0.22f + (1f - hp) * 0.15f * pulse + hurt * 0.25f));
                vignette.color.Override(Color.Lerp(baseVignette, new Color(0.45f, 0f, 0f), Mathf.Max(hurt, (1f - hp) * 0.6f)));
            }
            if (chroma != null) chroma.intensity.Override(Mathf.Clamp01(fear * 0.55f + hurt * 0.5f));
            if (grain != null) grain.intensity.Override(Mathf.Clamp01(0.3f + fear * 0.45f));
            if (lens != null) lens.intensity.Override(-fear * 0.22f * (0.6f + 0.4f * pulse) - hurt * 0.15f);
            if (color != null) color.saturation.Override(-25f - fear * 35f);

            // camera shake (camera is a child of the head pivot, so this never fights the controller)
            if (cam != null)
            {
                shake = Mathf.MoveTowards(shake, 0f, Time.deltaTime * 0.9f);
                float s = shake * shake;
                cam.transform.localPosition = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f) * 0.08f * s;
                cam.transform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-1f, 1f) * 4f * s + Mathf.Sin(Time.time * 0.7f) * fear * 1.2f);
            }
        }
    }
}
