using System;
using System.Collections;
using UnityEngine;

namespace Avoidance.Core.Services
{
    public interface ILevelLoader
    {
        string ActiveSceneName { get; }
        IEnumerator LoadAsync(string sceneName);
    }

    public interface ICheckpointService
    {
        string CurrentCheckpointId { get; }
        void Reset();
    }

    public interface IRunTimerService
    {
        bool IsRunning { get; }
        double ElapsedSeconds { get; }
        void Reset();
    }

    public interface IAudioService
    {
        void SetMasterVolume(float normalizedVolume);
    }

    public interface ISettingsService
    {
        GameSettings Current { get; }
        void Load();
        void Save();
    }

    public interface IDiagnosticsService
    {
        void SetValue(string key, string value);
        bool RemoveValue(string key);
        System.Collections.Generic.IReadOnlyDictionary<string, string> Values { get; }
    }

    [Serializable]
    public sealed class GameSettings
    {
        [Range(0f, 1f)] public float masterVolume = 1f;
        [Range(0f, 1f)] public float lookSensitivity = 0.5f;
        public bool diagnosticsVisible = true;
    }
}
