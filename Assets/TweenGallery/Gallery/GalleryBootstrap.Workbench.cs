using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TweenGallery
{
    public sealed partial class GalleryBootstrap
    {
        public static readonly string[] SkinNames={"薄荷胶囊","淡紫织边","金色圆钮","珊瑚软糖","奶油描边","圆角方钮"};
        public static readonly string[] FeedbackNames={"柔软按压","果冻挤压","悬浮抬升","图标回弹","点击涟漪","柔光扫过","选中反馈","加载反馈"};
        static readonly MotionDefinition[] interactiveDefinitions={
            new MotionDefinition{id="G01",name="拖拽与弹性回位",english="Drag and spring return",category="G",kind="真实操作",duration=.65f,strength=1,distance=55,description="按住物体拖动，松开后按当前速度弹性回位。",useCase="拖动反馈、弹性卡片",difference="固定命中区域与内部视觉分开，松开是弹簧回位。",dependencies="MotionDragInput + MotionJellyDrag；visual 是独立子层。"},
            new MotionDefinition{id="G02",name="连续操作与姿态接续",english="Interrupt and continue",category="G",kind="真实操作",duration=.65f,strength=1,distance=130,description="连续点击左、右，从当前可见姿态接续到新的目标。",useCase="可打断的选择、焦点交接",difference="固定时长接续保持位置连续；需要速度连续时使用弹簧。",dependencies="MotionPlayer.ContinueTo + 稳定基线；业务状态由宿主管理。"},
            new MotionDefinition{id="P01",name="旅程达成 · 整页编排",english="Staged achievement page",category="F",kind="页面配方",duration=1.1f,strength=1,distance=24,stagger=.08f,description="背景、面板、徽章、奖励与操作按层次进入。",useCase="成就、胜利、奖励页面",difference="一条确定性页面时间轴；采样不提交业务，结束只发视觉通知。",dependencies="MotionPageSequence + 独立图层；宿主负责奖励与导航。"},
            new MotionDefinition{id="P02",name="奖励选择 · 响应式",english="Responsive reward selection",category="F",kind="页面配方",duration=.3f,strength=1,distance=0,description="短屏、长屏和横屏使用同一选择状态，自适应重排控件。",useCase="奖励选择、弹窗、不同屏幕比例",difference="短屏滚正文，操作区固定；重排不重建选择数据。",dependencies="MotionResponsiveLayout + ScrollRect；安全区由宿主传入。"}
        };
        public static int BrowseCount => MotionCatalog.All.Length+interactiveDefinitions.Length;
        static IEnumerable<MotionDefinition> BrowseDefinitions(){foreach(var d in MotionCatalog.All)yield return d;foreach(var d in interactiveDefinitions)yield return d;}
        Button autoModeButton,liveModeButton,skinButton,variantButton,reducedButton,stateButton;
        Text timelineHint;
        GalleryInteractionPreview integratedPreview;
        bool interactionMode,reducedMotion;
        int selectedSkin,feedbackVariant=-1;
        float feedbackFrequency=7;
        public bool IsInteractionMode => interactionMode;
        public int SelectedSkin => selectedSkin;
        public int FeedbackVariant => feedbackVariant;
        public GalleryInteractionPreview IntegratedPreview => integratedPreview;
        bool IsButtonFamily => !string.IsNullOrEmpty(CurrentEffectId)&&CurrentEffectId.StartsWith("B");
        bool IsSkinApplicable => IsButtonFamily && (feedbackVariant>=0&&interactionMode || CurrentEffectId!="B05"&&CurrentEffectId!="B08");
        void BuildWorkbenchControls()
        {
            skinButton=ButtonAt(root,"造型：薄荷胶囊",364,195,176,30,Cyan,Ink,13,()=>SetButtonSkin((selectedSkin+1)%SkinNames.Length));
            variantButton=ButtonAt(root,"反馈：原效果",548,195,195,30,Violet,Ink,13,()=>SetFeedbackVariant(feedbackVariant==7?-1:feedbackVariant+1));
            reducedButton=ButtonAt(root,"减弱动效：关",751,195,145,30,Hex("343A60"),Ink,13,()=>SetReducedMotion(!reducedMotion));
            stateButton=ButtonAt(root,"完成 / 切换状态",904,195,280,30,Yellow,Ink,13,()=>{if(integratedPreview!=null)integratedPreview.ChangeState();});
        }
        void ConfigureWorkbenchSelection(string id)
        {
            if(id.StartsWith("G"))interactionMode=true;
            feedbackVariant=id=="B01"?0:id=="B02"?1:-1;
        }
        public void SetInteractionMode(bool enabled)
        {
            if(current==null)return;
            if((CurrentEffectId.StartsWith("G")||CurrentEffectId=="P02")&&!enabled){Notice("该效果由真实操作驱动，请直接操作预览。");return;}
            interactionMode=enabled;comparison=false;BuildDemos(new[]{CurrentEffectId});UpdateDefinitionText();RefreshControlLabels();
            if(!enabled)Replay();
        }
        public void SetButtonSkin(int index)
        {
            if(index<0||index>=SkinNames.Length)throw new ArgumentOutOfRangeException(nameof(index));
            selectedSkin=index;if(current==null)return;BuildDemos(new[]{CurrentEffectId});RefreshControlLabels();if(!interactionMode)Replay();
        }
        public void SetFeedbackVariant(int index)
        {
            if(index < -1 || index > 7)throw new ArgumentOutOfRangeException(nameof(index));
            feedbackVariant=index;if(!IsButtonFamily)return;
            interactionMode=true;comparison=false;BuildDemos(new[]{CurrentEffectId});RefreshControlLabels();
        }
        public void SetReducedMotion(bool value)
        {
            reducedMotion=value;if(current==null)return;
            BuildDemos(new[]{CurrentEffectId});RefreshControlLabels();if(!interactionMode)Replay();
        }
        void BuildIntegratedPreview()
        {
            if(CurrentEffectId=="G01")integratedPreview=GalleryInteractionPreview.CreateDrag(demoHost,selectedSkin,options,reducedMotion);
            else if(CurrentEffectId=="G02")integratedPreview=GalleryInteractionPreview.CreateContinuation(demoHost,options,reducedMotion);
            else if(IsButtonFamily&&feedbackVariant>=0){integratedPreview=GalleryInteractionPreview.CreateButton(demoHost,selectedSkin,(ButtonMotionStyle)feedbackVariant,options,reducedMotion);integratedPreview.Feedback.frequency=feedbackFrequency;integratedPreview.Feedback.damping=MotionStyleProfile.Get(styleKind).springDamping;integratedPreview.Feedback.accentDuration=.55f*MotionStyleProfile.Get(styleKind).timeScale;}
            else
            {
                integratedPreview=GalleryInteractionPreview.CreateTrigger(demoHost,CurrentEffectId,options,reducedMotion);
                ApplySelectedSkin(integratedPreview.Player);
            }
        }
        void ApplySelectedSkin(MotionPlayer player)
        {
            if(!IsSkinApplicable||player==null||player.bindings.target==null||!player.effectId.StartsWith("B")||player.effectId=="B05"||player.effectId=="B08")return;
            var image=player.bindings.target.GetComponent<Image>();if(image==null)return;
            image.sprite=GalleryTheme.Skin(selectedSkin);image.type=Image.Type.Simple;image.color=Color.white;image.preserveAspect=true;
            if(selectedSkin==2||selectedSkin==5)
            {
                var r=player.bindings.target;r.sizeDelta=Vector2.one*(player.effectId=="B07"?134:180);
                foreach(var text in r.GetComponentsInChildren<Text>(true))text.gameObject.SetActive(false);
                var star=r.Find("Star");if(star!=null){var rect=star as RectTransform;rect.anchoredPosition=Vector2.zero;rect.sizeDelta=new Vector2(46,46);var glyph=star.GetComponent<Graphic>();if(glyph!=null)glyph.color=Ink;}
                else{var label=Label(r,"★",0,0,r.sizeDelta.x,r.sizeDelta.y,32,Ink,TextAnchor.MiddleCenter);label.rectTransform.anchorMin=label.rectTransform.anchorMax=label.rectTransform.pivot=new Vector2(.5f,.5f);label.rectTransform.anchoredPosition=Vector2.zero;}
            }
            else if(player.effectId!="B07")player.bindings.target.sizeDelta=new Vector2(360,145);
            if(player.effectId=="B07"&&player.bindings.secondaryImage!=null)
            {
                var secondary=player.bindings.secondaryImage;secondary.sprite=GalleryTheme.Skin(selectedSkin);secondary.type=Image.Type.Simple;secondary.color=Color.white;secondary.preserveAspect=true;
                if(selectedSkin==2||selectedSkin==5)
                {
                    secondary.rectTransform.sizeDelta=Vector2.one*134;
                    foreach(var text in secondary.GetComponentsInChildren<Text>(true))text.gameObject.SetActive(false);
                    var glyph=Label(secondary.transform,"★",0,0,134,134,32,Ink,TextAnchor.MiddleCenter);glyph.rectTransform.anchorMin=glyph.rectTransform.anchorMax=glyph.rectTransform.pivot=new Vector2(.5f,.5f);glyph.rectTransform.anchoredPosition=Vector2.zero;
                }
            }
            player.CaptureBaseline();
        }
        void RefreshWorkbenchControls()
        {
            if(autoModeButton==null)return;
            autoModeButton.image.color=interactionMode?Hex("343A60"):Violet;liveModeButton.image.color=interactionMode?Cyan:Hex("343A60");
            autoModeButton.interactable=!CurrentEffectId.StartsWith("G");
            bool show=IsButtonFamily&&!comparison;
            skinButton.gameObject.SetActive(show&&IsSkinApplicable);variantButton.gameObject.SetActive(show);
            reducedButton.gameObject.SetActive(!comparison);
            stateButton.gameObject.SetActive(interactionMode&&integratedPreview!=null&&integratedPreview.Feedback!=null&&(feedbackVariant==6||feedbackVariant==7));
            skinButton.GetComponentInChildren<Text>().text="造型："+SkinNames[selectedSkin];
            variantButton.GetComponentInChildren<Text>().text="操作变体："+(feedbackVariant<0?"原效果":FeedbackNames[feedbackVariant]);
            reducedButton.GetComponentInChildren<Text>().text="减弱动效："+(reducedMotion?"开":"关");
            effectEnglish.gameObject.SetActive(!show&&!interactionMode);
            if(interactionMode&&IsButtonFamily&&feedbackVariant>=0)
            {
                effectTitle.text="按钮 · "+FeedbackNames[feedbackVariant];effectBadge.text="真实操作 / "+CurrentEffectId;
                useCase.text="当前反馈："+FeedbackNames[feedbackVariant]+" · 造型："+SkinNames[selectedSkin]+"。直接操作中间控件体验。";
                distinction.text="造型、原生文字与交互驱动独立；选择和加载由宿主业务状态决定。";
                dependency.text="组件：MotionButtonFeedback；命中层保持固定，动画作用于内部视觉层。";
            }
            timelineSlider.interactable=!interactionMode;
            playLabel.GetComponentInParent<Button>().interactable=!interactionMode&&!reducedMotion;
            slowLabel.GetComponentInParent<Button>().interactable=!interactionMode&&!reducedMotion;
            loopLabel.GetComponentInParent<Button>().interactable=!interactionMode&&!reducedMotion;
            compareLabel.GetComponentInParent<Button>().interactable=!interactionMode&&!CurrentEffectId.StartsWith("G");
            if(timelineHint!=null)timelineHint.text=interactionMode?"直接操作中间对象；时间轴在自动播放模式使用":"拖动右侧时间轴定位；继续播放不会跳回";
            if(interactionMode)
            {
                reverseButton.interactable=false;reverseLabel.text="输入决定方向与状态";easingButton.interactable=integratedPreview!=null&&integratedPreview.Player!=null;
                bool button=integratedPreview!=null&&integratedPreview.Feedback!=null;
                durationSlider.interactable=CurrentEffectId!="G01";strengthSlider.interactable=button||CurrentEffectId=="G01";
                distanceSlider.interactable=CurrentEffectId.StartsWith("G");staggerSlider.interactable=false;
                strengthValue.text=strengthSlider.interactable?options.strength.ToString("0.00")+" ×":"不适用";
                distanceValue.text=distanceSlider.interactable?options.distance.ToString("0")+" px":"不适用";staggerValue.text="不适用";
                progressLabel.text="输入驱动";
            }
            else if(!comparison)UpdateDefinitionText();
            bool liveButton=interactionMode&&integratedPreview!=null&&integratedPreview.Feedback!=null;
            var durationHeading=root.Find("Text 时长");if(durationHeading!=null)durationHeading.GetComponent<Text>().text=liveButton?"弹性频率":"时长";
            bool previousSuppress=suppress;suppress=true;durationSlider.minValue=liveButton?3:.15f;durationSlider.maxValue=liveButton?12:3;
            durationSlider.SetValueWithoutNotify(liveButton?feedbackFrequency:options.duration);suppress=previousSuppress;
            durationValue.text=liveButton?feedbackFrequency.ToString("0.0")+" Hz":interactionMode&&CurrentEffectId=="G01"?"输入驱动":options.duration.ToString("0.00")+" s";
            RefreshRecipeControls();
        }
        bool TrySetWorkbenchParameter(string title,float value)
        {
            if(title!="时长"||!interactionMode||integratedPreview==null||integratedPreview.Feedback==null)return false;
            feedbackFrequency=value;Replay();return true;
        }
        [Serializable] public sealed class InteractionConfiguration
        {
            public string definitionId,driver,variant,ease;
            public float intensity,frequency,damping,duration,maxDisplacement;
            public float accentDuration,dragRange,stretch,maxRotation,distance,leftScale,rightScale,tiltDegrees;
            public Vector2 axisRange;
            public bool reducedMotion,unscaledTime;
        }
        string ExportInteractionJson()
        {
            var config=new InteractionConfiguration{definitionId=CurrentEffectId,driver="MotionPlayer",variant="trigger",duration=options.duration,reducedMotion=reducedMotion,unscaledTime=options.unscaledTime};
            if(integratedPreview!=null)
            {
                if(integratedPreview.Feedback!=null){var b=integratedPreview.Feedback;config.driver="MotionButtonFeedback";config.variant=b.style.ToString();config.intensity=b.intensity;config.frequency=b.frequency;config.damping=b.damping;config.duration=0;config.accentDuration=b.accentDuration;}
                else if(integratedPreview.Jelly!=null){var j=integratedPreview.Jelly;config.driver="MotionDragInput + MotionJellyDrag";config.variant="drag-return";config.frequency=j.frequency;config.damping=j.damping;config.maxDisplacement=j.maxDisplacement;config.dragRange=j.dragRange;config.stretch=j.stretch;config.maxRotation=j.maxRotation;config.axisRange=j.axisRange;config.duration=0;}
                else if(CurrentEffectId=="G02"){config.variant="ContinueTo";config.distance=options.distance;config.leftScale=.94f;config.rightScale=1.08f;config.tiltDegrees=6;config.ease=options.ease=="Preset"?"OutCubic":options.ease;}
                else if(CurrentEffectId=="B07"||CurrentEffectId=="B08")config.variant="toggle";
            }
            var entries=CurrentEffectId.StartsWith("G")?Array.Empty<ConfigurationEntry>():new[]{new ConfigurationEntry{effectId=CurrentEffectId,settings=options.Clone()}};
            return JsonUtility.ToJson(new GalleryConfiguration{schemaVersion=2,previewMode="interaction",styleProfile=styleKind.ToString(),skinIndex=CurrentEffectId=="G01"?5:CurrentEffectId=="G02"?1:IsSkinApplicable?selectedSkin:-1,effects=entries,interaction=config},true);
        }
        string CopyInteractionPrompt()
        {
            string text="将以下主展馆真实操作配置接到我的 Unity UGUI，按 interaction.driver 选择组件；G01/G02 是交互示例标识，不传给 MotionPlayer.Play。命中层固定，视觉层独立，保留已有业务事件。素材使用公共 cream-buttons 图集对应 skinIndex；宿主可替换为同风格素材。导出只包含视觉配置，不代替选中、请求、奖励等业务状态。\n"+ExportInteractionJson();
            GUIUtility.systemCopyBuffer=text;Notice("已复制素材、驱动组件和真实操作参数");return text;
        }
    }
}
