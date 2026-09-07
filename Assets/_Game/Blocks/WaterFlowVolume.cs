using Avoidance.Gameplay.Audio;
using Avoidance.Gameplay.Player;
using UnityEngine;

namespace Avoidance.Gameplay.Blocks
{
    public static class WaterFlowMath
    {
        public static Vector3 ComputeHorizontalDelta(
            Vector3 currentHorizontalVelocity,
            Vector3 flowDirection,
            float flowForce,
            float maxAddedVelocity,
            float deltaTime)
        {
            var horizontal = Vector3.ProjectOnPlane(flowDirection, Vector3.up);
            if (horizontal.sqrMagnitude < 0.001f
                || flowForce <= 0f
                || maxAddedVelocity <= 0f
                || deltaTime <= 0f)
            {
                return Vector3.zero;
            }

            horizontal.Normalize();
            var currentAlongFlow = Vector3.Dot(currentHorizontalVelocity, horizontal);
            var remaining = Mathf.Max(0f, maxAddedVelocity - currentAlongFlow);
            var deltaSpeed = Mathf.Min(flowForce * deltaTime, remaining);
            return horizontal * deltaSpeed;
        }
    }

    [DisallowMultipleComponent]
    public sealed class WaterFlowVolume : MonoBehaviour
    {
        private Vector3 _flowDirection = Vector3.forward;
        private float _flowForce = 16f;
        private float _maxAddedVelocity = 5f;
        private float _verticalInfluence;
        private float _drag = 1f;
        private float _nextAudioTime;
        private System.Action<Vector3> _onFlowApplied;

        public Vector3 FlowDirection => _flowDirection;

        public void Initialize(
            Vector3 flowDirection,
            float flowForce,
            float maxAddedVelocity,
            float verticalInfluence,
            float drag,
            System.Action<Vector3> onFlowApplied)
        {
            _flowDirection = flowDirection.sqrMagnitude < 0.001f
                ? transform.forward
                : flowDirection.normalized;
            _flowForce = Mathf.Max(0f, flowForce);
            _maxAddedVelocity = Mathf.Max(0f, maxAddedVelocity);
            _verticalInfluence = Mathf.Max(0f, verticalInfluence);
            _drag = Mathf.Max(0f, drag);
            _onFlowApplied = onFlowApplied;
        }

        private void OnTriggerStay(Collider other)
        {
            var motor = other.GetComponent<ParkourMotor>();
            if (motor == null)
            {
                return;
            }

            motor.ApplyWaterFlow(
                _flowDirection,
                _flowForce,
                _maxAddedVelocity,
                _verticalInfluence,
                _drag,
                Time.deltaTime);
            if (Time.time >= _nextAudioTime)
            {
                other.GetComponent<MovementFeedback>()?.PlayWater();
                _nextAudioTime = Time.time + 0.75f;
            }

            _onFlowApplied?.Invoke(transform.position);
        }
    }
}
