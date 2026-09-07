using Avoidance.Gameplay.Audio;
using Avoidance.Gameplay.Levels;
using UnityEditor;
using UnityEngine;
using System.IO;

namespace Avoidance.EditorTools
{
    public static class Production091Content
    {
        public static void BuildContent()
        {
            Phase074WorldSystemRecovery.RecoverWorldSystemsAndModule003();
            const string feelPath = "Assets/_Game/Audio/Resources/GameFeelProfile.asset";
            if (AssetDatabase.LoadAssetAtPath<GameFeelProfile>(feelPath) == null)
                AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<GameFeelProfile>(), feelPath);
            const string folder = "Assets/_Game/Levels/Resources/FlowChallenges";
            Directory.CreateDirectory(folder); AssetDatabase.Refresh();
            var challenge = AssetDatabase.LoadAssetAtPath<FlowChallengeProfile>(folder + "/FlowChallenge_Module003.asset");
            if (challenge == null) { challenge = ScriptableObject.CreateInstance<FlowChallengeProfile>(); AssetDatabase.CreateAsset(challenge, folder + "/FlowChallenge_Module003.asset"); }
            challenge.ModuleId = "module.003.flow-error";
            challenge.Shards = new[]
            {
                Shard("flow.m03.arrival-cut", 0, 1.95f, 5),
                Shard("flow.m03.crossing-near", 2.2f, 3.6f, 30.7f),
                Shard("flow.m03.crossing-far", -3.6f, 3.75f, 33.4f),
                Shard("flow.m03.temple-ledge", -2, 5.7f, 57),
                Shard("flow.m03.temple-exit", 1, 6, 63)
            };
            EditorUtility.SetDirty(challenge);
            var poster = AssetImporter.GetAtPath("Assets/_Game/UI/Resources/Loading/RydersRoad_LoadingPoster.jpg") as TextureImporter;
            if (poster != null) { poster.mipmapEnabled = false; poster.maxTextureSize = 2048; poster.textureCompression = TextureImporterCompression.CompressedHQ; poster.SaveAndReimport(); }
            PlayerSettings.SplashScreen.show = false;
            AssetDatabase.SaveAssets();
            ProductionTruthAudit.Run();
        }
        private static FlowChallengeProfile.Shard Shard(string id, float x, float y, float z) =>
            new FlowChallengeProfile.Shard { StableId = id, Position = new Vector3(x, y, z) };
    }
}
