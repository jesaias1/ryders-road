using System;
using System.Collections.Generic;
using Avoidance.Gameplay.Audio;
using Avoidance.Gameplay.Player;
using Avoidance.Gameplay.Visuals;
using UnityEngine;

namespace Avoidance.Gameplay.Levels
{
    // Attempt-scoped challenge. Restore keeps discovery; Retry starts a fresh challenge.
    // It never writes currency, progression, rank, physics, or saves.
    public sealed class FlowChallenge : MonoBehaviour
    {
        private readonly HashSet<string> _collected = new HashSet<string>();
        private MovementFeedback _feedback;
        private GameplayVfxService _vfx;
        private Material _material;
        public event Action<int, int> Changed;
        public int Count => _collected.Count;
        public int Total { get; private set; }
        public void Initialize(FlowChallengeProfile profile, MovementFeedback feedback, GameplayVfxService vfx)
        {
            _feedback = feedback; _vfx = vfx;
            var feel = Resources.Load<GameFeelProfile>("GameFeelProfile");
            if (profile == null || feel == null) return;
            _material = VisualMaterialUtility.CreateRuntimeEmissiveMaterial("Flow Shard Cyan", new Color(0.08f, 0.85f, 1f), 1.8f);
            var ids = new HashSet<string>();
            foreach (var entry in profile.Shards)
            {
                if (string.IsNullOrWhiteSpace(entry.StableId) || !ids.Add(entry.StableId)) continue;
                var root = new GameObject(entry.StableId, typeof(SphereCollider), typeof(FlowPickup));
                root.transform.SetParent(transform, false);
                root.transform.position = entry.Position;
                var trigger = root.GetComponent<SphereCollider>(); trigger.isTrigger = true; trigger.radius = feel.ShardRadius;
                var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
                visual.name = "Flow Crystal"; visual.transform.SetParent(root.transform, false);
                visual.transform.localScale = Vector3.one * feel.ShardVisualSize;
                visual.transform.localRotation = Quaternion.Euler(45, 0, 45);
                Destroy(visual.GetComponent<Collider>());
                visual.GetComponent<Renderer>().sharedMaterial = _material;
                root.GetComponent<FlowPickup>().Initialize(this, entry.StableId, visual.transform, feel.ShardRotationSpeed);
                Total++;
            }
        }
        public bool TryCollect(string id, Vector3 position)
        {
            if (!_collected.Add(id)) return false;
            _feedback?.PlayCue(GameplayAudioCue.Pickup);
            _vfx?.PlayPickup(position);
            Changed?.Invoke(Count, Total);
            return true;
        }
        private void OnDestroy() { if (_material != null) Destroy(_material); }
    }
}
