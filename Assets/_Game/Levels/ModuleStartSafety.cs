using System;
using System.Linq;
using UnityEngine;

namespace Avoidance.Gameplay.Levels
{
    public readonly struct ModuleStartResolution
    {
        public ModuleStartResolution(
            ModulePose pose,
            string supportBlockStableId,
            bool usedSafetyCorrection,
            string diagnostic)
        {
            Pose = pose;
            SupportBlockStableId = supportBlockStableId;
            UsedSafetyCorrection = usedSafetyCorrection;
            Diagnostic = diagnostic;
        }

        public ModulePose Pose { get; }
        public string SupportBlockStableId { get; }
        public bool UsedSafetyCorrection { get; }
        public string Diagnostic { get; }
    }

    public static class ModuleStartSafety
    {
        public const float PlayerFootClearance = 0.05f;
        public const float PlayerCapsuleRadius = 0.38f;
        private const float VerticalTolerance = 0.08f;

        public static ModuleStartResolution Resolve(ModuleDefinition module)
        {
            if (module == null)
            {
                throw new ArgumentNullException(nameof(module));
            }

            var support = ResolveSupport(module);
            if (support == null)
            {
                throw new InvalidOperationException(
                    $"{module.StableModuleId} start anchor '{module.StartAnchorStableId}' "
                    + "does not reference or overlap a solid authored block.");
            }

            var authored = module.StartPoint;
            var expectedY = support.Pose.Position.y + support.Size.y * 0.5f + PlayerFootClearance;
            var local = Quaternion.Inverse(support.Pose.Rotation)
                * (authored.Position - support.Pose.Position);
            var insideX = Mathf.Abs(local.x) <= support.Size.x * 0.5f - PlayerCapsuleRadius;
            var insideZ = Mathf.Abs(local.z) <= support.Size.z * 0.5f - PlayerCapsuleRadius;
            var onTop = Mathf.Abs(authored.Position.y - expectedY) <= VerticalTolerance;
            if (insideX && insideZ && onTop)
            {
                return new ModuleStartResolution(
                    authored,
                    support.StableId,
                    false,
                    "Authored start anchor is centered above solid support.");
            }

            var corrected = new ModulePose(
                support.Pose.Position + Vector3.up * (support.Size.y * 0.5f + PlayerFootClearance),
                authored.EulerAngles);
            return new ModuleStartResolution(
                corrected,
                support.StableId,
                true,
                $"Authored start was outside '{support.StableId}' support; corrected to its solid top center.");
        }

        public static bool HasSolidSupport(ModuleDefinition module, ModulePose pose)
        {
            if (module == null)
            {
                return false;
            }

            return module.Blocks.Any(block => SupportsPose(block, pose));
        }

        private static ModuleBlockDefinition ResolveSupport(ModuleDefinition module)
        {
            if (!string.IsNullOrWhiteSpace(module.StartSupportBlockStableId))
            {
                return module.Blocks.FirstOrDefault(
                    block => block.StableId == module.StartSupportBlockStableId);
            }

            return module.Blocks.FirstOrDefault(block => SupportsPose(block, module.StartPoint));
        }

        private static bool SupportsPose(ModuleBlockDefinition block, ModulePose pose)
        {
            var local = Quaternion.Inverse(block.Pose.Rotation)
                * (pose.Position - block.Pose.Position);
            var expectedY = block.Size.y * 0.5f + PlayerFootClearance;
            return Mathf.Abs(local.x) <= block.Size.x * 0.5f - PlayerCapsuleRadius
                && Mathf.Abs(local.z) <= block.Size.z * 0.5f - PlayerCapsuleRadius
                && Mathf.Abs(local.y - expectedY) <= VerticalTolerance;
        }
    }
}
