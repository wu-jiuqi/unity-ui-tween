#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace TweenGallery.Editor
{
    /// <summary>Deterministic regression checks against the production evaluator, without a gallery build.</summary>
    public static class RuntimeValidation
    {
        static readonly List<string> Checks = new List<string>();
        static void Check(bool ok, string label) { if(!ok) throw new Exception(label); Checks.Add(label); }
        static bool Near(float a, float b) { return Mathf.Abs(a-b)<.0001f; }
        static void Reject(Action action, string label)
        {
            bool rejected=false;
            try { action(); } catch(ArgumentException) { rejected=true; } catch(InvalidOperationException) { rejected=true; }
            Check(rejected,label);
        }
        public static void Run()
        {
            Checks.Clear();
            var root=new GameObject("Runtime regression",typeof(RectTransform),typeof(Canvas));
            try
            {
                var player=DemoFactory.Create("D02",root.transform,new Vector2(600,420));
                player.StopAndRestore();
                var fill=player.bindings.fill;
                var labelObject=new GameObject("Regression percentage",typeof(RectTransform),typeof(Text));
                labelObject.transform.SetParent(player.bindings.target,false);
                var label=labelObject.GetComponent<Text>(); player.bindings.label=label;
                fill.fillAmount=.33f; label.text="Original label"; player.CaptureBaseline();
                player.settings.ease="Linear"; player.settings.fillFrom=.08f; player.settings.fillTo=.9f;
                player.Sample(0); Check(Near(fill.fillAmount,.08f)&&label.text=="8%","D02 starts at 8%");
                player.Sample(.5f); Check(Near(fill.fillAmount,.49f)&&label.text=="49%","D02 midpoint is 49%, not rounded before multiplying");
                player.Sample(1); Check(Near(fill.fillAmount,.9f)&&label.text=="90%","D02 ends at 90%");
                player.settings.reverse=true;
                player.Sample(0); Check(Near(fill.fillAmount,.9f)&&label.text=="90%","D02 reverse starts at endpoint");
                player.Sample(.5f); Check(label.text=="49%","D02 reverse midpoint");
                player.Sample(1); Check(Near(fill.fillAmount,.08f)&&label.text=="8%","D02 reverse ends at start");
                player.StopAndRestore(); Check(Near(fill.fillAmount,.33f)&&label.text=="Original label","D02 stop restores captured fill and text");
                player.settings.reverse=false;
                player.Sample(.5f); Check(label.text=="49%","D02 replay after reset refreshes text despite cached number");
                int dirties=0; UnityEngine.Events.UnityAction dirty=()=>dirties++;
                label.RegisterDirtyVerticesCallback(dirty); player.Sample(.5f); player.Sample(.50001f);
                Check(dirties==0,"D02 unchanged rounded values do not dirty text vertices");
                label.UnregisterDirtyVerticesCallback(dirty);
                label.text="External edit"; player.Sample(.5f); Check(label.text=="49%","D02 repairs external text changes");
                player.settings.fillFrom=-.5f; player.settings.fillTo=1.5f;
                player.Sample(0); Check(Near(fill.fillAmount,0)&&label.text=="0%","D02 clamps lower label and fill together");
                player.Sample(1); Check(Near(fill.fillAmount,1)&&label.text=="100%","D02 clamps upper label and fill together");
                player.settings.fillFrom=.08f; player.settings.fillTo=.9f;
                player.bindings.label=null; player.Sample(.5f); Check(Near(fill.fillAmount,.49f),"D02 label is optional");
                player.bindings.label=label; player.settings=null; player.Sample(.5f);
                Check(player.settings!=null,"Sample initializes missing settings");
                player.settings.ease="Linear"; player.settings.startValue=0; player.settings.endValue=100;
                player.effectId="D01"; player.Sample(.5f); Check(label.text==50.ToString("N0"),"D01 midpoint counter");
                dirties=0; label.RegisterDirtyVerticesCallback(dirty); player.Sample(.5f);
                Check(dirties==0,"D01 identical samples do not dirty text vertices"); label.UnregisterDirtyVerticesCallback(dirty);
                player.settings.ease="InQuad"; player.Sample(.5f); Check(label.text==25.ToString("N0"),"Changing ease at runtime invalidates easing cache");
                player.settings.ease="NotAnEase"; player.Sample(.5f); Check(label.text==75.ToString("N0"),"Invalid ease falls back to effect preset");
                player.settings.ease="Preset"; player.Sample(.5f); Check(label.text==75.ToString("N0"),"Preset easing fallback");
                player.Play("D01"); player.Pause(); var version=player.PlayVersion; string id=player.effectId; float progress=player.Progress;
                player.bindings.fill=null;
                Reject(()=>player.Play("D02"),"Play rejects missing required binding");
                Check(version==player.PlayVersion&&player.effectId==id&&Near(player.Progress,progress),"Invalid Play preserves playback token, effect and progress");
                Reject(()=>player.Play("Unknown"),"Play rejects unknown effect ID");
                player.effectId="D02"; Reject(()=>player.Sample(.5f),"Sample rejects missing required binding");
                player.effectId="Unknown"; Reject(()=>player.Sample(.5f),"Sample rejects unknown effect ID");
                player.effectId=id; player.bindings.fill=fill; player.StopAndRestore();

                player.effectId="D02"; player.settings.ease="Linear"; player.settings.duration=1f; player.settings.speed=1f;
                player.settings.fillFrom=0f; player.settings.fillTo=1f;
                // DOTween initializes getters lazily: sampling in the creation frame must
                // not change the catalog tween's true 0-to-1 interpolation interval.
                player.Play("D02"); player.Sample(.5f); player.Seek(.2f);
                var creationFrameTween=(Tween)typeof(MotionPlayer).GetField("ownedTween",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(player);
                creationFrameTween.SetUpdate(UpdateType.Manual);
                player.Resume(); DOTween.ManualUpdate(.1f,.1f); player.Pause();
                Check(Near(player.Progress,.3f)&&label.text=="30%","Creation-frame Play, Sample, Seek, Resume advances from 20% to 30%; actual progress="+player.Progress+", label="+label.text);
                player.StopAndRestore();
                player.Seek(.4f); Check(!player.IsPlaying&&Near(player.Progress,.4f)&&label.text=="40%","Seek creates a paused catalog timeline at requested progress");
                var tween=(Tween)typeof(MotionPlayer).GetField("ownedTween",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(player);
                tween.SetUpdate(UpdateType.Manual);
                player.Resume(); DOTween.ManualUpdate(.1f,.1f); player.Pause();
                Check(Near(player.Progress,.5f)&&label.text=="50%","Resume advances the real tween clock from scrubbed position");
                player.Seek(2f); Check(Near(player.Progress,1f)&&label.text=="100%"&&!player.IsPlaying,"Seek clamps upper endpoint and pauses");
                player.Seek(.2f); player.Resume(); DOTween.ManualUpdate(.1f,.1f); player.Pause();
                Check(Near(player.Progress,.3f)&&label.text=="30%","Seek back from completion resumes without restarting from zero");
                player.Seek(-1f); Check(Near(player.Progress,0f)&&label.text=="0%","Seek clamps lower endpoint");
                Reject(()=>player.Seek(float.NaN),"Seek rejects NaN"); Reject(()=>player.Seek(float.PositiveInfinity),"Seek rejects infinity");
                player.ContinueTo(MotionPose.Read(player.bindings.target),1f);
                Reject(()=>player.Seek(.5f),"Seek rejects pose continuations"); player.StopAndRestore();

                var settlement=DemoFactory.Create("D05",root.transform,new Vector2(600,420));
                settlement.settings.ease="Linear"; settlement.Sample(1);
                Check(settlement.bindings.label.text==Mathf.RoundToInt(settlement.settings.endValue).ToString("N0"),"D05 settlement reaches total");
                int settlementDirties=0; UnityEngine.Events.UnityAction settlementDirty=()=>settlementDirties++;
                settlement.bindings.label.RegisterDirtyVerticesCallback(settlementDirty);
                foreach(var text in settlement.bindings.labels) if(text!=null) text.RegisterDirtyVerticesCallback(settlementDirty);
                settlement.Sample(1); Check(settlementDirties==0,"D05 unchanged rows and total do not dirty text vertices");
                settlement.StopAndRestore(); settlement.Sample(1);
                Check(settlement.bindings.label.text==Mathf.RoundToInt(settlement.settings.endValue).ToString("N0"),"D05 reset and replay refreshes cached total");

                // Keep the hot-path fixture independent of generated art and gallery construction.
                var morphObject=new GameObject("C04 allocation target",typeof(RectTransform));
                morphObject.transform.SetParent(root.transform,false);
                var morphTarget=(RectTransform)morphObject.transform;
                morphTarget.sizeDelta=new Vector2(120,80);
                var destinationObject=new GameObject("C04 allocation destination",typeof(RectTransform));
                destinationObject.transform.SetParent(root.transform,false);
                var morphDestination=(RectTransform)destinationObject.transform;
                morphDestination.sizeDelta=new Vector2(320,240);
                morphDestination.anchoredPosition=new Vector2(150,40);
                var morph=morphObject.AddComponent<MotionPlayer>();
                morph.effectId="C04";
                morph.bindings=new MotionBindings { target=morphTarget, destination=morphDestination };
                morph.settings.ease="InOutSine"; morph.CaptureBaseline();
                for(int i=0;i<100;i++) morph.Sample(i/100f);
                long before=GC.GetAllocatedBytesForCurrentThread();
                for(int i=0;i<1000;i++) morph.Sample((i%100)/100f);
                long morphBytes=GC.GetAllocatedBytesForCurrentThread()-before;
                Check(morphBytes==0,"C04 1000 warmed samples allocate zero managed bytes: "+morphBytes);
                player.effectId="D02"; player.Sample(.5f);
                before=GC.GetAllocatedBytesForCurrentThread();
                for(int i=0;i<1000;i++) player.Sample(.5f);
                long textBytes=GC.GetAllocatedBytesForCurrentThread()-before;
                Check(textBytes==0,"D02 1000 unchanged samples allocate zero managed bytes: "+textBytes);
                Check(MotionCatalog.All.Length==46,"Catalog still exposes all 46 effects");
                Directory.CreateDirectory("evidence");
                File.WriteAllText("evidence/runtime-validation.txt","PASS Unity "+Application.unityVersion+"\n"+string.Join("\n",Checks)+"\n");
                Debug.Log("RUNTIME_VALIDATION_PASS: "+Checks.Count+" checks");
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }
    }
}
#endif
