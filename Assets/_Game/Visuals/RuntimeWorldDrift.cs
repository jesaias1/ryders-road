using UnityEngine;

namespace Avoidance.Gameplay.Visuals
{
    [DisallowMultipleComponent]
    public sealed class RuntimeWorldDrift : MonoBehaviour
    {
        private Vector3 _origin;
        private float _radius;
        private float _speed;
        private float _phase;

        public void Initialize(Vector3 origin, float radius, float speed, float phase = 0f)
        {
            _origin = origin;
            _radius = Mathf.Max(0f, radius);
            _speed = Mathf.Max(0f, speed);
            _phase = phase;
        }

        private void LateUpdate()
        {
            if (_radius <= 0f || _speed <= 0f)
            {
                return;
            }

            var time = Time.time * _speed + _phase;
            transform.position = _origin + new Vector3(
                Mathf.Sin(time) * _radius,
                Mathf.Sin(time * 0.37f) * _radius * 0.08f,
                Mathf.Cos(time * 0.71f) * _radius * 0.45f);
        }
    }
}
