using System;
using System.Collections.Generic;
using System.IO;
using Avoidance.Gameplay.Levels;
using Avoidance.Gameplay.Ranking;
using Avoidance.Gameplay.Visuals;
using Avoidance.Gameplay.Worlds;
using Avoidance.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Avoidance.EditorTools
{
    public static class Phase074WorldSystemRecovery
    {
        public const string TextureRoot = "Assets/_Game/Art/Textures";
        public const string MaterialRoot = "Assets/_Game/Art/Materials/World";
        public const string ResourceMaterialRoot = "Assets/_Game/Art/Resources/Materials";
        public const string AncientAbyssSkyPath = ResourceMaterialRoot + "/MAT_RR_AncientAbyssSky.mat";
        public const string AncientAbyssSkyTexturePath = "Assets/_Game/Art/Sky/Skybox_Ryders_Road_AncientAbyss_081.png";
        public const string LandmarkPrefabRoot = "Assets/_Game/Art/Environment/Landmarks";
        public const string WorldAssetPrefabRoot = "Assets/_Game/Art/Environment/WorldAssets";
        public const string BiomeRoot = "Assets/_Game/Worlds/Resources/EnvironmentBiomes";
        public const string ModuleRoot = "Assets/_Game/Levels/Resources/Modules";

        [MenuItem("RYDERS BLOCK/Phase 0.7.4/Recover World Systems And Module 003")]
        public static void RecoverWorldSystemsAndModule003()
        {
            EnsureDirectories();
            EnsureAncientAbyssSkybox();
            var materials = EnsureWorldMaterials();
            var landmarks = EnsureLandmarkPrefabs(materials);
            var ancientAbyssBiome = ConfigureAncientAbyssBiome(landmarks);
            ConfigureModule003Content(ancientAbyssBiome, landmarks);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("RYDER'S ROAD 0.7.4 World System Recovery and Module 003 Gold-Standard Pass complete.");
        }

        private static void EnsureDirectories()
        {
            Directory.CreateDirectory(TextureRoot);
            Directory.CreateDirectory(MaterialRoot);
            Directory.CreateDirectory(ResourceMaterialRoot);
            Directory.CreateDirectory(LandmarkPrefabRoot);
            Directory.CreateDirectory(BiomeRoot);
            Directory.CreateDirectory(ModuleRoot);
        }

        public static void EnsureAncientAbyssSkybox()
        {
            var importer = AssetImporter.GetAtPath(AncientAbyssSkyTexturePath) as TextureImporter;
            if (importer == null)
            {
                throw new InvalidOperationException("Ancient Abyss sky texture importer is missing.");
            }

            var textureSettings = new TextureImporterSettings();
            importer.ReadTextureSettings(textureSettings);
            if (importer.textureShape != TextureImporterShape.TextureCube ||
                importer.generateCubemap != TextureImporterGenerateCubemap.AutoCubemap ||
                !textureSettings.seamlessCubemap)
            {
                textureSettings.seamlessCubemap = true;
                importer.SetTextureSettings(textureSettings);
                importer.textureShape = TextureImporterShape.TextureCube;
                importer.generateCubemap = TextureImporterGenerateCubemap.AutoCubemap;
                importer.mipmapEnabled = true;
                importer.SaveAndReimport();
            }

            var shader = Shader.Find("Skybox/Cubemap");
            var panorama = AssetDatabase.LoadAssetAtPath<Cubemap>(AncientAbyssSkyTexturePath);
            if (shader == null || panorama == null)
            {
                throw new InvalidOperationException("Ancient Abyss cubemap sky dependencies are missing.");
            }

            var material = AssetDatabase.LoadAssetAtPath<Material>(AncientAbyssSkyPath);
            if (material == null)
            {
                material = new Material(shader) { name = "MAT_RR_AncientAbyssSky" };
                AssetDatabase.CreateAsset(material, AncientAbyssSkyPath);
            }

            material.shader = shader;
            material.SetTexture("_Tex", panorama);
            material.SetFloat("_Exposure", 0.62f);
            material.SetFloat("_Rotation", 8f);
            material.SetColor("_Tint", new Color(0.88f, 0.94f, 1f, 1f));
            EditorUtility.SetDirty(material);
        }

        public static Dictionary<string, Material> EnsureWorldMaterials()
        {
            return new Dictionary<string, Material>(StringComparer.Ordinal)
            {
                ["ivory"] = LoadOrCreateMaterial(
                    "MAT_RR_World_Ancient_Ivory",
                    new Color(0.9f, 0.87f, 0.76f, 1f)),
                ["slate"] = LoadOrCreateMaterial(
                    "MAT_RR_World_Deep_Slate",
                    new Color(0.1f, 0.15f, 0.27f, 1f)),
                ["sandstone"] = LoadOrCreateMaterial(
                    "MAT_RR_World_Ancient_Sandstone",
                    new Color(0.74f, 0.68f, 0.56f, 1f)),
                ["cyan"] = LoadOrCreateEmissiveMaterial(
                    "MAT_RR_World_City_Cyan",
                    new Color(0.08f, 0.72f, 0.95f, 1f),
                    1.6f),
                ["rock"] = LoadOrCreateMaterial(
                    "MAT_RR_World_Mountain_Rock",
                    new Color(0.27f, 0.34f, 0.48f, 1f)),
                ["green"] = LoadOrCreateMaterial(
                    "MAT_RR_World_Subtle_Vegetation",
                    new Color(0.12f, 0.38f, 0.2f, 1f)),
                ["gold"] = LoadOrCreateEmissiveMaterial(
                    "MAT_RR_World_Ancient_Gold",
                    new Color(0.95f, 0.72f, 0.18f, 1f),
                    1.2f)
            };
        }

        private static Material LoadOrCreateMaterial(string name, Color color)
        {
            var path = $"{MaterialRoot}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(VisualMaterialUtility.ResolveOpaqueShader()) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = VisualMaterialUtility.ResolveOpaqueShader();
            VisualMaterialUtility.ApplyColor(material, color);
            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", 0.22f);
            }
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material LoadOrCreateEmissiveMaterial(string name, Color color, float intensity)
        {
            var path = $"{MaterialRoot}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(VisualMaterialUtility.ResolveOpaqueShader()) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = VisualMaterialUtility.ResolveOpaqueShader();
            VisualMaterialUtility.ApplyColor(material, color);
            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * intensity);
            }
            EditorUtility.SetDirty(material);
            return material;
        }

        public static Dictionary<string, GameObject> EnsureLandmarkPrefabs(Dictionary<string, Material> materials)
        {
            var prefabs = new Dictionary<string, GameObject>(StringComparer.Ordinal);

            prefabs["AncientTempleComplex"] = BuildAncientTempleComplex(materials);
            prefabs["M03ArrivalSanctuary"] = BuildM03ArrivalSanctuary(materials);
            prefabs["M03BrokenCrossing"] = BuildM03BrokenCrossing(materials);
            prefabs["M03CollapsedTemple"] = BuildM03CollapsedTemple(materials);
            prefabs["M03EnergySpine"] = BuildM03EnergySpine(materials);
            prefabs["M03PatchSanctum"] = BuildM03PatchSanctum(materials);
            prefabs["BrokenProcession"] = BuildBrokenProcession(materials);
            prefabs["SunkenRuinCity"] = BuildSunkenRuinCity(materials);
            prefabs["ReclaimedSanctuary"] = BuildReclaimedSanctuary(materials);
            prefabs["ColossalAbyssTower"] = BuildColossalAbyssTower(materials);
            prefabs["CelestialBrokenArch"] = BuildCelestialBrokenArch(materials);
            prefabs["FloatingRuinIsland"] = BuildFloatingRuinIsland(materials);
            prefabs["DistantSunkenMonolith"] = BuildDistantSunkenMonolith(materials);
            prefabs["AncientCentralSpire"] = BuildAncientCentralSpire(materials);
            prefabs["AqueductButtress"] = BuildAqueductButtress(materials);

            return prefabs;
        }

        private static GameObject BuildAncientTempleComplex(Dictionary<string, Material> materials)
        {
            var root = new GameObject("PF_RR_Landmark_AncientTempleComplex");

            var rock = LoadPrefab("Assets/_Game/Art/Environment/Decoration/PF_RR_Rock_Broken.prefab");
            var arch = LoadPrefab("Assets/_Game/Art/Environment/Architecture/PF_RR_FloatingArch.prefab");
            var ruinedTower = LoadPrefab("Assets/_Game/Art/Environment/Architecture/PF_RR_RuinedTower.prefab");
            var pillar = LoadPrefab("Assets/_Game/Art/Environment/Architecture/PF_RR_Pillar_Tall.prefab");
            var energy = LoadPrefab("Assets/_Game/Art/Environment/Decoration/PF_RR_EnergyPillar.prefab");

            AddPrefabPiece(root, "Shattered_Crown_Foundation_Left", rock, new Vector3(-13f, -23f, 1f), Quaternion.Euler(2f, 22f, 4f), new Vector3(34f, 28f, 30f));
            AddPrefabPiece(root, "Shattered_Crown_Foundation_Right", rock, new Vector3(16f, -29f, -4f), Quaternion.Euler(-3f, -38f, -5f), new Vector3(28f, 31f, 27f));
            AddPrefabPiece(root, "Shattered_Crown_Left_Temple", ruinedTower, new Vector3(-11f, 8f, 0f), Quaternion.Euler(0f, 18f, -4f), Vector3.one * 15f);
            AddPrefabPiece(root, "Shattered_Crown_Right_Temple", ruinedTower, new Vector3(13f, -1f, -3f), Quaternion.Euler(0f, -28f, 9f), Vector3.one * 11f);
            AddPrefabPiece(root, "Shattered_Crown_Great_Arch", arch, new Vector3(-1f, 23f, 2f), Quaternion.Euler(0f, 8f, -9f), Vector3.one * 28f);
            AddPrefabPiece(root, "Shattered_Crown_Severed_Arch", arch, new Vector3(24f, 12f, -8f), Quaternion.Euler(18f, -42f, 36f), Vector3.one * 15f);
            AddPrefabPiece(root, "Shattered_Crown_Severed_Pillar", pillar, new Vector3(22f, 7f, 6f), Quaternion.Euler(0f, -24f, 14f), Vector3.one * 10f);
            AddPrefabPiece(root, "Shattered_Crown_Energy_Heart", energy, new Vector3(-1f, 20f, 1f), Quaternion.identity, Vector3.one * 9f);

            return SaveLandmarkPrefab(root, "PF_RR_Landmark_AncientTempleComplex");
        }

        private static GameObject BuildM03ArrivalSanctuary(Dictionary<string, Material> materials)
        {
            var root = new GameObject("PF_RR_M03_ArrivalSanctuary");
            var bush = LoadModel("Assets/_Game/Art/ThirdParty/Kenney/NatureKit/Source/RR_Nature_Bush_Large.fbx");
            var rock = LoadPrefab("Assets/_Game/Art/Environment/Decoration/PF_RR_Rock_Broken.prefab");
            var ruin = LoadPrefab("Assets/_Game/Art/Environment/Architecture/PF_RR_RuinedTower.prefab");
            var arch = LoadPrefab("Assets/_Game/Art/Environment/Architecture/PF_RR_FloatingArch.prefab");
            var pillar = LoadPrefab("Assets/_Game/Art/Environment/Architecture/PF_RR_Pillar_Tall.prefab");

            AddPrefabPiece(root, "M03_Arrival_Main_Foundation", rock, new Vector3(-2f, -17.5f, -5f), Quaternion.Euler(0f, 16f, 0f), new Vector3(14.88f, 13f, 17.68f));
            AddPrefabPiece(root, "M03_Arrival_Upper_Foundation", rock, new Vector3(3f, -15.5f, 8f), Quaternion.Euler(0f, 118f, 7f), new Vector3(11.16f, 10f, 13.6f));
            AddPrefabPiece(root, "M03_Arrival_Sanctuary_Ruin", ruin, new Vector3(-9f, -0.2f, 4f), Quaternion.Euler(0f, 32f, -3f), Vector3.one * 7.2f);
            AddPrefabPiece(root, "M03_Arrival_View_Arch", arch, new Vector3(10f, 0.4f, 9f), Quaternion.Euler(0f, -12f, 5f), Vector3.one * 7.4f);
            AddPrefabPiece(root, "M03_Arrival_Fallen_Pillar", pillar, new Vector3(-7f, -0.8f, -4f), Quaternion.Euler(72f, 24f, -5f), Vector3.one * 4.2f);
            AddPiece(root, "M03_Arrival_Vegetation_A", bush, new Vector3(-3f, -0.2f, 6f), new Vector3(0f, 20f, 0f), Vector3.one * 2.5f, materials["green"]);
            AddPiece(root, "M03_Arrival_Vegetation_B", bush, new Vector3(5f, -0.4f, 1f), new Vector3(0f, 65f, 0f), Vector3.one * 1.8f, materials["green"]);
            AddPrefabPiece(root, "M03_Arrival_Start_Buttress", pillar, new Vector3(0f, -4.35f, -13f), Quaternion.identity, new Vector3(7f, 8f, 7f));
            AddPrefabPiece(root, "M03_Arrival_Court_Buttress", pillar, new Vector3(4f, -3.35f, 9.5f), Quaternion.identity, new Vector3(6f, 8f, 6f));
            return SaveLandmarkPrefab(root, "PF_RR_M03_ArrivalSanctuary", true);
        }

        private static GameObject BuildM03BrokenCrossing(Dictionary<string, Material> materials)
        {
            var root = new GameObject("PF_RR_M03_BrokenCrossing");
            var rock = LoadPrefab("Assets/_Game/Art/Environment/Decoration/PF_RR_Rock_Broken.prefab");
            var pillar = LoadPrefab("Assets/_Game/Art/Environment/Architecture/PF_RR_Pillar_Tall.prefab");
            var arch = LoadPrefab("Assets/_Game/Art/Environment/Architecture/PF_RR_FloatingArch.prefab");

            AddPrefabPiece(root, "M03_Crossing_Near_Foundation", rock, new Vector3(7f, -17f, -6f), Quaternion.Euler(0f, -22f, 0f), new Vector3(13.02f, 14f, 15.64f));
            AddPrefabPiece(root, "M03_Crossing_Far_Foundation", rock, new Vector3(-6f, -16.7f, 6f), Quaternion.Euler(0f, 154f, 0f), new Vector3(13.02f, 14f, 15.64f));
            AddPrefabPiece(root, "M03_Crossing_Near_Mechanism", pillar, new Vector3(12f, -0.5f, -3f), Quaternion.Euler(0f, -24f, -7f), Vector3.one * 6.2f);
            AddPrefabPiece(root, "M03_Crossing_Far_Mechanism", pillar, new Vector3(-11f, -0.2f, 4f), Quaternion.Euler(0f, 18f, 8f), Vector3.one * 6.2f);
            AddPrefabPiece(root, "M03_Crossing_Near_Bridgehead", LoadPrefab("Assets/_Game/Art/Environment/Architecture/PF_RR_RuinedTower.prefab"), new Vector3(13f, 0.2f, -7f), Quaternion.Euler(0f, -34f, 14f), Vector3.one * 8.2f);
            AddPrefabPiece(root, "M03_Crossing_Far_Bridgehead", LoadPrefab("Assets/_Game/Art/Environment/Architecture/PF_RR_RuinedTower.prefab"), new Vector3(-12f, 0.4f, 7f), Quaternion.Euler(0f, 142f, -11f), Vector3.one * 8.6f);
            AddPrefabPiece(root, "M03_Crossing_Dock_Buttress", pillar, new Vector3(7f, -4.5f, -6f), Quaternion.identity, new Vector3(6f, 8f, 6f));
            AddPrefabPiece(root, "M03_Crossing_Exit_Buttress", pillar, new Vector3(-6f, -3.85f, 7f), Quaternion.identity, new Vector3(6f, 8f, 6f));
            return SaveLandmarkPrefab(root, "PF_RR_M03_BrokenCrossing", true);
        }

        private static GameObject BuildM03CollapsedTemple(Dictionary<string, Material> materials)
        {
            var root = new GameObject("PF_RR_M03_CollapsedTemple");
            var bush = LoadModel("Assets/_Game/Art/ThirdParty/Kenney/NatureKit/Source/RR_Nature_Bush_Large.fbx");
            var rock = LoadPrefab("Assets/_Game/Art/Environment/Decoration/PF_RR_Rock_Broken.prefab");
            var ruin = LoadPrefab("Assets/_Game/Art/Environment/Architecture/PF_RR_RuinedTower.prefab");
            var arch = LoadPrefab("Assets/_Game/Art/Environment/Architecture/PF_RR_FloatingArch.prefab");

            AddPrefabPiece(root, "M03_Temple_Main_Foundation", rock, new Vector3(1f, -17.5f, 0f), Quaternion.Euler(0f, 46f, 0f), new Vector3(14.26f, 15f, 18.36f));
            AddPrefabPiece(root, "M03_Temple_Fallen_Foundation", rock, new Vector3(9f, -16.8f, 5f), Quaternion.Euler(4f, -74f, -8f), new Vector3(9.3f, 12f, 12.24f));
            AddPrefabPiece(root, "M03_Temple_Fallen_Hall", ruin, new Vector3(-8f, 0f, 1f), Quaternion.Euler(8f, 28f, 13f), Vector3.one * 7.2f);
            AddPrefabPiece(root, "M03_Temple_Collapsed_Wall", ruin, new Vector3(7f, 1f, 5f), Quaternion.Euler(18f, -18f, 68f), new Vector3(9f, 11f, 9f));
            AddPiece(root, "M03_Temple_Vegetation_A", bush, new Vector3(-3f, 0f, -2f), new Vector3(0f, 18f, 0f), Vector3.one * 2.8f, materials["green"]);
            AddPiece(root, "M03_Temple_Vegetation_B", bush, new Vector3(6f, -0.2f, 4f), new Vector3(0f, 54f, 0f), Vector3.one * 2.1f, materials["green"]);
            return SaveLandmarkPrefab(root, "PF_RR_M03_CollapsedTemple", true);
        }

        private static GameObject BuildM03EnergySpine(Dictionary<string, Material> materials)
        {
            var root = new GameObject("PF_RR_M03_EnergySpine");
            var rock = LoadPrefab("Assets/_Game/Art/Environment/Decoration/PF_RR_Rock_Broken.prefab");
            var pillar = LoadPrefab("Assets/_Game/Art/Environment/Architecture/PF_RR_Pillar_Tall.prefab");
            var energy = LoadPrefab("Assets/_Game/Art/Environment/Decoration/PF_RR_EnergyPillar.prefab");

            AddPrefabPiece(root, "M03_Spine_Approach_Foundation", rock, new Vector3(4f, -18f, -14f), Quaternion.Euler(0f, 74f, -3f), new Vector3(10.54f, 13f, 12.92f));
            AddPrefabPiece(root, "M03_Spine_Launch_Foundation", rock, new Vector3(6f, -17.2f, -6f), Quaternion.Euler(0f, 18f, 0f), new Vector3(11.78f, 14f, 14.28f));
            AddPrefabPiece(root, "M03_Spine_Landing_Foundation", rock, new Vector3(-1f, -17.5f, 7f), Quaternion.Euler(0f, -34f, 0f), new Vector3(15.5f, 16f, 19.72f));
            AddPrefabPiece(root, "M03_Spine_Left_Pylon", pillar, new Vector3(-6f, -1f, 0f), Quaternion.Euler(0f, 18f, -8f), Vector3.one * 5f);
            AddPrefabPiece(root, "M03_Spine_Right_Pylon", pillar, new Vector3(10f, -1f, 1f), Quaternion.Euler(0f, -20f, 9f), Vector3.one * 5f);
            AddPrefabPiece(root, "M03_Spine_Energy_Source", energy, new Vector3(6f, 0.8f, -2f), Quaternion.identity, Vector3.one * 3.2f);
            AddPrefabPiece(root, "M03_Spine_Energy_Relay", energy, new Vector3(-7f, 4f, 8f), Quaternion.identity, Vector3.one * 3.8f);
            AddPrefabPiece(root, "M03_Spine_Receiving_Tower", pillar, new Vector3(-1f, -4.5f, 6f), Quaternion.identity, new Vector3(9f, 10f, 9f));
            return SaveLandmarkPrefab(root, "PF_RR_M03_EnergySpine", true);
        }

        private static GameObject BuildM03PatchSanctum(Dictionary<string, Material> materials)
        {
            var root = new GameObject("PF_RR_M03_PatchSanctum");
            var rock = LoadPrefab("Assets/_Game/Art/Environment/Decoration/PF_RR_Rock_Broken.prefab");
            var arch = LoadPrefab("Assets/_Game/Art/Environment/Architecture/PF_RR_FloatingArch.prefab");
            var ruin = LoadPrefab("Assets/_Game/Art/Environment/Architecture/PF_RR_RuinedTower.prefab");
            var pillar = LoadPrefab("Assets/_Game/Art/Environment/Architecture/PF_RR_Pillar_Tall.prefab");
            var energy = LoadPrefab("Assets/_Game/Art/Environment/Decoration/PF_RR_EnergyPillar.prefab");

            AddPrefabPiece(root, "M03_Sanctum_Processional_Foundation", rock, new Vector3(-11f, -19f, -24f), Quaternion.Euler(4f, 32f, -4f), new Vector3(10.54f, 15f, 13.6f));
            AddPrefabPiece(root, "M03_Sanctum_Ascent_Foundation", rock, new Vector3(-9f, -18.5f, -15f), Quaternion.Euler(-2f, 108f, 6f), new Vector3(12.4f, 16f, 15.64f));
            AddPrefabPiece(root, "M03_Sanctum_Threshold_Foundation", rock, new Vector3(-4f, -18f, -7f), Quaternion.Euler(3f, 52f, -4f), new Vector3(13.64f, 17f, 17f));
            AddPrefabPiece(root, "M03_Sanctum_Crown_Foundation", rock, new Vector3(0f, -19f, 0f), Quaternion.Euler(0f, -12f, 0f), new Vector3(17.36f, 20f, 21.76f));
            AddPrefabPiece(root, "M03_Sanctum_Lower_Foundation", rock, new Vector3(7f, -26f, 7f), Quaternion.Euler(-6f, 92f, 8f), new Vector3(11.16f, 20f, 14.96f));
            AddPrefabPiece(root, "M03_Sanctum_Threshold_Arch", arch, new Vector3(0f, 1.4f, 2f), Quaternion.Euler(0f, 4f, 0f), Vector3.one * 12f);
            AddPrefabPiece(root, "M03_Sanctum_Broken_Crown", ruin, new Vector3(9f, 0f, 5f), Quaternion.Euler(5f, -24f, 9f), Vector3.one * 7.4f);
            AddPrefabPiece(root, "M03_Sanctum_Fallen_Column", pillar, new Vector3(-7f, -1f, 3f), Quaternion.Euler(68f, 14f, -12f), Vector3.one * 4.5f);
            AddPrefabPiece(root, "M03_Sanctum_Patch_Nexus", energy, new Vector3(0f, 2.5f, 4f), Quaternion.identity, Vector3.one * 3.2f);
            AddPrefabPiece(root, "M03_Sanctum_Final_Buttress", pillar, new Vector3(-1f, -5f, -5f), Quaternion.identity, new Vector3(5f, 8f, 5f));
            return SaveLandmarkPrefab(root, "PF_RR_M03_PatchSanctum", true);
        }



        private static GameObject BuildPilgrimCauseway(Dictionary<string, Material> materials)
        {
            var root = new GameObject("PF_RR_Landmark_PilgrimCauseway");
            var gate = LoadModel("Assets/_Game/Art/ThirdParty/Kenney/CastleKit/Source/RR_Castle_Gate.fbx");
            var square = LoadModel("Assets/_Game/Art/ThirdParty/Kenney/CastleKit/Source/RR_Castle_Tower_Square.fbx");
            var open = LoadModel("Assets/_Game/Art/ThirdParty/Kenney/CastleKit/Source/RR_Castle_Tower_Mid_Open.fbx");
            var cliff = LoadModel("Assets/_Game/Art/ThirdParty/Kenney/NatureKit/Source/RR_Nature_Cliff_CornerLarge_Rock.fbx");
            var bush = LoadModel("Assets/_Game/Art/ThirdParty/Kenney/NatureKit/Source/RR_Nature_Bush_Large.fbx");

            // Act 1: the opening route skirts one reclaimed sanctuary island.
            AddPiece(root, "Arrival_Sanctuary_Rock", cliff, new Vector3(-8f, -8f, 11f), new Vector3(180f, 24f, 4f), new Vector3(10f, 9f, 14f), materials["rock"]);
            AddPiece(root, "Arrival_Sanctuary_Gate", gate, new Vector3(-11f, -1.6f, 15f), new Vector3(0f, 28f, -4f), new Vector3(3.2f, 4.2f, 3.2f), materials["ivory"]);
            AddPiece(root, "Arrival_Sanctuary_Green", bush, new Vector3(-9.5f, -0.8f, 13f), new Vector3(0f, 12f, 0f), Vector3.one * 2.4f, materials["green"]);

            // Act 2: two low bridgeheads make the Moving platform's ferry line legible.
            AddPiece(root, "Destroyed_Bridge_Near_Rock", cliff, new Vector3(9.5f, -8f, 30f), new Vector3(180f, -18f, 3f), new Vector3(8f, 8f, 10f), materials["rock"]);
            AddPiece(root, "Destroyed_Bridge_Near_Pier", square, new Vector3(10f, -3f, 31f), new Vector3(0f, -14f, 2f), new Vector3(3f, 5f, 3f), materials["sandstone"]);
            AddPiece(root, "Destroyed_Bridge_Far_Rock", cliff, new Vector3(-4f, -8f, 36f), new Vector3(180f, 20f, -3f), new Vector3(8f, 8f, 10f), materials["slate"]);
            AddPiece(root, "Destroyed_Bridge_Far_Pier", square, new Vector3(-4.5f, -3f, 35f), new Vector3(0f, 18f, -3f), new Vector3(3f, 5f, 3f), materials["sandstone"]);

            // Act 3: the Crumble chain crosses the split remains of one temple court.
            AddPiece(root, "Collapsed_Temple_Left_Rock", cliff, new Vector3(-9f, -8f, 48f), new Vector3(180f, 42f, 7f), new Vector3(9f, 8f, 12f), materials["rock"]);
            AddPiece(root, "Collapsed_Temple_Left_Ruin", open, new Vector3(-10f, -1.8f, 49f), new Vector3(0f, 34f, 7f), Vector3.one * 3.6f, materials["ivory"]);
            AddPiece(root, "Collapsed_Temple_Right_Rock", cliff, new Vector3(10f, -8f, 54f), new Vector3(180f, -26f, -6f), new Vector3(9f, 8f, 12f), materials["slate"]);
            AddPiece(root, "Collapsed_Temple_Right_Ruin", square, new Vector3(10.5f, -2.5f, 54f), new Vector3(0f, -22f, -5f), Vector3.one * 3.2f, materials["sandstone"]);
            AddPiece(root, "Collapsed_Temple_Green", bush, new Vector3(-8f, -1f, 51f), new Vector3(0f, 27f, 0f), Vector3.one * 2.2f, materials["green"]);

            // Act 4: low split pylons and foundations frame the Boost trajectory.
            AddPiece(root, "Ascent_Boost_Launch_Rock", cliff, new Vector3(7f, -9f, 61f), new Vector3(180f, 20f, 2f), new Vector3(8f, 9f, 11f), materials["rock"]);
            AddPiece(root, "Ascent_Boost_Left_Pylon", square, new Vector3(-5f, -3.5f, 62f), new Vector3(0f, 20f, 2f), new Vector3(2.8f, 5f, 2.8f), materials["slate"]);
            AddPiece(root, "Ascent_Boost_Landing_Rock", cliff, new Vector3(5f, -9f, 70f), new Vector3(180f, -18f, -3f), new Vector3(9f, 9f, 12f), materials["slate"]);
            AddPiece(root, "Ascent_Boost_Right_Pylon", square, new Vector3(11f, -3.5f, 69f), new Vector3(0f, -18f, -3f), new Vector3(2.8f, 5f, 2.8f), materials["sandstone"]);

            // Act 5: the final climb reaches a surviving sanctum foundation.
            AddPiece(root, "Patch_Sanctum_Rock", cliff, new Vector3(7f, -8f, 105f), new Vector3(180f, -20f, -5f), new Vector3(10f, 10f, 14f), materials["slate"]);
            AddPiece(root, "Patch_Sanctum_Gate", gate, new Vector3(12f, 5f, 110f), new Vector3(0f, -18f, 2f), new Vector3(3.2f, 4.5f, 3.2f), materials["sandstone"]);
            return SaveLandmarkPrefab(root, "PF_RR_Landmark_PilgrimCauseway");
        }

        private static GameObject BuildBrokenProcession(Dictionary<string, Material> materials)
        {
            var root = new GameObject("PF_RR_Landmark_BrokenProcession");
            var cliff = LoadModel("Assets/_Game/Art/ThirdParty/Kenney/NatureKit/Source/RR_Nature_Cliff_CornerLarge_Rock.fbx");

            AddPiece(root, "Procession_Cliff", cliff, new Vector3(0f, -10f, 0f), new Vector3(180f, 28f, 0f), new Vector3(24f, 18f, 21f), materials["rock"]);
            var arch = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Art/Environment/Architecture/PF_RR_FloatingArch.prefab");
            if (arch != null)
            {
                AddPrefabPiece(root, "Procession_Monumental_Arch", arch, new Vector3(0f, 7f, 0f), Quaternion.Euler(0f, 18f, 0f), Vector3.one * 14f);
            }
            return SaveLandmarkPrefab(root, "PF_RR_Landmark_BrokenProcession");
        }

        private static GameObject BuildSunkenRuinCity(Dictionary<string, Material> materials)
        {
            var root = new GameObject("PF_RR_Landmark_SunkenRuinCity");
            var square = LoadModel("Assets/_Game/Art/ThirdParty/Kenney/CastleKit/Source/RR_Castle_Tower_Square.fbx");
            var open = LoadModel("Assets/_Game/Art/ThirdParty/Kenney/CastleKit/Source/RR_Castle_Tower_Mid_Open.fbx");
            var roof = LoadModel("Assets/_Game/Art/ThirdParty/Kenney/CastleKit/Source/RR_Castle_Tower_Roof_High.fbx");
            var towers = new[]
            {
                new Vector3(-40f, 0f, -8f), new Vector3(-17f, 13f, 8f),
                new Vector3(9f, -4f, -3f), new Vector3(37f, 8f, 15f),
                new Vector3(1f, 18f, 37f), new Vector3(-30f, 5f, 44f)
            };
            for (var index = 0; index < towers.Length; index++)
            {
                var baseScale = 8f + index * 0.35f;
                var yaw = index * 37f;
                AddPiece(root, $"Drowned_City_Base_{index + 1:00}", square, towers[index],
                    new Vector3(0f, yaw, index % 2 == 0 ? 2f : -4f),
                    new Vector3(baseScale, 14f + index, baseScale), materials["slate"]);
                AddPiece(root, $"Drowned_City_Mid_{index + 1:00}", square, towers[index] + Vector3.up * 12f,
                    new Vector3(0f, yaw + 9f, index % 2 == 0 ? 2f : -4f),
                    new Vector3(baseScale * 0.9f, 12f, baseScale * 0.9f), materials["sandstone"]);
                AddPiece(root, $"Drowned_City_Crown_{index + 1:00}", open, towers[index] + Vector3.up * 24f,
                    new Vector3(0f, yaw + 18f, index % 2 == 0 ? 3f : -5f),
                    Vector3.one * (baseScale * 0.85f), materials["ivory"]);
                if (index % 2 == 0)
                {
                    AddPiece(root, $"Drowned_City_Peak_{index + 1:00}", roof, towers[index] + Vector3.up * 33f,
                        new Vector3(0f, yaw + 18f, -4f), Vector3.one * (baseScale * 0.75f), materials["gold"]);
                }
            }
            return SaveLandmarkPrefab(root, "PF_RR_Landmark_SunkenRuinCity");
        }

        private static GameObject BuildReclaimedSanctuary(Dictionary<string, Material> materials)
        {
            var root = new GameObject("PF_RR_Landmark_ReclaimedSanctuary");
            var cliff = LoadModel("Assets/_Game/Art/ThirdParty/Kenney/NatureKit/Source/RR_Nature_Cliff_Large_Rock.fbx");
            var open = LoadModel("Assets/_Game/Art/ThirdParty/Kenney/CastleKit/Source/RR_Castle_Tower_Mid_Open.fbx");
            var roof = LoadModel("Assets/_Game/Art/ThirdParty/Kenney/CastleKit/Source/RR_Castle_Tower_Roof_High.fbx");
            var bush = LoadModel("Assets/_Game/Art/ThirdParty/Kenney/NatureKit/Source/RR_Nature_Bush_Large.fbx");

            AddPiece(root, "Sanctuary_Cliff", cliff, new Vector3(0f, -10f, 0f), new Vector3(180f, 12f, 0f), new Vector3(22f, 22f, 20f), materials["rock"]);
            AddPiece(root, "Sanctuary_Open_Ruin", open, new Vector3(-3f, 4f, 0f), new Vector3(0f, 36f, 5f), new Vector3(11f, 13f, 11f), materials["ivory"]);
            AddPiece(root, "Sanctuary_Broken_Crown", roof, new Vector3(-4f, 17f, 0f), new Vector3(8f, 42f, 13f), Vector3.one * 8f, materials["sandstone"]);
            AddPiece(root, "Sanctuary_Green_Crown", bush, new Vector3(-5f, 13f, 1f), Vector3.zero, new Vector3(9f, 7f, 9f), materials["green"]);
            AddPiece(root, "Sanctuary_Green_Wall", bush, new Vector3(7f, 7f, 4f), new Vector3(0f, 50f, 0f), new Vector3(7f, 6f, 7f), materials["green"]);
            var arch = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Art/Environment/Architecture/PF_RR_FloatingArch.prefab");
            if (arch != null)
            {
                AddPrefabPiece(root, "Sanctuary_Broken_Arch", arch, new Vector3(8f, 6f, 5f), Quaternion.Euler(0f, -42f, -7f), Vector3.one * 8f);
            }
            return SaveLandmarkPrefab(root, "PF_RR_Landmark_ReclaimedSanctuary");
        }

        private static GameObject BuildColossalAbyssTower(Dictionary<string, Material> materials)
        {
            var root = new GameObject("PF_RR_Landmark_ColossalAbyssTower");

            AddPiece(root, "Abyss_Base_Hex", LoadModel("Assets/_Game/Art/ThirdParty/Kenney/CastleKit/Source/RR_Castle_Hex_Base.fbx"),
                new Vector3(0f, -30f, 0f), Vector3.zero, new Vector3(26f, 30f, 26f), materials["slate"]);

            AddPiece(root, "Tower_Body_Square", LoadModel("Assets/_Game/Art/ThirdParty/Kenney/CastleKit/Source/RR_Castle_Tower_Square.fbx"),
                new Vector3(0f, 0f, 0f), Vector3.zero, new Vector3(20f, 34f, 20f), materials["ivory"]);

            AddPiece(root, "Tower_Open_Mid", LoadModel("Assets/_Game/Art/ThirdParty/Kenney/CastleKit/Source/RR_Castle_Tower_Mid_Open.fbx"),
                new Vector3(0f, 22f, 0f), Vector3.zero, new Vector3(18f, 16f, 18f), materials["ivory"]);

            AddPiece(root, "Tower_Roof_Ruin", LoadModel("Assets/_Game/Art/ThirdParty/Kenney/CastleKit/Source/RR_Castle_Tower_Roof_High.fbx"),
                new Vector3(0f, 34f, 0f), Vector3.zero, new Vector3(16f, 16f, 16f), materials["ivory"]);

            var archPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Art/Environment/Architecture/PF_RR_FloatingArch.prefab");
            if (archPrefab != null)
            {
                AddPrefabPiece(root, "Tower_Crown_Arch", archPrefab, new Vector3(0f, 38f, 0f), Quaternion.Euler(0f, 45f, 0f), Vector3.one * 8f);
            }

            AddPiece(root, "Cliff_Buttress_L", LoadModel("Assets/_Game/Art/ThirdParty/Kenney/NatureKit/Source/RR_Nature_Cliff_CornerLarge_Rock.fbx"),
                new Vector3(-14f, -15f, -8f), new Vector3(0f, 30f, 0f), new Vector3(18f, 24f, 18f), materials["rock"]);
            AddPiece(root, "Cliff_Buttress_R", LoadModel("Assets/_Game/Art/ThirdParty/Kenney/NatureKit/Source/RR_Nature_Cliff_Slope_Rock.fbx"),
                new Vector3(14f, -18f, 8f), new Vector3(0f, -45f, 0f), new Vector3(20f, 22f, 20f), materials["rock"]);
            AddPiece(root, "Sunken_Fragment_L", LoadModel("Assets/_Game/Art/ThirdParty/Kenney/CastleKit/Source/RR_Castle_Hex_Mid.fbx"),
                new Vector3(-22f, -25f, 8f), new Vector3(0f, 18f, 8f), new Vector3(8f, 30f, 8f), materials["slate"]);
            AddPiece(root, "Sunken_Fragment_R", LoadModel("Assets/_Game/Art/ThirdParty/Kenney/CastleKit/Source/RR_Castle_Hex_Mid.fbx"),
                new Vector3(24f, -32f, -10f), new Vector3(0f, -25f, -6f), new Vector3(7f, 26f, 7f), materials["slate"]);

            return SaveLandmarkPrefab(root, "PF_RR_Landmark_ColossalAbyssTower");
        }

        private static GameObject BuildCelestialBrokenArch(Dictionary<string, Material> materials)
        {
            var root = new GameObject("PF_RR_Landmark_CelestialBrokenArch");

            var archPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Art/Environment/Architecture/PF_RR_FloatingArch.prefab");
            if (archPrefab != null)
            {
                AddPrefabPiece(root, "Hero_Arch", archPrefab, Vector3.zero, Quaternion.identity, Vector3.one * 14f);
            }

            AddPiece(root, "Left_Base_Hex", LoadModel("Assets/_Game/Art/ThirdParty/Kenney/CastleKit/Source/RR_Castle_Hex_Base.fbx"),
                new Vector3(-7f, -4f, 0f), Vector3.zero, new Vector3(8f, 8f, 8f), materials["ivory"]);
            AddPiece(root, "Left_Rock_Anchor", LoadModel("Assets/_Game/Art/ThirdParty/Kenney/NatureKit/Source/RR_Nature_Rock_Tall_A.fbx"),
                new Vector3(-9f, -8f, 2f), new Vector3(0f, 20f, 0f), new Vector3(10f, 14f, 10f), materials["rock"]);

            AddPiece(root, "Right_Base_Hex", LoadModel("Assets/_Game/Art/ThirdParty/Kenney/CastleKit/Source/RR_Castle_Hex_Base.fbx"),
                new Vector3(7f, -4f, 0f), Vector3.zero, new Vector3(8f, 8f, 8f), materials["ivory"]);
            AddPiece(root, "Right_Rock_Anchor", LoadModel("Assets/_Game/Art/ThirdParty/Kenney/NatureKit/Source/RR_Nature_Rock_Tall_F.fbx"),
                new Vector3(9f, -8f, -2f), new Vector3(0f, -30f, 0f), new Vector3(10f, 14f, 10f), materials["rock"]);

            AddPiece(root, "Span_Left", LoadModel("Assets/_Game/Art/ThirdParty/Kenney/CastleKit/Source/RR_Castle_Wall.fbx"),
                new Vector3(-12f, -1.5f, 0f), new Vector3(0f, 90f, 12f), new Vector3(10f, 6f, 6f), materials["ivory"]);
            AddPiece(root, "Span_Right", LoadModel("Assets/_Game/Art/ThirdParty/Kenney/CastleKit/Source/RR_Castle_Wall.fbx"),
                new Vector3(12f, -1.5f, 0f), new Vector3(0f, -90f, -12f), new Vector3(10f, 6f, 6f), materials["ivory"]);

            return SaveLandmarkPrefab(root, "PF_RR_Landmark_CelestialBrokenArch");
        }

        private static GameObject BuildFloatingRuinIsland(Dictionary<string, Material> materials)
        {
            var root = new GameObject("PF_RR_Landmark_FloatingRuinIsland");

            AddPiece(root, "Island_Rock_Large", LoadModel("Assets/_Game/Art/ThirdParty/Kenney/NatureKit/Source/RR_Nature_Cliff_Large_Rock.fbx"),
                new Vector3(0f, -3f, 0f), new Vector3(180f, 25f, 0f), new Vector3(16f, 12f, 16f), materials["rock"]);

            AddPiece(root, "Island_Tower", LoadModel("Assets/_Game/Art/ThirdParty/Kenney/CastleKit/Source/RR_Castle_Tower_Mid_Open.fbx"),
                new Vector3(-1.5f, 3f, 1f), new Vector3(0f, 15f, 0f), new Vector3(8f, 10f, 8f), materials["ivory"]);

            AddPiece(root, "Island_Wall", LoadModel("Assets/_Game/Art/ThirdParty/Kenney/CastleKit/Source/RR_Castle_Wall_Corner.fbx"),
                new Vector3(2.5f, 2f, -2f), new Vector3(0f, -40f, 0f), new Vector3(8f, 7f, 8f), materials["ivory"]);

            AddPiece(root, "Island_Bush_A", LoadModel("Assets/_Game/Art/ThirdParty/Kenney/NatureKit/Source/RR_Nature_Bush_Large.fbx"),
                new Vector3(3f, 2.5f, 2f), Vector3.zero, new Vector3(5f, 5f, 5f), materials["green"]);
            AddPiece(root, "Island_Bush_B", LoadModel("Assets/_Game/Art/ThirdParty/Kenney/NatureKit/Source/RR_Nature_Bush_Large.fbx"),
                new Vector3(-3f, 2f, -2.5f), new Vector3(0f, 60f, 0f), new Vector3(4f, 4f, 4f), materials["green"]);

            return SaveLandmarkPrefab(root, "PF_RR_Landmark_FloatingRuinIsland");
        }

        private static GameObject BuildDistantSunkenMonolith(Dictionary<string, Material> materials)
        {
            var root = new GameObject("PF_RR_Landmark_DistantSunkenMonolith");

            AddPiece(root, "Monolith_Base_Hex", LoadModel("Assets/_Game/Art/ThirdParty/Kenney/CastleKit/Source/RR_Castle_Hex_Base.fbx"),
                new Vector3(0f, -25f, 0f), Vector3.zero, new Vector3(32f, 40f, 32f), materials["slate"]);

            AddPiece(root, "Monolith_Spire_Body", LoadModel("Assets/_Game/Art/ThirdParty/Kenney/CastleKit/Source/RR_Castle_Hex_Mid.fbx"),
                new Vector3(0f, 20f, 0f), Vector3.zero, new Vector3(24f, 55f, 24f), materials["ivory"]);

            AddPiece(root, "Monolith_Peak", LoadModel("Assets/_Game/Art/ThirdParty/Kenney/CastleKit/Source/RR_Castle_Tower_Roof_High.fbx"),
                new Vector3(0f, 54f, 0f), Vector3.zero, new Vector3(20f, 24f, 20f), materials["ivory"]);

            var energyPillarPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Art/Environment/Decoration/PF_RR_EnergyPillar.prefab");
            if (energyPillarPrefab != null)
            {
                AddPrefabPiece(root, "Monolith_Energy_Nexus", energyPillarPrefab, new Vector3(0f, 25f, 0f), Quaternion.identity, Vector3.one * 12f);
            }

            return SaveLandmarkPrefab(root, "PF_RR_Landmark_DistantSunkenMonolith");
        }

        private static GameObject BuildAncientCentralSpire(Dictionary<string, Material> materials)
        {
            var root = new GameObject("PF_RR_Landmark_AncientCentralSpire");

            AddPiece(root, "Pedestal_Hex", LoadModel("Assets/_Game/Art/ThirdParty/Kenney/CastleKit/Source/RR_Castle_Hex_Base.fbx"),
                new Vector3(0f, -8f, 0f), Vector3.zero, new Vector3(14f, 14f, 14f), materials["slate"]);

            var ruinedTowerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Art/Environment/Architecture/PF_RR_RuinedTower.prefab");
            if (ruinedTowerPrefab != null)
            {
                AddPrefabPiece(root, "Central_Ruined_Core", ruinedTowerPrefab, new Vector3(0f, 2f, 0f), Quaternion.Euler(0f, 30f, 0f), Vector3.one * 10f);
            }

            var pillarPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Art/Environment/Architecture/PF_RR_Pillar_Tall.prefab");
            if (pillarPrefab != null)
            {
                AddPrefabPiece(root, "Central_Ascent_Pillar", pillarPrefab, new Vector3(2.5f, 8f, -1.5f), Quaternion.identity, Vector3.one * 9f);
            }

            AddPiece(root, "Arched_Buttress", LoadModel("Assets/_Game/Art/ThirdParty/Kenney/CastleKit/Source/RR_Castle_Tower_Mid_Open.fbx"),
                new Vector3(-3.5f, 4f, 2f), new Vector3(0f, 60f, 0f), new Vector3(8f, 10f, 8f), materials["ivory"]);

            return SaveLandmarkPrefab(root, "PF_RR_Landmark_AncientCentralSpire");
        }

        private static GameObject BuildAqueductButtress(Dictionary<string, Material> materials)
        {
            var root = new GameObject("PF_RR_Landmark_AqueductButtress");

            AddPiece(root, "Aqueduct_Rock", LoadModel("Assets/_Game/Art/ThirdParty/Kenney/NatureKit/Source/RR_Nature_Rock_Large_A.fbx"),
                new Vector3(0f, -4f, 0f), Vector3.zero, new Vector3(12f, 8f, 12f), materials["rock"]);

            AddPiece(root, "Aqueduct_Gate", LoadModel("Assets/_Game/Art/ThirdParty/Kenney/CastleKit/Source/RR_Castle_Gate.fbx"),
                new Vector3(0f, 1.5f, 0f), Vector3.zero, new Vector3(10f, 8f, 8f), materials["ivory"]);

            AddPiece(root, "Aqueduct_Wall", LoadModel("Assets/_Game/Art/ThirdParty/Kenney/CastleKit/Source/RR_Castle_Wall.fbx"),
                new Vector3(-6f, 1f, 0f), new Vector3(0f, 90f, 0f), new Vector3(8f, 7f, 6f), materials["ivory"]);

            return SaveLandmarkPrefab(root, "PF_RR_Landmark_AqueductButtress");
        }

        private static GameObject LoadModel(string path)
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (model == null)
            {
                throw new FileNotFoundException($"Required model asset not found: {path}");
            }
            return model;
        }

        private static GameObject LoadPrefab(string path)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                throw new FileNotFoundException($"Required prefab asset not found: {path}");
            }
            return prefab;
        }

        private static void AddPiece(
            GameObject parent,
            string name,
            GameObject model,
            Vector3 localPos,
            Vector3 localEuler,
            Vector3 localScale,
            Material material)
        {
            var child = UnityEngine.Object.Instantiate(model, parent.transform);
            child.name = name;
            child.transform.localPosition = localPos;
            child.transform.localRotation = Quaternion.Euler(localEuler);
            child.transform.localScale = localScale;

            foreach (var collider in child.GetComponentsInChildren<Collider>(true))
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }

            foreach (var renderer in child.GetComponentsInChildren<Renderer>(true))
            {
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                renderer.lightProbeUsage = LightProbeUsage.Off;
                renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
                renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
            }
        }

        private static void AddPrefabPiece(
            GameObject parent,
            string name,
            GameObject prefab,
            Vector3 localPos,
            Quaternion localRot,
            Vector3 localScale)
        {
            var child = UnityEngine.Object.Instantiate(prefab, parent.transform);
            child.name = name;
            child.transform.localPosition = localPos;
            child.transform.localRotation = localRot;
            child.transform.localScale = localScale;

            foreach (var collider in child.GetComponentsInChildren<Collider>(true))
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }

            foreach (var renderer in child.GetComponentsInChildren<Renderer>(true))
            {
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                renderer.lightProbeUsage = LightProbeUsage.Off;
                renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
                renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
            }
        }

        private static GameObject SaveLandmarkPrefab(GameObject instance, string prefabName, bool playableArchitecture = false)
        {
            foreach (var collider in instance.GetComponentsInChildren<Collider>(true))
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }

            if (playableArchitecture)
            {
                foreach (var filter in instance.GetComponentsInChildren<MeshFilter>(true))
                {
                    // Curated Meshy architecture/terrain only. Kenney foliage stays permeable.
                    if (!AssetDatabase.GetAssetPath(filter.sharedMesh).Contains("/MeshySource/")) continue;
                    filter.gameObject.layer = LayerMask.NameToLayer("Ground");
                    var collider = filter.gameObject.AddComponent<MeshCollider>();
                    collider.sharedMesh = filter.sharedMesh;
                    collider.convex = false;
                    filter.gameObject.AddComponent<AuthoredSurface>().SetSourceMesh(filter.sharedMesh);
                }
            }

            instance.isStatic = true;
            foreach (var child in instance.GetComponentsInChildren<Transform>(true))
            {
                child.gameObject.isStatic = true;
            }

            var prefabPath = $"{LandmarkPrefabRoot}/{prefabName}.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            UnityEngine.Object.DestroyImmediate(instance);
            return prefab;
        }

        public static EnvironmentBiomeProfile ConfigureAncientAbyssBiome(Dictionary<string, GameObject> landmarks)
        {
            var path = $"{BiomeRoot}/Biome_AncientAbyss.asset";
            var profile = AssetDatabase.LoadAssetAtPath<EnvironmentBiomeProfile>(path);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<EnvironmentBiomeProfile>();
                AssetDatabase.CreateAsset(profile, path);
            }

            // The panoramic sky carries the distant cloud ocean. Horizontal mist
            // meshes and billboard fields are forbidden because they read as floors.
            var mistLayers = Array.Empty<BiomeMistLayer>();

            var worldObjects = new[]
            {
                new BiomeWorldObject(
                    "biome.ancient-abyss.place.arrival-sanctuary",
                    landmarks["M03ArrivalSanctuary"],
                    new Vector3(0f, 0f, 6f),
                    Vector3.zero,
                    Vector3.one,
                    depthBand: BiomeDepthBand.NearEnvironment,
                    depthFadeStrength: 0.04f, hasPlayableArchitecture: true),

                new BiomeWorldObject(
                    "biome.ancient-abyss.place.broken-crossing",
                    landmarks["M03BrokenCrossing"],
                    new Vector3(0f, 2f, 36f),
                    Vector3.zero,
                    Vector3.one,
                    depthBand: BiomeDepthBand.NearEnvironment,
                    depthFadeStrength: 0.04f, hasPlayableArchitecture: true),

                new BiomeWorldObject(
                    "biome.ancient-abyss.place.collapsed-temple",
                    landmarks["M03CollapsedTemple"],
                    new Vector3(0f, 4f, 59f),
                    Vector3.zero,
                    Vector3.one,
                    depthBand: BiomeDepthBand.NearEnvironment,
                    depthFadeStrength: 0.04f, hasPlayableArchitecture: true),

                new BiomeWorldObject(
                    "biome.ancient-abyss.place.energy-spine",
                    landmarks["M03EnergySpine"],
                    new Vector3(0f, 7f, 90f),
                    Vector3.zero,
                    Vector3.one,
                    depthBand: BiomeDepthBand.NearEnvironment,
                    depthFadeStrength: 0.04f, hasPlayableArchitecture: true),

                new BiomeWorldObject(
                    "biome.ancient-abyss.place.patch-sanctum",
                    landmarks["M03PatchSanctum"],
                    new Vector3(8f, 14f, 129f),
                    Vector3.zero,
                    Vector3.one,
                    depthBand: BiomeDepthBand.NearEnvironment,
                    depthFadeStrength: 0.04f, hasPlayableArchitecture: true),

                new BiomeWorldObject(
                    "biome.ancient-abyss.mid.broken-procession",
                    landmarks["BrokenProcession"],
                    new Vector3(-110f, -60f, 95f),
                    new Vector3(0f, 24f, 0f),
                    Vector3.one * 1.25f,
                    depthBand: BiomeDepthBand.MidWorld,
                    depthFadeStrength: 0.52f),

                new BiomeWorldObject(
                    "biome.ancient-abyss.mid.celestial-arch",
                    landmarks["CelestialBrokenArch"],
                    new Vector3(120f, -70f, 155f),
                    new Vector3(0f, -38f, -4f),
                    Vector3.one * 1.35f,
                    depthBand: BiomeDepthBand.MidWorld,
                    depthFadeStrength: 0.48f),

                new BiomeWorldObject(
                    "biome.ancient-abyss.hero.shattered-crown",
                    landmarks["AncientTempleComplex"],
                    new Vector3(45f, -42f, 245f),
                    new Vector3(0f, -20f, 0f),
                    Vector3.one * 1.55f,
                    depthBand: BiomeDepthBand.FarWorld,
                    depthFadeStrength: 0.32f),

                new BiomeWorldObject(
                    "biome.ancient-abyss.below.sunken-city",
                    landmarks["SunkenRuinCity"],
                    new Vector3(4f, -94f, 148f),
                    new Vector3(0f, -12f, 0f),
                    Vector3.one * 1.05f,
                    depthBand: BiomeDepthBand.LowerAbyss,
                    depthFadeStrength: 0.9f)
            };

            profile.name = "Biome_AncientAbyss";
            profile.Configure(
                "biome.ancient-abyss",
                "Ancient Abyss",
                EnvironmentBiomeKind.AncientAbyss,
                new Color(0.55f, 0.68f, 0.88f, 1f),
                new Color(0.6f, 0.74f, 0.91f, 0.18f),
                new Color(0.32f, 0.48f, 0.7f, 1f),
                -72f,
                mistLayers,
                worldObjects);

            EditorUtility.SetDirty(profile);
            return profile;
        }

        public static void ConfigureModule003Content(
            EnvironmentBiomeProfile biome,
            Dictionary<string, GameObject> landmarks)
        {
            var path = $"{ModuleRoot}/Module_003_FlowError.asset";
            var module = AssetDatabase.LoadAssetAtPath<ModuleDefinition>(path);
            if (module == null)
            {
                throw new FileNotFoundException($"Module 003 asset missing: {path}");
            }

            var visual = AssetDatabase.LoadAssetAtPath<ModuleVisualProfile>(
                Phase2ModuleContentFactory.ModuleVisualProfilePath);
            var environment = AssetDatabase.LoadAssetAtPath<ModuleEnvironmentProfile>(
                Phase2ModuleContentFactory.ModuleEnvironmentProfilePath);

            var decorations = Array.Empty<ModuleBlockDefinition>();

            module.Configure(
                "module.003.flow-error",
                "Campaign 03 - Flow Error",
                "module_003_flow_error",
                "project.ryders-block",
                "world.prototype-sky",
                7,
                ModuleSelectionState.ModuleRunnerSceneName,
                ModuleDifficulty.Medium,
                new ModulePose(new Vector3(0f, 0.35f, -7f), Vector3.zero),
                new ModulePatchBlockDefinition(
                    "patch.module-003",
                    new Vector3(8f, 15f, 129f),
                    new Vector3(2.2f, 2.2f, 2.2f)),
                new[]
                {
                    new ModuleRestorePointDefinition(
                        "restore.module-003.arrival-sanctuary",
                        0,
                        new Vector3(5f, 1.15f, 20f),
                        new Vector3(0f, 0.35f, 0f)),
                    new ModuleRestorePointDefinition(
                        "restore.module-003.broken-crossing",
                        1,
                        new Vector3(-5f, 2.85f, 47f),
                        new Vector3(0f, 0.35f, 0f)),
                    new ModuleRestorePointDefinition(
                        "restore.module-003.collapsed-temple",
                        2,
                        new Vector3(1f, 4.95f, 71f),
                        new Vector3(0f, 0.35f, 0f))
                },
                72f,
                160f,
                new[]
                {
                    ModuleMechanic.StandardBlock,
                    ModuleMechanic.PrecisionBlock,
                    ModuleMechanic.MovingBlock,
                    ModuleMechanic.RestorePoint,
                    ModuleMechanic.JumpBoost,
                    ModuleMechanic.CrumblingBlock,
                    ModuleMechanic.Shortcut,
                    ModuleMechanic.PatchBlock,
                    ModuleMechanic.CameraGuide
                },
                new[]
                {
                    new ModuleShortcutDefinition(
                        "shortcut.module-003.arrival-court-cut",
                        "Arrival court momentum cut",
                        new Vector3(-2.5f, 0.1f, -2.5f),
                        new Vector3(1f, 0.8f, 11f),
                        2.8f,
                        new[] { ModuleMechanic.Shortcut, ModuleMechanic.PrecisionBlock }),
                    new ModuleShortcutDefinition(
                        "shortcut.module-003.moving-bypass",
                        "Broken crossing upper ledge",
                        new Vector3(7f, 1.8f, 30f),
                        new Vector3(-6f, 2.5f, 43f),
                        4.5f,
                        new[] { ModuleMechanic.Shortcut, ModuleMechanic.PrecisionBlock }),
                    new ModuleShortcutDefinition(
                        "shortcut.module-003.temple-upper-ledge",
                        "Collapsed temple upper ledge",
                        new Vector3(-3f, 3f, 51f),
                        new Vector3(3f, 4.6f, 67f),
                        3.6f,
                        new[] { ModuleMechanic.Shortcut, ModuleMechanic.PrecisionBlock })
                },
                environment,
                visual,
                "A broken route compiler has made the road unstable around an ancient abyss sanctuary.",
                "0.9.1 collision-truth and flow continuation: a connected journey through Arrival Sanctuary, Broken Crossing, Collapsed Temple, Energy Spine, and Patch Sanctum. Rank times require physical S23 calibration.",
                new[]
                {
                    Block("m03.start", 0f, 0f, -7f, 5f, 0.6f, 4f, ModuleMaterialRole.Normal),
                    Block("m03.opening.01", -2.5f, 0.1f, -2.5f, 2.5f, 0.6f, 2.5f, ModuleMaterialRole.Normal),
                    Block("m03.opening.02", -4f, 0.3f, 2f, 2.2f, 0.6f, 2.2f, ModuleMaterialRole.Normal),
                    Block("m03.opening.03", -2f, 0.5f, 6.5f, 2.5f, 0.6f, 2.5f, ModuleMaterialRole.Normal),
                    Block("m03.arrival.approach", 1f, 0.8f, 11f, 3f, 0.6f, 2.5f, ModuleMaterialRole.Normal),
                    Block("m03.arrival.court", 4f, 1f, 15.5f, 4f, 0.6f, 3f, ModuleMaterialRole.Normal),
                    Block("m03.crossing.approach", 7f, 1.5f, 25f, 3f, 0.6f, 3f, ModuleMaterialRole.Normal),
                    Block("m03.moving-setup", 7f, 1.8f, 30f, 4f, 0.6f, 3.5f, ModuleMaterialRole.Normal),
                    Block("m03.motion-exit", -6f, 2.5f, 43f, 4f, 0.6f, 4f, ModuleMaterialRole.Normal),
                    Block("m03.temple.entry", -3f, 3f, 51f, 3f, 0.6f, 2.5f, ModuleMaterialRole.Normal),
                    Block("m03.temple.exit", 3f, 4.6f, 67f, 3f, 0.6f, 3f, ModuleMaterialRole.Normal),
                    Block("m03.spine.approach", 3f, 5.2f, 75f, 3f, 0.6f, 3f, ModuleMaterialRole.Normal),
                    Block("m03.boost-runup", 6f, 5.6f, 80f, 4f, 0.6f, 3f, ModuleMaterialRole.Normal),
                    Block("m03.boost-landing", -1f, 8.8f, 96f, 8f, 0.6f, 8f, ModuleMaterialRole.Normal),
                    Block("m03.final.01", -3f, 9.4f, 102f, 3f, 0.6f, 3f, ModuleMaterialRole.Normal),
                    Block("m03.final.02", -6f, 10f, 107f, 2.2f, 0.6f, 2.2f, ModuleMaterialRole.Normal),
                    Block("m03.final.03", -4f, 10.7f, 112f, 2.2f, 0.6f, 2.2f, ModuleMaterialRole.Normal),
                    Block("m03.final.04", 0f, 11.5f, 116f, 3f, 0.6f, 3f, ModuleMaterialRole.Normal),
                    Block("m03.final.05", 4f, 12.4f, 120f, 2.5f, 0.6f, 2.5f, ModuleMaterialRole.Normal),
                    Block("m03.final.06", 7f, 13.4f, 124f, 3f, 0.6f, 3f, ModuleMaterialRole.Normal),
                    Block("m03.mastery.arrival-cut", 0f, 0.55f, 5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Precision),
                    Block("m03.mastery.moving-bypass.01", 2.2f, 2.2f, 30.7f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Precision),
                    Block("m03.mastery.moving-bypass.02", -3.6f, 2.35f, 33.4f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Precision),
                    Block("m03.mastery.moving-bypass.03", -8.9f, 2.5f, 36.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Precision),
                    Block("m03.mastery.temple-ledge.01", -2f, 4.3f, 57f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Precision),
                    Block("m03.mastery.temple-ledge.02", 1f, 4.6f, 63f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Precision)
                },
                new[]
                {
                    Moving(
                        "moving.m03.motion-gallery",
                        new Vector3(7f, 2.1f, 34f),
                        new Vector3(4f, 0.6f, 4f),
                        new[] { new Vector3(7f, 2.1f, 34f), new Vector3(-6f, 2.4f, 39f) },
                        5f,
                        0.24f)
                },
                new[]
                {
                    new ModuleBoostBlockDefinition(
                        "boost.m03.first",
                        new Vector3(6f, 5.9f, 84f),
                        new Vector3(3f, 0.6f, 3f),
                        new Vector3(-0.504f, 0f, 0.864f),
                        16f,
                        12.2f,
                        0.35f)
                },
                Array.Empty<ModuleWaterVolumeDefinition>(),
                new[]
                {
                    new ModuleCrumblingBlockDefinition(
                        "crumble.m03.01",
                        new Vector3(0f, 3.4f, 55f),
                        new Vector3(2f, 0.6f, 2f),
                        0.03f,
                        1f,
                        4f,
                        0.03f),
                    new ModuleCrumblingBlockDefinition(
                        "crumble.m03.02",
                        new Vector3(3.5f, 3.8f, 59f),
                        new Vector3(2f, 0.6f, 2f),
                        0.03f,
                        1f,
                        4f,
                        0.03f),
                    new ModuleCrumblingBlockDefinition(
                        "crumble.m03.03",
                        new Vector3(5f, 4.2f, 63f),
                        new Vector3(2f, 0.6f, 2f),
                        0.03f,
                        1f,
                        4f,
                        0.03f)
                },
                decorations,
                new[]
                {
                    new ModuleCameraHintDefinition(
                        "camera.m03.motion-gallery",
                        new Vector3(0f, 3f, 36f),
                        new Vector3(24f, 9f, 28f),
                        2f,
                        -8f,
                        12f),
                    new ModuleCameraHintDefinition(
                        "camera.m03.final-ascent",
                        new Vector3(1f, 11f, 112f),
                        new Vector3(22f, 14f, 40f),
                        8f,
                        -10f,
                        14f)
                });

            module.ConfigureEnvironmentBiome(biome);
            module.ConfigureRankThresholds(new ModuleRankThresholds(
                135f,
                100f,
                76f,
                RankCalibrationState.Uncalibrated,
                5,
                "UNCALIBRATED - 0.8.1 hand-authored Ancient Abyss route requires human Samsung S23 timing. Any valid completion remains Bronze."));
            module.ConfigurePlayability(
                "Module03StartAnchor",
                "m03.start",
                -12f,
                disableAutomaticDistantFragments: true);

            EditorUtility.SetDirty(module);
        }

        private static ModuleBlockDefinition Block(
            string stableId,
            float x,
            float y,
            float z,
            float sx,
            float sy,
            float sz,
            ModuleMaterialRole role) =>
            new ModuleBlockDefinition(
                stableId,
                new Vector3(x, y, z),
                new Vector3(sx, sy, sz),
                role,
                stableId,
                editorLabel: true);

        private static ModuleMovingBlockDefinition Moving(
            string stableId,
            Vector3 position,
            Vector3 size,
            Vector3[] points,
            float speed,
            float pause) =>
            new ModuleMovingBlockDefinition(
                stableId,
                position,
                size,
                points,
                speed,
                pause,
                ModulePathSpace.World,
                ModuleMovingLoopMode.PingPong,
                ModuleEasing.SmoothStep,
                0f);
    }
}
