using Avoidance.Gameplay.Blocks;
using Avoidance.Input;
using UnityEngine;

namespace Avoidance.Gameplay.Player
{
    public enum MovementStateKind
    {
        Grounded,
        Airborne,
        Surfing
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public sealed class ParkourMotor : MonoBehaviour
    {
        private readonly JumpWindowState _jumpWindow = new JumpWindowState();
        private readonly Blocks.PlatformMotionTracker _platformTracker =
            new Blocks.PlatformMotionTracker();
        private CharacterController _controller;
        private MovementProfile _profile;
        private Vector3 _horizontalVelocity;
        private float _verticalVelocity;
        private Transform _groundTransform;
        private Vector3 _lastPlatformVelocity;
        private float _movementLockRemaining;
        private bool _wasGrounded;
        private bool _contactFresh;
        private bool _suspendedUntilRestore;
        public void SuspendUntilRestore() { _suspendedUntilRestore = true; }
        private float _preMoveVerticalVelocity;
        private float _secondsSinceLanding = float.MaxValue;
        private SurfSurface _surfSurface;
        private SurfProfile _surfProfile;
        private Vector3 _surfNormal = Vector3.up;
        private float _smoothedSteering;
        private bool _jumpInFlight;
        private Vector3 _jumpTakeoffPosition;
        private float _jumpTakeoffTime;
        private float _jumpApexY;
        private int _groundMask;
        private float _simulationTime;
        private float _surfDetachRemaining;
        private float _lastPressTime = float.NegativeInfinity;
        private bool _landedFromJump;
        public Vector3 DisplacementVelocity { get; private set; }
        public float AirProjectedSpeed { get; private set; }
        public float AirRequestedDelta { get; private set; }
        public float AirAppliedWishDelta { get; private set; }
        public Vector3 AirNetDelta { get; private set; }
        public bool AirEnergyLimited { get; private set; }
        public Vector3 ContactVelocityDelta { get; private set; }
        public bool SafetyLimited { get; private set; }
        public float LastJumpPressContactOffset { get; private set; }
        public float LastCompleteTakeoffRetention { get; private set; } = 1;

        public Vector3 Velocity => _horizontalVelocity + Vector3.up * _verticalVelocity;
        public float HorizontalSpeed => _horizontalVelocity.magnitude;
        public float VerticalSpeed => _verticalVelocity;
        public bool IsGrounded { get; private set; }
        public bool IsSurfing => _surfSurface != null;
        public MovementStateKind MovementState => IsSurfing
            ? MovementStateKind.Surfing
            : IsGrounded
                ? MovementStateKind.Grounded
                : MovementStateKind.Airborne;
        public bool JumpBuffered => _jumpWindow.IsJumpBuffered;
        public bool CoyoteEligible => _jumpWindow.IsCoyoteEligible;
        public MovementProfile Profile => _profile;
        public Transform GroundTransform => _groundTransform;
        public Vector2 LastMoveInput { get; private set; }
        public Vector3 LastProjectedForward { get; private set; } = Vector3.forward;
        public Vector3 LastProjectedRight { get; private set; } = Vector3.right;
        public Vector3 LastDesiredDirection { get; private set; }
        public Vector3 LastDesiredVelocity { get; private set; }
        public float DesiredHeadingYaw { get; private set; }
        public float CurrentHeadingYaw => transform.eulerAngles.y;
        public float VelocityHeadingYaw { get; private set; }
        public float HeadingVelocityDelta { get; private set; }
        public float LastThrottle { get; private set; }
        public float LastSteering { get; private set; }
        public float LastHeadingYawRate { get; private set; }
        public Vector3 ActualHorizontalVelocity => _horizontalVelocity;
        public float LateralVelocity { get; private set; }
        public Vector3 LastExternalImpulse { get; private set; }
        public int JumpCount { get; private set; }
        public float LastLandingSpeed { get; private set; }
        public float LastTakeoffHorizontalSpeed { get; private set; }
        public float LastTakeoffVerticalVelocity { get; private set; }
        public float LastTakeoffMomentumRetention { get; private set; } = 1f;
        public float LastJumpApexHeight { get; private set; }
        public float LastJumpAirtime { get; private set; }
        public float LastJumpHorizontalDistance { get; private set; }
        public float MomentumRatio => _profile == null || _profile.BaseRunSpeed <= 0f
            ? 0f
            : HorizontalSpeed / _profile.BaseRunSpeed;
        public float PeakHorizontalSpeed { get; private set; }
        public float SurfSpeed => IsSurfing ? Velocity.magnitude : 0f;

        public event System.Action<float> Landed;
        public event System.Action Jumped;
        public event System.Action<float> Simulated;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _profile ??= MovementProfile.CreateRuntimeDefault();
            var groundLayer = LayerMask.NameToLayer("Ground");
            _groundMask = groundLayer >= 0 ? 1 << groundLayer : ~0;
        }

