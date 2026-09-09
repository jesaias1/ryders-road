using Avoidance.Input;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Avoidance.UI.Touch
{
    [DisallowMultipleComponent]
    public sealed class TouchLookControl :
        MonoBehaviour,
        IPointerDownHandler,
        IInitializePotentialDragHandler,
        IDragHandler,
        IPointerUpHandler
    {
        private TouchInputCoordinator _coordinator;
        private RectTransform _zone;
        private Image _zoneImage;
        private float _baseOpacity = 0.001f;
        private bool _zonesVisible;
        private bool _manualLookEnabled = true;
        private int _activePointer = TouchOwnershipRegistry.UnassignedPointerId;

        public void Configure(RectTransform zone, float baseOpacity)
        {
            _zone = zone;
            _baseOpacity = Mathf.Clamp01(baseOpacity);
            _zoneImage = GetComponent<Image>();
            SetZoneVisible(false);
        }

        public void Initialize(TouchInputCoordinator coordinator)
        {
            _coordinator = coordinator;
            _zoneImage ??= GetComponent<Image>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_coordinator.TryClaim(TouchControlRole.Look, eventData.pointerId))
            {
                _activePointer = eventData.pointerId;
                _coordinator.BeginLookGesture(
                    eventData.pointerId,
                    eventData.position,
                    Time.unscaledTime);
            }
        }

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            if (_coordinator.JumpLookEnabled) eventData.useDragThreshold = false;
        }

        private void OnDisable()
        {
            if (_coordinator != null && _coordinator.JumpLookEnabled)
                _coordinator.CancelLookGesture(_activePointer);
            _activePointer = TouchOwnershipRegistry.UnassignedPointerId;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_coordinator.IsOwner(TouchControlRole.Look, eventData.pointerId))
            {
                if (_coordinator.UpdateLookGesture(
                        eventData.pointerId,
                        eventData.position,
                        Time.unscaledTime,
                        ReferenceLength()))
                {
                    _coordinator.AddLookDelta(NormalizeDelta(eventData.delta));
                }
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_coordinator.IsOwner(TouchControlRole.Look, eventData.pointerId))
            {
                _coordinator.EndLookGesture(
                    eventData.pointerId,
                    eventData.position,
                    Time.unscaledTime,
                    ReferenceLength());
            }

            _coordinator.Release(TouchControlRole.Look, eventData.pointerId);
        }

        public void ResetVisual() { }

        public void SetZoneVisible(bool visible)
        {
            _zonesVisible = visible;
            if (_zoneImage != null)
            {
                _zoneImage.color = new Color(
                    0.1f,
                    0.75f,
                    1f,
                    visible ? Mathf.Max(_baseOpacity, 0.08f) : _baseOpacity);
                _zoneImage.raycastTarget = _manualLookEnabled;
            }
        }

        public void ApplyProfile(TouchControlRuntimeProfile profile)
        {
            if (_zone != null)
            {
                _zone.anchorMin = profile.LookZoneMin;
                _zone.anchorMax = profile.LookZoneMax;
                _zone.offsetMin = Vector2.zero;
                _zone.offsetMax = Vector2.zero;
            }

            _manualLookEnabled = profile.ManualLookEnabled;
            SetZoneVisible(_zonesVisible);
        }

        private float ReferenceLength()
        {
            if (_zone == null)
            {
                return Mathf.Max(1f, Mathf.Min(Screen.width, Screen.height));
            }

            var rect = _zone.rect;
            return Mathf.Max(1f, Mathf.Min(rect.width, rect.height));
        }

        public Vector2 NormalizeDelta(Vector2 pixelDelta)
        {
            return pixelDelta * (1080f / ReferenceLength());
        }
    }
}
