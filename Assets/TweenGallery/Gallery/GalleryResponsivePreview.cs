using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static TweenGallery.GalleryPageUi;
namespace TweenGallery
{
    public sealed class GalleryResponsivePreview : MonoBehaviour
    {
        sealed class Card
        {
            public RectTransform hit;public Image face,icon;public Text title,radio;public MotionButtonFeedback feedback;
        }
        sealed class Device
        {
            public RectTransform root;public MotionResponsiveLayout layout;public Card[] cards;public Button confirm,back;
            public Text top,bottom;public Scrollbar scrollbar;
        }
        readonly List<Device> devices=new List<Device>();
        public int SelectedReward { get; private set; }
        public int Mode { get; private set; }
        public int ConfirmCount { get; private set; }
        public string Status { get; private set; }="三种布局共享选择状态；短屏正文可滚动，底部操作始终可见";
        public MotionResponsiveLayout[] Layouts=>devices.ConvertAll(d=>d.layout).ToArray();
        Button modeButton;Text selectedLabel;
        public static GalleryResponsivePreview Create(Transform parent,MotionStyleProfile style,bool reduced)
        {
            var area=parent as RectTransform;var root=Rect(parent,"Responsive reward recipe",new Vector2(area.rect.width*.5f,-area.rect.height*.5f),area.rect.size,false);
            var view=root.gameObject.AddComponent<GalleryResponsivePreview>();view.Build(style,reduced);return view;
        }
        void Build(MotionStyleProfile style,bool reduced)
        {
            var root=transform as RectTransform;
            modeButton=Button(root,"Layout mode","布局：并排",new Vector2(-220,root.rect.height*.5f-27),new Vector2(240,38),4,()=>SetMode((Mode+1)%4));
            Button(root,"Rotate layout","旋转当前视图",new Vector2(55,root.rect.height*.5f-27),new Vector2(240,38),4,RotateCurrent);
            selectedLabel=Text(root,"Shared selection","当前：星星奖励",new Vector2(280,root.rect.height*.5f-27),new Vector2(240,38),17);
            BuildDevice("短屏",new Vector2(390,520),style,reduced);BuildDevice("长屏",new Vector2(390,780),style,reduced);BuildDevice("横屏",new Vector2(844,390),style,reduced);
            SetMode(0);SelectReward(0);
            GalleryPageUi.ApplyStyle(root,style,reduced);
            foreach(var d in devices)foreach(var card in d.cards)card.feedback.intensity=.6f*style.strengthScale;
        }
        void BuildDevice(string name,Vector2 size,MotionStyleProfile style,bool reduced)
        {
            var d=new Device();d.root=Rect(transform,name,Vector2.zero,size);var frame=d.root.gameObject.AddComponent<Image>();frame.sprite=GalleryPageArt.Panel;frame.type=Image.Type.Sliced;frame.pixelsPerUnitMultiplier=2;frame.raycastTarget=false;
            d.root.gameObject.AddComponent<RectMask2D>();
            d.top=Text(d.root,"Top reserved","系统区域 · 顶部预留",new Vector2(0,size.y*.5f-16),new Vector2(size.x-20,24),14);
            d.bottom=Text(d.root,"Bottom reserved","系统区域 · 底部预留",new Vector2(0,-size.y*.5f+14),new Vector2(size.x-20,22),14);
            var safe=Rect(d.root,"Safe content",Vector2.zero,size);var heading=Text(safe,"Page heading","选择奖励",Vector2.zero,new Vector2(300,56),34).rectTransform;
            var viewport=Rect(safe,"Reward viewport",Vector2.zero,new Vector2(300,300));viewport.gameObject.AddComponent<Image>().color=Color.clear;viewport.gameObject.AddComponent<RectMask2D>();
            var content=Rect(viewport,"Reward content",Vector2.zero,new Vector2(300,340));var scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.viewport=viewport;scroll.content=content;scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=28;
            var track=Art(safe,"Scroll track",GalleryPageArt.Panel,Vector2.zero,new Vector2(7,200),true);track.pixelsPerUnitMultiplier=25;track.color=new Color(.82f,.76f,.63f,.7f);
            var thumb=Art(track.transform,"Scroll thumb",GalleryPageArt.Panel,Vector2.zero,new Vector2(7,70),true);thumb.pixelsPerUnitMultiplier=25;thumb.color=GalleryTheme.Lavender;thumb.raycastTarget=true;
            d.scrollbar=track.gameObject.AddComponent<Scrollbar>();d.scrollbar.handleRect=thumb.rectTransform;d.scrollbar.targetGraphic=thumb;d.scrollbar.direction=Scrollbar.Direction.BottomToTop;
            scroll.verticalScrollbar=d.scrollbar;
            var actions=Rect(safe,"Fixed actions",Vector2.zero,new Vector2(300,108));
            d.confirm=Button(actions,"Confirm choice","确认选择",Vector2.zero,new Vector2(300,48),0,()=>{ConfirmCount++;Status="确认示例选择："+RewardName(SelectedReward)+"；未发放奖励";});
            d.back=Button(actions,"Return","返回",Vector2.zero,new Vector2(300,48),1,()=>Status="返回按钮演示：保留选择，不执行真实导航");
            d.cards=new Card[3];
            for(int i=0;i<3;i++)
            {
                int index=i;var card=new Card();card.hit=Rect(content,"Reward choice "+i,Vector2.zero,new Vector2(320,108));var hit=card.hit.gameObject.AddComponent<Image>();hit.color=Color.clear;
                var button=card.hit.gameObject.AddComponent<Button>();button.targetGraphic=hit;button.transition=Selectable.Transition.None;button.onClick.AddListener(()=>SelectReward(index));
                card.face=Art(card.hit,"Card surface",GalleryPageArt.Panel,Vector2.zero,new Vector2(320,108),true);card.face.pixelsPerUnitMultiplier=4;
                card.icon=Art(card.face.transform,"Reward illustration",GalleryPageArt.Icon(i),Vector2.zero,new Vector2(80,80));
                card.title=Text(card.face.transform,"Native reward text",RewardName(i)+"\n× "+new[]{5,3,1}[i],Vector2.zero,new Vector2(180,70),25);
                card.radio=Text(card.face.transform,"Selection indicator","○",Vector2.zero,new Vector2(35,35),29);
                card.feedback=card.hit.gameObject.AddComponent<MotionButtonFeedback>();card.feedback.surface=card.face.rectTransform;card.feedback.intensity=style.strengthScale*.6f;card.feedback.frequency=style.springFrequency;card.feedback.damping=style.springDamping;card.feedback.reducedMotion=reduced;card.feedback.CaptureBaseline();d.cards[i]=card;
            }
            d.layout=d.root.gameObject.AddComponent<MotionResponsiveLayout>();d.layout.safeRoot=safe;d.layout.title=heading;d.layout.viewport=viewport;d.layout.content=content;d.layout.actions=actions;d.layout.scroll=scroll;d.layout.cards=System.Array.ConvertAll(d.cards,c=>c.hit);
            d.layout.onBeforeLayout.AddListener(()=>{foreach(var c in d.cards){c.feedback.CancelInteraction();c.feedback.Restore();}});
            d.layout.onLayoutChanged.AddListener(()=>ArrangeContents(d));devices.Add(d);d.layout.Refresh();
        }
        void ArrangeContents(Device d)
        {
            bool wide=d.layout.IsLandscape;
            var size=d.root.rect.size;d.top.rectTransform.anchoredPosition=new Vector2(0,size.y*.5f-16);d.top.rectTransform.sizeDelta=new Vector2(size.x-20,24);
            d.bottom.rectTransform.anchoredPosition=new Vector2(0,-size.y*.5f+14);d.bottom.rectTransform.sizeDelta=new Vector2(size.x-20,22);
            var bar=d.scrollbar.transform as RectTransform;bar.anchorMin=bar.anchorMax=new Vector2(0,1);bar.anchoredPosition=new Vector2(d.layout.safeRoot.rect.width-9,-80-d.layout.viewport.rect.height*.5f);bar.sizeDelta=new Vector2(7,d.layout.viewport.rect.height);bar.gameObject.SetActive(d.layout.IsScrollable);
            foreach(var card in d.cards)
            {
                card.face.rectTransform.sizeDelta=card.hit.sizeDelta;
                card.icon.rectTransform.anchoredPosition=wide?new Vector2(0,43):new Vector2(-d.layout.viewport.sizeDelta.x*.5f+53,0);
                card.icon.rectTransform.sizeDelta=wide?new Vector2(65,65):new Vector2(78,78);
                card.title.rectTransform.anchoredPosition=wide?new Vector2(0,-22):new Vector2(15,0);card.title.rectTransform.sizeDelta=new Vector2(wide?card.hit.sizeDelta.x-12:175,68);card.title.fontSize=wide?23:25;
                card.radio.rectTransform.anchoredPosition=wide?new Vector2(0,-72):new Vector2(card.hit.sizeDelta.x*.5f-26,0);
                card.feedback.CaptureBaseline();
            }
            float width=d.layout.actions.sizeDelta.x;
            FitAction(d.confirm,wide?new Vector2(width*.25f, -27):new Vector2(width*.5f,-25),new Vector2(wide?width*.5f-9:width,48));
            FitAction(d.back,wide?new Vector2(width*.75f,-27):new Vector2(width*.5f,-82),new Vector2(wide?width*.5f-9:width,48));
        }
        static void FitAction(Button button,Vector2 position,Vector2 size)
        {
            var rect=button.transform as RectTransform;rect.anchorMin=rect.anchorMax=new Vector2(0,1);rect.pivot=new Vector2(.5f,.5f);rect.anchoredPosition=position;rect.sizeDelta=size;
            var feedback=button.GetComponent<MotionButtonFeedback>();feedback.CancelInteraction();feedback.Restore();feedback.surface.sizeDelta=size;
            var text=feedback.surface.GetComponentInChildren<Text>();text.rectTransform.sizeDelta=size-Vector2.one*8;text.fontSize=27;feedback.CaptureBaseline();
        }
        public void SelectReward(int index)
        {
            if(index<0||index>2)return;SelectedReward=index;
            foreach(var device in devices)for(int i=0;i<3;i++){device.cards[i].radio.text=i==index?"●":"○";device.cards[i].radio.color=i==index?GalleryTheme.Lavender:GalleryTheme.Ink;device.cards[i].face.color=i==index?new Color(1,.94f,.76f):Color.white;}
            if(selectedLabel!=null)selectedLabel.text="当前："+RewardName(index);Status="已选择 "+RewardName(index)+" · 三种布局同步保留";
        }
        public void SetMode(int mode)
        {
            Mode=Mathf.Clamp(mode,0,3);var stage=transform as RectTransform;
            for(int i=0;i<devices.Count;i++)
            {
                var d=devices[i];d.root.gameObject.SetActive(Mode==0||Mode==i+1);if(!d.root.gameObject.activeSelf)continue;
                if(Mode==0){d.root.localScale=Vector3.one*.44f;d.root.anchoredPosition=new Vector2(new[]{-305f,-95f,206f}[i],-8);}
                else{float fit=Mathf.Min((stage.rect.width-75)/d.root.rect.width,(stage.rect.height-70)/d.root.rect.height);d.root.localScale=Vector3.one*fit;d.root.anchoredPosition=new Vector2(0,-19);}
            }
            modeButton.GetComponentInChildren<Text>().text="布局："+new[]{"并排","短屏","长屏","横屏"}[Mode];
        }
        public void ResizePrimary(Vector2 size){devices[0].root.sizeDelta=size;devices[0].layout.Refresh();SetMode(1);}
        public void RotateCurrent(){int index=Mode==0?0:Mode-1;var r=devices[index].root;r.sizeDelta=new Vector2(r.rect.height,r.rect.width);devices[index].layout.Refresh();SetMode(index+1);Status="布局已重排，选择状态保留";}
        static string RewardName(int i)=>new[]{"星星奖励","幼芽奖励","宝石奖励"}[i];
    }
}