        public void Initialize(MovementProfile profile)
        {
            _controller ??= GetComponent<CharacterController>();
            SetProfile(profile);
            ProbeGround();
        }

        public void SetProfile(MovementProfile profile)
        {
            _profile = profile != null ? profile : MovementProfile.CreateRuntimeDefault();
        }

        public void Simulate(IPlayerInputSource input, float deltaTime)
        {
            if (_suspendedUntilRestore || _controller == null || !_controller.enabled || deltaTime <= 0f)
            {
                return;
            }

            _movementLockRemaining = Mathf.Max(0f, _movementLockRemaining - deltaTime);
            AirProjectedSpeed = AirRequestedDelta = AirAppliedWishDelta = 0;
            AirNetDelta = ContactVelocityDelta = Vector3.zero;
            AirEnergyLimited = SafetyLimited = false;
            _simulationTime += deltaTime;
            _surfDetachRemaining = Mathf.Max(0, _surfDetachRemaining - deltaTime);
            if (input.JumpPressed) _lastPressTime = _simulationTime;
            _secondsSinceLanding += deltaTime;
            ApplyMovingPlatformDelta(deltaTime);
            if (_suspendedUntilRestore) return;
            ProbeGround();
            if (_profile.MovementMastery) HandleLanding();
            _jumpWindow.Tick(
                deltaTime,
                IsGrounded || IsSurfing,
                input.JumpPressed || (_profile.HoldToHop
                    && input is IHeldJumpInputSource held && held.JumpHeld
                    && (IsGrounded || IsSurfing)),
                _profile.CoyoteTime,
                _profile.JumpBufferDuration);

            UpdateHorizontalVelocity(input, deltaTime);
            UpdateVerticalVelocity(deltaTime);
            ClampVelocityToSafetyLimit();

            _preMoveVerticalVelocity = _verticalVelocity;
            var movement = (_horizontalVelocity + Vector3.up * _verticalVelocity) * deltaTime;
            var beforeMove = transform.position;
            var collisionFlags = _controller.Move(movement);
            if (_suspendedUntilRestore) return;
            _contactFresh = true;
            DisplacementVelocity = (transform.position - beforeMove) / deltaTime;
            if ((collisionFlags & CollisionFlags.Above) != 0 && _verticalVelocity > 0f)
            {
                _verticalVelocity = 0f;
            }

            ProbeGround();
            ClampVelocityToSafetyLimit();
            PeakHorizontalSpeed = Mathf.Max(PeakHorizontalSpeed, HorizontalSpeed);
            HandleLanding();
            Simulated?.Invoke(deltaTime);
        }

        public void ResetMotion(Vector3 position, Quaternion rotation, float movementLockDuration)
        {
            _controller ??= GetComponent<CharacterController>();
            _controller.enabled = false;
            transform.SetPositionAndRotation(position, rotation);
            _controller.enabled = true;
            _horizontalVelocity = Vector3.zero;
            DisplacementVelocity = Vector3.zero;
            AirProjectedSpeed = AirRequestedDelta = AirAppliedWishDelta = 0;
            AirNetDelta = ContactVelocityDelta = Vector3.zero;
            AirEnergyLimited = SafetyLimited = false;
            _surfDetachRemaining = 0;
            _landedFromJump = false;
            _lastPressTime = float.NegativeInfinity;
            LastJumpPressContactOffset = 0;
            LastCompleteTakeoffRetention = 1;
            _verticalVelocity = 0f;
            LastMoveInput = Vector2.zero;
            LastDesiredDirection = Vector3.zero;
            LastDesiredVelocity = Vector3.zero;
            DesiredHeadingYaw = rotation.eulerAngles.y;
            VelocityHeadingYaw = rotation.eulerAngles.y;
            HeadingVelocityDelta = 0f;
            LastThrottle = 0f;
            LastSteering = 0f;
            LastHeadingYawRate = 0f;
            _smoothedSteering = 0f;
            _jumpInFlight = false;
            _jumpTakeoffPosition = position;
            _jumpTakeoffTime = _simulationTime;
            _jumpApexY = position.y;
            LastTakeoffHorizontalSpeed = 0f;
            LastTakeoffVerticalVelocity = 0f;
            LastTakeoffMomentumRetention = 1f;
            LastJumpApexHeight = 0f;
            LastJumpAirtime = 0f;
            LastJumpHorizontalDistance = 0f;
            LastExternalImpulse = Vector3.zero;
            LateralVelocity = 0f;
            PeakHorizontalSpeed = 0f;
            _secondsSinceLanding = float.MaxValue;
            ClearSurfState();
            _lastPlatformVelocity = Vector3.zero;
            _groundTransform = null;
            _movementLockRemaining = Mathf.Max(0f, movementLockDuration);
            _jumpWindow.Reset();
            _wasGrounded = false;
            _contactFresh = false;
            _suspendedUntilRestore = false;
            ProbeGround();
        }

