using Avoidance.Core.Services;
using Avoidance.Gameplay.Blocks;
using Avoidance.Gameplay.Camera;
using Avoidance.Gameplay.Checkpoints;
using Avoidance.Gameplay.Player;
using Avoidance.Gameplay.Respawn;
using Avoidance.Gameplay.Visuals;
using Avoidance.Input;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

namespace Avoidance.Tests.EditMode
{
    public sealed class MovementStateTests
    {
        [Test]
        public void JumpBuffer_RemainsEligibleUntilGrounded()
        {
            var state = new JumpWindowState();
            state.Tick(0.01f, false, true, 0.1f, 0.15f);
            Assert.That(state.TryConsumeJump(), Is.False);
            Assert.That(state.IsJumpBuffered, Is.True);

            state.Tick(0.05f, true, false, 0.1f, 0.15f);

            Assert.That(state.TryConsumeJump(), Is.True);
            Assert.That(state.IsJumpBuffered, Is.False);
        }

        [Test]
        public void CoyoteTime_IsEligibleBrieflyAfterLeavingGround()
        {
            var state = new JumpWindowState();
            state.Tick(0.01f, true, false, 0.12f, 0.1f);
            state.Tick(0.08f, false, true, 0.12f, 0.1f);

            Assert.That(state.IsCoyoteEligible, Is.True);
            Assert.That(state.TryConsumeJump(), Is.True);
        }

        [Test]
        public void CoyoteTime_Expires()
        {
            var state = new JumpWindowState();
            state.Tick(0.01f, true, false, 0.1f, 0.1f);
            state.Tick(0.11f, false, true, 0.1f, 0.1f);

            Assert.That(state.IsCoyoteEligible, Is.False);
            Assert.That(state.TryConsumeJump(), Is.False);
        }

        [Test]
        public void CheckpointService_KeepsLatestOrderedRestorePoint()
        {
            var service = new CheckpointService();
            service.SetStart(Vector3.zero, Quaternion.identity);

            Assert.That(
                service.TryActivate("restore.first", 0, Vector3.one, Quaternion.identity),
                Is.True);
            Assert.That(
                service.TryActivate("restore.old", 0, Vector3.right, Quaternion.identity),
                Is.False);

            Assert.That(service.CurrentCheckpointId, Is.EqualTo("restore.first"));
            Assert.That(service.Current.Position, Is.EqualTo(Vector3.one));
        }

        [Test]
        public void VoidRule_DetectsNullSpaceBoundary()
        {
            Assert.That(VoidRule.IsInNullSpace(-12.01f, -12f), Is.True);
            Assert.That(VoidRule.IsInNullSpace(-12f, -12f), Is.False);
        }

        [Test]
        public void InputSourceSelector_SwitchesBetweenEditorAndTouch()
        {
            Assert.That(
                InputSourceSelector.Resolve(PlayerInputMode.Auto, false, false),
                Is.EqualTo(PlayerInputMode.Editor));
            Assert.That(
                InputSourceSelector.Resolve(PlayerInputMode.Auto, true, false),
                Is.EqualTo(PlayerInputMode.Touch));
            Assert.That(
                InputSourceSelector.Resolve(PlayerInputMode.Editor, true, true),
                Is.EqualTo(PlayerInputMode.Editor));
        }

        [Test]
        public void MovementProfileSet_CyclesAndSelectsByStableId()
        {
            var first = MovementProfile.CreateRuntimeDefault();
            first.ConfigureForTests("movement.first", "First", 0.1f, 0.1f);
            var second = MovementProfile.CreateRuntimeDefault();
            second.ConfigureForTests("movement.second", "Second", 0.1f, 0.1f);
            var set = new MovementProfileSet(new[] { first, second });

            Assert.That(set.Next(), Is.SameAs(second));
            Assert.That(set.Select("movement.first"), Is.True);
            Assert.That(set.Current, Is.SameAs(first));
        }

        [Test]
        public void DefaultProfile_DerivesPredictableJumpVelocityAndDistance()
        {
            var profile = MovementProfile.CreateRuntimeDefault();
            var expectedGravity = 2f * profile.JumpHeight
                / (profile.TimeToApex * profile.TimeToApex);
            var expectedVelocity = expectedGravity * profile.TimeToApex;
            var previousMovementLockEstimate = 6.197f;

            Assert.That(profile.JumpGravity, Is.EqualTo(expectedGravity).Within(0.001f));
            Assert.That(profile.FallGravity, Is.GreaterThan(profile.JumpGravity));
            Assert.That(profile.JumpVelocity, Is.EqualTo(expectedVelocity).Within(0.001f));
            Assert.That(profile.TimeToApex, Is.EqualTo(0.335f).Within(0.001f));
            Assert.That(profile.EstimatedAirtime, Is.InRange(0.6f, 0.64f));
            Assert.That(profile.EstimatedMaximumJumpDistance, Is.InRange(4.25f, 4.45f));
            Assert.That(profile.EstimatedMaximumJumpDistance, Is.LessThan(previousMovementLockEstimate * 0.75f));

            Object.DestroyImmediate(profile);
        }

        [Test]
        public void TakeoffMomentumRetention_PreservesEarnedMomentumMoreThanNormalJump()
        {
            var profile = MovementProfile.CreateRuntimeDefault();

            var normal = profile.TakeoffMomentumRetentionForSpeed(profile.BaseRunSpeed);
            var advanced = profile.TakeoffMomentumRetentionForSpeed(profile.SoftMomentumLimit);

            Assert.That(normal, Is.EqualTo(profile.BaseTakeoffMomentumRetention).Within(0.001f));
            Assert.That(advanced, Is.GreaterThan(normal));
            Assert.That(advanced, Is.EqualTo(profile.HighMomentumTakeoffRetention).Within(0.001f));

            Object.DestroyImmediate(profile);
        }

        [Test]
        public void TouchOwnership_KeepsMovementLookAndJumpIndependent()
        {
            var ownership = new TouchOwnershipRegistry();

            Assert.That(ownership.TryClaim(TouchControlRole.Movement, 1), Is.True);
            Assert.That(ownership.TryClaim(TouchControlRole.Look, 2), Is.True);
            Assert.That(ownership.TryClaim(TouchControlRole.Jump, 3), Is.True);
            Assert.That(ownership.TryClaim(TouchControlRole.Jump, 2), Is.False);
            Assert.That(ownership.GetOwner(TouchControlRole.Movement), Is.EqualTo(1));
            Assert.That(ownership.GetOwner(TouchControlRole.Look), Is.EqualTo(2));
            Assert.That(ownership.GetOwner(TouchControlRole.Jump), Is.EqualTo(3));
        }

        [Test]
        public void TouchOwnership_ReleasingNonOwnerDoesNotClearOwner()
        {
            var ownership = new TouchOwnershipRegistry();
            Assert.That(ownership.TryClaim(TouchControlRole.Movement, 1), Is.True);

            ownership.Release(TouchControlRole.Movement, 2);

            Assert.That(ownership.GetOwner(TouchControlRole.Movement), Is.EqualTo(1));
        }

