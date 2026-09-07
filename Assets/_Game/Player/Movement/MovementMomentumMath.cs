using UnityEngine;

namespace Avoidance.Gameplay.Player
{
    public enum HopTimingQuality
    {
        Perfect,
        Good,
        Buffered,
        Late
    }

    public readonly struct HopRetentionResult
    {
        public HopRetentionResult(
            HopTimingQuality quality,
            float retention,
            float speedAfterRetention)
        {
            Quality = quality;
            Retention = retention;
            SpeedAfterRetention = speedAfterRetention;
        }

        public HopTimingQuality Quality { get; }
        public float Retention { get; }
        public float SpeedAfterRetention { get; }
    }

    public static class MovementMomentumMath
    {
        public static HopRetentionResult ResolveHopRetention(
            float secondsSinceLanding,
            float currentSpeed,
            float baseRunSpeed,
            MovementProfile profile)
        {
            var quality = ClassifyHop(secondsSinceLanding, profile);
            var retention = RetentionFor(quality, profile);
            var minimum = Mathf.Max(0f, baseRunSpeed);
            var clampedCurrent = Mathf.Max(0f, currentSpeed);
            var retainedSpeed = clampedCurrent <= minimum
                ? clampedCurrent
                : Mathf.Lerp(minimum, clampedCurrent, retention);
            return new HopRetentionResult(quality, retention, retainedSpeed);
        }

        public static HopTimingQuality ClassifyHop(
            float secondsSinceLanding,
            MovementProfile profile)
        {
            if (secondsSinceLanding <= profile.PerfectHopWindow)
            {
                return HopTimingQuality.Perfect;
            }

            if (secondsSinceLanding <= profile.GoodHopWindow)
            {
                return HopTimingQuality.Good;
            }

            if (secondsSinceLanding <= profile.BufferedHopWindow)
            {
                return HopTimingQuality.Buffered;
            }

            return HopTimingQuality.Late;
        }

        public static Vector3 ClampHorizontalVelocity(Vector3 velocity, float hardLimit)
        {
            var limit = Mathf.Max(0.1f, hardLimit);
            return velocity.sqrMagnitude > limit * limit
                ? velocity.normalized * limit
                : velocity;
        }

        private static float RetentionFor(
            HopTimingQuality quality,
            MovementProfile profile)
        {
            switch (quality)
            {
                case HopTimingQuality.Perfect:
                    return profile.PerfectHopRetention;
                case HopTimingQuality.Good:
                    return profile.GoodHopRetention;
                case HopTimingQuality.Buffered:
                    return profile.BufferedHopRetention;
                default:
                    return profile.LateHopPenalty;
            }
        }
    }
}
