using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TweenGallery
{
    /// <summary>One visual system for legacy fixtures, serialized templates and all gallery surfaces.</summary>
    public static class GalleryTheme
    {
        public static readonly Color Ink=Hex("4B2C20"), Muted=Hex("76513E"), Cream=Hex("FFF8E9"), Background=Hex("F8EEDC");
        public static readonly Color GreenInk=Hex("234B2B"),PlumInk=Hex("502C4D");
        public static readonly Color Mint=Hex("A6DCC1"), Lavender=Hex("CBADE5"), Gold=Hex("E9C36F"), Coral=Hex("F0ACA1");
        static readonly Dictionary<string,string> colors=new Dictionary<string,string>{
            {"171B38","77574C"},{"11142B","F6EDDD"},{"202442","FFF9EF"},{"AEB5D5","927B6A"},
            {"9254FF","CBADE5"},{"19D9FF","A6DCC1"},{"FF8A22","EDB89B"},{"FFE14A","E9C36F"},{"65EA75","A6DCC1"},{"FF4EAF","F0ACA1"},{"A7B0DA","927B6A"},
            {"087FB7","C1DED9"},{"176F8C","D5E5DC"},{"1A8FA6","BDDCD4"},{"1E6B88","F4E8CF"},{"242B59","E7D8C7"},{"244B72","F2E5C8"},
            {"252D5C","F5E9D6"},{"262E54","EEE4D6"},{"26325B","FFF6E5"},{"303964","FFF6E5"},{"344375","E7EEE1"},{"38336F","FFF6E5"},
            {"3B3D5F","D5CABC"},{"4D448B","F2E8D8"},{"4F5D92","E4D4EE"},{"68749D","DACFBD"},{"7140CD","E2CFEF"},{"ED9B00","C99A50"},
            {"303657","ECE0CF"},{"343A60","EADFF0"},{"111630","EEE5D8"},{"313656","D9C9B7"},{"BEC5E6","927B6A"}
        };
        static readonly Dictionary<int,Sprite> skins=new Dictionary<int,Sprite>();
        static readonly HashSet<string> themedColors=new HashSet<string>(colors.Values);
        public static Color Surface(Color color)
        {
            string key=ColorUtility.ToHtmlStringRGB(color);
            if(colors.TryGetValue(key,out var value)){var c=Hex(value);c.a=color.a;return c;}
            if(themedColors.Contains(key))return color;
            if(color.a<.001f)return color;
            Color.RGBToHSV(color,out float h,out float s,out float v);
            // Colored beams and historical dark surfaces use a restrained warm/pastel treatment.
            if(v<.58f){var c=Color.Lerp(Cream,Lavender,s*.25f);c.a=color.a;return c;}
            if(s>.55f){var c=Color.HSVToRGB(h,Mathf.Min(.28f,s*.35f),Mathf.Max(.82f,v));c.a=color.a;return c;}
            return color;
        }
        public static void Apply(GameObject root)
        {
            foreach(var graphic in root.GetComponentsInChildren<Graphic>(true))
            {
                if(graphic is Text text)
                {
                    float alpha=text.color.a;var c=text.fontSize<=12?Muted:Ink;c.a=alpha;text.color=c;
                    GalleryTypography.Style(text,text.fontStyle==FontStyle.Bold||text.fontSize>=18);
                }
                else graphic.color=Surface(graphic.color);
            }
            foreach(var shadow in root.GetComponentsInChildren<Shadow>(true))
            {
                if(shadow is Outline)continue;
                shadow.effectColor=new Color(.39f,.26f,.16f,.20f);shadow.effectDistance=new Vector2(0,-4);
                var outline=shadow.GetComponent<Outline>();
                if(outline==null)outline=shadow.gameObject.AddComponent<Outline>();
                outline.effectColor=new Color(1,.94f,.80f,.65f);outline.effectDistance=new Vector2(1.5f,1.5f);outline.useGraphicAlpha=true;
            }
        }
        public static void StylePrimaryButton(MotionPlayer player)
        {
            int skin;
            switch(player.effectId){case "B01":skin=0;break;case "B02":skin=3;break;case "B03":skin=4;break;case "B04":skin=3;break;case "B06":skin=1;break;default:return;}
            var image=player.bindings.target.GetComponent<Image>();if(image==null)return;
            image.sprite=Skin(skin);image.color=Color.white;image.type=Image.Type.Simple;image.preserveAspect=false;
            var highlight=player.bindings.target.Find("Highlight");if(highlight!=null){var g=highlight.GetComponent<Graphic>();if(g!=null)g.color=Color.clear;}
        }
        public static Sprite Skin(int index)
        {
            if(skins.TryGetValue(index,out var value)&&value!=null)return value;
            var atlas=Resources.Load<Texture2D>("ButtonArt/cream-buttons");
            // Tight authored regions retain the original skin proportions and avoid neighboring cells.
            Rect[] rects={new Rect(28,647,465,200),new Rect(535,650,465,210),new Rect(1095,605,350,350),
                new Rect(30,130,455,220),new Rect(535,140,470,205),new Rect(1120,125,310,300)};
            value=Sprite.Create(atlas,rects[index],new Vector2(.5f,.5f),100);value.name="CreamButtonSkin"+index;skins[index]=value;return value;
        }
        public static Color Hex(string h){ColorUtility.TryParseHtmlString("#"+h,out var c);return c;}
    }
}
