using UnityEngine;

namespace Avoidance.Gameplay.Audio
{
    [CreateAssetMenu(menuName = "RYDERS BLOCK/Game Feel Profile")]
    public sealed class GameFeelProfile : ScriptableObject
    {
        [Min(0)] public float SpeedThreshold = 9.5f;
        [Min(0.05f)] public float StreakInterval = 0.2f;
        [Min(0)] public float BoostStreakSeconds = 1.2f;
        [Min(0.1f)] public float HapticCooldown = 0.18f;
        [Range(1, 255)] public int HapticAmplitude = 70;
        [Range(1, 100)] public int LightPulseMilliseconds = 12;
        [Range(1, 100)] public int MediumPulseMilliseconds = 22;
        [Range(1, 150)] public int CelebrationMilliseconds = 38;
        [Range(0, 1)] public float WindGain = 0.35f;
        [Min(0.1f)] public float ShardRadius = 0.7f;
        [Min(0.1f)] public float ShardVisualSize = 0.3f;
        [Min(0)] public float ShardRotationSpeed = 70f;
    }
}
