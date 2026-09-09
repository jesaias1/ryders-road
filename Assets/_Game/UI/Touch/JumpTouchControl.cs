using Avoidance.Input;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Avoidance.UI.Touch
{
    [DisallowMultipleComponent]
    public sealed class JumpTouchControl :
        MonoBehaviour,
        IPointerDownHandler,
        IDragHandler,
        IPointerUpHandler
    {
        private TouchInputCoordinator _coordinator;
        private Image _image;
        private float _baseOpacity = 0.32f;
        private bool _fixedButtonEnabled = true;
        private bool _rightJumpZoneEnabled;

        public void Configure(float baseOpacity)
        {
            _baseOpacity = Mathf.Clamp01(baseOpacity);
        }

        public void Initialize(TouchInputCoordinator coordinator)
        {
            _coordinator = coordinator;
            _image = GetComponent<Image>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_fixedButtonEnabled && !_rightJumpZoneEnabled)
            {
                return;
            }

            if (_coordinator.TryClaim(TouchControlRole.Jump, eventData.pointerId))
            {
                _coordinator.PressJump(_rightJumpZoneEnabled
                    ? TouchJumpSource.RightTap
                    : TouchJumpSource.FixedButton);
                SetPressed(true);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_coordinator.IsOwner(TouchControlRole.Jump, eventData.pointerId))
            {
                return;
            }

            _coordinator.Release(TouchControlRole.Jump, eventData.pointerId);
            SetPressed(false);
        }

        public void OnDrag(PointerEventData eventData)
        {
            // Match the adjacent look surface's sensitivity; ownership stays with
            // the initial Jump contact even when it moves outside the button.
            var look = GetComponentInParent<Canvas>()?.GetComponentInChildren<TouchLookControl>();
            if (look != null)
                _coordinator.DragJumpLook(eventData.pointerId, look.NormalizeDelta(eventData.delta));
        }

        public void ResetVisual() => SetPressed(false);

        public void SetZoneVisible(bool visible)
        {
            if (_image != null)
            {
                var color = _image.color;
                color.a = !_fixedButtonEnabled && !_rightJumpZoneEnabled
                    ? 0f
                    : visible
                        ? Mathf.Max(_baseOpacity, 0.58f)
                        : _rightJumpZoneEnabled
                            ? 0.001f
                            : _baseOpacity;
                _image.color = color;
                _image.raycastTarget = _fixedButtonEnabled || _rightJumpZoneEnabled;
            }
        }

        public void ApplyProfile(TouchControlRuntimeProfile profile)
        {
            _fixedButtonEnabled = profile.EnableFixedButton;
            _rightJumpZoneEnabled = profile.EnableRightJumpZone;
            SetZoneVisible(false);
            foreach (var graphic in GetComponentsInChildren<Graphic>(true))
            {
                var color = graphic.color;
                if (graphic == _image)
                {
                    color.a = _rightJumpZoneEnabled
                        ? 0.001f
                        : _fixedButtonEnabled
                            ? _baseOpacity
                            : 0f;
                }
                else
                {
                    color.a = _fixedButtonEnabled && !_rightJumpZoneEnabled ? 0.82f : 0f;
                }

                graphic.color = color;
                graphic.raycastTarget = (_fixedButtonEnabled || _rightJumpZoneEnabled)
                    && graphic == _image;
            }
        }

        private void SetPressed(bool pressed)
        {
            if (_image != null)
            {
                var color = _image.color;
                color.r = pressed ? 0.35f : 0.15f;
                color.g = pressed ? 0.95f : 0.78f;
                color.a = _rightJumpZoneEnabled
                    ? (pressed ? 0.10f : 0.001f)
                    : _fixedButtonEnabled
                        ? _baseOpacity
                        : 0f;
                _image.color = color;
            }
        }
    }
}
