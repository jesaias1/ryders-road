using Avoidance.Gameplay.Audio;
using Avoidance.Gameplay.Timing;
using UnityEngine;

namespace Avoidance.Gameplay.Levels
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class MovementLabFinish : MonoBehaviour
    {
        private MovementSessionStats _stats;
        private MovementSessionReporter _reporter;
        private MovementFeedback _feedback;
        private System.Action _onFinished;
        private bool _activated;

        public bool IsActivated => _activated;

        public void Initialize(
            MovementSessionStats stats,
            MovementSessionReporter reporter,
            MovementFeedback feedback,
            System.Action onFinished)
        {
            _stats = stats;
            _reporter = reporter;
            _feedback = feedback;
            _onFinished = onFinished;
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_activated
                || other.GetComponent<Avoidance.Gameplay.Player.ParkourMotor>() == null)
            {
                return;
            }

            _activated = true;
            _stats.RecordFinish();
            _feedback.PlayFinish();
            _reporter.Export();
            _onFinished?.Invoke();
        }
    }
}
