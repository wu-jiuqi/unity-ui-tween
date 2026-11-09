using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TweenGallery
{
    /// <summary>Repositions existing controls after size/inset changes; preserves selection ownership and useful scroll state.</summary>
    public sealed class MotionResponsiveLayout : MonoBehaviour
    {
        public RectTransform safeRoot,title,viewport,content,actions;
        public RectTransform[] cards;
        public ScrollRect scroll;
        public Vector4 insets=new Vector4(18,34,18,24); // left, top, right, bottom in reference UI units
        public UnityEvent onLayoutChanged=new UnityEvent();
        public UnityEvent onBeforeLayout=new UnityEvent();
        public bool IsLandscape { get; private set; }
        public bool IsScrollable { get; private set; }
        public int LayoutVersion { get; private set; }
        public float SavedScroll { get; private set; }=1;
        bool applying;
        Vector2 previousSize;Vector4 previousInsets;
        public void RestoreScroll(float normalized)
        {
            if(float.IsNaN(normalized)||float.IsInfinity(normalized))return;
            SavedScroll=Mathf.Clamp01(normalized);if(scroll!=null&&IsScrollable)scroll.verticalNormalizedPosition=SavedScroll;
        }
        void OnRectTransformDimensionsChange(){if(isActiveAndEnabled)Refresh();}
        public void Refresh()
        {
            var root=transform as RectTransform;if(root==null||safeRoot==null||viewport==null||content==null||actions==null||title==null||cards==null||applying)return;
            var size=root.rect.size;if(size.x<=0||size.y<=0)return;if(size==previousSize&&insets==previousInsets&&LayoutVersion>0)return;
            applying=true;
            try
            {
                if(scroll!=null&&IsScrollable)SavedScroll=Mathf.Clamp01(scroll.verticalNormalizedPosition);
                onBeforeLayout.Invoke();
                previousSize=size;previousInsets=insets;IsLandscape=size.x>size.y;
                Place(safeRoot,new Vector2(insets.x,-insets.y),new Vector2(Mathf.Max(1,size.x-insets.x-insets.z),Mathf.Max(1,size.y-insets.y-insets.w)));
                float width=safeRoot.sizeDelta.x,height=safeRoot.sizeDelta.y,actionHeight=IsLandscape?56:108;
                Place(title,new Vector2(16,-12),new Vector2(width-32,56));
                Place(viewport,new Vector2(16,-80),new Vector2(width-32,Mathf.Max(40,height-96-actionHeight)));
                Place(actions,new Vector2(16,-height+actionHeight+8),new Vector2(width-32,actionHeight));
                float cellWidth=IsLandscape?(viewport.sizeDelta.x-12*(cards.Length-1))/Mathf.Max(1,cards.Length):viewport.sizeDelta.x;
                float cellHeight=IsLandscape?viewport.sizeDelta.y:108;
                float contentHeight=IsLandscape?cellHeight:cards.Length*cellHeight+10*Mathf.Max(0,cards.Length-1);
                Place(content,Vector2.zero,new Vector2(viewport.sizeDelta.x,contentHeight));
                for(int i=0;i<cards.Length;i++)if(cards[i]!=null)Place(cards[i],IsLandscape?new Vector2(i*(cellWidth+12),0):new Vector2(0,-i*(cellHeight+10)),new Vector2(cellWidth,cellHeight));
                IsScrollable=!IsLandscape&&contentHeight>viewport.sizeDelta.y+.1f;
                if(scroll!=null){scroll.vertical=!IsLandscape;scroll.horizontal=false;scroll.enabled=IsScrollable;scroll.StopMovement();}
                LayoutVersion++;onLayoutChanged.Invoke();Canvas.ForceUpdateCanvases();if(scroll!=null&&IsScrollable)scroll.verticalNormalizedPosition=SavedScroll;
            }
            finally{applying=false;}
        }
        static void Place(RectTransform r,Vector2 position,Vector2 size){r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=position;r.sizeDelta=size;}
    }
}
