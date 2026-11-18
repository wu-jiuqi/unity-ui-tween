using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TweenGallery.Recipes
{
    public static class RecipeUI
    {
        public static readonly Color Ink=Hex("EFF1F5"), Muted=Hex("A6ABB6"), Surface=Hex("292C33"), Dark=Hex("1C1E23");
        public static readonly Color Blue=Hex("367BFF"), Orange=Hex("FF8739"), Green=Hex("20BF88"), Pink=Hex("EC4C83"), Gold=Hex("FFD356");
        static Font font;
        public static Color Hex(string s){ColorUtility.TryParseHtmlString("#"+s,out var c);return c;}
        public static RectTransform Rect(Transform p,string name,float x,float y,float w,float h) {
            var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(p,false);
            r.anchorMin=r.anchorMax=r.pivot=Vector2.one*.5f;r.anchoredPosition=new Vector2(x,y);r.sizeDelta=new Vector2(w,h);return r;
        }
        public static Image Box(Transform p,string name,float x,float y,float w,float h,Color c,string shape="rounded") {
            var r=Rect(p,name,x,y,w,h);var image=r.gameObject.AddComponent<Image>();
            if(name=="Modal barrier"){image.sprite=null;image.color=c;}
            else if(p.GetComponentInParent<RecipeStage>()!=null&&RecipeArt.Available){string key=RecipeArt.Surface(name,shape,c);RecipeArt.Apply(image,key,!RecipeArt.IsIcon(key));}
            else{image.sprite=GalleryShapes.Get(shape);image.type=shape=="rounded"?Image.Type.Sliced:Image.Type.Simple;image.color=c;}
            image.raycastTarget=false;return image;
        }
        public static Text Label(Transform p,string value,float x,float y,float w,float h,int size=18,Color? color=null,TextAnchor align=TextAnchor.MiddleCenter) {
            if(font==null)font=Resources.Load<Font>("GalleryFont");
            var r=Rect(p,"Text",x,y,w,h);var t=r.gameObject.AddComponent<Text>();t.font=font;t.text=value;t.fontSize=size;t.color=color??Ink;t.alignment=align;t.raycastTarget=false;t.supportRichText=false;
            t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Truncate;return t;
        }
        public static Button Button(Transform p,string name,float x,float y,float w,Action click,Color? color=null,float h=40) {
            var hit=Box(p,name,x,y,w,h,color??Surface);hit.raycastTarget=true;var b=hit.gameObject.AddComponent<Button>();b.targetGraphic=hit;
            if(p.GetComponentInParent<RecipeStage>()!=null&&RecipeArt.Available){if(name.Length==0)hit.color=new Color(1,1,1,0);else RecipeArt.Apply(hit,color==Orange?"button-orange":color==Green?"button-green":color==Pink?"button-purple":"button-blue");}
            if(p.name=="Operations"){hit.sprite=GalleryShapes.Get("rounded");hit.type=Image.Type.Sliced;hit.preserveAspect=false;hit.color=Surface;}
            var colors=b.colors;colors.highlightedColor=new Color(.87f,.90f,1);colors.pressedColor=new Color(.65f,.70f,.8f);b.colors=colors;
            Label(hit.transform,name,0,0,w-12,h-4,16);if(click!=null)b.onClick.AddListener(()=>click());return b;
        }
        public static CanvasGroup Group(RectTransform r){var g=r.GetComponent<CanvasGroup>();return g!=null?g:r.gameObject.AddComponent<CanvasGroup>();}
        public static void Alpha(RectTransform r,float a){var g=Group(r);g.alpha=Mathf.Clamp01(a);g.interactable=g.blocksRaycasts=a>=.999f;}
        public static Slider Slider(Transform p,float x,float y,float w,float min,float max,float value,Action<float> changed) {
            var track=Box(p,"Track",x,y,w,8,Hex("454A55"));var slider=track.gameObject.AddComponent<Slider>();track.raycastTarget=true;
            var fillArea=Rect(track.transform,"Fill area",0,0,w,8);
            var fill=Box(fillArea,"Fill",0,0,0,0,Blue);fill.rectTransform.anchorMin=Vector2.zero;fill.rectTransform.anchorMax=Vector2.one;fill.rectTransform.sizeDelta=Vector2.zero;
            var handleArea=Rect(track.transform,"Handle area",0,0,w,8);
            var handle=Box(handleArea,"Handle",0,0,18,18,Ink,"circle");handle.raycastTarget=true;
            slider.fillRect=fill.rectTransform;slider.handleRect=handle.rectTransform;slider.targetGraphic=handle;slider.minValue=min;slider.maxValue=max;slider.SetValueWithoutNotify(value);
            slider.onValueChanged.AddListener(v=>changed?.Invoke(v));return slider;
        }
        public static InputField Input(Transform p,string placeholder,float x,float y,float w,float h=42) {
            var box=Box(p,"Input",x,y,w,h,Dark);box.raycastTarget=true;var f=box.gameObject.AddComponent<InputField>();
            f.textComponent=Label(box.transform,"",0,0,w-20,h-8,17,Ink,TextAnchor.MiddleLeft);
            f.placeholder=Label(box.transform,placeholder,0,0,w-20,h-8,16,Muted,TextAnchor.MiddleLeft);f.targetGraphic=box;return f;
        }
        public static RectTransform Card(Transform p,string title,float x,float y,float w=145,float h=190,bool art=true) {
            if(art&&RecipeArt.ApprovedAvailable)return RecipeStage.ApprovedCard(p,title,x,y,w,h);
            var c=Box(p,"Card "+title,x,y,w,h,Blue).rectTransform;
            if(!RecipeArt.Available)Box(c,"Inset",0,8,w-12,h-16,Hex("172B52"));
            if(art){var i=Box(c,"Illustration",0,15,w-30,h*.64f,Color.white);if(RecipeArt.Available)RecipeArt.Apply(i,"card-art",false);else i.sprite=GeneratedGalleryArt.Get("adventure-world");i.preserveAspect=true;}
            if(RecipeArt.Available){c.GetComponent<Image>().type=Image.Type.Simple;var rarity=Box(c,"Rarity",0,h*.38f,28,22,Color.white);RecipeArt.Apply(rarity,"card-rarity",false);var gloss=Box(c,"Card gloss",0,0,w,h,Color.white);RecipeArt.Apply(gloss,"card-gloss",false);gloss.color=new Color(1,1,1,.10f);}
            Label(c,title,0,-h*.35f,w-26,32,Mathf.RoundToInt(Mathf.Clamp(w*.115f,13,19)));return c;
        }
    }
    // Fixed hit surface. The pointer delta remains in the parent's canvas space.
    public sealed class RecipePointer : MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerEnterHandler,IPointerExitHandler,IBeginDragHandler,IDragHandler,IEndDragHandler,ICancelHandler
    {
        public Action<PointerEventData> down,up,enter,exit,drag,end;
        public Action cancel;public int Pointer {get;private set;}=int.MinValue;
        public bool Held=>Pointer!=int.MinValue;
        public void OnPointerDown(PointerEventData e){if(Held||e.button!=PointerEventData.InputButton.Left)return;Pointer=e.pointerId;down?.Invoke(e);}
        public void OnPointerUp(PointerEventData e){if(Pointer!=e.pointerId)return;up?.Invoke(e);Pointer=int.MinValue;}
        public void OnPointerEnter(PointerEventData e){enter?.Invoke(e);}
        public void OnPointerExit(PointerEventData e){exit?.Invoke(e);}
        public void OnBeginDrag(PointerEventData e){if(Pointer==e.pointerId)e.eligibleForClick=false;}
        public void OnDrag(PointerEventData e){if(Pointer==e.pointerId){e.eligibleForClick=false;drag?.Invoke(e);}}
        public void OnEndDrag(PointerEventData e){if(Pointer==e.pointerId){end?.Invoke(e);Pointer=int.MinValue;}}
        public void OnCancel(BaseEventData e){Cancel();}
        public void Cancel(){if(!Held)return;Pointer=int.MinValue;cancel?.Invoke();}
        void OnDisable(){Cancel();}
        void OnApplicationFocus(bool focused){if(!focused)Cancel();}
        void OnApplicationPause(bool paused){if(paused)Cancel();}
    }
}
