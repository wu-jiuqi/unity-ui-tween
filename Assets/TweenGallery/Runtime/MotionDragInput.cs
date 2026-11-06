using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TweenGallery
{
    /// <summary>
    /// Explicit drag surface. Unlike MotionButtonInput this is an IDragHandler and
    /// intentionally consumes drag, so do not put it on an ordinary scrolling button.
    /// It keeps tracking the captured pointer after leaving the fixed hit area.
    /// </summary>
    public sealed class MotionDragInput : MonoBehaviour, IPointerDownHandler, IPointerUpHandler,
        IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, ICancelHandler
    {
        [Serializable] public sealed class Vector2Event : UnityEvent<Vector2> { }
        public RectTransform hitArea;
        public MotionJellyDrag jelly;
        public UnityEvent onDragStarted = new UnityEvent();
        public Vector2Event onDisplacementChanged = new Vector2Event();
        public UnityEvent onReleased = new UnityEvent();
        public UnityEvent onCancelled = new UnityEvent();
        private int pointerId = int.MinValue;
        private PointerEventData activeEvent;
        private Vector2 start;
        private bool dragging;
        private Selectable selectable;
        public bool HasActivePointer => pointerId != int.MinValue;
        public bool IsDragging => dragging;
        public int ActivePointerId => pointerId;
        private void Awake() { RefreshBindings(); }
        private void OnTransformParentChanged() { RefreshBindings(); }
        private void OnTransformChildrenChanged() { RefreshBindings(); }
        private void Update() { if (HasActivePointer && selectable != null && (!selectable.isActiveAndEnabled || !selectable.IsInteractable())) CancelInteraction(); }
        public void RefreshBindings() { if (hitArea == null) hitArea = transform as RectTransform; selectable = GetComponent<Selectable>(); }
        public void OnPointerDown(PointerEventData e)
        {
            if (e == null || e.button != PointerEventData.InputButton.Left) return;
            if (HasActivePointer) { if (pointerId != e.pointerId) e.eligibleForClick = false; return; }
            if (!isActiveAndEnabled || hitArea == null || (selectable != null && (!selectable.isActiveAndEnabled || !selectable.IsInteractable()))) { e.eligibleForClick = false; return; }
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(hitArea,e.position,e.pressEventCamera,out start)) return;
            pointerId = e.pointerId; activeEvent = e; dragging = false;
        }
        public void OnInitializePotentialDrag(PointerEventData e) { if (IsActive(e)) e.useDragThreshold = true; }
        public void OnBeginDrag(PointerEventData e)
        {
            if (!IsActive(e) || dragging) return;
            dragging = true; e.eligibleForClick = false;
            if (jelly != null) jelly.BeginDrag();
            onDragStarted.Invoke();
            OnDrag(e);
        }
        public void OnDrag(PointerEventData e)
        {
            if (!IsActive(e) || !dragging || hitArea == null) return;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(hitArea,e.position,e.pressEventCamera,out var local)) return;
            Vector2 delta = local-start;
            if (jelly != null) jelly.Drag(delta);
            onDisplacementChanged.Invoke(delta);
        }
        public void OnEndDrag(PointerEventData e) { if (IsActive(e)) Finish(e, false); }
        public void OnPointerUp(PointerEventData e)
        {
            if (!IsActive(e)) return;
            // EventSystem dispatches pointer-up before end-drag; keep capture until end-drag.
            if (dragging || e.dragging) { e.eligibleForClick = false; return; }
            Finish(e, false);
        }
        public void OnCancel(BaseEventData e) { CancelInteraction(); }
        public void CancelInteraction() { if (HasActivePointer) Finish(activeEvent, true); }
        private void Finish(PointerEventData e, bool cancelled)
        {
            bool wasDragging = dragging;
            if (e != null && (cancelled || wasDragging)) e.eligibleForClick = false;
            pointerId = int.MinValue; activeEvent = null; dragging = false;
            if (cancelled) { if (jelly != null) jelly.CancelDrag(); onCancelled.Invoke(); }
            else if (wasDragging) { if (jelly != null) jelly.EndDrag(); onReleased.Invoke(); }
        }
        private bool IsActive(PointerEventData e) => e != null && HasActivePointer && e.pointerId == pointerId;
        private void OnDisable() { CancelInteraction(); }
        private void OnApplicationFocus(bool focus) { if (!focus) CancelInteraction(); }
        private void OnApplicationPause(bool pause) { if (pause) CancelInteraction(); }
    }
}
