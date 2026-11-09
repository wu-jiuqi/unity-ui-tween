using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TweenGallery
{
    /// <summary>The portable exhibition UI. All motion is delegated to the reusable MotionPlayer.</summary>
    public sealed class GalleryBootstrap : MonoBehaviour
    {
        public bool IsReady { get; private set; }
        public string CurrentEffectId { get; private set; }
        public MotionPlayer CurrentPlayer { get { return players.Count > 0 ? players[0] : null; } }
        public MotionPlayer[] ActivePlayers { get { return players.ToArray(); } }

        static readonly Color Ink = Hex("11142B"), Panel = Hex("202442"), Muted = Hex("AEB5D5");
        static readonly Color Violet = Hex("9254FF"), Cyan = Hex("19D9FF"), Yellow = Hex("FFE14A"), Pink = Hex("FF4EAF");
        const string StrengthIds = "A03 A04 A06 A08 B01 B02 B03 B04 B05 B06 B07 D03 D04 D05 D06 D08 D09 D10 F01 F02 F03 F04 F05 F06 E01 E03 E04 E05 E06 E07";
        const string DistanceIds = "A02 A05 B04 B07 C01 C02 C05 D03 D04 D08 D09 E01 E03 E05 E06 E07 F01 F02 F03 F04 F05 F06";
        const string StaggerIds = "D04 D05 D06 D08 E05 E06 F01 F02 F03 F04 F05 F06";
        readonly List<MotionPlayer> players = new List<MotionPlayer>();
        readonly Dictionary<string, Image> rows = new Dictionary<string, Image>();
        readonly Dictionary<string, Button> categories = new Dictionary<string, Button>();
        readonly List<string> visibleIds = new List<string>();
        Font font;
        Sprite rounded;
        RectTransform root, demoHost, listContent, easingPopup;
        ScrollRect listScroll;
        Text effectTitle, effectEnglish, effectBadge, useCase, distinction, dependency, status, playLabel, slowLabel, loopLabel, reverseLabel, compareLabel, easingLabel, progressLabel, countLabel;
        Image progressFill;
        Slider durationSlider, strengthSlider, distanceSlider, staggerSlider, timelineSlider;
        int lastProgressPercent = -1;
        float lastProgress = -1f;
        Text durationValue, strengthValue, distanceValue, staggerValue;
        Button reverseButton, easingButton;
        InteractionLab interactionLab;
        public bool IsInteractionLabOpen => interactionLab != null;
        public InteractionLab CurrentInteractionLab => interactionLab;
        MotionDefinition current;
        MotionSettings options;
        string category = "", search = "";
        bool paused, comparison, suppress;
        float statusUntil;
        readonly string[] easeValues = { "Preset", "Linear", "OutQuad", "OutCubic", "InOutSine", "OutBack", "OutBounce", "OutElastic" };
        readonly string[] easeNames = { "Preset · 推荐曲线", "Linear · 匀速", "OutQuad · 柔和减速", "OutCubic · 快速进入", "InOutSine · 平滑往返", "OutBack · 越界回拉", "OutBounce · 落地弹跳", "OutElastic · 弹簧振荡" };

        void Start()
        {
            Build();
            IsReady = true;
            SelectEffect("A04");
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "--button-lab") >= 0) OpenButtonLab();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (interactionLab != null) interactionLab.Close();
                else if (easingPopup != null && easingPopup.gameObject.activeSelf) easingPopup.gameObject.SetActive(false);
                else Application.Quit();
            }
            if (!IsReady || players.Count == 0) return;
            SyncTimeline();
            if (Time.unscaledTime > statusUntil)
                SetTextIfChanged(status, paused ? "已暂停 · 拖动时间轴逐帧观察，点击继续从此处播放" : players[0].IsPlaying ? "正在播放 · 展馆与模板使用相同动画逻辑" : "播放完成 · 点击重播，或拖动时间轴查看任意时刻");
        }

        void Build()
        {
            font = Resources.Load<Font>("GalleryFont");
            if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            rounded = MakeRoundedSprite();
            if (FindFirstObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            var canvasObject = new GameObject("Motion Gallery Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600, 900);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            Image background = ImageAt(canvasObject.transform, "Background", 0, 0, 1600, 900, Ink, false);
            Stretch(background.rectTransform);
            root = new GameObject("Centered 1600 x 900 Layout", typeof(RectTransform)).GetComponent<RectTransform>();
            root.SetParent(canvasObject.transform, false);
            root.anchorMin = root.anchorMax = root.pivot = new Vector2(.5f, .5f);
            root.sizeDelta = new Vector2(1600, 900);

            ImageAt(root, "Brand", 30, 25, 46, 46, Violet);
            Label(root, "M", 30, 27, 46, 40, 27, Color.white, TextAnchor.MiddleCenter, true);
            Label(root, "UI MOTION LAB", 91, 13, 490, 44, 27, Color.white, TextAnchor.MiddleLeft, true);
            Label(root, "Unity UI 动效展馆  /  看见差别，带走手感", 92, 56, 650, 24, 15, Muted);
            ButtonAt(root, "交互实验室", 1046, 30, 200, 42, Pink, Color.white, 17, OpenInteractionLab);
            ButtonAt(root, "按钮工坊", 1285, 30, 118, 42, Cyan, Ink, 16, OpenButtonLab);
            Chip(root, "UNITY 6", 1417, 30, 150, Violet, Color.white);

            ImageAt(root, "Catalog panel", 28, 104, 290, 752, Panel);
            Label(root, "动效图鉴", 48, 116, 200, 44, 24, Color.white, TextAnchor.MiddleLeft, true);
            countLabel = Label(root, MotionCatalog.All.Length + " / " + MotionCatalog.All.Length, 230, 125, 65, 26, 14, Muted, TextAnchor.MiddleRight);
            MakeSearch();
            string[] keys = { "", "A", "B", "C", "D", "E", "F" };
            string[] labels = { "全部", "出现", "交互", "转场", "奖励", "引导", "进阶" };
            for (int i = 0; i < keys.Length; i++)
            {
                string key = keys[i];
                Button button = ButtonAt(root, labels[i], 45 + i % 4 * 64, 218 + i / 4 * 36, 60, 29,
                    key == "" ? Violet : Hex("303657"), Color.white, 14, () => SetCategory(key));
                categories.Add(key, button);
            }
            MakeList();

            ImageAt(root, "Preview panel", 340, 104, 872, 468, Panel);
            Label(root, "THE PLAYGROUND", 364, 122, 440, 25, 13, Cyan, TextAnchor.MiddleLeft, true);
            effectTitle = Label(root, "", 364, 146, 640, 49, 28, Color.white, TextAnchor.MiddleLeft, true);
            effectEnglish = Label(root, "", 364, 198, 660, 27, 15, Muted);
            effectBadge = Label(root, "", 1015, 131, 169, 32, 16, Yellow, TextAnchor.MiddleRight, true);
            demoHost = Rect(root, "Live motion stage", 358, 231, 836, 319);

            ImageAt(root, "Playback panel", 340, 586, 872, 89, Panel);
            ButtonAt(root, "重播", 358, 604, 103, 42, Violet, Color.white, 17, Replay);
            playLabel = ButtonAt(root, "暂停", 470, 604, 93, 42, Hex("343A60"), Color.white, 17, TogglePause).GetComponentInChildren<Text>();
            slowLabel = ButtonAt(root, "1× 速度", 573, 604, 113, 42, Hex("343A60"), Color.white, 15, () => SetSpeed(options != null && options.speed < .9f ? 1 : .25f)).GetComponentInChildren<Text>();
            loopLabel = ButtonAt(root, "循环 关", 696, 604, 103, 42, Hex("343A60"), Color.white, 15, () => { if (options == null) return; options.loop = !options.loop; RefreshControlLabels(); Replay(); }).GetComponentInChildren<Text>();
            compareLabel = ButtonAt(root, "相似动效对比", 810, 604, 192, 42, Cyan, Ink, 16, ToggleComparison).GetComponentInChildren<Text>();
            progressLabel = Label(root, "0%", 1040, 600, 139, 23, 15, Muted, TextAnchor.MiddleRight);
            MakeTimeline();
            Label(root, "拖动右侧时间轴定位；继续播放不会跳回", 362, 650, 630, 19, 11, Muted);

            ImageAt(root, "Notes panel", 340, 692, 872, 164, Panel);
            Label(root, "适合用在", 362, 706, 103, 24, 14, Cyan, TextAnchor.MiddleLeft, true);
            useCase = Label(root, "", 476, 704, 711, 43, 16, Color.white);
            Label(root, "观察差别", 362, 753, 103, 24, 14, Pink, TextAnchor.MiddleLeft, true);
            distinction = Label(root, "", 476, 749, 711, 48, 15, Muted);
            dependency = Label(root, "", 362, 809, 827, 32, 12, Hex("BEC5E6"));

            ImageAt(root, "Parameter panel", 1234, 104, 338, 752, Panel);
            Label(root, "调出你的手感", 1257, 122, 289, 36, 23, Color.white, TextAnchor.MiddleLeft, true);
            Label(root, "参数变化后，从初始状态重新播放", 1257, 162, 290, 32, 12, Muted);
            durationSlider = MakeSlider("时长", "s", 207, .15f, 3f, out durationValue, v => { if (options != null) options.duration = v; });
            strengthSlider = MakeSlider("强度", "×", 281, .2f, 2f, out strengthValue, v => { if (options != null) options.strength = v; });
            distanceSlider = MakeSlider("位移", "px", 355, 0f, 280f, out distanceValue, v => { if (options != null) options.distance = v; });
            staggerSlider = MakeSlider("错开间隔", "s", 429, 0f, .24f, out staggerValue, v => { if (options != null) options.stagger = v; });
            Label(root, "缓动曲线", 1258, 505, 286, 25, 15, Muted);
            easingButton = ButtonAt(root, "Preset · 推荐曲线  ▾", 1257, 539, 292, 43, Hex("343A60"), Color.white, 15, ToggleEasingPopup);
            easingLabel = easingButton.GetComponentInChildren<Text>();
            reverseButton = ButtonAt(root, "方向：进入 / 正向", 1257, 598, 292, 40, Hex("343A60"), Color.white, 14, () => { if (options == null) return; options.reverse = !options.reverse; RefreshControlLabels(); Replay(); });
            reverseLabel = reverseButton.GetComponentInChildren<Text>();
            ButtonAt(root, "恢复推荐参数", 1257, 653, 292, 41, Hex("343A60"), Color.white, 15, ResetParameters);
            ButtonAt(root, "复制 Codex 提示词", 1257, 710, 292, 45, Yellow, Ink, 16, () => CopyPrompt());
            ButtonAt(root, "复制参数 JSON", 1257, 768, 292, 40, Cyan, Ink, 15, () => CopyConfigurationJson());
            Label(root, "含编号、全部参数及对比项目", 1257, 816, 292, 25, 11, Muted, TextAnchor.UpperCenter);
            Label(root, "UGUI + DOTween FREE", 30, 871, 290, 20, 11, Muted);
            status = Label(root, "准备完成", 341, 870, 870, 22, 12, Muted);
            Label(root, "v" + Application.version + "  ·  ESC 退出", 1262, 870, 300, 22, 11, Muted, TextAnchor.MiddleRight);
            MakeEasingPopup();
            RefreshList();
        }

        void MakeSearch()
        {
            var background = ImageAt(root, "Search", 45, 168, 255, 38, Hex("111630"));
            background.raycastTarget = true;
            var field = background.gameObject.AddComponent<InputField>();
            Text input = Label(background.transform, "", 12, 5, 229, 29, 15, Color.white);
            Text placeholder = Label(background.transform, "搜索编号 / 名称 / English", 12, 5, 229, 29, 12, Muted);
            field.textComponent = input;
            field.placeholder = placeholder;
            field.targetGraphic = background;
            field.onValueChanged.AddListener(value => { search = value.Trim(); RefreshList(); });
        }

        void MakeList()
        {
            var viewport = Rect(root, "Catalog viewport", 42, 300, 261, 536);
            var viewportImage = viewport.gameObject.AddComponent<Image>();
            viewportImage.color = Color.clear;
            viewport.gameObject.AddComponent<RectMask2D>();
            listScroll = viewport.gameObject.AddComponent<ScrollRect>();
            listScroll.viewport = viewport;
            listScroll.horizontal = false;
            listScroll.movementType = ScrollRect.MovementType.Clamped;
            listScroll.scrollSensitivity = 36;
            listContent = Rect(viewport, "Catalog entries", 0, 0, 254, 536);
            listScroll.content = listContent;
            var scrollbarBackground = ImageAt(root, "Catalog scrollbar", 307, 306, 4, 526, Hex("313656"));
            scrollbarBackground.raycastTarget = true;
            var scrollbar = scrollbarBackground.gameObject.AddComponent<Scrollbar>();
            var handle = ImageAt(scrollbarBackground.transform, "Handle", 0, 0, 4, 80, Violet);
            handle.raycastTarget = true;
            Stretch(handle.rectTransform);
            scrollbar.handleRect = handle.rectTransform;
            scrollbar.targetGraphic = handle;
            scrollbar.direction = Scrollbar.Direction.BottomToTop;
            listScroll.verticalScrollbar = scrollbar;
            listScroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
        }

        public void SetCategory(string value)
        {
            category = value ?? "";
            foreach (var entry in categories) entry.Value.image.color = entry.Key == category ? Violet : Hex("303657");
            RefreshList();
        }

        void RefreshList()
        {
            if (listContent == null) return;
            foreach (Transform child in listContent) { child.gameObject.SetActive(false); Destroy(child.gameObject); }
            rows.Clear(); visibleIds.Clear();
            float y = 0;
            foreach (MotionDefinition definition in MotionCatalog.All)
            {
                if (category.Length > 0 && definition.category != category) continue;
                string searchable = definition.id + " " + definition.name + " " + definition.english;
                if (search.Length > 0 && searchable.IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0) continue;
                string id = definition.id;
                Color accent = CategoryColor(definition.category);
                Button button = ButtonAt(listContent, "", 0, y, 254, 59, id == CurrentEffectId ? Hex("3C2C68") : Hex("282D4B"), Color.white, 14, () => SelectEffect(id));
                ImageAt(button.transform, "Category accent", 0, 10, 3, 39, accent);
                Label(button.transform, id, 12, 10, 39, 34, 15, accent, TextAnchor.MiddleLeft, true);
                Label(button.transform, definition.name, 55, 7, 190, 27, 16, Color.white, TextAnchor.MiddleLeft, true);
                Label(button.transform, definition.english, 55, 32, 190, 18, 10, Muted);
                rows.Add(id, button.image); visibleIds.Add(id); y += 66;
            }
            listContent.sizeDelta = new Vector2(254, Mathf.Max(y, 536));
            listScroll.verticalNormalizedPosition = 1;
            countLabel.text = visibleIds.Count + " / " + MotionCatalog.All.Length;
            if (visibleIds.Count == 0) Label(listContent, "没有匹配的动效\n试试 A04 或 Pop", 8, 35, 230, 100, 16, Muted, TextAnchor.MiddleCenter);
        }

        public void SelectEffect(string effectId)
        {
            MotionDefinition next = FindDefinition(effectId);
            if (next == null) { Notice("找不到动效：" + effectId); return; }
            current = next; CurrentEffectId = effectId;
            options = MotionSettings.FromDefinition(next);
            options.ease = "Preset";
            comparison = false;
            UpdateDefinitionText();
            SyncSliders();
            BuildDemos(new[] { effectId });
            foreach (var row in rows) row.Value.color = row.Key == effectId ? Hex("3C2C68") : Hex("282D4B");
            RefreshControlLabels();
            Replay();
        }

        void UpdateDefinitionText()
        {
            effectTitle.text = current.id + "  " + current.name;
            effectEnglish.text = current.english;
            effectBadge.text = current.kind;
            useCase.text = current.description + "  适用：" + current.useCase;
            distinction.text = current.difference;
            dependency.text = string.IsNullOrEmpty(current.sourceTitle)
                ? "接入提示：" + current.dependencies
                : "参考：" + current.sourceTitle + "  ·  完整链接与原创转化见图鉴";
        }

        void BuildDemos(string[] ids)
        {
            foreach (var player in players) if (player != null) player.StopAndRestore();
            players.Clear();
            foreach (Transform child in demoHost) { child.gameObject.SetActive(false); Destroy(child.gameObject); }
            float gap = ids.Length > 1 ? 12 : 0;
            float width = (836 - (ids.Length - 1) * gap) / ids.Length;
            for (int i = 0; i < ids.Length; i++)
            {
                Image backdrop = ImageAt(demoHost, "Demo " + ids[i], i * (width + gap), 0, width, 319, Hex("141A34"));
                backdrop.gameObject.AddComponent<RectMask2D>();
                var view = Rect(backdrop.transform, "Fixture area", 0, ids.Length > 1 ? 38 : 0, width, ids.Length > 1 ? 279 : 319);
                var player = DemoFactory.Create(ids[i], view, view.sizeDelta);
                players.Add(player);
                if (ids.Length > 1)
                {
                    MotionDefinition item = MotionCatalog.Find(ids[i]);
                    Label(backdrop.transform, ids[i] + "  " + (item == null ? "" : item.name), 12, 9, width - 24, 27, 14, CategoryColor(ids[i].Substring(0, 1)), TextAnchor.MiddleCenter, true);
                }
                Button click = backdrop.gameObject.AddComponent<Button>();
                click.targetGraphic = backdrop;
                click.transition = Selectable.Transition.None;
                backdrop.raycastTarget = true;
                click.onClick.AddListener(Replay);
            }
        }

        public void ShowComparison(params string[] ids)
        {
            if (!IsReady || current == null) return;
            var valid = new List<string>();
            foreach (string id in ids) if (FindDefinition(id) != null && valid.Count < 3) valid.Add(id);
            if (valid.Count == 0) return;
            comparison = true;
            options.reverse = false;
            options.ease = "Preset";
            BuildDemos(valid.ToArray());
            effectTitle.text = "相似动效 · 同步对比";
            effectEnglish.text = "Preset 最易辨认差别；统一时长，观察空间、缩放和节奏";
            effectBadge.text = "COMPARE " + valid.Count;
            var names = new List<string>();
            foreach (string id in valid) names.Add(id + " " + MotionCatalog.Find(id).name);
            useCase.text = string.Join("  /  ", names);
            if (valid.Contains("A03")) distinction.text = "A03 只缩放至正常尺寸；A04 越过目标尺寸后回缩；A05 在位置上落地弹跳。";
            else if (valid.Contains("B04")) distinction.text = "B04 快速改变位置；B05 左右旋转摆动；B06 以缩放表现一次冲击和衰减。";
            else if (valid.Contains("C05")) distinction.text = "C05 新旧页沿同一轴移动；C06 先淡出再淡入，中间留空；C07 同时淡出和淡入。";
            else if (valid.Contains("D03")) distinction.text = "D03 沿路径飞往资源栏；D04 先爆散再逐个收集；D10 强调到达后的目标变化。";
            else if (valid.Contains("E01")) distinction.text = "E01 上下悬浮；E02 连续旋转；E04 透明度往复。开启循环可观察持续节奏。";
            else distinction.text = "观察每项的运动属性和先后顺序；返回单项预览可查看完整说明。";
            dependency.text = "接入提示：复制按钮将导出全部对比项和当前参数；返回单项预览可查看各自的绑定说明。";
            RefreshControlLabels(); Replay();
        }

        void ToggleComparison()
        {
            if (comparison) { comparison = false; UpdateDefinitionText(); BuildDemos(new[] { CurrentEffectId }); RefreshControlLabels(); Replay(); return; }
            if (CurrentEffectId.StartsWith("B")) ShowComparison("B04", "B05", "B06");
            else if (CurrentEffectId.StartsWith("C")) ShowComparison("C05", "C06", "C07");
            else if (CurrentEffectId.StartsWith("D")) ShowComparison("D03", "D04", "D10");
            else if (CurrentEffectId.StartsWith("E")) ShowComparison("E01", "E02", "E04");
            else if (CurrentEffectId.StartsWith("F")) ShowComparison("F01", "F02", "F06");
            else ShowComparison("A03", "A04", "A05");
        }

        public void Replay()
        {
            if (options == null) return;
            paused = false;
            foreach (MotionPlayer player in players)
                if (player != null) player.Play(player.effectId, CloneOptions());
            RefreshControlLabels();
        }

        MotionSettings CloneOptions()
        {
            return options.Clone();
        }

        public void TogglePause()
        {
            paused = !paused;
            foreach (MotionPlayer player in players) { if (paused) player.Pause(); else player.Resume(); }
            RefreshControlLabels();
        }

        public void SetSpeed(float speed)
        {
            if (options == null) return;
            options.speed = Mathf.Clamp(speed, .1f, 2f);
            RefreshControlLabels(); Replay();
        }

        void ResetParameters()
        {
            if (current == null) return;
            options = MotionSettings.FromDefinition(current); options.ease = "Preset";
            SyncSliders(); RefreshControlLabels(); Replay();
            Notice("已恢复 " + current.id + " 的推荐参数");
        }

        void SyncSliders()
        {
            suppress = true;
            durationSlider.SetValueWithoutNotify(options.duration);
            strengthSlider.SetValueWithoutNotify(options.strength);
            distanceSlider.SetValueWithoutNotify(options.distance);
            staggerSlider.SetValueWithoutNotify(options.stagger);
            suppress = false; RefreshValueLabels();
        }

        void RefreshValueLabels()
        {
            if (options == null) return;
            durationValue.text = options.duration.ToString("0.00", CultureInfo.InvariantCulture) + " s";
            strengthValue.text = options.strength.ToString("0.00", CultureInfo.InvariantCulture) + " ×";
            distanceValue.text = options.distance.ToString("0", CultureInfo.InvariantCulture) + " px";
            staggerValue.text = options.stagger.ToString("0.00", CultureInfo.InvariantCulture) + " s";
        }

        void RefreshControlLabels()
        {
            if (options == null) return;
            playLabel.text = paused ? "继续" : "暂停";
            slowLabel.text = options.speed < .9f ? "¼× 慢放" : "1× 速度";
            slowLabel.color = options.speed < .9f ? Cyan : Color.white;
            loopLabel.text = options.loop ? "循环 开" : "循环 关";
            loopLabel.color = options.loop ? Cyan : Color.white;
            compareLabel.text = comparison ? "返回单项预览" : "相似动效对比";
            reverseLabel.text = options.reverse ? "方向：退出 / 反向" : "方向：进入 / 正向";
            bool reversible = CurrentEffectId == "A01" || CurrentEffectId == "A02" || CurrentEffectId == "A03" || CurrentEffectId == "A07" || CurrentEffectId == "C02" || CurrentEffectId == "C03" || CurrentEffectId == "C04" || CurrentEffectId == "D01" || CurrentEffectId == "D02";
            reverseButton.interactable = reversible && !comparison;
            if (!reversible || comparison) reverseLabel.text = "当前动效使用固定方向";
            int ease = Array.IndexOf(easeValues, options.ease);
            easingLabel.text = (ease < 0 ? options.ease : easeNames[ease]) + "  ▾";
            RefreshParameterAvailability();
        }

        void RefreshParameterAvailability()
        {
            bool strength = UsesAny(StrengthIds);
            bool distance = UsesAny(DistanceIds);
            bool stagger = UsesAny(StaggerIds);
            strengthSlider.interactable = strength;
            distanceSlider.interactable = distance;
            staggerSlider.interactable = stagger;
            RefreshValueLabels();
            if (!strength) strengthValue.text = "不适用";
            if (!distance) distanceValue.text = "不适用";
            if (!stagger) staggerValue.text = "不适用";
            strengthValue.color = strength ? Cyan : Muted;
            distanceValue.color = distance ? Cyan : Muted;
            staggerValue.color = stagger ? Cyan : Muted;
            easingButton.interactable = true;
        }

        bool UsesAny(string ids)
        {
            foreach (MotionPlayer player in players) if (Uses(ids, player.effectId)) return true;
            return false;
        }

        static bool Uses(string ids, string id) { return (" " + ids + " ").Contains(" " + id + " "); }

        public string CopyPrompt()
        {
            if (current == null || options == null) return "";
            var output = new StringBuilder();
            if (comparison)
            {
                output.AppendLine("请分别接入下面这些动效，保留各自的视觉差异：");
                foreach (var player in players)
                {
                    MotionDefinition definition = MotionCatalog.Find(player.effectId);
                    if (definition != null) output.AppendLine(BuildPrompt(definition));
                }
            }
            else output.Append(BuildPrompt(current));
            string prompt = output.ToString();
            GUIUtility.systemCopyBuffer = prompt;
            Notice(comparison ? "已复制所有对比动效与当前参数" : "已复制 " + current.id + " 和当前参数；粘贴给 Codex 即可接入");
            return prompt;
        }

        string BuildPrompt(MotionDefinition definition)
        {
            var prompt = new StringBuilder();
            prompt.AppendFormat(CultureInfo.InvariantCulture, "给我的 Unity UGUI 对象接入 {0} {1}，使用交付的 MotionPlayer 模板。duration={2:0.00}s", definition.id, definition.name, options.duration);
            if (Uses(StrengthIds, definition.id)) prompt.AppendFormat(CultureInfo.InvariantCulture, "，strength={0:0.00}", options.strength);
            if (Uses(DistanceIds, definition.id)) prompt.AppendFormat(CultureInfo.InvariantCulture, "，distance={0:0.0}px", options.distance);
            if (Uses(StaggerIds, definition.id)) prompt.AppendFormat(CultureInfo.InvariantCulture, "，stagger={0:0.00}s", options.stagger);
            prompt.Append("，ease=" + options.ease);
            prompt.AppendFormat(CultureInfo.InvariantCulture, "，reverse={0}，loop={1}，speed={2:0.00}，unscaledTime=true。", options.reverse, options.loop, options.speed);
            if (definition.id == "D01") prompt.AppendFormat(CultureInfo.InvariantCulture, "数值范围 startValue={0:0.##}，endValue={1:0.##}。", options.startValue, options.endValue);
            if (definition.id == "D05") prompt.AppendFormat(CultureInfo.InvariantCulture, "结算总分 endValue={0:0.##}，各项从0累加。", options.endValue);
            if (definition.id == "D02") prompt.AppendFormat(CultureInfo.InvariantCulture, "填充范围 fillFrom={0:0.##}，fillTo={1:0.##}。", options.fillFrom, options.fillTo);
            prompt.Append("绑定 " + definition.binding + "。保留原有交互；重复播放先取消本实例的动画并恢复基线，禁用和销毁时清理；验收连续触发、恢复、布局和分辨率。");
            if (!string.IsNullOrEmpty(definition.sourceUrl)) prompt.Append(" 参考来源：" + definition.sourceTitle + " " + definition.sourceUrl + "；原创转化：" + definition.transformation);
            return prompt.ToString();
        }

        /// <summary>Serializable settings only. No scene references, device data or filesystem paths.</summary>
        [Serializable] public sealed class ConfigurationEntry
        {
            public string effectId;
            public MotionSettings settings;
        }
        [Serializable] public sealed class GalleryConfiguration
        {
            public int schemaVersion = 1;
            public string catalog = "unity-ui-tween";
            public ConfigurationEntry[] effects;
        }

        public string ExportConfigurationJson()
        {
            var entries = new ConfigurationEntry[players.Count];
            for (int i = 0; i < entries.Length; i++)
                entries[i] = new ConfigurationEntry { effectId = players[i].effectId, settings = players[i].settings.Clone() };
            return JsonUtility.ToJson(new GalleryConfiguration { effects = entries }, true);
        }

        public string CopyConfigurationJson()
        {
            string json = ExportConfigurationJson();
            GUIUtility.systemCopyBuffer = json;
            Notice("已复制可复用参数 JSON；可保存为 .json 文件或交给 Codex 接入");
            return json;
        }

        public void SeekPreview(float normalizedTime)
        {
            if (float.IsNaN(normalizedTime) || float.IsInfinity(normalizedTime)) return;
            foreach (var player in players)
            {
                if (player == null) continue;
                try { player.Seek(Mathf.Clamp01(normalizedTime)); }
                catch (InvalidOperationException)
                {
                    // A real press may have taken over the preview. Return to its catalog timeline.
                    player.Play(player.effectId, CloneOptions());
                    player.Seek(Mathf.Clamp01(normalizedTime));
                }
            }
            paused = true;
            SyncTimeline();
            RefreshControlLabels();
        }

        void SyncTimeline()
        {
            if (players.Count == 0 || timelineSlider == null) return;
            float p = Mathf.Clamp01(players[0].Progress);
            if (!Mathf.Approximately(lastProgress, p))
            {
                timelineSlider.SetValueWithoutNotify(p);
                lastProgress = p;
            }
            int percent = Mathf.RoundToInt(p * 100);
            if (percent != lastProgressPercent)
            {
                progressLabel.text = percent + "%";
                lastProgressPercent = percent;
            }
        }

        void MakeTimeline()
        {
            var control = Rect(root, "Preview timeline", 1030, 625, 150, 30);
            var hit = control.gameObject.AddComponent<Image>(); hit.color = Color.clear; hit.raycastTarget = true;
            timelineSlider = control.gameObject.AddComponent<Slider>();
            timelineSlider.minValue = 0; timelineSlider.maxValue = 1;
            ImageAt(control, "Timeline track", 7, 13, 136, 5, Hex("3F466C"));
            var area = Rect(control, "Timeline fill area", 7, 13, 136, 5);
            progressFill = ImageAt(area, "Timeline fill", 0, 0, 136, 5, Cyan, false);
            Stretch(progressFill.rectTransform);
            var handles = Rect(control, "Timeline handle area", 7, 5, 136, 20);
            var handle = ImageAt(handles, "Timeline handle", 0, 0, 16, 20, Yellow);
            handle.raycastTarget = true;
            handle.rectTransform.anchorMin = handle.rectTransform.anchorMax = new Vector2(0, .5f);
            handle.rectTransform.pivot = new Vector2(.5f, .5f);
            handle.rectTransform.anchoredPosition = Vector2.zero;
            handle.rectTransform.sizeDelta = new Vector2(14, 0);
            timelineSlider.fillRect = progressFill.rectTransform;
            timelineSlider.handleRect = handle.rectTransform;
            timelineSlider.targetGraphic = handle;
            timelineSlider.onValueChanged.AddListener(SeekPreview);
        }

        static void SetTextIfChanged(Text text, string value)
        {
            if (text != null && text.text != value) text.text = value;
        }

        static MotionDefinition FindDefinition(string id)
        {
            foreach (MotionDefinition item in MotionCatalog.All) if (item.id == id) return item;
            return null;
        }

        Slider MakeSlider(string title, string units, float y, float min, float max, out Text valueLabel, Action<float> action)
        {
            Label(root, title, 1258, y, 155, 28, 16, Color.white);
            valueLabel = Label(root, "", 1411, y, 137, 28, 15, Cyan, TextAnchor.MiddleRight, true);
            var control = Rect(root, title + " slider", 1258, y + 35, 290, 24);
            var hitArea = control.gameObject.AddComponent<Image>();
            hitArea.color = Color.clear;
            hitArea.raycastTarget = true;
            var slider = control.gameObject.AddComponent<Slider>();
            ImageAt(control, "Track", 0, 10, 290, 5, Hex("3F466C"));
            var fillArea = Rect(control, "Fill area", 0, 10, 290, 5);
            var fill = ImageAt(fillArea, "Fill", 0, 0, 290, 5, Violet);
            Stretch(fill.rectTransform);
            var handleArea = Rect(control, "Handle area", 8, 3, 274, 18);
            var handle = ImageAt(handleArea, "Handle", 0, 3, 18, 18, Cyan);
            handle.raycastTarget = true;
            handle.rectTransform.pivot = new Vector2(.5f, .5f);
            handle.rectTransform.anchorMin = handle.rectTransform.anchorMax = new Vector2(0, .5f);
            handle.rectTransform.anchoredPosition = Vector2.zero;
            handle.rectTransform.sizeDelta = new Vector2(18, 0);
            slider.fillRect = fill.rectTransform; slider.handleRect = handle.rectTransform; slider.targetGraphic = handle;
            slider.minValue = min; slider.maxValue = max;
            slider.onValueChanged.AddListener(v => { if (suppress) return; action(v); RefreshValueLabels(); Replay(); });
            return slider;
        }

        void MakeEasingPopup()
        {
            easingPopup = Rect(root, "Easing dropdown", 1249, 250, 307, 324);
            ImageAt(easingPopup, "Popup panel", 0, 0, 307, 324, Hex("0C1027"));
            for (int i = 0; i < easeValues.Length; i++)
            {
                int index = i;
                ButtonAt(easingPopup, easeNames[i], 7, 7 + i * 39, 293, 35, Hex("303657"), Color.white, 14, () =>
                {
                    if (options == null) return;
                    options.ease = easeValues[index]; easingPopup.gameObject.SetActive(false); RefreshControlLabels(); Replay();
                });
            }
            easingPopup.gameObject.SetActive(false);
        }

        void ToggleEasingPopup() { easingPopup.gameObject.SetActive(!easingPopup.gameObject.activeSelf); easingPopup.SetAsLastSibling(); }
        public void OpenInteractionLab()
        {
            if (interactionLab != null) return;
            foreach (var player in players) if (player != null) player.StopAndRestore();
            interactionLab = InteractionLab.Create(root, () => { interactionLab = null; Replay(); });
        }
        public ButtonStyleLab CurrentButtonLab { get; private set; }
        public void OpenButtonLab()
        {
            if (CurrentButtonLab != null) return;
            foreach (var p in players) if (p != null) p.StopAndRestore();
            CurrentButtonLab = ButtonStyleLab.Create(root, () => { Destroy(CurrentButtonLab.gameObject); CurrentButtonLab = null; Replay(); });
        }
        void Notice(string message) { if (status == null) return; status.text = message; statusUntil = Time.unscaledTime + 4; }

        Button ButtonAt(Transform parent, string title, float x, float y, float w, float h, Color color, Color textColor, int size, Action onClick)
        {
            Image image = ImageAt(parent, title.Length > 0 ? title : "Catalog entry", x, y, w, h, color);
            image.raycastTarget = true;
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white; colors.highlightedColor = new Color(1.12f, 1.12f, 1.12f, 1);
            colors.pressedColor = new Color(.76f, .76f, .86f, 1); colors.selectedColor = Color.white; colors.fadeDuration = .08f;
            button.colors = colors;
            var navigation = new Navigation { mode = Navigation.Mode.None }; button.navigation = navigation;
            if (title.Length > 0) Label(image.transform, title, 5, 0, w - 10, h, size, textColor, TextAnchor.MiddleCenter, true);
            button.onClick.AddListener(() => onClick());
            return button;
        }

        void Chip(Transform parent, string title, float x, float y, float width, Color fill, Color text)
        {
            ImageAt(parent, title, x, y, width, 34, fill);
            Label(parent, title, x, y, width, 34, 14, text, TextAnchor.MiddleCenter, true);
        }

        Text Label(Transform parent, string content, float x, float y, float width, float height, int size, Color color, TextAnchor alignment = TextAnchor.UpperLeft, bool bold = false)
        {
            var rect = Rect(parent, "Text " + content, x, y, width, height);
            var label = rect.gameObject.AddComponent<Text>();
            // The bundled CJK face has generous ascenders; synthesized bold muddies small UI glyphs.
            label.font = font; label.fontSize = size; label.fontStyle = FontStyle.Normal;
            label.text = content; label.color = color; label.alignment = alignment; label.raycastTarget = false;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = alignment == TextAnchor.MiddleLeft || alignment == TextAnchor.MiddleCenter || alignment == TextAnchor.MiddleRight
                ? VerticalWrapMode.Overflow : VerticalWrapMode.Truncate;
            label.supportRichText = false;
            return label;
        }

        Image ImageAt(Transform parent, string name, float x, float y, float width, float height, Color color, bool soft = true)
        {
            var rect = Rect(parent, name, x, y, width, height);
            var image = rect.gameObject.AddComponent<Image>(); image.color = color; image.raycastTarget = false;
            if (soft) { image.sprite = rounded; image.type = Image.Type.Sliced; }
            return image;
        }

        static RectTransform Rect(Transform parent, string name, float x, float y, float width, float height)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false); rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y); rect.sizeDelta = new Vector2(width, height); return rect;
        }

        static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        static Color Hex(string value) { ColorUtility.TryParseHtmlString("#" + value, out Color color); return color; }
        static Color CategoryColor(string group)
        {
            switch (group) { case "A": return Violet; case "B": return Pink; case "C": return Cyan; case "D": return Yellow; case "F": return Hex("FF8A22"); default: return Hex("65EA75"); }
        }

        static Sprite MakeRoundedSprite()
        {
            const int size = 64; const float radius = 16;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.name = "Gallery generated rounded UI"; texture.filterMode = FilterMode.Bilinear;
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
            {
                float cx = Mathf.Clamp(x + .5f, radius, size - radius), cy = Mathf.Clamp(y + .5f, radius, size - radius);
                float distance = Vector2.Distance(new Vector2(x + .5f, y + .5f), new Vector2(cx, cy));
                pixels[y * size + x] = new Color(1, 1, 1, Mathf.Clamp01(radius - distance));
            }
            texture.SetPixels(pixels); texture.Apply();
            return Sprite.Create(texture, new UnityEngine.Rect(0, 0, size, size), new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect, new Vector4(18, 18, 18, 18));
        }

        void OnDestroy()
        {
            foreach (MotionPlayer player in players) if (player != null) player.StopAndRestore();
            if (rounded != null) { Destroy(rounded.texture); Destroy(rounded); }
        }
    }
}
