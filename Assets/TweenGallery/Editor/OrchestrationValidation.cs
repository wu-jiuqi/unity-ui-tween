#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace TweenGallery.Editor
{
    /// <summary>Independent smoke assertions for the orchestration components, runnable with -executeMethod.</summary>
    public static class OrchestrationValidation
    {
        static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
        public static void Run()
        {
            var go = new GameObject("Orchestration validation", typeof(RectTransform));
            try
            {
                var lease = go.AddComponent<MotionPlaybackLease>();
                lease.Borrow(); var first = lease.BeginPlayback();
                int writes = 0, returns = 0;
                Check(lease.TryRun(first, () => writes++), "Current lease should write");
                var newer = lease.BeginPlayback();
                Check(!lease.TryRun(first, () => writes++), "Restart must invalidate old playback");
                Check(!lease.TryReturn(first, () => returns++), "Old playback must not return the current borrow");
                lease.Borrow(); var nextBorrow = lease.BeginPlayback();
                Check(!lease.IsCurrent(newer), "A new borrow must invalidate prior borrow, regardless of playback count");
                Check(lease.TryReturn(nextBorrow, () => returns++), "Current borrow can be returned");
                Check(!lease.IsCurrent(nextBorrow) && writes == 1 && returns == 1, "Return invalidates token before reuse");

                var timeline = go.AddComponent<MotionSoundTimeline>();
                timeline.cues = new[] { new MotionSoundCue { normalizedTime = .75f }, new MotionSoundCue { normalizedTime = .25f }, new MotionSoundCue { normalizedTime = 0f } };
                var events = new List<string>(); timeline.CueCrossed += (i, cycle, cue) => events.Add(i + ":" + cycle);
                timeline.Begin(1, true); timeline.Advance(1.3);
                Check(string.Join(",", events) == "2:0,1:0,0:0,2:1,1:1", "Long frame must emit each cue once in time order across loop boundary");
                timeline.Advance(1.8, paused: true);
                Check(Math.Abs(timeline.Cursor - 1.3) < .00001, "Pause must not advance cursor");
                timeline.Advance(.2, reverseOrSeek: true); timeline.Advance(.8);
                Check(events.Count == 5, "Reverse and replay of fired loop are silent");
                timeline.Advance(1.8); Check(events.Count == 6 && events[5] == "0:1", "Unfired cue after reverse should still fire once");
                timeline.Skip(); timeline.Advance(3);
                Check(events.Count == 6 && !timeline.IsRunning, "Skip cancels all later cues");

                var player = go.AddComponent<MotionPlayer>();
                var group = go.AddComponent<CanvasGroup>();
                player.bindings = new MotionBindings { target = (RectTransform)go.transform, group = group };
                player.CaptureBaseline();
                var gate = go.AddComponent<MotionPlaybackGate>(); gate.player = player;
                gate.SetContext(true, true, true);
                Check(gate.Play("A01", MotionPurpose.EssentialTransition) == MotionGateDecision.CompleteImmediately && Mathf.Abs(group.alpha - 1) < .001f, "Reduced essential transition must finish visually");
                Check(gate.Play("B03", MotionPurpose.DecorativeLoop) == MotionGateDecision.Restore && !player.IsPlaying, "Reduced decorative loop should stop");
                Check(gate.Play("B01", MotionPurpose.InteractionFeedback) == MotionGateDecision.Restore, "Reduced interaction feedback should not animate");
                Directory.CreateDirectory("evidence");
                File.WriteAllText("evidence/orchestration-validation.txt", "PASS: playback and pool lease epochs; sound crossings, loops, pause, seek and skip; reduced-motion decisions.\n");
                Debug.Log("ORCHESTRATION_VALIDATION_PASS");
            }
            finally { UnityEngine.Object.DestroyImmediate(go); }
        }
    }
}
#endif
