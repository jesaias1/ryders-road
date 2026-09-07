using UnityEngine;

namespace Avoidance.Gameplay.Player
{
    public readonly struct MovementVectorState
    {
        public MovementVectorState(
            Vector2 input,
            Vector3 projectedForward,
            Vector3 projectedRight,
            Vector3 desiredDirection)
        {
            Input = input;
            ProjectedForward = projectedForward;
            ProjectedRight = projectedRight;
            DesiredDirection = desiredDirection;
        }

        public Vector2 Input { get; }
        public Vector3 ProjectedForward { get; }
        public Vector3 ProjectedRight { get; }
        public Vector3 DesiredDirection { get; }
    }

    public static class MovementVectorMath
    {
        public static MovementVectorState ResolveCameraRelative(
            Quaternion yawRotation,
            Vector2 moveInput)
        {
            var forward = Vector3.ProjectOnPlane(yawRotation * Vector3.forward, Vector3.up);
            var right = Vector3.ProjectOnPlane(yawRotation * Vector3.right, Vector3.up);
            forward = forward.sqrMagnitude <= 0.0001f ? Vector3.forward : forward.normalized;
            right = right.sqrMagnitude <= 0.0001f ? Vector3.right : right.normalized;
            var clampedInput = Vector2.ClampMagnitude(moveInput, 1f);
            var desired = Vector3.ClampMagnitude(
                right * clampedInput.x + forward * clampedInput.y,
                1f);

            return new MovementVectorState(clampedInput, forward, right, desired);
        }

        public static float ResolveLateralVelocity(Vector3 horizontalVelocity, Vector3 projectedRight)
        {
            if (projectedRight.sqrMagnitude <= 0.0001f)
            {
                return 0f;
            }

            return Vector3.Dot(horizontalVelocity, projectedRight.normalized);
        }
    }
}
