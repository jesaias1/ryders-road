using System.Collections.Generic;
using UnityEngine;

namespace Avoidance.Input
{
    public enum PlayerInputMode
    {
        Auto,
        Editor,
        Touch
    }

    public enum TouchControlRole
    {
        Movement,
        Look,
        Jump
    }

    public readonly struct TouchInputDebugState
    {
        public TouchInputDebugState(
            Vector2 rawJoystick,
            Vector2 postDeadZoneJoystick,
            Vector2 normalizedMovement,
            int movementTouchId,
            int lookTouchId,
            int jumpTouchId)
            : this(
                rawJoystick,
                postDeadZoneJoystick,
                normalizedMovement,
                int.MinValue,
                Vector2.zero,
                Vector2.zero,
                Vector2.zero,
                0f,
                0f,
                0f,
                0f,
                0f,
                false,
                true,
                false,
                -1f,
                TouchJumpSource.None,
                FlickRejectReason.None,
                TouchControlProfileKind.LeftMoveRightLookTapJump,
                TouchJumpMode.RightTap,
                TouchSensitivityPreset.Fast,
                TouchMovementSensitivityPreset.Medium,
                false,
                AutoCameraProfileKind.Balanced,
                movementTouchId,
                lookTouchId,
                jumpTouchId)
        {
        }

        public TouchInputDebugState(
            Vector2 rawJoystick,
            Vector2 postDeadZoneJoystick,
            Vector2 normalizedMovement,
            int rightTouchId,
            Vector2 movementOrigin,
            Vector2 movementKnob,
            Vector2 touchVelocity,
            float flickVelocity,
            float flickDistance,
            float flickDuration,
            float flickDirectionAngle,
            float lastFlickTime,
            bool flickCandidate,
            bool flickArmed,
            bool flickTriggered,
            float jumpTapDuration,
            TouchJumpSource lastJumpSource,
            FlickRejectReason flickRejectReason,
            TouchControlProfileKind controlProfile,
            TouchJumpMode jumpMode,
            TouchSensitivityPreset cameraSensitivity,
            TouchMovementSensitivityPreset movementSensitivity,
            bool flowSteeringEnabled,
            AutoCameraProfileKind autoCameraProfile,
            int movementTouchId,
            int lookTouchId,
            int jumpTouchId)
        {
            RawJoystick = rawJoystick;
            PostDeadZoneJoystick = postDeadZoneJoystick;
            NormalizedMovement = normalizedMovement;
            RightTouchId = rightTouchId;
            MovementOrigin = movementOrigin;
            MovementKnob = movementKnob;
            TouchVelocity = touchVelocity;
            FlickVelocity = flickVelocity;
            FlickDistance = flickDistance;
            FlickDuration = flickDuration;
            FlickDirectionAngle = flickDirectionAngle;
            LastFlickTime = lastFlickTime;
            FlickCandidate = flickCandidate;
            FlickArmed = flickArmed;
            FlickTriggered = flickTriggered;
            JumpTapDuration = jumpTapDuration;
            LastJumpSource = lastJumpSource;
            FlickRejectReason = flickRejectReason;
            ControlProfile = controlProfile;
            JumpMode = jumpMode;
            CameraSensitivity = cameraSensitivity;
            MovementSensitivity = movementSensitivity;
            FlowSteeringEnabled = flowSteeringEnabled;
            AutoCameraProfile = autoCameraProfile;
            MovementTouchId = movementTouchId;
            LookTouchId = lookTouchId;
            JumpTouchId = jumpTouchId;
        }

        public Vector2 RawJoystick { get; }
        public Vector2 PostDeadZoneJoystick { get; }
        public Vector2 NormalizedMovement { get; }
        public int RightTouchId { get; }
        public Vector2 MovementOrigin { get; }
        public Vector2 MovementKnob { get; }
        public Vector2 TouchVelocity { get; }
        public float FlickVelocity { get; }
        public float FlickDistance { get; }
        public float FlickDuration { get; }
        public float FlickDirectionAngle { get; }
        public float LastFlickTime { get; }
        public bool FlickCandidate { get; }
        public bool FlickArmed { get; }
        public bool FlickTriggered { get; }
        public float JumpTapDuration { get; }
        public TouchJumpSource LastJumpSource { get; }
        public FlickRejectReason FlickRejectReason { get; }
        public TouchControlProfileKind ControlProfile { get; }
        public TouchJumpMode JumpMode { get; }
        public TouchSensitivityPreset CameraSensitivity { get; }
        public TouchMovementSensitivityPreset MovementSensitivity { get; }
        public bool FlowSteeringEnabled { get; }
        public AutoCameraProfileKind AutoCameraProfile { get; }
        public int MovementTouchId { get; }
        public int LookTouchId { get; }
        public int JumpTouchId { get; }

        public static TouchInputDebugState Empty => new TouchInputDebugState(
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            TouchOwnershipRegistry.UnassignedPointerId,
            TouchOwnershipRegistry.UnassignedPointerId,
            TouchOwnershipRegistry.UnassignedPointerId);
    }

