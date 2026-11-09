#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
namespace TweenGallery.Editor
{
    /// <summary>Deterministic sprite dwell checks, runnable through -executeMethod.</summary>
    public static class SpriteSequenceValidation
    {
        static readonly List<string> checks=new List<string>();
        static void Check(bool ok,string label){if(!ok)throw new Exception(label);checks.Add(label);}
        static void Reject(Action action,string label)
        {
            bool rejected=false;
            try{action();}catch(InvalidOperationException){rejected=true;}
            Check(rejected,label);
        }
        public static void Run()
        {
            checks.Clear();
            var go=new GameObject("Sprite dwell validation",typeof(RectTransform),typeof(Image));
            var other=new GameObject("Other sprite target",typeof(RectTransform),typeof(Image));
            var texture=new Texture2D(2,2);
            var a=Sprite.Create(texture,new Rect(0,0,1,1),Vector2.zero);
            var b=Sprite.Create(texture,new Rect(1,0,1,1),Vector2.zero);
            var baseline=Sprite.Create(texture,new Rect(0,1,1,1),Vector2.zero);
            try
            {
                var image=go.GetComponent<Image>();image.sprite=baseline;
                var sequence=go.AddComponent<MotionSpriteSequence>();
                sequence.image=image;sequence.frames=new[]{a,b,a};sequence.framesPerSecond=3;
                Check(sequence.TotalDuration==1,"Uncaptured FPS duration remains compatible");
                sequence.CaptureBaseline();sequence.Sample(0);
                Check(sequence.CurrentFrame==0&&image.sprite==a,"FPS first frame");
                sequence.Sample(.34f);Check(sequence.CurrentFrame==1&&image.sprite==b,"FPS equal dwell");
                sequence.Sample(1);Check(sequence.CurrentFrame==2&&image.sprite==a,"FPS final repeated sprite");
                sequence.Restore();Check(image.sprite==baseline,"FPS restores authored sprite");
                sequence.frameDurations=null;sequence.CaptureBaseline();
                Check(sequence.TotalDuration==1,"Null durations retain FPS mode");sequence.Restore();
                sequence.frameDurations=new[]{1f,2f,1f};sequence.CaptureBaseline();
                Check(sequence.TotalDuration==4,"Nonuniform duration sums every dwell");
                foreach(float p in new[]{0f,.24999f})
                {sequence.Sample(p);Check(sequence.CurrentFrame==0&&image.sprite==a,"First dwell at "+p);}
                foreach(float p in new[]{.25f,.5f,.74999f})
                {sequence.Sample(p);Check(sequence.CurrentFrame==1&&image.sprite==b,"Middle dwell at "+p);}
                foreach(float p in new[]{.75f,.9f,1f})
                {sequence.Sample(p);Check(sequence.CurrentFrame==2&&image.sprite==a,"Last dwell at "+p);}
                sequence.Sample(-1);Check(sequence.CurrentFrame==0,"Negative sample clamps first frame");
                sequence.Sample(2);Check(sequence.CurrentFrame==2,"Large sample clamps last frame");
                int completions=0;sequence.onVisualComplete.AddListener(()=>completions++);
                sequence.Play();sequence.Step(1);
                Check(sequence.CurrentFrame==1&&sequence.Progress==.25f,"Step reaches exact first boundary");
                sequence.Step(2);Check(sequence.CurrentFrame==2&&sequence.IsPlaying,"Last dwell is held before completion");
                sequence.Step(1);sequence.Step(1);
                Check(!sequence.IsPlaying&&completions==1&&image.sprite==a,"Final dwell completes once");
                sequence.Restore();sequence.Play();
                sequence.frames[1]=null;sequence.frameDurations[1]=float.NaN;
                sequence.framesPerSecond=100;sequence.image=other.GetComponent<Image>();
                sequence.Step(1);
                Check(sequence.TotalDuration==4&&image.sprite==b&&sequence.CurrentFrame==1,
                    "In-place edits and target replacement cannot poison captured playback");
                Check(other.GetComponent<Image>().sprite==null,"Playback does not write a replacement target");
                Reject(sequence.CaptureBaseline,"Invalid recapture is rejected");
                sequence.Sample(.5f);Check(image.sprite==b&&sequence.TotalDuration==4,"Rejected recapture preserves valid snapshot");
                sequence.Restore();Check(image.sprite==baseline,"Restore uses captured target and baseline");
                sequence.image=image;sequence.frames=new[]{b,a};sequence.frameDurations=new[]{3f,1f};
                sequence.CaptureBaseline();sequence.Sample(0);
                Check(image.sprite==b&&sequence.TotalDuration==4,"Explicit recapture adopts new frames and schedule");
                sequence.Sample(.75f);Check(image.sprite==a&&sequence.CurrentFrame==1,"Recaptured boundary selects next frame");
                sequence.Restore();
                sequence.frames=new[]{a,a,b};sequence.frameDurations=new[]{1f,2f,1f};sequence.CaptureBaseline();
                int dirties=0;UnityEngine.Events.UnityAction dirty=()=>dirties++;
                sequence.Sample(0);image.RegisterDirtyVerticesCallback(dirty);sequence.Sample(.25f);sequence.Sample(.5f);
                image.UnregisterDirtyVerticesCallback(dirty);
                Check(sequence.CurrentFrame==1&&image.sprite==a&&dirties==0,"Repeated sprite entries retain separate dwells without redundant writes");
                sequence.Restore();
                sequence.frameDurations=new[]{1f};Reject(sequence.CaptureBaseline,"Mismatched count rejected");
                foreach(float value in new[]{0f,-1f,float.NaN,float.PositiveInfinity,float.NegativeInfinity})
                {sequence.frameDurations=new[]{1f,value,1f};Reject(sequence.CaptureBaseline,"Invalid dwell rejected: "+value);}
                sequence.frameDurations=new[]{float.MaxValue,float.MaxValue,1f};Reject(sequence.CaptureBaseline,"Overflowing total rejected");
                sequence.frameDurations=Array.Empty<float>();sequence.frames=null;Reject(sequence.CaptureBaseline,"Null frames rejected");
                sequence.frames=Array.Empty<Sprite>();Reject(sequence.CaptureBaseline,"Empty frames rejected");
                sequence.frames=new[]{a,(Sprite)null};Reject(sequence.CaptureBaseline,"Null sprite entry rejected");
                sequence.frames=new[]{a};sequence.frameDurations=new[]{2f};sequence.CaptureBaseline();
                sequence.Play();sequence.Step(1);Check(sequence.CurrentFrame==0&&sequence.IsPlaying,"Single frame holds full dwell");
                sequence.Step(1);Check(!sequence.IsPlaying&&image.sprite==a,"Single frame completes normally");
                sequence.Restore();sequence.frameDurations=new[]{.01f};sequence.CaptureBaseline();
                sequence.Play();sequence.Step(.005f);
                Check(sequence.IsPlaying&&sequence.Progress==.5f&&sequence.TotalDuration==.01f,"Short positive dwell uses declared duration, not a minimum clamp");
                sequence.Step(.005f);Check(!sequence.IsPlaying&&sequence.Progress==1,"Short positive dwell completes at its declared duration");
                sequence.Restore();sequence.reducedMotion=true;sequence.Play();
                Check(!sequence.IsPlaying&&sequence.Progress==1&&image.sprite==a,"Reduced motion immediately samples endpoint");
                sequence.SendMessage("OnDisable");Check(image.sprite==baseline,"Explicit OnDisable callback restores baseline (EditMode callback test)");
                Directory.CreateDirectory("evidence");
                File.WriteAllText("evidence/sprite-sequence-validation.txt","PASS\n"+string.Join("\n",checks));
                Debug.Log("SPRITE_SEQUENCE_VALIDATION PASS "+checks.Count);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);UnityEngine.Object.DestroyImmediate(other);
                UnityEngine.Object.DestroyImmediate(a);UnityEngine.Object.DestroyImmediate(b);
                UnityEngine.Object.DestroyImmediate(baseline);UnityEngine.Object.DestroyImmediate(texture);
            }
        }
    }
}
#endif
