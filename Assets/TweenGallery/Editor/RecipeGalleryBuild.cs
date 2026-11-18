using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using TweenGallery.Recipes;

namespace TweenGallery.Editor
{
    public static class RecipeGalleryBuild
    {
        const string Scene="Assets/TweenGallery/Scenes/RecipeGallery.unity";
        [MenuItem("Tools/UI Motion Gallery/96 recipes/Prepare new gallery")]
        public static void Prepare() {
            AssetDatabase.Refresh();var definitions=RecipeCatalog.All;if(definitions.Length!=96)throw new Exception("Recipe catalog invalid");
            if(!RecipeArt.Available)throw new Exception("Reference-derived imagegen art must be registered before building");
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var root=new GameObject("96 Recipe Gallery");root.AddComponent<RecipeGalleryBootstrap>();root.AddComponent<RecipeGalleryTest>();
            var cam=new GameObject("Camera").AddComponent<Camera>();cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.08f,.09f,.11f);cam.orthographic=true;
            EditorSceneManager.SaveScene(scene,Scene);
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(Scene,true),new EditorBuildSettingsScene("Assets/TweenGallery/Scenes/Gallery.unity",true)};
            PlayerSettings.bundleVersion="0.7.1-dev";PlayerSettings.productName="UI Recipe Gallery";PlayerSettings.defaultScreenWidth=1600;PlayerSettings.defaultScreenHeight=1000;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.resizableWindow=true;PlayerSettings.runInBackground=true;
            AssetDatabase.SaveAssets();Debug.Log("RECIPE_PREPARE_OK 96");
        }
        [MenuItem("Tools/UI Motion Gallery/96 recipes/Build Windows")]
        public static void BuildWindows() {
            Prepare();Directory.CreateDirectory("Builds/Recipes96");Directory.CreateDirectory("evidence/recipes96");
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{Scene,"Assets/TweenGallery/Scenes/Gallery.unity"},locationPathName="Builds/Recipes96/UIRecipeGallery.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
            File.WriteAllText("evidence/recipes96/build.json",JsonUtility.ToJson(new Result{unity=Application.unityVersion,result=report.summary.result.ToString(),errors=(int)report.summary.totalErrors,warnings=(int)report.summary.totalWarnings},true));
            if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Recipe build failed");
        }
        [Serializable] class Result {public string unity,result;public int errors,warnings;}
    }
}