        public void ApplyLaunch(Vector3 launchVelocity, bool replaceHorizontal)
        {
            var horizontal = Vector3.ProjectOnPlane(launchVelocity, Vector3.up);
            _horizontalVelocity = replaceHorizontal
                ? horizontal
                : _horizontalVelocity + horizontal;
            if (launchVelocity.y > _verticalVelocity)
            {
                _verticalVelocity = launchVelocity.y;
            }

            LastExternalImpulse = launchVelocity;
            IsGrounded = false;
            ClearSurfState();
            _groundTransform = null;
            _jumpWindow.Reset();
            ClampVelocityToSafetyLimit();
        }

        public void ApplyWaterFlow(
            Vector3 flowDirection,
            float flowForce,
            float maxAddedVelocity,
            float verticalInfluence,
            float drag,
            float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            var delta = WaterFlowMath.ComputeHorizontalDelta(
                _horizontalVelocity,
                flowDirection,
                flowForce,
                maxAddedVelocity,
                deltaTime);
            _horizontalVelocity += delta;

            var horizontal = Vector3.ProjectOnPlane(flowDirection, Vector3.up);
            if (drag > 0f && horizontal.sqrMagnitude > 0.001f)
            {
                horizontal.Normalize();
                var alongFlow = Vector3.Project(_horizontalVelocity, horizontal);
                var lateral = _horizontalVelocity - alongFlow;
                lateral = Vector3.MoveTowards(lateral, Vector3.zero, drag * deltaTime);
                _horizontalVelocity = alongFlow + lateral;
            }

            if (verticalInfluence > 0f && Mathf.Abs(flowDirection.y) > 0.001f)
            {
                _verticalVelocity = Mathf.Clamp(
                    _verticalVelocity + flowDirection.normalized.y * flowForce * verticalInfluence * deltaTime,
                    -_profile.MaximumFallSpeed,
                    _profile.JumpVelocity);
            }

            ClampVelocityToSafetyLimit();
        }

        private void UpdateHorizontalVelocity(IPlayerInputSource input, float deltaTime)
        {
            var moveInput = _movementLockRemaining > 0f ? Vector2.zero : input.Move;
            if (!_profile.MovementFoundation && input is IFlowSteeringInputSource flow && flow.FlowSteeringEnabled)
            {
                UpdateFlowSteeringVelocity(moveInput, deltaTime);
                return;
            }

            UpdateManualVelocity(moveInput, deltaTime);
        }

