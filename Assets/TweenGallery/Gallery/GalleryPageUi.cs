using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
namespace TweenGallery
{
    public static class GalleryPageUi
    {
        public static RectTransform Rect(Transform parent,string name,Vector2 position,Vector2 size,bool center=true)
        {
            var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=center?new Vector2(.5f,.5f):new Vector2(0,1);r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=position;r.sizeDelta=size;return r;
        }
        public static Image Art(Transform parent,string name,Sprite sprite,Vector2 position,Vector2 size,bool sliced=false)
        {
            var r=Rect(parent,name,position,size);var image=r.gameObject.AddComponent<Image>();image.sprite=sprite;image.raycastTarget=false;image.preserveAspect=!sliced;image.type=sliced?Image.Type.Sliced:Image.Type.Simple;if(sliced)image.pixelsPerUnitMultiplier=2.5f;return image;
        }
        public static Text Text(Transform parent,string name,string value,Vector2 position,Vector2 size,int points)
        {
            var r=Rect(parent,name,position,size);var text=r.gameObject.AddComponent<Text>();text.text=value;text.fontSize=points;GalleryTypography.Style(text,points>=17);text.alignment=TextAnchor.MiddleCenter;text.color=GalleryTheme.Ink;text.raycastTarget=false;text.horizontalOverflow=HorizontalWrapMode.Wrap;text.verticalOverflow=VerticalWrapMode.Overflow;return text;
        }
        public static Button Button(Transform parent,string name,string label,Vector2 position,Vector2 size,int skin,UnityAction action)
        {
            var r=Rect(parent,name,position,size);var hit=r.gameObject.AddComponent<Image>();hit.color=Color.clear;
            var button=r.gameObject.AddComponent<Button>();button.targetGraphic=hit;button.transition=Selectable.Transition.None;button.onClick.AddListener(action);
            int kind=skin==0?0:skin==1?1:2;
            var visual=Art(r,"Button surface",GalleryCompositionArt.Action(kind),Vector2.zero,size,true);visual.pixelsPerUnitMultiplier=10;
            var caption=Text(visual.transform,"Native label",label,new Vector2(0,1),size-new Vector2(24,8),Mathf.RoundToInt(Mathf.Min(27,size.y*.47f)));
            GalleryTypography.Style(caption,true);caption.color=kind==0?GalleryTheme.GreenInk:kind==1?GalleryTheme.PlumInk:GalleryTheme.Ink;
            var feedback=r.gameObject.AddComponent<MotionButtonFeedback>();feedback.surface=visual.rectTransform;feedback.CaptureBaseline();return button;
        }
        public static CanvasGroup Group(RectTransform rect){return rect.gameObject.AddComponent<CanvasGroup>();}
        public static void ApplyStyle(Transform root,MotionStyleProfile style,bool reduced)
        {
            foreach(var feedback in root.GetComponentsInChildren<MotionButtonFeedback>(true))
            {
                feedback.intensity=style.strengthScale;feedback.frequency=style.springFrequency;
                feedback.damping=style.springDamping;feedback.accentDuration=.55f*style.timeScale;feedback.reducedMotion=reduced;
            }
        }
    }
}
