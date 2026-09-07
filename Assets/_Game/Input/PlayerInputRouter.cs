using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Avoidance.Input
{
    public sealed class PlayerInputRouter : IDevelopmentPlayerInputSource, IFlowSteeringInputSource
    {
        private static readonly int[] NoTouches = new int[0];
        private ITouchInputProvider _touch;
        private PlayerInputMode _mode;
        private bool _editorPointerInitialized;

        public PlayerInputRouter(
            ITouchInputProvider touch = null,
            PlayerInputMode mode = PlayerInputMode.Auto)
        {
            _touch = touch;
            _mode = mode;
        }

        public Vector2 Move { get; private set; }
        public Vector2 LookDelta { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool RestartPressed { get; private set; }
        public bool SwitchProfilePressed { get; private set; }
        public bool ToggleTouchZonesPressed { get; private set; }
        public bool ToggleCameraEffectsPressed { get; private set; }
        public bool ToggleDiagnosticsPressed { get; private set; }
        public IReadOnlyList<int> ActiveTouchIds => _touch?.ActiveTouchIds ?? NoTouches;
        public TouchInputDebugState TouchDebugState => _touch?.DebugState ?? TouchInputDebugState.Empty;
        public PlayerInputMode ConfiguredMode => _mode;
        public PlayerInputMode ActiveMode { get; private set; }
        public bool FlowSteeringEnabled { get; private set; }
        public AutoCameraProfileKind AutoCameraProfile { get; private set; } =
            AutoCameraProfileKind.Balanced;

        public void SetMode(PlayerInputMode mode)
        {
            _mode = mode;
            ResetState();
        }

        public void SetTouchProvider(ITouchInputProvider touch)
        {
            _touch = touch;
            ResetState();
        }

        public void Sample()
        {
            var touchActive = _touch != null && _touch.ActiveTouchIds.Count > 0;
            ActiveMode = InputSourceSelector.Resolve(
                _mode,
                touchActive,
                Application.isMobilePlatform);

            Move = Vector2.zero;
            LookDelta = Vector2.zero;
            JumpPressed = false;
            RestartPressed = false;
            SwitchProfilePressed = false;
            ToggleTouchZonesPressed = false;
            ToggleCameraEffectsPressed = false;
            ToggleDiagnosticsPressed = false;
            FlowSteeringEnabled = false;
            AutoCameraProfile = AutoCameraProfileKind.Balanced;

            if (ActiveMode == PlayerInputMode.Touch)
            {
                SampleTouch();
            }
            else
            {
                SampleEditor();
            }
        }

        public void ResetState()
        {
            Move = Vector2.zero;
            LookDelta = Vector2.zero;
            JumpPressed = false;
            RestartPressed = false;
            SwitchProfilePressed = false;
            ToggleTouchZonesPressed = false;
            ToggleCameraEffectsPressed = false;
            ToggleDiagnosticsPressed = false;
            _touch?.ResetState();
        }

        private void SampleTouch()
        {
            if (_touch == null)
            {
                return;
            }

            Move = Vector2.ClampMagnitude(_touch.Movement, 1f);
            LookDelta = _touch.ConsumeLookDelta();
            JumpPressed = _touch.ConsumeJumpPressed();
            var touchProfile = _touch.DebugState;
            FlowSteeringEnabled = touchProfile.FlowSteeringEnabled;
            AutoCameraProfile = touchProfile.AutoCameraProfile;
            RestartPressed = _touch.ConsumeRestartPressed();
            SwitchProfilePressed = _touch.ConsumeSwitchProfilePressed();
            ToggleCameraEffectsPressed = _touch.ConsumeToggleCameraEffectsPressed();
            ToggleDiagnosticsPressed = _touch.ConsumeToggleDiagnosticsPressed();

            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                RestartPressed |= keyboard.rKey.wasPressedThisFrame;
                SwitchProfilePressed |= keyboard.f2Key.wasPressedThisFrame;
                ToggleTouchZonesPressed = keyboard.f3Key.wasPressedThisFrame;
                ToggleCameraEffectsPressed |= keyboard.f4Key.wasPressedThisFrame;
                ToggleDiagnosticsPressed |= keyboard.f1Key.wasPressedThisFrame;
            }
        }

        private void SampleEditor()
        {
            var keyboard = Keyboard.current;
            if (Application.isPlaying && !_editorPointerInitialized)
            {
                LockEditorPointer();
            }

            if (keyboard != null)
            {
                if (keyboard.escapeKey.wasPressedThisFrame)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }

                var horizontal = 0f;
                var vertical = 0f;
                if (keyboard.aKey.isPressed) horizontal -= 1f;
                if (keyboard.dKey.isPressed) horizontal += 1f;
                if (keyboard.sKey.isPressed) vertical -= 1f;
                if (keyboard.wKey.isPressed) vertical += 1f;
                Move = Vector2.ClampMagnitude(new Vector2(horizontal, vertical), 1f);
                JumpPressed = keyboard.spaceKey.wasPressedThisFrame;
                RestartPressed = keyboard.rKey.wasPressedThisFrame;
                SwitchProfilePressed = keyboard.f2Key.wasPressedThisFrame;
                ToggleTouchZonesPressed = keyboard.f3Key.wasPressedThisFrame;
                ToggleCameraEffectsPressed = keyboard.f4Key.wasPressedThisFrame;
                ToggleDiagnosticsPressed = keyboard.f1Key.wasPressedThisFrame;
            }

            var mouse = Mouse.current;
            if (mouse != null)
            {
                if (mouse.leftButton.wasPressedThisFrame
                    && Cursor.lockState != CursorLockMode.Locked)
                {
                    LockEditorPointer();
                }

                if (Cursor.lockState == CursorLockMode.Locked)
                {
                    LookDelta = mouse.delta.ReadValue();
                }
            }
        }

        private void LockEditorPointer()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            _editorPointerInitialized = true;
        }
    }
}
