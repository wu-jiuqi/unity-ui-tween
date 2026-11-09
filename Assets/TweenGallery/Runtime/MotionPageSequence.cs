using System;
using UnityEngine;
using UnityEngine.Events;

namespace TweenGallery
{
    /// <summary>Deterministic staged presentation. Sampling never emits completion or commits business data.</summary>
    public sealed class MotionPageSequence : MonoBehaviour
    {
        public RectTransform panel,emblem,rewardA,rewardB,walletTarget;
        public CanvasGroup backdrop,panelGroup,emblemGroup,rewardsGroup,actionsGroup;
        public RectTransform[] coins=Array.Empty<RectTransform>();
        public float duration=1.1f,overshoot=1.1f,strength=1,travel=24,stagger=.08f,speed=1;
        public bool unscaledTime=true,reducedMotion;
        public UnityEvent onVisualComplete=new UnityEvent();
        public float Progress { get; private set; }
        public float WalletProgress => Smooth(Phase(Progress,.52f,.9f));
        public bool IsPlaying { get; private set; }
        public uint PlayVersion { get; private set; }
        Vector3 panelScale,emblemScale;Vector2 panelPosition,emblemPosition,rewardAPosition,rewardBPosition;
        Vector2[] coinPositions;Vector3[] coinScales;float[] groupAlpha;
        bool captured,completed,focused=true,paused;
        bool originalInteractable,originalRaycasts;
        public void CaptureBaseline()
        {
            if(panel==null||emblem==null)throw new InvalidOperationException("Page sequence requires independent panel and emblem bindings.");
            IsPlaying=false;panelScale=panel.localScale;emblemScale=emblem.localScale;panelPosition=panel.anchoredPosition;emblemPosition=emblem.anchoredPosition;
            if(rewardA!=null)rewardAPosition=rewardA.anchoredPosition;if(rewardB!=null)rewardBPosition=rewardB.anchoredPosition;
            coinPositions=new Vector2[coins.Length];coinScales=new Vector3[coins.Length];
            for(int i=0;i<coins.Length;i++)if(coins[i]!=null){coinPositions[i]=coins[i].anchoredPosition;coinScales[i]=coins[i].localScale;}
            groupAlpha=new[]{Alpha(backdrop),Alpha(panelGroup),Alpha(emblemGroup),Alpha(rewardsGroup),Alpha(actionsGroup)};captured=true;
            if(actionsGroup!=null){originalInteractable=actionsGroup.interactable;originalRaycasts=actionsGroup.blocksRaycasts;}
        }
        public void Play(){Ensure();unchecked{PlayVersion++;}completed=false;Progress=0;IsPlaying=true;if(reducedMotion)Finish();else SampleVisual(0);}
        public void Pause(){IsPlaying=false;}
        public void Resume(){Ensure();if(Progress<1){IsPlaying=true;}}
        public void Seek(float normalized){Ensure();if(!Finite(normalized))return;IsPlaying=false;Progress=Mathf.Clamp01(normalized);SampleVisual(Progress);}
        public void Sample(float normalized){Seek(normalized);}
        public void Step(float seconds)
        {
            if(!IsPlaying||!focused||paused||!Finite(seconds)||seconds<=0)return;
            if(reducedMotion){Finish();return;}
            Progress=Mathf.Clamp01(Progress+seconds*Mathf.Clamp(FiniteOr(speed,1),.05f,4)/SafeDuration);SampleVisual(Progress);if(Progress>=1)Finish();
        }
        void Update(){Step(unscaledTime?Time.unscaledDeltaTime:Time.deltaTime);}
        void Finish(){Progress=1;SampleVisual(1);IsPlaying=false;if(completed)return;completed=true;onVisualComplete.Invoke();}
        void Ensure(){if(!captured)CaptureBaseline();}
        void SampleVisual(float t)
        {
            float enter=Phase(t,0,.28f),hero=Phase(t,.16f,.56f),reward=Phase(t,.45f,.82f),actions=Phase(t,.78f,1);
            Set(backdrop,groupAlpha[0]*Smooth(Phase(t,0,.18f)));Set(panelGroup,groupAlpha[1]*Smooth(enter));Set(emblemGroup,groupAlpha[2]*Smooth(hero));Set(rewardsGroup,groupAlpha[3]*Smooth(reward));Set(actionsGroup,groupAlpha[4]*Smooth(actions));
            panel.localScale=panelScale*Mathf.LerpUnclamped(Mathf.Clamp(1-.06f*strength,.5f,1),1,Back(enter,overshoot));panel.anchoredPosition=panelPosition+Vector2.down*travel*.5f*strength*(1-Smooth(enter));
            emblem.localScale=emblemScale*Mathf.LerpUnclamped(Mathf.Clamp(1-.18f*strength,.3f,1),1,Back(hero,overshoot));emblem.anchoredPosition=emblemPosition+Vector2.left*travel*strength*(1-Smooth(hero));
            float delay=Mathf.Clamp(FiniteOr(stagger,0)/SafeDuration,0,.2f);
            if(rewardA!=null)rewardA.anchoredPosition=rewardAPosition+Vector2.down*travel*.75f*strength*(1-Smooth(Phase(t,.45f,.72f)));
            if(rewardB!=null)rewardB.anchoredPosition=rewardBPosition+Vector2.right*travel*.9f*strength*(1-Smooth(Phase(t,.45f+delay,.74f+delay)));
            if(actionsGroup!=null){actionsGroup.interactable=originalInteractable&&t>=.999f;actionsGroup.blocksRaycasts=originalRaycasts&&t>=.999f;}
            for(int i=0;i<coins.Length;i++)
            {
                var coin=coins[i];if(coin==null)continue;
                // Keep every coin within the owned clock, preserving the original small-array cadence.
                float spacing=coins.Length>1?Mathf.Min(.045f,.22f/(coins.Length-1)):0;
                float local=t>=1?1:Phase(t,.48f+i*spacing,.78f+i*spacing);
                Vector2 end=coinPositions[i];if(walletTarget!=null&&coin.parent is RectTransform parent)
                {
                    Vector2 anchor=coin.anchorMin+Vector2.Scale(coin.anchorMax-coin.anchorMin,coin.pivot);
                    Vector2 reference=parent.rect.min+Vector2.Scale(parent.rect.size,anchor);
                    end=(Vector2)parent.InverseTransformPoint(walletTarget.TransformPoint(walletTarget.rect.center))-reference;
                }
                coin.anchoredPosition=Vector2.Lerp(coinPositions[i],end,Smooth(local))+Vector2.up*Mathf.Sin(local*Mathf.PI)*38;
                coin.localScale=coinScales[i]*(local>0&&local<1?Mathf.Lerp(1,.65f,local):0);
            }
        }
        public void Restore()
        {
            if(!captured)return;IsPlaying=false;Progress=0;completed=false;unchecked{PlayVersion++;}
            if(panel!=null){panel.localScale=panelScale;panel.anchoredPosition=panelPosition;}if(emblem!=null){emblem.localScale=emblemScale;emblem.anchoredPosition=emblemPosition;}
            if(rewardA!=null)rewardA.anchoredPosition=rewardAPosition;if(rewardB!=null)rewardB.anchoredPosition=rewardBPosition;
            var groups=new[]{backdrop,panelGroup,emblemGroup,rewardsGroup,actionsGroup};for(int i=0;i<groups.Length;i++)if(groups[i]!=null)groups[i].alpha=groupAlpha[i];
            for(int i=0;i<coins.Length;i++)if(coins[i]!=null){coins[i].anchoredPosition=coinPositions[i];coins[i].localScale=coinScales[i];}
            if(actionsGroup!=null){actionsGroup.interactable=originalInteractable;actionsGroup.blocksRaycasts=originalRaycasts;}
        }
        void OnDisable(){Restore();}
        void OnApplicationFocus(bool value){focused=value;}
        void OnApplicationPause(bool value){paused=value;}
        static float Alpha(CanvasGroup g)=>g!=null?g.alpha:1;
        static void Set(CanvasGroup g,float a){if(g!=null)g.alpha=Mathf.Clamp01(a);}
        static float Phase(float t,float a,float b)=>Mathf.Clamp01((t-a)/(b-a));
        static float Smooth(float t)=>t*t*(3-2*t);
        static float Back(float t,float amount){float x=t-1;return 1+(Mathf.Clamp(amount,0,3)+1)*x*x*x+Mathf.Clamp(amount,0,3)*x*x;}
        // Only Play starts a new completion round; seeking and resuming retain its latch.
        float SafeDuration=>Mathf.Max(.05f,FiniteOr(duration,.05f));
        static float FiniteOr(float value,float fallback)=>Finite(value)?value:fallback;
        static bool Finite(float n)=>!float.IsNaN(n)&&!float.IsInfinity(n);
    }
}
