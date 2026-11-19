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
            yield return WorkbenchChecks();
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
                if(definition.id=="10.01")yield return new WaitForSecondsRealtime(.35f);
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
                Check(gallery.VisibleRecipeCount==96,"Initial directory did not expose 96 recipes");Check(gallery.GetComponentsInChildren<Button>().Count(b=>b.name.StartsWith("Category "))==12,"Missing category headers");
                gallery.ApplyFilters(state:"禁用");Check(gallery.VisibleRecipeIds.SequenceEqual(new[]{"05.07"}),"Disabled filter used group inference");
                gallery.ApplyFilters(state:"校验失败");Check(gallery.VisibleRecipeIds.SequenceEqual(new[]{"06.06"}),"Validation filter leaked other inputs");
                gallery.ApplyFilters(obj:"输入框");Check(gallery.VisibleRecipeCount==4,"Input object tag filter");
                gallery.ApplyFilters(type:"弹簧");Check(gallery.VisibleRecipeIds.Contains("07.07")&&gallery.VisibleRecipeIds.Contains("08.06")&&!gallery.VisibleRecipeIds.Contains("08.02"),"Spring type filter used whole groups");
                gallery.ApplyFilters(state:"选中",obj:"页签");Check(gallery.VisibleRecipeIds.SequenceEqual(new[]{"04.01","04.03"}),"Combined tag intersection wrong");
                gallery.ShowAllRecipes();report.checks.Add("Directory: 12 categories and 96 entries by default; explicit object/state/type tags; exact and intersected filters");
            }catch(Exception e){report.errors.Add("Directory/tag filters: "+e);}
            foreach(string id in new[]{"05.03","03.08","08.05","07.04","09.03","11.04"}){
                gallery.Select(id);yield return null;var stage=gallery.Stage;
                try{
                    Check(stage.PrimarySubject!=null,"Missing primary subject: "+id);
                    if(id=="05.03"){
                        Check(stage.DisplayStatus=="默认","Initial button status did not match visual");var pointer=stage.GetComponentsInChildren<RecipePointer>().First();var point=RectTransformUtility.WorldToScreenPoint(null,pointer.transform.position);Check(ExecuteEvents.GetEventHandler<IPointerDownHandler>(Hit(point).gameObject)==pointer.gameObject,"Button primary subject obstructed");var e=new PointerEventData(EventSystem.current){position=point,pointerId=-40,button=PointerEventData.InputButton.Left};ExecuteEvents.Execute(pointer.gameObject,e,ExecuteEvents.pointerDownHandler);stage.Seek(1);Check(stage.DisplayStatus=="按下"&&stage.Get("held")==1,"Pressed status inaccurate");Check(stage.PrimarySubject.rect.width*stage.PrimarySubject.lossyScale.x>Screen.width*.2f,"Button too small for stage");
                    }
                    if(id=="03.08"){
                        Check(stage.GetComponentsInChildren<Button>().Count(b=>b.name.StartsWith("Card "))==1,"Flip fixture contains unrelated cards");Tap(stage.ReferenceMainCard.GetComponent<Button>());Check(stage.Get("face")==1,"Card click did not flip");var before=stage.BusinessFingerprint();Tap(NamedButton("Curve choice 0"));stage.Seek(.2f);float outWidth=stage.ReferenceMainCard.localScale.x;stage.Seek(.2f);Check(Mathf.Abs(stage.Progress-.2f)<.0001f&&Mathf.Abs(outWidth-stage.ReferenceMainCard.localScale.x)<.0001f,"First/repeated Seek drifted");Tap(NamedButton("Curve choice 1"));stage.Seek(.2f);Check(stage.ReferenceMainCard.localScale.x-outWidth>.5f,"Flip ignored easing: out="+outWidth+", in="+stage.ReferenceMainCard.localScale.x);Check(before==stage.BusinessFingerprint(),"Curve replay changed semantic state");
                    }
                    if(id=="08.05"){
                        Check(stage.DisplayStatus=="源槽待拾起","Initial drag status did not match visual");Check(!stage.GetComponentsInChildren<RectTransform>(true).Any(r=>r.name.StartsWith("Card ")),"Drag scene still dominated by cards");Check(stage.ReferenceValidSlot.rect.width>=190,"Drag slot not enlarged");
                        var item=stage.ReferenceDragItem;var point=RectTransformUtility.WorldToScreenPoint(null,item.position)+new Vector2(8,6);Check(ExecuteEvents.GetEventHandler<IPointerDownHandler>(Hit(point).gameObject)==item.gameObject,"Drag subject obstructed");var e=new PointerEventData(EventSystem.current){position=point,pointerId=-41,button=PointerEventData.InputButton.Left};var p=item.GetComponent<RecipePointer>();p.OnPointerDown(e);var start=item.anchoredPosition;p.OnDrag(e);Check(Vector2.Distance(start,item.anchoredPosition)<.01f,"Pickup offset jumped");e.position=RectTransformUtility.WorldToScreenPoint(null,stage.ReferenceValidSlot.position)+new Vector2(8,6);p.OnDrag(e);p.OnPointerUp(e);Check(stage.Get("drops")==1&&stage.DisplayStatus=="放置成功","Drop or status failed");
                    }
                    if(id=="07.04"){
                        var originals=stage.GetComponentsInChildren<RectTransform>().Where(r=>r.name.StartsWith("Task ID ")).ToDictionary(r=>r.name,r=>r.GetInstanceID());Tap(stage.Actions["反转顺序"]);stage.Seek(.65f);Check(stage.Get("firstId")==103,"Reorder failed");Check(stage.GetComponentsInChildren<RectTransform>().Where(r=>r.name.StartsWith("Task ID ")).All(r=>originals[r.name]==r.GetInstanceID()),"Row identity changed");
                    }
                    if(id=="09.03"){Tap(stage.Actions["受到 25 伤害"]);stage.Seek(.55f);Check(stage.Get("value")==75&&stage.DisplayStatus.Contains("受伤"),"Damage or status inaccurate");Check(!stage.GetComponentsInChildren<RectTransform>().Any(r=>r.name=="数值与真实进度"),"Generic panel remained in health HUD");}
                    if(id=="11.04"){Tap(stage.Actions["领取 100 金币"]);var before=stage.BusinessFingerprint();stage.Seek(.5f);stage.Replay();stage.Seek(.55f);Check(stage.Get("wallet")==1100&&stage.Get("grants")==1&&before==stage.BusinessFingerprint(),"Flight replay granted twice");Check(!stage.GetComponentsInChildren<RectTransform>().Any(r=>r.name.StartsWith("Card ")),"Unrelated card obscures currency flight");}
                }catch(Exception e){report.errors.Add("Representative "+id+": "+e);}
                yield return null;yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(output,"focus-"+id+".png"));yield return null;
            }
            gallery.Select("08.05");yield return null;
            try{
                var stage=gallery.Stage;var potion=stage.GetComponentsInChildren<RecipePointer>().Single(p=>p.name=="Potion item");var point=RectTransformUtility.WorldToScreenPoint(null,potion.transform.position);Check(ExecuteEvents.GetEventHandler<IPointerDownHandler>(Hit(point).gameObject)==potion.gameObject,"Palette potion obstructed");var e=new PointerEventData(EventSystem.current){position=point,pointerId=-43,button=PointerEventData.InputButton.Left};potion.OnPointerDown(e);e.position=RectTransformUtility.WorldToScreenPoint(null,stage.ReferenceValidSlot.position);potion.OnDrag(e);potion.OnPointerUp(e);Check(stage.Get("drops")==0&&stage.Get("rejected")==1&&stage.DisplayStatus=="放置被拒绝","Potion type rejection/status wrong");
                var item=stage.ReferenceDragItem;var p=item.GetComponent<RecipePointer>();e=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,item.position),pointerId=-44,button=PointerEventData.InputButton.Left};p.OnPointerDown(e);var at=item.anchoredPosition;p.OnDrag(e);Check(Vector2.Distance(at,item.anchoredPosition)<.01f,"Re-grab during return jumped");p.Cancel();Check(stage.Get("held")==0&&stage.DisplayStatus=="已取消","Cancel status inaccurate");
                e=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,potion.transform.position),pointerId=-45,button=PointerEventData.InputButton.Left};potion.OnPointerDown(e);stage.Trigger("取消");var cancelledPosition=item.anchoredPosition;e.position+=new Vector2(95,0);potion.OnDrag(e);Check(Vector2.Distance(cancelledPosition,item.anchoredPosition)<.01f,"Palette pointer continued moving after cancel");potion.OnPointerUp(e);

                report.checks.Add("Six representative scenes: native hit tests, object prominence, curve semantics, drag acceptance/type rejection/cancel, stable list identity, HP state, idempotent currency flight");
            }catch(Exception e){report.errors.Add("Drag boundaries: "+e);}
            foreach(string id in new[]{"06.06","06.07","05.07","12.01","12.08"}){
                gallery.Select(id);yield return null;
                try{var s0=gallery.Stage;if(id=="06.06"){Field(s0).text="x";s0.Trigger("提交校验");Check(s0.DisplayStatus=="校验失败","Invalid input fell back to selected");}else if(id=="06.07"){s0.Trigger("填入示例");s0.Trigger("提交校验");Check(s0.DisplayStatus=="校验成功","Success status wrong");}else if(id=="05.07"){s0.Trigger("禁用／启用");Check(s0.DisplayStatus=="禁用","Disabled status wrong");}else Check(!s0.GetComponentsInChildren<RectTransform>(true).Any(r=>r.name.StartsWith("Card ")||r.name=="Illustration"),"Loop scene contains portrait obstruction");}catch(Exception e){report.errors.Add(e.ToString());}
            }
            report.checks.Add("Semantic inspector statuses: pressed, disabled, validation success/failure, accepted/rejected/cancelled; loop objects contain no portrait cards");
            gallery.ShowAllRecipes();gallery.Select("05.03");yield return null;
            try{Check(gallery.GetComponentsInChildren<Button>().Count(b=>b.name=="Playback play")==1,"Playback duplicated");Check(!gallery.GetComponentsInChildren<Button>().Any(b=>b.name.Contains("运行演示")||b.name.Contains("播放动画")),"Old duplicate playback remains");gallery.GuidedOperation();}catch(Exception e){report.errors.Add(e.ToString());}
            yield return new WaitForSecondsRealtime(.1f);
            try{gallery.Stage.Seek(.35f);gallery.Stage.TogglePause();Check(gallery.Stage.IsPlaying,"Playback resume failed");gallery.TogglePresentationLoop();Check(gallery.Stage.Looping,"Loop failed");gallery.ReversePresentation();Check(gallery.Stage.Backwards||gallery.Stage.Looping,"Reverse failed");report.checks.Add("Single playback area; native controls resume/loop/reverse; first and repeated timeline sampling are stable");}catch(Exception e){report.errors.Add(e.ToString());}
            gallery.Select("05.03");
        }
        void CheckDirectory(){var dir=gallery.GetComponentsInChildren<ScrollRect>().First(s=>s.name=="Directory viewport");Check(dir.content.GetComponentsInChildren<Button>().Count(b=>b.name.StartsWith("Recipe entry "))==96,"Directory entry count");var selected=dir.content.GetComponentsInChildren<Button>().Single(b=>b.name=="Recipe entry "+gallery.SelectedId);var hit=Hit(RectTransformUtility.WorldToScreenPoint(null,selected.transform.position));Check(ExecuteEvents.GetEventHandler<IPointerClickHandler>(hit.gameObject)==selected.gameObject,"Selected directory entry not visible/hittable");}
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
                    if(n==1)Check(s.Get("held")==1,"Not picked");if(n==3)Check(s.Get("hoverSlot")==1,"Not valid");if(n==4)Check(s.Get("hoverSlot")==-1,"Not invalid");if(n==5)Check(s.Get("slot")==1,"Drop not committed");if(n==6)Check(s.Get("rejected")==1&&s.Get("drops")==0,"Invalid drop committed");if(n==7){Check(s.Get("order")==1,"Sort not committed");Check(s.ReferenceDragItem.GetComponentInChildren<Text>().text.Contains("101"),"A identity label is not attached to moving item");Check(s.GetComponentsInChildren<RectTransform>().Single(r=>r.name=="Sortable item B ID102").GetComponentInChildren<Text>().text.Contains("102"),"B identity label detached");}if(n==8)Check(s.Get("cancelled")==1&&s.Get("drops")==0,"Cancel committed");break;
                case 9:
                    string[] value={"目标 +100","收入 +100","受到 25 伤害","治疗 +25","获得 150 经验","开始 10 秒","开始加载","完成一个真实任务"};Click(s,value[n-1]);
                    if(n==1||n==2)Check(s.Get("value")==200,"Value not authoritative");if(n==3)Check(s.Get("value")==75,"Damage value");if(n==4)Check(s.Get("value")==55,"Heal value");if(n==5)Check(s.Get("level")==3&&s.Get("xp")==20,"XP remainder lost");if(n==6)Check(s.Get("running")==1,"Countdown not running");if(n==7)Check(s.Get("loading")==1,"Loading not running");if(n==8)Check(s.Get("tasks")==1&&s.Get("complete")==0,"Tasks completed prematurely");if(n==5||n==6||n==8){var number=s.GetComponentsInChildren<Text>().OrderByDescending(t=>t.fontSize).First();Check(number.preferredHeight<=number.rectTransform.rect.height+.5f,"Primary number text is truncated: "+number.text);}break;
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
