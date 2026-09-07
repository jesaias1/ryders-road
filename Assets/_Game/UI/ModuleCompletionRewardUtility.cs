using System;
using System.Collections.Generic;
using Avoidance.Gameplay.Levels;
using Avoidance.Gameplay.Ranking;
using Avoidance.SaveSystem;

namespace Avoidance.UI
{
    public static class ModuleCompletionRewardUtility
    {
        public static int ApplyCompletionRewards(
            ModuleDefinition module,
            SaveDocument save,
            ModuleRank achievedRank,
            RunValidity validity,
            string utcNow) =>
            ApplyCompletionRewards(
                module,
                save,
                achievedRank,
                validity,
                utcNow,
                out _);

        public static int ApplyCompletionRewards(
            ModuleDefinition module,
            SaveDocument save,
            ModuleRank achievedRank,
            RunValidity validity,
            string utcNow,
            out string[] rewardLines)
        {
            rewardLines = Array.Empty<string>();
            if (module == null
                || save == null
                || !ModuleRankUtility.IsValidForPersonalBest(validity))
            {
                return 0;
            }

            save.progression ??= new ProgressionData();
            save.economy ??= new EconomyData();
            save.inventory ??= new InventoryData();
            ModuleProgressionData.EnsureArrays(save.progression);
            EconomyProgressionData.EnsureArrays(save.economy);
            InventoryProgressionData.EnsureArrays(save.inventory);

            var lines = new List<string>();
            foreach (var reward in module.CompletionRewards)
            {
                if (!CanApply(save.progression, reward, achievedRank))
                {
                    continue;
                }

                ApplyReward(save, reward, utcNow);
                ModuleProgressionData.MarkRewardClaimed(save.progression, reward.StableId);
                lines.Add(RewardLine(reward));
            }

            rewardLines = lines.ToArray();
            return rewardLines.Length;
        }

        private static bool CanApply(
            ProgressionData progression,
            ModuleCompletionRewardDefinition reward,
            ModuleRank achievedRank)
        {
            if (reward == null || achievedRank < reward.MinimumRank)
            {
                return false;
            }

            return !reward.FirstCompletionOnly
                || !ModuleProgressionData.HasClaimedReward(progression, reward.StableId);
        }

        private static void ApplyReward(
            SaveDocument save,
            ModuleCompletionRewardDefinition reward,
            string utcNow)
        {
            switch (reward.Kind)
            {
                case ModuleRewardKind.Currency:
                    EconomyProgressionData.AddBalance(save.economy, reward.ContentId, reward.Quantity);
                    break;
                case ModuleRewardKind.InventoryItem:
                    InventoryProgressionData.GrantItem(
                        save.inventory,
                        reward.ContentId,
                        reward.Quantity,
                        utcNow,
                        reward.StableId);
                    break;
                case ModuleRewardKind.Unlock:
                    InventoryProgressionData.SetUnlock(
                        save.inventory,
                        reward.ContentId,
                        reward.UnlockType,
                        true,
                        utcNow,
                        reward.StableId);
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported reward kind: {reward.Kind}");
            }
        }

        private static string RewardLine(ModuleCompletionRewardDefinition reward)
        {
            switch (reward.Kind)
            {
                case ModuleRewardKind.Currency:
                    return $"+{reward.Quantity} PATCH SHARDS";
                case ModuleRewardKind.InventoryItem:
                    return "LOOT PACKAGE";
                case ModuleRewardKind.Unlock:
                    return "SKIN UNLOCK";
                default:
                    return "REWARD";
            }
        }
    }
}
