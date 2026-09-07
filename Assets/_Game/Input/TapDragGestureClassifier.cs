using UnityEngine;

namespace Avoidance.Input
{
    public enum TapDragGestureState
    {
        Idle,
        Pending,
        Camera,
        Tap,
        Cancelled
    }

    public readonly struct TapDragGestureSettings
    {
        public TapDragGestureSettings(
            float tapMaxDuration,
            float tapMovementTolerance,
            float dragActivationDistance)
        {
            TapMaxDuration = Mathf.Max(0f, tapMaxDuration);
            TapMovementTolerance = Mathf.Max(0f, tapMovementTolerance);
            DragActivationDistance = Mathf.Max(0f, dragActivationDistance);
        }

        public float TapMaxDuration { get; }
        public float TapMovementTolerance { get; }
        public float DragActivationDistance { get; }

        public static TapDragGestureSettings Default =>
            new TapDragGestureSettings(0.16f, 0.035f, 0.018f);
    }

    public readonly struct TapDragGestureSnapshot
    {
        public TapDragGestureSnapshot(
            int pointerId,
            Vector2 startPosition,
            Vector2 currentPosition,
            float startTime,
            float elapsed,
            float totalDisplacement,
            float peakVelocity,
            TapDragGestureState state)
        {
            PointerId = pointerId;
            StartPosition = startPosition;
            CurrentPosition = currentPosition;
            StartTime = startTime;
            Elapsed = elapsed;
            TotalDisplacement = totalDisplacement;
            PeakVelocity = peakVelocity;
            State = state;
        }

        public int PointerId { get; }
        public Vector2 StartPosition { get; }
        public Vector2 CurrentPosition { get; }
        public float StartTime { get; }
        public float Elapsed { get; }
        public float TotalDisplacement { get; }
        public float PeakVelocity { get; }
        public TapDragGestureState State { get; }
    }

    public sealed class TapDragGestureClassifier
    {
        private int _pointerId = TouchOwnershipRegistry.UnassignedPointerId;
        private Vector2 _startPosition;
        private Vector2 _currentPosition;
        private Vector2 _previousPosition;
        private float _startTime;
        private float _previousTime;
        private float _elapsed;
        private float _totalDisplacement;
        private float _peakVelocity;
        private TapDragGestureState _state = TapDragGestureState.Idle;

        public TapDragGestureState State => _state;
        public bool IsCameraActive => _state == TapDragGestureState.Camera;
        public TapDragGestureSnapshot Snapshot => new TapDragGestureSnapshot(
            _pointerId,
            _startPosition,
            _currentPosition,
            _startTime,
            _elapsed,
            _totalDisplacement,
            _peakVelocity,
            _state);

        public void Begin(int pointerId, Vector2 screenPosition, float time)
        {
            _pointerId = pointerId;
            _startPosition = screenPosition;
            _currentPosition = screenPosition;
            _previousPosition = screenPosition;
            _startTime = time;
            _previousTime = time;
            _elapsed = 0f;
            _totalDisplacement = 0f;
            _peakVelocity = 0f;
            _state = TapDragGestureState.Pending;
        }

        public TapDragGestureState Update(
            int pointerId,
            Vector2 screenPosition,
            float time,
            float referenceLength,
            TapDragGestureSettings settings)
        {
            if (_pointerId != pointerId || _state == TapDragGestureState.Idle)
            {
                return TapDragGestureState.Idle;
            }

            if (_state == TapDragGestureState.Cancelled || _state == TapDragGestureState.Tap)
            {
                return _state;
            }

            UpdateMotion(screenPosition, time, referenceLength);
            if (_state == TapDragGestureState.Pending
                && _totalDisplacement >= settings.DragActivationDistance)
            {
                _state = TapDragGestureState.Camera;
            }

            return _state;
        }

        public TapDragGestureState Release(
            int pointerId,
            Vector2 screenPosition,
            float time,
            float referenceLength,
            TapDragGestureSettings settings)
        {
            if (_pointerId != pointerId || _state == TapDragGestureState.Idle)
            {
                return TapDragGestureState.Idle;
            }

            UpdateMotion(screenPosition, time, referenceLength);
            if (_state == TapDragGestureState.Pending
                && _elapsed <= settings.TapMaxDuration
                && _totalDisplacement <= settings.TapMovementTolerance)
            {
                _state = TapDragGestureState.Tap;
                return _state;
            }

            if (_state != TapDragGestureState.Camera)
            {
                _state = TapDragGestureState.Cancelled;
            }

            return _state;
        }

        public void Reset()
        {
            _pointerId = TouchOwnershipRegistry.UnassignedPointerId;
            _startPosition = Vector2.zero;
            _currentPosition = Vector2.zero;
            _previousPosition = Vector2.zero;
            _startTime = 0f;
            _previousTime = 0f;
            _elapsed = 0f;
            _totalDisplacement = 0f;
            _peakVelocity = 0f;
            _state = TapDragGestureState.Idle;
        }

        public void Cancel(int pointerId)
        {
            if (_pointerId == pointerId)
            {
                _state = TapDragGestureState.Cancelled;
            }
        }

        private void UpdateMotion(Vector2 screenPosition, float time, float referenceLength)
        {
            var safeReference = Mathf.Max(1f, referenceLength);
            var safeDeltaTime = Mathf.Max(0.0001f, time - _previousTime);
            _currentPosition = screenPosition;
            _elapsed = Mathf.Max(0f, time - _startTime);
            _totalDisplacement = (_currentPosition - _startPosition).magnitude / safeReference;
            var velocity = (_currentPosition - _previousPosition).magnitude / safeReference / safeDeltaTime;
            _peakVelocity = Mathf.Max(_peakVelocity, velocity);
            _previousPosition = _currentPosition;
            _previousTime = time;
        }
    }
}
