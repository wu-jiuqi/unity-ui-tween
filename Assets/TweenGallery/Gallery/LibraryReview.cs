using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace TweenGallery
{
    /// <summary>Render all six motion families from the actual Player, using the same runtime fixtures.</summary>
    public sealed class LibraryReview : MonoBehaviour
    {
        IEnumerator Start()
        {
            var args=Environment.GetCommandLineArgs();if(Array.IndexOf(args,"--library-review")<0)yield break;
            int at=Array.IndexOf(args,"--test-output");string folder=at>=0?args[at+1]:Path.Combine(Application.persistentDataPath,"library-review");Directory.CreateDirectory(folder);
            yield return null;yield return null;
            var font=Resources.Load<Font>("GalleryFont");
            string[] titles={"入场与揭示","按钮与交互","页面与容器","奖励与结算","循环与引导","成套视觉编排"};
            for(int group=0;group<6;group++)
            {
                string category=((char)('A'+group)).ToString();var effects=MotionCatalog.All.Where(e=>e.category==category).ToArray();
                var go=new GameObject("Family review "+category,typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler));
                var canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
                var scaler=go.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);
                var board=(RectTransform)go.transform;
                var bg=Box(board,"Background",new Vector2(800,-450),new Vector2(1600,900));bg.gameObject.AddComponent<Image>().color=GalleryTheme.Background;
                Label(board,font,"奶油花园动效库 · "+category+" / "+titles[group],new Vector2(800,-65),new Vector2(1500,65),34);
                int rows=Mathf.CeilToInt(effects.Length/4f);float cellHeight=rows==3?227:330;
                for(int i=0;i<effects.Length;i++)
                {
                    var center=new Vector2(210+(i%4)*392,-145-(i/4)*cellHeight-cellHeight*.5f);
                    var card=Box(board,effects[i].id,center,new Vector2(370,cellHeight-16));card.gameObject.AddComponent<Image>().color=GalleryTheme.Cream;
                    Label(card,font,effects[i].id+" · "+effects[i].name,new Vector2(0,cellHeight*.5f-41),new Vector2(350,40),20,true);
                    var host=Box(card,"Live fixture",new Vector2(0,-16),new Vector2(340,cellHeight-80),true);
                    var player=DemoFactory.Create(effects[i].id,host,new Vector2(330,cellHeight-88));
                    player.Sample(effects[i].id=="D09"?.35f:effects[i].id=="D07"?.8f:.68f);
                    Label(card,font,effects[i].duration.ToString("0.##")+" s  ·  强度 "+effects[i].strength.ToString("0.##"),new Vector2(0,-cellHeight*.5f+29),new Vector2(350,30),14,true);
                }
                yield return null;CaptureCanvas(canvas,Path.Combine(folder,category+"-family.png"));Destroy(go);yield return null;
            }
            var gallery=GetComponent<GalleryBootstrap>();
            foreach(string id in new[]{"A04","B02","D08","F06"}){gallery.SelectEffect(id);gallery.SeekPreview(id=="B02"?.18f:.68f);yield return null;CaptureCanvas(gallery.GetComponentInChildren<Canvas>(),Path.Combine(folder,id+"-gallery.png"));}
            File.WriteAllText(Path.Combine(folder,"complete.txt"),"Rendered all 46 fixtures in six families and four live gallery views. Unity "+Application.unityVersion+" / "+Application.version);
            Application.Quit(0);
        }
        static RectTransform Box(Transform p,string name,Vector2 center,Vector2 size,bool local=false)
        {
            var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(p,false);r.anchorMin=r.anchorMax=local?new Vector2(.5f,.5f):new Vector2(0,1);r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=center;r.sizeDelta=size;return r;
        }
        static void Label(Transform p,Font font,string value,Vector2 center,Vector2 size,int points,bool local=false)
        {
            var r=Box(p,value,center,size,local);var t=r.gameObject.AddComponent<Text>();t.font=font;t.text=value;t.fontSize=points;t.color=GalleryTheme.Ink;t.alignment=TextAnchor.MiddleCenter;t.raycastTarget=false;
        }
        public static void CaptureCanvas(Canvas canvas,string path)
        {
            var mode=canvas.renderMode;var oldCamera=canvas.worldCamera;float distance=canvas.planeDistance;
            var go=new GameObject("Review camera",typeof(Camera));var camera=go.GetComponent<Camera>();camera.enabled=false;camera.orthographic=true;camera.orthographicSize=Screen.height*.5f;camera.transform.position=new Vector3(0,0,-10);camera.nearClipPlane=.1f;camera.farClipPlane=100;
            var rt=new RenderTexture(Screen.width,Screen.height,24);var active=RenderTexture.active;var tex=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);
            try{camera.targetTexture=rt;canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);tex.Apply();File.WriteAllBytes(path,tex.EncodeToPNG());}
            finally{canvas.renderMode=mode;canvas.worldCamera=oldCamera;canvas.planeDistance=distance;RenderTexture.active=active;camera.targetTexture=null;rt.Release();Destroy(rt);Destroy(tex);Destroy(go);}
        }
    }
}
