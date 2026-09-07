using System;
using Avoidance.Gameplay.Levels;

namespace Avoidance.Gameplay.Ranking
{
    public enum ModuleRank
    {
        None = 0,
        Bronze = 1,
        Silver = 2,
        Gold = 3,
        Diamond = 4
    }

    public enum RankCalibrationState
    {
        Uncalibrated,
        InTesting,
        Calibrated
    }

    public enum RunValidity
    {
        ValidUnassisted,
        ValidAssisted,
        InvalidDevelopment,
        InvalidContentVersion,
        InvalidOther
    }

    public static class ModuleRankUtility
    {
        public static ModuleRank Evaluate(double completionSeconds, ModuleRankThresholds thresholds)
        {
            if (completionSeconds <= 0d)
            {
                return ModuleRank.None;
            }

            if (thresholds != null && thresholds.IsOrdered)
            {
                if (completionSeconds <= thresholds.DiamondSeconds)
                {
                    return ModuleRank.Diamond;
                }

                if (completionSeconds <= thresholds.GoldSeconds)
                {
                    return ModuleRank.Gold;
                }

                if (completionSeconds <= thresholds.SilverSeconds)
                {
                    return ModuleRank.Silver;
                }
            }

            return ModuleRank.Bronze;
        }

        public static ModuleRank CurrentTarget(double elapsedSeconds, ModuleRankThresholds thresholds)
        {
            if (thresholds == null || !thresholds.IsOrdered)
            {
                return ModuleRank.Bronze;
            }

            if (elapsedSeconds <= thresholds.DiamondSeconds)
            {
                return ModuleRank.Diamond;
            }

            if (elapsedSeconds <= thresholds.GoldSeconds)
            {
                return ModuleRank.Gold;
            }

            if (elapsedSeconds <= thresholds.SilverSeconds)
            {
                return ModuleRank.Silver;
            }

            return ModuleRank.Bronze;
        }

        public static double ThresholdFor(ModuleRank rank, ModuleRankThresholds thresholds)
        {
            if (thresholds == null)
            {
                return 0d;
            }

            switch (rank)
            {
                case ModuleRank.Silver:
                    return thresholds.SilverSeconds;
                case ModuleRank.Gold:
                    return thresholds.GoldSeconds;
                case ModuleRank.Diamond:
                    return thresholds.DiamondSeconds;
                default:
                    return 0d;
            }
        }

        public static ModuleRank NextHigherRank(ModuleRank rank)
        {
            switch (rank)
            {
                case ModuleRank.Bronze:
                    return ModuleRank.Silver;
                case ModuleRank.Silver:
                    return ModuleRank.Gold;
                case ModuleRank.Gold:
                    return ModuleRank.Diamond;
                default:
                    return ModuleRank.None;
            }
        }

        public static bool IsValidForPersonalBest(RunValidity validity) =>
            validity == RunValidity.ValidUnassisted || validity == RunValidity.ValidAssisted;

        public static string Display(ModuleRank rank) =>
            rank == ModuleRank.None ? "UNRANKED" : rank.ToString().ToUpperInvariant();
    }

    public static class ModuleScoreService
    {
        public const long ScoreCeiling = 100000000000L;

        public static long CalculateScore(double completionSeconds, ModuleRankThresholds thresholds)
        {
            if (completionSeconds <= 0d || double.IsNaN(completionSeconds))
            {
                return 0L;
            }

            var elapsedMicroseconds = (long)Math.Ceiling(completionSeconds * 1000000d);
            var score = ScoreCeiling - elapsedMicroseconds;
            return Math.Max(1L, score);
        }
    }
}
