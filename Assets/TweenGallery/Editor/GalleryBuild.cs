using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace TweenGallery.Editor
{
    public static class GalleryBuild
    {
        public const string Version = "0.6.0-preview2";
        const string Root = "Assets/TweenGallery";

        [MenuItem("Tools/UI Motion Gallery/Prepare scene and templates")]
        public static void Prepare()
        {
            Directory.CreateDirectory("evidence");
            Directory.CreateDirectory(Root + "/Scenes");
            Directory.CreateDirectory(Root + "/Presets");
            Directory.CreateDirectory(Root + "/Prefabs");
            Directory.CreateDirectory(Root + "/GeneratedArt");
            AssetDatabase.Refresh();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("UI Motion Gallery");
            root.AddComponent<GalleryBootstrap>();
            root.AddComponent<GallerySelfTest>();
            root.AddComponent<UpgradeSelfTest>();
            root.AddComponent<GalleryEnhancementSelfTest>();
            root.AddComponent<ButtonStyleSelfTest>();
            root.AddComponent<LibraryReview>();
            root.AddComponent<WorkbenchSelfTest>();
            root.AddComponent<PresentationSelfTest>();
            root.AddComponent<TypographySelfTest>();
            var camera = new GameObject("Camera").AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.045f, 0.055f, 0.11f);
            camera.orthographic = true;
            EditorSceneManager.SaveScene(scene, Root + "/Scenes/Gallery.unity");
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(Root + "/Scenes/Gallery.unity", true) };

            var preview = new GameObject("Template canvas", typeof(RectTransform), typeof(Canvas));
            preview.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            foreach (var effect in MotionCatalog.All)
            {
                var presetPath = Root + "/Presets/" + effect.id + ".asset";
                var preset = AssetDatabase.LoadAssetAtPath<MotionPreset>(presetPath);
                if (preset == null) { preset = ScriptableObject.CreateInstance<MotionPreset>(); AssetDatabase.CreateAsset(preset, presetPath); }
                preset.effectId = effect.id;
                preset.settings = MotionSettings.FromDefinition(effect);
                EditorUtility.SetDirty(preset);
                // Existing base prefabs are part of the public API: preserve their
                // serialized child fileIDs so downstream overrides remain valid.
                string prefabPath = Root + "/Prefabs/" + effect.id + ".prefab";
                if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
                {
                    var contents=PrefabUtility.LoadPrefabContents(prefabPath);
                    try
                    {
                        GalleryTheme.Apply(contents);
                        var existingPlayer=contents.GetComponentInChildren<MotionPlayer>(true);
                        if(existingPlayer!=null){existingPlayer.settings=MotionSettings.FromDefinition(effect);GalleryTheme.StylePrimaryButton(existingPlayer);}
                        PersistArt(contents,effect.id);
                        PrefabUtility.SaveAsPrefabAsset(contents,prefabPath);
                    }
                    finally { PrefabUtility.UnloadPrefabContents(contents); }
                    continue;
                }
                var player = DemoFactory.Create(effect.id, preview.transform, new Vector2(600, 420));
                var prefabRoot = player.transform;
                while (prefabRoot.parent != preview.transform && prefabRoot.parent != null) prefabRoot = prefabRoot.parent;
                PersistArt(prefabRoot.gameObject, effect.id);
                PrefabUtility.SaveAsPrefabAsset(prefabRoot.gameObject, prefabPath);
                UnityEngine.Object.DestroyImmediate(prefabRoot.gameObject);
            }
            UnityEngine.Object.DestroyImmediate(preview);

            PlayerSettings.companyName = "UI Motion Gallery";
            PlayerSettings.productName = "UI Motion Gallery";
            PlayerSettings.bundleVersion = Version;
            PlayerSettings.defaultScreenWidth = 1600;
            PlayerSettings.defaultScreenHeight = 900;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.runInBackground = true;
            PlayerSettings.colorSpace = ColorSpace.Gamma;
            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
            PlayerSettings.SetApiCompatibilityLevel(UnityEditor.Build.NamedBuildTarget.Standalone, ApiCompatibilityLevel.NET_Standard);
            PlayerSettings.SetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.Standalone, "DOTWEEN;DOTUI");
            // Built-in Input Manager is used by StandaloneInputModule and gallery shortcuts.
            var settings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            var input = settings.FindProperty("activeInputHandler");
            if (input != null) input.intValue = 0;
            settings.ApplyModifiedPropertiesWithoutUndo();
            foreach (var guid in AssetDatabase.FindAssets("t:Shader", new[] { Root }))
                IncludeShader(AssetDatabase.LoadAssetAtPath<Shader>(AssetDatabase.GUIDToAssetPath(guid)));
            AssetDatabase.SaveAssets();
            File.WriteAllText("evidence/preparation.json", JsonUtility.ToJson(new Preparation { unity = Application.unityVersion, effects = MotionCatalog.All.Length, version = Version }, true));
            Debug.Log("GALLERY_PREPARE_OK " + MotionCatalog.All.Length);
        }

        static void PersistArt(GameObject root, string id)
        {
            foreach (var image in root.GetComponentsInChildren<Image>(true))
            {
                Sprite sprite = image.sprite;
                if (sprite == null || AssetDatabase.Contains(sprite)) continue;
                string safe = string.Concat(sprite.name.Select(c => char.IsLetterOrDigit(c) || c == '_' ? c : '_'));
                string spritePath = Root + "/GeneratedArt/" + safe + "_Sprite.asset";
                var existing = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                if (existing != null) { image.sprite = existing; continue; }
                var texture = sprite.texture;
                if (!AssetDatabase.Contains(texture))
                {
                    texture.hideFlags = HideFlags.None;
                    AssetDatabase.CreateAsset(texture, AssetDatabase.GenerateUniqueAssetPath(Root + "/GeneratedArt/" + safe + "_Texture.asset"));
                }
                sprite.hideFlags = HideFlags.None;
                AssetDatabase.CreateAsset(sprite, spritePath);
            }
            foreach (var graphic in root.GetComponentsInChildren<Graphic>(true))
            {
                Material material = graphic.material;
                if (material == null || material == graphic.defaultMaterial || AssetDatabase.Contains(material)) continue;
                material.hideFlags = HideFlags.None;
                AssetDatabase.CreateAsset(material, AssetDatabase.GenerateUniqueAssetPath(Root + "/GeneratedArt/" + id + "_Material.asset"));
            }
        }

        static void IncludeShader(Shader shader)
        {
            if (shader == null) return;
            var graphics = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0]);
            var list = graphics.FindProperty("m_AlwaysIncludedShaders");
            for (int i = 0; i < list.arraySize; i++) if (list.GetArrayElementAtIndex(i).objectReferenceValue == shader) return;
            int at = list.arraySize; list.InsertArrayElementAtIndex(at); list.GetArrayElementAtIndex(at).objectReferenceValue = shader;
            graphics.ApplyModifiedPropertiesWithoutUndo();
        }

        [MenuItem("Tools/UI Motion Gallery/Build Windows and export templates")]
        public static void BuildWindows()
        {
            Prepare();
            Directory.CreateDirectory("Builds/Windows");
            Directory.CreateDirectory("dist");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = EditorBuildSettings.scenes.Select(s => s.path).ToArray(),
                locationPathName = "Builds/Windows/UIMotionGallery.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            });
            File.WriteAllText("evidence/build-result.json", JsonUtility.ToJson(new BuildEvidence {
                unity = Application.unityVersion, version = Version, result = report.summary.result.ToString(),
                bytes = report.summary.totalSize.ToString(), errors = (int)report.summary.totalErrors, warnings = (int)report.summary.totalWarnings
            }, true));
            if (report.summary.result != BuildResult.Succeeded) throw new Exception("Windows build failed: " + report.summary.result);
            AssetDatabase.ExportPackage(new[] { Root }, "dist/UnityUITween-Templates-" + Version + ".unitypackage", ExportPackageOptions.Recurse);
            Debug.Log("GALLERY_BUILD_OK " + report.summary.totalSize);
        }
        [Serializable] class Preparation { public string unity, version; public int effects; }
        [Serializable] class BuildEvidence { public string unity, version, result, bytes; public int errors, warnings; }
    }
}
