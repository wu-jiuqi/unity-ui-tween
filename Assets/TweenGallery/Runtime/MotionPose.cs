using System;
using UnityEngine;

namespace TweenGallery
{
    [Serializable]
    public struct MotionPose
    {
        public Vector2 position;
        public Vector3 scale;
        public Quaternion rotation;
        [Range(0f,1f)] public float alpha;
        public static MotionPose Read(RectTransform target, CanvasGroup group = null)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            return new MotionPose { position = target.anchoredPosition, scale = target.localScale,
                rotation = target.localRotation, alpha = group != null ? group.alpha : 1f };
        }
        public void Apply(RectTransform target, CanvasGroup group = null)
        {
            if (target != null) { target.anchoredPosition = position; target.localScale = scale; target.localRotation = rotation; }
            if (group != null) group.alpha = Mathf.Clamp01(alpha);
        }
        public static MotionPose Interpolate(MotionPose a, MotionPose b, float t)
        {
            return new MotionPose { position = Vector2.LerpUnclamped(a.position,b.position,t),
                scale = Vector3.LerpUnclamped(a.scale,b.scale,t), rotation = Quaternion.SlerpUnclamped(a.rotation,b.rotation,t),
                alpha = Mathf.LerpUnclamped(a.alpha,b.alpha,t) };
        }
    }
}
