using System;
using UnityEngine;

namespace TweenGallery
{
    /// <summary>Sound events are intentionally independent of MotionPlayer.Sample.</summary>
    [Serializable]
    public sealed class MotionSoundCue
    {
        [Range(0f, 1f)] public float normalizedTime;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
    }

    /// <summary>
    /// A forward timeline using unwrapped elapsed time (seconds). The caller owns
    /// the clock and must Advance once per playback update, not from visual Sample.
    /// Crossing a node emits once per loop, even across several nodes/loops in a long
    /// frame. Reverse, seek and skip are silent; already-fired nodes stay fired.
    /// Pausing does not advance the cursor. Begin can explicitly emit time-zero cues.
    /// If ownedSource is supplied it must be dedicated to this presentation: Stop()
    /// is called only on that source, never on a shared/global AudioSource.
    /// </summary>
    public sealed class MotionSoundTimeline : MonoBehaviour
    {
        public AudioSource ownedSource;
        public MotionSoundCue[] cues = Array.Empty<MotionSoundCue>();
        public event Action<int, long, MotionSoundCue> CueCrossed;
        private double duration, cursor;
        private bool running, loop;
        private long[] lastFiredLoop = Array.Empty<long>();
        private int[] orderedCues = Array.Empty<int>();
        private ulong runVersion;

        public bool IsRunning => running;
        public double Cursor => cursor;

        public void Begin(double durationSeconds, bool looping, bool emitAtStart = true)
        {
            if (double.IsNaN(durationSeconds) || double.IsInfinity(durationSeconds) || durationSeconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(durationSeconds));
            Stop();
            duration = durationSeconds; loop = looping; cursor = 0; running = true;
            unchecked { runVersion++; }
            lastFiredLoop = new long[cues == null ? 0 : cues.Length];
            orderedCues = new int[lastFiredLoop.Length];
            for (int i = 0; i < lastFiredLoop.Length; i++) lastFiredLoop[i] = -1;
            for (int i = 0; i < orderedCues.Length; i++) orderedCues[i] = i;
            Array.Sort(orderedCues, (a, b) => {
                float left = cues[a] == null ? float.PositiveInfinity : cues[a].normalizedTime;
                float right = cues[b] == null ? float.PositiveInfinity : cues[b].normalizedTime;
                int compared = left.CompareTo(right); return compared != 0 ? compared : a.CompareTo(b);
            });
            if (emitAtStart)
            {
                ulong version = runVersion;
                foreach (int i in orderedCues)
                {
                    if (!running || version != runVersion) break;
                    if (cues[i] != null && Mathf.Clamp01(cues[i].normalizedTime) == 0f) Emit(i, 0);
                }
            }
        }

        /// <param name="elapsedSeconds">Unwrapped playback clock; include all completed loops.</param>
        /// <param name="paused">Leave cursor untouched while the presentation is paused.</param>
        /// <param name="reverseOrSeek">Move cursor silently; previously fired nodes remain fired.</param>
        public void Advance(double elapsedSeconds, bool paused = false, bool reverseOrSeek = false)
        {
            if (!running || paused) return;
            if (double.IsNaN(elapsedSeconds) || double.IsInfinity(elapsedSeconds) || elapsedSeconds < 0)
                throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));
            if (!loop) elapsedSeconds = Math.Min(elapsedSeconds, duration);
            if (reverseOrSeek || elapsedSeconds < cursor) { cursor = elapsedSeconds; return; }
            if (cues == null || cues.Length != lastFiredLoop.Length)
                throw new InvalidOperationException("Cannot replace cues during playback; call Begin again.");
            double previous = cursor;
            cursor = elapsedSeconds;
            long first = (long)Math.Floor(previous / duration);
            long last = (long)Math.Floor(elapsedSeconds / duration);
            ulong version = runVersion;
            // A non-looping playback has exactly one logical cycle, including t=duration.
            if (!loop) first = last = 0;
            for (long cycle = first; cycle <= last; cycle++)
            {
                foreach (int i in orderedCues)
                {
                    if (!running || version != runVersion) return;
                    var cue = cues[i]; if (cue == null) continue;
                    double at = cycle * duration + Mathf.Clamp01(cue.normalizedTime) * duration;
                    if (at > previous && at <= elapsedSeconds && cycle > lastFiredLoop[i]) Emit(i, cycle);
                }
                if (cycle == long.MaxValue) break;
            }
        }

        /// <summary>Skip cancels future events and any owned in-flight sounds; no cues are emitted.</summary>
        public void Skip() { Stop(); }
        public void Stop()
        {
            running = false;
            unchecked { runVersion++; }
            if (ownedSource != null) ownedSource.Stop();
        }
        private void Emit(int index, long cycle)
        {
            lastFiredLoop[index] = cycle;
            var cue = cues[index];
            if (ownedSource != null && cue.clip != null) ownedSource.PlayOneShot(cue.clip, Mathf.Clamp01(cue.volume));
            CueCrossed?.Invoke(index, cycle, cue);
        }
        private void OnDisable() { Stop(); }
        private void OnDestroy() { Stop(); }
    }
}
