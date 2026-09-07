using System;
using System.IO;
using Avoidance.Gameplay.Player;
using Avoidance.Input;
using UnityEngine;

namespace Avoidance.Gameplay.Timing
{
    [Serializable]
    public sealed class MovementSessionReportData
    {
        public string gameTitle;
        public string buildVersion;
        public string deviceModel;
        public string operatingSystem;
        public string resolution;
        public float aspectRatio;
        public string controlProfile;
        public string movementProfile;
        public float averageFps;
        public float minimumFps;
        public int jumps;
        public int falls;
        public int restores;
        public float testDurationSeconds;
        public bool finishReached;
        public string testerNotes;
    }

    [DisallowMultipleComponent]
    public sealed class MovementSessionReporter : MonoBehaviour
    {
        private MovementSessionStats _stats;
        private PlayerInputRouter _input;
        private ParkourMotor _motor;
        private string _buildVersion;
        private float _duration;
        private float _fpsTotal;
        private int _fpsSamples;
        private float _minimumFps = float.MaxValue;
        private bool _exported;

        public string LastExportPath { get; private set; }
        public string TesterNotes { get; set; } = string.Empty;
        public float AverageFps => _fpsSamples == 0 ? 0f : _fpsTotal / _fpsSamples;
        public float MinimumFps => _fpsSamples == 0 ? 0f : _minimumFps;

        public void Initialize(
            MovementSessionStats stats,
            PlayerInputRouter input,
            ParkourMotor motor,
            string buildVersion)
        {
            _stats = stats;
            _input = input;
            _motor = motor;
            _buildVersion = buildVersion;
        }

        public void Tick(float unscaledDeltaTime)
        {
            if (unscaledDeltaTime <= 0f)
            {
                return;
            }

            _duration += unscaledDeltaTime;
            var fps = 1f / unscaledDeltaTime;
            _fpsTotal += fps;
            _fpsSamples++;
            _minimumFps = Mathf.Min(_minimumFps, fps);
        }

        public string Export()
        {
            if (_stats == null || _motor == null)
            {
                return null;
            }

            var report = new MovementSessionReportData
            {
                gameTitle = "RYDER'S ROAD",
                buildVersion = _buildVersion,
                deviceModel = SystemInfo.deviceModel,
                operatingSystem = SystemInfo.operatingSystem,
                resolution = $"{Screen.width}x{Screen.height}",
                aspectRatio = Screen.height <= 0 ? 0f : Screen.width / (float)Screen.height,
                controlProfile = _input.ActiveMode.ToString(),
                movementProfile = _motor.Profile.DisplayName,
                averageFps = AverageFps,
                minimumFps = MinimumFps,
                jumps = _stats.Jumps,
                falls = _stats.Falls,
                restores = _stats.Restores,
                testDurationSeconds = _duration,
                finishReached = _stats.FinishReached,
                testerNotes = TesterNotes
            };
            try
            {
                var directory = Path.Combine(Application.persistentDataPath, "SessionReports");
                Directory.CreateDirectory(directory);
                var fileName = $"movement-{DateTime.UtcNow:yyyyMMdd-HHmmss-fff}.json";
                LastExportPath = Path.Combine(directory, fileName);
                File.WriteAllText(LastExportPath, JsonUtility.ToJson(report, true));
                _exported = true;
                return LastExportPath;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
                LastExportPath = null;
                return null;
            }
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused && (Debug.isDebugBuild || Application.isEditor))
            {
                Export();
            }
        }

        private void OnDestroy()
        {
            if (!_exported
                && _duration > 1f
                && (Debug.isDebugBuild || Application.isEditor))
            {
                Export();
            }
        }
    }
}
