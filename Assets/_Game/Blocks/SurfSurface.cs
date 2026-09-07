using Avoidance.Gameplay.Player;
using UnityEngine;

namespace Avoidance.Gameplay.Blocks
{
    [DisallowMultipleComponent]
    public sealed class SurfSurface : MonoBehaviour
    {
        [SerializeField] private SurfProfile _profile;
        private SurfProfile _runtimeProfile;

        public SurfProfile Profile
        {
            get
            {
                if (_profile != null)
                {
                    return _profile;
                }

                _runtimeProfile ??= SurfProfile.CreateRuntimeDefault();
                return _runtimeProfile;
            }
        }

        public bool IsValidSurface(Vector3 surfaceNormal)
        {
            return SurfMovementMath.IsSurfAngle(surfaceNormal, Profile);
        }

        private void OnDrawGizmosSelected()
        {
            var normal = transform.up;
            var origin = transform.position + normal * 0.8f;
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(origin, origin + normal * 1.25f);
            Gizmos.color = Color.white;
            Gizmos.DrawLine(origin, origin + SurfMovementMath.DownSlope(normal) * 1.4f);
        }
    }
}
