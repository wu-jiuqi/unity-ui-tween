using System;
using UnityEngine;
namespace TweenGallery
{
    /// <summary>One clipped right-to-left pass. Host owns queueing and repeat intervals.</summary>
    public sealed class MotionMarquee : MotionVisualSequence
    {
        public RectTransform visual,viewport;
        public float duration=6,padding=12;
        Vector2 position;
        public override float TotalDuration=>SafeDuration(duration);
        protected override void Capture(){if(visual==null||viewport==null||visual.parent!=viewport||visual.anchorMin!=Vector2.one*.5f||visual.anchorMax!=Vector2.one*.5f||visual.pivot!=Vector2.one*.5f)throw new InvalidOperationException("Marquee requires a centered visual directly inside its clipped viewport.");position=visual.anchoredPosition;}
        protected override void Evaluate(float p)
        {
            float half=visual.rect.width*.5f,edge=viewport.rect.width*.5f+padding;
            Vector2 start=position;start.x=edge+half;Vector2 end=position;end.x=-edge-half;
            // Require centered anchors/pivot, or use a dedicated centered visual wrapper.
            visual.anchoredPosition=Vector2.Lerp(start,end,p);
        }
        protected override void RestoreVisual(){if(visual!=null)visual.anchoredPosition=position;}
        protected override void EvaluateReduced(){visual.anchoredPosition=position;}
    }
}
