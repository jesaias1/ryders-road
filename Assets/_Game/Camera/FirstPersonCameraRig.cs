using Avoidance.Gameplay.Player;
using Avoidance.Input;
using UnityEngine;

namespace Avoidance.Gameplay.Camera
{
    [DisallowMultipleComponent]
    public sealed class FirstPersonCameraRig : MonoBehaviour
    {
        public const string FieldOfViewPreferenceKey = "settings.camera-fov";
        public const float MinimumPreferredFieldOfView = 76f;
        public const float DefaultPreferredFieldOfView = 94f;
        public const float MaximumPreferredFieldOfView = 94f;

        private Transform _playerYaw;
        private UnityEngine.Camera _camera;
        private CameraProfile _profile;
        private Vector2 _smoothedLook;
        private float _pitch;
        private float _landingAssistPitch;
        private float _autoPitchOffset;
        private float _smartYawOffset;
        private float _bobTime;
        private float _landingOffset;
        private Vector3 _baseLocalPosition;
        private SmartParkourCameraController _smartCamera;
        private ParkourCameraProfile _smartCameraProfile;
        private RouteCameraGraph _routeCameraGraph;
        private bool _effectsEnabled = true;
        private bool _touchInputMode;
        private float _baseFieldOfView;
        private float _maximumFieldOfView;
        private LandingView _landingView;
        private bool _flowTouchInput;
        private float _evaluationPitch;
        private float? _evaluationDefaultPitch;

        public void ConfigureLandingView(LandingViewSettings settings)
        {
            _landingView = settings == null ? null : new LandingView(settings);
            _evaluationDefaultPitch = settings?.initialPitch;
            _evaluationPitch = 0f;
            ApplyPitchRotation();
        }

        public Vector2 CurrentLookDelta { get; private set; }
        public bool EffectsEnabled => _effectsEnabled;
        public float Pitch => _pitch;
        public float Yaw => _playerYaw == null ? 0f : _playerYaw.eulerAngles.y;
        public float RenderedYaw => _playerYaw == null
            ? transform.eulerAngles.y
            : _playerYaw.eulerAngles.y + _smartYawOffset;
        public float CurrentFieldOfView => _camera == null ? 0f : _camera.fieldOfView;
        public bool ManualLookActive { get; private set; }
        public bool SmartCameraActive { get; private set; }
        public SmartParkourCameraOutput SmartCameraState { get; private set; }
        public AutoCameraProfileKind CurrentAutoCameraProfile { get; private set; } =
            AutoCameraProfileKind.Balanced;
        public float CameraPitchTarget => _pitch + _autoPitchOffset + _landingAssistPitch + _evaluationPitch;

        public void Initialize(
            Transform playerYaw,
            UnityEngine.Camera cameraComponent,
            CameraProfile profile,
            ParkourCameraProfile smartCameraProfile = null,
            RouteCameraGraph routeCameraGraph = null)
        {
            _playerYaw = playerYaw;
            _camera = cameraComponent;
            _profile = profile != null ? profile : CameraProfile.CreateRuntimeDefault();
            _smartCameraProfile = smartCameraProfile != null
                ? smartCameraProfile
                : ParkourCameraProfile.CreateRuntimeDefault();
            _routeCameraGraph = routeCameraGraph;
            _smartCamera = new SmartParkourCameraController(_smartCameraProfile, _routeCameraGraph);
            _baseLocalPosition = transform.localPosition;
            _baseFieldOfView = ResolvePreferredFieldOfView(_profile);
            _maximumFieldOfView = Mathf.Max(
                _baseFieldOfView,
                _profile.MaximumFieldOfView + (_baseFieldOfView - _profile.BaseFieldOfView));
            _camera.fieldOfView = _baseFieldOfView;
            _pitch = _profile.DefaultGameplayPitch;
            ApplyPitchRotation();
        }

        public void ConfigureSmartCamera(
            ParkourCameraProfile smartCameraProfile,
            RouteCameraGraph routeCameraGraph)
        {
            _smartCameraProfile = smartCameraProfile != null
                ? smartCameraProfile
                : _smartCameraProfile ?? ParkourCameraProfile.CreateRuntimeDefault();
            _routeCameraGraph = routeCameraGraph;
            _smartCamera ??= new SmartParkourCameraController(_smartCameraProfile, _routeCameraGraph);
            _smartCamera.Configure(_smartCameraProfile, _routeCameraGraph);
        }

