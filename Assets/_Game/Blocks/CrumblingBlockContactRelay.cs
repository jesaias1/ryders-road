using Avoidance.Gameplay.Player;
using UnityEngine;

namespace Avoidance.Gameplay.Blocks
{
    [DisallowMultipleComponent]
    public sealed class CrumblingBlockContactRelay : MonoBehaviour
    {
        private ParkourMotor _motor;

        private void Awake()
        {
            _motor = GetComponent<ParkourMotor>();
        }

        public void Initialize(ParkourMotor motor)
        {
            _motor = motor;
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            NotifyLanding(hit.collider, hit.normal);
        }

        public void NotifyLanding(Collider supportCollider, Vector3 contactNormal)
        {
            if (_motor == null || supportCollider == null || contactNormal.y < 0.35f)
            {
                return;
            }

            supportCollider.GetComponentInParent<CrumblingBlock>()?.Activate(_motor);
        }
    }
}
