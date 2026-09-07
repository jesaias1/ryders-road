using UnityEngine;

namespace Avoidance.Gameplay.Levels
{
    [DisallowMultipleComponent]
    public sealed class CameraGuideHint : MonoBehaviour
    {
        [SerializeField] private float _preferredYaw;
        [SerializeField] private float _preferredPitch;
        [SerializeField] private float _maxAssistDegrees = 12f;

        public float PreferredYaw => _preferredYaw;
        public float PreferredPitch => _preferredPitch;
        public float MaxAssistDegrees => _maxAssistDegrees;

        public void Configure(float preferredYaw, float preferredPitch, float maxAssistDegrees)
        {
            _preferredYaw = preferredYaw;
            _preferredPitch = preferredPitch;
            _maxAssistDegrees = Mathf.Clamp(maxAssistDegrees, 0f, 45f);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.3f, 0.85f, 1f, 0.28f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            Gizmos.DrawLine(Vector3.zero, Quaternion.Euler(0f, _preferredYaw, 0f) * Vector3.forward * 1.5f);
            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}
