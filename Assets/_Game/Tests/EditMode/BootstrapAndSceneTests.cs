using Avoidance.Bootstrap;
using Avoidance.Core.Configuration;
using Avoidance.Core.FeatureFlags;
using Avoidance.Core.Services;
using Avoidance.EditorTools;
using Avoidance.Gameplay.Visuals;
using Avoidance.SaveSystem;
using NUnit.Framework;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Avoidance.Tests.EditMode
{
    public sealed class BootstrapAndSceneTests
    {
        [Test]
        public void CompositionRoot_RegistersRequiredFoundationServices()
        {
            var configuration = GameConfiguration.CreateRuntimeDefault();
            var services = GameBootstrap.ComposeServices(configuration);

            Assert.That(services.Get<ISettingsService>(), Is.Not.Null);
            Assert.That(services.Get<ISaveService>(), Is.Not.Null);
            Assert.That(services.Get<IFeatureFlagService>(), Is.Not.Null);
            Assert.That(services.Get<IDiagnosticsService>(), Is.Not.Null);
            Assert.That(services.Get<IPlatformDisplayService>(), Is.Not.Null);
            Assert.That(services.Get<ILevelLoader>(), Is.Not.Null);
            Assert.That(services.Get<ICheckpointService>(), Is.Not.Null);
        }

        [Test]
        public void SceneConfiguration_IsValid()
        {
            Assert.That(
                FoundationProjectValidator.Validate(),
                Is.Empty,
                "Required Phase 1 scene configuration is invalid.");
        }

        [Test]
        public void GameplayMaterialShader_IsValidForUrp()
        {
            Assert.That(
                VisualMaterialUtility.IsInvalidGameplayShader(
                    VisualMaterialUtility.ResolveOpaqueShader()),
                Is.False);
            Assert.That(
                VisualMaterialUtility.IsInvalidGameplayShader(
                    VisualMaterialUtility.ResolveParticleShader()),
                Is.False);
        }

        [Test]
        public void AndroidOrientation_AllowsBothLandscapeDirectionsOnly()
        {
            Assert.That(PlayerSettings.defaultInterfaceOrientation, Is.EqualTo(UIOrientation.AutoRotation));
            Assert.That(PlayerSettings.allowedAutorotateToPortrait, Is.False);
            Assert.That(PlayerSettings.allowedAutorotateToPortraitUpsideDown, Is.False);
            Assert.That(PlayerSettings.allowedAutorotateToLandscapeLeft, Is.True);
            Assert.That(PlayerSettings.allowedAutorotateToLandscapeRight, Is.True);
            Assert.That(
                File.ReadAllText("ProjectSettings/ProjectSettings.asset"),
                Does.Contain("useOSAutorotation: 1"));
        }
    }
}