        [Test]
        public void TouchStickDeadZone_RemapsRemainingRangeLinearly()
        {
            Assert.That(
                TouchStickMath.ApplyDeadZone(new Vector2(0.1f, 0f), 0.12f),
                Is.EqualTo(Vector2.zero));
            Assert.That(
                TouchStickMath.ApplyDeadZone(new Vector2(0.56f, 0f), 0.12f).x,
                Is.EqualTo(0.5f).Within(0.001f));
            Assert.That(
                TouchStickMath.ApplyDeadZone(new Vector2(2f, 0f), 0.12f).x,
                Is.EqualTo(1f).Within(0.001f));
        }

        [Test]
        public void TouchStickMath_UsesRectCenterAsNeutral()
        {
            var rect = new Rect(0f, 0f, 280f, 280f);

            Assert.That(TouchStickMath.NormalizeLocalPoint(new Vector2(140f, 140f), rect), Is.EqualTo(Vector2.zero));
            Assert.That(TouchStickMath.NormalizeLocalPoint(new Vector2(140f, 280f), rect), Is.EqualTo(Vector2.up));
            Assert.That(TouchStickMath.NormalizeLocalPoint(new Vector2(140f, 0f), rect), Is.EqualTo(Vector2.down));
            Assert.That(TouchStickMath.NormalizeLocalPoint(new Vector2(0f, 140f), rect), Is.EqualTo(Vector2.left));
            Assert.That(TouchStickMath.NormalizeLocalPoint(new Vector2(280f, 140f), rect), Is.EqualTo(Vector2.right));
        }

        [Test]
        public void TouchLayout_DefaultsToResponsiveClassicManualMode()
        {
            var layout = TouchControlLayout.CreateRuntimeDefault();
            var profile = layout.CreateRuntimeProfile(
                layout.DefaultControlProfile,
                layout.DefaultJumpMode,
                layout.DefaultCameraSensitivity,
                layout.DefaultMovementSensitivity);

            Assert.That(profile.ControlProfile, Is.EqualTo(TouchControlProfileKind.LeftMoveRightLookTapJump));
            Assert.That(profile.JumpMode, Is.EqualTo(TouchJumpMode.RightTap));
            Assert.That(profile.MovementSensitivity, Is.EqualTo(TouchMovementSensitivityPreset.Fast));
            Assert.That(profile.MovementOnRight, Is.False);
            Assert.That(profile.EnableRightFlick, Is.False);
            Assert.That(profile.EnableRightTap, Is.True);
            Assert.That(profile.EnableRightJumpZone, Is.False);
            Assert.That(profile.EnableTapAnywhere, Is.False);
            Assert.That(profile.EnableFixedButton, Is.False);
            Assert.That(profile.ManualLookEnabled, Is.True);
            Assert.That(profile.FlowSteeringEnabled, Is.False);
            Assert.That(profile.AutoCameraProfile, Is.EqualTo(AutoCameraProfileKind.Balanced));
            Assert.That(profile.MovementRestCenter.x, Is.InRange(0.3f, 0.38f));
            Assert.That(profile.LookRestCenter.x, Is.GreaterThan(0.5f));

            Object.DestroyImmediate(layout);
        }

        [Test]
        public void TouchLayout_SmartParkourEasyModeRemainsAvailable()
        {
            var layout = TouchControlLayout.CreateRuntimeDefault();
            var profile = layout.CreateRuntimeProfile(
                TouchControlProfileKind.FlowSteerAutoBalanced,
                TouchJumpMode.RightJumpZone,
                layout.DefaultCameraSensitivity,
                layout.DefaultMovementSensitivity);

            Assert.That(profile.ControlProfile, Is.EqualTo(TouchControlProfileKind.FlowSteerAutoBalanced));
            Assert.That(profile.EnableRightTap, Is.True);
            Assert.That(profile.EnableRightJumpZone, Is.False);
            Assert.That(profile.ManualLookEnabled, Is.True);
            Assert.That(profile.FlowSteeringEnabled, Is.True);
            Assert.That(profile.AutoCameraProfile, Is.EqualTo(AutoCameraProfileKind.SmartParkour));

            Object.DestroyImmediate(layout);
        }

        [Test]
        public void TouchProfilePreference_MigratesPreVersionedNonClassicBackToClassic()
        {
            PlayerPrefs.SetInt(
                Avoidance.UI.Touch.TouchInputCoordinator.ControlProfilePreferenceKey,
                (int)TouchControlProfileKind.LegacyCenteredFlick);
            PlayerPrefs.DeleteKey(
                Avoidance.UI.Touch.TouchInputCoordinator.ControlProfilePreferenceVersionKey);

            var profile = Avoidance.UI.Touch.TouchInputCoordinator.LoadPreferredControlProfile(
                TouchControlProfileKind.LeftMoveRightLookTapJump);

            Assert.That(profile, Is.EqualTo(TouchControlProfileKind.LeftMoveRightLookTapJump));
            Assert.That(
                PlayerPrefs.GetInt(Avoidance.UI.Touch.TouchInputCoordinator.ControlProfilePreferenceKey),
                Is.EqualTo((int)TouchControlProfileKind.LeftMoveRightLookTapJump));
            Assert.That(
                PlayerPrefs.GetInt(Avoidance.UI.Touch.TouchInputCoordinator.ControlProfilePreferenceVersionKey),
                Is.EqualTo(Avoidance.UI.Touch.TouchInputCoordinator.CurrentControlProfilePreferenceVersion));
            PlayerPrefs.DeleteKey(Avoidance.UI.Touch.TouchInputCoordinator.ControlProfilePreferenceKey);
            PlayerPrefs.DeleteKey(Avoidance.UI.Touch.TouchInputCoordinator.ControlProfilePreferenceVersionKey);
        }

        [Test]
        public void TouchProfilePreference_MigratesPreVersionedEasyBackToClassic()
        {
            PlayerPrefs.SetInt(
                Avoidance.UI.Touch.TouchInputCoordinator.ControlProfilePreferenceKey,
                (int)TouchControlProfileKind.FlowSteerAutoBalanced);
            PlayerPrefs.DeleteKey(
                Avoidance.UI.Touch.TouchInputCoordinator.ControlProfilePreferenceVersionKey);

            var profile = Avoidance.UI.Touch.TouchInputCoordinator.LoadPreferredControlProfile(
                TouchControlProfileKind.LeftMoveRightLookTapJump);

            Assert.That(profile, Is.EqualTo(TouchControlProfileKind.LeftMoveRightLookTapJump));
            PlayerPrefs.DeleteKey(Avoidance.UI.Touch.TouchInputCoordinator.ControlProfilePreferenceKey);
            PlayerPrefs.DeleteKey(Avoidance.UI.Touch.TouchInputCoordinator.ControlProfilePreferenceVersionKey);
        }

        [Test]
        public void TouchProfilePreference_PreservesVersionedEasyChoice()
        {
            Avoidance.UI.Touch.TouchInputCoordinator.StorePreferredControlProfile(
                TouchControlProfileKind.FlowSteerAutoBalanced);

            var profile = Avoidance.UI.Touch.TouchInputCoordinator.LoadPreferredControlProfile(
                TouchControlProfileKind.LeftMoveRightLookTapJump);

            Assert.That(profile, Is.EqualTo(TouchControlProfileKind.FlowSteerAutoBalanced));
            PlayerPrefs.DeleteKey(Avoidance.UI.Touch.TouchInputCoordinator.ControlProfilePreferenceKey);
            PlayerPrefs.DeleteKey(Avoidance.UI.Touch.TouchInputCoordinator.ControlProfilePreferenceVersionKey);
        }

