using UnityEngine;
using UnityEngine.UI;
using static TweenGallery.GalleryPageUi;
namespace TweenGallery
{
    public sealed class GalleryPagePreview : MonoBehaviour
    {
        public MotionPageSequence Sequence { get; private set; }
        public string Status { get; private set; }="点击阶段定位，或使用下方时间轴观察整页编排";
        public int PreviewActionCount { get; private set; }
        Text wallet;Button[] stages;bool live;int lastWallet=-1;
        public static GalleryPagePreview Create(Transform parent,MotionSettings settings,MotionStyleProfile style,bool reduced,bool live)
        {
            var host=parent as RectTransform;var root=Rect(parent,"Achievement page recipe",new Vector2(host.rect.width*.5f,-host.rect.height*.5f),host.rect.size,false);
            var view=root.gameObject.AddComponent<GalleryPagePreview>();view.live=live;view.Build(settings,reduced);
            GalleryPageUi.ApplyStyle(root,style,reduced);return view;
        }
        void Build(MotionSettings settings,bool reduced)
        {
            var stage=transform as RectTransform;
            var environment=Art(stage,"Garden environment",GeneratedGalleryArt.Get("adventure-world"),Vector2.zero,stage.rect.size);environment.preserveAspect=false;
            var phone=Rect(stage,"Page viewport",new Vector2(0,-18),new Vector2(390,650));phone.localScale=Vector3.one*.67f;phone.gameObject.AddComponent<RectMask2D>();
            var background=Art(phone,"Garden crop",GeneratedGalleryArt.Get("adventure-world"),Vector2.zero,new Vector2(975,650));background.preserveAspect=false;
            var dim=Rect(phone,"Backdrop",Vector2.zero,new Vector2(390,650));dim.gameObject.AddComponent<Image>().color=new Color(.25f,.2f,.14f,.42f);
            var panel=Art(phone,"Achievement panel",GalleryPageArt.Panel,new Vector2(0,-15),new Vector2(350,520),true);panel.pixelsPerUnitMultiplier=2;
            Text(panel.transform,"Title","旅程达成",new Vector2(0,190),new Vector2(320,55),36);
            var emblem=Art(panel.transform,"Achievement emblem",GalleryPageArt.Medal,new Vector2(0,50),new Vector2(220,220));
            var rewardRoot=Rect(panel.transform,"Reward group",new Vector2(0,-103),new Vector2(320,86));
            var a=Reward(rewardRoot,3,"金币\n+120",new Vector2(-80,0));var b=Reward(rewardRoot,0,"星星\n+3",new Vector2(80,0));
            var actions=Rect(panel.transform,"Actions",new Vector2(0,-188),new Vector2(310,104));
            Button(actions,"Continue","继续探索",new Vector2(0,20),new Vector2(305,52),0,()=>{PreviewActionCount++;Status="预览操作已触发，不写入任何进度或奖励";});
            Button(actions,"Collection","查看收藏",new Vector2(0,-32),new Vector2(265,38),1,()=>{PreviewActionCount++;Status="收藏入口演示，不执行页面导航";});
            var counter=Art(phone,"Wallet pill",GalleryPageArt.Button(4),new Vector2(96,286),new Vector2(166,48),true);
            Art(counter.transform,"Wallet coin",GalleryPageArt.Icon(3),new Vector2(-52,0),new Vector2(38,38));wallet=Text(counter.transform,"Wallet label","1,000",new Vector2(16,0),new Vector2(108,42),24);
            Text(phone,"Preview disclaimer","示例数值 · 不发放奖励",new Vector2(0,-306),new Vector2(350,26),15);
            Sequence=gameObject.AddComponent<MotionPageSequence>();Sequence.panel=panel.rectTransform;Sequence.panelGroup=Group(panel.rectTransform);
            Sequence.emblem=emblem.rectTransform;Sequence.emblemGroup=Group(emblem.rectTransform);Sequence.backdrop=Group(dim);Sequence.rewardsGroup=Group(rewardRoot);Sequence.actionsGroup=Group(actions);
            Sequence.rewardA=a;Sequence.rewardB=b;Sequence.walletTarget=counter.rectTransform;Sequence.duration=settings.duration;Sequence.overshoot=settings.overshoot;Sequence.strength=settings.strength;Sequence.travel=settings.distance;Sequence.stagger=settings.stagger;Sequence.speed=settings.speed;Sequence.reducedMotion=reduced;Sequence.unscaledTime=settings.unscaledTime;
            Sequence.coins=new RectTransform[4];for(int i=0;i<4;i++)Sequence.coins[i]=Art(phone,"Flying coin "+i,GalleryPageArt.Icon(3),new Vector2(-92+i*12,-80),new Vector2(36,36)).rectTransform;
            Sequence.CaptureBaseline();Sequence.Seek(0);Sequence.onVisualComplete.AddListener(()=>Status="整页编排完成 · 此事件只表示视觉结束");
            stages=new Button[4];string[] labels={"背景","面板","主视觉","奖励与操作"};float[] marks={0,.25f/1.1f,.65f/1.1f,1};
            for(int i=0;i<4;i++){int at=i;stages[i]=Button(stage,"Stage "+i,labels[i],new Vector2(-300+i*200,stage.rect.height*.5f-27),new Vector2(185,36),4,()=>{Sequence.Seek(marks[at]);Status="阶段定位："+labels[at]+" · "+(Sequence.duration*marks[at]).ToString("0.00")+" s";});}
            if(live)Button(stage,"Play full page","点击播放整页",new Vector2(285,-stage.rect.height*.5f+35),new Vector2(210,42),0,()=>Sequence.Play());
        }
        RectTransform Reward(Transform parent,int icon,string text,Vector2 position)
        {
            var card=Art(parent,"Reward card",GalleryPageArt.Panel,position,new Vector2(148,84),true);card.pixelsPerUnitMultiplier=4;
            Art(card.transform,"Reward icon",GalleryPageArt.Icon(icon),new Vector2(-40,0),new Vector2(58,58));Text(card.transform,"Reward amount",text,new Vector2(24,0),new Vector2(90,70),23);return card.rectTransform;
        }
        void LateUpdate(){if(Sequence!=null&&wallet!=null){int value=1000+Mathf.RoundToInt(120*Sequence.WalletProgress);if(value!=lastWallet){wallet.text=value.ToString("N0");lastWallet=value;}}}
        void OnDisable(){if(Sequence!=null)Sequence.Restore();}
    }
}
