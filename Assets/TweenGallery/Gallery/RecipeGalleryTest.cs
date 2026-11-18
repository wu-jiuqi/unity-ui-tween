using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace TweenGallery.Recipes
{
    /// <summary>Opt-in tests drive the same operations exposed by the stage.</summary>
    public sealed class RecipeGalleryTest : MonoBehaviour
    {
        [Serializable] public class Case {public string id,name;public bool normal,boundary,sampling,reduced,cleanup;public string detail,screenshot;}
        [Serializable] public class Report {public string unity,version,platform,utc;public int width,height;public bool passed;public List<Case> cases=new List<Case>();public List<string> checks=new List<string>();public List<string> errors=new List<string>();}
        Report report=new Report();string output;RecipeGalleryBootstrap gallery;bool running;float started;
        IEnumerator Start() {
            var args=Environment.GetCommandLineArgs();if(!args.Contains("--recipes-test"))yield break;
            int at=Array.IndexOf(args,"--test-output");output=at>=0?args[at+1]:"validation-output/recipes96";Directory.CreateDirectory(output);
            running=true;started=Time.realtimeSinceStartup;Application.logMessageReceived+=Log;
            report.unity=Application.unityVersion;report.version=Application.version;report.platform=Application.platform.ToString();report.utc=DateTime.UtcNow.ToString("o");report.width=Screen.width;report.height=Screen.height;
            yield return null;yield return null;gallery=FindFirstObjectByType<RecipeGalleryBootstrap>();
            if(gallery==null){report.errors.Add("Missing new gallery");Finish();yield break;}
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(output,"reference-default.png"));yield return null;
            var beforePreview=string.Join("|",gallery.Stage.Data.OrderBy(k=>k.Key).Select(k=>k.Key+"="+k.Value));gallery.GuidedOperation();gallery.Stage.Seek(.5f);
            try{Check(gallery.Stage.ReferenceMainCard!=null,"Approved card composition missing");Check(gallery.Stage.ReferenceMainCard.localScale.x>1&&gallery.Stage.ReferenceMainCard.localScale.x<1.21f,"Scale preview did not animate");Check(beforePreview==string.Join("|",gallery.Stage.Data.OrderBy(k=>k.Key).Select(k=>k.Key+"="+k.Value)),"Preview changed business data");report.checks.Add("Approved reference composition and inspector scale preview rendered in Player");}catch(Exception e){report.errors.Add(e.ToString());}
            yield return WorkbenchChecks();
            gallery.Select("03.08");yield return null;
            gallery.ShowAllRecipes();yield return null;
            try{CheckChannels();Check(RecipeArt.Available,"Imagegen art missing");CheckDirectory();report.checks.Add("Atomic channel ownership; 96 visible directory entries; imagegen assets present");}
            catch(Exception e){report.errors.Add(e.ToString());}
            foreach(var definition in RecipeCatalog.All){
                gallery.Select(definition.id);yield return null;var stage=gallery.Stage;var item=new Case{id=definition.id,name=definition.name};report.cases.Add(item);
                try{Normal(stage);item.normal=true;}
                catch(Exception e){item.detail="Normal: "+e.Message;}
                if(definition.id=="05.08"){
                    stage.Trigger("按下");yield return new WaitForSecondsRealtime(1.28f);
                    try{Check(stage.Get("charges")==1,"Full hold did not fire exactly once");}catch(Exception e){item.normal=false;item.detail+=" "+e.Message;}
                    yield return new WaitForSecondsRealtime(.15f);if(stage.Get("charges")!=1){item.normal=false;item.detail+=" Hold repeated grant";}stage.Trigger("松开");
                }
                yield return null;
                try{
                    string before=stage.BusinessFingerprint();
                    if(stage.CanSeek){stage.Seek(.4f);stage.Seek(1);stage.Replay();stage.Seek(.6f);}
                    Check(before==stage.BusinessFingerprint(),"Sampling/replay changed business data");item.sampling=true;
                }catch(Exception e){item.detail+=" Sampling: "+e.Message;}
                if(definition.Group==8)yield return new WaitForSecondsRealtime(.3f);
                yield return new WaitForEndOfFrame();
                item.screenshot=definition.id+".png";ScreenCapture.CaptureScreenshot(Path.Combine(output,item.screenshot));yield return null;
                // Reset first: a boundary test must not inherit the normal path's effects.
                gallery.Select(definition.id);yield return null;stage=gallery.Stage;
                try{Boundary(stage);item.boundary=true;}catch(Exception e){item.detail+=" Boundary: "+e.Message;}
                gallery.Select(definition.id);yield return null;stage=gallery.Stage;
                try{stage.SetReduced(true);Normal(stage);if(stage.CanSeek)stage.Seek(1);Check(Finite(stage),"Non-finite reduced-motion transform");item.reduced=true;}catch(Exception e){item.detail+=" Reduced: "+e.Message;}
                try{stage.Shutdown();Check(!stage.IsPlaying,"Tween remained active after shutdown");item.cleanup=true;}catch(Exception e){item.detail+=" Cleanup: "+e.Message;}
            }
            gallery.Select("01.01");Finish();
        }
        static void Check(bool condition,string detail){if(!condition)throw new Exception(detail);}
        Button NamedButton(string name)=>gallery.GetComponentsInChildren<Button>().Single(b=>b.name==name);
        static RaycastResult Hit(Vector2 point){Canvas.ForceUpdateCanvases();var e=new PointerEventData(EventSystem.current){position=point};var results=new List<RaycastResult>();EventSystem.current.RaycastAll(e,results);Check(results.Count>0,"Nothing hit at "+point);return results[0];}
        static void Tap(Button b){var p=RectTransformUtility.WorldToScreenPoint(null,b.transform.position);var hit=Hit(p);Check(ExecuteEvents.GetEventHandler<IPointerClickHandler>(hit.gameObject)==b.gameObject,"Button obstructed: "+b.name+" by "+hit.gameObject.name);var e=new PointerEventData(EventSystem.current){position=p,button=PointerEventData.InputButton.Left,clickCount=1};ExecuteEvents.Execute(b.gameObject,e,ExecuteEvents.pointerClickHandler);}
        IEnumerator WorkbenchChecks(){
            try{
                for(int i=0;i<4;i++){Tap(NamedButton("Curve choice "+i));Check(gallery.SelectedCurveIndex==i,"Curve selection did not update");Check(gallery.Stage.Curve==new[]{DG.Tweening.Ease.OutCubic,DG.Tweening.Ease.InCubic,DG.Tweening.Ease.InOutCubic,DG.Tweening.Ease.OutElastic}[i],"Wrong evaluator");}
                Tap(NamedButton("Curve choice 0"));gallery.Stage.Seek(.25f);float slowEnd=gallery.Stage.ReferenceSelectedCard.localScale.x;
                Tap(NamedButton("Curve choice 1"));gallery.Stage.Seek(.25f);Check(slowEnd-gallery.Stage.ReferenceSelectedCard.localScale.x>.08f,"Curve did not affect card transform");
                var fingerprint=gallery.Stage.BusinessFingerprint();Tap(NamedButton("Curve choice 3"));gallery.Stage.Seek(.5f);Check(fingerprint==gallery.Stage.BusinessFingerprint(),"Curve preview changed business state");
                gallery.Stage.Strength=1.5f;gallery.Stage.PreviewReferenceScale();gallery.Stage.Seek(.13f);var peak=gallery.Stage.ReferenceMainCard;Check(peak.anchoredPosition.y+peak.rect.height*peak.localScale.y*.54f<=448.01f,"Elastic peak clipped above stage");gallery.Stage.Strength=1.2f;
                gallery.Select("03.08");gallery.TogglePresentationLoop();Check(gallery.Stage.Looping&&gallery.Stage.IsPlaying,"Loop before first play did nothing");gallery.TogglePresentationLoop();Check(!gallery.Stage.Looping,"Loop did not turn off");gallery.Stage.Seek(.4f);Check(gallery.Stage.Paused,"Timeline did not pause");gallery.Stage.TogglePause();Check(gallery.Stage.IsPlaying,"Resume did nothing");gallery.ReversePresentation();Check(gallery.Stage.Backwards,"Reverse did nothing");
                Tap(NamedButton("Shortcut 排列过渡"));Check(gallery.Stage.ReferenceComposition,"Arrange left the reference cards");gallery.GuidedOperation();gallery.Stage.Seek(.5f);Check(gallery.Stage.ReferenceMainCard.anchoredPosition.x>60,"Cards did not rearrange");
            }catch(Exception e){report.errors.Add("Workbench controls: "+e);}
            yield return null;
            foreach(string name in new[]{"位移滑动","旋转动画","淡入淡出","选中缩放"}){
                try{Tap(NamedButton("Shortcut "+name));Check(gallery.Stage.ReferenceComposition,"Shortcut changed fixture: "+name);gallery.GuidedOperation();Check(gallery.Stage.CanSeek,"Shortcut has no presentation: "+name);}
                catch(Exception e){report.errors.Add("Shortcut pointer: "+e);}
                yield return null;
            }
            try{
                Tap(NamedButton("Curve choice 3"));gallery.Stage.Seek(.4f);
                var slider=gallery.GetComponentsInChildren<Slider>().First(s=>s.minValue==.08f);var sr=(RectTransform)slider.transform;Check(sr.rect.height>=32,"Slider hit area too narrow");var point=RectTransformUtility.WorldToScreenPoint(null,sr.TransformPoint(new Vector3(sr.rect.width*.3f,10,0)));var hit=Hit(point);Check(ExecuteEvents.GetEventHandler<IPointerDownHandler>(hit.gameObject)==slider.gameObject,"Slider expanded hit area obstructed");var e=new PointerEventData(EventSystem.current){position=point,button=PointerEventData.InputButton.Left};ExecuteEvents.Execute(slider.gameObject,e,ExecuteEvents.pointerDownHandler);Check(gallery.Stage.Duration>1,"Slider click did not set duration");gallery.Stage.Duration=.3f;
                gallery.SelectShortcut("翻转切换");var card=gallery.Stage.ReferenceMainCard;var click=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left,clickCount=1};ExecuteEvents.Execute(card.gameObject,click,ExecuteEvents.pointerClickHandler);Check(gallery.Stage.Get("face")==0,"Single selection unexpectedly flipped");click.clickCount=2;ExecuteEvents.Execute(card.gameObject,click,ExecuteEvents.pointerClickHandler);Check(gallery.Stage.Get("face")==1,"Double click did not flip once");gallery.ChooseCurve(0);gallery.Stage.Seek(.2f);float outWidth=card.localScale.x;gallery.ChooseCurve(1);gallery.Stage.Seek(.2f);Check(card.localScale.x-outWidth>.5f,"Flip ignored curve selection");
                gallery.SelectShortcut("选中缩放");gallery.ChooseCurve(3);gallery.Stage.Seek(.4f);
                report.checks.Add("Workbench: raycast all four curves and card shortcuts; scale/flip use actual easing; single-select/double-flip; loop/reverse before play; pause/resume; 34px slider hit area");
            }catch(Exception e){report.errors.Add("Workbench slider: "+e);}
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(output,"workbench-curves.png"));yield return null;
            gallery.SelectShortcut("拖拽放置");yield return null;
            var stage=gallery.Stage;var item=stage.ReferenceDragItem;GameObject dragTarget=null;PointerEventData dragEvent=null;
            try{
                var point=RectTransformUtility.WorldToScreenPoint(null,item.position)+new Vector2(8,6);var hit=Hit(point);dragTarget=ExecuteEvents.GetEventHandler<IPointerDownHandler>(hit.gameObject);Check(dragTarget==item.gameObject,"Drag item is obstructed");dragEvent=new PointerEventData(EventSystem.current){position=point,pointerId=-80,button=PointerEventData.InputButton.Left};ExecuteEvents.Execute(dragTarget,dragEvent,ExecuteEvents.pointerDownHandler);var start=item.anchoredPosition;ExecuteEvents.Execute(dragTarget,dragEvent,ExecuteEvents.dragHandler);Check(Vector2.Distance(start,item.anchoredPosition)<.01f,"Pickup offset jumped");dragEvent.position=RectTransformUtility.WorldToScreenPoint(null,stage.ReferenceValidSlot.position)+new Vector2(8,6);ExecuteEvents.Execute(dragTarget,dragEvent,ExecuteEvents.dragHandler);Check(stage.Get("hoverSlot")==1,"Actual drag did not enter valid slot");ExecuteEvents.Execute(dragTarget,dragEvent,ExecuteEvents.pointerUpHandler);Check(stage.Get("drops")==1&&stage.Get("held")==0,"Actual drop not accepted");
                report.checks.Add("Pointer: actual raycast pickup, preserved grab offset, drag enter and accepted drop");
            }catch(Exception e){report.errors.Add("Pointer drag: "+e);}
            yield return new WaitForSecondsRealtime(.4f);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(output,"drag-accepted.png"));yield return null;
            try{
                var potion=stage.GetComponentsInChildren<RecipePointer>().Single(p=>p.name=="Potion item");var point=RectTransformUtility.WorldToScreenPoint(null,potion.transform.position);Check(ExecuteEvents.GetEventHandler<IPointerDownHandler>(Hit(point).gameObject)==potion.gameObject,"Inventory item obstructed");var e=new PointerEventData(EventSystem.current){position=point,pointerId=-81,button=PointerEventData.InputButton.Left};ExecuteEvents.Execute(potion.gameObject,e,ExecuteEvents.pointerDownHandler);e.position=RectTransformUtility.WorldToScreenPoint(null,stage.ReferenceValidSlot.position);ExecuteEvents.Execute(potion.gameObject,e,ExecuteEvents.dragHandler);Check(stage.Get("hoverSlot")==-1,"Potion accepted as crystal");ExecuteEvents.Execute(potion.gameObject,e,ExecuteEvents.pointerUpHandler);Check(stage.Get("rejected")==1&&stage.Get("drops")==1,"Wrong inventory type committed");
                var p=item.GetComponent<RecipePointer>();point=RectTransformUtility.WorldToScreenPoint(null,item.position);e=new PointerEventData(EventSystem.current){position=point,pointerId=-82,button=PointerEventData.InputButton.Left};p.OnPointerDown(e);var start=item.anchoredPosition;p.OnDrag(e);Check(Vector2.Distance(start,item.anchoredPosition)<.01f,"Re-grab during spring jumped");p.Cancel();Check(stage.Get("held")==0,"Cancellation left held state");
                report.checks.Add("Inventory: actual potion pointer rejects crystal slot; spring re-grab stays continuous; cancel releases pointer");
            }catch(Exception e){report.errors.Add("Inventory pointer: "+e);}
            gallery.ShowAllRecipes();gallery.Select("10.01");try{Check(!gallery.Stage.CurveEditable&&!gallery.Stage.ClockEditable,"Realtime toast exposed irrelevant controls");Check(!gallery.GetComponentsInChildren<Button>().Any(b=>b.name.StartsWith("Curve choice")),"Irrelevant curve controls remain visible");report.checks.Add("Inspector hides unsupported curve/strength/clock settings instead of exposing inert controls");}catch(Exception e){report.errors.Add(e.ToString());}
            foreach(string id in new[]{"02.02","04.06","06.07","11.02","05.06"}){
                gallery.Select(id);gallery.GuidedOperation();yield return new WaitForSecondsRealtime(id=="05.06"?.4f:.1f);
                try{var s=gallery.Stage;Check(id=="02.02"?s.Get("open")==0:id=="04.06"?s.Get("selected")==1:id=="06.07"?s.Get("valid")==1:id=="11.02"?s.Get("displayedRating")==2:s.Get("held")==0&&s.Get("clicks")==0,"Play did not execute the recipe-specific operation: "+id);}catch(Exception e){report.errors.Add(e.ToString());}
            }
            report.checks.Add("Play routes to close, next page, successful input, star reveal and outside-release sequences instead of blindly invoking the first action");
            gallery.SelectShortcut("卡牌展示");gallery.ChooseCurve(0);gallery.Select("03.08");
        }
        void CheckDirectory(){var directory=gallery.GetComponentsInChildren<ScrollRect>().First(s=>s.name=="Directory viewport");Check(directory.content.GetComponentsInChildren<Button>().Length==96,"Directory entry count");var button=directory.content.GetComponentsInChildren<Button>()[0];Vector3[] corners=new Vector3[4];button.GetComponent<RectTransform>().GetWorldCorners(corners);Check(corners[0].y>Screen.height*.1f&&corners[2].y<Screen.height,"Directory first row outside viewport");}
        static void CheckChannels(){var target=new GameObject("Lease fixture");try{var a=new MotionChannelArbiter();var p=new MotionBinding(target,MotionChannel.Position);var s=new MotionBinding(target,MotionChannel.Scale);object first=new object(),second=new object();int cancelled=0;a.Acquire(first,new[]{p},()=>cancelled++);a.Acquire(second,new[]{s},null);Check(a.Count==2&&cancelled==0,"Independent properties conflicted");a.Acquire(second,new[]{p,s},null);Check(cancelled==1&&a.Owns(second,p),"Conflicting owner not cancelled once");a.Release(second);Check(a.Count==0,"Leases leaked");}finally{Destroy(target);}}
        static void Click(RecipeStage s,string name)=>s.Trigger(name);
        static InputField Field(RecipeStage s)=>s.GetComponentInChildren<InputField>();
        static void Normal(RecipeStage s) {
            int g=s.Definition.Group,n=s.Definition.Number;
            switch(g){
                case 1:
                    if(n==1||n==7){Click(s,"进入页面");Check(s.Get("visible")==1,"Not visible");}
                    if(n==2){Click(s,"退出页面");Check(s.Get("visible")==0,"Still visible");}
                    if(n==3){Click(s,"切换栏目");Check(s.Get("column")==1,"Column unchanged");}
                    if(n==4){Click(s,"打开详情");Check(s.Get("depth")==1,"No detail depth");}
                    if(n==5){Click(s,"展开卡片");Check(s.Get("expanded")==1,"Not expanded");}
                    if(n==6){Click(s,"切换 HUD");Check(s.Get("hidden")==1,"HUD did not hide");}
                    if(n==8){Click(s,"切换宽窄布局");Check(s.Get("narrow")==1,"Layout unchanged");}break;
                case 2:
                    if(n<=4){Click(s,n==2?"关闭":"打开");Check(s.Get("open")== (n==2?0:1),"Modal visibility");}
                    if(n==5){Click(s,"展开菜单");Check(s.Get("open")==1,"Menu did not open");}
                    if(n==6){Click(s,"模拟停留");Check(s.Get("open")==1,"Tooltip not shown");}
                    if(n==7){Click(s,"下一步／上一步");Check(s.Get("selected")==1,"Modal step unchanged");}
                    if(n==8){Click(s,"尝试关闭");Check(s.Get("rejected")>=1&&s.Get("open")==1,"Unconfirmed close not rejected");}break;
                case 3:
                    string[] actions={"切换展开","切换展开","切换长短内容","吸附右档","更新任务","替换内容","加载／清空","翻面"};Click(s,actions[n-1]);
                    if(n==1)Check(s.Get("expanded")==1,"Not expanded");if(n==2)Check(s.Get("expanded")==0,"Not collapsed");if(n==3)Check(s.Get("long")==1,"Content unchanged");if(n==4)Check(s.Get("slot")==1,"Divider not snapped");if(n==5)Check(s.Get("updates")>0,"Task not updated");if(n==6)Check(s.Get("selected")==1,"Content not swapped");if(n==7)Check(s.Get("count")==3,"No loaded items");if(n==8)Check(s.Get("face")==1,"Card face not changed");break;
                case 4:
                    string[] nav={"选择下一项","移动焦点","选择下一项","完成当前步骤","进入下一级","下一页","选择下一项","选择下一项"};Click(s,nav[n-1]);
                    if(n==2)Check(s.Get("focus")==1,"Focus not moved");else if(n==4)Check(s.Get("step")==1,"Step did not advance");else if(n==5)Check(s.Get("depth")==1,"Breadcrumb not added");else Check(s.Get("selected")==1,"Navigation unchanged");break;
                case 5:
                    if(n==1||n==2){Click(s,"移入");Check(s.State=="Hovered","Hover missing");if(n==2){Click(s,"移出");Check(s.State=="Default","Hover exit missing");}}
                    else if(n==7){Click(s,"禁用／启用");Check(s.Get("enabled")==0,"Not disabled");}
                    else{Click(s,"按下");Check(s.Get("held")==1,"Not held");if(n==6){Click(s,"移出");Click(s,"松开");Check(s.Get("clicks")==0,"Outside release clicked");}else if(n==8){Click(s,"松开");Check(s.Get("charges")==0,"Early hold charged");}else{Click(s,"松开");Check(s.Get("clicks")==1,"Valid release not clicked");}}break;
                case 6:
                    if(n==1){Click(s,"切换开关");Check(s.Get("value")==1,"Toggle unchanged");}
                    if(n==2){Click(s,"勾选／取消");Check(s.Get("value")==1,"Checkbox unchanged");}
                    if(n==3){Click(s,"选择下一项");Check(s.Get("selected")==1,"Radio unchanged");}
                    if(n==4||n==5){Field(s).text="保留内容";Click(s,"聚焦输入");if(n==5)Click(s,"失焦");Check(Field(s).text=="保留内容","Input lost text");}
                    if(n==6){Field(s).text="x";Click(s,"提交校验");Check(s.Get("valid")==0,"Invalid input accepted");}
                    if(n==7){Click(s,"填入示例");Click(s,"提交校验");Check(s.Get("valid")==1,"Valid input rejected");}
                    if(n==8){Click(s,"设为 80");Check(s.Get("value")==80,"Slider value wrong");}break;
                case 7:
                    string[] list={"显示首屏","插入任务","删除首行","反转顺序","全部／未完成","模拟偏移后吸附","模拟越界","跳转任务08"};Click(s,list[n-1]);
                    if(n==2)Check(s.Get("count")==5,"Insert count");if(n==3)Check(s.Get("count")==3,"Delete count");if(n==4)Check(s.Get("firstId")==103,"Identity order");if(n==5)Check(s.Get("count")==2,"Filter count");if(n==6)Check(s.Get("snapped")==1,"Snap result");if(n==7)Check(s.Get("overscrolled")==1,"Boundary operation");if(n==8)Check(s.Get("targetId")==107,"Wrong target");break;
                case 8:
                    Click(s,"模拟拾起");if(n==4||n==6)Click(s,"移到无效槽");else if(n>=2)Click(s,"移到有效槽");
                    if(n==5||n==6||n==7)Click(s,"松开放置");if(n==8)Click(s,"取消");
                    if(n==1)Check(s.Get("held")==1,"Not picked");if(n==3)Check(s.Get("hoverSlot")==1,"Not valid");if(n==4)Check(s.Get("hoverSlot")==-1,"Not invalid");if(n==5)Check(s.Get("slot")==1,"Drop not committed");if(n==6)Check(s.Get("rejected")==1&&s.Get("drops")==0,"Invalid drop committed");if(n==7)Check(s.Get("order")==1,"Sort not committed");if(n==8)Check(s.Get("cancelled")==1&&s.Get("drops")==0,"Cancel committed");break;
                case 9:
                    string[] value={"目标 +100","收入 +100","受到 25 伤害","治疗 +25","获得 150 经验","开始 10 秒","开始加载","完成一个真实任务"};Click(s,value[n-1]);
                    if(n==1||n==2)Check(s.Get("value")==200,"Value not authoritative");if(n==3)Check(s.Get("value")==75,"Damage value");if(n==4)Check(s.Get("value")==55,"Heal value");if(n==5)Check(s.Get("level")==3&&s.Get("xp")==20,"XP remainder lost");if(n==6)Check(s.Get("running")==1,"Countdown not running");if(n==7)Check(s.Get("loading")==1,"Loading not running");if(n==8)Check(s.Get("tasks")==1&&s.Get("complete")==0,"Tasks completed prematurely");break;
                case 10:
                    string[] notice={"发送新消息","模拟成功","模拟失败","检查资源","增加未读","请求／重试","开始引导","移动目标"};Click(s,notice[n-1]);
                    if(n==1)Check(s.Get("queued")==1,"Toast not queued");if(n==2)Check(s.Get("result")==1,"Wrong success");if(n==3||n==4)Check(s.Get("result")==-1,"Wrong failure");if(n==5)Check(s.Get("unread")==1,"Unread unchanged");if(n==6)Check(s.Get("loading")==1,"Request not loading");if(n==7)Check(s.Get("guiding")==1,"Guide inactive");if(n==8)Check(s.Get("moved")==1,"Anchor unchanged");break;
                case 11:
                    string[] reward={"确认并揭示","展示星级","开启宝箱","领取 100 金币","解锁成就","等级 +1","更新比赛结果","开始结算"};Click(s,reward[n-1]);
                    if(n==1||n==3||n==4||n==8)Check(s.Get("grants")==1&&s.Get("wallet")==1100,"Reward not exactly once");if(n==2)Check(s.Get("displayedRating")==2,"Rating wrong");if(n==5)Check(s.Get("unlocked")==1,"Not unlocked");if(n==6)Check(s.Get("level")==2,"Level unchanged");if(n==7)Check(s.Get("playerId")==999&&s.Get("playerRank")==1,"Ranking identity");break;
                case 12:Click(s,"开启");Check(s.Get("running")==1,"Loop inactive");break;
            }
        }
        static void Boundary(RecipeStage s) {
            int g=s.Definition.Group,n=s.Definition.Number;
            if(g==5){Click(s,"按下");Click(s,"移出");Click(s,"松开");Check(s.Get("clicks")==0&&s.Get("held")==0,"Cancel leaked click/press");return;}
            if(g==8){Click(s,"移到无效槽");Click(s,"松开放置");if(n!=7)Check(s.Get("drops")==0,"Rejected accepted");Click(s,"模拟拾起");Click(s,"取消");Check(s.Get("held")==0,"Cancel stuck held");return;}
            if(g==11&&(n==1||n==3||n==4||n==8)){Normal(s);Normal(s);if(n==8){Click(s,"跳过并收尾");Click(s,"跳过并收尾");}Check(s.Get("grants")==1,"Duplicate grant");return;}
            if(g==7&&n==3){for(int i=0;i<8;i++)Click(s,"删除首行");Check(s.Get("count")==0,"Empty list removal");return;}
            if(g==7&&n==4){Click(s,"反转顺序");Click(s,"反转顺序");Check(s.Get("firstId")==100,"Identity not restored");return;}
            if(g==9&&n==1){for(int i=0;i<20;i++)Click(s,"目标 +100");Check(s.Get("target")==2100,"Latest target lost");return;}
            if(g==9&&n==2){Click(s,"支出 -80");Click(s,"支出 -80");Check(s.Get("value")==20&&s.Get("rejected")==1,"Overspent");return;}
            if(g==9&&n==4){for(int i=0;i<8;i++)Click(s,"治疗 +25");Check(s.Get("value")==100,"Heal exceeded max");return;}
            if(g==9&&n==8){for(int i=0;i<8;i++)Click(s,"完成一个真实任务");Check(s.Get("tasks")==4&&s.Get("complete")==1,"Task bound");return;}
            if(g==10&&n==1){for(int i=0;i<10;i++)Click(s,"发送新消息");Check(s.Get("queued")<=3,"Queue unbounded");return;}
            if(g==10&&n==6){Click(s,"请求／重试");Click(s,"请求失败");Click(s,"请求／重试");Click(s,"旧响应返回");Check(s.Get("loading")==1&&s.Get("staleIgnored")==1,"Stale request overwrote current");return;}
            if(g==6&&(n==6||n==7)){Field(s).text="";Click(s,"提交校验");Check(s.Get("valid")==0,"Empty input accepted");Click(s,"填入示例");Click(s,"提交校验");Check(s.Get("valid")==1,"Retry failed");return;}
            if(g==12){Click(s,"开启");Click(s,"开启");Click(s,"停止");Check(!s.IsPlaying&&s.Get("running")==0,"Loop did not stop");return;}
            // Every remaining recipe exercises cancellation at an in-flight point, then reset by the host.
            Normal(s);if(s.CanSeek)s.Seek(.35f);s.CancelInteractions();s.Shutdown();Check(!s.IsPlaying,"Cancelled scene still playing");
        }
        static bool Finite(RecipeStage s){foreach(var r in s.GetComponentsInChildren<RectTransform>(true)){var p=r.anchoredPosition;var z=r.localScale;if(float.IsNaN(p.x)||float.IsInfinity(p.y)||float.IsNaN(z.x)||float.IsInfinity(z.y))return false;}return true;}
        void Update(){if(running&&Time.realtimeSinceStartup-started>240){report.errors.Add("Watchdog timeout");Finish();}}
        void Log(string message,string stack,LogType type){if(type==LogType.Exception||type==LogType.Error||type==LogType.Assert)report.errors.Add(message+"\n"+stack);}
        void Finish(){if(!running)return;running=false;Application.logMessageReceived-=Log;report.passed=report.cases.Count==96&&report.cases.All(c=>c.normal&&c.boundary&&c.sampling&&c.reduced&&c.cleanup)&&report.errors.Count==0;File.WriteAllText(Path.Combine(output,"report.json"),JsonUtility.ToJson(report,true));Application.Quit(report.passed?0:2);}
    }
}
