using System;
using UnityEngine;

namespace TweenGallery
{
    /// <summary>Exact damped harmonic oscillator for a constant target. Frequency is Hz; damping ratio 1 is critical.
    /// Retargeting changes only Target; current Value and Velocity remain continuous. No time is discarded on long frames.</summary>
    [Serializable]
    public struct MotionSpring1D
    {
        public float value, velocity, target;
        public MotionSpring1D(float value) { this.value=value; velocity=0f; target=value; }
        public void Retarget(float next) { target=next; }
        public void Reset(float position) { value=target=position; velocity=0f; }
        public void Step(float deltaTime, float frequency, float damping)
        {
            if (float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime <= 0f) return;
            double w=2d*Math.PI*Math.Max(.001, frequency), z=Math.Max(0d,damping), dt=deltaTime;
            double x=value-target, v=velocity, nextX, nextV;
            if (z < .9999d)
            {
                double d=z*w, wd=w*Math.Sqrt(1d-z*z), exp=Math.Exp(-d*dt), c=Math.Cos(wd*dt), s=Math.Sin(wd*dt);
                double b=(v+d*x)/wd;
                nextX=exp*(x*c+b*s);
                nextV=exp*((-d*x+b*wd)*c+(-d*b-x*wd)*s);
            }
            else if (z <= 1.0001d)
            {
                double b=v+w*x, exp=Math.Exp(-w*dt);
                nextX=(x+b*dt)*exp;
                nextV=(v-w*b*dt)*exp;
            }
            else
            {
                double root=Math.Sqrt(z*z-1d), r1=-w*(z-root), r2=-w*(z+root);
                double c1=(v-r2*x)/(r1-r2), c2=x-c1, e1=Math.Exp(r1*dt), e2=Math.Exp(r2*dt);
                nextX=c1*e1+c2*e2; nextV=c1*r1*e1+c2*r2*e2;
            }
            value=(float)(target+nextX); velocity=(float)nextV;
        }
        public bool Settle(float positionTolerance=.01f,float velocityTolerance=.01f)
        {
            if (Mathf.Abs(value-target)>positionTolerance || Mathf.Abs(velocity)>velocityTolerance) return false;
            value=target; velocity=0f; return true;
        }
    }

    [Serializable]
    public struct MotionSpring2D
    {
        public MotionSpring1D x,y;
        public MotionSpring2D(Vector2 value) { x=new MotionSpring1D(value.x); y=new MotionSpring1D(value.y); }
        public Vector2 Value => new Vector2(x.value,y.value);
        public Vector2 Velocity => new Vector2(x.velocity,y.velocity);
        public Vector2 Target => new Vector2(x.target,y.target);
        public void Retarget(Vector2 next) { x.Retarget(next.x); y.Retarget(next.y); }
        public void Reset(Vector2 p) { x.Reset(p.x); y.Reset(p.y); }
        public void Step(float dt,float frequency,float damping) { x.Step(dt,frequency,damping); y.Step(dt,frequency,damping); }
        public bool Settle(float positionTolerance=.01f,float velocityTolerance=.01f)
        { bool sx=x.Settle(positionTolerance,velocityTolerance); bool sy=y.Settle(positionTolerance,velocityTolerance); return sx&&sy; }
    }
}
