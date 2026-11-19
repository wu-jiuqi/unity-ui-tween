using System;
using System.Collections.Generic;
using UnityEngine;

namespace TweenGallery.Recipes
{
    [Serializable] public sealed class RecipeDefinition
    {
        public string id, name, layer, operation, observe, initialState, finalState, tags, boundary, reducedMotion;
        public string[] objectTags, stateTags, typeTags;
        public bool HasTag(string tag)=>Array.IndexOf(objectTags,tag)>=0||Array.IndexOf(stateTags,tag)>=0||Array.IndexOf(typeTags,tag)>=0;
        public float duration = .3f;
        public bool interactive, loop;
        public int Group => int.Parse(id.Substring(0, 2));
        public int Number => int.Parse(id.Substring(3, 2));
    }
    public static class RecipeCatalog
    {
        [Serializable] sealed class Document { public RecipeDefinition[] recipes; }
        static RecipeDefinition[] cache;
        public static RecipeDefinition[] All {
            get {
                if (cache != null) return cache;
                var asset = Resources.Load<TextAsset>("recipe-catalog");
                if (asset == null) throw new InvalidOperationException("Missing recipe-catalog.json");
                var items = JsonUtility.FromJson<Document>(asset.text).recipes;
                if (items == null || items.Length != 96) throw new InvalidOperationException("Expected exactly 96 recipes");
                var ids = new HashSet<string>();
                foreach (var r in items) {
                    if (!ids.Add(r.id) || string.IsNullOrWhiteSpace(r.operation) || string.IsNullOrWhiteSpace(r.boundary)||r.objectTags==null||r.objectTags.Length==0||r.stateTags==null||r.stateTags.Length==0||r.typeTags==null||r.typeTags.Length==0)
                        throw new InvalidOperationException("Invalid recipe " + r.id);
                }
                for (int g=1;g<=12;g++) for(int n=1;n<=8;n++)
                    if(!ids.Contains(g.ToString("00")+"."+n.ToString("00"))) throw new InvalidOperationException("Missing recipe");
                return cache = items;
            }
        }
        public static RecipeDefinition Find(string id) {
            foreach(var r in All) if(r.id==id) return r;
            throw new ArgumentException("Unknown recipe " + id);
        }
        public static readonly string[] Layers = { "界面与场景 UI", "弹窗与浮层", "容器与布局", "导航与选中", "按钮", "输入与选项", "列表与滚动", "拖拽与放置", "数值与进度", "通知与反馈", "奖励与结算", "闲置与装饰循环" };
    }
}
