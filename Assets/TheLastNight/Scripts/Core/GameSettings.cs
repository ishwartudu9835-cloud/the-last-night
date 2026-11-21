// GameSettings: persistent player settings (PlayerPrefs) + applying graphics presets.
// Presets touch only cheap, known-safe knobs: URP render scale, MSAA, shadow distance,
// target FPS, real-time shadow toggles and fog density.
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace TheLastNight
{
    public enum GraphicsPreset { Low = 0, Medium = 1, High = 2 }

    public static class GameSettings
    {
        public static event Action Changed;

        public static GraphicsPreset Preset { get { return (GraphicsPreset)PlayerPrefs.GetInt("tln.preset", DefaultPreset()); } set { PlayerPrefs.SetInt("tln.preset", (int)value); } }
        public static int TargetFps { get { return PlayerPrefs.GetInt("tln.fps", 30); } set { PlayerPrefs.SetInt("tln.fps", value); } }
        public static float ResolutionScale { get { return PlayerPrefs.GetFloat("tln.res", 0.85f); } set { PlayerPrefs.SetFloat("tln.res", Mathf.Clamp(value, 0.5f, 1f)); } }
        public static float LookSensitivity { get { return PlayerPrefs.GetFloat("tln.sens", 1f); } set { PlayerPrefs.SetFloat("tln.sens", Mathf.Clamp(value, 0.2f, 3f)); } }
        public static bool InvertY { get { return PlayerPrefs.GetInt("tln.invy", 0) == 1; } set { PlayerPrefs.SetInt("tln.invy", value ? 1 : 0); } }
        public static float MasterVolume { get { return PlayerPrefs.GetFloat("tln.vol", 1f); } set { PlayerPrefs.SetFloat("tln.vol", Mathf.Clamp01(value)); } }
        public static float MusicVolume { get { return PlayerPrefs.GetFloat("tln.music", 0.8f); } set { PlayerPrefs.SetFloat("tln.music", Mathf.Clamp01(value)); } }
        public static float HudScale { get { return PlayerPrefs.GetFloat("tln.hud", 1f); } set { PlayerPrefs.SetFloat("tln.hud", Mathf.Clamp(value, 0.7f, 1.4f)); } }
        public static float HudOpacity { get { return PlayerPrefs.GetFloat("tln.hudop", 0.8f); } set { PlayerPrefs.SetFloat("tln.hudop", Mathf.Clamp(value, 0.2f, 1f)); } }
        public static bool SprintToggle { get { return PlayerPrefs.GetInt("tln.sprtog", 0) == 1; } set { PlayerPrefs.SetInt("tln.sprtog", value ? 1 : 0); } }

        /// <summary>Shadows are allowed for the flashlight on Medium/High, for the moon only on High.</summary>
        public static bool FlashlightShadows { get { return Preset != GraphicsPreset.Low; } }
        public static bool MoonShadows { get { return Preset == GraphicsPreset.High; } }

        static int DefaultPreset()
        {
            // rough first-launch guess from device memory
            int mem = SystemInfo.systemMemorySize;
            return mem >= 6000 ? 2 : mem >= 3500 ? 1 : 0;
        }

        public static void Save() { PlayerPrefs.Save(); Apply(); }

        public static void Apply()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = TargetFps;
            AudioListener.volume = MasterVolume;

            var urp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (urp != null)
            {
                // NOTE: in the Editor this edits the URP asset on disk (Unity behaviour). Harmless; values are re-applied at launch.
                float presetScale = Preset == GraphicsPreset.Low ? 0.75f : Preset == GraphicsPreset.Medium ? 0.9f : 1f;
                urp.renderScale = Mathf.Clamp(ResolutionScale * presetScale, 0.4f, 1f);
                urp.msaaSampleCount = Preset == GraphicsPreset.High ? 2 : 1;
                urp.shadowDistance = Preset == GraphicsPreset.Low ? 0f : Preset == GraphicsPreset.Medium ? 18f : 30f;
            }
            else Debug.LogWarning("[TLN] URP asset not active — graphics presets only partly applied. See SETUP.md step 1.");

            RenderSettings.fogDensity = Preset == GraphicsPreset.Low ? 0.075f : 0.06f; // denser fog on Low hides less draw distance
            if (Changed != null) Changed();
        }
    }
}
