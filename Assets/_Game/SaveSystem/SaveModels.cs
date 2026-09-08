using System;
using Avoidance.Core.Services;
using Avoidance.Core.Utilities;

namespace Avoidance.SaveSystem
{
    [Serializable]
    public sealed class SaveDocument
    {
        public int schemaVersion = SaveSchema.CurrentVersion;
        public string gameVersion = "0.0.1";
        public GameSettings settings = new GameSettings();
        public ProgressionData progression = new ProgressionData();
        public EconomyData economy = new EconomyData();
        public InventoryData inventory = new InventoryData();
    }

    [Serializable]
    public sealed class ProgressionData
    {
        public ModuleProgressRecord[] modules = Array.Empty<ModuleProgressRecord>();
        public ModuleProgressRecord[] historicalRecords = Array.Empty<ModuleProgressRecord>();
        public ModuleProgressRecord[] versionedBests = Array.Empty<ModuleProgressRecord>();
        public string[] exceptionalUnlocks = Array.Empty<string>();
        public string[] claimedRewardIds = Array.Empty<string>();
    }

    public static class SaveSchema
    {
        public const int CurrentVersion = 4;
    }

    [Serializable]
    public sealed class EconomyData
    {
        public CurrencyBalanceRecord[] balances = Array.Empty<CurrencyBalanceRecord>();
        public int lootPackagesOpened;
    }

    [Serializable]
    public sealed class CurrencyBalanceRecord
    {
        public string currencyId;
        public long amount;
    }

    [Serializable]
    public sealed class InventoryData
    {
        public InventoryItemRecord[] items = Array.Empty<InventoryItemRecord>();
        public UnlockRecord[] unlocks = Array.Empty<UnlockRecord>();
        public EquippedCosmeticRecord[] equippedCosmetics = Array.Empty<EquippedCosmeticRecord>();
    }

    [Serializable]
    public sealed class InventoryItemRecord
    {
        public string contentId;
        public int quantity;
        public string firstAcquiredUtc;
        public string lastAcquiredUtc;
        public string sourceId;
    }

    [Serializable]
    public sealed class UnlockRecord
    {
        public string contentId;
        public string unlockType;
        public bool unlocked;
        public string unlockedUtc;
        public string sourceId;
    }

    [Serializable]
    public sealed class EquippedCosmeticRecord
    {
        public string slotId;
        public string contentId;
    }

    [Serializable]
    public sealed class ModuleProgressRecord
    {
        public string moduleId;
        public bool completed;
        public double bestTimeSeconds;
        public string bestRank;
        public long bestScore;
        public int completionCount;
        public int attemptCount;
        public ModuleSplitRecord[] bestSplits = Array.Empty<ModuleSplitRecord>();
        public double latestCompletionTimeSeconds;
        public string latestRank;
        public string highestRank;
        public string firstCompletionUtc;
        public string lastCompletionUtc;
        public int moduleContentVersion;
        public int movementCompatibilityVersion;
        public int rankThresholdVersion;
        public string rankCalibrationState;
        public string runValidity;
    }

    [Serializable]
    public sealed class ModuleSplitRecord
    {
        public string checkpointId;
        public double seconds;
    }

    public static class ModuleProgressionData
    {
        public static ModuleProgressRecord GetRecord(
            ProgressionData progression,
            string moduleId)
        {
            if (progression == null || string.IsNullOrWhiteSpace(moduleId))
            {
                return null;
            }

            EnsureArrays(progression);
            foreach (var record in progression.modules)
            {
                if (record != null && string.Equals(record.moduleId, moduleId, StringComparison.Ordinal))
                {
                    return record;
                }
            }

            return null;
        }

        public static ModuleProgressRecord GetOrCreateRecord(
            ProgressionData progression,
            string moduleId)
        {
            if (progression == null)
            {
                throw new ArgumentNullException(nameof(progression));
            }

            if (string.IsNullOrWhiteSpace(moduleId))
            {
                throw new ArgumentException("Module ID is required.", nameof(moduleId));
            }

            EnsureArrays(progression);
            var existing = GetRecord(progression, moduleId);
            if (existing != null)
            {
                return existing;
            }

            var record = new ModuleProgressRecord { moduleId = moduleId };
            var next = new ModuleProgressRecord[progression.modules.Length + 1];
            Array.Copy(progression.modules, next, progression.modules.Length);
            next[next.Length - 1] = record;
            progression.modules = next;
            return record;
        }

