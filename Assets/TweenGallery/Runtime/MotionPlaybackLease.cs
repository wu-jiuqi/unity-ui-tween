using System;
using System.Collections;
using UnityEngine;

namespace TweenGallery
{
    /// <summary>
    /// Guards delayed writes and callbacks with two independent epochs: one for
    /// each pool borrow and one for each playback within that borrow.
    /// Unity objects and these tokens must be accessed on Unity's main thread.
    /// </summary>
    public sealed class MotionPlaybackLease : MonoBehaviour
    {
        public readonly struct Token
        {
            internal readonly MotionPlaybackLease owner;
            internal readonly ulong lease, playback;
            internal readonly MotionPlayer pageOwner;
            internal readonly ulong pagePlayback;
            internal readonly bool tracksPage;
            internal Token(MotionPlaybackLease owner, ulong lease, ulong playback, MotionPlayer pageOwner)
            {
                this.owner = owner; this.lease = lease; this.playback = playback;
                this.pageOwner = pageOwner; tracksPage = pageOwner != null;
                pagePlayback = tracksPage ? pageOwner.PlayVersion : 0;
            }
        }

        private ulong leaseVersion, playbackVersion;
        private bool borrowed;

        /// <summary>Call once on each borrow, before starting work on the reused object.</summary>
        public void Borrow() { unchecked { leaseVersion++; playbackVersion++; } borrowed = true; }
        /// <summary>Call for every play/replay, including plays with no delay.</summary>
        public Token BeginPlayback(MotionPlayer pageOwner = null)
        {
            if (!borrowed) throw new InvalidOperationException("Call Borrow before starting playback.");
            unchecked { playbackVersion++; }
            return new Token(this, leaseVersion, playbackVersion, pageOwner);
        }
        /// <summary>Cancels old work without relinquishing the current pool borrow.</summary>
        public void CancelPlayback() { unchecked { playbackVersion++; } }
        public bool IsCurrent(Token token) => this && isActiveAndEnabled && borrowed && token.owner == this && token.lease == leaseVersion && token.playback == playbackVersion && (!token.tracksPage || (token.pageOwner && token.pageOwner.IsCurrentPlayback(token.pagePlayback)));

        /// <summary>Guard each write and callback separately, including after any wait.</summary>
        public bool TryRun(Token token, Action action)
        {
            if (action == null || !IsCurrent(token)) return false;
            action();
            return true;
        }

        /// <summary>Return only if both borrow and playback are still current.</summary>
        public bool TryReturn(Token token, Action returnToPool)
        {
            if (returnToPool == null || !IsCurrent(token)) return false;
            borrowed = false;
            unchecked { leaseVersion++; playbackVersion++; }
            returnToPool();
            return true;
        }

        /// <summary>Checks again after waiting and immediately before invoking the callback.</summary>
        public Coroutine RunAfter(Token token, float delaySeconds, Action callback, bool unscaled = true)
        {
            if (!IsCurrent(token) || callback == null) return null;
            return StartCoroutine(WaitThenRun(token, delaySeconds, callback, unscaled));
        }
        private IEnumerator WaitThenRun(Token token, float delay, Action callback, bool unscaled)
        {
            if (delay > 0f)
            {
                if (unscaled) yield return new WaitForSecondsRealtime(delay);
                else yield return new WaitForSeconds(delay);
            }
            else yield return null;
            if (!IsCurrent(token)) yield break;
            TryRun(token, callback);
        }
        private void OnDisable() { borrowed = false; unchecked { leaseVersion++; playbackVersion++; } }
        private void OnDestroy() { borrowed = false; unchecked { leaseVersion++; playbackVersion++; } }
    }
}
