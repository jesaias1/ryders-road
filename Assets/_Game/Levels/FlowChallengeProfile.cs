using System;
using UnityEngine;

namespace Avoidance.Gameplay.Levels
{
    [CreateAssetMenu(menuName = "RYDERS BLOCK/Flow Challenge")]
    public sealed class FlowChallengeProfile : ScriptableObject
    {
        [Serializable] public struct Shard { public string StableId; public Vector3 Position; }
        public string ModuleId;
        public Shard[] Shards = Array.Empty<Shard>();
    }
}
