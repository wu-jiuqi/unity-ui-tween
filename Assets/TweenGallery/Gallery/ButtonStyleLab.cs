using System;
using UnityEngine;
using UnityEngine.UI;

namespace TweenGallery
{
    /// <summary>Eight new button recipes share six original blank skins; text/icons remain native UGUI.</summary>
    public sealed class ButtonStyleLab : MonoBehaviour
    {
        public MotionButtonFeedback[] Buttons { get; private set; }
        public bool HasGeneratedSkins { get; private set; }
        public int ClickCount { get; private set; }
        public Text Status { get; private set; }
        Font font; Sprite rounded, loadingArc; Texture2D atlas; readonly Sprite[] skins = new Sprite[6];
        static Color Ink => Hex("77574C");
        public static ButtonStyleLab Create(Transform parent, Action close)
        {
            var go = new GameObject("Cream Garden Button Lab",typeof(RectTransform));
            var rect = go.GetComponent<RectTransform>(); rect.SetParent(parent,false);
            rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(.5f,.5f);rect.sizeDelta=new Vector2(1600,900);
            var lab=go.AddComponent<ButtonStyleLab>();lab.Build(close);return lab;
        }
        void Build(Action close)
        {
            font=Resources.Load<Font>("GalleryFont");
            rounded=Rounded(); loadingArc=Arc(); atlas=Resources.Load<Texture2D>("ButtonArt/cream-buttons");
            HasGeneratedSkins=atlas!=null && atlas.width==1536 && atlas.height==1024;
            if (atlas!=null)
            {
                Rect[] areas={new Rect(20,620,472,260),new Rect(530,620,480,260),new Rect(1080,590,380,360),
                    new Rect(20,90,472,290),new Rect(530,100,480,270),new Rect(1090,90,360,340)};
                for(int i=0;i<skins.Length;i++)skins[i]=Sprite.Create(atlas,areas[i],new Vector2(.5f,.5f),100);
            }
            var bg=ImageAt(transform,"Ivory background",0,0,1600,900,Hex("F9F0DF"));bg.raycastTarget=true;
            Label(transform,"奶油花园 · 按钮工坊",58,-40,950,62,34);
            Label(transform,"六种新造型  /  八种交互配方  /  原生文字与图标",60,-100,1000,35,19);
            Control("返回展馆",1300,-55,210,close);
            Control("减弱动效",1060,-55,210,()=> { foreach(var b in Buttons)b.reducedMotion=!b.reducedMotion;Status.text=Buttons[0].reducedMotion?"减弱动效已开启：关闭装饰运动，保留选择状态":"标准动效：按住、松开、移出与连续点击均可体验"; });
            Buttons=new MotionButtonFeedback[8];
            string[] names={"BX01 柔软按压","BX02 果冻挤压","BX03 悬浮抬升","BX04 图标回弹","BX05 点击涟漪","BX06 柔光扫过","BX07 选中反馈","BX08 加载反馈"};
            string[] tips={"薄荷胶囊 · 按住后松开","珊瑚软糖 · 横向舒展","奶油描边 · 鼠标悬停","金色圆钮 · 图标轻跃","方形入口 · 中心扩散","淡紫织边 · 点击扫光","分段页签 · 再点取消","点击加载 · 右下结束"};
            string[] labels={"开始探险","领取好礼","了解详情","★","＋","立即解锁","花园","继续"};
            int[] skin={0,3,4,2,5,1,1,0};
            for(int i=0;i<8;i++)
            {
                int at=i;float x=58+(i%4)*379,y=-167-(i/4)*305;
                ImageAt(transform,"Recipe card "+i,x,y,350,280,Hex("FFFAF1"));
                Label(transform,names[i],x+18,y-14,314,40,21);
                var hit=RectAt(transform,"Fixed hit "+names[i],x+25,y-66,300,135);
                var hitImage=hit.gameObject.AddComponent<Image>();hitImage.color=Color.clear;
                var button=hit.gameObject.AddComponent<Button>();button.transition=Selectable.Transition.None;button.targetGraphic=hitImage;
                var surface=RectAt(hit,"Animated surface",150,-67.5f,300,140);surface.pivot=new Vector2(.5f,.5f);
                var image=surface.gameObject.AddComponent<Image>();image.sprite=skins[skin[i]]!=null?skins[skin[i]]:rounded;image.preserveAspect=true;image.raycastTarget=false;
                var decoration=RectAt(surface,"Shape-clipped decorations",0,0,300,140);decoration.anchorMin=decoration.anchorMax=decoration.pivot=new Vector2(.5f,.5f);
                var maskGraphic=decoration.gameObject.AddComponent<Image>();maskGraphic.sprite=image.sprite;maskGraphic.preserveAspect=true;maskGraphic.raycastTarget=false;
                decoration.gameObject.AddComponent<Mask>().showMaskGraphic=false;
                var feedback=hit.gameObject.AddComponent<MotionButtonFeedback>();feedback.style=(ButtonMotionStyle)i;feedback.surface=surface;
                var mark=RectAt(surface,"Native text and icon",0,0,260,92);mark.anchorMin=mark.anchorMax=new Vector2(.5f,.5f);mark.pivot=new Vector2(.5f,.5f);
                var text=mark.gameObject.AddComponent<Text>();text.font=font;text.text=labels[i];text.fontSize=i==3||i==4?42:25;text.alignment=TextAnchor.MiddleCenter;text.color=Ink;text.raycastTarget=false;feedback.icon=mark;
                if(i==7)
                {
                    var spinner=RectAt(surface,"Independent spinner",-83,0,40,40);spinner.anchorMin=spinner.anchorMax=spinner.pivot=new Vector2(.5f,.5f);
                    var glyph=spinner.gameObject.AddComponent<Image>();glyph.sprite=loadingArc;glyph.color=Ink;glyph.raycastTarget=false;feedback.icon=spinner;
                }
                var ring=ImageAt(hit,"Selected outline",150,-67.5f,310,128,Hex("EDD08F"));ring.rectTransform.pivot=new Vector2(.5f,.5f);ring.sprite=rounded;ring.type=Image.Type.Sliced;ring.transform.SetAsFirstSibling();
                feedback.selection=ring.rectTransform;feedback.selectionGroup=ring.gameObject.AddComponent<CanvasGroup>();feedback.selectionGroup.alpha=0;feedback.selectionGroup.blocksRaycasts=false;
                var ripple=ImageAt(decoration,"Ripple",0,0,130,130,new Color(1,1,1,.7f));ripple.rectTransform.anchorMin=ripple.rectTransform.anchorMax=ripple.rectTransform.pivot=new Vector2(.5f,.5f);
                feedback.ripple=ripple.rectTransform;feedback.rippleGroup=ripple.gameObject.AddComponent<CanvasGroup>();feedback.rippleGroup.alpha=0;
                var sheen=ImageAt(decoration,"Sheen",0,0,35,145,new Color(1,1,1,.7f));sheen.rectTransform.anchorMin=sheen.rectTransform.anchorMax=sheen.rectTransform.pivot=new Vector2(.5f,.5f);sheen.rectTransform.localRotation=Quaternion.Euler(0,0,-22);
                feedback.sheen=sheen.rectTransform;feedback.sheenGroup=sheen.gameObject.AddComponent<CanvasGroup>();feedback.sheenGroup.alpha=0;
                mark.SetAsLastSibling();feedback.CaptureBaseline();Buttons[i]=feedback;
                button.onClick.AddListener(()=> { ClickCount++; if(at==6)feedback.SetSelected(!feedback.IsSelected);if(at==7)feedback.SetBusy(!feedback.IsBusy);Status.text=names[at]+" · 点击次数 "+ClickCount+" · "+(feedback.IsBusy?"正在加载，点右侧结束":"业务事件由原 Button 发出"); });
                // Busy visuals intentionally leave business interactivity to the host; demo button toggles busy off.
                Label(transform,tips[i],x+18,y-221,314,42,16);
            }
            Status=Label(transform,"点击体验反馈；加载状态通过右侧按钮结束。所有命中区域保持固定。",60,-801,1120,48,18);
            Control("结束加载",1250,-801,260,()=>Buttons[7].SetBusy(false));
        }
        void Control(string title,float x,float y,float width,Action action)
        {
            var image=ImageAt(transform,title,x,y,width,43,Hex("E6D7EF"));image.raycastTarget=true;
            var b=image.gameObject.AddComponent<Button>();b.onClick.AddListener(()=>action());Label(image.transform,title,0,0,width,43,18).alignment=TextAnchor.MiddleCenter;
        }
        RectTransform RectAt(Transform parent,string name,float x,float y,float w,float h)
        {
            var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);
            r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,y);r.sizeDelta=new Vector2(w,h);return r;
        }
        Image ImageAt(Transform parent,string name,float x,float y,float w,float h,Color color)
        {
            var r=RectAt(parent,name,x,y,w,h);var im=r.gameObject.AddComponent<Image>();im.sprite=rounded;im.type=Image.Type.Sliced;im.color=color;im.raycastTarget=false;return im;
        }
        Text Label(Transform parent,string value,float x,float y,float w,float h,int size)
        {
            var r=RectAt(parent,value,x,y,w,h);var t=r.gameObject.AddComponent<Text>();t.text=value;t.font=font;t.fontSize=size;t.color=Ink;t.alignment=TextAnchor.MiddleLeft;t.raycastTarget=false;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Overflow;return t;
        }
        static Color Hex(string h) { ColorUtility.TryParseHtmlString("#"+h,out var c);return c; }
        static Sprite Rounded()
        {
            var t=new Texture2D(64,64,TextureFormat.RGBA32,false);var px=new Color[4096];
            for(int y=0;y<64;y++)for(int x=0;x<64;x++){float dx=Mathf.Max(Mathf.Abs(x-31.5f)-19,0),dy=Mathf.Max(Mathf.Abs(y-31.5f)-19,0);px[y*64+x]=new Color(1,1,1,Mathf.Clamp01(13-Mathf.Sqrt(dx*dx+dy*dy)));}
            t.SetPixels(px);t.Apply();return Sprite.Create(t,new Rect(0,0,64,64),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,new Vector4(16,16,16,16));
        }
        static Sprite Arc()
        {
            var t=new Texture2D(64,64,TextureFormat.RGBA32,false);var px=new Color[4096];
            for(int y=0;y<64;y++)for(int x=0;x<64;x++)
            {
                float dx=x-31.5f,dy=y-31.5f,r=Mathf.Sqrt(dx*dx+dy*dy),a=Mathf.Atan2(dy,dx);
                px[y*64+x]=new Color(1,1,1,a<2.25f?Mathf.Clamp01(3.5f-Mathf.Abs(r-24)):0);
            }
            t.SetPixels(px);t.Apply();return Sprite.Create(t,new Rect(0,0,64,64),new Vector2(.5f,.5f));
        }
        void OnDestroy() { foreach(var s in skins)if(s!=null)Destroy(s);if(rounded!=null){Destroy(rounded.texture);Destroy(rounded);}if(loadingArc!=null){Destroy(loadingArc.texture);Destroy(loadingArc);} }
    }
}
