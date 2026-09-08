using UnityEngine;

namespace Avoidance.Gameplay.Player
{
    // Explicit thumb deflection, never velocity/route-derived optimal strafing.
    public static class RouteAirControlMath
    {
        public static Vector3 FlowWish(Vector3 forward, Vector2 input, float angle)
        {
            if (Mathf.Abs(input.y) <= .001f) return Vector3.zero;
            return Quaternion.AngleAxis(input.x * angle, Vector3.up) * forward * Mathf.Sign(input.y);
        }

        public static Vector3 Accelerate(Vector3 velocity, Vector3 wish, float wishSpeed,
            float acceleration, float braking, float inputAmount, float speedLimit, float dt,
            out float projected, out float requested, out float applied, out bool limited)
        {
            projected = requested = applied = 0; limited = false;
            if (wish.sqrMagnitude < .0001f || inputAmount <= 0 || dt <= 0) return velocity;
            wish.Normalize();
            projected = Vector3.Dot(velocity, wish);
            requested = (projected < 0 ? braking : acceleration) * Mathf.Clamp01(inputAmount) * dt;
            applied = Mathf.Min(Mathf.Max(0, wishSpeed - projected), requested);
            var result = velocity + wish * applied;
            // Limit energy AFTER applying wish. The old energy-intersection bound
            // reduced lateral acceleration to zero at the ceiling, locking heading.
            float ceiling = Mathf.Max(velocity.magnitude, speedLimit);
            limited = result.sqrMagnitude > ceiling * ceiling;
            if (limited) result = result.normalized * ceiling;
            return result;
        }
    }
}
