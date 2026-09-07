using System;
using Avoidance.Gameplay.Audio;
using UnityEngine;
namespace Avoidance.UI
{
    public static class UiAudio
    {
        public static event Action<GameplayAudioCue> CueRequested;
        private static AudioSource _source;
        public static void Play()
        {
            CueRequested?.Invoke(GameplayAudioCue.Ui);
            var profile = Resources.Load<GameplayAudioProfile>("GameplayAudioProfile");
            if (profile == null || !profile.TryGet(GameplayAudioCue.Ui, out var clip, out var volume)) return;
            if (_source == null) { var root = new GameObject("UI Audio", typeof(AudioSource)); UnityEngine.Object.DontDestroyOnLoad(root); _source = root.GetComponent<AudioSource>(); _source.playOnAwake = false; }
            _source.PlayOneShot(clip, volume);
        }
    }
}
