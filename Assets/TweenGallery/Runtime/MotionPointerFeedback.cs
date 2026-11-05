using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TweenGallery
{
    /// <summary>Attach to the raycast target of an existing Button. Keeps its original onClick untouched.</summary>
    public sealed class MotionPointerFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public MotionPlayer player;
        public bool rebound = true;
        private int activePointer = int.MinValue;
        private void Reset() { player = GetComponent<MotionPlayer>(); }
        private void Awake() { if (player == null) player = GetComponent<MotionPlayer>(); }
        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left || activePointer != int.MinValue || player == null) return;
            Selectable selectable = GetComponent<Selectable>();
            if (selectable != null && !selectable.IsInteractable()) return;
            activePointer = eventData.pointerId;
            player.SetPressed(true, rebound);
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            if (activePointer != eventData.pointerId) return;
            Release();
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            if (activePointer == eventData.pointerId) Release();
        }
        private void Release()
        {
            activePointer = int.MinValue;
            if (player != null) player.SetPressed(false, rebound);
        }
        private void OnDisable()
        {
            activePointer = int.MinValue;
            if (player != null) player.StopAndRestore();
        }
    }
}
