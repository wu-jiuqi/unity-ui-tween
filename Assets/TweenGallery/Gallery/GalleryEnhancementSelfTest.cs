using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace TweenGallery
{
    /// <summary>Opt-in real Player check for the local gallery candidate, not a hardware touch test.</summary>
    public sealed class GalleryEnhancementSelfTest : MonoBehaviour
    {
        [Serializable] sealed class Report
        {
            public string version, unity;
            public bool passed;
            public List<string> checks = new List<string>(), errors = new List<string>();
        }
        readonly Report report = new Report();
        string folder;
        bool running;
        float deadline;
        void Check(bool ok, string message) { if (ok) report.checks.Add(message); else report.errors.Add(message); }
        void Log(string message, string stack, LogType type)
        { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) report.errors.Add(message); }
        void Update() { if (running && Time.realtimeSinceStartup > deadline) { report.errors.Add("Gallery enhancement test timeout"); Finish(); } }
        IEnumerator Start()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (!args.Contains("--gallery-enhancement-test")) yield break;
            int at = Array.IndexOf(args, "--test-output");
            folder = at >= 0 && at + 1 < args.Length ? args[at + 1] : Path.Combine(Application.persistentDataPath, "gallery-enhancement-test");
            Directory.CreateDirectory(folder);
            report.version = Application.version; report.unity = Application.unityVersion;
            running = true; deadline = Time.realtimeSinceStartup + 90;
            Application.logMessageReceived += Log;
            yield return null; yield return null;
            var gallery = FindFirstObjectByType<GalleryBootstrap>();
            Check(gallery != null && gallery.IsReady, "Gallery is ready");
            if (gallery == null) { Finish(); yield break; }
            foreach (string name in new[] { "reward-chest", "energy-crystal", "adventure-world" })
            {
                var art = Resources.Load<Sprite>("MotionArt/" + name);
                Check(art != null && art.texture.width >= 512 && art.texture.height >= 512, "Imported generated sprite: " + name);
            }
            gallery.SelectEffect("D02"); gallery.SetSpeed(.25f);
            yield return null;
            var timeline = gallery.GetComponentsInChildren<Slider>(true).FirstOrDefault(s => s.name == "Preview timeline");
            Check(timeline != null, "Interactive timeline Slider exists");
            if (timeline != null) timeline.value = .5f; else gallery.SeekPreview(.5f);
            var player = gallery.CurrentPlayer;
            Check(Mathf.Abs(player.Progress - .5f) < .001f && !player.IsPlaying, "Slider seeks and pauses at 50 percent");
            // Preset D02 easing is not necessarily linear. Display must agree with actual fill.
            Check(player.bindings.label.text == Mathf.RoundToInt(player.bindings.fill.fillAmount * 100) + "%", "D02 label matches the actual filled percentage after seeking");
            gallery.TogglePause();
            yield return new WaitForSecondsRealtime(.10f);
            Check(player.Progress > .5f && player.Progress < .8f, "Continue resumes from the scrubbed clock without jumping back");
            gallery.SeekPreview(1f); gallery.SeekPreview(.2f);
            Check(Mathf.Abs(player.Progress - .2f) < .001f && !player.IsPlaying, "Seek supports backward movement after the end");
            string json = gallery.ExportConfigurationJson();
            var config = JsonUtility.FromJson<GalleryBootstrap.GalleryConfiguration>(json);
            Check(config.schemaVersion == 1 && config.effects.Length == 1 && config.effects[0].effectId == "D02", "Single-effect JSON carries schema and ID");
            Check(Mathf.Abs(config.effects[0].settings.speed - .25f) < .001f && config.effects[0].settings.unscaledTime, "JSON preserves exact current playback settings");
            File.WriteAllText(Path.Combine(folder, "D02-configuration.json"), json);
            gallery.ShowComparison("A03", "A04", "A05");
            config = JsonUtility.FromJson<GalleryBootstrap.GalleryConfiguration>(gallery.ExportConfigurationJson());
            Check(config.effects.Select(e => e.effectId).SequenceEqual(new[] { "A03", "A04", "A05" }), "Comparison JSON keeps all three distinct IDs");
            gallery.SeekPreview(.65f);
            Check(gallery.ActivePlayers.All(p => Mathf.Abs(p.Progress - .65f) < .001f && !p.IsPlaying), "Comparison timeline scrubs all three players together");
            foreach (string pressId in new[] { "B01", "B02" })
            {
                gallery.SelectEffect(pressId);
                gallery.CurrentPlayer.SetPressed(true);
                gallery.SeekPreview(.4f);
                Check(Mathf.Abs(gallery.CurrentPlayer.Progress - .4f) < .001f && !gallery.CurrentPlayer.IsPlaying, pressId + " press takeover can return to catalog timeline");
                gallery.TogglePause();
                Check(gallery.CurrentPlayer.IsPlaying, pressId + " resumes after press-to-timeline fallback");
            }
            foreach (string id in new[] { "A04", "A06", "A08", "B05", "D02", "E01", "F01", "F06" })
            {
                gallery.SelectEffect(id); gallery.SeekPreview(id == "D02" ? .5f : 1f);
                yield return null;
                yield return new WaitForEndOfFrame();
                CaptureGallery(gallery, Path.Combine(folder, id + "-generated-art.png"));
                yield return null;
            }
            Finish();
        }
        // Hidden windows can return an all-black swapchain capture. Render the same live
        // Canvas to a dedicated camera target, then restore its normal overlay mode.
        void CaptureGallery(GalleryBootstrap gallery, string path)
        {
            var canvas = gallery.GetComponentInChildren<Canvas>();
            var previousMode = canvas.renderMode;
            var previousCamera = canvas.worldCamera;
            float previousDistance = canvas.planeDistance;
            var cameraObject = new GameObject("Gallery QA offscreen camera");
            var camera = cameraObject.AddComponent<Camera>();
            camera.enabled = false; camera.orthographic = true; camera.orthographicSize = Screen.height * .5f;
            camera.transform.position = new Vector3(0, 0, -10);
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.067f,.078f,.169f);
            camera.nearClipPlane = .1f; camera.farClipPlane = 100;
            var target = new RenderTexture(Screen.width, Screen.height, 24);
            var previousActive = RenderTexture.active;
            var image = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = target;
                canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1;
                Canvas.ForceUpdateCanvases(); camera.Render();
                RenderTexture.active = target;
                image.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0); image.Apply();
                var pixels = image.GetPixels32(); int varied = 0;
                for (int i=0;i<pixels.Length;i+=97) if (pixels[i].r > 80 || pixels[i].g > 80 || pixels[i].b > 80) varied++;
                Check(varied > 200, "Offscreen gallery render has visible UI: " + Path.GetFileName(path));
                File.WriteAllBytes(path, image.EncodeToPNG());
            }
            finally
            {
                canvas.renderMode = previousMode; canvas.worldCamera = previousCamera; canvas.planeDistance = previousDistance;
                Canvas.ForceUpdateCanvases(); camera.targetTexture = null; RenderTexture.active = previousActive;
                target.Release(); Destroy(target); Destroy(image); Destroy(cameraObject);
            }
        }

        void Finish()
        {
            if (!running) return;
            running = false; Application.logMessageReceived -= Log;
            report.passed = report.errors.Count == 0;
            File.WriteAllText(Path.Combine(folder, "gallery-enhancement-report.json"), JsonUtility.ToJson(report, true));
            Debug.Log("GALLERY_ENHANCEMENT_TEST " + (report.passed ? "PASS" : "FAIL"));
            Application.Quit(report.passed ? 0 : 1);
        }
    }
}
