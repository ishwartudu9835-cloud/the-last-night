// Touch widget that writes into InputBridge. Built on uGUI pointer events, so it works with
// both the Input System UI module and the legacy StandaloneInputModule, and supports multi-touch.
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TheLastNight
{
    /// <summary>Floating joystick: appears where the thumb lands inside its zone.</summary>
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public RectTransform baseRect, knob;
        public float radius = 110f;
        int pointerId = int.MinValue;
        RectTransform zone;

        void Awake() { zone = (RectTransform)transform; baseRect.gameObject.SetActive(false); }

        public void OnPointerDown(PointerEventData e)
        {
            if (pointerId != int.MinValue) return;
            pointerId = e.pointerId;
            Vector2 local;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(zone, e.position, e.pressEventCamera, out local);
            baseRect.anchoredPosition = local;
            baseRect.gameObject.SetActive(true);
            OnDrag(e);
        }

        public void OnDrag(PointerEventData e)
        {
            if (e.pointerId != pointerId) return;
            Vector2 local;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(baseRect, e.position, e.pressEventCamera, out local);
            Vector2 v = Vector2.ClampMagnitude(local, radius);
            knob.anchoredPosition = v;
            Vector2 n = v / radius;
            // small dead zone
            InputBridge.TouchMove = n.magnitude < 0.12f ? Vector2.zero : n;
        }

        public void OnPointerUp(PointerEventData e)
        {
            if (e.pointerId != pointerId) return;
            pointerId = int.MinValue;
            knob.anchoredPosition = Vector2.zero;
            baseRect.gameObject.SetActive(false);
            InputBridge.TouchMove = Vector2.zero;
        }

        void OnDisable() { pointerId = int.MinValue; InputBridge.TouchMove = Vector2.zero; if (baseRect != null) baseRect.gameObject.SetActive(false); }
    }
}
