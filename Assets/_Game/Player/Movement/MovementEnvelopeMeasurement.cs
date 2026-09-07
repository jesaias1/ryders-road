using Avoidance.Gameplay.Blocks;
using UnityEngine;

namespace Avoidance.Gameplay.Player
{
    public readonly struct MovementEnvelopeMetrics
    {
        public MovementEnvelopeMetrics(
            float runSpeed,
            float jumpAirtime,
            float normalJumpDistance,
            float comfortableBronzeGap,
            float landingMarginDistance,
            float maximumBronzeGap,
            float bhopAssistedDistance,
            float airStrafeAssistedDistance,
            float boostAssistedDistance)
        {
            RunSpeed = runSpeed;
            JumpAirtime = jumpAirtime;
            NormalJumpDistance = normalJumpDistance;
            ComfortableBronzeGap = comfortableBronzeGap;
            LandingMarginDistance = landingMarginDistance;
            MaximumBronzeGap = maximumBronzeGap;
            BhopAssistedDistance = bhopAssistedDistance;
            AirStrafeAssistedDistance = airStrafeAssistedDistance;
            BoostAssistedDistance = boostAssistedDistance;
        }

        public float RunSpeed { get; }
        public float JumpAirtime { get; }
        public float NormalJumpDistance { get; }
        public float ComfortableBronzeGap { get; }
        public float LandingMarginDistance { get; }
        public float MaximumBronzeGap { get; }
        public float BhopAssistedDistance { get; }
        public float AirStrafeAssistedDistance { get; }
        public float BoostAssistedDistance { get; }
    }

    public static class MovementEnvelopeMeasurement
    {
        public static MovementEnvelopeMetrics Measure(
            MovementProfile profile,
            float boostVerticalStrength = 12.4f,
            float boostHorizontalStrength = 7.2f)
        {
            var airtime = profile.EstimatedAirtime;
            var normalDistance = profile.NormalTakeoffSpeed * airtime;
            var bhopDistance = profile.SoftMomentumLimit
                * profile.HighMomentumTakeoffRetention
                * airtime;
            var strafeSpeed = Mathf.Min(
                profile.HardVelocitySafetyLimit,
                profile.BaseRunSpeed + profile.MaxAirSpeedContribution);
            var airStrafeDistance = strafeSpeed * airtime;
            var boost = JumpBoostMath.ResolveLaunchVelocity(
                Vector3.forward,
                boostVerticalStrength,
                boostHorizontalStrength);
            var boostAscent = boost.y / Mathf.Max(0.01f, profile.JumpGravity);
            var boostHeight = boost.y * boost.y / (2f * Mathf.Max(0.01f, profile.JumpGravity));
            var boostDescent = Mathf.Sqrt(
                2f * boostHeight / Mathf.Max(0.01f, profile.FallGravity));
            var boostHorizontal = Mathf.Min(
                profile.HardVelocitySafetyLimit,
                profile.NormalTakeoffSpeed + new Vector2(boost.x, boost.z).magnitude);

            return new MovementEnvelopeMetrics(
                profile.RunSpeed,
                airtime,
                normalDistance,
                normalDistance * 0.65f,
                normalDistance * 0.78f,
                normalDistance * 0.85f,
                bhopDistance,
                airStrafeDistance,
                boostHorizontal * (boostAscent + boostDescent));
        }
    }
}
