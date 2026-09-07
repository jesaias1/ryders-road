using UnityEngine;

namespace Avoidance.Input
{
    [CreateAssetMenu(menuName = "RYDERS BLOCK/Touch Control Layout", fileName = "Touch_Default")]
    public sealed class TouchControlLayout : ScriptableObject
    {
        [SerializeField] private string _layoutId = "touch.default";
        [SerializeField] private TouchControlProfileKind _defaultControlProfile =
            TouchControlProfileKind.LeftMoveRightLookTapJump;
        [SerializeField] private TouchJumpMode _defaultJumpMode = TouchJumpMode.RightTap;
        [SerializeField] private TouchSensitivityPreset _defaultCameraSensitivity =
            TouchSensitivityPreset.Fast;
        [SerializeField] private TouchMovementSensitivityPreset _defaultMovementSensitivity =
            TouchMovementSensitivityPreset.Fast;
        [Header("Ergonomic anchors")]
        [SerializeField] private Vector2 _leftRestCenter = new Vector2(0.35f, 0.38f);
        [SerializeField] private Vector2 _rightRestCenter = new Vector2(0.65f, 0.38f);
        [Range(0f, 1f)] [SerializeField] private float _movementOriginMinX = 0.30f;
        [Range(0f, 1f)] [SerializeField] private float _movementOriginMaxX = 0.38f;
        [Range(0f, 1f)] [SerializeField] private float _movementOriginMinY = 0.24f;
        [Range(0f, 1f)] [SerializeField] private float _movementOriginMaxY = 0.52f;
        [Range(180f, 460f)] [SerializeField] private float _joystickSize = 300f;
        [Range(0f, 0.6f)] [SerializeField] private float _joystickDeadZone = 0.08f;
        [SerializeField] private bool _floatingJoystick = true;
        [Range(140f, 340f)] [SerializeField] private float _jumpButtonSize = 220f;
        [Range(12f, 120f)] [SerializeField] private float _edgePadding = 42f;
        [Range(0f, 220f)] [SerializeField] private float _horizontalEdgeComfortMargin = 96f;
        [Range(0f, 180f)] [SerializeField] private float _verticalEdgeComfortMargin = 82f;
        [Range(0.05f, 0.65f)] [SerializeField] private float _joystickRingOpacity = 0.28f;
        [Range(0.05f, 0.75f)] [SerializeField] private float _joystickKnobOpacity = 0.38f;
        [Range(0.05f, 0.7f)] [SerializeField] private float _jumpButtonOpacity = 0.32f;
        [Range(0f, 0.16f)] [SerializeField] private float _lookZoneOpacity = 0.001f;
        [Header("Tap jump")]
        [Range(0.05f, 0.4f)] [SerializeField] private float _tapMaxDuration = 0.18f;
        [Range(0.005f, 0.1f)] [SerializeField] private float _tapMovementTolerance = 0.035f;
        [Range(0.005f, 0.08f)] [SerializeField] private float _dragActivationDistance = 0.014f;
        [Header("Right flick jump")]
        [Range(0.1f, 5f)] [SerializeField] private float _flickMinVelocity = 1.35f;
        [Range(0.01f, 0.16f)] [SerializeField] private float _flickMinDistance = 0.055f;
        [Range(0.05f, 0.45f)] [SerializeField] private float _flickMaxDuration = 0.22f;
        [Range(0.01f, 0.16f)] [SerializeField] private float _flickMaxHorizontalDeviation = 0.075f;
        [Range(5f, 65f)] [SerializeField] private float _flickDirectionToleranceDegrees = 34f;
        [Range(0f, 0.35f)] [SerializeField] private float _flickCooldown = 0.08f;
        [Range(0.01f, 0.14f)] [SerializeField] private float _flickRearmDistance = 0.045f;
        [SerializeField] private bool _flickRequireMovementTouch = true;
        [Range(0.02f, 0.18f)] [SerializeField] private float _flickVelocityWindow = 0.08f;
        [SerializeField] private bool _flickHapticsEnabled;
        [SerializeField] private bool _gestureDebuggingEnabled;

