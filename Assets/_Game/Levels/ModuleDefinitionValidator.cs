using System.Collections.Generic;
using System.Linq;
using Avoidance.Core.Utilities;
using UnityEngine;

namespace Avoidance.Gameplay.Levels
{
    public static class ModuleDefinitionValidator
    {
        public static IReadOnlyList<string> Validate(ModuleDefinition module)
        {
            var errors = new List<string>();
            if (module == null)
            {
                errors.Add("ModuleDefinition is missing.");
                return errors;
            }

            ValidateStableId(module.StableModuleId, "Module stable ID", errors);
            if (string.IsNullOrWhiteSpace(module.DisplayName))
            {
                errors.Add($"{module.name} has no display name.");
            }

            if (string.IsNullOrWhiteSpace(module.InternalName))
            {
                errors.Add($"{module.name} has no internal name.");
            }

            if (module.ContentVersion <= 0)
            {
                errors.Add($"{module.StableModuleId} content version must be positive.");
            }

            if (module.ExpectedCleanTime <= 0f || module.EstimatedCasualTime <= 0f)
            {
                errors.Add($"{module.StableModuleId} time estimates must be positive.");
            }

            ValidateRankThresholds(module, errors);
            ValidateStartSafety(module, errors);

            if (module.PatchBlock == null)
            {
                errors.Add($"{module.StableModuleId} is missing a Patch Block.");
            }
            else
            {
                ValidateStableId(module.PatchBlock.StableId, "Patch Block ID", errors);
                ValidateSupportReference(module, module.PatchBlock.SupportBlockStableId, errors);
            }

            if (module.VisualProfile == null)
            {
                errors.Add($"{module.StableModuleId} is missing a visual profile.");
            }

            if (module.EnvironmentProfile == null)
            {
                errors.Add($"{module.StableModuleId} is missing an environment profile.");
            }

            ValidateRestorePoints(module, errors);
            ValidateBlocks(module.StableModuleId, module.Blocks, "block", errors);
            ValidateBlocks(module.StableModuleId, module.Decorations, "decoration", errors);
            ValidateMovingBlocks(module, errors);
            ValidateBoosts(module, errors);
            ValidateWater(module, errors);
            ValidateSurfSurfaces(module, errors);
            ValidateCrumbling(module, errors);
            ValidateShortcuts(module, errors);
            ValidateCameraHints(module, errors);
            ValidateCompletionRewards(module, errors);
            return errors;
        }

        private static void ValidateSupportReference(ModuleDefinition module, string stableId, ICollection<string> errors)
        {
            if (string.IsNullOrEmpty(stableId)) return;
            if (!module.Blocks.Any(block => block != null && block.StableId == stableId))
                errors.Add($"{module.StableModuleId} references missing mechanic support {stableId}.");
        }

        private static void ValidateStartSafety(
            ModuleDefinition module,
            ICollection<string> errors)
        {
            try
            {
                var start = ModuleStartSafety.Resolve(module);
                if (start.UsedSafetyCorrection)
                {
                    errors.Add(
                        $"{module.StableModuleId} start anchor '{module.StartAnchorStableId}' "
                        + $"is not safely supported by '{start.SupportBlockStableId}'.");
                }
            }
            catch (System.InvalidOperationException exception)
            {
                errors.Add(exception.Message);
            }
        }

        private static void ValidateRankThresholds(
            ModuleDefinition module,
            ICollection<string> errors)
        {
            var thresholds = module.RankThresholds;
            if (thresholds == null)
            {
                errors.Add($"{module.StableModuleId} is missing rank thresholds.");
                return;
            }

            if (thresholds.SilverSeconds <= 0f
                || thresholds.GoldSeconds <= 0f
                || thresholds.DiamondSeconds <= 0f)
            {
                errors.Add($"{module.StableModuleId} rank thresholds must be positive.");
            }

            if (thresholds.DiamondSeconds >= thresholds.GoldSeconds
                || thresholds.GoldSeconds >= thresholds.SilverSeconds)
            {
                errors.Add(
                    $"{module.StableModuleId} rank thresholds must satisfy Diamond < Gold < Silver.");
            }

            if (thresholds.ThresholdVersion <= 0)
            {
                errors.Add($"{module.StableModuleId} threshold version must be positive.");
            }
        }

