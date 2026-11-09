using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TweenGallery.Editor
{
    public static class ButtonFeedbackValidation
    {
        static readonly List<string> checks=new List<string>();
        static void Check(bool ok,string message) { if(!ok)throw new Exception(message);checks.Add(message); }
        public static void Run()
        {
            checks.Clear();
            var events=new GameObject("Button test events",typeof(EventSystem));
            try
            {
                foreach(ButtonMotionStyle style in Enum.GetValues(typeof(ButtonMotionStyle)))Test(style);
                Directory.CreateDirectory("evidence");File.WriteAllText("evidence/button-feedback-validation.txt","PASS\n"+string.Join("\n",checks));
                Debug.Log("BUTTON_FEEDBACK_VALIDATION PASS "+checks.Count);
            }
            finally { UnityEngine.Object.DestroyImmediate(events); }
        }
        static void Test(ButtonMotionStyle style)
        {
            var go=new GameObject("Fixed hit",typeof(RectTransform),typeof(Image),typeof(Button));
            try
            {
                var root=(RectTransform)go.transform;root.sizeDelta=new Vector2(240,100);
                var visual=new GameObject("surface",typeof(RectTransform)).GetComponent<RectTransform>();visual.SetParent(root,false);
                visual.localScale=new Vector3(.9f,1.1f,1);visual.anchoredPosition=new Vector2(7,9);
                var icon=new GameObject("icon",typeof(RectTransform)).GetComponent<RectTransform>();icon.SetParent(visual,false);icon.localScale=Vector3.one*.8f;
                var b=go.AddComponent<MotionButtonFeedback>();b.surface=visual;b.icon=icon;b.style=style;
                b.ripple=Layer(visual,"ripple");b.rippleGroup=b.ripple.GetComponent<CanvasGroup>();
                b.sheen=Layer(visual,"sheen");b.sheenGroup=b.sheen.GetComponent<CanvasGroup>();
                b.selection=Layer(root,"selection");b.selectionGroup=b.selection.GetComponent<CanvasGroup>();b.CaptureBaseline();
                Vector3 rest=visual.localScale;Vector2 pos=visual.anchoredPosition;Vector2 hit=root.sizeDelta;
                int clicks=0;go.GetComponent<Button>().onClick.AddListener(()=>clicks++);
                var e=new PointerEventData(EventSystem.current){pointerId=-1,position=new Vector2(10,10),button=PointerEventData.InputButton.Left,eligibleForClick=true};
                b.OnPointerDown(e);b.Step(.12f);
                Check(b.IsPressed && Vector3.Distance(visual.localScale,rest)>.001f,style+" compresses on hold");
                Check(root.localScale==Vector3.one && root.sizeDelta==hit,style+" fixed hit area");
                var other=new PointerEventData(EventSystem.current){pointerId=2,eligibleForClick=true};b.OnPointerDown(other);
                Check(!other.eligibleForClick && b.IsPressed,style+" second pointer rejected");
                b.OnPointerUp(e);for(int i=0;i<180;i++)b.Step(1f/60);
                Check(Vector3.Distance(visual.localScale,rest)<.0001f && Vector2.Distance(visual.anchoredPosition,pos)<.0001f,style+" restores non-unit baseline");
                Check(clicks==0,style+" feedback does not invoke business click");
                ExecuteEvents.Execute(go,e,ExecuteEvents.pointerClickHandler);Check(clicks==1,style+" original Button listener preserved");
                e.eligibleForClick=true;b.OnPointerDown(e);e.position+=Vector2.right*40;b.OnPointerMove(e);b.OnPointerUp(e);
                Check(!e.eligibleForClick && !b.HasActivePointer,style+" drag cancels click without taking drag handler");
                e.position=Vector2.zero;e.eligibleForClick=true;b.OnPointerDown(e);b.OnPointerExit(e);b.OnPointerUp(e);
                Check(!e.eligibleForClick,style+" release outside cancels click");
                b.SetSelected(true);b.reducedMotion=true;b.PlayAccent();b.Step(.5f);
                Check(b.IsSelected && icon.localScale==Vector3.one*.8f,style+" reduced motion preserves semantic selection without icon pulse");
                b.SetBusy(true);e.eligibleForClick=true;b.OnPointerDown(e);Check(!e.eligibleForClick && !b.IsPressed,style+" busy rejects new press");b.SetBusy(false);
                b.reducedMotion=false;e.eligibleForClick=true;b.OnPointerDown(e);b.Step(.1f);go.SendMessage("OnApplicationFocus",false);b.Step(2);
                Check(!e.eligibleForClick && !b.HasActivePointer,style+" focus loss cancels owned pointer");go.SendMessage("OnApplicationFocus",true);
                b.SetSelected(false);b.Step(2);b.OnSubmit(new BaseEventData(EventSystem.current));b.Step(.03f);
                Check(b.PressAmount>0,style+" keyboard Submit has visible feedback without business dispatch");
                e.eligibleForClick=true;b.OnPointerDown(e);b.Step(.03f);go.GetComponent<Button>().interactable=false;
                b.SendMessage("Update");Check(!e.eligibleForClick && !b.HasActivePointer,style+" interactable change cancels press");
                b.SetSelected(false);b.Step(1);b.enabled=false;
                Check(visual.localScale==rest && visual.anchoredPosition==pos && icon.localScale==Vector3.one*.8f,style+" disable restores exact authored transforms");
            }
            finally { UnityEngine.Object.DestroyImmediate(go); }
        }
        static RectTransform Layer(Transform parent,string name)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(CanvasGroup));go.transform.SetParent(parent,false);go.GetComponent<CanvasGroup>().alpha=0;return (RectTransform)go.transform;
        }
    }
}
