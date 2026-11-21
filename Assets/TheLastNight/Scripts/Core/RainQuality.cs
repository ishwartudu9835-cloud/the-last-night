// Scales rain particle density with the graphics preset (fill-rate is the main mobile cost).
using UnityEngine;

namespace TheLastNight
{
    [RequireComponent(typeof(ParticleSystem))]
    public class RainQuality : MonoBehaviour
    {
        void Start() { GameSettings.Changed += Apply; Apply(); }
        void OnDestroy() { GameSettings.Changed -= Apply; }

        void Apply()
        {
            var em = GetComponent<ParticleSystem>().emission;
            em.rateOverTime = GameSettings.Preset == GraphicsPreset.Low ? 220f : GameSettings.Preset == GraphicsPreset.Medium ? 450f : 700f;
        }
    }
}