        public static IReadOnlyList<string> ValidateCatalog(IEnumerable<ModuleDefinition> modules)
        {
            var errors = new List<string>();
            var seen = new HashSet<string>();
            foreach (var module in modules.Where(module => module != null))
            {
                errors.AddRange(Validate(module));
                if (!string.IsNullOrWhiteSpace(module.StableModuleId)
                    && !seen.Add(module.StableModuleId))
                {
                    errors.Add($"Duplicate module stable ID: {module.StableModuleId}");
                }
            }

            return errors;
        }

        private static void ValidateRestorePoints(
            ModuleDefinition module,
            ICollection<string> errors)
        {
            var seen = new HashSet<string>();
            var lastOrder = -1;
            foreach (var restore in module.RestorePoints)
            {
                if (restore == null)
                {
                    errors.Add($"{module.StableModuleId} contains a null Restore Point.");
                    continue;
                }

                ValidateStableId(restore.StableId, "Restore Point ID", errors);
                ValidateSupportReference(module, restore.SupportBlockStableId, errors);
                if (!seen.Add(restore.StableId))
                {
                    errors.Add($"{module.StableModuleId} duplicates Restore Point ID {restore.StableId}.");
                }

                if (restore.Order <= lastOrder)
                {
                    errors.Add($"{module.StableModuleId} Restore Point orders must increase.");
                }

                lastOrder = restore.Order;
            }
        }

        private static void ValidateBlocks(
            string moduleId,
            IReadOnlyList<ModuleBlockDefinition> blocks,
            string label,
            ICollection<string> errors)
        {
            var seen = new HashSet<string>();
            foreach (var block in blocks)
            {
                if (block == null)
                {
                    errors.Add($"{moduleId} contains a null {label}.");
                    continue;
                }

                ValidateStableId(block.StableId, $"{label} ID", errors);
                if (!seen.Add(block.StableId))
                {
                    errors.Add($"{moduleId} duplicates {label} ID {block.StableId}.");
                }

                if (block.Size.x <= 0f || block.Size.y <= 0f || block.Size.z <= 0f)
                {
                    errors.Add($"{moduleId} {block.StableId} must have positive dimensions.");
                }
            }
        }

        private static void ValidateMovingBlocks(
            ModuleDefinition module,
            ICollection<string> errors)
        {
            foreach (var moving in module.MovingBlocks)
            {
                if (moving == null)
                {
                    errors.Add($"{module.StableModuleId} contains a null Moving Block.");
                    continue;
                }

                ValidateStableId(moving.StableId, "Moving Block ID", errors);
                if (moving.PathPoints.Count < 2)
                {
                    errors.Add($"{moving.StableId} must have at least two path points.");
                }

                if (moving.Speed <= 0f)
                {
                    errors.Add($"{moving.StableId} speed must be positive.");
                }

                if (moving.PauseAtEndpoints < 0f)
                {
                    errors.Add($"{moving.StableId} endpoint pause cannot be negative.");
                }
            }
        }

        private static void ValidateBoosts(ModuleDefinition module, ICollection<string> errors)
        {
            foreach (var boost in module.BoostBlocks)
            {
                if (boost == null)
                {
                    errors.Add($"{module.StableModuleId} contains a null Jump Boost.");
                    continue;
                }

                ValidateStableId(boost.StableId, "Jump Boost ID", errors);
                if (boost.LaunchDirection.sqrMagnitude < 0.001f)
                {
                    errors.Add($"{boost.StableId} launch direction must be non-zero.");
                }

                if (boost.VerticalStrength < 0f || boost.HorizontalStrength < 0f)
                {
                    errors.Add($"{boost.StableId} launch strengths cannot be negative.");
                }

                if (boost.Cooldown < 0f)
                {
                    errors.Add($"{boost.StableId} cooldown cannot be negative.");
                }
            }
        }

        private static void ValidateWater(ModuleDefinition module, ICollection<string> errors)
        {
            foreach (var water in module.WaterVolumes)
            {
                if (water == null)
                {
                    errors.Add($"{module.StableModuleId} contains a null Water volume.");
                    continue;
                }

                ValidateStableId(water.StableId, "Water ID", errors);
                if (water.FlowDirection.sqrMagnitude < 0.001f)
                {
                    errors.Add($"{water.StableId} flow direction must be non-zero.");
                }

                if (water.FlowForce < 0f || water.MaxAddedVelocity < 0f || water.Drag < 0f)
                {
                    errors.Add($"{water.StableId} flow force, max velocity, and drag cannot be negative.");
                }
            }
        }

