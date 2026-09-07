using Avoidance.Gameplay.Player;
using UnityEngine;

namespace Avoidance.Gameplay.Levels
{
    public sealed class FlowPickup : MonoBehaviour
    {
        private FlowChallenge _owner;
        private string _id;
        private Transform _visual;
        private float _rotationSpeed;
        public void Initialize(FlowChallenge owner, string id, Transform visual, float rotationSpeed)
        { _owner = owner; _id = id; _visual = visual; _rotationSpeed = rotationSpeed; }
        private void Update() { if (_visual != null) _visual.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime, Space.World); }
        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponentInParent<ParkourMotor>() == null) return;
            if (_owner.TryCollect(_id, transform.position)) gameObject.SetActive(false);
        }
    }
}