        public void ApplyLook(
            IPlayerInputSource input,
            PlayerInputMode inputMode,
            float deltaTime,
            ParkourMotor motor = null)
        {
            if (_playerYaw == null || _profile == null)
            {
                return;
            }

            _flowTouchInput = inputMode == PlayerInputMode.Touch
                && input is IFlowSteeringInputSource flowInput && flowInput.FlowSteeringEnabled;
            if (!_flowTouchInput)
            {
                _landingView?.Reset();
                _evaluationPitch = 0f;
            }
            else if (_landingView != null && Mathf.Abs(input.LookDelta.y) > 0f)
            {
                // Start dragging from the rendered angle; never snap back on takeover.
                _pitch = Mathf.Clamp(_pitch + _evaluationPitch, _profile.MinimumPitch, _profile.MaximumPitch);
                _evaluationPitch = 0f;
                _landingView.YieldToManual();
            }

            if (inputMode == PlayerInputMode.Touch
                && input is IFlowSteeringInputSource flow
                && flow.FlowSteeringEnabled)
            {
                _touchInputMode = true;
                var verticalLook = ApplyTouchLookResponse(new Vector2(
                    0f,
                    input.LookDelta.y
                    * _profile.TouchSensitivity
                    * _profile.VerticalMultiplier));
                CurrentLookDelta = verticalLook;
                ManualLookActive = verticalLook.sqrMagnitude > 0.001f;
                SmoothLookTarget(verticalLook, deltaTime);
                _pitch = Mathf.Clamp(
                    _pitch - _smoothedLook.y,
                    _profile.MinimumPitch,
                    _profile.MaximumPitch);
                if (flow.AutoCameraProfile == AutoCameraProfileKind.SmartParkour && motor != null)
                {
                    ApplySmartCamera(input, motor, deltaTime, useOutputPitch: false);
                }
                else
                {
                    AdoptSmartYawIntoPlayer();
                    SmartCameraActive = false;
                    SmartCameraState = SmartParkourCameraOutput.Inactive(RenderedYaw, _pitch);
                }

                ApplyPitchRotation();

                return;
            }

            _touchInputMode = inputMode == PlayerInputMode.Touch;
            AdoptSmartYawIntoPlayer();
            SmartCameraActive = false;
            SmartCameraState = SmartParkourCameraOutput.Inactive(RenderedYaw, _pitch);
            var sensitivity = inputMode == PlayerInputMode.Touch
                ? _profile.TouchSensitivity
                : _profile.Sensitivity;
            if (inputMode == PlayerInputMode.Touch && _profile.TouchAutoSteerEnabled)
            {
                var steer = Mathf.Abs(input.Move.x) <= _profile.TouchSteerDeadZone
                    ? 0f
                    : input.Move.x;
                _playerYaw.Rotate(
                    Vector3.up,
                    steer * _profile.TouchSteerDegreesPerSecond * deltaTime,
                    Space.World);
            }

            var target = new Vector2(
                input.LookDelta.x * sensitivity * _profile.HorizontalMultiplier,
                input.LookDelta.y * sensitivity * _profile.VerticalMultiplier);
            if (inputMode == PlayerInputMode.Touch)
            {
                target = ApplyTouchLookResponse(target);
            }

            CurrentLookDelta = target;
            ManualLookActive = target.sqrMagnitude > 0.001f;
            SmoothLookTarget(target, deltaTime);

            _playerYaw.Rotate(Vector3.up, _smoothedLook.x, Space.World);
            _pitch = Mathf.Clamp(
                _pitch - _smoothedLook.y,
                _profile.MinimumPitch,
                _profile.MaximumPitch);
            ApplyPitchRotation();
        }

