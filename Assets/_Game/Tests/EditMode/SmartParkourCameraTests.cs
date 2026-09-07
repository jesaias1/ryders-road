using Avoidance.Gameplay.Camera;
using NUnit.Framework;
using UnityEngine;

namespace Avoidance.Tests.EditMode
{
    public sealed class SmartParkourCameraTests
    {
        [Test]
        public void CameraCone_HasDeadZoneAndSmoothOuterResponse()
        {
            var profile = ParkourCameraProfile.CreateRuntimeDefault();

            Assert.That(SmartParkourCameraMath.ConeResponse(0f, 4f, profile), Is.EqualTo(0f));
            Assert.That(
                SmartParkourCameraMath.ConeResponse(0f, 16f, profile),
                Is.GreaterThan(0f).And.LessThan(1f));
            Assert.That(SmartParkourCameraMath.ConeResponse(0f, 40f, profile), Is.EqualTo(1f).Within(0.001f));

            Object.DestroyImmediate(profile);
        }

        [Test]
        public void CameraCone_ResponseIsContinuousAndMonotonic()
        {
            var profile = ParkourCameraProfile.CreateRuntimeDefault();
            var previous = 0f;

            for (var yaw = 0f; yaw <= 40f; yaw += 1f)
            {
                var response = SmartParkourCameraMath.ConeResponse(0f, yaw, profile);
                Assert.That(response, Is.GreaterThanOrEqualTo(previous - 0.0001f));
                previous = response;
            }

            Object.DestroyImmediate(profile);
        }

        [Test]
        public void SmartCamera_DoesNotUseRawStickNoiseWhenVelocityIsClear()
        {
            var profile = ParkourCameraProfile.CreateRuntimeDefault();
            var controller = new SmartParkourCameraController(profile, SimpleGraph());
            var yaw = 0f;

            for (var index = 0; index < 24; index++)
            {
                var noisyInput = index % 2 == 0 ? new Vector2(1f, 0.1f) : new Vector2(-1f, 0.1f);
                var output = controller.Update(new SmartParkourCameraInput(
                    Vector3.forward * index,
                    0f,
                    yaw,
                    0f,
                    Vector3.forward * 8f,
                    noisyInput,
                    true,
                    false,
                    0f,
                    1f / 60f));
                yaw = output.WorldYaw;
            }

            Assert.That(Mathf.Abs(Mathf.DeltaAngle(0f, yaw)), Is.LessThan(2f));
            Object.DestroyImmediate(profile);
        }

        [Test]
        public void SmartCamera_StationarySteeringCanTurnCameraWithoutVelocity()
        {
            var profile = ParkourCameraProfile.CreateRuntimeDefault();
            var controller = new SmartParkourCameraController(profile, SimpleGraph());

            var output = controller.Update(new SmartParkourCameraInput(
                Vector3.zero,
                0f,
                0f,
                0f,
                Vector3.zero,
                Vector2.right,
                true,
                false,
                0f,
                0.2f));

            Assert.That(Mathf.DeltaAngle(0f, output.WorldYaw), Is.GreaterThan(0.5f));
            Object.DestroyImmediate(profile);
        }

        [Test]
        public void SmartCamera_OffRouteFallsBackToVelocityLookAhead()
        {
            var profile = ParkourCameraProfile.CreateRuntimeDefault();
            var controller = new SmartParkourCameraController(profile, SimpleGraph());

            var output = controller.Update(new SmartParkourCameraInput(
                new Vector3(200f, 0f, 200f),
                0f,
                0f,
                0f,
                Vector3.right * 8f,
                Vector2.up,
                true,
                false,
                0f,
                0.2f));

            Assert.That(output.RouteWeight, Is.EqualTo(0f).Within(0.001f));
            Assert.That(Mathf.DeltaAngle(0f, output.WorldYaw), Is.GreaterThan(0.5f));
            Object.DestroyImmediate(profile);
        }

        [Test]
        public void SmartCamera_VerticalFramingPitchesUpAndDown()
        {
            var profile = ParkourCameraProfile.CreateRuntimeDefault();
            var uphill = new SmartParkourCameraController(profile, VerticalGraph(12f));
            var downhill = new SmartParkourCameraController(profile, VerticalGraph(-12f));

            var upOutput = uphill.Update(new SmartParkourCameraInput(
                Vector3.zero,
                0f,
                0f,
                0f,
                Vector3.forward * 8f,
                Vector2.up,
                true,
                false,
                0f,
                0.5f));
            var downOutput = downhill.Update(new SmartParkourCameraInput(
                Vector3.zero,
                0f,
                0f,
                0f,
                Vector3.forward * 8f,
                Vector2.up,
                false,
                false,
                -8f,
                0.5f));

            Assert.That(upOutput.Pitch, Is.GreaterThan(-2f));
            Assert.That(downOutput.Pitch, Is.LessThan(upOutput.Pitch));
            Object.DestroyImmediate(profile);
        }

