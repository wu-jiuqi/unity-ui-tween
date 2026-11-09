using System;
using UnityEngine;
namespace TweenGallery
{
    public enum ConfettiMotionKind { Falling, Burst }
    /// <summary>Deterministic decorative UI art; no spawning, pooling or global random state.</summary>
    public sealed class MotionConfettiSequence : MotionVisualSequence
    {
        public RectTransform[] items=Array.Empty<RectTransform>();public CanvasGroup[] groups=Array.Empty<CanvasGroup>();
        public ConfettiMotionKind kind;public float duration=2.4f,travel=360,spread=140,strength=1;public int seed=7;
        Vector2[] positions;Vector3[] scales;Quaternion[] rotations;float[] alpha;
        public override float TotalDuration=>Mathf.Max(.05f,duration);
        protected override void Capture()
        {
            if(items==null||items.Length==0||groups==null||groups.Length!=items.Length)throw new InvalidOperationException("Confetti needs preallocated art visuals and matching groups.");
            positions=new Vector2[items.Length];scales=new Vector3[items.Length];rotations=new Quaternion[items.Length];alpha=new float[items.Length];
            for(int i=0;i<items.Length;i++){if(items[i]==null||groups[i]==null)throw new InvalidOperationException("Confetti binding must not be null.");positions[i]=items[i].anchoredPosition;scales[i]=items[i].localScale;rotations[i]=items[i].localRotation;alpha[i]=groups[i].alpha;}
        }
        float RandomAt(int i){unchecked{uint x=(uint)(seed+i*374761393);x=(x^(x>>13))*1274126177;x^=x>>16;return (x&0xffffff)/16777215f;}}
        protected override void Evaluate(float p)
        {
            for(int i=0;i<items.Length;i++)
            {
                if(items[i]==null)continue;float r=RandomAt(i),phase=r*Mathf.PI*2,local=Mathf.Clamp01((p-r*.12f)/.88f),t=local*TotalDuration,amount=Mathf.Clamp(strength,0,2);Vector2 offset;
                if(kind==ConfettiMotionKind.Falling)offset=new Vector2((Mathf.Sin(t*3+phase)-Mathf.Sin(phase))*spread*.25f,-travel*local*(.75f+r*.5f));
                else{float a=i*2.399963f;offset=new Vector2(Mathf.Cos(a)*spread*local,Mathf.Sin(a)*spread*local+travel*local*(1-local));}
                items[i].anchoredPosition=positions[i]+offset*amount;items[i].localRotation=rotations[i]*Quaternion.Euler(0,0,t*(r<.5f?-170:170));
                items[i].localScale=Vector3.Scale(scales[i],new Vector3(Mathf.Max(.18f,Mathf.Abs(Mathf.Cos(t*5+phase))),1-local*.35f,1));
                groups[i].alpha=alpha[i]*(p>r*.12f&&local<1?Mathf.Clamp01((1-local)/.3f):0);
            }
        }
        protected override void RestoreVisual(){for(int i=0;i<items.Length;i++)if(items[i]!=null){items[i].anchoredPosition=positions[i];items[i].localScale=scales[i];items[i].localRotation=rotations[i];groups[i].alpha=alpha[i];}}
    }
}
