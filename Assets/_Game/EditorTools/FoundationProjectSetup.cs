using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Avoidance.EditorTools
{
    public static class FoundationProjectSetup
    {
        private const string RenderPipelinePath =
            "Assets/_Game/Visuals/Settings/FoundationUniversalRenderPipeline.asset";
        private const string RendererPath =
            "Assets/_Game/Visuals/Settings/FoundationUniversalRenderer.asset";

        [InitializeOnLoadMethod]
        private static void ScheduleSetup()
        {
            EditorApplication.delayCall += Apply;
        }

        [MenuItem("Avoidance/Apply Foundation Project Settings")]
        public static void Apply()
        {
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.bundleVersion = "0.0.1-foundation";
            PlayerSettings.SetApplicationIdentifier(
                NamedBuildTarget.Android,
                "com.avoidancestudio.avoidance");
            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;

            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(
                RenderPipelinePath);
            var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererPath);
            if (renderer == null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(RendererPath));
                renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(renderer, RendererPath);
            }

            if (pipeline == null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(RenderPipelinePath));
                pipeline = UniversalRenderPipelineAsset.Create(renderer);
                AssetDatabase.CreateAsset(pipeline, RenderPipelinePath);
            }

            var serializedPipeline = new SerializedObject(pipeline);
            var rendererList = serializedPipeline.FindProperty("m_RendererDataList");
            if (rendererList != null
                && (rendererList.arraySize != 1
                    || rendererList.GetArrayElementAtIndex(0).objectReferenceValue != renderer))
            {
                rendererList.arraySize = 1;
                rendererList.GetArrayElementAtIndex(0).objectReferenceValue = renderer;
                var defaultRenderer = serializedPipeline.FindProperty("m_DefaultRendererIndex");
                if (defaultRenderer != null)
                {
                    defaultRenderer.intValue = 0;
                }

                serializedPipeline.ApplyModifiedPropertiesWithoutUndo();
            }

            if (GraphicsSettings.defaultRenderPipeline != pipeline)
            {
                GraphicsSettings.defaultRenderPipeline = pipeline;
            }

            if (QualitySettings.renderPipeline != pipeline)
            {
                QualitySettings.renderPipeline = pipeline;
            }

            AssetDatabase.SaveAssets();
        }
    }
}
