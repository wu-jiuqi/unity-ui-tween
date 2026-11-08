using System;
using System.Collections.Generic;
using UnityEngine;

namespace TweenGallery
{
    /// <summary>Image-gen artwork. Shared sprites are loaded once; no per-preview texture allocation.</summary>
    public static class GeneratedGalleryArt
    {
        static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();
        public static Sprite Get(string name)
        {
            if (cache.TryGetValue(name, out var sprite) && sprite != null) return sprite;
            sprite = Resources.Load<Sprite>("MotionArt/" + name);
            if (sprite == null) throw new InvalidOperationException("Generated gallery artwork missing: Resources/MotionArt/" + name);
            cache[name] = sprite;
            return sprite;
        }
    }
}
