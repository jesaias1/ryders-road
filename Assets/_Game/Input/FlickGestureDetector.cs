using UnityEngine;

namespace Avoidance.Input
{
    public readonly struct FlickGestureSettings
    {
        public FlickGestureSettings(
            float minVelocity,
            float minDistance,
            float maxDuration,
            float maxHorizontalDeviation,
            float directionToleranceDegrees,
            float cooldown,
            float rearmDistance,
            bool requireMovementTouch,
            float velocityWindow)
        {
            MinVelocity = Mathf.Max(0f, minVelocity);
            MinDistance = Mathf.Max(0f, minDistance);
            MaxDuration = Mathf.Max(0.001f, maxDuration);
            MaxHorizontalDeviation = Mathf.Max(0f, maxHorizontalDeviation);
            DirectionToleranceDegrees = Mathf.Clamp(directionToleranceDegrees, 0f, 89f);
            Cooldown = Mathf.Max(0f, cooldown);
            RearmDistance = Mathf.Max(0f, rearmDistance);
            RequireMovementTouch = requireMovementTouch;
            VelocityWindow = Mathf.Max(0.001f, velocityWindow);
        }

        public float MinVelocity { get; }
        public float MinDistance { get; }
        public float MaxDuration { get; }
        public float MaxHorizontalDeviation { get; }
        public float DirectionToleranceDegrees { get; }
        public float Cooldown { get; }
        public float RearmDistance { get; }
        public bool RequireMovementTouch { get; }
        public float VelocityWindow { get; }

        public static FlickGestureSettings Default => new FlickGestureSettings(
            1.35f,
            0.055f,
            0.22f,
            0.075f,
            34f,
            0.08f,
            0.045f,
            true,
            0.08f);
    }

    public readonly struct FlickGestureResult
    {
        public FlickGestureResult(
            bool triggered,
            bool candidate,
            bool armed,
            Vector2 velocity,
            float flickVelocity,
            float distance,
            float duration,
            float directionAngle,
            float lastFlickTime,
            FlickRejectReason rejectReason)
        {
            Triggered = triggered;
            Candidate = candidate;
            Armed = armed;
            Velocity = velocity;
            FlickVelocity = flickVelocity;
            Distance = distance;
            Duration = duration;
            DirectionAngle = directionAngle;
            LastFlickTime = lastFlickTime;
            RejectReason = rejectReason;
        }

        public bool Triggered { get; }
        public bool Candidate { get; }
        public bool Armed { get; }
        public Vector2 Velocity { get; }
        public float FlickVelocity { get; }
        public float Distance { get; }
        public float Duration { get; }
        public float DirectionAngle { get; }
        public float LastFlickTime { get; }
        public FlickRejectReason RejectReason { get; }

        public static FlickGestureResult Empty => new FlickGestureResult(
            false,
            false,
            true,
            Vector2.zero,
            0f,
            0f,
            0f,
            0f,
            -1f,
            FlickRejectReason.None);
    }

    public sealed class FlickGestureDetector
    {
        private FlickGestureSettings _settings;
        private Vector2 _candidateStart;
        private Vector2 _lastPosition;
        private Vector2 _windowPosition;
        private Vector2 _triggerPosition;
        private float _candidateStartTime;
        private float _lastTime;
        private float _windowTime;
        private float _lastFlickTime = -999f;
        private bool _active;
        private bool _armed = true;
        private FlickGestureResult _lastResult = FlickGestureResult.Empty;

        public FlickGestureDetector(FlickGestureSettings settings)
        {
            _settings = settings;
        }

        public FlickGestureResult LastResult => _lastResult;

        public void Configure(FlickGestureSettings settings)
        {
            _settings = settings;
        }

        public void Begin(Vector2 position, float time)
        {
            _active = true;
            _armed = true;
            _candidateStart = position;
            _lastPosition = position;
            _windowPosition = position;
            _triggerPosition = position;
            _candidateStartTime = time;
            _lastTime = time;
            _windowTime = time;
            _lastResult = FlickGestureResult.Empty;
        }

