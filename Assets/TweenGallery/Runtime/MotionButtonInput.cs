using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TweenGallery
{
    /// <summary>
    /// Press feedback for a Button. This component intentionally does not implement
    /// IDragHandler: a parent ScrollRect remains the EventSystem drag receiver.
    /// Attach to the fixed raycast hit area; animate a descendant visual wrapper.
    /// The existing Button remains responsible for its own onClick.
    /// </summary>
    public sealed class MotionButtonInput : MonoBehaviour, IPointerDownHandler, IPointerUpHandler,
        IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler, ICancelHandler
    {
        public MotionPlayer visualPlayer;
        public bool rebound = true;
        [Tooltip("Release pressed visuals when the pointer leaves. Leaving still prevents a click on release.")]
        public bool releaseOnExit = true;
        private int pointerId = int.MinValue;
        private PointerEventData activeEvent;
        private Vector2 downPosition;
        private bool inside, dragCancelled, visualPressed;
        private Selectable selectable;

        public bool HasActivePointer => pointerId != int.MinValue;
        public int ActivePointerId => pointerId;
        public bool IsVisuallyPressed => visualPressed;
        public bool DragCancelled => dragCancelled;

        private void Awake() { RefreshBindings(); }
        private void OnTransformParentChanged() { RefreshBindings(); }
        private void OnTransformChildrenChanged() { RefreshBindings(); }
        private void Update() { if (HasActivePointer && selectable != null && (!selectable.isActiveAndEnabled || !selectable.IsInteractable())) CancelInteraction(); }
        public void RefreshBindings() { selectable = GetComponent<Selectable>(); }
        private bool CanAnimate()
        {
            if (visualPlayer == null || visualPlayer.bindings == null || visualPlayer.bindings.target == null) return false;
            Transform visual = visualPlayer.bindings.target;
            return visual != transform && visual.IsChildOf(transform);
        }
        public void OnPointerDown(PointerEventData e)
        {
            if (e == null || e.button != PointerEventData.InputButton.Left) return;
            if (HasActivePointer) { if (pointerId != e.pointerId) e.eligibleForClick = false; return; }
            if (!isActiveAndEnabled || (selectable != null && (!selectable.isActiveAndEnabled || !selectable.IsInteractable()))) { e.eligibleForClick = false; return; }
            pointerId = e.pointerId; activeEvent = e; downPosition = e.position;
            inside = true; dragCancelled = false; PressVisual();
        }
        public void OnPointerMove(PointerEventData e)
        {
            if (!IsActive(e)) return;
            int threshold = EventSystem.current != null ? EventSystem.current.pixelDragThreshold : 5;
            if (!dragCancelled && (e.dragging || (!e.useDragThreshold || (e.position-downPosition).sqrMagnitude >= threshold*threshold)))
            {
                dragCancelled = true; e.eligibleForClick = false; ReleaseVisual();
            }
        }
        public void OnPointerEnter(PointerEventData e) { if (IsActive(e)) { inside = true; if (!dragCancelled && !e.dragging) PressVisual(); } }
        public void OnPointerExit(PointerEventData e) { if (IsActive(e)) { inside = false; if (releaseOnExit) ReleaseVisual(); } }
        public void OnPointerUp(PointerEventData e)
        {
            if (!IsActive(e)) return;
            OnPointerMove(e);
            if (dragCancelled || e.dragging || !inside) e.eligibleForClick = false;
            ReleaseVisual(); ClearPointer();
        }
        public void OnCancel(BaseEventData e) { CancelInteraction(); }
        public void CancelInteraction()
        {
            bool hadPress = HasActivePointer || visualPressed;
            if (activeEvent != null) activeEvent.eligibleForClick = false;
            ClearPointer();
            visualPressed = false;
            if (hadPress && visualPlayer != null) visualPlayer.StopAndRestore();
        }
        private bool IsActive(PointerEventData e) => e != null && HasActivePointer && e.pointerId == pointerId;
        private void PressVisual() { if (visualPressed || !CanAnimate()) return; visualPressed = true; visualPlayer.SetPressed(true, rebound); }
        private void ReleaseVisual() { if (!visualPressed) return; visualPressed = false; if (visualPlayer != null) visualPlayer.SetPressed(false, rebound); }
        private void ClearPointer() { pointerId = int.MinValue; activeEvent = null; inside = false; dragCancelled = false; }
        private void OnDisable() { CancelInteraction(); }
        private void OnApplicationFocus(bool focus) { if (!focus) CancelInteraction(); }
        private void OnApplicationPause(bool pause) { if (pause) CancelInteraction(); }
    }
}
