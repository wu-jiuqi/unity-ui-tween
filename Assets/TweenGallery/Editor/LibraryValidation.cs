using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace TweenGallery.Editor
{
    public static class LibraryValidation
    {
        public static void Run()
        {
            var checks=new List<string>();
            foreach(var definition in MotionCatalog.All)
            {
                string path="Assets/TweenGallery/Prefabs/"+definition.id+".prefab";
                var prefab=PrefabUtility.LoadPrefabContents(path);
                try
                {
                    var player=prefab.GetComponentInChildren<MotionPlayer>(true);if(player==null)throw new Exception(path+" has no player");
                    var preset=AssetDatabase.LoadAssetAtPath<MotionPreset>("Assets/TweenGallery/Presets/"+definition.id+".asset");
                    if(preset==null||Mathf.Abs(preset.settings.duration-definition.duration)>.0001f||Mathf.Abs(player.settings.duration-definition.duration)>.0001f||Mathf.Abs(player.settings.strength-definition.strength)>.0001f||Mathf.Abs(player.settings.distance-definition.distance)>.0001f||Mathf.Abs(player.settings.stagger-definition.stagger)>.0001f||Mathf.Abs(player.settings.overshoot-definition.overshoot)>.0001f)
                        throw new Exception(definition.id+" catalog / preset / prefab drift");
                    checks.Add(definition.id+" actual prefab and preset use retuned catalog values");
                    foreach(var text in prefab.GetComponentsInChildren<Text>(true))if(text.color.r>.97f&&text.color.g>.97f&&text.color.b>.97f)throw new Exception(definition.id+" still uses white text on pastel surfaces");
                    checks.Add(definition.id+" serialized text uses warm readable ink");
                    var target=player.bindings.target;var scale=target.localScale;var position=target.anchoredPosition;player.CaptureBaseline();
                    foreach(float time in new[]{0,.2f,.5f,.8f,1}){player.Sample(time);if(float.IsNaN(target.localScale.x)||float.IsNaN(target.anchoredPosition.x))throw new Exception(definition.id+" nonfinite pose");}
                    player.StopAndRestore();if(Vector3.Distance(scale,target.localScale)>.0001f||Vector2.Distance(position,target.anchoredPosition)>.0001f)throw new Exception(definition.id+" restore drift");
                    checks.Add(definition.id+" prefab samples and restores authored baseline");
                }
                finally{PrefabUtility.UnloadPrefabContents(prefab);}
            }
            Directory.CreateDirectory("evidence");File.WriteAllText("evidence/library-validation.txt","PASS\n"+string.Join("\n",checks));Debug.Log("LIBRARY_VALIDATION PASS "+checks.Count);
        }
    }
}