        [Test]
        public void TouchLayout_ClassicManualProfileRemainsAvailable()
        {
            var layout = TouchControlLayout.CreateRuntimeDefault();
            var profile = layout.CreateRuntimeProfile(
                TouchControlProfileKind.LeftMoveRightLookTapJump,
                TouchJumpMode.RightTap,
                layout.DefaultCameraSensitivity,
                layout.DefaultMovementSensitivity);

            Assert.That(profile.ControlProfile, Is.EqualTo(TouchControlProfileKind.LeftMoveRightLookTapJump));
            Assert.That(profile.ManualLookEnabled, Is.True);
            Assert.That(profile.FlowSteeringEnabled, Is.False);
            Assert.That(profile.EnableRightTap, Is.True);
            Assert.That(profile.AutoCameraProfile, Is.EqualTo(AutoCameraProfileKind.Balanced));

            Object.DestroyImmediate(layout);
        }

        [Test]
        public void SafeOriginClamp_KeepsRightMovementTravelInsideComfortMargins()
        {
            var safeArea = Rect.MinMaxRect(0f, 0f, 2340f, 1080f);
            const float radius = 150f;
            const float horizontalMargin = 96f;
            const float verticalMargin = 82f;

            var origin = TouchControlGeometry.ClampOrigin(
                new Vector2(2330f, 20f),
                safeArea,
                radius,
                horizontalMargin,
                verticalMargin);

            Assert.That(origin.x, Is.EqualTo(2340f - radius - horizontalMargin).Within(0.001f));
            Assert.That(origin.y, Is.EqualTo(radius + verticalMargin).Within(0.001f));
            Assert.That(
                TouchControlGeometry.TravelCircleFits(origin, safeArea, radius, horizontalMargin, verticalMargin),
                Is.True);
        }

        [TestCase(1280f, 720f)]
        [TestCase(1920f, 1080f)]
        [TestCase(2400f, 1080f)]
        public void SafeOriginClamp_ProducesValidAreasAcrossLandscapeAspects(float width, float height)
        {
            var safeArea = Rect.MinMaxRect(0f, 0f, width, height);
            const float radius = 150f;
            var area = TouchControlGeometry.BuildSafeOriginArea(safeArea, radius, 96f, 82f);

            Assert.That(area.width, Is.GreaterThanOrEqualTo(0f));
            Assert.That(area.height, Is.GreaterThanOrEqualTo(0f));
            Assert.That(TouchControlGeometry.TravelCircleFits(area.center, safeArea, radius, 96f, 82f), Is.True);
        }

        [TestCase(120f, 0f, 2340f, 1080f)]
        [TestCase(0f, 0f, 2220f, 1080f)]
        public void SafeOriginClamp_RespectsLandscapeSafeAreaCutouts(
            float xMin,
            float yMin,
            float xMax,
            float yMax)
        {
            var safeArea = Rect.MinMaxRect(xMin, yMin, xMax, yMax);
            const float radius = 150f;
            var origin = TouchControlGeometry.ClampOrigin(
                new Vector2(xMax + 200f, yMax + 200f),
                safeArea,
                radius,
                96f,
                82f);

            Assert.That(TouchControlGeometry.TravelCircleFits(origin, safeArea, radius, 96f, 82f), Is.True);
        }

        [Test]
        public void FlickDetector_FastUpTriggersJumpIntent()
        {
            var detector = new FlickGestureDetector(FlickGestureSettings.Default);
            detector.Begin(Vector2.zero, 0f);

            var result = detector.Update(new Vector2(0f, 70f), 0.05f, 1000f);

            Assert.That(result.Triggered, Is.True);
            Assert.That(result.RejectReason, Is.EqualTo(FlickRejectReason.None));
        }

        [Test]
        public void FlickDetector_SlowUpwardDragDoesNotTrigger()
        {
            var detector = new FlickGestureDetector(FlickGestureSettings.Default);
            detector.Begin(Vector2.zero, 0f);

            var result = detector.Update(new Vector2(0f, 70f), 0.5f, 1000f);

            Assert.That(result.Triggered, Is.False);
        }

        [Test]
        public void FlickDetector_HoldingUpperPositionDoesNotRepeat()
        {
            var detector = new FlickGestureDetector(FlickGestureSettings.Default);
            detector.Begin(Vector2.zero, 0f);
            Assert.That(detector.Update(new Vector2(0f, 70f), 0.05f, 1000f).Triggered, Is.True);

            var held = detector.Update(new Vector2(0f, 70f), 0.12f, 1000f);

            Assert.That(held.Triggered, Is.False);
            Assert.That(held.RejectReason, Is.EqualTo(FlickRejectReason.NotRearmed));
        }

        [TestCase(80f, 0f)]
        [TestCase(-80f, 0f)]
        [TestCase(0f, -70f)]
        [TestCase(0f, 20f)]
        [TestCase(90f, 70f)]
        public void FlickDetector_RejectsNonUpwardOrSmallGestures(float x, float y)
        {
            var detector = new FlickGestureDetector(FlickGestureSettings.Default);
            detector.Begin(Vector2.zero, 0f);

            var result = detector.Update(new Vector2(x, y), 0.05f, 1000f);

            Assert.That(result.Triggered, Is.False);
        }

        [Test]
        public void FlickDetector_AllowsDiagonalUpWithinTolerance()
        {
            var detector = new FlickGestureDetector(FlickGestureSettings.Default);
            detector.Begin(Vector2.zero, 0f);

            var result = detector.Update(new Vector2(35f, 70f), 0.05f, 1000f);

            Assert.That(result.Triggered, Is.True);
        }

        [Test]
        public void FlickDetector_RearmsAfterRetreatAndSecondImpulse()
        {
            var detector = new FlickGestureDetector(FlickGestureSettings.Default);
            detector.Begin(Vector2.zero, 0f);
            Assert.That(detector.Update(new Vector2(0f, 70f), 0.05f, 1000f).Triggered, Is.True);
            Assert.That(detector.Update(new Vector2(0f, 10f), 0.16f, 1000f).Triggered, Is.False);

            var second = detector.Update(new Vector2(0f, 80f), 0.21f, 1000f);

            Assert.That(second.Triggered, Is.True);
        }

        [Test]
        public void TapDragClassifier_ShortStationaryReleaseProducesTap()
        {
            var classifier = new TapDragGestureClassifier();
            classifier.Begin(7, Vector2.zero, 0f);

            var state = classifier.Release(
                7,
                new Vector2(4f, 2f),
                0.12f,
                1000f,
                TapDragGestureSettings.Default);

            Assert.That(state, Is.EqualTo(TapDragGestureState.Tap));
            Assert.That(classifier.Snapshot.TotalDisplacement, Is.LessThan(0.035f));
        }

        [Test]
        public void TapDragClassifier_DragActivatesCameraAndReleaseDoesNotTap()
        {
            var classifier = new TapDragGestureClassifier();
            classifier.Begin(7, Vector2.zero, 0f);

            var drag = classifier.Update(
                7,
                new Vector2(24f, 0f),
                0.04f,
                1000f,
                TapDragGestureSettings.Default);
            var release = classifier.Release(
                7,
                new Vector2(28f, 0f),
                0.1f,
                1000f,
                TapDragGestureSettings.Default);

            Assert.That(drag, Is.EqualTo(TapDragGestureState.Camera));
            Assert.That(release, Is.EqualTo(TapDragGestureState.Camera));
        }

