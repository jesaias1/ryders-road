using System.Text;
using Avoidance.Core.Services;
using Avoidance.Gameplay.Audio;
using Avoidance.Gameplay.Camera;
using Avoidance.Gameplay.Checkpoints;
using Avoidance.Gameplay.Respawn;
using Avoidance.Gameplay.Timing;
using Avoidance.Gameplay.Visuals;
using Avoidance.Input;
using UnityEngine;

namespace Avoidance.Gameplay.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerRuntimeCoordinator : MonoBehaviour
    {
        private readonly StringBuilder _touchBuilder = new StringBuilder(32);
        private PlayerInputRouter _input;
        private ITouchInputProvider _touch;
        private MovementProfileSet _profiles;
        private ParkourMotor _motor;
        private FirstPersonCameraRig _cameraRig;
        private RestoreController _restore;
        private MovementFeedback _feedback;
        private MovementSessionStats _stats;
        private MovementSessionReporter _reporter;
        private CheckpointService _checkpoints;
        private IDiagnosticsService _diagnostics;
        private bool _touchZonesVisible;
        private float _diagnosticTimer;

        public PlayerInputRouter Input => _input;
        public ParkourMotor Motor => _motor;
        public FirstPersonCameraRig CameraRig => _cameraRig;
        public MovementProfileSet Profiles => _profiles;

        public void Initialize(
            PlayerInputRouter input,
            ITouchInputProvider touch,
            MovementProfileSet profiles,
            ParkourMotor motor,
            FirstPersonCameraRig cameraRig,
            RestoreController restore,
            MovementFeedback feedback,
            MovementSessionStats stats,
            MovementSessionReporter reporter,
            CheckpointService checkpoints,
            IDiagnosticsService diagnostics)
        {
            _input = input;
            _touch = touch;
            _profiles = profiles;
            _motor = motor;
            _cameraRig = cameraRig;
            _restore = restore;
            _feedback = feedback;
            _stats = stats;
            _reporter = reporter;
            _checkpoints = checkpoints;
            _diagnostics = diagnostics;

            _motor.Jumped += HandleJumped;
            _motor.Landed += HandleLanded;
        }

        private void Update()
        {
            if (_input == null)
            {
                return;
            }

            _input.Sample();
            if (_input.SwitchProfilePressed)
            {
                _motor.SetProfile(_profiles.Next());
            }

            if (_input.ToggleTouchZonesPressed)
            {
                _touchZonesVisible = !_touchZonesVisible;
                _touch?.SetZonesVisible(_touchZonesVisible);
            }

            if (_input.ToggleCameraEffectsPressed)
            {
                _cameraRig.ToggleEffects();
            }

            if (_input.ToggleDiagnosticsPressed)
            {
                _diagnostics?.CycleDisplayMode();
            }

            _cameraRig.SetAutoCameraProfile(_input.AutoCameraProfile);
            _cameraRig.ApplyLook(_input, _input.ActiveMode, Time.unscaledDeltaTime, _motor);
            _motor.Simulate(_input, Time.deltaTime);
            _restore.Tick(_input.RestartPressed, Time.deltaTime);
            _cameraRig.UpdatePresentation(
                _motor.HorizontalSpeed,
                _motor.Profile.RunSpeed,
                _motor.IsGrounded,
                _motor.IsSurfing,
                _motor.VerticalSpeed,
                Time.deltaTime);
            _reporter.Tick(Time.unscaledDeltaTime);
            PublishDiagnostics(Time.unscaledDeltaTime);
        }

        private void OnDestroy()
        {
            if (_motor != null)
            {
                _motor.Jumped -= HandleJumped;
                _motor.Landed -= HandleLanded;
            }
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                _input?.ResetState();
            }
        }

        private void HandleJumped()
        {
            _stats.RecordJump();
            _feedback.PlayJump();
        }

        private void HandleLanded(float speed)
        {
            _cameraRig.NotifyLanding(speed);
            _feedback.PlayLanding(speed);
        }

        private void PublishDiagnostics(float deltaTime)
        {
            if (_diagnostics == null)
            {
                return;
            }

            _diagnosticTimer -= deltaTime;
            if (_diagnosticTimer > 0f)
            {
                return;
            }

            _diagnosticTimer = 0.15f;
            _touchBuilder.Clear();
            for (var index = 0; index < _input.ActiveTouchIds.Count; index++)
            {
                if (index > 0)
                {
                    _touchBuilder.Append(',');
                }

                _touchBuilder.Append(_input.ActiveTouchIds[index]);
            }

            _diagnostics.SetValue("Player position", FormatVector(transform.position));
            _diagnostics.SetValue("Horizontal speed", _motor.HorizontalSpeed.ToString("0.00"));
            _diagnostics.SetValue("Air projected speed", _motor.AirProjectedSpeed.ToString("0.000"));
            _diagnostics.SetValue("Air requested/applied dv", $"{_motor.AirRequestedDelta:F3} / {_motor.AirAppliedWishDelta:F3}");
            _diagnostics.SetValue("Air net dv", FormatVector(_motor.AirNetDelta));
            _diagnostics.SetValue("Air energy/safety limit", $"{_motor.AirEnergyLimited} / {_motor.SafetyLimited}");
            _diagnostics.SetValue("Contact dv", FormatVector(_motor.ContactVelocityDelta));
            _diagnostics.SetValue("Vertical speed", _motor.VerticalSpeed.ToString("0.00"));
            _diagnostics.SetValue("Grounded", _motor.IsGrounded.ToString());
            _diagnostics.SetValue("Movement state", _motor.MovementState.ToString());
            _diagnostics.SetValue("Desired heading", _motor.DesiredHeadingYaw.ToString("0.0"));
            _diagnostics.SetValue("Current heading", _motor.CurrentHeadingYaw.ToString("0.0"));
            _diagnostics.SetValue("Velocity heading", _motor.VelocityHeadingYaw.ToString("0.0"));
            _diagnostics.SetValue("Heading/velocity delta", _motor.HeadingVelocityDelta.ToString("0.0"));
            _diagnostics.SetValue("Throttle", _motor.LastThrottle.ToString("0.00"));
            _diagnostics.SetValue("Steering", _motor.LastSteering.ToString("0.00"));
            _diagnostics.SetValue("Current yaw rate", _motor.LastHeadingYawRate.ToString("0.0"));
            _diagnostics.SetValue("Momentum ratio", _motor.MomentumRatio.ToString("0.00"));
            _diagnostics.SetValue("Peak speed", _motor.PeakHorizontalSpeed.ToString("0.00"));
            _diagnostics.SetValue("Surf speed", _motor.SurfSpeed.ToString("0.00"));
            _diagnostics.SetValue("Movement profile", $"{_motor.Profile.DisplayName} / {_motor.Profile.ProfileId} / v{_motor.Profile.CompatibilityVersion}");
            _diagnostics.SetValue("Development build", Application.version);
            _diagnostics.SetValue("Base run speed", _motor.Profile.BaseRunSpeed.ToString("0.00"));
            _diagnostics.SetValue("Soft momentum limit", _motor.Profile.ResponsiveAirControl
                ? "No ordinary air envelope" : _motor.Profile.SoftMomentumLimit.ToString("0.00"));
            _diagnostics.SetValue("Hard velocity limit", _motor.Profile.HardVelocitySafetyLimit.ToString("0.00"));
            _diagnostics.SetValue("Jump height", _motor.Profile.JumpHeight.ToString("0.00"));
            _diagnostics.SetValue("Time to apex", _motor.Profile.TimeToApex.ToString("0.000"));
            _diagnostics.SetValue("Jump gravity", _motor.Profile.JumpGravity.ToString("0.00"));
            _diagnostics.SetValue("Fall gravity", _motor.Profile.FallGravity.ToString("0.00"));
            _diagnostics.SetValue("Estimated airtime", _motor.Profile.EstimatedAirtime.ToString("0.000"));
            _diagnostics.SetValue("Normal takeoff speed", _motor.Profile.NormalTakeoffSpeed.ToString("0.00"));
            _diagnostics.SetValue("Takeoff speed", _motor.LastTakeoffHorizontalSpeed.ToString("0.00"));
            _diagnostics.SetValue("Takeoff vertical velocity", _motor.LastTakeoffVerticalVelocity.ToString("0.00"));
            _diagnostics.SetValue("Takeoff retention", _motor.LastTakeoffMomentumRetention.ToString("0.00"));
            _diagnostics.SetValue("Jump apex height", _motor.LastJumpApexHeight.ToString("0.00"));
            _diagnostics.SetValue("Jump airtime", _motor.LastJumpAirtime.ToString("0.000"));
            _diagnostics.SetValue("Jump horizontal distance", _motor.LastJumpHorizontalDistance.ToString("0.00"));
            _diagnostics.SetValue("Restore Point", _checkpoints.CurrentCheckpointId);
            _diagnostics.SetValue("Jump buffered", _motor.JumpBuffered.ToString());
            _diagnostics.SetValue("Coyote eligible", _motor.CoyoteEligible.ToString());
            _diagnostics.SetValue("Active touch IDs", _touchBuilder.Length == 0 ? "none" : _touchBuilder.ToString());
            var touch = _input.TouchDebugState;
            _diagnostics.SetValue("Raw joystick", FormatVector(touch.RawJoystick));
            _diagnostics.SetValue("Post-deadzone joystick", FormatVector(touch.PostDeadZoneJoystick));
            _diagnostics.SetValue("Normalized movement", FormatVector(touch.NormalizedMovement));
            _diagnostics.SetValue("Control profile", touch.ControlProfile.ToString());
            _diagnostics.SetValue("Jump mode", touch.JumpMode.ToString());
            _diagnostics.SetValue("Camera sensitivity", touch.CameraSensitivity.ToString());
            _diagnostics.SetValue("Movement sensitivity", touch.MovementSensitivity.ToString());
            _diagnostics.SetValue(
                "Touch owners",
                $"move {FormatTouchId(touch.MovementTouchId)}, look {FormatTouchId(touch.LookTouchId)}, jump {FormatTouchId(touch.JumpTouchId)}");
            _diagnostics.SetValue("Right Touch ID", FormatTouchId(touch.RightTouchId));
            _diagnostics.SetValue("Movement origin", FormatVector(touch.MovementOrigin));
            _diagnostics.SetValue("Movement knob", FormatVector(touch.MovementKnob));
            _diagnostics.SetValue("Touch Velocity X", touch.TouchVelocity.x.ToString("0.00"));
            _diagnostics.SetValue("Touch Velocity Y", touch.TouchVelocity.y.ToString("0.00"));
            _diagnostics.SetValue("Flick Velocity", touch.FlickVelocity.ToString("0.00"));
            _diagnostics.SetValue("Flick Distance", touch.FlickDistance.ToString("0.000"));
            _diagnostics.SetValue("Flick Duration", touch.FlickDuration.ToString("0.000"));
            _diagnostics.SetValue("Direction Angle", touch.FlickDirectionAngle.ToString("0.0"));
            _diagnostics.SetValue("Flick Candidate", touch.FlickCandidate.ToString());
            _diagnostics.SetValue("Flick Armed", touch.FlickArmed.ToString());
            _diagnostics.SetValue("Flick Triggered", touch.FlickTriggered.ToString());
            _diagnostics.SetValue("Last Flick Time", touch.LastFlickTime.ToString("0.00"));
            _diagnostics.SetValue("Flick Rejection", touch.FlickRejectReason.ToString());
            _diagnostics.SetValue("Last Jump Source", touch.LastJumpSource.ToString());
            _diagnostics.SetValue("Jump Tap Duration", touch.JumpTapDuration.ToString("0.000"));
            _diagnostics.SetValue("Move input", FormatVector(_input.Move));
            _diagnostics.SetValue("Camera delta", FormatVector(_cameraRig.CurrentLookDelta));
            _diagnostics.SetValue("Current FOV", _cameraRig.CurrentFieldOfView.ToString("0.0"));
            _diagnostics.SetValue("Current camera profile", _cameraRig.CurrentAutoCameraProfile.ToString());
            _diagnostics.SetValue("Camera pitch target", _cameraRig.CameraPitchTarget.ToString("0.0"));
            _diagnostics.SetValue("Smart camera active", _cameraRig.SmartCameraActive.ToString());
            if (_cameraRig.SmartCameraActive)
            {
                var smart = _cameraRig.SmartCameraState;
                _diagnostics.SetValue("Smart camera yaw", smart.WorldYaw.ToString("0.0"));
                _diagnostics.SetValue("Smart cone response", smart.ConeResponse.ToString("0.00"));
                _diagnostics.SetValue("Smart route branch", smart.MatchedBranchId);
                _diagnostics.SetValue("Smart route distance", smart.RouteDistance.ToString("0.00"));
                _diagnostics.SetValue("Smart route weight", smart.RouteWeight.ToString("0.00"));
                _diagnostics.SetValue("Smart branch state", smart.BranchIntent.State.ToString());
                _diagnostics.SetValue("Smart branch id", smart.BranchIntent.BranchId);
                _diagnostics.SetValue("Smart branch confidence", smart.BranchIntent.Confidence.ToString("0.00"));
                _diagnostics.SetValue("Smart travel dir", FormatVector(smart.TravelDirection));
                _diagnostics.SetValue("Smart route dir", FormatVector(smart.RouteDirection));
            }

            _diagnostics.SetValue(
                "Movement snapshot",
                $"{Application.version} | {_motor.Profile.DisplayName} | run {_motor.Profile.BaseRunSpeed:0.0} | jump {_motor.Profile.JumpHeight:0.00}/{_motor.Profile.TimeToApex:0.000} | air {_motor.Profile.AirControl:0.00} | FOV {_cameraRig.CurrentFieldOfView:0.0}");
            _diagnostics.SetValue("Camera forward", FormatVector(_cameraRig.transform.forward));
            _diagnostics.SetValue("Camera right", FormatVector(_cameraRig.transform.right));
            _diagnostics.SetValue("Projected forward", FormatVector(_motor.LastProjectedForward));
            _diagnostics.SetValue("Projected right", FormatVector(_motor.LastProjectedRight));
            _diagnostics.SetValue("Desired movement", FormatVector(_motor.LastDesiredDirection));
            _diagnostics.SetValue("Desired velocity", FormatVector(_motor.LastDesiredVelocity));
            _diagnostics.SetValue("Actual horizontal velocity", FormatVector(_motor.ActualHorizontalVelocity));
            _diagnostics.SetValue("Lateral velocity", _motor.LateralVelocity.ToString("0.000"));
            _diagnostics.SetValue(
                "Estimated max jump",
                _motor.Profile.EstimatedMaximumJumpDistance.ToString("0.00"));
            _diagnostics.SetValue("Restore count", _restore.RestoreCount.ToString());
            _diagnostics.SetValue(
                "Spawn protection",
                _restore.SpawnProtectionRemaining.ToString("0.00"));
            _diagnostics.SetValue("Camera effects", _cameraRig.EffectsEnabled.ToString());
            _diagnostics.SetValue("Session report", _reporter.LastExportPath ?? "pending");
        }

        private static string FormatVector(Vector3 value)
        {
            return $"{value.x:0.00},{value.y:0.00},{value.z:0.00}";
        }

        private static string FormatVector(Vector2 value)
        {
            return $"{value.x:0.00},{value.y:0.00}";
        }

        private static string FormatTouchId(int pointerId)
        {
            return pointerId == TouchOwnershipRegistry.UnassignedPointerId
                ? "none"
                : pointerId.ToString();
        }
    }
}
