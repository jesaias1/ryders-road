using System;
using System.Collections.Generic;
using System.IO;
using Avoidance.Gameplay.Levels;
using Avoidance.Gameplay.Player;
using Avoidance.Input;
using UnityEngine;

namespace Avoidance.Gameplay.Timing
{
    [Serializable]
    public sealed class ModuleAttemptReport
    {
        public string gameTitle;
        public string buildVersion;
        public string moduleId;
        public int moduleContentVersion;
        public string moduleDisplayName;
        public string movementProfile;
        public string controlProfile;
        public int attempts;
        public bool completed;
        public float elapsedSeconds;
        public float completionSeconds;
        public int falls;
        public int restores;
        public int jumps;
        public string[] restoreSplits;
        public string[] shortcutsUsed;
        public string[] fallLocations;
        public string achievedRank;
        public long score;
        public bool newPersonalBest;
        public string runValidity;
        public int boostsUsed;
        public int waterPulses;
        public int surfTicks;
        public bool surfUsed;
        public int developmentTeleports;
        public float peakHorizontalSpeed;
        public int rankThresholdVersion;
        public string rankCalibrationState;
        public int movementCompatibilityVersion;
    }

    public sealed class ModuleAttemptTelemetry
    {
        private readonly List<string> _restoreSplits = new List<string>();
        private readonly List<string> _shortcutsUsed = new List<string>();
        private readonly List<string> _fallLocations = new List<string>();
        private ModuleDefinition _module;
        private PlayerInputRouter _input;
        private ParkourMotor _motor;
        private string _buildVersion;
        private float _elapsed;
        private bool _running;

        public int Attempts { get; private set; }
        public int Falls { get; private set; }
        public int Restores { get; private set; }
        public int Jumps { get; private set; }
        public int BoostsUsed { get; private set; }
        public int WaterPulses { get; private set; }
        public int SurfTicks { get; private set; }
        public int DevelopmentTeleports { get; private set; }
        public float PeakHorizontalSpeed { get; private set; }
        public bool Completed { get; private set; }
        public float ElapsedSeconds => _elapsed;
        public float CompletionSeconds { get; private set; }
        public string LastExportPath { get; private set; }
        public ModuleRunResult LastRunResult { get; private set; }

        public void Initialize(
            ModuleDefinition module,
            PlayerInputRouter input,
            ParkourMotor motor,
            string buildVersion)
        {
            _module = module;
            _input = input;
            _motor = motor;
            _buildVersion = buildVersion;
            StartAttempt();
        }

        public void StartAttempt()
        {
            Attempts++;
            _elapsed = 0f;
            Completed = false;
            CompletionSeconds = 0f;
            Falls = 0;
            Restores = 0;
            Jumps = 0;
            BoostsUsed = 0;
            WaterPulses = 0;
            SurfTicks = 0;
            DevelopmentTeleports = 0;
            PeakHorizontalSpeed = 0f;
            _restoreSplits.Clear();
            _shortcutsUsed.Clear();
            _fallLocations.Clear();
            _running = true;
            LastRunResult = null;
        }

        public void Tick(float unscaledDeltaTime)
        {
            if (_running && unscaledDeltaTime > 0f)
            {
                _elapsed += unscaledDeltaTime;
            }

            if (_motor != null)
            {
                PeakHorizontalSpeed = Mathf.Max(PeakHorizontalSpeed, _motor.PeakHorizontalSpeed);
                if (_motor.IsSurfing)
                {
                    SurfTicks++;
                }
            }
        }

        public void RecordJump() => Jumps++;

        public void RecordFall(Vector3 position)
        {
            Falls++;
            _fallLocations.Add(FormatVector(position));
        }

        public void RecordRestore() => Restores++;

        public void RecordBoost() => BoostsUsed++;

        public void RecordWaterPulse() => WaterPulses++;

        public void RecordDevelopmentTeleport() => DevelopmentTeleports++;