        public static bool IsUnlocked(
            ProgressionData progression,
            string[] orderedModuleIds,
            string moduleId)
        {
            if (string.IsNullOrWhiteSpace(moduleId)
                || orderedModuleIds == null
                || orderedModuleIds.Length == 0)
            {
                return false;
            }

            EnsureArrays(progression);
            var target = Array.IndexOf(orderedModuleIds, moduleId);
            if (target < 0) return false;
            if (target == 0) return true;
            // Legacy saves can contain later completions. Preserve their whole earned road.
            // Exceptional unlocks remain explicit grants; mere attempts never open content.
            for (var index = target; index < orderedModuleIds.Length; index++)
            {
                if (HasValidCompletion(GetRecord(progression, orderedModuleIds[index]))) return true;
                if (progression != null && Array.IndexOf(progression.exceptionalUnlocks, orderedModuleIds[index]) >= 0) return true;
            }
            return HasValidCompletion(GetRecord(progression, orderedModuleIds[target - 1]));
        }

        public static bool HasValidCompletion(ModuleProgressRecord record)
        {
            if (record == null) return false;
            if (IsEarnedRank(record.bestRank) || IsEarnedRank(record.highestRank)) return true;
            if (!string.IsNullOrEmpty(record.runValidity) && record.runValidity.StartsWith("Invalid", StringComparison.Ordinal)) return false;
            return record.completed || IsEarnedRank(record.bestRank) || IsEarnedRank(record.highestRank);
        }

        private static bool IsEarnedRank(string rank) => rank == "Bronze" || rank == "Silver" || rank == "Gold" || rank == "Diamond";

        public static string GetContinueModuleId(ProgressionData progression, string[] orderedModuleIds)
        {
            if (orderedModuleIds == null) return null;
            for (var i = orderedModuleIds.Length - 1; i >= 0; i--)
                if (IsUnlocked(progression, orderedModuleIds, orderedModuleIds[i])
                    && !HasValidCompletion(GetRecord(progression, orderedModuleIds[i]))) return orderedModuleIds[i];
            return null;
        }

        public static ModuleProgressRecord[] CompletedRecords(ProgressionData progression)
        {
            EnsureArrays(progression);
            var count = 0;
            foreach (var record in progression.modules)
            {
                if (record != null && record.completed)
                {
                    count++;
                }
            }

            var completed = new ModuleProgressRecord[count];
            var index = 0;
            foreach (var record in progression.modules)
            {
                if (record != null && record.completed)
                {
                    completed[index++] = record;
                }
            }

            return completed;
        }

        public static bool RecordCompletion(
            ProgressionData progression,
            string moduleId,
            double completionSeconds,
            string rank,
            long score,
            ModuleSplitRecord[] splits,
            bool validForPersonalBest,
            int moduleContentVersion,
            int movementCompatibilityVersion,
            int rankThresholdVersion,
            string rankCalibrationState,
            string runValidity,
            string utcNow)
        {
            var record = GetOrCreateRecord(progression, moduleId);
            record.completionCount++;
            record.completed = true;
            record.latestCompletionTimeSeconds = completionSeconds;
            record.latestRank = rank;
            record.lastCompletionUtc = utcNow;
            record.moduleContentVersion = moduleContentVersion;
            record.movementCompatibilityVersion = movementCompatibilityVersion;
            record.rankThresholdVersion = rankThresholdVersion;
            record.rankCalibrationState = rankCalibrationState;
            record.runValidity = runValidity;
            if (string.IsNullOrWhiteSpace(record.firstCompletionUtc))
            {
                record.firstCompletionUtc = utcNow;
            }

            if (validForPersonalBest && IsHigherRank(rank, record.highestRank))
            {
                record.highestRank = rank;
            }

            var isNewPersonalBest = validForPersonalBest
                && (record.bestTimeSeconds <= 0d || completionSeconds < record.bestTimeSeconds);
            if (isNewPersonalBest)
            {
                record.bestTimeSeconds = completionSeconds;
                record.bestRank = rank;
                record.bestScore = score;
                record.bestSplits = splits ?? Array.Empty<ModuleSplitRecord>();
            }

            return isNewPersonalBest;
        }

