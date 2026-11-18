using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TweenGallery.Recipes
{
    /// <summary>96 scenario recipes, not a rename of the original primitive gallery.</summary>
    public sealed partial class RecipeGalleryBootstrap : MonoBehaviour
    {
        public RecipeStage Stage {get;private set;}
        public string SelectedId {get;private set;}="01.01";
        public bool Reduced {get;private set;}
        public bool Ready {get;private set;}
        RectTransform root,listContent,stageHost,parameters,popup;
        ScrollRect directory;Text heading,operation,observe,layerLabel,stateFilterLabel,reduceLabel,playLabel,timelineLabel,countLabel;
        Slider timeline;InputField search;
        int layerFilter,stateFilter;bool syncing,slow;string query="";
        readonly Dictionary<string,Image> rows=new Dictionary<string,Image>();
        static readonly string[] Filters={"全部状态","进入 / 退出","按压 / 焦点","值 / 选择","拖动 / 滚动","循环 / 闲置"};
        [Serializable] sealed class Export {public int schemaVersion=4;public string driver="RecipeStage",recipeId,operation,initialState,finalState,previewMode;public float duration,strength,springFrequency,springDamping;public string curve;public bool reducedMotion;}
        void Start(){BuildReference();Select("03.08");Ready=true;}
        void OpenMenu(bool layers) {
            if(popup!=null){Destroy(popup.gameObject);popup=null;return;}
            int count=layers?13:Filters.Length;popup=RecipeUI.Box(root,"Filter choices",referenceShell?(layers?-130:20):(layers?200:402),365-count*17,260,count*34+12,RecipeUI.Surface).rectTransform;
            for(int i=0;i<count;i++){int index=i;RecipeUI.Button(popup,layers?(i==0?"全部层级":i.ToString("00")+" "+RecipeCatalog.Layers[i-1]):Filters[i],0,(count-1)*17-i*34,245,()=>{if(layers){layerFilter=index;layerLabel.text=index==0?"全部层级 ▾":index.ToString("00")+" "+RecipeCatalog.Layers[index-1]+" ▾";}else{stateFilter=index;stateFilterLabel.text=Filters[index]+" ▾";}Destroy(popup.gameObject);popup=null;RefreshDirectory();},RecipeUI.Dark,30);}
        }
        bool Matches(RecipeDefinition d) {
            if(layerFilter!=0&&d.Group!=layerFilter)return false;
            if(query.Length>0&&!(d.id+" "+d.name+" "+d.initialState+" "+d.finalState+" "+d.tags+" "+d.operation).ToLowerInvariant().Contains(query.ToLowerInvariant()))return false;
            switch(stateFilter){case 1:return d.Group<=3;case 2:return d.Group==5||d.Group==6||d.id=="04.02";case 3:return d.Group==4||d.Group==6||d.Group==9||d.Group==11;case 4:return d.Group==7||d.Group==8||d.id=="03.04";case 5:return d.Group==12||d.id=="04.08"||d.id=="09.07";default:return true;}
        }
        void RefreshDirectory() {
            foreach(Transform child in listContent){child.gameObject.SetActive(false);Destroy(child.gameObject);}rows.Clear();float y=-20;int group=0,count=0;
            if(referenceShell&&shortcuts&&query.Length==0&&layerFilter==0&&stateFilter==0){BuildShortcuts();return;}
            foreach(var d in RecipeCatalog.All){if(!Matches(d))continue;if(d.Group!=group){group=d.Group;RecipeUI.Label(listContent,group.ToString("00")+"  "+d.layer,0,y,238,32,16,RecipeUI.Muted,TextAnchor.MiddleLeft);y-=38;}
                string id=d.id;var button=RecipeUI.Button(listContent,d.id+"  "+d.name,0,y,240,()=>Select(id),id==SelectedId?RecipeUI.Hex("3B465D"):RecipeUI.Hex("272B32"),33);button.GetComponentInChildren<Text>().alignment=TextAnchor.MiddleLeft;button.GetComponentInChildren<Text>().fontSize=15;rows[id]=button.GetComponent<Image>();y-=38;count++;
                if(referenceShell){RecipeArt.Apply(button.GetComponent<Image>(),"wb-play");button.GetComponent<Image>().pixelsPerUnitMultiplier=1;button.GetComponent<Image>().color=id==SelectedId?Color.white:Color.clear;}
            }
            if(count==0)RecipeUI.Label(listContent,"没有匹配的配方",0,-50,235,50,17,RecipeUI.Muted);
            foreach(RectTransform child in listContent)child.anchorMin=child.anchorMax=new Vector2(.5f,1);
            if(referenceShell)foreach(RectTransform child in listContent){child.sizeDelta=new Vector2(212,child.sizeDelta.y);var label=child.GetComponentInChildren<Text>();if(label!=null)label.rectTransform.sizeDelta=new Vector2(200,label.rectTransform.sizeDelta.y);}
            listContent.sizeDelta=new Vector2(referenceShell?223:250,Mathf.Max(referenceShell?790:680,-y+20));directory.verticalNormalizedPosition=1;countLabel.text=count+" / 96 条配方";
        }
        public void Select(string id) {
            if(Stage!=null){Stage.Shutdown();Stage.gameObject.SetActive(false);Destroy(Stage.gameObject);}
            SelectedId=id;var definition=RecipeCatalog.Find(id);var stage=RecipeUI.Rect(stageHost,"Recipe "+id,0,0,880,490);Stage=stage.gameObject.AddComponent<RecipeStage>();Stage.Build(definition,Reduced);Stage.SetSpeed(slow?.25f:1);
            heading.text=id+"  "+definition.name;operation.text="怎么操作  "+definition.operation;observe.text="看什么  "+definition.observe;
            heading.gameObject.SetActive(!Stage.ReferenceComposition);operation.gameObject.SetActive(!Stage.ReferenceComposition);observe.gameObject.SetActive(!Stage.ReferenceComposition);
            foreach(var kv in rows)kv.Value.color=referenceShell?(kv.Key==id?Color.white:Color.clear):(kv.Key==id?RecipeUI.Hex("3B465D"):RecipeUI.Hex("272B32"));BuildReferenceInspector();
            if(rows.TryGetValue(id,out var selected)){
                float top=-selected.rectTransform.anchoredPosition.y-18;
                float current=listContent.anchoredPosition.y;
                if(top<current||top+36>current+directory.viewport.rect.height){float next=Mathf.Clamp(top-30,0,Mathf.Max(0,listContent.rect.height-directory.viewport.rect.height));listContent.anchoredPosition=new Vector2(0,next);directory.StopMovement();}
            }
        }
        public void SetReduced(bool value){Reduced=value;reduceLabel.text=value?"减弱动态 开":"减弱动态 关";Stage.SetReduced(value);}
        void CopyParameters() {
            var d=Stage.Definition;GUIUtility.systemCopyBuffer=JsonUtility.ToJson(new Export{recipeId=d.id,operation=d.operation,initialState=d.initialState,finalState=d.finalState,previewMode=d.Group==8?"DragSpring":d.id=="03.08"?(referenceScaleMode?"Scale":"Flip"):"Recipe",duration=Stage.Duration,strength=Stage.Strength,springFrequency=Stage.SpringFrequency,springDamping=Stage.SpringDamping,curve=Stage.Curve.ToString(),reducedMotion=Reduced},true);
        }
        public void GuidedOperation() {
            // One clearly named operation at a time. This invokes real scenario controls, not a second animation implementation.
            if(Stage.Definition.id=="03.08"&&referenceScaleMode){Stage.PreviewReferenceScale();return;}
            if(Stage.Actions.Count==0)return;Stage.Record("引导操作");Stage.Trigger(Stage.Actions.Keys.First());
        }
        void Update(){if(!Ready||Stage==null)return;syncing=true;timeline.SetValueWithoutNotify(Stage.Progress);syncing=false;timeline.interactable=Stage.CanSeek;timelineLabel.text=Stage.CanSeek?Mathf.RoundToInt(Stage.Progress*100)+"%":"待操作";playLabel.text=Stage.Paused?"继续":"暂停";SyncReferenceState();if(referenceDurationValue!=null)referenceDurationValue.text=Stage.Definition.Group==8?Stage.SpringFrequency.ToString("0.0")+" Hz":Stage.Duration.ToString("0.00")+" s";}
        void OnDestroy(){if(Stage!=null)Stage.Shutdown();}
    }
}
