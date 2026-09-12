using System.Linq;
using Avoidance.Gameplay.Audio;
using Avoidance.Gameplay.Levels;
using Avoidance.Gameplay.Visuals;
using Avoidance.Gameplay.Worlds;
using NUnit.Framework;
using UnityEngine;

namespace Avoidance.Tests.EditMode
{
    public sealed class Production091Tests
    {
        [Test]
        public void EveryNearArchitecturalMeshHasOptedInMatchingCollision()
        {
            var module = Resources.LoadAll<ModuleDefinition>("Modules").Single(x => x.StableModuleId == "module.003.flow-error");
            foreach (var place in module.EnvironmentBiomeProfile.WorldObjects)
            {
                var near = place.DepthBand == BiomeDepthBand.NearEnvironment;
                Assert.That(place.HasPlayableArchitecture, Is.EqualTo(near), place.StableId);
                if (!near) { Assert.That(place.Prefab.GetComponentsInChildren<Collider>(), Is.Empty); continue; }
                var instance = Object.Instantiate(place.Prefab);
                try
                {
                    ModuleVisualPrefabLibrary.PrepareVisualInstance(instance, true);
                    var surfaces = instance.GetComponentsInChildren<AuthoredSurface>();
                    Assert.That(surfaces.Length, Is.GreaterThanOrEqualTo(1), place.StableId);
                    Assert.That(surfaces.All(surface => surface.HasMatchingCollision), Is.True, place.StableId);
                    foreach (var filter in instance.GetComponentsInChildren<MeshFilter>())
                        if (UnityEditor.AssetDatabase.GetAssetPath(filter.sharedMesh).Contains("/MeshySource/"))
                            Assert.That(filter.GetComponent<AuthoredSurface>(), Is.Not.Null, filter.name);
                }
                finally { Object.DestroyImmediate(instance); }
            }
        }
        [Test]
        public void HapticsAreRestrainedAndDoNotFireForContinuousMovement()
        {
            var profile = Resources.Load<GameFeelProfile>("GameFeelProfile");
            Assert.That(profile, Is.Not.Null);
            Assert.That(AndroidGameplayHaptics.PulseDuration(GameplayAudioCue.Wind, profile), Is.Zero);
            Assert.That(AndroidGameplayHaptics.PulseDuration(GameplayAudioCue.Jump, profile), Is.Zero);
            Assert.That(AndroidGameplayHaptics.PulseDuration(GameplayAudioCue.Landing, profile), Is.Zero);
            Assert.That(AndroidGameplayHaptics.PulseDuration(GameplayAudioCue.Pickup, profile), Is.InRange(1, 20));
            Assert.That(AndroidGameplayHaptics.PulseDuration(GameplayAudioCue.Patch, profile), Is.InRange(25, 60));
            Assert.That(profile.HapticCooldown, Is.GreaterThanOrEqualTo(0.15f));
        }
        [Test]
        public void FlowChallengeUsesUniqueAuthoredIdsAndCountsEachOnlyOnce()
        {
            var profile = Resources.LoadAll<FlowChallengeProfile>("FlowChallenges").Single(p => p.ModuleId == "module.003.flow-error");
            Assert.That(profile.ModuleId, Is.EqualTo("module.003.flow-error"));
            Assert.That(profile.Shards.Select(x => x.StableId).Distinct().Count(), Is.EqualTo(5));
            var root = new GameObject("Flow test");
            try
            {
                var challenge = root.AddComponent<FlowChallenge>();
                Assert.That(challenge.TryCollect(profile.Shards[0].StableId, Vector3.zero), Is.True);
                Assert.That(challenge.TryCollect(profile.Shards[0].StableId, Vector3.zero), Is.False);
                Assert.That(challenge.Count, Is.EqualTo(1));
            }
            finally { Object.DestroyImmediate(root); }
        }
    }
}
