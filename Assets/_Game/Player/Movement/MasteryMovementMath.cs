using UnityEngine;

namespace Avoidance.Gameplay.Player
{
    public static class MasteryMovementMath
    {
        // Add only along wish. Preserve the perpendicular component and pre-existing
        // overspeed; bound the added energy, never rotate velocity to match the view.
        public static Vector3 Accelerate(Vector3 velocity, Vector3 wish, float wishSpeed,
            float acceleration, float speedLimit, float dt)
        {
            if (wish.sqrMagnitude < .0001f || dt <= 0) return velocity;
            wish.Normalize();
            float along = Vector3.Dot(velocity, wish);
            float add = Mathf.Min(Mathf.Max(0, wishSpeed - along), acceleration * dt);
            float limit = Mathf.Max(velocity.magnitude, speedLimit);
            float available = Mathf.Max(0, -along + Mathf.Sqrt(Mathf.Max(0,
                along * along + limit * limit - velocity.sqrMagnitude)));
            return velocity + wish * Mathf.Min(add, available);
        }

        public static Vector3 ClipIntoPlane(Vector3 velocity, Vector3 normal)
        {
            float into = Vector3.Dot(velocity, normal);
            return into < 0 ? velocity - normal * into : velocity;
        }
    }
}