        // V4 additive extension: legacy aggregate metadata describes the latest run,
        // not necessarily its PB. Never guess a compatible PB from that record.
        public static ModuleProgressRecord GetVersionedBest(ProgressionData data, string id,
            int content, int movement, int thresholds)
        {
            if (data == null) return null;
            EnsureArrays(data);
            foreach (var record in data.versionedBests)
                if (record != null && record.moduleId == id && record.moduleContentVersion == content
                    && record.movementCompatibilityVersion == movement && record.rankThresholdVersion == thresholds)
                    return record;
            return null;
        }

        public static bool RecordVersionedCompletion(ProgressionData data, string id, double seconds,
            string rank, long score, ModuleSplitRecord[] splits, bool valid, int content,
            int movement, int thresholds, string calibration, string validity, string utc)
        {
            EnsureArrays(data);
            var legacy = GetRecord(data, id);
            bool archived = false;
            foreach (var item in data.historicalRecords) archived |= item != null && item.moduleId == id;
            if (!archived && legacy != null && legacy.bestTimeSeconds > 0)
            {
                var archive = data.historicalRecords;
                Array.Resize(ref archive, archive.Length + 1);
                archive[archive.Length - 1] = UnityEngine.JsonUtility.FromJson<ModuleProgressRecord>(UnityEngine.JsonUtility.ToJson(legacy));
                data.historicalRecords = archive;
            }
            // Retain aggregate completion counts, rewards and unlock meaning.
            RecordCompletion(data, id, seconds, rank, score, splits, valid, content, movement,
                thresholds, calibration, validity, utc);
            if (!valid) return false;
            var record = GetVersionedBest(data, id, content, movement, thresholds);
            if (record == null)
            {
                record = new ModuleProgressRecord { moduleId = id };
                var entries = data.versionedBests;
                Array.Resize(ref entries, entries.Length + 1);
                entries[entries.Length - 1] = record;
                data.versionedBests = entries;
            }
            var bucket = new ProgressionData { modules = new[] { record } };
            return RecordCompletion(bucket, id, seconds, rank, score, splits, true, content, movement,
                thresholds, calibration, validity, utc);
        }

        public static void RecordAttempt(ProgressionData progression, string moduleId)
        {
            GetOrCreateRecord(progression, moduleId).attemptCount++;
        }

        public static void EnsureArrays(ProgressionData progression)
        {
            if (progression == null)
            {
                return;
            }

            progression.modules ??= Array.Empty<ModuleProgressRecord>();
            progression.historicalRecords ??= Array.Empty<ModuleProgressRecord>();
            progression.versionedBests ??= Array.Empty<ModuleProgressRecord>();
            progression.exceptionalUnlocks ??= Array.Empty<string>();
            progression.claimedRewardIds ??= Array.Empty<string>();
        }

