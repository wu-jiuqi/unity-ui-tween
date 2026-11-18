using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace TweenGallery.Recipes
{
    // Draw the actual evaluator, including elastic overshoot, rather than a decorative icon.
    public sealed class RecipeCurveGraphic : MaskableGraphic
    {
        public Ease curve=Ease.OutCubic;
        public float progress;
        public bool showCursor;
        public static float Evaluate(Ease ease,float p)=>DG.Tweening.Core.Easing.EaseManager.Evaluate(ease,null,Mathf.Clamp01(p),1,1.70158f,0);
        public void Sample(float p){progress=p;SetVerticesDirty();}
        protected override void OnPopulateMesh(VertexHelper vh) {
            vh.Clear();var rect=rectTransform.rect;float w=rect.width-14,h=rect.height-16;
            System.Func<float,Vector2> point=p=>new Vector2(rect.xMin+7+w*p,rect.yMin+8+h*(.08f+.76f*Evaluate(curve,p)));
            Segment(vh,new Vector2(rect.xMin+7,rect.yMin+8),new Vector2(rect.xMax-7,rect.yMin+8),1,new Color(.45f,.55f,.7f,.4f));
            for(int i=0;i<48;i++)Segment(vh,point(i/48f),point((i+1)/48f),2.5f,color);
            if(showCursor){var p=point(progress);for(int i=0;i<16;i++){float a=i*Mathf.PI/8,b=(i+1)*Mathf.PI/8;int n=vh.currentVertCount;vh.AddVert(p,Color.white,Vector2.zero);vh.AddVert(p+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*4,Color.white,Vector2.zero);vh.AddVert(p+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*4,Color.white,Vector2.zero);vh.AddTriangle(n,n+1,n+2);}}
        }
        static void Segment(VertexHelper vh,Vector2 a,Vector2 b,float width,Color c){var n=(b-a).normalized;var d=new Vector2(-n.y,n.x)*width*.5f;int at=vh.currentVertCount;vh.AddVert(a-d,c,Vector2.zero);vh.AddVert(a+d,c,Vector2.zero);vh.AddVert(b+d,c,Vector2.zero);vh.AddVert(b-d,c,Vector2.zero);vh.AddTriangle(at,at+1,at+2);vh.AddTriangle(at,at+2,at+3);}
    }
}
