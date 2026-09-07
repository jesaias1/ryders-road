using UnityEngine;

namespace Avoidance.Gameplay.Visuals
{
    [DisallowMultipleComponent]
    public sealed class VisualBenchmarkAnchor : MonoBehaviour
    {
        [SerializeField] private string _benchmarkId;
        [SerializeField] private float _yaw;

        public string BenchmarkId => _benchmarkId;
        public float Yaw => _yaw;

        public void Initialize(string benchmarkId, float yaw)
        {
            _benchmarkId = benchmarkId;
            _yaw = yaw;
        }
    }
}
