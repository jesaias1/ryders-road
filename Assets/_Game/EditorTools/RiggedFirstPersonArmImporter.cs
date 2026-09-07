using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Avoidance.Gameplay.Visuals;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Avoidance.EditorTools
{
    public static class RiggedFirstPersonArmImporter
    {
        private const string SourceGlbPath =
            @"C:\Users\lin4s\Downloads\Ryders Block Assets\Ryders Arm Rigged.glb";
        private const string SourceAssetPath =
            "Assets/_Game/Art/FirstPerson/Arms/RyderArmV2/Source/Ryders_Arm_Rigged.glb";
        private const string RiggedRoot = "Assets/_Game/Art/FirstPerson/Arms/RyderArmV2";
        private const string LegacyRoot = "Assets/_Game/Art/FirstPerson/Arms/LegacyApproved";
        private const string ActiveLegacyLeftPath = "Assets/_Game/Art/FirstPerson/Arms/PF_RR_FP_Arm_Left.prefab";
        private const string ActiveLegacyRightPath = "Assets/_Game/Art/FirstPerson/Arms/PF_RR_FP_Arm_Right.prefab";
        private const string LegacyLeftPath = LegacyRoot + "/PF_RR_FP_Arm_Left_LegacyApproved.prefab";
        private const string LegacyRightPath = LegacyRoot + "/PF_RR_FP_Arm_Right_LegacyApproved.prefab";
        private const string PreviousRiggedLeftPath =
            "Assets/_Game/Art/FirstPerson/Arms/Rigged/PF_RR_FP_Arm_Rigged_Left.prefab";
        private const string PreviousRiggedRightPath =
            "Assets/_Game/Art/FirstPerson/Arms/Rigged/PF_RR_FP_Arm_Rigged_Right.prefab";
        private const string ProfilePath = "Assets/_Game/Visuals/Resources/FirstPersonArmProfile.asset";
        private const string AuditPath = "Logs/phase073-surgical-arm-presentation-recovery-audit.txt";
        private const float ElbowBendDegrees = 58f;
        private const float WristPronationDegrees = 68f;
        private static readonly Vector3 RigCorrectionLocalPosition = Vector3.zero;
        private static readonly Vector3 RigCorrectionLocalScale = Vector3.one;
        private static readonly Vector3 LeftBasePosition = new Vector3(-0.52f, -0.025f, 0.41f);
        private static readonly Vector3 RightBasePosition = new Vector3(0.52f, -0.02f, 0.41f);
        private static readonly Vector3 LeftBaseEulerAngles = new Vector3(0f, -52f, 202f);
        private static readonly Vector3 RightBaseEulerAngles = new Vector3(1f, 52f, 158f);
        private static readonly float[] FingerCurlDegrees = { 2f, 5f, 6f, 7f, 8f };

        private static readonly string[][] FingerChains =
        {
            new[] { "Bone_007", "Bone_006", "Bone_005", "Bone_004" },
            new[] { "Bone_011", "Bone_010", "Bone_009", "Bone_008" },
            new[] { "Bone_015", "Bone_014", "Bone_013", "Bone_012" },
            new[] { "Bone_019", "Bone_018", "Bone_017", "Bone_016" },
            new[] { "Bone_023", "Bone_022", "Bone_021", "Bone_020" }
        };

        [MenuItem("RYDERS BLOCK/Art/Import Ryder Arm V2")]
        public static void ImportRiggedFirstPersonArm()
        {
            if (!File.Exists(SourceGlbPath))
            {
                throw new FileNotFoundException("Rigged arm GLB source is missing.", SourceGlbPath);
            }

            EnsureFolders();
            CopySourceAndLegacyFallbacks();

            var document = GlbDocument.Load(SourceGlbPath);
            var material = CreateMaterial(document);
            var right = CreateRiggedPrefab(document, material, mirrorX: false);
            var left = CreateRiggedPrefab(document, material, mirrorX: true);
            ConfigureProfile(left.PrefabPath, right.PrefabPath);
            WriteAudit(document, left, right);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("RYDER'S ROAD Ryder Arm V2 import complete.");
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/_Game/Art/FirstPerson/Arms", "RyderArmV2");
            EnsureFolder("Assets/_Game/Art/FirstPerson/Arms", "LegacyApproved");
            EnsureFolder(RiggedRoot, "Source");
            EnsureFolder(RiggedRoot, "Meshes");
            EnsureFolder(RiggedRoot, "Materials");
            EnsureFolder(RiggedRoot, "Textures");
        }

        private static void EnsureFolder(string parent, string child)
        {
            var path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        private static void CopySourceAndLegacyFallbacks()
        {
            File.Copy(SourceGlbPath, SourceAssetPath, overwrite: true);

            CopyAssetIfMissing(ActiveLegacyLeftPath, LegacyLeftPath);
            CopyAssetIfMissing(ActiveLegacyRightPath, LegacyRightPath);
            AssetDatabase.ImportAsset(SourceAssetPath);
            AssetDatabase.ImportAsset(LegacyLeftPath);
            AssetDatabase.ImportAsset(LegacyRightPath);
        }

        private static void CopyAssetIfMissing(string source, string destination)
        {
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(destination) != null)
            {
                return;
            }

            if (!AssetDatabase.CopyAsset(source, destination))
            {
                throw new InvalidOperationException($"Could not copy fallback asset {source} to {destination}.");
            }
        }

        private static Material CreateMaterial(GlbDocument document)
        {
            var textures = ExtractTextures(document);
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var material = new Material(shader)
            {
                name = "MAT_RR_FP_RyderArmV2"
            };

            var gltfMaterial = document.Root.materials?[0];
            if (gltfMaterial?.pbrMetallicRoughness?.baseColorTexture != null)
            {
                SetTexture(material, "_BaseMap", textures.TextureFor(gltfMaterial.pbrMetallicRoughness.baseColorTexture.index));
                SetTexture(material, "_MainTex", textures.TextureFor(gltfMaterial.pbrMetallicRoughness.baseColorTexture.index));
            }

            if (gltfMaterial?.normalTexture != null)
            {
                SetTexture(material, "_BumpMap", textures.TextureFor(gltfMaterial.normalTexture.index));
                material.EnableKeyword("_NORMALMAP");
            }

            if (gltfMaterial?.emissiveTexture != null)
            {
                var emission = textures.TextureFor(gltfMaterial.emissiveTexture.index);
                SetTexture(material, "_EmissionMap", emission);
                if (emission != null && material.HasProperty("_EmissionColor"))
                {
                    material.SetColor("_EmissionColor", Color.white * 1.1f);
                    material.EnableKeyword("_EMISSION");
                    material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                }
            }

            if (material.HasProperty("_MetallicGlossMap"))
            {
                material.SetTexture("_MetallicGlossMap", null);
            }

            material.DisableKeyword("_METALLICSPECGLOSSMAP");
            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", 0.08f);
            }

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", 0.42f);
            }

            if (material.HasProperty("_GlossMapScale"))
            {
                material.SetFloat("_GlossMapScale", 0f);
            }

            if (material.HasProperty("_BumpScale"))
            {
                material.SetFloat("_BumpScale", 0.35f);
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", Color.white);
            }

            if (material.HasProperty("_Cull"))
            {
                material.SetFloat("_Cull", (float)CullMode.Back);
            }

            var path = RiggedRoot + "/Materials/MAT_RR_FP_RyderArmV2.mat";
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static ExtractedTextures ExtractTextures(GlbDocument document)
        {
            var paths = new string[document.Root.images?.Length ?? 0];
            var normalSources = new HashSet<int>();
            var linearSources = new HashSet<int>();
            var material = document.Root.materials?[0];
            var normalSource = TextureSourceIndex(document, material?.normalTexture?.index ?? -1);
            if (normalSource >= 0)
            {
                normalSources.Add(normalSource);
                linearSources.Add(normalSource);
            }

            var metallicRoughnessSource = TextureSourceIndex(
                document,
                material?.pbrMetallicRoughness?.metallicRoughnessTexture?.index ?? -1);
            if (metallicRoughnessSource >= 0)
            {
                linearSources.Add(metallicRoughnessSource);
            }

            for (var index = 0; index < paths.Length; index++)
            {
                var image = document.Root.images[index];
                var extension = image.mimeType == "image/png" ? ".png" : ".jpg";
                var path = $"{RiggedRoot}/Textures/RR_FP_RyderArmV2_Image_{index}{extension}";
                var bytes = document.BufferViewBytes(image.bufferView);
                File.WriteAllBytes(path, bytes);
                AssetDatabase.ImportAsset(path);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                {
                    importer.maxTextureSize = 2048;
                    importer.mipmapEnabled = true;
                    if (normalSources.Contains(index))
                    {
                        importer.textureType = TextureImporterType.NormalMap;
                    }
                    else
                    {
                        importer.textureType = TextureImporterType.Default;
                        importer.sRGBTexture = !linearSources.Contains(index);
                    }

                    importer.SaveAndReimport();
                }

                paths[index] = path;
            }

            return new ExtractedTextures(document.Root.textures, paths);
        }

        private static void SetTexture(Material material, string property, Texture texture)
        {
            if (texture != null && material.HasProperty(property))
            {
                material.SetTexture(property, texture);
            }
        }

        private static RiggedImportResult CreateRiggedPrefab(
            GlbDocument document,
            Material material,
            bool mirrorX)
        {
            var sideName = mirrorX ? "Left" : "Right";
            var root = new GameObject($"PF_RR_FP_RyderArmV2_{sideName}");
            var correction = new GameObject("RigCorrectionRoot").transform;
            correction.SetParent(root.transform, false);
            correction.localPosition = RigCorrectionLocalPosition;
            correction.localScale = RigCorrectionLocalScale;
            var riggedArm = new GameObject($"RyderArmV2Rig_{sideName}").transform;
            riggedArm.SetParent(correction, false);
            var skeletonRoot = new GameObject("Skeleton").transform;
            skeletonRoot.SetParent(riggedArm, false);
            var meshObject = new GameObject("SkinnedMesh", typeof(SkinnedMeshRenderer));
            meshObject.transform.SetParent(riggedArm, false);

            var nodeTransforms = new Transform[document.Root.nodes.Length];
            var sceneNodes = document.Root.scenes[document.Root.scene].nodes;
            foreach (var nodeIndex in sceneNodes)
            {
                CreateNodeHierarchy(document, nodeIndex, skeletonRoot, nodeTransforms, mirrorX, skipMeshNodes: true);
            }

            var skin = document.Root.skins[0];
            var bones = skin.joints.Select(joint => nodeTransforms[joint]).ToArray();
            var mesh = CreateMesh(document, mirrorX);
            mesh.bindposes = bones
                .Select(bone => bone.worldToLocalMatrix * meshObject.transform.localToWorldMatrix)
                .ToArray();

            var meshPath = $"{RiggedRoot}/Meshes/RR_FP_RyderArmV2_{sideName}.asset";
            AssetDatabase.DeleteAsset(meshPath);
            AssetDatabase.CreateAsset(mesh, meshPath);

            var renderer = meshObject.GetComponent<SkinnedMeshRenderer>();
            renderer.sharedMesh = mesh;
            renderer.sharedMaterials = new[] { material };
            renderer.bones = bones;
            renderer.rootBone = bones.Length > 0 ? bones[0] : null;
            renderer.localBounds = new Bounds(
                new Vector3(0f, 0.85f, 0f),
                new Vector3(1.2f, 2.0f, 1.2f));
            renderer.updateWhenOffscreen = true;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
            renderer.allowOcclusionWhenDynamic = false;
            ApplyCanonicalArmPose(riggedArm, mirrorX);
            ApplyCanonicalFingerRestPose(riggedArm, mirrorX);

            var prefabPath = $"{RiggedRoot}/PF_RR_FP_RyderArmV2_{sideName}.prefab";
            AssetDatabase.DeleteAsset(prefabPath);
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            return new RiggedImportResult(prefabPath, meshPath, bones.Length);
        }

        private static void CreateNodeHierarchy(
            GlbDocument document,
            int nodeIndex,
            Transform parent,
            Transform[] nodeTransforms,
            bool mirrorX,
            bool skipMeshNodes)
        {
            var node = document.Root.nodes[nodeIndex];
            if (skipMeshNodes && node.mesh >= 0)
            {
                return;
            }

            var transform = new GameObject(string.IsNullOrWhiteSpace(node.name) ? $"Node_{nodeIndex}" : node.name).transform;
            transform.SetParent(parent, false);
            transform.localPosition = ConvertVector3(node.translation, mirrorX);
            transform.localRotation = ConvertRotation(node.rotation, mirrorX);
            transform.localScale = ConvertScale(node.scale);
            nodeTransforms[nodeIndex] = transform;

            if (node.children == null)
            {
                return;
            }

            foreach (var child in node.children)
            {
                CreateNodeHierarchy(document, child, transform, nodeTransforms, mirrorX, skipMeshNodes);
            }
        }

        private static Mesh CreateMesh(GlbDocument document, bool mirrorX)
        {
            var primitive = document.Root.meshes[0].primitives[0];
            var positions = document.ReadVector3Accessor(primitive.attributes.POSITION, mirrorX);
            var normals = document.ReadVector3Accessor(primitive.attributes.NORMAL, mirrorX);
            var uvs = document.ReadVector2Accessor(primitive.attributes.TEXCOORD_0);
            var triangles = document.ReadIndexAccessor(primitive.indices);
            if (mirrorX)
            {
                for (var index = 0; index < triangles.Length; index += 3)
                {
                    (triangles[index], triangles[index + 1]) = (triangles[index + 1], triangles[index]);
                }
            }

            var mesh = new Mesh
            {
                name = mirrorX ? "RR_FP_RyderArmV2_Left" : "RR_FP_RyderArmV2_Right",
                indexFormat = positions.Length > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16
            };
            mesh.SetVertices(positions);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            ApplyBoneWeights(document, primitive, mesh);
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            return mesh;
        }

        private static void ApplyBoneWeights(
            GlbDocument document,
            GltfPrimitive primitive,
            Mesh mesh)
        {
            var joints0 = document.ReadVector4IntAccessor(primitive.attributes.JOINTS_0);
            var joints1 = document.ReadVector4IntAccessor(primitive.attributes.JOINTS_1);
            var weights0 = document.ReadVector4Accessor(primitive.attributes.WEIGHTS_0);
            var weights1 = document.ReadVector4Accessor(primitive.attributes.WEIGHTS_1);
            var bonesPerVertexList = new byte[joints0.Length];
            var allWeights = new List<BoneWeight1>(joints0.Length * 4);
            for (var vertex = 0; vertex < joints0.Length; vertex++)
            {
                var influences = new List<BoneWeight1>(8);
                AddInfluence(influences, joints0[vertex].x, weights0[vertex].x);
                AddInfluence(influences, joints0[vertex].y, weights0[vertex].y);
                AddInfluence(influences, joints0[vertex].z, weights0[vertex].z);
                AddInfluence(influences, joints0[vertex].w, weights0[vertex].w);
                AddInfluence(influences, joints1[vertex].x, weights1[vertex].x);
                AddInfluence(influences, joints1[vertex].y, weights1[vertex].y);
                AddInfluence(influences, joints1[vertex].z, weights1[vertex].z);
                AddInfluence(influences, joints1[vertex].w, weights1[vertex].w);

                if (influences.Count == 0)
                {
                    influences.Add(new BoneWeight1 { boneIndex = 0, weight = 1f });
                }

                var total = influences.Sum(weight => weight.weight);
                for (var index = 0; index < influences.Count; index++)
                {
                    var influence = influences[index];
                    influence.weight /= total;
                    influences[index] = influence;
                }

                bonesPerVertexList[vertex] = (byte)influences.Count;
                allWeights.AddRange(influences.OrderByDescending(weight => weight.weight));
            }

            using var bonesPerVertex = new NativeArray<byte>(bonesPerVertexList, Allocator.Temp);
            using var weights = new NativeArray<BoneWeight1>(allWeights.ToArray(), Allocator.Temp);
            mesh.SetBoneWeights(bonesPerVertex, weights);
        }

        private static void ApplyCanonicalFingerRestPose(Transform root, bool mirrorX)
        {
            var side = mirrorX ? -1f : 1f;
            for (var chainIndex = 0; chainIndex < FingerChains.Length; chainIndex++)
            {
                var curl = FingerCurlDegrees[chainIndex];
                var spread = chainIndex == 0 ? side * -1.2f : (chainIndex - 2f) * 0.18f * side;
                for (var jointIndex = 0; jointIndex < FingerChains[chainIndex].Length; jointIndex++)
                {
                    var bone = FindDeepChild(root, FingerChains[chainIndex][jointIndex]);
                    if (bone == null)
                    {
                        continue;
                    }

                    var jointWeight = jointIndex == 0 ? 1f : jointIndex == 1 ? 0.75f : jointIndex == 2 ? 0.5f : 0.25f;
                    bone.localRotation *= Quaternion.Euler(0f, spread, side * curl * jointWeight);
                }
            }
        }

        private static void ApplyCanonicalArmPose(Transform root, bool mirrorX)
        {
            var upperArm = FindDeepChild(root, "Bone_000");
            if (upperArm != null)
            {
                upperArm.localRotation *= Quaternion.identity;
            }

            var elbow = FindDeepChild(root, "Bone_003");
            if (elbow != null)
            {
                elbow.localRotation *= Quaternion.Euler(ElbowBendDegrees, 0f, 0f);
            }

            var wrist = FindDeepChild(root, "Bone_002");
            if (wrist != null)
            {
                wrist.localRotation *= Quaternion.Euler(
                    0f,
                    mirrorX ? -WristPronationDegrees : WristPronationDegrees,
                    0f);
            }

            var palm = FindDeepChild(root, "Bone_001");
            if (palm != null)
            {
                palm.localRotation *= Quaternion.identity;
            }
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

        private static void AddInfluence(ICollection<BoneWeight1> influences, int boneIndex, float weight)
        {
            if (weight <= 0.00001f)
            {
                return;
            }

            influences.Add(new BoneWeight1
            {
                boneIndex = boneIndex,
                weight = weight
            });
        }

        private static Vector3 ConvertVector3(float[] values, bool mirrorX)
        {
            if (values == null || values.Length < 3)
            {
                return Vector3.zero;
            }

            return new Vector3(mirrorX ? -values[0] : values[0], values[1], values[2]);
        }

        private static Vector3 ConvertScale(float[] values)
        {
            if (values == null || values.Length < 3)
            {
                return Vector3.one;
            }

            return new Vector3(values[0], values[1], values[2]);
        }

        private static Quaternion ConvertRotation(float[] values, bool mirrorX)
        {
            if (values == null || values.Length < 4)
            {
                return Quaternion.identity;
            }

            return mirrorX
                ? new Quaternion(values[0], -values[1], -values[2], values[3])
                : new Quaternion(values[0], values[1], values[2], values[3]);
        }

        private static void ConfigureProfile(string leftPrefabPath, string rightPrefabPath)
        {
            var profile = AssetDatabase.LoadAssetAtPath<FirstPersonArmProfile>(ProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<FirstPersonArmProfile>();
                AssetDatabase.CreateAsset(profile, ProfilePath);
            }

            var serialized = new SerializedObject(profile);
            serialized.FindProperty("_presentationMode").enumValueIndex =
                (int)FirstPersonArmPresentationMode.RyderArmV2;
            var showFirstPersonArms = serialized.FindProperty("_showFirstPersonArms");
            if (showFirstPersonArms != null)
            {
                showFirstPersonArms.boolValue = false;
            }

            serialized.FindProperty("_leftArmPrefab").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<GameObject>(LegacyLeftPath);
            serialized.FindProperty("_rightArmPrefab").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<GameObject>(LegacyRightPath);
            serialized.FindProperty("_riggedLeftArmPrefab").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<GameObject>(PreviousRiggedLeftPath);
            serialized.FindProperty("_riggedRightArmPrefab").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<GameObject>(PreviousRiggedRightPath);
            serialized.FindProperty("_ryderArmV2LeftArmPrefab").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<GameObject>(leftPrefabPath);
            serialized.FindProperty("_ryderArmV2RightArmPrefab").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<GameObject>(rightPrefabPath);
            serialized.FindProperty("_leftBaseLocalPosition").vector3Value = LeftBasePosition;
            serialized.FindProperty("_rightBaseLocalPosition").vector3Value = RightBasePosition;
            serialized.FindProperty("_leftBaseLocalEulerAngles").vector3Value = LeftBaseEulerAngles;
            serialized.FindProperty("_rightBaseLocalEulerAngles").vector3Value = RightBaseEulerAngles;
            serialized.FindProperty("_leftPrefabEulerOffset").vector3Value = Vector3.zero;
            serialized.FindProperty("_rightPrefabEulerOffset").vector3Value = Vector3.zero;
            serialized.FindProperty("_prefabLocalScale").vector3Value = Vector3.one * 0.36f;
            serialized.FindProperty("_neutralPoseLocked").boolValue = false;
            serialized.FindProperty("_armFieldOfView").floatValue = 72f;
            serialized.FindProperty("_armNearClipPlane").floatValue = 0.025f;
            serialized.FindProperty("_idleFloatAmount").floatValue = 0.0028f;
            serialized.FindProperty("_idleFrequency").floatValue = 1.05f;
            serialized.FindProperty("_runLateralAmount").floatValue = 0.002f;
            serialized.FindProperty("_runVerticalAmount").floatValue = 0.0075f;
            serialized.FindProperty("_runForwardAmount").floatValue = 0.028f;
            serialized.FindProperty("_runFrequencyMin").floatValue = 4.8f;
            serialized.FindProperty("_runFrequencyMax").floatValue = 8f;
            serialized.FindProperty("_runSpeedRollDegrees").floatValue = 0.9f;
            serialized.FindProperty("_runYawDegrees").floatValue = 0.9f;
            serialized.FindProperty("_runPitchDegrees").floatValue = 2f;
            serialized.FindProperty("_runLateralRollDegrees").floatValue = 0.8f;
            serialized.FindProperty("_jumpOffset").floatValue = 0.015f;
            serialized.FindProperty("_airborneOffset").vector3Value = new Vector3(0f, 0.002f, 0.006f);
            serialized.FindProperty("_fallOffset").vector3Value = new Vector3(0.006f, -0.004f, -0.003f);
            serialized.FindProperty("_landImpulse").floatValue = 0.02f;
            serialized.FindProperty("_boostOffset").floatValue = 0f;
            serialized.FindProperty("_waterOffset").floatValue = 0f;
            serialized.FindProperty("_speedInfluence").floatValue = 7.8f;
            serialized.FindProperty("_maximumAnimationMultiplier").floatValue = 1.22f;
            serialized.FindProperty("_maximumPresentationDisplacement").floatValue = 0.052f;
            serialized.FindProperty("_smoothing").floatValue = 18f;
            serialized.FindProperty("_fingerRestCurlDegrees").floatValue = 0f;
            serialized.FindProperty("_fingerRunCurlDegrees").floatValue = 0.8f;
            serialized.FindProperty("_fingerLandingCurlDegrees").floatValue = 1.1f;
            serialized.FindProperty("_thumbRestCurlDegrees").floatValue = 0f;
            serialized.FindProperty("_wristRunPitchDegrees").floatValue = 1.1f;
            serialized.FindProperty("_wristAirPitchDegrees").floatValue = 0.9f;
            serialized.FindProperty("_wristLandingPitchDegrees").floatValue = 1.4f;
            serialized.FindProperty("_boneSmoothing").floatValue = 20f;
            serialized.FindProperty("_jumpBlendTime").floatValue = 0.12f;
            serialized.FindProperty("_fallBlendTime").floatValue = 0.18f;
            serialized.FindProperty("_landingRecoveryDuration").floatValue = 0.24f;
            serialized.FindProperty("_maxLandingImpactMultiplier").floatValue = 1.25f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void WriteAudit(
            GlbDocument document,
            RiggedImportResult left,
            RiggedImportResult right)
        {
            Directory.CreateDirectory("Logs");
            var primitive = document.Root.meshes[0].primitives[0];
            var positions = document.Root.accessors[primitive.attributes.POSITION];
            var builder = new StringBuilder();
            builder.AppendLine("RYDER'S ROAD 0.7.3 surgical arm presentation recovery audit");
            builder.AppendLine($"Source GLB: {SourceGlbPath}");
            builder.AppendLine($"Copied source asset: {SourceAssetPath}");
            builder.AppendLine("Import path: editor-only GLB decode to Unity Mesh/SkinnedMeshRenderer prefab; no runtime glTF dependency.");
            builder.AppendLine($"Mesh vertices: {positions.count}");
            builder.AppendLine($"Mesh triangles: {document.ReadIndexAccessor(primitive.indices).Length / 3}");
            builder.AppendLine($"Mesh bounds min: {Format(positions.min)}");
            builder.AppendLine($"Mesh bounds max: {Format(positions.max)}");
            builder.AppendLine($"Materials: {document.Root.materials?.Length ?? 0}");
            builder.AppendLine($"Textures/images: {document.Root.textures?.Length ?? 0}/{document.Root.images?.Length ?? 0}");
            builder.AppendLine($"Skin count: {document.Root.skins?.Length ?? 0}");
            builder.AppendLine($"Joint count: {document.Root.skins?[0].joints?.Length ?? 0}");
            builder.AppendLine($"Animations in GLB: {document.Root.animations?.Length ?? 0}");
            builder.AppendLine($"Right rig prefab: {right.PrefabPath}");
            builder.AppendLine($"Left rig prefab: {left.PrefabPath}");
            builder.AppendLine("Handedness: right prefab uses the Ryder Arm V2 source rig/mesh; left prefab is a baked X-mirrored mesh plus mirrored bone hierarchy. Runtime scales remain positive.");
            builder.AppendLine("Material: base color, normal, and emissive textures are assigned by glTF role. Metallic/roughness is imported as linear data but not used as a Unity metallic-gloss map. Tangents are generated for both meshes and backface culling is enabled.");
            builder.AppendLine($"RigCorrectionRoot local position/euler/scale: {RigCorrectionLocalPosition} / (0, 0, 0) / {RigCorrectionLocalScale}.");
            builder.AppendLine("New camera-space anchors derived for the full upper-arm/elbow anatomy:");
            builder.AppendLine($"Left position/euler: {LeftBasePosition} / {LeftBaseEulerAngles}.");
            builder.AppendLine($"Right position/euler: {RightBasePosition} / {RightBaseEulerAngles}.");
            builder.AppendLine("Shared presentation scale: (0.36, 0.36, 0.36).");
            builder.AppendLine("Prefab visual offsets are identity; camera placement and skeleton pose remain separate.");
            builder.AppendLine($"Canonical elbow bend: Bone_003 receives {ElbowBendDegrees} degrees around local X.");
            builder.AppendLine($"Canonical wrist pronation: Bone_002 receives mirrored +/-{WristPronationDegrees} degrees around local Y.");
            builder.AppendLine("Canonical anatomy: Bone_000 is upper arm, Bone_003 is elbow, Bone_002 is forearm/wrist roll, and Bone_001 is palm/hand.");
            builder.AppendLine("Relaxed digit curls in thumb/index/middle/ring/pinky order: 2 / 5 / 6 / 7 / 8 degrees with 1.0 / 0.75 / 0.5 / 0.25 joint weighting.");
            builder.AppendLine("Runtime locomotion is additive around this canonical base: subtle idle, alternating fore/aft run, outward-only lateral clearance, high-speed clamp, jump/rise/fall, and landing compression.");
            builder.AppendLine("No synthetic forearm sleeve is used; the source's full upper-arm geometry exits below the camera edge.");
            builder.AppendLine("Fallback: previous Meshy rigged assets remain under Assets/_Game/Art/FirstPerson/Arms/Rigged and the approved unrigged fallback remains under LegacyApproved. Neither fallback is active in production mode.");
            builder.AppendLine("Finger chains:");
            foreach (var chain in FingerChains)
            {
                builder.AppendLine("- " + string.Join(" -> ", chain));
            }

            File.WriteAllText(AuditPath, builder.ToString());
        }

        private static int TextureSourceIndex(GlbDocument document, int textureIndex)
        {
            var textures = document.Root.textures;
            if (textures == null || textureIndex < 0 || textureIndex >= textures.Length)
            {
                return -1;
            }

            return textures[textureIndex].source;
        }

        private static string Format(float[] values)
        {
            if (values == null)
            {
                return "(missing)";
            }

            return "(" + string.Join(
                ", ",
                values.Select(value => value.ToString("0.#####", CultureInfo.InvariantCulture))) + ")";
        }

        private readonly struct RiggedImportResult
        {
            public RiggedImportResult(string prefabPath, string meshPath, int boneCount)
            {
                PrefabPath = prefabPath;
                MeshPath = meshPath;
                BoneCount = boneCount;
            }

            public string PrefabPath { get; }
            public string MeshPath { get; }
            public int BoneCount { get; }
        }

        private sealed class ExtractedTextures
        {
            private readonly GltfTexture[] _textures;
            private readonly string[] _imagePaths;

            public ExtractedTextures(GltfTexture[] textures, string[] imagePaths)
            {
                _textures = textures ?? Array.Empty<GltfTexture>();
                _imagePaths = imagePaths ?? Array.Empty<string>();
            }

            public Texture TextureFor(int textureIndex)
            {
                if (textureIndex < 0 || textureIndex >= _textures.Length)
                {
                    return null;
                }

                var source = _textures[textureIndex].source;
                if (source < 0 || source >= _imagePaths.Length)
                {
                    return null;
                }

                return AssetDatabase.LoadAssetAtPath<Texture>(_imagePaths[source]);
            }
        }

        private sealed class GlbDocument
        {
            private readonly byte[] _bytes;
            private readonly int _binStart;

            private GlbDocument(GltfRoot root, byte[] bytes, int binStart)
            {
                Root = root;
                _bytes = bytes;
                _binStart = binStart;
            }

            public GltfRoot Root { get; }

            public static GlbDocument Load(string path)
            {
                var bytes = File.ReadAllBytes(path);
                if (Encoding.ASCII.GetString(bytes, 0, 4) != "glTF")
                {
                    throw new InvalidDataException("Source is not a GLB file.");
                }

                var offset = 12;
                string json = null;
                var binStart = -1;
                while (offset < bytes.Length)
                {
                    var chunkLength = BitConverter.ToInt32(bytes, offset);
                    var chunkType = Encoding.ASCII.GetString(bytes, offset + 4, 4);
                    if (chunkType == "JSON")
                    {
                        json = Encoding.UTF8.GetString(bytes, offset + 8, chunkLength).TrimEnd('\0', ' ');
                    }
                    else if (chunkType.StartsWith("BIN", StringComparison.Ordinal))
                    {
                        binStart = offset + 8;
                    }

                    offset += 8 + chunkLength;
                }

                if (string.IsNullOrWhiteSpace(json) || binStart < 0)
                {
                    throw new InvalidDataException("GLB must contain JSON and BIN chunks.");
                }

                return new GlbDocument(JsonUtility.FromJson<GltfRoot>(json), bytes, binStart);
            }

            public byte[] BufferViewBytes(int bufferViewIndex)
            {
                var bufferView = Root.bufferViews[bufferViewIndex];
                var bytes = new byte[bufferView.byteLength];
                Buffer.BlockCopy(_bytes, _binStart + bufferView.byteOffset, bytes, 0, bufferView.byteLength);
                return bytes;
            }

            public Vector3[] ReadVector3Accessor(int accessorIndex, bool mirrorX)
            {
                var accessor = Root.accessors[accessorIndex];
                var values = new Vector3[accessor.count];
                for (var index = 0; index < values.Length; index++)
                {
                    var x = ReadFloat(accessorIndex, index, 0);
                    values[index] = new Vector3(mirrorX ? -x : x, ReadFloat(accessorIndex, index, 1), ReadFloat(accessorIndex, index, 2));
                }

                return values;
            }

            public Vector2[] ReadVector2Accessor(int accessorIndex)
            {
                var accessor = Root.accessors[accessorIndex];
                var values = new Vector2[accessor.count];
                for (var index = 0; index < values.Length; index++)
                {
                    values[index] = new Vector2(ReadFloat(accessorIndex, index, 0), 1f - ReadFloat(accessorIndex, index, 1));
                }

                return values;
            }

            public Vector4[] ReadVector4Accessor(int accessorIndex)
            {
                var accessor = Root.accessors[accessorIndex];
                var values = new Vector4[accessor.count];
                for (var index = 0; index < values.Length; index++)
                {
                    values[index] = new Vector4(
                        ReadFloat(accessorIndex, index, 0),
                        ReadFloat(accessorIndex, index, 1),
                        ReadFloat(accessorIndex, index, 2),
                        ReadFloat(accessorIndex, index, 3));
                }

                return values;
            }

            public Int4[] ReadVector4IntAccessor(int accessorIndex)
            {
                var accessor = Root.accessors[accessorIndex];
                var values = new Int4[accessor.count];
                for (var index = 0; index < values.Length; index++)
                {
                    values[index] = new Int4(
                        ReadInt(accessorIndex, index, 0),
                        ReadInt(accessorIndex, index, 1),
                        ReadInt(accessorIndex, index, 2),
                        ReadInt(accessorIndex, index, 3));
                }

                return values;
            }

            public int[] ReadIndexAccessor(int accessorIndex)
            {
                var accessor = Root.accessors[accessorIndex];
                var indices = new int[accessor.count];
                for (var index = 0; index < indices.Length; index++)
                {
                    indices[index] = ReadInt(accessorIndex, index, 0);
                }

                return indices;
            }

            private float ReadFloat(int accessorIndex, int elementIndex, int componentIndex)
            {
                var offset = ComponentOffset(accessorIndex, elementIndex, componentIndex, out var componentType);
                return componentType == 5126
                    ? BitConverter.ToSingle(_bytes, offset)
                    : ReadIntFromBytes(componentType, offset);
            }

            private int ReadInt(int accessorIndex, int elementIndex, int componentIndex)
            {
                var offset = ComponentOffset(accessorIndex, elementIndex, componentIndex, out var componentType);
                return ReadIntFromBytes(componentType, offset);
            }

            private int ComponentOffset(
                int accessorIndex,
                int elementIndex,
                int componentIndex,
                out int componentType)
            {
                var accessor = Root.accessors[accessorIndex];
                var bufferView = Root.bufferViews[accessor.bufferView];
                componentType = accessor.componentType;
                var componentSize = ComponentSize(componentType);
                var stride = bufferView.byteStride > 0 ? bufferView.byteStride : componentSize * ComponentCount(accessor.type);
                return _binStart
                    + bufferView.byteOffset
                    + accessor.byteOffset
                    + elementIndex * stride
                    + componentIndex * componentSize;
            }

            private int ReadIntFromBytes(int componentType, int offset)
            {
                return componentType switch
                {
                    5120 => (sbyte)_bytes[offset],
                    5121 => _bytes[offset],
                    5122 => BitConverter.ToInt16(_bytes, offset),
                    5123 => BitConverter.ToUInt16(_bytes, offset),
                    5125 => (int)BitConverter.ToUInt32(_bytes, offset),
                    _ => throw new NotSupportedException($"Unsupported integer component type {componentType}.")
                };
            }

            private static int ComponentSize(int componentType)
            {
                return componentType switch
                {
                    5120 or 5121 => 1,
                    5122 or 5123 => 2,
                    5125 or 5126 => 4,
                    _ => throw new NotSupportedException($"Unsupported component type {componentType}.")
                };
            }

            private static int ComponentCount(string type)
            {
                return type switch
                {
                    "SCALAR" => 1,
                    "VEC2" => 2,
                    "VEC3" => 3,
                    "VEC4" => 4,
                    "MAT4" => 16,
                    _ => throw new NotSupportedException($"Unsupported accessor type {type}.")
                };
            }
        }

        [Serializable]
        private sealed class GltfRoot
        {
            public int scene;
            public GltfScene[] scenes;
            public GltfNode[] nodes;
            public GltfMesh[] meshes;
            public GltfSkin[] skins;
            public GltfAccessor[] accessors;
            public GltfBufferView[] bufferViews;
            public GltfMaterial[] materials;
            public GltfTexture[] textures;
            public GltfImage[] images;
            public GltfAnimation[] animations;
        }

        [Serializable]
        private sealed class GltfScene
        {
            public int[] nodes;
        }

        [Serializable]
        private sealed class GltfNode
        {
            public string name;
            public int mesh = -1;
            public int skin = -1;
            public int[] children;
            public float[] translation;
            public float[] rotation;
            public float[] scale;
        }

        [Serializable]
        private sealed class GltfMesh
        {
            public string name;
            public GltfPrimitive[] primitives;
        }

        [Serializable]
        private sealed class GltfPrimitive
        {
            public GltfAttributes attributes;
            public int indices;
            public int material;
        }

        [Serializable]
        private sealed class GltfAttributes
        {
            public int POSITION;
            public int NORMAL;
            public int TEXCOORD_0;
            public int JOINTS_0;
            public int JOINTS_1;
            public int WEIGHTS_0;
            public int WEIGHTS_1;
        }

        [Serializable]
        private sealed class GltfSkin
        {
            public string name;
            public int[] joints;
            public int inverseBindMatrices;
        }

        [Serializable]
        private sealed class GltfAccessor
        {
            public int bufferView;
            public int byteOffset;
            public int componentType;
            public int count;
            public string type;
            public float[] min;
            public float[] max;
        }

        [Serializable]
        private sealed class GltfBufferView
        {
            public int buffer;
            public int byteOffset;
            public int byteLength;
            public int byteStride;
        }

        [Serializable]
        private sealed class GltfMaterial
        {
            public string name;
            public bool doubleSided;
            public float[] emissiveFactor;
            public GltfTextureInfo emissiveTexture;
            public GltfNormalTexture normalTexture;
            public GltfPbrMetallicRoughness pbrMetallicRoughness;
        }

        [Serializable]
        private sealed class GltfPbrMetallicRoughness
        {
            public GltfTextureInfo baseColorTexture;
            public GltfTextureInfo metallicRoughnessTexture;
        }

        [Serializable]
        private sealed class GltfTextureInfo
        {
            public int index = -1;
        }

        [Serializable]
        private sealed class GltfNormalTexture
        {
            public int index = -1;
        }

        [Serializable]
        private sealed class GltfTexture
        {
            public int source = -1;
        }

        [Serializable]
        private sealed class GltfImage
        {
            public string name;
            public string mimeType;
            public int bufferView;
        }

        [Serializable]
        private sealed class GltfAnimation
        {
            public string name;
        }

        private readonly struct Int4
        {
            public Int4(int x, int y, int z, int w)
            {
                this.x = x;
                this.y = y;
                this.z = z;
                this.w = w;
            }

            public readonly int x;
            public readonly int y;
            public readonly int z;
            public readonly int w;
        }
    }
}
