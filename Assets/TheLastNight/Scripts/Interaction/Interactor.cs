// Interactor: finds what the player is looking at (forgiving sphere-cast for touch screens),
// handles tap vs. hold interactions and exposes prompt/progress for the HUD.
using UnityEngine;

namespace TheLastNight
{
    public interface IInteractable
    {
        string Prompt(PlayerController p);         // e.g. "Darwaza kholo"; null = not interactable now
        float HoldTime(PlayerController p);        // 0 = instant tap
        void Interact(PlayerController p);
        Transform transform { get; }
    }

    public class Interactor : MonoBehaviour
    {
        public float reach = 2.4f;
        public float castRadius = 0.18f;
        public LayerMask mask = ~0;

        public IInteractable Current { get; private set; }
        public string CurrentPrompt { get; private set; }
        public float HoldProgress { get; private set; }     // 0..1 while holding

        PlayerController pc;
        float held;
        IInteractable heldTarget;
        bool waitRelease;

        void Awake() { pc = GetComponent<PlayerController>(); }

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || !gm.IsPlaying || pc.Health.IsDead) { Clear(); return; }
            InputBridge.Poll();

            Current = pc.IsHidden ? (IInteractable)pc.Hiding : FindTarget();
            CurrentPrompt = Current != null ? Current.Prompt(pc) : null;
            if (CurrentPrompt == null) Current = null;

            if (!InputBridge.InteractHeld) waitRelease = false;
            if (Current == null || waitRelease) { held = 0f; HoldProgress = 0f; heldTarget = null; return; }

            float need = Current.HoldTime(pc);
            if (need <= 0f)
            {
                if (InputBridge.InteractPressed) { Current.Interact(pc); waitRelease = true; }
                HoldProgress = 0f;
                return;
            }
            if (InputBridge.InteractHeld)
            {
                if (heldTarget != Current) { heldTarget = Current; held = 0f; }
                held += Time.deltaTime;
                HoldProgress = Mathf.Clamp01(held / need);
                if (held >= need) { Current.Interact(pc); held = 0f; HoldProgress = 0f; waitRelease = true; }
            }
            else { held = 0f; HoldProgress = 0f; heldTarget = null; }
        }

        IInteractable FindTarget()
        {
            Transform c = pc.cam.transform;
            RaycastHit hit;
            if (Physics.SphereCast(c.position, castRadius, c.forward, out hit, reach, mask, QueryTriggerInteraction.Collide))
            {
                var it = hit.collider.GetComponentInParent<IInteractable>();
                if (it != null) return it;
            }
            return null;
        }

        void Clear() { Current = null; CurrentPrompt = null; HoldProgress = 0f; held = 0f; }
    }
}
