using System;
using UnityEngine;

namespace Avoidance.Gameplay.Audio
{
    public enum GameplayAudioCue
    {
        Jump, Landing, HardLanding, RestorePoint, Fall, Restore, Patch, Boost,
        Water, CrumbleWarning, Split, Rank, PersonalBest, Diamond, Retry,
        MovingMechanism, CrumbleCollapse, Wind, HighSpeed, Ui, BiomeAmbience, Pickup, Footstep
    }

    [CreateAssetMenu(menuName = "RYDERS BLOCK/Gameplay Audio Profile")]
    public sealed class GameplayAudioProfile : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public GameplayAudioCue Cue;
            public AudioClip Clip;
            [Range(0f, 1f)] public float Volume;
        }

        [SerializeField, Min(.25f)] private float _footstepDistance = 2.1f;
        public float FootstepDistance => _footstepDistance;
        [SerializeField, Min(0f)] private float _hardLandingSpeed = 14f;
        [SerializeField] private Entry[] _entries = Array.Empty<Entry>();
        public float HardLandingSpeed => _hardLandingSpeed;

        public bool TryGet(GameplayAudioCue cue, out AudioClip clip, out float volume)
        {
            foreach (var entry in _entries)
            {
                if (entry.Cue != cue || entry.Clip == null) continue;
                clip = entry.Clip;
                volume = Mathf.Clamp01(entry.Volume);
                return true;
            }
            clip = null;
            volume = 0f;
            return false;
        }
    }
}
