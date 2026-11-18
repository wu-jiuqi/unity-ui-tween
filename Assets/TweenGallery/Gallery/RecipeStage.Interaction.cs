using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TweenGallery.Recipes
{
    public sealed partial class RecipeStage
    {
        void BuildButton() {
            int n=Definition.Number;var hit=RecipeUI.Button(world,"",0,0,300,null,RecipeUI.Dark,110);hit.transition=Selectable.Transition.None;
            var shadow=RecipeUI.Box(hit.transform,"Shadow",0,-8,290,100,RecipeUI.Hex("16439E")).rectTransform;
            var visual=RecipeUI.Box(hit.transform,"Button visual",0,0,290,100,RecipeUI.Blue).rectTransform;
            RecipeUI.Box(visual,"Highlight",0,38,250,4,new Color(1,1,1,.35f));
            var label=Caption(visual,n==8?"按住蓄力":"开始挑战",0,0,270);label.fontSize=28;
            if(RecipeArt.Available){var icon=RecipeUI.Box(visual,"Independent button icon",-100,0,52,58,Color.white);RecipeArt.Apply(icon,n==5?"gift":"swords",false);label.rectTransform.anchoredPosition=new Vector2(27,0);label.rectTransform.sizeDelta=new Vector2(218,60);}
            var info=Caption(world,"点击次数 0",0,-115,620);
            var charge=RecipeUI.Box(world,"Hold progress",-150,-75,0,6,RecipeUI.Gold).rectTransform;charge.pivot=new Vector2(0,.5f);
            bool held=false,inside=false,available=true,charged=false;float downAt=0;Vector3 rest=Vector3.one;
            Action<float> scale=factor=>{Vector3 from=visual.localScale;Transition(p=>visual.localScale=Vector3.LerpUnclamped(from,rest*factor,Reduced?1:Ease(p)),false,Duration*(factor<1?.35f:.6f));};
            Action cancel=()=>{held=false;charged=false;charge.sizeDelta=new Vector2(0,6);Put("held",0);SetState("取消 · 点击次数 "+Get("clicks"));scale(1);};
            var pointer=hit.gameObject.AddComponent<RecipePointer>();
            pointer.enter=e=>{inside=true;if(!held&&available){if(n==1||n==2)scale(1.055f);SetState("Hovered");}};
            pointer.exit=e=>{inside=false;if(held){e.eligibleForClick=false;cancel();}else {scale(1);SetState("Default");}};
            pointer.down=e=>{if(!available)return;held=true;inside=true;charged=false;downAt=Time.unscaledTime;Put("held",1);SetState("Pressed");if(n!=5)scale(.94f);};
            pointer.up=e=>{bool accept=held&&inside&&available;held=false;Put("held",0);if(!accept){e.eligibleForClick=false;return;}if(n==8&&!charged){cancel();e.eligibleForClick=false;return;}scale(1);SetState("Released");};
            pointer.drag=e=>{e.eligibleForClick=false;};pointer.end=e=>cancel();pointer.cancel=cancel;cancelling.Add(cancel);
            hit.onClick.AddListener(()=>{if(!available)return;if(n==8)return;Put("clicks",Get("clicks")+1);info.text="点击次数 "+Get("clicks");SetState("Clicked · "+Get("clicks"));if(n==5){Transition(p=>visual.localScale=Vector3.one*(1+(Reduced?0:Mathf.Sin(p*Mathf.PI)*.1f*Strength)));}});
            if(n==7)Operation("禁用／启用",()=>{available=!available;cancel();hit.interactable=available;label.text=available?"开始挑战":"已禁用";if(RecipeArt.Available)RecipeArt.Apply(visual.GetComponent<Image>(),available?"button-blue":"button-disabled");else visual.GetComponent<Image>().color=available?RecipeUI.Blue:RecipeUI.Hex("626873");Put("enabled",available?1:0);SetState(available?"Enabled":"Disabled");});
            if(n==8)ticking.Add(()=>{if(!held)return;float t=Mathf.Clamp01((Time.unscaledTime-downAt)/1.2f);charge.sizeDelta=new Vector2(300*t,6);info.text="蓄力 "+Mathf.RoundToInt(t*100)+"%";if(t>=1&&!charged){charged=true;Put("charges",Get("charges")+1);SetState("蓄满 · 触发 "+Get("charges")+" 次");}});
            // Deterministic guided operations use the same pointer handlers, rather than firing business from a tween.
            PointerEventData evt()=>new PointerEventData(EventSystem.current){pointerId=-1,button=PointerEventData.InputButton.Left};
            if(n==1||n==2)Operation("移入",()=>pointer.OnPointerEnter(evt()));
            Operation("按下",()=>pointer.OnPointerDown(evt()));Operation("移出",()=>pointer.OnPointerExit(evt()));
            Operation("松开",()=>{var e=evt();bool accept=held&&inside&&available&&(n!=8||charged);pointer.OnPointerUp(e);if(accept&&n!=8)hit.onClick.Invoke();});
        }
        void BuildInput() {
            int n=Definition.Number;var panel=Panel("输入与选项",0,0,570,280);var info=Caption(panel,"实际状态与视觉反馈同步",0,-80,520);
            if(n==1){var track=RecipeUI.Button(panel,"",0,10,190,null,RecipeUI.Hex("4E5666"),65);var knob=RecipeUI.Box(track.transform,"Knob",-55,0,53,53,RecipeUI.Ink,"circle").rectTransform;if(RecipeArt.Available){RecipeArt.Apply(track.GetComponent<Image>(),"toggle-track");RecipeArt.Apply(knob.GetComponent<Image>(),"toggle-knob",false);}Action toggle=()=>{Put("value",1-Get("value"));bool on=Get("value")==1;info.text=on?"开启":"关闭";SetState(info.text);Move(knob,new Vector2(on?55:-55,0));};track.onClick.AddListener(()=>toggle());Operation("切换开关",toggle);}
            if(n==2){var check=RecipeUI.Button(panel,"接收任务通知",30,10,330,null,RecipeUI.Blue,65);var basePlate=RecipeUI.Box(check.transform,"Checkbox base",-135,0,45,45,Color.white);var tick=RecipeUI.Box(basePlate.transform,"Independent tick",0,0,38,38,Color.white);if(RecipeArt.Available){RecipeArt.Apply(basePlate,"checkbox-off",false);RecipeArt.Apply(tick,"checkbox-on",false);}tick.gameObject.SetActive(false);Action toggle=()=>{Put("value",1-Get("value"));tick.gameObject.SetActive(Get("value")==1);SetState(Get("value")==1?"Checked":"Unchecked");Transition(p=>tick.transform.localScale=Vector3.one*(Reduced?1:.65f+.35f*Ease(p)));};check.onClick.AddListener(()=>toggle());Operation("勾选／取消",toggle);}
            if(n==3){var radios=new Image[3];var marks=new Text[3];Action<int> choose=i=>{Put("selected",i);for(int k=0;k<3;k++){marks[k].text=new[]{"普通","稀有","史诗"}[k];if(RecipeArt.Available)RecipeArt.Apply(radios[k],i==k?"radio-on":"radio-off",false);}SetState("单选 "+i);Transition(p=>marks[i].transform.localScale=Vector3.one*(1+(Reduced?0:Mathf.Sin(p*Mathf.PI)*.08f)));};for(int i=0;i<3;i++){int k=i;marks[i]=RecipeUI.Button(panel,(i==0?"● ":"○ ")+new[]{"普通","稀有","史诗"}[i],-170+i*170,10,150,()=>choose(k),RecipeUI.Blue).GetComponentInChildren<Text>();radios[i]=RecipeUI.Box(marks[i].transform.parent,"Radio marker",-54,0,27,27,Color.white);if(RecipeArt.Available)RecipeArt.Apply(radios[i],i==0?"radio-on":"radio-off",false);marks[i].text=new[]{"普通","稀有","史诗"}[i];marks[i].rectTransform.anchoredPosition=new Vector2(14,0);}Operation("选择下一项",()=>choose((Get("selected")+1)%3));}
            if(n>=4&&n<=7){
                var field=RecipeUI.Input(panel,"输入至少 3 个字符",0,20,420,54);var line=RecipeUI.Box(panel,"Focus underline",0,-10,420,3,RecipeUI.Blue).rectTransform;
                RecipeUI.Alpha(line,0);bool lastFocus=false;
                field.onValueChanged.AddListener(text=>{Put("textLength",text.Length);info.text="已输入 "+text.Length+" 个字符";});
                ticking.Add(()=>{bool focused=field.isFocused;if(focused==lastFocus)return;lastFocus=focused;Put("focused",focused?1:0);SetState(focused?"Focused":"Blurred · 保留 "+field.text.Length+" 个字符");if(RecipeArt.Available)RecipeArt.Apply(field.GetComponent<Image>(),focused?"input-focus":"input-normal");float from=RecipeUI.Group(line).alpha;Transition(p=>RecipeUI.Alpha(line,Mathf.Lerp(from,focused?1:0,Reduced?1:Ease(p))));});
                Operation("聚焦输入",()=>{EventSystem.current.SetSelectedGameObject(field.gameObject);field.ActivateInputField();});
                Operation("失焦",()=>{field.DeactivateInputField();EventSystem.current.SetSelectedGameObject(null);Put("focused",0);SetState("Blurred · 保留 "+field.text.Length+" 个字符");StartCoroutine(DefocusAfterFrame(field));});
                if(n==6||n==7){Action submit=()=>{bool valid=field.text.Trim().Length>=3;Put("valid",valid?1:0);Put("submits",Get("submits")+1);info.text=valid?"✓ 校验成功":"! 至少输入 3 个非空字符";info.color=valid?RecipeUI.Green:RecipeUI.Pink;SetState(valid?"Validation.Success":"Validation.Error");Vector2 from=field.GetComponent<RectTransform>().anchoredPosition;Transition(p=>field.GetComponent<RectTransform>().anchoredPosition=from+Vector2.right*(valid||Reduced?0:Mathf.Sin(p*Mathf.PI*8)*(1-p)*8));};Operation("提交校验",submit);Operation("填入示例",()=>field.text="星光旅者");}
            }
            if(n==8){var bubble=RecipeUI.Box(panel,"Value bubble",0,60,95,42,RecipeUI.Blue).rectTransform;var value=Caption(bubble,"50",0,0,80);var slider=RecipeUI.Slider(panel,0,0,400,0,100,50,v=>{int count=Mathf.RoundToInt(v);Put("value",count);value.text=count.ToString();bubble.anchoredPosition=new Vector2((v/100-.5f)*400,60);SetState("音量 "+count);});var hook=slider.handleRect.gameObject.AddComponent<RecipeHover>();hook.enter=()=>RecipeUI.Alpha(bubble,1);hook.exit=()=>{if(!Input.GetMouseButton(0))RecipeUI.Alpha(bubble,0);};ticking.Add(()=>{if(Input.GetMouseButtonUp(0))RecipeUI.Alpha(bubble,0);});Operation("设为 80",()=>slider.value=80);}
        }
        System.Collections.IEnumerator DefocusAfterFrame(InputField field){yield return new WaitForEndOfFrame();if(alive&&field!=null){field.DeactivateInputField();if(EventSystem.current!=null&&EventSystem.current.currentSelectedGameObject==field.gameObject)EventSystem.current.SetSelectedGameObject(null);}}
        sealed class TaskRow {public int id;public RectTransform root;public Button button;public Text label;}
        void BuildList() {
            int n=Definition.Number;var viewport=RecipeUI.Box(world,"List viewport",0,0,560,300,RecipeUI.Dark).rectTransform;
            viewport.gameObject.AddComponent<RectMask2D>();viewport.GetComponent<Image>().raycastTarget=true;
            var content=RecipeUI.Rect(viewport,"List content",0,0,540,300);content.anchorMin=content.anchorMax=new Vector2(.5f,1);content.pivot=new Vector2(.5f,1);content.anchoredPosition=Vector2.zero;
            var scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.viewport=viewport;scroll.content=content;scroll.horizontal=false;scroll.vertical=true;scroll.movementType=ScrollRect.MovementType.Elastic;scroll.elasticity=.14f;scroll.scrollSensitivity=32;
            int count=n>=6?12:4;var rows=new List<TaskRow>();TaskRow retiring=null;int nextId=100;var empty=Caption(viewport,"暂无任务",0,0);empty.gameObject.SetActive(false);
            Action layout=null;Action<int> remove=null;
            Func<int,TaskRow> create=id=>{var row=new TaskRow{id=id};row.root=RecipeUI.Box(content,"Task ID "+id,0,0,520,55,id%2==0?RecipeUI.Hex("334A6B"):RecipeUI.Hex("3C3F49")).rectTransform;row.root.anchorMin=row.root.anchorMax=new Vector2(.5f,1);row.button=RecipeUI.Button(row.root,"任务 "+id,-65,0,330,()=>{Put("clickedId",id);SetState("点击稳定 ID "+id);},RecipeUI.Blue);row.label=row.button.GetComponentInChildren<Text>();if(n==3)RecipeUI.Button(row.root,"删除",195,0,80,()=>remove(id),RecipeUI.Pink);return row;};
            for(int i=0;i<count;i++)rows.Add(create(nextId++));
            layout=()=>{content.sizeDelta=new Vector2(540,Mathf.Max(300,rows.Count*66+10));for(int i=0;i<rows.Count;i++)rows[i].root.anchoredPosition=new Vector2(0,-38-i*66);Put("count",rows.Count);empty.gameObject.SetActive(rows.Count==0);};layout();
            Action animateLayout=()=>{var starts=new Vector2[rows.Count];var targets=new Vector2[rows.Count];var activeRows=rows.ToArray();var leaving=retiring;var leaveFrom=leaving==null?Vector2.zero:leaving.root.anchoredPosition;for(int i=0;i<rows.Count;i++){starts[i]=rows[i].root.anchoredPosition;targets[i]=new Vector2(0,-38-i*66);}content.sizeDelta=new Vector2(540,Mathf.Max(300,rows.Count*66+10));Put("count",rows.Count);empty.gameObject.SetActive(rows.Count==0);Transition(p=>{float t=Reduced?1:Ease(p);if(leaving!=null){RecipeUI.Group(leaving.root).alpha=1-t;leaving.root.anchoredPosition=leaveFrom+Vector2.right*35*t;}for(int i=0;i<activeRows.Length;i++)if(activeRows[i].root!=null)activeRows[i].root.anchoredPosition=Vector2.Lerp(starts[i],targets[i],t);});};
            remove=id=>{var row=rows.Find(r=>r.id==id);if(row==null)return;if(retiring!=null)RecipeUI.Alpha(retiring.root,0);rows.Remove(row);retiring=row;row.button.interactable=false;RecipeUI.Group(row.root).interactable=RecipeUI.Group(row.root).blocksRaycasts=false;Put("removedId",id);SetState("已删除 ID "+id);animateLayout();};
            if(n==1){foreach(var row in rows)RecipeUI.Alpha(row.root,0);Operation("显示首屏",()=>{SetState("仅首屏可见行错峰进入");var active=rows.ToArray();Transition(p=>{for(int i=0;i<active.Length;i++)RecipeUI.Alpha(active[i].root,Reduced?1:Mathf.Clamp01((p-i*.1f)*2));});});}
            if(n==2)Operation("插入任务",()=>{var row=create(nextId++);row.root.anchoredPosition=new Vector2(0,20);rows.Insert(0,row);Put("insertedId",row.id);SetState("新任务 ID "+row.id);animateLayout();});
            if(n==3)Operation("删除首行",()=>{if(rows.Count>0)remove(rows[0].id);});
            if(n==4)Operation("反转顺序",()=>{rows.Reverse();Put("firstId",rows[0].id);SetState("重排 · 首行 ID "+rows[0].id);animateLayout();});
            if(n==5){var all=rows.ToArray();Operation("全部／未完成",()=>{Put("filtered",1-Get("filtered"));rows.Clear();foreach(var row in all){bool visible=Get("filtered")==0||row.id%2==0;row.root.gameObject.SetActive(visible);if(visible)rows.Add(row);}SetState("筛选结果 "+rows.Count+" 条 · 身份保留");animateLayout();});}
            if(n==6){float lastInput=Time.unscaledTime;bool snapped=false;ticking.Add(()=>{if(Input.GetMouseButton(0)||Mathf.Abs(Input.mouseScrollDelta.y)>.01f){lastInput=Time.unscaledTime;snapped=false;if(IsPlaying)StopMotion();}if(!snapped&&Time.unscaledTime-lastInput>.25f&&scroll.velocity.sqrMagnitude<80){snapped=true;scroll.StopMovement();float target=Mathf.Clamp(Mathf.Round(content.anchoredPosition.y/66)*66,0,content.rect.height-viewport.rect.height);SetState("已吸附行边界");Move(content,new Vector2(0,target));}});Operation("模拟偏移后吸附",()=>{content.anchoredPosition=new Vector2(0,107);scroll.StopMovement();Move(content,new Vector2(0,132));Put("snapped",1);SetState("吸附到第 3 行");});}
            if(n==7){Operation("模拟越界",()=>{StopMotion();content.anchoredPosition=new Vector2(0,-110);scroll.velocity=Vector2.zero;Put("overscrolled",1);SetState("ScrollRect 原生弹性回界");});}
            if(n==8){Operation("跳转任务08",()=>{scroll.StopMovement();Put("targetId",107);SetState("目标 ID 107 · 第 8 行");if(RecipeArt.Available)foreach(var row in rows)RecipeArt.Apply(row.root.GetComponent<Image>(),row.id==107?"list-selected":"list-row");Move(content,new Vector2(0,Mathf.Min(7*66,content.rect.height-viewport.rect.height)));});ticking.Add(()=>{if(Input.GetMouseButtonDown(0)&&IsPlaying)StopMotion();});}
        }
        void BuildDrag() {
            int n=Definition.Number;
            var source=RecipeUI.Box(world,"Source slot",-260,0,160,160,RecipeUI.Hex("353A45")).rectTransform;Caption(source,"源槽",0,-108,180);
            var valid=RecipeUI.Box(world,"Valid slot",10,0,170,170,RecipeUI.Hex("174C42")).rectTransform;Caption(valid,"✓ 可放置",0,-110,180);
            var invalid=RecipeUI.Box(world,"Invalid slot",265,0,160,160,RecipeUI.Hex("57293E")).rectTransform;Caption(invalid,"⊘ 不匹配",0,-108,180);
            var item=RecipeUI.Card(world,"物品 A",-260,0,110,140);var hit=item.gameObject.AddComponent<Button>();hit.transition=Selectable.Transition.None;item.GetComponent<Image>().raycastTarget=true;
            var pointer=item.gameObject.AddComponent<RecipePointer>();Vector2 home=item.anchoredPosition,grab=Vector2.zero;bool accepted=false,held=false;
            var spring=new MotionSpring2D(home);bool settling=false;
            Action<string> restore=reason=>{held=false;settling=true;spring.Reset(item.anchoredPosition);spring.Retarget(home);Put("accepted",accepted?1:0);Put("held",0);SetState(reason);item.localScale=Vector3.one;if(Reduced){item.anchoredPosition=home;settling=false;}};
            pointer.down=e=>{StopMotion();settling=false;held=true;RectTransformUtility.ScreenPointToLocalPointInRectangle(world,e.position,e.pressEventCamera,out var p);grab=p-item.anchoredPosition;Put("held",1);SetState("拾起 · 活动指针 "+e.pointerId);if(!Reduced)item.localScale=Vector3.one*1.08f;};
            pointer.drag=e=>{RectTransformUtility.ScreenPointToLocalPointInRectangle(world,e.position,e.pressEventCamera,out var p);item.anchoredPosition=new Vector2(Mathf.Clamp(p.x-grab.x,-365,365),Mathf.Clamp(p.y-grab.y,-110,125));bool good=Vector2.Distance(item.anchoredPosition,valid.anchoredPosition)<100;bool bad=Vector2.Distance(item.anchoredPosition,invalid.anchoredPosition)<100;Put("hoverSlot",good?1:bad?-1:0);SetState(good?"✓ 有效区域":bad?"⊘ 拒绝：物品类型不匹配":"拖动 · 保持抓取偏移");valid.GetComponent<Image>().color=good?RecipeUI.Green:RecipeUI.Hex("174C42");};
            Action drop=()=>{if(!held)return;held=false;Put("held",0);bool good=Vector2.Distance(item.anchoredPosition,valid.anchoredPosition)<100;if(good&&n!=6){accepted=true;home=valid.anchoredPosition;Put("slot",1);Put("drops",Get("drops")+1);restore("Accepted · 已提交目标槽");}else{Put("rejected",Get("rejected")+1);restore("Rejected · 未提交，回到当前源槽");}};
            pointer.up=e=>drop();pointer.end=e=>drop();pointer.cancel=()=>restore("Cancelled · 恢复源槽");cancelling.Add(()=>restore("Cancelled · 页面取消"));
            ticking.Add(()=>{if(settling){spring.Step(Time.unscaledDeltaTime,4.5f,.56f);item.anchoredPosition=spring.Value;if(spring.Settle(.05f,.05f)){item.anchoredPosition=home;settling=false;}}if(held&&Input.GetKeyDown(KeyCode.Escape)){pointer.Cancel();restore("Cancelled · Esc");}});
            Operation("模拟拾起",()=>{held=true;Put("held",1);SetState("PickedUp");item.localScale=Reduced?Vector3.one:Vector3.one*1.08f;});
            Operation("移到有效槽",()=>{held=true;Put("held",1);item.anchoredPosition=valid.anchoredPosition;Put("hoverSlot",1);SetState("✓ 有效区域");});
            Operation("移到无效槽",()=>{held=true;Put("held",1);item.anchoredPosition=invalid.anchoredPosition;Put("hoverSlot",-1);SetState("⊘ 无效区域：类型不匹配");});
            Operation("松开放置",drop);Operation("取消",()=>{pointer.Cancel();Put("cancelled",Get("cancelled")+1);restore("Cancelled · 不提交放置");});
            if(n==7){
                source.gameObject.SetActive(false);invalid.gameObject.SetActive(false);valid.gameObject.SetActive(false);
                var b=RecipeUI.Card(world,"物品 B",0,0,110,140);var c=RecipeUI.Card(world,"物品 C",250,0,110,140);
                var dragOriginal=pointer.drag;pointer.drag=e=>{dragOriginal(e);bool after=item.anchoredPosition.x>110;b.anchoredPosition=new Vector2(after?-250:0,0);c.anchoredPosition=new Vector2(after?0:250,0);Put("sortPreview",after?1:0);};
                pointer.up=e=>{if(Get("sortPreview")==1){Put("order",1);home=new Vector2(250,0);accepted=true;Put("slot",2);restore("排序提交：B / C / A");}else restore("排序取消：A / B / C");};pointer.end=pointer.up;
                Actions["松开放置"].onClick.RemoveAllListeners();Actions["松开放置"].onClick.AddListener(()=>{Put("order",1);home=new Vector2(250,0);b.anchoredPosition=new Vector2(-250,0);c.anchoredPosition=Vector2.zero;restore("排序提交：B / C / A");});
            }
        }
    }
}
