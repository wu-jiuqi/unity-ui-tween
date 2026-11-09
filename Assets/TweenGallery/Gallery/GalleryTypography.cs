using UnityEngine;
using UnityEngine.UI;
namespace TweenGallery
{
    public static class GalleryTypography
    {
        static Font regular,medium,heading;
        public static void Style(Text text,bool emphasis=false)
        {
            if(regular==null)regular=Resources.Load<Font>("GalleryFont");if(medium==null)medium=Resources.Load<Font>("GalleryMediumFont");if(heading==null)heading=Resources.Load<Font>("GalleryHeadingFont");
            text.font=text.fontSize>=23?heading:emphasis&&text.fontSize>=13?medium:regular;
            text.fontStyle=FontStyle.Normal;
        }
    }
}
