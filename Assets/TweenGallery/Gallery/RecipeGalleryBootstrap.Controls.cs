using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TweenGallery.Recipes
{
    public sealed partial class RecipeGalleryBootstrap
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("user32.dll")] static extern bool ShowWindow(System.IntPtr window,int command);
#endif
        void MinimizeWindow(){
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            ShowWindow(System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle,6);
#endif
        }
        void ToggleWindowSize(){bool expanded=Screen.fullScreenMode!=FullScreenMode.Windowed;Screen.SetResolution(expanded?1600:Screen.currentResolution.width,expanded?1000:Screen.currentResolution.height,expanded?FullScreenMode.Windowed:FullScreenMode.FullScreenWindow);}
        public void SelectShortcut(string name) {
            shortcuts=true;SelectedShortcut=name;query="";search.SetTextWithoutNotify("");layerFilter=stateFilter=0;
            string id="03.08";PreviewMode="scale";
            switch(name){case "翻转切换":PreviewMode="flip";break;case "排列过渡":PreviewMode="arrange";id="07.04";break;
                case "拖拽放置":PreviewMode="";id="08.02";break;case "放置校验":PreviewMode="";id="08.05";break;case "槽位反馈":PreviewMode="";id="08.07";break;
                case "位移滑动":PreviewMode="slide";id="01.02";break;case "缩放弹性":PreviewMode="scale";id="05.03";SelectedCurveIndex=3;break;
                case "旋转动画":PreviewMode="rotate";id="12.03";break;case "淡入淡出":PreviewMode="fade";id="01.01";break;}
            referenceScaleMode=PreviewMode=="scale";RefreshDirectory();Select(id);
        }
        string ModeTitle(){switch(PreviewMode){case "flip":return "翻转 (Flip)";case "arrange":return "排列 (Arrange)";case "slide":return "位移 (Slide)";case "rotate":return "旋转 (Rotate)";case "fade":return "淡入淡出 (Fade)";default:return "缩放 (Scale)";}}
        void OpenModeMenu(){
            if(popup!=null){Destroy(popup.gameObject);popup=null;return;}
            popup=RecipeUI.Box(root,"Animation choices",635,3,290,250,RecipeUI.Dark).rectTransform;
            string[] titles={"选中缩放","翻转切换","排列过渡","位移滑动","旋转动画","淡入淡出"};
            for(int i=0;i<titles.Length;i++){string title=titles[i];Tool(popup,title,0,100-i*40,275,()=>{Destroy(popup.gameObject);popup=null;SelectShortcut(title);},false,36);}
        }
        public void ChooseCurve(int index){
            SelectedCurveIndex=Mathf.Clamp(index,0,3);Stage.Curve=CurveOptions[SelectedCurveIndex];RefreshCurveSelection();
            if(Stage.CanSeek)Stage.Replay();
            else if(Stage.ReferenceComposition&&Stage.Definition.Group!=8&&PreviewMode!="flip")Stage.PreviewReferenceEffect(string.IsNullOrEmpty(PreviewMode)?"scale":PreviewMode);
        }
        void RefreshCurveSelection(){
            for(int i=0;i<4;i++){if(curveButtons[i]==null)continue;bool selected=i==SelectedCurveIndex;curveButtons[i].color=selected?new Color(.4f,.85f,1):new Color(.7f,.74f,.82f);var outline=curveButtons[i].GetComponent<Outline>()??curveButtons[i].gameObject.AddComponent<Outline>();outline.effectDistance=new Vector2(2,-2);outline.effectColor=new Color(.08f,.85f,1,selected?1:0);if(curveGraphs[i]!=null)curveGraphs[i].showCursor=selected;}
            if(curveHint!=null)curveHint.text="当前："+CurveNames[SelectedCurveIndex]+" · 曲线与动画使用同一计算";
        }
        public void TogglePresentationLoop(){if(!Stage.CanSeek)GuidedOperation();if(guidance!=null){StopCoroutine(guidance);guidance=null;}Stage.ToggleLoop();}
        public void ReversePresentation(){if(!Stage.CanSeek)GuidedOperation();if(guidance!=null){StopCoroutine(guidance);guidance=null;}Stage.ReversePresentation();}
        void ResetCurrent(){float duration=Stage.Duration,strength=Stage.Strength,hz=Stage.SpringFrequency,damping=Stage.SpringDamping;Select(SelectedId);Stage.Duration=duration;Stage.Strength=strength;Stage.SpringFrequency=hz;Stage.SpringDamping=damping;BuildReferenceInspector();}
        void StartDragGuide(bool invalid){if(guidance!=null)StopCoroutine(guidance);guidance=StartCoroutine(GuideDrag(Stage,invalid));}
        void CancelDragGuide(){if(guidance!=null){StopCoroutine(guidance);guidance=null;}Stage.Trigger("取消");}
        IEnumerator GuideDrag(RecipeStage target,bool invalid){
            if(target.Definition.Number==7){target.Trigger("模拟拾起");yield return new WaitForSecondsRealtime(.2f);target.Trigger(invalid?"移到无效槽":"移到有效槽");yield return new WaitForSecondsRealtime(.3f);target.Trigger("松开放置");guidance=null;yield break;}
            var item=target.ReferenceDragItem;var destination=invalid?target.ReferenceInvalidSlot:target.ReferenceValidSlot;
            var pointer=item.GetComponent<RecipePointer>();var start=RectTransformUtility.WorldToScreenPoint(null,item.position);var end=RectTransformUtility.WorldToScreenPoint(null,destination.position);
            var e=new PointerEventData(EventSystem.current){pointerId=-55,position=start,button=PointerEventData.InputButton.Left};pointer.OnPointerDown(e);
            float t=0;while(t<1){t=target.Reduced?1:Mathf.Min(1,t+Time.unscaledDeltaTime*target.Speed/.7f);e.position=Vector2.Lerp(start,end,RecipeCurveGraphic.Evaluate(DG.Tweening.Ease.InOutCubic,t));pointer.OnDrag(e);yield return null;}
            pointer.OnPointerUp(e);guidance=null;
        }
        IEnumerator GuideRecipe(RecipeStage target){
            int g=target.Definition.Group,n=target.Definition.Number;
            if(g==5){
                if(n==1){target.Trigger("移入");}
                else if(n==2){target.Trigger("移入");yield return new WaitForSecondsRealtime(.35f);target.Trigger("移出");}
                else if(n==7){target.Trigger("禁用／启用");yield return new WaitForSecondsRealtime(.6f);target.Trigger("禁用／启用");}
                else{target.Trigger("按下");yield return new WaitForSecondsRealtime(n==8?1.3f:.25f);if(n==6)target.Trigger("移出");target.Trigger("松开");}
            } else if(g==2&&n==2)target.Trigger("关闭");
            else if(g==4&&n==6)target.Trigger("下一页");
            else if(g==6&&n==5){target.Trigger("聚焦输入");yield return new WaitForSecondsRealtime(.4f);target.Trigger("失焦");}
            else if(g==6&&(n==6||n==7)){if(n==7)target.Trigger("填入示例");else target.GetComponentInChildren<InputField>().text="x";target.Trigger("提交校验");}
            else if(g==11&&n==2)target.Trigger("展示星级");
            else{foreach(var key in target.Actions.Keys){target.Trigger(key);break;}}
            guidance=null;
        }
        void UpdateWorkbench(){
            if(speedButton!=null){speedButton.interactable=Stage.ClockEditable;speedButton.GetComponentInChildren<Text>().text=Stage.ClockEditable?(slow?"速度 0.25×":"速度 1×"):"实时输入";if(!Stage.ClockEditable)Stage.SetSpeed(1);}
            bool typing=EventSystem.current!=null&&EventSystem.current.currentSelectedGameObject!=null&&EventSystem.current.currentSelectedGameObject.GetComponent<InputField>()!=null;
            if(!typing&&Stage.CanSeek){if(Input.GetKeyDown(KeyCode.Space))Stage.TogglePause();if(Input.GetKeyDown(KeyCode.R))Stage.Replay();}
            if(previewDue>=0&&Time.unscaledTime>=previewDue){previewDue=-1;if(Stage.ReferenceComposition&&Stage.Definition.Group!=8)Stage.PreviewReferenceEffect(PreviewMode);}
            for(int i=0;i<4;i++)if(curveGraphs[i]!=null)curveGraphs[i].Sample(Stage.CanSeek?Stage.Progress:Mathf.Repeat(Time.unscaledTime*.7f,1));
            if(loopLabel!=null)loopLabel.text=Stage.Looping?"循环：开":"循环：关";
            if(reverseLabel!=null)reverseLabel.text=Stage.Backwards?"改为正向":"反向播放";
            if(referenceObjectName!=null&&Stage.Definition.Group==8)referenceObjectName.text=Stage.Get("itemKind")==3?"名称  恢复药水":Stage.Get("itemKind")==2?"名称  蓝色水晶":"名称  能量晶石";
            if(workbenchHint!=null)workbenchHint.text=Stage.ReferenceComposition?(Stage.Definition.Group==8?"拖动物品到插槽 · Esc 取消 · 右侧可演示接受 / 拒绝":"单击选中 · 双击翻面 · 曲线点击即预览")+"   |   "+Stage.State:"先操作舞台或右侧播放，再用下方时间轴检查过程";
            if(popup!=null&&Input.GetKeyDown(KeyCode.Escape)){Destroy(popup.gameObject);popup=null;}
        }
    }
}
