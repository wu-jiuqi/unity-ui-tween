using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core.Easing;
using UnityEngine;
using UnityEngine.UI;

namespace TweenGallery
{
    /// <summary>A per-instance, reversible UGUI motion controller. Animate a child wrapper when a layout owns its parent.</summary>
    public sealed class MotionPlayer : MonoBehaviour
    {
        public string effectId = "A04";
        public MotionSettings settings = new MotionSettings();
        public MotionBindings bindings = new MotionBindings();
        private Tween ownedTween;
        private Snapshot baseline;
        private string ownedEffect;
        private float progress;
        private Material blurMaterial;
        private Image blurOwner;
        private static readonly Dictionary<RectTransform, MotionPlayer> Owners = new Dictionary<RectTransform, MotionPlayer>();
        private RectTransform claimedTarget;
        public bool IsPlaying { get { return ownedTween != null && ownedTween.IsActive() && ownedTween.IsPlaying(); } }
        public float Progress { get { return progress; } }

        public void CaptureBaseline()
        {
            KillOwnedTween();
            baseline = new Snapshot(bindings);
            ownedEffect = null;
            progress = 0f;
        }

        public void Play(string id, MotionSettings options = null)
        {
            MotionCatalog.Find(id); // Fail explicitly on a catalog mismatch.
            if (bindings == null || bindings.target == null)
                throw new InvalidOperationException("MotionPlayer requires bindings.target (a RectTransform).");
            if (!isActiveAndEnabled) return;
            // Newest request owns this target. Other targets and unrelated tweens remain untouched.
            if (Owners.TryGetValue(bindings.target, out var previous) && previous != null && previous != this) previous.StopAndRestore();
            ReleaseTarget();
            claimedTarget = bindings.target;
            Owners[claimedTarget] = this;
            if (options != null) settings = options.Clone();
            if (settings == null) settings = MotionSettings.FromDefinition(MotionCatalog.Find(id));
            KillOwnedTween();
            if (baseline == null) baseline = new Snapshot(bindings);
            baseline.Restore(ownedEffect, true);
            effectId = id;
            ownedEffect = id;
            ValidateBindings();
            progress = 0f;
            Evaluate(0f);
            ownedTween = DOTween.To(() => progress, value => { progress = value; Evaluate(value); }, 1f, Mathf.Max(.05f, settings.duration))
                .SetEase(Ease.Linear).SetUpdate(settings.unscaledTime).SetAutoKill(false)
                .SetLoops(settings.loop ? -1 : 1, LoopType.Restart);
            ownedTween.timeScale = Mathf.Clamp(settings.speed, .05f, 4f);
        }

        public void Pause() { if (ownedTween != null && ownedTween.IsActive()) ownedTween.Pause(); }
        public void Resume() { if (ownedTween != null && ownedTween.IsActive()) ownedTween.Play(); }
        /// <summary>Real pointer feedback: hold the compressed state until release, then return smoothly.</summary>
        public void SetPressed(bool pressed, bool rebound = true)
        {
            if (!isActiveAndEnabled || bindings == null || bindings.target == null) return;
            if (Owners.TryGetValue(bindings.target, out var previous) && previous != null && previous != this) previous.StopAndRestore();
            ReleaseTarget();
            claimedTarget = bindings.target;
            Owners[claimedTarget] = this;
            if (settings == null) settings = new MotionSettings();
            if (baseline == null) baseline = new Snapshot(bindings);
            float baseX = S(bindings.target).x;
            float factor = Mathf.Abs(baseX) > .0001f ? bindings.target.localScale.x / baseX : 1f;
            KillOwnedTween();
            baseline.Restore(ownedEffect, true);
            ownedEffect = rebound ? "B02" : "B01";
            Scale(bindings.target, factor);
            float end = pressed ? 1f - .14f * Mathf.Clamp(settings.strength, 0f, 3f) : 1f;
            ownedTween = DOTween.To(() => factor, value => { factor = value; Scale(bindings.target, value); }, end,
                    pressed ? Mathf.Min(.15f, Mathf.Max(.05f, settings.duration * .25f)) : Mathf.Max(.1f, settings.duration * .6f))
                .SetEase(pressed ? Ease.OutQuad : rebound ? Ease.OutElastic : Ease.OutQuad)
                .SetUpdate(settings.unscaledTime).SetAutoKill(false);
            ownedTween.timeScale = Mathf.Clamp(settings.speed, .05f, 4f);
        }
        public void StopAndRestore()
        {
            KillOwnedTween();
            ReleaseTarget();
            if (baseline != null) baseline.Restore(ownedEffect, true);
            ownedEffect = null;
            progress = 0f;
        }

        /// <summary>Samples the production evaluator without creating a second animation implementation.</summary>
        public void Sample(float normalizedTime)
        {
            Pause();
            if (bindings == null || bindings.target == null) return;
            if (baseline == null) baseline = new Snapshot(bindings);
            if (ownedEffect != effectId) baseline.Restore(ownedEffect, true);
            ownedEffect = effectId;
            progress = Mathf.Clamp01(normalizedTime);
            Evaluate(progress);
        }

        private void Update()
        {
            if (ownedTween != null && ownedTween.IsActive()) ownedTween.timeScale = Mathf.Clamp(settings.speed, .05f, 4f);
        }
        private void OnDisable() { StopAndRestore(); }
        private void OnDestroy()
        {
            KillOwnedTween();
            ReleaseTarget();
            if (baseline != null) baseline.Restore(ownedEffect, true);
            if (blurMaterial != null) Destroy(blurMaterial);
        }
        private void KillOwnedTween()
        {
            if (ownedTween != null && ownedTween.IsActive()) ownedTween.Kill(false);
            ownedTween = null;
        }
        private void ReleaseTarget()
        {
            if (!ReferenceEquals(claimedTarget, null) && Owners.TryGetValue(claimedTarget, out var owner) && owner == this) Owners.Remove(claimedTarget);
            claimedTarget = null;
        }

