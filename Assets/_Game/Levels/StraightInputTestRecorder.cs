using Avoidance.Core.Services;
using Avoidance.Gameplay.Player;
using UnityEngine;

namespace Avoidance.Gameplay.Levels
{
    [DisallowMultipleComponent]
    public sealed class StraightInputTestRecorder : MonoBehaviour
    {
        private IDiagnosticsService _diagnostics;
        private PlayerRuntimeCoordinator _player;
        private float _laneCenterX;
        private float _startLateral;
        private float _endLateral;
        private float _sampleCount;
        private float _joystickXTotal;
        private float _lateralVelocityTotal;

        public void Initialize(float laneCenterX)
        {
            _laneCenterX = laneCenterX;
            if (GameServices.Current != null)
            {
                GameServices.Current.TryGet<IDiagnosticsService>(out _diagnostics);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_player != null)
            {
                return;
            }

            var player = other.GetComponent<PlayerRuntimeCoordinator>();
            if (player == null)
            {
                return;
            }

            _player = player;
            _sampleCount = 0f;
            _joystickXTotal = 0f;
            _lateralVelocityTotal = 0f;
            _startLateral = other.transform.position.x - _laneCenterX;
            _endLateral = _startLateral;
            Publish("running");
        }

        private void OnTriggerStay(Collider other)
        {
            if (_player == null || other.transform != _player.transform)
            {
                return;
            }

            _sampleCount += 1f;
            _joystickXTotal += _player.Input.Move.x;
            _lateralVelocityTotal += _player.Motor.LateralVelocity;
            _endLateral = other.transform.position.x - _laneCenterX;
            Publish("running");
        }

        private void OnTriggerExit(Collider other)
        {
            if (_player == null || other.transform != _player.transform)
            {
                return;
            }

            _endLateral = other.transform.position.x - _laneCenterX;
            Publish("complete");
            _player = null;
        }

        private void Publish(string state)
        {
            if (_diagnostics == null)
            {
                return;
            }

            var averageJoystickX = _sampleCount <= 0f ? 0f : _joystickXTotal / _sampleCount;
            var averageLateralVelocity = _sampleCount <= 0f
                ? 0f
                : _lateralVelocityTotal / _sampleCount;
            _diagnostics.SetValue("Straight test state", state);
            _diagnostics.SetValue("Straight start lateral", _startLateral.ToString("0.000"));
            _diagnostics.SetValue("Straight end lateral", _endLateral.ToString("0.000"));
            _diagnostics.SetValue("Straight total drift", (_endLateral - _startLateral).ToString("0.000"));
            _diagnostics.SetValue("Straight average joystick X", averageJoystickX.ToString("0.000"));
            _diagnostics.SetValue("Straight average lateral velocity", averageLateralVelocity.ToString("0.000"));
        }
    }
}
