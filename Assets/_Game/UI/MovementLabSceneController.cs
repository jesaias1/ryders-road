using System;
using System.Collections.Generic;
using System.Linq;
using Avoidance.Core.Configuration;
using Avoidance.Core.Services;
using Avoidance.Gameplay.Audio;
using Avoidance.Gameplay.Blocks;
using Avoidance.Gameplay.Camera;
using Avoidance.Gameplay.Checkpoints;
using Avoidance.Gameplay.Levels;
using Avoidance.Gameplay.Player;
using Avoidance.Gameplay.Respawn;
using Avoidance.Gameplay.Timing;
using Avoidance.Gameplay.Visuals;
using Avoidance.Input;
using Avoidance.UI.Touch;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Avoidance.UI
{
    [DisallowMultipleComponent]
    public sealed class MovementLabSceneController : MonoBehaviour
    {
        private const MovementLabMaterialRole Standard = MovementLabMaterialRole.NormalPlatform;
        private const MovementLabMaterialRole Precision = MovementLabMaterialRole.PrecisionPlatform;
        private const MovementLabMaterialRole Moving = MovementLabMaterialRole.MovingPlatform;
        private const MovementLabMaterialRole Restore = MovementLabMaterialRole.RestorePoint;
        private const MovementLabMaterialRole Patch = MovementLabMaterialRole.PatchBlock;
        private const MovementLabMaterialRole Surf = MovementLabMaterialRole.SurfSurface;

        private readonly List<MovingPlatformMotion> _movingPlatforms =
            new List<MovingPlatformMotion>();
        private static Sprite _circleRingSprite;
        private static Sprite _filledCircleSprite;
        private MovementLabVisualProfile _visuals;
        private MovementLabHud _hud;

        private void Awake()
        {
            if (FlowLabSceneController.ConsumeLaunchRequest())
            {
                _visuals = Resources.Load<MovementLabVisualProfile>(MovementLabVisualProfile.ResourceName)
                    ?? MovementLabVisualProfile.CreateRuntimeDefault();
                gameObject.AddComponent<FlowLabSceneController>().Initialize(this);
                return;
            }
            var configuration = Resources.Load<GameConfiguration>("FoundationGameConfiguration")
                ?? GameConfiguration.CreateRuntimeDefault();
            var movementProfiles = LoadMovementProfiles();
            var cameraProfile = Resources.Load<CameraProfile>("Camera_Default")
                ?? CameraProfile.CreateRuntimeDefault();
            var touchLayout = Resources.Load<TouchControlLayout>("Touch_Default")
                ?? TouchControlLayout.CreateRuntimeDefault();
            _visuals = Resources.Load<MovementLabVisualProfile>(MovementLabVisualProfile.ResourceName)
                ?? MovementLabVisualProfile.CreateRuntimeDefault();

            CreateEnvironment(_visuals);
            var checkpointService = ResolveCheckpointService();
            var startPosition = new Vector3(0f, 0.35f, -2f);
            checkpointService.SetStart(startPosition, Quaternion.identity);
            CreateCourse(checkpointService);
            var touchInput = CreateTouchInterface(touchLayout, out var statusText, out var completionText);
            touchInput.EnableFoundationJumpLook();
            Avoidance.Gameplay.Audio.MusicDirector.SetContext("practice");
            CreatePlayer(
                startPosition,
                movementProfiles,
                cameraProfile,
                touchInput,
                checkpointService,
                configuration.BuildVersion,
                statusText,
                completionText);
        }

        private static MovementProfileSet LoadMovementProfiles()
        {
            return new MovementProfileSet(new[] { MovementProfile.LoadShared() });
        }

        private static int ProfileOrder(MovementProfile profile)
        {
            if (profile == null)
            {
                return int.MaxValue;
            }

            switch (profile.ProfileId)
            {
                case "movement.default": return 0;
                case "movement.forgiving": return 1;
                case "movement.precise": return 2;
                case "movement.experimental": return 3;
                default: return 100;
            }
        }

        private static CheckpointService ResolveCheckpointService()
        {
            if (GameServices.Current != null
                && GameServices.Current.TryGet<ICheckpointService>(out var service)
                && service is CheckpointService checkpointService)
            {
                checkpointService.Reset();
                return checkpointService;
            }

            return new CheckpointService();
        }

        private static void CreateEnvironment(MovementLabVisualProfile visuals)
        {
            RenderSettings.fog = true;
            RenderSettings.fogColor = visuals.SkyFog;
            RenderSettings.fogDensity = 0.006f;
            RenderSettings.ambientLight = visuals.AmbientLight;

            var sunObject = new GameObject("Movement Lab Sun", typeof(Light));
            sunObject.transform.rotation = Quaternion.Euler(48f, -28f, 0f);
            var sun = sunObject.GetComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.25f;
            sun.color = visuals.SunLight;

            var nullSpace = GameObject.CreatePrimitive(PrimitiveType.Cube);
            nullSpace.name = "Null Space Visual";
            nullSpace.transform.position = new Vector3(0f, -14f, 55f);
            nullSpace.transform.localScale = new Vector3(180f, 0.5f, 180f);
            nullSpace.GetComponent<Renderer>().sharedMaterial =
                visuals.MaterialFor(MovementLabMaterialRole.NullSpace);
            Destroy(nullSpace.GetComponent<Collider>());

            CreateDistantVisualBlock(
                visuals,
                "Distant Floating Block A",
                new Vector3(-24f, -2.8f, 44f),
                new Vector3(10f, 2.2f, 8f));
            CreateDistantVisualBlock(
                visuals,
                "Distant Floating Block B",
                new Vector3(24f, -3.4f, 72f),
                new Vector3(12f, 2.4f, 9f));
            CreateDistantVisualBlock(
                visuals,
                "Distant Floating Block C",
                new Vector3(-18f, -2.6f, 100f),
                new Vector3(8f, 1.8f, 7f));
        }

        private static void CreateDistantVisualBlock(
            MovementLabVisualProfile visuals,
            string objectName,
            Vector3 position,
            Vector3 scale)
        {
            var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = objectName;
            block.transform.position = position;
            block.transform.localScale = scale;
            block.GetComponent<Renderer>().sharedMaterial =
                visuals.MaterialFor(MovementLabMaterialRole.Underside);
            Destroy(block.GetComponent<Collider>());
        }

        private void CreateCourse(CheckpointService checkpoints)
        {
            CreateBlock("Start Platform", new Vector3(0f, 0f, 0f), new Vector3(8f, 0.6f, 10f), Standard);
            CreateLabel(BrandPresentation.PlayerFacingTitle + "\nMOVEMENT LAB", new Vector3(0f, 2.4f, 2f));
            CreateStraightInputTestSection();

            CreateBlock("Straight Running Lane", new Vector3(0f, 0f, 9f), new Vector3(4f, 0.6f, 7f), Standard);
            CreateLabel("RUNNING + SEAMS", new Vector3(0f, 1.6f, 7f));
            CreateBlock("Seam A", new Vector3(0f, 0f, 14f), new Vector3(4f, 0.6f, 3f), Standard);
            CreateBlock("Seam B", new Vector3(0f, 0.01f, 17f), new Vector3(4f, 0.6f, 3f), Standard);

            CreateLabel("STANDARD JUMPS", new Vector3(0f, 1.7f, 20f));
            CreateBlock("Jump 1", new Vector3(0f, 0f, 21f), new Vector3(3.6f, 0.6f, 3.2f), Standard);
            CreateBlock("Jump 2", new Vector3(0f, 0f, 26f), new Vector3(3.2f, 0.6f, 3f), Standard);
            CreateBlock("Jump 3", new Vector3(0f, 0f, 31.4f), new Vector3(3f, 0.6f, 2.8f), Standard);

            CreateLabel("PRECISION + GAP RANGE", new Vector3(0f, 1.7f, 35f));
            CreateBlock("Precision 1", new Vector3(1.4f, 0f, 36f), new Vector3(2.1f, 0.6f, 2.1f), Precision);
            CreateBlock("Precision 2", new Vector3(-1.3f, 0f, 40.2f), new Vector3(1.8f, 0.6f, 1.8f), Precision);
            CreateBlock("Precision 3", new Vector3(1.5f, 0.3f, 44.7f), new Vector3(1.7f, 0.6f, 1.7f), Precision);
            CreateBlock("Edge Landing", new Vector3(0f, 0f, 49.4f), new Vector3(2.2f, 0.6f, 1.25f), Precision);

            CreateLabel("TURNING JUMPS", new Vector3(1f, 1.8f, 53f));
            CreateBlock("Turn 1", new Vector3(3.8f, 0f, 53f), new Vector3(3f, 0.6f, 3f), Standard);
            CreateBlock("Turn 2", new Vector3(7.5f, 0f, 56.2f), new Vector3(3f, 0.6f, 3f), Standard);
            CreateBlock("Turn 3", new Vector3(7.5f, 0f, 61f), new Vector3(3f, 0.6f, 3f), Standard);

            CreateRestorePoint(
                checkpoints,
                "restore.movement-lab.midpoint",
                0,
                new Vector3(7.5f, 1.6f, 64f));

            CreateLabel("ASCEND / DESCEND", new Vector3(7.5f, 2.2f, 66f));
            CreateBlock("Ascend 1", new Vector3(7.5f, 0.6f, 67f), new Vector3(3f, 0.6f, 3f), Standard);
            CreateBlock("Ascend 2", new Vector3(7.5f, 1.4f, 71.4f), new Vector3(3f, 0.6f, 3f), Standard);
            CreateBlock("Descend", new Vector3(7.5f, 0.5f, 76f), new Vector3(3f, 0.6f, 3f), Standard);

            CreateLabel("HEAD CLEARANCE", new Vector3(7.5f, 2.2f, 79f));
            CreateBlock("Head Clearance Floor", new Vector3(7.5f, 0f, 81f), new Vector3(3.4f, 0.6f, 5f), Precision);
            CreateBlock("Head Clearance Roof", new Vector3(7.5f, 2.65f, 81f), new Vector3(3.4f, 0.35f, 2.5f), Moving);

            CreateLabel("MOVING PLATFORM", new Vector3(7.5f, 2f, 86f));
            var moving = CreateBlock(
                "Moving Platform",
                new Vector3(4.5f, 0f, 89f),
                new Vector3(3f, 0.6f, 3f),
                Moving);
            var motion = moving.AddComponent<MovingPlatformMotion>();
            motion.Configure(new Vector3(6f, 0f, 0f), 4.5f);
            _movingPlatforms.Add(motion);
            CreateBlock("Moving Platform Exit", new Vector3(10.5f, 0f, 94f), new Vector3(4f, 0.6f, 4f), Standard);

            CreateLabel("AIR CONTROL", new Vector3(8f, 2f, 98f));
            CreateBlock("Air Control Left", new Vector3(7.5f, 0f, 99f), new Vector3(2.2f, 0.6f, 2.2f), Precision);
            CreateBlock("Air Control Right", new Vector3(10.5f, 0f, 103.5f), new Vector3(2.2f, 0.6f, 2.2f), Precision);
            CreateBlock("Finish Approach", new Vector3(8f, 0f, 108f), new Vector3(4f, 0.6f, 4f), Standard);

            CreateMaximumJumpLane();
            CreateJumpCalibrationLane();
            CreateMomentumLabSection();
            CreateMovementLockLabSection(checkpoints);
        }

        private void CreateStraightInputTestSection()
        {
            const float laneX = 11.5f;
            const float startZ = 2.5f;
            const float laneCenterZ = 18.5f;
            const float laneLength = 34f;

            CreateLabel("STRAIGHT INPUT TEST", new Vector3(laneX, 1.7f, 0.5f));
            CreateBlock(
                "Straight Test Connector",
                new Vector3(7f, 0f, 0f),
                new Vector3(6f, 0.6f, 3f),
                MovementLabMaterialRole.StraightTestPlatform);
            CreateBlock(
                "Straight Input Test Lane",
                new Vector3(laneX, 0f, laneCenterZ),
                new Vector3(2.4f, 0.55f, laneLength),
                MovementLabMaterialRole.StraightTestPlatform);

            Destroy(CreateBlock(
                "Straight Test Center Line",
                new Vector3(laneX, 0.34f, laneCenterZ),
                new Vector3(0.08f, 0.08f, laneLength - 1f),
                MovementLabMaterialRole.CenterLine).GetComponent<Collider>());
            Destroy(CreateBlock(
                "Straight Test Left Boundary",
                new Vector3(laneX - 1.12f, 0.35f, laneCenterZ),
                new Vector3(0.08f, 0.08f, laneLength),
                MovementLabMaterialRole.BoundaryLine).GetComponent<Collider>());
            Destroy(CreateBlock(
                "Straight Test Right Boundary",
                new Vector3(laneX + 1.12f, 0.35f, laneCenterZ),
                new Vector3(0.08f, 0.08f, laneLength),
                MovementLabMaterialRole.BoundaryLine).GetComponent<Collider>());

            CreateStraightMarker("Straight Test Start Line", "START", laneX, startZ);
            CreateStraightMarker("Straight Test 10m Marker", "10m", laneX, startZ + 10f);
            CreateStraightMarker("Straight Test 20m Marker", "20m", laneX, startZ + 20f);
            CreateStraightMarker("Straight Test 30m Marker", "30m", laneX, startZ + 30f);

            var recorderObject = new GameObject(
                "Straight Input Test Recorder",
                typeof(BoxCollider),
                typeof(StraightInputTestRecorder));
            recorderObject.transform.position = new Vector3(laneX, 1f, laneCenterZ);
            var trigger = recorderObject.GetComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(2.8f, 3f, laneLength - 1f);
            recorderObject.GetComponent<StraightInputTestRecorder>().Initialize(laneX);
        }

        private void CreateStraightMarker(
            string blockName,
            string label,
            float laneX,
            float z)
        {
            Destroy(CreateBlock(
                blockName,
                new Vector3(laneX, 0.38f, z),
                new Vector3(2.25f, 0.08f, 0.12f),
                MovementLabMaterialRole.MarkerLine).GetComponent<Collider>());
            CreateLabel(label, new Vector3(laneX + 1.8f, 1.05f, z));
        }

        private void CreateMaximumJumpLane()
        {
            CreateLabel("MAX JUMP MEASUREMENT", new Vector3(-9f, 1.7f, 14f));
            CreateBlock("Measurement Branch A", new Vector3(-6f, 0f, 4f), new Vector3(4f, 0.6f, 4f), Standard);
            CreateBlock("Measurement Branch B", new Vector3(-9f, 0f, 8.5f), new Vector3(4f, 0.6f, 5f), Standard);
            CreateBlock("Measurement Start", new Vector3(-9f, 0f, 17f), new Vector3(4f, 0.6f, 8f), Standard);
            for (var index = 0; index < 5; index++)
            {
                var marker = CreateBlock(
                    $"Distance Marker {index + 1}",
                    new Vector3(-9f, -0.45f, 22f + index),
                    new Vector3(3.6f, 0.3f, 0.22f),
                    Precision);
                Destroy(marker.GetComponent<Collider>());
            }

            CreateBlock("Measurement Landing", new Vector3(-9f, 0f, 30f), new Vector3(5f, 0.6f, 7f), Standard);
        }

        private void CreateJumpCalibrationLane()
        {
            const float baseZ = 114f;
            CreateLabel("JUMP CALIBRATION LANE", new Vector3(-24f, 2.2f, baseZ - 6f));
            CreateBlock("Jump Calibration Hub", new Vector3(-18f, 0f, baseZ - 6f), new Vector3(10f, 0.6f, 5f), Standard);

            CreateLabel("A STANDING\nSHORT GAP", new Vector3(-30f, 1.6f, baseZ - 2.2f));
            CreateBlock("A Standing Jump Start", new Vector3(-30f, 0f, baseZ), new Vector3(3f, 0.6f, 3f), Standard);
            CreateBlock("A Standing Short Gap Target", new Vector3(-30f, 0f, baseZ + 4.2f), new Vector3(3f, 0.6f, 3f), Precision);
            CreateCalibrationMarker("A 3m Marker", "3m", -27.8f, baseZ + 3f);
            CreateCalibrationMarker("A 4m Marker", "4m", -27.8f, baseZ + 4f);

            CreateLabel("B BASE SPEED\nNORMAL GAP", new Vector3(-24f, 1.6f, baseZ - 1.7f));
            CreateBlock("B Base Speed Runway", new Vector3(-24f, 0f, baseZ + 2.5f), new Vector3(3f, 0.6f, 7f), Standard);
            CreateBlock("B Normal Gap Target", new Vector3(-24f, 0f, baseZ + 9.5f), new Vector3(3.2f, 0.6f, 3.2f), Precision);
            CreateCalibrationMarker("B 4m Marker", "4m", -21.8f, baseZ + 7f);
            CreateCalibrationMarker("B 5m Marker", "5m", -21.8f, baseZ + 8f);
            CreateCalibrationMarker("B 6m Marker", "6m", -21.8f, baseZ + 9f);

            CreateLabel("C DIAGONAL", new Vector3(-18f, 1.6f, baseZ - 1.7f));
            CreateBlock("C Diagonal Start", new Vector3(-18f, 0f, baseZ + 2f), new Vector3(3f, 0.6f, 5f), Standard);
            CreateBlock("C Diagonal Target", new Vector3(-15.6f, 0f, baseZ + 8.7f), new Vector3(3f, 0.6f, 3f), Precision);

            CreateLabel("D RUNNING\nLONG GAP", new Vector3(-33f, 1.6f, baseZ + 11.5f));
            CreateBlock("D Running Runway", new Vector3(-33f, 0f, baseZ + 18f), new Vector3(3f, 0.6f, 10f), Standard);
            CreateBlock("D Long Gap Target", new Vector3(-33f, 0f, baseZ + 28f), new Vector3(3.4f, 0.6f, 3.4f), Precision);
            CreateCalibrationMarker("D 5m Marker", "5m", -30.8f, baseZ + 24f);
            CreateCalibrationMarker("D 6m Marker", "6m", -30.8f, baseZ + 25f);
            CreateCalibrationMarker("D 7m Marker", "7m", -30.8f, baseZ + 26f);

            CreateLabel("E BHOP SPEED", new Vector3(-27f, 1.6f, baseZ + 12.2f));
            CreateBlock("E Bhop Start", new Vector3(-27f, 0f, baseZ + 17f), new Vector3(3f, 0.6f, 3f), Standard);
            CreateBlock("E Bhop Hop 1", new Vector3(-27f, 0f, baseZ + 22.2f), new Vector3(3f, 0.6f, 3f), Precision);
            CreateBlock("E Bhop Hop 2", new Vector3(-27f, 0f, baseZ + 27.6f), new Vector3(3f, 0.6f, 3f), Precision);
            CreateBlock("E Bhop Exit", new Vector3(-27f, 0f, baseZ + 34f), new Vector3(4.5f, 0.6f, 4f), Standard);

            CreateLabel("F ADVANCED\nMOMENTUM", new Vector3(-21f, 1.6f, baseZ + 12.2f));
            CreateBlock("F Momentum Runway", new Vector3(-21f, 0f, baseZ + 18f), new Vector3(3f, 0.6f, 12f), Standard);
            CreateBlock("F Advanced Gap Target", new Vector3(-21f, 0f, baseZ + 31.5f), new Vector3(4f, 0.6f, 4f), Precision);
            CreateCalibrationMarker("F 7m Marker", "7m", -18.8f, baseZ + 27f);
            CreateCalibrationMarker("F 8m Marker", "8m", -18.8f, baseZ + 28f);
            CreateCalibrationMarker("F 9m Marker", "9m", -18.8f, baseZ + 29f);

            CreateLabel("G AIR STRAFE", new Vector3(-15f, 1.6f, baseZ + 12.2f));
            CreateBlock("G Air Strafe Start", new Vector3(-15f, 0f, baseZ + 18f), new Vector3(3f, 0.6f, 7f), Standard);
            CreateBlock("G Air Strafe Offset Target", new Vector3(-11.5f, 0f, baseZ + 27f), new Vector3(3.4f, 0.6f, 3.4f), Precision);
            CreateBlock("G Air Strafe Recovery", new Vector3(-15f, 0f, baseZ + 33f), new Vector3(4f, 0.6f, 4f), Standard);

            CreateLabel("H LOWER", new Vector3(-30f, 1.6f, baseZ + 36.5f));
            CreateBlock("H Lower Jump Start", new Vector3(-30f, 0.4f, baseZ + 41f), new Vector3(3f, 0.6f, 4f), Standard);
            CreateBlock("H Lower Target", new Vector3(-30f, -0.45f, baseZ + 48f), new Vector3(3.4f, 0.6f, 3.4f), Precision);

            CreateLabel("I HIGHER", new Vector3(-24f, 1.9f, baseZ + 36.5f));
            CreateBlock("I Higher Jump Start", new Vector3(-24f, 0f, baseZ + 41f), new Vector3(3f, 0.6f, 4f), Standard);
            CreateBlock("I Higher Target", new Vector3(-24f, 0.75f, baseZ + 47.2f), new Vector3(3.4f, 0.6f, 3.4f), Precision);
        }

        private void CreateCalibrationMarker(string objectName, string label, float x, float z)
        {
            Destroy(CreateBlock(
                objectName,
                new Vector3(x, 0.38f, z),
                new Vector3(1.6f, 0.08f, 0.12f),
                MovementLabMaterialRole.MarkerLine).GetComponent<Collider>());
            CreateLabel(label, new Vector3(x + 1.1f, 1.05f, z));
        }

        private void CreateMomentumLabSection()
        {
            const float z = 122f;
            CreateLabel("MOMENTUM LAB", new Vector3(0f, 2.2f, z - 5f));
            CreateBlock("Momentum Lab Connector", new Vector3(8f, 0f, 116f), new Vector3(4f, 0.6f, 8f), Standard);
            CreateBlock("Lane A Baseline Run", new Vector3(-12f, 0f, z), new Vector3(3f, 0.6f, 14f), Standard);
            CreateLabel("A BASE RUN", new Vector3(-12f, 1.5f, z - 5f));
            CreateBlock("Lane B Tap Jump Start", new Vector3(-6f, 0f, z - 3f), new Vector3(3f, 0.6f, 4f), Standard);
            CreateBlock("Lane B Tap Jump Landing", new Vector3(-6f, 0f, z + 5.5f), new Vector3(3f, 0.6f, 5f), Standard);
            CreateLabel("B REPEATED TAPS", new Vector3(-6f, 1.5f, z - 5f));
            CreateBlock("Lane C Bhop Timing 1", new Vector3(0f, 0f, z - 5f), new Vector3(3f, 0.6f, 3f), Precision);
            CreateBlock("Lane C Bhop Timing 2", new Vector3(0f, 0f, z), new Vector3(3f, 0.6f, 3f), Precision);
            CreateBlock("Lane C Bhop Timing 3", new Vector3(0f, 0f, z + 5.2f), new Vector3(3f, 0.6f, 3f), Precision);
            CreateLabel("C BHOP TIMING", new Vector3(0f, 1.5f, z - 7f));
            CreateBlock("Lane D Air Strafe Start", new Vector3(6f, 0f, z - 4f), new Vector3(3f, 0.6f, 3f), Standard);
            CreateBlock("Lane D Air Strafe Offset", new Vector3(9f, 0f, z + 2f), new Vector3(3f, 0.6f, 3f), Precision);
            CreateBlock("Lane D Air Strafe Exit", new Vector3(6f, 0f, z + 8f), new Vector3(4f, 0.6f, 4f), Standard);
            CreateLabel("D AIR STRAFE", new Vector3(7f, 1.5f, z - 7f));
            CreateSurfRamp("Lane E Gentle Surf Left", new Vector3(14f, 0.6f, z - 2f), new Vector3(4f, 0.45f, 9f), 34f);
            CreateSurfRamp("Lane F Medium Surf Right", new Vector3(20f, 0.9f, z + 4f), new Vector3(4f, 0.45f, 10f), -46f);
            CreateBlock("Surf Exit Platform", new Vector3(17f, 0f, z + 12f), new Vector3(8f, 0.6f, 5f), Standard);
            CreateLabel("E-F SURF FOUNDATION", new Vector3(17f, 2f, z - 8f));
        }

        private void CreateMovementLockLabSection(CheckpointService checkpoints)
        {
            const float baseZ = 145f;
            CreateLabel("MOVEMENT LOCK LAB", new Vector3(15f, 2.2f, baseZ - 7f));
            CreateBlock("Movement Lock Entry", new Vector3(17f, 0f, baseZ - 4f), new Vector3(5f, 0.6f, 5f), Standard);
            CreateBlock("Flow Throttle Lane", new Vector3(17f, 0f, baseZ + 3f), new Vector3(2.4f, 0.6f, 9f), Standard);
            Destroy(CreateBlock(
                "Flow Throttle Center Line",
                new Vector3(17f, 0.38f, baseZ + 3f),
                new Vector3(0.08f, 0.08f, 8.2f),
                MovementLabMaterialRole.CenterLine).GetComponent<Collider>());
            CreateLabel("LEFT MOVE\nRIGHT LOOK", new Vector3(20.5f, 1.6f, baseZ + 1.5f));

            var spiralPoints = new[]
            {
                new Vector3(15.5f, 0f, baseZ + 9f),
                new Vector3(12.5f, 0.15f, baseZ + 12.5f),
                new Vector3(8.5f, 0.3f, baseZ + 12.5f),
                new Vector3(5.4f, 0.45f, baseZ + 9.4f),
                new Vector3(5.4f, 0.6f, baseZ + 5.4f),
                new Vector3(8.2f, 0.75f, baseZ + 2.4f),
                new Vector3(11.8f, 0.9f, baseZ + 2.4f),
                new Vector3(14.2f, 1.05f, baseZ + 5.2f),
                new Vector3(14.2f, 1.2f, baseZ + 8.5f),
                new Vector3(11.6f, 1.35f, baseZ + 10.8f),
                new Vector3(8.6f, 1.5f, baseZ + 10.8f)
            };
            for (var index = 0; index < spiralPoints.Length; index++)
            {
                CreateBlock(
                    $"Movement Lock Spiral {index + 1:00}",
                    spiralPoints[index],
                    new Vector3(2.7f, 0.6f, 2.7f),
                    index % 3 == 1 ? Precision : Standard);
            }

            CreateRestorePoint(
                checkpoints,
                "restore.movement-lab.movement-lock",
                1,
                new Vector3(8.6f, 3.1f, baseZ + 14.2f));
            CreateBlock("Movement Lock Landing Strip", new Vector3(8.6f, 1.5f, baseZ + 16.5f), new Vector3(4.5f, 0.6f, 8f), Standard);
            CreateLabel("MANUAL CAMERA\nSPIRAL CHECK", new Vector3(4.8f, 3f, baseZ + 12f));
        }

        internal PlayerRuntimeCoordinator CreatePlayer(
            Vector3 startPosition,
            MovementProfileSet profiles,
            CameraProfile cameraProfile,
            TouchInputCoordinator touchInput,
            CheckpointService checkpoints,
            string buildVersion,
            Text statusText,
            Text completionText,
            bool training = false)
        {
            var player = new GameObject(
                "Player",
                typeof(CharacterController),
                typeof(ParkourMotor),
                typeof(AudioSource),
                typeof(MovementFeedback),
                typeof(RestoreController),
                typeof(MovementSessionReporter),
                typeof(PlayerRuntimeCoordinator));
            player.layer = LayerMask.NameToLayer("Player");
            player.transform.position = startPosition;
            var character = player.GetComponent<CharacterController>();
            character.height = 1.8f;
            character.radius = 0.38f;
            character.center = new Vector3(0f, 0.9f, 0f);
            character.skinWidth = 0.045f;
            character.stepOffset = 0.32f;
            character.slopeLimit = 52f;

            var cameraObject = new GameObject(
                "First Person Camera",
                typeof(UnityEngine.Camera),
                typeof(AudioListener),
                typeof(FirstPersonCameraRig),
                typeof(FirstPersonHands));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(player.transform, false);
            cameraObject.transform.localPosition = new Vector3(0f, 1.62f, 0f);
            var camera = cameraObject.GetComponent<UnityEngine.Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = _visuals.SkyHorizon;
            camera.nearClipPlane = 0.04f;

            var motor = player.GetComponent<ParkourMotor>();
            motor.Initialize(profiles.Current);
            var cameraRig = cameraObject.GetComponent<FirstPersonCameraRig>();
            cameraRig.Initialize(player.transform, camera, cameraProfile);
            cameraObject.GetComponent<FirstPersonHands>().Initialize(motor);

            var input = new PlayerInputRouter(touchInput);
            var stats = new MovementSessionStats();
            var feedback = player.GetComponent<MovementFeedback>();
            var reporter = player.GetComponent<MovementSessionReporter>();
            reporter.Initialize(stats, input, motor, buildVersion);
            var restoreController = player.GetComponent<RestoreController>();
            restoreController.Initialize(
                motor,
                cameraRig,
                input,
                checkpoints,
                _movingPlatforms.ToArray(),
                _ =>
                {
                    stats.RecordRestore();
                    feedback.PlayRestore();
                },
                () =>
                {
                    stats.RecordFall();
                    feedback.PlayFall();
                });

            IDiagnosticsService diagnostics = null;
            if (GameServices.Current != null)
            {
                GameServices.Current.TryGet<IDiagnosticsService>(out diagnostics);
            }
            var coordinator = player.GetComponent<PlayerRuntimeCoordinator>();
            coordinator.Initialize(
                input,
                touchInput,
                profiles,
                motor,
                cameraRig,
                restoreController,
                feedback,
                stats,
                reporter,
                checkpoints,
                diagnostics);

            if (training) return coordinator;
            _hud = statusText.GetComponentInParent<Canvas>().gameObject.AddComponent<MovementLabHud>();
            _hud.Initialize(statusText, completionText, coordinator, restoreController, buildVersion);
            CreatePatchBlock(stats, reporter, feedback);
            return coordinator;
        }

        private void CreatePatchBlock(
            MovementSessionStats stats,
            MovementSessionReporter reporter,
            MovementFeedback feedback)
        {
            var patch = CreateBlock(
                "Temporary Patch Block",
                new Vector3(8f, 1.25f, 114f),
                new Vector3(2.5f, 2.5f, 2.5f),
                Patch);
            var collider = patch.GetComponent<BoxCollider>();
            collider.isTrigger = true;
            var finish = patch.AddComponent<MovementLabFinish>();
            finish.Initialize(
                stats,
                reporter,
                feedback,
                () =>
                {
                    PlayBurst(patch.transform.position, Patch);
                    _hud.ShowModuleFixed();
                });
            CreateLabel("TEMPORARY PATCH BLOCK", new Vector3(8f, 3.4f, 114f));
        }

        internal TouchInputCoordinator CreateTouchInterface(
            TouchControlLayout layout,
            out Text statusText,
            out Text completionText,
            bool training = false)
        {
            var canvasObject = new GameObject(
                "Movement Lab UI",
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(TouchInputCoordinator));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            var safeAreaObject = CreateUiObject(
                "Safe Area",
                canvasObject.transform,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero,
                typeof(SafeAreaFitter));
            var safeArea = safeAreaObject.GetComponent<RectTransform>();

            var lookObject = CreateUiObject(
                "Right Look Zone",
                safeArea,
                new Vector2(0.48f, 0.08f),
                new Vector2(1f, 0.92f),
                Vector2.zero,
                Vector2.zero,
                typeof(Image),
                typeof(TouchLookControl));
            var lookImage = lookObject.GetComponent<Image>();
            lookImage.color = new Color(0.1f, 0.75f, 1f, layout.LookZoneOpacity);
            lookObject.GetComponent<TouchLookControl>().Configure(
                lookObject.GetComponent<RectTransform>(),
                layout.LookZoneOpacity);

            var movementZoneObject = CreateUiObject(
                "Left Movement Zone",
                safeArea,
                new Vector2(0f, 0.08f),
                new Vector2(0.52f, 0.92f),
                Vector2.zero,
                Vector2.zero,
                typeof(Image),
                typeof(MovementJoystickControl));
            var movementZoneImage = movementZoneObject.GetComponent<Image>();
            movementZoneImage.color = new Color(1f, 0.84f, 0.16f, 0.001f);

            var joystickObject = CreateUiObject(
                "Movement Floating Origin",
                safeArea,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(layout.JoystickSize, layout.JoystickSize),
                typeof(Image));
            var joystickRect = joystickObject.GetComponent<RectTransform>();
            joystickRect.pivot = new Vector2(0.5f, 0.5f);
            var joystickImage = joystickObject.GetComponent<Image>();
            joystickImage.sprite = GetCircleRingSprite();
            joystickImage.color = new Color(1f, 1f, 1f, layout.JoystickRingOpacity);
            joystickImage.raycastTarget = false;
            var knobObject = CreateUiObject(
                "Knob",
                joystickRect,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                Vector2.one * layout.JoystickSize * 0.42f,
                typeof(Image));
            var knobImage = knobObject.GetComponent<Image>();
            knobImage.sprite = GetFilledCircleSprite();
            knobImage.color = new Color(0.86f, 0.94f, 1f, layout.JoystickKnobOpacity);
            knobImage.raycastTarget = false;
            var joystick = movementZoneObject.GetComponent<MovementJoystickControl>();
            joystick.Configure(
                movementZoneObject.GetComponent<RectTransform>(),
                safeArea,
                joystickRect,
                knobObject.GetComponent<RectTransform>(),
                layout.JoystickDeadZone,
                layout.FloatingJoystick,
                layout.HorizontalEdgeComfortMargin,
                layout.VerticalEdgeComfortMargin,
                layout.JoystickRingOpacity,
                layout.JoystickKnobOpacity);

            var jumpObject = CreateUiObject(
                "Right Jump Zone",
                safeArea,
                new Vector2(0.52f, 0.08f),
                new Vector2(1f, 0.92f),
                Vector2.zero,
                Vector2.zero,
                typeof(Image),
                typeof(JumpTouchControl));
            var jumpRect = jumpObject.GetComponent<RectTransform>();
            jumpRect.pivot = new Vector2(0.5f, 0.5f);
            var jumpImage = jumpObject.GetComponent<Image>();
            jumpImage.sprite = GetFilledCircleSprite();
            jumpImage.color = new Color(0.12f, 0.78f, 0.94f, layout.JumpButtonOpacity);
            jumpObject.GetComponent<JumpTouchControl>().Configure(layout.JumpButtonOpacity);
            CreateJumpGlyph(jumpRect);

            statusText = CreateText(
                "Movement Status",
                safeArea,
                string.Empty,
                22,
                TextAnchor.UpperCenter,
                new Vector2(0.41f, 1f),
                new Vector2(0.74f, 1f),
                new Vector2(12f, -150f),
                new Vector2(-12f, -24f));
            completionText = CreateText(
                "Completion Message",
                safeArea,
                string.Empty,
                44,
                TextAnchor.MiddleCenter,
                new Vector2(0.28f, 0.72f),
                new Vector2(0.72f, 0.92f),
                Vector2.zero,
                Vector2.zero);
            completionText.enabled = false;

            var touchCoordinator = canvasObject.GetComponent<TouchInputCoordinator>();
            if (!training)
            {
            CreateDevelopmentButton(
                safeArea,
                "CTRL",
                new Vector2(-906f, -28f),
                touchCoordinator.CycleControlProfile);
            CreateDevelopmentButton(
                safeArea,
                "JUMP",
                new Vector2(-730f, -28f),
                touchCoordinator.CycleJumpMode);
            CreateDevelopmentButton(
                safeArea,
                "DIAG",
                new Vector2(-554f, -28f),
                touchCoordinator.PressToggleDiagnostics);
            CreateDevelopmentButton(
                safeArea,
                "PROFILE",
                new Vector2(-378f, -28f),
                touchCoordinator.PressSwitchProfile);
            CreateDevelopmentButton(
                safeArea,
                "CAMERA FX",
                new Vector2(-202f, -28f),
                touchCoordinator.PressToggleCameraEffects);
            CreateDevelopmentButton(
                safeArea,
                "RESTORE",
                new Vector2(-26f, -28f),
                touchCoordinator.PressRestart);
            CreateDevelopmentButton(
                safeArea,
                "LOOK",
                new Vector2(-378f, -88f),
                touchCoordinator.CycleCameraSensitivity);
            CreateDevelopmentButton(
                safeArea,
                "MOVE",
                new Vector2(-202f, -88f),
                touchCoordinator.CycleMovementSensitivity);

            }
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                new GameObject(
                    "Event System",
                    typeof(EventSystem),
                    typeof(InputSystemUIInputModule));
            }

            touchCoordinator.Initialize(
                layout,
                joystick,
                lookObject.GetComponent<TouchLookControl>(),
                jumpObject.GetComponent<JumpTouchControl>());
            return touchCoordinator;
        }

        private void CreateRestorePoint(
            CheckpointService checkpoints,
            string id,
            int order,
            Vector3 position)
        {
            CreateBlock(
                "Restore Point Floor",
                new Vector3(position.x, 0f, position.z),
                new Vector3(4.5f, 0.6f, 4f),
                Standard);
            var gate = CreateBlock(
                "Restore Point",
                position,
                new Vector3(4.5f, 3f, 0.35f),
                Restore);
            var restore = gate.AddComponent<RestorePoint>();
            restore.Initialize(
                checkpoints,
                id,
                order,
                new Vector3(position.x, 0.35f, position.z + 1.25f),
                () =>
                {
                    var player = FindAnyObjectByType<MovementFeedback>();
                    player?.PlayRestorePoint();
                    PlayBurst(position, Restore);
                });
            CreateLabel("RESTORE POINT", position + Vector3.up * 2.1f);
        }

        private GameObject CreateBlock(
            string blockName,
            Vector3 position,
            Vector3 scale,
            MovementLabMaterialRole role)
        {
            var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = blockName;
            block.transform.position = position;
            block.transform.localScale = scale;
            block.layer = LayerMask.NameToLayer("Ground");
            block.GetComponent<Renderer>().sharedMaterial = _visuals.MaterialFor(role);
            AddReadabilityTrim(block, scale, role);
            return block;
        }

        private GameObject CreateSurfRamp(
            string blockName,
            Vector3 position,
            Vector3 scale,
            float angleDegrees)
        {
            var ramp = CreateBlock(blockName, position, scale, Surf);
            ramp.transform.rotation = Quaternion.Euler(angleDegrees, 0f, 0f);
            ramp.AddComponent<SurfSurface>();
            CreateVisualPlate(
                blockName + " Direction Line",
                position + ramp.transform.forward * (scale.z * 0.32f) + ramp.transform.up * (scale.y * 0.7f),
                new Vector3(scale.x * 0.68f, 0.06f, 0.12f),
                MovementLabMaterialRole.MarkerLine);
            return ramp;
        }

        private void AddReadabilityTrim(
            GameObject block,
            Vector3 scale,
            MovementLabMaterialRole role)
        {
            if (role == MovementLabMaterialRole.CenterLine
                || role == MovementLabMaterialRole.BoundaryLine
                || role == MovementLabMaterialRole.MarkerLine
                || role == MovementLabMaterialRole.NullSpace
                || scale.x < 1.2f
                || scale.z < 1.2f)
            {
                return;
            }

            CreateVisualPlate(
                block.name + " Underside",
                block.transform.position + Vector3.down * (scale.y * 0.5f + 0.045f),
                new Vector3(scale.x * 1.02f, 0.09f, scale.z * 1.02f),
                MovementLabMaterialRole.Underside);

            var topY = block.transform.position.y + scale.y * 0.5f + 0.035f;
            var edgeThickness = 0.08f;
            CreateVisualPlate(
                block.name + " Left Edge",
                new Vector3(block.transform.position.x - scale.x * 0.5f, topY, block.transform.position.z),
                new Vector3(edgeThickness, 0.06f, scale.z),
                MovementLabMaterialRole.BoundaryLine);
            CreateVisualPlate(
                block.name + " Right Edge",
                new Vector3(block.transform.position.x + scale.x * 0.5f, topY, block.transform.position.z),
                new Vector3(edgeThickness, 0.06f, scale.z),
                MovementLabMaterialRole.BoundaryLine);
        }

        private void CreateVisualPlate(
            string objectName,
            Vector3 position,
            Vector3 scale,
            MovementLabMaterialRole role)
        {
            var plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plate.name = objectName;
            plate.transform.position = position;
            plate.transform.localScale = scale;
            plate.GetComponent<Renderer>().sharedMaterial = _visuals.MaterialFor(role);
            Destroy(plate.GetComponent<Collider>());
        }

        private static void CreateLabel(string label, Vector3 position)
        {
            var labelObject = new GameObject(label, typeof(TextMesh), typeof(WorldSpaceBillboardLabel));
            labelObject.transform.position = position;
            labelObject.transform.rotation = Quaternion.identity;
            var text = labelObject.GetComponent<TextMesh>();
            text.text = label;
            text.alignment = TextAlignment.Center;
            text.anchor = TextAnchor.MiddleCenter;
            text.fontSize = 42;
            text.characterSize = 0.052f;
            text.color = new Color(1f, 1f, 1f, 0.82f);
        }

        private void PlayBurst(Vector3 position, MovementLabMaterialRole role)
        {
            var particleObject = new GameObject("Restore Point Burst", typeof(ParticleSystem));
            particleObject.transform.position = position;
            var particles = particleObject.GetComponent<ParticleSystem>();
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = particles.main;
            main.duration = 0.45f;
            main.startLifetime = 0.5f;
            main.startSpeed = 4f;
            main.startSize = 0.14f;
            main.startColor = _visuals.ColorFor(role);
            main.loop = false;
            var emission = particles.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 24) });
            var renderer = particleObject.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = VisualMaterialUtility.CreateRuntimeMaterial(
                "RB Particle Burst",
                _visuals.ColorFor(role),
                VisualMaterialUtility.ResolveParticleShader());
            particles.Play();
            Destroy(particleObject, 1.2f);
        }

        private static GameObject CreateUiObject(
            string objectName,
            Transform parent,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 sizeDelta,
            params Type[] extraComponents)
        {
            var types = new List<Type> { typeof(RectTransform) };
            types.AddRange(extraComponents);
            var gameObject = new GameObject(objectName, types.ToArray());
            gameObject.transform.SetParent(parent, false);
            var rect = gameObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
            return gameObject;
        }

        private static Text CreateText(
            string objectName,
            Transform parent,
            string value,
            int fontSize,
            TextAnchor alignment,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMax)
        {
            var textObject = CreateUiObject(
                objectName,
                parent,
                anchorMin,
                anchorMax,
                Vector2.zero,
                Vector2.zero,
                typeof(Text));
            var rect = textObject.GetComponent<RectTransform>();
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            var text = textObject.GetComponent<Text>();
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }

        private static void CreateJumpGlyph(RectTransform parent)
        {
            CreateGlyphBar(parent, "Jump Glyph Left A", new Vector2(-18f, 20f), -36f);
            CreateGlyphBar(parent, "Jump Glyph Right A", new Vector2(18f, 20f), 36f);
            CreateGlyphBar(parent, "Jump Glyph Left B", new Vector2(-18f, -18f), -36f);
            CreateGlyphBar(parent, "Jump Glyph Right B", new Vector2(18f, -18f), 36f);
        }

        private static void CreateGlyphBar(
            Transform parent,
            string objectName,
            Vector2 anchoredPosition,
            float rotation)
        {
            var bar = CreateUiObject(
                objectName,
                parent,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                anchoredPosition,
                new Vector2(68f, 16f),
                typeof(Image));
            bar.transform.localRotation = Quaternion.Euler(0f, 0f, rotation);
            var image = bar.GetComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0.82f);
            image.raycastTarget = false;
        }

        private static Sprite GetCircleRingSprite()
        {
            _circleRingSprite ??= CreateCircleSprite("RB Touch Ring", true);
            return _circleRingSprite;
        }

        private static Sprite GetFilledCircleSprite()
        {
            _filledCircleSprite ??= CreateCircleSprite("RB Touch Circle", false);
            return _filledCircleSprite;
        }

        private static Sprite CreateCircleSprite(string spriteName, bool ring)
        {
            const int size = 96;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = spriteName + " Texture",
                hideFlags = HideFlags.HideAndDontSave
            };
            var center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            var outer = size * 0.46f;
            var inner = ring ? size * 0.38f : 0f;
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var distance = Vector2.Distance(new Vector2(x, y), center);
                    var alpha = distance <= outer && distance >= inner ? 1f : 0f;
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply();
            var sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f),
                size);
            sprite.name = spriteName;
            return sprite;
        }

        private static void CreateDevelopmentButton(
            Transform parent,
            string label,
            Vector2 anchoredPosition,
            UnityEngine.Events.UnityAction onPressed)
        {
            var buttonObject = CreateUiObject(
                $"{label} Button",
                parent,
                Vector2.one,
                Vector2.one,
                anchoredPosition,
                new Vector2(158f, 58f),
                typeof(Image),
                typeof(Button));
            var rect = buttonObject.GetComponent<RectTransform>();
            rect.pivot = Vector2.one;
            buttonObject.GetComponent<Image>().color = new Color(0.04f, 0.12f, 0.22f, 0.72f);
            buttonObject.GetComponent<Button>().onClick.AddListener(onPressed);
            CreateText(
                $"{label} Label",
                rect,
                label,
                20,
                TextAnchor.MiddleCenter,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero);
        }
    }
}