        public void UpdatePresentation(
            float horizontalSpeed,
            float maximumSpeed,
            bool grounded,
            bool surfing,
            float verticalSpeed,
            float deltaTime)
        {
            if (_camera == null || _profile == null)
            {
                return;
            }

            var speed01 = maximumSpeed <= 0f
                ? 0f
                : Mathf.Clamp01(horizontalSpeed / maximumSpeed);
            var bob = Vector3.zero;
            if (_effectsEnabled && _profile.BobEnabled && grounded && speed01 > 0.1f)
            {
                _bobTime += deltaTime * _profile.BobFrequency * Mathf.Lerp(0.4f, 1f, speed01);
                bob = new Vector3(
                    Mathf.Cos(_bobTime * 0.5f) * _profile.BobAmplitude * 0.45f,
                    Mathf.Sin(_bobTime) * _profile.BobAmplitude,
                    0f);
            }
            else
            {
                _bobTime = 0f;
            }

            _landingOffset = Mathf.MoveTowards(_landingOffset, 0f, deltaTime * 1.6f);
            transform.localPosition = _baseLocalPosition
                + bob
                + Vector3.down * _landingOffset;
            var targetFov = _baseFieldOfView;
            if (_effectsEnabled && _profile.SpeedFovEnabled)
            {
                targetFov += _profile.SpeedFovIncrease * speed01;
                if (!grounded)
                {
                    targetFov += _profile.AirFovIncrease;
                }
                if (surfing)
                {
                    targetFov += _profile.SurfFovIncrease;
                }
            }

            targetFov = Mathf.Clamp(
                targetFov,
                _baseFieldOfView,
                _maximumFieldOfView);
            var fovBlendSpeed = targetFov > _camera.fieldOfView
                ? _profile.FovBlendInSpeed
                : _profile.FovBlendOutSpeed;
            _camera.fieldOfView = Mathf.Lerp(
                _camera.fieldOfView,
                targetFov,
                1f - Mathf.Exp(-fovBlendSpeed * deltaTime));

            var landingAssistTarget = !_touchInputMode
                && ShouldApplyLandingAwareness(grounded, verticalSpeed)
                ? _profile.LandingAwarenessPitch
                : 0f;
            var automaticPitchTarget = _touchInputMode || ManualLookActive
                ? 0f
                : ResolveAutomaticPitchTarget(
                    grounded,
                    verticalSpeed,
                    horizontalSpeed);
            _autoPitchOffset = Mathf.Lerp(
                _autoPitchOffset,
                automaticPitchTarget,
                1f - Mathf.Exp(-_profile.LandingAwarenessBlendSpeed * deltaTime));
            _landingAssistPitch = Mathf.Lerp(
                _landingAssistPitch,
                landingAssistTarget,
                1f - Mathf.Exp(-_profile.LandingAwarenessBlendSpeed * deltaTime));
            _evaluationPitch = _landingView?.Tick(_flowTouchInput && _effectsEnabled,
                grounded, surfing, verticalSpeed, _pitch, deltaTime) ?? 0f;
            ApplyPitchRotation();
        }

        public void NotifyLanding(float landingSpeed)
        {
            if (!_effectsEnabled || _profile == null || !_profile.LandingResponseEnabled)
            {
                return;
            }

            _landingOffset = Mathf.Min(
                _profile.LandingResponse,
                landingSpeed * 0.006f);
        }

        public void ToggleEffects()
        {
            _effectsEnabled = !_effectsEnabled;
            if (!_effectsEnabled && _camera != null)
            {
                _camera.fieldOfView = _baseFieldOfView;
                transform.localPosition = _baseLocalPosition;
                _landingAssistPitch = 0f;
                _evaluationPitch = 0f;
                _landingView?.Reset();
                _smartYawOffset = 0f;
                ApplyPitchRotation();
            }
        }

        public void ResetView(Quaternion playerRotation, float? preservedPitch = null)
        {
            if (_playerYaw != null)
            {
                _playerYaw.rotation = Quaternion.Euler(0f, playerRotation.eulerAngles.y, 0f);
            }

            _pitch = _profile == null
                ? 0f
                : Mathf.Clamp(
                    preservedPitch ?? _evaluationDefaultPitch ?? _profile.DefaultGameplayPitch,
                    _profile.MinimumPitch,
                    _profile.MaximumPitch);
            _touchInputMode = false;
            _flowTouchInput = false;
            _evaluationPitch = 0f;
            _landingView?.Reset();
            _landingAssistPitch = 0f;
            _autoPitchOffset = 0f;
            _smartYawOffset = 0f;
            _smoothedLook = Vector2.zero;
            _landingOffset = 0f;
            transform.localRotation = Quaternion.identity;
            transform.localPosition = _baseLocalPosition;
            _smartCamera?.Reset(playerRotation.eulerAngles.y, _pitch);
            SmartCameraActive = false;
            ApplyPitchRotation();
            if (_camera != null && _profile != null)
            {
                _camera.fieldOfView = _baseFieldOfView;
            }
        }

        public static float ResolvePreferredFieldOfView(CameraProfile profile)
        {
            var fallback = profile == null
                ? DefaultPreferredFieldOfView
                : profile.BaseFieldOfView;
            return Mathf.Clamp(
                PlayerPrefs.GetFloat(FieldOfViewPreferenceKey, fallback),
                MinimumPreferredFieldOfView,
                MaximumPreferredFieldOfView);
        }

        private bool ShouldApplyLandingAwareness(bool grounded, float verticalSpeed)
        {
            if (!_effectsEnabled
                || !_profile.LandingAwarenessEnabled
                || grounded
                || ManualLookActive
                || verticalSpeed > _profile.LandingAwarenessMinVerticalSpeed)
            {
                return false;
            }

            var origin = transform.position + transform.forward * 0.85f;
            return Physics.Raycast(
                origin,
                Vector3.down,
                _profile.LandingAwarenessRayDistance,
                ~0,
                QueryTriggerInteraction.Ignore);
        }

