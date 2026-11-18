using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TweenGallery.Recipes
{
    public sealed partial class RecipeStage
    {
        public bool ReferenceComposition=>RecipeArt.ApprovedAvailable&&(Definition.id=="03.08"||Definition.Group==8);
        public RectTransform ReferenceMainCard {get;private set;}
        public RectTransform ReferenceSelectedCard {get;private set;}
        public void PreviewReferenceScale(){var target=ReferenceSelectedCard??ReferenceMainCard;if(target==null)return;var from=Vector3.one;var to=Vector3.one*Strength;SetState("缩放表现 · 目标 "+Strength.ToString("0.00"));target.SetAsLastSibling();Transition(p=>target.localScale=Vector3.LerpUnclamped(from,to,Reduced?1:Ease(p)));}
        public static Image Art(Transform parent,string name,string key,float x,float y,float width,float height,bool sliced=false) {
            var rect=RecipeUI.Rect(parent,name,x,y,width,height);var image=rect.gameObject.AddComponent<Image>();RecipeArt.Apply(image,key,sliced);image.raycastTarget=false;return image;
        }
        public static RectTransform ApprovedCard(Transform parent,string title,float x,float y,float width,float height,bool back=false,bool selected=false) {
            var root=RecipeUI.Rect(parent,"Card "+title,x,y,width,height);var hit=root.gameObject.AddComponent<Image>();hit.color=Color.clear;hit.raycastTarget=false;
            var glow=Art(root,"Selection glow","approved-glow",0,0,width*1.16f,height*1.13f);glow.gameObject.SetActive(selected);
            var front=RecipeUI.Rect(root,"Front",0,0,width,height);
            // The portrait fills the entire card window. It is behind the genuinely hollow frame.
            var portrait=Art(root,"Illustration","approved-exact-portrait",0,height*.06f,width*.86f,height*.64f);portrait.preserveAspect=false;portrait.transform.SetSiblingIndex(1);
            Art(front,"Hollow ornate frame","approved-frame",0,0,width,height).preserveAspect=false;
            Art(front,"Name banner","approved-banner",0,-height*.31f,width*1.06f,height*.19f).preserveAspect=false;
            Art(front,"Rarity plaque","approved-rarity",0,-height*.427f,width*.57f,height*.105f).preserveAspect=false;
            Art(front,"Top gem","approved-gem",0,height*.455f,width*.27f,height*.10f);
            var name=RecipeUI.Label(front,title,0,-height*.315f,width*.84f,height*.14f,Mathf.RoundToInt(width*.105f));
            var rarity=RecipeUI.Label(front,"稀有",0,-height*.427f,width*.47f,height*.085f,Mathf.RoundToInt(width*.079f));
            var font=Resources.Load<Font>("GalleryHeadingFont");if(font!=null){name.font=font;rarity.font=font;}
            foreach(var t in new[]{name,rarity}){var outline=t.gameObject.AddComponent<Outline>();outline.effectColor=new Color(.03f,.03f,.16f,.95f);outline.effectDistance=new Vector2(1.3f,-1.3f);}
            var reverse=Art(root,"Back","approved-back",0,0,width,height);reverse.preserveAspect=false;reverse.gameObject.SetActive(back);front.gameObject.SetActive(!back);portrait.gameObject.SetActive(!back);
            return root;
        }
        void BuildReferenceCards() {
            if(Definition.id=="03.08")Duration=.3f;
            world.anchoredPosition=Vector2.zero;world.sizeDelta=new Vector2(1010,910);stateText.gameObject.SetActive(false);actionsRoot.gameObject.SetActive(false);
            var left=ApprovedCard(world,"星光旅者",-312,176,282,390);left.localRotation=Quaternion.Euler(0,0,3);
            Strength=1.2f;ReferenceMainCard=ApprovedCard(world,"星光旅者",27,195,317,425,false,true);ReferenceMainCard.localScale=Vector3.one*1.2f;
            var back=ApprovedCard(world,"星光旅者",346,175,246,390,true);back.localRotation=Quaternion.Euler(0,0,-4);
            var main=ReferenceMainCard;ReferenceSelectedCard=main;Put("backFace",1);
            Func<RectTransform,string> faceKey=card=>card==left?"leftFace":card==main?"mainFace":"backFace";
            Action flip=()=>{var target=ReferenceSelectedCard??main;var front=target.Find("Front").gameObject;var portrait=target.Find("Illustration").gameObject;var reverse=target.Find("Back").gameObject;bool initialBack=reverse.activeSelf;bool showBack=Get(faceKey(target))==0;Put(faceKey(target),showBack?1:0);Put("face",showBack?1:0);SetState(showBack?"背面 · 冒险罗盘":"正面 · 星光旅者");float baseline=target.localScale.y;float from=Mathf.Abs(target.localScale.x)/Mathf.Max(.01f,baseline);target.SetAsLastSibling();Transition(p=>{float t=Reduced?1:p;float scale=Reduced?1:t<.5f?Mathf.Lerp(from,.015f,t*2):Mathf.Lerp(.015f,1,(t-.5f)*2);target.localScale=new Vector3(scale*baseline,baseline,baseline);bool isBack=p<.5f?initialBack:showBack;front.SetActive(!isBack);portrait.SetActive(!isBack);reverse.SetActive(isBack);});};
            if(Definition.id=="03.08")Operation("翻面",flip);
            foreach(var card in new[]{left,main,back}){var b=card.gameObject.AddComponent<Button>();b.transition=Selectable.Transition.None;card.GetComponent<Image>().raycastTarget=true;b.onClick.AddListener(()=>{ReferenceSelectedCard=card;foreach(var c in new[]{left,main,back})c.Find("Selection glow").gameObject.SetActive(c==card);Put("selectedCard",card==left?0:card==main?1:2);Put("face",Get(faceKey(card)));if(card==main&&Definition.id=="03.08")flip();else SetState("选中卡牌");});}
            main.SetAsLastSibling();

            // Lower composition follows the approved source: ghost, source, valid and invalid slots.
            var source=Art(world,"Source slot","slot-source",-267,-280,124,124);
            var valid=Art(world,"Valid slot","slot-valid",-117,-280,126,126);
            var invalid=Art(world,"Invalid slot","slot-invalid",30,-280,126,126);
            Art(valid.transform,"Accept mark","check",0,0,70,70);Art(invalid.transform,"Reject mark","prohibit",0,0,75,75);
            var ghost=Art(world,"Ghost item","crystal",-403,-280,115,125);ghost.color=new Color(.65f,.9f,1,.23f);
            var ghostFrame=Art(world,"Ghost outline",Resources.Load<Texture2D>("RecipeArt/approved-ghost")!=null?"approved-ghost":"focus-outline",-403,-280,128,136);ghostFrame.color=new Color(.4f,.85f,1,.8f);
            var sourceLabel=Art(world,"Source label","wb-gray",-267,-367,135,40,true);RecipeUI.Label(sourceLabel.transform,"源槽",0,0,125,38,21);
            var validLabel=Art(world,"Valid label","button-green",-117,-367,139,42,true);validLabel.color=new Color(.1f,1,.1f,1);RecipeUI.Label(validLabel.transform,"可放置",0,0,125,38,21);
            var invalidLabel=Art(world,"Invalid label","button-purple",30,-367,145,42,true);invalidLabel.color=new Color(1,.1f,.1f,1);RecipeUI.Label(invalidLabel.transform,"类型不匹配",0,0,139,38,19);
            var inventory=Art(world,"Inventory","wb-inventory",318,-287,370,185,true);inventory.pixelsPerUnitMultiplier=1;
            var inventoryTitle=Art(inventory.transform,"Inventory title","wb-gray",0,93,180,37,true);RecipeUI.Label(inventoryTitle.transform,"物品背包",0,0,160,35,22);
            Art(inventory.transform,"Amber item","approved-amber",-115,0,100,104);
            var blue=Art(inventory.transform,"Blue item frame","button-square",0,0,100,104);Art(blue.transform,"Blue crystal","crystal",0,0,65,82);
            Art(inventory.transform,"Potion item","approved-potion",115,0,100,104);
            var inventoryAmber=(RectTransform)inventory.transform.Find("Amber item");var inventoryBlue=(RectTransform)inventory.transform.Find("Blue item frame");var inventoryPotion=(RectTransform)inventory.transform.Find("Potion item");bool sorting=Definition.Group==8&&Definition.Number==7,sorted=false;
            var item=Art(world,"Draggable amber","approved-amber",-252,-124,123,128).rectTransform;item.localRotation=Quaternion.Euler(0,0,9);item.GetComponent<Image>().raycastTarget=true;
            var hand=Art(world,"Guide hand","hand",-213,-173,80,75);hand.rectTransform.localRotation=Quaternion.Euler(0,0,85);
            if(Resources.Load<Texture2D>("RecipeArt/approved-pointer")!=null){var arrow=Art(world,"Drag arrow","approved-pointer",-312,-178,160,110);arrow.transform.SetSiblingIndex(item.GetSiblingIndex());}
            var pointer=item.gameObject.AddComponent<RecipePointer>();var home=source.rectTransform.anchoredPosition;Vector2 grab=Vector2.zero;bool held=false,settling=false;var spring=new MotionSpring2D(item.anchoredPosition);
            Action<bool> sortPreview=inside=>{inventoryAmber.gameObject.SetActive(!inside&&!sorted);inventoryBlue.anchoredPosition=new Vector2(inside||sorted?-115:0,0);inventoryPotion.anchoredPosition=new Vector2(inside||sorted?0:115,0);Put("sortPreview",inside?1:0);};
            Action<string> restore=reason=>{held=false;Put("held",0);if(sorting&&!sorted)sortPreview(false);spring.Reset(item.anchoredPosition);spring.Retarget(home);settling=!Reduced;if(Reduced){item.anchoredPosition=home;item.localRotation=Quaternion.identity;}item.localScale=Vector3.one;SetState(reason);};
            Action moveValid=()=>{held=true;Put("held",1);Put("hoverSlot",1);item.anchoredPosition=sorting?inventory.rectTransform.anchoredPosition+new Vector2(115,0):valid.rectTransform.anchoredPosition;if(sorting)sortPreview(true);SetState("✓ 有效区域");};
            Action drop=()=>{if(!held)return;held=false;Put("held",0);if(sorting){if(Get("sortPreview")==1){sorted=true;sortPreview(true);Put("order",1);Put("slot",2);Put("firstId",101);home=inventory.rectTransform.anchoredPosition+new Vector2(115,0);restore("排序提交：B / C / A");}else{Put("rejected",Get("rejected")+1);restore("排序拒绝 · 恢复原顺序");}return;}bool good=Vector2.Distance(item.anchoredPosition,valid.rectTransform.anchoredPosition)<90;if(good&&Definition.Number!=6){Put("slot",1);Put("drops",Get("drops")+1);home=valid.rectTransform.anchoredPosition;restore("Accepted · 已提交目标槽");}else{Put("rejected",Get("rejected")+1);restore("Rejected · 回到源槽");}};
            pointer.down=e=>{StopMotion();settling=false;held=true;hand.gameObject.SetActive(false);Put("held",1);RectTransformUtility.ScreenPointToLocalPointInRectangle(world,e.position,e.pressEventCamera,out var p);grab=p-item.anchoredPosition;item.SetAsLastSibling();if(!Reduced)item.localScale=Vector3.one*1.08f;SetState("拾起");};
            pointer.drag=e=>{RectTransformUtility.ScreenPointToLocalPointInRectangle(world,e.position,e.pressEventCamera,out var p);item.anchoredPosition=p-grab;bool good=Vector2.Distance(item.anchoredPosition,valid.rectTransform.anchoredPosition)<90;bool bad=Vector2.Distance(item.anchoredPosition,invalid.rectTransform.anchoredPosition)<90;if(sorting){bool inside=RectTransformUtility.RectangleContainsScreenPoint(inventory.rectTransform,e.position,e.pressEventCamera);sortPreview(inside);good=inside;bad=!inside;}Put("hoverSlot",good?1:bad?-1:0);SetState(good?"✓ 可放置":bad?"⊘ 类型不匹配":"拖动中");};
            pointer.up=e=>drop();pointer.end=e=>drop();pointer.cancel=()=>restore("Cancelled · 恢复源槽");
            cancelling.Add(()=>restore("Cancelled"));
            ticking.Add(()=>{if(settling){spring.Step(Time.unscaledDeltaTime,SpringFrequency,SpringDamping);item.anchoredPosition=spring.Value;item.localRotation=Quaternion.Slerp(item.localRotation,Quaternion.identity,1-Mathf.Exp(-Time.unscaledDeltaTime*15));if(spring.Settle(.03f,.03f)){item.anchoredPosition=home;item.localRotation=Quaternion.identity;settling=false;}}if(held&&Input.GetKeyDown(KeyCode.Escape)){pointer.Cancel();restore("Cancelled · Esc");}});
            if(Definition.Group==8){
                Operation("模拟拾起",()=>{held=true;Put("held",1);SetState("PickedUp");if(!Reduced)item.localScale=Vector3.one*1.08f;});Operation("移到有效槽",moveValid);
                Operation("移到无效槽",()=>{held=true;Put("held",1);Put("hoverSlot",-1);item.anchoredPosition=invalid.rectTransform.anchoredPosition;SetState("⊘ 无效区域");});
                Operation("松开放置",drop);
                Operation("取消",()=>{Put("cancelled",Get("cancelled")+1);pointer.Cancel();restore("Cancelled · 不提交放置");});
            }
        }
    }
}
