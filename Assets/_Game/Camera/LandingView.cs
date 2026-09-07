using System;
using UnityEngine;

namespace Avoidance.Gameplay.Camera
{
    [Serializable]
    public sealed class LandingViewSettings
    {
        public float initialPitch = 16f;
        public float descentPitch = 8f;
        public float maximumFramedPitch = 24f;
        public float fullDescentSpeed = 7f;
        public float degreesPerSecond = 36f;
        public float manualHoldSeconds = 1.2f;
    }

    // Presentation only. No geometry queries, route targets or motor reference.
    public sealed class LandingView
    {
        private readonly LandingViewSettings settings;
        private float manualHold;
        public float Offset { get; private set; }

        public LandingView(LandingViewSettings settings) { this.settings = settings; }
        public void Reset() { Offset = manualHold = 0f; }

        // The rig adopts this offset into manual pitch before calling this method.
        public void YieldToManual()
        {
            Offset = 0f;
            manualHold = Mathf.Max(0f, settings.manualHoldSeconds);
        }

        public float Tick(bool enabled, bool grounded, bool surfing, float verticalSpeed,
            float manualPitch, float deltaTime)
        {
            if (!enabled) { Reset(); return 0f; }
            float dt = Mathf.Max(0f, deltaTime);
            manualHold = Mathf.Max(0f, manualHold - dt);
            float target = 0f;
            if (!grounded && !surfing && manualHold <= 0f)
            {
                float descent = Mathf.Clamp01(-verticalSpeed / Mathf.Max(.01f, settings.fullDescentSpeed));
                target = Mathf.Min(Mathf.Max(0f, settings.descentPitch) * descent,
                    Mathf.Max(0f, settings.maximumFramedPitch - manualPitch));
            }
            Offset = Mathf.MoveTowards(Offset, target, Mathf.Max(0f, settings.degreesPerSecond) * dt);
            return Offset;
        }
    }
}
