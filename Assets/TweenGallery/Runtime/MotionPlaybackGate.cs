using System;
using UnityEngine;

namespace TweenGallery
{
    public enum MotionPurpose { DecorativeLoop, InteractionFeedback, EssentialTransition }
    public enum MotionGateDecision { Play, Restore, CompleteImmediately }

    /// <summary>
    /// Host-controlled visibility and reduced-motion policy for one player.
    /// Hiding/backgrounding cancels decorative and interaction motion, while a
    /// necessary transition goes to its final visual state. Nothing changes game time.
    /// Host code still owns navigation, rewards and input state.
    /// </summary>
    public sealed class MotionPlaybackGate : MonoBehaviour
    {
        public MotionPlayer player;
        public bool screenVisible = true, inViewport = true, reducedMotion;
        public bool blockedByModal, navigating;
        /// <summary>Connect the relevant input component's CancelInteraction; unsubscribe on teardown.</summary>
        public event Action InteractionCancelled;
        private bool focused = true, applicationPaused;
        private string activeId;
        private MotionPurpose activePurpose;
        private MotionSettings activeSettings;
        private bool hasActive;

        public MotionGateDecision Decide(MotionPurpose purpose)
        {
            bool visible = isActiveAndEnabled && gameObject.activeInHierarchy && screenVisible && inViewport && !blockedByModal && !navigating && focused && !applicationPaused;
            if (visible && !reducedMotion) return MotionGateDecision.Play;
            return purpose == MotionPurpose.EssentialTransition ? MotionGateDecision.CompleteImmediately : MotionGateDecision.Restore;
        }

        public MotionGateDecision Play(string id, MotionPurpose purpose, MotionSettings settings = null)
        {
            if (player == null) throw new InvalidOperationException("MotionPlaybackGate needs its own MotionPlayer.");
            if (hasActive) Cancel();
            activeId = id; activePurpose = purpose;
            activeSettings = settings == null ? MotionSettings.FromDefinition(MotionCatalog.Find(id)) : settings.Clone();
            hasActive = true;
            return ApplyDecision();
        }

        /// <summary>Call when host page, viewport or accessibility preference changes.</summary>
        public void SetContext(bool pageVisible, bool viewportVisible, bool reduceMotion)
        {
            bool wasAllowed = Decide(MotionPurpose.InteractionFeedback) == MotionGateDecision.Play;
            screenVisible = pageVisible; inViewport = viewportVisible; reducedMotion = reduceMotion;
            HandleContextChanged(wasAllowed);
        }
        public void SetHostBlockers(bool modalVisible, bool navigationInProgress)
        {
            bool wasAllowed = Decide(MotionPurpose.InteractionFeedback) == MotionGateDecision.Play;
            blockedByModal = modalVisible; navigating = navigationInProgress; HandleContextChanged(wasAllowed);
        }
        public void Cancel()
        {
            if (!hasActive) return;
            if (activePurpose == MotionPurpose.InteractionFeedback) InteractionCancelled?.Invoke();
            hasActive = false; if (player != null) player.StopAndRestore();
        }
        private MotionGateDecision ApplyDecision()
        {
            var decision = Decide(activePurpose);
            if (decision == MotionGateDecision.Play) player.Play(activeId, activeSettings);
            else if (decision == MotionGateDecision.Restore)
            {
                if (activePurpose == MotionPurpose.InteractionFeedback) InteractionCancelled?.Invoke();
                player.StopAndRestore(); hasActive = false;
            }
            else
            {
                // Sample applies only visuals and does not emit sound or business events.
                var oneShot = activeSettings.Clone(); oneShot.loop = false;
                player.StopAndRestore();
                player.effectId = activeId;
                player.settings = oneShot;
                player.Sample(1f);
                // Explicitly keep the required terminal visual when this player is disabled.
                player.ReleaseAtCurrentState();
                hasActive = false;
            }
            return decision;
        }
        private void EnforceCurrent()
        {
            // Do not automatically replay when a context becomes visible again.
            if (hasActive && Decide(activePurpose) != MotionGateDecision.Play) ApplyDecision();
        }
        private void HandleContextChanged(bool wasAllowed)
        {
            // The host may play interaction feedback directly through the input component,
            // without calling Gate.Play. Still cancel that explicitly subscribed input.
            if (wasAllowed && Decide(MotionPurpose.InteractionFeedback) != MotionGateDecision.Play &&
                !(hasActive && activePurpose == MotionPurpose.InteractionFeedback)) InteractionCancelled?.Invoke();
            EnforceCurrent();
        }
        private void OnApplicationFocus(bool value)
        {
            bool before = Decide(MotionPurpose.InteractionFeedback) == MotionGateDecision.Play;
            focused = value; HandleContextChanged(before);
        }
        private void OnApplicationPause(bool value)
        {
            bool before = Decide(MotionPurpose.InteractionFeedback) == MotionGateDecision.Play;
            applicationPaused = value; HandleContextChanged(before);
        }
        private void OnDisable()
        {
            if (hasActive && activePurpose == MotionPurpose.EssentialTransition && player != null)
            {
                // The gate is disabled now, so Decide selects CompleteImmediately.
                ApplyDecision();
                return;
            }
            if (!hasActive || activePurpose != MotionPurpose.InteractionFeedback) InteractionCancelled?.Invoke();
            Cancel();
        }
    }
}