        [Test]
        public void TapDragClassifier_LongHoldDoesNotTap()
        {
            var classifier = new TapDragGestureClassifier();
            classifier.Begin(7, Vector2.zero, 0f);

            var state = classifier.Release(
                7,
                Vector2.zero,
                0.32f,
                1000f,
                TapDragGestureSettings.Default);

            Assert.That(state, Is.EqualTo(TapDragGestureState.Cancelled));
        }

        [Test]
        public void TouchCoordinator_RightTapJumpPreservesMovementAndOwnership()
        {
            var inputObject = new GameObject(
                "Touch Tap Continuity Test",
                typeof(Avoidance.UI.Touch.TouchInputCoordinator));
            var touch = inputObject.GetComponent<Avoidance.UI.Touch.TouchInputCoordinator>();
            Assert.That(touch.TryClaim(TouchControlRole.Movement, 11), Is.True);
            touch.SetMovement(Vector2.up, Vector2.up);
            touch.PressJump(TouchJumpSource.RightTap);

            Assert.That(touch.ConsumeJumpPressed(), Is.True);
            Assert.That(touch.Movement, Is.EqualTo(Vector2.up));
            Assert.That(touch.IsOwner(TouchControlRole.Movement, 11), Is.True);
            Object.DestroyImmediate(inputObject);
        }

        [Test]
        public void TouchCoordinator_LeftMovementTouchNeverJumps()
        {
            var inputObject = new GameObject(
                "Left Touch No Jump Test",
                typeof(Avoidance.UI.Touch.TouchInputCoordinator));
            var touch = inputObject.GetComponent<Avoidance.UI.Touch.TouchInputCoordinator>();
            Assert.That(touch.TryClaim(TouchControlRole.Movement, 21), Is.True);
            touch.SetMovement(Vector2.up, Vector2.up);
            touch.Release(TouchControlRole.Movement, 21);

            Assert.That(touch.ConsumeJumpPressed(), Is.False);
            Object.DestroyImmediate(inputObject);
        }

        [Test]
        public void TouchCoordinator_RightCameraDragReleaseDoesNotJump()
        {
            var inputObject = new GameObject(
                "Right Drag No Jump Test",
                typeof(Avoidance.UI.Touch.TouchInputCoordinator));
            var touch = inputObject.GetComponent<Avoidance.UI.Touch.TouchInputCoordinator>();
            Assert.That(touch.TryClaim(TouchControlRole.Look, 31), Is.True);

            touch.BeginLookGesture(31, Vector2.zero, 0f);
            Assert.That(
                touch.UpdateLookGesture(31, new Vector2(24f, 0f), 0.04f, 1000f),
                Is.True);
            touch.EndLookGesture(31, new Vector2(34f, 0f), 0.1f, 1000f);

            Assert.That(touch.ConsumeJumpPressed(), Is.False);
            Object.DestroyImmediate(inputObject);
        }

        [Test]
        public void TouchCoordinator_RightDragFeedsCameraOnActivationFrame()
        {
            PlayerPrefs.DeleteKey(Avoidance.UI.Touch.TouchInputCoordinator.ControlProfilePreferenceKey);
            var inputObject = new GameObject(
                "Right Drag Activation Test",
                typeof(Avoidance.UI.Touch.TouchInputCoordinator));
            var touch = inputObject.GetComponent<Avoidance.UI.Touch.TouchInputCoordinator>();
            touch.SetControlProfile(TouchControlProfileKind.LeftMoveRightLookTapJump);
            Assert.That(touch.TryClaim(TouchControlRole.Look, 41), Is.True);

            touch.BeginLookGesture(41, Vector2.zero, 0f);
            var cameraActive = touch.UpdateLookGesture(
                41,
                new Vector2(18f, 0f),
                0.035f,
                1000f);

            Assert.That(cameraActive, Is.True);
            touch.EndLookGesture(41, new Vector2(28f, 0f), 0.08f, 1000f);
            Assert.That(touch.ConsumeJumpPressed(), Is.False);
            PlayerPrefs.DeleteKey(Avoidance.UI.Touch.TouchInputCoordinator.ControlProfilePreferenceKey);
            Object.DestroyImmediate(inputObject);
        }

        [Test]
        public void TouchCoordinator_DevelopmentJumpCycleSwitchesClassicVariants()
        {
            PlayerPrefs.DeleteKey(Avoidance.UI.Touch.TouchInputCoordinator.ControlProfilePreferenceKey);
            PlayerPrefs.DeleteKey(Avoidance.UI.Touch.TouchInputCoordinator.ControlProfilePreferenceVersionKey);
            var inputObject = new GameObject(
                "Jump Cycle Test",
                typeof(Avoidance.UI.Touch.TouchInputCoordinator));
            var touch = inputObject.GetComponent<Avoidance.UI.Touch.TouchInputCoordinator>();
            touch.SetControlProfile(TouchControlProfileKind.LeftMoveRightLookTapJump);

            touch.CycleJumpMode();
            Assert.That(
                touch.RuntimeProfile.ControlProfile,
                Is.EqualTo(TouchControlProfileKind.LeftMoveRightLookFixedJump));
            Assert.That(touch.RuntimeProfile.JumpMode, Is.EqualTo(TouchJumpMode.FixedButton));

            touch.CycleJumpMode();
            Assert.That(
                touch.RuntimeProfile.ControlProfile,
                Is.EqualTo(TouchControlProfileKind.LeftMoveRightLookTapAndButton));
            Assert.That(touch.RuntimeProfile.JumpMode, Is.EqualTo(TouchJumpMode.RightTapAndFixedButton));

            touch.CycleJumpMode();
            Assert.That(
                touch.RuntimeProfile.ControlProfile,
                Is.EqualTo(TouchControlProfileKind.LeftMoveRightLookTapJump));
            Assert.That(touch.RuntimeProfile.JumpMode, Is.EqualTo(TouchJumpMode.RightTap));
            PlayerPrefs.DeleteKey(Avoidance.UI.Touch.TouchInputCoordinator.ControlProfilePreferenceKey);
            PlayerPrefs.DeleteKey(Avoidance.UI.Touch.TouchInputCoordinator.ControlProfilePreferenceVersionKey);
            Object.DestroyImmediate(inputObject);
        }

        [Test]
        public void FlowSteering_RightInputRotatesHeadingWithoutDroppingThrottle()
        {
            var player = new GameObject("Flow Steering Test", typeof(CharacterController), typeof(ParkourMotor));
            var motor = player.GetComponent<ParkourMotor>();
            motor.Initialize(MovementProfile.CreateRuntimeDefault());
            var input = new FakeInput
            {
                MoveValue = new Vector2(0.8f, 1f),
                FlowSteering = true
            };

            motor.Simulate(input, 0.1f);

            Assert.That(motor.CurrentHeadingYaw, Is.GreaterThan(1f));
            Assert.That(motor.LastThrottle, Is.GreaterThan(0.9f));
            Assert.That(motor.LastSteering, Is.GreaterThan(0.7f));
            Object.DestroyImmediate(player);
        }

