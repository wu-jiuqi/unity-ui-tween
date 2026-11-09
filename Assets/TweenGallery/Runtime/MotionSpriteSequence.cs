using System;
using UnityEngine;
using UnityEngine.UI;
namespace TweenGallery
{
    /// <summary>Discrete art frames on the same owned visual clock; no implicit tween or gameplay clock.</summary>
    public sealed class MotionSpriteSequence : MotionVisualSequence
    {
        public Image image;public Sprite[] frames=Array.Empty<Sprite>();public float framesPerSecond=12;
        Sprite original;
        public int CurrentFrame { get; private set; }
        public override float TotalDuration=>Mathf.Max(1,frames.Length)/Mathf.Max(.1f,framesPerSecond);
        protected override void Capture(){if(image==null||frames==null||frames.Length==0||Array.Exists(frames,s=>s==null))throw new InvalidOperationException("Sprite sequence requires an Image and non-null art frames.");original=image.sprite;}
        protected override void Evaluate(float p){CurrentFrame=Mathf.Clamp(Mathf.FloorToInt(p*frames.Length),0,frames.Length-1);if(image.sprite!=frames[CurrentFrame])image.sprite=frames[CurrentFrame];}
        protected override void RestoreVisual(){if(image!=null)image.sprite=original;CurrentFrame=0;}
    }
}
