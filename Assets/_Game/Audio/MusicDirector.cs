using Avoidance.Core.Services;
using UnityEngine;

namespace Avoidance.Gameplay.Audio
{
    // Persistent music-only presentation. Never owns loading, gameplay or save records.
    public sealed class MusicDirector : MonoBehaviour
    {
        private static MusicDirector instance;
        private MusicProfile profile;
        private readonly AudioSource[] channels = new AudioSource[2];
        private readonly float[] levels = new float[2];
        private readonly float[] starts = new float[2];
        private int active;
        private float fade;
        private bool paused;
        public AudioClip CurrentClip => channels[active] != null ? channels[active].clip : null;
        public static MusicDirector Current => instance;

        public static void SetContext(string context)
        {
            if (instance == null)
            {
                var settings = Resources.Load<MusicProfile>("MusicProfile");
                if (settings == null) return;
                var root = new GameObject("Soundtrack", typeof(MusicDirector));
                instance = root.GetComponent<MusicDirector>();
                DontDestroyOnLoad(root);
                instance.profile = settings;
                for (int i = 0; i < 2; i++)
                {
                    var source = root.AddComponent<AudioSource>();
                    source.playOnAwake = false; source.loop = true; source.spatialBlend = 0;
                    source.volume = 0; instance.channels[i] = source;
                }
            }
            instance.Select(instance.profile.Resolve(context));
        }

        private void Select(AudioClip clip)
        {
            if (clip == CurrentClip) return; // Retry/Restore and shared-track worlds do not restart music.
            int existing = System.Array.FindIndex(channels, source => source.clip == clip && clip != null);
            active = existing >= 0 ? existing : levels[0] <= levels[1] ? 0 : 1;
            if(existing < 0)
            {
                channels[active].Stop(); channels[active].clip = clip;
                levels[active] = 0;
            }
            for (int i = 0; i < 2; i++) starts[i] = levels[i];
            fade = 0;
            if (clip != null && !channels[active].isPlaying) { channels[active].Play(); if(paused) channels[active].Pause(); }
        }

        private void Update()
        {
            if (profile == null || paused) return;
            fade = Mathf.Min(1, fade + Time.unscaledDeltaTime/profile.FadeSeconds);
            ISettingsService settings = null;
            GameServices.Current?.TryGet(out settings);
            float volume = settings != null ? settings.Current.musicVolume
                : PlayerPrefs.GetFloat(PlayerPrefsSettingsService.MusicVolumeKey, .65f);
            for (int i = 0; i < 2; i++)
            {
                levels[i] = Mathf.Lerp(starts[i], i == active ? 1 : 0, fade);
                channels[i].volume = levels[i] * profile.Gain * Mathf.Clamp01(volume);
                if (i != active && fade >= 1 && channels[i].isPlaying) channels[i].Stop();
            }
        }

        private void OnApplicationPause(bool value)
        {
            paused = value;
            foreach(var channel in channels)
            {
                if(channel == null) continue;
                if(value) channel.Pause(); else channel.UnPause();
            }
        }
        private void OnDestroy() { if(instance == this) instance = null; }
    }
}
