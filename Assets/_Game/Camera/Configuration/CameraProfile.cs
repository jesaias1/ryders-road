using UnityEngine;
using UnityEngine.Serialization;

namespace Avoidance.Gameplay.Camera
{
    [CreateAssetMenu(menuName = "RYDERS BLOCK/Camera Profile", fileName = "Camera_Default")]
    public sealed class CameraProfile : ScriptableObject
    {
        [SerializeField] private string _profileId = "camera.default";
        [Min(0.01f)] [SerializeField] private float _sensitivity = 0.12f;
        [Min(0.01f)] [SerializeField] private float _touchSensitivity = 0.22f;
        [Min(0.01f)] [SerializeField] private float _horizontalMultiplier = 1f;
        [Min(0.01f)] [SerializeField] private float _verticalMultiplier = 0.85f;
        [Range(0f, 0.2f)] [SerializeField] private float _microJitterDegrees = 0.024f;
        [Range(0.1f, 1f)] [SerializeField] private float _fineLookScale = 0.82f;
        [Range(0.05f, 3f)] [SerializeField] private float _fineLookThresholdDegrees = 0.35f;
        [Range(1f, 2.5f)] [SerializeField] private float _fastSwipeGain = 1.14f;
        [Range(0.5f, 12f)] [SerializeField] private float _fastSwipeThresholdDegrees = 3.5f;
        [Range(-89f, 0f)] [SerializeField] private float _minimumPitch = -75f;
        [Range(0f, 89f)] [SerializeField] private float _maximumPitch = 75f;
        [FormerlySerializedAs("_fixedGameplayPitch")]
        [Range(-20f, 20f)] [SerializeField] private float _defaultGameplayPitch = 9f;
        [Range(0f, 0.2f)] [SerializeField] private float _smoothingTime = 0.012f;
        [Range(0f, 0.12f)] [SerializeField] private float _bobAmplitude = 0.008f;
        [Range(0f, 20f)] [SerializeField] private float _bobFrequency = 7f;
        [Range(0f, 0.25f)] [SerializeField] private float _landingResponse = 0.025f;
        [Range(0f, 12f)] [SerializeField] private float _speedFovIncrease = 2.4f;
        [Range(0f, 8f)] [SerializeField] private float _airFovIncrease = 0.8f;
        [Range(0f, 10f)] [SerializeField] private float _boostFovIncrease;
        [Range(0f, 8f)] [SerializeField] private float _surfFovIncrease = 1.4f;
        [Range(50f, 100f)] [SerializeField] private float _baseFieldOfView = 94f;
        [Range(60f, 110f)] [SerializeField] private float _maximumFieldOfView = 100f;
        [Range(0.5f, 20f)] [SerializeField] private float _fovBlendInSpeed = 10f;
        [Range(0.5f, 20f)] [SerializeField] private float _fovBlendOutSpeed = 7f;
        [Header("Touch auto camera")]
        [SerializeField] private bool _touchAutoSteerEnabled;
        [Range(0f, 220f)] [SerializeField] private float _touchSteerDegreesPerSecond = 110f;
        [Range(0f, 0.5f)] [SerializeField] private float _touchSteerDeadZone = 0.12f;
        [SerializeField] private bool _touchAutoPitchEnabled;
        [Range(-30f, 30f)] [SerializeField] private float _touchNeutralPitch = -3f;
        [Range(-30f, 30f)] [SerializeField] private float _touchForwardPitch = -6f;
        [Range(0f, 20f)] [SerializeField] private float _touchPitchAssistSpeed = 4f;
        [SerializeField] private bool _smoothingEnabled = true;
        [SerializeField] private bool _bobEnabled = true;
        [SerializeField] private bool _landingResponseEnabled = true;
        [SerializeField] private bool _speedFovEnabled = true;
        [Header("Landing awareness")]
        [SerializeField] private bool _landingAwarenessEnabled = true;
        [Range(0f, 8f)] [SerializeField] private float _landingAwarenessPitch = 3f;
        [Range(0.5f, 10f)] [SerializeField] private float _landingAwarenessRayDistance = 3.2f;
        [Range(-20f, -0.1f)] [SerializeField] private float _landingAwarenessMinVerticalSpeed = -2f;
        [Range(0.5f, 20f)] [SerializeField] private float _landingAwarenessBlendSpeed = 6f;

        public string ProfileId => _profileId;
        public float Sensitivity => _sensitivity;
        public float TouchSensitivity => _touchSensitivity;
        public float HorizontalMultiplier => _horizontalMultiplier;
        public float VerticalMultiplier => _verticalMultiplier;
        public float MicroJitterDegrees => _microJitterDegrees;
        public float FineLookScale => _fineLookScale;
        public float FineLookThresholdDegrees => _fineLookThresholdDegrees;
        public float FastSwipeGain => _fastSwipeGain;
        public float FastSwipeThresholdDegrees => _fastSwipeThresholdDegrees;
        public float MinimumPitch => _minimumPitch;
        public float MaximumPitch => _maximumPitch;
        public float DefaultGameplayPitch => Mathf.Clamp(
            _defaultGameplayPitch,
            _minimumPitch,
            _maximumPitch);
        public float FixedGameplayPitch => DefaultGameplayPitch;
        public float SmoothingTime => _smoothingTime;
        public float BobAmplitude => _bobAmplitude;
        public float BobFrequency => _bobFrequency;
        public float LandingResponse => _landingResponse;
        public float SpeedFovIncrease => _speedFovIncrease;
        public float AirFovIncrease => _airFovIncrease;
        public float BoostFovIncrease => _boostFovIncrease;
        public float SurfFovIncrease => _surfFovIncrease;
        public float BaseFieldOfView => _baseFieldOfView;
        public float MaximumFieldOfView => Mathf.Max(_baseFieldOfView, _maximumFieldOfView);
        public float FovBlendInSpeed => _fovBlendInSpeed;
        public float FovBlendOutSpeed => _fovBlendOutSpeed;
        public bool TouchAutoSteerEnabled => _touchAutoSteerEnabled;
        public float TouchSteerDegreesPerSecond => _touchSteerDegreesPerSecond;
        public float TouchSteerDeadZone => _touchSteerDeadZone;
        public bool TouchAutoPitchEnabled => _touchAutoPitchEnabled;
        public float TouchNeutralPitch => _touchNeutralPitch;
        public float TouchForwardPitch => _touchForwardPitch;
        public float TouchPitchAssistSpeed => _touchPitchAssistSpeed;
        public bool SmoothingEnabled => _smoothingEnabled;
        public bool BobEnabled => _bobEnabled;
        public bool LandingResponseEnabled => _landingResponseEnabled;
        public bool SpeedFovEnabled => _speedFovEnabled;
        public bool LandingAwarenessEnabled => _landingAwarenessEnabled;
        public float LandingAwarenessPitch => _landingAwarenessPitch;
        public float LandingAwarenessRayDistance => _landingAwarenessRayDistance;
        public float LandingAwarenessMinVerticalSpeed => _landingAwarenessMinVerticalSpeed;
        public float LandingAwarenessBlendSpeed => _landingAwarenessBlendSpeed;

        public static CameraProfile CreateRuntimeDefault()
        {
            var profile = CreateInstance<CameraProfile>();
            profile.name = "Camera_Default_Runtime";
            profile.hideFlags = HideFlags.DontSave;
            return profile;
        }
    }
}
