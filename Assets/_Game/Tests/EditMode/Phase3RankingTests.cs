using System;
using Avoidance.Gameplay.Levels;
using Avoidance.Gameplay.Ranking;
using Avoidance.Gameplay.Timing;
using Avoidance.Gameplay.Visuals;
using Avoidance.SaveSystem;
using Avoidance.UI;
using NUnit.Framework;
using UnityEngine;

namespace Avoidance.Tests.EditMode
{
    public sealed class Phase3RankingTests
    {
        [Test]
        public void RunTimer_RestoreDoesNotResetButFullRestartDoes()
        {
            var timer = new RunTimerService();
            timer.Start();
            timer.Tick(12.5d);
            timer.Tick(4.25d);

            Assert.That(timer.ElapsedSeconds, Is.EqualTo(16.75d).Within(0.0001d));

            timer.Reset();

            Assert.That(timer.ElapsedSeconds, Is.EqualTo(0d));
            Assert.That(timer.IsRunning, Is.False);
        }

        [Test]
        public void RunTimer_CompletionFreezesFinalTime()
        {
            var timer = new RunTimerService();
            timer.Start();
            timer.Tick(8.25d);
            var first = timer.Complete();
            timer.Tick(10d);
            var second = timer.Complete();

            Assert.That(first, Is.EqualTo(8.25d).Within(0.0001d));
            Assert.That(second, Is.EqualTo(first).Within(0.0001d));
            Assert.That(timer.ElapsedSeconds, Is.EqualTo(8.25d).Within(0.0001d));
        }

        [Test]
        public void RunTimer_StopPausesUntilStartedAgain()
        {
            var timer = new RunTimerService();
            timer.Start();
            timer.Tick(5d);
            timer.Stop();
            timer.Tick(20d);
            timer.Start();
            timer.Tick(2.5d);

            Assert.That(timer.ElapsedSeconds, Is.EqualTo(7.5d).Within(0.0001d));
        }

        [Test]
        public void TimerFormatting_UsesMinutesSecondsMilliseconds()
        {
            Assert.That(RunTimerFormatting.Format(34.281d), Is.EqualTo("00:34.281"));
            Assert.That(RunTimerFormatting.Format(312.004d), Is.EqualTo("05:12.004"));
        }

        [Test]
        public void RankEvaluation_UsesBronzeMinimumAndExactBoundaries()
        {
            var thresholds = Thresholds();

            Assert.That(ModuleRankUtility.Evaluate(90d, thresholds), Is.EqualTo(ModuleRank.Bronze));
            Assert.That(ModuleRankUtility.Evaluate(50d, thresholds), Is.EqualTo(ModuleRank.Silver));
            Assert.That(ModuleRankUtility.Evaluate(49.99d, thresholds), Is.EqualTo(ModuleRank.Silver));
            Assert.That(ModuleRankUtility.Evaluate(35d, thresholds), Is.EqualTo(ModuleRank.Gold));
            Assert.That(ModuleRankUtility.Evaluate(28d, thresholds), Is.EqualTo(ModuleRank.Diamond));
            Assert.That(ModuleRankUtility.Evaluate(20d, thresholds), Is.EqualTo(ModuleRank.Diamond));
        }

        [Test]
        public void ModuleValidator_RejectsInvalidRankThresholdOrdering()
        {
            var module = CreateModule();
            module.ConfigureRankThresholds(new ModuleRankThresholds(
                30f,
                35f,
                35f,
                RankCalibrationState.Uncalibrated,
                1,
                "bad"));

            var errors = ModuleDefinitionValidator.Validate(module);

            Assert.That(errors, Does.Contain("module.test.phase3 rank thresholds must satisfy Diamond < Gold < Silver."));

            DestroyModule(module);
        }