        [Test]
        public void TouchCoordinator_ReleasingJoystickOwnerReturnsMovementToZero()
        {
            var inputObject = new GameObject(
                "Touch Release Test",
                typeof(Avoidance.UI.Touch.TouchInputCoordinator));
            var touch = inputObject.GetComponent<Avoidance.UI.Touch.TouchInputCoordinator>();
            Assert.That(touch.TryClaim(TouchControlRole.Movement, 1), Is.True);
            touch.SetMovement(Vector2.up, Vector2.up);

            touch.Release(TouchControlRole.Look, 2);
            Assert.That(touch.Movement, Is.EqualTo(Vector2.up));

            touch.Release(TouchControlRole.Movement, 1);
            Assert.That(touch.Movement, Is.EqualTo(Vector2.zero));
            Object.DestroyImmediate(inputObject);
        }

        [Test]
        public void MovementVectorMath_MapsCardinalInputsAtYawZero()
        {
            var yaw = Quaternion.identity;

            AssertVector(MovementVectorMath.ResolveCameraRelative(yaw, Vector2.zero).DesiredDirection, Vector3.zero);
            AssertVector(MovementVectorMath.ResolveCameraRelative(yaw, Vector2.up).DesiredDirection, Vector3.forward);
            AssertVector(MovementVectorMath.ResolveCameraRelative(yaw, Vector2.down).DesiredDirection, Vector3.back);
            AssertVector(MovementVectorMath.ResolveCameraRelative(yaw, Vector2.left).DesiredDirection, Vector3.left);
            AssertVector(MovementVectorMath.ResolveCameraRelative(yaw, Vector2.right).DesiredDirection, Vector3.right);
        }

        [TestCase(0f, 0f, 0f, 1f)]
        [TestCase(90f, 1f, 0f, 0f)]
        [TestCase(180f, 0f, 0f, -1f)]
        [TestCase(270f, -1f, 0f, 0f)]
        public void MovementVectorMath_ForwardFollowsCameraYaw(
            float yawDegrees,
            float expectedX,
            float expectedY,
            float expectedZ)
        {
            var movement = MovementVectorMath.ResolveCameraRelative(
                Quaternion.Euler(0f, yawDegrees, 0f),
                Vector2.up);

            AssertVector(
                movement.DesiredDirection,
                new Vector3(expectedX, expectedY, expectedZ));
        }

        [Test]
        public void MovementVectorMath_DiagonalInputDoesNotExceedUnitMagnitude()
        {
            var movement = MovementVectorMath.ResolveCameraRelative(
                Quaternion.identity,
                new Vector2(1f, 1f));

            Assert.That(movement.DesiredDirection.magnitude, Is.LessThanOrEqualTo(1.001f));
        }

        [Test]
        public void PlatformTracker_ReturnsDeterministicPlatformDelta()
        {
            var tracker = new PlatformMotionTracker();
            tracker.Reset(new Vector3(2f, 0f, 1f));

            var delta = tracker.Capture(new Vector3(2.5f, 0.25f, 1f));

            Assert.That(delta, Is.EqualTo(new Vector3(0.5f, 0.25f, 0f)));
        }

        [Test]
        public void HopRetention_PerfectPreservesMoreMomentumThanLate()
        {
            var profile = MovementProfile.CreateRuntimeDefault();
            var perfect = MovementMomentumMath.ResolveHopRetention(
                0.02f,
                11f,
                profile.BaseRunSpeed,
                profile);
            var late = MovementMomentumMath.ResolveHopRetention(
                0.4f,
                11f,
                profile.BaseRunSpeed,
                profile);

            Assert.That(perfect.Quality, Is.EqualTo(HopTimingQuality.Perfect));
            Assert.That(late.Quality, Is.EqualTo(HopTimingQuality.Late));
            Assert.That(perfect.SpeedAfterRetention, Is.GreaterThan(late.SpeedAfterRetention));
            Object.DestroyImmediate(profile);
        }

        [Test]
        public void VelocitySafetyLimit_ClampsHorizontalMagnitude()
        {
            var clamped = MovementMomentumMath.ClampHorizontalVelocity(Vector3.right * 80f, 18f);

            Assert.That(clamped.magnitude, Is.EqualTo(18f).Within(0.001f));
        }

        [Test]
        public void SurfMath_RequiresAuthoredValidAngleAndProjectsVelocity()
        {
            var surf = SurfProfile.CreateRuntimeDefault();
            var normal = Quaternion.Euler(42f, 0f, 0f) * Vector3.up;
            var ordinarySlope = Quaternion.Euler(12f, 0f, 0f) * Vector3.up;

            Assert.That(SurfMovementMath.IsSurfAngle(normal, surf), Is.True);
            Assert.That(SurfMovementMath.IsSurfAngle(ordinarySlope, surf), Is.False);
            Assert.That(
                Vector3.Dot(SurfMovementMath.ProjectVelocity(Vector3.forward + Vector3.up, normal), normal),
                Is.EqualTo(0f).Within(0.001f));
            Object.DestroyImmediate(surf);
        }

        [Test]
        public void TouchDevelopmentActions_AreEdgeTriggered()
        {
            var inputObject = new GameObject(
                "Touch Input Test",
                typeof(Avoidance.UI.Touch.TouchInputCoordinator));
            var touch = inputObject.GetComponent<Avoidance.UI.Touch.TouchInputCoordinator>();
            touch.PressRestart();
            touch.PressSwitchProfile();
            touch.PressToggleCameraEffects();
            touch.PressToggleDiagnostics();

            Assert.That(touch.ConsumeRestartPressed(), Is.True);
            Assert.That(touch.ConsumeRestartPressed(), Is.False);
            Assert.That(touch.ConsumeSwitchProfilePressed(), Is.True);
            Assert.That(touch.ConsumeSwitchProfilePressed(), Is.False);
            Assert.That(touch.ConsumeToggleCameraEffectsPressed(), Is.True);
            Assert.That(touch.ConsumeToggleCameraEffectsPressed(), Is.False);
            Assert.That(touch.ConsumeToggleDiagnosticsPressed(), Is.True);
            Assert.That(touch.ConsumeToggleDiagnosticsPressed(), Is.False);
            Object.DestroyImmediate(inputObject);
        }

        [Test]
        public void TouchCameraSensitivity_LoadsAndSavesSettingsPreset()
        {
            var inputObject = new GameObject(
                "Touch Settings Test",
                typeof(Avoidance.UI.Touch.TouchInputCoordinator));
            var touch = inputObject.GetComponent<Avoidance.UI.Touch.TouchInputCoordinator>();
            var settings = new MemorySettingsService();
            settings.Current.lookSensitivity = 0.95f;

            touch.ApplySettings(settings);

            Assert.That(
                touch.RuntimeProfile.CameraSensitivity,
                Is.EqualTo(TouchSensitivityPreset.VeryFast));

            touch.CycleCameraSensitivity();

            Assert.That(
                touch.RuntimeProfile.CameraSensitivity,
                Is.EqualTo(TouchSensitivityPreset.Low));
            Assert.That(settings.Current.lookSensitivity, Is.EqualTo(0.15f).Within(0.001f));
            Assert.That(settings.SaveCount, Is.EqualTo(1));
            Object.DestroyImmediate(inputObject);
        }

