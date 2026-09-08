using System;
using UnityEngine;

namespace Avoidance.Gameplay.Player
{
    [CreateAssetMenu(menuName = "RYDERS BLOCK/Movement Profile", fileName = "Movement_Profile")]
    public sealed class MovementProfile : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _profileId = "movement.default";
        [SerializeField] private string _displayName = "Default";
        [SerializeField] private int _compatibilityVersion = 1;

        [Header("Ground movement")]
        [Tooltip("Opt-in Flow Lab strategy. Never enable on Campaign before physical acceptance.")]
        [SerializeField] private bool _movementMastery;
        [Range(0f, .3f)] [SerializeField] private float _surfDetachDuration = .12f;
        public bool MovementMastery => _movementMastery;
        [Header("Isolated real-route candidate")]
        [SerializeField] private bool _realRouteAirControl;
        [Range(0f, 90f)] [SerializeField] private float _flowWishAngle = 65f;
        [Min(0f)] [SerializeField] private float _airBraking = 30f;
        public bool RealRouteAirControl => _realRouteAirControl;
        public float FlowWishAngle => _flowWishAngle;
        public float AirBraking => _airBraking;
        public float SurfDetachDuration => _surfDetachDuration;
        [Min(0.1f)] [SerializeField] private float _walkSpeed = 5.4f;
        [Min(0.1f)] [SerializeField] private float _runSpeed = 7.8f;
        [Min(0.1f)] [SerializeField] private float _acceleration = 52f;
        [Min(0.1f)] [SerializeField] private float _deceleration = 62f;
        [Min(0f)] [SerializeField] private float _groundFriction = 12f;
        [Range(0.1f, 3f)] [SerializeField] private float _groundDirectionResponse = 1.12f;
        [Header("Flow steering")]
        [Range(45f, 540f)] [SerializeField] private float _steeringYawRate = 220f;
        [Range(45f, 540f)] [SerializeField] private float _lowSpeedYawRate = 260f;
        [Range(45f, 540f)] [SerializeField] private float _highSpeedYawRate = 150f;
        [Range(0f, 3f)] [SerializeField] private float _groundSteeringStrength = 1f;
        [Range(0f, 3f)] [SerializeField] private float _airSteeringStrength = 0.72f;
        [Range(0f, 3f)] [SerializeField] private float _surfSteeringStrength = 0.85f;
        [Range(45f, 720f)] [SerializeField] private float _maximumHeadingDeltaPerSecond = 260f;
        [Range(0.1f, 30f)] [SerializeField] private float _steeringAcceleration = 8f;
        [Range(0.1f, 30f)] [SerializeField] private float _steeringDeceleration = 12f;
        [Range(0.25f, 3f)] [SerializeField] private float _steeringResponseCurve = 1.15f;

        [Header("Momentum")]
        [Min(0.1f)] [SerializeField] private float _softMomentumLimit = 10.8f;
        [Min(0.1f)] [SerializeField] private float _hardVelocitySafetyLimit = 18f;

        [Header("Air movement")]
        [Min(0.1f)] [SerializeField] private float _airAcceleration = 13f;
        [Min(0.1f)] [SerializeField] private float _airWishSpeed = 8.2f;
        [Range(0f, 1f)] [SerializeField] private float _airControl = 0.72f;
        [Range(0f, 12f)] [SerializeField] private float _airTurnStrength = 4.7f;
        [Range(0f, 4f)] [SerializeField] private float _airMomentumGain = 0.32f;
        [Range(0f, 12f)] [SerializeField] private float _maxAirSpeedContribution = 3.1f;
        [Min(0.1f)] [SerializeField] private float _jumpHeight = 1.66f;
        [Min(0.1f)] [SerializeField] private float _gravity = 24f;
        [Range(0.15f, 0.8f)] [SerializeField] private float _timeToApex = 0.335f;
        [Range(0.5f, 3f)] [SerializeField] private float _gravityScale = 1f;
        [Range(1f, 3f)] [SerializeField] private float _fallGravityMultiplier = 1.38f;
        [Range(0.5f, 1f)] [SerializeField] private float _baseTakeoffMomentumRetention = 0.9f;
        [Range(0.5f, 1.15f)] [SerializeField] private float _highMomentumTakeoffRetention = 1f;
        [Range(0.25f, 4f)] [SerializeField] private float _takeoffMomentumRetentionPower = 1.6f;
        [Range(0.5f, 1.1f)] [SerializeField] private float _landingMomentumRetention = 0.99f;
        [Min(1f)] [SerializeField] private float _maximumFallSpeed = 32f;

