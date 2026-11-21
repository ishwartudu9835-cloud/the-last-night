// Turns a light's real-time shadows on/off according to the graphics preset.
using UnityEngine;

namespace TheLastNight
{
    [RequireComponent(typeof(Light))]
    public class ShadowQualitySwitch : MonoBehaviour
    {
        public GraphicsPreset minimumPreset = GraphicsPreset.High;
        public LightShadows shadowType = LightShadows.Soft;

        void Start() { GameSettings.Changed += Apply; Apply(); }
        void OnDestroy() { GameSettings.Changed -= Apply; }
        void Apply() { GetComponent<Light>().shadows = GameSettings.Preset >= minimumPreset ? shadowType : LightShadows.None; }
    }
}
