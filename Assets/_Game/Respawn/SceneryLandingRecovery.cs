using Avoidance.Gameplay.Worlds;
using UnityEngine;

namespace Avoidance.Gameplay.Respawn
{
    // Explicit non-route collision ends the run on any face, never only the top.
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RestoreController), typeof(CharacterController))]
    public sealed class SceneryLandingRecovery : MonoBehaviour
    {
        private RestoreController restore;
        public Collider LastFatalCollider { get; private set; }

        private void Awake()
        {
            restore = GetComponent<RestoreController>();
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            NotifyContact(hit.collider, hit.normal);
        }

        public void NotifyContact(Collider collider, Vector3 normal)
        {
            if (restore == null || collider == null || restore.IsRestorePending) return;
            var surface = collider.GetComponent<AuthoredSurface>();
            if (surface != null && surface.GeometryKind == WorldGeometryKind.FatalScenery)
            { LastFatalCollider=collider; restore.RequestFatalContact(); }
        }
    }
}
