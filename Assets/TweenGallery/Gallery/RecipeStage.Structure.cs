using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TweenGallery.Recipes
{
    public sealed partial class RecipeStage
    {
        void BuildPage() {
            int n=Definition.Number;
            var page=Panel("探索旅程",0,0,640,300);
            var title=Caption(page,"星光群岛",0,70);
            var card=RecipeUI.Card(page,"森林 01",-160,-30,140,160);
            var second=RecipeUI.Card(page,"海湾 02",10,-30,140,160);
            var controls=RecipeUI.Button(page,"继续旅程",185,-50,150,()=>{Put("pageClicks",Get("pageClicks")+1);SetState("页面点击 "+Get("pageClicks"));},RecipeUI.Blue);
            var group=RecipeUI.Group(page);
            if(n==1||n==7){group.alpha=0;group.blocksRaycasts=group.interactable=false;}
            switch(n) {
                case 1:case 2:case 7:
                    Operation(n==2?"退出页面":"进入页面",()=>{
                        bool show=n!=2;Put("visible",show?1:0);SetState(show?"页面已进入，操作区随进度就绪":"页面退出，不再接收输入");
                        float start=group.alpha;var from=page.anchoredPosition;
                        Transition(p=>{float t=Reduced?1:Ease(p);group.alpha=Mathf.Lerp(start,show?1:0,t);page.anchoredPosition=Vector2.Lerp(from,show?Vector2.zero:new Vector2(-45,0),t);
                            if(n==7){RecipeUI.Alpha(title.rectTransform,Reduced?1:Mathf.Clamp01(p*3));RecipeUI.Alpha(card,Reduced?1:Mathf.Clamp01((p-.15f)*3));RecipeUI.Alpha(second,Reduced?1:Mathf.Clamp01((p-.3f)*3));}
                            group.interactable=group.blocksRaycasts=show&&p>=.85f;
                        });
                    });break;
                case 3:
                    var columnB=Panel("竞技栏目",0,0,640,300);Caption(columnB,"每周排行 · 玩家当前第 4 名",0,10,550);RecipeUI.Button(columnB,"查看榜单",0,-70,180,()=>{Put("rankingClicks",Get("rankingClicks")+1);},RecipeUI.Orange);RecipeUI.Alpha(columnB,0);
                    Operation("切换栏目",()=>{Put("selected",1-Get("selected"));Put("column",Get("selected"));SetState("栏目 "+Get("column"));FadeSwap(page,columnB);});break;
                case 4:
                    var detail=Panel("森林关卡详情",55,0,560,290);Caption(detail,"目标：收集 12 颗星星",0,25);RecipeUI.Alpha(detail,0);
                    Operation("打开详情",()=>{Put("depth",1);SetState("详情层 · 父页保留");var start=detail.anchoredPosition;Transition(p=>{float t=Reduced?1:Ease(p);RecipeUI.Alpha(detail,t);detail.anchoredPosition=Vector2.Lerp(start,new Vector2(0,0),t);RecipeUI.Group(page).interactable=false;});});
                    Operation("返回父页",()=>{Put("depth",0);SetState("父页 · 原内容保留");var start=RecipeUI.Group(detail).alpha;Transition(p=>RecipeUI.Alpha(detail,Mathf.Lerp(start,0,Reduced?1:Ease(p))));group.interactable=group.blocksRaycasts=true;});break;
                case 5:
                    RecipeUI.Group(second).alpha=0;controls.gameObject.SetActive(false);
                    var startPos=card.anchoredPosition;var pageHeading=page.GetComponentInChildren<Text>().rectTransform;
                    var cardHit=card.gameObject.AddComponent<Button>();card.GetComponent<Image>().raycastTarget=true;cardHit.onClick.AddListener(()=>Trigger("展开卡片"));
                    Operation("展开卡片",()=>{Put("expanded",1);SetState("同一插画进入详情");var from=card.anchoredPosition;var scale=card.localScale;float headingAlpha=RecipeUI.Group(pageHeading).alpha,captionAlpha=RecipeUI.Group(title.rectTransform).alpha;Transition(p=>{float t=Reduced?1:Ease(p);RecipeUI.Alpha(pageHeading,Mathf.Lerp(headingAlpha,0,t));RecipeUI.Alpha(title.rectTransform,Mathf.Lerp(captionAlpha,0,t));card.anchoredPosition=Vector2.Lerp(from,Vector2.zero,t);card.localScale=Vector3.Lerp(scale,Vector3.one*1.55f,t);});});
                    Operation("返回卡片",()=>{Put("expanded",0);SetState("卡片原位");var from=card.anchoredPosition;var scale=card.localScale;float headingAlpha=RecipeUI.Group(pageHeading).alpha,captionAlpha=RecipeUI.Group(title.rectTransform).alpha;Transition(p=>{float t=Reduced?1:Ease(p);RecipeUI.Alpha(pageHeading,Mathf.Lerp(headingAlpha,1,t));RecipeUI.Alpha(title.rectTransform,Mathf.Lerp(captionAlpha,1,t));card.anchoredPosition=Vector2.Lerp(from,startPos,t);card.localScale=Vector3.Lerp(scale,Vector3.one,t);});});break;
                case 6:
                    title.text="场景演出 · HUD 独立显隐";var hud=RecipeUI.Rect(world,"HUD",0,0,700,320);
                    var score=RecipeUI.Box(hud,"Score HUD",-230,140,210,45,RecipeUI.Blue).rectTransform;Caption(score,"★ 1,280",0,0,180);
                    controls.transform.SetParent(hud,false);
                    Operation("切换 HUD",()=>{Put("hidden",1-Get("hidden"));bool hide=Get("hidden")==1;SetState(hide?"HUD隐藏，场景继续":"HUD显示");float a=RecipeUI.Group(hud).alpha;Transition(p=>RecipeUI.Alpha(hud,Mathf.Lerp(a,hide?0:1,Reduced?1:Ease(p))));});break;
                case 8:
                    Operation("切换宽窄布局",()=>{Put("narrow",1-Get("narrow"));bool narrow=Get("narrow")==1;SetState(narrow?"窄布局 · 数据保留":"宽布局 · 数据保留");Vector2 a=card.anchoredPosition,b=second.anchoredPosition;float width=page.sizeDelta.x;controls.gameObject.SetActive(!narrow);Transition(p=>{float t=Reduced?1:Ease(p);page.sizeDelta=new Vector2(Mathf.Lerp(width,narrow?380:640,t),300);card.anchoredPosition=Vector2.Lerp(a,new Vector2(narrow?-85:-160,-30),t);second.anchoredPosition=Vector2.Lerp(b,new Vector2(narrow?85:10,-30),t);});});break;
            }
        }
        void BuildModal() {
            int n=Definition.Number;Put("backgroundClicks",0);
            RecipeUI.Button(world,"底层页面按钮",0,0,240,()=>{Put("backgroundClicks",Get("backgroundClicks")+1);SetState("底层点击 "+Get("backgroundClicks"));},RecipeUI.Blue);
            var blocker=RecipeUI.Box(world,"Modal barrier",0,0,820,350,new Color(0,0,0,.55f));blocker.raycastTarget=true;
            var panel=Panel(n==3?"设置抽屉":n==4?"底部操作面板":"操作确认",0,0,n==3?280:430,n==4?210:260);
            if(RecipeArt.Available&&n==3)RecipeArt.Apply(panel.GetComponent<Image>(),"drawer");
            var body=Caption(panel,"确认继续探索星光群岛？",0,25);
            bool initiallyOpen=n==2||n==7||n==8;
            RecipeUI.Alpha(panel,initiallyOpen?1:0);blocker.gameObject.SetActive(initiallyOpen);
            Put("open",initiallyOpen?1:0);
            Action<bool> visibility=show=>{
                Put("open",show?1:0);SetState(show?"打开 · 屏障阻挡底层":"关闭请求 · 退出完成才释放屏障");
                blocker.gameObject.SetActive(true);var g=RecipeUI.Group(panel);float a=g.alpha;Vector2 pos=panel.anchoredPosition;
                Vector2 target=show?new Vector2(n==3?255:0,n==4?-60:0):new Vector2(n==3?500:0,n==4?-280:0);
                Transition(p=>{float t=Reduced?1:Ease(p);g.alpha=Mathf.Lerp(a,show?1:0,t);panel.anchoredPosition=Vector2.Lerp(pos,target,t);g.interactable=g.blocksRaycasts=show&&p>=.9f;blocker.gameObject.SetActive(show||p<1);});
            };
            if(n<=4){Operation("打开",()=>visibility(true));Operation("关闭",()=>visibility(false));RecipeUI.Button(panel,"关闭",0,-75,130,()=>visibility(false),RecipeUI.Pink);}
            if(n==5){
                blocker.gameObject.SetActive(false);panel.sizeDelta=new Vector2(290,170);panel.anchoredPosition=new Vector2(0,-20);panel.GetComponentInChildren<Text>().text="目的地";body.gameObject.SetActive(false);RecipeUI.Alpha(panel,0);
                var trigger=RecipeUI.Button(world,"选择目的地 ▾",0,105,290,()=>Trigger("展开菜单"),RecipeUI.Blue);
                Action<bool> menu=show=>{Put("open",show?1:0);SetState(show?"菜单展开 · 锚定入口":"菜单收起");float a=RecipeUI.Group(panel).alpha;Transition(p=>RecipeUI.Alpha(panel,Mathf.Lerp(a,show?1:0,Reduced?1:Ease(p))));};
                Operation("展开菜单",()=>menu(Get("open")==0));
                RecipeUI.Button(panel,"星光森林",0,0,260,()=>{Put("destination",1);menu(false);trigger.GetComponentInChildren<Text>().text="星光森林 ▾";SetState("已选择：星光森林");},RecipeUI.Green);
                RecipeUI.Button(panel,"蔚蓝海湾",0,-48,260,()=>{Put("destination",2);menu(false);trigger.GetComponentInChildren<Text>().text="蔚蓝海湾 ▾";SetState("已选择：蔚蓝海湾");},RecipeUI.Blue);
            }
            if(n==6){
                blocker.gameObject.SetActive(false);panel.sizeDelta=new Vector2(340,100);panel.anchoredPosition=new Vector2(0,20);panel.GetComponentInChildren<Text>().gameObject.SetActive(false);RecipeUI.Alpha(panel,0);body.text="稀有物品可用于升级";body.rectTransform.anchoredPosition=Vector2.zero;body.rectTransform.sizeDelta=new Vector2(310,70);
                if(RecipeArt.Available)RecipeArt.Apply(panel.GetComponent<Image>(),"tooltip");
                var target=RecipeUI.Button(world,"i  信息",0,-105,140,null,RecipeUI.Blue);target.transform.SetAsLastSibling();
                var hover=target.gameObject.AddComponent<RecipeHover>();float entered=-1;
                Action<bool> tooltip=show=>{Put("open",show?1:0);float a=RecipeUI.Group(panel).alpha;Transition(p=>RecipeUI.Alpha(panel,Mathf.Lerp(a,show?1:0,Reduced?1:Ease(p))));};
                hover.enter=()=>{entered=Time.unscaledTime;SetState("等待 0.45 秒");};
                hover.exit=()=>{entered=-1;tooltip(false);SetState("提示已取消");};
                ticking.Add(()=>{if(entered>=0&&Time.unscaledTime-entered>=.45f){entered=-1;tooltip(true);SetState("悬停提示");}});
                Operation("模拟停留",()=>{tooltip(true);});Operation("离开",()=>hover.exit());
            }
            if(n==7){
                var contentA=RecipeUI.Rect(panel,"Step one",0,-15,380,120);Caption(contentA,"步骤 1：选择目的地");
                var contentB=RecipeUI.Rect(panel,"Step two",0,-15,380,120);Caption(contentB,"步骤 2：确认携带道具");RecipeUI.Alpha(contentB,0);body.text="分步操作";
                Operation("下一步／上一步",()=>{Put("selected",1-Get("selected"));SetState("弹窗步骤 "+(Get("selected")+1));FadeSwap(contentA,contentB);});
            }
            if(n==8){
                bool confirmed=false;body.text="请先勾选确认，再关闭";
                RecipeUI.Button(panel,"□ 我已确认",0,-35,210,()=>{confirmed=!confirmed;Put("confirmed",confirmed?1:0);body.text=confirmed?"✓ 已确认，可以关闭":"请先勾选确认，再关闭";},RecipeUI.Blue);
                Operation("尝试关闭",()=>{if(confirmed){visibility(false);return;}Put("rejected",Get("rejected")+1);SetState("拒绝关闭：尚未勾选确认");var rest=panel.anchoredPosition;Transition(p=>panel.anchoredPosition=rest+Vector2.right*(Reduced?0:Mathf.Sin(p*Mathf.PI*8)*(1-p)*12*Strength));});
                Operation("勾选确认",()=>{confirmed=true;Put("confirmed",1);body.text="✓ 已确认，可以关闭";});
            }
        }
        void BuildContainer(){
            int n=Definition.Number;bool expanded=n!=1;float full=300,small=92;
            var container=RecipeUI.Box(world,"Daily mission board",0,180,590,expanded?full:small,RecipeUI.Surface).rectTransform;RecipeArt.Apply(container.GetComponent<Image>(),"wb-inventory");container.pivot=new Vector2(.5f,1);container.gameObject.AddComponent<RectMask2D>();PrimarySubject=container;if(n==1||n==2)Put("expanded",expanded?1:0);
            var header=RecipeUI.Rect(container,"Mission header",0,-39,550,64);header.anchorMin=header.anchorMax=new Vector2(.5f,1);header.anchoredPosition=new Vector2(0,-39);
            Art(header,"Mission icon","star-lit",-239,0,42,42);RecipeUI.Label(header,n==3?"探索委托说明":n==4?"任务 / 奖励分栏":"每日探索任务",-24,0,365,48,25,RecipeUI.Ink,TextAnchor.MiddleLeft);var taskCount=RecipeUI.Label(header,n==3?"详情":n==4?"三档":n==7?"0 项":"3 项",228,0,70,40,19,RecipeUI.Gold);
            var content=RecipeUI.Rect(container,"Mission contents",0,-86,544,198);content.anchorMin=content.anchorMax=new Vector2(.5f,1);content.pivot=new Vector2(.5f,1);content.anchoredPosition=new Vector2(0,-86);
            System.Func<Transform,string[],RectTransform> tasks=(parent,names)=>{var r=RecipeUI.Rect(parent,"Task rows",0,0,540,195);r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,1);r.anchoredPosition=Vector2.zero;for(int i=0;i<3;i++){var row=RecipeUI.Rect(r,"Mission "+i,0,-30-i*62,530,54);row.anchorMin=row.anchorMax=new Vector2(.5f,1);row.anchoredPosition=new Vector2(0,-30-i*62);RecipeUI.Box(row,"Mission row",0,0,530,54,RecipeUI.Surface).sprite=RecipeArt.Get("wb-gray");Art(row,"Task marker",i==0?"swords":i==1?"crystal":"coin",-230,0,37,40);RecipeUI.Label(row,names[i],-8,0,375,46,20,RecipeUI.Ink,TextAnchor.MiddleLeft);RecipeUI.Label(row,"+20",221,0,70,42,18,RecipeUI.Gold);}return r;};
            var rows=tasks(content,new[]{"探索星光森林  0/3","收集蓝色水晶  2/5","返回营地领取补给"});
            var footer=RecipeUI.Box(world,"Adjacent mission",0,180-(expanded?full:small)-45,590,46,RecipeUI.Surface).rectTransform;RecipeArt.Apply(footer.GetComponent<Image>(),"wb-gray");RecipeUI.Label(footer,"下一项：周常挑战 · 进度保留",0,0,550,42,20,RecipeUI.Muted);
            if(n==1||n==2){RecipeUI.Alpha(content,expanded?1:0);var hit=header.gameObject.AddComponent<Image>();hit.color=new Color(1,1,1,.01f);hit.raycastTarget=true;var button=header.gameObject.AddComponent<Button>();
                Action toggle=()=>{expanded=!expanded;Put("expanded",expanded?1:0);SetState(expanded?"展开 · 下方条目让位":"收起 · 内容停止接收输入");float h=container.sizeDelta.y,a0=RecipeUI.Group(content).alpha;var from=footer.anchoredPosition;Transition(p=>{float t=Reduced?1:Ease(p);float height=Mathf.Lerp(h,expanded?full:small,t);container.sizeDelta=new Vector2(590,height);RecipeUI.Alpha(content,Mathf.Lerp(a0,expanded?1:0,t));footer.anchoredPosition=Vector2.Lerp(from,new Vector2(0,180-(expanded?full:small)-45),t);});};button.onClick.AddListener(()=>toggle());Operation("切换展开",toggle);return;
            }
            if(n==3){rows.gameObject.SetActive(false);var text=RecipeUI.Label(content,"探索星光群岛，收集沿途水晶。",0,-40,490,70,22,RecipeUI.Ink,TextAnchor.UpperLeft);text.rectTransform.anchorMin=text.rectTransform.anchorMax=text.rectTransform.pivot=new Vector2(.5f,1);text.rectTransform.anchoredPosition=new Vector2(0,-10);container.sizeDelta=new Vector2(590,190);footer.anchoredPosition=new Vector2(0,-55);
                Operation("切换长短内容",()=>{Put("long",1-Get("long"));bool longer=Get("long")==1;text.text=longer?"探索星光群岛，收集沿途水晶。\n完成三次探索后返回营地领取补给。\n任务进度会被保留。\n奖励：金币 ×60，经验 ×150。":"探索星光群岛，收集沿途水晶。";text.rectTransform.sizeDelta=new Vector2(490,longer?160:70);float target=Mathf.Max(190,text.preferredHeight+125),from=container.sizeDelta.y;var position=footer.anchoredPosition;SetState("内容变化 · 面板包住实际文字");Transition(p=>{float t=Reduced?1:Ease(p);container.sizeDelta=new Vector2(590,Mathf.Lerp(from,target,t));footer.anchoredPosition=Vector2.Lerp(position,new Vector2(0,180-target-45),t);});});
            }
            if(n==4){rows.gameObject.SetActive(false);
                var leftText=RecipeUI.Label(content,"任务清单\n\n拖动竖条\n松开吸附至三档",-140,-83,220,150,21,RecipeUI.Muted);leftText.rectTransform.anchorMin=leftText.rectTransform.anchorMax=new Vector2(.5f,1);leftText.rectTransform.anchoredPosition=new Vector2(-140,-83);
                var rightText=RecipeUI.Label(content,"奖励详情",140,-30,220,42,23);rightText.rectTransform.anchorMin=rightText.rectTransform.anchorMax=new Vector2(.5f,1);rightText.rectTransform.anchoredPosition=new Vector2(140,-30);
                var chest=Art(content,"Preview reward","chest-body",140,-115,95,95);chest.rectTransform.anchorMin=chest.rectTransform.anchorMax=new Vector2(.5f,1);chest.rectTransform.anchoredPosition=new Vector2(140,-115);
                var divider=RecipeUI.Rect(container,"Divider",0,-175,44,164);divider.anchorMin=divider.anchorMax=new Vector2(.5f,1);divider.anchoredPosition=new Vector2(0,-175);var hit=divider.gameObject.AddComponent<Image>();hit.color=new Color(1,1,1,.01f);hit.raycastTarget=true;
                var line=RecipeUI.Rect(divider,"Divider rail",0,0,5,164).gameObject.AddComponent<Image>();line.color=RecipeUI.Blue;line.raycastTarget=false;Art(divider,"Divider grip","wb-handle",0,0,27,27);
                var pointer=divider.gameObject.AddComponent<RecipePointer>();Vector2 grab=Vector2.zero;pointer.down=e=>{StopMotion();RectTransformUtility.ScreenPointToLocalPointInRectangle(container,e.position,e.pressEventCamera,out var p0);grab=p0-divider.anchoredPosition;};pointer.drag=e=>{RectTransformUtility.ScreenPointToLocalPointInRectangle(container,e.position,e.pressEventCamera,out var p0);divider.anchoredPosition=new Vector2(Mathf.Clamp(p0.x-grab.x,-160,160),-175);SetState("分隔条跟手");};Action settle=()=>{float target=Mathf.Round(divider.anchoredPosition.x/160)*160;Put("slot",Mathf.RoundToInt(target/160));SetState("已吸附 · 档位 "+Get("slot"));Move(divider,new Vector2(target,-175));};pointer.up=e=>settle();pointer.end=e=>settle();pointer.cancel=settle;Operation("吸附右档",()=>{divider.anchoredPosition=new Vector2(125,-175);settle();});
            }
            if(n==5){var outline=container.gameObject.AddComponent<Outline>();outline.effectDistance=new Vector2(3,-3);outline.effectColor=new Color(.1f,.85f,1,0);Operation("更新任务",()=>{Put("updates",Get("updates")+1);rows.GetComponentsInChildren<Text>()[0].text="探索星光森林  已完成";SetState("任务已更新 · "+Get("updates"));Transition(p=>outline.effectColor=new Color(.1f,.85f,1,Reduced?.6f:Mathf.Sin(p*Mathf.PI)));});}
            if(n==6){var other=tasks(content,new[]{"抵达雪山营地  1/1","击败霜狼首领  0/1","领取保暖装备"});RecipeUI.Alpha(other,0);Operation("替换内容",()=>{Put("selected",1-Get("selected"));SetState(Get("selected")==1?"雪山任务 · 内容乙":"森林任务 · 内容甲");FadeSwap(rows,other);});}
            if(n==7){RecipeUI.Alpha(rows,0);var empty=RecipeUI.Label(content,"暂无探索任务\n点击下方加载，观察真实条目进入",0,-80,490,110,22,RecipeUI.Muted);empty.rectTransform.anchorMin=empty.rectTransform.anchorMax=new Vector2(.5f,1);empty.rectTransform.anchoredPosition=new Vector2(0,-80);Operation("加载／清空",()=>{Put("count",Get("count")==0?3:0);bool loaded=Get("count")>0;taskCount.text=loaded?"3 项":"0 项";SetState(loaded?"已加载 3 项任务":"空列表");float start=RecipeUI.Group(rows).alpha;Transition(p=>{float t=Reduced?1:Ease(p);RecipeUI.Alpha(rows,Mathf.Lerp(start,loaded?1:0,t));RecipeUI.Alpha(empty.rectTransform,Mathf.Lerp(1-start,loaded?0:1,t));});});}
        }
        void BuildNavigation() {
            int n=Definition.Number;var panel=Panel("旅程导航",0,0,660,300);var body=Caption(panel,"当前内容：星光森林",0,-45,540);var indicator=RecipeUI.Box(panel,"Selection indicator",-190,30,150,4,RecipeUI.Gold).rectTransform;
            var tabs=new Button[3];string[] names=n==4?new[]{"选关","准备","出发"}:n==6?new[]{"第 1 页","第 2 页","第 3 页"}:new[]{"森林","海湾","雪山"};
            Action<int> select=i=>{
                int previous=Get("selected");Put("selected",i);Put("identity",100+i);SetState("选中 "+names[i]);
                body.text=n==6?"第 "+(i+1)+" 页 · 任务 "+(i*3+1)+"–"+(i*3+3):n==7?(i==0?"全部物品：普通、稀有、史诗":"稀有物品：水晶、宝冠"):"当前内容："+names[i];
                Vector2 from=indicator.anchoredPosition;float alpha=RecipeUI.Group(body.rectTransform).alpha;
                if(RecipeArt.Available)for(int k=0;k<tabs.Length;k++){RecipeArt.Apply(tabs[k].GetComponent<Image>(),k==i?"tab-selected":"tab");tabs[k].GetComponentInChildren<Text>().color=k==i?RecipeUI.Hex("572C10"):RecipeUI.Ink;}
                Transition(p=>{float t=Reduced?1:Ease(p);indicator.anchoredPosition=Vector2.Lerp(from,new Vector2(-190+i*190,30),t);if(n==3||n==7)RecipeUI.Group(body.rectTransform).alpha=Mathf.Lerp(alpha*.2f,1,t);});
            };
            for(int i=0;i<3;i++){int index=i;tabs[i]=RecipeUI.Button(panel,n==7?(i==0?"全部":i==1?"稀有":"史诗"):names[i],-190+i*190,60,150,()=>select(index),RecipeUI.Blue);if(RecipeArt.Available){RecipeArt.Apply(tabs[i].GetComponent<Image>(),i==0?"tab-selected":"tab");tabs[i].GetComponentInChildren<Text>().color=i==0?RecipeUI.Hex("572C10"):RecipeUI.Ink;}}
            if(n==7)body.text="全部物品：普通、稀有、史诗";
            if(n==1||n==3||n==7){Operation("选择下一项",()=>select((Get("selected")+1)%3));}
            if(n==2){
                body.text="点击任意按钮，再按 Tab 或方向键";int focus=0;
                if(RecipeArt.Available)RecipeArt.Apply(indicator.GetComponent<Image>(),"focus-outline",false);indicator.GetComponent<Image>().preserveAspect=false;indicator.sizeDelta=new Vector2(172,65);indicator.anchoredPosition=new Vector2(-190,60);indicator.SetAsLastSibling();
                for(int i=0;i<3;i++){int index=i;tabs[i].onClick.RemoveAllListeners();tabs[i].onClick.AddListener(()=>EventSystem.current.SetSelectedGameObject(tabs[index].gameObject));var navigation=tabs[i].navigation;navigation.mode=Navigation.Mode.Explicit;navigation.selectOnLeft=tabs[(i+2)%3];navigation.selectOnRight=tabs[(i+1)%3];navigation.selectOnUp=tabs[(i+2)%3];navigation.selectOnDown=tabs[(i+1)%3];tabs[i].navigation=navigation;var hook=tabs[i].gameObject.AddComponent<RecipeFocus>();hook.selected=()=>{focus=index;Put("focus",index);SetState("键盘焦点："+names[index]);Move(indicator,new Vector2(-190+index*190,60));};}
                ticking.Add(()=>{if(Input.GetKeyDown(KeyCode.Tab)){focus=(focus+1)%3;EventSystem.current.SetSelectedGameObject(tabs[focus].gameObject);}});
                Operation("移动焦点",()=>{focus=(focus+1)%3;EventSystem.current.SetSelectedGameObject(tabs[focus].gameObject);});
            }
            if(n==4){foreach(var b in tabs)b.interactable=false;indicator.sizeDelta=new Vector2(0,6);indicator.anchoredPosition=new Vector2(-190,30);indicator.pivot=new Vector2(0,.5f);Operation("完成当前步骤",()=>{Put("step",Mathf.Min(2,Get("step")+1));int step=Get("step");body.text="实际已完成步骤 "+step;SetState("步骤 "+step+" 已完成");float width=indicator.sizeDelta.x;Transition(p=>indicator.sizeDelta=new Vector2(Mathf.Lerp(width,step*190,Reduced?1:Ease(p)),6));});}
            if(n==5){foreach(var b in tabs)b.gameObject.SetActive(false);indicator.gameObject.SetActive(false);Operation("进入下一级",()=>{Put("depth",Mathf.Min(3,Get("depth")+1));body.text="首页"+new string('/',Get("depth"))+" 当前层级 "+Get("depth");SetState("路径深度 "+Get("depth"));Transition(p=>RecipeUI.Alpha(body.rectTransform,Reduced?1:Ease(p)));});Operation("返回",()=>{Put("depth",Mathf.Max(0,Get("depth")-1));body.text="首页"+new string('/',Get("depth"))+" 当前层级 "+Get("depth");SetState("路径深度 "+Get("depth"));});}
            if(n==6){body.text="第 1 页 · 任务 1–3";Operation("上一页",()=>select(Mathf.Max(0,Get("selected")-1)));Operation("下一页",()=>select(Mathf.Min(2,Get("selected")+1)));}
            if(n==8){SetState("选中 "+names[0]);body.text="选中项具有独立光环";if(RecipeArt.Available)RecipeArt.Apply(indicator.GetComponent<Image>(),"selection-outline",false);indicator.GetComponent<Image>().preserveAspect=false;indicator.sizeDelta=new Vector2(170,60);indicator.anchoredPosition=new Vector2(-190,60);
                Action<int> selectGlow=i=>{Put("selected",i);SetState("选中 "+names[i]);indicator.anchoredPosition=new Vector2(-190+i*190,60);for(int k=0;k<3;k++)if(RecipeArt.Available){RecipeArt.Apply(tabs[k].GetComponent<Image>(),k==i?"tab-selected":"tab");tabs[k].GetComponentInChildren<Text>().color=k==i?RecipeUI.Hex("572C10"):RecipeUI.Ink;}Transition(p=>RecipeUI.Alpha(indicator,Reduced?1:.5f+.5f*Mathf.Sin(p*Mathf.PI*2)),true);};
                for(int i=0;i<3;i++){int k=i;tabs[i].onClick.RemoveAllListeners();tabs[i].onClick.AddListener(()=>selectGlow(k));}Operation("选择下一项",()=>selectGlow((Get("selected")+1)%3));indicator.SetAsLastSibling();}

        }
    }
    public sealed class RecipeHover : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler {public Action enter,exit;public void OnPointerEnter(PointerEventData e)=>enter?.Invoke();public void OnPointerExit(PointerEventData e)=>exit?.Invoke();void OnDisable()=>exit?.Invoke();}
    public sealed class RecipeFocus : MonoBehaviour,ISelectHandler {public Action selected;public void OnSelect(BaseEventData e)=>selected?.Invoke();}
}
