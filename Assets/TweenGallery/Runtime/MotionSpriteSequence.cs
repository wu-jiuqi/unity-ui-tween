using System;
using UnityEngine;
using UnityEngine.UI;
namespace TweenGallery
{
    /// <summary>Discrete art frames on the same owned visual clock; no implicit tween or gameplay clock.</summary>
    public sealed class MotionSpriteSequence : MotionVisualSequence
    {
        public Image image;
        public Sprite[] frames=Array.Empty<Sprite>();
        public float framesPerSecond=12;
        [Tooltip("Optional seconds per frame. Empty uses framesPerSecond. Call CaptureBaseline after changing configuration.")]
        public float[] frameDurations=Array.Empty<float>();
        Image capturedImage;
        Sprite original;
        Sprite[] capturedFrames;
        float[] frameEnds;
        float duration;
        public int CurrentFrame { get; private set; }
        public override float TotalDuration=>captured?duration:ConfiguredDuration();

        float ConfiguredDuration()
        {
            if(frameDurations==null||frameDurations.Length==0)
                return Mathf.Max(1,frames==null?0:frames.Length)/Mathf.Max(.1f,FiniteOr(framesPerSecond,12));
            if(frames==null||frames.Length!=frameDurations.Length)
                throw new InvalidOperationException("Sprite frame durations must match the art frame count.");
            double total=0;
            for(int i=0;i<frameDurations.Length;i++)
            {
                float seconds=frameDurations[i];
                if(float.IsNaN(seconds)||float.IsInfinity(seconds)||seconds<=0)
                    throw new InvalidOperationException("Sprite frame durations must be finite and positive.");
                total+=seconds;
            }
            if(total>float.MaxValue)
                throw new InvalidOperationException("Sprite sequence total duration must be finite.");
            return (float)total;
        }

        // Capture is transactional: invalid edits leave the previous snapshot usable.
        // Play, Seek and Restore reuse that snapshot. Restore before recapturing to keep
        // the authored baseline rather than the currently displayed animation frame.
        protected override void Capture()
        {
            if(image==null||frames==null||frames.Length==0||Array.Exists(frames,s=>s==null))
                throw new InvalidOperationException("Sprite sequence requires an Image and non-null art frames.");
            float nextDuration=ConfiguredDuration();
            var nextFrames=(Sprite[])frames.Clone();
            float[] nextEnds=null;
            if(frameDurations!=null&&frameDurations.Length>0)
            {
                nextEnds=new float[frameDurations.Length];
                double elapsed=0;
                for(int i=0;i<nextEnds.Length;i++)
                {
                    elapsed+=frameDurations[i];
                    nextEnds[i]=(float)(elapsed/nextDuration);
                }
                nextEnds[nextEnds.Length-1]=1;
            }
            capturedImage=image;original=image.sprite;
            capturedFrames=nextFrames;frameEnds=nextEnds;duration=nextDuration;
        }
        protected override void Evaluate(float p)
        {
            if(frameEnds==null)
                CurrentFrame=Mathf.Clamp(Mathf.FloorToInt(p*capturedFrames.Length),0,capturedFrames.Length-1);
            else
            {
                // Half-open dwell intervals: an exact boundary selects the next frame.
                int low=0,high=frameEnds.Length-1;
                while(low<high)
                {
                    int middle=low+(high-low)/2;
                    if(p>=frameEnds[middle])low=middle+1;else high=middle;
                }
                CurrentFrame=low;
            }
            if(capturedImage!=null&&capturedImage.sprite!=capturedFrames[CurrentFrame])
                capturedImage.sprite=capturedFrames[CurrentFrame];
        }
        protected override void RestoreVisual()
        {
            if(capturedImage!=null)capturedImage.sprite=original;
            CurrentFrame=0;
        }
    }
}
