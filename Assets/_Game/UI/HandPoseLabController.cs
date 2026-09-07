using Avoidance.Gameplay.Camera;
using Avoidance.Gameplay.Visuals;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Avoidance.UI
{
    [DisallowMultipleComponent]
    public sealed class HandPoseLabController : MonoBehaviour
    {
        private void Awake()
        {
            var profile = Resources.Load<FirstPersonArmProfile>(FirstPersonArmProfile.ResourceName);
            if (profile == null || !profile.HasActiveArmPrefabs)
            {
                Debug.LogError("HandPoseLab requires the production FirstPersonArmProfile.");
                return;
            }

            CreateLighting();
            CreateReferenceFloor();

            var player = new GameObject("Player Camera Root");
            var cameraObject = new GameObject("First Person Camera", typeof(UnityEngine.Camera));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(player.transform, false);
            cameraObject.transform.localPosition = new Vector3(0f, 1.62f, 0f);
            var camera = cameraObject.GetComponent<UnityEngine.Camera>();
            var cameraProfile = Resources.Load<CameraProfile>("Camera_Default")
                ?? CameraProfile.CreateRuntimeDefault();
            camera.fieldOfView = cameraProfile.BaseFieldOfView;
            camera.nearClipPlane = 0.04f;
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.cullingMask &= ~(1 << FirstPersonHands.ResolveArmLayer());

            var armCameraObject = new GameObject(FirstPersonHands.ArmCameraName, typeof(UnityEngine.Camera));
            armCameraObject.transform.SetParent(cameraObject.transform, false);
            var armCamera = armCameraObject.GetComponent<UnityEngine.Camera>();
            armCamera.fieldOfView = profile.ArmFieldOfView;
            armCamera.nearClipPlane = profile.ArmNearClipPlane;
            armCamera.farClipPlane = 4f;
            armCamera.clearFlags = CameraClearFlags.Depth;
            armCamera.cullingMask = 1 << FirstPersonHands.ResolveArmLayer();
            armCamera.depth = camera.depth + 1f;
            armCamera.useOcclusionCulling = false;
            var worldData = camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
            var armData = armCamera.gameObject.AddComponent<UniversalAdditionalCameraData>();
            worldData.renderType = CameraRenderType.Base;
            armData.renderType = CameraRenderType.Overlay;
            worldData.cameraStack.Add(armCamera);

            var visualRoot = new GameObject("FirstPersonVisualRoot").transform;
            visualRoot.SetParent(armCameraObject.transform, false);
            visualRoot.gameObject.layer = FirstPersonHands.ResolveArmLayer();
            CreateArm(
                visualRoot,
                "LeftArmAnchor",
                profile.LeftArmPrefab,
                profile.LeftBaseLocalPosition,
                profile.LeftBaseLocalRotation,
                profile.LeftPrefabEulerOffset,
                profile.PrefabLocalScale);
            CreateArm(
                visualRoot,
                "RightArmAnchor",
                profile.RightArmPrefab,
                profile.RightBaseLocalPosition,
                profile.RightBaseLocalRotation,
                profile.RightPrefabEulerOffset,
                profile.PrefabLocalScale);
        }

        private static void CreateArm(
            Transform camera,
            string name,
            GameObject prefab,
            Vector3 position,
            Quaternion rotation,
            Vector3 prefabEulerOffset,
            Vector3 scale)
        {
            var root = new GameObject(name).transform;
            root.SetParent(camera, false);
            root.localPosition = position;
            root.localRotation = rotation;
            root.gameObject.layer = FirstPersonHands.ResolveArmLayer();
            var visual = Instantiate(prefab, root);
            visual.name = name + " Visual";
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.Euler(prefabEulerOffset);
            visual.transform.localScale = scale;
            ModuleVisualPrefabLibrary.PrepareVisualInstance(visual);
            SetLayerRecursive(visual.transform, FirstPersonHands.ResolveArmLayer());
        }

        private static void SetLayerRecursive(Transform root, int layer)
        {
            root.gameObject.layer = layer;
            for (var index = 0; index < root.childCount; index++)
            {
                SetLayerRecursive(root.GetChild(index), layer);
            }
        }

        private static void CreateLighting()
        {
            var lightObject = new GameObject("Pose Lab Key Light", typeof(Light));
            lightObject.transform.rotation = Quaternion.Euler(44f, -28f, 0f);
            var light = lightObject.GetComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.5f;
            light.color = new Color(1f, 0.92f, 0.8f);
            light.shadows = LightShadows.Soft;
            RenderSettings.ambientLight = new Color(0.58f, 0.68f, 0.82f);
        }

        private static void CreateReferenceFloor()
        {
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Neutral Reference Floor";
            floor.transform.position = new Vector3(0f, -0.1f, 7f);
            floor.transform.localScale = new Vector3(14f, 0.2f, 18f);
            var renderer = floor.GetComponent<Renderer>();
            renderer.sharedMaterial = VisualMaterialUtility.CreateRuntimeMaterial(
                "Pose Lab Floor",
                new Color(0.28f, 0.32f, 0.38f));
            renderer.shadowCastingMode = ShadowCastingMode.On;

            for (var index = -6; index <= 6; index++)
            {
                CreateGridLine(new Vector3(index, 0.015f, 7f), new Vector3(0.018f, 0.018f, 18f));
            }

            for (var index = -1; index <= 15; index++)
            {
                CreateGridLine(new Vector3(0f, 0.016f, index), new Vector3(14f, 0.018f, 0.018f));
            }
        }

        private static void CreateGridLine(Vector3 position, Vector3 scale)
        {
            var line = GameObject.CreatePrimitive(PrimitiveType.Cube);
            line.name = "Grid Line";
            line.transform.position = position;
            line.transform.localScale = scale;
            line.GetComponent<Renderer>().sharedMaterial = VisualMaterialUtility.CreateRuntimeMaterial(
                "Pose Lab Grid",
                new Color(0.18f, 0.7f, 0.9f));
            Destroy(line.GetComponent<Collider>());
        }
    }
}
