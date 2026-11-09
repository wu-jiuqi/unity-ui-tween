using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TweenGallery
{
    /// <summary>Reference-derived components. Original PNGs stay intact; rects exclude export padding.</summary>
    public static class GalleryCompositionArt
    {
        static readonly Dictionary<string,Sprite> cache=new Dictionary<string,Sprite>();
        public static Sprite SolidTile=>Get("ivory-panel",new Rect(0,0,1254,1254),new Vector4(96,96,96,96));
        public static Sprite Panel=>Action(2);
        public static Sprite Brand=>Get("brand-title",new Rect(30,60,2112,590));
        public static Sprite AchievementTitle=>Get("achievement-title",new Rect(50,65,2072,625));
        public static Sprite Ribbon=>Get("blank-ribbon",new Rect(30,65,2112,625));
        public static Sprite LogoGlyph(int index)
        {
            var texture=Resources.Load<Texture2D>("CompositionArt/logo-glyphs");float side=texture.width*.5f;
            return Get("logo-glyphs",new Rect(index%2*side,(1-index/2)*side,side,side),default,"logo-glyph-"+index);
        }
        public static Sprite Action(int kind)
        {
            string key=kind==0?"mint-action":kind==1?"lavender-action":"cream-action";
            var rect=kind==0?new Rect(10,94,2152,550):kind==1?new Rect(20,100,2132,530):new Rect(10,69,2152,590);
            float cap=kind==0?270:kind==1?260:280;
            return Get(key,rect,new Vector4(330,cap,330,cap));
        }
        static Sprite Get(string key,Rect rect,Vector4 border=default,string cacheKey=null)
        {
            cacheKey=cacheKey??key;if(cache.TryGetValue(cacheKey,out var sprite)&&sprite!=null)return sprite;
            var texture=Resources.Load<Texture2D>("CompositionArt/"+key);
            if(texture==null)throw new InvalidOperationException("Missing reference-derived PNG: "+key);
            sprite=Sprite.Create(texture,rect,new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,border);sprite.name="Composition/"+cacheKey;cache[cacheKey]=sprite;return sprite;
        }
        public static void SetButton(Image image,int kind)
        {
            image.sprite=Action(kind);image.type=Image.Type.Sliced;image.preserveAspect=false;image.pixelsPerUnitMultiplier=10;image.color=Color.white;
            foreach(var text in image.GetComponentsInChildren<Text>(true))
            {
                text.color=kind==0?GalleryTheme.GreenInk:kind==1?GalleryTheme.PlumInk:GalleryTheme.Ink;
                GalleryTypography.Style(text,true);
            }
        }
        public static void SetCard(Image image,bool selected)
        {
            image.sprite=Panel;image.type=Image.Type.Sliced;image.pixelsPerUnitMultiplier=20;image.color=selected?new Color(.81f,1,.78f):Color.white;
        }
    }
}
