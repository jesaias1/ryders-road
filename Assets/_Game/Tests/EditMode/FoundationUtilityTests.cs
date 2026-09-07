using System.Collections.Generic;
using System.IO;
using Avoidance.Core.FeatureFlags;
using Avoidance.Core.Utilities;
using Avoidance.EditorTools;
using Avoidance.Gameplay.Camera;
using Avoidance.Input;
using Avoidance.UI;
using Avoidance.UI.Touch;
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
        public void AndroidDisplayService_RecordsImmersiveRequestAndSafeArea()
        {
            var display = new AndroidDisplayService();

            display.RequestImmersiveMode();
            display.SetFocusState(false);

            Assert.That(display.ImmersiveRequested, Is.True);
            Assert.That(display.HasFocus, Is.False);
            Assert.That(display.SafeArea.width, Is.GreaterThanOrEqualTo(0f));
            Assert.That(display.SafeArea.height, Is.GreaterThanOrEqualTo(0f));
            Assert.That(display.DisplaySummary, Does.Contain("safe"));
        }

        [Test]
        public void BrandPresentation_UsesRydersRoadPublicIdentity()
        {
            Assert.That(BrandPresentation.PlayerFacingTitle, Is.EqualTo("RYDER'S ROAD"));
            Assert.That(BrandPresentation.ProductName, Is.EqualTo("Ryder's Road"));
            Assert.That(BrandPresentation.LegacyCodename, Is.EqualTo("RYDERS BLOCK"));
            Assert.That(BrandPresentation.AndroidPackageIdentifier, Is.EqualTo("com.rydersblockstudio.rydersblock"));
        }

        [Test]
        public void BrandPresentation_LoadsLogoTextureFromResources()
        {
            var logo = Resources.Load<Texture2D>(BrandPresentation.LogoResourcePath);

            Assert.That(logo, Is.Not.Null);
            Assert.That(logo.width, Is.GreaterThan(logo.height));
        }

        [Test]
        public void CameraFieldOfViewPreference_ClampsToMobileAlphaRange()
        {
            PlayerPrefs.SetFloat(FirstPersonCameraRig.FieldOfViewPreferenceKey, 160f);

            var resolved = FirstPersonCameraRig.ResolvePreferredFieldOfView(null);

            Assert.That(resolved, Is.EqualTo(FirstPersonCameraRig.MaximumPreferredFieldOfView));
            PlayerPrefs.DeleteKey(FirstPersonCameraRig.FieldOfViewPreferenceKey);
        }

        [Test]
        public void CameraFieldOfViewPreference_DefaultsToWideMobileParkourView()
        {
            PlayerPrefs.DeleteKey(FirstPersonCameraRig.FieldOfViewPreferenceKey);

            var resolved = FirstPersonCameraRig.ResolvePreferredFieldOfView(null);

            Assert.That(resolved, Is.EqualTo(94f));
            Assert.That(FirstPersonCameraRig.DefaultPreferredFieldOfView, Is.EqualTo(94f));
            Assert.That(FirstPersonCameraRig.MaximumPreferredFieldOfView, Is.EqualTo(94f));
        }

        [Test]
        public void TouchControlPreferences_PersistControlAndMovementMode()
        {
            PlayerPrefs.DeleteKey(TouchInputCoordinator.ControlProfilePreferenceKey);
            PlayerPrefs.DeleteKey(TouchInputCoordinator.MovementSensitivityPreferenceKey);

            TouchInputCoordinator.StorePreferredControlProfile(
                TouchControlProfileKind.LeftMoveRightLookTapJump);
            TouchInputCoordinator.StorePreferredMovementSensitivity(
                TouchMovementSensitivityPreset.Fast);

            Assert.That(
                TouchInputCoordinator.LoadPreferredControlProfile(
                    TouchControlProfileKind.FlowSteerAutoBalanced),
                Is.EqualTo(TouchControlProfileKind.LeftMoveRightLookTapJump));
            Assert.That(
                TouchInputCoordinator.LoadPreferredMovementSensitivity(
                    TouchMovementSensitivityPreset.Medium),
                Is.EqualTo(TouchMovementSensitivityPreset.Fast));
            PlayerPrefs.DeleteKey(TouchInputCoordinator.ControlProfilePreferenceKey);
            PlayerPrefs.DeleteKey(TouchInputCoordinator.MovementSensitivityPreferenceKey);
        }

        [Test]
        public void AndroidDevelopmentBuilder_ReportsOutputFileSize()
        {
            var path = Path.GetTempFileName();
            try
            {
                File.WriteAllBytes(path, new byte[] { 1, 2, 3, 4 });

                Assert.That(AndroidDevelopmentBuilder.ResolveOutputFileSize(path), Is.EqualTo(4));
                Assert.That(AndroidDevelopmentBuilder.ResolveOutputFileSize(path + ".missing"), Is.EqualTo(0));
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Test]
        public void AndroidDevelopmentBuilder_RemovesStaleApkBeforeBuild()
        {
            var directory = Path.Combine(Path.GetTempPath(), "RydersRoad-AndroidBuilder-Test");
            var path = Path.Combine(directory, "stale.apk");
            try
            {
                Directory.CreateDirectory(directory);
                File.WriteAllBytes(path, new byte[] { 1, 2, 3, 4 });

                AndroidDevelopmentBuilder.PrepareOutputFile(path);

                Assert.That(File.Exists(path), Is.False);
                Assert.That(Directory.Exists(directory), Is.True);
            }
            finally
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory);
                }
            }
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
