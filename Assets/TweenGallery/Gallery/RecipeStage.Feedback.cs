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
        void BuildValue() {
            int n=Definition.Number;var panel=Panel("数值与真实进度",0,0,620,280);var label=Caption(panel,"100",0,30,550);label.fontSize=38;
            float shown=n==4?30:100;Put("value",Mathf.RoundToInt(shown));Put("target",Get("value"));
            Action<int> chase=target=>{float from=shown;Put("target",target);Put("value",target);SetState("真实值 "+target+" · 显示值追逐中");Transition(p=>{shown=Mathf.Lerp(from,target,Reduced?1:Ease(p));label.text=Mathf.RoundToInt(shown).ToString("N0");});};
            if(n==1){Operation("目标 +100",()=>chase(Get("target")+100));Operation("目标 -50",()=>chase(Mathf.Max(0,Get("target")-50)));}
            if(n==2){Operation("收入 +100",()=>chase(Get("value")+100));Operation("支出 -80",()=>{if(Get("value")<80){SetState("余额不足 · 拒绝支出");Put("rejected",Get("rejected")+1);}else chase(Get("value")-80);});}
            if(n==3||n==4){
                int hp=n==3?100:30;Put("value",hp);label.text="生命 "+hp+" / 100";
                var trail=Bar(panel,-45,RecipeUI.Gold,hp/100f);var fill=RecipeUI.Box(trail.parent,"Current health",-205,0,410*hp/100f,18,RecipeUI.Green).rectTransform;fill.pivot=new Vector2(0,.5f);
                Operation(n==3?"受到 25 伤害":"治疗 +25",()=>{
                    hp=Mathf.Clamp(hp+(n==3?-25:25),0,100);Put("value",hp);SetState("真实生命 "+hp);label.text="生命 "+hp+" / 100";
                    float oldFill=fill.sizeDelta.x,oldTrail=trail.sizeDelta.x,target=410*hp/100f;
                    if(n==3)fill.sizeDelta=new Vector2(target,18);
                    Transition(p=>{float t=Reduced?1:Ease(p);if(n==4)fill.sizeDelta=new Vector2(Mathf.Lerp(oldFill,target,t),18);trail.sizeDelta=new Vector2(Mathf.Lerp(oldTrail,target,Reduced?1:Mathf.Clamp01((p-.25f)/.75f)),18);});
                });
            }
            if(n==5){Put("level",1);Put("xp",70);var fill=Bar(panel,-50,RecipeUI.Blue,.7f);label.text="Lv.1   70 / 100";
                Operation("获得 150 经验",()=>{int oldLevel=Get("level"),oldXp=Get("xp"),total=oldXp+150;Put("level",oldLevel+total/100);Put("xp",total%100);SetState("真实等级 "+Get("level")+" · 余量 "+Get("xp"));Transition(p=>{float amount=Reduced?total:Mathf.Lerp(oldXp,total,p);int displayed=oldLevel+Mathf.FloorToInt(amount/100);float fraction=Mathf.Repeat(amount,100)/100;label.text="Lv."+displayed+"   "+Mathf.FloorToInt(fraction*100)+" / 100";fill.sizeDelta=new Vector2(410*fraction,18);});});
            }
            if(n==6){double deadline=0;bool active=false;label.text="10 秒";Operation("开始 10 秒",()=>{deadline=Time.realtimeSinceStartupAsDouble+10;active=true;Put("running",1);SetState("使用真实截止时间 · 暂停表现不暂停倒计时");});ticking.Add(()=>{if(!active)return;float remaining=(float)Math.Max(0,deadline-Time.realtimeSinceStartupAsDouble);label.text=Mathf.CeilToInt(remaining)+" 秒";label.color=remaining<=3?RecipeUI.Pink:RecipeUI.Ink;label.transform.localScale=Vector3.one*(remaining<=3&&!Reduced?1+Mathf.Sin(remaining*Mathf.PI*2)*.035f:1);Put("remaining",Mathf.CeilToInt(remaining));if(remaining<=0){active=false;Put("running",0);SetState("倒计时结束");}});}
            if(n==7){label.text="等待开始";var ring=RecipeUI.Box(panel,"Directional spinner",0,-45,44,44,RecipeUI.Blue,"ring").rectTransform;RecipeUI.Box(ring,"Direction marker",0,22,12,12,RecipeUI.Gold,"circle");Operation("开始加载",()=>{Put("loading",1);label.text="加载中…";SetState("未知时长 · 不显示百分比");Transition(p=>ring.localRotation=Quaternion.Euler(0,0,Reduced?0:-360*p),true);});Operation("完成",()=>{StopMotion();Put("loading",0);Put("success",1);label.text="✓ 加载完成";SetState("真实任务完成");});Operation("失败",()=>{StopMotion();Put("loading",0);Put("success",0);label.text="! 加载失败，可以重试";SetState("真实任务失败");});}
            if(n==8){Put("tasks",0);label.text="0 / 4";var fill=Bar(panel,-45,RecipeUI.Green,0);Operation("完成一个真实任务",()=>{int tasks=Mathf.Min(4,Get("tasks")+1);Put("tasks",tasks);Put("complete",tasks==4?1:0);SetState(tasks==4?"真实任务全部完成":"真实完成 "+tasks+" / 4");label.text=tasks+" / 4";float from=fill.sizeDelta.x;Transition(p=>fill.sizeDelta=new Vector2(Mathf.Lerp(from,410*tasks/4f,Reduced?1:Ease(p)),18));});}
        }
        void BuildNotice() {
            int n=Definition.Number;var panel=Panel("操作反馈",0,0,620,280);var message=Caption(panel,"等待操作",0,10,550);var feedback=RecipeUI.Box(panel,"Feedback icon",0,-65,55,55,RecipeUI.Blue,"circle").rectTransform;var mark=Caption(feedback,"i",0,0,50);
            if(RecipeArt.Available)mark.text="";
            if(n==1){
                var queue=new Queue<string>();string active=null;float elapsed=0;bool showing=false;int serial=0;RecipeUI.Alpha(message.rectTransform,0);
                Action<string> send=text=>{if(active==text||queue.Contains(text)){Put("merged",Get("merged")+1);SetState("相同消息已合并");return;}if(queue.Count>=3){queue.Dequeue();Put("dropped",Get("dropped")+1);}queue.Enqueue(text);Put("queued",queue.Count);SetState("有界队列 "+queue.Count+" / 3");};
                Operation("发送新消息",()=>send("补给已送达 #"+(++serial)));Operation("发送重复消息",()=>send("每日补给已送达"));
                ticking.Add(()=>{if(!showing&&queue.Count>0){active=queue.Dequeue();Put("queued",queue.Count);message.text=active;elapsed=0;showing=true;}if(!showing)return;if(!Paused)elapsed+=Time.unscaledDeltaTime*Speed;float a=elapsed<.2f?elapsed/.2f:elapsed<1.5f?1:1-(elapsed-1.5f)/.2f;RecipeUI.Alpha(message.rectTransform,Mathf.Clamp01(a));if(elapsed>=1.7f){showing=false;active=null;}});
            }
            if(n==2||n==3||n==4){Operation(n==2?"模拟成功":n==3?"模拟失败":"检查资源",()=>{Put("requests",Get("requests")+1);Put("result",n==2?1:-1);message.text=n==2?"✓ 操作成功，结果已确认":n==3?"! 网络不可用，请修正后重试":"! 资源不足，需要 50 金币";if(RecipeArt.Available){RecipeArt.Apply(feedback.GetComponent<Image>(),n==2?"notice-success":n==3?"notice-error":"notice-warning",false);mark.text="";}else{mark.text=n==2?"✓":"!";feedback.GetComponent<Image>().color=n==2?RecipeUI.Green:RecipeUI.Pink;}SetState(n==2?"Success":n==3?"Error · 可重试":"Warning · 静态原因保留");var start=feedback.anchoredPosition;Transition(p=>{feedback.anchoredPosition=start+Vector2.right*(Reduced||n==2?0:Mathf.Sin(p*Mathf.PI*8)*(1-p)*10);feedback.localScale=Vector3.one*(1+(Reduced?0:Mathf.Sin(p*Mathf.PI)*.12f));});});}
            if(n==5){Operation("增加未读",()=>{Put("unread",Get("unread")+1);message.text="未读消息 "+Get("unread");mark.text=Get("unread").ToString();if(RecipeArt.Available)RecipeArt.Apply(feedback.GetComponent<Image>(),"badge",false);SetState("未读数据 "+Get("unread"));Transition(p=>feedback.localScale=Vector3.one*(1+(Reduced?0:Mathf.Sin(p*Mathf.PI)*.2f)));});Operation("清空未读",()=>{Put("unread",0);message.text="未读消息 0";mark.text="0";SetState("未读已清空");});}
            if(n==6){
                int request=0;Action start=()=>{request++;Put("request",request);Put("loading",1);message.text="请求 #"+request+" 加载中…";mark.text=RecipeArt.Available?"":"↻";if(RecipeArt.Available)RecipeArt.Apply(feedback.GetComponent<Image>(),"spinner",false);SetState("Loading #"+request);Transition(p=>feedback.localRotation=Quaternion.Euler(0,0,Reduced?0:-360*p),true);};
                Operation("请求／重试",start);Operation("请求失败",()=>{if(Get("loading")==0)return;StopMotion();Put("loading",0);message.text="! 请求失败，请点击重试";SetState("Failed · 显示重试入口");});
                Operation("当前请求成功",()=>{if(Get("loading")==0)return;StopMotion();Put("loading",0);Put("completedRequest",request);message.text="✓ 请求 #"+request+" 完成";SetState("Success #"+request);});
                Operation("旧响应返回",()=>{int old=request-1;if(old!=Get("request")){Put("staleIgnored",Get("staleIgnored")+1);SetState("已忽略旧响应 #"+old);}});
            }
            if(n==7){
                var target=RecipeUI.Button(panel,"真实目标按钮",0,-60,210,()=>{if(Get("guiding")==0)return;Put("guiding",0);Put("done",1);StopMotion();RecipeUI.Alpha(feedback,0);message.text="✓ 真实操作完成教学";SetState("教学完成");},RecipeUI.Blue);feedback.anchoredPosition=new Vector2(0,35);if(RecipeArt.Available){RecipeArt.Apply(feedback.GetComponent<Image>(),"arrow",false);mark.text="";}else mark.text="↓";
                Operation("开始引导",()=>{Put("guiding",1);Put("done",0);message.text="点击下方目标按钮";SetState("教学进行中");RecipeUI.Alpha(feedback,1);Transition(p=>feedback.anchoredPosition=new Vector2(0,35+(Reduced?0:Mathf.Sin(p*Mathf.PI*2)*8)),true);});
                Operation("跳过引导",()=>{Put("guiding",0);StopMotion();RecipeUI.Alpha(feedback,0);message.text="已跳过引导";SetState("教学已跳过");});
                Operation("操作目标",()=>target.onClick.Invoke());
            }
            if(n==8){
                message.text="拖动下方目标，气泡持续跟随";if(RecipeArt.Available){RecipeArt.Apply(feedback.GetComponent<Image>(),"tooltip");feedback.sizeDelta=new Vector2(120,55);mark.text="提示";mark.color=RecipeUI.Ink;}var target=RecipeUI.Box(panel,"Anchor target",0,-65,95,45,RecipeUI.Orange).rectTransform;target.GetComponent<Image>().raycastTarget=true;var pointer=target.gameObject.AddComponent<RecipePointer>();Vector2 offset=Vector2.zero;
                pointer.down=e=>{RectTransformUtility.ScreenPointToLocalPointInRectangle(panel,e.position,e.pressEventCamera,out var p);offset=p-target.anchoredPosition;};pointer.drag=e=>{RectTransformUtility.ScreenPointToLocalPointInRectangle(panel,e.position,e.pressEventCamera,out var p);target.anchoredPosition=p-offset;Put("moved",1);};
                ticking.Add(()=>{bool visible=target.gameObject.activeSelf;feedback.gameObject.SetActive(visible);if(visible)feedback.anchoredPosition=target.anchoredPosition+Vector2.up*65;});
                Operation("移动目标",()=>{target.anchoredPosition+=Vector2.right*60;Put("moved",1);SetState("气泡跟随实际目标");});Operation("隐藏／显示目标",()=>{target.gameObject.SetActive(!target.gameObject.activeSelf);Put("targetVisible",target.gameObject.activeSelf?1:0);SetState(target.gameObject.activeSelf?"目标可见":"目标失效 · 气泡隐藏");});
            }
        }
        void BuildReward() {
            int n=Definition.Number;var hud=RecipeUI.Box(world,"Wallet HUD",290,140,215,45,RecipeUI.Surface).rectTransform;var wallet=Caption(hud,"金币 1,000",0,0,210);Put("wallet",1000);Put("level",1);if(RecipeArt.Available)hud.GetComponent<Image>().pixelsPerUnitMultiplier=8;
            var center=RecipeUI.Card(world,n==3?"神秘宝箱":n==5?"成就徽章":n==6?"等级 1":"旅程奖励",0,0,160,195,n!=3);
            if(RecipeArt.Available){var header=RecipeUI.Box(world,"Reward header",-100,150,330,55,Color.white);RecipeArt.Apply(header,"header");RecipeUI.Label(header.transform,n==3?"宝箱揭晓":n==5?"成就解锁":n==6?"等级提升":"旅程奖励",0,0,240,35,23,RecipeUI.Hex("572C10"));}
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
                Action finish=()=>{grant();Put("finished",1);StopMotion();center.localScale=Vector3.one;RecipeUI.Alpha(center,1);status.text="✓ 结算完成 · 可继续 · 发放次数 "+Get("grants");SetState("FinishPresentation · 幂等");};
                Operation("开始结算",()=>{grant();Put("finished",0);SetState("结算表现中 · 可跳过");status.text="金币已到账，动画结束不再发放";Transition(p=>{RecipeUI.Alpha(center,Reduced?1:p);center.localScale=Vector3.one*(Reduced?1:.7f+.3f*Ease(p));});});
                Operation("跳过并收尾",finish);
                ticking.Add(()=>{if(Get("claimed")==1&&Get("finished")==0&&Progress>=1&&!Paused&&!IsPlaying)finish();});
            }
        }
        void BuildIdle() {
            int n=Definition.Number;var card=RecipeUI.Card(world,n==4?"悬挂标牌":"能量核心",0,0,180,220);
            var jewel=RecipeUI.Box(card,"Energy crystal",0,15,70,95,RecipeUI.Blue,"diamond").rectTransform;
            var halo=RecipeUI.Box(world,"Halo",0,0,245,245,RecipeUI.Blue,"ring").rectTransform;halo.SetAsFirstSibling();
            var marker=RecipeUI.Box(jewel,"Direction",20,27,18,18,RecipeUI.Gold,"circle").rectTransform;
            var clip=RecipeUI.Rect(card,"Shine clip",0,0,180,220);clip.gameObject.AddComponent<RectMask2D>();var shine=RecipeUI.Box(clip,"Shine",-180,0,28,300,new Color(1,1,1,.45f)).rectTransform;shine.localRotation=Quaternion.Euler(0,0,-20);RecipeUI.Alpha(shine,0);
            if(n==4){card.pivot=new Vector2(.5f,1);card.anchoredPosition=new Vector2(0,110);}
            bool running=false;Vector2 rest=card.anchoredPosition;
            Action restore=()=>{card.anchoredPosition=rest;card.localScale=Vector3.one;card.localRotation=Quaternion.identity;jewel.localRotation=Quaternion.identity;jewel.anchoredPosition=new Vector2(0,15);RecipeUI.Alpha(halo,n==5?1:0);RecipeUI.Alpha(shine,0);};restore();
            Action<float> sample=p=>{if(Reduced){restore();return;}float wave=Mathf.Sin(p*Mathf.PI*2);switch(n){case 1:card.localScale=Vector3.one*(1+wave*.025f*Strength);break;case 2:card.anchoredPosition=rest+Vector2.up*wave*15*Strength;break;case 3:jewel.localRotation=Quaternion.Euler(0,0,-360*p);break;case 4:card.localRotation=Quaternion.Euler(0,0,wave*7*Strength);break;case 5:RecipeUI.Alpha(halo,.3f+.6f*(wave*.5f+.5f));break;case 6:float t=Mathf.Clamp01(p/.28f);RecipeUI.Alpha(shine,p<.28f?1:0);shine.anchoredPosition=new Vector2(Mathf.Lerp(-160,160,t),0);break;case 7:jewel.anchoredPosition=new Vector2(Mathf.Sin(p*Mathf.PI*2)*190,Mathf.Cos(p*Mathf.PI*2)*105);break;case 8:break;}};
            Operation("开启",()=>{running=true;Put("running",1);SetState(Reduced?"减少动态 · 静态标识":"循环开启 · 单一实例");Transition(sample,true);});
            Operation("停止",()=>{running=false;Put("running",0);StopMotion();restore();SetState("停止 · 恢复基线");});
            cancelling.Add(()=>{running=false;Put("running",0);restore();});
            if(n==8)ticking.Add(()=>{if(!running||Reduced)return;var canvas=(RectTransform)GetComponentInParent<Canvas>().transform;RectTransformUtility.ScreenPointToLocalPointInRectangle(world,Input.mousePosition,null,out var p);bool inStage=Mathf.Abs(p.x)<420&&Mathf.Abs(p.y)<180;Vector2 offset=inStage?new Vector2(Mathf.Clamp(p.x/420,-1,1)*15,Mathf.Clamp(p.y/180,-1,1)*12):Vector2.zero;card.anchoredPosition=Vector2.Lerp(card.anchoredPosition,rest+offset,1-Mathf.Exp(-Time.unscaledDeltaTime*10));jewel.anchoredPosition=new Vector2(0,15)-offset*.7f;});
        }
    }
}
