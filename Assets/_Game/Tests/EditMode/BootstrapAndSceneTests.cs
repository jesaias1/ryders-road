using Avoidance.Bootstrap;
using Avoidance.Core.Configuration;
using Avoidance.Core.FeatureFlags;
using Avoidance.Core.Services;
using Avoidance.EditorTools;
using Avoidance.SaveSystem;
using NUnit.Framework;

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
            Assert.That(services.Get<ILevelLoader>(), Is.Not.Null);
        }

        [Test]
        public void SceneConfiguration_IsValid()
        {
            Assert.That(
                FoundationProjectValidator.Validate(),
                Is.Empty,
                "Required Phase 0 scene configuration is invalid.");
        }
    }
}
