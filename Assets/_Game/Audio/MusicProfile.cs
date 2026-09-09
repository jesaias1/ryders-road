using System;
using UnityEngine;

namespace Avoidance.Gameplay.Audio
{
    [CreateAssetMenu(menuName = "RYDERS BLOCK/Music Profile")]
    public sealed class MusicProfile : ScriptableObject
    {
        [Serializable] public struct Entry { public string Context; public AudioClip Clip; }
        [SerializeField] private Entry[] _entries = Array.Empty<Entry>();
        [SerializeField, Range(0, 1)] private float _gain = .85f;
        [SerializeField, Min(.1f)] private float _fadeSeconds = 2f;
        public float Gain => _gain;
        public float FadeSeconds => _fadeSeconds;
        public AudioClip Resolve(string context)
        {
            foreach (var entry in _entries) if (entry.Context == context) return entry.Clip;
            foreach (var entry in _entries) if (entry.Context == "default") return entry.Clip;
            return null;
        }
    }
}
