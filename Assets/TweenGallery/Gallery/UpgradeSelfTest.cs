using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace TweenGallery
{
    /// <summary>Opt-in real Player regression. Pointer events here are synthetic, not touch hardware.</summary>
    public sealed class UpgradeSelfTest : MonoBehaviour
    {
        [Serializable] class Result { public string unity, version; public bool passed; public List<string> checks = new List<string>(), errors = new List<string>(); public int sampleActors, sampleIterations; public double sampleCpuMs; public long sampleManagedBytes; }
        readonly Result result = new Result(); string folder; bool running; float deadline;
        void Check(bool passed, string label) { if (passed) result.checks.Add(label); else result.errors.Add(label); }
        void OnLog(string message,string stack,LogType type) { if(type==LogType.Error || type==LogType.Exception || type==LogType.Assert) result.errors.Add(message+"\n"+stack); }
        void Update() { if(running && Time.realtimeSinceStartup > deadline) { result.errors.Add("Upgrade test timed out"); Finish(); } }
        IEnumerator Start()
        {
            var args=Environment.GetCommandLineArgs(); if(!args.Contains("--upgrade-test")) yield break;
            running=true; deadline=Time.realtimeSinceStartup+120;
            int index=Array.IndexOf(args,"--test-output"); folder=index>=0&&index+1<args.Length?args[index+1]:Path.Combine(Application.persistentDataPath,"upgrade-test");
            Directory.CreateDirectory(folder);result.unity=Application.unityVersion;result.version=Application.version;Application.logMessageReceived+=OnLog;
            yield return null;yield return null;
            var gallery=FindFirstObjectByType<GalleryBootstrap>(); while(gallery!=null&&!gallery.IsReady) yield return null;
            Check(gallery!=null,"Gallery created"); if(gallery==null){Finish();yield break;}
            gallery.OpenInteractionLab();yield return null;Canvas.ForceUpdateCanvases();
            var lab=gallery.CurrentInteractionLab;
            Check(lab!=null && lab.HasGeneratedBackground && lab.ButtonInput!=null && lab.DragInput!=null && lab.Jelly!=null,"Interactive lab uses generated background and real input components");
            yield return new WaitForEndOfFrame(); ScreenCapture.CaptureScreenshot(Path.Combine(folder,"interaction-lab.png"));yield return null;
            if(lab!=null)
            {
                var player=lab.ContinuationPlayer; var rt=player.bindings.target;
                player.StopAndRestore(); var stable=MotionPose.Read(rt,player.bindings.group);
                var first=stable; first.position+=new Vector2(140,35); first.scale*=1.2f; first.rotation*=Quaternion.Euler(0,0,24);
                player.ContinueTo(first,.6f,Ease.Linear); yield return new WaitForSecondsRealtime(.18f);
                var before=MotionPose.Read(rt,player.bindings.group); var second=stable; second.position+=new Vector2(-110,-20);second.scale*=.85f;second.rotation*=Quaternion.Euler(0,0,-20);
                ulong oldVersion=player.PlayVersion; player.ContinueTo(second,.5f,Ease.Linear);var after=MotionPose.Read(rt,player.bindings.group);
                Check(Vector2.Distance(before.position,after.position)<.01f && Vector3.Distance(before.scale,after.scale)<.001f && Quaternion.Angle(before.rotation,after.rotation)<.01f,"Mid-motion reversal keeps position, scale and rotation continuous");
                Check(!player.IsCurrentPlayback(oldVersion),"New playback invalidates prior player token");
                yield return new WaitForSecondsRealtime(.12f);before=MotionPose.Read(rt,player.bindings.group);player.SetPressed(true);after=MotionPose.Read(rt,player.bindings.group);
                Check(Vector2.Distance(before.position,after.position)<.01f && Vector3.Distance(before.scale,after.scale)<.001f && Quaternion.Angle(before.rotation,after.rotation)<.01f,"Press interruption retains current pose");
                player.StopAndRestore();after=MotionPose.Read(rt,player.bindings.group);
                Check(Vector2.Distance(stable.position,after.position)<.01f && Vector3.Distance(stable.scale,after.scale)<.001f && Quaternion.Angle(stable.rotation,after.rotation)<.01f,"Stop restores stable baseline after continuation");
                lab.Close();yield return null;Check(!gallery.IsInteractionLabOpen,"Close lab returns to original catalog");
            }
            var springA=new MotionSpring1D(0);springA.Retarget(100);var springB=springA;
            var noBaselineGo=new GameObject("No baseline test",typeof(RectTransform)); var noBaseline=noBaselineGo.AddComponent<MotionPlayer>();
            noBaseline.bindings=new MotionBindings{target=noBaselineGo.GetComponent<RectTransform>()};ulong initialVersion=noBaseline.PlayVersion;bool rejected=false;
            try{noBaseline.ContinueTo(MotionPose.Read(noBaseline.bindings.target),.3f);}catch(InvalidOperationException){rejected=true;}
            Check(rejected && noBaseline.PlayVersion==initialVersion,"Continuation refuses to capture an accidental intermediate baseline");Destroy(noBaselineGo);
            springA.Step(2f,4f,.55f);for(int i=0;i<120;i++)springB.Step(1f/60f,4f,.55f);
            Check(Mathf.Abs(springA.value-springB.value)<.01f && Mathf.Abs(springA.velocity-springB.velocity)<.01f,"Spring consumes full long frame consistently with 120 short frames");
            springB.Step(.06f,4f,.55f);float priorVelocity=springB.velocity;springB.Retarget(-40);
            Check(springB.velocity==priorVelocity,"Spring retarget preserves velocity");
            var jellyHost=new GameObject("Jelly physics test",typeof(RectTransform));
            var jelly=jellyHost.AddComponent<MotionJellyDrag>();jelly.autoUpdate=false;jelly.CaptureBaseline();jelly.BeginDrag();jelly.Drag(new Vector2(100000,-100000));jelly.Step(.2f);
            Check(Mathf.Abs(jelly.Offset.x)<=jelly.maxDisplacement+.001f && Mathf.Abs(jelly.Offset.y)<=jelly.maxDisplacement+.001f,"Jelly remains bounded on both axes under a very large drag");
            jelly.EndDrag();jelly.Step(.7f);Check(jelly.Offset.magnitude<1f,"Extreme drag releases near baseline within 0.7 seconds");
            jelly.Step(10f);Check(jelly.Offset.magnitude<.01f && jelly.Velocity.magnitude<.01f,"Jelly settles after a long frame");Destroy(jellyHost);
            yield return UpgradeInputTest.Run(Check);
            yield return UpgradeOrchestrationTest.Run(message=>result.errors.Add(message));
            // Desktop CPU/managed allocation evidence for sampled reward update only;
            // this is not a measurement of whole frames or mobile performance.
            var perfHost=new GameObject("Reward sample performance",typeof(RectTransform),typeof(Canvas));
            perfHost.GetComponent<Canvas>().enabled=false;
            var actors=new List<MotionPlayer>();
            for(int i=0;i<100;i++){var p=DemoFactory.Create("D03",perfHost.transform,new Vector2(600,420));p.Play("D03");p.Pause();actors.Add(p);}
            for(int j=0;j<5;j++)foreach(var p in actors)p.Sample(j/5f);
            long allocBefore=GC.GetAllocatedBytesForCurrentThread();var clock=System.Diagnostics.Stopwatch.StartNew();
            for(int j=0;j<60;j++)foreach(var p in actors)p.Sample(j/60f);
            clock.Stop();result.sampleManagedBytes=GC.GetAllocatedBytesForCurrentThread()-allocBefore;
            result.sampleCpuMs=clock.Elapsed.TotalMilliseconds;result.sampleActors=actors.Count;result.sampleIterations=60;
            Check(result.sampleCpuMs>0,"Collected desktop CPU and managed allocation data for 100 concurrent reward samples");Destroy(perfHost);yield return null;
            result.passed=result.errors.Count==0;Finish();
        }
        void Finish(){ if(!running)return;running=false;Application.logMessageReceived-=OnLog;File.WriteAllText(Path.Combine(folder,"upgrade-report.json"),JsonUtility.ToJson(result,true));Debug.Log("UPGRADE_SELF_TEST "+(result.passed?"PASS":"FAIL"));Application.Quit(result.passed?0:1); }
    }
}
