using System.Collections.Generic;
using Avoidance.Gameplay.Audio;
using Avoidance.Gameplay.Player;
using UnityEngine;

namespace Avoidance.Gameplay.Blocks
{
    public static class JumpBoostMath
    {
        public static Vector3 ResolveLaunchVelocity(
            Vector3 launchDirection,
            float verticalStrength,
            float horizontalStrength)
        {
            var horizontal = Vector3.ProjectOnPlane(launchDirection, Vector3.up);
            if (horizontal.sqrMagnitude > 0.001f)
            {
                horizontal = horizontal.normalized * Mathf.Max(0f, horizontalStrength);
            }

            return horizontal + Vector3.up * Mathf.Max(0f, verticalStrength);
        }

        public static bool TryEstimateDescendingFlightTime(
            float verticalSpeed,
            float targetHeightDelta,
            float ascentGravity,
            float descentGravity,
            out float flightTime)
        {
            flightTime = 0f;
            if (verticalSpeed <= 0f || ascentGravity <= 0f || descentGravity <= 0f)
            {
                return false;
            }

            var apexTime = verticalSpeed / ascentGravity;
            var apexHeight = verticalSpeed * verticalSpeed / (2f * ascentGravity);
            if (targetHeightDelta > apexHeight)
            {
                return false;
            }

            var descentDistance = Mathf.Max(0f, apexHeight - targetHeightDelta);
            flightTime = apexTime + Mathf.Sqrt(2f * descentDistance / descentGravity);
            return true;
        }

        public static float EstimateAlignedHorizontalTravel(
            float horizontalBoostSpeed,
            float approachSpeed,
            float flightTime)
        {
            return (Mathf.Max(0f, horizontalBoostSpeed) + Mathf.Max(0f, approachSpeed))
                * Mathf.Max(0f, flightTime);
        }
    }

    [DisallowMultipleComponent]
    public sealed class JumpBoostBlock : MonoBehaviour, IModuleResettable
    {
        private readonly Dictionary<ParkourMotor, float> _nextAllowedLaunchTime =
            new Dictionary<ParkourMotor, float>();
        private Vector3 _launchDirection = Vector3.forward;
        private float _verticalStrength = 12f;
        private float _horizontalStrength = 7f;
        private float _cooldown = 0.35f;
        private System.Action<Vector3> _onLaunched;

        public Vector3 LaunchVelocity => JumpBoostMath.ResolveLaunchVelocity(
            _launchDirection,
            _verticalStrength,
            _horizontalStrength);

        public void Initialize(
            Vector3 launchDirection,
            float verticalStrength,
            float horizontalStrength,
            float cooldown,
            System.Action<Vector3> onLaunched)
        {
            _launchDirection = launchDirection.sqrMagnitude < 0.001f
                ? transform.forward
                : launchDirection;
            _verticalStrength = Mathf.Max(0f, verticalStrength);
            _horizontalStrength = Mathf.Max(0f, horizontalStrength);
            _cooldown = Mathf.Max(0f, cooldown);
            _onLaunched = onLaunched;
        }

        public bool CanLaunch(ParkourMotor motor, float time)
        {
            if (motor == null)
            {
                return false;
            }

            return !_nextAllowedLaunchTime.TryGetValue(motor, out var next)
                || time >= next;
        }

        public bool TryLaunch(ParkourMotor motor, float time)
        {
            if (!CanLaunch(motor, time))
            {
                return false;
            }

            motor.ApplyLaunch(LaunchVelocity, replaceHorizontal: false);
            _nextAllowedLaunchTime[motor] = time + _cooldown;
            var feedback = motor.GetComponent<MovementFeedback>();
            feedback?.PlayBoost();
            _onLaunched?.Invoke(transform.position);
            return true;
        }

        public void ResetLaunchCooldowns()
        {
            _nextAllowedLaunchTime.Clear();
        }

        public void ResetForModule()
        {
            ResetLaunchCooldowns();
        }
    }

    public sealed class JumpBoostTrigger : MonoBehaviour
    {
        private JumpBoostBlock _boost;

        public void Initialize(JumpBoostBlock boost)
        {
            _boost = boost;
        }

        private void OnTriggerEnter(Collider other)
        {
            _boost?.TryLaunch(other.GetComponent<ParkourMotor>(), Time.time);
        }
    }
}
