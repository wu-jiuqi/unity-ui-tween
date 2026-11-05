using System;
using System.Collections.Generic;
using UnityEngine;

namespace TweenGallery
{
    [Serializable]
    public sealed class MotionDefinition
    {
        public string id, name, english, category, kind, description, useCase, difference, dependencies, binding, ease, prompt;
        public string sourceUrl, sourceTitle, transformation;
        public float duration = .7f, strength = 1f, distance = 120f, stagger = .1f;
        public float startValue = 100f, endValue = 1250f, fillFrom = .08f, fillTo = .9f;
    }

    public static class MotionCatalog
    {
        [Serializable] private sealed class Wrapper { public MotionDefinition[] effects; }
        private static MotionDefinition[] all;
        public static MotionDefinition[] All
        {
            get
            {
                if (all != null) return all;
                TextAsset json = Resources.Load<TextAsset>("motion-catalog");
                if (json == null) throw new InvalidOperationException("Missing Resources/motion-catalog.json. Import the catalog with the runtime templates.");
                var parsed = JsonUtility.FromJson<Wrapper>(json.text);
                if (parsed == null || parsed.effects == null || parsed.effects.Length != 46)
                    throw new InvalidOperationException("Motion catalog must contain the 40 base and six advanced effects.");
                var ids = new HashSet<string>();
                foreach (var effect in parsed.effects)
                    if (effect == null || string.IsNullOrEmpty(effect.id) || !ids.Add(effect.id))
                        throw new InvalidOperationException("Motion catalog contains a missing or duplicate ID.");
                foreach (var category in new[] { ("A",8), ("B",8), ("C",7), ("D",10), ("E",7), ("F",6) })
                    for (int i=1; i<=category.Item2; i++)
                        if (!ids.Contains(category.Item1 + i.ToString("00")))
                            throw new InvalidOperationException("Motion catalog is missing required ID " + category.Item1 + i.ToString("00"));
                all = parsed.effects;
                return all;
            }
        }
        public static MotionDefinition Find(string id)
        {
            foreach (var effect in All) if (effect.id == id) return effect;
            throw new ArgumentException("Unknown motion ID: " + id, nameof(id));
        }
    }

    [Serializable]
    public sealed class MotionSettings
    {
        [Min(.05f)] public float duration = .7f;
        [Range(0f, 3f)] public float strength = 1f;
        public float distance = 120f;
        [Min(0f)] public float stagger = .1f;
        public string ease = "Preset";
        public bool reverse, loop;
        public bool unscaledTime = true;
        [Range(.05f, 4f)] public float speed = 1f;
        public float startValue = 100f, endValue = 1250f;
        [Range(0f, 1f)] public float fillFrom = .08f, fillTo = .9f;

        public MotionSettings Clone() { return (MotionSettings)MemberwiseClone(); }
        public static MotionSettings FromDefinition(MotionDefinition definition)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            return new MotionSettings { duration = definition.duration, strength = definition.strength,
                distance = definition.distance, stagger = definition.stagger,
                ease = string.IsNullOrEmpty(definition.ease) ? "Preset" : definition.ease,
                startValue = definition.startValue, endValue = definition.endValue,
                fillFrom = definition.fillFrom, fillTo = definition.fillTo };
        }
    }
}
