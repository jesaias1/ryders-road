using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Avoidance.Gameplay.Visuals;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Avoidance.EditorTools
{
    public static class MeshyArtKitIntegrator
    {
        private const string ArtRoot = "Assets/_Game/Art";
        private const string SourceRoot = ArtRoot + "/MeshySource";
        private const string TextureRoot = ArtRoot + "/Textures";
        private const string MaterialRoot = ArtRoot + "/Materials";
        private const string GeneratedRoot = ArtRoot + "/Generated";
        private const string ManifestPath = "ART_ASSET_MANIFEST.md";
        private const string GalleryScenePath = "Assets/_Game/Levels/Scenes/ArtKitGallery.unity";
        private const string PrefabLibraryPath = "Assets/_Game/Visuals/Resources/ModuleVisualPrefabLibrary.asset";
        private const string ArmProfilePath = "Assets/_Game/Visuals/Resources/FirstPersonArmProfile.asset";
        private const string ArmSourceKey = "Meshy_AI_Neon_Vanguard_Gauntle";
        private const string LegacyHighPolyArmSourceKey = "Meshy_AI_mech_gauntlet";
        private const string ArmCanonicalName = "RR_FP_Arm_Right";
        private const string LeftArmCanonicalName = "RR_FP_Arm_Left";
        private const int MaximumRuntimeArmTriangles = 50000;
        private const int MaximumCanonicalBlockTriangles = 25000;
        private const int MaximumCrumbleStageTriangles = 50000;
        private const string SafeBlockPrefabPath =
            ArtRoot + "/Environment/Platforms/PF_RR_Block_Safe.prefab";
        private const string Phase057BlockAuditPath =
            "Logs/phase057-canonical-block-audit.txt";
        private const string Phase059CrumbleAuditPath =
            "Logs/phase059-crumble-asset-audit.txt";
        private const string CrumbleVisualSetPath =
            "Assets/_Game/Visuals/Resources/CrumbleVisualSet.asset";
        private static readonly ArtEntry StandardBlockEntry = new ArtEntry(
            "Meshy_AI_MedTech_Supply_Crate",
            "RR_Block_Standard",
            "Standard parkour block",
            "Environment/Platforms",
            1024,
            true);
        private static readonly ArtEntry ArmEntry = new ArtEntry(
            ArmSourceKey,
            ArmCanonicalName,
            "First-person right arm/gauntlet",
            "FirstPerson/Arms",
            2048,
            true);
        private static readonly ArtEntry[] CrumbleStageEntries =
        {
            new ArtEntry(
                "Meshy_AI_Chrono_Core_Crate",
                "RR_Crumble_Stage1",
                "Crumble intact stage",
                "Environment/Platforms/Crumble",
                1024,
                false),
            new ArtEntry(
                "Meshy_AI_Fractured_Power_Core",
                "RR_Crumble_Stage2",
                "Crumble fractured stage",
                "Environment/Platforms/Crumble",
                1024,
                false),
            new ArtEntry(
                "Meshy_AI_Molten_Ruins",
                "RR_Crumble_Stage3",
                "Crumble critical stage",
                "Environment/Platforms/Crumble",
                1024,
                false)
        };

        private static readonly ArtEntry[] Entries =
        {
            StandardBlockEntry,
            new ArtEntry("Meshy_AI_Aetherstone_Platform", "RR_Platform_Long", "Long parkour platform", "Environment/Platforms", 1024, true),
            new ArtEntry("Meshy_AI_Aether_Pillar", "RR_Pillar_Tall", "Tall floating pillar", "Environment/Architecture", 1024, true),
            new ArtEntry("Meshy_AI_Clay_Meadow_Island", "RR_Rock_Broken", "Broken floating rock chunk", "Environment/Decoration", 1024, false),
            new ArtEntry("Meshy_AI_Elderwood_Isle", "RR_Tree_Floating", "Stylized floating tree/island", "Environment/Nature", 1024, false),
            new ArtEntry("Meshy_AI_Azure_Sentinel_Obelis", "RR_GrassTopper", "Grass/flower island topper", "Environment/Nature", 1024, false),
            new ArtEntry("Meshy_AI_Glacier_Core_Chest", "RR_RestorePoint", "Restore Point", "Gameplay/Restore", 2048, true),
            new ArtEntry("Meshy_AI_Neon_Launch_Pad", "RR_BoostPad", "Boost Pad", "Gameplay/Boost", 2048, true),
            new ArtEntry("Meshy_AI_Aetherstone_Relic", "RR_PatchBlock", "Patch Block", "Gameplay/Patch", 2048, true),
            new ArtEntry("Meshy_AI_Arcane_Ruins", "RR_RuinedTower", "Ruined tower segment", "Environment/Architecture", 1024, true),
            new ArtEntry("Meshy_AI_Luminous_Portal_Ruins", "RR_FloatingArch", "Floating arch", "Environment/Architecture", 1024, true),
            new ArtEntry("Meshy_AI_Aether_Crystal_Nexus", "RR_EnergyPillar", "Decorative cyan energy pillar", "Environment/Decoration", 2048, true)
        };

        [MenuItem("RYDERS BLOCK/Art/Integrate Meshy Art Kit")]
        public static void IntegrateMeshyArtKit()
        {
            EnsureDirectories();
            AssetDatabase.Refresh();

            var report = new List<AssetReport>();
            var prefabs = new Dictionary<string, GameObject>(StringComparer.Ordinal);
            foreach (var entry in Entries)
            {
                var sourceFbx = FindSourceFbx(entry.SourceKey);
                if (string.IsNullOrEmpty(sourceFbx))
                {
                    throw new FileNotFoundException("Missing Meshy FBX for " + entry.CanonicalName);
                }

                ConfigureModelImporter(sourceFbx, entry);
                var textures = CopyAndConfigureTextures(sourceFbx, entry);
                var material = CreateOrUpdateMaterial(entry, textures);
                var prefab = CreateProductionPrefab(entry, sourceFbx, material);
                prefabs[entry.CanonicalName] = prefab;
                report.Add(InspectAsset(entry, sourceFbx, prefab, material, textures));
            }

            prefabs["RR_Block_Safe"] = CreateSafeBlockVariant(prefabs["RR_Block_Standard"]);

            var armIntegration = TryIntegrateOptimizedArmCandidate();
            var armReference = armIntegration.Report;
            ConfigureModulePrefabLibrary(prefabs);
            IntegrateSkyboxAsset();
            CreateGalleryScene(prefabs, armIntegration);
            WriteManifest(report, armReference, armIntegration);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("RYDER'S ROAD Meshy art kit integration complete.");
        }

        [MenuItem("RYDERS BLOCK/Art/Integrate Phase 0.5.7 Canonical Block")]
        public static void IntegratePhase057CanonicalBlock()
        {
            EnsureDirectories();
            Directory.CreateDirectory("Logs");
            AssetDatabase.Refresh();

            var sourceFbx = FindSourceFbx(StandardBlockEntry.SourceKey);
            if (string.IsNullOrEmpty(sourceFbx))
            {
                throw new FileNotFoundException(
                    "Missing supplied Phase 0.5.7 Standard block FBX.");
            }

            ConfigureModelImporter(sourceFbx, StandardBlockEntry);
            var meshes = AssetDatabase.LoadAllAssetsAtPath(sourceFbx).OfType<Mesh>().ToArray();
            var triangleCount = meshes.Sum(mesh => mesh.triangles.Length / 3);
            if (triangleCount > MaximumCanonicalBlockTriangles)
            {
                WriteCanonicalBlockAudit(sourceFbx, null, null, triangleCount);
                throw new InvalidOperationException(
                    $"Supplied Standard block has {triangleCount.ToString(CultureInfo.InvariantCulture)} triangles; "
                    + $"runtime limit is {MaximumCanonicalBlockTriangles.ToString(CultureInfo.InvariantCulture)}.");
            }

            var textures = CopyAndConfigureTextures(sourceFbx, StandardBlockEntry);
            var material = CreateOrUpdateMaterial(StandardBlockEntry, textures);
            var standardPrefab = CreateProductionPrefab(
                StandardBlockEntry,
                sourceFbx,
                material);
            var safePrefab = CreateSafeBlockVariant(standardPrefab);
            var prefabs = LoadCurrentEnvironmentPrefabs();
            prefabs["RR_Block_Standard"] = standardPrefab;
            prefabs["RR_Block_Safe"] = safePrefab;
            ConfigureModulePrefabLibrary(prefabs);
            WriteCanonicalBlockAudit(sourceFbx, standardPrefab, safePrefab, triangleCount);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log(
                $"RYDER'S ROAD 0.5.7 canonical block integrated: {triangleCount.ToString(CultureInfo.InvariantCulture)} triangles.");
        }

        [MenuItem("RYDERS BLOCK/Art/Integrate Phase 0.5.9 Crumble Stages")]
        public static void IntegratePhase059CrumbleStages()
        {
            EnsureDirectories();
            Directory.CreateDirectory("Logs");
            AssetDatabase.Refresh();

            var reports = new List<AssetReport>();
            var prefabs = new List<GameObject>();
            foreach (var entry in CrumbleStageEntries)
            {
                var sourceFbx = FindSourceFbx(entry.SourceKey);
                if (string.IsNullOrEmpty(sourceFbx))
                {
                    throw new FileNotFoundException("Missing supplied crumble FBX for " + entry.CanonicalName);
                }

                ConfigureModelImporter(sourceFbx, entry);
                var meshes = AssetDatabase.LoadAllAssetsAtPath(sourceFbx).OfType<Mesh>().ToArray();
                var triangleCount = meshes.Sum(mesh => mesh.triangles.Length / 3);
                if (triangleCount > MaximumCrumbleStageTriangles)
                {
                    throw new InvalidOperationException(
                        $"{entry.CanonicalName} has {triangleCount.ToString(CultureInfo.InvariantCulture)} triangles; "
                        + $"runtime limit is {MaximumCrumbleStageTriangles.ToString(CultureInfo.InvariantCulture)}.");
                }

                var textures = CopyAndConfigureTextures(sourceFbx, entry);
                var material = CreateOrUpdateMaterial(entry, textures);
                var prefab = CreateProductionPrefab(entry, sourceFbx, material);
                prefabs.Add(prefab);
                reports.Add(InspectAsset(entry, sourceFbx, prefab, material, textures));
            }

            ConfigureCrumbleVisualSet(prefabs);
            WriteCrumbleStageAudit(reports);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("RYDER'S ROAD 0.5.9 crumble stages integrated.");
        }

        [MenuItem("RYDERS BLOCK/Art/Report First Person Arm Candidate")]
        public static void ReportFirstPersonArmCandidate()
        {
            EnsureDirectories();
            AssetDatabase.Refresh();
            var report = InspectArmCandidate();
            WriteArmCandidateReport(report);
            Debug.Log(
                $"RR_FP_Arm_Right candidate triangles: {report.TriangleCount.ToString(CultureInfo.InvariantCulture)}; " +
                $"source: {report.SourceFbx}; status: {report.Status}");
        }

        [MenuItem("RYDERS BLOCK/Art/Integrate First Person Arm Candidate")]
        public static void IntegrateFirstPersonArmCandidate()
        {
            EnsureDirectories();
            AssetDatabase.Refresh();
            var armIntegration = TryIntegrateOptimizedArmCandidate();
            WriteManifest(InspectCurrentEnvironmentAssets(), armIntegration.Report, armIntegration);
            CreateGalleryScene(LoadCurrentEnvironmentPrefabs(), armIntegration);
            WriteArmCandidateReport(armIntegration.Report);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("RYDER'S ROAD first-person arm candidate integration complete.");
        }

        private static void EnsureDirectories()
        {
            var directories = new[]
            {
                ArtRoot,
                SourceRoot,
                TextureRoot,
                MaterialRoot,
                GeneratedRoot,
                GeneratedRoot + "/EmissionMasks",
                GeneratedRoot + "/FirstPerson",
                $"{ArtRoot}/FirstPerson/Arms",
                $"{ArtRoot}/Environment/Platforms/Crumble",
                "Assets/_Game/Visuals/Resources",
                "Assets/_Game/Levels/Scenes"
            };
            foreach (var entry in Entries)
            {
                directories = directories.Append(ArtRoot + "/" + entry.CategoryPath).ToArray();
            }

            foreach (var directory in directories.Distinct(StringComparer.Ordinal))
            {
                Directory.CreateDirectory(directory);
            }
        }

        private static string FindSourceFbx(string sourceKey)
        {
            return AssetDatabase.FindAssets("t:Model", new[] { SourceRoot })
                .Select(AssetDatabase.GUIDToAssetPath)
                .FirstOrDefault(path =>
                    path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase)
                    && path.IndexOf(sourceKey, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static void ConfigureModelImporter(string fbxPath, ArtEntry entry)
        {
            var importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
            if (importer == null)
            {
                return;
            }

            importer.importAnimation = false;
            importer.animationType = ModelImporterAnimationType.None;
            importer.importCameras = false;
            importer.importLights = false;
            importer.importBlendShapes = false;
            importer.importVisibility = false;
            importer.importConstraints = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.isReadable = false;
            importer.meshCompression = ModelImporterMeshCompression.Off;
            importer.optimizeMeshPolygons = true;
            importer.optimizeMeshVertices = true;
            importer.SaveAndReimport();
        }

        private static TextureSet CopyAndConfigureTextures(string fbxPath, ArtEntry entry)
        {
            var sourceDirectory = Path.GetDirectoryName(fbxPath);
            var set = new TextureSet();
            foreach (var source in Directory.GetFiles(sourceDirectory, "*.png"))
            {
                var suffix = TextureSuffix(source);
                var destination = $"{TextureRoot}/{entry.CanonicalName}_{suffix}.png";
                File.Copy(source, destination, overwrite: true);
                AssetDatabase.ImportAsset(destination, ImportAssetOptions.ForceSynchronousImport);
                ConfigureTextureImporter(destination, suffix, entry.RuntimeMaxTextureSize);

                switch (suffix)
                {
                    case "Base":
                        set.BaseMap = destination;
                        break;
                    case "Normal":
                        set.NormalMap = destination;
                        break;
                    case "Metallic":
                        set.MetallicMap = destination;
                        break;
                    case "Roughness":
                        set.RoughnessMap = destination;
                        break;
                }
            }

            if (!string.IsNullOrEmpty(set.BaseMap) && entry.CyanEmission)
            {
                set.EmissionMap = CreateCyanEmissionMask(set.BaseMap, entry.CanonicalName, entry.RuntimeMaxTextureSize);
            }

            return set;
        }

        private static string TextureSuffix(string path)
        {
            var name = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
            if (name.Contains("normal"))
            {
                return "Normal";
            }

            if (name.Contains("metallic"))
            {
                return "Metallic";
            }

            if (name.Contains("roughness"))
            {
                return "Roughness";
            }

            return "Base";
        }

        private static void ConfigureTextureImporter(string path, string suffix, int maxSize)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            importer.textureType = suffix == "Normal"
                ? TextureImporterType.NormalMap
                : TextureImporterType.Default;
            importer.sRGBTexture = suffix == "Base";
            importer.mipmapEnabled = true;
            importer.isReadable = false;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.maxTextureSize = maxSize;

            var android = new TextureImporterPlatformSettings
            {
                name = "Android",
                overridden = true,
                maxTextureSize = maxSize,
                format = TextureImporterFormat.ASTC_6x6,
                compressionQuality = 80
            };
            importer.SetPlatformTextureSettings(android);
            importer.SaveAndReimport();
        }

        private static string CreateCyanEmissionMask(string baseMapPath, string canonicalName, int maxSize)
        {
            var importer = AssetImporter.GetAtPath(baseMapPath) as TextureImporter;
            if (importer == null)
            {
                return string.Empty;
            }

            importer.isReadable = true;
            importer.SaveAndReimport();
            var baseTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(baseMapPath);
            var mask = new Texture2D(baseTexture.width, baseTexture.height, TextureFormat.RGBA32, false, true);
            var sourcePixels = baseTexture.GetPixels32();
            var outputPixels = new Color32[sourcePixels.Length];
            for (var index = 0; index < sourcePixels.Length; index++)
            {
                var pixel = sourcePixels[index];
                var cyan = Mathf.Clamp01(((pixel.g / 255f) + (pixel.b / 255f)) * 0.5f - pixel.r / 255f * 0.7f);
                var alpha = (byte)Mathf.RoundToInt(Mathf.Pow(cyan, 1.8f) * 255f);
                outputPixels[index] = new Color32(alpha, alpha, alpha, alpha);
            }

            mask.SetPixels32(outputPixels);
            mask.Apply(false, false);
            var path = $"{GeneratedRoot}/EmissionMasks/{canonicalName}_CyanEmission.png";
            File.WriteAllBytes(path, mask.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(mask);

            importer.isReadable = false;
            importer.SaveAndReimport();
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            ConfigureTextureImporter(path, "Emission", maxSize);
            return path;
        }

        private static Material CreateOrUpdateMaterial(ArtEntry entry, TextureSet textures)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var path = $"{MaterialRoot}/MAT_{entry.CanonicalName}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            else
            {
                material.shader = shader;
            }

            material.name = "MAT_" + entry.CanonicalName;
            SetTexture(material, "_BaseMap", textures.BaseMap);
            SetTexture(material, "_BumpMap", textures.NormalMap);
            SetTexture(material, "_MetallicGlossMap", textures.MetallicMap);
            if (!string.IsNullOrEmpty(textures.NormalMap))
            {
                material.EnableKeyword("_NORMALMAP");
            }

            material.SetFloat("_Metallic", entry.CanonicalName.Contains("Arm", StringComparison.Ordinal) ? 0.15f : 0.05f);
            material.SetFloat("_Smoothness", 0.38f);
            if (!string.IsNullOrEmpty(textures.EmissionMap))
            {
                SetTexture(material, "_EmissionMap", textures.EmissionMap);
                material.SetColor("_EmissionColor", new Color(0.05f, 0.85f, 1f) * 1.7f);
                material.EnableKeyword("_EMISSION");
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            else
            {
                material.SetColor("_EmissionColor", Color.black);
                material.DisableKeyword("_EMISSION");
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static void SetTexture(Material material, string property, string texturePath)
        {
            if (!material.HasProperty(property) || string.IsNullOrEmpty(texturePath))
            {
                return;
            }

            material.SetTexture(property, AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath));
        }

        private static GameObject CreateProductionPrefab(ArtEntry entry, string fbxPath, Material material)
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            var root = new GameObject("PF_" + entry.CanonicalName);
            var child = (GameObject)PrefabUtility.InstantiatePrefab(model);
            child.name = entry.CanonicalName + "_Model";
            child.transform.SetParent(root.transform, false);
            PrefabUtility.UnpackPrefabInstance(child, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            AssignMaterialAndClean(root, material);
            if (entry.CanonicalName == StandardBlockEntry.CanonicalName
                || entry.CanonicalName.StartsWith("RR_Crumble_", StringComparison.Ordinal))
            {
                NormalizeToHorizontalUnitTop(root);
            }
            else
            {
                NormalizeToUnitBounds(root, uniform: true);
            }
            var prefabPath = $"{ArtRoot}/{entry.CategoryPath}/PF_{entry.CanonicalName}.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static void ConfigureCrumbleVisualSet(IReadOnlyList<GameObject> prefabs)
        {
            if (prefabs == null || prefabs.Count != 3 || prefabs.Any(prefab => prefab == null))
            {
                throw new InvalidOperationException("All three crumble stage prefabs are required.");
            }

            var visualSet = AssetDatabase.LoadAssetAtPath<CrumbleVisualSet>(CrumbleVisualSetPath);
            if (visualSet == null)
            {
                visualSet = ScriptableObject.CreateInstance<CrumbleVisualSet>();
                AssetDatabase.CreateAsset(visualSet, CrumbleVisualSetPath);
            }

            var serialized = new SerializedObject(visualSet);
            serialized.FindProperty("_stage1Prefab").objectReferenceValue = prefabs[0];
            serialized.FindProperty("_stage2Prefab").objectReferenceValue = prefabs[1];
            serialized.FindProperty("_stage3Prefab").objectReferenceValue = prefabs[2];
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(visualSet);
        }

        private static void WriteCrumbleStageAudit(IReadOnlyList<AssetReport> reports)
        {
            var builder = new StringBuilder();
            builder.AppendLine("RYDER'S ROAD - PHASE 0.5.9 CRUMBLE ASSET AUDIT");
            builder.AppendLine("Runtime triangle limit per stage: "
                + MaximumCrumbleStageTriangles.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("Authoritative collision: primitive box on gameplay root; imported meshes are visual-only.");
            builder.AppendLine();
            foreach (var report in reports)
            {
                builder.AppendLine(report.Entry.CanonicalName);
                builder.AppendLine("  source: " + report.SourceFbx);
                builder.AppendLine("  prefab: " + report.PrefabPath);
                builder.AppendLine("  material: " + report.MaterialPath);
                builder.AppendLine("  triangles: " + report.TriangleCount.ToString(CultureInfo.InvariantCulture));
                builder.AppendLine("  vertices: " + report.VertexCount.ToString(CultureInfo.InvariantCulture));
                builder.AppendLine("  submeshes: " + report.SubmeshCount.ToString(CultureInfo.InvariantCulture));
                builder.AppendLine("  runtime base texture: " + report.TextureSize);
                builder.AppendLine();
            }

            File.WriteAllText(Phase059CrumbleAuditPath, builder.ToString());
        }

        private static GameObject CreateSafeBlockVariant(GameObject standardPrefab)
        {
            if (standardPrefab == null)
            {
                throw new MissingReferenceException("Canonical Standard block prefab is unavailable.");
            }

            var root = new GameObject("PF_RR_Block_Safe");
            var visual = (GameObject)PrefabUtility.InstantiatePrefab(standardPrefab);
            visual.name = "RR_Block_Safe_Model";
            visual.transform.SetParent(root.transform, false);
            PrefabUtility.UnpackPrefabInstance(
                visual,
                PrefabUnpackMode.Completely,
                InteractionMode.AutomatedAction);
            AssignMaterialAndClean(
                root,
                AssetDatabase.LoadAssetAtPath<Material>(
                    $"{MaterialRoot}/MAT_{StandardBlockEntry.CanonicalName}.mat"));
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, SafeBlockPrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static ArmIntegrationResult TryIntegrateOptimizedArmCandidate()
        {
            var report = InspectArmCandidate();
            WriteArmCandidateReport(report);
            if (string.IsNullOrEmpty(report.SourceFbx) || report.TriangleCount > MaximumRuntimeArmTriangles)
            {
                DeleteGeneratedArmRuntimeAssets();
                return new ArmIntegrationResult(report);
            }

            ConfigureArmProductionImporter(report.SourceFbx);
            DeleteGeneratedArmRuntimeAssets();
            var textures = CopyAndConfigureTextures(report.SourceFbx, ArmEntry);
            var material = CreateOrUpdateMaterial(ArmEntry, textures);
            var result = CreateArmPrefabPair(report.SourceFbx, material);
            ConfigureArmProfile(result);
            report = new ArmReferenceReport(
                report.SourceFbx,
                report.TriangleCount,
                report.VertexCount,
                report.SubmeshCount,
                report.TextureSize,
                "Integrated runtime arms");
            return new ArmIntegrationResult(report, result.RightPrefabPath, result.LeftPrefabPath, result.MaterialPath, result.RightMeshAssetPath, result.LeftMeshAssetPath);
        }

        private static ArmIntegrationResult CreateArmPrefabPair(string fbxPath, Material material)
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            var rightRoot = CreateArmPrefabRoot(model, ArmCanonicalName, material);
            DuplicateRuntimeMeshes(rightRoot, $"{GeneratedRoot}/FirstPerson/{ArmCanonicalName}_00.asset", mirrorX: false);
            var rightPrefabPath = $"{ArtRoot}/FirstPerson/Arms/PF_{ArmCanonicalName}.prefab";
            var rightPrefab = PrefabUtility.SaveAsPrefabAsset(rightRoot, rightPrefabPath);
            UnityEngine.Object.DestroyImmediate(rightRoot);

            var leftRoot = (GameObject)PrefabUtility.InstantiatePrefab(rightPrefab);
            leftRoot.name = "PF_" + LeftArmCanonicalName;
            PrefabUtility.UnpackPrefabInstance(leftRoot, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            MirrorChildPositions(leftRoot.transform);
            DuplicateRuntimeMeshes(leftRoot, $"{GeneratedRoot}/FirstPerson/{LeftArmCanonicalName}_00.asset", mirrorX: true);
            EnsurePositiveLocalScales(leftRoot.transform);
            AssignMaterialAndClean(leftRoot, material);
            var leftPrefabPath = $"{ArtRoot}/FirstPerson/Arms/PF_{LeftArmCanonicalName}.prefab";
            PrefabUtility.SaveAsPrefabAsset(leftRoot, leftPrefabPath);
            UnityEngine.Object.DestroyImmediate(leftRoot);

            return new ArmIntegrationResult(
                new ArmReferenceReport(string.Empty, 0, 0, 0, string.Empty, "Integrated runtime arms"),
                rightPrefabPath,
                leftPrefabPath,
                AssetDatabase.GetAssetPath(material),
                $"{GeneratedRoot}/FirstPerson/{ArmCanonicalName}_00.asset",
                $"{GeneratedRoot}/FirstPerson/{LeftArmCanonicalName}_00.asset");
        }

        private static GameObject CreateArmPrefabRoot(GameObject model, string canonicalName, Material material)
        {
            var root = new GameObject("PF_" + canonicalName);
            var child = (GameObject)PrefabUtility.InstantiatePrefab(model);
            child.name = canonicalName + "_Model";
            child.transform.SetParent(root.transform, false);
            PrefabUtility.UnpackPrefabInstance(child, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            AssignMaterialAndClean(root, material);
            NormalizeToUnitBounds(root, uniform: true);
            EnsurePositiveLocalScales(root.transform);
            return root;
        }

        private static void DuplicateRuntimeMeshes(GameObject root, string assetPath, bool mirrorX)
        {
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath) != null)
            {
                AssetDatabase.DeleteAsset(assetPath);
            }

            var meshes = new List<Mesh>();
            foreach (var filter in root.GetComponentsInChildren<MeshFilter>(includeInactive: true))
            {
                if (filter.sharedMesh == null)
                {
                    continue;
                }

                var mesh = CloneRuntimeMesh(filter.sharedMesh, filter.name, mirrorX);
                filter.sharedMesh = mesh;
                meshes.Add(mesh);
            }

            foreach (var renderer in root.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true))
            {
                if (renderer.sharedMesh == null)
                {
                    continue;
                }

                var mesh = CloneRuntimeMesh(renderer.sharedMesh, renderer.name, mirrorX);
                renderer.sharedMesh = mesh;
                meshes.Add(mesh);
            }

            if (meshes.Count == 0)
            {
                throw new InvalidOperationException("Arm source has no meshes to generate.");
            }

            AssetDatabase.CreateAsset(meshes[0], assetPath);
            for (var index = 1; index < meshes.Count; index++)
            {
                AssetDatabase.AddObjectToAsset(meshes[index], assetPath);
            }
        }

        private static Mesh CloneRuntimeMesh(Mesh source, string ownerName, bool mirrorX)
        {
            var mesh = UnityEngine.Object.Instantiate(source);
            mesh.name = SanitizeAssetName(ownerName) + (mirrorX ? "_LeftMirrored" : "_Right");
            if (mirrorX)
            {
                MirrorMeshX(mesh);
            }

            mesh.RecalculateBounds();
            return mesh;
        }

        private static void MirrorMeshX(Mesh mesh)
        {
            var vertices = mesh.vertices;
            for (var index = 0; index < vertices.Length; index++)
            {
                vertices[index].x = -vertices[index].x;
            }

            mesh.vertices = vertices;

            var normals = mesh.normals;
            if (normals != null && normals.Length == vertices.Length)
            {
                for (var index = 0; index < normals.Length; index++)
                {
                    normals[index].x = -normals[index].x;
                }

                mesh.normals = normals;
            }

            var tangents = mesh.tangents;
            if (tangents != null && tangents.Length == vertices.Length)
            {
                for (var index = 0; index < tangents.Length; index++)
                {
                    tangents[index].x = -tangents[index].x;
                    tangents[index].w = -tangents[index].w;
                }

                mesh.tangents = tangents;
            }

            for (var submesh = 0; submesh < mesh.subMeshCount; submesh++)
            {
                var triangles = mesh.GetTriangles(submesh);
                for (var index = 0; index < triangles.Length; index += 3)
                {
                    (triangles[index], triangles[index + 2]) = (triangles[index + 2], triangles[index]);
                }

                mesh.SetTriangles(triangles, submesh);
            }
        }

        private static void MirrorChildPositions(Transform root)
        {
            foreach (Transform child in root)
            {
                var position = child.localPosition;
                position.x = -position.x;
                child.localPosition = position;
                MirrorChildPositions(child);
            }
        }

        private static void EnsurePositiveLocalScales(Transform root)
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(includeInactive: true))
            {
                child.localScale = new Vector3(
                    Mathf.Abs(child.localScale.x),
                    Mathf.Abs(child.localScale.y),
                    Mathf.Abs(child.localScale.z));
            }
        }

        private static string SanitizeAssetName(string value)
        {
            var builder = new StringBuilder(value.Length);
            foreach (var character in value)
            {
                builder.Append(char.IsLetterOrDigit(character) ? character : '_');
            }

            return builder.ToString();
        }

        private static void ConfigureArmProfile(ArmIntegrationResult result)
        {
            var profile = AssetDatabase.LoadAssetAtPath<FirstPersonArmProfile>(ArmProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<FirstPersonArmProfile>();
                AssetDatabase.CreateAsset(profile, ArmProfilePath);
            }

            var serialized = new SerializedObject(profile);
            serialized.FindProperty("_leftArmPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(result.LeftPrefabPath);
            serialized.FindProperty("_rightArmPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(result.RightPrefabPath);
            serialized.FindProperty("_leftBaseLocalPosition").vector3Value = new Vector3(-0.38f, -0.40f, 0.40f);
            serialized.FindProperty("_rightBaseLocalPosition").vector3Value = new Vector3(0.38f, -0.40f, 0.40f);
            serialized.FindProperty("_leftBaseLocalEulerAngles").vector3Value = new Vector3(348.34290f, 263.26410f, 257.66420f);
            serialized.FindProperty("_rightBaseLocalEulerAngles").vector3Value = new Vector3(348.34290f, 96.73593f, 102.33580f);
            serialized.FindProperty("_leftPrefabEulerOffset").vector3Value = Vector3.zero;
            serialized.FindProperty("_rightPrefabEulerOffset").vector3Value = Vector3.zero;
            serialized.FindProperty("_prefabLocalScale").vector3Value = Vector3.one * 0.38f;
            serialized.FindProperty("_neutralPoseLocked").boolValue = true;
            serialized.FindProperty("_idleFloatAmount").floatValue = 0.0018f;
            serialized.FindProperty("_runLateralAmount").floatValue = 0.018f;
            serialized.FindProperty("_runVerticalAmount").floatValue = 0.014f;
            serialized.FindProperty("_runForwardAmount").floatValue = 0.022f;
            serialized.FindProperty("_runFrequencyMin").floatValue = 5.1f;
            serialized.FindProperty("_runFrequencyMax").floatValue = 9.2f;
            serialized.FindProperty("_runSpeedRollDegrees").floatValue = 3.8f;
            serialized.FindProperty("_runYawDegrees").floatValue = 2.6f;
            serialized.FindProperty("_runPitchDegrees").floatValue = 4.2f;
            serialized.FindProperty("_runLateralRollDegrees").floatValue = 2.8f;
            serialized.FindProperty("_jumpOffset").floatValue = 0.032f;
            serialized.FindProperty("_airborneOffset").vector3Value = new Vector3(0f, 0.010f, 0.010f);
            serialized.FindProperty("_fallOffset").vector3Value = new Vector3(0f, -0.004f, 0.014f);
            serialized.FindProperty("_landImpulse").floatValue = 0.04f;
            serialized.FindProperty("_boostOffset").floatValue = 0.025f;
            serialized.FindProperty("_maximumPresentationDisplacement").floatValue = 0.085f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void DeleteGeneratedArmRuntimeAssets()
        {
            foreach (var path in new[]
            {
                $"{ArtRoot}/FirstPerson/Arms/PF_RR_FP_Arm_Left.prefab",
                $"{ArtRoot}/FirstPerson/Arms/PF_RR_FP_Arm_Right.prefab",
                $"{GeneratedRoot}/FirstPerson/RR_FP_Arm_Right_00.asset",
                $"{GeneratedRoot}/FirstPerson/RR_FP_Arm_Left_00.asset",
                ArmProfilePath,
                $"{MaterialRoot}/MAT_{ArmCanonicalName}.mat",
                $"{TextureRoot}/{ArmCanonicalName}_Base.png",
                $"{TextureRoot}/{ArmCanonicalName}_Normal.png",
                $"{TextureRoot}/{ArmCanonicalName}_Metallic.png",
                $"{TextureRoot}/{ArmCanonicalName}_Roughness.png",
                $"{GeneratedRoot}/EmissionMasks/{ArmCanonicalName}_CyanEmission.png"
            })
            {
                if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) != null)
                {
                    AssetDatabase.DeleteAsset(path);
                }
            }
        }

        private static void ConfigureModulePrefabLibrary(IReadOnlyDictionary<string, GameObject> prefabs)
        {
            var library = AssetDatabase.LoadAssetAtPath<ModuleVisualPrefabLibrary>(PrefabLibraryPath);
            if (library == null)
            {
                library = ModuleVisualPrefabLibrary.CreateRuntimeDefault();
                AssetDatabase.CreateAsset(library, PrefabLibraryPath);
            }

            var entries = new[]
            {
                Role(ModuleMaterialRole.Normal, prefabs["RR_Block_Standard"], 0f, Vector3.one * 1.5f, VisualFitMode.TopAlignedUniform, 1f, 1.14f),
                Role(ModuleMaterialRole.Normal, prefabs["RR_Block_Safe"], 2.75f, Vector3.one * 3f, VisualFitMode.TopAlignedUniform, 1f, 1.14f),
                Role(ModuleMaterialRole.Normal, prefabs["RR_Platform_Long"], 0f, new Vector3(3.0f, 1.285f, 4.57f), VisualFitMode.ExactFootprint, 1.14f),
                Role(ModuleMaterialRole.Normal, prefabs["RR_Block_Standard"], 6f, Vector3.one * 1.5f, VisualFitMode.ModularTile, 1f, 1.14f),
                Role(ModuleMaterialRole.Precision, prefabs["RR_Block_Standard"], 0f, Vector3.one * 1.3f, VisualFitMode.TopAlignedUniform),
                Role(ModuleMaterialRole.Moving, prefabs["RR_Platform_Long"], 0f, new Vector3(3.0f, 1.285f, 4.57f), VisualFitMode.ExactFootprint),
                Role(ModuleMaterialRole.Crumbling, prefabs["RR_Block_Standard"], 0f, Vector3.one * 1.4f, VisualFitMode.TopAlignedUniform),
                Role(ModuleMaterialRole.Restore, prefabs["RR_RestorePoint"], 0f, Vector3.one * 1.6f, VisualFitMode.Uniform),
                Role(ModuleMaterialRole.Boost, prefabs["RR_BoostPad"], 0f, Vector3.one * 1.8f, VisualFitMode.Uniform),
                Role(ModuleMaterialRole.Patch, prefabs["RR_PatchBlock"], 0f, Vector3.one * 2.2f, VisualFitMode.Uniform),
                Role(ModuleMaterialRole.TowerCore, prefabs["RR_Pillar_Tall"], 0f, Vector3.one * 8.0f, VisualFitMode.Uniform),
                Role(ModuleMaterialRole.TowerCore, prefabs["RR_RuinedTower"], 3.5f, Vector3.one * 6.5f, VisualFitMode.Uniform),
                Role(ModuleMaterialRole.DecorationStone, prefabs["RR_Rock_Broken"], 0f, Vector3.one * 4.5f, VisualFitMode.Uniform),
                Role(ModuleMaterialRole.Vegetation, prefabs["RR_Tree_Floating"], 0f, Vector3.one * 4.0f, VisualFitMode.Uniform),
                Role(ModuleMaterialRole.Flower, prefabs["RR_GrassTopper"], 0f, Vector3.one * 2.5f, VisualFitMode.Uniform),
                Role(ModuleMaterialRole.ShortcutCue, prefabs["RR_FloatingArch"], 0f, Vector3.one * 5.0f, VisualFitMode.Uniform),
                Role(ModuleMaterialRole.CircuitLine, prefabs["RR_EnergyPillar"], 0f, Vector3.one * 4.5f, VisualFitMode.Uniform)
            };

            var serialized = new SerializedObject(library);
            var array = serialized.FindProperty("_rolePrefabs");
            array.arraySize = entries.Length;
            for (var index = 0; index < entries.Length; index++)
            {
                var element = array.GetArrayElementAtIndex(index);
                element.FindPropertyRelative("_role").enumValueIndex = (int)entries[index].Role;
                element.FindPropertyRelative("_prefab").objectReferenceValue = entries[index].Prefab;
                element.FindPropertyRelative("_localPosition").vector3Value = entries[index].Position;
                element.FindPropertyRelative("_localEulerAngles").vector3Value = entries[index].Euler;
                element.FindPropertyRelative("_localScale").vector3Value = entries[index].Scale;
                element.FindPropertyRelative("_minimumBlockExtent").floatValue = entries[index].MinimumExtent;
                element.FindPropertyRelative("_minimumAspectRatio").floatValue = entries[index].MinimumAspectRatio;
                element.FindPropertyRelative("_maximumAspectRatio").floatValue = entries[index].MaximumAspectRatio;
                element.FindPropertyRelative("_fitMode").enumValueIndex = (int)entries[index].FitMode;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(library);
        }

        private static RolePlacement Role(
            ModuleMaterialRole role,
            GameObject prefab,
            float minimumExtent,
            Vector3? scale = null,
            VisualFitMode fitMode = VisualFitMode.Uniform,
            float minimumAspectRatio = 1f,
            float maximumAspectRatio = 0f)
        {
            return new RolePlacement(
                role,
                prefab,
                Vector3.zero,
                Vector3.zero,
                scale ?? Vector3.one,
                minimumExtent,
                fitMode,
                minimumAspectRatio,
                maximumAspectRatio);
        }

        private static Dictionary<string, GameObject> LoadCurrentEnvironmentPrefabs()
        {
            var prefabs = new Dictionary<string, GameObject>(StringComparer.Ordinal);
            foreach (var entry in Entries)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{ArtRoot}/{entry.CategoryPath}/PF_{entry.CanonicalName}.prefab");
                if (prefab != null)
                {
                    prefabs[entry.CanonicalName] = prefab;
                }
            }

            return prefabs;
        }

        private static List<AssetReport> InspectCurrentEnvironmentAssets()
        {
            var reports = new List<AssetReport>();
            foreach (var entry in Entries)
            {
                var sourceFbx = FindSourceFbx(entry.SourceKey);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{ArtRoot}/{entry.CategoryPath}/PF_{entry.CanonicalName}.prefab");
                var material = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialRoot}/MAT_{entry.CanonicalName}.mat");
                if (string.IsNullOrEmpty(sourceFbx) || prefab == null || material == null)
                {
                    continue;
                }

                reports.Add(InspectAsset(
                    entry,
                    sourceFbx,
                    prefab,
                    material,
                    new TextureSet
                    {
                        BaseMap = $"{TextureRoot}/{entry.CanonicalName}_Base.png",
                        NormalMap = $"{TextureRoot}/{entry.CanonicalName}_Normal.png",
                        MetallicMap = $"{TextureRoot}/{entry.CanonicalName}_Metallic.png",
                        RoughnessMap = $"{TextureRoot}/{entry.CanonicalName}_Roughness.png",
                        EmissionMap = $"{GeneratedRoot}/EmissionMasks/{entry.CanonicalName}_CyanEmission.png"
                    }));
            }

            return reports;
        }

        private static void CreateGalleryScene(IReadOnlyDictionary<string, GameObject> prefabs, ArmIntegrationResult armIntegration)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "ArtKitGallery";
            var camera = new GameObject("Gallery Camera", typeof(Camera));
            camera.transform.SetPositionAndRotation(new Vector3(0f, 5.5f, -15f), Quaternion.Euler(18f, 0f, 0f));
            camera.GetComponent<Camera>().fieldOfView = 60f;
            var sun = new GameObject("Gallery Sun", typeof(Light));
            sun.transform.rotation = Quaternion.Euler(48f, -28f, 0f);
            sun.GetComponent<Light>().type = LightType.Directional;
            sun.GetComponent<Light>().intensity = 1.15f;
            RenderSettings.ambientLight = new Color(0.58f, 0.68f, 0.82f);

            var index = 0;
            foreach (var entry in Entries)
            {
                if (prefabs.TryGetValue(entry.CanonicalName, out var prefab))
                {
                    PlaceGalleryPrefab(prefab, index, entry.CanonicalName);
                }

                index++;
            }

            if (armIntegration.HasRuntimePrefabs)
            {
                PlaceGalleryPrefab(AssetDatabase.LoadAssetAtPath<GameObject>(armIntegration.RightPrefabPath), index++, ArmCanonicalName);
                PlaceGalleryPrefab(AssetDatabase.LoadAssetAtPath<GameObject>(armIntegration.LeftPrefabPath), index, LeftArmCanonicalName);
            }

            EditorSceneManager.SaveScene(scene, GalleryScenePath);
        }

        private static void PlaceGalleryPrefab(GameObject prefab, int index, string label)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            var column = index % 5;
            var row = index / 5;
            instance.name = label;
            instance.transform.position = new Vector3((column - 2) * 4.2f, 0f, row * 4.6f);
            instance.transform.localScale = Vector3.one * 2.2f;
        }

        private static void AssignMaterialAndClean(GameObject root, Material material)
        {
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(includeInactive: true))
            {
                var materials = renderer.sharedMaterials;
                for (var index = 0; index < materials.Length; index++)
                {
                    materials[index] = material;
                }

                renderer.sharedMaterials = materials;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                renderer.lightProbeUsage = LightProbeUsage.Off;
                renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
                renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
                renderer.allowOcclusionWhenDynamic = false;
            }

            foreach (var collider in root.GetComponentsInChildren<Collider>(includeInactive: true))
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }
        }

        private static void NormalizeToUnitBounds(GameObject root, bool uniform)
        {
            if (!TryCalculateBounds(root, out var bounds))
            {
                return;
            }

            var size = bounds.size;
            var scale = uniform
                ? Vector3.one / Mathf.Max(size.x, size.y, size.z, 0.001f)
                : new Vector3(
                    1f / Mathf.Max(size.x, 0.001f),
                    1f / Mathf.Max(size.y, 0.001f),
                    1f / Mathf.Max(size.z, 0.001f));
            foreach (Transform child in root.transform)
            {
                child.localPosition = Vector3.Scale(child.localPosition - bounds.center, scale);
                child.localScale = Vector3.Scale(child.localScale, scale);
            }
        }

        private static void NormalizeToHorizontalUnitTop(GameObject root)
        {
            if (!TryCalculateBounds(root, out var bounds))
            {
                return;
            }

            var horizontalExtent = Mathf.Max(bounds.size.x, bounds.size.z, 0.001f);
            var scale = 1f / horizontalExtent;
            var topCenter = new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
            foreach (Transform child in root.transform)
            {
                child.localPosition = (child.localPosition - topCenter) * scale;
                child.localScale *= scale;
            }
        }

        private static void WriteCanonicalBlockAudit(
            string sourceFbx,
            GameObject standardPrefab,
            GameObject safePrefab,
            int triangleCount)
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(sourceFbx);
            var meshes = AssetDatabase.LoadAllAssetsAtPath(sourceFbx).OfType<Mesh>().ToArray();
            var instance = model == null ? null : UnityEngine.Object.Instantiate(model);
            var nativeBounds = default(Bounds);
            var hasBounds = instance != null && TryCalculateBounds(instance, out nativeBounds);
            var renderers = instance == null
                ? Array.Empty<Renderer>()
                : instance.GetComponentsInChildren<Renderer>(includeInactive: true);
            var materialCount = renderers
                .SelectMany(renderer => renderer.sharedMaterials)
                .Where(material => material != null)
                .Distinct()
                .Count();
            var meshColliderCount = instance == null
                ? 0
                : instance.GetComponentsInChildren<MeshCollider>(includeInactive: true).Length;
            var importer = AssetImporter.GetAtPath(sourceFbx) as ModelImporter;
            var standardBounds = BoundsForPrefab(standardPrefab);
            var safeBounds = BoundsForPrefab(safePrefab);
            var textureFiles = Directory.GetFiles(Path.GetDirectoryName(sourceFbx), "*.png")
                .Select(Path.GetFileName)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();
            var builder = new StringBuilder();
            builder.AppendLine("RYDER'S ROAD - PHASE 0.5.7 CANONICAL BLOCK AUDIT");
            builder.AppendLine($"Source: {sourceFbx}");
            builder.AppendLine($"FBX filename: {Path.GetFileName(sourceFbx)}");
            builder.AppendLine($"Mesh count: {meshes.Length.ToString(CultureInfo.InvariantCulture)}");
            builder.AppendLine($"Triangles: {triangleCount.ToString(CultureInfo.InvariantCulture)}");
            builder.AppendLine($"Vertices: {meshes.Sum(mesh => mesh.vertexCount).ToString(CultureInfo.InvariantCulture)}");
            builder.AppendLine($"Submeshes: {meshes.Sum(mesh => mesh.subMeshCount).ToString(CultureInfo.InvariantCulture)}");
            builder.AppendLine($"Material count: {materialCount.ToString(CultureInfo.InvariantCulture)}");
            builder.AppendLine($"Texture maps: {string.Join(", ", textureFiles)}");
            builder.AppendLine($"Native bounds center/size: {(hasBounds ? FormatVector(nativeBounds.center) + " / " + FormatVector(nativeBounds.size) : "unavailable")}");
            builder.AppendLine($"Pivot offset from native bounds center: {(hasBounds ? FormatVector(-nativeBounds.center) : "unavailable")}");
            builder.AppendLine($"Root orientation: {(model == null ? "unavailable" : FormatVector(model.transform.eulerAngles))}");
            builder.AppendLine($"Import scale: {(importer == null ? "unavailable" : importer.globalScale.ToString("0.###", CultureInfo.InvariantCulture))}");
            builder.AppendLine($"Unexpected MeshColliders: {meshColliderCount.ToString(CultureInfo.InvariantCulture)}");
            builder.AppendLine($"Standard normalized bounds center/size: {FormatBounds(standardBounds)}");
            builder.AppendLine($"Safe normalized bounds center/size: {FormatBounds(safeBounds)}");
            builder.AppendLine($"Runtime triangle policy: {(triangleCount <= MaximumCanonicalBlockTriangles ? "PASS" : "REJECT")}");
            File.WriteAllText(Phase057BlockAuditPath, builder.ToString());
            if (instance != null)
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        private static Bounds BoundsForPrefab(GameObject prefab)
        {
            if (prefab == null)
            {
                return default;
            }

            var instance = UnityEngine.Object.Instantiate(prefab);
            TryCalculateBounds(instance, out var bounds);
            UnityEngine.Object.DestroyImmediate(instance);
            return bounds;
        }

        private static string FormatBounds(Bounds bounds) =>
            $"{FormatVector(bounds.center)} / {FormatVector(bounds.size)}";

        private static string FormatVector(Vector3 value) =>
            $"({value.x.ToString("0.####", CultureInfo.InvariantCulture)}, "
            + $"{value.y.ToString("0.####", CultureInfo.InvariantCulture)}, "
            + $"{value.z.ToString("0.####", CultureInfo.InvariantCulture)})";

        private static bool TryCalculateBounds(GameObject root, out Bounds bounds)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(includeInactive: true);
            if (renderers.Length == 0)
            {
                bounds = default;
                return false;
            }

            bounds = renderers[0].bounds;
            for (var index = 1; index < renderers.Length; index++)
            {
                bounds.Encapsulate(renderers[index].bounds);
            }

            return true;
        }

        private static AssetReport InspectAsset(
            ArtEntry entry,
            string sourceFbx,
            GameObject prefab,
            Material material,
            TextureSet textures)
        {
            var meshes = AssetDatabase.LoadAllAssetsAtPath(sourceFbx).OfType<Mesh>().ToArray();
            var triangleCount = meshes.Sum(mesh => mesh.triangles.Length / 3);
            var vertexCount = meshes.Sum(mesh => mesh.vertexCount);
            var submeshCount = meshes.Sum(mesh => mesh.subMeshCount);
            var textureSize = TextureSize(textures.BaseMap);
            return new AssetReport(
                entry,
                sourceFbx,
                AssetDatabase.GetAssetPath(prefab),
                AssetDatabase.GetAssetPath(material),
                triangleCount,
                vertexCount,
                submeshCount,
                textureSize,
                textures);
        }

        private const string SkyboxDirectory = "Assets/_Game/Art/Sky";
        private const string SkyboxSourcePath = SkyboxDirectory + "/Skybox_Ryders_Road.png";
        private const string SkyboxMaterialPath = "Assets/_Game/Art/Materials/MAT_RR_Skybox_Panoramic.mat";
        private const string RuntimeSkyboxResourcePath = "Assets/_Game/Visuals/Resources/Textures/RydersRoad_SkyPanorama_01.png";

        private static void IntegrateSkyboxAsset()
        {
            var downloadsCandidate = @"C:\Users\lin4s\Downloads\Ryders Block Assets\Skybox Ryders Road.png";
            Directory.CreateDirectory(SkyboxDirectory);
            Directory.CreateDirectory("Assets/_Game/Visuals/Resources/Textures");
            if (File.Exists(downloadsCandidate) && !File.Exists(SkyboxSourcePath))
            {
                File.Copy(downloadsCandidate, SkyboxSourcePath, overwrite: true);
                AssetDatabase.ImportAsset(SkyboxSourcePath, ImportAssetOptions.ForceSynchronousImport);
            }

            if (!File.Exists(SkyboxSourcePath))
            {
                return;
            }

            File.Copy(SkyboxSourcePath, RuntimeSkyboxResourcePath, overwrite: true);
            AssetDatabase.ImportAsset(SkyboxSourcePath, ImportAssetOptions.ForceSynchronousImport);
            AssetDatabase.ImportAsset(RuntimeSkyboxResourcePath, ImportAssetOptions.ForceSynchronousImport);

            ConfigureSkyboxTextureImporter(SkyboxSourcePath);
            ConfigureSkyboxTextureImporter(RuntimeSkyboxResourcePath);

            var shader = Shader.Find("Skybox/Panoramic") ?? Shader.Find("Universal Render Pipeline/Lit");
            var material = AssetDatabase.LoadAssetAtPath<Material>(SkyboxMaterialPath);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, SkyboxMaterialPath);
            }
            else
            {
                material.shader = shader;
            }

            material.name = "MAT_RR_Skybox_Panoramic";
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(SkyboxSourcePath);
            if (texture != null && material.HasProperty("_Tex"))
            {
                material.SetTexture("_Tex", texture);
            }

            if (material.HasProperty("_Exposure"))
            {
                material.SetFloat("_Exposure", 1.15f);
            }

            if (material.HasProperty("_Rotation"))
            {
                material.SetFloat("_Rotation", 0f);
            }

            EditorUtility.SetDirty(material);
        }

        private static void ConfigureSkyboxTextureImporter(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            importer.textureType = TextureImporterType.Default;
            importer.textureShape = TextureImporterShape.Texture2D;
            importer.sRGBTexture = true;
            importer.mipmapEnabled = true;
            importer.isReadable = false;
            importer.wrapModeU = TextureWrapMode.Repeat;
            importer.wrapModeV = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.maxTextureSize = 2048;

            var android = new TextureImporterPlatformSettings
            {
                name = "Android",
                overridden = true,
                maxTextureSize = 2048,
                format = TextureImporterFormat.ASTC_6x6,
                compressionQuality = 80
            };
            importer.SetPlatformTextureSettings(android);
            importer.SaveAndReimport();
        }

        private static ArmReferenceReport InspectArmCandidate()
        {
            var sourceFbx = FindSourceFbx(ArmSourceKey);
            if (string.IsNullOrEmpty(sourceFbx))
            {
                return new ArmReferenceReport(
                    string.Empty,
                    0,
                    0,
                    0,
                    "missing",
                    "WAITING FOR OPTIMIZED REMESH");
            }

            ConfigureArmReferenceImporter(sourceFbx);
            var meshes = AssetDatabase.LoadAllAssetsAtPath(sourceFbx).OfType<Mesh>().ToArray();
            var sourceDirectory = Path.GetDirectoryName(sourceFbx);
            var baseTexture = Directory.GetFiles(sourceDirectory, "*.png")
                .FirstOrDefault(path => TextureSuffix(path) == "Base");
            var textureSize = "missing";
            if (!string.IsNullOrEmpty(baseTexture))
            {
                var relativeTexturePath = baseTexture.Replace('\\', '/');
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(relativeTexturePath);
                textureSize = texture == null ? "missing" : $"{texture.width}x{texture.height}";
            }

            var triangles = meshes.Sum(mesh => mesh.triangles.Length / 3);
            var status = triangles > MaximumRuntimeArmTriangles
                ? "WAITING FOR OPTIMIZED REMESH"
                : "OPTIMIZED SOURCE AVAILABLE; production integration pending";
            return new ArmReferenceReport(
                sourceFbx,
                triangles,
                meshes.Sum(mesh => mesh.vertexCount),
                meshes.Sum(mesh => mesh.subMeshCount),
                textureSize,
                status);
        }

        private static void WriteArmCandidateReport(ArmReferenceReport report)
        {
            Directory.CreateDirectory("Logs");
            File.WriteAllText(
                "Logs/meshy-arm-candidate-report.txt",
                "RR_FP_Arm_Right candidate report" + Environment.NewLine
                + "Generated: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss zzz", CultureInfo.InvariantCulture) + Environment.NewLine
                + "Source FBX: " + (string.IsNullOrEmpty(report.SourceFbx) ? "missing" : report.SourceFbx) + Environment.NewLine
                + "Triangles: " + report.TriangleCount.ToString(CultureInfo.InvariantCulture) + Environment.NewLine
                + "Vertices: " + report.VertexCount.ToString(CultureInfo.InvariantCulture) + Environment.NewLine
                + "Submeshes: " + report.SubmeshCount.ToString(CultureInfo.InvariantCulture) + Environment.NewLine
                + "Base texture: " + report.TextureSize + Environment.NewLine
                + "Runtime cap: " + MaximumRuntimeArmTriangles.ToString(CultureInfo.InvariantCulture) + Environment.NewLine
                + "Status: " + report.Status + Environment.NewLine
                + "Legacy high-poly source key ignored for runtime: " + LegacyHighPolyArmSourceKey + Environment.NewLine);
        }

        private static void ConfigureArmReferenceImporter(string fbxPath)
        {
            var importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
            if (importer == null)
            {
                return;
            }

            importer.importAnimation = false;
            importer.animationType = ModelImporterAnimationType.None;
            importer.importCameras = false;
            importer.importLights = false;
            importer.importBlendShapes = false;
            importer.importVisibility = false;
            importer.importConstraints = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.isReadable = false;
            importer.meshCompression = ModelImporterMeshCompression.Off;
            importer.optimizeMeshPolygons = true;
            importer.optimizeMeshVertices = true;
            importer.SaveAndReimport();
        }

        private static void ConfigureArmProductionImporter(string fbxPath)
        {
            var importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
            if (importer == null)
            {
                return;
            }

            importer.importAnimation = false;
            importer.animationType = ModelImporterAnimationType.None;
            importer.importCameras = false;
            importer.importLights = false;
            importer.importBlendShapes = false;
            importer.importVisibility = false;
            importer.importConstraints = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.isReadable = true;
            importer.meshCompression = ModelImporterMeshCompression.Off;
            importer.optimizeMeshPolygons = true;
            importer.optimizeMeshVertices = true;
            importer.SaveAndReimport();
        }

        private static string TextureSize(string path)
        {
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            return texture == null ? "missing" : $"{texture.width}x{texture.height}";
        }

        private static void WriteManifest(
            IEnumerable<AssetReport> reports,
            ArmReferenceReport armReference,
            ArmIntegrationResult armIntegration)
        {
            var builder = new StringBuilder();
            builder.AppendLine("# Ryder's Road Art Asset Manifest");
            builder.AppendLine();
            builder.AppendLine("Generated by `MeshyArtKitIntegrator` for `0.5.3-hand-presentation-and-world-cleanup`.");
            builder.AppendLine("Downloads originals remain untouched; copied source zips/extracts live under `Assets/_Game/Art/MeshySource`.");
            builder.AppendLine("Final Surf asset pending; current Surf presentation intentionally remains unchanged.");
            builder.AppendLine();
            builder.AppendLine("| Source Meshy name | Canonical name | Role | Triangles | Vertices | Submeshes | Base texture | Android max | Material | Prefab | Status |");
            builder.AppendLine("| --- | --- | --- | ---: | ---: | ---: | --- | ---: | --- | --- | --- |");
            foreach (var report in reports)
            {
                builder.Append("| ");
                builder.Append(Path.GetFileName(report.SourceFbx));
                builder.Append(" | ");
                builder.Append(report.Entry.CanonicalName);
                builder.Append(" | ");
                builder.Append(report.Entry.Role);
                builder.Append(" | ");
                builder.Append(report.TriangleCount.ToString(CultureInfo.InvariantCulture));
                builder.Append(" | ");
                builder.Append(report.VertexCount.ToString(CultureInfo.InvariantCulture));
                builder.Append(" | ");
                builder.Append(report.SubmeshCount.ToString(CultureInfo.InvariantCulture));
                builder.Append(" | ");
                builder.Append(report.TextureSize);
                builder.Append(" | ");
                builder.Append(report.Entry.RuntimeMaxTextureSize.ToString(CultureInfo.InvariantCulture));
                builder.Append(" | ");
                builder.Append(report.MaterialPath);
                builder.Append(" | ");
                builder.Append(report.PrefabPath);
                builder.Append(" | Integrated |");
                builder.AppendLine();
            }

            builder.AppendLine();
            builder.AppendLine("## First-Person Arm Status");
            builder.AppendLine();
            builder.AppendLine("| Source Meshy name | Canonical name | Role | Triangles | Vertices | Submeshes | Base texture | Android max | Material | Prefab | Status |");
            builder.AppendLine("| --- | --- | --- | ---: | ---: | ---: | --- | ---: | --- | --- | --- |");
            builder.Append("| ");
            builder.Append(string.IsNullOrEmpty(armReference.SourceFbx)
                ? "missing"
                : Path.GetFileName(armReference.SourceFbx));
            builder.Append(" | ");
            builder.Append(ArmCanonicalName);
            builder.Append(" | First-person right arm/gauntlet reference | ");
            builder.Append(armReference.TriangleCount.ToString(CultureInfo.InvariantCulture));
            builder.Append(" | ");
            builder.Append(armReference.VertexCount.ToString(CultureInfo.InvariantCulture));
            builder.Append(" | ");
            builder.Append(armReference.SubmeshCount.ToString(CultureInfo.InvariantCulture));
            builder.Append(" | ");
            builder.Append(armReference.TextureSize);
            builder.Append(" | 2048 | ");
            builder.Append(armIntegration.HasRuntimePrefabs ? armIntegration.MaterialPath : "none");
            builder.Append(" | ");
            builder.Append(armIntegration.HasRuntimePrefabs ? armIntegration.RightPrefabPath : "none");
            builder.Append(" | ");
            builder.Append(armReference.Status);
            builder.AppendLine(" |");
            builder.AppendLine();
            if (armIntegration.HasRuntimePrefabs)
            {
                builder.AppendLine("- Runtime right arm prefab: `" + armIntegration.RightPrefabPath + "`.");
                builder.AppendLine("- Runtime left arm prefab: `" + armIntegration.LeftPrefabPath + "`.");
                builder.AppendLine("- Runtime right mesh asset: `" + armIntegration.RightMeshAssetPath + "`.");
                builder.AppendLine("- Runtime left mesh asset: `" + armIntegration.LeftMeshAssetPath + "`.");
                builder.AppendLine("- Left arm is generated by mirroring mesh vertices/normals/tangents on X and reversing triangle winding; runtime transforms keep positive scale.");
                builder.AppendLine("- Arms are first-person presentation only and do not affect movement, collision, Restore, Patch, scoring, or saves.");
            }
            else
            {
                builder.AppendLine("- High-poly arm source remains under `Assets/_Game/Art/MeshySource` as reference/source only.");
                builder.AppendLine("- No runtime arm prefab, no mirrored left-arm mesh, and no `FirstPersonArmProfile` asset are generated from this source.");
                builder.AppendLine("- Current lightweight first-person hands remain active until an optimized approximately 15k-triangle Meshy remesh is provided.");
            }

            builder.AppendLine("- Legacy high-poly arm key `" + LegacyHighPolyArmSourceKey + "` is ignored for runtime generation.");
            File.WriteAllText(ManifestPath, builder.ToString());
        }

        private readonly struct ArtEntry
        {
            public ArtEntry(
                string sourceKey,
                string canonicalName,
                string role,
                string categoryPath,
                int runtimeMaxTextureSize,
                bool cyanEmission)
            {
                SourceKey = sourceKey;
                CanonicalName = canonicalName;
                Role = role;
                CategoryPath = categoryPath;
                RuntimeMaxTextureSize = runtimeMaxTextureSize;
                CyanEmission = cyanEmission;
            }

            public string SourceKey { get; }
            public string CanonicalName { get; }
            public string Role { get; }
            public string CategoryPath { get; }
            public int RuntimeMaxTextureSize { get; }
            public bool CyanEmission { get; }
        }

        private sealed class TextureSet
        {
            public string BaseMap;
            public string NormalMap;
            public string MetallicMap;
            public string RoughnessMap;
            public string EmissionMap;
        }

        private readonly struct RolePlacement
        {
            public RolePlacement(
                ModuleMaterialRole role,
                GameObject prefab,
                Vector3 position,
                Vector3 euler,
                Vector3 scale,
                float minimumExtent,
                VisualFitMode fitMode = VisualFitMode.Uniform,
                float minimumAspectRatio = 1f,
                float maximumAspectRatio = 0f)
            {
                Role = role;
                Prefab = prefab;
                Position = position;
                Euler = euler;
                Scale = scale;
                MinimumExtent = minimumExtent;
                FitMode = fitMode;
                MinimumAspectRatio = minimumAspectRatio;
                MaximumAspectRatio = maximumAspectRatio;
            }

            public ModuleMaterialRole Role { get; }
            public GameObject Prefab { get; }
            public Vector3 Position { get; }
            public Vector3 Euler { get; }
            public Vector3 Scale { get; }
            public float MinimumExtent { get; }
            public VisualFitMode FitMode { get; }
            public float MinimumAspectRatio { get; }
            public float MaximumAspectRatio { get; }
        }

        private readonly struct AssetReport
        {
            public AssetReport(
                ArtEntry entry,
                string sourceFbx,
                string prefabPath,
                string materialPath,
                int triangleCount,
                int vertexCount,
                int submeshCount,
                string textureSize,
                TextureSet textures)
            {
                Entry = entry;
                SourceFbx = sourceFbx;
                PrefabPath = prefabPath;
                MaterialPath = materialPath;
                TriangleCount = triangleCount;
                VertexCount = vertexCount;
                SubmeshCount = submeshCount;
                TextureSize = textureSize;
                Textures = textures;
            }

            public ArtEntry Entry { get; }
            public string SourceFbx { get; }
            public string PrefabPath { get; }
            public string MaterialPath { get; }
            public int TriangleCount { get; }
            public int VertexCount { get; }
            public int SubmeshCount { get; }
            public string TextureSize { get; }
            public TextureSet Textures { get; }
        }

        private readonly struct ArmReferenceReport
        {
            public ArmReferenceReport(
                string sourceFbx,
                int triangleCount,
                int vertexCount,
                int submeshCount,
                string textureSize,
                string status)
            {
                SourceFbx = sourceFbx;
                TriangleCount = triangleCount;
                VertexCount = vertexCount;
                SubmeshCount = submeshCount;
                TextureSize = textureSize;
                Status = status;
            }

            public string SourceFbx { get; }
            public int TriangleCount { get; }
            public int VertexCount { get; }
            public int SubmeshCount { get; }
            public string TextureSize { get; }
            public string Status { get; }
        }

        private readonly struct ArmIntegrationResult
        {
            public ArmIntegrationResult(ArmReferenceReport report)
                : this(report, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty)
            {
            }

            public ArmIntegrationResult(
                ArmReferenceReport report,
                string rightPrefabPath,
                string leftPrefabPath,
                string materialPath,
                string rightMeshAssetPath,
                string leftMeshAssetPath)
            {
                Report = report;
                RightPrefabPath = rightPrefabPath;
                LeftPrefabPath = leftPrefabPath;
                MaterialPath = materialPath;
                RightMeshAssetPath = rightMeshAssetPath;
                LeftMeshAssetPath = leftMeshAssetPath;
            }

            public ArmReferenceReport Report { get; }
            public string RightPrefabPath { get; }
            public string LeftPrefabPath { get; }
            public string MaterialPath { get; }
            public string RightMeshAssetPath { get; }
            public string LeftMeshAssetPath { get; }
            public bool HasRuntimePrefabs =>
                !string.IsNullOrEmpty(RightPrefabPath)
                && !string.IsNullOrEmpty(LeftPrefabPath);
        }
    }
}
