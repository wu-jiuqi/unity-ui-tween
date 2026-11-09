using System.Globalization;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static TweenGallery.GalleryPageUi;
namespace TweenGallery
{
    public sealed class GalleryTypographyPreview : MonoBehaviour
    {
        public MotionVisualSequence Sequence { get; private set; }
        public string Status { get; private set; }="文字与Logo使用独立视觉层；点击阶段或拖动时间轴观察";
        public string Content { get; private set; }
        public static GalleryTypographyPreview Create(Transform parent,string id,MotionSettings settings,MotionStyleProfile profile,bool reduced)
        {
            var host=parent as RectTransform;var root=Rect(parent,"Typography recipe "+id,new Vector2(host.rect.width*.5f,-host.rect.height*.5f),host.rect.size,false);
            var view=root.gameObject.AddComponent<GalleryTypographyPreview>();view.Build(id,settings,profile,reduced);return view;
        }
        void Build(string id,MotionSettings settings,MotionStyleProfile profile,bool reduced)
        {
            var area=transform as RectTransform;
            Art(area,"Quiet typography stage",GalleryCompositionArt.SolidTile,new Vector2(0,-25),new Vector2(area.rect.width,area.rect.height-50),true).pixelsPerUnitMultiplier=4;
            var viewportRoot=Rect(area,"Motion viewport",new Vector2(0,-25),new Vector2(area.rect.width,area.rect.height-50));viewportRoot.gameObject.AddComponent<RectMask2D>();
            var plate=Art(viewportRoot,"Stable layout footprint",GalleryCompositionArt.Action(2),new Vector2(0,20),new Vector2(640,205),true);plate.pixelsPerUnitMultiplier=6;
            var slots=Rect(plate.transform,"Fixed glyph layout",Vector2.zero,new Vector2(600,160));
            if(id=="T07")
            {
                var viewport=Rect(slots,"Clipped announcement viewport",Vector2.zero,new Vector2(540,100));viewport.gameObject.AddComponent<RectMask2D>();
                Content="花园来信 · 新的旅程正在等你";var line=Text(viewport,"Moving native announcement",Content,Vector2.zero,new Vector2(560,80),31);
                var marquee=gameObject.AddComponent<MotionMarquee>();marquee.viewport=viewport;marquee.visual=line.rectTransform;marquee.duration=settings.duration;Sequence=marquee;
            }
            else if(id=="T01")
            {
                Content="奖励 +120";var label=Text(slots,"Whole editable label",Content,Vector2.zero,new Vector2(540,110),58);
                var clip=gameObject.AddComponent<MotionTextSequence>();clip.visuals=new[]{label.rectTransform};clip.duration=settings.duration;clip.height=settings.distance;clip.strength=settings.strength;Sequence=clip;
            }
            else
            {
                Content=id=="T05"?"新的旅程，现在出发！":"动效图鉴";
                var elements=StringInfo.GetTextElementEnumerator(Content);var glyphs=new List<RectTransform>();var groups=new List<CanvasGroup>();var meshes=new List<MotionJellyGraphic>();
                var strings=new List<string>();while(elements.MoveNext())strings.Add(elements.GetTextElement());float cell=id=="T05"?48:115;
                for(int i=0;i<strings.Count;i++)
                {
                    var slot=Rect(slots,"Fixed slot "+i,new Vector2((i-(strings.Count-1)*.5f)*cell,0),new Vector2(cell,140));RectTransform visual;
                    if(id=="T03")
                    {
                        var image=Art(slot,"Independent logo glyph "+i,GalleryCompositionArt.LogoGlyph(i),Vector2.zero,new Vector2(140,140));visual=image.rectTransform;meshes.Add(image.gameObject.AddComponent<MotionJellyGraphic>());
                    }
                    else visual=Text(slot,"Native grapheme "+i,strings[i],Vector2.zero,new Vector2(cell+5,110),id=="T05"?35:68).rectTransform;
                    glyphs.Add(visual);groups.Add(Group(visual));
                }
                if(id=="T03")
                {
                    var logo=gameObject.AddComponent<MotionLogoDrop>();logo.glyphs=glyphs.ToArray();logo.groups=groups.ToArray();logo.meshes=meshes.ToArray();
                    logo.timingScale=settings.duration/6.8f;logo.stagger=settings.stagger;logo.dropHeight=settings.distance;logo.strength=settings.strength;Sequence=logo;
                }
                else
                {
                    var clip=gameObject.AddComponent<MotionTextSequence>();clip.visuals=glyphs.ToArray();clip.groups=groups.ToArray();clip.duration=settings.duration;clip.stagger=settings.stagger;clip.height=settings.distance;clip.strength=settings.strength;
                    clip.kind=id=="T04"?TextMotionKind.Shiver:id=="T05"?TextMotionKind.Reveal:id=="T06"?TextMotionKind.Wave:TextMotionKind.Bounce;Sequence=clip;
                }
            }
            Sequence.speed=settings.speed;Sequence.unscaledTime=settings.unscaledTime;Sequence.reducedMotion=reduced;Sequence.CaptureBaseline();Sequence.Seek(0);Sequence.onVisualComplete.AddListener(()=>Status="本轮视觉完成 · 文案、布局占位与业务状态保持独立");
            string[] labels={"准备","动作起点","强调阶段","稳定末态"};float[] marks=id=="T03"?new[]{0,.15f,.24f,1}:new[]{0,.22f,.52f,1};
            for(int i=0;i<4;i++){int at=i;Button(area,"Text stage "+i,labels[i],new Vector2(-300+i*200,area.rect.height*.5f-25),new Vector2(185,36),4,()=>Sequence.Seek(marks[at]));}
            Text(area,"Text driver description",id=="T03"?"逐字掉落 → 30 fps 落地形变 → 全部落稳 → 统一果冻衰减":id=="T05"?"先固定完整排版，再按字素显现；采样不触发提示音或业务":id=="T07"?"完整进入裁切区，再完整离开；提示队列与重复间隔由宿主管理":"布局占位固定，只移动内部文字；动态文案与固定文字素材可分别绑定",new Vector2(0,-190),new Vector2(780,40),17);
            Button(area,"Play text recipe","播放本轮",new Vector2(0,-230),new Vector2(180,36),0,()=>Sequence.Play());GalleryPageUi.ApplyStyle(area,profile,reduced);
        }
        void OnDisable(){if(Sequence!=null)Sequence.Restore();}
    }
}
