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
                    RecipeUI.Button(card,"查看",0,0,100,()=>Trigger("展开卡片"),RecipeUI.Blue);
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
        void BuildContainer() {
            int n=Definition.Number;var container=Panel(n==8?"冒险卡牌":"任务容器",0,0,430,250);
            var body=Caption(container,"每日任务 · 完成 3 次探索",0,20);var footer=RecipeUI.Box(world,"Next row",0,-155,430,45,RecipeUI.Surface).rectTransform;Caption(footer,"下一个布局条目",0,0);
            if(n==1||n==2){
                bool expanded=n==2;float height=expanded?250:90;container.sizeDelta=new Vector2(430,height);RecipeUI.Alpha(body.rectTransform,expanded?1:0);
                container.GetComponentInChildren<Text>().gameObject.SetActive(false);
                var header=RecipeUI.Button(container,"任务详情 ▾",0,0,360,null,RecipeUI.Blue);var headerRect=(RectTransform)header.transform;headerRect.anchorMin=headerRect.anchorMax=new Vector2(.5f,1);headerRect.anchoredPosition=new Vector2(0,-28);body.rectTransform.anchoredPosition=new Vector2(0,-20);
                Action toggle=()=>{expanded=!expanded;Put("expanded",expanded?1:0);SetState(expanded?"展开 · 下方条目让位":"折叠 · 内部输入关闭");float from=container.sizeDelta.y;var position=footer.anchoredPosition;Transition(p=>{float t=Reduced?1:Ease(p);container.sizeDelta=new Vector2(430,Mathf.Lerp(from,expanded?250:90,t));RecipeUI.Alpha(body.rectTransform,expanded?t:1-t);footer.anchoredPosition=Vector2.Lerp(position,new Vector2(0,expanded?-155:-75),t);});};
                Operation("切换展开",toggle);header.onClick.AddListener(()=>toggle());return;
            }
            if(n==3){body.rectTransform.anchoredPosition=new Vector2(0,-30);Operation("切换长短内容",()=>{Put("long",1-Get("long"));bool longer=Get("long")==1;body.text=longer?"探索星光群岛，收集沿途水晶。\n完成三次探索后返回营地领取补给。\n任务进度会被保留。":"每日任务 · 完成 3 次探索";body.rectTransform.sizeDelta=new Vector2(380,longer?125:50);float from=container.sizeDelta.y;float target=Mathf.Max(150,body.preferredHeight+135);SetState("内容实测高度 "+Mathf.RoundToInt(target));Transition(p=>container.sizeDelta=new Vector2(430,Mathf.Lerp(from,target,Reduced?1:Ease(p))));});}
            if(n==4){
                body.text="拖动分隔条，释放吸附到三档";var divider=RecipeUI.Box(container,"Divider",0,-40,12,85,RecipeUI.Orange).rectTransform;divider.GetComponent<Image>().raycastTarget=true;
                var pointer=divider.gameObject.AddComponent<RecipePointer>();Vector2 grab=Vector2.zero;
                pointer.down=e=>{StopMotion();RectTransformUtility.ScreenPointToLocalPointInRectangle(container,e.position,e.pressEventCamera,out var p);grab=p-divider.anchoredPosition;};
                pointer.drag=e=>{RectTransformUtility.ScreenPointToLocalPointInRectangle(container,e.position,e.pressEventCamera,out var p);divider.anchoredPosition=new Vector2(Mathf.Clamp(p.x-grab.x,-150,150),-40);SetState("分隔条跟手");};
                Action settle=()=>{float target=Mathf.Round(divider.anchoredPosition.x/150)*150;Put("slot",Mathf.RoundToInt(target/150));SetState("吸附档位 "+Get("slot"));Move(divider,new Vector2(target,-40));};pointer.up=e=>settle();pointer.end=e=>settle();pointer.cancel=settle;Operation("吸附右档",()=>{divider.anchoredPosition=new Vector2(112,-40);settle();});
            }
            if(n==5){var outline=RecipeUI.Box(container,"Emphasis",0,0,445,265,RecipeUI.Gold).rectTransform;if(RecipeArt.Available)RecipeArt.Apply(outline.GetComponent<Image>(),"focus-outline",false);outline.SetAsLastSibling();RecipeUI.Alpha(outline,0);Operation("更新任务",()=>{Put("updates",Get("updates")+1);body.text="任务已更新 · "+Get("updates");SetState("真实任务状态已更新");Transition(p=>RecipeUI.Alpha(outline,Reduced?.65f:Mathf.Sin(p*Mathf.PI)));});}
            if(n==6){var b=Caption(container,"新内容：雪山营地",0,20).rectTransform;RecipeUI.Alpha(b,0);Operation("替换内容",()=>{Put("selected",1-Get("selected"));SetState("容器内容 "+(Get("selected")==1?"乙":"甲"));FadeSwap(body.rectTransform,b);});}
            if(n==7){body.text="暂无任务";body.rectTransform.sizeDelta=new Vector2(360,125);body.rectTransform.anchoredPosition=new Vector2(0,-20);Operation("加载／清空",()=>{Put("count",Get("count")==0?3:0);SetState(Get("count")==0?"空列表":"已加载 3 项任务");body.text=Get("count")==0?"暂无任务":"① 探索森林\n② 收集水晶\n③ 返回营地";Transition(p=>RecipeUI.Alpha(body.rectTransform,Reduced?1:Ease(p)));});}
            if(n==8){
                body.gameObject.SetActive(false);var card=RecipeUI.Card(container,"星光旅者",0,-15,160,185);var back=RecipeUI.Box(card,"Card back",0,0,160,185,RecipeUI.Pink).rectTransform;if(!RecipeArt.Available)Caption(back,"✦\n冒险档案",0,0,140);
                RecipeUI.Button(card,"翻面",0,-65,110,()=>Trigger("翻面"),RecipeUI.Orange);
                Operation("翻面",()=>{Put("face",1-Get("face"));int face=Get("face");SetState(face==1?"正面 · 星光旅者":"背面 · 冒险档案");Transition(p=>{float t=Reduced?1:p;card.localScale=new Vector3(Reduced?1:Mathf.Max(.02f,Mathf.Abs(t*2-1)),1,1);back.gameObject.SetActive(p<.5f?face==1:face==0);});});
            }
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
            if(n==1||n==3||n==7){Operation("选择下一项",()=>select((Get("selected")+1)%3));}
            if(n==2){
                body.text="点击任意按钮，再按 Tab 或方向键";int focus=0;
                if(RecipeArt.Available)RecipeArt.Apply(indicator.GetComponent<Image>(),"focus-outline",false);indicator.sizeDelta=new Vector2(172,65);indicator.anchoredPosition=new Vector2(-190,60);indicator.SetAsLastSibling();
                for(int i=0;i<3;i++){int index=i;var hook=tabs[i].gameObject.AddComponent<RecipeFocus>();hook.selected=()=>{Put("focus",index);SetState("键盘焦点："+names[index]);Move(indicator,new Vector2(-190+index*190,60));};}
                ticking.Add(()=>{if(Input.GetKeyDown(KeyCode.Tab)){focus=(focus+1)%3;EventSystem.current.SetSelectedGameObject(tabs[focus].gameObject);}});
                Operation("移动焦点",()=>{focus=(focus+1)%3;EventSystem.current.SetSelectedGameObject(tabs[focus].gameObject);});
            }
            if(n==4){foreach(var b in tabs)b.interactable=false;indicator.sizeDelta=new Vector2(0,6);indicator.anchoredPosition=new Vector2(-190,30);indicator.pivot=new Vector2(0,.5f);Operation("完成当前步骤",()=>{Put("step",Mathf.Min(2,Get("step")+1));int step=Get("step");body.text="实际已完成步骤 "+step;SetState("步骤 "+step+" 已完成");float width=indicator.sizeDelta.x;Transition(p=>indicator.sizeDelta=new Vector2(Mathf.Lerp(width,step*190,Reduced?1:Ease(p)),6));});}
            if(n==5){foreach(var b in tabs)b.gameObject.SetActive(false);indicator.gameObject.SetActive(false);Operation("进入下一级",()=>{Put("depth",Mathf.Min(3,Get("depth")+1));body.text="首页"+new string('/',Get("depth"))+" 当前层级 "+Get("depth");SetState("路径深度 "+Get("depth"));Transition(p=>RecipeUI.Alpha(body.rectTransform,Reduced?1:Ease(p)));});Operation("返回",()=>{Put("depth",Mathf.Max(0,Get("depth")-1));body.text="首页"+new string('/',Get("depth"))+" 当前层级 "+Get("depth");SetState("路径深度 "+Get("depth"));});}
            if(n==6){body.text="第 1 页 · 任务 1–3";Operation("上一页",()=>select(Mathf.Max(0,Get("selected")-1)));Operation("下一页",()=>select(Mathf.Min(2,Get("selected")+1)));}
            if(n==8){body.text="选中项具有独立光环";if(RecipeArt.Available)RecipeArt.Apply(indicator.GetComponent<Image>(),"selection-outline",false);indicator.sizeDelta=new Vector2(170,60);indicator.anchoredPosition=new Vector2(-190,60);Operation("选择下一项",()=>{Put("selected",(Get("selected")+1)%3);int selected=Get("selected");SetState("选中 "+names[selected]);indicator.anchoredPosition=new Vector2(-190+selected*190,60);Transition(p=>RecipeUI.Alpha(indicator,Reduced?1:.5f+.5f*Mathf.Sin(p*Mathf.PI*2)),true);});indicator.SetAsLastSibling();}
        }
    }
    public sealed class RecipeHover : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler {public Action enter,exit;public void OnPointerEnter(PointerEventData e)=>enter?.Invoke();public void OnPointerExit(PointerEventData e)=>exit?.Invoke();void OnDisable()=>exit?.Invoke();}
    public sealed class RecipeFocus : MonoBehaviour,ISelectHandler {public Action selected;public void OnSelect(BaseEventData e)=>selected?.Invoke();}
}
