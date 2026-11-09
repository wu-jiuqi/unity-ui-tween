using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace TweenGallery
{
    public sealed class TypographySelfTest : MonoBehaviour
    {
        [Serializable] class Report {public bool passed;public string version;public int width,height;public List<string> checks=new List<string>(),errors=new List<string>();}
        readonly Report report=new Report();bool running;string folder;float deadline;
        void Check(bool ok,string label){if(ok)report.checks.Add(label);else report.errors.Add(label);}
        static bool Near(float a,float b)=>Mathf.Abs(a-b)<.002f;
        void Log(string message,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)report.errors.Add(message);}
        void Update(){if(running&&Time.realtimeSinceStartup>deadline){report.errors.Add("Typography validation timeout");Finish();}}
        IEnumerator Start()
        {
            var args=Environment.GetCommandLineArgs();if(Array.IndexOf(args,"--typography-test")<0)yield break;int at=Array.IndexOf(args,"--test-output");folder=at<0?Path.Combine(Application.persistentDataPath,"typography-test"):args[at+1];Directory.CreateDirectory(folder);
            running=true;deadline=Time.realtimeSinceStartup+150;Application.logMessageReceived+=Log;report.version=Application.version;report.width=Screen.width;report.height=Screen.height;yield return null;yield return null;
            var gallery=GetComponent<GalleryBootstrap>();gallery.SetCategory("F");
            foreach(string id in new[]{"T01","T02","T03","T04","T05","T06","T07"})
            {
                gallery.SelectEffect(id);gallery.SetInteractionMode(false);yield return null;var view=gallery.TypographyPreview;var clip=view.Sequence;int completions=0;clip.onVisualComplete.AddListener(()=>completions++);
                Check(gallery.CurrentPlayer==null&&clip!=null,id+" is a distinct visual recipe, not a MotionPlayer ID");
                Check(view.GetComponentsInChildren<RectMask2D>().Any(m=>m.name=="Motion viewport"),id+" moving art is clipped away from application headings and controls");
                clip.Seek(0);clip.Seek(.5f);clip.Seek(1);Check(completions==0&&!clip.IsPlaying,id+" timeline sampling is silent");
                clip.Play();clip.Step(clip.TotalDuration*2);clip.Step(1);Check(completions==1&&Near(clip.Progress,1),id+" playback completes once");
                clip.Seek(.5f);clip.Resume();clip.Step(clip.TotalDuration*2);Check(completions==1,id+" resumed replay retains completion latch");
                clip.Seek(.3f);gallery.TogglePause();clip.Step(clip.TotalDuration*.1f);gallery.TogglePause();Check(Near(clip.Progress,.4f)&&!clip.IsPlaying,id+" pause control resumes the sampled clock");
                clip.reducedMotion=true;clip.Play();Check(completions==2&&Near(clip.Progress,1)&&!clip.IsPlaying,id+" reduced motion reaches necessary final state");clip.reducedMotion=false;
                clip.Restore();var anchors=view.GetComponentsInChildren<RectTransform>().Where(r=>r.name.StartsWith("Fixed slot")||r.name=="Fixed glyph layout").ToArray();var positions=anchors.Select(r=>r.anchoredPosition).ToArray();
                clip.Seek(.5f);Check(anchors.Select((r,i)=>Vector2.Distance(r.anchoredPosition,positions[i])<.001f).All(v=>v),id+" layout slots remain stable");
                var config=JsonUtility.FromJson<GalleryBootstrap.GalleryConfiguration>(gallery.ExportConfigurationJson());Check(config.previewMode=="typography"&&config.recipe.driver==clip.GetType().Name&&config.effects.Length==0,id+" export carries the actual driver and no invalid catalog IDs");
                Check(Near(config.recipe.totalDuration,clip.TotalDuration),id+" export includes actual full duration");
                if(clip is MotionTextSequence text)
                {
                    text.Seek(1);Check(text.visuals.All(v=>v.localScale==Vector3.one),id+" exact scale at rest");
                    if(text.kind==TextMotionKind.Reveal)
                    {
                        text.Seek(0);Check(text.groups.All(g=>Near(g.alpha,0)),"Reveal begins with all glyphs hidden");text.Seek(.5f);Check(text.groups.Count(g=>g.alpha>.5f)==Mathf.FloorToInt(text.groups.Length*.5f),"Reveal shows the expected glyph count without changing layout");
                        text.Restore();text.groups[0].alpha=.45f;text.CaptureBaseline();text.Seek(1);Check(Near(text.groups[0].alpha,.45f),"Reveal respects captured semi-transparent glyph opacity");
                    }
                    if(id=="T02"){text.Seek(.16f);Check(text.visuals[0].anchoredPosition.y!=text.visuals[text.visuals.Length-1].anchoredPosition.y,"Staggered glyphs use different local clocks");}
                }
                if(clip is MotionLogoDrop logo)
                {
                    float time=.12f+.5f+.04f;logo.Seek(time/6.8f);Check(Near(logo.glyphs[0].localScale.x,1.39f)&&Near(logo.glyphs[0].localScale.y,.61f),"Logo retains original 30fps squash impact key");
                    logo.Seek((logo.SharedJellyStart-.002f)/6.8f);Check(!logo.InSharedJellyPhase,"Shared jelly waits for the final glyph impact to finish");logo.Seek((logo.SharedJellyStart+.2f)/6.8f);Check(logo.InSharedJellyPhase,"All settled glyphs enter the shared sustained phase");
                    Check(logo.meshes.Length==logo.glyphs.Length&&logo.meshes.All(m=>m!=null),"Logo has independent mesh deformation bindings");logo.Seek(1);Check(logo.groups.All(g=>Near(g.alpha,1))&&logo.glyphs.All(g=>g.localScale==Vector3.one),"Logo final state restores stable full visibility");
                }
                if(clip is MotionMarquee marquee){marquee.Seek(0);float x=marquee.visual.anchoredPosition.x;marquee.Seek(1);Check(x>0&&marquee.visual.anchoredPosition.x<0,"Marquee fully crosses both viewport edges");marquee.reducedMotion=true;marquee.Play();Check(Near(marquee.visual.anchoredPosition.x,0),"Reduced marquee retains readable static content");marquee.reducedMotion=false;}
                clip.Seek(id=="T03"?.21f:.48f);yield return null;Capture(gallery,id+"-emphasis.png");clip.Seek(1);yield return null;Capture(gallery,id+"-rest.png");
                if(Array.IndexOf(args,"--capture-motion")>=0&&(id=="T03"||id=="T02"||id=="T05"))
                {
                    string framesFolder=Path.Combine(folder,id+"-frames");Directory.CreateDirectory(framesFolder);int count=Mathf.CeilToInt(clip.TotalDuration*24);
                    for(int frame=0;frame<=count;frame++){clip.Seek(frame/(float)count);yield return null;LibraryReview.CaptureCanvas(gallery.GetComponentInChildren<Canvas>(),Path.Combine(framesFolder,frame.ToString("D4")+".png"));}
                }
                clip.Play();clip.Step(clip.TotalDuration*.3f);view.gameObject.SetActive(false);
                Check(!clip.IsPlaying&&Near(clip.Progress,0),id+" actual mid-play disable restores the owned clock");
            }
            var root=new GameObject("Frame sequence validation",typeof(RectTransform),typeof(Image));var image=root.GetComponent<Image>();image.sprite=GalleryPageArt.Icon(0);var frames=root.AddComponent<MotionSpriteSequence>();frames.image=image;frames.frames=new[]{GalleryPageArt.Icon(1),GalleryPageArt.Icon(2),GalleryPageArt.Icon(3)};frames.framesPerSecond=10;frames.CaptureBaseline();frames.Seek(.5f);
            Check(frames.CurrentFrame==1&&image.sprite==frames.frames[1]&&Near(frames.TotalDuration,.3f),"Sprite frame clock samples the expected discrete frame");frames.Seek(1);Check(frames.CurrentFrame==2,"Sprite final sample stays on last frame");frames.Restore();Check(image.sprite==GalleryPageArt.Icon(0),"Sprite sequence restores original art");
            frames.frameDurations=new[]{1.8f,.55f,4.35f};frames.CaptureBaseline();frames.Seek(1.8f/6.7f);
            Check(frames.CurrentFrame==1&&Near(frames.TotalDuration,6.7f),"Unequal sprite dwells preserve source cadence");
            frames.Play();frames.Step(2);root.SetActive(false);
            Check(!frames.IsPlaying&&image.sprite==GalleryPageArt.Icon(0),"Actual mid-play sprite disable restores original art");Destroy(root);
            var confettiRoot=new GameObject("Confetti validation",typeof(RectTransform));var confetti=confettiRoot.AddComponent<MotionConfettiSequence>();var part=new GameObject("Preallocated paper",typeof(RectTransform),typeof(CanvasGroup));part.transform.SetParent(confettiRoot.transform,false);confetti.items=new[]{part.GetComponent<RectTransform>()};confetti.groups=new[]{part.GetComponent<CanvasGroup>()};confetti.items[0].anchoredPosition=new Vector2(4,30);confetti.CaptureBaseline();
            foreach(var kind in new[]{ConfettiMotionKind.Falling,ConfettiMotionKind.Burst}){confetti.kind=kind;confetti.Seek(.5f);var pose=confetti.items[0].anchoredPosition;confetti.Seek(.2f);confetti.Seek(.5f);Check(Vector2.Distance(pose,confetti.items[0].anchoredPosition)<.001f,"Confetti deterministic sampling: "+kind);confetti.Seek(1);Check(Near(confetti.groups[0].alpha,0),"Confetti final frame clears decorative art: "+kind);}
            confetti.Restore();Check(Vector2.Distance(confetti.items[0].anchoredPosition,new Vector2(4,30))<.001f&&Near(confetti.groups[0].alpha,1),"Confetti restores original position and visibility");
            confetti.Play();confetti.Step(.6f);confettiRoot.SetActive(false);
            Check(!confetti.IsPlaying&&Vector2.Distance(confetti.items[0].anchoredPosition,new Vector2(4,30))<.001f&&Near(confetti.groups[0].alpha,1),"Actual mid-play confetti disable restores original pose");Destroy(confettiRoot);
            gallery.SelectEffect("P01");gallery.SeekPreview(1);yield return null;var page=gallery.PagePreview;
            Check(page.GetComponentsInChildren<Image>().Any(i=>i.sprite==GalleryCompositionArt.AchievementTitle),"Page uses separately generated crafted title art");
            Check(page.GetComponentsInChildren<Text>().Any(t=>t.name=="Reward amount"&&t.text=="+120"),"Dynamic reward amount stays native text");
            Check(page.GetComponentsInChildren<Text>().All(t=>t.fontStyle==FontStyle.Normal),"Real font weights avoid synthetic bold distortion");Capture(gallery,"composition-page.png");
            gallery.SelectEffect("P02");yield return null;Capture(gallery,"composition-responsive.png");Finish();
        }
        void Capture(GalleryBootstrap gallery,string file)=>LibraryReview.CaptureCanvas(gallery.GetComponentInChildren<Canvas>(),Path.Combine(folder,file));
        void Finish(){if(!running)return;running=false;Application.logMessageReceived-=Log;report.passed=report.errors.Count==0;File.WriteAllText(Path.Combine(folder,"typography-report.json"),JsonUtility.ToJson(report,true));Application.Quit(report.passed?0:1);}
    }
}
