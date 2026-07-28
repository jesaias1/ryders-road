using System.Collections.Generic;
using Avoidance.Core.FeatureFlags;
using Avoidance.Core.Utilities;
using Avoidance.UI;
using NUnit.Framework;
using UnityEngine;

namespace Avoidance.Tests.EditMode
{
    public sealed class FoundationUtilityTests
    {
        [TestCase("level.world-01.level-01")]
        [TestCase("powerup.jump-boost")]
        [TestCase("cosmetic.gloves-neon")]
        public void StableId_AcceptsCanonicalIds(string id)
        {
            Assert.That(StableId.IsValid(id), Is.True);
            Assert.That(new StableId(id).Value, Is.EqualTo(id));
        }

        [TestCase("")]
        [TestCase("Level 1")]
        [TestCase("level/one")]
        [TestCase("01-level")]
        [TestCase("UPPERCASE")]
        public void StableId_RejectsUnstableOrUnsafeIds(string id)
        {
            Assert.That(StableId.IsValid(id), Is.False);
        }

        [Test]
        public void SafeAreaHelper_ConvertsPixelsToNormalizedAnchors()
        {
            SafeAreaLayoutHelper.CalculateAnchors(
                new Rect(100f, 50f, 1800f, 980f),
                new Vector2(2000f, 1080f),
                out var minimum,
                out var maximum);

            Assert.That(minimum.x, Is.EqualTo(0.05f).Within(0.0001f));
            Assert.That(minimum.y, Is.EqualTo(50f / 1080f).Within(0.0001f));
            Assert.That(maximum.x, Is.EqualTo(0.95f).Within(0.0001f));
            Assert.That(maximum.y, Is.EqualTo(1030f / 1080f).Within(0.0001f));
        }

        [Test]
        public void FeatureFlags_PersistThroughStore()
        {
            var definition = new FeatureFlagDefinition
            {
                _id = "experimental-ui",
                _displayName = "Experimental UI"
            };
            var store = new MemoryFeatureFlagStore();
            var first = new FeatureFlagService(new[] { definition }, store, true);
            Assert.That(first.TrySetEnabled(definition.Id, true), Is.True);

            var second = new FeatureFlagService(new[] { definition }, store, true);
            Assert.That(second.IsEnabled(definition.Id), Is.True);
        }

        [Test]
        public void FeatureFlags_DevelopmentOnlyFlagIsOffInRelease()
        {
            var definition = new FeatureFlagDefinition
            {
                _id = "experimental-ui",
                _displayName = "Experimental UI",
                _allowedInRelease = false
            };
            var store = new MemoryFeatureFlagStore();
            store.Write(definition.Id, true);
            var releaseService = new FeatureFlagService(new[] { definition }, store, false);

            Assert.That(releaseService.IsEnabled(definition.Id), Is.False);
            Assert.That(releaseService.TrySetEnabled(definition.Id, true), Is.False);
        }

        private sealed class MemoryFeatureFlagStore : IFeatureFlagStore
        {
            private readonly Dictionary<string, bool> _values = new Dictionary<string, bool>();

            public bool TryRead(string id, out bool enabled) => _values.TryGetValue(id, out enabled);
            public void Write(string id, bool enabled) => _values[id] = enabled;
        }
    }
}