        public string LayoutId => _layoutId;
        public TouchControlProfileKind DefaultControlProfile => _defaultControlProfile;
        public TouchJumpMode DefaultJumpMode => _defaultJumpMode;
        public TouchSensitivityPreset DefaultCameraSensitivity => _defaultCameraSensitivity;
        public TouchMovementSensitivityPreset DefaultMovementSensitivity => _defaultMovementSensitivity;
        public Vector2 LeftRestCenter => ClampMovementAnchor(_leftRestCenter);
        public Vector2 RightRestCenter => ClampAnchor(_rightRestCenter);
        public float MovementOriginMinX => Mathf.Clamp01(Mathf.Min(_movementOriginMinX, _movementOriginMaxX));
        public float MovementOriginMaxX => Mathf.Clamp01(Mathf.Max(_movementOriginMinX, _movementOriginMaxX));
        public float MovementOriginMinY => Mathf.Clamp01(Mathf.Min(_movementOriginMinY, _movementOriginMaxY));
        public float MovementOriginMaxY => Mathf.Clamp01(Mathf.Max(_movementOriginMinY, _movementOriginMaxY));
        public float JoystickSize => _joystickSize;
        public float ControlRadius => _joystickSize * 0.5f;
        public float MaximumDisplacement => _joystickSize * 0.48f;
        public float JoystickDeadZone => _joystickDeadZone;
        public bool FloatingJoystick => _floatingJoystick;
        public float JumpButtonSize => _jumpButtonSize;
        public float EdgePadding => _edgePadding;
        public float HorizontalEdgeComfortMargin => _horizontalEdgeComfortMargin;
        public float VerticalEdgeComfortMargin => _verticalEdgeComfortMargin;
        public float JoystickRingOpacity => _joystickRingOpacity;
        public float JoystickKnobOpacity => _joystickKnobOpacity;
        public float JumpButtonOpacity => _jumpButtonOpacity;
        public float LookZoneOpacity => _lookZoneOpacity;
        public float TapMaxDuration => _tapMaxDuration;
        public float TapMovementTolerance => _tapMovementTolerance;
        public float TapMaxDistance => _tapMovementTolerance;
        public float DragActivationDistance => _dragActivationDistance;
        public bool FlickHapticsEnabled => _flickHapticsEnabled;
        public bool GestureDebuggingEnabled => _gestureDebuggingEnabled;
        public FlickGestureSettings FlickSettings => new FlickGestureSettings(
            _flickMinVelocity,
            _flickMinDistance,
            _flickMaxDuration,
            _flickMaxHorizontalDeviation,
            _flickDirectionToleranceDegrees,
            _flickCooldown,
            _flickRearmDistance,
            _flickRequireMovementTouch,
            _flickVelocityWindow);

        public TouchControlRuntimeProfile CreateRuntimeProfile(
            TouchControlProfileKind controlProfile,
            TouchJumpMode jumpMode,
            TouchSensitivityPreset cameraSensitivity,
            TouchMovementSensitivityPreset movementSensitivity)
        {
            var movementOnRight = controlProfile == TouchControlProfileKind.LegacyCenteredFlick
                || controlProfile == TouchControlProfileKind.LegacyCenteredFlickAndTap;
            var profileJumpMode = ResolveJumpMode(controlProfile, jumpMode);
            var flowSteering = controlProfile == TouchControlProfileKind.FlowSteerAutoBalanced
                || controlProfile == TouchControlProfileKind.FlowSteerAutoDirect
                || controlProfile == TouchControlProfileKind.FlowSteerAutoFlow;
            var manualLook = controlProfile == TouchControlProfileKind.LegacyManual
                || controlProfile == TouchControlProfileKind.LeftMoveRightLookTapJump
                || controlProfile == TouchControlProfileKind.LeftMoveRightLookFixedJump
                || controlProfile == TouchControlProfileKind.LeftMoveRightLookTapAndButton
                || flowSteering
                || movementOnRight;

            var leftZoneMin = new Vector2(0f, 0.08f);
            var leftZoneMax = new Vector2(0.52f, 0.92f);
            var rightZoneMin = new Vector2(0.48f, 0.08f);
            var rightZoneMax = new Vector2(1f, 0.92f);
            var movementCenter = movementOnRight ? RightRestCenter : LeftRestCenter;
            var lookCenter = movementOnRight ? LeftRestCenter : RightRestCenter;
            return new TouchControlRuntimeProfile(
                controlProfile,
                profileJumpMode,
                cameraSensitivity,
                movementSensitivity,
                movementOnRight,
                movementOnRight ? rightZoneMin : leftZoneMin,
                movementOnRight ? rightZoneMax : leftZoneMax,
                movementCenter,
                movementOnRight ? leftZoneMin : rightZoneMin,
                movementOnRight ? leftZoneMax : rightZoneMax,
                lookCenter,
                CameraSensitivityMultiplier(cameraSensitivity),
                MovementSensitivityMultiplier(movementSensitivity),
                manualLook,
                flowSteering,
                ResolveAutoCameraProfile(controlProfile));
        }

