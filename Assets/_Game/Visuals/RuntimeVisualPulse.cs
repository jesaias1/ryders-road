using UnityEngine;

namespace Avoidance.Gameplay.Visuals
{
    [DisallowMultipleComponent]
    public sealed class RuntimeVisualPulse : MonoBehaviour
    {
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        private Renderer[] _renderers;
        private MaterialPropertyBlock _properties;
        private Color _baseColor;
        private Color _pulseColor;
        private float _speed;
        private float _amount;
        private float _phase;

        public void Initialize(Color baseColor, Color pulseColor, float speed, float amount, float phase = 0f)
        {
            _baseColor = baseColor;
            _pulseColor = pulseColor;
            _speed = Mathf.Max(0.01f, speed);
            _amount = Mathf.Clamp01(amount);
            _phase = phase;
            _renderers = GetComponentsInChildren<Renderer>(true);
            _properties = new MaterialPropertyBlock();
        }

        private void LateUpdate()
        {
            if (_renderers == null || _renderers.Length == 0)
            {
                return;
            }

            var blend = (Mathf.Sin(Time.time * _speed + _phase) + 1f) * 0.5f * _amount;
            var color = Color.Lerp(_baseColor, _pulseColor, blend);
            for (var index = 0; index < _renderers.Length; index++)
            {
                var target = _renderers[index];
                if (target == null)
                {
                    continue;
                }

                target.GetPropertyBlock(_properties);
                _properties.SetColor(BaseColor, color);
                _properties.SetColor(ColorId, color);
                target.SetPropertyBlock(_properties);
            }
        }
    }
}
