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
    public sealed class PresentationSelfTest : MonoBehaviour
    {
        [Serializable] class Report { public bool passed;public string version;public int width,height;public List<string> checks=new List<string>(),errors=new List<string>(); }
        readonly Report report=new Report();string folder;bool running;float deadline;
        void Check(bool value,string label){if(value)report.checks.Add(label);else report.errors.Add(label);}
        static bool Near(float a,float b)=>Mathf.Abs(a-b)<.001f;
        void Log(string message,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)report.errors.Add(message);}
        void Update(){if(running&&Time.realtimeSinceStartup>deadline){report.errors.Add("Presentation test timeout");Finish();}}
        IEnumerator Start()
        {
            var args=Environment.GetCommandLineArgs();if(Array.IndexOf(args,"--presentation-test")<0)yield break;
            int at=Array.IndexOf(args,"--test-output");folder=at>=0?args[at+1]:Path.Combine(Application.persistentDataPath,"presentation-test");Directory.CreateDirectory(folder);
            running=true;deadline=Time.realtimeSinceStartup+120;Application.logMessageReceived+=Log;report.version=Application.version;report.width=Screen.width;report.height=Screen.height;
            yield return null;yield return null;var gallery=GetComponent<GalleryBootstrap>();
            var baseline=MotionSettings.FromDefinition(MotionCatalog.Find("A04"));var copy=JsonUtility.ToJson(baseline);
            foreach(MotionStyleKind kind in Enum.GetValues(typeof(MotionStyleKind)))
            {
                var profile=MotionStyleProfile.Get(kind);var effective=profile.Apply(baseline);
                Check(JsonUtility.ToJson(baseline)==copy,"Profile leaves canonical settings unchanged: "+kind);
                Check(Near(effective.duration,baseline.duration*profile.timeScale),"Profile duration mapping: "+kind);
                gallery.SelectEffect("A04");gallery.SelectStyle(kind);gallery.SeekPreview(.6f);yield return null;
                Check(Near(gallery.CurrentPlayer.settings.duration,effective.duration),"Gallery applies effective profile: "+kind);
                Capture(gallery,"01-style-"+kind+".png");
            }
            gallery.SelectStyle(MotionStyleKind.Standard);gallery.SelectStyle(MotionStyleKind.Celebratory);gallery.SelectStyle(MotionStyleKind.Standard);
            Check(Near(gallery.CurrentPlayer.settings.duration,baseline.duration),"Repeated profile changes derive from canonical values without accumulation");
            gallery.SetCategory("");gallery.SelectEffect("P01");gallery.SetInteractionMode(false);yield return null;
            var page=gallery.PagePreview;var sequence=page.Sequence;sequence.Pause();int completed=0;sequence.onVisualComplete.AddListener(()=>completed++);
            Check(gallery.CurrentPlayer==null,"Page recipe is not a MotionPlayer catalog ID");
            for(int i=0;i<4;i++)
            {
                sequence.Seek(new[]{0,.25f/1.1f,.65f/1.1f,1}[i]);yield return null;Capture(gallery,"02-page-stage-"+i+".png");
                Check(!sequence.IsPlaying&&completed==0,"Stage sampling is paused and silent: "+i);
            }
            Check(sequence.actionsGroup.interactable&&Near(sequence.WalletProgress,1),"Page end opens actions and completes visual wallet progression");
            sequence.Seek(.4f);gallery.TogglePause();sequence.Step(sequence.duration*.1f);gallery.TogglePause();
            Check(Near(sequence.Progress,.5f)&&!sequence.IsPlaying,"Gallery pause control resumes from a stage seek and then pauses");
            sequence.Play();sequence.Step(sequence.duration*2);sequence.Step(1);
            Check(completed==1&&!sequence.IsPlaying,"Actual playback emits visual completion once");
            sequence.reducedMotion=true;sequence.Play();Check(completed==2&&Near(sequence.Progress,1)&&!sequence.IsPlaying,"Reduced motion reaches the final pose and completes once");sequence.reducedMotion=false;
            Check(page.PreviewActionCount==0,"Sampling and playback do not invoke action or business callbacks");
            NativeClick(page.GetComponentsInChildren<Button>().First(b=>b.name=="Continue"));
            Check(page.PreviewActionCount==1,"Page final action dispatches exactly one native button click");
            sequence.Restore();var p=sequence.panel.anchoredPosition;var s=sequence.panel.localScale;float alpha=sequence.panelGroup.alpha;
            sequence.Seek(.4f);sequence.Restore();Check(Vector2.Distance(p,sequence.panel.anchoredPosition)<.001f&&Vector3.Distance(s,sequence.panel.localScale)<.001f&&Near(alpha,sequence.panelGroup.alpha),"Restore recovers exact captured panel pose and opacity");
            sequence.actionsGroup.interactable=false;sequence.actionsGroup.blocksRaycasts=false;sequence.CaptureBaseline();sequence.Seek(1);
            Check(!sequence.actionsGroup.interactable&&!sequence.actionsGroup.blocksRaycasts,"Page animation respects host-disabled actions");
            sequence.Restore();sequence.actionsGroup.interactable=true;sequence.actionsGroup.blocksRaycasts=true;sequence.CaptureBaseline();sequence.Seek(1);
            var config=JsonUtility.FromJson<GalleryBootstrap.GalleryConfiguration>(gallery.ExportConfigurationJson());
            Check(config.schemaVersion==3&&config.recipe.driver=="MotionPageSequence"&&config.effects.Length==0,"Page export uses schema 3 and an explicit recipe driver");
            Check(config.recipe.panelAsset=="StyleArt/cream-panel"&&config.recipe.milestones.Length==4,"Page export includes raster resource paths and stage marks");
            gallery.SelectEffect("P02");yield return null;var view=gallery.ResponsivePreview;var layouts=view.Layouts;
            Check(layouts.Length==3&&layouts[0].IsScrollable&&!layouts[1].IsScrollable&&!layouts[2].IsScrollable,"Short screen scrolls; tall and landscape fit their content");
            Check(layouts[2].IsLandscape,"Landscape uses the horizontal card layout");
            for(int i=0;i<3;i++)
            {
                var l=layouts[i];Check(Inside(l.safeRoot,l.actions)&&Inside(l.safeRoot,l.viewport),"Safe content contains viewport and fixed actions: "+i);
                Check(!Overlap(l.viewport,l.actions),"Scrollable body and fixed actions do not overlap: "+i);
            }
            Capture(gallery,"03-responsive-compare.png");
            view.SelectReward(2);var ids=layouts[0].cards.Select(c=>c.GetInstanceID()).ToArray();layouts[0].scroll.verticalNormalizedPosition=.35f;
            view.ResizePrimary(new Vector2(844,390));yield return null;
            Check(view.SelectedReward==2&&layouts[0].IsLandscape,"Rotation preserves host-owned selection");
            Check(ids.SequenceEqual(layouts[0].cards.Select(c=>c.GetInstanceID())),"Layout reuses existing controls rather than recreating them");
            view.ResizePrimary(new Vector2(390,520));yield return null;
            Check(Near(layouts[0].scroll.verticalNormalizedPosition,.35f),"Returning to a short screen restores its scroll position");
            Check(Inside(layouts[0].safeRoot,layouts[0].actions),"Fixed actions stay within safe bounds after rotation");
            for(int i=1;i<=3;i++){view.SetMode(i);yield return null;Capture(gallery,"04-responsive-"+i+".png");}
            config=JsonUtility.FromJson<GalleryBootstrap.GalleryConfiguration>(gallery.ExportConfigurationJson());
            Check(config.recipe.driver=="MotionResponsiveLayout"&&config.recipe.referenceSizes.Length==3&&config.recipe.safeInsets.Length==3,"Responsive export records current sizes and safe insets");
            gallery.SelectStyle(MotionStyleKind.Restrained);gallery.SetReducedMotion(true);yield return null;view=gallery.ResponsivePreview;
            Check(view.SelectedReward==2,"Profile and accessibility rebuild preserve selected reward");
            Check(Near(view.Layouts[0].scroll.verticalNormalizedPosition,.35f),"Profile and accessibility rebuild preserve scroll position");
            Check(view.GetComponentsInChildren<MotionButtonFeedback>(true).All(f=>f.reducedMotion&&Near(f.damping,.9f)),"All page controls share reduced motion and profile damping");
            Check(view.ConfirmCount==0,"Reflow and selection never confirm or grant a reward");
            view.SetMode(3);NativeClick(view.GetComponentsInChildren<Button>().First(b=>b.name=="Reward choice 1"));
            Check(view.SelectedReward==1,"Responsive reward selection accepts native pointer click");
            NativeClick(view.GetComponentsInChildren<Button>().First(b=>b.name=="Confirm choice"));
            Check(view.ConfirmCount==1,"Responsive fixed action dispatches one native confirmation");
            Check(GalleryPageArt.Panel.texture.width>1000&&GalleryPageArt.Medal.texture.width>1000&&Enumerable.Range(0,4).All(i=>GalleryPageArt.Icon(i).texture.width>1000),"New presentation assets are imported high-resolution raster sprites");
            gallery.SetReducedMotion(false);gallery.SelectStyle(MotionStyleKind.Standard);gallery.SelectEffect("B02");gallery.SetInteractionMode(false);yield return null;
            Check(gallery.CurrentPlayer!=null&&gallery.PagePreview==null&&gallery.ResponsivePreview==null,"Returning to an existing effect cleans up recipe controllers");Capture(gallery,"05-existing-button.png");Finish();
        }
        static bool Inside(RectTransform parent,RectTransform child)
        {
            var corners=new Vector3[4];child.GetWorldCorners(corners);var rect=parent.rect;rect.xMin-=.1f;rect.yMin-=.1f;rect.xMax+=.1f;rect.yMax+=.1f;return corners.All(c=>rect.Contains(parent.InverseTransformPoint(c)));
        }
        static bool Overlap(RectTransform a,RectTransform b)
        {
            var corners=new Vector3[4];b.GetWorldCorners(corners);var local=corners.Select(c=>(Vector2)a.InverseTransformPoint(c)).ToArray();var bounds=Rect.MinMaxRect(local.Min(p=>p.x),local.Min(p=>p.y),local.Max(p=>p.x),local.Max(p=>p.y));return a.rect.Overlaps(bounds);
        }
        void Capture(GalleryBootstrap gallery,string name){LibraryReview.CaptureCanvas(gallery.GetComponentInChildren<Canvas>(),Path.Combine(folder,name));}
        static void NativeClick(Button button)
        {
            var pointer=new PointerEventData(EventSystem.current){pointerId=-1,button=PointerEventData.InputButton.Left,eligibleForClick=true};
            ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerUpHandler);
            if(pointer.eligibleForClick)ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerClickHandler);
        }
        void Finish(){if(!running)return;running=false;Application.logMessageReceived-=Log;report.passed=report.errors.Count==0;File.WriteAllText(Path.Combine(folder,"presentation-report.json"),JsonUtility.ToJson(report,true));Debug.Log("PRESENTATION_TEST "+(report.passed?"PASS":"FAIL"));Application.Quit(report.passed?0:1);}
    }
}
