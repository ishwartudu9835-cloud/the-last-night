// InputBridge: single place where gameplay code reads input.
// Mobile UI widgets (joystick, look pad, buttons) WRITE into this class,
// keyboard/mouse (editor testing) is merged in Poll().
// Works with both the new Input System package and the legacy Input Manager.
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace TheLastNight
{
    public static class InputBridge
    {
        // ---- written by mobile UI ----
        public static Vector2 TouchMove;        // virtual joystick, -1..1
        public static Vector2 TouchLookDelta;   // pixels this frame, consumed in Poll()
        public static bool TouchSprintHeld;
        public static bool TouchInteractHeld;
        public static bool TouchCrouchToggle;   // one-shot
        public static bool TouchFlashlightToggle;
        public static bool TouchInventoryToggle;
        public static bool TouchPause;

        // ---- read by gameplay (valid after Poll) ----
        public static Vector2 Move { get; private set; }
        public static Vector2 Look { get; private set; }   // degrees this frame
        public static bool Sprint { get; private set; }
        public static bool InteractHeld { get; private set; }
        public static bool InteractPressed { get; private set; }
        public static bool CrouchPressed { get; private set; }
        public static bool FlashlightPressed { get; private set; }
        public static bool InventoryPressed { get; private set; }
        public static bool PausePressed { get; private set; }
        public static bool ReloadPressed { get; private set; }

        static bool lastInteract;
        static int lastFrame = -1;

        /// <summary>Call once per frame before reading (PlayerController does this).</summary>
        public static void Poll()
        {
            if (lastFrame == Time.frameCount) return;
            lastFrame = Time.frameCount;

            float sens = GameSettings.LookSensitivity;
            Vector2 move = TouchMove;
            Vector2 look = TouchLookDelta * (0.12f * sens);
            TouchLookDelta = Vector2.zero;
            bool sprint = TouchSprintHeld, interact = TouchInteractHeld;
            bool crouch = TouchCrouchToggle, flash = TouchFlashlightToggle, inv = TouchInventoryToggle, pause = TouchPause, reload = false;
            TouchCrouchToggle = TouchFlashlightToggle = TouchInventoryToggle = TouchPause = false;

#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb != null)
            {
                Vector2 k = Vector2.zero;
                if (kb.wKey.isPressed) k.y += 1; if (kb.sKey.isPressed) k.y -= 1;
                if (kb.dKey.isPressed) k.x += 1; if (kb.aKey.isPressed) k.x -= 1;
                if (k != Vector2.zero) move = k;
                sprint |= kb.leftShiftKey.isPressed;
                interact |= kb.eKey.isPressed;
                crouch |= kb.cKey.wasPressedThisFrame;
                flash |= kb.fKey.wasPressedThisFrame;
                inv |= kb.tabKey.wasPressedThisFrame;
                pause |= kb.escapeKey.wasPressedThisFrame;
                reload |= kb.rKey.wasPressedThisFrame;
            }
            var mouse = Mouse.current;
            if (mouse != null && Cursor.lockState == CursorLockMode.Locked)
                look += mouse.delta.ReadValue() * (0.08f * sens);
#else
            Vector2 k = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (k != Vector2.zero) move = k;
            sprint |= Input.GetKey(KeyCode.LeftShift);
            interact |= Input.GetKey(KeyCode.E);
            crouch |= Input.GetKeyDown(KeyCode.C);
            flash |= Input.GetKeyDown(KeyCode.F);
            inv |= Input.GetKeyDown(KeyCode.Tab);
            pause |= Input.GetKeyDown(KeyCode.Escape);
            reload |= Input.GetKeyDown(KeyCode.R);
            if (Cursor.lockState == CursorLockMode.Locked)
                look += new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * (2.2f * sens);
#endif
            if (GameSettings.InvertY) look.y = -look.y;
            Move = Vector2.ClampMagnitude(move, 1f);
            Look = look;
            Sprint = sprint;
            InteractPressed = interact && !lastInteract;
            InteractHeld = interact;
            lastInteract = interact;
            CrouchPressed = crouch;
            FlashlightPressed = flash;
            InventoryPressed = inv;
            PausePressed = pause;
            ReloadPressed = reload;
        }

        public static void ResetAll()
        {
            TouchMove = TouchLookDelta = Vector2.zero;
            TouchSprintHeld = TouchInteractHeld = false;
            TouchCrouchToggle = TouchFlashlightToggle = TouchInventoryToggle = TouchPause = false;
        }
    }
}