        [Header("Bunny hop timing")]
        [Range(0f, 0.2f)] [SerializeField] private float _perfectHopWindow = 0.055f;
        [Range(0f, 0.3f)] [SerializeField] private float _goodHopWindow = 0.12f;
        [Range(0f, 0.35f)] [SerializeField] private float _bufferedHopWindow = 0.18f;
        [Range(0f, 1f)] [SerializeField] private float _perfectHopRetention = 1f;
        [Range(0f, 1f)] [SerializeField] private float _goodHopRetention = 0.96f;
        [Range(0f, 1f)] [SerializeField] private float _bufferedHopRetention = 0.9f;
        [Range(0f, 1f)] [SerializeField] private float _lateHopPenalty = 0.72f;

        [Header("Forgiveness")]
        [Range(0f, 0.3f)] [SerializeField] private float _coyoteTime = 0.15f;
        [Range(0f, 0.3f)] [SerializeField] private float _jumpBufferDuration = 0.17f;
        [Range(0f, 0.5f)] [SerializeField] private float _groundSnap = 0.22f;
        [Range(0f, 0.25f)] [SerializeField] private float _edgeTolerance = 0.08f;
        [SerializeField] private bool _landingAssistance = true;

        [Header("Platforms and restore")]
        [Range(0f, 1.5f)] [SerializeField] private float _movingPlatformInheritance = 1f;
        [Range(0f, 0.5f)] [SerializeField] private float _respawnMovementLockDuration = 0.08f;

        public string ProfileId => _profileId;
        public string DisplayName => _displayName;
        public int CompatibilityVersion => _compatibilityVersion;
        public float WalkSpeed => _walkSpeed;
        public float RunSpeed => _runSpeed;
        public float Acceleration => _acceleration;
        public float Deceleration => _deceleration;
        public float GroundFriction => _groundFriction;
        public float GroundDirectionResponse => _groundDirectionResponse;
        public float SteeringYawRate => _steeringYawRate;
        public float LowSpeedYawRate => _lowSpeedYawRate;
        public float HighSpeedYawRate => _highSpeedYawRate;
        public float GroundSteeringStrength => _groundSteeringStrength;
        public float AirSteeringStrength => _airSteeringStrength;
        public float SurfSteeringStrength => _surfSteeringStrength;
        public float MaximumHeadingDeltaPerSecond => _maximumHeadingDeltaPerSecond;
        public float SteeringAcceleration => _steeringAcceleration;
        public float SteeringDeceleration => _steeringDeceleration;
        public float SteeringResponseCurve => _steeringResponseCurve;
        public float BaseRunSpeed => _runSpeed;
        public float SoftMomentumLimit => Mathf.Max(_runSpeed, _softMomentumLimit);
        public float HardVelocitySafetyLimit => Mathf.Max(SoftMomentumLimit, _hardVelocitySafetyLimit);
        public float AirAcceleration => _airAcceleration;
        public float AirWishSpeed => _airWishSpeed;
        public float AirControl => _airControl;
        public float AirTurnStrength => _airTurnStrength;
        public float AirMomentumGain => _airMomentumGain;
        public float MaxAirSpeedContribution => _maxAirSpeedContribution;
        public float JumpHeight => _jumpHeight;
        public float LegacyGravity => _gravity;
        public float TimeToApex => Mathf.Max(0.01f, _timeToApex);
        public float GravityScale => _gravityScale;
        public float FallGravityMultiplier => _fallGravityMultiplier;
        public float BaseTakeoffMomentumRetention => _baseTakeoffMomentumRetention;
        public float HighMomentumTakeoffRetention => _highMomentumTakeoffRetention;
        public float TakeoffMomentumRetentionPower => _takeoffMomentumRetentionPower;
        public float LandingMomentumRetention => _landingMomentumRetention;
        public float Gravity => JumpGravity;
        public float JumpGravity => (2f * _jumpHeight / (TimeToApex * TimeToApex))
            * Mathf.Max(0.01f, _gravityScale);
        public float FallGravity => JumpGravity * Mathf.Max(1f, _fallGravityMultiplier);
        public float MaximumFallSpeed => _maximumFallSpeed;
        public float CoyoteTime => _coyoteTime;
        public float JumpBufferDuration => _jumpBufferDuration;
        public float GroundSnap => _groundSnap;
        public float EdgeTolerance => _edgeTolerance;
        public bool LandingAssistance => _landingAssistance;
        public float MovingPlatformInheritance => _movingPlatformInheritance;
        public float RespawnMovementLockDuration => _respawnMovementLockDuration;
        public float PerfectHopWindow => _perfectHopWindow;
        public float GoodHopWindow => Mathf.Max(_perfectHopWindow, _goodHopWindow);
        public float BufferedHopWindow => Mathf.Max(GoodHopWindow, _bufferedHopWindow);
        public float PerfectHopRetention => _perfectHopRetention;
        public float GoodHopRetention => _goodHopRetention;
        public float BufferedHopRetention => _bufferedHopRetention;
        public float LateHopPenalty => _lateHopPenalty;

