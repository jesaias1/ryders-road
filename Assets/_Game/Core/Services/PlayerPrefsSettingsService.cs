using UnityEngine;

namespace Avoidance.Core.Services
{
    public sealed class PlayerPrefsSettingsService : ISettingsService
    {
        private const string MasterVolumeKey = "settings.master-volume";
        private const string LookSensitivityKey = "settings.look-sensitivity";
        private const string DiagnosticsVisibleKey = "settings.diagnostics-visible";

        public GameSettings Current { get; } = new GameSettings();

        public void Load()
        {
            Current.masterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
            Current.lookSensitivity = PlayerPrefs.GetFloat(LookSensitivityKey, 0.5f);
            Current.diagnosticsVisible = PlayerPrefs.GetInt(DiagnosticsVisibleKey, 1) != 0;
        }

        public void Save()
        {
            PlayerPrefs.SetFloat(MasterVolumeKey, Mathf.Clamp01(Current.masterVolume));
            PlayerPrefs.SetFloat(LookSensitivityKey, Mathf.Clamp01(Current.lookSensitivity));
            PlayerPrefs.SetInt(DiagnosticsVisibleKey, Current.diagnosticsVisible ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