        [Test]
        public void Score_IsStrictlyBetterForFasterComparableRuns()
        {
            var thresholds = Thresholds();
            var previous = ModuleScoreService.CalculateScore(10.000001d, thresholds);
            for (var index = 2; index < 2000; index++)
            {
                var slower = ModuleScoreService.CalculateScore(10d + index * 0.000001d, thresholds);
                Assert.That(previous, Is.GreaterThan(slower));
                previous = slower;
            }
        }

        [Test]
        public void Progression_BronzeCompletionUnlocksNextModule()
        {
            var progression = new ProgressionData();
            var modules = new[]
            {
                "module.001.first-steps",
                "module.002.moving-parts",
                "module.003.flow-error"
            };

            Assert.That(ModuleProgressionData.IsUnlocked(progression, modules, modules[0]), Is.True);
            Assert.That(ModuleProgressionData.IsUnlocked(progression, modules, modules[1]), Is.False);
            Assert.That(ModuleProgressionData.IsUnlocked(progression, modules, modules[2]), Is.False);

            ModuleProgressionData.RecordCompletion(
                progression,
                modules[0],
                999d,
                ModuleRank.Bronze.ToString(),
                1000,
                Array.Empty<ModuleSplitRecord>(),
                true,
                1,
                1,
                1,
                RankCalibrationState.Uncalibrated.ToString(),
                RunValidity.ValidUnassisted.ToString(),
                "2026-08-12T00:00:00Z");

            Assert.That(ModuleProgressionData.IsUnlocked(progression, modules, modules[1]), Is.True);
            Assert.That(ModuleProgressionData.IsUnlocked(progression, modules, modules[2]), Is.False);

            ModuleProgressionData.RecordCompletion(
                progression,
                modules[1],
                999d,
                ModuleRank.Bronze.ToString(),
                1000,
                Array.Empty<ModuleSplitRecord>(),
                true,
                1,
                1,
                1,
                RankCalibrationState.Uncalibrated.ToString(),
                RunValidity.ValidUnassisted.ToString(),
                "2026-08-12T00:01:00Z");

            Assert.That(ModuleProgressionData.IsUnlocked(progression, modules, modules[2]), Is.True);
            Assert.That(
                ModuleProgressionData.IsUnlocked(
                    progression,
                    modules,
                    ModuleSelectionState.SpiralModuleId),
                Is.False);
        }

        [Test]
        public void PersonalBest_FasterValidRunReplacesButSlowerAndInvalidDoNot()
        {
            var progression = new ProgressionData();
            var moduleId = "module.001.first-steps";

            var first = ModuleProgressionData.RecordCompletion(
                progression,
                moduleId,
                40d,
                ModuleRank.Silver.ToString(),
                100,
                new[] { new ModuleSplitRecord { checkpointId = "restore.one", seconds = 12d } },
                true,
                1,
                1,
                1,
                "Uncalibrated",
                RunValidity.ValidUnassisted.ToString(),
                "2026-08-12T00:00:00Z");
            var slower = ModuleProgressionData.RecordCompletion(
                progression,
                moduleId,
                42d,
                ModuleRank.Bronze.ToString(),
                90,
                Array.Empty<ModuleSplitRecord>(),
                true,
                1,
                1,
                1,
                "Uncalibrated",
                RunValidity.ValidUnassisted.ToString(),
                "2026-08-12T00:01:00Z");
            var invalidFaster = ModuleProgressionData.RecordCompletion(
                progression,
                moduleId,
                20d,
                ModuleRank.Diamond.ToString(),
                200,
                Array.Empty<ModuleSplitRecord>(),
                false,
                1,
                1,
                1,
                "Uncalibrated",
                RunValidity.InvalidDevelopment.ToString(),
                "2026-08-12T00:02:00Z");

            var record = ModuleProgressionData.GetRecord(progression, moduleId);
            Assert.That(first, Is.True);
            Assert.That(slower, Is.False);
            Assert.That(invalidFaster, Is.False);
            Assert.That(record.bestTimeSeconds, Is.EqualTo(40d));
            Assert.That(record.bestSplits, Has.Length.EqualTo(1));
            Assert.That(record.completionCount, Is.EqualTo(3));
        }

