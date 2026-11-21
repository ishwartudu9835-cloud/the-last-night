// HidingSpot: cupboard / under-bed. Not 100% safe: if the monster saw you get in,
// or rolls its search check, it will open the spot and pull you out.
using System.Collections.Generic;
using UnityEngine;

namespace TheLastNight
{
    public class HidingSpot : MonoBehaviour, IInteractable
    {
        public static readonly List<HidingSpot> All = new List<HidingSpot>();

        public Transform hidePoint;          // where the player's feet go
        public Transform exitPoint;          // where the player steps out
        public Transform monsterCheckPoint;  // where the monster stands to search it
        public float eyeHeight = 0.9f;       // camera height while hidden
        public Transform doorVisual;         // optional: cupboard door that cracks open
        public string label = "Almari";

        public PlayerController Occupant { get; private set; }
        public float EnteredAt { get; private set; }

        void OnEnable() { All.Add(this); }
        void OnDisable() { All.Remove(this); }

        public string Prompt(PlayerController p)
        {
            if (Occupant == p) return "Bahar niklo";
            if (Occupant != null) return null;
            return label + " mein chhupo";
        }

        public float HoldTime(PlayerController p) { return 0f; }

        public void Interact(PlayerController p)
        {
            if (Occupant == p) { Leave(); return; }
            if (Occupant != null) return;
            Occupant = p;
            EnteredAt = Time.time;
            p.EnterHiding(this);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayAt(ProceduralAudio.DoorCreak, transform.position + Vector3.up, 0.35f, 1.3f);
            NoiseSystem.Emit(transform.position, 3.5f, NoiseKind.Object, p.gameObject);
            SetDoor(true);
        }

        public void Leave()
        {
            if (Occupant == null) return;
            var p = Occupant;
            Occupant = null;
            p.ExitHiding();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayAt(ProceduralAudio.DoorCreak, transform.position + Vector3.up, 0.35f, 1.2f);
            SetDoor(false);
        }

        /// <summary>Called by the monster when it searches this spot. Returns the pulled-out player or null.</summary>
        public PlayerController MonsterSearch()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayAt(ProceduralAudio.DoorSlam, transform.position + Vector3.up, 0.9f, 0.8f);
            SetDoor(true);
            Invoke("CloseDoorVisual", 1.2f);
            if (Occupant == null) return null;
            var p = Occupant;
            Leave();
            return p;
        }

        void CloseDoorVisual() { if (Occupant == null) SetDoor(false); }

        void SetDoor(bool ajar)
        {
            if (doorVisual != null) doorVisual.localRotation = Quaternion.Euler(0f, ajar ? 35f : 0f, 0f);
        }
    }
}
