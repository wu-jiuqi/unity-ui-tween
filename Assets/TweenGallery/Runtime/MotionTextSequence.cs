using System;
using UnityEngine;
namespace TweenGallery
{
    public enum TextMotionKind { Bounce, Shiver, Wave, Reveal }
    /// <summary>Animate whole labels or independent glyph visuals, never their layout slots or strings.</summary>
    public sealed class MotionTextSequence : MotionVisualSequence
    {
        public RectTransform[] visuals=Array.Empty<RectTransform>();
        public CanvasGroup[] groups=Array.Empty<CanvasGroup>();
        public TextMotionKind kind;
        public float duration=.55f,stagger=.07f,height=22,strength=1;
        Vector2[] positions;Vector3[] scales;Quaternion[] rotations;float[] alpha;
        public override float TotalDuration=>Mathf.Max(.05f,duration)+(kind==TextMotionKind.Reveal?0:Mathf.Max(0,stagger)*Mathf.Max(0,visuals.Length-1));
        protected override void Capture()
        {
            if(visuals==null||visuals.Length==0)throw new InvalidOperationException("Bind independent text or glyph visuals.");
            if(kind==TextMotionKind.Reveal&&(groups==null||groups.Length!=visuals.Length))throw new InvalidOperationException("Reveal requires one CanvasGroup per glyph.");
            positions=new Vector2[visuals.Length];scales=new Vector3[visuals.Length];rotations=new Quaternion[visuals.Length];alpha=new float[visuals.Length];
            for(int i=0;i<visuals.Length;i++)
            {
                if(visuals[i]==null)throw new InvalidOperationException("Text visual must not be null.");
                if(kind==TextMotionKind.Reveal&&Group(i)==null)throw new InvalidOperationException("Reveal glyph CanvasGroup must not be null.");
                positions[i]=visuals[i].anchoredPosition;scales[i]=visuals[i].localScale;rotations[i]=visuals[i].localRotation;
                if(Group(i)!=null)alpha[i]=Group(i).alpha;
            }
        }
        protected override void Evaluate(float p)
        {
            float seconds=p*TotalDuration,amount=Mathf.Clamp(strength,0,2);
            for(int i=0;i<visuals.Length;i++)
            {
                if(visuals[i]==null)continue;float u=Mathf.Clamp01((seconds-i*Mathf.Max(0,stagger))/Mathf.Max(.05f,duration));
                Vector2 offset=Vector2.zero;Vector3 scale=Vector3.one;float angle=0;
                if(kind==TextMotionKind.Reveal){if(Group(i)!=null)Group(i).alpha=p>=(i+1f)/visuals.Length?alpha[i]:0;}
                else if(u>0&&u<1)
                {
                    if(kind==TextMotionKind.Shiver)
                    {
                        float wave=Mathf.Sin(u*Mathf.PI*8)*Mathf.Sin(u*Mathf.PI)*amount;
                        offset=new Vector2(wave*height*.375f,wave*height);angle=wave*2.2f;scale=new Vector3(1+wave*.018f,1-wave*.025f,1);
                    }
                    else if(kind==TextMotionKind.Wave)
                    {
                        float wave=Mathf.Sin(u*Mathf.PI)*amount;offset=Vector2.up*height*wave;scale=new Vector3(1-.025f*wave,1+.035f*wave,1);angle=(i%2==0?1:-1)*wave*1.43f;
                    }
                    else
                    {
                        float squash=u<.16f?Mathf.Sin(u/.16f*Mathf.PI)*.09f*amount:0;
                        float hop=Mathf.Sin(Mathf.Clamp01((u-.16f)/.62f)*Mathf.PI)*amount;
                        float settle=u>.78f?Mathf.Sin((u-.78f)/.22f*Mathf.PI*2)*(1-u)*.2f*amount:0;
                        offset=Vector2.up*(height*hop);scale=new Vector3(1+squash-hop*.025f+settle,1-squash+hop*.045f-settle,1);
                    }
                }
                visuals[i].anchoredPosition=positions[i]+offset;visuals[i].localScale=Vector3.Scale(scales[i],scale);visuals[i].localRotation=rotations[i]*Quaternion.Euler(0,0,angle);
            }
        }
        CanvasGroup Group(int i)=>groups!=null&&i<groups.Length?groups[i]:null;
        protected override void RestoreVisual(){for(int i=0;i<visuals.Length;i++)if(visuals[i]!=null){visuals[i].anchoredPosition=positions[i];visuals[i].localScale=scales[i];visuals[i].localRotation=rotations[i];if(Group(i)!=null)Group(i).alpha=alpha[i];}}
    }
}
