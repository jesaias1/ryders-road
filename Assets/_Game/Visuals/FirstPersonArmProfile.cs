using UnityEngine;

namespace Avoidance.Gameplay.Visuals
{
    public enum FirstPersonArmPresentationMode
    {
        LegacyApproved = 0,
        RiggedExperimental = 1,
        RyderArmV2 = 2
    }

    [CreateAssetMenu(
        menuName = "RYDERS BLOCK/First Person Arm Profile",
        fileName = ResourceName)]
    public sealed class FirstPersonArmProfile : ScriptableObject
    {
        public const string ResourceName = "FirstPersonArmProfile";

        [SerializeField] private FirstPersonArmPresentationMode _presentationMode =
            FirstPersonArmPresentationMode.LegacyApproved;
        [SerializeField] private bool _showFirstPersonArms = true;
        [SerializeField] private GameObject _leftArmPrefab;
        [SerializeField] private GameObject _rightArmPrefab;
        [SerializeField] private GameObject _riggedLeftArmPrefab;
        [SerializeField] private GameObject _riggedRightArmPrefab;
        [SerializeField] private GameObject _ryderArmV2LeftArmPrefab;
        [SerializeField] private GameObject _ryderArmV2RightArmPrefab;
        [SerializeField] private Vector3 _leftBaseLocalPosition = new Vector3(-0.52f, -0.025f, 0.41f);
        [SerializeField] private Vector3 _rightBaseLocalPosition = new Vector3(0.52f, -0.02f, 0.41f);
        [SerializeField] private Vector3 _leftBaseLocalEulerAngles = new Vector3(0f, -52f, 202f);
        [SerializeField] private Vector3 _rightBaseLocalEulerAngles = new Vector3(1f, 52f, 158f);
        [SerializeField] private Vector3 _leftPrefabEulerOffset = Vector3.zero;
        [SerializeField] private Vector3 _rightPrefabEulerOffset = Vector3.zero;
        [SerializeField] private Vector3 _prefabLocalScale = Vector3.one * 0.36f;
        [SerializeField] private bool _neutralPoseLocked;
        [SerializeField] private float _armFieldOfView = 72f;
        [SerializeField] private float _armNearClipPlane = 0.025f;
        [SerializeField] private float _idleFloatAmount = 0.0028f;
        [SerializeField] private float _idleFrequency = 1.05f;
        [SerializeField] private float _runLateralAmount = 0.002f;
        [SerializeField] private float _runVerticalAmount = 0.0075f;
        [SerializeField] private float _runForwardAmount = 0.028f;
        [SerializeField] private float _runFrequencyMin = 4.8f;
        [SerializeField] private float _runFrequencyMax = 8f;
        [SerializeField] private float _runSpeedRollDegrees = 0.9f;
        [SerializeField] private float _runYawDegrees = 0.9f;
        [SerializeField] private float _runPitchDegrees = 2f;
        [SerializeField] private float _runLateralRollDegrees = 0.8f;
        [SerializeField] private float _jumpOffset = 0.015f;
        [SerializeField] private Vector3 _airborneOffset = new Vector3(0f, 0.002f, 0.006f);
        [SerializeField] private Vector3 _fallOffset = new Vector3(0.006f, -0.004f, -0.003f);
        [SerializeField] private float _landImpulse = 0.02f;
        [SerializeField] private float _boostOffset;
        [SerializeField] private float _waterOffset;
        [SerializeField] private float _speedInfluence = 7.8f;
        [SerializeField] private float _maximumAnimationMultiplier = 1.22f;
        [SerializeField] private float _maximumPresentationDisplacement = 0.052f;
        [SerializeField] private float _smoothing = 18f;
        [SerializeField] private float _fingerRestCurlDegrees;
        [SerializeField] private float _fingerRunCurlDegrees = 0.8f;
        [SerializeField] private float _fingerLandingCurlDegrees = 1.1f;
        [SerializeField] private float _thumbRestCurlDegrees;
        [SerializeField] private float _wristRunPitchDegrees = 1.1f;
        [SerializeField] private float _wristAirPitchDegrees = 0.9f;
        [SerializeField] private float _wristLandingPitchDegrees = 1.4f;
        [SerializeField] private float _boneSmoothing = 20f;
        [SerializeField] private float _jumpBlendTime = 0.12f;
        [SerializeField] private float _fallBlendTime = 0.18f;
        [SerializeField] private float _landingRecoveryDuration = 0.24f;
        [SerializeField] private float _maxLandingImpactMultiplier = 1.25f;