        private void ValidateBindings()
        {
            string missing = null;
            switch (effectId)
            {
                case "A01": if (bindings.group == null) missing = "group"; break;
                case "A07": case "C03": if (bindings.mask == null) missing = "mask"; break;
                case "A08": if (bindings.image == null) missing = "image"; break;
                case "C05": case "C06": case "C07": if (bindings.secondary == null || bindings.secondaryGroup == null || bindings.group == null) missing = "secondary + both CanvasGroups"; break;
                case "C04": case "D03": if (bindings.destination == null) missing = "destination"; break;
                case "D01": if (bindings.label == null) missing = "label"; break;
                case "D02": if (bindings.fill == null) missing = "fill"; break;
                case "D04": if (bindings.items == null || bindings.items.Length == 0 || bindings.destination == null) missing = "items + destination"; break;
                case "D05": if (bindings.labels == null || bindings.labels.Length == 0) missing = "labels"; break;
                case "D06": case "E05": case "E06": if (bindings.items == null || bindings.items.Length == 0) missing = "items"; break;
                case "D07": if (bindings.image == null || bindings.secondaryImage == null) missing = "image (back) + secondaryImage (front)"; break;
                case "D08": if (bindings.lid == null) missing = "lid"; break;
                case "E03": if (bindings.shine == null || bindings.mask == null) missing = "shine + mask"; break;
                case "F01": if (bindings.group == null || bindings.shine == null || bindings.items == null || bindings.items.Length == 0) missing = "group + shine + items"; break;
                case "F02": case "F06": if (bindings.group == null || bindings.secondary == null || bindings.items == null || bindings.items.Length == 0) missing = "group + secondary + items"; break;
                case "F03": case "F04": if (bindings.secondary == null || bindings.items == null || bindings.items.Length == 0) missing = "secondary + items"; break;
                case "F05": if (bindings.secondary == null || bindings.items == null || bindings.items.Length == 0) missing = "secondary + items"; break;
            }
            if (missing != null) throw new InvalidOperationException(effectId + " requires MotionBindings." + missing + ". See the template manual.");
        }

        private float E(float t, Ease preset = Ease.OutCubic)
        {
            Ease easing;
            if (string.IsNullOrEmpty(settings.ease) || settings.ease == "Preset" || !Enum.TryParse(settings.ease, out easing)) easing = preset;
            return EaseManager.Evaluate(easing, null, Mathf.Clamp01(t), 1f, 1.70158f, 0f);
        }
        private float Phase(float t, float begin, float end) { return Mathf.Clamp01((t - begin) / Mathf.Max(.001f, end - begin)); }
        private Vector2 P(RectTransform target) { return baseline.Position(target); }
        private Vector3 S(RectTransform target) { return baseline.Scale(target); }
        private void Move(RectTransform target, Vector2 offset) { if (target != null) target.anchoredPosition = P(target) + offset; }
        private void Scale(RectTransform target, float scale) { if (target != null) target.localScale = S(target) * scale; }
        private void ScaleXY(RectTransform target, float x, float y) { if (target != null) target.localScale = Vector3.Scale(S(target), new Vector3(x, y, 1f)); }
        private void Rotate(RectTransform target, Vector3 degrees) { if (target != null) target.localRotation = baseline.Rotation(target) * Quaternion.Euler(degrees); }
        private void Alpha(CanvasGroup group, float alpha)
        {
            if (group == null) return;
            group.alpha = baseline.Alpha(group) * Mathf.Clamp01(alpha);
            baseline.ManageRaycasts(group, group.alpha <= .001f);
        }
        private void GraphicAlpha(Graphic graphic, float alpha)
        {
            if (graphic == null) return;
            Color c = graphic.color; c.a = baseline.Color(graphic).a * Mathf.Clamp01(alpha); graphic.color = c;
        }
        private void ItemAlpha(int index, float alpha)
        {
            if (bindings.itemGroups != null && index < bindings.itemGroups.Length && bindings.itemGroups[index] != null)
                Alpha(bindings.itemGroups[index], alpha);
            else if (bindings.items != null && index < bindings.items.Length && bindings.items[index] != null)
                foreach (Graphic graphic in bindings.items[index].GetComponentsInChildren<Graphic>(true)) GraphicAlpha(graphic, alpha);
        }
        private float StaggerPhase(float t, int index, int count, float available = 1f)
        {
            float spacing = Mathf.Min(Mathf.Max(0f, settings.stagger) / Mathf.Max(.05f, settings.duration), .7f * available / Mathf.Max(1, count - 1));
            return Mathf.Clamp01((t - spacing * index) / Mathf.Max(.05f, available - spacing * (count - 1)));
        }
        private Vector2 ToDestination(RectTransform from)
        {
            if (from == null || bindings.destination == null || from.parent == null) return Vector2.right * settings.distance;
            Vector3 world = bindings.destination.TransformPoint(bindings.destination.rect.center);
            Vector3 local = from.parent.InverseTransformPoint(world);
            Vector3 currentCenter = from.parent.InverseTransformPoint(from.TransformPoint(from.rect.center));
            return (Vector2)(local - currentCenter);
        }