        private void UpdateFlowSteeringVelocity(Vector2 moveInput, float deltaTime)
        {
            var input = new Vector2(
                Mathf.Clamp(moveInput.x, -1f, 1f),
                Mathf.Clamp(moveInput.y, -1f, 1f));
            LastMoveInput = input;
            LastThrottle = input.y;
            LastSteering = input.x;

            var speed01 = Mathf.Clamp01(_profile.BaseRunSpeed <= 0f
                ? 0f
                : HorizontalSpeed / _profile.SoftMomentumLimit);
            var stateStrength = IsSurfing
                ? _profile.SurfSteeringStrength
                : IsGrounded
                    ? _profile.GroundSteeringStrength
                    : _profile.AirSteeringStrength;
            var speedYawRate = Mathf.Lerp(
                _profile.LowSpeedYawRate,
                _profile.HighSpeedYawRate,
                speed01);
            var steeringMagnitude = Mathf.Sign(input.x)
                * Mathf.Pow(Mathf.Abs(input.x), _profile.SteeringResponseCurve);
            var steeringResponseRate = Mathf.Abs(steeringMagnitude) > Mathf.Abs(_smoothedSteering)
                ? _profile.SteeringAcceleration
                : _profile.SteeringDeceleration;
            _smoothedSteering = Mathf.MoveTowards(
                _smoothedSteering,
                steeringMagnitude,
                steeringResponseRate * deltaTime);
            var yawRate = Mathf.Min(
                _profile.MaximumHeadingDeltaPerSecond,
                Mathf.Min(_profile.SteeringYawRate, speedYawRate) * stateStrength);
            var yawDelta = _smoothedSteering * yawRate * deltaTime;
            transform.Rotate(Vector3.up, yawDelta, Space.World);
            LastHeadingYawRate = deltaTime <= 0f ? 0f : yawDelta / deltaTime;
            LastSteering = _smoothedSteering;
            DesiredHeadingYaw = transform.eulerAngles.y;

            var forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            var right = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;
            LastProjectedForward = forward;
            LastProjectedRight = right;
            var throttle = Mathf.Clamp(input.y, -1f, 1f);
            var inputAmount = Mathf.Clamp01(Mathf.Abs(throttle));
            var desiredDirection = inputAmount <= 0.001f
                ? Vector3.zero
                : forward * Mathf.Sign(throttle);
            if (_profile.RealRouteAirControl)
                desiredDirection = RouteAirControlMath.FlowWish(forward, input, _profile.FlowWishAngle);
            LastDesiredDirection = desiredDirection;
            if (_profile.RealRouteAirControl && desiredDirection.sqrMagnitude > .001f)
                DesiredHeadingYaw = Mathf.Atan2(desiredDirection.x, desiredDirection.z) * Mathf.Rad2Deg;
            var targetSpeed = Mathf.Lerp(_profile.WalkSpeed, _profile.BaseRunSpeed, inputAmount);
            var desiredVelocity = desiredDirection * targetSpeed * inputAmount;
            LastDesiredVelocity = desiredVelocity;

            if (IsSurfing)
            {
                var surfVelocity = SurfMovementMath.ComputeVelocity(
                    Velocity,
                    _surfNormal,
                    desiredDirection,
                    inputAmount,
                    _surfProfile,
                    deltaTime, _profile.MovementMastery, _profile.RealRouteAirControl);
                _horizontalVelocity = Vector3.ProjectOnPlane(surfVelocity, Vector3.up);
                _verticalVelocity = Mathf.Clamp(
                    surfVelocity.y,
                    -_profile.MaximumFallSpeed,
                    _profile.JumpVelocity);
            }
            else if (IsGrounded)
            {
                if (_profile.MovementMastery && _jumpWindow.IsJumpBuffered && _jumpWindow.IsCoyoteEligible)
                    return; // Resolve takeoff before ground braking, including at 30 Hz.
                var rate = inputAmount > 0.001f
                    ? _profile.Acceleration
                    : _profile.Deceleration + _profile.GroundFriction;
                if (_profile.RealRouteAirControl && _landedFromJump
                    && _secondsSinceLanding <= _profile.PerfectHopWindow
                    && Vector3.Dot(_horizontalVelocity.normalized, desiredDirection) > .25f)
                    desiredVelocity = desiredDirection * Mathf.Max(HorizontalSpeed, desiredVelocity.magnitude);
                _horizontalVelocity = Vector3.MoveTowards(
                    _horizontalVelocity,
                    desiredVelocity,
                    rate * deltaTime);
            }
            else if (inputAmount > 0.001f)
            {
                if (_profile.RealRouteAirControl)
                {
                    AccelerateRouteAir(desiredDirection, inputAmount, deltaTime);
                    return;
                }
                if (_profile.MovementMastery)
                {
                    AcceleratePreviousAir(desiredDirection, _profile.AirWishSpeed, inputAmount, deltaTime);
                    return;
                }
                var wishSpeed = Mathf.Min(
                    _profile.AirWishSpeed * inputAmount,
                    _profile.BaseRunSpeed + _profile.MaxAirSpeedContribution);
                var currentAlongWish = Vector3.Dot(_horizontalVelocity, desiredDirection);
                var addSpeed = wishSpeed - currentAlongWish;
                if (addSpeed > 0f)
                {
                    _horizontalVelocity += desiredDirection
                        * Mathf.Min(addSpeed, _profile.AirAcceleration * inputAmount * deltaTime);
                }

                if (HorizontalSpeed < _profile.SoftMomentumLimit)
                {
                    _horizontalVelocity += desiredDirection
                        * _profile.AirMomentumGain
                        * inputAmount
                        * deltaTime;
                }
            }

            LateralVelocity = MovementVectorMath.ResolveLateralVelocity(
                _horizontalVelocity,
                right);
            if (_horizontalVelocity.sqrMagnitude > 0.001f)
            {
                VelocityHeadingYaw = Quaternion.LookRotation(_horizontalVelocity.normalized, Vector3.up).eulerAngles.y;
                HeadingVelocityDelta = Mathf.Abs(Mathf.DeltaAngle(CurrentHeadingYaw, VelocityHeadingYaw));
            }
            else
            {
                VelocityHeadingYaw = CurrentHeadingYaw;
                HeadingVelocityDelta = 0f;
            }
        }

