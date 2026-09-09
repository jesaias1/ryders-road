using UnityEngine;

namespace Avoidance.Gameplay.Player
{
    // Explicit thumb deflection, never velocity/route-derived optimal strafing.
    public static class RouteAirControlMath
    {
        // Original mobile adaptation: projected acceleration can earn energy;
        // unused projection budget can still correct heading with explicit input.
        // The correction preserves speed rather than imposing a total-speed ceiling.
        public static Vector3 AccelerateResponsive(Vector3 velocity, Vector3 wish,
            float wishSpeed, float acceleration, float braking, float amount,
            float steering, float dt, out float projected, out float requested)
        {
            projected = requested = 0;
            if (wish.sqrMagnitude < .0001f || amount <= 0 || dt <= 0) return velocity;
            wish.Normalize(); amount = Mathf.Clamp01(amount);
            projected = Vector3.Dot(velocity, wish);
            // Bounded integration steps reduce steering differences across frame rates.
            int steps = Mathf.CeilToInt(dt * 120f);
            float step = dt / steps;
            for (int i = 0; i < steps; i++)
            {
                float projection = Vector3.Dot(velocity, wish);
                float budget = (projection < 0 ? braking : acceleration) * amount * step;
                requested += budget;
                float push = Mathf.Min(budget, Mathf.Max(0, wishSpeed - projection));
                velocity += wish * push;
                float speed = velocity.magnitude;
                float unused = (budget - push) * steering;
                if (speed <= .001f || unused <= 0) continue;
                var heading = velocity / speed;
                var lateralWish = wish - heading * Vector3.Dot(wish, heading);
                var corrected = velocity + lateralWish * unused;
                velocity = corrected.normalized * speed;
            }
            return velocity;
        }

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
