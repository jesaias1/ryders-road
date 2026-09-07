using UnityEngine;

namespace Avoidance.Input
{
    public enum TouchControlProfileKind
    {
        FlowSteerAutoBalanced,
        FlowSteerAutoDirect,
        FlowSteerAutoFlow,
        LegacyManual,
        LeftMoveRightLookTapJump,
        LeftMoveRightLookFixedJump,
        LeftMoveRightLookTapAndButton,
        LegacyCenteredFlick,
        LegacyCenteredFlickAndTap,
        LegacyAutoSteer,
        LegacyStandardReversed
    }

    public enum TouchJumpMode
    {
        RightJumpZone,
        RightTap,
        FixedButton,
        RightTapAndFixedButton,
        RightJumpZoneAndFixedButton,
        LegacyRightFlick,
        LegacyRightFlickAndTap,
        LegacyRightFlickAndFixedButton,
        LegacyTapAnywhere
    }

    public enum TouchSensitivityPreset
    {
        Low,
        Medium,
        Fast,
        VeryFast
    }

    public enum TouchMovementSensitivityPreset
    {
        Low,
        Medium,
        Fast
    }

    public enum AutoCameraProfileKind
    {
        Direct,
        Balanced,
        Flow,
        SmartParkour
    }

    public enum TouchJumpSource
    {
        None,
        RightTap,
        RightFlick,
        TapAnywhere,
        FixedButton,
        Multiple
    }

    public enum FlickRejectReason
    {
        None,
        TooSlow,
        TooShort,
        TooLong,
        TooSideways,
        DirectionMismatch,
        NotRearmed,
        Cooldown
    }

    public readonly struct TouchControlRuntimeProfile
    {
        public TouchControlRuntimeProfile(
            TouchControlProfileKind controlProfile,
            TouchJumpMode jumpMode,
            TouchSensitivityPreset cameraSensitivity,
            TouchMovementSensitivityPreset movementSensitivity,
            bool movementOnRight,
            Vector2 movementZoneMin,
            Vector2 movementZoneMax,
            Vector2 movementRestCenter,
            Vector2 lookZoneMin,
            Vector2 lookZoneMax,
            Vector2 lookRestCenter,
            float cameraSensitivityMultiplier,
            float movementSensitivityMultiplier,
            bool manualLookEnabled,
            bool flowSteeringEnabled,
            AutoCameraProfileKind autoCameraProfile)
        {
            ControlProfile = controlProfile;
            JumpMode = jumpMode;
            CameraSensitivity = cameraSensitivity;
            MovementSensitivity = movementSensitivity;
            MovementOnRight = movementOnRight;
            MovementZoneMin = movementZoneMin;
            MovementZoneMax = movementZoneMax;
            MovementRestCenter = movementRestCenter;
            LookZoneMin = lookZoneMin;
            LookZoneMax = lookZoneMax;
            LookRestCenter = lookRestCenter;
            CameraSensitivityMultiplier = cameraSensitivityMultiplier;
            MovementSensitivityMultiplier = movementSensitivityMultiplier;
            ManualLookEnabled = manualLookEnabled;
            FlowSteeringEnabled = flowSteeringEnabled;
            AutoCameraProfile = autoCameraProfile;
        }

        public TouchControlProfileKind ControlProfile { get; }
        public TouchJumpMode JumpMode { get; }
        public TouchSensitivityPreset CameraSensitivity { get; }
        public TouchMovementSensitivityPreset MovementSensitivity { get; }
        public bool MovementOnRight { get; }
        public Vector2 MovementZoneMin { get; }
        public Vector2 MovementZoneMax { get; }
        public Vector2 MovementRestCenter { get; }
        public Vector2 LookZoneMin { get; }
        public Vector2 LookZoneMax { get; }
        public Vector2 LookRestCenter { get; }
        public float CameraSensitivityMultiplier { get; }
        public float MovementSensitivityMultiplier { get; }
        public bool ManualLookEnabled { get; }
        public bool FlowSteeringEnabled { get; }
        public AutoCameraProfileKind AutoCameraProfile { get; }
        public bool EnableRightFlick =>
            JumpMode == TouchJumpMode.LegacyRightFlick
            || JumpMode == TouchJumpMode.LegacyRightFlickAndTap
            || JumpMode == TouchJumpMode.LegacyRightFlickAndFixedButton;
        public bool EnableRightTap =>
            JumpMode == TouchJumpMode.RightTap
            || JumpMode == TouchJumpMode.RightTapAndFixedButton;
        public bool EnableRightJumpZone =>
            JumpMode == TouchJumpMode.RightJumpZone
            || JumpMode == TouchJumpMode.RightJumpZoneAndFixedButton;
        public bool EnableTapAnywhere =>
            JumpMode == TouchJumpMode.LegacyTapAnywhere;
        public bool EnableFixedButton =>
            JumpMode == TouchJumpMode.FixedButton
            || JumpMode == TouchJumpMode.RightTapAndFixedButton
            || JumpMode == TouchJumpMode.RightJumpZoneAndFixedButton
            || JumpMode == TouchJumpMode.LegacyRightFlickAndFixedButton;
        public string DisplayName => ControlProfile.ToString();

        public static TouchControlProfileKind Next(TouchControlProfileKind value)
        {
            return value == TouchControlProfileKind.LegacyStandardReversed
                ? TouchControlProfileKind.FlowSteerAutoBalanced
                : (TouchControlProfileKind)((int)value + 1);
        }

        public static TouchJumpMode Next(TouchJumpMode value)
        {
            return value == TouchJumpMode.LegacyTapAnywhere
                ? TouchJumpMode.RightJumpZone
                : (TouchJumpMode)((int)value + 1);
        }

        public static TouchSensitivityPreset Next(TouchSensitivityPreset value)
        {
            return value == TouchSensitivityPreset.VeryFast
                ? TouchSensitivityPreset.Low
                : (TouchSensitivityPreset)((int)value + 1);
        }

        public static TouchMovementSensitivityPreset Next(TouchMovementSensitivityPreset value)
        {
            return value == TouchMovementSensitivityPreset.Fast
                ? TouchMovementSensitivityPreset.Low
                : (TouchMovementSensitivityPreset)((int)value + 1);
        }
    }
}