        private void Evaluate(float rawTime)
        {
            if (baseline == null || bindings.target == null) return;
            baseline.Restore(ownedEffect, false);
            float t = settings.reverse ? 1f - rawTime : rawTime;
            float a = Mathf.Clamp(settings.strength,0f,3f), d = settings.distance;
            RectTransform target = bindings.target;
            float q;
            switch (effectId)
            {
                // Representative templates: Pop, elastic button, arcing collection, clipped sweep.
                case "A04":
                    q = E(t, Ease.OutBack); Scale(target, Mathf.LerpUnclamped(Mathf.Clamp(1f - .8f * a, .05f, 1f), 1f, q)); Alpha(bindings.group, Phase(t, 0f, .45f)); break;
                case "B02":
                    q = t < .3f ? Mathf.Lerp(1f, 1f - .16f * a, E(t / .3f, Ease.OutQuad)) : Mathf.LerpUnclamped(1f - .16f * a, 1f, E(Phase(t, .3f, 1f), Ease.OutElastic));
                    Scale(target, q); break;
                case "D03":
                    q = E(t, Ease.InOutSine); Vector2 flight = ToDestination(target);
                    Move(target, flight * q + Vector2.up * (Mathf.Sin(Mathf.PI * q) * Mathf.Abs(d) * a));
                    Scale(target, Mathf.Lerp(1f, .55f, q)); Alpha(bindings.group, 1f - Phase(t, .92f, 1f));
                    Scale(bindings.destination, 1f + Mathf.Sin(Phase(t, .77f, 1f) * Mathf.PI) * .18f * a); break;
                case "E03":
                    float travel = Mathf.Abs(d) > 1f ? Mathf.Abs(d) : bindings.mask.rect.width;
                    Move(bindings.shine, Vector2.right * Mathf.Lerp(-travel, travel, E(t, Ease.InOutSine)));
                    GraphicAlpha(bindings.shine.GetComponent<Graphic>(), Mathf.Sin(Mathf.PI * t) * Mathf.Min(1f, a)); break;

                case "A01": Alpha(bindings.group, E(t, Ease.InOutSine)); break;
                case "A02": Move(target, Vector2.left * d * (1f - E(t))); Alpha(bindings.group, Phase(t, 0f, .35f)); break;
                case "A03": Scale(target, Mathf.LerpUnclamped(Mathf.Clamp(1f - .85f * a, .02f, 1f), 1f, E(t))); Alpha(bindings.group, Phase(t, 0f, .35f)); break;
                case "A05": Move(target, Vector2.up * d * (1f - E(t, Ease.OutBounce))); Alpha(bindings.group, Phase(t, 0f, .2f)); break;
                case "A06": Rotate(target, new Vector3(0f, 90f * Mathf.Min(1f, a) * (1f - E(t)), 0f)); Alpha(bindings.group, Phase(t, 0f, .5f)); break;
                case "A07": if (bindings.mask != null) bindings.mask.sizeDelta = new Vector2(baseline.Size(bindings.mask).x * Mathf.Clamp01(E(t)), baseline.Size(bindings.mask).y); break;
                case "A08": ApplyBlur(12f * a * (1f - Mathf.Clamp01(E(t, Ease.InOutSine)))); Alpha(bindings.group, Mathf.Lerp(.25f, 1f, t)); break;
                case "B01": q = t < .45f ? E(t / .45f, Ease.OutQuad) : 1f - E(Phase(t, .45f, 1f), Ease.OutQuad); Scale(target, 1f - .14f * a * q); break;
                case "B03": q = Mathf.Sin(Mathf.PI * E(t, Ease.InOutSine)); Scale(target, 1f + .12f * a * q); break;
                case "B04": float shakeTime=Mathf.Clamp01(E(t,Ease.Linear)); q = (1f - shakeTime) * Mathf.Sin(shakeTime * Mathf.PI * 12f); Move(target, new Vector2(q * Mathf.Abs(d) * .22f * a, Mathf.Sin(shakeTime * 37f) * (1f - shakeTime) * Mathf.Abs(d) * .035f * a)); break;
                case "B05": float wiggleTime=Mathf.Clamp01(E(t,Ease.Linear)); Rotate(target, new Vector3(0f, 0f, Mathf.Sin(wiggleTime * Mathf.PI * 6f) * (1f - wiggleTime) * 20f * a)); break;
                case "B06": float punchTime=Mathf.Clamp01(E(t,Ease.Linear)); q = Mathf.Sin(punchTime * Mathf.PI * 5f) * Mathf.Pow(1f - punchTime, 2f) * .32f * a; ScaleXY(target, 1f + q, 1f - q * .45f); break;
                case "B07":
                    q = E(t); Move(target, Vector2.up * d * .25f * q); Scale(target, 1f + .12f * a * q);
                    Scale(bindings.secondary, 1.12f - .12f * q); Move(bindings.secondary, Vector2.up * d * .25f * (1f - q));
                    if (bindings.image != null) bindings.image.color = Color.Lerp(baseline.Color(bindings.image) * new Color(.5f,.5f,.5f,1f), baseline.Color(bindings.image), q);
                    if (bindings.secondaryImage != null) bindings.secondaryImage.color = Color.Lerp(baseline.Color(bindings.secondaryImage), baseline.Color(bindings.secondaryImage) * new Color(.5f,.5f,.5f,1f), q); break;
                case "B08":
                    q = E(t, Ease.InOutCubic); Move(target, (bindings.destination != null ? ToDestination(target) : Vector2.right * d) * q);
                    if (bindings.image != null) bindings.image.color = Color.Lerp(new Color(.25f,.3f,.48f,1f), new Color(.25f,.95f,.49f,1f), q); break;
                case "C01": Move(target, Vector2.up * d * (1f - E(t, Ease.OutBack))); Alpha(bindings.group, Phase(t, 0f, .3f)); break;
                case "C02": Move(target, Vector2.left * d * (1f - E(t))); Alpha(bindings.secondaryGroup, .7f * E(t, Ease.OutQuad)); break;
                case "C03": if (bindings.mask != null) bindings.mask.sizeDelta = new Vector2(baseline.Size(bindings.mask).x, baseline.Size(bindings.mask).y * Mathf.Clamp01(E(t))); break;
                case "C04":
                    q = E(t, Ease.InOutCubic); Vector2 destinationDelta = ToDestination(target);
                    if (bindings.destination != null)
                    {
                        Vector3[] corners = new Vector3[4]; bindings.destination.GetWorldCorners(corners);
                        Vector3 lo = target.InverseTransformPoint(corners[0]); Vector3 hi = target.InverseTransformPoint(corners[2]);
                        Vector2 targetSize = new Vector2(Mathf.Abs(hi.x-lo.x), Mathf.Abs(hi.y-lo.y));
                        Vector2 baseRect = baseline.RectSize(target);
                        Vector2 sizeChange = (targetSize - baseRect) * q;
                        target.sizeDelta = baseline.Size(target) + sizeChange;
                        Vector3 centerShift = target.localRotation * Vector3.Scale(new Vector3((.5f-target.pivot.x)*sizeChange.x,(.5f-target.pivot.y)*sizeChange.y,0f),target.localScale);
                        Move(target,destinationDelta*q-(Vector2)centerShift);
                    } break;
                case "C05":
                    q = E(t, Ease.InOutCubic); Move(target, Vector2.left * d * q); Move(bindings.secondary, Vector2.right * d * (1f-q)); Alpha(bindings.group, 1f-q); Alpha(bindings.secondaryGroup,q); break;
                case "C06": Alpha(bindings.group, 1f-E(Phase(t,0f,.4f), Ease.OutQuad)); Alpha(bindings.secondaryGroup,E(Phase(t,.6f,1f),Ease.OutQuad)); break;
                case "C07": q=E(t,Ease.InOutSine); Alpha(bindings.group,1f-q); Alpha(bindings.secondaryGroup,q); break;
                case "D01": if(bindings.label!=null) bindings.label.text=Mathf.RoundToInt(Mathf.LerpUnclamped(settings.startValue,settings.endValue,E(t,Ease.OutQuad))).ToString("N0"); break;
                case "D02": if(bindings.fill!=null) bindings.fill.fillAmount=Mathf.Lerp(settings.fillFrom,settings.fillTo,E(t,Ease.InOutSine)); if(bindings.label!=null) bindings.label.text=Mathf.RoundToInt(Mathf.Lerp(settings.fillFrom,settings.fillTo,E(t,Ease.InOutSine))*100f)+"%"; break;
                case "D04": EvaluateBurst(t,a,d); break;
                case "D05": EvaluateSettlement(t,a); break;
                case "D06":
                    for(int i=0;i<bindings.items.Length;i++) { q=StaggerPhase(t,i,bindings.items.Length); Scale(bindings.items[i],Mathf.LerpUnclamped(Mathf.Clamp(1f-.65f*a,.05f,1f),1f,E(q,Ease.OutBack))); ItemAlpha(i,Mathf.Lerp(.15f,1f,Phase(q,0f,.3f))); Image star=bindings.items[i]==null?null:bindings.items[i].GetComponent<Image>(); if(star!=null) star.color=Color.Lerp(new Color(.24f,.27f,.39f,1f),baseline.Color(star),Phase(q,0f,.25f)); } break;
                case "D07":
                    q=E(t,Ease.InOutSine); Rotate(target,new Vector3(0f,q<.5f?q*180f:(q-1f)*180f,0f));
                    GraphicAlpha(bindings.image,q<.5f?1f:0f); GraphicAlpha(bindings.secondaryImage,q>=.5f?1f:0f); Alpha(bindings.group,q<.5f?1f:0f); Alpha(bindings.secondaryGroup,q>=.5f?1f:0f); break;
                case "D08": EvaluateChest(t,a,d); break;
                case "D09": q=E(t,Ease.OutQuad); Move(target,new Vector2(Mathf.Sin(t*Mathf.PI)*d*.2f*a,d*q)); Alpha(bindings.group,1f-Phase(t,.25f,1f)); Scale(target,1f+.15f*a*Mathf.Sin(t*Mathf.PI)); break;
                case "D10":
                    float impactTime=Mathf.Clamp01(E(t,Ease.Linear)); q=Mathf.Sin(impactTime*Mathf.PI)*Mathf.Pow(1f-impactTime,1.5f); Scale(target,1f+.6f*a*q);
                    if(bindings.image!=null) bindings.image.color=Color.Lerp(baseline.Color(bindings.image),Color.white,Mathf.Clamp01(q*2f*a)); break;
                case "E01": Move(target,Vector2.up*Mathf.Sin(Mathf.Clamp01(E(t,Ease.Linear))*Mathf.PI*2f)*d*a); break;
                case "E02": Rotate(target,new Vector3(0f,0f,-360f*E(t,Ease.Linear))); break;
                case "E04": Alpha(bindings.group,1f-.7f*Mathf.Min(1f,a)*Mathf.Pow(Mathf.Sin(Mathf.Clamp01(E(t,Ease.Linear))*Mathf.PI),2f)); break;
                case "E05":
                    for(int i=0;i<bindings.items.Length;i++) { q=StaggerPhase(t,i,bindings.items.Length); Move(bindings.items[i],Vector2.down*d*(1f-E(q))); Scale(bindings.items[i],Mathf.LerpUnclamped(Mathf.Clamp(1f-.2f*a,.05f,1f),1f,E(q,Ease.OutBack))); ItemAlpha(i,Phase(q,0f,.5f)); } break;
                case "E06":
                    for(int i=0;i<bindings.items.Length;i++) { q=StaggerPhase(t,i,bindings.items.Length); float p=E(q,Ease.InOutSine); Move(bindings.items[i],new Vector2(Mathf.Sin(p*Mathf.PI*2f)*d,Mathf.Sin(p*Mathf.PI*4f)*d*.35f*a)); ItemAlpha(i,1f-i*.1f); } break;
                case "E07":
                    q=E(Phase(t,0f,.45f),Ease.InOutSine); Move(target,new Vector2(-d*.5f*q,d*.35f*q));
                    float press=Mathf.Sin(Phase(t,.45f,.85f)*Mathf.PI); Scale(target,1f-.22f*a*press); Alpha(bindings.secondaryGroup,.25f+.75f*press);
                    if(t>.85f) { float leave=E(Phase(t,.85f,1f),Ease.InOutSine); Move(target,new Vector2(-d*.5f,d*.35f)*(1f-leave)); } break;
                case "F01": EvaluateHologram(t,a,d); break;
                case "F02": EvaluateConstellation(t,a,d); break;
                case "F03": EvaluateEditorial(t,a,d); break;
                case "F04": EvaluateDashboard(t,a,d); break;
                case "F05": EvaluateCarousel(t,a,d); break;
                case "F06": EvaluatePortal(t,a,d); break;
            }
        }

