using UnityEngine;

namespace Avoidance.Gameplay.Camera
{
    public readonly struct SmartParkourCameraInput
    {
        public SmartParkourCameraInput(
            Vector3 playerPosition,
            float playerBodyYaw,
            float currentCameraYaw,
            float currentPitch,
            Vector3 horizontalVelocity,
            Vector2 moveInput,
            bool grounded,
            bool surfing,
            float verticalSpeed,
            float deltaTime)
        {
            PlayerPosition = playerPosition;
            PlayerBodyYaw = playerBodyYaw;
            CurrentCameraYaw = currentCameraYaw;
            CurrentPitch = currentPitch;
            HorizontalVelocity = Vector3.ProjectOnPlane(horizontalVelocity, Vector3.up);
            MoveInput = moveInput;
            Grounded = grounded;
            Surfing = surfing;
            VerticalSpeed = verticalSpeed;
            DeltaTime = Mathf.Max(0f, deltaTime);
        }

        public Vector3 PlayerPosition { get; }
        public float PlayerBodyYaw { get; }
        public float CurrentCameraYaw { get; }
        public float CurrentPitch { get; }
        public Vector3 HorizontalVelocity { get; }
        public Vector2 MoveInput { get; }
        public bool Grounded { get; }
        public bool Surfing { get; }
        public float VerticalSpeed { get; }
        public float DeltaTime { get; }
    }

    public readonly struct SmartParkourCameraOutput
    {
        public SmartParkourCameraOutput(
            bool active,
            float worldYaw,
            float pitch,
            float coneResponse,
            Vector3 travelDirection,
            Vector3 routeDirection,
            BranchIntentResult branchIntent,
            string matchedBranchId,
            float routeDistance,
            float routeWeight)
        {
            Active = active;
            WorldYaw = worldYaw;
            Pitch = pitch;
            ConeResponse = coneResponse;
            TravelDirection = travelDirection;
            RouteDirection = routeDirection;
            BranchIntent = branchIntent;
            MatchedBranchId = matchedBranchId ?? string.Empty;
            RouteDistance = routeDistance;
            RouteWeight = routeWeight;
        }

        public bool Active { get; }
        public float WorldYaw { get; }
        public float Pitch { get; }
        public float ConeResponse { get; }
        public Vector3 TravelDirection { get; }
        public Vector3 RouteDirection { get; }
        public BranchIntentResult BranchIntent { get; }
        public string MatchedBranchId { get; }
        public float RouteDistance { get; }
        public float RouteWeight { get; }

        public static SmartParkourCameraOutput Inactive(float yaw, float pitch) =>
            new SmartParkourCameraOutput(
                false,
                yaw,
                pitch,
                0f,
                Vector3.forward,
                Vector3.forward,
                new BranchIntentResult(BranchIntentState.Neutral, string.Empty, 0f, 0f, 0f, Vector3.forward),
                string.Empty,
                float.MaxValue,
                0f);
    }

    public static class SmartParkourCameraMath
    {
        public static Vector3 HorizontalDirectionOrFallback(Vector3 value, Vector3 fallback)
        {
            var horizontal = Vector3.ProjectOnPlane(value, Vector3.up);
            if (horizontal.sqrMagnitude > 0.0001f)
            {
                return horizontal.normalized;
            }

            var fallbackHorizontal = Vector3.ProjectOnPlane(fallback, Vector3.up);
            return fallbackHorizontal.sqrMagnitude > 0.0001f
                ? fallbackHorizontal.normalized
                : Vector3.forward;
        }

        public static float YawFromDirection(Vector3 direction)
        {
            var horizontal = HorizontalDirectionOrFallback(direction, Vector3.forward);
            return Mathf.Atan2(horizontal.x, horizontal.z) * Mathf.Rad2Deg;
        }

        public static Vector3 DirectionFromYaw(float yaw) =>
            Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;

