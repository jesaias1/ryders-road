using Avoidance.Core.Services;
using UnityEngine;

namespace Avoidance.Gameplay.Checkpoints
{
    public readonly struct CheckpointSnapshot
    {
        public CheckpointSnapshot(string id, int order, Vector3 position, Quaternion rotation)
        {
            Id = id;
            Order = order;
            Position = position;
            Rotation = rotation;
        }

        public string Id { get; }
        public int Order { get; }
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }
    }

    public sealed class CheckpointService : ICheckpointService
    {
        private CheckpointSnapshot _start;

        public string CurrentCheckpointId => Current.Id;
        public CheckpointSnapshot Current { get; private set; }

        public void SetStart(Vector3 position, Quaternion rotation)
        {
            _start = new CheckpointSnapshot("restore.start", -1, position, rotation);
            Current = _start;
        }

        public bool TryActivate(string id, int order, Vector3 position, Quaternion rotation)
        {
            if (string.IsNullOrWhiteSpace(id) || order <= Current.Order)
            {
                return false;
            }

            Current = new CheckpointSnapshot(id, order, position, rotation);
            return true;
        }

        public void Reset()
        {
            Current = _start;
        }
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class RestorePoint : MonoBehaviour
    {
        private CheckpointService _service;
        private string _checkpointId;
        private int _order;
        private Vector3 _restorePosition;
        private bool _activated;
        private System.Action _onActivated;

        public string CheckpointId => _checkpointId;
        public bool IsActivated => _activated;

        public void Initialize(
            CheckpointService service,
            string checkpointId,
            int order,
            Vector3 restorePosition,
            System.Action onActivated)
        {
            _service = service;
            _checkpointId = checkpointId;
            _order = order;
            _restorePosition = restorePosition;
            _onActivated = onActivated;
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_activated
                || other.GetComponent<Avoidance.Gameplay.Player.ParkourMotor>() == null)
            {
                return;
            }

            if (_service != null
                && _service.TryActivate(
                    _checkpointId,
                    _order,
                    _restorePosition,
                    transform.rotation))
            {
                _activated = true;
                _onActivated?.Invoke();
            }
        }
    }
}
