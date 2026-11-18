using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TweenGallery.Recipes
{
    public sealed partial class RecipeGalleryBootstrap
    {
        bool referenceShell,shortcuts=true,referenceScaleMode=true;GameObject filterPanel,detailsPanel;Text referenceTitle,referenceDurationValue,referenceStateValue;
        public void ShowAllRecipes(){shortcuts=false;RefreshDirectory();}
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
            Tool(root,"↻ 重置",510,472,120,()=>Select(SelectedId));
            RecipeUI.Label(root,"—    □    ×",692,472,200,45,25,RecipeUI.Ink);

            Skin(root,"Catalog frame","wb-sidebar",-676,-25,235,918);
            Skin(root,"Catalog heading","wb-gray",-676,402,235,55);
            Skin(root,"Folder heading icon","ref-folder",-756,402,33,29,false);
            var catalogTitle=RecipeUI.Label(root,"演示目录",-661,402,160,45,25);catalogTitle.font=brand.font;
            var viewport=RecipeUI.Rect(root,"Directory viewport",-676,-20,225,790);viewport.gameObject.AddComponent<RectMask2D>();var hit=viewport.gameObject.AddComponent<Image>();hit.color=Color.clear;
            directory=viewport.gameObject.AddComponent<ScrollRect>();directory.viewport=viewport;directory.horizontal=false;directory.scrollSensitivity=38;directory.movementType=ScrollRect.MovementType.Clamped;
            listContent=RecipeUI.Rect(viewport,"Directory content",0,0,223,790);listContent.anchorMin=listContent.anchorMax=new Vector2(.5f,1);listContent.pivot=new Vector2(.5f,1);listContent.anchoredPosition=Vector2.zero;directory.content=listContent;
            countLabel=RecipeUI.Label(root,"12 层级 · 96 配方",-676,-441,200,25,13,RecipeUI.Muted);
            Tool(root,"全部配方 / 搜索",-676,-469,207,()=>{shortcuts=false;filterPanel.SetActive(!filterPanel.activeSelf);RefreshDirectory();},false,30).GetComponentInChildren<Text>().fontSize=14;

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
            search=RecipeUI.Input(filterPanel.transform,"搜索配方、状态、素材",-210,0,310);search.onValueChanged.AddListener(v=>{query=v.Trim();shortcuts=false;RefreshDirectory();});
            layerLabel=Tool(filterPanel.transform,"层级 ▾",20,0,145,()=>OpenMenu(true)).GetComponentInChildren<Text>();
            stateFilterLabel=Tool(filterPanel.transform,"状态 ▾",170,0,145,()=>OpenMenu(false)).GetComponentInChildren<Text>();
            reduceLabel=Tool(filterPanel.transform,"减弱动态 关",325,0,150,()=>SetReduced(!Reduced)).GetComponentInChildren<Text>();filterPanel.SetActive(false);
            // Timeline lives in an optional tool drawer. It is not an extra permanent band on the reference stage.
            var tools=RecipeUI.Rect(root,"Playback drawer",-45,-455,950,48);
            Tool(tools,"重播",-390,0,85,()=>Stage.Replay(),false,30).GetComponentInChildren<Text>().fontSize=14;
            playLabel=Tool(tools,"暂停",-290,0,85,()=>Stage.TogglePause(),false,30).GetComponentInChildren<Text>();playLabel.fontSize=14;
            Tool(tools,"0.25× / 1×",-180,0,115,()=>{slow=!slow;Stage.SetSpeed(slow?.25f:1);},false,30).GetComponentInChildren<Text>().fontSize=14;
            timeline=RecipeUI.Slider(tools,95,0,340,0,1,0,v=>{if(!syncing)Stage.Seek(v);});timelineLabel=RecipeUI.Label(tools,"待操作",325,0,90,30,14,RecipeUI.Muted);
            tools.gameObject.SetActive(false);Tool(root,"时间轴 ▾",363,-469,150,()=>tools.gameObject.SetActive(!tools.gameObject.activeSelf),false,26).GetComponentInChildren<Text>().fontSize=13;
            Tool(root,"旧馆",745,-469,72,()=>{Stage.Shutdown();SceneManager.LoadScene("Gallery");},false,26).GetComponentInChildren<Text>().fontSize=12;
            RefreshDirectory();
        }
        void BuildShortcuts() {
            float y=-25;
            Action<string> folder=name=>{var icon=Skin(listContent,"Folder","ref-folder",-88,y,29,25,false);var text=RecipeUI.Label(listContent,name,28,y,175,35,20,RecipeUI.Ink,TextAnchor.MiddleLeft);text.font=Resources.Load<Font>("GalleryHeadingFont")??text.font;y-=47;};
            Action<string,string,string> entry=(id,name,iconKey)=>{string selected=id;var b=RecipeUI.Button(listContent,"",5,y,212,()=>{if(id=="03.08")referenceScaleMode=name!="翻转切换";Select(selected);});RecipeArt.Apply(b.GetComponent<Image>(),"wb-play");b.GetComponent<Image>().pixelsPerUnitMultiplier=1;b.GetComponent<Image>().color=id==SelectedId?Color.white:Color.clear;Skin(b.transform,"Entry icon",iconKey,-72,0,26,27,false);var text=RecipeUI.Label(b.transform,name,23,0,140,38,20,RecipeUI.Ink,TextAnchor.MiddleLeft);text.font=Resources.Load<Font>("GalleryHeadingFont")??text.font;if(!rows.ContainsKey(id))rows[id]=b.GetComponent<Image>();y-=47;};
            folder("卡牌动效");entry("03.08","卡牌展示","ref-card-icon");entry("03.08","选中缩放","focus-outline");entry("03.08","翻转切换","ref-card-icon");entry("07.04","排列过渡","ref-card-icon");y-=20;
            folder("拖放交互");entry("08.02","拖拽放置","ref-drag-icon");entry("08.05","放置校验","check");entry("08.07","槽位反馈","ref-drag-icon");y-=20;
            folder("基础动画");entry("01.02","位移滑动","direction-arrow");entry("05.03","缩放弹性","ref-card-icon");entry("12.03","旋转动画","spinner");entry("01.01","淡入淡出","bubble");
            foreach(RectTransform child in listContent)child.anchorMin=child.anchorMax=new Vector2(.5f,1);
            listContent.sizeDelta=new Vector2(223,Mathf.Max(790,-y+20));directory.verticalNormalizedPosition=1;countLabel.text="快捷示例 · 全部 96 条可搜索";
        }
        void BuildReferenceInspector() {
            if(detailsPanel!=null){Destroy(detailsPanel);detailsPanel=null;}
            foreach(Transform child in parameters){child.gameObject.SetActive(false);Destroy(child.gameObject);}
            var d=Stage.Definition;bool cards=Stage.ReferenceComposition,drag=d.Group==8;referenceTitle.text=cards?"卡牌与拖放":d.layer;
            Skin(parameters,"Selected object panel","wb-inventory",0,272,290,190);
            Skin(parameters,"Selected object header","wb-gray",0,354,290,38);RecipeUI.Label(parameters,drag?"▾ 拖拽物品":cards?"▾ 选中卡牌":"▾ 当前配方",0,354,270,34,22,RecipeUI.Ink,TextAnchor.MiddleLeft);
            string thumb=drag?"approved-amber":cards?"ref-thumbnail":d.Group==5?"button-blue":d.Group==6?"checkbox-on":d.Group==9?"bar-health":d.Group==11?"chest-body":d.Group==12?"crystal":"panel";
            Skin(parameters,"Portrait thumbnail",thumb,-91,258,88,108,false);
            RecipeUI.Label(parameters,drag?"名称  能量晶石":cards?"名称  星光旅者":d.id+"  "+d.name,49,295,180,48,18,RecipeUI.Ink,TextAnchor.MiddleLeft);
            RecipeUI.Label(parameters,cards?"稀有度":"层级",17,252,100,36,18,RecipeUI.Ink,TextAnchor.MiddleLeft);
            var rarity=Skin(parameters,"Rarity","wb-gray",96,252,82,33);if(cards)rarity.color=new Color(1,.45f,1,1);RecipeUI.Label(rarity.transform,cards?"稀有":d.Group.ToString("00"),0,0,72,30,17);
            RecipeUI.Label(parameters,"状态",17,211,100,36,18,RecipeUI.Ink,TextAnchor.MiddleLeft);
            var selected=Skin(parameters,"Selected state","wb-gray",96,211,82,32);selected.color=new Color(.4f,1,.55f,1);referenceStateValue=RecipeUI.Label(selected.transform,"已选中",0,0,74,30,16);
            Skin(parameters,"Tween header","wb-gray",0,166,290,42);RecipeUI.Label(parameters,"▾ UI 动效 (DOTween)",0,166,275,38,22,RecipeUI.Ink,TextAnchor.MiddleLeft);
            RecipeUI.Label(parameters,"动效类型",0,126,275,30,18,RecipeUI.Muted,TextAnchor.MiddleLeft);
            Tool(parameters,cards?(d.Group==8?"拖拽 (Drag)  ▾":referenceScaleMode?"缩放 (Scale)  ▾":"翻转 (Flip)  ▾"):d.name+"  ▾",0,87,280,()=>{referenceScaleMode=!referenceScaleMode;BuildReferenceInspector();},false,39);
            RecipeUI.Label(parameters,drag?"回弹模型":"缓动曲线",0,41,275,30,18,RecipeUI.Muted,TextAnchor.MiddleLeft);
            string[] names={"缓出","缓入","缓入缓出","弹性"};
            if(drag)RecipeUI.Label(parameters,"解析阻尼弹簧\n跟手时不创建 Tween",0,-12,275,85,18,RecipeUI.Ink,TextAnchor.MiddleLeft);
            else for(int i=0;i<4;i++){int k=i;var b=Tool(parameters,"",-105+i*70,-9,63,()=>{Stage.Curve=k==0?DG.Tweening.Ease.OutCubic:k==1?DG.Tweening.Ease.InCubic:k==2?DG.Tweening.Ease.InOutCubic:DG.Tweening.Ease.OutElastic;},false,58);Skin(b.transform,"Curve icon","ref-curve"+i,0,0,61,56,false);RecipeUI.Label(parameters,names[i],-105+i*70,-55,69,27,16);}
            RecipeUI.Label(parameters,drag?"回弹频率":"持续时间",0,-96,275,30,18,RecipeUI.Ink,TextAnchor.MiddleLeft);
            RecipeUI.Slider(parameters,-36,-132,195,drag?2:.08f,drag?10:2,drag?Stage.SpringFrequency:Stage.Duration,v=>{if(drag)Stage.SpringFrequency=v;else Stage.Duration=v;});var durationValue=Skin(parameters,"Duration value","wb-gray",105,-132,75,36);referenceDurationValue=RecipeUI.Label(durationValue.transform,"",0,0,72,32,16);
            RecipeUI.Label(parameters,drag?"阻尼比":"目标缩放",0,-175,275,30,18,RecipeUI.Ink,TextAnchor.MiddleLeft);
            var scaleValue=Skin(parameters,"Scale value","wb-gray",105,-210,75,36);var sv=RecipeUI.Label(scaleValue.transform,drag?Stage.SpringDamping.ToString("0.00"):"1.20 ×",0,0,72,32,16);
            RecipeUI.Slider(parameters,-36,-210,195,drag?.2f:.8f,drag?1.4f:1.5f,drag?Stage.SpringDamping:1.2f,v=>{if(drag)Stage.SpringDamping=v;else Stage.Strength=v;sv.text=v.ToString("0.00")+(drag?"":" ×");});
            Skin(parameters,"Playback header","wb-gray",0,-266,290,43);RecipeUI.Label(parameters,"▾ 演示控制",0,-266,275,38,22,RecipeUI.Ink,TextAnchor.MiddleLeft);
            Tool(parameters,"▶ 播放动画",0,-330,280,()=>GuidedOperation(),true,58);
            Tool(parameters,"↻ 循环",-73,-408,133,()=>Stage.ToggleLoop(),false,48);Tool(parameters,"↗ 反向",75,-408,133,()=>Stage.ReversePresentation(),false,48);
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
        void SyncReferenceState(){if(referenceStateValue==null||Stage==null)return;referenceStateValue.text=Stage.Get("held")==1?"拖动中":Stage.Get("cancelled")>0?"已取消":Stage.Get("rejected")>0?"已拒绝":Stage.Get("drops")>0?"已放置":Stage.Get("face")==1?"背面":"已选中";}
    }
}
