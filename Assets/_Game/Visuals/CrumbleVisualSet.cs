using UnityEngine;

namespace Avoidance.Gameplay.Visuals
{
    [CreateAssetMenu(
        menuName = "RYDERS BLOCK/Crumble Visual Set",
        fileName = ResourceName)]
    public sealed class CrumbleVisualSet : ScriptableObject
    {
        public const string ResourceName = "CrumbleVisualSet";

        [SerializeField] private GameObject _stage1Prefab;
        [SerializeField] private GameObject _stage2Prefab;
        [SerializeField] private GameObject _stage3Prefab;

        public GameObject Stage1Prefab => _stage1Prefab;
        public GameObject Stage2Prefab => _stage2Prefab;
        public GameObject Stage3Prefab => _stage3Prefab;
        public bool IsComplete => _stage1Prefab != null && _stage2Prefab != null && _stage3Prefab != null;
    }
}
