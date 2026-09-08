using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Avoidance.Gameplay.Levels;
using Avoidance.Gameplay.Visuals;
using Avoidance.Gameplay.Worlds;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Avoidance.EditorTools
{
    public static class Phase073WorldBiomeImporter
    {
        private const string ThirdPartyRoot = "Assets/_Game/Art/ThirdParty/Kenney";
        private const string PrefabRoot = "Assets/_Game/Art/Environment/WorldAssets";
        private const string MaterialRoot = "Assets/_Game/Art/Materials/World";
        private const string BiomeRoot = "Assets/_Game/Worlds/Resources/EnvironmentBiomes";
        private const string ModuleRoot = "Assets/_Game/Levels/Resources/Modules";

        [MenuItem("RYDERS BLOCK/Phase 0.7.3/Import World Assets And Biomes")]
        public static void ImportWorldAssetsAndBiomes()
        {
            EnsureImportedAssetsAndBiomes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void EnsureImportedAssetsAndBiomes()
        {
            Directory.CreateDirectory(ThirdPartyRoot);
            Directory.CreateDirectory(PrefabRoot);
            Directory.CreateDirectory(MaterialRoot);
            Directory.CreateDirectory(BiomeRoot);

            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            if (string.IsNullOrWhiteSpace(projectRoot))
            {
                throw new InvalidOperationException("Could not resolve project root.");
            }

            var cacheRoot = Path.Combine(projectRoot, ".codex-asset-cache", "phase073");
            Directory.CreateDirectory(cacheRoot);

            foreach (var pack in Packs)
            {
                EnsurePack(cacheRoot, pack);
            }

            var materials = EnsureWorldMaterials();
            var prefabs = new Dictionary<string, GameObject>(StringComparer.Ordinal);
            foreach (var asset in Assets)
            {
                var modelPath = CopySelectedModel(cacheRoot, asset);
                AssetDatabase.ImportAsset(modelPath, ImportAssetOptions.ForceUpdate);
                prefabs[asset.Key] = EnsurePreparedPrefab(asset, modelPath, materials[asset.MaterialKey]);
            }

            var firstSteps = AssetDatabase.LoadAssetAtPath<ModuleDefinition>(ModuleRoot + "/Module_001_FirstSteps.asset");
            var authoredCity = firstSteps != null && firstSteps.ContentVersion >= 2
                ? AssetDatabase.LoadAssetAtPath<EnvironmentBiomeProfile>(BiomeRoot + "/Biome_SkyCity.asset") : null;
            var skyCity = authoredCity != null ? authoredCity : EnsureBiome(
                "Biome_SkyCity",
                "biome.sky-city",
                "Sky City",
                EnvironmentBiomeKind.SkyCity,
                new Color(0.58f, 0.78f, 0.96f, 1f),
                new Color(0.78f, 0.9f, 1f, 0.24f),
                new Color(0.28f, 0.5f, 0.78f, 0.18f),
                -38f,
                CreateSkyCityMist(),
                CreateSkyCityObjects(prefabs));
            var movingParts = AssetDatabase.LoadAssetAtPath<ModuleDefinition>(ModuleRoot + "/Module_002_MovingParts.asset");
            var authoredMountain = movingParts != null && movingParts.ContentVersion >= 2
                ? AssetDatabase.LoadAssetAtPath<EnvironmentBiomeProfile>(BiomeRoot + "/Biome_MountainSky.asset") : null;
            var mountainSky = authoredMountain != null ? authoredMountain : EnsureBiome(
                "Biome_MountainSky",
                "biome.mountain-sky",
                "Mountain Sky",
                EnvironmentBiomeKind.MountainSky,
                new Color(0.7f, 0.82f, 0.96f, 1f),
                new Color(0.82f, 0.9f, 0.98f, 0.24f),
                new Color(0.42f, 0.55f, 0.72f, 0.2f),
                -42f,
                CreateMountainMist(),
                CreateMountainObjects(prefabs));
            Phase074WorldSystemRecovery.EnsureAncientAbyssSkybox();
            var worldMats = Phase074WorldSystemRecovery.EnsureWorldMaterials();
            var landmarks = Phase074WorldSystemRecovery.EnsureLandmarkPrefabs(worldMats);
            var authoredAbyss = AssetDatabase.LoadAssetAtPath<ModuleDefinition>(ModuleRoot + "/Module_003_FlowError.asset");
            var ancientAbyss = authoredAbyss != null && authoredAbyss.ContentVersion >= 7
                ? AssetDatabase.LoadAssetAtPath<EnvironmentBiomeProfile>(BiomeRoot + "/Biome_AncientAbyss.asset") : null;
            if (ancientAbyss == null)
            {
                ancientAbyss = Phase074WorldSystemRecovery.ConfigureAncientAbyssBiome(landmarks);
                Phase074WorldSystemRecovery.ConfigureModule003Content(ancientAbyss, landmarks);
            }
            EnsureBiome(
                "Biome_EnergyVoid",
                "biome.energy-void",
                "Energy Void",
                EnvironmentBiomeKind.EnergyVoid,
                new Color(0.35f, 0.62f, 0.9f, 1f),
                new Color(0.45f, 0.82f, 1f, 0.2f),
                new Color(0.08f, 0.16f, 0.28f, 0.28f),
                -48f,
                CreateEnergyMist(),
                CreateEnergyObjects(prefabs));

            AssignCampaignBiomes(skyCity, mountainSky, ancientAbyss);
        }

        private static void EnsurePack(string cacheRoot, PackSpec pack)
        {
            var zipPath = Path.Combine(cacheRoot, pack.ZipName);
            if (!File.Exists(zipPath))
            {
                using var client = new System.Net.WebClient();
                client.DownloadFile(pack.Url, zipPath);
            }

            var extractRoot = Path.Combine(cacheRoot, pack.ExtractDirectory);
            if (Directory.Exists(extractRoot))
            {
                return;
            }

            ZipFile.ExtractToDirectory(zipPath, extractRoot);
        }

        private static Dictionary<string, Material> EnsureWorldMaterials()
        {
            return new Dictionary<string, Material>(StringComparer.Ordinal)
            {
                ["city"] = EnsureMaterial(
                    "MAT_RR_World_City_Navy",
                    new Color(0.08f, 0.12f, 0.22f, 1f)),
                ["tech"] = EnsureEmissiveMaterial(
                    "MAT_RR_World_City_Cyan",
                    new Color(0.08f, 0.72f, 0.95f, 1f)),
                ["rock"] = EnsureMaterial(
                    "MAT_RR_World_Mountain_Rock",
                    new Color(0.46f, 0.52f, 0.58f, 1f)),
                ["green"] = EnsureMaterial(
                    "MAT_RR_World_Subtle_Vegetation",
                    new Color(0.18f, 0.38f, 0.26f, 1f)),
                ["ancient"] = EnsureMaterial(
                    "MAT_RR_World_Ancient_Ivory",
                    new Color(0.7f, 0.68f, 0.58f, 1f)),
                ["dark"] = EnsureMaterial(
                    "MAT_RR_World_Deep_Slate",
                    new Color(0.12f, 0.14f, 0.22f, 1f))
            };
        }

        private static Material EnsureMaterial(string name, Color color)
        {
            var path = $"{MaterialRoot}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(VisualMaterialUtility.ResolveOpaqueShader());
                AssetDatabase.CreateAsset(material, path);
            }

            material.name = name;
            material.shader = VisualMaterialUtility.ResolveOpaqueShader();
            VisualMaterialUtility.ApplyColor(material, color);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material EnsureEmissiveMaterial(string name, Color color)
        {
            var path = $"{MaterialRoot}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(VisualMaterialUtility.ResolveOpaqueShader());
                AssetDatabase.CreateAsset(material, path);
            }

            material.name = name;
            material.shader = VisualMaterialUtility.ResolveOpaqueShader();
            VisualMaterialUtility.ApplyColor(material, color);
            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 1.35f);
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static string CopySelectedModel(string cacheRoot, WorldAssetSpec asset)
        {
            var targetDirectory = $"{ThirdPartyRoot}/{asset.Pack.AssetDirectory}/Source";
            var targetPath = $"{targetDirectory}/{asset.ProductionFileName}";
            // Authored, versioned models are authoritative. Startup must not overwrite
            // them from a workstation cache (or replace a file Unity has mapped).
            if (File.Exists(targetPath)) return targetPath.Replace('\\', '/');
            var sourcePath = Path.Combine(
                cacheRoot,
                asset.Pack.ExtractDirectory,
                "Models",
                "FBX format",
                asset.OriginalFileName);
            if (!File.Exists(sourcePath))
            {
                throw new FileNotFoundException($"Selected Kenney source model is missing: {sourcePath}");
            }

            Directory.CreateDirectory(targetDirectory);
            File.Copy(sourcePath, targetPath, overwrite: true);
            return targetPath.Replace('\\', '/');
        }

        private static GameObject EnsurePreparedPrefab(
            WorldAssetSpec asset,
            string modelPath,
            Material material)
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (model == null)
            {
                throw new InvalidOperationException($"Unity could not import selected model: {modelPath}");
            }

            var prefabDirectory = $"{PrefabRoot}/{asset.Category}";
            Directory.CreateDirectory(prefabDirectory);
            var prefabPath = $"{prefabDirectory}/PF_RR_World_{asset.Key}.prefab";
            var instance = PrefabUtility.InstantiatePrefab(model) as GameObject;
            if (instance == null)
            {
                instance = UnityEngine.Object.Instantiate(model);
            }

            instance.name = $"PF_RR_World_{asset.Key}";
            foreach (var renderer in instance.GetComponentsInChildren<Renderer>(includeInactive: true))
            {
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
                renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
                renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
            }

            foreach (var collider in instance.GetComponentsInChildren<Collider>(includeInactive: true))
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }

            instance.isStatic = true;
            var prefab = PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            UnityEngine.Object.DestroyImmediate(instance);
            return prefab;
        }

        private static EnvironmentBiomeProfile EnsureBiome(
            string assetName,
            string biomeId,
            string displayName,
            EnvironmentBiomeKind kind,
            Color atmosphereTint,
            Color mistColor,
            Color hazeColor,
            float mistOceanY,
            BiomeMistLayer[] mistLayers,
            BiomeWorldObject[] worldObjects)
        {
            var path = $"{BiomeRoot}/{assetName}.asset";
            var profile = AssetDatabase.LoadAssetAtPath<EnvironmentBiomeProfile>(path);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<EnvironmentBiomeProfile>();
                AssetDatabase.CreateAsset(profile, path);
            }

            profile.name = assetName;
            profile.Configure(
                biomeId,
                displayName,
                kind,
                atmosphereTint,
                mistColor,
                hazeColor,
                mistOceanY,
                mistLayers,
                worldObjects);
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void AssignCampaignBiomes(
            EnvironmentBiomeProfile skyCity,
            EnvironmentBiomeProfile mountainSky,
            EnvironmentBiomeProfile ancientAbyss)
        {
            AssignBiome("Module_001_FirstSteps.asset", skyCity);
            AssignBiome("Module_002_MovingParts.asset", mountainSky);
            AssignBiome("Module_003_FlowError.asset", ancientAbyss);
        }

        private static void AssignBiome(string moduleAssetName, EnvironmentBiomeProfile biome)
        {
            var module = AssetDatabase.LoadAssetAtPath<ModuleDefinition>($"{ModuleRoot}/{moduleAssetName}");
            if (module == null)
            {
                return;
            }

            module.ConfigureEnvironmentBiome(biome);
            EditorUtility.SetDirty(module);
        }

        private static BiomeMistLayer[] CreateSkyCityMist() => new[]
        {
            Mist("biome.sky-city.mist.ocean-a", 0f, -34f, 18f, 92f, 7f, 62f, 0.24f),
            Mist("biome.sky-city.mist.ocean-b", -35f, -39f, 58f, 82f, 8f, 74f, 0.2f),
            Mist("biome.sky-city.mist.ocean-c", 38f, -42f, 92f, 88f, 9f, 76f, 0.18f),
            Mist("biome.sky-city.mist.deep", 0f, -52f, 78f, 132f, 14f, 104f, 0.16f)
        };

        private static BiomeMistLayer[] CreateMountainMist() => new[]
        {
            Mist("biome.mountain-sky.mist.ocean-a", -8f, -39f, 16f, 96f, 8f, 70f, 0.23f),
            Mist("biome.mountain-sky.mist.ocean-b", 36f, -44f, 62f, 86f, 9f, 78f, 0.2f),
            Mist("biome.mountain-sky.mist.ocean-c", -42f, -46f, 96f, 92f, 10f, 82f, 0.18f),
            Mist("biome.mountain-sky.mist.deep", 0f, -57f, 72f, 126f, 15f, 112f, 0.15f)
        };

        private static BiomeMistLayer[] CreateAncientMist() => new[]
        {
            Mist("biome.ancient-abyss.mist.ocean-a", 0f, -42f, 20f, 86f, 8f, 66f, 0.24f),
            Mist("biome.ancient-abyss.mist.ocean-b", -38f, -47f, 58f, 92f, 10f, 84f, 0.2f),
            Mist("biome.ancient-abyss.mist.ocean-c", 34f, -50f, 98f, 90f, 11f, 88f, 0.18f),
            Mist("biome.ancient-abyss.mist.deep", 0f, -62f, 76f, 132f, 16f, 112f, 0.16f)
        };

        private static BiomeMistLayer[] CreateEnergyMist() => new[]
        {
            Mist("biome.energy-void.mist.ocean-a", 0f, -48f, 26f, 98f, 7f, 72f, 0.2f),
            Mist("biome.energy-void.mist.ocean-b", 38f, -54f, 78f, 94f, 9f, 82f, 0.18f),
            Mist("biome.energy-void.mist.deep", -28f, -66f, 96f, 130f, 14f, 110f, 0.15f)
        };

        private static BiomeWorldObject[] CreateSkyCityObjects(Dictionary<string, GameObject> prefabs) => new[]
        {
            Obj("biome.sky-city.cluster.hero-tower", prefabs["City_Skyscraper_A"], -44f, -48f, 48f, 0f, 18f, 0f, 14f, 64f, 14f),
            Obj("biome.sky-city.cluster.twin-spire", prefabs["City_Skyscraper_C"], -28f, -50f, 72f, 0f, -10f, 0f, 12f, 58f, 12f),
            Obj("biome.sky-city.cluster.deep-rise", prefabs["City_Skyscraper_E"], 36f, -54f, 92f, 0f, 26f, 0f, 16f, 72f, 16f),
            Obj("biome.sky-city.cluster.mid-block", prefabs["City_Building_K"], 24f, -43f, 38f, 0f, -20f, 0f, 18f, 32f, 18f),
            Obj("biome.sky-city.cluster.wide-roof", prefabs["City_LowWide_A"], -4f, -46f, 105f, 0f, 12f, 0f, 22f, 24f, 22f),
            Obj("biome.sky-city.energy-tower", prefabs["Industrial_Chimney_Large"], 50f, -42f, 62f, 0f, 8f, 0f, 10f, 54f, 10f),
            Obj("biome.sky-city.tech-silo", prefabs["Industrial_Tank"], -54f, -40f, 88f, 0f, -18f, 0f, 14f, 22f, 14f)
        };

        private static BiomeWorldObject[] CreateMountainObjects(Dictionary<string, GameObject> prefabs) => new[]
        {
            Obj("biome.mountain-sky.primary-peak", prefabs["Nature_Cliff_Large_Rock"], -38f, -44f, 44f, 0f, 24f, 0f, 28f, 46f, 28f),
            Obj("biome.mountain-sky.secondary-ridge", prefabs["Nature_Cliff_CornerLarge_Rock"], 34f, -48f, 68f, 0f, -35f, 0f, 26f, 38f, 26f),
            Obj("biome.mountain-sky.slope-peak", prefabs["Nature_Cliff_Slope_Rock"], -8f, -50f, 98f, 0f, 12f, 0f, 32f, 36f, 32f),
            Obj("biome.mountain-sky.needle-a", prefabs["Nature_Rock_Tall_A"], 48f, -38f, 28f, 0f, 8f, 0f, 18f, 34f, 18f),
            Obj("biome.mountain-sky.needle-b", prefabs["Nature_Rock_Tall_F"], -54f, -42f, 82f, 0f, -16f, 0f, 16f, 30f, 16f),
            Obj("biome.mountain-sky.pine-silhouette", prefabs["Nature_Pine_Tall_A"], 22f, -26f, 72f, 0f, 24f, 0f, 10f, 18f, 10f),
            Obj("biome.mountain-sky.bush-island", prefabs["Nature_Bush_Large"], -24f, -26f, 58f, 0f, -12f, 0f, 12f, 10f, 12f)
        };

        private static BiomeWorldObject[] CreateAncientObjects(Dictionary<string, GameObject> prefabs) => new[]
        {
            Obj("biome.ancient-abyss.primary-broken-tower", prefabs["Castle_Tower_Square"], -36f, -45f, 40f, 0f, 18f, 0f, 16f, 48f, 16f),
            Obj("biome.ancient-abyss.open-mid-tower", prefabs["Castle_Tower_Mid_Open"], 32f, -38f, 62f, 0f, -22f, 0f, 18f, 34f, 18f),
            Obj("biome.ancient-abyss.high-roof-ruin", prefabs["Castle_Tower_Roof_High"], -18f, -32f, 86f, 0f, 12f, 0f, 14f, 30f, 14f),
            Obj("biome.ancient-abyss.hex-base", prefabs["Castle_Hex_Base"], 46f, -52f, 98f, 0f, 34f, 0f, 18f, 32f, 18f),
            Obj("biome.ancient-abyss.fallen-wall", prefabs["Castle_Wall"], -52f, -28f, 68f, 0f, 35f, 0f, 24f, 16f, 12f),
            Obj("biome.ancient-abyss.corner-ruin", prefabs["Castle_Wall_Corner"], 14f, -34f, 28f, 0f, -16f, 0f, 18f, 22f, 18f),
            Obj("biome.ancient-abyss.gate-silhouette", prefabs["Castle_Gate"], 0f, -30f, 112f, 0f, 0f, 0f, 22f, 26f, 22f)
        };

        private static BiomeWorldObject[] CreateEnergyObjects(Dictionary<string, GameObject> prefabs) => new[]
        {
            Obj("biome.energy-void.cyan-stack-a", prefabs["Industrial_Building_Q"], -38f, -50f, 44f, 0f, 12f, 0f, 20f, 44f, 20f),
            Obj("biome.energy-void.cyan-stack-b", prefabs["Industrial_Building_T"], 42f, -54f, 82f, 0f, -22f, 0f, 18f, 38f, 18f),
            Obj("biome.energy-void.energy-chimney", prefabs["Industrial_Chimney_Large"], 8f, -48f, 112f, 0f, 8f, 0f, 12f, 52f, 12f)
        };

        private static BiomeMistLayer Mist(
            string id,
            float x,
            float y,
            float z,
            float sx,
            float sy,
            float sz,
            float alpha) =>
            new BiomeMistLayer(id, new Vector3(x, y, z), new Vector3(sx, sy, sz), alpha);

        private static BiomeWorldObject Obj(
            string id,
            GameObject prefab,
            float x,
            float y,
            float z,
            float rx,
            float ry,
            float rz,
            float sx,
            float sy,
            float sz) =>
            new BiomeWorldObject(
                id,
                prefab,
                new Vector3(x, y, z),
                new Vector3(rx, ry, rz),
                new Vector3(sx, sy, sz));

        private static readonly PackSpec CityCommercial = new PackSpec(
            "kenney_city-kit-commercial_2.1.zip",
            "kenney_city-kit-commercial_2.1",
            "CityKitCommercial",
            "https://kenney.nl/media/pages/assets/city-kit-commercial/a742d900eb-1753115042/kenney_city-kit-commercial_2.1.zip");
        private static readonly PackSpec CityIndustrial = new PackSpec(
            "kenney_city-kit-industrial_1.0.zip",
            "kenney_city-kit-industrial_1.0",
            "CityKitIndustrial",
            "https://kenney.nl/media/pages/assets/city-kit-industrial/5fcb837741-1750838303/kenney_city-kit-industrial_1.0.zip");
        private static readonly PackSpec NatureKit = new PackSpec(
            "kenney_nature-kit.zip",
            "kenney_nature-kit",
            "NatureKit",
            "https://kenney.nl/media/pages/assets/nature-kit/37ac38a37b-1677698939/kenney_nature-kit.zip");
        private static readonly PackSpec CastleKit = new PackSpec(
            "kenney_castle-kit.zip",
            "kenney_castle-kit",
            "CastleKit",
            "https://kenney.nl/media/pages/assets/castle-kit/a395102d20-1711543616/kenney_castle-kit.zip");

        private static readonly PackSpec[] Packs =
        {
            CityCommercial,
            CityIndustrial,
            NatureKit,
            CastleKit
        };

        private static readonly WorldAssetSpec[] Assets =
        {
            Asset("City_Skyscraper_A", CityCommercial, "building-skyscraper-a.fbx", "SkyCity", "city"),
            Asset("City_Skyscraper_B", CityCommercial, "building-skyscraper-b.fbx", "SkyCity", "city"),
            Asset("City_Skyscraper_C", CityCommercial, "building-skyscraper-c.fbx", "SkyCity", "city"),
            Asset("City_Skyscraper_D", CityCommercial, "building-skyscraper-d.fbx", "SkyCity", "city"),
            Asset("City_Skyscraper_E", CityCommercial, "building-skyscraper-e.fbx", "SkyCity", "city"),
            Asset("City_Building_K", CityCommercial, "building-k.fbx", "SkyCity", "city"),
            Asset("City_Building_N", CityCommercial, "building-n.fbx", "SkyCity", "city"),
            Asset("City_LowWide_A", CityCommercial, "low-detail-building-wide-a.fbx", "SkyCity", "city"),
            Asset("Industrial_Building_Q", CityIndustrial, "building-q.fbx", "SkyCity", "tech"),
            Asset("Industrial_Building_T", CityIndustrial, "building-t.fbx", "SkyCity", "tech"),
            Asset("Industrial_Chimney_Large", CityIndustrial, "chimney-large.fbx", "SkyCity", "tech"),
            Asset("Industrial_Tank", CityIndustrial, "detail-tank.fbx", "SkyCity", "tech"),
            Asset("Nature_Cliff_Large_Rock", NatureKit, "cliff_large_rock.fbx", "MountainSky", "rock"),
            Asset("Nature_Cliff_CornerLarge_Rock", NatureKit, "cliff_cornerLarge_rock.fbx", "MountainSky", "rock"),
            Asset("Nature_Cliff_Slope_Rock", NatureKit, "cliff_blockSlope_rock.fbx", "MountainSky", "rock"),
            Asset("Nature_Rock_Tall_A", NatureKit, "rock_tallA.fbx", "MountainSky", "rock"),
            Asset("Nature_Rock_Tall_F", NatureKit, "rock_tallF.fbx", "MountainSky", "rock"),
            Asset("Nature_Rock_Large_A", NatureKit, "rock_largeA.fbx", "MountainSky", "rock"),
            Asset("Nature_Pine_Tall_A", NatureKit, "tree_pineTallA.fbx", "MountainSky", "green"),
            Asset("Nature_Bush_Large", NatureKit, "plant_bushLarge.fbx", "MountainSky", "green"),
            Asset("Castle_Tower_Square", CastleKit, "tower-square.fbx", "AncientAbyss", "ancient"),
            Asset("Castle_Tower_Base", CastleKit, "tower-square-base.fbx", "AncientAbyss", "ancient"),
            Asset("Castle_Tower_Mid_Open", CastleKit, "tower-square-mid-open.fbx", "AncientAbyss", "ancient"),
            Asset("Castle_Tower_Roof_High", CastleKit, "tower-square-top-roof-high.fbx", "AncientAbyss", "ancient"),
            Asset("Castle_Hex_Base", CastleKit, "tower-hexagon-base.fbx", "AncientAbyss", "ancient"),
            Asset("Castle_Hex_Mid", CastleKit, "tower-hexagon-mid.fbx", "AncientAbyss", "ancient"),
            Asset("Castle_Wall", CastleKit, "wall.fbx", "AncientAbyss", "ancient"),
            Asset("Castle_Wall_Corner", CastleKit, "wall-corner.fbx", "AncientAbyss", "ancient"),
            Asset("Castle_Gate", CastleKit, "gate.fbx", "AncientAbyss", "dark")
        };

        private static WorldAssetSpec Asset(
            string key,
            PackSpec pack,
            string originalFileName,
            string category,
            string materialKey) =>
            new WorldAssetSpec(key, pack, originalFileName, $"RR_{key}.fbx", category, materialKey);

        private readonly struct PackSpec
        {
            public PackSpec(string zipName, string extractDirectory, string assetDirectory, string url)
            {
                ZipName = zipName;
                ExtractDirectory = extractDirectory;
                AssetDirectory = assetDirectory;
                Url = url;
            }

            public string ZipName { get; }
            public string ExtractDirectory { get; }
            public string AssetDirectory { get; }
            public string Url { get; }
        }

        private readonly struct WorldAssetSpec
        {
            public WorldAssetSpec(
                string key,
                PackSpec pack,
                string originalFileName,
                string productionFileName,
                string category,
                string materialKey)
            {
                Key = key;
                Pack = pack;
                OriginalFileName = originalFileName;
                ProductionFileName = productionFileName;
                Category = category;
                MaterialKey = materialKey;
            }

            public string Key { get; }
            public PackSpec Pack { get; }
            public string OriginalFileName { get; }
            public string ProductionFileName { get; }
            public string Category { get; }
            public string MaterialKey { get; }
        }
    }
}
