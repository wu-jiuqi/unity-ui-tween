using UnityEngine;
using UnityEngine.EventSystems;

namespace TweenGallery
{
    /// <summary>
    /// Optional pointer-driven layers for a card. Bind surface to an inner wrapper,
    /// not the RectTransform animated by MotionPlayer, so both effects can coexist.
    /// The attached object needs a raycastable Graphic and an EventSystem.
    /// </summary>
    public sealed class MotionParallaxTilt : MonoBehaviour, IPointerEnterHandler, IPointerMoveHandler, IPointerExitHandler
    {
        public RectTransform surface, highlight, glint;
        [Range(0f,30f)] public float maxAngle = 10f;
        public float highlightTravel = 36f, glintTravel = 18f;
        [Min(.01f)] public float response = 12f;
        private RectTransform hitArea;
        private Quaternion surfaceRest;
        private Vector2 highlightRest, glintRest, pointer, current;
        private bool captured;

        private void Awake() { Capture(); }
        private void OnEnable() { if (!captured) Capture(); }
        /// <summary>Re-capture after assigning bindings dynamically or after layout changes.</summary>
        public void CaptureBaseline() { Capture(); }
        private void Capture()
        {
            hitArea = transform as RectTransform;
            surfaceRest = surface != null ? surface.localRotation : Quaternion.identity;
            highlightRest = highlight != null ? highlight.anchoredPosition : Vector2.zero;
            glintRest = glint != null ? glint.anchoredPosition : Vector2.zero;
            captured = true;
        }
        public void OnPointerEnter(PointerEventData e) { UpdatePointer(e); }
        public void OnPointerMove(PointerEventData e) { UpdatePointer(e); }
        public void OnPointerExit(PointerEventData e) { pointer = Vector2.zero; }
        private void UpdatePointer(PointerEventData e)
        {
            if (hitArea == null || !RectTransformUtility.ScreenPointToLocalPointInRectangle(hitArea, e.position, e.enterEventCamera, out var local)) return;
            Rect rect = hitArea.rect;
            if (rect.width <= .001f || rect.height <= .001f) return;
            pointer = new Vector2(Mathf.Clamp((local.x-rect.center.x)/(rect.width*.5f),-1f,1f),Mathf.Clamp((local.y-rect.center.y)/(rect.height*.5f),-1f,1f));
        }
        private void Update()
        {
            current = Vector2.Lerp(current, pointer, 1f-Mathf.Exp(-Mathf.Max(.01f,response)*Time.unscaledDeltaTime));
            if (surface != null) surface.localRotation = surfaceRest * Quaternion.Euler(-current.y*maxAngle,current.x*maxAngle,0f);
            if (highlight != null) highlight.anchoredPosition = highlightRest + current * highlightTravel;
            if (glint != null) glint.anchoredPosition = glintRest - current * glintTravel;
        }
        private void OnDisable()
        {
            if (!captured) return;
            pointer = current = Vector2.zero;
            if (surface != null) surface.localRotation = surfaceRest;
            if (highlight != null) highlight.anchoredPosition = highlightRest;
            if (glint != null) glint.anchoredPosition = glintRest;
        }
    }
}
