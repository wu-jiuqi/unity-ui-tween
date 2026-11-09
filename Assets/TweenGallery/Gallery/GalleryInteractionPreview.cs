using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace TweenGallery
{
    /// <summary>Embedded live previews share the main stage, skins and export workflow.</summary>
    public sealed class GalleryInteractionPreview : MonoBehaviour
    {
        public MotionButtonFeedback Feedback { get; private set; }
        public MotionJellyDrag Jelly { get; private set; }
        public MotionDragInput DragInput { get; private set; }
        public MotionPlayer Player { get; private set; }
        public Button ActionButton { get; private set; }
        public int ClickCount { get; private set; }
        public string Status { get; private set; }
        MotionSettings settings;bool reduced,toggled;MotionPose rest;
        Font font;
        static GalleryInteractionPreview Root(Transform parent,MotionSettings settings,bool reduced)
        {
            var r=Rect(parent,"Embedded interaction preview",new Vector2(418,-159.5f),new Vector2(836,319),false);
            var p=r.gameObject.AddComponent<GalleryInteractionPreview>();p.settings=settings.Clone();p.reduced=reduced;p.font=Resources.Load<Font>("GalleryFont");
            r.gameObject.AddComponent<Image>().color=new Color(.97f,.94f,.91f);return p;
        }
        public static GalleryInteractionPreview CreateButton(Transform parent,int skin,ButtonMotionStyle style,MotionSettings settings,bool reduced)
        {
            var p=Root(parent,settings,reduced);p.BuildButton(skin,style);return p;
        }
        void BuildButton(int skin,ButtonMotionStyle style)
        {
            bool compact=skin==2||skin==5;
            var hit=Rect(transform,"Fixed button hit",new Vector2(0,5),new Vector2(430,210));
            var hitImage=hit.gameObject.AddComponent<Image>();hitImage.color=Color.clear;
            ActionButton=hit.gameObject.AddComponent<Button>();ActionButton.transition=Selectable.Transition.None;ActionButton.targetGraphic=hitImage;
            var visual=Rect(hit,"Button visual",Vector2.zero,compact?new Vector2(166,166):new Vector2(352,148));
            var face=visual.gameObject.AddComponent<Image>();face.sprite=GalleryTheme.Skin(skin);face.preserveAspect=true;face.raycastTarget=false;
            Feedback=hit.gameObject.AddComponent<MotionButtonFeedback>();Feedback.surface=visual;Feedback.style=style;Feedback.intensity=settings.strength;Feedback.frequency=Mathf.Clamp(2.5f/settings.duration,3,12);Feedback.damping=.68f;Feedback.reducedMotion=reduced;Feedback.unscaledTime=settings.unscaledTime;
            var glyph=Label(visual,"Independent icon",style==ButtonMotionStyle.Loading?"":"★",new Vector2(compact?0:-110,0),new Vector2(54,54),32);
            Feedback.icon=glyph.rectTransform;
            if(style==ButtonMotionStyle.Loading){glyph.gameObject.SetActive(false);var spinner=Rect(visual,"Independent loading arc",new Vector2(compact?0:-110,0),new Vector2(45,45));var image=spinner.gameObject.AddComponent<Image>();image.sprite=GalleryPreviewShapes.Arc;image.color=GalleryTheme.Ink;image.raycastTarget=false;Feedback.icon=spinner;}
            if(!compact)Label(visual,"Button label",style==ButtonMotionStyle.Selection?"选择花园":style==ButtonMotionStyle.Loading?"开始任务":"立即体验",new Vector2(20,0),new Vector2(240,60),27);
            var maskRoot=Rect(visual,"Decorations",Vector2.zero,visual.sizeDelta);var maskImage=maskRoot.gameObject.AddComponent<Image>();maskImage.sprite=face.sprite;maskImage.preserveAspect=true;maskImage.raycastTarget=false;maskRoot.gameObject.AddComponent<Mask>().showMaskGraphic=false;
            var ripple=Rect(maskRoot,"Ripple",Vector2.zero,new Vector2(130,130));var ri=ripple.gameObject.AddComponent<Image>();ri.sprite=GalleryPreviewShapes.Disc;ri.color=Color.white;ri.raycastTarget=false;
            Feedback.ripple=ripple;Feedback.rippleGroup=Group(ripple);
            var sheen=Rect(maskRoot,"Sheen",Vector2.zero,new Vector2(34,190));sheen.localRotation=Quaternion.Euler(0,0,-22);var si=sheen.gameObject.AddComponent<Image>();si.color=Color.white;si.raycastTarget=false;Feedback.sheen=sheen;Feedback.sheenGroup=Group(sheen);
            maskRoot.SetAsFirstSibling();
            var selection=Rect(hit,"Selected surround",Vector2.zero,visual.sizeDelta+new Vector2(10,10));selection.SetAsFirstSibling();var outline=selection.gameObject.AddComponent<Image>();outline.sprite=face.sprite;outline.preserveAspect=true;outline.color=GalleryTheme.Gold;outline.raycastTarget=false;
            Feedback.selection=selection;Feedback.selectionGroup=Group(selection);Feedback.CaptureBaseline();
            ActionButton.onClick.AddListener(()=>{ClickCount++;if(style==ButtonMotionStyle.Selection)Feedback.SetSelected(!Feedback.IsSelected);if(style==ButtonMotionStyle.Loading){Feedback.SetBusy(true);ActionButton.interactable=false;}Status=style==ButtonMotionStyle.Loading?"任务进行中；点击上方“完成 / 切换状态”结束加载。":"真实点击 "+ClickCount+" 次 · "+GalleryBootstrap.FeedbackNames[(int)style];});
            Label(transform,"Instruction","按住、松开、移出，直接体验当前造型与反馈",new Vector2(0,-130),new Vector2(780,32),17);
            Status="当前反馈："+GalleryBootstrap.FeedbackNames[(int)style]+" · 固定命中层，内部视觉独立运动";
        }
        public static GalleryInteractionPreview CreateDrag(Transform parent,int skin,MotionSettings settings,bool reduced)
        {
            var p=Root(parent,settings,reduced);var hit=Rect(p.transform,"Fixed drag area",new Vector2(0,8),new Vector2(730,215));hit.gameObject.AddComponent<Image>().color=Color.clear;
            var visual=Rect(hit,"Elastic object",Vector2.zero,new Vector2(155,155));var im=visual.gameObject.AddComponent<Image>();im.sprite=GalleryTheme.Skin(5);im.preserveAspect=true;im.raycastTarget=false;
            p.Label(visual,"Drag icon","↔",Vector2.zero,new Vector2(120,70),42);
            p.Jelly=hit.gameObject.AddComponent<MotionJellyDrag>();p.Jelly.visual=visual;p.Jelly.maxDisplacement=reduced?0:settings.distance;p.Jelly.stretch=reduced?0:.16f*settings.strength;p.Jelly.maxRotation=reduced?0:8;p.Jelly.unscaledTime=settings.unscaledTime;p.Jelly.CaptureBaseline();
            p.DragInput=hit.gameObject.AddComponent<MotionDragInput>();p.DragInput.hitArea=hit;p.DragInput.jelly=p.Jelly;p.DragInput.RefreshBindings();
            p.DragInput.onDragStarted.AddListener(()=>p.Status="正在拖动 · 松开后弹性回位");p.DragInput.onReleased.AddListener(()=>p.Status="已松开 · 保留速度，弹性回位");p.DragInput.onCancelled.AddListener(()=>p.Status="操作已取消，恢复稳定基线");
            p.Label(p.transform,"Drag instruction","按住物体拖动 · 松开回弹 · 切换效果会取消当前操作",new Vector2(0,-130),new Vector2(790,32),17);p.Status="请在物体上按住并拖动";return p;
        }
        public static GalleryInteractionPreview CreateContinuation(Transform parent,MotionSettings settings,bool reduced)
        {
            var p=Root(parent,settings,reduced);var target=Rect(p.transform,"Continuing card",new Vector2(0,25),new Vector2(220,110));var image=target.gameObject.AddComponent<Image>();image.sprite=GalleryTheme.Skin(1);image.raycastTarget=false;
            p.Label(target,"Card label","焦点卡片",Vector2.zero,new Vector2(200,60),25);
            p.Player=target.gameObject.AddComponent<MotionPlayer>();p.Player.settings=settings.Clone();p.Player.bindings=new MotionBindings{target=target,group=target.gameObject.AddComponent<CanvasGroup>()};p.Player.CaptureBaseline();p.rest=MotionPose.Read(target,p.Player.bindings.group);
            p.Control("← 左",new Vector2(-230,-93),()=>p.Continue(false));p.Control("右 →",new Vector2(230,-93),()=>p.Continue(true));
            p.Label(p.transform,"Continue instruction","连续点击方向，观察当前画面直接接续；右侧可调整时长和位移",new Vector2(0,-135),new Vector2(790,28),16);p.Status="连续点击左 / 右，动画不从起点重播";return p;
        }
        public void Continue(bool right)
        {
            var pose=rest;pose.position+=Vector2.right*(right?1:-1)*settings.distance;pose.scale*=right?1.08f:.94f;pose.rotation*=Quaternion.Euler(0,0,right?-6:6);
            var ease=System.Enum.TryParse<Ease>(settings.ease,out var configured)?configured:Ease.OutCubic;
            if(reduced){Player.StopAndRestore();pose.Apply(Player.bindings.target,Player.bindings.group);Player.ReleaseAtCurrentState();}
            else Player.ContinueTo(pose,settings.duration,ease);Status="从当前姿态接续至"+(right?"右":"左")+"侧目标";
        }
        public static GalleryInteractionPreview CreateTrigger(Transform parent,string id,MotionSettings settings,bool reduced)
        {
            var p=Root(parent,settings,reduced);p.Player=DemoFactory.Create(id,p.transform,new Vector2(760,260));p.Player.StopAndRestore();
            var bg=p.gameObject.GetComponent<Image>();bg.color=new Color(.97f,.94f,.91f);
            p.ActionButton=p.gameObject.AddComponent<Button>();p.ActionButton.targetGraphic=bg;p.ActionButton.transition=Selectable.Transition.None;
            p.ActionButton.onClick.AddListener(()=>{p.ClickCount++;p.PlayTriggered(id);});
            p.Label(p.transform,"Trigger instruction",id=="B07"||id=="B08"?"点击切换状态；可在播放中再次点击":"点击画面触发一次当前动效",new Vector2(0,-137),new Vector2(780,28),16);p.Status="点击画面触发动效，保留当前主展馆参数";return p;
        }
        void PlayTriggered(string id)
        {
            if(id=="B07"||id=="B08")
            {
                bool active=Player.IsPlaying;float progress=Player.Progress;toggled=!toggled;settings.reverse=!toggled;Player.Play(id,settings);
                if(active){Player.Seek(1-progress);Player.Resume();}
            }
            else Player.Play(id,settings);
            if(reduced){Player.Seek(1);Player.ReleaseAtCurrentState();}
            Status="已触发 "+ClickCount+" 次 · "+(id=="B07"||id=="B08"?(toggled?"开启 / 选中":"关闭 / 未选中"):"点击可再次播放");
        }
        public void ChangeState()
        {
            if(Feedback==null)return;
            if(Feedback.style==ButtonMotionStyle.Loading){Feedback.SetBusy(false);ActionButton.interactable=true;Status="任务完成，可以再次触发";}
            else if(Feedback.style==ButtonMotionStyle.Selection){Feedback.SetSelected(!Feedback.IsSelected);Status=Feedback.IsSelected?"已选中":"未选中";}
        }
        void Control(string title,Vector2 position,UnityEngine.Events.UnityAction action)
        {
            var r=Rect(transform,title,position,new Vector2(160,45));var im=r.gameObject.AddComponent<Image>();im.sprite=GalleryTheme.Skin(4);var b=r.gameObject.AddComponent<Button>();b.onClick.AddListener(action);Label(r,title,title,Vector2.zero,new Vector2(150,40),20);
        }
        Text Label(Transform parent,string name,string text,Vector2 center,Vector2 size,int points)
        {
            var r=Rect(parent,name,center,size);var label=r.gameObject.AddComponent<Text>();label.font=font;label.text=text;label.fontSize=points;label.color=GalleryTheme.Ink;label.alignment=TextAnchor.MiddleCenter;label.raycastTarget=false;return label;
        }
        static CanvasGroup Group(RectTransform r){var g=r.gameObject.AddComponent<CanvasGroup>();g.alpha=0;g.blocksRaycasts=false;return g;}
        static RectTransform Rect(Transform parent,string name,Vector2 position,Vector2 size,bool centered=true)
        {
            var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=centered?new Vector2(.5f,.5f):new Vector2(0,1);r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=position;r.sizeDelta=size;return r;
        }
        void CancelOwned(){if(Feedback!=null)Feedback.CancelInteraction();if(DragInput!=null)DragInput.CancelInteraction();if(Player!=null)Player.StopAndRestore();}
        void OnDisable(){CancelOwned();}
        void OnApplicationFocus(bool focus){if(!focus){CancelOwned();Status="失去焦点，当前操作已取消";}}
        void OnApplicationPause(bool pause){if(pause){CancelOwned();Status="进入后台，当前操作已取消";}}
    }
    static class GalleryPreviewShapes
    {
        static Sprite arc,disc;
        public static Sprite Arc=>arc!=null?arc:arc=Make(true);
        public static Sprite Disc=>disc!=null?disc:disc=Make(false);
        static Sprite Make(bool ring)
        {
            var texture=new Texture2D(64,64,TextureFormat.RGBA32,false);var pixels=new Color[4096];
            for(int y=0;y<64;y++)for(int x=0;x<64;x++){float dx=x-31.5f,dy=y-31.5f,r=Mathf.Sqrt(dx*dx+dy*dy);pixels[y*64+x]=new Color(1,1,1,ring?(Mathf.Atan2(dy,dx)<2.25f?Mathf.Clamp01(3.5f-Mathf.Abs(r-24)):0):Mathf.Clamp01(30-r));}
            texture.SetPixels(pixels);texture.Apply();return Sprite.Create(texture,new Rect(0,0,64,64),new Vector2(.5f,.5f));
        }
    }
}