        public static float ConeResponse(float currentYaw, float targetYaw, ParkourCameraProfile profile)
        {
            var angle = Mathf.Abs(Mathf.DeltaAngle(currentYaw, targetYaw));
            if (angle <= profile.CameraConeInnerAngle)
            {
                return 0f;
            }

            var t = Mathf.InverseLerp(profile.CameraConeInnerAngle, profile.CameraConeOuterAngle, angle);
            t = Mathf.SmoothStep(0f, 1f, t);
            return Mathf.Pow(t, profile.CameraConeResponseCurve);
        }

        public static Vector3 BlendDirections(Vector3 primary, Vector3 secondary, float secondaryWeight)
        {
            primary = HorizontalDirectionOrFallback(primary, Vector3.forward);
            secondary = HorizontalDirectionOrFallback(secondary, primary);
            var blended = primary * Mathf.Max(0.001f, 1f - secondaryWeight)
                + secondary * Mathf.Clamp01(secondaryWeight);
            return HorizontalDirectionOrFallback(blended, primary);
        }

        public static float Alignment01(Vector3 a, Vector3 b)
        {
            if (a.sqrMagnitude <= 0.0001f || b.sqrMagnitude <= 0.0001f)
            {
                return 0f;
            }

            return Mathf.Clamp01((Vector3.Dot(a.normalized, b.normalized) + 1f) * 0.5f);
        }

        public static float ExpBlend(float speed, float deltaTime) =>
            deltaTime <= 0f ? 1f : 1f - Mathf.Exp(-Mathf.Max(0.001f, speed) * deltaTime);
    }

    public sealed class SmartParkourCameraController
    {
        private readonly RouteCameraBranch[] _candidateBuffer = new RouteCameraBranch[8];
        private readonly BranchIntentResolver _branchIntent = new BranchIntentResolver();
        private ParkourCameraProfile _profile;
        private RouteCameraGraph _routeGraph;
        private Vector3 _smoothedTravelDirection = Vector3.forward;
        private float _routeProgress = -1f;
        private float _smoothedPitch;
        private string _matchedBranchId = string.Empty;

        public SmartParkourCameraController(ParkourCameraProfile profile, RouteCameraGraph routeGraph)
        {
            _profile = profile != null ? profile : ParkourCameraProfile.CreateRuntimeDefault();
            _routeGraph = routeGraph;
            _smoothedPitch = _profile.NeutralPitch;
        }

        public SmartParkourCameraOutput LastOutput { get; private set; }
        public string MatchedBranchId => _matchedBranchId;

        public void Configure(ParkourCameraProfile profile, RouteCameraGraph routeGraph)
        {
            _profile = profile != null ? profile : _profile ?? ParkourCameraProfile.CreateRuntimeDefault();
            _routeGraph = routeGraph;
        }

        public void Reset(float yaw, float pitch)
        {
            _smoothedTravelDirection = SmartParkourCameraMath.DirectionFromYaw(yaw);
            _routeProgress = -1f;
            _smoothedPitch = pitch;
            _matchedBranchId = string.Empty;
            _branchIntent.Reset();
            LastOutput = SmartParkourCameraOutput.Inactive(yaw, pitch);
        }

