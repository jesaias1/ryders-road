using System;
using UnityEngine;

namespace Avoidance.Gameplay.Camera
{
    public enum RouteCameraBranchKind
    {
        Standard,
        Shortcut,
        Alternate
    }

    public readonly struct RouteCameraNode
    {
        public RouteCameraNode(
            string stableId,
            Vector3 position,
            Vector3 forward,
            float verticalFramingHint = 0f,
            float cameraWeight = 1f,
            float lookAheadModifier = 1f,
            bool disabled = false)
        {
            StableId = stableId ?? string.Empty;
            Position = position;
            Forward = SmartParkourCameraMath.HorizontalDirectionOrFallback(forward, Vector3.forward);
            VerticalFramingHint = verticalFramingHint;
            CameraWeight = Mathf.Max(0f, cameraWeight);
            LookAheadModifier = Mathf.Max(0.1f, lookAheadModifier);
            Disabled = disabled;
        }

        public string StableId { get; }
        public Vector3 Position { get; }
        public Vector3 Forward { get; }
        public float VerticalFramingHint { get; }
        public float CameraWeight { get; }
        public float LookAheadModifier { get; }
        public bool Disabled { get; }
    }

    public sealed class RouteCameraBranch
    {
        private readonly RouteCameraNode[] _nodes;
        private readonly float[] _cumulativeDistances;

        public RouteCameraBranch(
            string stableId,
            RouteCameraBranchKind kind,
            RouteCameraNode[] nodes,
            float corridorRadius = 4f,
            string mergeBranchId = "")
        {
            StableId = stableId ?? string.Empty;
            Kind = kind;
            _nodes = nodes ?? Array.Empty<RouteCameraNode>();
            CorridorRadius = Mathf.Max(0.1f, corridorRadius);
            MergeBranchId = mergeBranchId ?? string.Empty;
            _cumulativeDistances = BuildCumulativeDistances(_nodes);
        }

        public string StableId { get; }
        public RouteCameraBranchKind Kind { get; }
        public float CorridorRadius { get; }
        public string MergeBranchId { get; }
        public int NodeCount => _nodes.Length;
        public float Length => _cumulativeDistances.Length == 0 ? 0f : _cumulativeDistances[_cumulativeDistances.Length - 1];
        public bool IsValid => _nodes.Length >= 2 && !string.IsNullOrWhiteSpace(StableId);

        public RouteCameraNode NodeAt(int index) => _nodes[Mathf.Clamp(index, 0, _nodes.Length - 1)];

        public RouteCameraSample Project(Vector3 position, float previousDistance = -1f)
        {
            if (!IsValid)
            {
                return RouteCameraSample.Empty;
            }

            var bestDistance = float.MaxValue;
            var bestAlong = 0f;
            var bestPoint = _nodes[0].Position;
            var bestDirection = _nodes[0].Forward;
            for (var index = 0; index < _nodes.Length - 1; index++)
            {
                var a = _nodes[index].Position;
                var b = _nodes[index + 1].Position;
                var segment = b - a;
                var segmentLength = segment.magnitude;
                if (segmentLength <= 0.001f)
                {
                    continue;
                }

                var t = Mathf.Clamp01(Vector3.Dot(position - a, segment) / (segmentLength * segmentLength));
                var point = a + segment * t;
                var distance = Vector3.Distance(position, point);
                var along = _cumulativeDistances[index] + segmentLength * t;
                var continuityPenalty = previousDistance < 0f
                    ? 0f
                    : Mathf.Abs(along - previousDistance) * 0.025f;
                var score = distance + continuityPenalty;
                if (score < bestDistance)
                {
                    bestDistance = score;
                    bestAlong = along;
                    bestPoint = point;
                    bestDirection = SmartParkourCameraMath.HorizontalDirectionOrFallback(segment, bestDirection);
                }
            }

            return new RouteCameraSample(this, bestPoint, bestDirection, bestAlong, bestDistance);
        }

        public RouteCameraSample SampleAhead(float distanceAlong, float lookAheadDistance)
        {
            if (!IsValid)
            {
                return RouteCameraSample.Empty;
            }

            var targetDistance = Mathf.Clamp(distanceAlong + Mathf.Max(0f, lookAheadDistance), 0f, Length);
            for (var index = 0; index < _nodes.Length - 1; index++)
            {
                var start = _cumulativeDistances[index];
                var end = _cumulativeDistances[index + 1];
                if (targetDistance > end && index < _nodes.Length - 2)
                {
                    continue;
                }

                var segmentLength = Mathf.Max(0.001f, end - start);
                var t = Mathf.Clamp01((targetDistance - start) / segmentLength);
                var a = _nodes[index].Position;
                var b = _nodes[index + 1].Position;
                var point = Vector3.Lerp(a, b, t);
                var direction = SmartParkourCameraMath.HorizontalDirectionOrFallback(b - a, _nodes[index].Forward);
                var verticalHint = Mathf.Lerp(
                    _nodes[index].VerticalFramingHint,
                    _nodes[index + 1].VerticalFramingHint,
                    t);
                return new RouteCameraSample(this, point, direction, targetDistance, 0f, verticalHint);
            }

            var last = _nodes[_nodes.Length - 1];
            return new RouteCameraSample(this, last.Position, last.Forward, Length, 0f, last.VerticalFramingHint);
        }