        private void UpdateManualVelocity(Vector2 moveInput, float deltaTime)
        {
            var movement = MovementVectorMath.ResolveCameraRelative(
                transform.rotation,
                moveInput);
            LastMoveInput = movement.Input;
            LastProjectedForward = movement.ProjectedForward;
            LastProjectedRight = movement.ProjectedRight;
            LastDesiredDirection = movement.DesiredDirection;
            var inputAmount = Mathf.Clamp01(movement.Input.magnitude);
            var targetSpeed = Mathf.Lerp(_profile.WalkSpeed, _profile.BaseRunSpeed, inputAmount);
            var desiredVelocity = movement.DesiredDirection * targetSpeed * inputAmount;
            LastDesiredVelocity = desiredVelocity;

            if (IsSurfing)
            {
                var surfVelocity = SurfMovementMath.ComputeVelocity(
                    Velocity,
                    _surfNormal,
                    movement.DesiredDirection,
                    inputAmount,
                    _surfProfile,
                    deltaTime, _profile.MovementMastery, _profile.RealRouteAirControl);
                _horizontalVelocity = Vector3.ProjectOnPlane(surfVelocity, Vector3.up);
                _verticalVelocity = Mathf.Clamp(
                    surfVelocity.y,
                    -_profile.MaximumFallSpeed,
                    _profile.JumpVelocity);
                LateralVelocity = MovementVectorMath.ResolveLateralVelocity(
                    _horizontalVelocity,
                    movement.ProjectedRight);
                return;
            }

            if (IsGrounded)
            {
                if (_profile.MovementMastery && _jumpWindow.IsJumpBuffered && _jumpWindow.IsCoyoteEligible)
                    return;
                if (inputAmount > 0.001f)
                {
                    var currentSpeed = HorizontalSpeed;
                    var alignment = currentSpeed <= 0.001f || movement.DesiredDirection.sqrMagnitude <= 0.001f
                        ? 1f
                        : Vector3.Dot(_horizontalVelocity.normalized, movement.DesiredDirection.normalized);
                    if (alignment > 0.25f && currentSpeed > targetSpeed)
                    {
                        desiredVelocity = movement.DesiredDirection.normalized
                            * (_profile.MovementFoundation ? currentSpeed : Mathf.Min(currentSpeed, _profile.SoftMomentumLimit));
                    }
                    else if (alignment < -0.25f)
                    {
                        _horizontalVelocity = Vector3.MoveTowards(
                            _horizontalVelocity,
                            Vector3.zero,
                            _profile.Deceleration
                            * _profile.GroundDirectionResponse
                            * deltaTime);
                    }
                }

                var rate = inputAmount > 0.001f
                    ? _profile.Acceleration * _profile.GroundDirectionResponse
                    : _profile.Deceleration + _profile.GroundFriction;
                LastDesiredVelocity = desiredVelocity;
                _horizontalVelocity = Vector3.MoveTowards(
                    _horizontalVelocity,
                    desiredVelocity,
                    rate * deltaTime);
                LateralVelocity = MovementVectorMath.ResolveLateralVelocity(
                    _horizontalVelocity,
                    movement.ProjectedRight);
                return;
            }

            if (inputAmount > 0.001f && movement.DesiredDirection.sqrMagnitude > 0.001f)
            {
                if (_profile.RealRouteAirControl)
                {
                    AccelerateRouteAir(movement.DesiredDirection, inputAmount, deltaTime);
                    return;
                }
                if (_profile.MovementMastery)
                {
                    AcceleratePreviousAir(movement.DesiredDirection, _profile.AirWishSpeed * inputAmount, inputAmount, deltaTime);
                    return;
                }
                var wishDirection = movement.DesiredDirection.normalized;
                var wishSpeed = Mathf.Min(
                    _profile.AirWishSpeed * inputAmount,
                    _profile.BaseRunSpeed + _profile.MaxAirSpeedContribution);
                var currentAlongWish = Vector3.Dot(_horizontalVelocity, wishDirection);
                var addSpeed = wishSpeed - currentAlongWish;
                if (addSpeed > 0f)
                {
                    var accelerationSpeed = Mathf.Min(
                        addSpeed,
                        _profile.AirAcceleration * Mathf.Max(0.25f, inputAmount) * deltaTime);
                    _horizontalVelocity += wishDirection * accelerationSpeed;
                }

                var currentSpeed = HorizontalSpeed;
                if (currentSpeed > 0.001f)
                {
                    var turnedDirection = Vector3.RotateTowards(
                        _horizontalVelocity.normalized,
                        wishDirection,
                        _profile.AirTurnStrength * deltaTime,
                        0f);
                    var turnedVelocity = turnedDirection * currentSpeed;
                    _horizontalVelocity = Vector3.Lerp(
                        _horizontalVelocity,
                        turnedVelocity,
                        _profile.AirControl * 0.35f);
                }

                if (currentAlongWish > 0f && HorizontalSpeed < _profile.SoftMomentumLimit)
                {
                    _horizontalVelocity += wishDirection
                        * _profile.AirMomentumGain
                        * inputAmount
                        * deltaTime;
                }
            }

            LateralVelocity = MovementVectorMath.ResolveLateralVelocity(
                _horizontalVelocity,
                movement.ProjectedRight);
        }