        public FlickGestureResult Update(Vector2 position, float time, float referenceLength)
        {
            if (!_active)
            {
                Begin(position, time);
            }

            var reference = Mathf.Max(1f, referenceLength);
            var elapsedSinceLast = Mathf.Max(0.0001f, time - _lastTime);
            var instantVelocity = (position - _lastPosition) / elapsedSinceLast / reference;
            if (time - _windowTime > _settings.VelocityWindow)
            {
                _windowPosition = _lastPosition;
                _windowTime = _lastTime;
            }

            var windowElapsed = Mathf.Max(0.0001f, time - _windowTime);
            var windowVelocity = (position - _windowPosition) / windowElapsed / reference;
            var displacement = position - _candidateStart;
            var normalizedDistance = displacement.y / reference;
            var normalizedHorizontal = Mathf.Abs(displacement.x) / reference;
            var duration = Mathf.Max(0f, time - _candidateStartTime);
            var angle = displacement.sqrMagnitude <= 0.0001f
                ? 0f
                : Vector2.Angle(Vector2.up, displacement);
            var candidate = normalizedDistance > 0f
                && duration <= _settings.MaxDuration
                && normalizedHorizontal <= _settings.MaxHorizontalDeviation;
            var reject = ResolveRejectReason(
                candidate,
                normalizedDistance,
                normalizedHorizontal,
                duration,
                angle,
                windowVelocity.y,
                time);
            var triggered = reject == FlickRejectReason.None;

            if (triggered)
            {
                _armed = false;
                _triggerPosition = position;
                _lastFlickTime = time;
            }

            _lastResult = new FlickGestureResult(
                triggered,
                candidate,
                _armed,
                instantVelocity,
                windowVelocity.y,
                normalizedDistance,
                duration,
                angle,
                _lastFlickTime < 0f ? -1f : _lastFlickTime,
                reject);

            UpdateRearm(position, time, windowVelocity.y, reference);
            if (position.y <= _candidateStart.y || duration > _settings.MaxDuration || Mathf.Abs(instantVelocity.y) < 0.01f)
            {
                _candidateStart = position;
                _candidateStartTime = time;
            }

            _lastPosition = position;
            _lastTime = time;
            return _lastResult;
        }

        public void End()
        {
            _active = false;
            _armed = true;
            _lastResult = new FlickGestureResult(
                false,
                false,
                true,
                Vector2.zero,
                0f,
                0f,
                0f,
                0f,
                _lastFlickTime < 0f ? -1f : _lastFlickTime,
                FlickRejectReason.None);
        }

        private FlickRejectReason ResolveRejectReason(
            bool candidate,
            float normalizedDistance,
            float normalizedHorizontal,
            float duration,
            float directionAngle,
            float verticalVelocity,
            float time)
        {
            if (!_armed)
            {
                return FlickRejectReason.NotRearmed;
            }

            if (time - _lastFlickTime < _settings.Cooldown)
            {
                return FlickRejectReason.Cooldown;
            }

            if (duration > _settings.MaxDuration)
            {
                return FlickRejectReason.TooLong;
            }

            if (normalizedHorizontal > _settings.MaxHorizontalDeviation)
            {
                return FlickRejectReason.TooSideways;
            }

            if (directionAngle > _settings.DirectionToleranceDegrees)
            {
                return FlickRejectReason.DirectionMismatch;
            }

            if (normalizedDistance < _settings.MinDistance)
            {
                return FlickRejectReason.TooShort;
            }

            if (verticalVelocity < _settings.MinVelocity)
            {
                return FlickRejectReason.TooSlow;
            }

            return candidate ? FlickRejectReason.None : FlickRejectReason.TooShort;
        }

        private void UpdateRearm(
            Vector2 position,
            float time,
            float verticalVelocity,
            float referenceLength)
        {
            if (_armed || time - _lastFlickTime < _settings.Cooldown)
            {
                return;
            }

            var retreated = (_triggerPosition.y - position.y) / referenceLength >= _settings.RearmDistance;
            var settled = verticalVelocity <= _settings.MinVelocity * 0.35f;
            if (retreated && settled)
            {
                _armed = true;
                _candidateStart = position;
                _candidateStartTime = time;
            }
        }
    }
}