        [Test]
        public void CompletionRewards_ValidBronzeCompletionGrantsOnce()
        {
            var module = CreateModule();
            module.ConfigureCompletionRewards(new[]
            {
                new ModuleCompletionRewardDefinition(
                    "reward.module-test.bronze.shards",
                    ModuleRewardKind.Currency,
                    "currency.patch-shards",
                    75),
                new ModuleCompletionRewardDefinition(
                    "reward.module-test.bronze.package",
                    ModuleRewardKind.InventoryItem,
                    "loot.package.test",
                    1),
                new ModuleCompletionRewardDefinition(
                    "reward.module-test.bronze.skin",
                    ModuleRewardKind.Unlock,
                    "skin.ryder.test",
                    1,
                    ModuleRank.Bronze,
                    true,
                    "Skin")
            });
            var save = new SaveDocument();

            var first = ModuleCompletionRewardUtility.ApplyCompletionRewards(
                module,
                save,
                ModuleRank.Bronze,
                RunValidity.ValidUnassisted,
                "2026-08-16T15:00:00Z",
                out var rewardLines);
            var second = ModuleCompletionRewardUtility.ApplyCompletionRewards(
                module,
                save,
                ModuleRank.Diamond,
                RunValidity.ValidUnassisted,
                "2026-08-16T15:01:00Z");

            Assert.That(first, Is.EqualTo(3));
            Assert.That(rewardLines, Is.EqualTo(new[] { "+75 PATCH SHARDS", "LOOT PACKAGE", "SKIN UNLOCK" }));
            Assert.That(second, Is.Zero);
            Assert.That(EconomyProgressionData.GetBalance(save.economy, "currency.patch-shards"), Is.EqualTo(75));
            Assert.That(InventoryProgressionData.GetQuantity(save.inventory, "loot.package.test"), Is.EqualTo(1));
            Assert.That(InventoryProgressionData.HasUnlock(save.inventory, "skin.ryder.test"), Is.True);
            Assert.That(save.progression.claimedRewardIds, Has.Length.EqualTo(3));

            DestroyModule(module);
        }

        [Test]
        public void CompletionRewards_DevelopmentRunsDoNotGrant()
        {
            var module = CreateModule();
            module.ConfigureCompletionRewards(new[]
            {
                new ModuleCompletionRewardDefinition(
                    "reward.module-test.bronze.shards",
                    ModuleRewardKind.Currency,
                    "currency.patch-shards",
                    75)
            });
            var save = new SaveDocument();

            var applied = ModuleCompletionRewardUtility.ApplyCompletionRewards(
                module,
                save,
                ModuleRank.Diamond,
                RunValidity.InvalidDevelopment,
                "2026-08-16T15:00:00Z");

            Assert.That(applied, Is.Zero);
            Assert.That(EconomyProgressionData.GetBalance(save.economy, "currency.patch-shards"), Is.Zero);
            Assert.That(save.progression.claimedRewardIds, Is.Empty);

            DestroyModule(module);
        }

        [Test]
        public void SplitComparison_ComputesAheadAndBehindDeltas()
        {
            var module = CreateModule();
            var session = new ModuleRunSession();
            session.Begin(
                module,
                RunValidity.ValidUnassisted,
                new[] { new RunSplit { checkpointId = "restore.one", seconds = 16.1d } });
            session.Timer.Tick(15.4d);

            var split = session.RecordSplit("restore.one");

            Assert.That(split.hasPersonalBestComparison, Is.True);
            Assert.That(split.pbDeltaSeconds, Is.EqualTo(-0.7d).Within(0.0001d));

            DestroyModule(module);
        }

