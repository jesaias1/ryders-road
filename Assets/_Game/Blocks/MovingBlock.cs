using System.Collections.Generic;
using Avoidance.Gameplay.Levels;
using UnityEngine;

namespace Avoidance.Gameplay.Blocks
{
    public static class MovingBlockMotionMath
    {
        public static Vector3 Evaluate(
            IReadOnlyList<Vector3> points,
            ModuleMovingLoopMode loopMode,
            ModuleEasing easing,
            float speed,
            float elapsedSeconds,
            float pauseAtEndpoints = 0f)
        {
            if (points == null || points.Count == 0)
            {
                return Vector3.zero;
            }

            if (points.Count == 1 || speed <= 0f)
            {
                return points[0];
            }

            var totalLength = 0f;
            for (var index = 0; index < points.Count - 1; index++)
            {
                totalLength += Vector3.Distance(points[index], points[index + 1]);
            }

            if (totalLength <= 0.001f)
            {
                return points[0];
            }

            // A time-based endpoint dwell works at every speed and timestep, including
            // Linear easing. It never depends on accidentally sampling near zero motion.
            var duration = totalLength / speed;
            var pause = Mathf.Max(0f, pauseAtEndpoints);
            var legDuration = duration + pause;
            var time = Mathf.Max(0f, elapsedSeconds);
            if (loopMode == ModuleMovingLoopMode.PingPong)
            {
                var cycle = Mathf.Repeat(time, legDuration * 2f);
                var returning = cycle >= legDuration;
                var legTime = returning ? cycle - legDuration : cycle;
                var distance = Mathf.Min(legTime, duration) * speed;
                return EvaluateDistance(points, returning ? totalLength - distance : distance, easing);
            }

            return EvaluateDistance(points, Mathf.Min(Mathf.Repeat(time, legDuration), duration) * speed, easing);
        }

        private static Vector3 EvaluateDistance(
            IReadOnlyList<Vector3> points,
            float distance,
            ModuleEasing easing)
        {
            for (var index = 0; index < points.Count - 1; index++)
            {
                var length = Vector3.Distance(points[index], points[index + 1]);
                if (length <= 0.001f)
                {
                    continue;
                }

                if (distance <= length || index == points.Count - 2)
                {
                    var t = Mathf.Clamp01(distance / length);
                    if (easing == ModuleEasing.SmoothStep)
                    {
                        t = t * t * (3f - 2f * t);
                    }

                    return Vector3.Lerp(points[index], points[index + 1], t);
                }

                distance -= length;
            }

            return points[points.Count - 1];
        }
    }

    [DisallowMultipleComponent]
    public sealed class MovingBlock : MonoBehaviour, IModuleResettable
    {
        [SerializeField] private Vector3[] _pathPoints = new[] { Vector3.zero, Vector3.right * 4f };
        [SerializeField] private ModulePathSpace _pathSpace = ModulePathSpace.World;
        [SerializeField] private ModuleMovingLoopMode _loopMode = ModuleMovingLoopMode.PingPong;
        [SerializeField] private ModuleEasing _easing = ModuleEasing.SmoothStep;
        [SerializeField] private float _speed = 2.5f;
        [SerializeField] private float _pauseAtEndpoints = 0.15f;
        [SerializeField] private float _startingPhase;

        private Vector3 _basePosition;
        private float _elapsed;

        public Vector3 Velocity { get; private set; }

        private void Awake()
        {
            _basePosition = transform.position;
        }

        private void FixedUpdate()
        {
            _elapsed += Time.fixedDeltaTime;
            var previous = transform.position;
            var next = EvaluatePosition(_elapsed + _startingPhase);
            transform.position = next;
            Velocity = (next - previous) / Time.fixedDeltaTime;


        }

        public void Configure(ModuleMovingBlockDefinition definition)
        {
            _pathPoints = CopyPoints(definition.PathPoints);
            _pathSpace = definition.PathSpace;
            _loopMode = definition.LoopMode;
            _easing = definition.Easing;
            _speed = Mathf.Max(0.01f, definition.Speed);
            _pauseAtEndpoints = Mathf.Max(0f, definition.PauseAtEndpoints);
            _startingPhase = Mathf.Max(0f, definition.StartingPhase);
            _basePosition = transform.position;
            ResetForModule();
        }

        public Vector3 EvaluatePosition(float elapsedSeconds)
        {
            var evaluated = MovingBlockMotionMath.Evaluate(
                _pathPoints,
                _loopMode,
                _easing,
                _speed,
                elapsedSeconds,
                _pauseAtEndpoints);
            return _pathSpace == ModulePathSpace.Local
                ? _basePosition + evaluated
                : evaluated;
        }

        public void ResetForModule()
        {
            _elapsed = 0f;
            transform.position = EvaluatePosition(_startingPhase);
            Velocity = Vector3.zero;
        }

        private static Vector3[] CopyPoints(IReadOnlyList<Vector3> points)
        {
            if (points == null || points.Count == 0)
            {
                return new[] { Vector3.zero, Vector3.right };
            }

            var copy = new Vector3[points.Count];
            for (var index = 0; index < points.Count; index++)
            {
                copy[index] = points[index];
            }

            return copy;
        }
    }
}
