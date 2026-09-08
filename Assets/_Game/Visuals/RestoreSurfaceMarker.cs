using UnityEngine;
namespace Avoidance.Gameplay.Visuals
{
    // Surface-only checkpoint signal. It never owns Restore state or collision.
    public sealed class RestoreSurfaceMarker : MonoBehaviour
    {
        [SerializeField] private Color _idle = new Color(.08f,.48f,.55f);
        [SerializeField] private Color _activated = new Color(.22f,.92f,1f);
        [SerializeField] private float _activationSeconds = .65f;
        private float _remaining;
        private Renderer _renderer;
        private MaterialPropertyBlock _properties;
        public bool IsPulsing => _remaining > 0;
        public void Activate(){_remaining=_activationSeconds;}
        private void Awake(){_renderer=GetComponent<Renderer>();_properties=new MaterialPropertyBlock();}
        private void LateUpdate()
        {
            _remaining=Mathf.Max(0,_remaining-Time.deltaTime);
            _renderer.GetPropertyBlock(_properties);
            _properties.SetColor("_BaseColor",Color.Lerp(_idle,_activated,_remaining/Mathf.Max(.01f,_activationSeconds)));
            _renderer.SetPropertyBlock(_properties);
        }
    }
}