        public static bool HasClaimedReward(ProgressionData progression, string rewardId)
        {
            if (progression == null || string.IsNullOrWhiteSpace(rewardId))
            {
                return false;
            }

            EnsureArrays(progression);
            foreach (var claimed in progression.claimedRewardIds)
            {
                if (string.Equals(claimed, rewardId, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool MarkRewardClaimed(ProgressionData progression, string rewardId)
        {
            if (progression == null)
            {
                throw new ArgumentNullException(nameof(progression));
            }

            if (!StableId.IsValid(rewardId))
            {
                throw new ArgumentException("Stable reward ID is required.", nameof(rewardId));
            }

            EnsureArrays(progression);
            if (HasClaimedReward(progression, rewardId))
            {
                return false;
            }

            var next = new string[progression.claimedRewardIds.Length + 1];
            Array.Copy(progression.claimedRewardIds, next, progression.claimedRewardIds.Length);
            next[next.Length - 1] = rewardId;
            progression.claimedRewardIds = next;
            return true;
        }

        private static bool IsHigherRank(string candidate, string existing) =>
            RankValue(candidate) > RankValue(existing);

        private static int RankValue(string rank)
        {
            switch (rank)
            {
                case "Diamond": return 4;
                case "Gold": return 3;
                case "Silver": return 2;
                case "Bronze": return 1;
                default: return 0;
            }
        }
    }

    public static class EconomyProgressionData
    {
        public static long GetBalance(EconomyData economy, string currencyId)
        {
            if (economy == null || string.IsNullOrWhiteSpace(currencyId))
            {
                return 0L;
            }

            EnsureArrays(economy);
            foreach (var balance in economy.balances)
            {
                if (balance != null && string.Equals(balance.currencyId, currencyId, StringComparison.Ordinal))
                {
                    return balance.amount;
                }
            }

            return 0L;
        }

        public static long AddBalance(EconomyData economy, string currencyId, long delta)
        {
            if (economy == null)
            {
                throw new ArgumentNullException(nameof(economy));
            }

            RequireStableId(currencyId, nameof(currencyId));
            EnsureArrays(economy);
            var balance = GetOrCreateBalance(economy, currencyId);
            var amount = checked(balance.amount + delta);
            if (amount < 0L)
            {
                throw new InvalidOperationException("Currency balance cannot become negative.");
            }

            balance.amount = amount;
            return balance.amount;
        }

        public static bool TrySpend(EconomyData economy, string currencyId, long amount)
        {
            if (economy == null)
            {
                throw new ArgumentNullException(nameof(economy));
            }

            if (amount <= 0L)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Spend amount must be positive.");
            }

            RequireStableId(currencyId, nameof(currencyId));
            if (GetBalance(economy, currencyId) < amount)
            {
                return false;
            }

            AddBalance(economy, currencyId, -amount);
            return true;
        }

        public static void EnsureArrays(EconomyData economy)
        {
            if (economy == null)
            {
                return;
            }

            economy.balances ??= Array.Empty<CurrencyBalanceRecord>();
        }

        private static CurrencyBalanceRecord GetOrCreateBalance(EconomyData economy, string currencyId)
        {
            foreach (var balance in economy.balances)
            {
                if (balance != null && string.Equals(balance.currencyId, currencyId, StringComparison.Ordinal))
                {
                    return balance;
                }
            }

            var record = new CurrencyBalanceRecord { currencyId = currencyId };
            var next = new CurrencyBalanceRecord[economy.balances.Length + 1];
            Array.Copy(economy.balances, next, economy.balances.Length);
            next[next.Length - 1] = record;
            economy.balances = next;
            return record;
        }

        private static void RequireStableId(string stableId, string parameterName)
        {
            if (!StableId.IsValid(stableId))
            {
                throw new ArgumentException("Stable content ID is required.", parameterName);
            }
        }
    }

    public static class InventoryProgressionData
    {
        public static int GetQuantity(InventoryData inventory, string contentId)
        {
            if (inventory == null || string.IsNullOrWhiteSpace(contentId))
            {
                return 0;
            }

            EnsureArrays(inventory);
            foreach (var item in inventory.items)
            {
                if (item != null && string.Equals(item.contentId, contentId, StringComparison.Ordinal))
                {
                    return item.quantity;
                }
            }

            return 0;
        }

        public static InventoryItemRecord GrantItem(
            InventoryData inventory,
            string contentId,
            int quantity,
            string utcNow,
            string sourceId)
        {
            if (inventory == null)
            {
                throw new ArgumentNullException(nameof(inventory));
            }

            if (quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity), "Grant quantity must be positive.");
            }

            RequireStableId(contentId, nameof(contentId));
            if (!string.IsNullOrWhiteSpace(sourceId))
            {
                RequireStableId(sourceId, nameof(sourceId));
            }

            EnsureArrays(inventory);
            var item = GetOrCreateItem(inventory, contentId);
            item.quantity = checked(item.quantity + quantity);
            if (string.IsNullOrWhiteSpace(item.firstAcquiredUtc))
            {
                item.firstAcquiredUtc = utcNow;
            }

            item.lastAcquiredUtc = utcNow;
            item.sourceId = sourceId;
            return item;
        }

        public static bool HasUnlock(InventoryData inventory, string contentId)
        {
            if (inventory == null || string.IsNullOrWhiteSpace(contentId))
            {
                return false;
            }

            EnsureArrays(inventory);
            foreach (var unlock in inventory.unlocks)
            {
                if (unlock != null
                    && unlock.unlocked
                    && string.Equals(unlock.contentId, contentId, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        public static UnlockRecord SetUnlock(
            InventoryData inventory,
            string contentId,
            string unlockType,
            bool unlocked,
            string utcNow,
            string sourceId)
        {
            if (inventory == null)
            {
                throw new ArgumentNullException(nameof(inventory));
            }

            RequireStableId(contentId, nameof(contentId));
            if (!string.IsNullOrWhiteSpace(sourceId))
            {
                RequireStableId(sourceId, nameof(sourceId));
            }

            EnsureArrays(inventory);
            var unlock = GetOrCreateUnlock(inventory, contentId);
            unlock.unlockType = unlockType;
            unlock.unlocked = unlocked;
            unlock.unlockedUtc = unlocked ? utcNow : string.Empty;
            unlock.sourceId = sourceId;
            return unlock;
        }

        public static string GetEquippedCosmetic(InventoryData inventory, string slotId)
        {
            if (inventory == null || string.IsNullOrWhiteSpace(slotId))
            {
                return string.Empty;
            }

            EnsureArrays(inventory);
            foreach (var equipped in inventory.equippedCosmetics)
            {
                if (equipped != null && string.Equals(equipped.slotId, slotId, StringComparison.Ordinal))
                {
                    return equipped.contentId ?? string.Empty;
                }
            }

            return string.Empty;
        }

        public static bool TryEquipCosmetic(InventoryData inventory, string slotId, string contentId)
        {
            if (inventory == null)
            {
                throw new ArgumentNullException(nameof(inventory));
            }

            RequireStableId(slotId, nameof(slotId));
            if (!string.IsNullOrWhiteSpace(contentId))
            {
                RequireStableId(contentId, nameof(contentId));
            }

            if (!string.IsNullOrWhiteSpace(contentId) && !HasUnlock(inventory, contentId))
            {
                return false;
            }

            EnsureArrays(inventory);
            var equipped = GetOrCreateEquippedCosmetic(inventory, slotId);
            equipped.contentId = string.IsNullOrWhiteSpace(contentId) ? string.Empty : contentId;
            return true;
        }

        public static void EnsureArrays(InventoryData inventory)
        {
            if (inventory == null)
            {
                return;
            }

            inventory.items ??= Array.Empty<InventoryItemRecord>();
            inventory.unlocks ??= Array.Empty<UnlockRecord>();
            inventory.equippedCosmetics ??= Array.Empty<EquippedCosmeticRecord>();
        }

        private static InventoryItemRecord GetOrCreateItem(InventoryData inventory, string contentId)
        {
            foreach (var item in inventory.items)
            {
                if (item != null && string.Equals(item.contentId, contentId, StringComparison.Ordinal))
                {
                    return item;
                }
            }

            var record = new InventoryItemRecord { contentId = contentId };
            var next = new InventoryItemRecord[inventory.items.Length + 1];
            Array.Copy(inventory.items, next, inventory.items.Length);
            next[next.Length - 1] = record;
            inventory.items = next;
            return record;
        }

        private static UnlockRecord GetOrCreateUnlock(InventoryData inventory, string contentId)
        {
            foreach (var unlock in inventory.unlocks)
            {
                if (unlock != null && string.Equals(unlock.contentId, contentId, StringComparison.Ordinal))
                {
                    return unlock;
                }
            }

            var record = new UnlockRecord { contentId = contentId };
            var next = new UnlockRecord[inventory.unlocks.Length + 1];
            Array.Copy(inventory.unlocks, next, inventory.unlocks.Length);
            next[next.Length - 1] = record;
            inventory.unlocks = next;
            return record;
        }

        private static EquippedCosmeticRecord GetOrCreateEquippedCosmetic(
            InventoryData inventory,
            string slotId)
        {
            foreach (var equipped in inventory.equippedCosmetics)
            {
                if (equipped != null && string.Equals(equipped.slotId, slotId, StringComparison.Ordinal))
                {
                    return equipped;
                }
            }

            var record = new EquippedCosmeticRecord { slotId = slotId };
            var next = new EquippedCosmeticRecord[inventory.equippedCosmetics.Length + 1];
            Array.Copy(inventory.equippedCosmetics, next, inventory.equippedCosmetics.Length);
            next[next.Length - 1] = record;
            inventory.equippedCosmetics = next;
            return record;
        }

        private static void RequireStableId(string stableId, string parameterName)
        {
            if (!StableId.IsValid(stableId))
            {
                throw new ArgumentException("Stable content ID is required.", parameterName);
            }
        }
    }
}
