// FlickerLight: street lamps / bulbs. Idle flicker, scripted bursts, power cuts,
// and a "Powered" flag the generator switches on.
using UnityEngine;

namespace TheLastNight
{
    [RequireComponent(typeof(Light))]
    public class FlickerLight : MonoBehaviour
    {
        public static readonly System.Collections.Generic.List<FlickerLight> All = new System.Collections.Generic.List<FlickerLight>();

        public bool Powered = true;
        public float baseIntensity = 3f;
        [Range(0f, 1f)] public float idleFlicker = 0.08f;   // 0 = steady
        public Renderer bulb;                                 // emissive mesh (optional)

        Light lt;
        Material bulbMat;
        float burst, cut, t;
        Color bulbColor;

        void Awake()
        {
            lt = GetComponent<Light>();
            if (bulb != null) { bulbMat = bulb.material; bulbColor = lt.color; }
        }
        void OnEnable() { All.Add(this); }
        void OnDisable() { All.Remove(this); }

        public void Burst(float seconds) { burst = Mathf.Max(burst, seconds); }
        public void PowerCut(float seconds) { cut = Mathf.Max(cut, seconds); }

        void Update()
        {
            float target = Powered && cut <= 0f ? baseIntensity : 0f;
            if (target > 0f)
            {
                t -= Time.deltaTime;
                float chance = burst > 0f ? 0.6f : idleFlicker;
                if (t <= 0f) { t = Random.Range(0.04f, 0.18f); if (Random.value < chance) target *= Random.Range(0f, 0.35f); }
                else if (t < 0.03f && Random.value < chance) target *= 0.2f;
            }
            if (burst > 0f) burst -= Time.deltaTime;
            if (cut > 0f) cut -= Time.deltaTime;
            lt.intensity = target;
            lt.enabled = target > 0.01f;
            if (bulbMat != null) bulbMat.SetColor("_EmissionColor", bulbColor * (target / Mathf.Max(0.01f, baseIntensity)) * 3f);
        }
    }
}
