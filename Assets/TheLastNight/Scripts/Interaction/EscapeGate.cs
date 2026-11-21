// Escape gate: powered by the generator, slides open slowly (metal grinding),
// and a trigger zone beyond it ends the level.
using System.Collections;
using UnityEngine;

namespace TheLastNight
{
    public class EscapeGate : MonoBehaviour
    {
        public Transform leftPanel, rightPanel;
        public float slideDistance = 2.2f;
        public float openSeconds = 6f;
        public Collider blocker;           // solid collider removed when open
        public Light signalLight;

        bool opening;
        public bool IsOpening { get { return opening; } }

        public void Open()
        {
            if (opening) return;
            opening = true;
            StartCoroutine(OpenRoutine());
        }

        IEnumerator OpenRoutine()
        {
            yield return new WaitForSeconds(1.5f);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayAt(ProceduralAudio.MetalDrag, transform.position + Vector3.up * 1.5f, 1f, 0.7f);
            NoiseSystem.Emit(transform.position, 18f, NoiseKind.Object, null);
            Vector3 l0 = leftPanel.localPosition, r0 = rightPanel.localPosition;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / openSeconds;
                float e = Mathf.SmoothStep(0f, 1f, t);
                leftPanel.localPosition = l0 + Vector3.left * slideDistance * e;
                rightPanel.localPosition = r0 + Vector3.right * slideDistance * e;
                yield return null;
            }
            if (blocker != null) blocker.enabled = false;
            if (signalLight != null) signalLight.color = Color.green;
            if (GameManager.Instance != null) GameManager.Instance.OnGateOpened();
        }
    }
}