    public interface ITouchInputProvider
    {
        Vector2 Movement { get; }
        TouchInputDebugState DebugState { get; }
        Vector2 ConsumeLookDelta();
        bool ConsumeJumpPressed();
        bool ConsumeRestartPressed();
        bool ConsumeSwitchProfilePressed();
        bool ConsumeToggleCameraEffectsPressed();
        bool ConsumeToggleDiagnosticsPressed();
        IReadOnlyList<int> ActiveTouchIds { get; }
        void ResetState();
        void SetZonesVisible(bool visible);
    }

    public sealed class TouchOwnershipRegistry
    {
        public const int UnassignedPointerId = int.MinValue;

        private readonly int[] _owners =
        {
            UnassignedPointerId,
            UnassignedPointerId,
            UnassignedPointerId
        };

        public bool TryClaim(TouchControlRole role, int pointerId)
        {
            var roleIndex = (int)role;
            for (var index = 0; index < _owners.Length; index++)
            {
                if (_owners[index] == pointerId && index != roleIndex)
                {
                    return false;
                }
            }

            if (_owners[roleIndex] != UnassignedPointerId && _owners[roleIndex] != pointerId)
            {
                return false;
            }

            _owners[roleIndex] = pointerId;
            return true;
        }

        public bool IsOwner(TouchControlRole role, int pointerId)
        {
            return _owners[(int)role] == pointerId;
        }

        public void Release(TouchControlRole role, int pointerId)
        {
            if (IsOwner(role, pointerId))
            {
                _owners[(int)role] = UnassignedPointerId;
            }
        }

        public int GetOwner(TouchControlRole role) => _owners[(int)role];

        public void Reset()
        {
            for (var index = 0; index < _owners.Length; index++)
            {
                _owners[index] = UnassignedPointerId;
            }
        }
    }

    public static class InputSourceSelector
    {
        public static PlayerInputMode Resolve(
            PlayerInputMode configuredMode,
            bool touchIsActive,
            bool isMobilePlatform)
        {
            if (configuredMode != PlayerInputMode.Auto)
            {
                return configuredMode;
            }

            return touchIsActive || isMobilePlatform
                ? PlayerInputMode.Touch
                : PlayerInputMode.Editor;
        }
    }

    public static class TouchStickMath
    {
        public static Vector2 NormalizeLocalPoint(Vector2 localPoint, Rect rect)
        {
            var radius = Mathf.Min(rect.width, rect.height) * 0.5f;
            if (radius <= 0f)
            {
                return Vector2.zero;
            }

            var centered = localPoint - rect.center;
            return Vector2.ClampMagnitude(centered / radius, 1f);
        }

        public static Vector2 ApplyDeadZone(Vector2 normalizedInput, float deadZone)
        {
            var clamped = Vector2.ClampMagnitude(normalizedInput, 1f);
            var magnitude = clamped.magnitude;
            var threshold = Mathf.Clamp(deadZone, 0f, 0.99f);
            if (magnitude <= threshold)
            {
                return Vector2.zero;
            }

            return clamped.normalized * ((magnitude - threshold) / (1f - threshold));
        }
    }
}
