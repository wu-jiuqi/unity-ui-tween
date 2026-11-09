using UnityEngine;
using UnityEngine.UI;
namespace TweenGallery
{
    public sealed partial class GalleryBootstrap
    {
        void ScrollSelectedIntoView(string id)
        {
            int index=visibleIds.IndexOf(id);float overflow=listContent.rect.height-536;if(index<0||overflow<=0)return;
            float offset=listContent.anchoredPosition.y,y=index*66;if(y<offset)offset=y;else if(y+59>offset+536)offset=y+59-536;
            Canvas.ForceUpdateCanvases();listScroll.verticalNormalizedPosition=1-Mathf.Clamp01(offset/overflow);
        }
        void BuildCompositionSurfaces()
        {
            var inspector=root.Find("Parameter panel");
            for(int i=0;i<4;i++)
            {
                var card=ImageAt(root,"Parameter backing "+i,1248,232+i*74,308,70,Color.white,false);
                card.sprite=GalleryCompositionArt.Panel;card.type=Image.Type.Sliced;card.pixelsPerUnitMultiplier=20;
                card.transform.SetSiblingIndex(inspector.GetSiblingIndex()+1);
            }
            foreach(var label in new[]{durationValue,strengthValue,distanceValue,staggerValue}){label.fontSize=18;GalleryTypography.Style(label,true);label.color=Ink;}
            foreach(string name in new[]{"时长","强度","位移","错开间隔"}){var item=root.Find("Text "+name);if(item!=null){var text=item.GetComponent<Text>();text.fontSize=17;GalleryTypography.Style(text,true);}}
        }
        void RefreshCompositionButtons()
        {
            foreach(var b in root.GetComponentsInChildren<Button>(true))
            {
                var image=b.image;if(image==null||image.sprite==null)continue;
                for(int kind=0;kind<3;kind++)if(image.sprite==GalleryCompositionArt.Action(kind)){GalleryCompositionArt.SetButton(image,kind);break;}
            }
        }
    }
}