        private void AccelerateRouteAir(Vector3 wish, float amount, float dt)
        {
            var before = _horizontalVelocity;
            if (_profile.ResponsiveAirControl)
            {
                _horizontalVelocity = RouteAirControlMath.AccelerateResponsive(before, wish,
                    _profile.AirWishSpeed * amount, _profile.AirAcceleration, _profile.AirBraking,
                    amount, _profile.ProjectionSteering, dt, out var projection, out var request);
                AirProjectedSpeed = projection; AirRequestedDelta = request;
                AirNetDelta = _horizontalVelocity - before;
                AirAppliedWishDelta = Vector3.Dot(AirNetDelta, wish.normalized);
                LateralVelocity = Vector3.Dot(_horizontalVelocity, LastProjectedRight);
                VelocityHeadingYaw = Mathf.Atan2(_horizontalVelocity.x, _horizontalVelocity.z) * Mathf.Rad2Deg;
                HeadingVelocityDelta = Mathf.Abs(Mathf.DeltaAngle(CurrentHeadingYaw, VelocityHeadingYaw));
                return;
            }
            var wishSpeed = _profile.MovementFoundation
                ? Mathf.Max(_profile.AirWishSpeed * amount, before.magnitude * _profile.OverspeedWishRatio)
                : _profile.AirWishSpeed;
            _horizontalVelocity = RouteAirControlMath.Accelerate(before, wish, wishSpeed,
                _profile.AirAcceleration, _profile.AirBraking, amount, _profile.SoftMomentumLimit, dt,
                out var projected, out var requested, out _, out var limited);
            AirProjectedSpeed = projected; AirRequestedDelta = requested;
            AirEnergyLimited = limited;
            AirNetDelta = _horizontalVelocity - before;
            // Report the velocity change that survived the energy bound, as the
            // previous candidate does. The pre-bound allowance overstates control.
            AirAppliedWishDelta = Vector3.Dot(AirNetDelta, wish.normalized);
            LateralVelocity = Vector3.Dot(_horizontalVelocity, LastProjectedRight);
            VelocityHeadingYaw = Mathf.Atan2(_horizontalVelocity.x, _horizontalVelocity.z) * Mathf.Rad2Deg;
            HeadingVelocityDelta = Mathf.Abs(Mathf.DeltaAngle(CurrentHeadingYaw, VelocityHeadingYaw));
        }

        private void AcceleratePreviousAir(Vector3 wish, float wishSpeed, float amount, float dt)
        {
            var before = _horizontalVelocity;
            AirProjectedSpeed = Vector3.Dot(before, wish);
            AirRequestedDelta = _profile.AirAcceleration * amount * dt;
            float projectedAllowance = Mathf.Min(Mathf.Max(0, wishSpeed - AirProjectedSpeed), AirRequestedDelta);
            _horizontalVelocity = MasteryMovementMath.Accelerate(before, wish, wishSpeed,
                _profile.AirAcceleration * amount, _profile.SoftMomentumLimit, dt);
            AirNetDelta = _horizontalVelocity - before;
            AirAppliedWishDelta = Vector3.Dot(AirNetDelta, wish);
            AirEnergyLimited = AirAppliedWishDelta + .00001f < projectedAllowance;
        }

