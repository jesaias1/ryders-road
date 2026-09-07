using System;
using UnityEngine;
using Avoidance.Gameplay.Camera;

namespace Avoidance.Gameplay.Levels
{
    [Serializable] public sealed class FlowLabDefinition
    {
        public float fallDepth;
        public float minimumAirGain;
        public float minimumSurfSeconds;
        public int requiredChain;
        public string initialRoomId;
        public LandingViewSettings landingView;
        public FlowLabRoom[] rooms;
    }
    [Serializable] public sealed class FlowLabRoom
    {
        public string id, title, hint, exercise;
        public Vector3 start, finish, finishSize;
        public float initialPitch = 9;
        public FlowLabSolid[] solids;
    }
    [Serializable] public sealed class FlowLabSolid
    {
        public string id;
        public Vector3 position, size;
        public float slope;
        public bool surf;
    }
}
