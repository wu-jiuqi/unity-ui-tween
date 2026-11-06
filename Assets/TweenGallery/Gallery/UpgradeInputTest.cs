using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TweenGallery
{
    /// <summary>Player-mode EventSystem regression checks. These simulate pointer events;
    /// they do not claim validation on physical touch hardware.</summary>
    public static class UpgradeInputTest
    {
        public static IEnumerator Run(Action<bool, string> check)
        {
            if (check == null) throw new ArgumentNullException(nameof(check));
            var existing = EventSystem.current;
            GameObject eventObject = null;
            if (existing == null) { eventObject = new GameObject("Upgrade input test EventSystem", typeof(EventSystem), typeof(StandaloneInputModule)); existing = eventObject.GetComponent<EventSystem>(); }
            int oldThreshold = existing.pixelDragThreshold;
            existing.pixelDragThreshold = 8;
            var host = new GameObject("Upgrade input test", typeof(RectTransform), typeof(Canvas));
            var canvas = host.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.enabled = false;
            var scrollObject = new GameObject("ScrollRect", typeof(RectTransform), typeof(ScrollRect));
            scrollObject.transform.SetParent(host.transform, false);
            var scroll = scrollObject.GetComponent<ScrollRect>();
            var hit = new GameObject("ButtonHitArea", typeof(RectTransform), typeof(Image), typeof(Button), typeof(MotionButtonInput));
            hit.transform.SetParent(scrollObject.transform, false);
            var hitRect = hit.GetComponent<RectTransform>(); hitRect.sizeDelta = new Vector2(200, 100);
            var visual = new GameObject("MotionVisual", typeof(RectTransform)); visual.transform.SetParent(hit.transform, false);
            var motion = visual.AddComponent<MotionPlayer>(); motion.bindings = new MotionBindings { target = visual.GetComponent<RectTransform>() }; motion.CaptureBaseline();
            var input = hit.GetComponent<MotionButtonInput>(); input.visualPlayer = motion; input.RefreshBindings();
            try
            {
                check(ExecuteEvents.GetEventHandler<IDragHandler>(hit) == scrollObject, "Ordinary button leaves drag ownership to parent ScrollRect");
                var first = Pointer(existing, 11, new Vector2(100, 100));
                var second = Pointer(existing, 12, new Vector2(110, 100));
                ExecuteEvents.Execute(hit, first, ExecuteEvents.pointerDownHandler);
                ExecuteEvents.Execute(hit, second, ExecuteEvents.pointerDownHandler);
                check(input.HasActivePointer && input.ActivePointerId == 11 && !second.eligibleForClick && input.IsVisuallyPressed,
                    "Second touch cannot take over the active pointer or retain click eligibility");
                first.position = new Vector2(108, 100);
                ExecuteEvents.Execute(hit, first, ExecuteEvents.pointerMoveHandler);
                check(input.DragCancelled && !first.eligibleForClick && !input.IsVisuallyPressed,
                    "EventSystem pixel drag threshold cancels press and click");
                ExecuteEvents.Execute(hit, first, ExecuteEvents.pointerUpHandler);
                check(!input.HasActivePointer && !first.eligibleForClick, "Threshold drag releases without a click");

                var outside = Pointer(existing, 13, new Vector2(100, 100));
                ExecuteEvents.Execute(hit, outside, ExecuteEvents.pointerDownHandler);
                ExecuteEvents.Execute(hit, outside, ExecuteEvents.pointerExitHandler);
                outside.position = new Vector2(400, 400);
                ExecuteEvents.Execute(hit, outside, ExecuteEvents.pointerUpHandler);
                check(!input.HasActivePointer && !outside.eligibleForClick && !input.IsVisuallyPressed,
                    "Release outside button clears state and click eligibility");

                var focus = Pointer(existing, 14, new Vector2(100, 100));
                ExecuteEvents.Execute(hit, focus, ExecuteEvents.pointerDownHandler);
                hit.SendMessage("OnApplicationFocus", false, SendMessageOptions.DontRequireReceiver);
                check(!input.HasActivePointer && !focus.eligibleForClick && !input.IsVisuallyPressed,
                    "Losing application focus cancels active press and click");
                var disabled = Pointer(existing, 15, new Vector2(100, 100));
                ExecuteEvents.Execute(hit, disabled, ExecuteEvents.pointerDownHandler);
                input.enabled = false;
                check(!input.HasActivePointer && !disabled.eligibleForClick && !input.IsVisuallyPressed,
                    "Disabling input cancels active press and click");
                input.enabled = true;

                var dragObject = new GameObject("ExplicitDragHitArea", typeof(RectTransform), typeof(Image), typeof(MotionDragInput));
                dragObject.transform.SetParent(scrollObject.transform, false);
                var drag = dragObject.GetComponent<MotionDragInput>();
                drag.hitArea = dragObject.GetComponent<RectTransform>(); drag.hitArea.sizeDelta = new Vector2(200, 100); drag.RefreshBindings();
                check(ExecuteEvents.GetEventHandler<IDragHandler>(dragObject) == dragObject,
                    "Explicit draggable surface owns its drag events");
                var dragEvent = Pointer(existing, 16, new Vector2(100, 100));
                int moves = 0, released = 0, cancelled = 0; Vector2 delta = Vector2.zero;
                drag.onDisplacementChanged.AddListener(v => { moves++; delta = v; });
                drag.onReleased.AddListener(() => released++); drag.onCancelled.AddListener(() => cancelled++);
                ExecuteEvents.Execute(dragObject, dragEvent, ExecuteEvents.pointerDownHandler);
                dragEvent.position = new Vector2(500, 400);
                ExecuteEvents.Execute(dragObject, dragEvent, ExecuteEvents.beginDragHandler);
                dragEvent.dragging = true;
                ExecuteEvents.Execute(dragObject, dragEvent, ExecuteEvents.dragHandler);
                ExecuteEvents.Execute(dragObject, dragEvent, ExecuteEvents.pointerUpHandler);
                bool capturedUntilEnd = drag.HasActivePointer && drag.IsDragging;
                ExecuteEvents.Execute(dragObject, dragEvent, ExecuteEvents.endDragHandler);
                check(moves > 0 && delta.magnitude > 200 && capturedUntilEnd && released == 1 && !drag.HasActivePointer && !dragEvent.eligibleForClick,
                    "Drag continues outside hit area until end-drag, releases once and does not click");
                var interrupted = Pointer(existing, 17, new Vector2(100, 100));
                ExecuteEvents.Execute(dragObject, interrupted, ExecuteEvents.pointerDownHandler);
                ExecuteEvents.Execute(dragObject, interrupted, ExecuteEvents.beginDragHandler);
                dragObject.SendMessage("OnApplicationPause", true, SendMessageOptions.DontRequireReceiver);
                check(!drag.HasActivePointer && !drag.IsDragging && !interrupted.eligibleForClick && cancelled == 1,
                    "Backgrounding an active drag cancels the capture exactly once");
            }
            finally
            {
                existing.pixelDragThreshold = oldThreshold;
                UnityEngine.Object.Destroy(host);
                if (eventObject != null) UnityEngine.Object.Destroy(eventObject);
            }
            yield return null;
        }

        static PointerEventData Pointer(EventSystem system, int id, Vector2 position)
        {
            return new PointerEventData(system) { pointerId = id, button = PointerEventData.InputButton.Left,
                position = position, pressPosition = position, eligibleForClick = true, useDragThreshold = true };
        }
    }
}
