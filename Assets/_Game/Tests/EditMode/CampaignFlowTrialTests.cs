using Avoidance.Gameplay.Levels;
using NUnit.Framework;
using UnityEngine;

namespace Avoidance.Tests.EditMode
{
    public sealed class CampaignFlowTrialTests
    {
        [TearDown] public void Clear() { CampaignFlowTrial.Clear(); }
        [Test] public void SessionTimesSeparateRoadsModesAndNormalSelection()
        {
            var foundry=Resources.Load<ModuleDefinition>("Modules/Module_004_SolarFoundry");
            var first=Resources.Load<ModuleDefinition>("Modules/Module_001_FirstSteps");
            CampaignFlowTrial.Launch(foundry.StableModuleId,CampaignTrialMode.FlowLanding);
            CampaignFlowTrial.Complete(foundry,74);
            CampaignFlowTrial.Complete(foundry,80);
            Assert.That(CampaignFlowTrial.Best(foundry),Is.EqualTo(74));
            Assert.That(CampaignFlowTrial.Best(first),Is.Zero);
            CampaignFlowTrial.Launch(foundry.StableModuleId,CampaignTrialMode.Accepted);
            Assert.That(CampaignFlowTrial.Best(foundry),Is.Zero);
            CampaignFlowTrial.Complete(foundry,100);
            CampaignFlowTrial.Launch(foundry.StableModuleId,CampaignTrialMode.FlowLanding);
            Assert.That(CampaignFlowTrial.Best(foundry),Is.EqualTo(74));
            CampaignFlowTrial.Launch(foundry.StableModuleId,CampaignTrialMode.Foundation);
            Assert.That(CampaignFlowTrial.Best(foundry),Is.Zero);
            var profile=Resources.Load<Avoidance.Gameplay.Player.MovementProfile>(CampaignFlowTrial.MovementResource);
            Assert.That(profile.MovementFoundation && profile.HoldToHop,Is.True);
            Assert.That(profile.CompatibilityVersion,Is.EqualTo(4));
            CampaignFlowTrial.Complete(foundry,42);
            CampaignFlowTrial.Launch(foundry.StableModuleId,CampaignTrialMode.FlowLanding);
            Assert.That(CampaignFlowTrial.Best(foundry),Is.EqualTo(74));
            Assert.That(Resources.Load<Avoidance.Gameplay.Player.MovementProfile>("MovementProfiles/Movement_Default").HoldToHop,Is.False);
            ModuleSelectionState.Select(first.StableModuleId);
            Assert.That(CampaignFlowTrial.Active,Is.False);
            Assert.That(ModuleSelectionState.DevelopmentOverride,Is.False);
            Assert.Throws<System.ArgumentException>(()=>CampaignFlowTrial.Launch(ModuleSelectionState.SpiralModuleId,CampaignTrialMode.FlowLanding));
        }
    }
}
