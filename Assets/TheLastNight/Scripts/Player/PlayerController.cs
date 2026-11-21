// First-person controller for mobile + PC: walk / sprint (stamina) / crouch, footsteps that
// make noise, head-bob, and the "hidden" state used by hiding spots.
using UnityEngine;

namespace TheLastNight
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("References")]
        public Transform head;              // camera pivot (child)
        public Camera cam;

        [Header("Movement")]
        public float walkSpeed = 2.6f;
        public float sprintSpeed = 5.2f;
        public float crouchSpeed = 1.3f;
        public float standHeight = 1.75f;
        public float crouchHeight = 1.0f;
        public float gravity = -18f;

        [Header("Stamina")]
        public float staminaMax = 5f;       // seconds of sprint
        public float staminaRegen = 0.8f;   // per second
        public float tiredRecoverAt = 1.8f;

        [Header("Footsteps")]
        public float stepDistance = 2.1f;

        public bool IsCrouching { get; private set; }
        public bool IsSprinting { get; private set; }
        public bool IsMoving { get; private set; }
        public bool IsHidden { get { return Hiding != null; } }
        public HidingSpot Hiding { get; private set; }
        public float Stamina { get; private set; }
        public bool Tired { get; private set; }
        public float Stamina01 { get { return Stamina / staminaMax; } }
        public Vector3 HeadPosition { get { return head.position; } }

        public Flashlight Flashlight { get; private set; }
        public PlayerHealth Health { get; private set; }
        public Inventory Inventory { get; private set; }

        CharacterController cc;
        float pitch, yVel, stepAccum, bobPhase, currentHeight;
        bool sprintLatched;
        Vector3 headBase;

        void Awake()
        {
            cc = GetComponent<CharacterController>();
            Flashlight = GetComponentInChildren<Flashlight>();
            Health = GetComponent<PlayerHealth>();
            Inventory = GetComponent<Inventory>();
            Stamina = staminaMax;
            currentHeight = standHeight;
            headBase = head.localPosition;
        }

        void Start() { if (GameManager.Instance != null) GameManager.Instance.Register(this); }

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || !gm.IsPlaying) return;
            InputBridge.Poll();
            if (Health != null && Health.IsDead) return;

            Look();
            if (IsHidden) { HiddenUpdate(); return; }
            Move();
        }

        void Look()
        {
            Vector2 l = InputBridge.Look;
            transform.Rotate(0f, l.x, 0f, Space.Self);
            pitch = Mathf.Clamp(pitch - l.y, -80f, 80f);
            head.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        void Move()
        {
            if (InputBridge.CrouchPressed)
            {
                if (!IsCrouching) IsCrouching = true;
                else if (CanStand()) IsCrouching = false;
            }
            Vector2 mv = InputBridge.Move;
            IsMoving = mv.sqrMagnitude > 0.02f;

            // sprint (hold or toggle) — only forward-ish, not crouched, not tired
            if (GameSettings.SprintToggle) { if (InputBridge.Sprint && !sprintLatched) { sprintLatched = true; IsSprinting = !IsSprinting; } if (!InputBridge.Sprint) sprintLatched = false; }
            else IsSprinting = InputBridge.Sprint;
            if (!IsMoving || mv.y < 0.3f || IsCrouching || Tired) IsSprinting = false;
            
            if (IsSprinting) { Stamina -= Time.deltaTime; if (Stamina <= 0f) { Stamina = 0f; Tired = true; IsSprinting = false; } }
            else { Stamina = Mathf.Min(staminaMax, Stamina + staminaRegen * Time.deltaTime); if (Tired && Stamina >= tiredRecoverAt) Tired = false; }

            float speed = IsCrouching ? crouchSpeed : IsSprinting ? sprintSpeed : walkSpeed;
            if (Health != null) speed *= Health.SpeedMultiplier;
            Vector3 dir = transform.right * mv.x + transform.forward * mv.y;
            if (dir.sqrMagnitude > 1f) dir.Normalize();

            yVel = cc.isGrounded ? -2f : yVel + gravity * Time.deltaTime;
            Vector3 vel = dir * speed + Vector3.up * yVel;
            cc.Move(vel * Time.deltaTime);

            // crouch height
            float targetH = IsCrouching ? crouchHeight : standHeight;
            currentHeight = Mathf.MoveTowards(currentHeight, targetH, Time.deltaTime * 4f);
            cc.height = currentHeight;
            cc.center = new Vector3(0f, currentHeight * 0.5f, 0f);

            // footsteps + head bob
            float planar = new Vector3(cc.velocity.x, 0f, cc.velocity.z).magnitude;
            if (cc.isGrounded && planar > 0.3f)
            {
                stepAccum += planar * Time.deltaTime;
                float stepLen = IsSprinting ? stepDistance * 1.35f : IsCrouching ? stepDistance * 0.7f : stepDistance;
                if (stepAccum >= stepLen) { stepAccum = 0f; Footstep(); }
                bobPhase += planar * Time.deltaTime * (IsSprinting ? 2.4f : 2.9f);
            }
            float bobAmp = IsSprinting ? 0.06f : IsCrouching ? 0.015f : 0.035f;
            Vector3 bob = new Vector3(Mathf.Cos(bobPhase) * bobAmp * 0.5f, Mathf.Abs(Mathf.Sin(bobPhase)) * bobAmp, 0f);
            Vector3 hb = new Vector3(headBase.x, currentHeight - 0.12f, headBase.z);
            head.localPosition = Vector3.Lerp(head.localPosition, hb + (IsMoving ? bob : Vector3.zero), Time.deltaTime * 12f);
        }

        bool CanStand()
        {
            // don't stand up into a ceiling (beds, low beams)
            Vector3 p = transform.position + Vector3.up * (crouchHeight);
            RaycastHit hit;
            return !Physics.SphereCast(p, cc.radius * 0.9f, Vector3.up, out hit, standHeight - crouchHeight, ~0, QueryTriggerInteraction.Ignore);
        }

        void Footstep()
        {
            NoiseKind kind = IsSprinting ? NoiseKind.Sprint : NoiseKind.Footstep;
            float radius = NoiseSystem.DefaultRadius(kind) * (IsCrouching ? 0.35f : 1f);
            NoiseSystem.Emit(transform.position, radius, kind, gameObject);
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayFootstep(transform.position, IsSprinting ? 0.75f : IsCrouching ? 0.22f : 0.45f);
        }

        // ---------------- hiding ----------------
        public void EnterHiding(HidingSpot spot)
        {
            Hiding = spot;
            cc.enabled = false;
            IsSprinting = false;
            transform.position = spot.hidePoint.position;
            transform.rotation = Quaternion.Euler(0f, spot.hidePoint.eulerAngles.y, 0f);
            pitch = 0f;
            head.localPosition = new Vector3(headBase.x, spot.eyeHeight, headBase.z);
        }

        public void ExitHiding()
        {
            if (Hiding == null) return;
            Transform exit = Hiding.exitPoint;
            Hiding = null;
            transform.position = exit.position;
            transform.rotation = Quaternion.Euler(0f, exit.eulerAngles.y, 0f);
            cc.enabled = true;
        }

        void HiddenUpdate()
        {
            // limited look while hidden; stamina recovers; interact = leave
            Stamina = Mathf.Min(staminaMax, Stamina + staminaRegen * 1.5f * Time.deltaTime);
            if (Tired && Stamina >= tiredRecoverAt) Tired = false;
            IsMoving = false;
        }

        /// <summary>Teleport used by monster "pull-out" from a hiding spot.</summary>
        public void ForceOutOfHiding() { ExitHiding(); }

        /// <summary>0..1 how easy the player is to spot (used by MonsterAI).</summary>
        public float VisibilityFactor()
        {
            if (IsHidden) return 0f;
            float v = 1f;
            if (Flashlight != null && Flashlight.IsOn) v *= 1.7f;
            if (IsCrouching) v *= 0.6f;
            if (Health != null) v *= 1f + Health.Fear * 0.3f;   // story rule: it sees fear
            return v;
        }
    }
}