        private static float[] BuildCumulativeDistances(RouteCameraNode[] nodes)
        {
            var distances = new float[nodes == null ? 0 : nodes.Length];
            if (nodes == null)
            {
                return distances;
            }

            for (var index = 1; index < nodes.Length; index++)
            {
                distances[index] = distances[index - 1]
                    + Vector3.Distance(nodes[index - 1].Position, nodes[index].Position);
            }

            return distances;
        }
    }

    public readonly struct RouteCameraSample
    {
        public RouteCameraSample(
            RouteCameraBranch branch,
            Vector3 point,
            Vector3 direction,
            float distanceAlong,
            float distanceToRoute,
            float verticalFramingHint = 0f)
        {
            Branch = branch;
            Point = point;
            Direction = SmartParkourCameraMath.HorizontalDirectionOrFallback(direction, Vector3.forward);
            DistanceAlong = distanceAlong;
            DistanceToRoute = distanceToRoute;
            VerticalFramingHint = verticalFramingHint;
        }

        public RouteCameraBranch Branch { get; }
        public Vector3 Point { get; }
        public Vector3 Direction { get; }
        public float DistanceAlong { get; }
        public float DistanceToRoute { get; }
        public float VerticalFramingHint { get; }
        public bool IsValid => Branch != null && Branch.IsValid;
        public static RouteCameraSample Empty => new RouteCameraSample(null, Vector3.zero, Vector3.forward, 0f, float.MaxValue);
    }

    public sealed class RouteCameraGraph
    {
        private readonly RouteCameraBranch[] _branches;
        private RouteCameraBranch _primary;

        public RouteCameraGraph(string graphId, RouteCameraBranch[] branches, string primaryBranchId)
        {
            GraphId = graphId ?? string.Empty;
            _branches = branches ?? Array.Empty<RouteCameraBranch>();
            for (var index = 0; index < _branches.Length; index++)
            {
                if (_branches[index] != null && _branches[index].StableId == primaryBranchId)
                {
                    _primary = _branches[index];
                    break;
                }
            }

            _primary ??= _branches.Length > 0 ? _branches[0] : null;
        }

        public string GraphId { get; }
        public int BranchCount => _branches.Length;
        public RouteCameraBranch PrimaryBranch => _primary;
        public bool IsValid => _primary != null && _primary.IsValid;

        public RouteCameraBranch BranchAt(int index) => _branches[Mathf.Clamp(index, 0, _branches.Length - 1)];

        public RouteCameraBranch FindBranch(string branchId)
        {
            if (string.IsNullOrWhiteSpace(branchId))
            {
                return null;
            }

            for (var index = 0; index < _branches.Length; index++)
            {
                if (_branches[index] != null && _branches[index].StableId == branchId)
                {
                    return _branches[index];
                }
            }

            return null;
        }

        public RouteCameraSample ProjectBest(
            Vector3 position,
            string preferredBranchId,
            float previousDistance,
            float queryRadius)
        {
            var preferred = FindBranch(preferredBranchId) ?? _primary;
            var best = preferred == null ? RouteCameraSample.Empty : preferred.Project(position, previousDistance);
            var bestScore = best.DistanceToRoute;
            for (var index = 0; index < _branches.Length; index++)
            {
                var branch = _branches[index];
                if (branch == null || branch == preferred || !branch.IsValid)
                {
                    continue;
                }

                var sample = branch.Project(position);
                if (!sample.IsValid || sample.DistanceToRoute > queryRadius)
                {
                    continue;
                }

                var branchBias = branch.Kind == RouteCameraBranchKind.Standard ? 0.25f : 0f;
                var score = sample.DistanceToRoute + branchBias;
                if (score < bestScore)
                {
                    best = sample;
                    bestScore = score;
                }
            }

            return best;
        }

        public int CandidateBranches(Vector3 position, RouteCameraBranch[] buffer, float queryRadius)
        {
            if (buffer == null || buffer.Length == 0)
            {
                return 0;
            }

            var count = 0;
            for (var index = 0; index < _branches.Length && count < buffer.Length; index++)
            {
                var branch = _branches[index];
                if (branch == null || !branch.IsValid)
                {
                    continue;
                }

                var sample = branch.Project(position);
                if (branch.Kind == RouteCameraBranchKind.Standard
                    || sample.DistanceToRoute <= Mathf.Max(queryRadius, branch.CorridorRadius))
                {
                    buffer[count++] = branch;
                }
            }

            return count;
        }
    }
}