        [Test]
        public void BranchIntent_StaysNeutralWithoutMovementEvidence()
        {
            var profile = ParkourCameraProfile.CreateRuntimeDefault();
            var resolver = new BranchIntentResolver();
            var candidates = ForkBranches();

            var result = resolver.Update(
                candidates,
                candidates.Length,
                Vector3.zero,
                Vector3.zero,
                Vector3.zero,
                false,
                profile,
                0.25f);

            Assert.That(result.State, Is.EqualTo(BranchIntentState.Neutral));
            Assert.That(result.HasBranch, Is.False);
            Object.DestroyImmediate(profile);
        }

        [Test]
        public void BranchIntent_CommitsAfterSustainedShortcutEvidence()
        {
            var profile = ParkourCameraProfile.CreateRuntimeDefault();
            var resolver = new BranchIntentResolver();
            var candidates = ForkBranches();
            BranchIntentResult result = default;

            for (var index = 0; index < 16; index++)
            {
                result = resolver.Update(
                    candidates,
                    candidates.Length,
                    Vector3.zero,
                    Vector3.right,
                    Vector3.right,
                    false,
                    profile,
                    1f / 60f);
            }

            Assert.That(result.State, Is.EqualTo(BranchIntentState.Committed));
            Assert.That(result.BranchId, Is.EqualTo("route.shortcut"));
            Object.DestroyImmediate(profile);
        }

        [Test]
        public void BranchIntent_HysteresisRejectsBriefForkNoise()
        {
            var profile = ParkourCameraProfile.CreateRuntimeDefault();
            var resolver = new BranchIntentResolver();
            var candidates = ForkBranches();

            for (var index = 0; index < 16; index++)
            {
                resolver.Update(candidates, candidates.Length, Vector3.zero, Vector3.right, Vector3.right, false, profile, 1f / 60f);
            }

            var noisyResult = resolver.Update(
                candidates,
                candidates.Length,
                Vector3.zero,
                Vector3.forward,
                Vector3.forward,
                false,
                profile,
                1f / 60f);

            Assert.That(noisyResult.State, Is.EqualTo(BranchIntentState.Committed));
            Assert.That(noisyResult.BranchId, Is.EqualTo("route.shortcut"));
            Object.DestroyImmediate(profile);
        }

        [Test]
        public void RouteGraph_ReacquiresShortcutNearBranchAndPrimaryAfterMerge()
        {
            var graph = ForkGraph();

            var shortcut = graph.ProjectBest(new Vector3(9f, 0f, 0.5f), "route.standard", -1f, 7f);
            var merged = graph.ProjectBest(new Vector3(0.1f, 0f, 18f), shortcut.Branch.StableId, shortcut.DistanceAlong, 7f);

            Assert.That(shortcut.Branch.StableId, Is.EqualTo("route.shortcut"));
            Assert.That(merged.Branch.StableId, Is.EqualTo("route.standard"));
        }

        private static RouteCameraGraph SimpleGraph()
        {
            return new RouteCameraGraph(
                "test.simple",
                new[]
                {
                    new RouteCameraBranch(
                        "route.standard",
                        RouteCameraBranchKind.Standard,
                        new[]
                        {
                            new RouteCameraNode("route.start", Vector3.zero, Vector3.forward),
                            new RouteCameraNode("route.end", Vector3.forward * 80f, Vector3.forward)
                        },
                        7f)
                },
                "route.standard");
        }

        private static RouteCameraGraph VerticalGraph(float height)
        {
            return new RouteCameraGraph(
                "test.vertical",
                new[]
                {
                    new RouteCameraBranch(
                        "route.vertical",
                        RouteCameraBranchKind.Standard,
                        new[]
                        {
                            new RouteCameraNode("route.start", Vector3.zero, Vector3.forward),
                            new RouteCameraNode("route.end", new Vector3(0f, height, 48f), Vector3.forward)
                        },
                        7f)
                },
                "route.vertical");
        }

        private static RouteCameraGraph ForkGraph() =>
            new RouteCameraGraph("test.fork", ForkBranches(), "route.standard");

        private static RouteCameraBranch[] ForkBranches()
        {
            return new[]
            {
                new RouteCameraBranch(
                    "route.standard",
                    RouteCameraBranchKind.Standard,
                    new[]
                    {
                        new RouteCameraNode("standard.start", Vector3.zero, Vector3.forward),
                        new RouteCameraNode("standard.end", Vector3.forward * 24f, Vector3.forward)
                    },
                    7f),
                new RouteCameraBranch(
                    "route.shortcut",
                    RouteCameraBranchKind.Shortcut,
                    new[]
                    {
                        new RouteCameraNode("shortcut.start", Vector3.zero, Vector3.right),
                        new RouteCameraNode("shortcut.mid", Vector3.right * 12f, Vector3.right),
                        new RouteCameraNode("shortcut.end", new Vector3(0f, 0f, 18f), Vector3.forward)
                    },
                    7f,
                    "route.standard")
            };
        }
    }
}
