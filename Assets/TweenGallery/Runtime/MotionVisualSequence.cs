using UnityEngine;
using UnityEngine.Events;
namespace TweenGallery
{
    /// <summary>Owned visual clock: silent sampling, scoped restore, visual-only completion.</summary>
    public abstract class MotionVisualSequence : MonoBehaviour
    {
        public bool unscaledTime=true,reducedMotion;
        public float speed=1;
        public UnityEvent onVisualComplete=new UnityEvent();
        public float Progress { get; private set; }
        public bool IsPlaying { get; private set; }
        public uint PlayVersion { get; private set; }
        public abstract float TotalDuration { get; }
        protected bool captured;
        bool completed,focused=true,background;
        public void CaptureBaseline(){IsPlaying=false;Capture();captured=true;}
        protected abstract void Capture();
        protected abstract void Evaluate(float normalized);
        protected abstract void RestoreVisual();
        protected virtual void EvaluateReduced()=>Evaluate(1);
        void Ensure(){if(!captured)CaptureBaseline();}
        public void Play(){Ensure();unchecked{PlayVersion++;}completed=false;Progress=0;IsPlaying=true;if(reducedMotion)Finish();else Evaluate(0);}
        public void Pause(){IsPlaying=false;}
        public void Resume(){Ensure();if(Progress<1){IsPlaying=true;completed=false;}}
        public void Seek(float value){Ensure();if(!Finite(value))return;IsPlaying=false;Progress=Mathf.Clamp01(value);completed=Progress>=1;Evaluate(Progress);}
        public void Sample(float value)=>Seek(value);
        public void Step(float seconds)
        {
            if(!IsPlaying||!focused||background||!Finite(seconds)||seconds<=0)return;
            if(reducedMotion){Finish();return;}
            Progress=Mathf.Clamp01(Progress+seconds*Mathf.Clamp(speed,.05f,4)/Mathf.Max(.05f,TotalDuration));Evaluate(Progress);if(Progress>=1)Finish();
        }
        void Finish(){Progress=1;if(reducedMotion)EvaluateReduced();else Evaluate(1);IsPlaying=false;if(completed)return;completed=true;onVisualComplete.Invoke();}
        public void Restore(){if(!captured)return;IsPlaying=false;Progress=0;completed=false;unchecked{PlayVersion++;}RestoreVisual();}
        void Update()=>Step(unscaledTime?Time.unscaledDeltaTime:Time.deltaTime);
        protected virtual void OnDisable()=>Restore();
        void OnApplicationFocus(bool value){focused=value;}
        void OnApplicationPause(bool value){background=value;}
        static bool Finite(float value)=>!float.IsNaN(value)&&!float.IsInfinity(value);
    }
}
