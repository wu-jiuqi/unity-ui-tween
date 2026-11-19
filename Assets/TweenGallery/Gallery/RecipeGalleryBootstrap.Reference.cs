using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TweenGallery.Recipes
{
    public sealed partial class RecipeGalleryBootstrap
    {
        GameObject detailsPanel;Text referenceDurationValue,referenceStateValue,workbenchHint,curveHint;
        public int SelectedCurveIndex {get;private set;}
        readonly Image[] curveButtons=new Image[4];readonly RecipeCurveGraphic[] curveGraphs=new RecipeCurveGraphic[4];
        static readonly DG.Tweening.Ease[] CurveOptions={DG.Tweening.Ease.OutCubic,DG.Tweening.Ease.InCubic,DG.Tweening.Ease.InOutCubic,DG.Tweening.Ease.OutElastic};
        static readonly string[] CurveNames={"缓出","缓入","缓入缓出","弹性"};
        float previewDue=-1,feedbackUntil;Coroutine guidance;int popupFrame;
        Button speedButton,playButton;Text speedLabel;
        Image Skin(Transform parent,string name,string key,float x,float y,float w,float h,bool sliced=true)=>RecipeStage.Art(parent,name,key,x,y,w,h,sliced);
        Button Tool(Transform parent,string title,float x,float y,float w,Action action,bool primary=false,float h=42){var b=RecipeUI.Button(parent,title,x,y,w,action,primary?RecipeUI.Blue:RecipeUI.Surface,h);b.GetComponentInChildren<Text>().fontSize=18;return b;}
        void BuildReference(){
            if(EventSystem.current==null)new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));
            var canvas=new GameObject("Recipe Workbench",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));canvas.transform.SetParent(transform,false);canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=canvas.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,1000);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
            root=RecipeUI.Rect(canvas.transform,"Workspace",0,0,1600,1000);
            RecipeUI.Box(root,"Backdrop",0,0,2200,1400,RecipeUI.Hex("121822"));
            RecipeUI.Label(root,"UI 动效展馆",-605,467,365,44,29,RecipeUI.Ink,TextAnchor.MiddleLeft);
            RecipeUI.Label(root,"12 类 / 96 条 · 点击案例后直接操作",-195,467,480,40,18,RecipeUI.Muted,TextAnchor.MiddleLeft);
            reduceLabel=Tool(root,"减弱动态：关",500,467,175,()=>SetReduced(!Reduced),false,38).GetComponentInChildren<Text>();
            Tool(root,"旧馆归档",686,467,150,()=>{Stage.Shutdown();SceneManager.LoadScene("Gallery");},false,38);
            RecipeUI.Box(root,"Directory frame",-652,-19,282,922,RecipeUI.Hex("1C2431"));
            RecipeUI.Label(root,"演示目录   12 类 × 8 条",-652,404,254,40,21,RecipeUI.Ink,TextAnchor.MiddleLeft);
            search=RecipeUI.Input(root,"搜索名称 / 编号 / 素材 / 类型",-652,353,254,42);search.onValueChanged.AddListener(v=>{query=v.Trim();collapsed.Clear();RefreshDirectory();});
            stateFilterLabel=Tool(root,"状态：全部 ▾",-718,300,122,()=>OpenMenu(false),false,38).GetComponentInChildren<Text>();stateFilterLabel.fontSize=16;
            layerLabel=Tool(root,"对象：全部 ▾",-586,300,122,()=>OpenMenu(true),false,38).GetComponentInChildren<Text>();layerLabel.fontSize=16;
            var viewport=RecipeUI.Rect(root,"Directory viewport",-652,-70,254,674);viewport.gameObject.AddComponent<RectMask2D>();var hit=viewport.gameObject.AddComponent<Image>();hit.color=new Color(1,1,1,.01f);
            directory=viewport.gameObject.AddComponent<ScrollRect>();directory.viewport=viewport;directory.horizontal=false;directory.scrollSensitivity=40;directory.movementType=ScrollRect.MovementType.Clamped;
            listContent=RecipeUI.Rect(viewport,"Directory content",0,0,254,800);listContent.anchorMin=listContent.anchorMax=new Vector2(.5f,1);listContent.pivot=new Vector2(.5f,1);listContent.anchoredPosition=Vector2.zero;directory.content=listContent;
            var track=RecipeUI.Box(root,"Directory scrollbar",-518,-70,7,674,RecipeUI.Dark);track.raycastTarget=true;var bar=track.gameObject.AddComponent<Scrollbar>();var thumb=RecipeUI.Box(track.transform,"Scroll thumb",0,0,7,0,RecipeUI.Hex("71839C"));thumb.raycastTarget=true;bar.handleRect=thumb.rectTransform;bar.targetGraphic=thumb;bar.direction=Scrollbar.Direction.BottomToTop;directory.verticalScrollbar=bar;
            countLabel=RecipeUI.Label(root,"",-652,-443,254,34,16,RecipeUI.Muted);
            RecipeUI.Box(root,"Stage frame",-5,-19,988,922,RecipeUI.Hex("182131"));
            heading=RecipeUI.Label(root,"",-5,393,920,48,32,RecipeUI.Ink,TextAnchor.MiddleLeft);
            operation=RecipeUI.Label(root,"",-5,347,920,38,21,RecipeUI.Ink,TextAnchor.MiddleLeft);
            observe=RecipeUI.Label(root,"",-5,308,920,34,19,RecipeUI.Muted,TextAnchor.MiddleLeft);
            RecipeUI.Box(root,"Stage header divider",-5,279,920,1,RecipeUI.Hex("344156"));
            stageHost=RecipeUI.Rect(root,"Interactive stage",-5,-95,970,760);stageHost.gameObject.AddComponent<RectMask2D>();
            // Scenario operation buttons live below the demonstration, outside its clipping area.
            workbenchHint=RecipeUI.Label(root,"",-5,-407,920,27,15,RecipeUI.Muted);
            var tools=RecipeUI.Rect(root,"Playback controls",-5,-449,950,58);RecipeUI.Box(tools,"Playback surface",0,0,950,58,RecipeUI.Hex("263348"));
            playButton=Tool(tools,"播放演示",-385,0,135,PlayOrPause,true,42);playButton.name="Playback play";playLabel=playButton.GetComponentInChildren<Text>();
            var replay=Tool(tools,"重播",-268,0,80,()=>{StopGuidance();if(Stage.CanSeek)Stage.Replay();else GuidedOperation();},false,42);replay.name="Playback replay";
            speedButton=Tool(tools,"速度 1×",-168,0,108,()=>{slow=!slow;Stage.SetSpeed(slow?.25f:1);},false,42);speedLabel=speedButton.GetComponentInChildren<Text>();
            timeline=RecipeUI.Slider(tools,58,0,305,0,1,0,v=>{if(!syncing){StopGuidance();Stage.Seek(v);}});timelineLabel=RecipeUI.Label(tools,"",248,0,60,35,16);
            Tool(tools,"更多 ▴",370,0,120,OpenPlaybackMenu,false,42).name="Playback more";
            RecipeUI.Box(root,"Parameters frame",650,-19,282,922,RecipeUI.Hex("1C2431"));
            RecipeUI.Label(root,"当前案例参数",650,404,250,42,23,RecipeUI.Ink,TextAnchor.MiddleLeft);
            parameters=RecipeUI.Rect(root,"Parameters",650,-18,256,816);
            RefreshDirectory();
        }
        void BuildReferenceInspector(){
            if(detailsPanel!=null){detailsPanel.SetActive(false);Destroy(detailsPanel);detailsPanel=null;}
            foreach(Transform child in parameters){child.gameObject.SetActive(false);Destroy(child.gameObject);}Array.Clear(curveButtons,0,4);Array.Clear(curveGraphs,0,4);curveHint=null;referenceDurationValue=null;
            var d=Stage.Definition;bool drag=d.Group==8;
            RecipeUI.Label(parameters,d.id+"  "+d.name,0,363,250,60,22,RecipeUI.Ink,TextAnchor.MiddleLeft);
            RecipeUI.Label(parameters,"当前状态",0,305,250,30,16,RecipeUI.Muted,TextAnchor.MiddleLeft);
            referenceStateValue=RecipeUI.Label(parameters,Stage.DisplayStatus,0,253,250,76,22,RecipeUI.Ink,TextAnchor.UpperLeft);
            RecipeUI.Box(parameters,"Parameter separator",0,200,250,1,RecipeUI.Hex("344156"));
            RecipeUI.Label(parameters,drag?"回弹频率":Stage.ClockEditable?"持续时间":"驱动方式",0,169,250,30,18,RecipeUI.Ink,TextAnchor.MiddleLeft);
            if(drag||Stage.ClockEditable){RecipeUI.Slider(parameters,-32,131,178,drag?2:.08f,drag?10:2,drag?Stage.SpringFrequency:Stage.Duration,v=>{if(drag)Stage.SpringFrequency=v;else Stage.Duration=v;});referenceDurationValue=RecipeUI.Label(parameters,"",100,131,63,36,17);}
            else RecipeUI.Label(parameters,"由输入或真实时间决定",0,128,250,40,17,RecipeUI.Muted,TextAnchor.MiddleLeft);
            RecipeUI.Label(parameters,drag?"回弹阻尼":"缓动曲线",0,79,250,30,18,RecipeUI.Ink,TextAnchor.MiddleLeft);
            if(drag){RecipeUI.Slider(parameters,-32,39,178,.2f,1.4f,Stage.SpringDamping,v=>Stage.SpringDamping=v);dampingValue=RecipeUI.Label(parameters,Stage.SpringDamping.ToString("0.00"),100,39,63,36,17);}
            else if(Stage.CurveEditable){for(int i=0;i<4;i++){int k=i;var b=Tool(parameters,"",-94+i*63,24,57,()=>ChooseCurve(k),false,56);b.name="Curve choice "+i;curveButtons[i]=b.GetComponent<Image>();var r=RecipeUI.Rect(b.transform,"Actual easing",0,0,55,52);var graph=r.gameObject.AddComponent<RecipeCurveGraphic>();graph.curve=CurveOptions[i];graph.color=RecipeUI.Hex("59D9F5");graph.raycastTarget=false;curveGraphs[i]=graph;RecipeUI.Label(parameters,CurveNames[i],-94+i*63,-20,61,28,14);}curveHint=RecipeUI.Label(parameters,"",0,-53,250,35,14,RecipeUI.Muted);RefreshCurveSelection();}
            else RecipeUI.Label(parameters,"固定节奏 / 实时输入\n本案例不使用缓动曲线",0,16,250,82,17,RecipeUI.Muted,TextAnchor.MiddleLeft);
            if(Stage.StrengthEditable){RecipeUI.Label(parameters,"表现强度",0,-113,250,32,18,RecipeUI.Ink,TextAnchor.MiddleLeft);RecipeUI.Slider(parameters,-32,-151,178,.8f,1.5f,Stage.Strength,v=>{Stage.Strength=v;previewDue=Time.unscaledTime+.2f;});strengthValue=RecipeUI.Label(parameters,Stage.Strength.ToString("0.00")+"×",100,-151,63,36,17);}else strengthValue=null;
            RecipeUI.Label(parameters,"演示对象",0,-217,250,30,16,RecipeUI.Muted,TextAnchor.MiddleLeft);
            RecipeUI.Label(parameters,string.Join(" / ",d.objectTags),0,-253,250,52,18,RecipeUI.Ink,TextAnchor.UpperLeft);
            RecipeUI.Label(parameters,"状态标签",0,-300,250,30,16,RecipeUI.Muted,TextAnchor.MiddleLeft);
            RecipeUI.Label(parameters,string.Join(" / ",d.stateTags),0,-337,250,50,17,RecipeUI.Ink,TextAnchor.UpperLeft);
            Tool(parameters,"验收 / 导出 / 接入 ▸",0,-398,250,OpenReferenceDetails,false,38).GetComponentInChildren<Text>().fontSize=16;
            SyncReferenceState();
        }
        Text dampingValue,strengthValue;
        void OpenReferenceDetails(){
            if(detailsPanel!=null){detailsPanel.SetActive(false);Destroy(detailsPanel);detailsPanel=null;return;}
            var d=Stage.Definition;detailsPanel=RecipeUI.Rect(root,"Recipe details",650,-19,272,822).gameObject;RecipeUI.Box(detailsPanel.transform,"Details surface",0,0,272,822,RecipeUI.Hex("253246"));
            RecipeUI.Label(detailsPanel.transform,d.id+" "+d.name,0,358,246,65,23);
            RecipeUI.Label(detailsPanel.transform,"边界验收\n"+d.boundary+"\n\n减弱动态\n"+d.reducedMotion+"\n\n接入原则\n业务状态先确定，动画只负责表现。重播与时间轴不重复提交结果。",0,85,242,435,18,RecipeUI.Ink,TextAnchor.UpperLeft);
            Tool(detailsPanel.transform,"复制当前配方 JSON",0,-230,242,CopyParameters,true);
            Tool(detailsPanel.transform,"返回参数",0,-345,242,()=>{Destroy(detailsPanel);detailsPanel=null;});
        }
        void SyncReferenceState(){if(Stage==null)return;if(referenceStateValue!=null)referenceStateValue.text=Stage.DisplayStatus;if(referenceDurationValue!=null)referenceDurationValue.text=Stage.Definition.Group==8?Stage.SpringFrequency.ToString("0.0")+" Hz":Stage.Duration.ToString("0.00")+"s";if(dampingValue!=null)dampingValue.text=Stage.SpringDamping.ToString("0.00");if(strengthValue!=null)strengthValue.text=Stage.Strength.ToString("0.00")+"×";}
    }
}
