// Flashlight with limited battery, low-battery / scare flicker, and swap-in of spare batteries.
using UnityEngine;

namespace TheLastNight
{
    public class Flashlight : MonoBehaviour
    {
        public Light spot;
        public Renderer lensRenderer;        // optional glowing lens on the torch model
        public float batterySeconds = 100f;  // one full cell
        public float baseIntensity = 6f;
        public float lowBatteryThreshold = 0.18f;

        public bool IsOn { get; private set; }
        public float Charge { get; private set; }  // seconds left
        public float Charge01 { get { return Charge / batterySeconds; } }

        float flickerTimer, scareFlicker;
        Material lensMat;

        void Awake()
        {
            Charge = batterySeconds;
            if (lensRenderer != null) lensMat = lensRenderer.material;
            SetOn(true, false);
            GameSettings.Changed += ApplyQuality;
            ApplyQuality();
        }

        void OnDestroy() { GameSettings.Changed -= ApplyQuality; }

        void ApplyQuality()
        {
            if (spot != null) spot.shadows = GameSettings.FlashlightShadows ? LightShadows.Soft : LightShadows.None;
        }

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || !gm.IsPlaying) return;
            InputBridge.Poll();
            if (InputBridge.FlashlightPressed) Toggle();
            if (InputBridge.ReloadPressed) { var inv = GetComponentInParent<Inventory>(); if (inv != null) inv.UseBattery(); }

            if (IsOn)
            {
                Charge -= Time.deltaTime;
                if (Charge <= 0f) { Charge = 0f; SetOn(false, true); gm.Say("Battery khatam... Inventory se nayi lagao", 3f); }
            }

            float intensity = IsOn ? baseIntensity * Mathf.Lerp(0.45f, 1f, Mathf.Clamp01(Charge01 / 0.35f)) : 0f;
            // flicker: low battery or a scare
            if (IsOn && (Charge01 < lowBatteryThreshold || scareFlicker > 0f))
            {
                flickerTimer -= Time.deltaTime;
                if (flickerTimer <= 0f) flickerTimer = Random.Range(0.03f, 0.25f);
                if (flickerTimer < 0.06f) intensity *= Random.Range(0f, 0.3f);
            }
            if (scareFlicker > 0f) scareFlicker -= Time.deltaTime;
            if (spot != null) spot.intensity = Mathf.Lerp(spot.intensity, intensity, Time.deltaTime * 25f);
            if (lensMat != null) lensMat.SetColor("_EmissionColor", Color.white * (intensity > 0.1f ? 2f : 0f));
        }

        public void Toggle()
        {
            if (!IsOn && Charge <= 0f) { if (AudioManager.Instance != null) AudioManager.Instance.PlayUI(ProceduralAudio.Click, 0.4f); return; }
            SetOn(!IsOn, true);
        }

        void SetOn(bool on, bool sound)
        {
            IsOn = on;
            if (spot != null) spot.enabled = true; // intensity handles visibility (no shader variant churn)
            if (sound && AudioManager.Instance != null) AudioManager.Instance.PlayUI(ProceduralAudio.Click, 0.5f);
        }

        public void InsertBattery()
        {
            Charge = batterySeconds;
            if (!IsOn) SetOn(true, false);
        }

        /// <summary>Make the light stutter for a moment (scares, monster nearby).</summary>
        public void ScareFlicker(float seconds) { scareFlicker = Mathf.Max(scareFlicker, seconds); }
    }
}
