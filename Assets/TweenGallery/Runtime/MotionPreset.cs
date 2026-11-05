using UnityEngine;

namespace TweenGallery
{
    [CreateAssetMenu(menuName = "Tween Gallery/Motion Preset", fileName = "MotionPreset")]
    public sealed class MotionPreset : ScriptableObject
    {
        public string effectId = "A04";
        public MotionSettings settings = new MotionSettings();
        public void ApplyTo(MotionPlayer player)
        {
            if (player == null) return;
            player.StopAndRestore();
            player.effectId = effectId;
            player.settings = settings == null ? MotionSettings.FromDefinition(MotionCatalog.Find(effectId)) : settings.Clone();
        }
    }
}
