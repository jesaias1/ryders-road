using System;
using UnityEngine;
namespace Avoidance.Gameplay.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class MechanismAudio : MonoBehaviour
    {
        public event Action<GameplayAudioCue> CueRequested;
        private AudioSource _source;
        private Vector3 _previous;
        private bool _moving;
        private void Awake()
        {
            _source = GetComponent<AudioSource>(); _source.playOnAwake = false; _source.loop = true;
            _source.spatialBlend = 1; _source.minDistance = 2; _source.maxDistance = 18;
            var profile = Resources.Load<GameplayAudioProfile>("GameplayAudioProfile");
            if (profile != null && profile.TryGet(GameplayAudioCue.MovingMechanism, out var clip, out var volume))
            { _source.clip = clip; _source.volume = volume; }
            _previous = transform.position;
        }
        private void LateUpdate()
        {
            var moving = (transform.position - _previous).sqrMagnitude > 0.000001f;
            if (moving && !_moving) { CueRequested?.Invoke(GameplayAudioCue.MovingMechanism); if (_source.clip != null) _source.Play(); }
            if (!moving && _moving) _source.Stop();
            _moving = moving; _previous = transform.position;
        }
    }
}
