using UnityEngine;
namespace Avoidance.Gameplay.Levels
{
    [CreateAssetMenu(menuName = "RYDERS BLOCK/Loading Transition Profile")]
    public sealed class LoadingTransitionProfile : ScriptableObject
    {
        [Range(1, 12)] public float ConstructionBudgetMilliseconds = 4;
        [Min(1)] public float SceneTimeoutSeconds = 20;
        [Min(0.1f)] public float MediaTimeoutSeconds = 2;
        [Min(.1f)] public float PulsePeriodSeconds = 2.4f;
        [Min(0.01f)] public float RevealSeconds = 0.18f;
    }
}
