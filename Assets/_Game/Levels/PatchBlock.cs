using Avoidance.Gameplay.Audio;
using Avoidance.Gameplay.Player;
using Avoidance.Gameplay.Timing;
using UnityEngine;

namespace Avoidance.Gameplay.Levels
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class PatchBlock : MonoBehaviour
    {
        private ModuleAttemptTelemetry _telemetry;
        private MovementFeedback _feedback;
        private System.Action _onCompleted;
        private System.Action<ParkourMotor> _onCompletedWithMotor;
        private bool _completed;

        public bool Completed => _completed;

        public void Initialize(
            ModuleAttemptTelemetry telemetry,
            MovementFeedback feedback,
            System.Action onCompleted,
            System.Action<ParkourMotor> onCompletedWithMotor = null)
        {
            _telemetry = telemetry;
            _feedback = feedback;
            _onCompleted = onCompleted;
            _onCompletedWithMotor = onCompletedWithMotor;
            GetComponent<Collider>().isTrigger = true;
        }

        public bool TryComplete(ParkourMotor motor)
        {
            if (_completed || motor == null)
            {
                return false;
            }

            _completed = true;
            _onCompletedWithMotor?.Invoke(motor);
            _telemetry?.RecordFinish();
            _feedback?.PlayFinish();
            _onCompleted?.Invoke();
            return true;
        }

        private void OnTriggerEnter(Collider other)
        {
            TryComplete(other.GetComponent<ParkourMotor>());
        }
    }
}