        private static void ValidateSurfSurfaces(ModuleDefinition module, ICollection<string> errors)
        {
            foreach (var surf in module.SurfSurfaces)
            {
                if (surf == null)
                {
                    errors.Add($"{module.StableModuleId} contains a null Surf surface.");
                    continue;
                }

                ValidateStableId(surf.StableId, "Surf surface ID", errors);
                if (surf.Size.x <= 0f || surf.Size.y <= 0f || surf.Size.z <= 0f)
                {
                    errors.Add($"{surf.StableId} must have positive dimensions.");
                }

                if (surf.FlowDirection.sqrMagnitude < 0.001f)
                {
                    errors.Add($"{surf.StableId} flow direction must be non-zero.");
                }
            }
        }

        private static void ValidateCrumbling(ModuleDefinition module, ICollection<string> errors)
        {
            foreach (var crumbling in module.CrumblingBlocks)
            {
                if (crumbling == null)
                {
                    errors.Add($"{module.StableModuleId} contains a null Crumbling Block.");
                    continue;
                }

                ValidateStableId(crumbling.StableId, "Crumbling Block ID", errors);
                if (crumbling.ActivationDelay < 0f
                    || crumbling.FallDelay <= 0f
                    || crumbling.ResetTime < 0f
                    || crumbling.ShakeAmount < 0f)
                {
                    errors.Add($"{crumbling.StableId} has invalid crumbling timing.");
                }
            }
        }

        private static void ValidateShortcuts(ModuleDefinition module, ICollection<string> errors)
        {
            foreach (var shortcut in module.OptionalShortcuts)
            {
                if (shortcut == null)
                {
                    errors.Add($"{module.StableModuleId} contains a null shortcut.");
                    continue;
                }

                ValidateStableId(shortcut.StableId, "Shortcut ID", errors);
                if (string.IsNullOrWhiteSpace(shortcut.DisplayName))
                {
                    errors.Add($"{shortcut.StableId} has no display name.");
                }
            }
        }

        private static void ValidateCameraHints(ModuleDefinition module, ICollection<string> errors)
        {
            foreach (var hint in module.CameraHints)
            {
                if (hint == null)
                {
                    errors.Add($"{module.StableModuleId} contains a null camera hint.");
                    continue;
                }

                ValidateStableId(hint.StableId, "Camera hint ID", errors);
                if (hint.MaxAssistDegrees < 0f || hint.MaxAssistDegrees > 45f)
                {
                    errors.Add($"{hint.StableId} camera assist must be between 0 and 45 degrees.");
                }
            }
        }

        private static void ValidateCompletionRewards(
            ModuleDefinition module,
            ICollection<string> errors)
        {
            var seen = new HashSet<string>();
            foreach (var reward in module.CompletionRewards)
            {
                if (reward == null)
                {
                    errors.Add($"{module.StableModuleId} contains a null completion reward.");
                    continue;
                }

                ValidateStableId(reward.StableId, "Completion reward ID", errors);
                ValidateStableId(reward.ContentId, "Completion reward content ID", errors);
                if (!seen.Add(reward.StableId))
                {
                    errors.Add($"{module.StableModuleId} duplicates completion reward ID {reward.StableId}.");
                }

                if (reward.Quantity <= 0)
                {
                    errors.Add($"{reward.StableId} reward quantity must be positive.");
                }

                if (reward.MinimumRank < Avoidance.Gameplay.Ranking.ModuleRank.Bronze
                    || reward.MinimumRank > Avoidance.Gameplay.Ranking.ModuleRank.Diamond)
                {
                    errors.Add($"{reward.StableId} reward minimum rank must be Bronze, Silver, Gold, or Diamond.");
                }

                if (reward.Kind == ModuleRewardKind.Unlock
                    && string.IsNullOrWhiteSpace(reward.UnlockType))
                {
                    errors.Add($"{reward.StableId} unlock rewards require an unlock type.");
                }
            }
        }

        private static void ValidateStableId(
            string value,
            string label,
            ICollection<string> errors)
        {
            if (!StableId.IsValid(value))
            {
                errors.Add($"{label} is invalid: {value}");
            }
        }
    }
}
