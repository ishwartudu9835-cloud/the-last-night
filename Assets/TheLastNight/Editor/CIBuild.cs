// Headless (cloud) build entry point used by GitHub Actions / GameCI:
//   -executeMethod TheLastNight.EditorTools.CIBuild.BuildAndroid
// 1. makes sure URP is set up (creates a URP asset if the project has none)
// 2. builds the vertical-slice scene with SliceSceneBuilder
// 3. applies Android player settings and builds the APK
// Also runnable from the menu: The Last Night > 3. Build Android APK (CI pipeline)
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace TheLastNight.EditorTools
{
    public static class CIBuild
    {
        const string UrpDir = "Assets/TheLastNight/Settings";
        const string UrpPackage = "Packages/com.unity.render-pipelines.universal";

        [MenuItem("The Last Night/3. Build Android APK (CI pipeline)", priority = 3)]
        public static void BuildAndroidMenu() { Run(false); }

        /// <summary>GameCI buildMethod.</summary>
        public static void BuildAndroid() { Run(true); }

        static void Run(bool exitWhenDone)
        {
            int code = 0;
            try
            {
                Log("Step 1/4: URP setup");
                EnsureUrp();
                Log("Step 2/4: building vertical slice scene");
                SliceSceneBuilder.BuildScene(false);
                Log("Step 3/4: Android player settings");
                SliceSceneBuilder.ApplyAndroidSettings(false);
                EditorUserBuildSettings.buildAppBundle = false;
                Log("Step 4/4: building APK");
                string path = OutputPath();
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                var options = new BuildPlayerOptions
                {
                    scenes = new[] { SliceSceneBuilder.ScenePath },
                    locationPathName = path,
                    target = BuildTarget.Android,
                    targetGroup = BuildTargetGroup.Android,
                    options = BuildOptions.None,
                };
                BuildReport report = BuildPipeline.BuildPlayer(options);
                var s = report.summary;
                Log(string.Format("Result: {0}  size: {1:0.0} MB  time: {2}  errors: {3}", s.result, s.totalSize / 1048576.0, s.totalTime, s.totalErrors));
                if (s.result != BuildResult.Succeeded) code = 1;
                else Log("APK: " + path);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                code = 2;
            }
            if (exitWhenDone) EditorApplication.Exit(code);
        }

        /// <summary>GameCI passes -customBuildPath; otherwise build/Android/TheLastNight.apk.</summary>
        static string OutputPath()
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
                if (args[i] == "-customBuildPath")
                {
                    string p = args[i + 1];
                    return p.EndsWith(".apk") ? p : Path.Combine(p, "TheLastNight.apk");
                }
            return "build/Android/TheLastNight.apk";
        }

        /// <summary>Creates and activates a mobile-friendly URP asset if none is active.</summary>
        public static void EnsureUrp()
        {
            if (GraphicsSettings.defaultRenderPipeline is UniversalRenderPipelineAsset) { Log("URP already active"); return; }
            if (!AssetDatabase.IsValidFolder(UrpDir)) AssetDatabase.CreateFolder("Assets/TheLastNight", "Settings");

            var renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
            renderer.postProcessData = AssetDatabase.LoadAssetAtPath<PostProcessData>(UrpPackage + "/Runtime/Data/PostProcessData.asset");
            AssetDatabase.CreateAsset(renderer, UrpDir + "/TLN_Mobile_Renderer.asset");

            var asset = UniversalRenderPipelineAsset.Create(renderer);
            asset.renderScale = 0.9f;
            asset.msaaSampleCount = 1;
            asset.shadowDistance = 25f;
            AssetDatabase.CreateAsset(asset, UrpDir + "/TLN_Mobile_URP.asset");
            AssetDatabase.SaveAssets();

            GraphicsSettings.defaultRenderPipeline = asset;
            // make every quality level use it too (some projects override per level)
            int current = QualitySettings.GetQualityLevel();
            for (int i = 0; i < QualitySettings.names.Length; i++)
            {
                QualitySettings.SetQualityLevel(i, false);
                QualitySettings.renderPipeline = asset;
            }
            QualitySettings.SetQualityLevel(current, false);
            AssetDatabase.SaveAssets();
            Log("Created and activated " + UrpDir + "/TLN_Mobile_URP.asset");
        }

        static void Log(string m) { Debug.Log("[TLN-CI] " + m); }
    }
}
