using System;
using UnityEngine;

namespace TweenGallery
{
    /// <summary>
    /// Drive a visual child from MotionDragInput events; keep the hit area outside this
    /// component stationary. Release retargets a physical spring without losing velocity.
    /// Background/focus loss and disable deliberately reset to the stable baseline.
    /// </summary>
    public sealed class MotionJellyDrag : MonoBehaviour
    {
        public RectTransform visual;
        [Min(.01f)] public float dragRange=120f;
        [Tooltip("Optional horizontal and vertical ranges; a zero axis uses dragRange.")]
        public Vector2 axisRange=Vector2.zero;
        [Min(0f)] public float maxDisplacement=55f;
        [Range(0f,.8f)] public float stretch=.16f;
        [Range(0f,30f)] public float maxRotation=8f;
        [Min(.01f)] public float frequency=4f;
        [Range(0f,3f)] public float damping=.72f;
        public bool unscaledTime=true;
        public bool autoUpdate=true;
        private Vector2 positionRest;
        private Vector3 scaleRest;
        private Quaternion rotationRest;
        private MotionSpring2D spring;
        private bool captured, dragging, awakeSpring;
        public bool IsDragging => dragging;
        public Vector2 Offset => Project(spring.Value);
        /// <summary>Velocity of the physical spring before display soft-limiting, in UI units per second.</summary>
        public Vector2 Velocity => spring.Velocity;

        private void Awake() { if (visual == null) visual = transform as RectTransform; CaptureBaseline(); }
        /// <summary>Call after assigning a visual or changing layout, while the component is at rest.</summary>
        public void CaptureBaseline()
        {
            if (visual == null) { captured=false; return; }
            positionRest=visual.anchoredPosition; scaleRest=visual.localScale; rotationRest=visual.localRotation;
            spring=new MotionSpring2D(Vector2.zero); captured=true; dragging=false; awakeSpring=false;
        }
        public void BeginDrag() { if (!isActiveAndEnabled || !captured || visual == null) return; dragging=true; awakeSpring=true; }
        public void Drag(Vector2 delta)
        {
            if (!dragging || visual == null) return;
            // Bound the physical goal before retargeting. Feeding the raw pointer
            // distance to the spring makes very large drags linger at the visual
            // boundary for an arbitrarily long time after release.
            spring.Retarget(SaturateInput(delta));
            awakeSpring=true;
        }
        public void EndDrag() { if (!captured) return; dragging=false; spring.Retarget(Vector2.zero); awakeSpring=true; }
        public void CancelDrag() { ResetVisual(); }
        /// <summary>Advance the exact spring by all elapsed time; useful for deterministic tests.</summary>
        public void Step(float deltaTime)
        {
            if (!captured || !awakeSpring || visual == null) return;
            spring.Step(deltaTime,frequency,damping);
            // Project the physical position itself rather than only its goal: an
            // underdamped overshoot remains bounded and the projection is smooth.
            Vector2 offset=Project(spring.Value);
            float limit=Mathf.Max(.01f,maxDisplacement);
            Vector2 unit=new Vector2(Mathf.Clamp(offset.x/limit,-1f,1f),Mathf.Clamp(offset.y/limit,-1f,1f));
            float sx=1f+stretch*(Mathf.Abs(unit.x)-.5f*Mathf.Abs(unit.y));
            float sy=1f+stretch*(Mathf.Abs(unit.y)-.5f*Mathf.Abs(unit.x));
            visual.anchoredPosition=positionRest+offset;
            visual.localScale=Vector3.Scale(scaleRest,new Vector3(sx,sy,1f));
            visual.localRotation=rotationRest*Quaternion.Euler(0,0,-unit.x*maxRotation);
            if (!dragging && spring.Settle()) { ResetVisual(); }
        }
        private void Update() { if (autoUpdate) Step(unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime); }
        private void OnDisable() { ResetVisual(); }
        private void OnApplicationPause(bool paused) { if (paused) ResetVisual(); }
        private void OnApplicationFocus(bool focus) { if (!focus) ResetVisual(); }
        private Vector2 SaturateInput(Vector2 value)
        {
            float rangeX=Mathf.Max(.01f,axisRange.x>0f?axisRange.x:dragRange);
            float rangeY=Mathf.Max(.01f,axisRange.y>0f?axisRange.y:dragRange);
            float limit=Mathf.Max(0f,maxDisplacement);
            return new Vector2((float)Math.Tanh(value.x/rangeX)*limit,(float)Math.Tanh(value.y/rangeY)*limit);
        }
        private Vector2 Project(Vector2 value)
        {
            // Preserve the physical value through 80% of the range; beyond that,
            // a C1-continuous soft bound approaches the limit without clipping
            // an underdamped spring or discontinuously changing its velocity.
            float limit=Mathf.Max(0f,maxDisplacement);
            if (limit <= .000001f) return Vector2.zero;
            return new Vector2(SoftBound(value.x,limit),SoftBound(value.y,limit));
        }
        private static float SoftBound(float value,float limit)
        {
            float magnitude=Mathf.Abs(value), threshold=limit*.8f;
            if (magnitude <= threshold) return value;
            float remainder=limit-threshold;
            return Mathf.Sign(value)*(threshold+remainder*(float)Math.Tanh((magnitude-threshold)/remainder));
        }
        private void ResetVisual()
        {
            dragging=false; awakeSpring=false;
            if (!captured) return;
            spring.Reset(Vector2.zero);
            if (visual != null) { visual.anchoredPosition=positionRest; visual.localScale=scaleRest; visual.localRotation=rotationRest; }
        }
    }
}