        [Test]
        public void SplitComparison_AllowsZeroMissingAndManyRestorePoints()
        {
            var module = CreateModule();
            var session = new ModuleRunSession();
            session.Begin(module, RunValidity.ValidUnassisted);

            Assert.That(session.Splits, Is.Empty);

            for (var index = 0; index < 24; index++)
            {
                session.Timer.Tick(1.25d);
                var split = session.RecordSplit($"restore.{index:00}");
                Assert.That(split.hasPersonalBestComparison, Is.False);
            }

            Assert.That(session.Splits, Has.Count.EqualTo(24));

            DestroyModule(module);
        }

        [Test]
        public void ResultModel_ReportsNextRankGap()
        {
            var module = CreateModule();
            var session = new ModuleRunSession();
            session.Begin(module, RunValidity.ValidUnassisted);
            session.Timer.Tick(40d);

            var result = session.Complete(1);

            Assert.That(result.Rank, Is.EqualTo(ModuleRank.Silver));
            Assert.That(result.NextRank, Is.EqualTo(ModuleRank.Gold));
            Assert.That(result.SecondsFromNextRank, Is.EqualTo(5d));

            DestroyModule(module);
        }

        [Test]
        public void ResultModel_DuplicateCompletionReturnsSameFrozenResult()
        {
            var module = CreateModule();
            var session = new ModuleRunSession();
            session.Begin(module, RunValidity.ValidUnassisted);
            session.Timer.Tick(12d);

            var first = session.Complete(1);
            session.Timer.Tick(10d);
            var second = session.Complete(99);

            Assert.That(second, Is.SameAs(first));
            Assert.That(second.CompletionSeconds, Is.EqualTo(12d).Within(0.0001d));
            Assert.That(second.MovementCompatibilityVersion, Is.EqualTo(1));

            DestroyModule(module);
        }

        private static ModuleRankThresholds Thresholds() =>
            new ModuleRankThresholds(
                50f,
                35f,
                28f,
                RankCalibrationState.Uncalibrated,
                1,
                "test");

        private static ModuleDefinition CreateModule()
        {
            var module = ScriptableObject.CreateInstance<ModuleDefinition>();
            module.Configure(
                "module.test.phase3",
                "Phase 3 Test",
                "phase_3_test",
                "project.ryders-block",
                "world.prototype-sky",
                1,
                ModuleSelectionState.ModuleRunnerSceneName,
                ModuleDifficulty.Intro,
                new ModulePose(Vector3.zero, Vector3.zero),
                new ModulePatchBlockDefinition("patch.test", Vector3.forward, Vector3.one),
                Array.Empty<ModuleRestorePointDefinition>(),
                20f,
                60f,
                new[] { ModuleMechanic.StandardBlock, ModuleMechanic.PatchBlock },
                Array.Empty<ModuleShortcutDefinition>(),
                ModuleEnvironmentProfile.CreateRuntimeDefault(),
                ModuleVisualProfile.CreateRuntimeDefault(),
                string.Empty,
                string.Empty,
                new[]
                {
                    new ModuleBlockDefinition("block.test", Vector3.zero, Vector3.one, ModuleMaterialRole.Normal)
                },
                Array.Empty<ModuleMovingBlockDefinition>(),
                Array.Empty<ModuleBoostBlockDefinition>(),
                Array.Empty<ModuleWaterVolumeDefinition>(),
                Array.Empty<ModuleCrumblingBlockDefinition>(),
                Array.Empty<ModuleBlockDefinition>(),
                Array.Empty<ModuleCameraHintDefinition>());
            module.ConfigureRankThresholds(Thresholds());
            return module;
        }

        private static void DestroyModule(ModuleDefinition module)
        {
            UnityEngine.Object.DestroyImmediate(module.VisualProfile);
            UnityEngine.Object.DestroyImmediate(module.EnvironmentProfile);
            UnityEngine.Object.DestroyImmediate(module);
        }
    }
}
