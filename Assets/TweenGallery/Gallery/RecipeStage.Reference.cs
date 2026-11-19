using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TweenGallery.Recipes
{
    public sealed partial class RecipeStage
    {
        public string ReferencePreviewMode="";
        public bool ReferenceComposition=>RecipeArt.ApprovedAvailable&&(Definition.id=="03.08"||Definition.Group==8);
        public RectTransform ReferenceMainCard {get;private set;}
        public RectTransform ReferenceSelectedCard {get;private set;}
        public RectTransform ReferenceDragItem {get;private set;}
        public RectTransform ReferenceValidSlot {get;private set;}
        public RectTransform ReferenceInvalidSlot {get;private set;}
        public static Image Art(Transform parent,string name,string key,float x,float y,float width,float height,bool sliced=false) {
            var rect=RecipeUI.Rect(parent,name,x,y,width,height);var image=rect.gameObject.AddComponent<Image>();RecipeArt.Apply(image,key,sliced);image.raycastTarget=false;return image;
        }
        public static RectTransform ApprovedCard(Transform parent,string title,float x,float y,float width,float height,bool back=false,bool selected=false) {
            var root=RecipeUI.Rect(parent,"Card "+title,x,y,width,height);var hit=root.gameObject.AddComponent<Image>();hit.color=new Color(1,1,1,.01f);hit.raycastTarget=false;
            var glow=Art(root,"Selection glow","approved-glow",0,0,width*1.10f,height*1.07f);glow.gameObject.SetActive(selected);
            var front=RecipeUI.Rect(root,"Front",0,0,width,height);
            // The portrait fills the entire card window. It is behind the genuinely hollow frame.
            var portrait=Art(root,"Illustration","approved-exact-portrait",0,height*.06f,width*.86f,height*.64f);portrait.preserveAspect=false;portrait.transform.SetSiblingIndex(1);
            Art(front,"Hollow ornate frame","approved-frame",0,height*.025f,width,height*.95f).preserveAspect=false;
            Art(front,"Name banner","approved-banner",0,-height*.31f,width*1.06f,height*.19f).preserveAspect=false;
            Art(front,"Rarity plaque","approved-rarity",0,-height*.427f,width*.57f,height*.105f).preserveAspect=false;
            Art(front,"Top gem","approved-gem",0,height*.455f,width*.32f,height*.16f);
            var name=RecipeUI.Label(front,title,0,-height*.315f,width*.84f,height*.14f,Mathf.RoundToInt(width*.105f));
            var rarity=RecipeUI.Label(front,"稀有",0,-height*.427f,width*.47f,height*.12f,Mathf.RoundToInt(width*.079f));
            var font=Resources.Load<Font>("GalleryHeadingFont");if(font!=null){name.font=font;rarity.font=font;}
            foreach(var t in new[]{name,rarity}){var outline=t.gameObject.AddComponent<Outline>();outline.effectColor=new Color(.03f,.03f,.16f,.95f);outline.effectDistance=new Vector2(1.3f,-1.3f);}
            var reverse=Art(root,"Back","approved-back",0,0,width,height);reverse.preserveAspect=false;reverse.gameObject.SetActive(back);front.gameObject.SetActive(!back);portrait.gameObject.SetActive(!back);
            return root;
        }
        void BuildReferenceCards(){if(Definition.Group==8)BuildFocusedDrag();else BuildFocusedCard();}
        void BuildFocusedCard(){
            var card=ApprovedCard(world,"星光旅者",-110,5,302,410,false,false);PrimarySubject=ReferenceMainCard=ReferenceSelectedCard=card;
            var note=RecipeUI.Rect(world,"Card explanation",260,0,235,270);
            RecipeUI.Label(note,"点击卡牌翻面",0,94,235,40,25);
            RecipeUI.Label(note,"正面 · 角色与稀有度\n\n背面 · 冒险罗盘\n\n观察侧面切换，文字保持正向",0,-32,235,205,21,RecipeUI.Muted,TextAnchor.UpperLeft);
            Action flip=()=>{bool before=card.Find("Back").gameObject.activeSelf;bool next=Get("face")==0;Put("face",next?1:0);SetState(next?"背面 · 冒险罗盘":"正面 · 星光旅者");var front=card.Find("Front").gameObject;var art=card.Find("Illustration").gameObject;var back=card.Find("Back").gameObject;float start=Mathf.Abs(card.localScale.x);Transition(p=>{float t=Reduced?1:Mathf.Clamp01(Ease(p));float x=Reduced?1:t<.5f?Mathf.Lerp(start,.015f,t*2):Mathf.Lerp(.015f,1,(t-.5f)*2);card.localScale=new Vector3(x,1,1);bool reverse=t<.5f?before:next;front.SetActive(!reverse);art.SetActive(!reverse);back.SetActive(reverse);});};
            var b=card.gameObject.AddComponent<Button>();b.transition=Selectable.Transition.None;card.GetComponent<Image>().raycastTarget=true;b.onClick.AddListener(()=>{Record("点击卡牌");flip();});Operation("翻面",flip);
        }
        public void PreviewReferenceScale(){if(ReferenceMainCard==null)return;var card=ReferenceMainCard;Transition(p=>card.localScale=Vector3.one*Mathf.LerpUnclamped(1,Strength,Reduced?1:Ease(p)));}
        public void PreviewReferenceEffect(string mode){if(mode=="flip")Trigger("翻面");else PreviewReferenceScale();}
        void BuildFocusedDrag(){
            bool sorting=Definition.Number==7;Put("itemKind",1);SetState(sorting?"原顺序 A → B → C":"源槽待拾起");
            float left=-285,right=285,y=25;int n=Definition.Number;
            var source=Art(world,"Source slot","slot-source",left,y,200,200);
            var valid=Art(world,"Valid slot",sorting?"slot-source":"slot-valid",0,y,200,200);
            var invalid=Art(world,"Invalid slot",sorting?"slot-source":"slot-invalid",right,y,200,200);
            ReferenceValidSlot=sorting?invalid.rectTransform:valid.rectTransform;ReferenceInvalidSlot=source.rectTransform;
            if(!sorting){ReferenceInvalidSlot=invalid.rectTransform;Art(valid.transform,"Accept mark","check",0,0,100,100);Art(invalid.transform,"Reject mark","prohibit",0,0,112,112);}
            RecipeUI.Label(world,sorting?"位置 1":"源槽",left,174,250,45,24);
            RecipeUI.Label(world,sorting?"位置 2":"水晶槽 · 可放置",0,174,250,45,24,sorting?RecipeUI.Ink:RecipeUI.Green);
            RecipeUI.Label(world,sorting?"位置 3":"类型不匹配",right,174,250,45,24,sorting?RecipeUI.Ink:RecipeUI.Pink);
            var item=Art(world,"Draggable amber","approved-amber",left,y,162,177).rectTransform;item.GetComponent<Image>().raycastTarget=true;ReferenceDragItem=PrimarySubject=item;
            var blueOverlay=Art(item,"Selected blue crystal","crystal",0,0,96,125);blueOverlay.gameObject.SetActive(false);
            var ghost=Art(world,"Pickup ghost","approved-ghost",left,y,175,185);ghost.color=new Color(.4f,.9f,1,.4f);ghost.transform.SetSiblingIndex(item.GetSiblingIndex());ghost.gameObject.SetActive(false);
            var pointer=item.gameObject.AddComponent<RecipePointer>();Vector2 home=item.anchoredPosition,grab=Vector2.zero;bool held=false,settling=false,sorted=false;var spring=new MotionSpring2D(home);
            RectTransform neighborB=null,neighborC=null;var springB=new MotionSpring2D(new Vector2(0,y));var springC=new MotionSpring2D(new Vector2(right,y));
            if(sorting){RecipeUI.Label(item,"A · ID 101",0,-125,195,34,18);neighborB=Art(world,"Sortable item B ID102","button-square",0,y,160,175).rectTransform;Art(neighborB,"Blue crystal","crystal",0,0,96,125);neighborC=Art(world,"Sortable item C ID103","approved-potion",right,y,160,175).rectTransform;RecipeUI.Label(neighborB,"B · ID 102",0,-125,195,34,18);RecipeUI.Label(neighborC,"C · ID 103",0,-125,195,34,18);RecipeUI.Label(world,"向右拖动 A，观察 B、C 让位；松开后提交新顺序",0,-167,830,62,23,RecipeUI.Muted);}
            else {
                RecipeUI.Label(world,"拖到带勾槽后松开 → 接受\n拖到禁止槽或放入药水 → 拒绝\nEsc 取消 → 回到拾起前的槽位",-207,-183,505,115,21,RecipeUI.Muted,TextAnchor.MiddleLeft);
                var palette=RecipeUI.Rect(world,"Inventory palette",275,-180,320,130);RecipeUI.Label(palette,"背包 · 可直接拖出",0,72,320,30,18,RecipeUI.Muted);
                string[] keys={"approved-amber","button-square","approved-potion"};
                for(int i=0;i<3;i++){int kind=i+1;var icon=Art(palette,kind==3?"Potion item":kind==2?"Blue item":"Amber item",keys[i],-105+i*105,0,88,98);if(kind==2)Art(icon.transform,"Blue crystal","crystal",0,0,55,72);icon.raycastTarget=true;var proxy=icon.gameObject.AddComponent<RecipePointer>();
                    proxy.down=e=>{pointer.Cancel();Put("itemKind",kind);RecipeArt.Apply(item.GetComponent<Image>(),keys[kind-1],false);blueOverlay.gameObject.SetActive(kind==2);home=world.InverseTransformPoint(icon.transform.position);item.anchoredPosition=home;pointer.down(e);};proxy.drag=e=>pointer.drag(e);proxy.up=e=>pointer.up(e);proxy.cancel=()=>pointer.cancel();
                }
            }
            Action<bool> sortPreview=after=>{if(!sorting)return;springB.Retarget(new Vector2(after?left:0,y));springC.Retarget(new Vector2(after?0:right,y));Put("sortPreview",after?1:0);};
            Action<string> restore=reason=>{held=false;Put("held",0);ghost.gameObject.SetActive(false);valid.transform.localScale=invalid.transform.localScale=Vector3.one;if(sorting&&!sorted)sortPreview(false);spring.Reset(item.anchoredPosition);spring.Retarget(home);settling=!Reduced;if(Reduced)item.anchoredPosition=home;item.localScale=Vector3.one;SetState(reason);};
            Action drop=()=>{if(!held)return;if(sorting){if(Get("sortPreview")==1){sorted=true;Put("order",1);Put("firstId",102);Put("slot",2);home=new Vector2(right,y);sortPreview(true);restore("排序提交 · B → C → A，ID 不变");}else{Put("rejected",Get("rejected")+1);restore("Rejected · 恢复原顺序");}return;}
                bool good=Get("itemKind")!=3&&Vector2.Distance(item.anchoredPosition,valid.rectTransform.anchoredPosition)<95;
                if(good&&n!=6){Put("drops",Get("drops")+1);Put("slot",1);home=valid.rectTransform.anchoredPosition;restore("Accepted · 物品已归入水晶槽");}else{Put("rejected",Get("rejected")+1);restore("Rejected · 不提交，回到来源");}};
            pointer.down=e=>{StopMotion();settling=false;held=true;Put("held",1);ghost.rectTransform.anchoredPosition=home;ghost.gameObject.SetActive(true);RectTransformUtility.ScreenPointToLocalPointInRectangle(world,e.position,e.pressEventCamera,out var p);grab=p-item.anchoredPosition;item.SetAsLastSibling();item.localScale=Vector3.one*(Reduced?1:1.08f);SetState("PickedUp · 保留抓取偏移");};
            pointer.drag=e=>{if(!held)return;RectTransformUtility.ScreenPointToLocalPointInRectangle(world,e.position,e.pressEventCamera,out var p);item.anchoredPosition=p-grab;bool good=Vector2.Distance(item.anchoredPosition,valid.rectTransform.anchoredPosition)<95&&Get("itemKind")!=3;bool bad=Vector2.Distance(item.anchoredPosition,invalid.rectTransform.anchoredPosition)<95||Get("itemKind")==3&&Vector2.Distance(item.anchoredPosition,valid.rectTransform.anchoredPosition)<95;
                if(sorting){good=item.anchoredPosition.x>100;bad=!good;sortPreview(good);}
                Put("hoverSlot",good?1:bad?-1:0);valid.transform.localScale=Vector3.one*(good&&!Reduced?1.08f:1);invalid.transform.localScale=Vector3.one*(bad&&!Reduced?1.08f:1);SetState(good?"有效区域 · 松开确认":bad?"无效区域 · 松开会回弹":"拖动中 · 物品跟随指针");};
            pointer.up=e=>drop();pointer.end=e=>drop();pointer.cancel=()=>restore("Cancelled · 回到来源");
            cancelling.Add(()=>{if(held)restore("Cancelled · 操作取消");});
            ticking.Add(()=>{
                if(settling){if(Reduced){item.anchoredPosition=home;settling=false;}else{spring.Step(Time.unscaledDeltaTime,SpringFrequency,SpringDamping);item.anchoredPosition=spring.Value;if(spring.Settle(.03f,.03f)){item.anchoredPosition=home;settling=false;}}}
                if(sorting){if(Reduced){neighborB.anchoredPosition=springB.Target;neighborC.anchoredPosition=springC.Target;}else{springB.Step(Time.unscaledDeltaTime,6,.8f);springC.Step(Time.unscaledDeltaTime,6,.8f);neighborB.anchoredPosition=springB.Value;neighborC.anchoredPosition=springC.Value;}}
                if(held&&Input.GetKeyDown(KeyCode.Escape)){Put("cancelled",Get("cancelled")+1);pointer.Cancel();restore("Cancelled · Esc 取消");}
            });
            Operation("模拟拾起",()=>{held=true;settling=false;Put("held",1);ghost.gameObject.SetActive(true);item.localScale=Vector3.one*(Reduced?1:1.08f);SetState("PickedUp");});
            Operation("移到有效槽",()=>{held=true;Put("held",1);Put("hoverSlot",1);item.anchoredPosition=ReferenceValidSlot.anchoredPosition;if(sorting)sortPreview(true);SetState("有效区域 · 松开确认");});
            Operation("移到无效槽",()=>{held=true;Put("held",1);Put("hoverSlot",-1);item.anchoredPosition=sorting?new Vector2(-70,y):invalid.rectTransform.anchoredPosition;if(sorting)sortPreview(false);SetState("无效区域 · 松开会回弹");});
            Operation("松开放置",drop);Operation("取消",()=>{Put("cancelled",Get("cancelled")+1);pointer.Cancel();restore("Cancelled · 不提交放置");});
        }
    }
}