        [Test]
        public void ResetMotion_ClearsVelocityAndRestoresPosition()
        {
            var player = new GameObject("Test Player", typeof(CharacterController), typeof(ParkourMotor));
            var motor = player.GetComponent<ParkourMotor>();
            motor.Initialize(MovementProfile.CreateRuntimeDefault());
            player.transform.position = Vector3.up * 10f;
            motor.Simulate(new NullPlayerInputSource(), 0.1f);
            Assert.That(motor.VerticalSpeed, Is.LessThan(0f));

            var target = new Vector3(3f, 2f, 4f);
            motor.ResetMotion(target, Quaternion.Euler(0f, 45f, 0f), 0f);

            Assert.That(motor.Velocity, Is.EqualTo(Vector3.zero));
            Assert.That(player.transform.position, Is.EqualTo(target));
            Object.DestroyImmediate(player);
        }

        [Test]
        public void CameraReset_UsesDefaultDownwardPitchAndCheckpointYaw()
        {
            var player = new GameObject("Camera Test Player");
            var cameraObject = new GameObject("Camera", typeof(UnityEngine.Camera), typeof(FirstPersonCameraRig));
            cameraObject.transform.SetParent(player.transform, false);
            var rig = cameraObject.GetComponent<FirstPersonCameraRig>();
            rig.Initialize(
                player.transform,
                cameraObject.GetComponent<UnityEngine.Camera>(),
                CameraProfile.CreateRuntimeDefault());
            var input = new FakeInput { LookDeltaValue = new Vector2(12f, 20f) };
            rig.ApplyLook(input, PlayerInputMode.Editor, 0.016f);
            Assert.That(rig.Pitch, Is.Not.EqualTo(0f));

            rig.ResetView(Quaternion.Euler(0f, 90f, 0f));

            Assert.That(rig.Pitch, Is.EqualTo(9f).Within(0.001f));
            Assert.That(cameraObject.transform.forward.y, Is.LessThan(0f));
            Assert.That(Mathf.DeltaAngle(rig.Yaw, 90f), Is.EqualTo(0f).Within(0.01f));
            Object.DestroyImmediate(player);
        }

        [Test]
        public void CameraReset_CanPreserveRecentPitchAcrossFallRestore()
        {
            var player = new GameObject("Camera Restore Pitch Player");
            var cameraObject = new GameObject("Camera", typeof(UnityEngine.Camera), typeof(FirstPersonCameraRig));
            cameraObject.transform.SetParent(player.transform, false);
            var rig = cameraObject.GetComponent<FirstPersonCameraRig>();
            rig.Initialize(
                player.transform,
                cameraObject.GetComponent<UnityEngine.Camera>(),
                CameraProfile.CreateRuntimeDefault());

            rig.ResetView(Quaternion.Euler(0f, 127f, 0f), -32f);

            Assert.That(rig.Pitch, Is.EqualTo(-32f).Within(0.001f));
            Assert.That(Mathf.DeltaAngle(rig.Yaw, 127f), Is.EqualTo(0f).Within(0.01f));
            Object.DestroyImmediate(player);
        }

        [Test]
        public void TouchCamera_MicroJitterDoesNotRotate()
        {
            var player = new GameObject("Touch Camera Jitter Player");
            var cameraObject = new GameObject("Camera", typeof(UnityEngine.Camera), typeof(FirstPersonCameraRig));
            cameraObject.transform.SetParent(player.transform, false);
            var rig = cameraObject.GetComponent<FirstPersonCameraRig>();
            rig.Initialize(
                player.transform,
                cameraObject.GetComponent<UnityEngine.Camera>(),
                CameraProfile.CreateRuntimeDefault());
            var input = new FakeInput { LookDeltaValue = new Vector2(0.02f, 0.02f) };

            rig.ApplyLook(input, PlayerInputMode.Touch, 0.016f);

            Assert.That(Mathf.DeltaAngle(rig.Yaw, 0f), Is.EqualTo(0f).Within(0.001f));
            Assert.That(rig.Pitch, Is.EqualTo(9f).Within(0.001f));
            Object.DestroyImmediate(player);
        }

        [Test]
        public void TouchCamera_VerticalDragChangesPitchAndClampsWithoutRoll()
        {
            var player = new GameObject("Touch Vertical Camera Player");
            var cameraObject = new GameObject("Camera", typeof(UnityEngine.Camera), typeof(FirstPersonCameraRig));
            cameraObject.transform.SetParent(player.transform, false);
            var rig = cameraObject.GetComponent<FirstPersonCameraRig>();
            rig.Initialize(
                player.transform,
                cameraObject.GetComponent<UnityEngine.Camera>(),
                CameraProfile.CreateRuntimeDefault());
            rig.ToggleEffects();

            rig.ApplyLook(
                new FakeInput { LookDeltaValue = new Vector2(0f, 10000f) },
                PlayerInputMode.Touch,
                0.016f);
            Assert.That(rig.Pitch, Is.EqualTo(-75f).Within(0.001f));
            Assert.That(Mathf.DeltaAngle(rig.Yaw, 0f), Is.EqualTo(0f).Within(0.001f));

            rig.ApplyLook(
                new FakeInput { LookDeltaValue = new Vector2(0f, -10000f) },
                PlayerInputMode.Touch,
                0.016f);
            Assert.That(rig.Pitch, Is.EqualTo(75f).Within(0.001f));
            Assert.That(
                Mathf.Abs(Mathf.DeltaAngle(0f, cameraObject.transform.localEulerAngles.z)),
                Is.LessThan(0.001f));
            Object.DestroyImmediate(player);
        }

        [Test]
        public void FlowSteeringTouchCamera_UsesRightDragForPitchOnly()
        {
            var player = new GameObject("Flow Pitch Camera Player");
            var cameraObject = new GameObject("Camera", typeof(UnityEngine.Camera), typeof(FirstPersonCameraRig));
            cameraObject.transform.SetParent(player.transform, false);
            var rig = cameraObject.GetComponent<FirstPersonCameraRig>();
            rig.Initialize(
                player.transform,
                cameraObject.GetComponent<UnityEngine.Camera>(),
                CameraProfile.CreateRuntimeDefault());
            var input = new FakeInput
            {
                FlowSteering = true,
                LookDeltaValue = new Vector2(80f, 80f)
            };

            rig.ApplyLook(input, PlayerInputMode.Touch, 0.016f);

            Assert.That(rig.Pitch, Is.LessThan(9f));
            Assert.That(Mathf.DeltaAngle(rig.Yaw, 0f), Is.EqualTo(0f).Within(0.001f));
            Assert.That(rig.CurrentLookDelta.x, Is.EqualTo(0f).Within(0.001f));
            Object.DestroyImmediate(player);
        }

