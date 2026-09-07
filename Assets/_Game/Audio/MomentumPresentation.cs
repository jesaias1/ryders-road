using Avoidance.Gameplay.Player;
using Avoidance.Gameplay.Visuals;
using UnityEngine;

namespace Avoidance.Gameplay.Audio
{
    public sealed class MomentumPresentation : MonoBehaviour
    {
        private ParkourMotor _motor;
        private MovementFeedback _feedback;
        private GameplayVfxService _vfx;
        private Transform _view;
        private GameFeelProfile _profile;
        private float _boostUntil, _nextStreak;
        private bool _flowing;
        public void Initialize(ParkourMotor motor, MovementFeedback feedback, GameplayVfxService vfx, Transform view)
        {
            _motor = motor; _feedback = feedback; _vfx = vfx; _view = view;
            _profile = Resources.Load<GameFeelProfile>("GameFeelProfile");
            _feedback.CueRequested += OnCue;
        }
        private void OnCue(GameplayAudioCue cue)
        {
            if (_profile != null && cue == GameplayAudioCue.Boost) _boostUntil = Time.time + _profile.BoostStreakSeconds;
            if (cue == GameplayAudioCue.Restore || cue == GameplayAudioCue.Patch) _boostUntil = 0;
        }
        private void Update()
        {
            if (_profile == null || _motor == null || Time.timeScale == 0) return;
            var active = _motor.HorizontalSpeed > _profile.SpeedThreshold || Time.time < _boostUntil;
            _feedback.SetWind(active ? _profile.WindGain : 0f);
            if (active && !_flowing) _feedback.PlayCue(GameplayAudioCue.HighSpeed);
            _flowing = active;
            if (!active || Time.time < _nextStreak) return;
            _nextStreak = Time.time + _profile.StreakInterval;
            _vfx.PlayFlowStreak(_view.position + _view.forward * 1.6f, _view.right, -_view.forward);
        }
        private void OnDisable() { if (_feedback != null) _feedback.StopLoops(); }
        private void OnDestroy() { if (_feedback != null) _feedback.CueRequested -= OnCue; }
    }
}
