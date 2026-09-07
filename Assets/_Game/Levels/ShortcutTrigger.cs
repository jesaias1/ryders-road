using Avoidance.Gameplay.Player;
using Avoidance.Gameplay.Timing;
using UnityEngine;

namespace Avoidance.Gameplay.Levels
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class ShortcutTrigger : MonoBehaviour
    {
        private string _shortcutId;
        private ModuleAttemptTelemetry _telemetry;
        private bool _used;

        public void Initialize(string shortcutId, ModuleAttemptTelemetry telemetry)
        {
            _shortcutId = shortcutId;
            _telemetry = telemetry;
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_used || other.GetComponent<ParkourMotor>() == null)
            {
                return;
            }

            _used = true;
            _telemetry?.RecordShortcut(_shortcutId);
        }
    }
}