        [Test]
        public void TouchCamera_FastSwipeTurnsWithoutRoll()
        {
            var player = new GameObject("Touch Camera Swipe Player");
            var cameraObject = new GameObject("Camera", typeof(UnityEngine.Camera), typeof(FirstPersonCameraRig));
            cameraObject.transform.SetParent(player.transform, false);
            var rig = cameraObject.GetComponent<FirstPersonCameraRig>();
            rig.Initialize(
                player.transform,
                cameraObject.GetComponent<UnityEngine.Camera>(),
                CameraProfile.CreateRuntimeDefault());
            var input = new FakeInput { LookDeltaValue = new Vector2(80f, 0f) };

            rig.ApplyLook(input, PlayerInputMode.Touch, 0.016f);

            Assert.That(Mathf.DeltaAngle(0f, rig.Yaw), Is.GreaterThan(4f));
            Assert.That(
                Mathf.Abs(Mathf.DeltaAngle(0f, cameraObject.transform.localEulerAngles.z)),
                Is.LessThan(0.001f));
            Object.DestroyImmediate(player);
        }

        [Test]
        public void ManualLookSuppressesLandingAutoPitch()
        {
            var player = new GameObject("Manual Camera Player");
            var cameraObject = new GameObject("Camera", typeof(UnityEngine.Camera), typeof(FirstPersonCameraRig));
            cameraObject.transform.SetParent(player.transform, false);
            var rig = cameraObject.GetComponent<FirstPersonCameraRig>();
            rig.Initialize(
                player.transform,
                cameraObject.GetComponent<UnityEngine.Camera>(),
                CameraProfile.CreateRuntimeDefault());
            var input = new FakeInput { LookDeltaValue = new Vector2(24f, 0f) };

            rig.ApplyLook(input, PlayerInputMode.Touch, 0.016f);
            rig.UpdatePresentation(8f, 10f, false, false, -8f, 0.016f);

            Assert.That(rig.ManualLookActive, Is.True);
            Assert.That(rig.CameraPitchTarget, Is.EqualTo(9f).Within(0.1f));
            Object.DestroyImmediate(player);
        }

        [Test]
        public void Phase077_GlobalCameraProfileEnablesManualPitchAndKeepsLockedFov()
        {
            var profile = Resources.Load<CameraProfile>("Camera_Default");

            Assert.That(profile, Is.Not.Null);
            Assert.That(profile.DefaultGameplayPitch, Is.EqualTo(9f).Within(0.001f));
            Assert.That(profile.MinimumPitch, Is.EqualTo(-75f).Within(0.001f));
            Assert.That(profile.MaximumPitch, Is.EqualTo(75f).Within(0.001f));
            Assert.That(profile.BaseFieldOfView, Is.EqualTo(94f).Within(0.001f));
            Assert.That(profile.TouchAutoPitchEnabled, Is.False);
            Assert.That(FirstPersonCameraRig.MaximumPreferredFieldOfView, Is.EqualTo(94f));
        }

        [Test]
        public void Phase076_PlayerGroundingProfileIsSubtleAndHeightFaded()
        {
            var profile = Resources.Load<PlayerGroundingProfile>(PlayerGroundingProfile.ResourceName);

            Assert.That(profile, Is.Not.Null);
            Assert.That(profile.Radius, Is.InRange(0.3f, 0.55f));
            Assert.That(profile.ShadowColor.a, Is.InRange(0.15f, 0.4f));
            Assert.That(profile.FullOpacityHeight, Is.LessThan(0.3f));
            Assert.That(profile.FadeOutHeight, Is.GreaterThan(2f));
            Assert.That(profile.MaximumSurfaceDistance, Is.GreaterThanOrEqualTo(profile.FadeOutHeight));
        }

        [Test]
        public void FirstPersonHands_HiddenProfileDoesNotCreateArmRenderers()
        {
            var player = new GameObject("Hands Player", typeof(CharacterController), typeof(ParkourMotor));
            var motor = player.GetComponent<ParkourMotor>();
            motor.Initialize(MovementProfile.CreateRuntimeDefault());
            var cameraObject = new GameObject("Hands Camera", typeof(UnityEngine.Camera), typeof(FirstPersonHands));
            cameraObject.transform.SetParent(player.transform, false);

            var hands = cameraObject.GetComponent<FirstPersonHands>();
            hands.Initialize(motor);

            Assert.That(hands.ArmsVisible, Is.False);
            Assert.That(hands.ArmCamera, Is.Null);
            Assert.That(cameraObject.GetComponentsInChildren<Renderer>(), Is.Empty);

            Object.DestroyImmediate(player);
        }