        // All compositions use deterministic normalized time so the gallery, prefab,
        // project and snapshot tests render exactly the same sequences.
        private void EvaluateHologram(float t,float strength,float distance)
        {
            float sx=E(Phase(t,0f,.30f),Ease.OutCubic);
            float sy=E(Phase(t,.12f,.48f),Ease.OutBack);
            ScaleXY(bindings.target,Mathf.LerpUnclamped(.08f,1f,sx),Mathf.LerpUnclamped(.12f,1f,sy));
            Alpha(bindings.group,Phase(t,0f,.16f));
            float scan=E(Phase(t,.20f,.78f),Ease.InOutSine);
            Move(bindings.shine,Vector2.right*Mathf.Lerp(-Mathf.Abs(distance),Mathf.Abs(distance),scan));
            GraphicAlpha(bindings.shine.GetComponent<Graphic>(),Mathf.Sin(Mathf.PI*Phase(t,.2f,.8f))*Mathf.Clamp01(strength));
            // Optional independent colored wipe. The parent uses RectMask2D so
            // the moving panel covers the card, then exits to reveal the artwork.
            if(bindings.mask!=null)
            {
                float width=Mathf.Max(Mathf.Abs(distance)*2f,baseline.RectSize(bindings.mask).x);
                float inBlock=E(Phase(t,0f,.35f),Ease.InOutSine);
                float outBlock=E(Phase(t,.35f,.8f),Ease.InOutSine);
                Move(bindings.mask,Vector2.right*(width*inBlock+width*outBlock));
            }
            for(int i=0;i<bindings.items.Length;i++)
            {
                float p=StaggerPhase(Phase(t,.15f,.92f),i,bindings.items.Length);
                ScaleXY(bindings.items[i],Mathf.Clamp01(E(p,Ease.OutCubic)),1f);
                ItemAlpha(i,Phase(p,0f,.25f));
            }
        }
        private void EvaluateConstellation(float t,float strength,float distance)
        {
            float ring=E(Phase(t,.05f,.8f),Ease.OutCubic);
            Rotate(bindings.secondary,new Vector3(0f,0f,-100f*(1f-ring)*strength));
            Scale(bindings.secondary,Mathf.Lerp(.48f,1f,ring));
            for(int i=0;i<bindings.items.Length;i++)
            {
                float p=StaggerPhase(Phase(t,.08f,.82f),i,bindings.items.Length);
                float angle=(i*137.508f+25f)*Mathf.Deg2Rad;
                var direction=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));
                Move(bindings.items[i],direction*Mathf.Abs(distance)*strength*(1f-E(p,Ease.OutCubic)));
                Scale(bindings.items[i],Mathf.LerpUnclamped(.2f,1f,E(p,Ease.OutBack)));
                ItemAlpha(i,Phase(p,0f,.3f));
            }
            float badge=E(Phase(t,.48f,.9f),Ease.OutBack);
            Scale(bindings.target,Mathf.LerpUnclamped(.25f,1f,badge));
            Alpha(bindings.group,Phase(t,.48f,.65f));
            if(bindings.labels!=null) foreach(var label in bindings.labels) GraphicAlpha(label,Phase(t,.55f,.78f));
        }
        private void EvaluateEditorial(float t,float strength,float distance)
        {
            for(int i=0;i<bindings.items.Length;i++)
            {
                float p=StaggerPhase(Phase(t,0f,.9f),i,bindings.items.Length);
                float remaining=1f-E(p,Ease.OutCubic);
                var offset=i%2==0 ? new Vector2(-distance*.4f,-distance) : new Vector2(distance*.4f,distance);
                Move(bindings.items[i],offset*remaining*strength);
                ItemAlpha(i,Phase(p,0f,.3f));
            }
            ScaleXY(bindings.secondary,Mathf.Clamp01(E(Phase(t,.3f,.9f),Ease.OutCubic)),1f);
        }
        private void EvaluateDashboard(float t,float strength,float distance)
        {
            for(int i=0;i<bindings.items.Length;i++)
            {
                float p=StaggerPhase(Phase(t,0f,.9f),i,bindings.items.Length);
                float remaining=1f-E(p,Ease.OutCubic);
                var offset=new Vector2((i%2==0?-1:1)*distance*.28f,-distance)*remaining*strength;
                Move(bindings.items[i],offset);
                Scale(bindings.items[i],Mathf.LerpUnclamped(.9f,1f,E(p,Ease.OutBack)));
                ItemAlpha(i,Phase(p,0f,.3f));
            }
            ScaleXY(bindings.secondary,Mathf.Lerp(.2f,1f,E(Phase(t,.35f,.75f))),1f);
            if(bindings.fill!=null) bindings.fill.fillAmount=Mathf.Lerp(settings.fillFrom,settings.fillTo,E(Phase(t,.55f,1f),Ease.InOutSine));
        }
        private void EvaluateCarousel(float t,float strength,float distance)
        {
            float inPhase=E(Phase(t,.08f,.88f),Ease.OutCubic);
            Move(bindings.target,new Vector2(distance*.8f,-distance*.22f)*(1f-inPhase));
            Scale(bindings.target,Mathf.LerpUnclamped(.68f,1f,E(Phase(t,.08f,.88f),Ease.OutBack)));
            Rotate(bindings.target,new Vector3(0f,0f,-9f*strength*(1f-inPhase)));
            Alpha(bindings.group,Phase(t,.08f,.36f));
            float outPhase=E(Phase(t,0f,.72f),Ease.InOutSine);
            Move(bindings.secondary,new Vector2(-distance*.8f,-distance*.22f)*outPhase);
            Scale(bindings.secondary,Mathf.Lerp(1f,.68f,outPhase));
            Rotate(bindings.secondary,new Vector3(0f,0f,9f*strength*outPhase));
            Alpha(bindings.secondaryGroup,1f-Phase(t,.38f,.75f));
            for(int i=0;i<bindings.items.Length;i++)
            {
                float p=StaggerPhase(Phase(t,.12f,.9f),i,bindings.items.Length);
                Move(bindings.items[i],new Vector2((i%2==0?-1:1)*distance*.18f, -distance*.12f)*(1f-E(p)));
                ItemAlpha(i,Mathf.Lerp(.4f,1f,Phase(p,0f,.5f)));
            }
        }
        private void EvaluatePortal(float t,float strength,float distance)
        {
            float open=E(Phase(t,.03f,.48f),Ease.OutBack);
            ScaleXY(bindings.secondary,Mathf.LerpUnclamped(.04f,1f,open),Mathf.LerpUnclamped(.3f,1f,open));
            Rotate(bindings.secondary,new Vector3(0f,0f,110f*(1f-open)*strength));
            Alpha(bindings.secondaryGroup,Phase(t,.03f,.2f));
            for(int i=0;i<bindings.items.Length;i++)
            {
                float p=StaggerPhase(Phase(t,.18f,.87f),i,bindings.items.Length);
                float angle=i*Mathf.PI*2f/Mathf.Max(1,bindings.items.Length);
                Vector2 radial=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));
                Move(bindings.items[i],-radial*Mathf.Abs(distance)*.55f*(1f-E(p,Ease.OutCubic)));
                Scale(bindings.items[i],Mathf.LerpUnclamped(.05f,1f,E(p,Ease.OutBack)));
                ItemAlpha(i,Phase(p,0f,.3f));
            }
            float core=E(Phase(t,.36f,.8f),Ease.OutBack);
            Scale(bindings.target,Mathf.LerpUnclamped(.05f,1f,core));
            Alpha(bindings.group,Phase(t,.36f,.52f));
            if(bindings.labels!=null) foreach(var label in bindings.labels) GraphicAlpha(label,Phase(t,.47f,.7f));
        }

        private void EvaluateBurst(float t,float strength,float distance)
        {
            for(int i=0;i<bindings.items.Length;i++)
            {
                RectTransform item=bindings.items[i]; if(item==null) continue;
                float angle=(i*137.508f+25f)*Mathf.Deg2Rad;
                Vector2 spread=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*Mathf.Abs(distance)*strength;
                float local=StaggerPhase(t,i,bindings.items.Length);
                float burst=E(Phase(local,0f,.28f),Ease.OutCubic), collect=E(Phase(local,.38f,1f),Ease.InOutCubic);
                Move(item,Vector2.LerpUnclamped(spread*burst,ToDestination(item),collect));
                Scale(item,Mathf.Lerp(1f,.45f,collect)); ItemAlpha(i,1f-Phase(local,.94f,1f));
            }
            Scale(bindings.destination,1f+.15f*strength*Mathf.Sin(Phase(t,.7f,1f)*Mathf.PI));
        }
        private void EvaluateSettlement(float t,float strength)
        {
            int count=bindings.labels.Length; float total=0f;
            for(int i=0;i<count;i++)
            {
                float local=StaggerPhase(t,i,count); float value=Mathf.Lerp(0f,settings.endValue*(i+1)/(count*(count+1)*.5f),E(local,Ease.OutQuad)); total+=value;
                if(bindings.labels[i]!=null) bindings.labels[i].text=Mathf.RoundToInt(value).ToString("N0");
                if(bindings.items!=null&&i<bindings.items.Length) { ItemAlpha(i,Phase(local,0f,.15f)); Scale(bindings.items[i],1f+.08f*strength*Mathf.Sin(local*Mathf.PI)); }
            }
            if(bindings.label!=null) bindings.label.text=Mathf.RoundToInt(total).ToString("N0");
        }
        private void EvaluateChest(float t,float strength,float distance)
        {
            float charge=Mathf.Sin(Phase(t,0f,.4f)*Mathf.PI);
            ScaleXY(bindings.target,1f+.12f*strength*charge,1f-.14f*strength*charge);
            Rotate(bindings.target,new Vector3(0f,0f,Mathf.Sin(t*65f)*charge*4f*strength));
            Rotate(bindings.lid,new Vector3(0f,0f,70f*E(Phase(t,.4f,.72f),Ease.OutBack)));
            for(int i=0;i<bindings.items.Length;i++)
            {
                float local=StaggerPhase(Phase(t,.48f,1f),i,bindings.items.Length);
                Move(bindings.items[i],Vector2.up*Mathf.Abs(distance)*(1f-E(local,Ease.OutBack))*-1f);
                Scale(bindings.items[i],Mathf.LerpUnclamped(.1f,1f,E(local,Ease.OutBack)));
                ItemAlpha(i,Phase(local,0f,.2f));
            }
        }
        private void ApplyBlur(float radius)
        {
            if(bindings.image==null) return;
            if(blurMaterial==null || blurOwner!=bindings.image)
            {
                if(blurMaterial!=null) Destroy(blurMaterial);
                Shader shader=Shader.Find("TweenGallery/UITextureBlur");
                if(shader==null) throw new InvalidOperationException("A08 requires included shader TweenGallery/UITextureBlur.");
                blurMaterial=new Material(shader) { name="MotionPlayer Blur (instance)", hideFlags=HideFlags.DontSave };
                blurOwner=bindings.image;
            }
            bindings.image.material=blurMaterial;
            blurMaterial.SetFloat("_Blur",Mathf.Clamp(radius,0f,12f));
        }

        private sealed class Snapshot
        {
            private sealed class RectState { public RectTransform target; public Vector3 position,scale; public Quaternion rotation; public Vector2 size,rectSize; }
            private sealed class GraphicState { public Graphic target; public Color color; public Material material; public float fill; public string text; }
            private sealed class GroupState { public CanvasGroup target; public float alpha; public bool raycasts,suppressed; }
            private readonly MotionBindings bindings;
            private readonly Dictionary<RectTransform,RectState> rects=new Dictionary<RectTransform,RectState>();
            private readonly Dictionary<Graphic,GraphicState> graphics=new Dictionary<Graphic,GraphicState>();
            private readonly Dictionary<CanvasGroup,GroupState> groups=new Dictionary<CanvasGroup,GroupState>();
            public Snapshot(MotionBindings b)
            {
                bindings=b;
                if(b==null) return;
                Add(b.target); Add(b.secondary); Add(b.destination); Add(b.mask); Add(b.shine); Add(b.lid);
                if(b.items!=null) foreach(var item in b.items) Add(item);
                Add(b.group); Add(b.secondaryGroup); if(b.itemGroups!=null) foreach(var group in b.itemGroups) Add(group);
                Add(b.image); Add(b.secondaryImage); Add(b.fill); Add(b.label); if(b.labels!=null) foreach(var label in b.labels) Add(label);
            }
            private void Add(RectTransform rect)
            {
                if(rect==null||rects.ContainsKey(rect)) return;
                rects.Add(rect,new RectState {target=rect,position=rect.anchoredPosition3D,scale=rect.localScale,rotation=rect.localRotation,size=rect.sizeDelta,rectSize=rect.rect.size});
                foreach(var graphic in rect.GetComponentsInChildren<Graphic>(true)) Add(graphic);
                foreach(var group in rect.GetComponentsInChildren<CanvasGroup>(true)) Add(group);
            }
            private void Add(Graphic graphic)
            {
                if(graphic==null||graphics.ContainsKey(graphic)) return;
                graphics.Add(graphic,new GraphicState {target=graphic,color=graphic.color,material=graphic.material,fill=graphic is Image image?image.fillAmount:0f,text=graphic is UnityEngine.UI.Text text?text.text:null});
            }
            private void Add(CanvasGroup group)
            {
                if(group==null||groups.ContainsKey(group)) return;
                groups.Add(group,new GroupState {target=group,alpha=group.alpha,raycasts=group.blocksRaycasts});
            }
            public Vector2 Position(RectTransform rect) { return rect!=null&&rects.TryGetValue(rect,out var state)?(Vector2)state.position:Vector2.zero; }
            public Vector3 Scale(RectTransform rect) { return rect!=null&&rects.TryGetValue(rect,out var state)?state.scale:Vector3.one; }
            public Quaternion Rotation(RectTransform rect) { return rect!=null&&rects.TryGetValue(rect,out var state)?state.rotation:Quaternion.identity; }
            public Vector2 Size(RectTransform rect) { return rect!=null&&rects.TryGetValue(rect,out var state)?state.size:Vector2.zero; }
            public Vector2 RectSize(RectTransform rect) { return rect!=null&&rects.TryGetValue(rect,out var state)?state.rectSize:Vector2.zero; }
            public Color Color(Graphic graphic) { return graphic!=null&&graphics.TryGetValue(graphic,out var state)?state.color:UnityEngine.Color.white; }
            public float Alpha(CanvasGroup group) { return group!=null&&groups.TryGetValue(group,out var state)?state.alpha:1f; }
            // Restore only properties owned by the selected effect. Business text, disabled
            // controls and unrelated artwork may change while an animation is running.
            public void Restore(string effect, bool releaseInput)
            {
                if(bindings==null || string.IsNullOrEmpty(effect)) return;
                var b=bindings;
                switch(effect)
                {
                    case "A01": Group(b.group,releaseInput); break;
                    case "A02": case "A05": case "C01": Rect(b.target,1); Group(b.group,releaseInput); break;
                    case "A03": case "A04": Rect(b.target,2); Group(b.group,releaseInput); break;
                    case "A06": Rect(b.target,4); Group(b.group,releaseInput); break;
                    case "A07": case "C03": Rect(b.mask,8); break;
                    case "A08": if(releaseInput) Material(b.image); Group(b.group,releaseInput); break;
                    case "B01": case "B02": case "B03": case "B06": Rect(b.target,2); break;
                    case "B04": case "E01": Rect(b.target,1); break;
                    case "B05": case "E02": Rect(b.target,4); break;
                    case "B07": Rect(b.target,3); Rect(b.secondary,3); FullColor(b.image); FullColor(b.secondaryImage); break;
                    case "B08": Rect(b.target,1); FullColor(b.image); break;
                    case "C02": Rect(b.target,1); Group(b.secondaryGroup,releaseInput); break;
                    case "C04": Rect(b.target,9); break;
                    case "C05": Rect(b.target,1); Rect(b.secondary,1); Group(b.group,releaseInput); Group(b.secondaryGroup,releaseInput); break;
                    case "C06": case "C07": Group(b.group,releaseInput); Group(b.secondaryGroup,releaseInput); break;
                    case "D01": Text(b.label); break;
                    case "D02": Fill(b.fill); Text(b.label); break;
                    case "D03": Rect(b.target,3); Rect(b.destination,2); Group(b.group,releaseInput); break;
                    case "D04": Items(3,releaseInput); Rect(b.destination,2); break;
                    case "D05": Items(2,releaseInput); Text(b.label); if(b.labels!=null) foreach(var text in b.labels) Text(text); break;
                    case "D06": Items(2,releaseInput); if(b.items!=null) foreach(var item in b.items) if(item!=null) FullColor(item.GetComponent<Image>()); break;
                    case "D07": Rect(b.target,4); GraphicAlpha(b.image); GraphicAlpha(b.secondaryImage); Group(b.group,releaseInput); Group(b.secondaryGroup,releaseInput); break;
                    case "D08": Rect(b.target,6); Rect(b.lid,4); Items(3,releaseInput); break;
                    case "D09": Rect(b.target,3); Group(b.group,releaseInput); break;
                    case "D10": Rect(b.target,2); FullColor(b.image); break;
                    case "E03": Rect(b.shine,1); if(b.shine!=null) GraphicAlpha(b.shine.GetComponent<Graphic>()); break;
                    case "E04": Group(b.group,releaseInput); break;
                    case "E05": Items(3,releaseInput); break;
                    case "E06": Items(1,releaseInput); break;
                    case "E07": Rect(b.target,3); Group(b.secondaryGroup,releaseInput); break;
                    case "F01": Rect(b.target,2); Group(b.group,releaseInput); Rect(b.shine,1); Rect(b.mask,1); if(b.shine!=null) GraphicAlpha(b.shine.GetComponent<Graphic>()); Items(2,releaseInput); break;
                    case "F02": Rect(b.target,2); Group(b.group,releaseInput); Rect(b.secondary,6); Items(3,releaseInput); if(b.labels!=null) foreach(var label in b.labels) GraphicAlpha(label); break;
                    case "F03": Rect(b.secondary,2); Items(1,releaseInput); break;
                    case "F04": Rect(b.secondary,2); Fill(b.fill); Items(3,releaseInput); break;
                    case "F05": Rect(b.target,7); Rect(b.secondary,7); Group(b.group,releaseInput); Group(b.secondaryGroup,releaseInput); Items(1,releaseInput); break;
                    case "F06": Rect(b.target,2); Group(b.group,releaseInput); Rect(b.secondary,6); Group(b.secondaryGroup,releaseInput); Items(3,releaseInput); if(b.labels!=null) foreach(var label in b.labels) GraphicAlpha(label); break;
                }
            }
            private void Rect(RectTransform rect,int flags)
            {
                if(rect==null||!rects.TryGetValue(rect,out var state)) return;
                if((flags&1)!=0) rect.anchoredPosition3D=state.position;
                if((flags&2)!=0) rect.localScale=state.scale;
                if((flags&4)!=0) rect.localRotation=state.rotation;
                if((flags&8)!=0) rect.sizeDelta=state.size;
            }
            private void Group(CanvasGroup group,bool releaseInput)
            {
                if(group==null||!groups.TryGetValue(group,out var state)) return;
                group.alpha=state.alpha;
                if(releaseInput) ManageRaycasts(group,false);
            }
            public void ManageRaycasts(CanvasGroup group,bool hidden)
            {
                if(group==null||!groups.TryGetValue(group,out var state)) return;
                if(hidden && !state.suppressed)
                {
                    state.raycasts=group.blocksRaycasts;
                    state.suppressed=true;
                    group.blocksRaycasts=false;
                }
                else if(!hidden && state.suppressed)
                {
                    group.blocksRaycasts=state.raycasts;
                    state.suppressed=false;
                }
            }
            private void FullColor(Graphic graphic) { if(graphic!=null&&graphics.TryGetValue(graphic,out var state)) graphic.color=state.color; }
            private void GraphicAlpha(Graphic graphic)
            {
                if(graphic==null||!graphics.TryGetValue(graphic,out var state)) return;
                var color=graphic.color; color.a=state.color.a; graphic.color=color;
            }
            private void Material(Graphic graphic) { if(graphic!=null&&graphics.TryGetValue(graphic,out var state)) graphic.material=state.material; }
            private void Text(UnityEngine.UI.Text text) { if(text!=null&&graphics.TryGetValue(text,out var state)) text.text=state.text; }
            private void Fill(Image image) { if(image!=null&&graphics.TryGetValue(image,out var state)) image.fillAmount=state.fill; }
            private void Items(int flags,bool releaseInput)
            {
                if(bindings.items==null) return;
                for(int i=0;i<bindings.items.Length;i++)
                {
                    RectTransform item=bindings.items[i]; Rect(item,flags);
                    if(bindings.itemGroups!=null&&i<bindings.itemGroups.Length&&bindings.itemGroups[i]!=null) Group(bindings.itemGroups[i],releaseInput);
                    else if(item!=null) foreach(var graphic in item.GetComponentsInChildren<Graphic>(true)) GraphicAlpha(graphic);
                }
            }
        }
    }
}
