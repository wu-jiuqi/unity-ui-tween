using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TweenGallery.Recipes
{
    /// <summary>Original imagegen atlases, split by Sprite rects. Source PNG pixels are never patched.</summary>
    public static class RecipeArt
    {
        [Serializable] public sealed class Part {public string key,atlas;public float x,y,width,height,left,bottom,right,top;}
        [Serializable] public sealed class Manifest {public Part[] parts;}
        static readonly Dictionary<string,Sprite> cache=new Dictionary<string,Sprite>();
        static Dictionary<string,Part> parts;
        public static bool Available=>Resources.Load<TextAsset>("RecipeArt/manifest")!=null;
        public static bool ApprovedAvailable=>Resources.Load<Texture2D>("RecipeArt/approved-cards")!=null;
        public static Sprite Get(string key) {
            if(cache.TryGetValue(key,out var sprite)&&sprite!=null)return sprite;
            if(parts==null){var json=Resources.Load<TextAsset>("RecipeArt/manifest");if(json==null)throw new InvalidOperationException("Recipe art manifest missing");parts=new Dictionary<string,Part>();foreach(var p in JsonUtility.FromJson<Manifest>(json.text).parts)parts.Add(p.key,p);}
            if(!parts.TryGetValue(key,out var part))throw new ArgumentException("Recipe sprite not in manifest: "+key);
            var texture=Resources.Load<Texture2D>("RecipeArt/"+part.atlas);if(texture==null)throw new InvalidOperationException("Missing atlas "+part.atlas);
            sprite=Sprite.Create(texture,new Rect(part.x,part.y,part.width,part.height),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,new Vector4(part.left,part.bottom,part.right,part.top));sprite.name="RecipeArt/"+key;cache[key]=sprite;return sprite;
        }
        public static void Apply(Image image,string key,bool sliced=true) {
            image.sprite=Get(key);image.type=sliced?Image.Type.Sliced:Image.Type.Simple;image.pixelsPerUnitMultiplier=3;image.color=Color.white;image.preserveAspect=!sliced;
        }
        public static string Surface(string name,string shape,Color color) {
            string lower=name.ToLowerInvariant();
            if(lower.Contains("chest body"))return "chest-body";if(lower.Contains("chest lid"))return "chest-lid";
            if(lower.Contains("rating star"))return "star-lit";if(lower.Contains("representative coin"))return "coin";
            if(lower.Contains("reward jewel")||lower.Contains("energy crystal"))return "crystal";
            if(lower.Contains("directional spinner"))return "spinner";
            if(lower.Contains("achievement archive"))return "archive-slot";
            if(lower.Contains("wallet"))return "wallet";
            if(lower.Contains("card back"))return "card-back";
            if(lower.StartsWith("card "))return "card-front";
            if(lower.Contains("source slot"))return "slot-source";if(lower.Contains("invalid slot"))return "slot-invalid";if(lower.Contains("valid slot"))return "slot-valid";
            if(lower.Contains("halo"))return "halo";
            if(lower.Contains("shine"))return "shine";
            if(lower.Contains("bar track"))return "bar-track";
            if(lower.Contains("bar fill"))return "bar-xp";
            if(lower.Contains("current health"))return "bar-health";
            if(lower.Contains("focus underline")||lower.Contains("selection indicator"))return "underline";
            if(lower.Contains("hold progress")||lower.Contains("fill"))return "bar-gold";
            if(lower.Contains("knob")||lower.Contains("handle"))return "slider-handle";
            if(lower.Contains("track"))return "slider-track";
            if(lower.Contains("input"))return "input-normal";
            if(lower.Contains("value bubble"))return "value-bubble";
            if(lower.Contains("task id")||lower.Contains("user id")||lower.Contains("next row"))return "list-row";
            if(lower.Contains("feedback icon"))return "notice-info";
            if(lower.Contains("shadow"))return "button-shadow";
            if(lower.Contains("highlight"))return "button-gloss";
            if(lower.Contains("button visual"))return "button-blue";
            if(shape=="star")return "star-lit";if(shape=="diamond")return "crystal";if(shape=="hand")return "hand";if(shape=="ring")return "halo";if(shape=="circle")return "badge";
            if(lower.Contains("emphasis"))return "selection-outline";
            return "panel";
        }
        public static bool IsIcon(string key)=>key=="coin"||key=="crystal"||key=="spinner"||key=="star-lit"||key=="star-unlit"||key=="medal"||key=="level-badge"||key=="halo"||key=="shine"||key=="hand"||key.StartsWith("notice-")||key=="badge"||key=="chest-body"||key=="chest-lid"||key=="selection-outline";
    }
}
