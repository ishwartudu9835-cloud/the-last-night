// Touch widget that writes into InputBridge. Built on uGUI pointer events, so it works with
// both the Input System UI module and the legacy StandaloneInputModule, and supports multi-touch.
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TheLastNight
{
    /// <summary>Button that reports press/release (for hold actions) and/or fires a tap.</summary>
    public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public Action<bool> onHeld;   // true on press, false on release
        public Action onTap;
        public UnityEngine.UI.Image graphic;
        public Color normal = new Color(1f, 1f, 1f, 0.18f), pressed = new Color(1f, 1f, 1f, 0.45f), active = new Color(1f, 0.75f, 0.3f, 0.45f);
        public bool IsActive;         // toggled visual state (crouch / flashlight)
        bool down;

        public void OnPointerDown(PointerEventData e) { down = true; if (onHeld != null) onHeld(true); if (onTap != null) onTap(); Refresh(); }
        public void OnPointerUp(PointerEventData e) { Release(); }
        public void OnPointerExit(PointerEventData e) { if (down && e.pointerPress != gameObject) Release(); }
        void Release() { if (!down) return; down = false; if (onHeld != null) onHeld(false); Refresh(); }
        void OnDisable() { if (down) Release(); }

        public void SetActiveVisual(bool on) { IsActive = on; Refresh(); }
        void Refresh() { if (graphic != null) graphic.color = down ? pressed : IsActive ? active : normal; }
    }
}
