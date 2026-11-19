using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TweenGallery.Recipes
{
    public sealed partial class RecipeStage
    {
        RectTransform Bar(Transform parent,float y,Color color,float fraction=1) {
            var track=RecipeUI.Box(parent,"Bar track",0,y,420,28,RecipeUI.Dark).rectTransform;
            var fill=RecipeUI.Box(track,"Bar fill",-205,0,410*fraction,18,color).rectTransform;fill.pivot=new Vector2(0,.5f);if(RecipeArt.Available)RecipeArt.Apply(fill.GetComponent<Image>(),color==RecipeUI.Gold?"bar-damage":color==RecipeUI.Blue?"bar-xp":"bar-gold");return fill;
        }
        RectTransform HudStrip(Transform parent,string key,float x,float y,float width,float fraction=1){
            var track=Art(parent,"HUD track","bar-track",x,y,width,42,true).rectTransform;
            var fill=Art(track,"HUD fill",key,-width*.5f+6,0,(width-12)*fraction,28,true).rectTransform;fill.pivot=new Vector2(0,.5f);return fill;
        }
        void BuildValue(){
            int n=Definition.Number;var hud=RecipeUI.Rect(world,"Value HUD",0,10,710,290);PrimarySubject=hud;
            var title=RecipeUI.Label(hud,n==1?"累计战力":n==2?"探险金币":n==3||n==4?"探索队长 · 生命":n==5?"冒险者经验":n==6?"下一批补给":n==7?"正在连接营地":"补给准备进度",0,118,670,45,28);
            var label=RecipeUI.Label(hud,"100",30,35,590,110,54);
            float shown=n==4?30:100;Put("value",Mathf.RoundToInt(shown));Put("target",Get("value"));
            Action<int> chase=target=>{float from=shown;Put("target",target);Put("value",target);SetState("实际目标 "+target+" · 显示数值追逐中");Transition(p=>{shown=Mathf.Lerp(from,target,Reduced?1:Ease(p));label.text=Mathf.RoundToInt(shown).ToString("N0");});};
            if(n==1||n==2){Art(hud,"Value emblem",n==2?"coin":"xp",-260,25,125,135);label.rectTransform.anchoredPosition=new Vector2(75,30);label.rectTransform.sizeDelta=new Vector2(425,90);label.color=n==2?RecipeUI.Gold:RecipeUI.Ink;RecipeUI.Label(hud,"连续操作，观察显示值从当前位置追到最新目标",0,-98,680,55,21,RecipeUI.Muted);
                if(n==1){Operation("目标 +100",()=>chase(Get("target")+100));Operation("目标 -50",()=>chase(Mathf.Max(0,Get("target")-50)));}
                else{Operation("收入 +100",()=>chase(Get("value")+100));Operation("支出 -80",()=>{if(Get("value")<80){Put("rejected",Get("rejected")+1);SetState("余额不足 · 拒绝支出");}else chase(Get("value")-80);});}
            }
            if(n==3||n==4){int hp=n==3?100:30;Put("value",hp);Art(hud,"Explorer portrait","ref-thumbnail",-280,9,112,130);label.rectTransform.anchoredPosition=new Vector2(65,55);label.rectTransform.sizeDelta=new Vector2(510,70);label.fontSize=42;label.text="生命 "+hp+" / 100";
                var trail=HudStrip(hud,"bar-damage",52,-12,525,hp/100f);var fill=Art(trail.parent,"Current health","bar-health",-256.5f,0,513*hp/100f,28,true).rectTransform;fill.pivot=new Vector2(0,.5f);
                RecipeUI.Label(hud,n==3?"红条立即扣血，金色拖尾随后追上":"治疗先更新实际生命，再填充血条",45,-105,600,58,21,RecipeUI.Muted);
                Operation(n==3?"受到 25 伤害":"治疗 +25",()=>{hp=Mathf.Clamp(hp+(n==3?-25:25),0,100);Put("value",hp);label.text="生命 "+hp+" / 100";SetState((n==3?"受伤":"治疗")+" · 当前生命 "+hp);float f=fill.sizeDelta.x,t0=trail.sizeDelta.x,target=513*hp/100f;if(n==3)fill.sizeDelta=new Vector2(target,28);Transition(p=>{float t=Reduced?1:Ease(p);if(n==4)fill.sizeDelta=new Vector2(Mathf.Lerp(f,target,t),28);trail.sizeDelta=new Vector2(Mathf.Lerp(t0,target,Reduced?1:Mathf.Clamp01((p-.25f)/.75f)),28);});});
            }
            if(n==5){Put("level",1);Put("xp",70);label.text="Lv.1    70 / 100";var fill=HudStrip(hud,"bar-xp",0,-35,640,.7f);RecipeUI.Label(hud,"跨级后继续填充，不丢失剩余经验",0,-108,670,45,21,RecipeUI.Muted);Operation("获得 150 经验",()=>{int oldLevel=Get("level"),oldXp=Get("xp"),total=oldXp+150;Put("level",oldLevel+total/100);Put("xp",total%100);SetState("实际等级 "+Get("level")+" · 剩余经验 "+Get("xp"));Transition(p=>{float amount=Reduced?total:Mathf.Lerp(oldXp,total,p);int lv=oldLevel+Mathf.FloorToInt(amount/100);float fraction=Mathf.Repeat(amount,100)/100;label.text="Lv."+lv+"    "+Mathf.FloorToInt(fraction*100)+" / 100";fill.sizeDelta=new Vector2(628*fraction,28);});});}
            if(n==6){double deadline=0;bool active=false;label.rectTransform.sizeDelta=new Vector2(620,140);label.rectTransform.anchoredPosition=new Vector2(0,25);label.fontSize=76;label.text="10 秒";RecipeUI.Label(hud,"剩余 3 秒进入临界提示；以真实时间为准",0,-97,680,55,21,RecipeUI.Muted);Operation("开始 10 秒",()=>{deadline=Time.realtimeSinceStartupAsDouble+10;active=true;Put("running",1);SetState("倒计时进行中");});ticking.Add(()=>{if(!active)return;float remaining=(float)Math.Max(0,deadline-Time.realtimeSinceStartupAsDouble);label.text=Mathf.CeilToInt(remaining)+" 秒";label.color=remaining<=3?RecipeUI.Pink:RecipeUI.Ink;label.transform.localScale=Vector3.one*(remaining<=3&&!Reduced?1+Mathf.Sin(remaining*Mathf.PI*2)*.035f:1);Put("remaining",Mathf.CeilToInt(remaining));if(remaining<=0){active=false;Put("running",0);SetState("倒计时结束");}else if(remaining<=3)SetState("临界倒计时 · "+Mathf.CeilToInt(remaining)+" 秒");});}
            if(n==7){label.rectTransform.anchoredPosition=new Vector2(0,-105);label.fontSize=28;label.text="等待开始，不显示虚构百分比";var ring=Art(hud,"Directional spinner","spinner",0,13,130,130).rectTransform;Operation("开始加载",()=>{Put("loading",1);label.text="连接中…";SetState("加载中 · 等待真实结果");Transition(p=>ring.localRotation=Quaternion.Euler(0,0,Reduced?0:-360*p),true);});Operation("完成",()=>{StopMotion();Put("loading",0);Put("success",1);label.text="连接成功";SetState("Success · 连接成功");});Operation("失败",()=>{StopMotion();Put("loading",0);Put("success",0);label.text="连接失败，可以重试";SetState("Failed · 连接失败");});}
            if(n==8){Put("tasks",0);label.text="0 / 4";var fill=HudStrip(hud,"bar-gold",0,-35,640,0);RecipeUI.Label(hud,"每次完成一项真实任务，进度随完成数增长",0,-108,680,45,21,RecipeUI.Muted);Operation("完成一个真实任务",()=>{int tasks=Mathf.Min(4,Get("tasks")+1);Put("tasks",tasks);Put("complete",tasks==4?1:0);SetState(tasks==4?"全部任务已完成":"已完成 "+tasks+" / 4");label.text=tasks+" / 4";float from=fill.sizeDelta.x;Transition(p=>fill.sizeDelta=new Vector2(Mathf.Lerp(from,628*tasks/4f,Reduced?1:Ease(p)),28));});}
        }
        void BuildNotice() {
            int n=Definition.Number;var panel=RecipeUI.Rect(world,"Feedback scene",0,0,700,330);PrimarySubject=panel;var message=RecipeUI.Label(panel,"等待操作",0,80,670,80,26);var feedback=RecipeUI.Box(panel,"Feedback icon",0,-35,110,110,RecipeUI.Blue,"circle").rectTransform;var mark=RecipeUI.Label(feedback,"i",0,0,105,70,36);
            if(RecipeArt.Available)mark.text="";
            if(n==1){
                var queue=new Queue<string>();string active=null;float elapsed=0;bool showing=false;int serial=0;var toast=Art(panel,"Toast capsule","toast",0,30,620,115,true).rectTransform;message.transform.SetParent(toast,false);message.rectTransform.anchoredPosition=new Vector2(25,0);message.rectTransform.sizeDelta=new Vector2(500,90);feedback.SetParent(toast,false);feedback.anchoredPosition=new Vector2(-250,0);feedback.sizeDelta=new Vector2(60,60);RecipeUI.Alpha(toast,0);
                Action<string> send=text=>{if(active==text||queue.Contains(text)){Put("merged",Get("merged")+1);SetState("相同消息已合并");return;}if(queue.Count>=3){queue.Dequeue();Put("dropped",Get("dropped")+1);}queue.Enqueue(text);Put("queued",queue.Count);SetState("有界队列 "+queue.Count+" / 3");};
                Operation("发送新消息",()=>send("补给已送达 #"+(++serial)));Operation("发送重复消息",()=>send("每日补给已送达"));
                ticking.Add(()=>{if(!showing&&queue.Count>0){active=queue.Dequeue();Put("queued",queue.Count);message.text=active;elapsed=0;showing=true;SetState("通知显示中 · 等待队列 "+queue.Count);}if(!showing)return;if(!Paused)elapsed+=Time.unscaledDeltaTime*Speed;float a=elapsed<.2f?elapsed/.2f:elapsed<1.5f?1:1-(elapsed-1.5f)/.2f;RecipeUI.Alpha(toast,Mathf.Clamp01(a));if(elapsed>=1.7f){showing=false;active=null;SetState(queue.Count>0?"等待下一条通知":"无消息");}});
            }
            if(n==2||n==3||n==4){Operation(n==2?"模拟成功":n==3?"模拟失败":"检查资源",()=>{Put("requests",Get("requests")+1);Put("result",n==2?1:-1);message.text=n==2?"✓ 操作成功，结果已确认":n==3?"! 网络不可用，请修正后重试":"! 资源不足，需要 50 金币";if(RecipeArt.Available){RecipeArt.Apply(feedback.GetComponent<Image>(),n==2?"notice-success":n==3?"notice-error":"notice-warning",false);mark.text="";}else{mark.text=n==2?"✓":"!";feedback.GetComponent<Image>().color=n==2?RecipeUI.Green:RecipeUI.Pink;}SetState(n==2?"Success":n==3?"Error · 可重试":"Warning · 静态原因保留");var start=feedback.anchoredPosition;Transition(p=>{feedback.anchoredPosition=start+Vector2.right*(Reduced||n==2?0:Mathf.Sin(p*Mathf.PI*8)*(1-p)*10);feedback.localScale=Vector3.one*(1+(Reduced?0:Mathf.Sin(p*Mathf.PI)*.12f));});});}
            if(n==5){mark.text="0";if(RecipeArt.Available)RecipeArt.Apply(feedback.GetComponent<Image>(),"badge",false);Operation("增加未读",()=>{Put("unread",Get("unread")+1);message.text="未读消息 "+Get("unread");mark.text=Get("unread").ToString();if(RecipeArt.Available)RecipeArt.Apply(feedback.GetComponent<Image>(),"badge",false);SetState("未读数据 "+Get("unread"));Transition(p=>feedback.localScale=Vector3.one*(1+(Reduced?0:Mathf.Sin(p*Mathf.PI)*.2f)));});Operation("清空未读",()=>{Put("unread",0);message.text="未读消息 0";mark.text="0";SetState("未读已清空");});}
            if(n==6){
                int request=0;Action start=()=>{request++;Put("request",request);Put("loading",1);message.text="请求 #"+request+" 加载中…";mark.text=RecipeArt.Available?"":"↻";if(RecipeArt.Available)RecipeArt.Apply(feedback.GetComponent<Image>(),"spinner",false);SetState("Loading #"+request);Transition(p=>feedback.localRotation=Quaternion.Euler(0,0,Reduced?0:-360*p),true);};
                Operation("请求／重试",start);Operation("请求失败",()=>{if(Get("loading")==0)return;StopMotion();Put("loading",0);message.text="! 请求失败，请点击重试";SetState("Failed · 显示重试入口");});
                Operation("当前请求成功",()=>{if(Get("loading")==0)return;StopMotion();Put("loading",0);Put("completedRequest",request);message.text="✓ 请求 #"+request+" 完成";SetState("Success #"+request);});
                Operation("旧响应返回",()=>{int old=request-1;if(old!=Get("request")){Put("staleIgnored",Get("staleIgnored")+1);SetState("已忽略旧响应 #"+old);}});
            }
            if(n==7){
                var target=RecipeUI.Button(panel,"真实目标按钮",0,-60,330,()=>{if(Get("guiding")==0)return;Put("guiding",0);Put("done",1);StopMotion();RecipeUI.Alpha(feedback,0);message.text="✓ 真实操作完成教学";SetState("教学完成");},RecipeUI.Blue,74);target.GetComponentInChildren<Text>().fontSize=24;feedback.sizeDelta=new Vector2(78,88);message.rectTransform.anchoredPosition=new Vector2(0,160);feedback.anchoredPosition=new Vector2(0,35);if(RecipeArt.Available){RecipeArt.Apply(feedback.GetComponent<Image>(),"arrow",false);mark.text="";}else mark.text="↓";
                Operation("开始引导",()=>{Put("guiding",1);Put("done",0);message.text="点击下方目标按钮";SetState("教学进行中");RecipeUI.Alpha(feedback,1);Transition(p=>feedback.anchoredPosition=new Vector2(0,35+(Reduced?0:Mathf.Sin(p*Mathf.PI*2)*8)),true);});
                Operation("跳过引导",()=>{Put("guiding",0);StopMotion();RecipeUI.Alpha(feedback,0);message.text="已跳过引导";SetState("教学已跳过");});
                Operation("操作目标",()=>target.onClick.Invoke());
            }
            if(n==8){
                message.text="拖动晶石，观察气泡保持锚定";message.rectTransform.anchoredPosition=new Vector2(0,160);if(RecipeArt.Available){RecipeArt.Apply(feedback.GetComponent<Image>(),"tooltip");feedback.sizeDelta=new Vector2(230,80);mark.rectTransform.sizeDelta=new Vector2(210,65);mark.fontSize=23;mark.text="锚定提示";mark.color=RecipeUI.Ink;}var target=RecipeUI.Box(panel,"Anchor target",0,-65,180,140,RecipeUI.Orange).rectTransform;RecipeArt.Apply(target.GetComponent<Image>(),"button-square",false);Art(target,"Tracked crystal","crystal",0,0,90,110);target.GetComponent<Image>().raycastTarget=true;var pointer=target.gameObject.AddComponent<RecipePointer>();Vector2 offset=Vector2.zero;
                pointer.down=e=>{RectTransformUtility.ScreenPointToLocalPointInRectangle(panel,e.position,e.pressEventCamera,out var p);offset=p-target.anchoredPosition;};pointer.drag=e=>{RectTransformUtility.ScreenPointToLocalPointInRectangle(panel,e.position,e.pressEventCamera,out var p);target.anchoredPosition=p-offset;Put("moved",1);};
                ticking.Add(()=>{bool visible=target.gameObject.activeSelf;feedback.gameObject.SetActive(visible);if(visible)feedback.anchoredPosition=target.anchoredPosition+Vector2.up*130;});
                Operation("移动目标",()=>{target.anchoredPosition+=Vector2.right*60;Put("moved",1);SetState("气泡跟随实际目标");});Operation("隐藏／显示目标",()=>{target.gameObject.SetActive(!target.gameObject.activeSelf);Put("targetVisible",target.gameObject.activeSelf?1:0);SetState(target.gameObject.activeSelf?"目标可见":"目标失效 · 气泡隐藏");});
            }
        }
        void BuildCurrencyFlight(){
            Put("wallet",1000);var scene=RecipeUI.Rect(world,"Reward flight scene",0,0,740,340);PrimarySubject=scene;
            var source=Art(scene,"Reward supply","chest-body",-245,-10,155,160).rectTransform;
            RecipeUI.Label(scene,"探索奖励 · 100 金币",-220,-128,330,55,23,RecipeUI.Gold);
            var wallet=Art(scene,"Wallet HUD","wb-gray",240,130,245,72,true).rectTransform;Art(wallet,"HUD coin","coin",-85,0,43,43);var amount=RecipeUI.Label(wallet,"1,000",25,0,155,62,30,RecipeUI.Gold);
            RecipeUI.Label(scene,"金币从奖励处飞入钱包\n余额只入账一次\n取消表现不会撤销到账",215,-80,285,115,20,RecipeUI.Muted);
            var coins=new RectTransform[7];for(int i=0;i<coins.Length;i++){coins[i]=Art(scene,"Flying coin "+i,"coin",-245+(i-3)*9,10,48,48).rectTransform;RecipeUI.Alpha(coins[i],0);}
            Operation("领取 100 金币",()=>{if(Get("claimed")==0){Put("claimed",1);Put("grants",1);Put("wallet",1100);amount.text="1,100";}SetState("已到账 100 金币 · 只播放飞行表现");Transition(p=>{for(int i=0;i<coins.Length;i++){float t=Reduced?1:Mathf.Clamp01((p-i*.045f)*1.45f);coins[i].anchoredPosition=Vector2.Lerp(new Vector2(-245+(i-3)*9,10),wallet.anchoredPosition,t)+Vector2.up*Mathf.Sin(t*Mathf.PI)*110;RecipeUI.Alpha(coins[i],Reduced||t>=1?0:1);}});});
            Operation("取消飞行",()=>{StopMotion();foreach(var c in coins)RecipeUI.Alpha(c,0);SetState("表现已取消 · 已到账金币保持");});
        }
        void BuildReward() {
            if(Definition.Number==4){BuildCurrencyFlight();return;}
            int n=Definition.Number;var hud=RecipeUI.Box(world,"Wallet HUD",290,140,215,45,RecipeUI.Surface).rectTransform;var wallet=Caption(hud,"金币 1,000",0,0,210);Put("wallet",1000);Put("level",1);if(RecipeArt.Available)hud.GetComponent<Image>().pixelsPerUnitMultiplier=8;
            var center=RecipeUI.Card(world,n==3?"神秘宝箱":n==5?"成就徽章":n==6?"等级 1":"旅程奖励",0,0,160,195,n!=3);
            if(RecipeArt.Available){var header=RecipeUI.Box(world,"Reward header",-100,180,330,55,Color.white);RecipeArt.Apply(header,"header");RecipeUI.Label(header.transform,n==3?"宝箱揭晓":n==5?"成就解锁":n==6?"等级提升":n==7?"竞技排行":n==2?"本次评分":n==8?"探索结算":"探索奖励",0,0,240,35,23,RecipeUI.Hex("572C10"));}
            var status=Caption(world,"奖励由场景状态确定",0,-140,720);
            Action grant=()=>{if(Get("claimed")==1)return;Put("claimed",1);Put("wallet",Get("wallet")+100);Put("grants",Get("grants")+1);wallet.text="金币 "+Get("wallet").ToString("N0");};
            if(n==1){center.gameObject.SetActive(false);var cards=new RectTransform[3];for(int i=0;i<3;i++){cards[i]=RecipeUI.Card(world,new[]{"金币 ×100","水晶 ×5","道具 ×1"}[i],(i-1)*190,0);if(RecipeArt.Available)RecipeArt.Apply(cards[i].Find("Illustration").GetComponent<Image>(),new[]{"coin","crystal","gift"}[i],false);RecipeUI.Alpha(cards[i],0);}Operation("确认并揭示",()=>{grant();SetState("奖励已确认 · 仅表现错峰");Transition(p=>{for(int i=0;i<3;i++)RecipeUI.Alpha(cards[i],Reduced?1:Mathf.Clamp01((p-i*.16f)*2));});});Operation("跳过揭示",()=>{grant();StopMotion();foreach(var c in cards)RecipeUI.Alpha(c,1);SetState("最终奖励可见 · 发放一次");});}
            if(n==2){center.gameObject.SetActive(false);Put("rating",2);var stars=new RectTransform[3];for(int i=0;i<3;i++){stars[i]=RecipeUI.Box(world,"Rating star "+i,(i-1)*125,20,100,100,RecipeUI.Hex("4E515A"),"star").rectTransform;}
                Operation("切换评分",()=>{Put("rating",Get("rating")%3+1);status.text="真实评分 "+Get("rating")+" 星";SetState(status.text);});
                Operation("展示星级",()=>{int rating=Get("rating");Put("displayedRating",rating);SetState("展示实际 "+rating+" 星");Transition(p=>{for(int i=0;i<3;i++){bool lit=i<rating&&(Reduced||p>=i*.25f);if(RecipeArt.Available)RecipeArt.Apply(stars[i].GetComponent<Image>(),lit?"star-lit":"star-unlit",false);else stars[i].GetComponent<Image>().color=lit?RecipeUI.Gold:RecipeUI.Hex("4E515A");stars[i].localScale=Vector3.one*(lit&&!Reduced?1+Mathf.Sin(Mathf.Clamp01((p-i*.25f)*3)*Mathf.PI)*.16f:1);}});});}
            if(n==3){
                center.GetComponent<Image>().enabled=false;foreach(Transform child in center)child.gameObject.SetActive(false);
                var box=RecipeUI.Box(center,"Chest body",0,-15,145,95,RecipeUI.Orange).rectTransform;RecipeUI.Box(box,"Gold band",0,0,18,95,RecipeUI.Gold);
                if(RecipeArt.Available)box.Find("Gold band").gameObject.SetActive(false);
                var lid=RecipeUI.Box(center,"Independent chest lid",0,32,155,60,RecipeUI.Gold).rectTransform;lid.pivot=new Vector2(.5f,0);lid.anchoredPosition=new Vector2(0,28);
                var reward=RecipeUI.Box(center,"Reward jewel",0,30,50,65,RecipeUI.Blue,"diamond").rectTransform;RecipeUI.Alpha(reward,0);reward.SetAsFirstSibling();
                Operation("开启宝箱",()=>{grant();Put("opened",1);SetState("宝箱已开 · 奖励只发放一次");float angle=lid.localEulerAngles.x;Transition(p=>{float t=Reduced?1:Ease(p);lid.localRotation=Quaternion.Euler(Mathf.LerpAngle(angle,-80,t),0,0);RecipeUI.Alpha(reward,t);reward.anchoredPosition=new Vector2(0,Mathf.Lerp(10,90,t));});});
            }
            if(n==4){center.gameObject.SetActive(false);var coins=new RectTransform[7];for(int i=0;i<coins.Length;i++)coins[i]=RecipeUI.Box(world,"Representative coin "+i,(i-3)*18,-30,30,30,RecipeUI.Gold,"circle").rectTransform;
                Operation("领取 100 金币",()=>{grant();SetState("已到账 100 · 7 个图标代表表现");Transition(p=>{for(int i=0;i<coins.Length;i++){float t=Reduced?1:Mathf.Clamp01((p-i*.045f)*1.4f);coins[i].anchoredPosition=Vector2.Lerp(new Vector2((i-3)*18,-30),hud.anchoredPosition,t)+Vector2.up*Mathf.Sin(t*Mathf.PI)*90;RecipeUI.Alpha(coins[i],t>=1?0:1);}});});Operation("取消飞行",()=>{StopMotion();foreach(var c in coins)RecipeUI.Alpha(c,0);SetState("表现取消 · 已到账金币保持");});}
            if(n==5){if(RecipeArt.Available){foreach(Transform child in center)child.gameObject.SetActive(false);RecipeArt.Apply(center.GetComponent<Image>(),"medal",false);}var slot=RecipeUI.Box(world,"Achievement archive",-280,100,100,100,RecipeUI.Surface).rectTransform;center.SetAsLastSibling();Operation("解锁成就",()=>{Put("unlocked",1);SetState("成就已保存 · 归档表现");var from=center.anchoredPosition;var scale=center.localScale;Transition(p=>{float t=Reduced?1:Ease(p);center.anchoredPosition=Vector2.Lerp(from,slot.anchoredPosition,t);center.localScale=Vector3.Lerp(scale,Vector3.one*.42f,t);});});}
            if(n==6){if(RecipeArt.Available){foreach(Transform child in center)child.gameObject.SetActive(false);RecipeArt.Apply(center.GetComponent<Image>(),"level-badge",false);}var number=RecipeUI.Label(center,"1",0,-8,100,70,44);Operation("等级 +1",()=>{Put("level",Get("level")+1);status.text="真实等级 "+Get("level");SetState(status.text);number.text=Get("level").ToString();Transition(p=>center.localScale=Vector3.one*(1+(Reduced?0:Mathf.Sin(p*Mathf.PI)*.18f*Strength)));});}
            if(n==7){center.gameObject.SetActive(false);var rows=new RectTransform[4];int[] ids={101,102,103,999};for(int i=0;i<4;i++){rows[i]=RecipeUI.Box(world,"User ID "+ids[i],0,105-i*65,450,55,i==3?RecipeUI.Blue:RecipeUI.Surface).rectTransform;Caption(rows[i],ids[i]==999?"玩家 · ID 999":"对手 · ID "+ids[i],0,0,430);}
                Operation("更新比赛结果",()=>{Put("playerId",999);Put("playerRank",1);SetState("玩家 ID 999 · 第 4 名 → 第 1 名");var from=new Vector2[4];for(int i=0;i<4;i++)from[i]=rows[i].anchoredPosition;Transition(p=>{float t=Reduced?1:Ease(p);for(int i=0;i<4;i++)rows[i].anchoredPosition=Vector2.Lerp(from[i],new Vector2(0,105-(i==3?0:i+1)*65),t);});});}
            if(n==8){
                SetState("结算待开始");
                Action finish=()=>{grant();Put("finished",1);StopMotion();center.localScale=Vector3.one;RecipeUI.Alpha(center,1);status.text="✓ 结算完成 · 可继续 · 发放次数 "+Get("grants");SetState("FinishPresentation · 幂等");};
                Operation("开始结算",()=>{grant();Put("finished",0);SetState("结算表现中 · 可跳过");status.text="金币已到账，动画结束不再发放";Transition(p=>{RecipeUI.Alpha(center,Reduced?1:p);center.localScale=Vector3.one*(Reduced?1:.7f+.3f*Ease(p));});});
                Operation("跳过并收尾",finish);
                ticking.Add(()=>{if(Get("claimed")==1&&Get("finished")==0&&Progress>=1&&!Paused&&!IsPlaying)finish();});
            }
        }
        void BuildIdle(){
            int n=Definition.Number;var item=RecipeUI.Rect(world,"Loop subject",0,0,230,245);PrimarySubject=item;
            var jewel=Art(item,"Independent subject",n==2?"approved-potion":n==3?"swords":n==4?"medal":n==7?"coin":"crystal",0,0,n==3?205:190,n==3?205:220).rectTransform;
            var halo=Art(world,"Independent halo","halo",0,0,320,320);halo.transform.SetAsFirstSibling();RecipeUI.Alpha(halo.rectTransform,n==5?1:0);
            var clip=RecipeUI.Rect(item,"Shine clip",0,0,350,110);clip.gameObject.AddComponent<RectMask2D>();var shine=Art(clip,"Independent shine","shine",-240,0,65,190).rectTransform;RecipeUI.Alpha(shine,0);
            RectTransform back=null;
            if(n==4){item.pivot=new Vector2(.5f,1);item.anchoredPosition=new Vector2(0,180);jewel.anchorMin=jewel.anchorMax=new Vector2(.5f,1);jewel.anchoredPosition=new Vector2(0,-160);var cord=RecipeUI.Box(item,"Hanging cord",0,-50,5,100,RecipeUI.Gold);cord.sprite=null;cord.color=RecipeUI.Gold;cord.rectTransform.anchorMin=cord.rectTransform.anchorMax=new Vector2(.5f,1);cord.rectTransform.anchoredPosition=new Vector2(0,-50);}
            if(n==6){jewel.gameObject.SetActive(false);var button=Art(item,"Reward button","button-blue",0,0,350,110,true);RecipeUI.Label(button.transform,"领取补给",22,0,255,70,31);Art(button.transform,"Gift icon","gift",-117,0,52,60);clip.SetAsLastSibling();}
            if(n==7){item.anchoredPosition=new Vector2(0,120);for(int i=0;i<32;i++){float angle=i*Mathf.PI/16;var dot=RecipeUI.Box(world,"Closed path marker",Mathf.Sin(angle)*220,Mathf.Cos(angle)*120,4,4,RecipeUI.Hex("61718A"),"circle");dot.sprite=null;dot.color=RecipeUI.Hex("61718A");}item.SetAsLastSibling();}
            if(n==8){back=Art(world,"Parallax rear layer","wb-inventory",0,0,350,270,true).rectTransform;back.SetAsFirstSibling();}
            Vector2 rest=item.anchoredPosition;bool running=false;
            Action restore=()=>{item.anchoredPosition=rest;item.localScale=Vector3.one;item.localRotation=Quaternion.identity;jewel.localRotation=Quaternion.identity;RecipeUI.Alpha(halo.rectTransform,n==5?1:0);RecipeUI.Alpha(shine,0);if(back!=null)back.anchoredPosition=Vector2.zero;};restore();
            Action<float> sample=p=>{if(Reduced){restore();return;}float wave=Mathf.Sin(p*Mathf.PI*2);switch(n){case 1:item.localScale=Vector3.one*(1+wave*.045f*Strength);break;case 2:item.anchoredPosition=rest+Vector2.up*wave*24*Strength;break;case 3:jewel.localRotation=Quaternion.Euler(0,0,-360*p);break;case 4:item.localRotation=Quaternion.Euler(0,0,wave*9*Strength);break;case 5:RecipeUI.Alpha(halo.rectTransform,.25f+.65f*(wave*.5f+.5f));break;case 6:float t=Mathf.Clamp01(p/.28f);RecipeUI.Alpha(shine,p<.28f?1:0);shine.anchoredPosition=new Vector2(Mathf.Lerp(-240,240,t),0);break;case 7:item.anchoredPosition=new Vector2(Mathf.Sin(p*Mathf.PI*2)*220,Mathf.Cos(p*Mathf.PI*2)*120);break;}};
            Operation("开启",()=>{running=true;Put("running",1);SetState(Reduced?"减弱动态 · 静态标识":n==8?"指针视差 · 舞台内移动指针":"循环进行中 · "+Definition.name);if(n!=8)Transition(sample,true);});
            Operation("停止",()=>{running=false;Put("running",0);StopMotion();restore();SetState("已停止 · 恢复基线");});cancelling.Add(()=>{running=false;Put("running",0);restore();});
            if(n==8)ticking.Add(()=>{if(!running||Reduced)return;RectTransformUtility.ScreenPointToLocalPointInRectangle(world,Input.mousePosition,null,out var p);bool inside=Mathf.Abs(p.x)<370&&Mathf.Abs(p.y)<210;Vector2 offset=inside?new Vector2(Mathf.Clamp(p.x/370,-1,1)*28,Mathf.Clamp(p.y/210,-1,1)*22):Vector2.zero;float t=1-Mathf.Exp(-Time.unscaledDeltaTime*10);item.anchoredPosition=Vector2.Lerp(item.anchoredPosition,rest+offset,t);back.anchoredPosition=Vector2.Lerp(back.anchoredPosition,-offset*.45f,t);});
        }
    }
}