        public void SetAutoCameraProfile(AutoCameraProfileKind profile)
        {
            CurrentAutoCameraProfile = profile;
        }

        private void ApplySmartCamera(
            IPlayerInputSource input,
            ParkourMotor motor,
            float deltaTime,
            bool useOutputPitch)
        {
            _smartCamera ??= new SmartParkourCameraController(
                _smartCameraProfile ?? ParkourCameraProfile.CreateRuntimeDefault(),
                _routeCameraGraph);
            var output = _smartCamera.Update(
                new SmartParkourCameraInput(
                    _playerYaw.position,
                    _playerYaw.eulerAngles.y,
                    RenderedYaw,
                    _pitch,
                    motor.ActualHorizontalVelocity,
                    input.Move,
                    motor.IsGrounded,
                    motor.IsSurfing,
                    motor.VerticalSpeed,
                    deltaTime));
            var desiredOffset = Mathf.Clamp(
                Mathf.DeltaAngle(_playerYaw.eulerAngles.y, output.WorldYaw),
                -_smartCameraProfile.MaxRenderedYawOffset,
                _smartCameraProfile.MaxRenderedYawOffset);
            _smartYawOffset = desiredOffset;
            if (useOutputPitch)
            {
                _pitch = Mathf.Clamp(output.Pitch, _profile.MinimumPitch, _profile.MaximumPitch);
            }
            _autoPitchOffset = 0f;
            _landingAssistPitch = 0f;
            SmartCameraActive = true;
            SmartCameraState = output;
            ApplyPitchRotation();
        }

        private void AdoptSmartYawIntoPlayer()
        {
            if (Mathf.Abs(_smartYawOffset) <= 0.001f || _playerYaw == null)
            {
                _smartYawOffset = 0f;
                return;
            }

            _playerYaw.Rotate(Vector3.up, _smartYawOffset, Space.World);
            _smartYawOffset = 0f;
            _smartCamera?.Reset(RenderedYaw, _pitch);
        }

        private float ResolveAutomaticPitchTarget(
            bool grounded,
            float verticalSpeed,
            float horizontalSpeed)
        {
            var profileScale = CurrentAutoCameraProfile == AutoCameraProfileKind.Direct
                ? 0.65f
                : CurrentAutoCameraProfile == AutoCameraProfileKind.Flow
                    ? 1.35f
                    : 1f;
            if (verticalSpeed > 3f)
            {
                return -1.6f * profileScale;
            }

            if (!grounded && verticalSpeed < -2f)
            {
                return Mathf.Clamp(horizontalSpeed * 0.12f, 1.5f, 4f) * profileScale;
            }

            return 0f;
        }

        private Vector2 ApplyTouchLookResponse(Vector2 target)
        {
            var magnitude = target.magnitude;
            if (magnitude <= _profile.MicroJitterDegrees)
            {
                return Vector2.zero;
            }

            var fineT = Mathf.Clamp01(magnitude / Mathf.Max(0.001f, _profile.FineLookThresholdDegrees));
            var gain = Mathf.Lerp(_profile.FineLookScale, 1f, fineT);
            var fastT = Mathf.Clamp01(
                (magnitude - _profile.FastSwipeThresholdDegrees)
                / Mathf.Max(0.001f, _profile.FastSwipeThresholdDegrees));
            gain *= Mathf.Lerp(1f, _profile.FastSwipeGain, fastT);
            return target * gain;
        }

        private void SmoothLookTarget(Vector2 target, float deltaTime)
        {
            if (_profile.SmoothingEnabled && _effectsEnabled)
            {
                var smoothing = Mathf.Max(0.0001f, _profile.SmoothingTime);
                var blend = 1f - Mathf.Exp(-deltaTime / smoothing);
                _smoothedLook = Vector2.Lerp(_smoothedLook, target, blend);
                if (target.sqrMagnitude <= 0.0001f
                    && _smoothedLook.magnitude <= _profile.MicroJitterDegrees)
                {
                    _smoothedLook = Vector2.zero;
                }
            }
            else
            {
                _smoothedLook = target;
            }
        }

        private void ApplyPitchRotation()
        {
            var presentationPitch = Mathf.Clamp(
                _pitch + _autoPitchOffset + _landingAssistPitch + _evaluationPitch,
                _profile.MinimumPitch,
                _profile.MaximumPitch);
            transform.localRotation = Quaternion.Euler(presentationPitch, _smartYawOffset, 0f);
        }
    }
}
