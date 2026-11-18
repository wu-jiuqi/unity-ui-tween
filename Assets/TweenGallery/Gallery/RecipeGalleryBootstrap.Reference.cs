using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TweenGallery.Recipes
{
    public sealed partial class RecipeGalleryBootstrap
    {
        bool referenceShell,shortcuts=true,referenceScaleMode=true;GameObject filterPanel,detailsPanel;
        Text referenceTitle,referenceDurationValue,referenceStateValue,referenceObjectName,workbenchHint,curveHint,loopLabel,reverseLabel;
        public string SelectedShortcut {get;private set;}="卡牌展示";
        public string PreviewMode {get;private set;}="scale";
        public int SelectedCurveIndex {get;private set;}
        readonly Image[] curveButtons=new Image[4];readonly RecipeCurveGraphic[] curveGraphs=new RecipeCurveGraphic[4];
        static readonly DG.Tweening.Ease[] CurveOptions={DG.Tweening.Ease.OutCubic,DG.Tweening.Ease.InCubic,DG.Tweening.Ease.InOutCubic,DG.Tweening.Ease.OutElastic};
        static readonly string[] CurveNames={"缓出","缓入","缓入缓出","弹性"};
        float previewDue=-1;Coroutine guidance;
        Button speedButton;
        public void ShowAllRecipes(){shortcuts=false;SelectedShortcut="";RefreshDirectory();}
        Image Skin(Transform parent,string name,string key,float x,float y,float w,float h,bool sliced=true) {
            var image=RecipeStage.Art(parent,name,key,x,y,w,h,sliced);if(key.StartsWith("wb-"))image.pixelsPerUnitMultiplier=1;return image;
        }
        Button Tool(Transform parent,string title,float x,float y,float w,Action action,bool primary=false,float h=42) {
            var b=RecipeUI.Button(parent,title,x,y,w,action,null,h);RecipeArt.Apply(b.GetComponent<Image>(),primary?"wb-play":"wb-gray");b.GetComponent<Image>().pixelsPerUnitMultiplier=1;
            var t=b.GetComponentInChildren<Text>();t.fontSize=20;t.font=Resources.Load<Font>("GalleryHeadingFont")??t.font;return b;
        }
        void BuildReference() {
            referenceShell=true;
            if(EventSystem.current==null)new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));
            var canvas=new GameObject("Recipe Workbench",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));canvas.transform.SetParent(transform,false);canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=canvas.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,1000);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
            root=RecipeUI.Rect(canvas.transform,"Workspace",0,0,1600,1000);
            RecipeUI.Box(root,"Outside background",0,0,2000,1400,RecipeUI.Hex("121824"));
            Skin(root,"Application bar","wb-gray",0,472,1610,60);
            Skin(root,"Application logo","ref-logo",-754,472,43,43,false);
            var brand=RecipeUI.Label(root,"Unity UI 动效演示工作台",-493,472,460,45,23,RecipeUI.Ink,TextAnchor.MiddleLeft);brand.font=Resources.Load<Font>("GalleryHeadingFont")??brand.font;
            Skin(root,"Title plaque","wb-title",5,472,350,57);referenceTitle=RecipeUI.Label(root,"卡牌与拖放",5,472,300,47,31,RecipeUI.Gold);referenceTitle.font=brand.font;
            Tool(root,"▶ 运行演示",360,472,145,()=>GuidedOperation(),true);
            Tool(root,"↻ 重置",510,472,120,ResetCurrent);
            Tool(root,"—",647,472,48,MinimizeWindow,false,34);Tool(root,"□",701,472,48,ToggleWindowSize,false,34);Tool(root,"×",755,472,48,()=>Application.Quit(),false,34);

            Skin(root,"Catalog frame","wb-sidebar",-676,-25,235,918);
            Skin(root,"Catalog heading","wb-gray",-676,402,235,55);
            Skin(root,"Folder heading icon","ref-folder",-756,402,33,29,false);
            var catalogTitle=RecipeUI.Label(root,"演示目录",-661,402,160,45,25);catalogTitle.font=brand.font;
            search=RecipeUI.Input(root,"搜索名称、编号、素材",-676,351,209,38);search.onValueChanged.AddListener(v=>{query=v.Trim();ShowAllRecipes();});
            var viewport=RecipeUI.Rect(root,"Directory viewport",-676,-52,225,742);viewport.gameObject.AddComponent<RectMask2D>();var hit=viewport.gameObject.AddComponent<Image>();hit.color=Color.clear;
            directory=viewport.gameObject.AddComponent<ScrollRect>();directory.viewport=viewport;directory.horizontal=false;directory.scrollSensitivity=38;directory.movementType=ScrollRect.MovementType.Clamped;
            listContent=RecipeUI.Rect(viewport,"Directory content",0,0,223,790);listContent.anchorMin=listContent.anchorMax=new Vector2(.5f,1);listContent.pivot=new Vector2(.5f,1);listContent.anchoredPosition=Vector2.zero;directory.content=listContent;
            var scrollTrack=RecipeUI.Box(root,"Directory scrollbar",-566,-52,9,742,RecipeUI.Dark);scrollTrack.raycastTarget=true;var scrollbar=scrollTrack.gameObject.AddComponent<Scrollbar>();var thumb=RecipeUI.Box(scrollTrack.transform,"Scroll thumb",0,0,7,0,RecipeUI.Hex("7788A6"));thumb.raycastTarget=true;scrollbar.handleRect=thumb.rectTransform;scrollbar.targetGraphic=thumb;scrollbar.direction=Scrollbar.Direction.BottomToTop;directory.verticalScrollbar=scrollbar;directory.verticalScrollbarVisibility=ScrollRect.ScrollbarVisibility.AutoHide;
            countLabel=RecipeUI.Label(root,"12 层级 · 96 配方",-676,-441,200,25,13,RecipeUI.Muted);
            Tool(root,"全部 96 条 / 筛选",-676,-469,207,()=>{ShowAllRecipes();filterPanel.SetActive(!filterPanel.activeSelf);},false,30).GetComponentInChildren<Text>().fontSize=14;
            Tool(root,"快捷示例",-676,-432,207,()=>{shortcuts=true;query="";search.SetTextWithoutNotify("");layerFilter=stateFilter=0;SelectedShortcut="卡牌展示";PreviewMode="scale";referenceScaleMode=true;RefreshDirectory();Select("03.08");},false,30).GetComponentInChildren<Text>().fontSize=14;
            countLabel.gameObject.SetActive(false);

            Skin(root,"Demonstration frame","wb-stage",-45,-25,1015,918);
            stageHost=RecipeUI.Rect(root,"Interactive stage",-45,-25,1010,910);
            stageHost.gameObject.AddComponent<RectMask2D>();
            heading=RecipeUI.Label(root,"",-45,375,930,42,25,RecipeUI.Ink,TextAnchor.MiddleLeft);
            operation=RecipeUI.Label(root,"",-45,336,930,30,17,RecipeUI.Ink,TextAnchor.MiddleLeft);
            observe=RecipeUI.Label(root,"",-45,304,930,30,16,RecipeUI.Muted,TextAnchor.MiddleLeft);

            Skin(root,"Inspector frame","wb-inspector",635,-25,315,918);
            Skin(root,"Inspector heading","wb-gray",635,402,315,55);
            var gear=Skin(root,"Inspector gear","ref-gear",515,402,35,34,false);gear.raycastTarget=true;gear.gameObject.AddComponent<Button>().onClick.AddListener(OpenReferenceDetails);
            var inspectorTitle=RecipeUI.Label(root,"属性检查器",650,402,220,45,25);inspectorTitle.font=brand.font;
            parameters=RecipeUI.Rect(root,"Parameters",635,-25,290,840);

            // Search and accessibility remain available without displacing the approved default composition.
            filterPanel=RecipeUI.Rect(root,"Search filters",-150,407,800,53).gameObject;
            Skin(filterPanel.transform,"Filter surface","wb-gray",0,0,800,53);
            RecipeUI.Label(filterPanel.transform,"筛选完整目录",-240,0,270,38,18);
            layerLabel=Tool(filterPanel.transform,"层级 ▾",20,0,145,()=>OpenMenu(true)).GetComponentInChildren<Text>();
            stateFilterLabel=Tool(filterPanel.transform,"状态 ▾",170,0,145,()=>OpenMenu(false)).GetComponentInChildren<Text>();
            reduceLabel=Tool(filterPanel.transform,"减弱动态 关",325,0,150,()=>SetReduced(!Reduced)).GetComponentInChildren<Text>();filterPanel.SetActive(false);
            workbenchHint=RecipeUI.Label(root,"单击选中卡牌 · 双击翻面 · 拖动晶石到插槽 · Esc 取消拖动",-45,-416,930,26,15,RecipeUI.Muted);
            var tools=RecipeUI.Rect(root,"Playback controls",-45,-453,950,48);Skin(tools,"Playback background","wb-gray",0,0,950,46);
            Tool(tools,"重播",-405,0,85,()=>{if(Stage.CanSeek)Stage.Replay();else GuidedOperation();},false,34).GetComponentInChildren<Text>().fontSize=15;
            playLabel=Tool(tools,"暂停",-305,0,85,()=>Stage.TogglePause(),false,34).GetComponentInChildren<Text>();playLabel.fontSize=15;
            speedButton=Tool(tools,"速度 1×",-180,0,115,()=>{slow=!slow;Stage.SetSpeed(slow?.25f:1);},false,34);speedButton.GetComponentInChildren<Text>().fontSize=14;
            timeline=RecipeUI.Slider(tools,95,0,340,0,1,0,v=>{if(!syncing)Stage.Seek(v);});timelineLabel=RecipeUI.Label(tools,"待操作",325,0,90,30,14,RecipeUI.Muted);
            Tool(root,"旧馆",745,-469,72,()=>{Stage.Shutdown();SceneManager.LoadScene("Gallery");},false,26).GetComponentInChildren<Text>().fontSize=12;
            RefreshDirectory();
        }
        void BuildShortcuts() {
            float y=-25;
            Action<string> folder=name=>{Skin(listContent,"Folder","ref-folder",-88,y,29,25,false);var text=RecipeUI.Label(listContent,name,28,y,175,35,20,RecipeUI.Ink,TextAnchor.MiddleLeft);text.font=Resources.Load<Font>("GalleryHeadingFont")??text.font;y-=40;};
            Action<string,string,string> entry=(id,name,iconKey)=>{var b=RecipeUI.Button(listContent,"",5,y,212,()=>SelectShortcut(name));b.name="Shortcut "+name;RecipeArt.Apply(b.GetComponent<Image>(),"wb-play");b.GetComponent<Image>().pixelsPerUnitMultiplier=1;b.GetComponent<Image>().color=name==SelectedShortcut?Color.white:new Color(1,1,1,.025f);Skin(b.transform,"Entry icon",iconKey,-72,0,26,27,false);var text=RecipeUI.Label(b.transform,name,23,0,140,38,20,RecipeUI.Ink,TextAnchor.MiddleLeft);text.font=Resources.Load<Font>("GalleryHeadingFont")??text.font;rows[name]=b.GetComponent<Image>();y-=40;};
            folder("卡牌动效");entry("03.08","卡牌展示","ref-card-icon");entry("03.08","选中缩放","focus-outline");entry("03.08","翻转切换","ref-card-icon");entry("07.04","排列过渡","ref-card-icon");y-=20;
            folder("拖放交互");entry("08.02","拖拽放置","ref-drag-icon");entry("08.05","放置校验","check");entry("08.07","槽位反馈","ref-drag-icon");y-=20;
            folder("基础动画");entry("01.02","位移滑动","direction-arrow");entry("05.03","缩放弹性","ref-card-icon");entry("12.03","旋转动画","spinner");entry("01.01","淡入淡出","bubble");
            foreach(RectTransform child in listContent)child.anchorMin=child.anchorMax=new Vector2(.5f,1);
            listContent.sizeDelta=new Vector2(223,Mathf.Max(742,-y+20));directory.verticalNormalizedPosition=1;
        }
        void BuildReferenceInspector() {
            if(detailsPanel!=null){Destroy(detailsPanel);detailsPanel=null;}
            foreach(Transform child in parameters){child.gameObject.SetActive(false);Destroy(child.gameObject);}
            Array.Clear(curveButtons,0,curveButtons.Length);Array.Clear(curveGraphs,0,curveGraphs.Length);curveHint=null;
            var d=Stage.Definition;bool cards=Stage.ReferenceComposition,drag=d.Group==8;referenceTitle.text=cards?"卡牌与拖放":d.layer;
            Skin(parameters,"Selected object panel","wb-inventory",0,272,290,190);
            Skin(parameters,"Selected object header","wb-gray",0,354,290,38);RecipeUI.Label(parameters,drag?"▾ 拖拽物品":cards?"▾ 选中卡牌":"▾ 当前配方",0,354,270,34,22,RecipeUI.Ink,TextAnchor.MiddleLeft);
            string thumb=drag?"approved-amber":cards?"ref-thumbnail":d.Group==5?"button-blue":d.Group==6?"checkbox-on":d.Group==9?"bar-health":d.Group==11?"chest-body":d.Group==12?"crystal":"panel";
            Skin(parameters,"Portrait thumbnail",thumb,-91,258,88,108,false);
            referenceObjectName=RecipeUI.Label(parameters,drag?"名称  能量晶石":cards?"名称  星光旅者":d.id+"  "+d.name,49,295,180,48,18,RecipeUI.Ink,TextAnchor.MiddleLeft);
            RecipeUI.Label(parameters,cards?"稀有度":"层级",17,252,100,36,18,RecipeUI.Ink,TextAnchor.MiddleLeft);
            var rarity=Skin(parameters,"Rarity","wb-gray",96,252,82,33);if(cards)rarity.color=new Color(1,.45f,1,1);RecipeUI.Label(rarity.transform,cards?"稀有":d.Group.ToString("00"),0,0,72,30,17);
            RecipeUI.Label(parameters,"状态",17,211,100,36,18,RecipeUI.Ink,TextAnchor.MiddleLeft);
            var selected=Skin(parameters,"Selected state","wb-gray",96,211,82,32);selected.color=new Color(.4f,1,.55f,1);referenceStateValue=RecipeUI.Label(selected.transform,"已选中",0,0,74,30,16);
            Skin(parameters,"Tween header","wb-gray",0,166,290,42);RecipeUI.Label(parameters,"▾ UI 动效 (DOTween)",0,166,275,38,22,RecipeUI.Ink,TextAnchor.MiddleLeft);
            RecipeUI.Label(parameters,"动效类型",0,126,275,30,18,RecipeUI.Muted,TextAnchor.MiddleLeft);
            if(cards&&!drag)Tool(parameters,ModeTitle()+"  ▾",0,87,280,OpenModeMenu,false,39);
            else RecipeUI.Label(parameters,drag?"拖拽 / 吸附 / 回弹":d.name,0,87,275,39,21,RecipeUI.Ink,TextAnchor.MiddleLeft);
            RecipeUI.Label(parameters,drag?"回弹模型":"缓动曲线",0,41,275,30,18,RecipeUI.Muted,TextAnchor.MiddleLeft);
            if(drag){
                Tool(parameters,"有效放置",-72,3,135,()=>StartDragGuide(false),false,36).GetComponentInChildren<Text>().fontSize=17;
                Tool(parameters,"无效回弹",72,3,135,()=>StartDragGuide(true),false,36).GetComponentInChildren<Text>().fontSize=17;
                Tool(parameters,"取消拖动 / Esc",0,-42,280,CancelDragGuide,false,34).GetComponentInChildren<Text>().fontSize=16;
            } else if(Stage.CurveEditable) {
                for(int i=0;i<4;i++){int k=i;var b=Tool(parameters,"",-105+i*70,-9,63,()=>ChooseCurve(k),false,56);b.name="Curve choice "+i;curveButtons[i]=b.GetComponent<Image>();var r=RecipeUI.Rect(b.transform,"Evaluated curve",0,0,61,52);var graph=r.gameObject.AddComponent<RecipeCurveGraphic>();graph.curve=CurveOptions[i];graph.color=RecipeUI.Hex("52DFFF");graph.raycastTarget=false;curveGraphs[i]=graph;RecipeUI.Label(parameters,CurveNames[i],-105+i*70,-51,69,27,16);}
                curveHint=RecipeUI.Label(parameters,"",0,-76,280,23,13,RecipeUI.Muted);RefreshCurveSelection();
            } else RecipeUI.Label(parameters,"本配方使用固定节奏 / 实时输入\n曲线选择不适用于此操作",0,-15,275,95,17,RecipeUI.Muted);
            RecipeUI.Label(parameters,drag?"回弹频率":Stage.ClockEditable?"持续时间":"驱动方式",0,-96,275,30,18,RecipeUI.Ink,TextAnchor.MiddleLeft);
            referenceDurationValue=null;
            if(drag||Stage.ClockEditable){RecipeUI.Slider(parameters,-36,-132,195,drag?2:.08f,drag?10:2,drag?Stage.SpringFrequency:Stage.Duration,v=>{if(drag)Stage.SpringFrequency=v;else Stage.Duration=v;});var durationValue=Skin(parameters,"Duration value","wb-gray",105,-132,75,36);referenceDurationValue=RecipeUI.Label(durationValue.transform,(drag?Stage.SpringFrequency:Stage.Duration).ToString("0.00")+(drag?" Hz":" s"),0,0,72,32,16);}
            else RecipeUI.Label(parameters,"由真实时间或指针输入驱动",0,-132,280,36,16,RecipeUI.Muted);
            bool scaleControl=cards&&!drag&&(PreviewMode=="scale"||PreviewMode=="");
            RecipeUI.Label(parameters,drag?"阻尼比":scaleControl?"目标缩放":Stage.StrengthEditable&&PreviewMode!="flip"?"表现强度":"操作说明",0,-175,275,30,18,RecipeUI.Ink,TextAnchor.MiddleLeft);
            if(drag||Stage.StrengthEditable&&(PreviewMode!="flip")){
                var scaleValue=Skin(parameters,"Scale value","wb-gray",105,-210,75,36);var sv=RecipeUI.Label(scaleValue.transform,(drag?Stage.SpringDamping:Stage.Strength).ToString("0.00")+(drag?"":" ×"),0,0,72,32,16);
                RecipeUI.Slider(parameters,-36,-210,195,drag?.2f:.8f,drag?1.4f:1.5f,drag?Stage.SpringDamping:Stage.Strength,v=>{if(drag)Stage.SpringDamping=v;else{Stage.Strength=v;if(scaleControl||PreviewMode=="slide")previewDue=Time.unscaledTime+.18f;}sv.text=v.ToString("0.00")+(drag?"":" ×");});
            } else RecipeUI.Label(parameters,cards?"单击选中卡牌 · 双击翻面":"直接操作舞台中的展示对象",0,-210,280,40,16,RecipeUI.Muted);
            Skin(parameters,"Playback header","wb-gray",0,-266,290,43);RecipeUI.Label(parameters,"▾ 演示控制",0,-266,275,38,22,RecipeUI.Ink,TextAnchor.MiddleLeft);
            Tool(parameters,drag?"▶ 演示完整拖放":"▶ 播放动画",0,-330,280,()=>GuidedOperation(),true,58);
            loopLabel=Tool(parameters,"循环：关",-73,-408,133,TogglePresentationLoop,false,48).GetComponentInChildren<Text>();reverseLabel=Tool(parameters,"反向播放",75,-408,133,ReversePresentation,false,48).GetComponentInChildren<Text>();
            loopLabel.GetComponentInParent<Button>().interactable=Stage.ClockEditable;reverseLabel.GetComponentInParent<Button>().interactable=Stage.ClockEditable;
            var headingFont=Resources.Load<Font>("GalleryHeadingFont");if(headingFont!=null)foreach(var label in parameters.GetComponentsInChildren<Text>())if(label.fontSize>=21)label.font=headingFont;
        }
        void OpenReferenceDetails(){
            if(detailsPanel!=null){Destroy(detailsPanel);detailsPanel=null;return;}
            var d=Stage.Definition;detailsPanel=RecipeUI.Rect(root,"Recipe details",635,-25,300,840).gameObject;Skin(detailsPanel.transform,"Details plate","wb-inspector",0,0,300,840);
            RecipeUI.Label(detailsPanel.transform,d.id+" "+d.name,0,370,270,55,23);
            RecipeUI.Label(detailsPanel.transform,"怎么操作\n"+d.operation+"\n\n看什么\n"+d.observe+"\n\n边界验收\n"+d.boundary+"\n\n减弱动态\n"+d.reducedMotion,0,85,264,490,17,RecipeUI.Ink,TextAnchor.UpperLeft);
            Tool(detailsPanel.transform,Reduced?"减弱动态：开":"减弱动态：关",0,-220,260,()=>{SetReduced(!Reduced);Destroy(detailsPanel);detailsPanel=null;OpenReferenceDetails();});
            Tool(detailsPanel.transform,"复制配方参数 JSON",0,-280,260,CopyParameters,true);
            Tool(detailsPanel.transform,"返回参数",0,-350,260,()=>{Destroy(detailsPanel);detailsPanel=null;});
        }
        void SyncReferenceState(){if(referenceStateValue==null||Stage==null)return;var s=Stage.State;referenceStateValue.text=Stage.Get("held")==1?"拖动中":s.Contains("Cancelled")?"已取消":s.Contains("Rejected")?"已拒绝":s.Contains("Accepted")||s.Contains("排序提交")?"已放置":Stage.Get("face")==1?"背面":"已选中";}
    }
}