        [Test]
        public void FirstPersonHands_ArePresentationOnlyAndDoNotMoveGameplayRoot()
        {
            var player = new GameObject("Hands Player", typeof(CharacterController), typeof(ParkourMotor));
            var motor = player.GetComponent<ParkourMotor>();
            motor.Initialize(MovementProfile.CreateRuntimeDefault());
            var cameraObject = new GameObject("Hands Camera", typeof(UnityEngine.Camera), typeof(FirstPersonHands));
            cameraObject.transform.SetParent(player.transform, false);
            var startPosition = player.transform.position;
            var startRotation = player.transform.rotation;
            var hands = cameraObject.GetComponent<FirstPersonHands>();

            hands.Initialize(motor);
            typeof(FirstPersonHands)
                .GetMethod("LateUpdate", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.Invoke(hands, null);

            Assert.That(player.transform.position, Is.EqualTo(startPosition));
            Assert.That(player.transform.rotation, Is.EqualTo(startRotation));
            Assert.That(cameraObject.GetComponentsInChildren<Collider>(true), Is.Empty);
            Assert.That(cameraObject.GetComponentsInChildren<Rigidbody>(true), Is.Empty);
            Assert.That(hands.ArmsVisible, Is.False);
            Assert.That(cameraObject.transform.Find(FirstPersonHands.ArmCameraName), Is.Null);

            Object.DestroyImmediate(player);
        }

        [Test]
        public void FirstPersonArmProfile_UsesRyderArmV2CanonicalAnimatedPose()
        {
            var profile = Resources.Load<FirstPersonArmProfile>(FirstPersonArmProfile.ResourceName);

            Assert.That(profile, Is.Not.Null);
            Assert.That(profile.PresentationMode, Is.EqualTo(FirstPersonArmPresentationMode.RyderArmV2));
            Assert.That(profile.ShowFirstPersonArms, Is.False);
            Assert.That(profile.UseRiggedArms, Is.True);
            Assert.That(profile.HasRyderArmV2Prefabs, Is.True);
            Assert.That(profile.HasPreviousRiggedFallback, Is.True);
            Assert.That(profile.HasLegacyApprovedFallback, Is.True);
            Assert.That(profile.LeftArmPrefab, Is.SameAs(profile.RyderArmV2LeftArmPrefab));
            Assert.That(profile.RightArmPrefab, Is.SameAs(profile.RyderArmV2RightArmPrefab));
            Assert.That(profile.LeftBaseLocalPosition.z, Is.EqualTo(profile.RightBaseLocalPosition.z).Within(0.03f));
            Assert.That(Mathf.Abs(profile.LeftBaseLocalPosition.x), Is.EqualTo(Mathf.Abs(profile.RightBaseLocalPosition.x)).Within(0.02f));
            Assert.That(profile.PrefabLocalScale.x, Is.EqualTo(profile.PrefabLocalScale.y).Within(0.001f));
            Assert.That(profile.PrefabLocalScale.y, Is.EqualTo(profile.PrefabLocalScale.z).Within(0.001f));
            Assert.That(profile.PrefabLocalScale.x, Is.InRange(0.355f, 0.365f));
            Assert.That(profile.LeftBaseLocalPosition.x, Is.InRange(-0.54f, -0.50f));
            Assert.That(profile.RightBaseLocalPosition.x, Is.InRange(0.50f, 0.54f));
            Assert.That(profile.RightBaseLocalPosition.x - profile.LeftBaseLocalPosition.x, Is.GreaterThan(1.0f));
            Assert.That(profile.LeftBaseLocalPosition.y, Is.InRange(-0.04f, -0.01f));
            Assert.That(profile.RightBaseLocalPosition.y, Is.InRange(-0.035f, -0.005f));
            Assert.That(profile.LeftBaseLocalPosition.y, Is.GreaterThan(-0.08f));
            Assert.That(profile.RightBaseLocalPosition.y, Is.GreaterThan(-0.08f));
            Assert.That(profile.LeftBaseLocalPosition.z, Is.InRange(0.39f, 0.43f));
            Assert.That(profile.RightBaseLocalPosition.z, Is.InRange(0.39f, 0.43f));
            AssertVector(profile.LeftBaseLocalEulerAngles, new Vector3(0f, -52f, 202f));
            AssertVector(profile.RightBaseLocalEulerAngles, new Vector3(1f, 52f, 158f));
            AssertVector(profile.LeftPrefabEulerOffset, Vector3.zero);
            AssertVector(profile.RightPrefabEulerOffset, Vector3.zero);
            Assert.That(profile.NeutralPoseLocked, Is.False);
            Assert.That(profile.ArmFieldOfView, Is.EqualTo(72f).Within(0.001f));
            Assert.That(profile.ArmNearClipPlane, Is.EqualTo(0.025f).Within(0.001f));
            Assert.That(profile.IdleFloatAmount, Is.InRange(0.0025f, 0.004f));
            Assert.That(profile.RunLateralAmount, Is.LessThanOrEqualTo(0.0025f));
            Assert.That(profile.RunForwardAmount, Is.InRange(0.026f, 0.03f));
            Assert.That(profile.MaximumPresentationDisplacement, Is.LessThanOrEqualTo(0.055f));
            Assert.That(profile.MaximumAnimationMultiplier, Is.LessThanOrEqualTo(1.25f));
            Assert.That(profile.JumpOffset, Is.GreaterThan(0f));
            Assert.That(profile.LandImpulse, Is.GreaterThan(0f));
        }

        [Test]
        public void FirstPersonHands_RunForwardOffsetsAlternateAndStayBounded()
        {
            var profile = Resources.Load<FirstPersonArmProfile>(FirstPersonArmProfile.ResourceName);
            Assert.That(profile, Is.Not.Null);

            var offsets = FirstPersonHands.ResolveOpposedRunForwardOffsets(
                0f,
                7.8f,
                grounded: true,
                profile.SpeedInfluence,
                profile.RunForwardAmount);

            Assert.That(Mathf.Abs(offsets.x), Is.GreaterThan(0f));
            Assert.That(offsets.x, Is.EqualTo(-offsets.y).Within(0.0001f));
            Assert.That(Mathf.Abs(offsets.x), Is.LessThanOrEqualTo(profile.MaximumPresentationDisplacement));
        }

        [Test]
        public void FirstPersonHands_RunLateralOffsetsOnlyOpenAwayFromCenter()
        {
            var profile = Resources.Load<FirstPersonArmProfile>(FirstPersonArmProfile.ResourceName);
            Assert.That(profile, Is.Not.Null);

            var openOffsets = FirstPersonHands.ResolveOutwardRunLateralOffsets(
                Mathf.PI * 0.5f,
                7.8f,
                grounded: true,
                profile.SpeedInfluence,
                profile.RunLateralAmount);
            var returnOffsets = FirstPersonHands.ResolveOutwardRunLateralOffsets(
                Mathf.PI * 1.5f,
                7.8f,
                grounded: true,
                profile.SpeedInfluence,
                profile.RunLateralAmount);

            Assert.That(openOffsets.x, Is.LessThanOrEqualTo(0f));
            Assert.That(openOffsets.y, Is.GreaterThanOrEqualTo(0f));
            Assert.That(Mathf.Abs(openOffsets.x), Is.LessThanOrEqualTo(profile.RunLateralAmount));
            Assert.That(Mathf.Abs(openOffsets.y), Is.LessThanOrEqualTo(profile.RunLateralAmount));
            Assert.That(returnOffsets.x, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(returnOffsets.y, Is.EqualTo(0f).Within(0.0001f));
        }

        [Test]
        public void FirstPersonHands_HiddenProfileLeavesWorldCameraAndAudioListenerOnly()
        {
            var player = new GameObject("Hands Player", typeof(CharacterController), typeof(ParkourMotor));
            var motor = player.GetComponent<ParkourMotor>();
            motor.Initialize(MovementProfile.CreateRuntimeDefault());
            var cameraObject = new GameObject(
                "Hands Camera",
                typeof(UnityEngine.Camera),
                typeof(AudioListener),
                typeof(FirstPersonHands));
            cameraObject.transform.SetParent(player.transform, false);
            cameraObject.GetComponent<UnityEngine.Camera>().fieldOfView = 94f;

            var hands = cameraObject.GetComponent<FirstPersonHands>();
            hands.Initialize(motor);

            var profile = Resources.Load<FirstPersonArmProfile>(FirstPersonArmProfile.ResourceName);
            var armLayer = FirstPersonHands.ResolveArmLayer();
            var armMask = 1 << armLayer;
            Assert.That(armLayer, Is.GreaterThanOrEqualTo(0));
            Assert.That(profile.ShowFirstPersonArms, Is.False);
            Assert.That(hands.ArmsVisible, Is.False);
            Assert.That(hands.ArmCamera, Is.Null);
            Assert.That(cameraObject.GetComponent<UnityEngine.Camera>().fieldOfView, Is.EqualTo(94f).Within(0.01f));
            Assert.That(cameraObject.GetComponent<UnityEngine.Camera>().cullingMask & armMask, Is.EqualTo(0));
            Assert.That(cameraObject.GetComponentsInChildren<AudioListener>(true).Length, Is.EqualTo(1));
            Assert.That(cameraObject.GetComponentsInChildren<Renderer>(true), Is.Empty);

            Object.DestroyImmediate(player);
        }

        private sealed class FakeInput : IPlayerInputSource, IFlowSteeringInputSource
        {
            public Vector2 LookDeltaValue;
            public Vector2 MoveValue;
            public bool FlowSteering;
            public Vector2 Move => MoveValue;
            public Vector2 LookDelta => LookDeltaValue;
            public bool JumpPressed => false;
            public bool FlowSteeringEnabled => FlowSteering;
            public AutoCameraProfileKind AutoCameraProfile => AutoCameraProfileKind.Balanced;
            public void ResetState() { }
        }

        private sealed class MemorySettingsService : ISettingsService
        {
            public GameSettings Current { get; } = new GameSettings();
            public int SaveCount { get; private set; }

            public void Load() { }
            public void Save() => SaveCount++;
        }

        private static void AssertVector(Vector3 actual, Vector3 expected)
        {
            Assert.That(actual.x, Is.EqualTo(expected.x).Within(0.001f));
            Assert.That(actual.y, Is.EqualTo(expected.y).Within(0.001f));
            Assert.That(actual.z, Is.EqualTo(expected.z).Within(0.001f));
        }
    }
}
