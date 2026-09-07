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

    public enum DiagnosticsDisplayMode
    {
        Hidden,
        Normal,
        Full
    }

    public interface IDiagnosticsService
    {
        void SetValue(string key, string value);
        bool RemoveValue(string key);
        System.Collections.Generic.IReadOnlyDictionary<string, string> Values { get; }
        DiagnosticsDisplayMode DisplayMode { get; }
        void SetDisplayMode(DiagnosticsDisplayMode mode);
        void CycleDisplayMode();
    }

    public interface IPlatformDisplayService
    {
        bool ImmersiveRequested { get; }
        bool HasFocus { get; }
        int AndroidApiLevel { get; }
        Rect SafeArea { get; }
        string DisplaySummary { get; }
        void RequestImmersiveMode();
        void SetFocusState(bool focused);
    }

    [Serializable]
    public sealed class GameSettings
    {
        [Range(0f, 1f)] public float masterVolume = 1f;
        [Range(0f, 1f)] public float lookSensitivity = 0.5f;
        public bool diagnosticsVisible = true;
    }
}