        public void RecordRestoreSplit(string restorePointId)
        {
            if (!string.IsNullOrWhiteSpace(restorePointId))
            {
                _restoreSplits.Add($"{restorePointId}@{_elapsed:0.00}");
            }
        }

        public void RecordShortcut(string shortcutId)
        {
            if (!string.IsNullOrWhiteSpace(shortcutId)
                && !_shortcutsUsed.Contains(shortcutId))
            {
                _shortcutsUsed.Add(shortcutId);
            }
        }

        public void RecordFinish()
        {
            if (Completed)
            {
                return;
            }

            Completed = true;
            CompletionSeconds = LastRunResult == null
                ? _elapsed
                : (float)LastRunResult.CompletionSeconds;
            _elapsed = Mathf.Max(_elapsed, CompletionSeconds);
            _running = false;
            Export();
        }

        public void ApplyRunResult(ModuleRunResult result)
        {
            LastRunResult = result;
        }

        public ModuleAttemptReport CreateReport()
        {
            return new ModuleAttemptReport
            {
                gameTitle = "RYDER'S ROAD",
                buildVersion = _buildVersion,
                moduleId = _module == null ? string.Empty : _module.StableModuleId,
                moduleContentVersion = _module == null ? 0 : _module.ContentVersion,
                moduleDisplayName = _module == null ? string.Empty : _module.DisplayName,
                movementProfile = _motor == null || _motor.Profile == null
                    ? string.Empty
                    : _motor.Profile.DisplayName,
                controlProfile = _input == null ? string.Empty : _input.ActiveMode.ToString(),
                attempts = Attempts,
                completed = Completed,
                elapsedSeconds = _elapsed,
                completionSeconds = CompletionSeconds,
                falls = Falls,
                restores = Restores,
                jumps = Jumps,
                restoreSplits = _restoreSplits.ToArray(),
                shortcutsUsed = _shortcutsUsed.ToArray(),
                fallLocations = _fallLocations.ToArray(),
                achievedRank = LastRunResult == null ? string.Empty : LastRunResult.Rank.ToString(),
                score = LastRunResult == null ? 0L : LastRunResult.Score,
                newPersonalBest = LastRunResult != null && LastRunResult.IsNewPersonalBest,
                runValidity = LastRunResult == null ? string.Empty : LastRunResult.Validity.ToString(),
                boostsUsed = BoostsUsed,
                waterPulses = WaterPulses,
                surfTicks = SurfTicks,
                surfUsed = SurfTicks > 0,
                developmentTeleports = DevelopmentTeleports,
                peakHorizontalSpeed = PeakHorizontalSpeed,
                rankThresholdVersion = LastRunResult == null ? 0 : LastRunResult.RankThresholdVersion,
                rankCalibrationState = LastRunResult == null ? string.Empty : LastRunResult.RankCalibrationState,
                movementCompatibilityVersion = LastRunResult == null ? 0 : LastRunResult.MovementCompatibilityVersion
            };
        }

        public string Export()
        {
            try
            {
                var directory = Path.Combine(Application.persistentDataPath, "ModuleReports");
                Directory.CreateDirectory(directory);
                var moduleId = _module == null ? "module" : _module.StableModuleId.Replace('.', '-');
                var fileName = $"{moduleId}-{DateTime.UtcNow:yyyyMMdd-HHmmss-fff}.json";
                LastExportPath = Path.Combine(directory, fileName);
                File.WriteAllText(LastExportPath, JsonUtility.ToJson(CreateReport(), true));
                return LastExportPath;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                LastExportPath = null;
                return null;
            }
        }

        public static void ClearLocalReports()
        {
            var directory = Path.Combine(Application.persistentDataPath, "ModuleReports");
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }

        private static string FormatVector(Vector3 value)
        {
            return $"{value.x:0.00},{value.y:0.00},{value.z:0.00}";
        }
    }
}