        public float JumpVelocity => JumpGravity * TimeToApex;
        public float EstimatedAirtime
        {
            get
            {
                var descent = Mathf.Sqrt(2f * _jumpHeight / Mathf.Max(0.01f, FallGravity));
                return TimeToApex + descent;
            }
        }

        public float NormalTakeoffSpeed => _runSpeed
            * TakeoffMomentumRetentionForSpeed(_runSpeed);

        public float EstimatedMaximumJumpDistance
        {
            get
            {
                return NormalTakeoffSpeed * EstimatedAirtime;
            }
        }

        public float TakeoffMomentumRetentionForSpeed(float horizontalSpeed)
        {
            if (_runSpeed <= 0.001f)
            {
                return Mathf.Clamp01(_baseTakeoffMomentumRetention);
            }

            var momentum01 = Mathf.InverseLerp(
                _runSpeed,
                SoftMomentumLimit,
                Mathf.Max(0f, horizontalSpeed));
            momentum01 = Mathf.Pow(
                Mathf.Clamp01(momentum01),
                Mathf.Max(0.01f, _takeoffMomentumRetentionPower));
            return Mathf.Lerp(
                _baseTakeoffMomentumRetention,
                _highMomentumTakeoffRetention,
                momentum01);
        }

        public static MovementProfile CreateRuntimeDefault()
        {
            var profile = CreateInstance<MovementProfile>();
            profile.name = "Movement_Default_Runtime";
            profile.hideFlags = HideFlags.DontSave;
            return profile;
        }

        public void ConfigureForTests(
            string profileId,
            string displayName,
            float coyoteTime,
            float jumpBufferDuration)
        {
            _profileId = profileId;
            _displayName = displayName;
            _coyoteTime = Mathf.Max(0f, coyoteTime);
            _jumpBufferDuration = Mathf.Max(0f, jumpBufferDuration);
        }
    }

    public sealed class MovementProfileSet
    {
        private readonly MovementProfile[] _profiles;

        public MovementProfileSet(MovementProfile[] profiles)
        {
            if (profiles == null || profiles.Length == 0)
            {
                _profiles = new[] { MovementProfile.CreateRuntimeDefault() };
            }
            else
            {
                _profiles = profiles;
            }
        }

        public int CurrentIndex { get; private set; }
        public MovementProfile Current => _profiles[CurrentIndex];
        public int Count => _profiles.Length;

        public MovementProfile Next()
        {
            CurrentIndex = (CurrentIndex + 1) % _profiles.Length;
            return Current;
        }

        public bool Select(string profileId)
        {
            for (var index = 0; index < _profiles.Length; index++)
            {
                if (string.Equals(
                    _profiles[index].ProfileId,
                    profileId,
                    StringComparison.Ordinal))
                {
                    CurrentIndex = index;
                    return true;
                }
            }

            return false;
        }
    }
}
