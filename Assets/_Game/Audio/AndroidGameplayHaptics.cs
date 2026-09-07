using UnityEngine;

namespace Avoidance.Gameplay.Audio
{
    [RequireComponent(typeof(MovementFeedback))]
    public sealed class AndroidGameplayHaptics : MonoBehaviour
    {
        public const string PreferenceKey = "settings.haptics-enabled";
        private MovementFeedback _feedback;
        private GameFeelProfile _profile;
        private float _nextPulse;
#if UNITY_ANDROID && !UNITY_EDITOR
        private AndroidJavaObject _vibrator;
#endif
        private void Awake()
        {
            _profile = Resources.Load<GameFeelProfile>("GameFeelProfile");
            _feedback = GetComponent<MovementFeedback>();
            _feedback.CueRequested += OnCue;
        }
        public static int PulseDuration(GameplayAudioCue cue, GameFeelProfile profile)
        {
            if (profile == null) return 0;
            switch (cue)
            {
                case GameplayAudioCue.Pickup: case GameplayAudioCue.Restore: case GameplayAudioCue.CrumbleWarning:
                    return profile.LightPulseMilliseconds;
                case GameplayAudioCue.Boost: case GameplayAudioCue.HardLanding:
                    return profile.MediumPulseMilliseconds;
                case GameplayAudioCue.Patch: case GameplayAudioCue.Rank: case GameplayAudioCue.PersonalBest: case GameplayAudioCue.Diamond:
                    return profile.CelebrationMilliseconds;
                default: return 0;
            }
        }
        private void OnCue(GameplayAudioCue cue)
        {
            var duration = PulseDuration(cue, _profile);
            if (duration == 0 || Time.unscaledTime < _nextPulse || PlayerPrefs.GetInt(PreferenceKey, 1) == 0) return;
            _nextPulse = Time.unscaledTime + _profile.HapticCooldown;
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                if (_vibrator == null)
                {
                    using var unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                    using var activity = unity.GetStatic<AndroidJavaObject>("currentActivity");
                    _vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                }
                if (!_vibrator.Call<bool>("hasVibrator")) return;
                using var effectClass = new AndroidJavaClass("android.os.VibrationEffect");
                using var effect = effectClass.CallStatic<AndroidJavaObject>("createOneShot", (long)duration, _profile.HapticAmplitude);
                _vibrator.Call("vibrate", effect);
            }
            catch (System.Exception) { /* Devices without this service remain playable. */ }
#endif
        }
        private void OnDestroy()
        {
            if (_feedback != null) _feedback.CueRequested -= OnCue;
#if UNITY_ANDROID && !UNITY_EDITOR
            _vibrator?.Dispose();
#endif
        }
    }
}
