// Billboard: keeps a quad (lamp halo, glow) facing the active camera. Fades with distance.
using UnityEngine;

namespace TheLastNight
{
    public class Billboard : MonoBehaviour
    {
        public float maxVisibleDistance = 35f;
        public FlickerLight followIntensity;   // halo dims with its lamp
        Renderer rend;
        Material mat;
        Color baseColor;

        void Awake()
        {
            rend = GetComponent<Renderer>();
            if (rend != null) { mat = rend.material; baseColor = mat.GetColor("_BaseColor"); }
        }

        void LateUpdate()
        {
            var cam = Camera.main;
            if (cam == null) return;
            transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position);
            if (mat == null) return;
            float k = 1f;
            if (followIntensity != null)
            {
                var l = followIntensity.GetComponent<Light>();
                k = l != null && l.enabled ? Mathf.Clamp01(l.intensity / Mathf.Max(0.01f, followIntensity.baseIntensity)) : 0f;
            }
            float d = Vector3.Distance(cam.transform.position, transform.position);
            k *= Mathf.Clamp01(1.2f - d / maxVisibleDistance);
            mat.SetColor("_BaseColor", new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * k));
        }
    }
}