        public FirstPersonArmPresentationMode PresentationMode => _presentationMode;
        public bool ShowFirstPersonArms => _showFirstPersonArms;
        public GameObject LegacyLeftArmPrefab => _leftArmPrefab;
        public GameObject LegacyRightArmPrefab => _rightArmPrefab;
        public GameObject RiggedLeftArmPrefab => _riggedLeftArmPrefab;
        public GameObject RiggedRightArmPrefab => _riggedRightArmPrefab;
        public GameObject RyderArmV2LeftArmPrefab => _ryderArmV2LeftArmPrefab;
        public GameObject RyderArmV2RightArmPrefab => _ryderArmV2RightArmPrefab;
        public GameObject LeftArmPrefab => _presentationMode switch
        {
            FirstPersonArmPresentationMode.RyderArmV2 when HasRyderArmV2Prefabs => _ryderArmV2LeftArmPrefab,
            FirstPersonArmPresentationMode.RiggedExperimental when HasPreviousRiggedFallback => _riggedLeftArmPrefab,
            _ => _leftArmPrefab
        };
        public GameObject RightArmPrefab => _presentationMode switch
        {
            FirstPersonArmPresentationMode.RyderArmV2 when HasRyderArmV2Prefabs => _ryderArmV2RightArmPrefab,
            FirstPersonArmPresentationMode.RiggedExperimental when HasPreviousRiggedFallback => _riggedRightArmPrefab,
            _ => _rightArmPrefab
        };
        public Vector3 LeftBaseLocalPosition => _leftBaseLocalPosition;
        public Vector3 RightBaseLocalPosition => _rightBaseLocalPosition;
        public Quaternion LeftBaseLocalRotation => Quaternion.Euler(_leftBaseLocalEulerAngles);
        public Quaternion RightBaseLocalRotation => Quaternion.Euler(_rightBaseLocalEulerAngles);
        public Vector3 LeftBaseLocalEulerAngles => _leftBaseLocalEulerAngles;
        public Vector3 RightBaseLocalEulerAngles => _rightBaseLocalEulerAngles;
        public Vector3 LeftPrefabEulerOffset => _leftPrefabEulerOffset;
        public Vector3 RightPrefabEulerOffset => _rightPrefabEulerOffset;
        public Vector3 PrefabLocalScale => _prefabLocalScale == Vector3.zero ? Vector3.one : _prefabLocalScale;
        public bool NeutralPoseLocked => _neutralPoseLocked;
        public float ArmFieldOfView => Mathf.Clamp(_armFieldOfView, 60f, 72f);
        public float ArmNearClipPlane => Mathf.Clamp(_armNearClipPlane, 0.01f, 0.08f);
        public float IdleFloatAmount => Mathf.Max(0f, _idleFloatAmount);
        public float IdleFrequency => Mathf.Max(0.1f, _idleFrequency);
        public float RunLateralAmount => Mathf.Max(0f, _runLateralAmount);
        public float RunVerticalAmount => Mathf.Max(0f, _runVerticalAmount);
        public float RunForwardAmount => Mathf.Max(0f, _runForwardAmount);
        public float RunFrequencyMin => Mathf.Max(0.1f, _runFrequencyMin);
        public float RunFrequencyMax => Mathf.Max(RunFrequencyMin, _runFrequencyMax);
        public float RunSpeedRollDegrees => Mathf.Max(0f, _runSpeedRollDegrees);
        public float RunYawDegrees => Mathf.Max(0f, _runYawDegrees);
        public float RunPitchDegrees => Mathf.Max(0f, _runPitchDegrees);
        public float RunLateralRollDegrees => Mathf.Max(0f, _runLateralRollDegrees);
        public float JumpOffset => Mathf.Max(0f, _jumpOffset);
        public Vector3 AirborneOffset => _airborneOffset;
        public Vector3 FallOffset => _fallOffset;
        public float LandImpulse => Mathf.Max(0f, _landImpulse);
        public float BoostOffset => Mathf.Max(0f, _boostOffset);
        public float WaterOffset => Mathf.Max(0f, _waterOffset);
        public float SpeedInfluence => Mathf.Max(0.1f, _speedInfluence);
        public float MaximumAnimationMultiplier => Mathf.Clamp(_maximumAnimationMultiplier, 1f, 2f);
        public float MaximumPresentationDisplacement => Mathf.Max(0.01f, _maximumPresentationDisplacement);
        public float Smoothing => Mathf.Max(0.1f, _smoothing);
        public float FingerRestCurlDegrees => Mathf.Max(0f, _fingerRestCurlDegrees);
        public float FingerRunCurlDegrees => Mathf.Max(0f, _fingerRunCurlDegrees);
        public float FingerLandingCurlDegrees => Mathf.Max(0f, _fingerLandingCurlDegrees);
        public float ThumbRestCurlDegrees => Mathf.Max(0f, _thumbRestCurlDegrees);
        public float WristRunPitchDegrees => Mathf.Max(0f, _wristRunPitchDegrees);
        public float WristAirPitchDegrees => Mathf.Max(0f, _wristAirPitchDegrees);
        public float WristLandingPitchDegrees => Mathf.Max(0f, _wristLandingPitchDegrees);
        public float BoneSmoothing => Mathf.Max(0.1f, _boneSmoothing);
        public float JumpBlendTime => Mathf.Max(0.04f, _jumpBlendTime);
        public float FallBlendTime => Mathf.Max(0.04f, _fallBlendTime);
        public float LandingRecoveryDuration => Mathf.Max(0.05f, _landingRecoveryDuration);
        public float MaxLandingImpactMultiplier => Mathf.Clamp(_maxLandingImpactMultiplier, 0.1f, 2f);
        public bool UseRiggedArms => (_presentationMode == FirstPersonArmPresentationMode.RiggedExperimental
                && HasPreviousRiggedFallback)
            || (_presentationMode == FirstPersonArmPresentationMode.RyderArmV2 && HasRyderArmV2Prefabs);
        public bool HasLegacyApprovedFallback => _leftArmPrefab != null && _rightArmPrefab != null;
        public bool HasPreviousRiggedFallback => _riggedLeftArmPrefab != null && _riggedRightArmPrefab != null;
        public bool HasRyderArmV2Prefabs => _ryderArmV2LeftArmPrefab != null && _ryderArmV2RightArmPrefab != null;
        public bool HasArmPrefabs => _leftArmPrefab != null && _rightArmPrefab != null;
        public bool HasActiveArmPrefabs => LeftArmPrefab != null && RightArmPrefab != null;
    }
}
