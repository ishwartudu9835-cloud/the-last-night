// Touch widget that writes into InputBridge. Built on uGUI pointer events, so it works with
// both the Input System UI module and the legacy StandaloneInputModule, and supports multi-touch.
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TheLastNight
{
    /// <summary>Drag anywhere on the right side of the screen to look around.</summary>
    public class TouchLookPad : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        int pointerId = int.MinValue;
        public void OnPointerDown(PointerEventData e) { if (pointerId == int.MinValue) pointerId = e.pointerId; }
        public void OnDrag(PointerEventData e)
        {
            if (e.pointerId != pointerId) return;
            // normalise by screen height so feel is the same on every resolution
            InputBridge.TouchLookDelta += e.delta * (1080f / Mathf.Max(1, Screen.height));
        }
        public void OnPointerUp(PointerEventData e) { if (e.pointerId == pointerId) pointerId = int.MinValue; }
        void OnDisable() { pointerId = int.MinValue; }
    }
}