        private void UpdateVerticalVelocity(float deltaTime)
        {
            // Preserve accepted consumption ordering; only the new candidate keeps
            // a buffered tap pending through a restore movement lock.
            var jumpRequested = !(_profile.MovementFoundation && _movementLockRemaining > 0f)
                && _jumpWindow.TryConsumeJump();
            if (jumpRequested && _movementLockRemaining <= 0f)
            {
                float beforeTakeoff = HorizontalSpeed;
                ApplyTakeoffMomentumRetention();
                ApplyHopRetention();
                _verticalVelocity = _profile.JumpVelocity;
                IsGrounded = false;
                if (IsSurfing)
                {
                    _horizontalVelocity *= _surfProfile.ExitMomentumRetention;
                    _verticalVelocity *= _surfProfile.SurfJumpInfluence;
                    _surfDetachRemaining = _profile.SurfDetachDuration;
                    ClearSurfState();
                }

                LastCompleteTakeoffRetention = beforeTakeoff > .001f ? HorizontalSpeed / beforeTakeoff : 1;
                if (IsGrounded || _secondsSinceLanding < _profile.BufferedHopWindow)
                    LastJumpPressContactOffset = _lastPressTime - (_simulationTime - _secondsSinceLanding);

                BeginJumpMeasurement();
                JumpCount++;
                Jumped?.Invoke();
                return;
            }

            if (IsSurfing)
            {
                return;
            }

            if (IsGrounded && _verticalVelocity <= 0f)
            {
                _verticalVelocity = -Mathf.Max(1f, _profile.GroundSnap * 10f);
                return;
            }

            var gravity = _verticalVelocity > 0f
                ? _profile.JumpGravity
                : _profile.FallGravity;
            _verticalVelocity = Mathf.Max(
                -_profile.MaximumFallSpeed,
                _verticalVelocity - gravity * deltaTime);
        }

        private void ProbeGround()
        {
            var assistance = _profile.LandingAssistance ? _profile.EdgeTolerance : 0f;
            var radius = Mathf.Clamp(
                _controller.radius - 0.03f + assistance,
                0.05f,
                _controller.radius + 0.12f);
            var bottomSphereCenter = transform.TransformPoint(
                _controller.center
                + Vector3.down * (_controller.height * 0.5f - _controller.radius));
            var origin = bottomSphereCenter + Vector3.up * (assistance + 0.06f);
            var distance = _profile.GroundSnap + 0.12f;
            var hitGround = Physics.SphereCast(
                origin,
                radius,
                Vector3.down,
                out var hit,
                distance,
                _groundMask,
                QueryTriggerInteraction.Ignore);

            if (hitGround)
            {
                var surf = hit.collider.GetComponentInParent<SurfSurface>();
                if (surf != null && surf.IsValidSurface(hit.normal)
                    && (!_profile.MovementMastery || (_surfDetachRemaining <= 0
                        && Vector3.Dot(Velocity, hit.normal) <= .1f)))
                {
                    _surfSurface = surf;
                    _surfProfile = surf.Profile;
                    _surfNormal = hit.normal.normalized;
                    IsGrounded = false;
                    _groundTransform = null;
                    return;
                }
            }

            ClearSurfState();

            IsGrounded = ((_controller.isGrounded && (!_profile.ResponsiveAirControl || _contactFresh))
                    || (!_profile.RealRouteAirControl && hitGround && Vector3.Dot(hit.normal, Vector3.up) >= 0.55f))
                && _verticalVelocity <= 0.5f;
            var nextGround = IsGrounded && hit.collider != null ? hit.collider.transform : null;
            if (nextGround != _groundTransform)
            {
                if (_groundTransform != null && nextGround == null)
                {
                    _horizontalVelocity += Vector3.ProjectOnPlane(
                        _lastPlatformVelocity * _profile.MovingPlatformInheritance,
                        Vector3.up);
                }

                _groundTransform = nextGround;
                if (_groundTransform != null)
                {
                    _platformTracker.Reset(_groundTransform.position);
                }
            }
        }

