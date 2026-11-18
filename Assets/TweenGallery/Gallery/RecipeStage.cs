using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace TweenGallery.Recipes
{
    // Business mutations happen in operations, never in Render, Seek, Replay or completion callbacks.
    public sealed partial class RecipeStage : MonoBehaviour
    {
        public RecipeDefinition Definition {get;private set;}
        public readonly Dictionary<string,Button> Actions=new Dictionary<string,Button>();
        public readonly Dictionary<string,int> Data=new Dictionary<string,int>();
        public readonly List<string> Trace=new List<string>();
        public string State {get;private set;}
        public bool Reduced {get;private set;}
        public float Duration=.3f, Strength=1, Speed=1;
        public float SpringFrequency=4.5f,SpringDamping=.6f;
        public DG.Tweening.Ease Curve=DG.Tweening.Ease.OutCubic;
        public float Progress {get;private set;}
        public bool IsPlaying=>motion!=null&&motion.IsActive()&&motion.IsPlaying();
        public bool CanSeek=>render!=null;
        public bool Paused {get;private set;}
        public int Generation {get;private set;}
        Tween motion;Action<float> render;Text stateText;RectTransform world,actionsRoot;
        readonly List<Action> ticking=new List<Action>();
        readonly List<Action> cancelling=new List<Action>();
        readonly MotionChannelArbiter channelArbiter=new MotionChannelArbiter();
        bool alive,playbackLoop;float playbackDuration,durationScale=1;int actionCount;
        public void Build(RecipeDefinition definition,bool reduced) {
            Definition=definition;Duration=definition.duration;Reduced=reduced;alive=true;
            State=definition.initialState;
            stateText=RecipeUI.Label(transform,State,0,218,830,35,17,RecipeUI.Muted);
            world=RecipeUI.Rect(transform,"Scenario "+definition.id,0,15,850,360);
            actionsRoot=RecipeUI.Rect(transform,"Operations",0,-210,850,44);
            if(ReferenceComposition)BuildReferenceCards();
            else{stateText.rectTransform.anchoredPosition=new Vector2(0,270);actionsRoot.anchoredPosition=new Vector2(0,-355);world.localScale=Vector3.one*1.2f;
                switch(definition.Group){case 1:BuildPage();break;case 2:BuildModal();break;case 3:BuildContainer();break;case 4:BuildNavigation();break;case 5:BuildButton();break;case 6:BuildInput();break;case 7:BuildList();break;case 8:BuildDrag();break;case 9:BuildValue();break;case 10:BuildNotice();break;case 11:BuildReward();break;case 12:BuildIdle();break;}}
            LayoutActions();SetState(State);
        }
        void Update(){if(!alive)return;foreach(var tick in ticking)tick();}
        public void SetState(string value){State=value;if(stateText!=null)stateText.text=value;}
        public void Record(string value){Trace.Add(value);if(Trace.Count>32)Trace.RemoveAt(0);}
        public int Get(string key)=>Data.TryGetValue(key,out var v)?v:0;
        public void Put(string key,int v){Data[key]=v;}
        public Button Operation(string name,Action action) {
            var button=RecipeUI.Button(actionsRoot,name,0,0,135,()=>{Record(name);action();});Actions.Add(name,button);actionCount++;return button;
        }
        void LayoutActions(){int i=0;float width=Mathf.Min(158,820f/Mathf.Max(1,Actions.Count)-8);foreach(var b in Actions.Values){var r=(RectTransform)b.transform;r.sizeDelta=new Vector2(width,40);r.anchoredPosition=new Vector2((i++-(Actions.Count-1)*.5f)*(width+8),0);}}
        public void Trigger(string name){if(!Actions.TryGetValue(name,out var b))throw new ArgumentException(name);if(b.IsInteractable())b.onClick.Invoke();}
        public void Transition(Action<float> renderer,bool loop=false,float? seconds=null) {
            if(!alive)return;
            StopMotion();render=renderer;Progress=0;Paused=false;playbackLoop=loop;playbackDuration=Mathf.Max(.06f,seconds??Duration);durationScale=playbackDuration/Mathf.Max(.06f,Duration);
            // Each scenario has a single coordinator; reserve every bound object's visual channels,
            // including secondaries, rather than registering only one main target.
            var bindings=new List<MotionBinding>();
            foreach(var rect in world.GetComponentsInChildren<RectTransform>(true)){
                bindings.Add(new MotionBinding(rect,MotionChannel.Position));bindings.Add(new MotionBinding(rect,MotionChannel.Scale));bindings.Add(new MotionBinding(rect,MotionChannel.Rotation));bindings.Add(new MotionBinding(rect,MotionChannel.Size));
                var group=rect.GetComponent<CanvasGroup>();if(group!=null)bindings.Add(new MotionBinding(group,MotionChannel.Alpha));
                var graphic=rect.GetComponent<Graphic>();if(graphic!=null)bindings.Add(new MotionBinding(graphic,graphic is Text?MotionChannel.TextValue:MotionChannel.Color));
            }
            channelArbiter.Acquire(this,bindings,StopMotion);
            if(Reduced){Progress=1;render(1);return;}
            render(0);
            motion=DOTween.To(()=>Progress,v=>{Progress=v;render(v);},1,playbackDuration).SetEase(DG.Tweening.Ease.Linear).SetUpdate(true).SetAutoKill(false);
            if(loop)motion.SetLoops(-1,LoopType.Restart);
            motion.timeScale=Speed;
        }
        public float Ease(float p)=>DG.Tweening.Core.Easing.EaseManager.Evaluate(Curve,null,Mathf.Clamp01(p),1,1.70158f,0);
        public void Move(RectTransform r,Vector2 target,bool elastic=false) {
            var start=r.anchoredPosition;Transition(p=>r.anchoredPosition=Vector2.LerpUnclamped(start,target,Reduced?1:elastic?DG.Tweening.Core.Easing.EaseManager.Evaluate(DG.Tweening.Ease.OutBack,null,p,1,1.70158f,0):Ease(p)));
        }
        public void Replay(){if(render==null)return;var r=render;Transition(r,playbackLoop,Duration*durationScale);Record("重播表现");}
        public void Seek(float value){if(render==null)return;motion?.Pause();Paused=true;Progress=Mathf.Clamp01(value);motion?.Goto(Progress*playbackDuration,false);render(Progress);}
        public void TogglePause(){if(motion==null)return;Paused=!Paused;if(Paused)motion.Pause();else motion.Play();}
        public void ToggleLoop(){if(render==null)return;var r=render;Transition(r,!playbackLoop,Duration*durationScale);}
        public void ReversePresentation(){if(motion==null||render==null)return;Paused=false;if(playbackLoop){var r=render;Transition(p=>r(1-p),true,Duration*durationScale);return;}if(motion.IsBackwards())motion.PlayForward();else{if(Progress<=0)motion.Goto(playbackDuration,false);motion.PlayBackwards();}}
        public void SetSpeed(float speed){Speed=speed;if(motion!=null)motion.timeScale=speed;}
        public void SetReduced(bool value){Reduced=value;if(value){Paused=false;foreach(var cancel in cancelling)cancel();StopMotion();if(render!=null){Progress=1;render(1);}}}
        void StopMotion(){Generation++;motion?.Kill(false);motion=null;channelArbiter.Release(this);}
        public void CancelInteractions(){foreach(var cancel in cancelling)cancel();foreach(var p in GetComponentsInChildren<RecipePointer>(true))p.Cancel();}
        public void Shutdown(){if(!alive)return;alive=false;CancelInteractions();StopMotion();ticking.Clear();cancelling.Clear();render=null;}
        void OnDisable(){Shutdown();}
        void OnDestroy(){Shutdown();}
        void OnApplicationFocus(bool focused){if(!focused){CancelInteractions();if(playbackLoop){StopMotion();render?.Invoke(1);}}}
        public string BusinessFingerprint(){var keys=new List<string>(Data.Keys);keys.Sort();string result=State;foreach(var k in keys)result+="|"+k+"="+Data[k];return result;}
        RectTransform Panel(string title,float x=0,float y=0,float w=420,float h=255) {
            var r=RecipeUI.Box(world,title,x,y,w,h,RecipeUI.Hex("34405A")).rectTransform;
            RecipeUI.Label(r,title,0,h*.5f-32,w-30,40,23);return r;
        }
        Text Caption(Transform p,string value,float x=0,float y=0,float w=360)=>RecipeUI.Label(p,value,x,y,w,60,19);
        void FadeSwap(RectTransform a,RectTransform b,Action extra=null) {
            float aa=RecipeUI.Group(a).alpha,ba=RecipeUI.Group(b).alpha;bool next=Get("selected")==1;
            RecipeUI.Group(a).interactable=RecipeUI.Group(a).blocksRaycasts=!next;RecipeUI.Group(b).interactable=RecipeUI.Group(b).blocksRaycasts=next;
            Transition(p=>{float t=Reduced?1:Ease(p);RecipeUI.Group(a).alpha=Mathf.Lerp(aa,next?0:1,t);RecipeUI.Group(b).alpha=Mathf.Lerp(ba,next?1:0,t);});extra?.Invoke();
        }
    }
}
