using UnityEngine;

namespace Avoidance.Gameplay.Player
{
    public static class SurfMovementMath
    {
        public static bool IsSurfAngle(Vector3 surfaceNormal, SurfProfile profile)
        {
            if (profile == null || surfaceNormal.sqrMagnitude <= 0.0001f)
            {
                return false;
            }

            var angle = Vector3.Angle(surfaceNormal.normalized, Vector3.up);
            return profile.IsValidAngle(angle);
        }

        public static Vector3 ProjectVelocity(Vector3 velocity, Vector3 surfaceNormal)
        {
            if (surfaceNormal.sqrMagnitude <= 0.0001f)
            {
                return velocity;
            }

            return Vector3.ProjectOnPlane(velocity, surfaceNormal.normalized);
        }

        public static Vector3 DownSlope(Vector3 surfaceNormal)
        {
            var downSlope = Vector3.ProjectOnPlane(Vector3.down, surfaceNormal.normalized);
            return downSlope.sqrMagnitude <= 0.0001f ? Vector3.zero : downSlope.normalized;
        }

        public static Vector3 ComputeVelocity(
            Vector3 currentVelocity,
            Vector3 surfaceNormal,
            Vector3 wishDirection,
            float inputAmount,
            SurfProfile profile,
            float deltaTime,
            bool mastery = false)
        {
            if (profile == null || deltaTime <= 0f)
            {
                return currentVelocity;
            }

            var normal = surfaceNormal.sqrMagnitude <= 0.0001f
                ? Vector3.up
                : surfaceNormal.normalized;
            var velocity = ProjectVelocity(currentVelocity, normal);
            velocity += (mastery ? Vector3.ProjectOnPlane(Vector3.down, normal) : DownSlope(normal))
                * profile.SurfGravity * deltaTime;

            if (wishDirection.sqrMagnitude > 0.0001f && inputAmount > 0.001f)
            {
                var surfWish = Vector3.ProjectOnPlane(wishDirection, normal);
                if (surfWish.sqrMagnitude > 0.0001f)
                {
                    if (mastery)
                    {
                        velocity = MasteryMovementMath.Accelerate(velocity, surfWish,
                            profile.SurfWishSpeed * Mathf.Clamp01(inputAmount),
                            profile.SurfAcceleration * profile.SurfControl * Mathf.Clamp01(inputAmount),
                            profile.MaximumSurfSpeed, deltaTime);
                    }
                    else
                    velocity += surfWish.normalized
                        * profile.SurfAcceleration
                        * profile.SurfControl
                        * Mathf.Clamp01(inputAmount)
                        * deltaTime;
                }
            }

            if (profile.SurfFriction > 0f)
            {
                velocity = Vector3.MoveTowards(
                    velocity,
                    Vector3.zero,
                    profile.SurfFriction * deltaTime);
            }

            return velocity.sqrMagnitude > profile.MaximumSurfSpeed * profile.MaximumSurfSpeed
                ? velocity.normalized * profile.MaximumSurfSpeed
                : velocity;
        }
    }
}
