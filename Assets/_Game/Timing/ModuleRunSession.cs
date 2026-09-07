using System;
using System.Collections.Generic;
using Avoidance.Gameplay.Levels;
using Avoidance.Gameplay.Ranking;

namespace Avoidance.Gameplay.Timing
{
    [Serializable]
    public sealed class RunSplit
    {
        public string checkpointId;
        public double seconds;
        public double pbDeltaSeconds;
        public bool hasPersonalBestComparison;
    }

    public sealed class ModuleRunResult
    {
        public string ModuleId { get; set; }
        public string NewlyUnlockedModuleId { get; set; }
        public int ModuleContentVersion { get; set; }
        public int MovementCompatibilityVersion { get; set; }
        public int RankThresholdVersion { get; set; }
        public string RankCalibrationState { get; set; }
        public double CompletionSeconds { get; set; }
        public ModuleRank Rank { get; set; }
        public long Score { get; set; }
        public RunValidity Validity { get; set; }
        public RunSplit[] Splits { get; set; } = Array.Empty<RunSplit>();
        public bool IsNewPersonalBest { get; set; }
        public double PersonalBestSeconds { get; set; }
        public double PersonalBestDeltaSeconds { get; set; }
        public ModuleRank NextRank { get; set; }
        public double SecondsFromNextRank { get; set; }
        public string[] RewardLines { get; set; } = Array.Empty<string>();
    }

    public sealed class ModuleRunSession
    {
        private readonly List<RunSplit> _splits = new List<RunSplit>();
        private readonly Dictionary<string, double> _pbSplitsById =
            new Dictionary<string, double>(StringComparer.Ordinal);
        private ModuleDefinition _module;
        private RunValidity _validity;

        public RunTimerService Timer { get; } = new RunTimerService();
        public IReadOnlyList<RunSplit> Splits => _splits;
        public ModuleRunResult Result { get; private set; }
        public RunValidity Validity => _validity;

        public void InvalidateForDevelopmentUse()
        {
            if (_validity != RunValidity.InvalidDevelopment)
            {
                _validity = RunValidity.InvalidDevelopment;
            }
        }

        public void Begin(
            ModuleDefinition module,
            RunValidity validity,
            IEnumerable<RunSplit> personalBestSplits = null)
        {
            _module = module;
            _validity = validity;
            Result = null;
            _splits.Clear();
            _pbSplitsById.Clear();
            if (personalBestSplits != null)
            {
                foreach (var split in personalBestSplits)
                {
                    if (split != null && !string.IsNullOrWhiteSpace(split.checkpointId))
                    {
                        _pbSplitsById[split.checkpointId] = split.seconds;
                    }
                }
            }

            Timer.Reset();
            Timer.Start();
        }

        public RunSplit RecordSplit(string checkpointId)
        {
            if (string.IsNullOrWhiteSpace(checkpointId))
            {
                return null;
            }

            var split = new RunSplit
            {
                checkpointId = checkpointId,
                seconds = Timer.ElapsedSeconds
            };
            if (_pbSplitsById.TryGetValue(checkpointId, out var pbSeconds))
            {
                split.hasPersonalBestComparison = true;
                split.pbDeltaSeconds = split.seconds - pbSeconds;
            }

            _splits.Add(split);
            return split;
        }

        public ModuleRunResult Complete(int movementCompatibilityVersion)
        {
            if (Result != null)
            {
                return Result;
            }

            var finalSeconds = Timer.Complete();
            var thresholds = _module?.RankThresholds;
            var rank = ModuleRankUtility.Evaluate(finalSeconds, thresholds);
            var nextRank = ModuleRankUtility.NextHigherRank(rank);
            var nextThreshold = ModuleRankUtility.ThresholdFor(nextRank, thresholds);
            Result = new ModuleRunResult
            {
                ModuleId = _module == null ? string.Empty : _module.StableModuleId,
                ModuleContentVersion = _module == null ? 0 : _module.ContentVersion,
                MovementCompatibilityVersion = movementCompatibilityVersion,
                RankThresholdVersion = thresholds?.ThresholdVersion ?? 0,
                RankCalibrationState = thresholds == null
                    ? RankCalibrationState.Uncalibrated.ToString()
                    : thresholds.CalibrationState.ToString(),
                CompletionSeconds = finalSeconds,
                Rank = rank,
                Score = ModuleScoreService.CalculateScore(finalSeconds, thresholds),
                Validity = _validity,
                Splits = _splits.ToArray(),
                NextRank = nextRank,
                SecondsFromNextRank = nextThreshold <= 0d
                    ? 0d
                    : Math.Max(0d, finalSeconds - nextThreshold)
            };
            return Result;
        }
    }
}
