using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TweenGallery.Recipes
{
    public sealed partial class RecipeGalleryBootstrap : MonoBehaviour
    {
        public RecipeStage Stage {get;private set;}
        public string SelectedId {get;private set;}="05.03";
        public bool Reduced {get;private set;}
        public bool Ready {get;private set;}
        public string PreviewMode=>SelectedId=="03.08"?"flip":"";
        RectTransform root,listContent,stageHost,parameters,popup;
        ScrollRect directory;Text heading,operation,observe,layerLabel,stateFilterLabel,reduceLabel,playLabel,timelineLabel,countLabel;
        Slider timeline;InputField search;
        bool syncing,slow;string query="",stateQuery="",objectQuery="",typeQuery="";
        readonly Dictionary<string,Image> rows=new Dictionary<string,Image>();
        readonly HashSet<int> collapsed=new HashSet<int>();
        public int VisibleRecipeCount=>RecipeCatalog.All.Count(Matches);
        public string[] VisibleRecipeIds=>RecipeCatalog.All.Where(Matches).Select(d=>d.id).ToArray();
        [Serializable] sealed class Export {public int schemaVersion=5;public string recipeId,operation,initialState,finalState,curve;public string[] objectTags,stateTags,typeTags;public float duration,strength,springFrequency,springDamping;public bool reducedMotion;}
        void Start(){BuildReference();Select(SelectedId);Ready=true;}
        bool Matches(RecipeDefinition d){
            if(stateQuery.Length>0&&!d.stateTags.Contains(stateQuery))return false;
            if(objectQuery.Length>0&&!d.objectTags.Contains(objectQuery))return false;
            if(typeQuery.Length>0&&!d.typeTags.Contains(typeQuery))return false;
            var haystack=d.id+" "+d.name+" "+d.operation+" "+d.initialState+" "+d.finalState+" "+d.tags+" "+string.Join(" ",d.objectTags.Concat(d.stateTags).Concat(d.typeTags));
            return query.Length==0||haystack.IndexOf(query,StringComparison.OrdinalIgnoreCase)>=0;
        }
        public void ApplyFilters(string state="",string obj="",string type="",string text=""){
            stateQuery=state;objectQuery=obj;typeQuery=type;query=text;search.SetTextWithoutNotify(text);collapsed.Clear();RefreshDirectory();
            stateFilterLabel.text=state.Length==0?"状态：全部 ▾":state+" ▾";layerLabel.text=obj.Length==0?"对象：全部 ▾":obj+" ▾";
        }
        public void ShowAllRecipes(){ApplyFilters();}
        void OpenMenu(bool objects){
            if(popup!=null){CloseMenu();return;}
            var tags=RecipeCatalog.All.SelectMany(d=>objects?d.objectTags:d.stateTags).Distinct().ToArray();
            popup=RecipeUI.Box(root,"Tag filter choices",-652,36,274,510,RecipeUI.Surface).rectTransform;
            var view=RecipeUI.Rect(popup,"Tag choices viewport",0,0,264,496);view.gameObject.AddComponent<RectMask2D>();var hit=view.gameObject.AddComponent<Image>();hit.color=new Color(1,1,1,.01f);
            var scroll=view.gameObject.AddComponent<ScrollRect>();scroll.viewport=view;scroll.horizontal=false;scroll.scrollSensitivity=35;
            var content=RecipeUI.Rect(view,"Tag choices",0,0,254,(tags.Length+1)*38);content.anchorMin=content.anchorMax=new Vector2(.5f,1);content.pivot=new Vector2(.5f,1);content.anchoredPosition=Vector2.zero;scroll.content=content;
            for(int i=0;i<=tags.Length;i++){string value=i==0?"":tags[i-1];var b=Tool(content,i==0?"全部":value,0,-20-i*38,250,()=>{if(objects)objectQuery=value;else stateQuery=value;CloseMenu();ApplyFilters(stateQuery,objectQuery,typeQuery,query);},false,36);AnchorDirectory((RectTransform)b.transform,-20-i*38);}
            popupFrame=Time.frameCount;
        }
        void CloseMenu(){if(popup!=null){popup.gameObject.SetActive(false);Destroy(popup.gameObject);popup=null;}}
        static void AnchorDirectory(RectTransform r,float y){r.anchorMin=r.anchorMax=new Vector2(.5f,1);r.anchoredPosition=new Vector2(0,y);}
        void RefreshDirectory(){
            float offset=listContent.anchoredPosition.y;
            foreach(Transform child in listContent){child.gameObject.SetActive(false);Destroy(child.gameObject);}rows.Clear();float y=-22;
            for(int group=1;group<=12;group++){
                int g=group;var items=RecipeCatalog.All.Where(d=>d.Group==g&&Matches(d)).ToArray();
                var header=Tool(listContent,(collapsed.Contains(g)?"▸ ":"▾ ")+g.ToString("00")+" "+RecipeCatalog.Layers[g-1],0,y,252,()=>{if(!collapsed.Add(g))collapsed.Remove(g);RefreshDirectory();},false,38);header.name="Category "+g;header.GetComponentInChildren<Text>().fontSize=16;header.GetComponentInChildren<Text>().alignment=TextAnchor.MiddleLeft;header.GetComponentInChildren<Text>().rectTransform.sizeDelta=new Vector2(208,34);header.GetComponentInChildren<Text>().rectTransform.anchoredPosition=new Vector2(-18,0);RecipeUI.Label(header.transform,items.Length+"/8",108,0,42,34,14,RecipeUI.Muted);AnchorDirectory((RectTransform)header.transform,y);y-=42;
                if(!collapsed.Contains(g))foreach(var d in items){string id=d.id;var b=Tool(listContent,d.id+"  "+d.name,0,y,249,()=>Select(id),id==SelectedId,38);b.name="Recipe entry "+id;b.GetComponentInChildren<Text>().fontSize=17;b.GetComponentInChildren<Text>().alignment=TextAnchor.MiddleLeft;AnchorDirectory((RectTransform)b.transform,y);rows[id]=b.GetComponent<Image>();y-=41;}
                y-=9;
            }
            listContent.sizeDelta=new Vector2(254,Mathf.Max(directory.viewport.rect.height,-y+20));listContent.anchoredPosition=new Vector2(0,Mathf.Clamp(offset,0,Mathf.Max(0,listContent.rect.height-directory.viewport.rect.height)));directory.StopMovement();
            countLabel.text=VisibleRecipeCount==96?"12 类 · 96 条配方，全部可操作":"筛选结果 "+VisibleRecipeCount+" / 96 条";
        }
        public void Select(string id){
            CloseMenu();StopGuidance();previewDue=-1;
            if(Stage!=null){Stage.Shutdown();Stage.gameObject.SetActive(false);Destroy(Stage.gameObject);}
            SelectedId=id;SelectedCurveIndex=0;var d=RecipeCatalog.Find(id);var stage=RecipeUI.Rect(stageHost,"Recipe "+id,0,67,960,570);Stage=stage.gameObject.AddComponent<RecipeStage>();Stage.Build(d,Reduced);Stage.Curve=CurveOptions[SelectedCurveIndex];Stage.SetSpeed(slow&&Stage.ClockEditable?.25f:1);
            heading.text=d.id+"  "+d.name;operation.text="操作  "+d.operation;observe.text="观察  "+d.observe;
            foreach(var kv in rows)kv.Value.color=kv.Key==id?RecipeUI.Blue:RecipeUI.Surface;
            BuildReferenceInspector();
            if(rows.TryGetValue(id,out var selected)){float top=-selected.rectTransform.anchoredPosition.y-20,current=listContent.anchoredPosition.y;var category=listContent.Find("Category "+d.Group) as RectTransform;float categoryTop=category==null?top:-category.anchoredPosition.y-20;if(categoryTop<current||top+40>current+directory.viewport.rect.height){listContent.anchoredPosition=new Vector2(0,Mathf.Clamp(categoryTop,0,Mathf.Max(0,listContent.rect.height-directory.viewport.rect.height)));directory.StopMovement();}}
        }
        public void SetReduced(bool value){Reduced=value;if(reduceLabel!=null)reduceLabel.text=value?"减弱动态：开":"减弱动态：关";Stage.SetReduced(value);}
        void CopyParameters(){var d=Stage.Definition;GUIUtility.systemCopyBuffer=JsonUtility.ToJson(new Export{recipeId=d.id,operation=d.operation,initialState=d.initialState,finalState=d.finalState,objectTags=d.objectTags,stateTags=d.stateTags,typeTags=d.typeTags,duration=Stage.Duration,strength=Stage.Strength,springFrequency=Stage.SpringFrequency,springDamping=Stage.SpringDamping,curve=Stage.Curve.ToString(),reducedMotion=Reduced},true);feedbackUntil=Time.unscaledTime+2;}
        void Update(){if(!Ready||Stage==null)return;syncing=true;timeline.SetValueWithoutNotify(Stage.Progress);syncing=false;timeline.interactable=Stage.CanSeek;timelineLabel.text=Stage.CanSeek?Mathf.RoundToInt(Stage.Progress*100)+"%":Stage.ClockEditable?"待播放":"实时";UpdateWorkbench();SyncReferenceState();}
        void OnDestroy(){StopGuidance();if(Stage!=null)Stage.Shutdown();}
    }
}
