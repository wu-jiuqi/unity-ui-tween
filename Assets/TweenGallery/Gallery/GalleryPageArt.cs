using System;
using UnityEngine;
namespace TweenGallery
{
    public static class GalleryPageArt
    {
        static Sprite panel,medal;static readonly Sprite[] icons=new Sprite[4];
        static readonly Sprite[] buttons=new Sprite[6];
        public static Sprite Panel=>panel!=null?panel:panel=Load("cream-panel");
        public static Sprite Medal=>medal!=null?medal:medal=Load("achievement-medal");
        static Sprite Load(string name){var s=Resources.Load<Sprite>("StyleArt/"+name);if(s==null)throw new InvalidOperationException("Missing generated StyleArt/"+name);return s;}
        public static Sprite Button(int index)
        {
            if(buttons[index]!=null)return buttons[index];var source=GalleryTheme.Skin(index);
            buttons[index]=Sprite.Create(source.texture,source.rect,new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,new Vector4(100,70,100,70));return buttons[index];
        }
        public static Sprite Icon(int index)
        {
            if(index<0||index>3)throw new ArgumentOutOfRangeException(nameof(index));if(icons[index]!=null)return icons[index];
            var atlas=Resources.Load<Texture2D>("StyleArt/reward-icons");if(atlas==null)throw new InvalidOperationException("Missing generated reward icon atlas");
            float w=atlas.width*.5f,h=atlas.height*.5f;icons[index]=Sprite.Create(atlas,new Rect(index%2*w,(1-index/2)*h,w,h),new Vector2(.5f,.5f),100);icons[index].name="RewardIcon"+index;return icons[index];
        }
    }
}
