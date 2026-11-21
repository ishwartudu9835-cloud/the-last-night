// Trigger volume placed just beyond the escape gate (own file: Unity needs class name == file name).
using UnityEngine;

namespace TheLastNight
{
    /// <summary>Trigger volume placed just beyond the gate.</summary>
    public class EscapeZone : MonoBehaviour
    {
        void OnTriggerEnter(Collider other)
        {
            var gm = GameManager.Instance;
            if (gm == null || !gm.GateOpen) return;
            if (other.GetComponentInParent<PlayerController>() != null) gm.PlayerEscaped();
        }
    }
}
