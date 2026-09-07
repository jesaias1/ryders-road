using System.Collections.Generic;
using Avoidance.Gameplay.Player;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Avoidance.Gameplay.Visuals
{
    [DisallowMultipleComponent]
    public sealed class FirstPersonHands : MonoBehaviour
    {
        public const string ArmRenderingLayerName = "FirstPersonArms";
        public const string ArmCameraName = "First Person Arm Camera";

        private UnityEngine.Camera _worldCamera;
        private UnityEngine.Camera _armCamera;
        private Transform _visualRoot;
        private Transform _leftHand;
        private Transform _rightHand;
        private ParkourMotor _motor;
        private ModuleVisualProfile _visuals;
        private FirstPersonArmProfile _armProfile;
        private Vector3 _leftBase;
        private Vector3 _rightBase;
        private Quaternion _leftBaseRotation;
        private Quaternion _rightBaseRotation;
        private Vector3 _leftRenderedPosition;
        private Vector3 _rightRenderedPosition;
        private Quaternion _leftRenderedRotation;
        private Quaternion _rightRenderedRotation;
        private RiggedArmBoneAnimator _leftRig;
        private RiggedArmBoneAnimator _rightRig;
        private float _jumpReaction;
        private float _landingReaction;
        private float _boostReaction;
        private float _waterReaction;
        private float _idleTime;
        private float _runPhase;
        private float _smoothedHorizontalSpeed;
        private float _groundedBlend;
        private float _riseBlend;
        private float _fallBlend;
        private bool _renderStateInitialized;
        private bool _armsVisible = true;

        public UnityEngine.Camera ArmCamera => _armCamera;
        public bool ArmsVisible => _armsVisible;

        public void Initialize(ParkourMotor motor)
        {
            _motor = motor;
            _visuals = Resources.Load<ModuleVisualProfile>(ModuleVisualProfile.ResourceName)
                ?? ModuleVisualProfile.CreateRuntimeDefault();
            _armProfile = Resources.Load<FirstPersonArmProfile>(FirstPersonArmProfile.ResourceName);
            _worldCamera = GetComponent<UnityEngine.Camera>();
            _armsVisible = _armProfile == null || _armProfile.ShowFirstPersonArms;
            if (!_armsVisible)
            {
                DisableArmRendering(_worldCamera);
                return;
            }

            _armCamera = ConfigureArmCamera(_worldCamera, _armProfile);
            _visualRoot = new GameObject("FirstPersonVisualRoot").transform;
            _visualRoot.SetParent(_armCamera != null ? _armCamera.transform : transform, false);
            _visualRoot.gameObject.layer = ResolveArmLayerOrDefault();
            if (_armProfile != null && _armProfile.HasActiveArmPrefabs)
            {
                _leftHand = CreateArmFromProfile(
                    "LeftArmAnchor",
                    _armProfile.LeftArmPrefab,
                    _armProfile.LeftBaseLocalPosition,
                    _armProfile.LeftBaseLocalRotation,
                    _armProfile.LeftPrefabEulerOffset,
                    -1f,
                    out _leftRig);
                _rightHand = CreateArmFromProfile(
                    "RightArmAnchor",
                    _armProfile.RightArmPrefab,
                    _armProfile.RightBaseLocalPosition,
                    _armProfile.RightBaseLocalRotation,
                    _armProfile.RightPrefabEulerOffset,
                    1f,
                    out _rightRig);
            }
            else
            {
                _leftHand = CreateHand(
                    "Left RB Glove",
                    new Vector3(-0.74f, -0.68f, 0.78f),
                    -1f);
                _rightHand = CreateHand(
                    "Right RB Glove",
                    new Vector3(0.74f, -0.68f, 0.78f),
                    1f);
            }
            _leftBase = _leftHand.localPosition;
            _rightBase = _rightHand.localPosition;
            _leftBaseRotation = _leftHand.localRotation;
            _rightBaseRotation = _rightHand.localRotation;
            _leftRenderedPosition = _leftBase;
            _rightRenderedPosition = _rightBase;
            _leftRenderedRotation = _leftBaseRotation;
            _rightRenderedRotation = _rightBaseRotation;
            _renderStateInitialized = true;
            if (_armProfile == null || !_armProfile.NeutralPoseLocked)
            {
                _motor.Jumped += OnJumped;
                _motor.Landed += OnLanded;
            }
        }

        private void LateUpdate()
        {
            if (!_armsVisible || _motor == null)
            {
                return;
            }

            if (_armProfile != null && _armProfile.NeutralPoseLocked)
            {
                return;
            }

            var deltaTime = Mathf.Min(Time.unscaledDeltaTime, 0.033f);
            var speedInfluence = _armProfile == null ? 7.8f : _armProfile.SpeedInfluence;
            var maxAnimationMultiplier = _armProfile == null ? 1.45f : _armProfile.MaximumAnimationMultiplier;
            _smoothedHorizontalSpeed = Mathf.Lerp(
                _smoothedHorizontalSpeed,
                _motor.HorizontalSpeed,
                1f - Mathf.Exp(-10f * deltaTime));
            var speed01 = Mathf.Clamp01(_smoothedHorizontalSpeed / speedInfluence);
            var highSpeed01 = Mathf.Clamp01((_smoothedHorizontalSpeed - speedInfluence) / speedInfluence);
            var groundedTarget = _motor.IsGrounded ? speed01 : 0f;
            _groundedBlend = Mathf.Lerp(_groundedBlend, groundedTarget, 1f - Mathf.Exp(-8f * deltaTime));
            _riseBlend = Mathf.Lerp(
                _riseBlend,
                !_motor.IsGrounded && _motor.VerticalSpeed > 1.5f ? Mathf.Clamp01(_motor.VerticalSpeed / 10f) : 0f,
                1f - Mathf.Exp(-deltaTime / (_armProfile == null ? 0.18f : _armProfile.FallBlendTime)));
            _fallBlend = Mathf.Lerp(
                _fallBlend,
                !_motor.IsGrounded && _motor.VerticalSpeed < -1.5f ? Mathf.Clamp01(-_motor.VerticalSpeed / 18f) : 0f,
                1f - Mathf.Exp(-deltaTime / (_armProfile == null ? 0.18f : _armProfile.FallBlendTime)));

            _jumpReaction = Mathf.MoveTowards(
                _jumpReaction,
                0f,
                deltaTime / (_armProfile == null ? 0.12f : _armProfile.JumpBlendTime));
            _landingReaction = Mathf.MoveTowards(
                _landingReaction,
                0f,
                deltaTime / (_armProfile == null ? 0.24f : _armProfile.LandingRecoveryDuration));
            _boostReaction = Mathf.MoveTowards(_boostReaction, 0f, deltaTime * 5f);
            _waterReaction = Mathf.MoveTowards(_waterReaction, 0f, deltaTime * 4.5f);

            var airborne01 = _motor.IsGrounded ? 0f : 1f;
            var runFrequencyMin = _armProfile == null ? 4.8f : _armProfile.RunFrequencyMin;
            var runFrequencyMax = _armProfile == null ? 8.8f : _armProfile.RunFrequencyMax;
            var cycleSpeed = Mathf.Lerp(0.72f, maxAnimationMultiplier, Mathf.Clamp01(speed01 + highSpeed01 * 0.35f));
            _runPhase += deltaTime * Mathf.Lerp(runFrequencyMin, runFrequencyMax, speed01) * cycleSpeed;
            _idleTime += deltaTime;
            var idleFloatAmount = _armProfile == null ? 0.004f : _armProfile.IdleFloatAmount;
            var idleFrequency = _armProfile == null ? 1.15f : _armProfile.IdleFrequency;
            var leftIdle = new Vector3(
                Mathf.Sin(_idleTime * idleFrequency * 0.73f) * idleFloatAmount * 0.35f,
                Mathf.Sin(_idleTime * idleFrequency) * idleFloatAmount,
                Mathf.Cos(_idleTime * idleFrequency * 0.61f) * idleFloatAmount * 0.42f);
            var rightIdle = new Vector3(
                Mathf.Sin(_idleTime * idleFrequency * 0.67f + 1.7f) * idleFloatAmount * 0.35f,
                Mathf.Sin(_idleTime * idleFrequency + 1.1f) * idleFloatAmount * 0.82f,
                Mathf.Cos(_idleTime * idleFrequency * 0.59f + 0.8f) * idleFloatAmount * 0.42f);
            var runSin = Mathf.Sin(_runPhase);
            var runCos = Mathf.Cos(_runPhase);
            var runLateralAmount = _armProfile == null ? 0.018f : _armProfile.RunLateralAmount;
            var runVerticalAmount = _armProfile == null ? 0.012f : _armProfile.RunVerticalAmount;
            var runForwardAmount = _armProfile == null ? 0.024f : _armProfile.RunForwardAmount;
            var runIntensity = Mathf.Clamp01(_groundedBlend * (1f + highSpeed01 * 0.22f));
            var lateralRun = Mathf.Max(0f, runSin) * runIntensity * runLateralAmount;
            var verticalRun = runSin * runIntensity * runVerticalAmount;
            var forwardRun = runCos * runIntensity * runForwardAmount;
            var waterSway = Mathf.Sin(_idleTime * 12f) * _waterReaction;
            var airborneOffset = _armProfile == null ? Vector3.zero : _armProfile.AirborneOffset * airborne01;
            var fallOffset = _armProfile == null ? Vector3.zero : _armProfile.FallOffset * _fallBlend;
            var maxDisplacement = _armProfile == null ? 0.12f : _armProfile.MaximumPresentationDisplacement;
            var leftTarget = _leftBase
                + leftIdle
                + airborneOffset
                + fallOffset
                + new Vector3(
                    -_boostReaction * 0.4f - lateralRun - _fallBlend * 0.008f,
                    verticalRun + _jumpReaction * 0.35f - _landingReaction + waterSway - _fallBlend * 0.006f + _riseBlend * 0.006f,
                    forwardRun + _jumpReaction - _landingReaction * 0.45f - _fallBlend * 0.012f + _riseBlend * 0.006f);
            var rightTarget = _rightBase
                + rightIdle
                + new Vector3(-airborneOffset.x, airborneOffset.y, airborneOffset.z)
                + new Vector3(-fallOffset.x, fallOffset.y, fallOffset.z)
                + new Vector3(
                    _boostReaction * 0.4f + lateralRun + _fallBlend * 0.008f,
                    -verticalRun * 0.92f + _jumpReaction * 0.3f - _landingReaction - waterSway - _fallBlend * 0.006f + _riseBlend * 0.006f,
                    -forwardRun + _jumpReaction * 0.88f - _landingReaction * 0.45f - _fallBlend * 0.012f + _riseBlend * 0.006f);
            leftTarget = _leftBase + Vector3.ClampMagnitude(leftTarget - _leftBase, maxDisplacement);
            rightTarget = _rightBase + Vector3.ClampMagnitude(rightTarget - _rightBase, maxDisplacement);
            var jump01 = _armProfile == null || _armProfile.JumpOffset <= 0f
                ? 0f
                : Mathf.Clamp01(_jumpReaction / _armProfile.JumpOffset);
            var landing01 = _armProfile == null || _armProfile.LandImpulse <= 0f
                ? 0f
                : Mathf.Clamp01(_landingReaction / _armProfile.LandImpulse);
            var pitchReaction = jump01 * 2.8f - landing01 * 4.5f + _fallBlend * 1.4f + _riseBlend * 1.2f;
            var speedRollDegrees = _armProfile == null ? 3f : _armProfile.RunSpeedRollDegrees;
            var runYawDegrees = _armProfile == null ? 2.4f : _armProfile.RunYawDegrees;
            var runPitchDegrees = _armProfile == null ? 4.2f : _armProfile.RunPitchDegrees;
            var lateralRollDegrees = _armProfile == null ? 2.6f : _armProfile.RunLateralRollDegrees;
            var speedRoll = _groundedBlend * speedRollDegrees;
            var runYaw = runSin * runIntensity * runYawDegrees;
            var runPitch = runCos * runIntensity * runPitchDegrees;
            var lateralRoll = runSin * runIntensity * lateralRollDegrees;
            var leftRotationTarget = _leftBaseRotation
                * Quaternion.Euler(pitchReaction - runPitch, -speedRoll - runYaw, lateralRoll);
            var rightRotationTarget = _rightBaseRotation
                * Quaternion.Euler(pitchReaction + runPitch, speedRoll + runYaw, -lateralRoll);
            if (!_renderStateInitialized)
            {
                _leftRenderedPosition = leftTarget;
                _rightRenderedPosition = rightTarget;
                _leftRenderedRotation = leftRotationTarget;
                _rightRenderedRotation = rightRotationTarget;
                _renderStateInitialized = true;
            }

            var smoothing = _armProfile == null ? 20f : _armProfile.Smoothing;
            var blend = 1f - Mathf.Exp(-smoothing * deltaTime);
            _leftRenderedPosition = Vector3.Lerp(_leftRenderedPosition, leftTarget, blend);
            _rightRenderedPosition = Vector3.Lerp(_rightRenderedPosition, rightTarget, blend);
            _leftRenderedRotation = Quaternion.Slerp(_leftRenderedRotation, leftRotationTarget, blend);
            _rightRenderedRotation = Quaternion.Slerp(_rightRenderedRotation, rightRotationTarget, blend);
            _leftHand.localPosition = _leftRenderedPosition;
            _rightHand.localPosition = _rightRenderedPosition;
            _leftHand.localRotation = _leftRenderedRotation;
            _rightHand.localRotation = _rightRenderedRotation;
            var bonePose = new RiggedArmPose(
                runSin,
                runCos,
                runIntensity,
                _riseBlend,
                _fallBlend,
                jump01,
                landing01);
            _leftRig?.Apply(bonePose, deltaTime);
            _rightRig?.Apply(bonePose, deltaTime);
        }

        private void OnDestroy()
        {
            if (_motor != null)
            {
                _motor.Jumped -= OnJumped;
                _motor.Landed -= OnLanded;
            }
        }

        private void OnJumped() => _jumpReaction = _armProfile == null ? 0.04f : _armProfile.JumpOffset;

        public void NotifyBoost() => _boostReaction = _armProfile == null ? 0.035f : _armProfile.BoostOffset;

        public void NotifyWaterPulse() => _waterReaction = _armProfile == null ? 0.01f : _armProfile.WaterOffset;

        private void OnLanded(float speed)
        {
            var landImpulse = _armProfile == null ? 0.044f : _armProfile.LandImpulse;
            var impactMultiplier = _armProfile == null ? 1.25f : _armProfile.MaxLandingImpactMultiplier;
            _landingReaction = Mathf.Clamp(speed * 0.0024f, 0f, landImpulse * impactMultiplier);
        }

        private static UnityEngine.Camera ConfigureArmCamera(
            UnityEngine.Camera worldCamera,
            FirstPersonArmProfile profile)
        {
            if (worldCamera == null)
            {
                return null;
            }

            var armLayer = ResolveArmLayer();
            if (armLayer < 0)
            {
                Debug.LogError($"Layer '{ArmRenderingLayerName}' is required for first-person arm rendering.");
                return null;
            }

            var armMask = 1 << armLayer;
            worldCamera.cullingMask &= ~armMask;

            var armCameraTransform = worldCamera.transform.Find(ArmCameraName);
            var cameraObject = armCameraTransform == null
                ? new GameObject(ArmCameraName, typeof(UnityEngine.Camera))
                : armCameraTransform.gameObject;
            cameraObject.transform.SetParent(worldCamera.transform, false);
            cameraObject.transform.localPosition = Vector3.zero;
            cameraObject.transform.localRotation = Quaternion.identity;
            cameraObject.transform.localScale = Vector3.one;

            var armCamera = cameraObject.GetComponent<UnityEngine.Camera>()
                ?? cameraObject.AddComponent<UnityEngine.Camera>();
            var audioListener = cameraObject.GetComponent<AudioListener>();
            if (audioListener != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(audioListener);
                }
                else
                {
                    DestroyImmediate(audioListener);
                }
            }

            armCamera.fieldOfView = profile == null ? 70f : profile.ArmFieldOfView;
            armCamera.nearClipPlane = profile == null ? 0.025f : profile.ArmNearClipPlane;
            armCamera.farClipPlane = 4f;
            armCamera.depth = worldCamera.depth + 1f;
            armCamera.cullingMask = armMask;
            armCamera.clearFlags = CameraClearFlags.Depth;
            armCamera.allowHDR = worldCamera.allowHDR;
            armCamera.allowMSAA = worldCamera.allowMSAA;
            armCamera.useOcclusionCulling = false;

            var worldData = worldCamera.GetComponent<UniversalAdditionalCameraData>()
                ?? worldCamera.gameObject.AddComponent<UniversalAdditionalCameraData>();
            var armData = armCamera.GetComponent<UniversalAdditionalCameraData>()
                ?? cameraObject.AddComponent<UniversalAdditionalCameraData>();
            worldData.renderType = CameraRenderType.Base;
            armData.renderType = CameraRenderType.Overlay;
            if (!worldData.cameraStack.Contains(armCamera))
            {
                worldData.cameraStack.Add(armCamera);
            }

            return armCamera;
        }

        private static void DisableArmRendering(UnityEngine.Camera worldCamera)
        {
            if (worldCamera == null)
            {
                return;
            }

            var armLayer = ResolveArmLayer();
            if (armLayer < 0)
            {
                return;
            }

            var armMask = 1 << armLayer;
            worldCamera.cullingMask &= ~armMask;
            var worldData = worldCamera.GetComponent<UniversalAdditionalCameraData>();
            var existing = worldCamera.transform.Find(ArmCameraName);
            if (existing != null
                && existing.TryGetComponent<UnityEngine.Camera>(out var existingCamera))
            {
                worldData?.cameraStack.Remove(existingCamera);
                existing.gameObject.SetActive(false);
            }
        }

        public static int ResolveArmLayer() => LayerMask.NameToLayer(ArmRenderingLayerName);

        private static int ResolveArmLayerOrDefault()
        {
            var layer = ResolveArmLayer();
            return layer < 0 ? 0 : layer;
        }

        public static Vector2 ResolveOpposedRunForwardOffsets(
            float phase,
            float horizontalSpeed,
            bool grounded,
            float speedInfluence,
            float forwardAmount)
        {
            var speed01 = Mathf.Clamp01(horizontalSpeed / Mathf.Max(0.1f, speedInfluence));
            var highSpeed01 = Mathf.Clamp01((horizontalSpeed - speedInfluence) / Mathf.Max(0.1f, speedInfluence));
            var runIntensity = Mathf.Clamp01((grounded ? speed01 : speed01 * 0.28f) * (1f + highSpeed01 * 0.22f));
            var forwardRun = Mathf.Cos(phase) * runIntensity * Mathf.Max(0f, forwardAmount);
            return new Vector2(forwardRun, -forwardRun);
        }

        public static Vector2 ResolveOutwardRunLateralOffsets(
            float phase,
            float horizontalSpeed,
            bool grounded,
            float speedInfluence,
            float lateralAmount)
        {
            var speed01 = Mathf.Clamp01(horizontalSpeed / Mathf.Max(0.1f, speedInfluence));
            var highSpeed01 = Mathf.Clamp01((horizontalSpeed - speedInfluence) / Mathf.Max(0.1f, speedInfluence));
            var runIntensity = Mathf.Clamp01((grounded ? speed01 : speed01 * 0.28f) * (1f + highSpeed01 * 0.22f));
            var outwardRun = Mathf.Max(0f, Mathf.Sin(phase)) * runIntensity * Mathf.Max(0f, lateralAmount);
            return new Vector2(-outwardRun, outwardRun);
        }

        public static float ResolveAnimationCycleMultiplier(
            float horizontalSpeed,
            float normalReferenceSpeed,
            float maximumMultiplier)
        {
            var speed01 = Mathf.Clamp01(horizontalSpeed / Mathf.Max(0.1f, normalReferenceSpeed));
            var highSpeed01 = Mathf.Clamp01((horizontalSpeed - normalReferenceSpeed) / Mathf.Max(0.1f, normalReferenceSpeed));
            return Mathf.Lerp(0.72f, Mathf.Clamp(maximumMultiplier, 1f, 2f), Mathf.Clamp01(speed01 + highSpeed01 * 0.35f));
        }

        private Transform CreateArmFromProfile(
            string armName,
            GameObject prefab,
            Vector3 localPosition,
            Quaternion localRotation,
            Vector3 prefabEulerOffset,
            float side,
            out RiggedArmBoneAnimator rigAnimator)
        {
            var arm = new GameObject(armName).transform;
            arm.SetParent(_visualRoot, false);
            arm.localPosition = localPosition;
            arm.localRotation = localRotation;
            arm.localScale = Vector3.one;
            arm.gameObject.layer = ResolveArmLayerOrDefault();

            var visual = Instantiate(prefab, arm);
            visual.name = armName + " Visual";
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.Euler(prefabEulerOffset);
            visual.transform.localScale = _armProfile.PrefabLocalScale;
            ModuleVisualPrefabLibrary.PrepareVisualInstance(visual);
            SetLayerRecursive(visual.transform, ResolveArmLayerOrDefault());
            foreach (var renderer in visual.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true))
            {
                renderer.updateWhenOffscreen = true;
                renderer.localBounds = new Bounds(
                    new Vector3(0f, 0.85f, 0f),
                    new Vector3(1.2f, 2.0f, 1.2f));
            }

            rigAnimator = _armProfile.UseRiggedArms
                ? new RiggedArmBoneAnimator(visual.transform, _armProfile, side)
                : null;
            return arm;
        }

        private Transform CreateHand(string handName, Vector3 localPosition, float side)
        {
            var hand = new GameObject(handName).transform;
            hand.name = handName;
            hand.SetParent(_visualRoot, false);
            hand.localPosition = localPosition;
            hand.localRotation = Quaternion.Euler(7f, side * -18f, side * 7f);
            hand.localScale = Vector3.one * 0.68f;
            hand.gameObject.layer = ResolveArmLayerOrDefault();

            CreatePiece(
                hand,
                "Palm",
                new Vector3(0f, 0f, 0f),
                new Vector3(0.21f, 0.115f, 0.22f),
                ModuleMaterialRole.GloveDark);
            CreatePiece(
                hand,
                "Wrist Plate",
                new Vector3(0f, -0.035f, -0.16f),
                new Vector3(0.22f, 0.055f, 0.095f),
                ModuleMaterialRole.GloveAccent);
            CreatePiece(
                hand,
                "Forearm Sleeve",
                new Vector3(0f, -0.105f, -0.28f),
                new Vector3(0.2f, 0.08f, 0.15f),
                ModuleMaterialRole.GloveDark,
                new Vector3(-7f, side * 4f, side * 3f));
            CreatePiece(
                hand,
                "Wrist Cyan Rail",
                new Vector3(side * 0.085f, -0.005f, -0.235f),
                new Vector3(0.024f, 0.028f, 0.095f),
                ModuleMaterialRole.CircuitLine);
            CreatePiece(
                hand,
                "Knuckle Plate",
                new Vector3(0f, 0.075f, 0.135f),
                new Vector3(0.19f, 0.04f, 0.085f),
                ModuleMaterialRole.GloveAccent);
            CreatePiece(
                hand,
                "Cyan Tech Pin",
                new Vector3(side * 0.095f, 0.108f, 0.02f),
                new Vector3(0.026f, 0.02f, 0.05f),
                ModuleMaterialRole.MovingAccent);
            CreatePiece(
                hand,
                "Thumb",
                new Vector3(side * 0.14f, -0.018f, 0.12f),
                new Vector3(0.045f, 0.052f, 0.105f),
                ModuleMaterialRole.GloveDark,
                new Vector3(0f, side * 18f, side * -14f));
            for (var index = 0; index < 4; index++)
            {
                var spread = -0.09f + index * 0.06f;
                var curl = Mathf.Lerp(-6f, 8f, index / 3f);
                CreatePiece(
                    hand,
                    "Finger",
                    new Vector3(spread * side, -0.004f, 0.26f),
                    new Vector3(0.032f, 0.044f, 0.09f),
                    ModuleMaterialRole.GloveDark,
                    new Vector3(curl, side * spread * 38f, 0f));
                CreatePiece(
                    hand,
                    "Finger Tip",
                    new Vector3(spread * side, 0.012f, 0.322f),
                    new Vector3(0.028f, 0.024f, 0.032f),
                    index == 1 || index == 2
                        ? ModuleMaterialRole.WarmAccent
                        : ModuleMaterialRole.SurfaceHighlight,
                    new Vector3(curl, side * spread * 38f, 0f));
            }

            return hand;
        }

        private void CreatePiece(
            Transform parent,
            string pieceName,
            Vector3 localPosition,
            Vector3 localScale,
            ModuleMaterialRole role,
            Vector3 localEulerAngles = default)
        {
            var piece = GameObject.CreatePrimitive(PrimitiveType.Cube);
            piece.name = pieceName;
            piece.layer = ResolveArmLayerOrDefault();
            piece.transform.SetParent(parent, false);
            piece.transform.localPosition = localPosition;
            piece.transform.localRotation = Quaternion.Euler(localEulerAngles);
            piece.transform.localScale = localScale;
            var renderer = piece.GetComponent<Renderer>();
            renderer.sharedMaterial = _visuals.MaterialFor(role);
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
            renderer.allowOcclusionWhenDynamic = false;
            var collider = piece.GetComponent<Collider>();
            if (collider != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(collider);
                }
                else
                {
                    DestroyImmediate(collider);
                }
            }
        }

        private static void SetLayerRecursive(Transform root, int layer)
        {
            if (root == null || layer < 0)
            {
                return;
            }

            root.gameObject.layer = layer;
            for (var index = 0; index < root.childCount; index++)
            {
                SetLayerRecursive(root.GetChild(index), layer);
            }
        }

        private readonly struct RiggedArmPose
        {
            public RiggedArmPose(
                float runSin,
                float runCos,
                float runIntensity,
                float rise,
                float fall,
                float jump,
                float landing)
            {
                RunSin = runSin;
                RunCos = runCos;
                RunIntensity = runIntensity;
                Rise = rise;
                Fall = fall;
                Jump = jump;
                Landing = landing;
            }

            public float RunSin { get; }
            public float RunCos { get; }
            public float RunIntensity { get; }
            public float Rise { get; }
            public float Fall { get; }
            public float Jump { get; }
            public float Landing { get; }
        }

        private sealed class RiggedArmBoneAnimator
        {
            private static readonly string[][] FingerChains =
            {
                new[] { "Bone_007", "Bone_006", "Bone_005", "Bone_004" },
                new[] { "Bone_011", "Bone_010", "Bone_009", "Bone_008" },
                new[] { "Bone_015", "Bone_014", "Bone_013", "Bone_012" },
                new[] { "Bone_019", "Bone_018", "Bone_017", "Bone_016" },
                new[] { "Bone_023", "Bone_022", "Bone_021", "Bone_020" }
            };

            private readonly FirstPersonArmProfile _profile;
            private readonly float _side;
            private readonly BonePose[] _wristBones;
            private readonly BonePose[][] _fingerChains;

            public RiggedArmBoneAnimator(
                Transform root,
                FirstPersonArmProfile profile,
                float side)
            {
                _profile = profile;
                _side = side < 0f ? -1f : 1f;
                _wristBones = new[]
                {
                    TryCreateBonePose(root, "Bone_002"),
                    TryCreateBonePose(root, "Bone_001")
                };
                _fingerChains = BuildFingerChains(root);
                ApplyRestPose();
            }

            public void Apply(RiggedArmPose pose, float deltaTime)
            {
                var blend = 1f - Mathf.Exp(-_profile.BoneSmoothing * Mathf.Max(0f, deltaTime));
                var wristPitch = pose.RunCos * pose.RunIntensity * _profile.WristRunPitchDegrees
                    + pose.Rise * _profile.WristAirPitchDegrees
                    - pose.Fall * _profile.WristAirPitchDegrees * 0.55f
                    - pose.Landing * _profile.WristLandingPitchDegrees;
                var wristYaw = pose.RunSin * pose.RunIntensity * _profile.WristRunPitchDegrees * 0.35f * _side;
                foreach (var wrist in _wristBones)
                {
                    wrist.Apply(
                        Quaternion.Euler(wristPitch, wristYaw, -wristYaw * 0.45f),
                        blend);
                }

                var runCurl = Mathf.Abs(pose.RunSin) * pose.RunIntensity * _profile.FingerRunCurlDegrees;
                var landingCurl = pose.Landing * _profile.FingerLandingCurlDegrees;
                var airRelax = pose.Fall * _profile.FingerRestCurlDegrees * 0.25f;
                for (var chainIndex = 0; chainIndex < _fingerChains.Length; chainIndex++)
                {
                    var chain = _fingerChains[chainIndex];
                    var chainBias = 1f + chainIndex * 0.08f;
                    var restCurl = chainIndex == 0
                        ? _profile.ThumbRestCurlDegrees
                        : _profile.FingerRestCurlDegrees;
                    var curl = Mathf.Max(
                        0f,
                        restCurl * chainBias
                        + runCurl
                        + landingCurl
                        - airRelax);
                    var spread = (chainIndex - 2f) * 0.7f * pose.Fall * _side;
                    for (var jointIndex = 0; jointIndex < chain.Length; jointIndex++)
                    {
                        var jointWeight = jointIndex == 0 ? 0.32f : jointIndex == 1 ? 0.24f : jointIndex == 2 ? 0.16f : 0.08f;
                        chain[jointIndex].Apply(
                            Quaternion.Euler(curl * jointWeight, spread, 0f),
                            blend);
                    }
                }
            }

            private void ApplyRestPose()
            {
                Apply(new RiggedArmPose(0f, 0f, 0f, 0f, 0f, 0f, 0f), 1f);
            }

            private static BonePose[][] BuildFingerChains(Transform root)
            {
                var chains = new List<BonePose[]>();
                foreach (var names in FingerChains)
                {
                    var bones = new List<BonePose>();
                    foreach (var name in names)
                    {
                        var pose = TryCreateBonePose(root, name);
                        if (pose.IsValid)
                        {
                            bones.Add(pose);
                        }
                    }

                    if (bones.Count > 0)
                    {
                        chains.Add(bones.ToArray());
                    }
                }

                return chains.ToArray();
            }

            private static BonePose TryCreateBonePose(Transform root, string name)
            {
                var transform = FindDeepChild(root, name);
                return transform == null ? default : new BonePose(transform);
            }

            private static Transform FindDeepChild(Transform parent, string name)
            {
                if (parent.name == name)
                {
                    return parent;
                }

                for (var index = 0; index < parent.childCount; index++)
                {
                    var match = FindDeepChild(parent.GetChild(index), name);
                    if (match != null)
                    {
                        return match;
                    }
                }

                return null;
            }
        }

        private readonly struct BonePose
        {
            private readonly Transform _transform;
            private readonly Quaternion _restRotation;

            public BonePose(Transform transform)
            {
                _transform = transform;
                _restRotation = transform.localRotation;
            }

            public bool IsValid => _transform != null;

            public void Apply(Quaternion additiveRotation, float blend)
            {
                if (_transform == null)
                {
                    return;
                }

                var target = _restRotation * additiveRotation;
                _transform.localRotation = Quaternion.Slerp(
                    _transform.localRotation,
                    target,
                    Mathf.Clamp01(blend));
            }
        }
    }
}
