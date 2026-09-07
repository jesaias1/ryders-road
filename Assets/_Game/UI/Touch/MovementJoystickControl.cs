using Avoidance.Input;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Avoidance.UI.Touch
{
    [DisallowMultipleComponent]
    public sealed class MovementJoystickControl :
        MonoBehaviour,
        IPointerDownHandler,
        IDragHandler,
        IPointerUpHandler
    {
        private TouchInputCoordinator _coordinator;
        private RectTransform _zone;
        private RectTransform _originParent;
        private RectTransform _background;
        private RectTransform _knob;
        private Image _zoneImage;
        private Image _backgroundImage;
        private Image _knobImage;
        private Vector2 _fixedPosition;
        private float _deadZone;
        private float _horizontalEdgeComfortMargin;
        private float _verticalEdgeComfortMargin;
        private float _ringOpacity = 0.28f;
        private float _knobOpacity = 0.38f;
        private bool _floating;
        private bool _active;
        private bool _zonesVisible;

        public void Configure(
            RectTransform zone,
            RectTransform originParent,
            RectTransform background,
            RectTransform knob,
            float deadZone,
            bool floating,
            float horizontalEdgeComfortMargin,
            float verticalEdgeComfortMargin,
            float ringOpacity,
            float knobOpacity)
        {
            _zone = zone;
            _originParent = originParent;
            _background = background;
            _knob = knob;
            _zoneImage = zone.GetComponent<Image>();
            _backgroundImage = background.GetComponent<Image>();
            _knobImage = knob.GetComponent<Image>();
            _fixedPosition = background.anchoredPosition;
            _deadZone = Mathf.Clamp01(deadZone);
            _floating = floating;
            _horizontalEdgeComfortMargin = Mathf.Max(0f, horizontalEdgeComfortMargin);
            _verticalEdgeComfortMargin = Mathf.Max(0f, verticalEdgeComfortMargin);
            _ringOpacity = Mathf.Clamp01(ringOpacity);
            _knobOpacity = Mathf.Clamp01(knobOpacity);
            SetActiveVisual(false);
        }

        public void Initialize(TouchInputCoordinator coordinator)
        {
            _coordinator = coordinator;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_coordinator.TryClaim(TouchControlRole.Movement, eventData.pointerId))
            {
                return;
            }

            var localOrigin = ResolveOriginPoint(eventData);
            if (_floating
                && localOrigin.HasValue)
            {
                _background.anchoredPosition = ClampOrigin(localOrigin.Value);
            }

            _active = true;
            SetActiveVisual(true);
            if (localOrigin.HasValue)
            {
                _coordinator.BeginMovementGesture(
                    eventData.pointerId,
                    localOrigin.Value,
                    Time.unscaledTime);
            }

            UpdateFromPointer(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_coordinator.IsOwner(TouchControlRole.Movement, eventData.pointerId))
            {
                var localOrigin = ResolveOriginPoint(eventData);
                if (localOrigin.HasValue)
                {
                    _coordinator.UpdateMovementGesture(
                        eventData.pointerId,
                        localOrigin.Value,
                        Time.unscaledTime,
                        ReferenceLength());
                }

                UpdateFromPointer(eventData);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_coordinator.IsOwner(TouchControlRole.Movement, eventData.pointerId))
            {
                return;
            }

            _coordinator.EndMovementGesture(eventData.pointerId);
            _coordinator.Release(TouchControlRole.Movement, eventData.pointerId);
            ResetVisual();
        }

        public void ResetVisual()
        {
            _active = false;
            if (_knob != null)
            {
                _knob.anchoredPosition = Vector2.zero;
            }

            if (_background != null && _floating)
            {
                _background.anchoredPosition = _fixedPosition;
            }

            SetActiveVisual(false);
        }

        public void SetZoneVisible(bool visible)
        {
            _zonesVisible = visible;
            if (_zoneImage != null)
            {
                var color = _zoneImage.color;
                color.a = visible ? 0.06f : 0.001f;
                _zoneImage.color = color;
            }

            SetActiveVisual(_active);
        }

        public void ApplyProfile(TouchControlRuntimeProfile profile)
        {
            if (_zone != null)
            {
                _zone.anchorMin = profile.MovementZoneMin;
                _zone.anchorMax = profile.MovementZoneMax;
                _zone.offsetMin = Vector2.zero;
                _zone.offsetMax = Vector2.zero;
            }

            if (_background != null && _originParent != null)
            {
                var desired = TouchControlGeometry.AnchorToLocal(
                    _originParent.rect,
                    profile.MovementRestCenter);
                _fixedPosition = ClampOrigin(desired);
                if (!_active || !_floating)
                {
                    _background.anchoredPosition = _fixedPosition;
                }
            }

            SetActiveVisual(_active);
        }

        private void SetActiveVisual(bool active)
        {
            if (_backgroundImage != null)
            {
                var color = _backgroundImage.color;
                color.a = active
                    ? _ringOpacity
                    : _zonesVisible
                        ? Mathf.Max(_ringOpacity * 0.55f, 0.12f)
                        : 0f;
                _backgroundImage.color = color;
            }

            if (_knobImage != null)
            {
                var color = _knobImage.color;
                color.a = active
                    ? _knobOpacity
                    : _zonesVisible
                        ? Mathf.Max(_knobOpacity * 0.5f, 0.14f)
                        : 0f;
                _knobImage.color = color;
            }
        }

        private void UpdateFromPointer(PointerEventData eventData)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _background,
                eventData.position,
                eventData.pressEventCamera,
                out var localPoint))
            {
                return;
            }

            var normalized = TouchStickMath.NormalizeLocalPoint(localPoint, _background.rect);
            var movement = TouchStickMath.ApplyDeadZone(normalized, _deadZone);
            var radius = Mathf.Min(_background.rect.width, _background.rect.height) * 0.5f;
            _knob.anchoredPosition = normalized * radius * 0.48f;
            _coordinator.SetMovement(normalized, movement);
            _coordinator.SetMovementVisualState(_background.anchoredPosition, _knob.anchoredPosition);
        }

        private Vector2? ResolveOriginPoint(PointerEventData eventData)
        {
            if (_originParent == null
                || !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _originParent,
                    eventData.position,
                    eventData.pressEventCamera,
                    out var localPoint))
            {
                return null;
            }

            return localPoint;
        }

        private Vector2 ClampOrigin(Vector2 localPoint)
        {
            if (_originParent == null || _background == null)
            {
                return localPoint;
            }

            var radius = Mathf.Min(_background.rect.width, _background.rect.height) * 0.5f;
            return TouchControlGeometry.ClampOrigin(
                localPoint,
                _originParent.rect,
                radius,
                _horizontalEdgeComfortMargin,
                _verticalEdgeComfortMargin);
        }

        private float ReferenceLength()
        {
            if (_originParent == null)
            {
                return Mathf.Max(1f, Mathf.Min(Screen.width, Screen.height));
            }

            var rect = _originParent.rect;
            return Mathf.Max(1f, Mathf.Min(rect.width, rect.height));
        }
    }
}
