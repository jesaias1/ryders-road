using UnityEngine;

namespace Avoidance.Gameplay.Camera
{
    [CreateAssetMenu(menuName = "RYDERS BLOCK/Parkour Camera Profile", fileName = "ParkourCamera_Default")]
    public sealed class ParkourCameraProfile : ScriptableObject
    {
        [SerializeField] private string _profileId = "smart-camera.default";
        [SerializeField] private string _displayName = "SMART CAMERA DEFAULT";
        [Range(1f, 30f)] [SerializeField] private float _cameraConeInnerAngle = 10f;
        [Range(2f, 45f)] [SerializeField] private float _cameraConeOuterAngle = 24f;
        [Range(0.5f, 4f)] [SerializeField] private float _cameraConeResponseCurve = 1.65f;
        [Range(0.1f, 2f)] [SerializeField] private float _minVelocityLookAheadTime = 0.55f;
        [Range(0.2f, 3f)] [SerializeField] private float _maxVelocityLookAheadTime = 1.25f;
        [Range(0.5f, 8f)] [SerializeField] private float _speedForMaxLookAhead = 10f;
        [Range(0.1f, 5f)] [SerializeField] private float _minVelocityCameraSpeed = 1.1f;
        [Range(1f, 30f)] [SerializeField] private float _travelDirectionFilterSpeed = 12f;
        [Range(0f, 1f)] [SerializeField] private float _routeGuidanceWeight = 0.28f;
        [Range(0f, 1f)] [SerializeField] private float _branchGuidanceWeight = 0.5f;
        [Range(1f, 80f)] [SerializeField] private float _routeAgreementAngle = 55f;
        [Range(2f, 80f)] [SerializeField] private float _routeLookAheadDistance = 18f;
        [Range(0f, 2f)] [SerializeField] private float _routeLookAheadSpeedScale = 0.45f;
        [Range(0f, 1f)] [SerializeField] private float _stationarySteeringCameraWeight = 0.38f;
        [Range(0f, 1f)] [SerializeField] private float _stationarySteeringDeadZone = 0.42f;
        [Range(-15f, 15f)] [SerializeField] private float _neutralPitch = -2f;
        [Range(0f, 18f)] [SerializeField] private float _maxRoutePitchUp = 6f;
        [Range(0f, 24f)] [SerializeField] private float _maxRoutePitchDown = 10f;
        [Range(0f, 14f)] [SerializeField] private float _airborneDownBias = 3f;
        [Range(1f, 30f)] [SerializeField] private float _pitchFilterSpeed = 7f;
        [Range(20f, 360f)] [SerializeField] private float _maxCameraYawSpeed = 130f;
        [Range(10f, 180f)] [SerializeField] private float _maxCameraPitchSpeed = 70f;
        [Range(10f, 140f)] [SerializeField] private float _maxRenderedYawOffset = 86f;
        [Range(0f, 1f)] [SerializeField] private float _velocityBranchWeight = 0.5f;
        [Range(0f, 1f)] [SerializeField] private float _steeringBranchWeight = 0.28f;
        [Range(0f, 1f)] [SerializeField] private float _positionBranchWeight = 0.22f;
        [Range(0.1f, 0.95f)] [SerializeField] private float _branchCommitThreshold = 0.64f;
        [Range(0.1f, 0.95f)] [SerializeField] private float _branchSwitchThreshold = 0.78f;
        [Range(0f, 1f)] [SerializeField] private float _branchScoreHysteresis = 0.16f;
        [Range(0.01f, 1f)] [SerializeField] private float _branchCommitDuration = 0.22f;
        [Range(0.01f, 2f)] [SerializeField] private float _branchReleaseDuration = 0.6f;
        [Range(0.5f, 12f)] [SerializeField] private float _branchQueryRadius = 7f;

        public string ProfileId => _profileId;
        public string DisplayName => _displayName;
        public float CameraConeInnerAngle => Mathf.Min(_cameraConeInnerAngle, _cameraConeOuterAngle);
        public float CameraConeOuterAngle => Mathf.Max(_cameraConeInnerAngle, _cameraConeOuterAngle);
        public float CameraConeResponseCurve => _cameraConeResponseCurve;
        public float MinVelocityLookAheadTime => _minVelocityLookAheadTime;
        public float MaxVelocityLookAheadTime => Mathf.Max(_minVelocityLookAheadTime, _maxVelocityLookAheadTime);
        public float SpeedForMaxLookAhead => _speedForMaxLookAhead;
        public float MinVelocityCameraSpeed => _minVelocityCameraSpeed;
        public float TravelDirectionFilterSpeed => _travelDirectionFilterSpeed;
        public float RouteGuidanceWeight => _routeGuidanceWeight;
        public float BranchGuidanceWeight => _branchGuidanceWeight;
        public float RouteAgreementAngle => _routeAgreementAngle;
        public float RouteLookAheadDistance => _routeLookAheadDistance;
        public float RouteLookAheadSpeedScale => _routeLookAheadSpeedScale;
        public float StationarySteeringCameraWeight => _stationarySteeringCameraWeight;
        public float StationarySteeringDeadZone => _stationarySteeringDeadZone;
        public float NeutralPitch => _neutralPitch;
        public float MaxRoutePitchUp => _maxRoutePitchUp;
        public float MaxRoutePitchDown => _maxRoutePitchDown;
        public float AirborneDownBias => _airborneDownBias;
        public float PitchFilterSpeed => _pitchFilterSpeed;
        public float MaxCameraYawSpeed => _maxCameraYawSpeed;
        public float MaxCameraPitchSpeed => _maxCameraPitchSpeed;
        public float MaxRenderedYawOffset => _maxRenderedYawOffset;
        public float VelocityBranchWeight => _velocityBranchWeight;
        public float SteeringBranchWeight => _steeringBranchWeight;
        public float PositionBranchWeight => _positionBranchWeight;
        public float BranchCommitThreshold => _branchCommitThreshold;
        public float BranchSwitchThreshold => _branchSwitchThreshold;
        public float BranchScoreHysteresis => _branchScoreHysteresis;
        public float BranchCommitDuration => _branchCommitDuration;
        public float BranchReleaseDuration => _branchReleaseDuration;
        public float BranchQueryRadius => _branchQueryRadius;

        public static ParkourCameraProfile CreateRuntimeDefault()
        {
            var profile = CreateInstance<ParkourCameraProfile>();
            profile.name = "ParkourCamera_Default_Runtime";
            profile.hideFlags = HideFlags.DontSave;
            return profile;
        }
    }
}
