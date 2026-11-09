using System;
using UnityEngine;
namespace TweenGallery
{
    /// <summary>Staggered drop, 30fps impact keys, then one shared damped jelly phase.</summary>
    public sealed class MotionLogoDrop : MotionVisualSequence
    {
        public RectTransform[] glyphs=Array.Empty<RectTransform>();
        public CanvasGroup[] groups=Array.Empty<CanvasGroup>();
        public MotionJellyGraphic[] meshes=Array.Empty<MotionJellyGraphic>();
        public float firstDelay=.12f,stagger=.105f,dropDuration=.5f,dropHeight=440,holdDuration=6.8f,timingScale=1,strength=1;
        static readonly Vector3[] poses={new Vector3(1.32f,.65f,-7),new Vector3(1.39f,.61f,-4),new Vector3(1.26f,.73f,4),new Vector3(.87f,1.22f,6),new Vector3(.82f,1.27f,3),new Vector3(.94f,1.12f,-3),new Vector3(1.12f,.87f,-4),new Vector3(1.15f,.85f,-1),new Vector3(.95f,1.07f,2),new Vector3(.97f,1.04f,1),new Vector3(1.04f,.96f,-1),new Vector3(1.02f,.98f,0),new Vector3(1,1,0)};
        Vector2[] positions;Vector3[] scales;Quaternion[] rotations;float[] alpha;
        public float SharedJellyStart=>Mathf.Max(0,firstDelay)+Mathf.Max(0,stagger)*Mathf.Max(0,glyphs.Length-1)+Mathf.Max(.05f,dropDuration)+poses.Length/30f;
        float BaseDuration=>Mathf.Max(holdDuration,SharedJellyStart+.65f);
        public override float TotalDuration=>BaseDuration*Mathf.Max(.1f,timingScale);
        public bool InSharedJellyPhase=>Progress*BaseDuration>=SharedJellyStart&&Progress<1;
        protected override void Capture()
        {
            if(glyphs==null||glyphs.Length==0||groups==null||groups.Length!=glyphs.Length)throw new InvalidOperationException("Logo requires independent glyph visuals and matching CanvasGroups.");
            positions=new Vector2[glyphs.Length];scales=new Vector3[glyphs.Length];rotations=new Quaternion[glyphs.Length];alpha=new float[glyphs.Length];
            for(int i=0;i<glyphs.Length;i++){if(glyphs[i]==null||groups[i]==null)throw new InvalidOperationException("Logo glyph bindings must not be null.");positions[i]=glyphs[i].anchoredPosition;scales[i]=glyphs[i].localScale;rotations[i]=glyphs[i].localRotation;alpha[i]=groups[i].alpha;}
        }
        protected override void Evaluate(float p)
        {
            float time=p*BaseDuration,amount=Mathf.Clamp(strength,0,1.5f);
            for(int i=0;i<glyphs.Length;i++)
            {
                if(glyphs[i]==null)continue;float start=Mathf.Max(0,firstDelay)+i*Mathf.Max(0,stagger),age=time-start;
                Vector2 offset=Vector2.zero;Vector3 pose=new Vector3(1,1,0);float deform=0;
                groups[i].alpha=age>=0?alpha[i]:0;
                if(p<1&&age<Mathf.Max(.05f,dropDuration))
                {
                    float t=Mathf.Clamp01(age/Mathf.Max(.05f,dropDuration));offset=new Vector2((i%2==0?-22:22)*(1-t*t*t),dropHeight*(1-t*t*t));
                    pose=new Vector3(.87f,1.14f,(i%2==0?-13:12)*(1-t)*(1-t));
                }
                else if(p<1&&age<Mathf.Max(.05f,dropDuration)+poses.Length/30f)
                {
                    int frame=Mathf.Clamp(Mathf.FloorToInt((age-Mathf.Max(.05f,dropDuration))*30),0,poses.Length-1);pose=poses[frame];
                    deform=frame==poses.Length-1?0:Mathf.Sin(frame*1.2f)*(1-frame/(float)poses.Length);
                }
                else if(p<1&&time>=SharedJellyStart)
                {
                    float t=time-SharedJellyStart,envelope=Mathf.Exp(-.42f*t)*Mathf.Clamp01((BaseDuration-time)/.65f),wave=Mathf.Sin(t*11.5f);
                    pose=new Vector3(1+wave*.17f*envelope,1-wave*.14f*envelope,Mathf.Sin(t*8)*3.8f*envelope);
                    offset=Vector2.up*Mathf.Abs(Mathf.Sin(t*5.75f))*21*envelope;deform=Mathf.Sin(Mathf.Floor(t*30)/30*11.5f)*.42f*envelope;
                }
                glyphs[i].anchoredPosition=positions[i]+offset;
                glyphs[i].localScale=Vector3.Scale(scales[i],new Vector3(1+(pose.x-1)*amount,1+(pose.y-1)*amount,1));glyphs[i].localRotation=rotations[i]*Quaternion.Euler(0,0,pose.z*amount);
                if(meshes!=null&&i<meshes.Length&&meshes[i]!=null)meshes[i].SetDeformation(deform*amount);
            }
        }
        protected override void RestoreVisual(){for(int i=0;i<glyphs.Length;i++){if(glyphs[i]==null)continue;glyphs[i].anchoredPosition=positions[i];glyphs[i].localScale=scales[i];glyphs[i].localRotation=rotations[i];groups[i].alpha=alpha[i];if(meshes!=null&&i<meshes.Length&&meshes[i]!=null)meshes[i].SetDeformation(0);}}
    }
}
