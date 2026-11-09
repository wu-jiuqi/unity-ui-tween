using System;
using UnityEngine;

namespace TweenGallery
{
    public enum MotionStyleKind { Restrained, Standard, Celebratory }

    /// <summary>Material-aware starting points; Apply returns a new settings object and never mutates user input.</summary>
    [Serializable]
    public sealed class MotionStyleProfile
    {
        public MotionStyleKind kind;
        public float timeScale=1,strengthScale=1,distanceScale=1,staggerScale=1,overshootScale=1;
        public float springFrequency=7,springDamping=.68f;
        public static MotionStyleProfile Get(MotionStyleKind kind)
        {
            switch(kind)
            {
                case MotionStyleKind.Restrained:return new MotionStyleProfile{kind=kind,timeScale=.85f,strengthScale=.6f,distanceScale=.65f,staggerScale=.75f,overshootScale=.45f,springFrequency=9,springDamping=.9f};
                case MotionStyleKind.Celebratory:return new MotionStyleProfile{kind=kind,timeScale=1.18f,strengthScale=1.25f,distanceScale=1.15f,staggerScale=1.15f,overshootScale=1.45f,springFrequency=5.8f,springDamping=.55f};
                default:return new MotionStyleProfile{kind=MotionStyleKind.Standard};
            }
        }
        public MotionSettings Apply(MotionSettings source)
        {
            if(source==null)throw new ArgumentNullException(nameof(source));
            var result=source.Clone();result.duration=Mathf.Max(.05f,result.duration*timeScale);
            result.strength=Mathf.Clamp(result.strength*strengthScale,0,3);result.distance*=distanceScale;
            result.stagger=Mathf.Max(0,result.stagger*staggerScale);result.overshoot=Mathf.Clamp(result.overshoot*overshootScale,0,3);return result;
        }
    }
}
