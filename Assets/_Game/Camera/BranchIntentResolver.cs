using UnityEngine;

namespace Avoidance.Gameplay.Camera
{
    public enum BranchIntentState
    {
        Neutral,
        Leaning,
        Committed
    }

    public readonly struct BranchIntentResult
    {
        public BranchIntentResult(
            BranchIntentState state,
            string branchId,
            float confidence,
            float winningScore,
            float runnerUpScore,
            Vector3 direction)
        {
            State = state;
            BranchId = branchId ?? string.Empty;
            Confidence = confidence;
            WinningScore = winningScore;
            RunnerUpScore = runnerUpScore;
            Direction = SmartParkourCameraMath.HorizontalDirectionOrFallback(direction, Vector3.forward);
        }

        public BranchIntentState State { get; }
        public string BranchId { get; }
        public float Confidence { get; }
        public float WinningScore { get; }
        public float RunnerUpScore { get; }
        public Vector3 Direction { get; }
        public bool HasBranch => !string.IsNullOrWhiteSpace(BranchId);
    }

    public sealed class BranchIntentResolver
    {
        private string _committedBranchId = string.Empty;
        private string _candidateBranchId = string.Empty;
        private float _candidateSeconds;
        private float _releaseSeconds;
        private BranchIntentState _state = BranchIntentState.Neutral;

        public string CommittedBranchId => _committedBranchId;
        public BranchIntentState State => _state;

        public void Reset()
        {
            _committedBranchId = string.Empty;
            _candidateBranchId = string.Empty;
            _candidateSeconds = 0f;
            _releaseSeconds = 0f;
            _state = BranchIntentState.Neutral;
        }

        public BranchIntentResult Update(
            RouteCameraBranch[] candidates,
            int count,
            Vector3 playerPosition,
            Vector3 velocityDirection,
            Vector3 steeringDirection,
            bool airborne,
            ParkourCameraProfile profile,
            float deltaTime)
        {
            if (candidates == null || count <= 0 || profile == null)
            {
                Reset();
                return new BranchIntentResult(BranchIntentState.Neutral, string.Empty, 0f, 0f, 0f, Vector3.forward);
            }

            var winningScore = -1f;
            var runnerUpScore = -1f;
            RouteCameraBranch winner = null;
            var winnerDirection = Vector3.forward;
            for (var index = 0; index < count; index++)
            {
                var branch = candidates[index];
                if (branch == null || !branch.IsValid)
                {
                    continue;
                }

                var projection = branch.Project(playerPosition);
                var ahead = branch.SampleAhead(
                    projection.DistanceAlong,
                    Mathf.Max(4f, profile.RouteLookAheadDistance * 0.65f));
                var direction = SmartParkourCameraMath.HorizontalDirectionOrFallback(
                    ahead.Point - playerPosition,
                    projection.Direction);
                var score = ScoreBranch(
                    branch,
                    projection.DistanceToRoute,
                    direction,
                    velocityDirection,
                    steeringDirection,
                    profile);
                if (branch.StableId == _committedBranchId)
                {
                    score += profile.BranchScoreHysteresis;
                }

                if (score > winningScore)
                {
                    runnerUpScore = winningScore;
                    winningScore = score;
                    winner = branch;
                    winnerDirection = direction;
                }
                else if (score > runnerUpScore)
                {
                    runnerUpScore = score;
                }
            }

            if (winner == null)
            {
                Reset();
                return new BranchIntentResult(BranchIntentState.Neutral, string.Empty, 0f, 0f, 0f, Vector3.forward);
            }

            var margin = Mathf.Max(0f, winningScore - Mathf.Max(0f, runnerUpScore));
            var confidence = Mathf.Clamp01(winningScore * 0.75f + margin * 0.5f);
            var threshold = string.IsNullOrWhiteSpace(_committedBranchId) || winner.StableId == _committedBranchId
                ? profile.BranchCommitThreshold
                : profile.BranchSwitchThreshold;
            if (airborne && !string.IsNullOrWhiteSpace(_committedBranchId) && winner.StableId != _committedBranchId)
            {
                threshold = Mathf.Min(0.98f, threshold + 0.12f);
            }

            if (winner.StableId == _candidateBranchId)
            {
                _candidateSeconds += Mathf.Max(0f, deltaTime);
            }
            else
            {
                _candidateBranchId = winner.StableId;
                _candidateSeconds = Mathf.Max(0f, deltaTime);
            }

            if (confidence >= threshold && _candidateSeconds >= profile.BranchCommitDuration)
            {
                _committedBranchId = winner.StableId;
                _releaseSeconds = 0f;
                _state = BranchIntentState.Committed;
            }
            else if (!string.IsNullOrWhiteSpace(_committedBranchId))
            {
                if (confidence < profile.BranchCommitThreshold * 0.6f)
                {
                    _releaseSeconds += Mathf.Max(0f, deltaTime);
                    if (_releaseSeconds >= profile.BranchReleaseDuration)
                    {
                        _committedBranchId = string.Empty;
                        _state = BranchIntentState.Neutral;
                    }
                }
                else
                {
                    _releaseSeconds = 0f;
                    _state = BranchIntentState.Committed;
                }
            }
            else
            {
                _state = confidence >= profile.BranchCommitThreshold * 0.65f
                    ? BranchIntentState.Leaning
                    : BranchIntentState.Neutral;
            }

            var outputBranch = _state == BranchIntentState.Committed
                ? _committedBranchId
                : _state == BranchIntentState.Leaning
                    ? winner.StableId
                    : string.Empty;
            var outputDirection = winnerDirection;
            if (_state == BranchIntentState.Committed && winner.StableId != _committedBranchId)
            {
                for (var index = 0; index < count; index++)
                {
                    var candidate = candidates[index];
                    if (candidate == null || candidate.StableId != _committedBranchId)
                    {
                        continue;
                    }

                    var projection = candidate.Project(playerPosition);
                    var ahead = candidate.SampleAhead(
                        projection.DistanceAlong,
                        Mathf.Max(4f, profile.RouteLookAheadDistance * 0.65f));
                    outputDirection = SmartParkourCameraMath.HorizontalDirectionOrFallback(
                        ahead.Point - playerPosition,
                        projection.Direction);
                    break;
                }
            }

            return new BranchIntentResult(
                _state,
                outputBranch,
                confidence,
                winningScore,
                Mathf.Max(0f, runnerUpScore),
                outputDirection);
        }

        private static float ScoreBranch(
            RouteCameraBranch branch,
            float distanceToRoute,
            Vector3 branchDirection,
            Vector3 velocityDirection,
            Vector3 steeringDirection,
            ParkourCameraProfile profile)
        {
            var velocityAlignment = SmartParkourCameraMath.Alignment01(velocityDirection, branchDirection);
            var steeringAlignment = SmartParkourCameraMath.Alignment01(steeringDirection, branchDirection);
            var proximity = Mathf.Clamp01(1f - distanceToRoute / Mathf.Max(0.01f, branch.CorridorRadius));
            var shortcutBias = branch.Kind == RouteCameraBranchKind.Shortcut ? 0.03f : 0f;
            return velocityAlignment * profile.VelocityBranchWeight
                + steeringAlignment * profile.SteeringBranchWeight
                + proximity * profile.PositionBranchWeight
                + shortcutBias;
        }
    }
}
