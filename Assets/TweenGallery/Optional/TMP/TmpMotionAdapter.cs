#if TWEEN_GALLERY_TMP
using System;
using System.Globalization;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace TweenGallery.Optional
{
    /// <summary>
    /// Optional TextMeshPro presentation only. Business state is owned by the caller.
    /// Enable TWEEN_GALLERY_TMP only in projects where TMP is installed and referenced.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TmpMotionAdapter : MonoBehaviour
    {
        [SerializeField] private TMP_Text target;
        private Tween owned;
        private int revision;
        private int lastNumber;
        private bool haveNumber;
        private bool ownsRevealLimit;
        private int priorVisibleCharacters;

        public TMP_Text Target { get { return target; } }
        public bool IsPlaying { get { return owned != null && owned.IsActive() && owned.IsPlaying(); } }

        /// <summary>Bind explicitly; stopping the previous binding restores its reveal limit.</summary>
        public void Bind(TMP_Text value)
        {
            Stop();
            target = value;
            haveNumber = false;
        }

        /// <summary>
        /// Animate integer UI from the supplied start toward the supplied authoritative value.
        /// Grouping uses invariant comma separators, or no grouping when false.
        /// Cancellation leaves the current displayed number; call SetBusinessInteger to commit state.
        /// </summary>
        public void PlayInteger(int from, int businessValue, float duration, bool grouping = true,
            bool unscaledTime = true, Action onComplete = null)
        {
            StartNumber(from, businessValue, duration, grouping, "", unscaledTime, onComplete);
        }

        /// <summary>
        /// Animate whole percentage points, e.g. 25 to 80 displays 25% to 80%.
        /// Values are not clamped: business rules decide the valid range.
        /// </summary>
        public void PlayPercent(int fromPercent, int businessPercent, float duration,
            bool unscaledTime = true, Action onComplete = null)
        {
            StartNumber(fromPercent, businessPercent, duration, false, "%", unscaledTime, onComplete);
        }

        private void StartNumber(int from, int finalValue, float duration, bool grouping, string suffix,
            bool unscaledTime, Action onComplete)
        {
            RequireTarget();
            Stop();
            int currentRevision = revision;
            haveNumber = false;
            WriteNumber(from, grouping, suffix);
            if (duration <= 0f || from == finalValue)
            {
                WriteNumber(finalValue, grouping, suffix);
                if (isActiveAndEnabled && currentRevision == revision) onComplete?.Invoke();
                return;
            }
            float progress = 0f;
            owned = DOTween.To(() => progress, p => {
                    progress = p;
                    if (currentRevision != revision || target == null) return;
                    // Double interpolation avoids overflow and avoids float integer precision loss.
                    int value = (int)Math.Round((double)from + ((double)finalValue - from) * p,
                        MidpointRounding.AwayFromZero);
                    WriteNumber(value, grouping, suffix);
                }, 1f, duration)
                .SetEase(Ease.OutQuad).SetUpdate(unscaledTime).SetAutoKill(true)
                .OnComplete(() => {
                    if (currentRevision != revision || target == null || !isActiveAndEnabled) return;
                    WriteNumber(finalValue, grouping, suffix);
                    owned = null;
                    onComplete?.Invoke();
                });
        }

        /// <summary>Cancel animation and display the latest authoritative business value.</summary>
        public void SetBusinessInteger(int value, bool grouping = true, bool percent = false)
        {
            RequireTarget();
            Stop();
            haveNumber = false;
            WriteNumber(value, grouping && !percent, percent ? "%" : "");
        }

        private void WriteNumber(int value, bool grouping, string suffix)
        {
            if (target == null || (haveNumber && value == lastNumber)) return;
            target.text = value.ToString(grouping ? "N0" : "D", CultureInfo.InvariantCulture) + suffix;
            lastNumber = value;
            haveNumber = true;
        }

        /// <summary>
        /// Reveal the existing TMP text without changing its string or rich-text tags.
        /// Restores the previously configured maxVisibleCharacters on Stop/disable.
        /// </summary>
        public void Reveal(float duration, bool unscaledTime = true, Action onComplete = null)
        {
            RequireTarget();
            Stop();
            int currentRevision = revision;
            priorVisibleCharacters = target.maxVisibleCharacters;
            ownsRevealLimit = true;
            // ForceMeshUpdate ignores inactive objects unless the first parameter is true.
            target.ForceMeshUpdate(true, true);
            int total = target.textInfo.characterCount;
            target.maxVisibleCharacters = 0;
            if (duration <= 0f || total == 0)
            {
                FinishReveal(currentRevision, onComplete);
                return;
            }
            float progress = 0f;
            int lastVisible = -1;
            owned = DOTween.To(() => progress, p => {
                    progress = p;
                    if (currentRevision != revision || target == null) return;
                    int count = Mathf.Clamp(Mathf.FloorToInt(total * p), 0, total);
                    if (count == lastVisible) return;
                    lastVisible = count;
                    target.maxVisibleCharacters = count;
                }, 1f, duration)
                .SetEase(Ease.Linear).SetUpdate(unscaledTime).SetAutoKill(true)
                .OnComplete(() => FinishReveal(currentRevision, onComplete));
        }

        private void FinishReveal(int currentRevision, Action onComplete)
        {
            if (currentRevision != revision || target == null || !isActiveAndEnabled) return;
            target.maxVisibleCharacters = priorVisibleCharacters;
            ownsRevealLimit = false;
            owned = null;
            onComplete?.Invoke();
        }

        public void Pause() { if (owned != null && owned.IsActive()) owned.Pause(); }
        public void Resume() { if (owned != null && owned.IsActive()) owned.Play(); }

        /// <summary>Kill only this adapter's tween and invalidate its callbacks.</summary>
        public void Stop()
        {
            revision++;
            if (owned != null && owned.IsActive()) owned.Kill(false);
            owned = null;
            if (ownsRevealLimit && target != null) target.maxVisibleCharacters = priorVisibleCharacters;
            ownsRevealLimit = false;
        }

        private void RequireTarget()
        {
            if (target == null) throw new InvalidOperationException("Bind a TMP_Text before starting TMP animation.");
            if (!isActiveAndEnabled) throw new InvalidOperationException("TMP adapter must be active and enabled.");
        }
        private void OnDisable() { Stop(); }
        private void OnDestroy() { Stop(); }
    }
}
#endif