        public SmartParkourCameraOutput Update(SmartParkourCameraInput input)
        {
            var profile = _profile ?? ParkourCameraProfile.CreateRuntimeDefault();
            var speed = input.HorizontalVelocity.magnitude;
            var bodyDirection = SmartParkourCameraMath.DirectionFromYaw(input.PlayerBodyYaw);
            var steeringDirection = ResolveSteeringDirection(input, bodyDirection);
            var travelDirection = ResolveTravelDirection(input, steeringDirection, speed, profile);
            var routeSample = ResolveRouteSample(input.PlayerPosition, speed, profile);
            var routeDirection = routeSample.IsValid
                ? SmartParkourCameraMath.HorizontalDirectionOrFallback(routeSample.Point - input.PlayerPosition, routeSample.Direction)
                : travelDirection;
            var branchIntent = ResolveBranchIntent(input, travelDirection, steeringDirection, profile);
            var routeWeight = ResolveRouteWeight(travelDirection, routeDirection, routeSample, profile);
            var desiredDirection = SmartParkourCameraMath.BlendDirections(
                travelDirection,
                routeDirection,
                routeWeight);
            if (branchIntent.State != BranchIntentState.Neutral)
            {
                desiredDirection = SmartParkourCameraMath.BlendDirections(
                    desiredDirection,
                    branchIntent.Direction,
                    profile.BranchGuidanceWeight * branchIntent.Confidence);
            }

            var desiredYaw = SmartParkourCameraMath.YawFromDirection(desiredDirection);
            var coneResponse = SmartParkourCameraMath.ConeResponse(input.CurrentCameraYaw, desiredYaw, profile);
            var stationaryTurn = speed < profile.MinVelocityCameraSpeed
                && input.MoveInput.magnitude >= profile.StationarySteeringDeadZone;
            if (stationaryTurn)
            {
                coneResponse = Mathf.Max(coneResponse, profile.StationarySteeringCameraWeight);
            }

            var yawStep = profile.MaxCameraYawSpeed * Mathf.Clamp01(coneResponse) * input.DeltaTime;
            var nextYaw = coneResponse <= 0.001f
                ? input.CurrentCameraYaw
                : Mathf.MoveTowardsAngle(input.CurrentCameraYaw, desiredYaw, yawStep);
            var targetPitch = ResolvePitch(input, routeSample, profile);
            _smoothedPitch = Mathf.Lerp(
                _smoothedPitch,
                targetPitch,
                SmartParkourCameraMath.ExpBlend(profile.PitchFilterSpeed, input.DeltaTime));
            var nextPitch = Mathf.MoveTowardsAngle(
                input.CurrentPitch,
                _smoothedPitch,
                profile.MaxCameraPitchSpeed * input.DeltaTime);

            LastOutput = new SmartParkourCameraOutput(
                true,
                nextYaw,
                nextPitch,
                coneResponse,
                travelDirection,
                routeDirection,
                branchIntent,
                _matchedBranchId,
                routeSample.DistanceToRoute,
                routeWeight);
            return LastOutput;
        }

        private Vector3 ResolveTravelDirection(
            SmartParkourCameraInput input,
            Vector3 steeringDirection,
            float speed,
            ParkourCameraProfile profile)
        {
            var target = speed >= profile.MinVelocityCameraSpeed
                ? SmartParkourCameraMath.HorizontalDirectionOrFallback(input.HorizontalVelocity, _smoothedTravelDirection)
                : input.MoveInput.magnitude >= profile.StationarySteeringDeadZone
                    ? steeringDirection
                    : _smoothedTravelDirection;
            _smoothedTravelDirection = Vector3.Slerp(
                _smoothedTravelDirection,
                target,
                SmartParkourCameraMath.ExpBlend(profile.TravelDirectionFilterSpeed, input.DeltaTime));
            return SmartParkourCameraMath.HorizontalDirectionOrFallback(_smoothedTravelDirection, target);
        }

        private RouteCameraSample ResolveRouteSample(Vector3 playerPosition, float speed, ParkourCameraProfile profile)
        {
            if (_routeGraph == null || !_routeGraph.IsValid)
            {
                _matchedBranchId = string.Empty;
                return RouteCameraSample.Empty;
            }

            var preferred = _branchIntent.State == BranchIntentState.Committed
                ? _branchIntent.CommittedBranchId
                : _matchedBranchId;
            var projected = _routeGraph.ProjectBest(
                playerPosition,
                preferred,
                _routeProgress,
                profile.BranchQueryRadius);
            if (!projected.IsValid)
            {
                _matchedBranchId = string.Empty;
                return RouteCameraSample.Empty;
            }

            _matchedBranchId = projected.Branch.StableId;
            _routeProgress = projected.DistanceAlong;
            var speedT = Mathf.Clamp01(speed / Mathf.Max(0.001f, profile.SpeedForMaxLookAhead));
            var lookAhead = profile.RouteLookAheadDistance
                * (1f + speedT * profile.RouteLookAheadSpeedScale);
            var ahead = projected.Branch.SampleAhead(projected.DistanceAlong, lookAhead);
            return new RouteCameraSample(
                ahead.Branch,
                ahead.Point,
                ahead.Direction,
                ahead.DistanceAlong,
                projected.DistanceToRoute,
                ahead.VerticalFramingHint);
        }

