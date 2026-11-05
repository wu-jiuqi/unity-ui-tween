using System;
using UnityEngine;
using UnityEngine.UI;

namespace TweenGallery
{
    [Serializable]
    public sealed class MotionBindings
    {
        public RectTransform target, secondary, destination, mask, shine, lid;
        public CanvasGroup group, secondaryGroup;
        public Image image, secondaryImage, fill;
        public Text label;
        public RectTransform[] items = Array.Empty<RectTransform>();
        public Text[] labels = Array.Empty<Text>();
        public CanvasGroup[] itemGroups = Array.Empty<CanvasGroup>();
    }
}
