using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace TweenGallery.Editor
{
    /// <summary>Deterministic contract regressions; no scene, DOTween clock or play-mode loop required.</summary>
    public static class SourcePatternValidation
    {
        static readonly List<string> checks=new List<string>();
        static void Check(bool ok,string message){if(!ok)throw new InvalidOperationException(message);checks.Add(message);}
        static bool Finite(float n)=>!float.IsNaN(n)&&!float.IsInfinity(n);
        static bool Near(float a,float b)=>Mathf.Abs(a-b)<.0001f;
        static RectTransform Visual(Transform parent,string name)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(CanvasGroup));go.transform.SetParent(parent,false);
            var rect=(RectTransform)go.transform;rect.anchoredPosition=new Vector2(13,27);rect.localScale=new Vector3(.8f,1.2f,1);rect.localRotation=Quaternion.Euler(0,0,17);go.GetComponent<CanvasGroup>().alpha=.42f;return rect;
        }
        static void PoseFinite(RectTransform rect,string label)
        {
            var p=rect.anchoredPosition;var s=rect.localScale;var q=rect.localRotation;
            Check(Finite(p.x)&&Finite(p.y)&&Finite(s.x)&&Finite(s.y)&&Finite(q.z)&&Finite(q.w),label+" finite pose");
        }
        public static void Run()
        {
            checks.Clear();foreach(int count in new[]{0,1,4,6,12})Page(count);
            foreach(string kind in new[]{"text","logo","confetti","marquee","sprite"})Sequence(kind);
            Jelly();Directory.CreateDirectory("evidence");File.WriteAllText("evidence/source-pattern-validation.txt","PASS\n"+string.Join("\n",checks));
            Debug.Log("SOURCE_PATTERN_VALIDATION PASS "+checks.Count);
        }
        static void Page(int count)
        {
            var root=new GameObject("Page contract fixture");
            try
            {
                var page=root.AddComponent<MotionPageSequence>();page.panel=Visual(root.transform,"panel");page.emblem=Visual(root.transform,"emblem");page.actionsGroup=Visual(root.transform,"actions").GetComponent<CanvasGroup>();
                page.rewardA=Visual(root.transform,"reward A");page.rewardB=Visual(root.transform,"reward B");page.walletTarget=Visual(root.transform,"wallet");page.walletTarget.anchoredPosition=new Vector2(120,60);
                page.coins=new RectTransform[count];for(int i=0;i<count;i++)page.coins[i]=Visual(root.transform,"coin");
                var position=page.panel.anchoredPosition;var scale=page.panel.localScale;page.CaptureBaseline();int complete=0;page.onVisualComplete.AddListener(()=>complete++);
                page.Seek(1);Check(complete==0,"Page seek is silent: "+count);foreach(var coin in page.coins)Check(coin.localScale==Vector3.zero,"Page terminal coin hidden: "+count);
                page.Play();uint version=page.PlayVersion;page.Step(5);page.Step(5);Check(complete==1,"Page completes once: "+count);
                page.Seek(.5f);page.Resume();page.Step(5);Check(complete==1&&page.PlayVersion==version,"Page resumed replay retains completion latch: "+count);
                page.reducedMotion=true;page.Play();Check(complete==2&&!page.IsPlaying,"Page reduced motion new round: "+count);foreach(var coin in page.coins)Check(coin.localScale==Vector3.zero,"Page reduced terminal coin hidden: "+count);page.reducedMotion=false;
                foreach(float invalid in new[]{float.NaN,float.PositiveInfinity,float.NegativeInfinity})
                {
                    page.speed=invalid;page.duration=invalid;page.stagger=invalid;page.Play();page.Step(.1f);Check(Finite(page.Progress),"Page invalid clock finite: "+count);PoseFinite(page.panel,"Page");PoseFinite(page.emblem,"Page emblem");PoseFinite(page.rewardB,"Page staggered reward");
                }
                page.speed=1;page.duration=1.1f;page.stagger=.08f;page.Play();page.Step(.2f);page.Pause();float progress=page.Progress;page.Step(1);Check(Near(progress,page.Progress),"Page pause stable: "+count);page.Resume();page.Step(.1f);Check(page.Progress>progress,"Page resumes: "+count);
                // Direct callback invocation is intentional: this validator runs outside play mode.
                page.SendMessage("OnDisable");Check(!page.IsPlaying&&page.panel.anchoredPosition==position&&page.panel.localScale==scale,"Page disable callback restores authored baseline: "+count);
                foreach(var coin in page.coins)Check(coin.anchoredPosition==position&&coin.localScale==scale,"Page coin restore: "+count);
            }
            finally{UnityEngine.Object.DestroyImmediate(root);}
        }
        static void Sequence(string kind)
        {
            var root=new GameObject("Sequence contract fixture");Texture2D texture=null;Sprite original=null,frame=null;
            try
            {
                var visual=Visual(root.transform,"visual");var group=visual.GetComponent<CanvasGroup>();MotionVisualSequence clip;Action<float> setDuration;
                if(kind=="text"){var c=root.AddComponent<MotionTextSequence>();c.visuals=new[]{visual};c.groups=new[]{group};clip=c;setDuration=v=>c.duration=v;}
                else if(kind=="logo"){var c=root.AddComponent<MotionLogoDrop>();c.glyphs=new[]{visual};c.groups=new[]{group};clip=c;setDuration=v=>{c.dropDuration=v;c.holdDuration=v;c.timingScale=v;c.stagger=v;c.firstDelay=v;};}
                else if(kind=="confetti"){var c=root.AddComponent<MotionConfettiSequence>();c.items=new[]{visual};c.groups=new[]{group};clip=c;setDuration=v=>c.duration=v;}
                else if(kind=="marquee"){var viewport=root.AddComponent<RectTransform>();visual.anchorMin=visual.anchorMax=visual.pivot=Vector2.one*.5f;var c=root.AddComponent<MotionMarquee>();c.visual=visual;c.viewport=viewport;clip=c;setDuration=v=>c.duration=v;}
                else{texture=new Texture2D(2,2);original=Sprite.Create(texture,new Rect(0,0,2,2),Vector2.one*.5f);frame=Sprite.Create(texture,new Rect(0,0,1,1),Vector2.one*.5f);var image=visual.gameObject.AddComponent<Image>();image.sprite=original;var c=root.AddComponent<MotionSpriteSequence>();c.image=image;c.frames=new[]{frame,original,frame};clip=c;setDuration=v=>c.framesPerSecond=v;}
                var position=visual.anchoredPosition;var scale=visual.localScale;var rotation=visual.localRotation;clip.CaptureBaseline();int complete=0;clip.onVisualComplete.AddListener(()=>complete++);
                clip.Seek(1);clip.Resume();clip.Step(10);Check(complete==0&&!clip.IsPlaying,kind+" final seek remains silent");
                clip.Play();uint version=clip.PlayVersion;clip.Step(100);clip.Step(100);Check(complete==1,kind+" completes once");clip.Seek(.5f);clip.Resume();clip.Step(100);Check(complete==1&&clip.PlayVersion==version,kind+" replay retains completion latch");
                clip.Play();clip.Step(100);Check(complete==2,kind+" new Play re-arms completion");
                foreach(float invalid in new[]{float.NaN,float.PositiveInfinity,float.NegativeInfinity})
                {
                    clip.Restore();setDuration(invalid);clip.CaptureBaseline();clip.speed=invalid;clip.Play();clip.Step(.1f);clip.Seek(.4f);Check(Finite(clip.Progress)&&Finite(clip.TotalDuration),kind+" invalid clock remains finite");PoseFinite(visual,kind);Check(Finite(group.alpha),kind+" finite opacity");
                }
                setDuration(1);clip.speed=1;clip.Play();clip.Step(.02f);clip.Pause();float progress=clip.Progress;clip.Step(1);Check(Near(progress,clip.Progress),kind+" pause stable");clip.Resume();clip.Step(.01f);Check(clip.Progress>progress,kind+" resumes");
                clip.SendMessage("OnApplicationFocus",false);progress=clip.Progress;clip.Step(1);Check(Near(progress,clip.Progress),kind+" focus pauses clock");clip.SendMessage("OnApplicationFocus",true);
                clip.SendMessage("OnApplicationPause",true);clip.Step(1);Check(Near(progress,clip.Progress),kind+" background pauses clock");clip.SendMessage("OnApplicationPause",false);clip.Step(.01f);Check(clip.Progress>progress,kind+" foreground resumes clock");
                clip.SendMessage("OnDisable");Check(!clip.IsPlaying&&visual.anchoredPosition==position&&visual.localScale==scale&&Quaternion.Angle(visual.localRotation,rotation)<.01f&&Near(group.alpha,.42f),kind+" disable callback restores authored baseline");
                if(kind=="sprite")Check(visual.GetComponent<Image>().sprite==original,"Sprite restores original art");
            }
            finally{UnityEngine.Object.DestroyImmediate(root);if(original!=null)UnityEngine.Object.DestroyImmediate(original);if(frame!=null)UnityEngine.Object.DestroyImmediate(frame);if(texture!=null)UnityEngine.Object.DestroyImmediate(texture);}
        }
        static void Jelly()
        {
            var go=new GameObject("Jelly fixture",typeof(RectTransform),typeof(Image),typeof(MotionJellyGraphic));
            try
            {
                var jelly=go.GetComponent<MotionJellyGraphic>();((RectTransform)go.transform).sizeDelta=new Vector2(100,60);jelly.SetDeformation(.5f);
                foreach(float invalid in new[]{float.NaN,float.PositiveInfinity,float.NegativeInfinity})
                using(var mesh=new VertexHelper())
                {
                    mesh.AddVert(new Vector3(20,20),Color.white,Vector2.zero);jelly.SetDeformation(invalid);jelly.ModifyMesh(mesh);UIVertex vertex=default;mesh.PopulateUIVertex(ref vertex,0);
                    Check(Finite(vertex.position.x)&&Finite(vertex.position.y),"Jelly rejects nonfinite deformation");Check(!Near(vertex.position.x,20),"Jelly retains last valid deformation");
                }
            }
            finally{UnityEngine.Object.DestroyImmediate(go);}
        }
    }
}
