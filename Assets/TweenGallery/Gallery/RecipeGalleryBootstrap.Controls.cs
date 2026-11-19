using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TweenGallery.Recipes
{
    public sealed partial class RecipeGalleryBootstrap
    {
        bool guidePaused;
        void StopGuidance(){if(guidance!=null){StopCoroutine(guidance);guidance=null;}guidePaused=false;}
        void PlayOrPause(){
            if(guidance!=null){guidePaused=!guidePaused;if(Stage.CanSeek&&Stage.Paused!=guidePaused)Stage.TogglePause();return;}
            if(Stage.IsPlaying||Stage.Paused){Stage.TogglePause();return;}
            if(Stage.CanSeek){Stage.Replay();return;}GuidedOperation();
        }
        public void GuidedOperation(){StopGuidance();if(Stage.Get("held")==1)Stage.CancelInteractions();Stage.Record("引导播放");guidance=StartCoroutine(Stage.Definition.Group==8?GuideDrag(Stage):GuideRecipe(Stage));}
        public void ChooseCurve(int index){SelectedCurveIndex=Mathf.Clamp(index,0,3);Stage.Curve=CurveOptions[SelectedCurveIndex];RefreshCurveSelection();if(Stage.CanSeek){StopGuidance();Stage.Replay();}}
        void RefreshCurveSelection(){for(int i=0;i<4;i++){if(curveButtons[i]==null)continue;bool selected=i==SelectedCurveIndex;curveButtons[i].color=selected?RecipeUI.Blue:RecipeUI.Surface;var o=curveButtons[i].GetComponent<Outline>()??curveButtons[i].gameObject.AddComponent<Outline>();o.effectColor=new Color(.15f,.8f,1,selected?.9f:0);o.effectDistance=new Vector2(1,-1);if(curveGraphs[i]!=null)curveGraphs[i].showCursor=selected;}if(curveHint!=null)curveHint.text="当前："+CurveNames[SelectedCurveIndex]+" · 点击主体观察";}
        public void TogglePresentationLoop(){if(!Stage.CanSeek)GuidedOperation();StopGuidance();Stage.ToggleLoop();}
        public void ReversePresentation(){if(!Stage.CanSeek)GuidedOperation();StopGuidance();Stage.ReversePresentation();}
        void ResetCurrent(){Select(SelectedId);}
        void OpenPlaybackMenu(){
            if(popup!=null){CloseMenu();return;}
            popup=RecipeUI.Box(root,"Playback options",345,-295,235,232,RecipeUI.Surface).rectTransform;popupFrame=Time.frameCount;
            var loop=Tool(popup,Stage.Looping?"循环：开":"循环：关",0,88,218,()=>{TogglePresentationLoop();CloseMenu();},false,38);loop.interactable=Stage.ClockEditable;
            var reverse=Tool(popup,"反向播放",0,44,218,()=>{ReversePresentation();CloseMenu();},false,38);reverse.interactable=Stage.ClockEditable;
            Tool(popup,"重置场景与参数",0,0,218,ResetCurrent,false,38);Tool(popup,"复制当前参数",0,-44,218,()=>{CopyParameters();CloseMenu();},false,38);Tool(popup,"关闭",0,-88,218,CloseMenu,false,38);
        }
        IEnumerator WaitGuide(float seconds){float elapsed=0;while(elapsed<seconds){if(!guidePaused)elapsed+=Time.unscaledDeltaTime;yield return null;}}
        IEnumerator GuideDrag(RecipeStage target){
            int n=target.Definition.Number;if(n==1){target.Trigger("模拟拾起");guidance=null;yield break;}
            var item=target.ReferenceDragItem;var destination=n==4||n==6?target.ReferenceInvalidSlot:target.ReferenceValidSlot;
            var p=item.GetComponent<RecipePointer>();var start=RectTransformUtility.WorldToScreenPoint(null,item.position);var end=RectTransformUtility.WorldToScreenPoint(null,destination.position);
            var e=new PointerEventData(EventSystem.current){pointerId=-55,position=start,button=PointerEventData.InputButton.Left};p.OnPointerDown(e);
            float t=0;while(t<1){if(!guidePaused){t=target.Reduced?1:Mathf.Min(1,t+Time.unscaledDeltaTime/.9f);e.position=Vector2.Lerp(start,end,RecipeCurveGraphic.Evaluate(DG.Tweening.Ease.InOutCubic,t));p.OnDrag(e);}yield return null;}
            if(n==8)target.Trigger("取消");else if(n>=5)p.OnPointerUp(e);else p.ReleaseCapture();guidance=null;
        }
        IEnumerator GuideRecipe(RecipeStage target){
            int g=target.Definition.Group,n=target.Definition.Number;
            if(g==5){if(n==1)target.Trigger("移入");else if(n==2){target.Trigger("移入");yield return WaitGuide(.4f);target.Trigger("移出");}else if(n==7){target.Trigger("禁用／启用");yield return WaitGuide(.7f);target.Trigger("禁用／启用");}else{target.Trigger("按下");yield return WaitGuide(n==8?1.3f:.4f);if(n==6)target.Trigger("移出");target.Trigger("松开");}}
            else if(g==2&&n==2)target.Trigger("关闭");else if(g==4&&n==6)target.Trigger("下一页");
            else if(g==6&&n==5){target.Trigger("聚焦输入");yield return WaitGuide(.5f);target.Trigger("失焦");}
            else if(g==6&&(n==6||n==7)){if(n==7)target.Trigger("填入示例");else target.GetComponentInChildren<InputField>().text="x";target.Trigger("提交校验");}
            else if(g==11&&n==2)target.Trigger("展示星级");else foreach(var key in target.Actions.Keys){target.Trigger(key);break;}guidance=null;
        }
        void UpdateWorkbench(){
            playLabel.text=guidance!=null?(guidePaused?"继续":"暂停演示"):Stage.Paused?"继续":Stage.IsPlaying?"暂停":"播放演示";
            speedButton.interactable=Stage.ClockEditable;speedLabel.text=Stage.ClockEditable?(slow?"速度 .25×":"速度 1×"):"实时输入";
            if(!Stage.ClockEditable)Stage.SetSpeed(1);
            if(previewDue>=0&&Time.unscaledTime>=previewDue){previewDue=-1;if(Stage.CanSeek)Stage.Replay();}
            for(int i=0;i<4;i++)if(curveGraphs[i]!=null)curveGraphs[i].Sample(Stage.CanSeek?Stage.Progress:0);
            workbenchHint.text=feedbackUntil>Time.unscaledTime?"已复制当前配方参数":Stage.Definition.Group==8?"直接拖动物品 · Esc 取消 · 下方播放按钮按当前案例演示":"主体可直接操作 · P 播放/暂停 · R 重播最近一次表现";
            bool typing=EventSystem.current!=null&&EventSystem.current.currentSelectedGameObject!=null&&EventSystem.current.currentSelectedGameObject.GetComponent<InputField>()!=null;
            if(!typing){if(Input.GetKeyDown(KeyCode.P))PlayOrPause();if(Input.GetKeyDown(KeyCode.R)){if(Stage.CanSeek)Stage.Replay();else GuidedOperation();}}
            if(popup!=null&&(Input.GetKeyDown(KeyCode.Escape)||Time.frameCount>popupFrame&&Input.GetMouseButtonDown(0)&&!RectTransformUtility.RectangleContainsScreenPoint(popup,Input.mousePosition,null)))CloseMenu();
        }
    }
}
