using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TweenGallery
{
    /// <summary>Opt-in player checks. Pass a failure sink; emits nothing on success.</summary>
    public static class UpgradeOrchestrationTest
    {
        public static IEnumerator Run(Action<string> failure)
        {
            if (failure == null) throw new ArgumentNullException(nameof(failure));
            var root = new GameObject("Orchestration runtime test", typeof(RectTransform));
            var other = new GameObject("Unrelated audio");
            var clip = AudioClip.Create("Silence for ownership test", 441000, 1, 44100, false);
            var ownSource = root.AddComponent<AudioSource>(); var otherSource = other.AddComponent<AudioSource>();
            ownSource.clip = clip; otherSource.clip = clip; otherSource.Play();
            var lease = root.AddComponent<MotionPlaybackLease>();
            int writes = 0, returns = 0;
            lease.Borrow(); var first = lease.BeginPlayback();
            lease.RunAfter(first, .08f, () => writes++);
            var second = lease.BeginPlayback();
            if (lease.TryReturn(first, () => returns++)) failure("Old playback returned a live pooled object");
            yield return new WaitForSecondsRealtime(.15f);
            if (writes != 0) failure("Delayed callback from old playback ran after restart");
            lease.Borrow(); var third = lease.BeginPlayback();
            if (lease.IsCurrent(second) || lease.TryReturn(second, () => returns++)) failure("Old pool lease remained valid after reborrow");
            if (!lease.TryReturn(third, () => returns++) || lease.IsCurrent(third) || returns != 1) failure("Current pool lease did not return exactly once");

            var pageObject = new GameObject("Independent page owner", typeof(RectTransform));
            var page = pageObject.AddComponent<MotionPlayer>();
            page.bindings = new MotionBindings { target = (RectTransform)pageObject.transform };
            page.CaptureBaseline(); lease.Borrow(); var pageOld = lease.BeginPlayback(page);
            lease.RunAfter(pageOld, .08f, () => writes++);
            page.StopAndRestore(); page.CaptureBaseline();
            var pageNew = lease.BeginPlayback(page);
            yield return new WaitForSecondsRealtime(.15f);
            if (writes != 0 || lease.IsCurrent(pageOld) || lease.TryReturn(pageOld, () => returns++)) failure("Old page wrote or returned a reward after page reopen");
            if (!lease.IsCurrent(pageNew)) failure("Current re-opened page and borrowed reward should remain valid");
            lease.Borrow(); var freshLoan = lease.BeginPlayback(page);
            if (lease.IsCurrent(pageNew) || !lease.IsCurrent(freshLoan)) failure("Independent object reborrow must invalidate its token without changing page version");

            var sound = root.AddComponent<MotionSoundTimeline>(); sound.ownedSource = ownSource;
            sound.cues = new[] { new MotionSoundCue { normalizedTime = .75f }, new MotionSoundCue { normalizedTime = .25f }, new MotionSoundCue { normalizedTime = 0f } };
            var calls = new List<string>(); sound.CueCrossed += (i, cycle, cue) => calls.Add(i + ":" + cycle);
            sound.Begin(1, true); sound.Advance(2.3);
            if (string.Join(",", calls) != "2:0,1:0,0:0,2:1,1:1,0:1,2:2,1:2") failure("Long frame did not cross all nodes/loops in order");
            sound.Advance(2.8, paused: true);
            if (Math.Abs(sound.Cursor - 2.3) > .00001) failure("Paused audio timeline advanced");
            sound.Advance(.2, reverseOrSeek: true); sound.Advance(1.8);
            if (calls.Count != 8) failure("Reverse or rewind re-fired a previous cue");
            sound.Skip(); sound.Advance(4);
            if (calls.Count != 8 || sound.IsRunning) failure("Skip emitted later audio cues");
            if (!otherSource.isPlaying) failure("Stopping owned sound affected unrelated audio source");

            var group = root.AddComponent<CanvasGroup>(); var player = root.AddComponent<MotionPlayer>();
            player.bindings = new MotionBindings { target = (RectTransform)root.transform, group = group };
            player.CaptureBaseline();
            var gate = root.AddComponent<MotionPlaybackGate>(); gate.player = player;
            gate.SetContext(true, true, true);
            if (gate.Play("A01", MotionPurpose.EssentialTransition) != MotionGateDecision.CompleteImmediately || Mathf.Abs(group.alpha - 1) > .001f) failure("Reduced essential transition did not reach final state");
            if (gate.Play("B03", MotionPurpose.DecorativeLoop) != MotionGateDecision.Restore || player.IsPlaying) failure("Reduced decorative loop continued playing");
            gate.SetContext(true, true, false); int cancelled = 0; gate.InteractionCancelled += () => cancelled++;
            gate.Play("B01", MotionPurpose.InteractionFeedback); gate.SetHostBlockers(true, false);
            if (cancelled != 1 || player.IsPlaying) failure("Modal blocking failed to cancel interaction and restore");
            var endObject = new GameObject("Required hidden terminal state", typeof(RectTransform), typeof(CanvasGroup));
            var endGroup = endObject.GetComponent<CanvasGroup>(); var endPlayer = endObject.AddComponent<MotionPlayer>();
            endPlayer.bindings = new MotionBindings { target = (RectTransform)endObject.transform, group = endGroup };
            endPlayer.CaptureBaseline(); var endGate = endObject.AddComponent<MotionPlaybackGate>(); endGate.player = endPlayer;
            endGate.SetContext(true, true, true);
            var closing = MotionSettings.FromDefinition(MotionCatalog.Find("A01")); closing.reverse = true;
            endGate.Play("A01", MotionPurpose.EssentialTransition, closing);
            if (endGroup.alpha > .001f || endGroup.blocksRaycasts) failure("Reduced closing transition did not reach non-interactive hidden state");
            endObject.SetActive(false); endObject.SetActive(true);
            if (endGroup.alpha > .001f || endGroup.blocksRaycasts) failure("Required terminal state was undone by disable/re-enable");
            endGate.SetContext(true, true, false);
            endGate.Play("A01", MotionPurpose.EssentialTransition, closing);
            endObject.SetActive(false); endObject.SetActive(true);
            if (endGroup.alpha > .001f || endGroup.blocksRaycasts) failure("Disabling during required transition failed to commit terminal state");
            UnityEngine.Object.Destroy(root); UnityEngine.Object.Destroy(other); UnityEngine.Object.Destroy(pageObject); UnityEngine.Object.Destroy(clip);
            UnityEngine.Object.Destroy(endObject);
            yield return null;
        }
    }
}
