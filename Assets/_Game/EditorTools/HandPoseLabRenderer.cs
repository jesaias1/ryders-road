using System.Collections.Generic;
using System.IO;
using System.Text;
using Avoidance.Gameplay.Visuals;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Avoidance.EditorTools
{
    public static class HandPoseLabRenderer
    {
        private const string OutputDirectory = "Logs/HandPoseLab";
        private const int RenderWidth = 2340;
        private const int RenderHeight = 1080;

        public static void RenderCandidates()
        {
            var profile = Resources.Load<FirstPersonArmProfile>(FirstPersonArmProfile.ResourceName);
            if (profile == null || !profile.HasActiveArmPrefabs)
            {
                throw new MissingReferenceException("Production FirstPersonArmProfile is unavailable.");
            }

            Directory.CreateDirectory(OutputDirectory);
            var candidates = new Dictionary<string, (Quaternion Left, Quaternion Right)>
            {
                { "00_current", (Quaternion.identity, Quaternion.identity) },
                { "01_inward_roll_60", (Quaternion.Euler(0f, 60f, 0f), Quaternion.Euler(0f, -60f, 0f)) },
                { "02_inward_roll_75", (Quaternion.Euler(0f, 75f, 0f), Quaternion.Euler(0f, -75f, 0f)) },
                { "03_inward_roll_90", (Quaternion.Euler(0f, 90f, 0f), Quaternion.Euler(0f, -90f, 0f)) },
                { "04_opposite_roll_60", (Quaternion.Euler(0f, -60f, 0f), Quaternion.Euler(0f, 60f, 0f)) },
                { "05_opposite_roll_75", (Quaternion.Euler(0f, -75f, 0f), Quaternion.Euler(0f, 75f, 0f)) },
                { "06_opposite_roll_90", (Quaternion.Euler(0f, -90f, 0f), Quaternion.Euler(0f, 90f, 0f)) }
            };

            var report = new List<string>();
            foreach (var candidate in candidates)
            {
                var left = profile.LeftBaseLocalRotation * candidate.Value.Left;
                var right = profile.RightBaseLocalRotation * candidate.Value.Right;
                var path = Path.Combine(OutputDirectory, candidate.Key + ".png");
                RenderPose(profile, left, right, path);
                report.Add(
                    $"{candidate.Key}: left={FormatEuler(left.eulerAngles)} right={FormatEuler(right.eulerAngles)}");
            }

            File.WriteAllLines(Path.Combine(OutputDirectory, "candidates.txt"), report);
            File.WriteAllText(Path.Combine(OutputDirectory, "orientation-report.txt"), BuildOrientationReport(profile));
            Debug.Log($"Rendered {candidates.Count} HandPoseLab candidates to {OutputDirectory}.");
        }

        public static void RenderFinal()
        {
            var profile = Resources.Load<FirstPersonArmProfile>(FirstPersonArmProfile.ResourceName);
            if (profile == null || !profile.HasActiveArmPrefabs)
            {
                throw new MissingReferenceException("Production FirstPersonArmProfile is unavailable.");
            }

            Directory.CreateDirectory(OutputDirectory);
            RenderPose(
                profile,
                profile.LeftBaseLocalRotation,
                profile.RightBaseLocalRotation,
                Path.Combine(OutputDirectory, "final.png"));
            File.WriteAllText(Path.Combine(OutputDirectory, "orientation-report.txt"), BuildOrientationReport(profile));
            File.WriteAllText(Path.Combine(OutputDirectory, "final-axis-acceptance.txt"), BuildAxisAcceptanceReport(profile));
        }

        public static void WriteRyderArmV2GeometryAudit()
        {
            var profile = Resources.Load<FirstPersonArmProfile>(FirstPersonArmProfile.ResourceName);
            if (profile == null || !profile.HasActiveArmPrefabs)
            {
                throw new MissingReferenceException("Production FirstPersonArmProfile is unavailable.");
            }

            Directory.CreateDirectory(OutputDirectory);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cameraObject = new GameObject("First Person Camera", typeof(UnityEngine.Camera));
            cameraObject.transform.position = new Vector3(0f, 1.62f, 0f);
            var camera = cameraObject.GetComponent<UnityEngine.Camera>();
            camera.fieldOfView = profile.ArmFieldOfView;
            camera.nearClipPlane = 0.04f;
            var visualRoot = CreateVisualRoot(cameraObject.transform);
            var left = CreateArm(visualRoot, profile.LeftArmPrefab, profile.LeftBaseLocalPosition, profile.LeftBaseLocalRotation, profile.LeftPrefabEulerOffset, profile.PrefabLocalScale);
            var right = CreateArm(visualRoot, profile.RightArmPrefab, profile.RightBaseLocalPosition, profile.RightBaseLocalRotation, profile.RightPrefabEulerOffset, profile.PrefabLocalScale);

            var builder = new StringBuilder();
            AppendGeometryAudit(builder, camera, "Left", left);
            AppendGeometryAudit(builder, camera, "Right", right);
            File.WriteAllText(Path.Combine(OutputDirectory, "ryder-arm-v2-geometry-audit.txt"), builder.ToString());
        }

        public static void RenderRyderArmV2PoseSweeps()
        {
            var profile = Resources.Load<FirstPersonArmProfile>(FirstPersonArmProfile.ResourceName);
            if (profile == null || !profile.HasRyderArmV2Prefabs)
            {
                throw new MissingReferenceException("Ryder Arm V2 prefabs are unavailable.");
            }

            Directory.CreateDirectory(OutputDirectory);
            var elbowCandidates = new Dictionary<string, Vector3>
            {
                { "elbow_x_plus_45", new Vector3(45f, 0f, 0f) },
                { "elbow_x_minus_45", new Vector3(-45f, 0f, 0f) },
                { "elbow_y_plus_45", new Vector3(0f, 45f, 0f) },
                { "elbow_y_minus_45", new Vector3(0f, -45f, 0f) },
                { "elbow_z_plus_45", new Vector3(0f, 0f, 45f) },
                { "elbow_z_minus_45", new Vector3(0f, 0f, -45f) }
            };
            foreach (var candidate in elbowCandidates)
            {
                RenderPose(
                    profile,
                    profile.LeftBaseLocalRotation,
                    profile.RightBaseLocalRotation,
                    Path.Combine(OutputDirectory, candidate.Key + ".png"),
                    (left, right) => ApplyMirroredBoneRotation(left, right, "Bone_003", candidate.Value));
            }

            var fingerCandidates = new Dictionary<string, Vector3>
            {
                { "fingers_x_plus_20", new Vector3(20f, 0f, 0f) },
                { "fingers_x_minus_20", new Vector3(-20f, 0f, 0f) },
                { "fingers_y_plus_20", new Vector3(0f, 20f, 0f) },
                { "fingers_y_minus_20", new Vector3(0f, -20f, 0f) },
                { "fingers_z_plus_20", new Vector3(0f, 0f, 20f) },
                { "fingers_z_minus_20", new Vector3(0f, 0f, -20f) }
            };
            foreach (var candidate in fingerCandidates)
            {
                RenderPose(
                    profile,
                    profile.LeftBaseLocalRotation,
                    profile.RightBaseLocalRotation,
                    Path.Combine(OutputDirectory, candidate.Key + ".png"),
                    (left, right) =>
                    {
                        ApplyMirroredFingerRotation(left, right, candidate.Value);
                    });
            }
        }

        public static void RenderRyderArmV2FramingCandidates()
        {
            var profile = Resources.Load<FirstPersonArmProfile>(FirstPersonArmProfile.ResourceName);
            if (profile == null || !profile.HasRyderArmV2Prefabs)
            {
                throw new MissingReferenceException("Ryder Arm V2 prefabs are unavailable.");
            }

            Directory.CreateDirectory(OutputDirectory);
            var candidates = new[]
            {
                new FramingCandidate("runner_24_z_thumb20_a", new Vector3(-0.58f, -0.12f, 0.46f), new Vector3(0.58f, -0.11f, 0.48f), new Vector3(0f, -60f, 200f), new Vector3(1f, 60f, 160f), 45f, 75f),
                new FramingCandidate("runner_25_z_thumb35_a", new Vector3(-0.58f, -0.12f, 0.46f), new Vector3(0.58f, -0.11f, 0.48f), new Vector3(0f, -60f, 215f), new Vector3(1f, 60f, 145f), 45f, 75f),
                new FramingCandidate("runner_26_z_thumb20_b", new Vector3(-0.58f, -0.12f, 0.46f), new Vector3(0.58f, -0.11f, 0.48f), new Vector3(0f, -60f, 160f), new Vector3(1f, 60f, 200f), 45f, 75f),
                new FramingCandidate("runner_27_z_thumb35_b", new Vector3(-0.58f, -0.12f, 0.46f), new Vector3(0.58f, -0.11f, 0.48f), new Vector3(0f, -60f, 145f), new Vector3(1f, 60f, 215f), 45f, 75f),
                new FramingCandidate("runner_28_z_thumb15_a_lower", new Vector3(-0.6f, -0.16f, 0.48f), new Vector3(0.59f, -0.14f, 0.5f), new Vector3(0f, -60f, 195f), new Vector3(1f, 60f, 165f), 45f, 75f),
                new FramingCandidate("runner_29_z_thumb25_a_lower", new Vector3(-0.6f, -0.16f, 0.48f), new Vector3(0.59f, -0.14f, 0.5f), new Vector3(0f, -60f, 205f), new Vector3(1f, 60f, 155f), 45f, 75f)
            };
            foreach (var candidate in candidates)
            {
                RenderPoseAt(
                    profile,
                    candidate.LeftPosition,
                    candidate.RightPosition,
                    Quaternion.Euler(candidate.LeftEuler),
                    Quaternion.Euler(candidate.RightEuler),
                    Path.Combine(OutputDirectory, candidate.Name + ".png"),
                    (left, right) =>
                    {
                        ApplyMirroredBoneRotation(left, right, "Bone_003", new Vector3(candidate.ElbowBend, 0f, 0f));
                        ApplyMirroredBoneRotation(left, right, "Bone_002", new Vector3(0f, candidate.WristRoll, 0f));
                    });
            }
        }

        public static void RenderAxisSigns()
        {
            var profile = Resources.Load<FirstPersonArmProfile>(FirstPersonArmProfile.ResourceName);
            if (profile == null || !profile.HasActiveArmPrefabs)
            {
                throw new MissingReferenceException("Production FirstPersonArmProfile is unavailable.");
            }

            Directory.CreateDirectory(OutputDirectory);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cameraObject = new GameObject("First Person Camera", typeof(UnityEngine.Camera));
            cameraObject.transform.position = new Vector3(0f, 1.62f, 0f);
            var camera = cameraObject.GetComponent<UnityEngine.Camera>();
            camera.fieldOfView = profile.ArmFieldOfView;
            camera.nearClipPlane = 0.04f;
            camera.clearFlags = CameraClearFlags.Skybox;
            RenderSettings.skybox = Resources.Load<Material>("Materials/MAT_RR_Skybox_Seamless");

            var visualRoot = CreateVisualRoot(cameraObject.transform);
            var left = CreateArm(visualRoot, profile.LeftArmPrefab, profile.LeftBaseLocalPosition, profile.LeftBaseLocalRotation, profile.LeftPrefabEulerOffset, profile.PrefabLocalScale);
            var right = CreateArm(visualRoot, profile.RightArmPrefab, profile.RightBaseLocalPosition, profile.RightBaseLocalRotation, profile.RightPrefabEulerOffset, profile.PrefabLocalScale);
            CreateAxisMarker(left, Vector3.up * 0.19f, Color.green);
            CreateAxisMarker(left, Vector3.down * 0.19f, Color.magenta);
            CreateAxisMarker(right, Vector3.up * 0.19f, Color.green);
            CreateAxisMarker(right, Vector3.down * 0.19f, Color.magenta);
            CreateFloor();
            CreateLight();
            Capture(camera, Path.Combine(OutputDirectory, "axis-signs.png"));
        }

        private static void RenderPose(
            FirstPersonArmProfile profile,
            Quaternion leftRotation,
            Quaternion rightRotation,
            string outputPath,
            System.Action<Transform, Transform> adjustment = null)
        {
            RenderPoseAt(
                profile,
                profile.LeftBaseLocalPosition,
                profile.RightBaseLocalPosition,
                leftRotation,
                rightRotation,
                outputPath,
                adjustment);
        }

        private static void RenderPoseAt(
            FirstPersonArmProfile profile,
            Vector3 leftPosition,
            Vector3 rightPosition,
            Quaternion leftRotation,
            Quaternion rightRotation,
            string outputPath,
            System.Action<Transform, Transform> adjustment = null)
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cameraObject = new GameObject("First Person Camera", typeof(UnityEngine.Camera));
            cameraObject.transform.position = new Vector3(0f, 1.62f, 0f);
            var camera = cameraObject.GetComponent<UnityEngine.Camera>();
            camera.fieldOfView = profile.ArmFieldOfView;
            camera.nearClipPlane = 0.04f;
            camera.clearFlags = CameraClearFlags.Skybox;
            RenderSettings.skybox = Resources.Load<Material>("Materials/MAT_RR_Skybox_Seamless");

            var visualRoot = CreateVisualRoot(cameraObject.transform);
            var left = CreateArm(visualRoot, profile.LeftArmPrefab, leftPosition, leftRotation, profile.LeftPrefabEulerOffset, profile.PrefabLocalScale);
            var right = CreateArm(visualRoot, profile.RightArmPrefab, rightPosition, rightRotation, profile.RightPrefabEulerOffset, profile.PrefabLocalScale);
            adjustment?.Invoke(left, right);
            CreateFloor();
            CreateLight();

            Capture(camera, outputPath);
        }

        private static Transform CreateArm(
            Transform camera,
            GameObject prefab,
            Vector3 position,
            Quaternion rotation,
            Vector3 prefabEulerOffset,
            Vector3 scale)
        {
            var root = new GameObject("Arm Root").transform;
            root.SetParent(camera, false);
            root.localPosition = position;
            root.localRotation = rotation;
            var visual = Object.Instantiate(prefab, root);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.Euler(prefabEulerOffset);
            visual.transform.localScale = scale;
            ModuleVisualPrefabLibrary.PrepareVisualInstance(visual);
            return root;
        }

        private static Transform CreateVisualRoot(Transform camera)
        {
            var root = new GameObject("FirstPersonVisualRoot").transform;
            root.SetParent(camera, false);
            return root;
        }

        private static void AppendGeometryAudit(StringBuilder builder, UnityEngine.Camera camera, string side, Transform root)
        {
            var skin = root.GetComponentInChildren<SkinnedMeshRenderer>(true);
            var baked = new Mesh();
            skin.BakeMesh(baked, true);
            var worldCenter = skin.transform.TransformPoint(baked.bounds.center);
            builder.AppendLine($"{side} renderer enabled/active: {skin.enabled}/{skin.gameObject.activeInHierarchy}");
            builder.AppendLine($"{side} source bounds: {skin.sharedMesh.bounds}");
            builder.AppendLine($"{side} baked bounds: {baked.bounds}");
            builder.AppendLine($"{side} baked world center: {worldCenter}");
            builder.AppendLine($"{side} baked viewport center: {camera.WorldToViewportPoint(worldCenter)}");
            builder.AppendLine($"{side} renderer world bounds: {skin.bounds}");
            builder.AppendLine($"{side} root bone: {skin.rootBone?.name} at {skin.rootBone?.position}");
            foreach (var boneName in new[] { "Bone_003", "Bone_002", "Bone_001", "Bone_011", "Bone_008" })
            {
                var bone = FindDeepChild(root, boneName);
                builder.AppendLine($"{side} {boneName}: world={bone?.position} arm-local={(bone == null ? Vector3.zero : root.InverseTransformPoint(bone.position))} rotation={bone?.rotation.eulerAngles}");
            }

            Object.DestroyImmediate(baked);
        }

        private static Transform FindDeepChild(Transform parent, string name)
        {
            if (parent.name == name)
            {
                return parent;
            }

            for (var index = 0; index < parent.childCount; index++)
            {
                var match = FindDeepChild(parent.GetChild(index), name);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        private static void ApplyMirroredBoneRotation(Transform left, Transform right, string boneName, Vector3 rightEuler)
        {
            RotateBone(right, boneName, rightEuler);
            RotateBone(left, boneName, new Vector3(rightEuler.x, -rightEuler.y, -rightEuler.z));
        }

        private static void ApplyMirroredFingerRotation(Transform left, Transform right, Vector3 rightEuler)
        {
            foreach (var boneName in new[]
                     {
                         "Bone_007", "Bone_006", "Bone_005", "Bone_004",
                         "Bone_011", "Bone_010", "Bone_009", "Bone_008",
                         "Bone_015", "Bone_014", "Bone_013", "Bone_012",
                         "Bone_019", "Bone_018", "Bone_017", "Bone_016",
                         "Bone_023", "Bone_022", "Bone_021", "Bone_020"
                     })
            {
                ApplyMirroredBoneRotation(left, right, boneName, rightEuler);
            }
        }

        private static void RotateBone(Transform root, string boneName, Vector3 euler)
        {
            var bone = FindDeepChild(root, boneName);
            if (bone != null)
            {
                bone.localRotation *= Quaternion.Euler(euler);
            }
        }

        private readonly struct FramingCandidate
        {
            public FramingCandidate(string name, Vector3 leftPosition, Vector3 rightPosition, Vector3 leftEuler, Vector3 rightEuler, float elbowBend, float wristRoll)
            {
                Name = name;
                LeftPosition = leftPosition;
                RightPosition = rightPosition;
                LeftEuler = leftEuler;
                RightEuler = rightEuler;
                ElbowBend = elbowBend;
                WristRoll = wristRoll;
            }

            public string Name { get; }
            public Vector3 LeftPosition { get; }
            public Vector3 RightPosition { get; }
            public Vector3 LeftEuler { get; }
            public Vector3 RightEuler { get; }
            public float ElbowBend { get; }
            public float WristRoll { get; }
        }

        private static void CreateAxisMarker(Transform root, Vector3 localPosition, Color color)
        {
            var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.transform.SetParent(root, false);
            marker.transform.localPosition = localPosition;
            marker.transform.localScale = Vector3.one * 0.075f;
            var shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
            var material = new Material(shader) { name = "HandPoseLab Axis Marker", color = color };
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }
            marker.GetComponent<Renderer>().sharedMaterial = material;
            Object.DestroyImmediate(marker.GetComponent<Collider>());
        }

        private static void Capture(UnityEngine.Camera camera, string outputPath)
        {
            var texture = new RenderTexture(RenderWidth, RenderHeight, 24, RenderTextureFormat.ARGB32);
            var image = new Texture2D(RenderWidth, RenderHeight, TextureFormat.RGB24, false);
            camera.targetTexture = texture;
            camera.Render();
            RenderTexture.active = texture;
            image.ReadPixels(new Rect(0f, 0f, texture.width, texture.height), 0, 0);
            image.Apply();
            File.WriteAllBytes(outputPath, image.EncodeToPNG());
            camera.targetTexture = null;
            RenderTexture.active = null;
            Object.DestroyImmediate(image);
            Object.DestroyImmediate(texture);
        }

        private static void CreateFloor()
        {
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.position = new Vector3(0f, -0.1f, 7f);
            floor.transform.localScale = new Vector3(14f, 0.2f, 18f);
            floor.GetComponent<Renderer>().sharedMaterial = VisualMaterialUtility.CreateRuntimeMaterial(
                "HandPoseLab Floor",
                new Color(0.25f, 0.3f, 0.38f));
        }

        private static void CreateLight()
        {
            var lightObject = new GameObject("Key Light", typeof(Light));
            lightObject.transform.rotation = Quaternion.Euler(44f, -28f, 0f);
            var light = lightObject.GetComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.5f;
            light.color = new Color(1f, 0.92f, 0.8f);
            RenderSettings.ambientLight = new Color(0.58f, 0.68f, 0.82f);
        }

        private static string FormatEuler(Vector3 euler)
        {
            return $"({euler.x:F5}, {euler.y:F5}, {euler.z:F5})";
        }

        private static string BuildOrientationReport(FirstPersonArmProfile profile)
        {
            var builder = new StringBuilder();
            AppendPrefabOrientation(builder, "Left", profile.LeftArmPrefab);
            AppendPrefabOrientation(builder, "Right", profile.RightArmPrefab);

            var leftMesh = ResolvePrefabMesh(profile.LeftArmPrefab);
            var rightMesh = ResolvePrefabMesh(profile.RightArmPrefab);
            var mirrored = leftMesh != null
                && rightMesh != null
                && AreMirroredMeshes(leftMesh, rightMesh);
            builder.AppendLine($"Baked X-mirrored mesh pair: {mirrored}");
            builder.AppendLine("Camera local axes: +X right, +Y up, +Z forward.");
            builder.AppendLine(profile.PresentationMode == FirstPersonArmPresentationMode.RyderArmV2
                ? "Ryder Arm V2 directions are validated from named joints; prefab-root basis vectors are diagnostic only."
                : "Legacy mesh orientation is retained as fallback data.");
            AppendCameraBasis(builder, "Left current", profile.LeftBaseLocalRotation);
            AppendCameraBasis(builder, "Right current", profile.RightBaseLocalRotation);
            return builder.ToString();
        }

        private static string BuildAxisAcceptanceReport(FirstPersonArmProfile profile)
        {
            var leftRotation = profile.LeftBaseLocalRotation * Quaternion.Euler(profile.LeftPrefabEulerOffset);
            var rightRotation = profile.RightBaseLocalRotation * Quaternion.Euler(profile.RightPrefabEulerOffset);
            var leftFinger = leftRotation * Vector3.up;
            var rightFinger = rightRotation * Vector3.up;
            var leftPalm = -(leftRotation * Vector3.forward);
            var rightPalm = -(rightRotation * Vector3.forward);
            var leftThumb = -(leftRotation * Vector3.right);
            var rightThumb = rightRotation * Vector3.right;

            var builder = new StringBuilder();
            builder.AppendLine($"Left finger-forward: {leftFinger}; dot camera-forward={Vector3.Dot(leftFinger, Vector3.forward):F4}");
            builder.AppendLine($"Right finger-forward: {rightFinger}; dot camera-forward={Vector3.Dot(rightFinger, Vector3.forward):F4}");
            builder.AppendLine($"Left palm normal: {leftPalm}; dot inward-right={Vector3.Dot(leftPalm, Vector3.right):F4}");
            builder.AppendLine($"Right palm normal: {rightPalm}; dot inward-left={Vector3.Dot(rightPalm, Vector3.left):F4}");
            builder.AppendLine($"Left thumb direction: {leftThumb}; dot camera-up={Vector3.Dot(leftThumb, Vector3.up):F4}");
            builder.AppendLine($"Right thumb direction: {rightThumb}; dot camera-up={Vector3.Dot(rightThumb, Vector3.up):F4}");
            builder.AppendLine($"Left/right positions: {profile.LeftBaseLocalPosition} / {profile.RightBaseLocalPosition}");
            builder.AppendLine($"Left/right root eulers: {FormatEuler(profile.LeftBaseLocalEulerAngles)} / {FormatEuler(profile.RightBaseLocalEulerAngles)}");
            builder.AppendLine($"Left/right prefab euler offsets: {FormatEuler(profile.LeftPrefabEulerOffset)} / {FormatEuler(profile.RightPrefabEulerOffset)}");
            builder.AppendLine($"Shared scale: {profile.PrefabLocalScale}");
            builder.AppendLine($"Neutral pose locked: {profile.NeutralPoseLocked}");
            return builder.ToString();
        }

        private static void AppendCameraBasis(StringBuilder builder, string label, Quaternion rotation)
        {
            builder.AppendLine($"{label} prefab-root axes in camera space:");
            builder.AppendLine($"  +X width: {rotation * Vector3.right}");
            builder.AppendLine($"  +Y long/finger: {rotation * Vector3.up}");
            builder.AppendLine($"  +Z palm-depth: {rotation * Vector3.forward}");
        }

        private static void AppendPrefabOrientation(StringBuilder builder, string side, GameObject prefab)
        {
            var root = prefab.transform;
            var skin = prefab.GetComponentInChildren<SkinnedMeshRenderer>();
            var filter = skin == null ? prefab.GetComponentInChildren<MeshFilter>() : null;
            var model = filter != null ? filter.transform : skin == null ? null : skin.transform;
            var mesh = filter != null ? filter.sharedMesh : skin == null ? null : skin.sharedMesh;
            builder.AppendLine($"{side} prefab: {prefab.name}");
            builder.AppendLine($"  root position/rotation/scale: {root.localPosition} / {FormatEuler(root.localEulerAngles)} / {root.localScale}");
            if (model == null)
            {
                builder.AppendLine("  model child: missing");
                return;
            }

            builder.AppendLine($"  model position/rotation/scale: {model.localPosition} / {FormatEuler(model.localEulerAngles)} / {model.localScale}");
            builder.AppendLine($"  renderer type: {(skin == null ? "MeshRenderer" : "SkinnedMeshRenderer")}");
            builder.AppendLine($"  mesh bounds center/extents: {mesh.bounds.center} / {mesh.bounds.extents}");
            builder.AppendLine($"  negative scale in hierarchy: {HasNegativeScale(root) || HasNegativeScale(model)}");
        }

        private static Mesh ResolvePrefabMesh(GameObject prefab)
        {
            var skin = prefab.GetComponentInChildren<SkinnedMeshRenderer>();
            if (skin != null)
            {
                return skin.sharedMesh;
            }

            return prefab.GetComponentInChildren<MeshFilter>()?.sharedMesh;
        }

        private static bool HasNegativeScale(Transform transform)
        {
            var scale = transform.localScale;
            return scale.x < 0f || scale.y < 0f || scale.z < 0f;
        }

        private static bool AreMirroredMeshes(Mesh left, Mesh right)
        {
            if (left == null || right == null || left.vertexCount != right.vertexCount)
            {
                return false;
            }

            var leftVertices = left.vertices;
            var rightVertices = right.vertices;
            for (var index = 0; index < leftVertices.Length; index++)
            {
                var expected = new Vector3(-rightVertices[index].x, rightVertices[index].y, rightVertices[index].z);
                if ((leftVertices[index] - expected).sqrMagnitude > 0.000000000001f)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
