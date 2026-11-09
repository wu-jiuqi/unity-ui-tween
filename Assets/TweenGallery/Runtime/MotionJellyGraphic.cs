using UnityEngine;
using UnityEngine.UI;
namespace TweenGallery
{
    /// <summary>Small UI mesh shear layered on a dedicated logo glyph Graphic.</summary>
    public sealed class MotionJellyGraphic : BaseMeshEffect
    {
        float deformation;
        public void SetDeformation(float value){if(float.IsNaN(value)||float.IsInfinity(value))return;if(Mathf.Approximately(deformation,value))return;deformation=value;if(graphic!=null)graphic.SetVerticesDirty();}
        public override void ModifyMesh(VertexHelper mesh)
        {
            if(!IsActive()||Mathf.Abs(deformation)<.001f)return;
            var rect=(transform as RectTransform).rect;if(rect.width<.001f)return;UIVertex vertex=default;
            for(int i=0;i<mesh.currentVertCount;i++)
            {
                mesh.PopulateUIVertex(ref vertex,i);float y=Mathf.InverseLerp(rect.yMin,rect.yMax,vertex.position.y);
                vertex.position.x+=(y-.5f)*rect.width*.065f*deformation;
                vertex.position.y+=Mathf.Abs((vertex.position.x-rect.center.x)/rect.width)*rect.height*.04f*deformation;mesh.SetUIVertex(vertex,i);
            }
        }
        protected override void OnDisable(){SetDeformation(0);base.OnDisable();}
    }
}
