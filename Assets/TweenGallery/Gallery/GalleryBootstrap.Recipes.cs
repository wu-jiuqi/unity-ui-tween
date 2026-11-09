using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace TweenGallery
{
    public sealed partial class GalleryBootstrap
    {
        MotionStyleKind styleKind=MotionStyleKind.Standard;
        Button[] styleButtons;
        GalleryPagePreview pagePreview;GalleryResponsivePreview responsivePreview;
        readonly Dictionary<RectTransform,Vector2> playbackPositions=new Dictionary<RectTransform,Vector2>();
        readonly List<GameObject> noteObjects=new List<GameObject>();
        int responsiveChoice,responsiveMode;
        Vector2[] responsiveSizes;float[] responsiveScroll;
        public MotionStyleKind StyleKind=>styleKind;
        public GalleryPagePreview PagePreview=>pagePreview;
        public GalleryResponsivePreview ResponsivePreview=>responsivePreview;
        bool IsRecipe=>CurrentEffectId=="P01"||CurrentEffectId=="P02";
        MotionSettings StyledSettings(MotionDefinition definition)=>MotionStyleProfile.Get(styleKind).Apply(MotionSettings.FromDefinition(definition));
        void BuildStyleControls()
        {
            foreach(RectTransform child in root)
            {
                var pos=child.anchoredPosition;
                if(pos.x>=1234&&pos.y<=-190&&pos.y>=-840)child.anchoredPosition=pos+Vector2.down*32;
                if(pos.x>=340&&pos.x<1212&&pos.y<=-586&&pos.y>=-675)playbackPositions[child]=pos;
                if(pos.x>=340&&pos.x<1212&&pos.y<=-692&&pos.y>=-856)noteObjects.Add(child.gameObject);
            }
            var note=root.Find("Text 含编号、全部参数及对比项目");if(note!=null)note.gameObject.SetActive(false);
            styleButtons=new Button[3];string[] labels={"克制","标准","庆祝"};
            for(int i=0;i<3;i++){int at=i;styleButtons[i]=ButtonAt(root,"Style "+labels[i],1257+i*98,193,94,32,Hex("343A60"),Ink,15,()=>SelectStyle((MotionStyleKind)at));styleButtons[i].GetComponentInChildren<Text>().text=labels[i];}
        }
        public void SelectStyle(MotionStyleKind kind)
        {
            if(!Enum.IsDefined(typeof(MotionStyleKind),kind))throw new ArgumentOutOfRangeException(nameof(kind));
            styleKind=kind;feedbackFrequency=MotionStyleProfile.Get(kind).springFrequency;if(current==null)return;
            options=StyledSettings(current);SyncSliders();
            if(comparison){var ids=new List<string>();foreach(var p in players)ids.Add(p.effectId);BuildDemos(ids.ToArray());}
            else BuildDemos(new[]{CurrentEffectId});
            RefreshControlLabels();if(!interactionMode)Replay();Notice("已应用"+new[]{"克制","标准","庆祝"}[(int)kind]+"档位；参数从基准生成，可继续微调");
        }
        void PrepareRecipeRebuild()
        {
            if(responsivePreview!=null)
            {
                responsiveChoice=responsivePreview.SelectedReward;responsiveMode=responsivePreview.Mode;
                var layouts=responsivePreview.Layouts;responsiveSizes=Array.ConvertAll(layouts,l=>(l.transform as RectTransform).rect.size);
                responsiveScroll=Array.ConvertAll(layouts,l=>l.IsScrollable?l.scroll.verticalNormalizedPosition:l.SavedScroll);
            }
            pagePreview=null;responsivePreview=null;
            bool expanded=IsRecipe&&!comparison;
            var panel=root.Find("Preview panel") as RectTransform;if(panel!=null)panel.sizeDelta=new Vector2(872,expanded?648:468);
            demoHost.sizeDelta=new Vector2(836,expanded?518:319);
            foreach(var item in playbackPositions)if(item.Key!=null)item.Key.anchoredPosition=item.Value+Vector2.down*(expanded?179:0);
            foreach(var item in noteObjects)if(item!=null)item.SetActive(!expanded);
        }
        void BuildRecipe()
        {
            if(CurrentEffectId=="P01")pagePreview=GalleryPagePreview.Create(demoHost,options,MotionStyleProfile.Get(styleKind),reducedMotion,interactionMode);
            else
            {
                interactionMode=true;responsivePreview=GalleryResponsivePreview.Create(demoHost,MotionStyleProfile.Get(styleKind),reducedMotion);
                if(responsiveSizes!=null){var layouts=responsivePreview.Layouts;for(int i=0;i<layouts.Length;i++){(layouts[i].transform as RectTransform).sizeDelta=responsiveSizes[i];layouts[i].Refresh();layouts[i].RestoreScroll(responsiveScroll[i]);}}
                responsivePreview.SetMode(responsiveMode);responsivePreview.SelectReward(responsiveChoice);
            }
        }
        bool UpdateRecipe()
        {
            if(pagePreview!=null)
            {
                paused=!pagePreview.Sequence.IsPlaying;SetTextIfChanged(playLabel,paused?"继续":"暂停");
                float progress=pagePreview.Sequence.Progress;timelineSlider.SetValueWithoutNotify(progress);progressLabel.text=Mathf.RoundToInt(progress*100)+"%";
                if(Time.unscaledTime>statusUntil)SetTextIfChanged(status,pagePreview.Sequence.IsPlaying?"整页编排播放中 · 背景 → 面板 → 主视觉 → 奖励与操作":pagePreview.Status);return true;
            }
            if(responsivePreview!=null){if(Time.unscaledTime>statusUntil)SetTextIfChanged(status,responsivePreview.Status);return true;}
            return false;
        }
        void RefreshRecipeControls()
        {
            if(styleButtons!=null)for(int i=0;i<styleButtons.Length;i++)styleButtons[i].image.color=i==(int)styleKind?Cyan:Hex("343A60");
            if(!IsRecipe)return;
            bool page=CurrentEffectId=="P01";
            autoModeButton.interactable=page;effectEnglish.gameObject.SetActive(false);variantButton.gameObject.SetActive(false);skinButton.gameObject.SetActive(false);stateButton.gameObject.SetActive(false);
            timelineSlider.interactable=page;playLabel.GetComponentInParent<Button>().interactable=page&&!reducedMotion;
            slowLabel.GetComponentInParent<Button>().interactable=page&&!reducedMotion;loopLabel.GetComponentInParent<Button>().interactable=false;compareLabel.GetComponentInParent<Button>().interactable=false;
            reverseButton.interactable=false;reverseLabel.text=page?"分层页面时间轴":"布局与输入驱动";easingButton.interactable=false;
            durationSlider.interactable=page;strengthSlider.interactable=page;distanceSlider.interactable=page;staggerSlider.interactable=page;
            durationValue.text=page?options.duration.ToString("0.00")+" s":"自适应";strengthValue.text=page?options.strength.ToString("0.00")+" ×":"随档位";
            distanceValue.text=page?options.distance.ToString("0")+" px":"随布局";staggerValue.text=page?options.stagger.ToString("0.00")+" s":"不适用";
            timelineHint.text=page?"时间轴可采样整页；阶段按钮直接定位，采样不提交任何业务":"短屏滚动正文，操作区固定；点击布局切换或旋转当前视图";
            if(!page)progressLabel.text="布局驱动";
        }
        [Serializable] public sealed class RecipeConfiguration
        {
            public string id,driver,panelAsset,emblemAsset,iconAtlas;
            public MotionSettings settings;
            public MotionStyleProfile profile;
            public float[] milestones;
            public Vector2[] referenceSizes;
            public Vector4[] safeInsets;
            public int displayMode;
            public bool reducedMotion;
            public float cardIntensity,buttonIntensity,springFrequency,springDamping,accentDuration;
        }
        string ExportRecipeJson()
        {
            var config=new RecipeConfiguration{id=CurrentEffectId,driver=CurrentEffectId=="P01"?"MotionPageSequence":"MotionResponsiveLayout",panelAsset="StyleArt/cream-panel",emblemAsset="StyleArt/achievement-medal",iconAtlas="StyleArt/reward-icons",settings=options.Clone(),profile=MotionStyleProfile.Get(styleKind),reducedMotion=reducedMotion};
            config.cardIntensity=.6f*config.profile.strengthScale;config.buttonIntensity=config.profile.strengthScale;config.springFrequency=config.profile.springFrequency;config.springDamping=config.profile.springDamping;config.accentDuration=.55f*config.profile.timeScale;
            if(pagePreview!=null)config.milestones=new[]{0,.25f/1.1f,.65f/1.1f,1};
            if(responsivePreview!=null){var layouts=responsivePreview.Layouts;config.referenceSizes=Array.ConvertAll(layouts,l=>(l.transform as RectTransform).rect.size);config.safeInsets=Array.ConvertAll(layouts,l=>l.insets);config.displayMode=responsivePreview.Mode;}
            return JsonUtility.ToJson(new GalleryConfiguration{schemaVersion=3,previewMode=CurrentEffectId=="P01"?"page":"responsive",styleProfile=styleKind.ToString(),recipe=config,effects=Array.Empty<ConfigurationEntry>()},true);
        }
        string CopyRecipePrompt()
        {
            string text="按以下页面配方配置接入宿主 Unity UGUI。MotionSettings 已应用档位，不重复乘 profile；P 编号不是 MotionPlayer.Play 编号。文字、底板、徽章与图标独立绑定；只复用视觉和布局，不写奖励/进度/导航。预览用的图片可替换为宿主同风格资源。\n"+ExportRecipeJson();GUIUtility.systemCopyBuffer=text;Notice("已复制页面结构、档位、素材和布局配置");return text;
        }
    }
}
