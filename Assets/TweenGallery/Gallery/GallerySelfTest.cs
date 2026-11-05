using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace TweenGallery
{
    // This opt-in player test also runs from a separately extracted portable build.
    // It does not claim to substitute for human review or a separate physical PC.
    public sealed class GallerySelfTest : MonoBehaviour
    {
        [Serializable] public class CaseResult { public string id; public bool changed, restored, replay, disabled, finite, played; public string detail; }
        [Serializable] public class Report { public string unity, version, platform, utc, output; public int width, height; public bool passed; public List<CaseResult> cases = new List<CaseResult>(); public List<string> checks = new List<string>(); public List<string> errors = new List<string>(); }
        readonly Report report = new Report();
        string folder;
        bool enabledTest;
        float deadline;

        void Update()
        {
            if (enabledTest && Time.realtimeSinceStartup > deadline)
            {
                report.errors.Add("Self-test watchdog: did not complete in 120 seconds");
                Finish();
            }
        }

        IEnumerator Start()
        {
            var args = Environment.GetCommandLineArgs();
            enabledTest = args.Contains("--self-test");
            if (!enabledTest) yield break;
            deadline = Time.realtimeSinceStartup + 120f;
            int at = Array.IndexOf(args, "--test-output");
            folder = at >= 0 && at + 1 < args.Length ? args[at + 1] : Path.Combine(Application.persistentDataPath, "validation");
            Directory.CreateDirectory(folder);
            report.unity = Application.unityVersion; report.version = Application.version; report.platform = Application.platform.ToString();
            report.utc = DateTime.UtcNow.ToString("o"); report.output = folder; report.width = Screen.width; report.height = Screen.height;
            Application.logMessageReceived += Log;
            yield return null; yield return null;
            var gallery = FindFirstObjectByType<GalleryBootstrap>();
            if (gallery == null) { report.errors.Add("GalleryBootstrap missing"); Finish(); yield break; }
            while (!gallery.IsReady) yield return null;
            float sentinelValue = 0;
            Tween sentinel = DOTween.To(() => sentinelValue, v => sentinelValue = v, 1f, 999f).SetUpdate(true);
            foreach (var def in MotionCatalog.All)
            {
                gallery.SelectEffect(def.id);
                yield return null;
                var player = gallery.CurrentPlayer;
                var item = new CaseResult { id = def.id };
                report.cases.Add(item);
                try
                {
                    if (player == null) throw new Exception("No preview player");
                    player.StopAndRestore();
                    string baseline = Fingerprint(player);
                    player.Play(def.id, MotionSettings.FromDefinition(def));
                    player.Sample(0); string start = Fingerprint(player);
                    player.Sample(.29f); string mid1 = Fingerprint(player);
                    player.Sample(.68f); string mid2 = Fingerprint(player);
                    player.Sample(1); string end = Fingerprint(player);
                    item.changed = start != mid1 || mid1 != mid2 || mid2 != end;
                    item.finite = AllFinite(player);
                    player.StopAndRestore(); item.restored = baseline == Fingerprint(player);
                    for (int repeat = 0; repeat < 8; repeat++) { player.Play(def.id); player.Sample(repeat / 8f); }
                    player.StopAndRestore(); item.replay = baseline == Fingerprint(player);
                    player.Play(def.id); player.Sample(.45f); player.enabled = false;
                    item.disabled = baseline == Fingerprint(player); player.enabled = true;
                    player.Play(def.id); player.Sample(.35f);
                }
                catch (Exception e) { item.detail = e.ToString(); report.errors.Add(def.id + ": " + e.Message); }
                yield return new WaitForEndOfFrame();
                ScreenCapture.CaptureScreenshot(Path.Combine(folder, def.id + ".png"));
                yield return null;
                if (player != null && new[] { "A08", "C04", "D03", "D04", "D07", "D08", "E07" }.Contains(def.id))
                {
                    player.Sample(def.id == "C04" || def.id == "A08" ? 1f : .85f);
                    yield return new WaitForEndOfFrame(); ScreenCapture.CaptureScreenshot(Path.Combine(folder, def.id + "-late.png"));
                    yield return null;
                }
                if (player != null)
                {
                    var settings = MotionSettings.FromDefinition(def); settings.duration = .1f; settings.speed = 1; settings.loop = false;
                    bool started = false;
                    try { player.Play(def.id, settings); started = true; }
                    catch (Exception e) { report.errors.Add(def.id + " real playback: " + e.Message); }
                    yield return new WaitForSecondsRealtime(.25f);
                    item.played = started && !player.IsPlaying && player.Progress >= .99f;
                    player.StopAndRestore();
                }
            }
            Check(sentinel.IsActive(), "Stopping gallery effects preserves an unrelated DOTween tween"); sentinel.Kill();
            gallery.SelectEffect("A04");
            yield return null;
            var current = gallery.CurrentPlayer;
            var competitor = current.gameObject.AddComponent<MotionPlayer>();
            competitor.bindings = current.bindings;
            current.Play("A04"); competitor.Play("A04");
            Check(!current.IsPlaying && competitor.IsPlaying, "A second player takes ownership of the same target");
            current.SetPressed(true);
            Check(!competitor.IsPlaying && current.IsPlaying, "Pointer press also takes ownership of the same target");
            current.StopAndRestore(); Destroy(competitor);
            var timing = MotionSettings.FromDefinition(MotionCatalog.Find("A04")); timing.duration = 1; timing.loop = false;
            current.Play("A04", timing);
            yield return new WaitForSecondsRealtime(.12f);
            current.Pause(); float paused = current.Progress;
            yield return new WaitForSecondsRealtime(.12f);
            Check(Mathf.Abs(current.Progress - paused) < .003f, "Pause freezes normalized progress");
            current.Resume(); yield return new WaitForSecondsRealtime(.12f);
            Check(current.Progress > paused + .03f, "Resume advances normalized progress");
            current.StopAndRestore(); timing.speed = .25f; current.Play("A04", timing);
            yield return new WaitForSecondsRealtime(.2f);
            Check(current.Progress > .015f && current.Progress < .13f, "Quarter-speed playback advances about 0.05 in 0.2 seconds");
            current.StopAndRestore(); timing.speed = 1; timing.unscaledTime = true; Time.timeScale = 0; current.Play("A04", timing);
            yield return new WaitForSecondsRealtime(.15f);
            Check(current.Progress > .05f, "Unscaled UI animation advances while game time is paused"); Time.timeScale = 1;
            current.StopAndRestore();
            var businessText = current.GetComponentInChildren<Text>();
            if (businessText != null)
            {
                string original = businessText.text;
                current.Play("A04", timing); current.Sample(.2f); businessText.text = "BUSINESS STATE 42"; current.Sample(.7f); current.StopAndRestore();
                Check(businessText.text == "BUSINESS STATE 42", "A04 preserves business text updated during animation");
                businessText.text = original;
            }
            if (current.bindings.group != null)
            {
                current.Play("A04", timing); current.Sample(.4f); current.bindings.group.interactable = false; current.Sample(.8f); current.StopAndRestore();
                Check(!current.bindings.group.interactable, "A04 preserves external interactable=false state");
                current.bindings.group.interactable = true;
            }
            var layoutTarget = current.bindings.target;
            Vector2 oldPosition = layoutTarget.anchoredPosition;
            layoutTarget.anchoredPosition += new Vector2(15, 12); current.CaptureBaseline();
            current.Play("A04", timing); current.Sample(.5f); current.StopAndRestore();
            Check(Vector2.Distance(layoutTarget.anchoredPosition, oldPosition + new Vector2(15, 12)) < .01f, "CaptureBaseline accepts a new layout position");
            layoutTarget.anchoredPosition = oldPosition; current.CaptureBaseline();
            if (current.bindings.group != null)
            {
                current.bindings.group.alpha = .5f; current.CaptureBaseline(); current.Play("A04", timing); current.Sample(1);
                Check(Mathf.Abs(current.bindings.group.alpha - .5f) < .01f, "Fade respects a captured half-opacity baseline");
                current.StopAndRestore(); current.bindings.group.alpha = 1; current.CaptureBaseline();
            }
            // A separate fixture validates reuse without any gallery control or state.
            var host = new GameObject("Isolated integration test", typeof(RectTransform), typeof(Canvas));
            foreach (string id in new[] { "A04", "B02", "D03", "E03" })
            {
                var separate = DemoFactory.Create(id, host.transform, new Vector2(600, 420));
                separate.Play(id); separate.Sample(.4f); separate.StopAndRestore();
                if (id == "D03")
                {
                    separate.Play(id); separate.Sample(1);
                    Check(Vector3.Distance(separate.bindings.target.TransformPoint(separate.bindings.target.rect.center), separate.bindings.destination.TransformPoint(separate.bindings.destination.rect.center)) < .1f, "D03 reward reaches destination center");
                    separate.StopAndRestore();
                }
                if (id == "B02")
                {
                    Vector3 normal = separate.bindings.target.localScale;
                    var pointer = new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left };
                    ExecuteEvents.Execute(separate.bindings.target.gameObject, pointer, ExecuteEvents.pointerDownHandler);
                    yield return new WaitForSecondsRealtime(.18f);
                    Check(separate.bindings.target.localScale.x < normal.x, "B02 pointer down visibly compresses button");
                    ExecuteEvents.Execute(separate.bindings.target.gameObject, pointer, ExecuteEvents.pointerUpHandler);
                    yield return new WaitForSecondsRealtime(.8f);
                    Check(Vector3.Distance(separate.bindings.target.localScale, normal) < .01f, "B02 pointer up restores size after rebound");
                }
                separate.Play(id); Destroy(separate.gameObject); yield return null;
            }
            var shared = DemoFactory.Create("C04", host.transform, new Vector2(600, 420));
            shared.bindings.target.pivot = new Vector2(.2f, .8f); shared.bindings.target.localScale = new Vector3(.7f, 1.2f, 1); shared.CaptureBaseline();
            shared.Play("C04"); shared.Sample(1);
            var sourceBounds = WorldBounds(shared.bindings.target); var destBounds = WorldBounds(shared.bindings.destination);
            Check(Vector3.Distance(sourceBounds.center, destBounds.center) < .1f && Vector3.Distance(sourceBounds.size, destBounds.size) < .1f, "C04 matches destination with non-centered pivot and non-unit scale");
            Destroy(shared.gameObject);
            var rotating = DemoFactory.Create("E02", host.transform, new Vector2(600, 420));
            rotating.settings.strength = 1.3f; rotating.Play("E02"); rotating.Sample(0); Quaternion first = rotating.bindings.target.localRotation; rotating.Sample(1);
            Check(Quaternion.Angle(first, rotating.bindings.target.localRotation) < .01f, "E02 rotation loop has a continuous seam at strength 1.3");
            Destroy(rotating.gameObject);
            Destroy(host); yield return null;
            report.checks.Add("Four representative templates instantiated and destroyed outside gallery controls");
            foreach (var ids in new[] { new[] { "A03", "A04", "A05" }, new[] { "B04", "B05", "B06" }, new[] { "C05", "C06", "C07" } })
            {
                gallery.ShowComparison(ids); yield return null;
                Check(gallery.ActivePlayers.Length == 3, "Three independent compare players: " + string.Join("/", ids));
                foreach (var p in gallery.ActivePlayers) p.Sample(.45f);
                yield return new WaitForEndOfFrame(); ScreenCapture.CaptureScreenshot(Path.Combine(folder, "compare-" + ids[0] + ".png"));
                yield return null;
            }
            gallery.SetCategory("D"); yield return null;
            Check(gallery.GetComponentsInChildren<Text>().Any(t => t.text == "10 / 40"), "Category D exposes ten catalog entries");
            gallery.SetCategory("");
            var search = gallery.GetComponentInChildren<InputField>();
            search.text = "A04"; yield return null;
            Check(gallery.GetComponentsInChildren<Text>().Any(t => t.text == "1 / 40"), "Search A04 isolates one catalog entry");
            search.text = "no-such-motion-xyz"; yield return null;
            Check(gallery.GetComponentsInChildren<Text>().Any(t => t.text == "0 / 40"), "Search handles zero results");
            search.text = ""; yield return null;
            gallery.SelectEffect("A04"); yield return null;
            var replay = gallery.GetComponentsInChildren<Button>().FirstOrDefault(b => b.GetComponentsInChildren<Text>().Any(t => t.text == "重播"));
            gallery.CurrentPlayer.Sample(.8f);
            if (replay != null) ExecuteEvents.Execute(replay.gameObject, new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left }, ExecuteEvents.pointerClickHandler);
            Check(replay != null && gallery.CurrentPlayer.IsPlaying && gallery.CurrentPlayer.Progress < .1f, "Replay button receives EventSystem pointer click");
            string prompt = gallery.CopyPrompt();
            Check(!string.IsNullOrEmpty(prompt) && prompt.Contains("A04") && prompt.Contains("duration"), "Copied prompt contains effect and reproduction parameters");
            gallery.CurrentPlayer.Sample(1);
            yield return new WaitForEndOfFrame(); ScreenCapture.CaptureScreenshot(Path.Combine(folder, "gallery-overview.png"));
            yield return null;
            report.passed = report.cases.Count == 40 && report.cases.All(c => c.changed && c.restored && c.replay && c.disabled && c.finite && c.played) && report.errors.Count == 0;
            Finish();
        }

        void Check(bool ok, string label) { if (ok) report.checks.Add(label); else report.errors.Add(label); }
        void Log(string message, string stack, LogType type) { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) report.errors.Add(message + "\n" + stack); }
        void Finish()
        {
            enabledTest = false;
            Application.logMessageReceived -= Log;
            File.WriteAllText(Path.Combine(folder, "report.json"), JsonUtility.ToJson(report, true));
            Debug.Log("GALLERY_SELF_TEST " + (report.passed ? "PASS" : "FAIL") + " " + report.cases.Count);
            Application.Quit(report.passed ? 0 : 1);
        }
        static bool AllFinite(MotionPlayer player)
        {
            foreach (var r in player.GetComponentsInChildren<RectTransform>(true))
            {
                var p = r.anchoredPosition; var s = r.localScale;
                foreach (float n in new[] { p.x, p.y, s.x, s.y, s.z, r.localEulerAngles.x, r.localEulerAngles.y, r.localEulerAngles.z, r.sizeDelta.x, r.sizeDelta.y })
                    if (float.IsNaN(n) || float.IsInfinity(n)) return false;
            }
            foreach (var g in player.GetComponentsInChildren<CanvasGroup>(true)) if (float.IsNaN(g.alpha) || float.IsInfinity(g.alpha)) return false;
            foreach (var i in player.GetComponentsInChildren<Image>(true)) if (float.IsNaN(i.fillAmount) || float.IsInfinity(i.fillAmount)) return false;
            return true;
        }
        static Bounds WorldBounds(RectTransform rect)
        {
            var corners = new Vector3[4]; rect.GetWorldCorners(corners); var bounds = new Bounds(corners[0], Vector3.zero);
            foreach (var corner in corners) bounds.Encapsulate(corner); return bounds;
        }
        public static string Fingerprint(MotionPlayer player)
        {
            var s = new StringBuilder();
            void V(float f) { s.Append(Mathf.Round(f * 1000).ToString(CultureInfo.InvariantCulture)).Append(','); }
            // Bindings may point at siblings, so snapshot all fixture descendants.
            Transform root = player.transform;
            foreach (var r in root.GetComponentsInChildren<RectTransform>(true)) { V(r.anchoredPosition.x); V(r.anchoredPosition.y); V(r.localScale.x); V(r.localScale.y); V(r.localEulerAngles.z); V(r.localEulerAngles.y); V(r.sizeDelta.x); V(r.sizeDelta.y); }
            foreach (var g in root.GetComponentsInChildren<CanvasGroup>(true)) { V(g.alpha); s.Append(g.blocksRaycasts).Append(g.interactable); }
            foreach (var g in root.GetComponentsInChildren<Graphic>(true)) { V(g.color.r); V(g.color.g); V(g.color.b); V(g.color.a); if (g.material != null && g.material.HasProperty("_Blur")) V(g.material.GetFloat("_Blur")); }
            foreach (var t in root.GetComponentsInChildren<Text>(true)) s.Append(t.text);
            foreach (var i in root.GetComponentsInChildren<Image>(true)) V(i.fillAmount);
            return s.ToString();
        }
    }
}