        public static TouchControlLayout CreateRuntimeDefault()
        {
            var layout = CreateInstance<TouchControlLayout>();
            layout.name = "Touch_Default_Runtime";
            layout.hideFlags = HideFlags.DontSave;
            return layout;
        }

        private static Vector2 ClampAnchor(Vector2 value)
        {
            return new Vector2(Mathf.Clamp01(value.x), Mathf.Clamp01(value.y));
        }

        private Vector2 ClampMovementAnchor(Vector2 value)
        {
            return new Vector2(
                Mathf.Clamp(value.x, MovementOriginMinX, MovementOriginMaxX),
                Mathf.Clamp(value.y, MovementOriginMinY, MovementOriginMaxY));
        }

        private static TouchJumpMode ResolveJumpMode(
            TouchControlProfileKind controlProfile,
            TouchJumpMode requested)
        {
            switch (controlProfile)
            {
                case TouchControlProfileKind.FlowSteerAutoBalanced:
                case TouchControlProfileKind.FlowSteerAutoDirect:
                case TouchControlProfileKind.FlowSteerAutoFlow:
                    return TouchJumpMode.RightTap;
                case TouchControlProfileKind.LegacyManual:
                    return TouchJumpMode.RightTap;
                case TouchControlProfileKind.LeftMoveRightLookTapJump:
                    return TouchJumpMode.RightTap;
                case TouchControlProfileKind.LeftMoveRightLookFixedJump:
                    return TouchJumpMode.FixedButton;
                case TouchControlProfileKind.LeftMoveRightLookTapAndButton:
                    return TouchJumpMode.RightTapAndFixedButton;
                case TouchControlProfileKind.LegacyCenteredFlick:
                    return TouchJumpMode.LegacyRightFlick;
                case TouchControlProfileKind.LegacyCenteredFlickAndTap:
                    return TouchJumpMode.LegacyRightFlickAndTap;
                case TouchControlProfileKind.LegacyAutoSteer:
                case TouchControlProfileKind.LegacyStandardReversed:
                    return TouchJumpMode.FixedButton;
                default:
                    return requested;
            }
        }

        private static AutoCameraProfileKind ResolveAutoCameraProfile(
            TouchControlProfileKind controlProfile)
        {
            switch (controlProfile)
            {
                case TouchControlProfileKind.FlowSteerAutoBalanced:
                    return AutoCameraProfileKind.SmartParkour;
                case TouchControlProfileKind.FlowSteerAutoDirect:
                    return AutoCameraProfileKind.Direct;
                case TouchControlProfileKind.FlowSteerAutoFlow:
                    return AutoCameraProfileKind.Flow;
                default:
                    return AutoCameraProfileKind.Balanced;
            }
        }

        private static float CameraSensitivityMultiplier(TouchSensitivityPreset preset)
        {
            switch (preset)
            {
                case TouchSensitivityPreset.Low:
                    return 0.72f;
                case TouchSensitivityPreset.Medium:
                    return 1f;
                case TouchSensitivityPreset.Fast:
                    return 1.32f;
                case TouchSensitivityPreset.VeryFast:
                    return 1.68f;
                default:
                    return 1f;
            }
        }

        private static float MovementSensitivityMultiplier(TouchMovementSensitivityPreset preset)
        {
            switch (preset)
            {
                case TouchMovementSensitivityPreset.Low:
                    return 0.82f;
                case TouchMovementSensitivityPreset.Fast:
                    return 1.18f;
                case TouchMovementSensitivityPreset.Medium:
                default:
                    return 1f;
            }
        }
    }
}
