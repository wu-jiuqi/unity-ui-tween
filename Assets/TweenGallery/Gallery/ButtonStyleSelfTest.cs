using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TweenGallery
{
    public sealed class ButtonStyleSelfTest : MonoBehaviour
    {
        [Serializable] class Report { public bool passed; public string version,unity; public int width,height; public List<string> checks=new List<string>(),errors=new List<string>(); }
        Report report=new Report();string folder;bool running;float deadline;
        void Check(bool ok,string label) { if(ok)report.checks.Add(label);else report.errors.Add(label); }
        void Log(string message,string stack,LogType type) { if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)report.errors.Add(message); }
        void Update() { if(running&&Time.realtimeSinceStartup>deadline){report.errors.Add("Button lab timeout");Finish();} }
        IEnumerator Start()
        {
            var args=Environment.GetCommandLineArgs();if(Array.IndexOf(args,"--button-test")<0)yield break;
            running=true;deadline=Time.realtimeSinceStartup+90;Application.logMessageReceived+=Log;
            int at=Array.IndexOf(args,"--test-output");folder=at>=0?args[at+1]:Path.Combine(Application.persistentDataPath,"button-test");Directory.CreateDirectory(folder);
            report.version=Application.version;report.unity=Application.unityVersion;report.width=Screen.width;report.height=Screen.height;
            yield return null;yield return null;
            var gallery=GetComponent<GalleryBootstrap>();gallery.OpenButtonLab();yield return null;
            var lab=gallery.CurrentButtonLab;Check(lab!=null&&lab.Buttons.Length==8,"Eight button recipes are available");Check(lab.HasGeneratedSkins,"Original six-skin atlas loaded in Player");
            foreach(var b in lab.Buttons)
            {
                var hit=(RectTransform)b.transform;Vector2 size=hit.sizeDelta;
                var e=new PointerEventData(EventSystem.current){pointerId=-1,eligibleForClick=true,position=Vector2.zero};
                b.OnPointerDown(e);yield return new WaitForSecondsRealtime(.12f);
                Check(b.IsPressed&&b.PressAmount>.4f,b.style+" holds pressed state");Check(hit.localScale==Vector3.one&&hit.sizeDelta==size,b.style+" leaves hit geometry fixed");
                b.OnPointerUp(e);b.OnSubmit(new BaseEventData(EventSystem.current));
            }
            yield return new WaitForSecondsRealtime(.7f);Capture(gallery,"01-idle.png");
            foreach(var b in lab.Buttons)
            {
                var e=new PointerEventData(EventSystem.current){pointerId=-1,eligibleForClick=true,position=Vector2.zero};b.OnPointerDown(e);
            }
            yield return new WaitForSecondsRealtime(.12f);Capture(gallery,"02-held.png");
            foreach(var b in lab.Buttons){b.CancelInteraction();b.PlayAccent();}
            lab.Buttons[6].SetSelected(true);lab.Buttons[7].SetBusy(true);
            yield return new WaitForSecondsRealtime(.18f);Capture(gallery,"03-accents.png");
            Check(lab.Buttons[4].rippleGroup.alpha>0,"Ripple rendered during click accent");Check(lab.Buttons[5].sheenGroup.alpha>0,"Sheen rendered during click accent");Check(lab.Buttons[6].selectionGroup.alpha>.5f,"Selection visual follows host state");
            foreach(var b in lab.Buttons){b.reducedMotion=true;b.Step(.01f);}
            Check(lab.Buttons[4].rippleGroup.alpha==0&&lab.Buttons[5].sheenGroup.alpha==0,"Reduced motion cancels decorative accents");
            Capture(gallery,"04-reduced-motion.png");
            Check(lab.ClickCount==0,"Feedback simulation did not dispatch business events");
            var normal=lab.Buttons[0];normal.reducedMotion=false;
            var click=new PointerEventData(EventSystem.current){pointerId=-1,eligibleForClick=true,position=Vector2.zero};
            ExecuteEvents.Execute(normal.gameObject,click,ExecuteEvents.pointerDownHandler);ExecuteEvents.Execute(normal.gameObject,click,ExecuteEvents.pointerUpHandler);
            if(click.eligibleForClick)ExecuteEvents.Execute(normal.gameObject,click,ExecuteEvents.pointerClickHandler);
            Check(lab.ClickCount==1,"UGUI Button delivers one business click");
            Finish();
        }
        void Capture(GalleryBootstrap gallery,string name)
        {
            var canvas=gallery.GetComponentInChildren<Canvas>();var mode=canvas.renderMode;var oldCamera=canvas.worldCamera;float distance=canvas.planeDistance;
            var go=new GameObject("Button lab capture",typeof(Camera));var camera=go.GetComponent<Camera>();camera.enabled=false;camera.orthographic=true;camera.orthographicSize=Screen.height*.5f;camera.transform.position=new Vector3(0,0,-10);camera.nearClipPlane=.1f;camera.farClipPlane=100;
            var rt=new RenderTexture(Screen.width,Screen.height,24);var active=RenderTexture.active;var tex=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);
            try { camera.targetTexture=rt;canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);tex.Apply();File.WriteAllBytes(Path.Combine(folder,name),tex.EncodeToPNG()); }
            finally { canvas.renderMode=mode;canvas.worldCamera=oldCamera;canvas.planeDistance=distance;RenderTexture.active=active;camera.targetTexture=null;rt.Release();Destroy(rt);Destroy(tex);Destroy(go);Canvas.ForceUpdateCanvases(); }
        }
        void Finish() { if(!running)return;running=false;Application.logMessageReceived-=Log;report.passed=report.errors.Count==0;File.WriteAllText(Path.Combine(folder,"button-report.json"),JsonUtility.ToJson(report,true));Debug.Log("BUTTON_STYLE_TEST "+(report.passed?"PASS":"FAIL"));Application.Quit(report.passed?0:1); }
    }
}
