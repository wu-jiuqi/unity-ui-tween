using System.Collections.Generic;
using UnityEngine;

namespace TweenGallery
{
    /// <summary>Original vector-like artwork generated locally; no downloaded art dependency.</summary>
    public static class GalleryShapes
    {
        static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        public static Sprite Get(string shape)
        {
            if (Cache.TryGetValue(shape, out var sprite) && sprite != null) return sprite;
            const int n = 128;
            var texture = new Texture2D(n, n, TextureFormat.RGBA32, false) { name = "Gallery_" + shape, filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            var pixels = new Color[n * n];
            var star = new Vector2[10];
            for (int i = 0; i < star.Length; i++)
            {
                float angle = Mathf.PI * .5f + i * Mathf.PI / 5;
                float radius = i % 2 == 0 ? .98f : .43f;
                star[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            }
            for (int y = 0; y < n; y++) for (int x = 0; x < n; x++)
            {
                float u = (x + .5f) / n * 2 - 1, v = (y + .5f) / n * 2 - 1;
                float a = 0;
                switch (shape)
                {
                    case "circle": a = Edge(1 - Mathf.Sqrt(u*u + v*v)); break;
                    case "ring": a = Edge(.15f - Mathf.Abs(Mathf.Sqrt(u*u + v*v) - .77f)); break;
                    case "diamond": a = Edge(1 - Mathf.Abs(u) - Mathf.Abs(v)); break;
                    case "star":
                        a = PolygonAlpha(new Vector2(u, v), star); break;
                    case "hand":
                        // Index finger, palm, thumb, and three folded fingers.
                        a = Mathf.Max(RoundBox(u, v, -.16f, .28f, .17f, .64f, .14f), RoundBox(u,v,.08f,-.35f,.49f,.48f,.23f));
                        a = Mathf.Max(a, RoundBox(u,v,-.52f,-.31f,.22f,.33f,.16f));
                        a = Mathf.Max(a, RoundBox(u,v,.18f,-.02f,.16f,.36f,.14f));
                        a = Mathf.Max(a, RoundBox(u,v,.47f,-.11f,.15f,.30f,.13f)); break;
                    case "spark": a = Edge(.16f - Mathf.Min(Mathf.Abs(u), Mathf.Abs(v))) * Edge(1 - Mathf.Abs(u) - Mathf.Abs(v)); break;
                    default: a = RoundBox(u,v,0,0,1,1,.22f); break;
                }
                pixels[y*n+x] = new Color(1,1,1,a);
            }
            texture.SetPixels(pixels); texture.Apply(false, false);
            sprite = Sprite.Create(texture, new Rect(0,0,n,n), new Vector2(.5f,.5f), 100, 0, SpriteMeshType.FullRect,
                shape == "rounded" ? new Vector4(18,18,18,18) : Vector4.zero);
            sprite.name = "Gallery_" + shape; Cache[shape] = sprite; return sprite;
        }

        public static Sprite RewardArt()
        {
            const string key = "reward-art";
            if (Cache.TryGetValue(key, out var cached) && cached != null) return cached;
            const int w = 384, h = 256;
            var texture = new Texture2D(w,h,TextureFormat.RGBA32,false) { name="OriginalRewardArt", filterMode=FilterMode.Bilinear, wrapMode=TextureWrapMode.Clamp };
            var pixels = new Color[w*h];
            for (int y=0;y<h;y++) for(int x=0;x<w;x++)
            {
                float u=(x+.5f)/w, v=(y+.5f)/h;
                var c=Color.Lerp(new Color(.30f,.10f,.78f),new Color(.82f,.15f,.96f),v);
                float dx=(u-.5f)*1.5f, dy=v-.48f;
                float r=Mathf.Sqrt(dx*dx+dy*dy);
                float rays=Mathf.Sin(Mathf.Atan2(dy,dx)*12);
                c=Color.Lerp(c,new Color(1,.75f,.18f),Mathf.Max(0,rays)*.2f);
                if(r<.30f)c=new Color(1,.63f,.03f);
                if(r<.255f)c=new Color(1,.86f,.16f);
                if(r<.22f)c=Color.Lerp(new Color(1,.93f,.35f),new Color(1,.67f,.06f),v);
                if(Mathf.Abs(dx)+Mathf.Abs(dy)<.13f)c=Color.white;
                if((Mathf.Abs(u-.17f)<.012f&&Mathf.Abs(v-.73f)<.07f)||(Mathf.Abs(v-.73f)<.016f&&Mathf.Abs(u-.17f)<.05f))c=Color.white;
                if((Mathf.Abs(u-.83f)<.008f&&Mathf.Abs(v-.27f)<.055f)||(Mathf.Abs(v-.27f)<.012f&&Mathf.Abs(u-.83f)<.035f))c=Color.white;
                float edge=RoundBox((u-.5f)*2,(v-.5f)*2,0,0,1,1,.13f);
                c.a=edge; pixels[y*w+x]=c;
            }
            texture.SetPixels(pixels);texture.Apply(false,false);
            cached=Sprite.Create(texture,new Rect(0,0,w,h),new Vector2(.5f,.5f),100);
            cached.name="OriginalRewardArt";Cache[key]=cached;return cached;
        }

        static float Edge(float inside) => Mathf.Clamp01(inside*64+.5f);
        static float PolygonAlpha(Vector2 p, Vector2[] vertices)
        {
            bool inside = false;
            float nearest = 10;
            for (int i = 0, j = vertices.Length - 1; i < vertices.Length; j = i++)
            {
                Vector2 a = vertices[j], b = vertices[i], ab = b - a;
                float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
                nearest = Mathf.Min(nearest, Vector2.Distance(p, a + t * ab));
                if ((a.y > p.y) != (b.y > p.y) && p.x < (b.x - a.x) * (p.y - a.y) / (b.y - a.y) + a.x) inside = !inside;
            }
            return Edge(inside ? nearest : -nearest);
        }
        static float RoundBox(float x,float y,float cx,float cy,float hx,float hy,float radius)
        {
            float qx=Mathf.Abs(x-cx)-hx+radius,qy=Mathf.Abs(y-cy)-hy+radius;
            float d=Mathf.Sqrt(Mathf.Max(qx,0)*Mathf.Max(qx,0)+Mathf.Max(qy,0)*Mathf.Max(qy,0))+Mathf.Min(Mathf.Max(qx,qy),0)-radius;
            return Edge(-d);
        }
    }
}
