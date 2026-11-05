using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TweenGallery
{
    /// <summary>Visual fixtures only. Every animation is performed by the reusable MotionPlayer.</summary>
    public static class DemoFactory
    {
        static readonly Color Ink = Hex("171B38"), Purple = Hex("9254FF"), Cyan = Hex("19D9FF"),
            Orange = Hex("FF8A22"), Yellow = Hex("FFE14A"), Green = Hex("65EA75"), Pink = Hex("FF4EAF"), Muted = Hex("A7B0DA");
        static Font font;

        public static MotionPlayer Create(string effectId, Transform parent, Vector2 size)
        {
            var root = Rect("Demo " + effectId, parent, new Vector2(640,360));
            bool compact = effectId == "A03" || effectId == "A04" || effectId == "A05" || effectId == "B04" || effectId == "B05" || effectId == "B06";
            root.localScale = Vector3.one * Mathf.Min(size.x / (compact ? 440f : 660f), size.y / 380f);
            var player = root.gameObject.AddComponent<MotionPlayer>();
            var b = new MotionBindings();
            player.bindings = b;
            player.effectId = effectId;
            var definition = MotionCatalog.Find(effectId);
            player.settings = definition != null ? MotionSettings.FromDefinition(definition) : new MotionSettings();

            switch (effectId)
            {
                case "A01":
                    Bind(b, Toast(root, "每日补给已送达", "一份小小奖励，开启新旅程", Cyan)); break;
                case "A02":
                    Bind(b, Toast(root, "新任务已解锁", "探索星光森林 · 3 项奖励", Green)); break;
                case "A03": case "A04": case "A05":
                    Bind(b, VictoryCard(root)); break;
                case "A06":
                    Bind(b, HeroCard(root, Purple, "星光旅者", "RARE HERO")); break;
                case "A07":
                    Reveal(root,b,false); break;
                case "A08":
                    var art = Picture("Reward illustration",root,new Vector2(384,256),Vector2.zero,Color.white,GalleryShapes.RewardArt());
                    Bind(b,art.rectTransform); b.image=art;
                    Text("Art note",root,"稀有奖励",22,new Vector2(320,38),new Vector2(0,-150),Yellow); break;
                case "B01": case "B02":
                    Bind(b, GameButton(root,effectId=="B01"?"开始挑战":"领取奖励",effectId=="B01"?Cyan:Orange));
                    Interactive(player,b.target); break;
                case "B03":
                    Bind(b, GameButton(root,"免费宝箱",Green)); Badge(b.target,"FREE",new Vector2(112,43),Pink); break;
                case "B04":
                    Bind(b, GameButton(root,"金币不足",Pink));
                    Text("Context",root,"还差 50 金币",20,new Vector2(320,40),new Vector2(0,-98),Muted); break;
                case "B05":
                    Bind(b, Gift(root)); break;
                case "B06":
                    Bind(b, GameButton(root,"完美命中",Purple)); break;
                case "B07":
                    Selection(root,b); break;
                case "B08":
                    Toggle(root,b); break;
                case "C01":
                    Bind(b, VictoryCard(root)); break;
                case "C02":
                    Drawer(root,b); break;
                case "C03":
                    Reveal(root,b,true); break;
                case "C04":
                    SharedElement(root,b); break;
                case "C05": case "C06": case "C07":
                    Pages(root,b); break;
                case "D01":
                    Count(root,b); break;
                case "D02":
                    Progress(root,b); break;
                case "D03":
                    Fly(root,b,false); break;
                case "D04":
                    Fly(root,b,true); break;
                case "D05":
                    Settlement(root,b); break;
                case "D06":
                    Stars(root,b); break;
                case "D07":
                    FlipCard(root,b); break;
                case "D08":
                    Chest(root,b); break;
                case "D09":
                    Floating(root,b); break;
                case "D10":
                    Bind(b, ResourcePill(root,new Vector2(0,0),"1,280")); break;
                case "E01":
                    Bind(b, Gem(root,Vector2.zero,110,Cyan));
                    Text("Caption",root,"能量水晶",22,new Vector2(280,40),new Vector2(0,-114),Color.white);
                    Picture("Ground",root,new Vector2(136,20),new Vector2(0,-75),new Color(.05f,.07f,.14f,.5f),GalleryShapes.Get("circle")).transform.SetAsFirstSibling(); break;
                case "E02":
                    Rotate(root,b); break;
                case "E03":
                    Shine(root,b); break;
                case "E04":
                    Bind(b, GameButton(root,"限时活动",Pink)); Badge(b.target,"NEW",new Vector2(118,43),Yellow); break;
                case "E05":
                    Stagger(root,b); break;
                case "E06":
                    Trail(root,b); break;
                case "E07":
                    Gesture(root,b); break;
                default:
                    Bind(b, GameButton(root,effectId,Purple)); break;
            }
            if (b.items == null) b.items = new RectTransform[0];
            if (b.labels == null) b.labels = new Text[0];
            if (b.itemGroups == null) b.itemGroups = new CanvasGroup[0];
            player.CaptureBaseline();
            return player;
        }

        static void Bind(MotionBindings b,RectTransform target)
        {
            b.target=target; b.group=Group(target); b.image=target.GetComponent<Image>();
        }

        static RectTransform VictoryCard(Transform parent)
        {
            var card=Panel("Victory popup",parent,new Vector2(330,250),Vector2.zero,Purple);
            Picture("Top shine",card,new Vector2(300,4),new Vector2(0,110),new Color(1,1,1,.35f));
            Star(card,new Vector2(-62,56),56,Yellow); Star(card,new Vector2(0,70),76,Yellow); Star(card,new Vector2(62,56),56,Yellow);
            Text("Title",card,"挑战成功",34,new Vector2(290,52),new Vector2(0,-3),Color.white);
            Text("Subtitle",card,"精彩表现，继续前进！",17,new Vector2(295,32),new Vector2(0,-43),new Color(1,1,1,.82f));
            var cta=Panel("Continue",card,new Vector2(220,48),new Vector2(0,-88),Yellow);
            Text("Continue label",cta,"继续冒险",21,new Vector2(210,42),Vector2.zero,Ink);
            return card;
        }

        static RectTransform Toast(Transform parent,string title,string subtitle,Color accent)
        {
            var card=Panel("Notification",parent,new Vector2(440,126),Vector2.zero,Hex("303964"));
            Picture("Accent",card,new Vector2(8,96),new Vector2(-208,0),accent);
            var icon=Picture("Icon",card,new Vector2(70,70),new Vector2(-153,0),accent,GalleryShapes.Get("circle"));
            Star(icon.transform,Vector2.zero,42,Ink);
            Text("Title",card,title,26,new Vector2(290,44),new Vector2(48,20),Color.white);
            Text("Subtitle",card,subtitle,16,new Vector2(290,35),new Vector2(48,-23),Muted);
            return card;
        }

        static RectTransform GameButton(Transform parent,string label,Color color)
        {
            var button=Panel("Game button",parent,new Vector2(310,92),Vector2.zero,color);
            Picture("Highlight",button,new Vector2(280,4),new Vector2(0,35),new Color(1,1,1,.45f));
            Star(button,new Vector2(-111,0),33,new Color(1,1,1,.9f));
            Text("Button label",button,label,30,new Vector2(238,60),new Vector2(20,0),Ink);
            return button;
        }

        static RectTransform HeroCard(Transform parent,Color color,string title,string rarity)
        {
            var card=Panel("Hero card",parent,new Vector2(210,278),Vector2.zero,color);
            var face=Panel("Portrait",card,new Vector2(180,164),new Vector2(0,34),Hex("242B59"));
            Picture("Halo",face,new Vector2(115,115),new Vector2(0,6),new Color(.8f,.5f,1,.4f),GalleryShapes.Get("ring"));
            Gem(face,new Vector2(0,4),83,Cyan);
            Text("Rarity",card,rarity,14,new Vector2(190,32),new Vector2(0,-66),Yellow);
            Text("Name",card,title,24,new Vector2(195,40),new Vector2(0,-101),Color.white);
            return card;
        }

        static void Reveal(Transform parent,MotionBindings b,bool vertical)
        {
            var mask=Rect(vertical?"Height clip":"Width clip",parent,new Vector2(380,220));
            mask.pivot=vertical?new Vector2(.5f,1):new Vector2(0,.5f);
            mask.anchoredPosition=vertical?new Vector2(0,110):new Vector2(-190,0);
            mask.gameObject.AddComponent<RectMask2D>();
            var content=Panel("Unscaled content",mask,new Vector2(380,220),Vector2.zero,vertical?Hex("176F8C"):Purple);
            content.anchorMin=content.anchorMax=vertical?new Vector2(.5f,1):new Vector2(0,.5f);
            content.pivot=vertical?new Vector2(.5f,1):new Vector2(.5f,.5f);
            content.anchoredPosition=vertical?Vector2.zero:new Vector2(190,0);
            float cy=vertical?-110:0;
            Text("Title",content,vertical?"任务详情":"发现新世界",29,new Vector2(340,45),new Vector2(0,cy+62),Color.white);
            if(vertical)
            {
                for(int i=0;i<3;i++)
                {
                    Picture("Check "+i,content,new Vector2(24,24),new Vector2(-146,cy+12-i*40),Green,GalleryShapes.Get("circle"));
                    Text("Quest "+i,content,new[]{"完成一次冒险","收集 5 枚星星","开启森林宝箱"}[i],20,new Vector2(270,34),new Vector2(20,cy+12-i*40),Color.white);
                }
            }
            else
            {
                Gem(content,new Vector2(-95,cy-25),80,Cyan);
                Text("Body",content,"星光森林\n探索进度  3 / 8",22,new Vector2(210,90),new Vector2(62,cy-26),Yellow);
            }
            Bind(b,content); b.mask=mask;
        }

        static RectTransform Gift(Transform parent)
        {
            var root=Rect("Gift",parent,new Vector2(180,180));
            var body=Panel("Box",root,new Vector2(154,130),new Vector2(0,-20),Pink);
            Picture("Ribbon",body,new Vector2(30,130),Vector2.zero,Yellow);
            Panel("Lid",root,new Vector2(174,40),new Vector2(0,57),Purple);
            Picture("Lid ribbon",root,new Vector2(30,40),new Vector2(0,57),Yellow);
            var left=Picture("Bow left",root,new Vector2(53,35),new Vector2(-24,91),Yellow,GalleryShapes.Get("ring"));left.rectTransform.localEulerAngles=new Vector3(0,0,-25);
            var right=Picture("Bow right",root,new Vector2(53,35),new Vector2(24,91),Yellow,GalleryShapes.Get("ring"));right.rectTransform.localEulerAngles=new Vector3(0,0,25);
            return root;
        }

        static void Selection(Transform root,MotionBindings b)
        {
            var rail=Panel("Selection rail",root,new Vector2(490,100),Vector2.zero,Hex("262E54"));
            var old=Panel("Previous selected",rail,new Vector2(138,70),new Vector2(-155,0),Purple);
            Text("Old tab",old,"主页",24,new Vector2(128,60),Vector2.zero,Color.white);
            var target=Panel("New selection",rail,new Vector2(138,70),Vector2.zero,Hex("4F5D92"));
            Text("New tab",target,"冒险",24,new Vector2(128,60),Vector2.zero,Color.white);
            Text("Other tab",rail,"背包",24,new Vector2(138,60),new Vector2(155,0),Muted);
            Bind(b,target); b.secondary=old;b.secondaryImage=old.GetComponent<Image>();b.secondaryGroup=Group(old);
            Text("Arrow note",root,"主页  →  冒险",20,new Vector2(400,35),new Vector2(0,-93),Muted);
        }

        static void Toggle(Transform root,MotionBindings b)
        {
            var panel=Panel("Toggle setting",root,new Vector2(410,140),Vector2.zero,Hex("303964"));
            Text("Setting label",panel,"背景音乐",25,new Vector2(195,50),new Vector2(-80,0),Color.white);
            var track=Panel("Switch track",panel,new Vector2(130,62),new Vector2(113,0),Hex("68749D"));
            var knob=Picture("Switch knob",track,new Vector2(49,49),new Vector2(-31,0),Color.white,GalleryShapes.Get("circle"));
            var end=Rect("Enabled position",track,Vector2.one);end.anchoredPosition=new Vector2(31,0);
            Bind(b,knob.rectTransform);b.image=track.GetComponent<Image>();b.destination=end;
            Text("State",root,"OFF                         ON",18,new Vector2(320,35),new Vector2(0,-108),Muted);
        }

        static void Drawer(Transform root,MotionBindings b)
        {
            var screen=Panel("Background page",root,new Vector2(480,290),Vector2.zero,Hex("344375"));
            screen.gameObject.AddComponent<RectMask2D>();
            Text("Background title",screen,"冒险地图",27,new Vector2(340,45),new Vector2(0,99),Color.white);
            for(int i=0;i<3;i++)Gem(screen,new Vector2(-110+i*110,-10),50,new[]{Green,Cyan,Pink}[i]);
            var dim=Picture("Dimmer",screen,new Vector2(480,290),Vector2.zero,new Color(0,0,0,.7f));
            var panel=Panel("Drawer",screen,new Vector2(224,290),new Vector2(-128,0),Purple);
            Text("Drawer heading",panel,"菜单",28,new Vector2(180,45),new Vector2(0,99),Color.white);
            for(int i=0;i<3;i++)
            {
                var row=Panel("Menu row "+i,panel,new Vector2(184,48),new Vector2(0,34-i*65),i==0?Yellow:Hex("7140CD"));
                Text("Menu label "+i,row,new[]{"角色","背包","设置"}[i],22,new Vector2(170,44),Vector2.zero,i==0?Ink:Color.white);
            }
            Bind(b,panel);b.secondary=dim.rectTransform;b.secondaryGroup=Group(dim.rectTransform);
        }

        static void SharedElement(Transform root,MotionBindings b)
        {
            var destination=Panel("Destination outline",root,new Vector2(330,230),new Vector2(125,15),new Color(.3f,.35f,.6f,.22f));
            Text("Destination hint",destination,"详情",21,new Vector2(280,40),new Vector2(0,75),Muted);
            var card=Panel("Shared element",root,new Vector2(145,108),new Vector2(-170,-55),Orange);
            Star(card,new Vector2(0,14),45,Yellow);
            Text("Card name",card,"森林宝箱",18,new Vector2(140,35),new Vector2(0,-33),Ink);
            Bind(b,card);b.destination=destination;
            Text("Relation",root,"卡片 → 详情面板",19,new Vector2(370,36),new Vector2(0,-147),Muted);
        }

        static void Pages(Transform root,MotionBindings b)
        {
            var clip=Rect("Page viewport",root,new Vector2(430,268));clip.gameObject.AddComponent<RectMask2D>();
            var old=Page(clip,Purple,"星光森林","CHAPTER 01",Green);
            var next=Page(clip,Hex("087FB7"),"珊瑚海湾","CHAPTER 02",Orange);
            Bind(b,old);b.secondary=next;b.secondaryGroup=Group(next);b.secondaryImage=next.GetComponent<Image>();
        }

        static RectTransform Page(Transform root,Color color,string title,string chapter,Color gem)
        {
            var panel=Panel("Page "+chapter,root,new Vector2(420,258),Vector2.zero,color);
            Text("Chapter",panel,chapter,14,new Vector2(330,30),new Vector2(0,91),new Color(1,1,1,.65f));
            Text("Title",panel,title,32,new Vector2(360,48),new Vector2(0,53),Color.white);
            Gem(panel,new Vector2(-54,-32),65,gem);Star(panel,new Vector2(59,-33),62,Yellow);
            Text("Footer",panel,"探索新的冒险",17,new Vector2(330,32),new Vector2(0,-99),Color.white);
            return panel;
        }

        static void Count(Transform root,MotionBindings b)
        {
            var card=Panel("Wallet",root,new Vector2(390,198),Vector2.zero,Purple);
            Text("Title",card,"金币余额",21,new Vector2(330,36),new Vector2(0,58),new Color(1,1,1,.8f));
            Coin(card,new Vector2(-110,-12),66);
            var number=Text("Value",card,"120",52,new Vector2(230,80),new Vector2(47,-14),Yellow);
            Bind(b,card);b.label=number;
        }

        static void Progress(Transform root,MotionBindings b)
        {
            var card=Panel("Experience card",root,new Vector2(430,200),Vector2.zero,Hex("1E6B88"));
            Text("Level",card,"LEVEL 08",22,new Vector2(350,40),new Vector2(0,56),Yellow);
            var track=Panel("Progress track",card,new Vector2(354,36),new Vector2(0,-4),Ink);
            var fill=Picture("Progress fill",track,new Vector2(342,24),Vector2.zero,Green,GalleryShapes.Get("rounded"));
            fill.type=Image.Type.Filled;fill.fillMethod=Image.FillMethod.Horizontal;fill.fillOrigin=0;fill.fillAmount=.15f;
            Text("Caption",card,"距离下一级更近一步",18,new Vector2(360,40),new Vector2(0,-58),Color.white);
            Bind(b,card);b.fill=fill;
        }

        static RectTransform ResourcePill(Transform root,Vector2 position,string amount)
        {
            var pill=Panel("Gold counter",root,new Vector2(178,68),position,Purple);
            Coin(pill,new Vector2(-53,0),44);
            Text("Gold amount",pill,amount,27,new Vector2(116,56),new Vector2(28,0),Yellow);
            return pill;
        }

        static void Fly(Transform root,MotionBindings b,bool burst)
        {
            var dest=ResourcePill(root,new Vector2(193,104),"1,280");
            Text("Earned",root,burst?"大额奖励":"金币 +100",24,new Vector2(250,40),burst ? new Vector2(-157,135) : new Vector2(-135,-132),Yellow);
            if(!burst)
            {
                Bind(b,Coin(root,new Vector2(-170,-55),72));b.destination=dest;
                Text("Guide",root,"奖励进入资源栏",17,new Vector2(260,35),new Vector2(120,-93),Muted);
            }
            else
            {
                var start=Rect("Reward coins",root,new Vector2(180,140));start.anchoredPosition=new Vector2(-150,-42);
                Bind(b,start);var items=new List<RectTransform>();var groups=new List<CanvasGroup>();
                for(int i=0;i<9;i++){var coin=Coin(start,new Vector2((i%3-1)*12,(i/3-1)*10),42);items.Add(coin);groups.Add(Group(coin));}
                b.items=items.ToArray();b.itemGroups=groups.ToArray();b.destination=dest;
            }
        }

        static void Settlement(Transform root,MotionBindings b)
        {
            var panel=Panel("Results",root,new Vector2(370,298),Vector2.zero,Hex("38336F"));
            Text("Heading",panel,"冒险结算",27,new Vector2(330,42),new Vector2(0,112),Color.white);
            var items=new List<RectTransform>();var groups=new List<CanvasGroup>();var labels=new List<Text>();
            string[] names={"基础分数","连击加成","时间奖励"};
            for(int i=0;i<3;i++)
            {
                var row=Panel("Score row "+i,panel,new Vector2(320,40),new Vector2(0,53-i*48),Hex("4D448B"));
                Text("Name",row,names[i],18,new Vector2(170,34),new Vector2(-61,0),Muted);
                labels.Add(Text("Value",row,"0",22,new Vector2(100,36),new Vector2(99,0),Yellow));items.Add(row);groups.Add(Group(row));
            }
            Text("Total caption",panel,"总分",20,new Vector2(100,40),new Vector2(-103,-108),Color.white);
            var total=Text("Total",panel,"0",32,new Vector2(160,44),new Vector2(76,-108),Green);
            Bind(b,panel);b.items=items.ToArray();b.itemGroups=groups.ToArray();b.labels=labels.ToArray();b.label=total;
        }

        static void Stars(Transform root,MotionBindings b)
        {
            var frame=Rect("Star rating",root,new Vector2(450,200));
            Text("Heading",frame,"完美通关",30,new Vector2(380,45),new Vector2(0,110),Color.white);
            var items=new List<RectTransform>();var groups=new List<CanvasGroup>();
            for(int i=0;i<3;i++)
            {
                var p=new Vector2((i-1)*128,i==1?15:0);
                Star(frame,p,i==1?112:92,Hex("3B3D5F"));
                var star=Star(frame,p,i==1?112:92,Yellow);items.Add(star);groups.Add(Group(star));
            }
            Text("Caption",frame,"三颗星，一次漂亮的冒险",19,new Vector2(430,40),new Vector2(0,-99),Muted);
            Bind(b,frame);b.items=items.ToArray();b.itemGroups=groups.ToArray();
        }

        static void FlipCard(Transform root,MotionBindings b)
        {
            var pivot=Rect("Flipping card",root,new Vector2(210,278));
            var back=Panel("Card back",pivot,new Vector2(210,278),Vector2.zero,Purple);
            Picture("Border",back,new Vector2(178,242),Vector2.zero,new Color(1,1,1,.13f));
            Gem(back,Vector2.zero,104,Yellow);
            Text("Mystery",back,"?",48,new Vector2(90,70),Vector2.zero,Ink);
            var front=HeroCard(pivot,Orange,"烈焰勇者","LEGENDARY");
            Bind(b,pivot);b.image=back.GetComponent<Image>();b.secondaryImage=front.GetComponent<Image>();b.secondaryGroup=Group(front);
            // The back face has its own group so its child artwork never leaks through the front.
            b.group=Group(back);
        }

        static void Chest(Transform root,MotionBindings b)
        {
            var chest=Rect("Treasure chest",root,new Vector2(240,176));chest.anchoredPosition=new Vector2(0,-31);
            var body=Panel("Chest body",chest,new Vector2(230,132),new Vector2(0,-12),Orange);
            Picture("Left belt",body,new Vector2(25,125),new Vector2(-77,0),Yellow);
            Picture("Right belt",body,new Vector2(25,125),new Vector2(77,0),Yellow);
            var lid=Panel("Chest lid",chest,new Vector2(244,72),new Vector2(0,54),Yellow);
            lid.pivot=new Vector2(.5f,0);lid.anchoredPosition=new Vector2(0,32);
            Picture("Lid inset",lid,new Vector2(210,35),Vector2.zero,Orange);
            var lockPlate=Panel("Lock",chest,new Vector2(48,59),new Vector2(0,4),Yellow);
            Picture("Keyhole",lockPlate,new Vector2(13,20),Vector2.zero,Ink,GalleryShapes.Get("circle"));
            var rewards=new List<RectTransform>();var groups=new List<CanvasGroup>();
            for(int i=0;i<3;i++)
            {
                var gem=Gem(root,new Vector2((i-1)*67,112+(i==1?20:0)),i==1?64:46,new[]{Cyan,Pink,Green}[i]);rewards.Add(gem);groups.Add(Group(gem));
            }
            Bind(b,chest);b.lid=lid;b.items=rewards.ToArray();b.itemGroups=groups.ToArray();
        }

        static void Floating(Transform root,MotionBindings b)
        {
            var target=Gem(root,new Vector2(0,-68),98,Purple);
            Text("Target name",root,"星光水晶",19,new Vector2(260,35),new Vector2(0,-139),Muted);
            var value=Text("Floating reward",root,"+ 100",52,new Vector2(330,80),new Vector2(0,4),Green);
            var outline=value.gameObject.AddComponent<Outline>();outline.effectColor=Ink;outline.effectDistance=new Vector2(2,-2);
            Bind(b,value.rectTransform);b.label=value;
        }

        static void Rotate(Transform root,MotionBindings b)
        {
            var wheel=Rect("Loading wheel",root,new Vector2(160,160));
            for(int i=0;i<8;i++)
            {
                float angle=i*Mathf.PI/4;
                Picture("Orbit dot "+i,wheel,new Vector2(27,27),new Vector2(Mathf.Cos(angle)*64,Mathf.Sin(angle)*64),Color.Lerp(Cyan,Purple,i/7f),GalleryShapes.Get("circle"));
            }
            Star(wheel,Vector2.zero,50,Yellow);Bind(b,wheel);
            Text("Loading caption",root,"准备冒险中",22,new Vector2(300,40),new Vector2(0,-125),Muted);
        }

        static void Shine(Transform root,MotionBindings b)
        {
            var card=GameButton(root,"传说奖励",Purple);
            var mask=card.gameObject.AddComponent<Mask>();mask.showMaskGraphic=true;
            var stripe=Picture("Shine stripe",card,new Vector2(45,160),Vector2.zero,new Color(1,1,1,.55f));
            stripe.rectTransform.localEulerAngles=new Vector3(0,0,-22);
            Bind(b,card);b.mask=card;b.shine=stripe.rectTransform;
        }

        static void Stagger(Transform root,MotionBindings b)
        {
            var menu=Rect("Stagger menu",root,new Vector2(360,270));
            var items=new List<RectTransform>();var groups=new List<CanvasGroup>();
            string[] names={"继续冒险","探索地图","打开背包"};Color[] colors={Cyan,Purple,Orange};
            for(int i=0;i<3;i++)
            {
                var row=Panel("Menu item "+i,menu,new Vector2(320,65),new Vector2(0,89-i*89),colors[i]);
                Text("Name",row,names[i],25,new Vector2(300,56),Vector2.zero,i==1?Color.white:Ink);
                items.Add(row);groups.Add(Group(row));
            }
            Bind(b,menu);b.items=items.ToArray();b.itemGroups=groups.ToArray();
        }

        static void Trail(Transform root,MotionBindings b)
        {
            var leader=Star(root,Vector2.zero,63,Yellow);
            var items=new List<RectTransform>();var groups=new List<CanvasGroup>();
            items.Add(leader);groups.Add(Group(leader));
            for(int i=0;i<6;i++)
            {
                var bead=Gem(root,Vector2.zero,43-i*4,Color.Lerp(Cyan,Purple,i/5f));
                bead.SetAsFirstSibling();items.Add(bead);groups.Add(Group(bead));
            }
            Bind(b,leader);b.items=items.ToArray();b.itemGroups=groups.ToArray();
            Text("Trail caption",root,"同一条路径，错开的时间",19,new Vector2(420,40),new Vector2(0,-114),Muted);
        }

        static void Gesture(Transform root,MotionBindings b)
        {
            var target=GameButton(root,"点击领取",Green);target.anchoredPosition=new Vector2(0,38);
            var halo=Picture("Target highlight",root,new Vector2(340,120),new Vector2(0,38),new Color(1,.88f,.2f,.35f));halo.transform.SetAsFirstSibling();
            var hand=Picture("Tutorial hand",root,new Vector2(88,110),new Vector2(105,-91),Color.white,GalleryShapes.Get("hand"));
            hand.rectTransform.localEulerAngles=new Vector3(0,0,-20);
            Bind(b,hand.rectTransform);b.secondary=halo.rectTransform;b.secondaryGroup=Group(halo.rectTransform);
        }

        static void Interactive(MotionPlayer player,RectTransform target)
        {
            var image=target.GetComponent<Image>();if(image)image.raycastTarget=true;
            var button=target.gameObject.AddComponent<Button>();button.transition=Selectable.Transition.None;
            var feedback=target.gameObject.AddComponent<MotionPointerFeedback>();
            feedback.player=player;feedback.rebound=player.effectId=="B02";
        }

        static RectTransform Gem(Transform parent,Vector2 position,float size,Color color)
        {
            var gem=Picture("Gem",parent,new Vector2(size,size*1.12f),position,color,GalleryShapes.Get("diamond")).rectTransform;
            var shine=Picture("Gem facet",gem,new Vector2(size*.48f,size*.54f),new Vector2(-size*.13f,size*.14f),new Color(1,1,1,.48f),GalleryShapes.Get("diamond"));
            return gem;
        }

        static RectTransform Coin(Transform parent,Vector2 position,float size)
        {
            var coin=Picture("Coin",parent,new Vector2(size,size),position,Hex("ED9B00"),GalleryShapes.Get("circle")).rectTransform;
            Picture("Coin face",coin,new Vector2(size*.83f,size*.83f),new Vector2(0,size*.025f),Yellow,GalleryShapes.Get("circle"));
            Star(coin,new Vector2(0,size*.025f),size*.48f,Orange);return coin;
        }

        static RectTransform Star(Transform parent,Vector2 position,float size,Color color) => Picture("Star",parent,new Vector2(size,size),position,color,GalleryShapes.Get("star")).rectTransform;

        static void Badge(Transform parent,string label,Vector2 position,Color color)
        {
            var badge=Panel("Badge",parent,new Vector2(76,32),position,color);
            Text("Badge text",badge,label,16,new Vector2(70,28),Vector2.zero,Ink);
        }

        static RectTransform Panel(string name,Transform parent,Vector2 size,Vector2 position,Color color)
        {
            var panel=Picture(name,parent,size,position,color).rectTransform;
            var shadow=panel.gameObject.AddComponent<Shadow>();shadow.effectColor=new Color(.02f,.025f,.08f,.45f);shadow.effectDistance=new Vector2(0,-7);shadow.useGraphicAlpha=true;
            return panel;
        }

        static Image Picture(string name,Transform parent,Vector2 size,Vector2 position,Color color,Sprite sprite=null)
        {
            var rect=Rect(name,parent,size);rect.anchoredPosition=position;
            var image=rect.gameObject.AddComponent<Image>();image.sprite=sprite?sprite:GalleryShapes.Get("rounded");
            image.type=sprite?Image.Type.Simple:Image.Type.Sliced;image.color=color;image.raycastTarget=false;return image;
        }

        static Text Text(string name,Transform parent,string value,int fontSize,Vector2 size,Vector2 position,Color color)
        {
            if(!font)font=Resources.Load<Font>("GalleryFont");if(!font)font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var rect=Rect(name,parent,size);rect.anchoredPosition=position;
            var text=rect.gameObject.AddComponent<Text>();text.font=font;text.text=value;text.fontSize=fontSize;text.color=color;text.alignment=TextAnchor.MiddleCenter;
            text.horizontalOverflow=HorizontalWrapMode.Wrap;text.verticalOverflow=VerticalWrapMode.Overflow;text.raycastTarget=false;
            return text;
        }

        static RectTransform Rect(string name,Transform parent,Vector2 size)
        {
            var rect=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();rect.SetParent(parent,false);
            rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(.5f,.5f);rect.sizeDelta=size;return rect;
        }

        static CanvasGroup Group(RectTransform rect)
        {
            var group=rect.GetComponent<CanvasGroup>();if(!group)group=rect.gameObject.AddComponent<CanvasGroup>();group.alpha=1;return group;
        }

        static Color Hex(string hex) { ColorUtility.TryParseHtmlString("#"+hex,out var color);return color; }
    }
}
