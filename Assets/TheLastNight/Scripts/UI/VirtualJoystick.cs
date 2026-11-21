// Floating joystick: rests faintly at its home spot, jumps to wherever the thumb lands
// inside its zone, and writes the direction into InputBridge. Multi-touch safe.
using UnityEngine;
using UnityEngine.EventSystems;

namespace TheLastNight
{
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public RectTransform baseRect, knob;
        public CanvasGroup group;
        public float radius = 110f;
        public float idleAlpha = 0.35f;
        int pointerId = int.MinValue;
        RectTransform zone;
        Vector2 home;

        void Awake() { zone = (RectTransform)transform; home = baseRect.anchoredPosition; }

        public void OnPointerDown(PointerEventData e)
        {
            if (pointerId != int.MinValue) return;
            pointerId = e.pointerId;
            Vector2 local;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(zone, e.position, e.pressEventCamera, out local);
            baseRect.anchoredPosition = local + Vector2.Scale(zone.rect.size, zone.pivot - baseRect.anchorMin);
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
            InputBridge.TouchMove = n.magnitude < 0.12f ? Vector2.zero : n;
        }

        public void OnPointerUp(PointerEventData e)
        {
            if (e.pointerId != pointerId) return;
            Reset();
        }

        void Reset()
        {
            pointerId = int.MinValue;
            knob.anchoredPosition = Vector2.zero;
            baseRect.anchoredPosition = home;
            InputBridge.TouchMove = Vector2.zero;
        }

        void Update()
        {
            if (group != null) group.alpha = Mathf.MoveTowards(group.alpha, pointerId == int.MinValue ? idleAlpha : 1f, Time.unscaledDeltaTime * 6f);
        }

        void OnDisable() { if (baseRect != null) Reset(); }
    }
}
