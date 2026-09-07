using System;
using UnityEngine;

namespace Avoidance.Gameplay.Audio
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AudioSource))]
    public sealed class MovementFeedback : MonoBehaviour
    {
        [SerializeField] private GameplayAudioProfile _profile;
        private AudioSource _source;
        private AudioSource _loopSource;
        private bool _loopActive;

        // Presentation subscribers (including a future haptic adapter) observe intent;
        // feedback never changes movement or requires an audio asset to function.
        public event Action<GameplayAudioCue> CueRequested;

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.spatialBlend = 0f;
            if (_profile == null)
                _profile = Resources.Load<GameplayAudioProfile>("GameplayAudioProfile");
        }

        public void Configure(GameplayAudioProfile profile) => _profile = profile;
        public void PlayJump() => PlayCue(GameplayAudioCue.Jump);
        public void PlayLanding(float speed) => PlayCue(speed > (_profile != null ? _profile.HardLandingSpeed : 14f)
            ? GameplayAudioCue.HardLanding : GameplayAudioCue.Landing);
        public void PlayRestorePoint() => PlayCue(GameplayAudioCue.RestorePoint);
        public void PlayFall() => PlayCue(GameplayAudioCue.Fall);
        public void PlayRestore() => PlayCue(GameplayAudioCue.Restore);
        public void PlayFinish() => PlayCue(GameplayAudioCue.Patch);
        public void PlayBoost() => PlayCue(GameplayAudioCue.Boost);
        public void PlayWater() => PlayCue(GameplayAudioCue.Water);
        public void PlayCrumblingWarning() => PlayCue(GameplayAudioCue.CrumbleWarning);
        public void PlaySplit() => PlayCue(GameplayAudioCue.Split);
        public void PlayRank() => PlayCue(GameplayAudioCue.Rank);
        public void PlayPersonalBest() => PlayCue(GameplayAudioCue.PersonalBest);
        public void PlayDiamond() => PlayCue(GameplayAudioCue.Diamond);
        public void PlayRetry() => PlayCue(GameplayAudioCue.Retry);

        public void StopLoops() { if (_loopSource != null) { _loopSource.Stop(); _loopSource.volume = 0; } _loopActive = false; }
        public void SetWind(float gain) => SetLoop(GameplayAudioCue.Wind, gain);
        public void SetLoop(GameplayAudioCue cue, float gain, bool spatial = false)
        {
            var active = gain > 0.001f;
            if (active && !_loopActive) CueRequested?.Invoke(cue);
            _loopActive = active;
            if (_profile == null || !_profile.TryGet(cue, out var clip, out var volume)) return;
            if (_loopSource == null)
            {
                _loopSource = gameObject.AddComponent<AudioSource>();
                _loopSource.playOnAwake = false; _loopSource.loop = true; _loopSource.volume = 0;
                _loopSource.spatialBlend = spatial ? 1f : 0f;
                _loopSource.minDistance = 2f; _loopSource.maxDistance = 18f;
                _loopSource.clip = clip;
            }
            _loopSource.volume = Mathf.MoveTowards(_loopSource.volume, volume * Mathf.Clamp01(gain), Time.unscaledDeltaTime * 2f);
            if (active && !_loopSource.isPlaying) _loopSource.Play();
            if (!active && _loopSource.volume <= 0.001f) _loopSource.Stop();
        }
        private void OnDisable() { if (_loopSource != null) _loopSource.Stop(); _loopActive = false; }

        public void PlayCue(GameplayAudioCue cue)
        {
            CueRequested?.Invoke(cue);
            if (_source != null && _profile != null && _profile.TryGet(cue, out var clip, out var volume))
                _source.PlayOneShot(clip, volume);
        }
    }
}