        private BranchIntentResult ResolveBranchIntent(
            SmartParkourCameraInput input,
            Vector3 travelDirection,
            Vector3 steeringDirection,
            ParkourCameraProfile profile)
        {
            if (_routeGraph == null || !_routeGraph.IsValid)
            {
                _branchIntent.Reset();
                return new BranchIntentResult(BranchIntentState.Neutral, string.Empty, 0f, 0f, 0f, travelDirection);
            }

            var count = _routeGraph.CandidateBranches(
                input.PlayerPosition,
                _candidateBuffer,
                profile.BranchQueryRadius);
            return _branchIntent.Update(
                _candidateBuffer,
                count,
                input.PlayerPosition,
                travelDirection,
                steeringDirection,
                !input.Grounded,
                profile,
                input.DeltaTime);
        }

        private static Vector3 ResolveSteeringDirection(SmartParkourCameraInput input, Vector3 bodyDirection)
        {
            var bodyRight = Vector3.Cross(Vector3.up, bodyDirection).normalized;
            var forwardAmount = Mathf.Abs(input.MoveInput.y) > 0.1f
                ? Mathf.Sign(input.MoveInput.y)
                : 1f;
            var direction = bodyDirection * forwardAmount + bodyRight * input.MoveInput.x;
            return SmartParkourCameraMath.HorizontalDirectionOrFallback(direction, bodyDirection);
        }

        private static float ResolveRouteWeight(
            Vector3 travelDirection,
            Vector3 routeDirection,
            RouteCameraSample routeSample,
            ParkourCameraProfile profile)
        {
            if (!routeSample.IsValid || routeSample.DistanceToRoute > profile.BranchQueryRadius * 2f)
            {
                return 0f;
            }

            var agreement = Vector3.Angle(travelDirection, routeDirection);
            var agreementWeight = 1f - Mathf.InverseLerp(
                profile.RouteAgreementAngle,
                180f,
                agreement);
            var proximityWeight = 1f - Mathf.InverseLerp(
                profile.BranchQueryRadius,
                profile.BranchQueryRadius * 2f,
                routeSample.DistanceToRoute);
            return Mathf.Clamp01(profile.RouteGuidanceWeight * agreementWeight * proximityWeight);
        }

        private static float ResolvePitch(
            SmartParkourCameraInput input,
            RouteCameraSample routeSample,
            ParkourCameraProfile profile)
        {
            var pitch = profile.NeutralPitch;
            if (routeSample.IsValid)
            {
                var delta = routeSample.Point - input.PlayerPosition;
                var horizontalDistance = Mathf.Max(1f, Vector3.ProjectOnPlane(delta, Vector3.up).magnitude);
                var routePitch = Mathf.Atan2(delta.y, horizontalDistance) * Mathf.Rad2Deg
                    + routeSample.VerticalFramingHint;
                pitch += Mathf.Clamp(routePitch, -profile.MaxRoutePitchDown, profile.MaxRoutePitchUp);
            }

            if (!input.Grounded && input.VerticalSpeed < -1f)
            {
                pitch += Mathf.Lerp(0f, profile.AirborneDownBias, Mathf.InverseLerp(-1f, -10f, input.VerticalSpeed));
            }

            return Mathf.Clamp(pitch, -profile.MaxRoutePitchDown, profile.MaxRoutePitchUp);
        }
    }
}
