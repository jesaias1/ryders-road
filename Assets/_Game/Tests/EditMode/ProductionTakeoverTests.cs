using Avoidance.Gameplay.Audio;
using Avoidance.Gameplay.Blocks;
using Avoidance.Gameplay.Levels;
using Avoidance.Gameplay.Worlds;
using NUnit.Framework;
using UnityEngine;
using System.Linq;

namespace Avoidance.Tests.EditMode
{
    public sealed class ProductionTakeoverTests
    {
        [TestCase("module.001.first-steps", "module.002.moving-parts")]
        [TestCase("module.002.moving-parts", "module.003.flow-error")]
        [TestCase("module.003.flow-error", "module.004.solar-foundry")]
        [TestCase("module.004.solar-foundry", "module.005.foundry-pulse")]
        [TestCase("module.005.foundry-pulse", null)]
        [TestCase("module.004.the-spiral", null)]
        [TestCase("module.lab.art-scale-collision", null)]
        public void NextCampaignNeverIncludesSpiralOrLabs(string current, string expected)
        {
            Assert.That(ModuleSelectionState.GetNextCampaignModuleId(current), Is.EqualTo(expected));
        }

        [Test]
        public void ResultsPrioritizeMasteryWithoutEconomyClutter()
        {
            var result = new Avoidance.Gameplay.Timing.ModuleRunResult
            {
                CompletionSeconds = 200, PersonalBestSeconds = 200, IsNewPersonalBest = true,
                Rank = Avoidance.Gameplay.Ranking.ModuleRank.Bronze,
                NextRank = Avoidance.Gameplay.Ranking.ModuleRank.Silver,
                SecondsFromNextRank = 65, Score = 12345, RewardLines = new[] { "shards" }
            };
            var text = Avoidance.UI.ModuleHud.FormatResults(result);
            Assert.That(text, Does.Contain("BRONZE").And.Contain("NEW PB").And.Contain("SILVER"));
            Assert.That(text, Does.Not.Contain("SCORE").And.Not.Contain("shards"));
            result.Validity = Avoidance.Gameplay.Ranking.RunValidity.InvalidDevelopment;
            Assert.That(Avoidance.UI.ModuleHud.FormatResults(result), Does.Contain("PRACTICE COMPLETE")
                .And.Contain("NOT RECORDED").And.Not.Contain("NEW PB"));
        }

        [TestCase(ModuleEasing.Linear)]
        [TestCase(ModuleEasing.SmoothStep)]
        public void FerryDwellsAtBothEndpointsAndResumesWithoutTeleporting(ModuleEasing easing)
        {
            var points = new[] { Vector3.zero, Vector3.right * 10f };
            Vector3 At(float time) => MovingBlockMotionMath.Evaluate(points,
                ModuleMovingLoopMode.PingPong, easing, 5f, time, 0.3f);
            Assert.That(At(2.01f).x, Is.EqualTo(10f).Within(0.001f));
            Assert.That(At(2.29f).x, Is.EqualTo(10f).Within(0.001f));
            Assert.That(At(2.32f).x, Is.LessThan(10f).And.GreaterThan(9.8f));
            Assert.That(At(4.31f).x, Is.EqualTo(0f).Within(0.001f));
            Assert.That(At(4.59f).x, Is.EqualTo(0f).Within(0.001f));
            Assert.That(At(4.62f).x, Is.GreaterThan(0f).And.LessThan(0.2f));
        }

        [Test]
        public void FerryHandlesRepeatedPointsAndZeroPause()
        {
            var points = new[] { Vector3.zero, Vector3.zero, Vector3.forward * 10f };
            Assert.That(MovingBlockMotionMath.Evaluate(points, ModuleMovingLoopMode.PingPong,
                ModuleEasing.Linear, 5f, 3f).z, Is.EqualTo(5f).Within(0.001f));
        }

        [Test]
        public void MissingAudioIsSilentButStillPublishesFeedbackIntent()
        {
            var root = new GameObject("Audio contract test");
            var profile = ScriptableObject.CreateInstance<GameplayAudioProfile>();
            try
            {
                var feedback = root.AddComponent<MovementFeedback>();
                feedback.Configure(profile);
                GameplayAudioCue? received = null;
                feedback.CueRequested += cue => received = cue;
                feedback.PlayBoost();
                Assert.That(received, Is.EqualTo(GameplayAudioCue.Boost));
                Assert.That(profile.TryGet(GameplayAudioCue.Boost, out var clip, out var volume), Is.False);
                Assert.That(clip, Is.Null);
                Assert.That(volume, Is.Zero);
                Assert.That(root.GetComponent<AudioSource>().isPlaying, Is.False);
                feedback.PlayLanding(20f);
                Assert.That(received, Is.EqualTo(GameplayAudioCue.HardLanding));
            }
            finally { Object.DestroyImmediate(root); Object.DestroyImmediate(profile); }
        }

        [Test]
        public void AncientAbyssDecorativeFoundationsStayWellBelowThePlayableLane()
        {
            var module = Resources.LoadAll<ModuleDefinition>("Modules")
                .Single(item => item.StableModuleId == "module.003.flow-error");
            var places = module.EnvironmentBiomeProfile.WorldObjects
                .Where(item => item.DepthBand == BiomeDepthBand.NearEnvironment
                    && !item.StableId.StartsWith("biome.flow-foundations.")).ToArray();
            Assert.That(places.Length, Is.EqualTo(5));
            foreach (var place in places)
            {
                var instance = Object.Instantiate(place.Prefab);
                try
                {
                    var foundations = instance.GetComponentsInChildren<Transform>()
                        .Where(item => item.name.EndsWith("_Foundation")).ToArray();
                    Assert.That(foundations, Is.Not.Empty, place.StableId);
                    foreach (var foundation in foundations)
                    {
                        foreach (var renderer in foundation.GetComponentsInChildren<Renderer>())
                        {
                            var nearest=module.Blocks.OrderBy(b=>Mathf.Abs(b.Pose.Position.z-renderer.bounds.center.z)).First();
                            Assert.That(renderer.bounds.max.y, Is.LessThan(nearest.Pose.Position.y-3f), foundation.name+" beneath local route height");
                        }
                        Assert.That(foundation.GetComponentsInChildren<Avoidance.Gameplay.Worlds.AuthoredSurface>().All(surface => surface.HasMatchingCollision), Is.True);
                    }
                }
                finally { Object.DestroyImmediate(instance); }
            }
        }
    }
}
