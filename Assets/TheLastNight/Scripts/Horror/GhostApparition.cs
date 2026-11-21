// GhostApparition: a pale figure that appears at the edge of vision and vanishes when looked at
// (or when you get close). One instance is reused by the HorrorDirector.
using UnityEngine;

namespace TheLastNight
{
    public class GhostApparition : MonoBehaviour
    {
        public float vanishLookAngle = 9f;
        public float vanishDistance = 3.5f;

        float life;
        Transform viewer;
        bool jumpscare;

        public bool Visible { get { return gameObject.activeSelf; } }

        void Awake() { gameObject.SetActive(false); }

        public void Show(Vector3 pos, Transform cam, float seconds, bool isJumpscare)
        {
            viewer = cam;
            jumpscare = isJumpscare;
            life = seconds;
            Vector3 look = cam.position - pos; look.y = 0f;
            transform.SetPositionAndRotation(pos, Quaternion.LookRotation(look.sqrMagnitude > 0.01f ? look : Vector3.forward));
            gameObject.SetActive(true);
        }

        void Update()
        {
            life -= Time.deltaTime;
            if (viewer == null || life <= 0f) { Hide(); return; }
            if (jumpscare) return; // jumpscare just times out
            Vector3 to = transform.position + Vector3.up * 1.5f - viewer.position;
            if (to.magnitude < vanishDistance || Vector3.Angle(viewer.forward, to) < vanishLookAngle)
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayAt(ProceduralAudio.RadioStatic, transform.position + Vector3.up, 0.35f, 1.4f, 12f);
                Hide();
            }
        }

        public void Hide() { gameObject.SetActive(false); }
    }
}
