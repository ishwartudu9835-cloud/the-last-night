// Touch button for the mobile HUD: press/release (hold actions) and/or tap, with
// a ring + icon look, pressed/active states and an optional radial progress fill.
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastNight
{
    public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public Action<bool> onHeld;   // true on press, false on release
        public Action onTap;
        public Image background, ring, icon, progress;
        public Color idleRing = new Color(0.88f, 0.84f, 0.76f, 0.55f);
        public Color activeRing = new Color(1f, 0.62f, 0.25f, 0.95f);
        public Color pressedRing = new Color(0.85f, 0.12f, 0.08f, 1f);
        public bool IsActive;
        bool down;
        float scale = 1f;

        public void OnPointerDown(PointerEventData e) { down = true; if (onHeld != null) onHeld(true); if (onTap != null) onTap(); }
        public void OnPointerUp(PointerEventData e) { Release(); }
        public void OnPointerExit(PointerEventData e) { if (down && e.pointerPress != gameObject) Release(); }
        void Release() { if (!down) return; down = false; if (onHeld != null) onHeld(false); }
        void OnDisable() { if (down) Release(); }

        public void SetActiveVisual(bool on) { IsActive = on; }
        public void SetProgress(float v) { if (progress != null) progress.fillAmount = v; }

        void Update()
        {
            // small squash on press, colour by state
            scale = Mathf.MoveTowards(scale, down ? 0.9f : 1f, Time.unscaledDeltaTime * 4f);
            transform.localScale = Vector3.one * scale;
            Color c = down ? pressedRing : IsActive ? activeRing : idleRing;
            if (ring != null) ring.color = Color.Lerp(ring.color, c, Time.unscaledDeltaTime * 18f);
            if (icon != null) icon.color = Color.Lerp(icon.color, IsActive && !down ? activeRing : new Color(0.92f, 0.88f, 0.8f, 0.95f), Time.unscaledDeltaTime * 18f);
            if (background != null) background.color = new Color(0f, 0f, 0f, down ? 0.55f : 0.35f);
        }
    }
}
