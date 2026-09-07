using Avoidance.Gameplay.Worlds;
using UnityEngine;

namespace Avoidance.Gameplay.Respawn
{
    // Solid scenery remains truthful, but explicitly non-route landings end a fall.
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RestoreController), typeof(CharacterController))]
    public sealed class SceneryLandingRecovery : MonoBehaviour
    {
        private RestoreController restore;
        private CharacterController character;

        private void Awake()
        {
            restore = GetComponent<RestoreController>();
            character = GetComponent<CharacterController>();
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            NotifyContact(hit.collider, hit.normal);
        }

        public void NotifyContact(Collider collider, Vector3 normal)
        {
            if (restore == null || collider == null || restore.IsRestorePending
                || restore.SpawnProtectionRemaining > 0f
                || normal.y < Mathf.Cos(character.slopeLimit * Mathf.Deg2Rad)) return;
            var surface = collider.GetComponent<AuthoredSurface>();
            if (surface != null && surface.RestoreOnLanding) restore.RequestRestore(true);
        }
    }
}