        private void ApplyMovingPlatformDelta(float deltaTime)
        {
            if (_groundTransform == null)
            {
                _lastPlatformVelocity = Vector3.zero;
                return;
            }

            var delta = _platformTracker.Capture(_groundTransform.position);
            _lastPlatformVelocity = deltaTime > 0f ? delta / deltaTime : Vector3.zero;
            if (delta.sqrMagnitude > 0f)
            {
                // A horizontal carry Move clears Unity's ground flag unless it
                // also resolves contact. Settle only an already-owned support;
                // never turn the broad airborne probe into a landing detector.
                var settle = _profile.ResponsiveAirControl && IsGrounded && _verticalVelocity <= 0
                    ? Vector3.down * _controller.skinWidth : Vector3.zero;
                _controller.Move(delta * _profile.MovingPlatformInheritance + settle);
            }
        }

        private void ApplyHopRetention()
        {
            var speed = HorizontalSpeed;
            if (speed <= _profile.BaseRunSpeed || speed <= 0.001f)
            {
                return;
            }

            var result = MovementMomentumMath.ResolveHopRetention(
                _secondsSinceLanding,
                speed,
                _profile.BaseRunSpeed,
                _profile);
            _horizontalVelocity = _horizontalVelocity.normalized * result.SpeedAfterRetention;
        }

        private void ClampVelocityToSafetyLimit()
        {
            SafetyLimited |= HorizontalSpeed > _profile.HardVelocitySafetyLimit;
            _horizontalVelocity = MovementMomentumMath.ClampHorizontalVelocity(
                _horizontalVelocity,
                _profile.HardVelocitySafetyLimit);
            _verticalVelocity = Mathf.Clamp(
                _verticalVelocity,
                -_profile.MaximumFallSpeed,
                Mathf.Max(_profile.JumpVelocity, _profile.HardVelocitySafetyLimit));
        }

        private void ClearSurfState()
        {
            _surfSurface = null;
            _surfProfile = null;
            _surfNormal = Vector3.up;
        }

        private void HandleLanding()
        {
            if (IsGrounded && !_wasGrounded)
            {
                _landedFromJump = _jumpInFlight;
                LastJumpPressContactOffset = _lastPressTime - _simulationTime;
                LastLandingSpeed = Mathf.Max(0f, -_preMoveVerticalVelocity);
                FinishJumpMeasurement();
                _secondsSinceLanding = 0f;
                if (_profile.LandingMomentumRetention < 0.999f)
                {
                    _horizontalVelocity *= _profile.LandingMomentumRetention;
                }

                Landed?.Invoke(LastLandingSpeed);
            }

            if (!IsGrounded && _jumpInFlight)
            {
                _jumpApexY = Mathf.Max(_jumpApexY, transform.position.y);
            }

            _wasGrounded = IsGrounded;
        }

        private void ApplyTakeoffMomentumRetention()
        {
            LastTakeoffHorizontalSpeed = HorizontalSpeed;
            LastTakeoffMomentumRetention =
                _profile.TakeoffMomentumRetentionForSpeed(LastTakeoffHorizontalSpeed);
            if (_profile.MovementMastery && _landedFromJump && _secondsSinceLanding <= _profile.GoodHopWindow)
                LastTakeoffMomentumRetention = 1;
            _horizontalVelocity *= LastTakeoffMomentumRetention;
        }

        private void BeginJumpMeasurement()
        {
            _jumpInFlight = true;
            _jumpTakeoffPosition = transform.position;
            _jumpTakeoffTime = _simulationTime;
            _jumpApexY = transform.position.y;
            LastTakeoffVerticalVelocity = _verticalVelocity;
        }

        private void FinishJumpMeasurement()
        {
            if (!_jumpInFlight)
            {
                return;
            }

            _jumpInFlight = false;
            var horizontalStart = Vector3.ProjectOnPlane(_jumpTakeoffPosition, Vector3.up);
            var horizontalEnd = Vector3.ProjectOnPlane(transform.position, Vector3.up);
            LastJumpHorizontalDistance = Vector3.Distance(horizontalStart, horizontalEnd);
            LastJumpApexHeight = Mathf.Max(0f, _jumpApexY - _jumpTakeoffPosition.y);
            LastJumpAirtime = Mathf.Max(0f, _simulationTime - _jumpTakeoffTime);
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (_profile == null || !_profile.MovementMastery) return;
            // Ground contact remains owned by the existing ground probe/platform tracker.
            if (hit.normal.y >= .55f) return;
            var clipped = MasteryMovementMath.ClipIntoPlane(Velocity, hit.normal);
            ContactVelocityDelta += clipped - Velocity;
            _horizontalVelocity = Vector3.ProjectOnPlane(clipped, Vector3.up);
            _verticalVelocity = clipped.y;
        }
    }
}

