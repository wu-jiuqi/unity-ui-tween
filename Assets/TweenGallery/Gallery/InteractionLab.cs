using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace TweenGallery
{
    /// <summary>Playable examples built from the same public components shipped with the templates.</summary>
    public sealed class InteractionLab : MonoBehaviour
    {
        public MotionButtonInput ButtonInput { get; private set; }
        public MotionDragInput DragInput { get; private set; }
        public MotionJellyDrag Jelly { get; private set; }
        public MotionPlayer ContinuationPlayer { get; private set; }
        public MotionPlaybackGate Gate { get; private set; }
        public bool ReducedMotion { get; private set; }
        public bool HasGeneratedBackground { get; private set; }
        Action onClose;
        Sprite bgSprite, rounded;
        Font font;
        Text gateLabel, status;
        MotionPose poseA, poseB;

        static readonly Color Ink = Hex("11142B"), Panel = new Color(.075f,.095f,.18f,.94f), White = GalleryTheme.Ink;
        static readonly Color Purple=Hex("9254FF"), Pink=Hex("FF4EAF"), Cyan=Hex("19D9FF"), Yellow=Hex("FFE14A"), Mint=Hex("65EA75"), Orange=Hex("FF8A22");

        public static InteractionLab Create(Transform parent, Action closed)
        {
            var go=new GameObject("Interaction Lab",typeof(RectTransform));
            var rect=go.GetComponent<RectTransform>(); rect.SetParent(parent,false);
            rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
            var lab=go.AddComponent<InteractionLab>();lab.onClose=closed;lab.Build();return lab;
        }

        void Build()
        {
            font=Resources.Load<Font>("GalleryFont");
            if(font==null) font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            rounded=GalleryShapes.Get("rounded");
            var background=Image(transform,"Background",0,0,1600,900,Color.white,false);
            // Modal hit surface prevents pointer events from reaching the catalog beneath.
            background.raycastTarget=true;
            var texture=Resources.Load<Texture2D>("ObservatoryBackground");
            if(texture!=null)
            {
                bgSprite=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f,.5f));
                background.sprite=bgSprite; background.type=UnityEngine.UI.Image.Type.Simple; background.preserveAspect=false;
                HasGeneratedBackground=true;
            }
            else background.color=Ink;
            Image(transform,"Contrast overlay",0,0,1600,900,new Color(.025f,.04f,.12f,.70f),false);
            Label(transform,"INTERACTION LAB",46,27,520,35,25,White,TextAnchor.MiddleLeft);
            Label(transform,"真实输入 · 物理弹簧 · 当前姿态接续 · 减弱动效",46,68,820,27,15,new Color(.75f,.84f,.98f,1),TextAnchor.MiddleLeft);
            Button(transform,"返回图鉴",1382,28,164,48,Purple,White,Close);

            PanelAt("Button card",40,119,740,325);
            Header("01", "触感按钮", "按住、移出与松开；命中区域保持不动",70,140,Pink);
            MakePress();
            PanelAt("Jelly card",805,119,750,325);
            Header("02", "果冻拖拽", "拖出圆圈再松开；弹簧会保留速度并回弹",835,140,Cyan);
            MakeJelly();
            PanelAt("Continuation card",40,468,740,330);
            Header("03", "从当前姿态接续", "连续点击左 / 右；观察位置、缩放与角度",70,489,Yellow);
            MakeContinuation();
            PanelAt("Reduced motion card",805,468,750,330);
            Header("04", "减弱动态效果", "必要过渡直接抵达终态，不改变游戏暂停状态",835,489,Mint);
            MakeGate();
            GalleryTheme.Apply(gameObject);
            status=Label(transform,"拖动请在右上实验卡中进行；返回后继续查看 46 项动效。",48,832,1400,35,15,White,TextAnchor.MiddleLeft);
        }
        void Header(string num,string title,string description,float x,float y,Color accent)
        {
            Label(transform,num,x,y,55,34,18,accent,TextAnchor.MiddleLeft);
            Label(transform,title,x+58,y-1,510,37,24,White,TextAnchor.MiddleLeft);
            Label(transform,description,x+58,y+42,610,32,15,new Color(.72f,.79f,.91f,1),TextAnchor.MiddleLeft);
        }
        void MakePress()
        {
            var hit=Image(transform,"ButtonHitArea",220,258,310,100,new Color(.28f,.20f,.47f,.85f));
            hit.raycastTarget=true;
            var button=hit.gameObject.AddComponent<UnityEngine.UI.Button>();button.transition=Selectable.Transition.None;button.targetGraphic=hit;
            var visual=Image(hit.transform,"MotionVisual",8,8,294,84,Pink).rectTransform;
            Label(visual,"按住我",0,0,294,84,23,White,TextAnchor.MiddleCenter);
            var group=visual.gameObject.AddComponent<CanvasGroup>();
            var player=hit.gameObject.AddComponent<MotionPlayer>();player.bindings=new MotionBindings{target=visual,group=group};player.effectId="B02";player.settings=MotionSettings.FromDefinition(MotionCatalog.Find("B02"));player.CaptureBaseline();
            ButtonInput=hit.gameObject.AddComponent<MotionButtonInput>();ButtonInput.visualPlayer=player;ButtonInput.RefreshBindings();
            button.onClick.AddListener(()=>SetStatus("按钮点击已触发；拖动取消后不会触发点击。"));
            Label(transform,"固定命中层 / 内部动态层",192,372,370,27,14,new Color(.68f,.75f,.87f,1),TextAnchor.MiddleCenter);
        }
        void MakeJelly()
        {
            var hit=Image(transform,"DragHitArea",1019,237,320,140,new Color(.08f,.29f,.38f,.8f));hit.raycastTarget=true;
            var visual=Image(hit.transform,"MotionVisual",65,18,190,104,Cyan).rectTransform;
            Label(visual,"拖动  ↗",0,0,190,104,23,Ink,TextAnchor.MiddleCenter);
            Jelly=hit.gameObject.AddComponent<MotionJellyDrag>();Jelly.visual=visual;Jelly.CaptureBaseline();
            DragInput=hit.gameObject.AddComponent<MotionDragInput>();DragInput.hitArea=hit.rectTransform;DragInput.jelly=Jelly;DragInput.RefreshBindings();
            DragInput.onDragStarted.AddListener(()=>SetStatus("正在拖动：释放后由阻尼弹簧自然回弹。"));
            DragInput.onReleased.AddListener(()=>SetStatus("已释放；弹簧正在返回稳定基线。"));
            Label(transform,"保持按住并移动鼠标或手指",980,389,400,25,14,new Color(.68f,.75f,.87f,1),TextAnchor.MiddleCenter);
        }
        void MakeContinuation()
        {
            var target=Image(transform,"Continuation visual",328,612,164,72,Orange).rectTransform;
            Label(target,"接续",0,0,164,72,19,Ink,TextAnchor.MiddleCenter);
            var group=target.gameObject.AddComponent<CanvasGroup>();
            ContinuationPlayer=target.gameObject.AddComponent<MotionPlayer>();
            ContinuationPlayer.bindings=new MotionBindings{target=target,group=group};ContinuationPlayer.CaptureBaseline();
            var baseline=MotionPose.Read(target,group);
            poseA=baseline;poseA.position+=new Vector2(-130,0);poseA.scale=baseline.scale*.82f;poseA.rotation=Quaternion.Euler(0,0,8);
            poseB=baseline;poseB.position+=new Vector2(130,0);poseB.scale=baseline.scale*1.12f;poseB.rotation=Quaternion.Euler(0,0,-8);
            Button(transform,"← 左",100,724,220,48,Purple,White,()=>Continue(false));
            Button(transform,"右 →",500,724,220,48,Purple,White,()=>Continue(true));
        }
        public void Continue(bool right)
        {
            ContinuationPlayer.ContinueTo(right?poseB:poseA,.65f,Ease.OutCubic);
            SetStatus("从当前画面接续；固定时长 Tween 不承诺速度连续。");
        }
        void MakeGate()
        {
            var target=Image(transform,"Reduced transition visual",1109,603,165,80,Yellow).rectTransform;
            Label(target,"完成",0,0,165,80,22,Ink,TextAnchor.MiddleCenter);
            var group=target.gameObject.AddComponent<CanvasGroup>();
            var player=target.gameObject.AddComponent<MotionPlayer>();player.bindings=new MotionBindings{target=target,group=group};player.CaptureBaseline();
            Gate=target.gameObject.AddComponent<MotionPlaybackGate>();Gate.player=player;
            var mode=Button(transform,"",870,719,290,48,Purple,White,ToggleReduced);
            gateLabel=Label(mode.transform,"减弱动效：关",0,0,290,48,17,White,TextAnchor.MiddleCenter);
            Button(transform,"播放必要过渡",1185,719,290,48,Mint,Ink,PlayEssential);
        }
        public void ToggleReduced()
        {
            ReducedMotion=!ReducedMotion;Gate.SetContext(true,true,ReducedMotion);
            gateLabel.text=ReducedMotion?"减弱动效：开":"减弱动效：关";
            SetStatus(ReducedMotion?"减弱模式：必要过渡直接应用终态。":"正常模式：播放完整过渡。");
        }
        public void PlayEssential()
        {
            Gate.Play("A04",MotionPurpose.EssentialTransition);
            SetStatus(ReducedMotion?"已直接应用必要状态的最终画面。":"正在播放弹出过渡。");
        }
        public void Close()
        {
            var callback=onClose;onClose=null;
            gameObject.SetActive(false);Destroy(gameObject);callback?.Invoke();
        }
        void SetStatus(string s) { if(status!=null)status.text=s; }
        void PanelAt(string name,float x,float y,float w,float h) { Image(transform,name,x,y,w,h,Panel); }
        Image Image(Transform parent,string name,float x,float y,float w,float h,Color color,bool soft=true)
        {
            var rect=RectAt(parent,name,x,y,w,h);var image=rect.gameObject.AddComponent<UnityEngine.UI.Image>();
            image.color=color;image.raycastTarget=false;if(soft&&rounded!=null){image.sprite=rounded;image.type=UnityEngine.UI.Image.Type.Sliced;}return image;
        }
        Text Label(Transform parent,string text,float x,float y,float w,float h,int size,Color color,TextAnchor anchor)
        {
            var rect=RectAt(parent,"Label",x,y,w,h);var t=rect.gameObject.AddComponent<Text>();t.font=font;t.text=text;t.fontSize=size;t.color=color;t.alignment=anchor;t.raycastTarget=false;t.verticalOverflow=VerticalWrapMode.Overflow;return t;
        }
        Button Button(Transform parent,string text,float x,float y,float w,float h,Color color,Color foreground,Action action)
        {
            var image=Image(parent,"Button "+text,x,y,w,h,color);image.raycastTarget=true;
            var b=image.gameObject.AddComponent<UnityEngine.UI.Button>();b.targetGraphic=image;var nav=new Navigation{mode=Navigation.Mode.None};b.navigation=nav;
            if(text.Length>0)Label(image.transform,text,0,0,w,h,17,foreground,TextAnchor.MiddleCenter);b.onClick.AddListener(()=>action());return b;
        }
        static RectTransform RectAt(Transform parent,string name,float x,float y,float w,float h)
        {
            var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);return r;
        }
        static Color Hex(string s) { ColorUtility.TryParseHtmlString("#"+s,out var color);return GalleryTheme.Surface(color); }
        void OnDestroy() { if(bgSprite!=null) Destroy(bgSprite); }
    }
}
