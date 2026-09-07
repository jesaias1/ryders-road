using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Avoidance.Gameplay.Levels;
using Avoidance.Gameplay.Player;
using Avoidance.Gameplay.Visuals;
using UnityEditor;
using UnityEngine;

namespace Avoidance.EditorTools
{
    public static class Phase056ProductionAudit
    {
        private const string SpiralResourcePath = "Modules/Module_004_TheSpiral";

        [MenuItem("RYDERS BLOCK/QA/Write Phase 0.5.6 Audit")]
        public static void WriteReport()
        {
            var spiral = Resources.Load<ModuleDefinition>(SpiralResourcePath);
            var movement = Resources.Load<MovementProfile>("MovementProfiles/Movement_Default");
            var arms = Resources.Load<FirstPersonArmProfile>(FirstPersonArmProfile.ResourceName);
            var sky = Resources.Load<Material>("Materials/MAT_RR_Skybox_Seamless");
            if (spiral == null || movement == null || arms == null || sky == null)
            {
                throw new InvalidDataException("Phase 0.5.6 audit inputs are missing.");
            }

            var metrics = MovementEnvelopeMeasurement.Measure(movement);
            var supports = BuildSupports(spiral).OrderBy(support => support.TopY).ToArray();
            var gaps = MeasureGaps(supports).ToArray();
            var start = ModuleStartSafety.Resolve(spiral);
            var builder = new StringBuilder();
            builder.AppendLine("RYDER'S ROAD - PHASE 0.5.6 PRODUCTION AUDIT");
            builder.AppendLine("Generated: " + System.DateTime.UtcNow.ToString("u", CultureInfo.InvariantCulture));
            builder.AppendLine();
            builder.AppendLine("MOVEMENT ENVELOPE (production profile unchanged)");
            AppendMetric(builder, "Grounded run speed", metrics.RunSpeed, "m/s");
            AppendMetric(builder, "Jump airtime", metrics.JumpAirtime, "s");
            AppendMetric(builder, "Normal forward jump distance", metrics.NormalJumpDistance);
            AppendMetric(builder, "Comfortable Bronze gap", metrics.ComfortableBronzeGap);
            AppendMetric(builder, "Landing-margin distance", metrics.LandingMarginDistance);
            AppendMetric(builder, "Maximum reasonable Bronze gap", metrics.MaximumBronzeGap);
            AppendMetric(builder, "Bhop-assisted range", metrics.BhopAssistedDistance);
            AppendMetric(builder, "Air-strafe-assisted range", metrics.AirStrafeAssistedDistance);
            AppendMetric(builder, "Boost-assisted range", metrics.BoostAssistedDistance);
            builder.AppendLine();
            builder.AppendLine("SPIRAL AUTHORING");
            builder.AppendLine($"- Content version: {spiral.ContentVersion}");
            builder.AppendLine($"- Start: {start.Pose.Position} on {start.SupportBlockStableId}; correction={start.UsedSafetyCorrection}");
            builder.AppendLine($"- Fall threshold: {spiral.ResolveFallThreshold(-30f):0.00} m");
            builder.AppendLine($"- Route supports: {supports.Length}; measured transitions: {gaps.Length}");
            builder.AppendLine($"- Edge gaps min/mean/max: {gaps.Min():0.00} / {gaps.Average():0.00} / {gaps.Max():0.00} m");
            builder.AppendLine($"- Edge gaps > 0.50 m: {gaps.Count(gap => gap > 0.5f)}");
            builder.AppendLine($"- Blocks/restores/landmarks: {spiral.Blocks.Count}/{spiral.RestorePoints.Count}/{spiral.Decorations.Count}");
            builder.AppendLine($"- Optional mechanics moving/boost/crumble/surf: {spiral.MovingBlocks.Count}/{spiral.BoostBlocks.Count}/{spiral.CrumblingBlocks.Count}/{spiral.SurfSurfaces.Count}");
            builder.AppendLine($"- Automatic distant fragments disabled: {spiral.DisableAutomaticDistantFragments}");
            builder.AppendLine();
            builder.AppendLine("PRESENTATION");
            builder.AppendLine($"- Hands scale: {arms.PrefabLocalScale.x:0.00}; left pose: {arms.LeftBaseLocalPosition} / {arms.LeftBaseLocalEulerAngles}");
            builder.AppendLine($"- Sky shader: {sky.shader.name}; exposure: {sky.GetFloat("_Exposure"):0.00}");

            var logs = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Logs");
            Directory.CreateDirectory(logs);
            var path = Path.Combine(logs, "phase056-production-audit.txt");
            File.WriteAllText(path, builder.ToString());
            Debug.Log("Phase 0.5.6 production audit written to " + path);
        }

        private static void AppendMetric(
            StringBuilder builder,
            string label,
            float value,
            string unit = "m")
        {
            builder.AppendLine($"- {label}: {value:0.00} {unit}");
        }

        private static IEnumerable<RouteSupport> BuildSupports(ModuleDefinition module)
        {
            foreach (var block in module.Blocks)
            {
                yield return new RouteSupport(block.Pose, block.Size);
            }

            foreach (var restore in module.RestorePoints)
            {
                yield return new RouteSupport(restore.Pose, new Vector3(3f, 0.6f, 3f));
            }

            yield return new RouteSupport(
                new ModulePose(
                    module.PatchBlock.Pose.Position + Vector3.down * 0.8f,
                    module.PatchBlock.Pose.EulerAngles),
                new Vector3(3f, 0.6f, 3f));
        }

        private static IEnumerable<float> MeasureGaps(IReadOnlyList<RouteSupport> supports)
        {
            for (var index = 0; index < supports.Count - 1; index++)
            {
                var from = supports[index];
                var to = supports[index + 1];
                var delta = to.Pose.Position - from.Pose.Position;
                delta.y = 0f;
                var distance = delta.magnitude;
                var direction = distance > 0.001f ? delta / distance : Vector3.zero;
                yield return Mathf.Max(
                    0f,
                    distance - SupportRadius(from, direction) - SupportRadius(to, direction));
            }
        }

        private static float SupportRadius(RouteSupport support, Vector3 direction)
        {
            var local = Quaternion.Inverse(support.Pose.Rotation) * direction;
            return Mathf.Abs(local.x) * support.Size.x * 0.5f
                + Mathf.Abs(local.z) * support.Size.z * 0.5f;
        }

        private readonly struct RouteSupport
        {
            public RouteSupport(ModulePose pose, Vector3 size)
            {
                Pose = pose;
                Size = size;
            }

            public ModulePose Pose { get; }
            public Vector3 Size { get; }
            public float TopY => Pose.Position.y + Size.y * 0.5f;
        }
    }
}
