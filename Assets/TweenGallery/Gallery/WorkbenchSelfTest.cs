using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TweenGallery
{
    public sealed class WorkbenchSelfTest : MonoBehaviour
    {
        [Serializable] class Report { public bool passed; public string version; public int width,height; public List<string> checks=new List<string>(),errors=new List<string>(); }
        Report report=new Report();string folder;bool running;float deadline;
        void Check(bool ok,string label){if(ok)report.checks.Add(label);else report.errors.Add(label);}
        void Log(string message,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)report.errors.Add(message);}
        void Update(){if(running&&Time.realtimeSinceStartup>deadline){report.errors.Add("Unified preview timeout");Finish();}}
        IEnumerator Start()
        {
            var args=Environment.GetCommandLineArgs();if(Array.IndexOf(args,"--workbench-test")<0)yield break;
            int at=Array.IndexOf(args,"--test-output");folder=at>=0?args[at+1]:Path.Combine(Application.persistentDataPath,"workbench-test");Directory.CreateDirectory(folder);
            running=true;deadline=Time.realtimeSinceStartup+150;Application.logMessageReceived+=Log;report.version=Application.version;report.width=Screen.width;report.height=Screen.height;
            yield return null;yield return null;var gallery=GetComponent<GalleryBootstrap>();
            Check(gallery.GetComponentsInChildren<Button>(true).All(b=>{var text=b.GetComponentInChildren<Text>();return text==null||text.text!="交互实验室"&&text.text!="按钮工坊";}),"No separate lab/workshop entry buttons");
            gallery.SetCategory("B");gallery.SelectEffect("B01");gallery.SetButtonSkin(3);yield return null;
            Check(gallery.CurrentPlayer!=null&&gallery.CurrentPlayer.bindings.target.GetComponent<Image>().sprite==GalleryTheme.Skin(3),"Main timeline uses the shared selected skin");
            Check(gallery.CurrentPlayer.GetComponentsInChildren<MotionPointerFeedback>(true).All(f=>!f.enabled)&&gallery.CurrentPlayer.GetComponentsInChildren<Button>(true).All(b=>!b.enabled),"Automatic button preview does not capture held input");
            for(int skin=0;skin<6;skin++)
            {
                gallery.SetButtonSkin(skin);yield return null;var target=gallery.CurrentPlayer.bindings.target;
                Check(target.GetComponent<Image>().sprite==GalleryTheme.Skin(skin),"Timeline material selection "+skin);
                if(skin==2||skin==5){var star=target.Find("Star");Check(star!=null&&star.GetComponent<Graphic>().color.r<.6f&&target.GetComponentsInChildren<Text>().Length==0,"Compact timeline shape keeps a readable independent glyph");}
            }
            gallery.SetButtonSkin(3);
            Capture(gallery,"01-timeline-skin.png");gallery.SetInteractionMode(true);
            for(int skin=0;skin<6;skin++)for(int variant=0;variant<8;variant++)
            {
                gallery.SetButtonSkin(skin);gallery.SetFeedbackVariant(variant);yield return null;
                var view=gallery.IntegratedPreview;var b=view.Feedback;
                Check(b!=null&&b.style==(ButtonMotionStyle)variant&&b.surface.GetComponent<Image>().sprite==GalleryTheme.Skin(skin),"Shared skin / feedback combination "+skin+" / "+variant);
                Vector3 hitScale=b.transform.localScale;var e=Pointer(-1);
                ExecuteEvents.Execute(b.gameObject,e,ExecuteEvents.pointerDownHandler);b.Step(.08f);
                Check(b.IsPressed&&b.transform.localScale==hitScale,"Fixed hit with real press "+skin+" / "+variant);
                ExecuteEvents.Execute(b.gameObject,e,ExecuteEvents.pointerUpHandler);if(e.eligibleForClick)ExecuteEvents.Execute(b.gameObject,e,ExecuteEvents.pointerClickHandler);
                Check(view.ClickCount==1,"One native business click "+skin+" / "+variant);
                if(variant==7){Check(b.IsBusy&&!view.ActionButton.interactable,"Loading blocks duplicate requests");view.ChangeState();Check(!b.IsBusy&&view.ActionButton.interactable,"Completion restores interactivity");}
            }
            gallery.SetButtonSkin(2);gallery.SetFeedbackVariant(3);yield return null;
            var config=JsonUtility.FromJson<GalleryBootstrap.GalleryConfiguration>(gallery.ExportConfigurationJson());
            Check(config.schemaVersion==2&&config.previewMode=="interaction"&&config.skinIndex==2&&config.interaction.driver=="MotionButtonFeedback"&&config.interaction.variant=="IconPop","Export carries actual skin, driver and variant");
            var strength=gallery.GetComponentsInChildren<Slider>().First(s=>s.name=="强度 slider");strength.value=.8f;yield return null;
            Check(Mathf.Abs(gallery.IntegratedPreview.Feedback.intensity-.8f)<.001f,"Main parameter slider updates live feedback");
            var frequency=gallery.GetComponentsInChildren<Slider>().First(s=>s.name=="时长 slider");frequency.value=9.5f;yield return null;
            config=JsonUtility.FromJson<GalleryBootstrap.GalleryConfiguration>(gallery.ExportConfigurationJson());
            Check(Mathf.Abs(gallery.IntegratedPreview.Feedback.frequency-9.5f)<.001f&&Mathf.Abs(config.interaction.frequency-9.5f)<.001f&&config.interaction.duration==0,"Frequency UI and export match actual spring timing");
            Check(!gallery.GetComponentsInChildren<Slider>().First(s=>s.name=="Preview timeline").interactable,"Timeline disabled only during input-driven interaction");
            Capture(gallery,"02-live-icon-button.png");
            gallery.SetFeedbackVariant(7);gallery.IntegratedPreview.ActionButton.onClick.Invoke();yield return new WaitForSecondsRealtime(.12f);Capture(gallery,"03-live-loading.png");
            foreach(string id in new[]{"B07","B08"})
            {
                gallery.SelectEffect(id);var toggle=gallery.IntegratedPreview;toggle.ActionButton.onClick.Invoke();yield return new WaitForSecondsRealtime(.06f);
                var position=toggle.Player.bindings.target.anchoredPosition;var scale=toggle.Player.bindings.target.localScale;
                toggle.ActionButton.onClick.Invoke();Check(Vector2.Distance(position,toggle.Player.bindings.target.anchoredPosition)<.001f&&Vector3.Distance(scale,toggle.Player.bindings.target.localScale)<.001f,id+" live state reversal preserves visible pose");
            }
            gallery.SetCategory("G");gallery.SelectEffect("G01");yield return null;var drag=gallery.IntegratedPreview.DragInput;var jelly=gallery.IntegratedPreview.Jelly;
            Check(drag!=null&&jelly!=null,"Drag preview lives in main catalog");
            var start=RectTransformUtility.WorldToScreenPoint(null,drag.hitArea.TransformPoint(Vector3.zero));var pointer=Pointer(-1);pointer.position=start;
            drag.OnPointerDown(pointer);pointer.position=start+new Vector2(130,40);drag.OnBeginDrag(pointer);yield return new WaitForSecondsRealtime(.16f);
            Check(jelly.Offset.magnitude>1&&drag.HasActivePointer,"Drag input changes the dedicated visual");Capture(gallery,"04-drag.png");
            drag.OnEndDrag(pointer);yield return new WaitForSecondsRealtime(.9f);Check(jelly.Offset.magnitude<.5f,"Release returns to stable baseline");
            pointer.position=start;drag.OnPointerDown(pointer);gallery.SelectEffect("G02");Check(!drag.HasActivePointer,"Changing catalog item cancels captured drag");yield return null;
            var continuing=gallery.IntegratedPreview;continuing.Continue(true);yield return new WaitForSecondsRealtime(.12f);
            var before=MotionPose.Read(continuing.Player.bindings.target,continuing.Player.bindings.group);continuing.Continue(false);var after=MotionPose.Read(continuing.Player.bindings.target,continuing.Player.bindings.group);
            Check(Vector2.Distance(before.position,after.position)<.001f,"Interruption starts from the visible pose");Capture(gallery,"05-continuation.png");
            config=JsonUtility.FromJson<GalleryBootstrap.GalleryConfiguration>(gallery.ExportConfigurationJson());Check(config.effects.Length==0&&config.interaction.definitionId=="G02"&&config.interaction.variant=="ContinueTo","Interactive-only recipe export avoids unknown MotionPlayer IDs");
            gallery.SetReducedMotion(true);gallery.IntegratedPreview.Continue(true);Check(!gallery.IntegratedPreview.Player.IsPlaying,"Reduced continuation reaches its target without a running tween");gallery.SetReducedMotion(false);
            gallery.SetCategory("");gallery.SelectEffect("A04");gallery.SetInteractionMode(false);yield return null;
            Check(gallery.CurrentPlayer!=null&&gallery.GetComponentsInChildren<GalleryInteractionPreview>().Length==0,"Return to automatic playback cleans live controllers");
            Check(gallery.GetComponentsInChildren<Slider>().First(s=>s.name=="Preview timeline").interactable,"Timeline is restored in automatic mode");
            Check(gallery.GetComponentsInChildren<InteractionLab>(true).Length==0&&gallery.GetComponentsInChildren<ButtonStyleLab>(true).Length==0,"Normal flow never creates legacy diagnostic pages");
            Capture(gallery,"06-main-gallery.png");Finish();
        }
        static PointerEventData Pointer(int id)=>new PointerEventData(EventSystem.current){pointerId=id,button=PointerEventData.InputButton.Left,position=Vector2.zero,eligibleForClick=true};
        void Capture(GalleryBootstrap gallery,string file){LibraryReview.CaptureCanvas(gallery.GetComponentInChildren<Canvas>(),Path.Combine(folder,file));}
        void Finish(){if(!running)return;running=false;Application.logMessageReceived-=Log;report.passed=report.errors.Count==0;File.WriteAllText(Path.Combine(folder,"workbench-report.json"),JsonUtility.ToJson(report,true));Debug.Log("WORKBENCH_TEST "+(report.passed?"PASS":"FAIL"));Application.Quit(report.passed?0:1);}
    }
}
