using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TweenGallery
{
    public enum ButtonMotionStyle { SoftPress, Jelly, Lift, IconPop, Ripple, Sheen, Selection, Loading }

    /// <summary>BX01–BX08. Attach to the fixed Button hit area; all animated bindings are dedicated descendants.
    /// Never invokes onClick or changes interactable. Selection/busy are supplied by the host.</summary>
    public sealed class MotionButtonFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler,
        IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler, ISubmitHandler, ICancelHandler
    {
        public ButtonMotionStyle style;
        public RectTransform surface, icon, ripple, sheen, selection;
        public CanvasGroup rippleGroup, sheenGroup, selectionGroup;
        public bool unscaledTime = true, reducedMotion;
        [Range(0, 2)] public float intensity = 1;
        public float frequency = 7, damping = .64f;
        public bool IsBusy { get; private set; }
        public bool IsSelected { get; private set; }
        public bool IsPressed => pointer != int.MinValue && inside && !dragged;
        public bool HasActivePointer => pointer != int.MinValue;
        public float PressAmount => press.value;
        Selectable selectable;
        MotionSpring1D press, hover, chosen;
        int pointer = int.MinValue;
        bool inside, dragged, hovering, captured, focusLost, paused;
        PointerEventData activeEvent;
        Vector2 down;
        Vector3 baseScale, iconScale, rippleScale, sheenScale, selectionScale;
        Vector2 basePosition, iconPosition, sheenPosition;
        Quaternion baseRotation, iconRotation;
        float rippleAlpha, sheenAlpha, selectionAlpha;
        float pulse = -1, clock;

        void Awake() { selectable = GetComponent<Selectable>(); }
        bool Available => isActiveAndEnabled && !IsBusy && !focusLost && !paused && (selectable == null || (selectable.isActiveAndEnabled && selectable.IsInteractable()));
        bool Valid(RectTransform r) => r != null && r != transform && r.IsChildOf(transform);
        public void CaptureBaseline()
        {
            if (!Valid(surface)) throw new InvalidOperationException("Button feedback needs a dedicated descendant surface, outside the layout/hit root.");
            var layers = new HashSet<RectTransform> { surface };
            foreach (var r in new[] { icon, ripple, sheen, selection })
                if (r != null && (!Valid(r) || !layers.Add(r))) throw new InvalidOperationException("Optional layers must be distinct dedicated descendant transforms.");
            if ((style == ButtonMotionStyle.IconPop || style == ButtonMotionStyle.Loading) && icon == null)
                throw new InvalidOperationException("IconPop / Loading needs an independent icon binding.");
            if (style == ButtonMotionStyle.Ripple && (ripple == null || rippleGroup == null))
                throw new InvalidOperationException("Ripple needs a ripple transform and its CanvasGroup.");
            if (style == ButtonMotionStyle.Sheen && (sheen == null || sheenGroup == null))
                throw new InvalidOperationException("Sheen needs a sheen transform and its CanvasGroup.");
            if (style == ButtonMotionStyle.Selection && (selection == null || selectionGroup == null))
                throw new InvalidOperationException("Selection needs a selection transform and its CanvasGroup.");
            if ((rippleGroup != null && rippleGroup.transform != ripple) || (sheenGroup != null && sheenGroup.transform != sheen) || (selectionGroup != null && selectionGroup.transform != selection))
                throw new InvalidOperationException("Each optional CanvasGroup belongs on its corresponding dedicated layer.");
            selectable = GetComponent<Selectable>();
            baseScale = surface.localScale; basePosition = surface.anchoredPosition; baseRotation = surface.localRotation;
            if (icon != null) { iconScale = icon.localScale; iconPosition = icon.anchoredPosition; iconRotation = icon.localRotation; }
            if (ripple != null) rippleScale = ripple.localScale;
            if (sheen != null) { sheenPosition = sheen.anchoredPosition; sheenScale = sheen.localScale; }
            if (selection != null) selectionScale = selection.localScale;
            rippleAlpha = rippleGroup != null ? rippleGroup.alpha : 0;
            sheenAlpha = sheenGroup != null ? sheenGroup.alpha : 0;
            selectionAlpha = selectionGroup != null ? selectionGroup.alpha : 0;
            captured = true; press.Reset(0); hover.Reset(0); chosen.Reset(IsSelected ? 1 : 0); pulse = -1;
            Apply();
        }
        void Ensure() { if (!captured) CaptureBaseline(); }
        void Update()
        {
            if (!captured) { if (!Valid(surface)) return; CaptureBaseline(); }
            if (HasActivePointer && !Available) CancelInteraction();
            if (!Available) hovering = false;
            Step(unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
        }
        /// <summary>Deterministic stepping for previews/tests. Normal playback calls this from Update.</summary>
        public void Step(float dt)
        {
            Ensure();
            if (focusLost || paused) return;
            if (float.IsNaN(dt) || float.IsInfinity(dt) || dt < 0) return;
            press.Retarget(IsPressed ? 1 : 0); hover.Retarget(hovering && Available ? 1 : 0); chosen.Retarget(IsSelected ? 1 : 0);
            if (reducedMotion) { press.Reset(press.target); hover.Reset(hover.target); chosen.Reset(chosen.target); pulse = -1; }
            else { press.Step(dt, frequency, damping); hover.Step(dt, frequency, .85f); chosen.Step(dt, frequency, .85f); }
            press.Settle(.0001f,.001f); hover.Settle(.0001f,.001f); chosen.Settle(.0001f,.001f);
            clock = Mathf.Repeat(clock + dt, 3600);
            if (pulse >= 0) { pulse += dt / .55f; if (pulse >= 1) pulse = -1; }
            Apply();
        }
        void Apply()
        {
            if (!captured || surface == null) return;
            float p = press.value * intensity, h = hover.value * intensity, s = chosen.value;
            float pop = pulse < 0 ? 0 : Mathf.Sin(pulse * Mathf.PI) * (1 - pulse);
            float small = reducedMotion ? .25f : 1;
            Vector2 scale = new Vector2(1 - p * .055f, 1 - p * .055f);
            Vector2 offset = Vector2.zero;
            if (style == ButtonMotionStyle.Jelly) scale = new Vector2(1 + p * .075f, 1 - p * .14f);
            if (style == ButtonMotionStyle.Lift) { offset.y = 8 * h - 6 * p; scale += Vector2.one * h * .025f; }
            if (style == ButtonMotionStyle.Selection) { scale += Vector2.one * s * .035f; offset.y = s * 3; }
            scale = Vector2.LerpUnclamped(Vector2.one, scale, small);
            surface.localScale = Vector3.Scale(baseScale, new Vector3(scale.x, scale.y, 1));
            surface.anchoredPosition = basePosition + offset * small;
            surface.localRotation = baseRotation;
            if (icon != null)
            {
                float amount = style == ButtonMotionStyle.IconPop ? pop * intensity : 0;
                icon.localScale = iconScale * (1 + amount * .22f);
                icon.anchoredPosition = iconPosition + Vector2.up * amount * 16;
                icon.localRotation = iconRotation * Quaternion.Euler(0,0, IsBusy && !reducedMotion ? -clock * 220 : amount * -12);
            }
            if (ripple != null)
            {
                float t = pulse < 0 ? 0 : pulse;
                ripple.localScale = rippleScale * Mathf.Lerp(.15f,1.3f,t);
                if (rippleGroup != null) rippleGroup.alpha = style == ButtonMotionStyle.Ripple && pulse >= 0 ? .45f * (1-t) : 0;
            }
            if (sheen != null)
            {
                float t = pulse < 0 ? 0 : pulse;
                sheen.anchoredPosition = sheenPosition + Vector2.right * Mathf.Lerp(-surface.rect.width,surface.rect.width,t);
                if (sheenGroup != null) sheenGroup.alpha = style == ButtonMotionStyle.Sheen && pulse >= 0 ? Mathf.Sin(t*Mathf.PI)*.6f : 0;
            }
            if (selection != null) selection.localScale = selectionScale * (1 + .035f*s*small);
            if (selectionGroup != null) selectionGroup.alpha = s;
        }
        public void SetSelected(bool value) { Ensure(); IsSelected = value; if (reducedMotion) { chosen.Reset(value ? 1 : 0); Apply(); } }
        public void SetBusy(bool value) { Ensure(); if (value) CancelInteraction(); IsBusy = value; clock = 0; Apply(); }
        public void PlayAccent() { Ensure(); if (Available && !reducedMotion) { pulse = 0; Apply(); } }
        public void OnPointerDown(PointerEventData e)
        {
            if (e == null || e.button != PointerEventData.InputButton.Left) return;
            if (HasActivePointer || !Available) { e.eligibleForClick = false; return; }
            Ensure(); pointer = e.pointerId; activeEvent = e; down = e.position; inside = true; dragged = false;
        }
        bool Matches(PointerEventData e) => e != null && HasActivePointer && e.pointerId == pointer;
        public void OnPointerMove(PointerEventData e)
        {
            if (!Matches(e)) return;
            int threshold = EventSystem.current != null ? EventSystem.current.pixelDragThreshold : 5;
            if (e.dragging || (e.position-down).sqrMagnitude >= threshold*threshold)
            { dragged = true; e.eligibleForClick = false; }
        }
        public void OnPointerEnter(PointerEventData e) { if (e == null) return; if (e.pointerId < 0) hovering = true; if (Matches(e)) inside = true; }
        public void OnPointerExit(PointerEventData e) { if (e == null) return; if (e.pointerId < 0) hovering = false; if (Matches(e)) inside = false; }
        public void OnPointerUp(PointerEventData e)
        {
            if (!Matches(e)) return;
            OnPointerMove(e);
            bool valid = Available && inside && !dragged && e.eligibleForClick;
            if (!valid) e.eligibleForClick = false;
            Clear(); if (valid) PlayAccent();
        }
        public void OnSubmit(BaseEventData e) { if (Available) { Ensure(); if (!reducedMotion) press.velocity += 8; PlayAccent(); } }
        public void OnCancel(BaseEventData e) { CancelInteraction(); }
        void Clear() { pointer = int.MinValue; activeEvent = null; inside = dragged = false; }
        public void CancelInteraction()
        {
            if (activeEvent != null) activeEvent.eligibleForClick = false;
            Clear(); hovering = false; press.Reset(0); hover.Reset(0); pulse = -1; if (captured) Apply();
        }
        public void Restore()
        {
            if (!captured) return;
            if (surface != null) { surface.localScale = baseScale; surface.anchoredPosition = basePosition; surface.localRotation = baseRotation; }
            if (icon != null) { icon.localScale = iconScale; icon.anchoredPosition = iconPosition; icon.localRotation = iconRotation; }
            if (ripple != null) ripple.localScale = rippleScale;
            if (sheen != null) { sheen.anchoredPosition = sheenPosition; sheen.localScale = sheenScale; }
            if (selection != null) selection.localScale = selectionScale;
            if (rippleGroup != null) rippleGroup.alpha = rippleAlpha;
            if (sheenGroup != null) sheenGroup.alpha = sheenAlpha;
            if (selectionGroup != null) selectionGroup.alpha = selectionAlpha;
        }
        void OnDisable() { CancelInteraction(); IsBusy = false; Restore(); }
        void OnApplicationFocus(bool focus) { focusLost = !focus; if (!focus) { clock = 0; CancelInteraction(); } }
        void OnApplicationPause(bool pause) { paused = pause; if (pause) { clock = 0; CancelInteraction(); } }
    }
}
