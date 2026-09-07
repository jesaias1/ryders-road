using UnityEngine;

namespace Avoidance.Gameplay.Blocks
{
    public sealed class PlatformMotionTracker
    {
        private Vector3 _lastPosition;
        private bool _hasPosition;

        public Vector3 Capture(Vector3 currentPosition)
        {
            var delta = _hasPosition ? currentPosition - _lastPosition : Vector3.zero;
            _lastPosition = currentPosition;
            _hasPosition = true;
            return delta;
        }

        public void Reset(Vector3 currentPosition)
        {
            _lastPosition = currentPosition;
            _hasPosition = true;
        }
    }

    [DisallowMultipleComponent]
    public sealed class MovingPlatformMotion : MonoBehaviour
    {
        [SerializeField] private Vector3 _localOffset = new Vector3(5f, 0f, 0f);
        [SerializeField] private float _cycleDuration = 4f;
        private Vector3 _startPosition;
        private float _elapsed;

        public Vector3 Velocity { get; private set; }

        private void Awake()
        {
            _startPosition = transform.position;
        }

        private void FixedUpdate()
        {
            var duration = Mathf.Max(0.2f, _cycleDuration);
            _elapsed += Time.fixedDeltaTime;
            var normalized = Mathf.PingPong(_elapsed / duration * 2f, 1f);
            var previous = transform.position;
            transform.position = Vector3.Lerp(_startPosition, _startPosition + _localOffset, normalized);
            Velocity = (transform.position - previous) / Time.fixedDeltaTime;
        }

        public void Configure(Vector3 localOffset, float cycleDuration)
        {
            _localOffset = localOffset;
            _cycleDuration = Mathf.Max(0.2f, cycleDuration);
        }

        public void ResetMotion()
        {
            _elapsed = 0f;
            transform.position = _startPosition;
            Velocity = Vector3.zero;
        }
    }
}
