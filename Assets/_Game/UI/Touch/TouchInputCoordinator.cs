using System;
using System.Collections.Generic;
using Avoidance.Core.Services;
using Avoidance.Input;
using UnityEngine;

namespace Avoidance.UI.Touch
{
    [DisallowMultipleComponent]
    public sealed class TouchInputCoordinator : MonoBehaviour, ITouchInputProvider, IHeldJumpInputSource
    {
        public const string ControlProfilePreferenceKey = "settings.touch-control-profile";
        public const string ControlProfilePreferenceVersionKey = "settings.touch-control-profile-version";
        public const string MovementSensitivityPreferenceKey = "settings.touch-movement-sensitivity";
        public const string TouchZonesPreferenceKey = "settings.touch-zones-visible";
        public const string HapticsPreferenceKey = "settings.haptics-enabled";
        public const int CurrentControlProfilePreferenceVersion = 1;

        private readonly TouchOwnershipRegistry _ownership = new TouchOwnershipRegistry();
        private readonly List<int> _activeTouchIds = new List<int>(3);
        private readonly TapDragGestureClassifier _lookGesture =
            new TapDragGestureClassifier();
        private readonly FlickGestureDetector _flickDetector =
            new FlickGestureDetector(FlickGestureSettings.Default);
        private Vector2 _rawMovement;
        private Vector2 _postDeadZoneMovement;
        private Vector2 _movement;
        private Vector2 _lookDelta;
        private Vector2 _movementOrigin;
        private Vector2 _movementKnob;
        private FlickGestureResult _flickResult = FlickGestureResult.Empty;
        private TouchControlLayout _layout;
        private TouchControlProfileKind _controlProfile;
        private TouchJumpMode _jumpMode;
        private TouchSensitivityPreset _cameraSensitivity;
        private TouchMovementSensitivityPreset _movementSensitivity;
        private TouchControlRuntimeProfile _runtimeProfile;
        private bool _jumpPressed;
        private bool _restartPressed;
        private bool _switchProfilePressed;
        private bool _toggleCameraEffectsPressed;
        private bool _toggleDiagnosticsPressed;
        private bool _zonesVisible;
        private float _lastTapDuration = -1f;
        private float _lastTapDisplacement = -1f;
        private float _lastTapPeakVelocity = -1f;
        private TouchJumpSource _lastJumpSource = TouchJumpSource.None;
        private MovementJoystickControl _joystick;
        private TouchLookControl _look;
        private JumpTouchControl _jump;
        private ISettingsService _settings;

        public Vector2 Movement => _movement;
        public bool JumpLookEnabled { get; private set; }
        private Vector2 _savedJumpMin, _savedJumpMax, _savedJumpPosition, _savedJumpSize;
        public bool JumpHeld => JumpLookEnabled
            && _ownership.GetOwner(TouchControlRole.Jump) != TouchOwnershipRegistry.UnassignedPointerId;

        public void EnableFoundationJumpLook()
        {
            if (JumpLookEnabled) return;
            JumpLookEnabled = true;
            SetSessionControlProfile(TouchControlProfileKind.LeftMoveRightLookTapAndButton);
            // Keep a distinct inward acquisition area, with ordinary look outside it.
            if (_jump != null)
            {
                var rect = _jump.GetComponent<RectTransform>();
                _savedJumpMin = rect.anchorMin; _savedJumpMax = rect.anchorMax;
                _savedJumpPosition = rect.anchoredPosition; _savedJumpSize = rect.sizeDelta;
                rect.anchorMin = rect.anchorMax = EnsureLayout().RightRestCenter;
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = Vector2.one * EnsureLayout().JumpButtonSize;
            }
        }

        public void DisableFoundationJumpLook()
        {
            if (!JumpLookEnabled) return;
            JumpLookEnabled = false;
            ResetState();
            if (_jump != null)
            {
                var rect = _jump.GetComponent<RectTransform>();
                rect.anchorMin = _savedJumpMin; rect.anchorMax = _savedJumpMax;
                rect.anchoredPosition = _savedJumpPosition; rect.sizeDelta = _savedJumpSize;
            }
        }

        public void DragJumpLook(int pointerId, Vector2 normalizedDelta)
        {
            if (JumpLookEnabled && IsOwner(TouchControlRole.Jump, pointerId))
                AddLookDelta(normalizedDelta);
        }
        public TouchInputDebugState DebugState => new TouchInputDebugState(
            _rawMovement,
            _postDeadZoneMovement,
            _movement,
            _ownership.GetOwner(_runtimeProfile.MovementOnRight
                ? TouchControlRole.Movement
                : TouchControlRole.Look),
            _movementOrigin,
            _movementKnob,
            _flickResult.Velocity,
            _flickResult.FlickVelocity,
            _flickResult.Distance,
            _flickResult.Duration,
            _flickResult.DirectionAngle,
            _flickResult.LastFlickTime,
            _flickResult.Candidate,
            _flickResult.Armed,
            _flickResult.Triggered,
            _lastTapDuration,
            _lastJumpSource,
            _flickResult.RejectReason,
            _runtimeProfile.ControlProfile,
            _runtimeProfile.JumpMode,
            _runtimeProfile.CameraSensitivity,
            _runtimeProfile.MovementSensitivity,
            _runtimeProfile.FlowSteeringEnabled,
            _runtimeProfile.AutoCameraProfile,
            _ownership.GetOwner(TouchControlRole.Movement),
            _ownership.GetOwner(TouchControlRole.Look),
            _ownership.GetOwner(TouchControlRole.Jump));
        public IReadOnlyList<int> ActiveTouchIds => _activeTouchIds;
        public bool ZonesVisible => _zonesVisible;
        public TouchControlRuntimeProfile RuntimeProfile => _runtimeProfile;

        public void Initialize(
            TouchControlLayout layout,
            MovementJoystickControl joystick,
            TouchLookControl look,
            JumpTouchControl jump,
            ISettingsService settingsService = null)
        {
            _layout = layout != null ? layout : TouchControlLayout.CreateRuntimeDefault();
            _settings = settingsService;
            _controlProfile = LoadPreferredControlProfile(_layout.DefaultControlProfile);
            _jumpMode = ResolveJumpModeForControlProfile(_controlProfile);
            _cameraSensitivity = settingsService == null
                ? _layout.DefaultCameraSensitivity
                : PresetFromNormalized(settingsService.Current.lookSensitivity);
            _movementSensitivity = LoadPreferredMovementSensitivity(_layout.DefaultMovementSensitivity);
            _flickDetector.Configure(_layout.FlickSettings);
            _joystick = joystick;
            _look = look;
            _jump = jump;
            _joystick.Initialize(this);
            _look.Initialize(this);
            _jump.Initialize(this);
            ApplyRuntimeProfile();
            SetZonesVisible(PlayerPrefs.GetInt(TouchZonesPreferenceKey, 0) != 0, save: false);
        }

        public void Initialize(
            MovementJoystickControl joystick,
            TouchLookControl look,
            JumpTouchControl jump)
        {
            Initialize(TouchControlLayout.CreateRuntimeDefault(), joystick, look, jump);
        }

        public void CycleControlProfile()
        {
            EnsureLayout();
            _controlProfile = TouchControlRuntimeProfile.Next(_controlProfile);
            _jumpMode = ResolveJumpModeForControlProfile(_controlProfile);
            ApplyRuntimeProfile();
        }

        public void SetControlProfile(TouchControlProfileKind controlProfile)
        {
            if (JumpLookEnabled) return; // This isolated trial evaluates manual view only.
            SetSessionControlProfile(controlProfile);
            PersistControlProfile();
        }

        // Training comparisons must not overwrite the player's Campaign preference.
        public void SetSessionControlProfile(TouchControlProfileKind controlProfile)
        {
            EnsureLayout();
            if (JumpLookEnabled) controlProfile = TouchControlProfileKind.LeftMoveRightLookTapAndButton;
            _controlProfile = controlProfile;
            _jumpMode = ResolveJumpModeForControlProfile(_controlProfile);
            ApplyRuntimeProfile();
            ResetState();
        }

        public void UseSmartParkourEasyMode()
        {
            SetControlProfile(TouchControlProfileKind.FlowSteerAutoBalanced);
        }

        public void UseClassicManualMode()
        {
            SetControlProfile(TouchControlProfileKind.LeftMoveRightLookTapJump);
        }

        public void CycleJumpMode()
        {
            EnsureLayout();
            CycleJumpModeVariant();
            ApplyRuntimeProfile();
        }

        public void CycleCameraSensitivity()
        {
            EnsureLayout();
            _cameraSensitivity = TouchControlRuntimeProfile.Next(_cameraSensitivity);
            ApplyRuntimeProfile();
            PersistCameraSensitivity();
        }

        public void ApplySettings(ISettingsService settingsService)
        {
            _settings = settingsService;
            if (_settings != null)
            {
                SetCameraSensitivityNormalized(_settings.Current.lookSensitivity, save: false);
            }
        }

        public void SetCameraSensitivityNormalized(float normalized, bool save = true)
        {
            EnsureLayout();
            _cameraSensitivity = PresetFromNormalized(normalized);
            ApplyRuntimeProfile();
            if (save)
            {
                PersistCameraSensitivity();
            }
        }

        public void CycleMovementSensitivity()
        {
            EnsureLayout();
            _movementSensitivity = TouchControlRuntimeProfile.Next(_movementSensitivity);
            ApplyRuntimeProfile();
            PersistMovementSensitivity();
        }

        public bool TryClaim(TouchControlRole role, int pointerId)
        {
            if (role == TouchControlRole.Jump
                && !_runtimeProfile.EnableFixedButton
                && !_runtimeProfile.EnableRightJumpZone)
            {
                return false;
            }

            if (!_ownership.TryClaim(role, pointerId))
            {
                return false;
            }

            RebuildActiveIds();
            return true;
        }

        public bool IsOwner(TouchControlRole role, int pointerId)
        {
            return _ownership.IsOwner(role, pointerId);
        }

        public void Release(TouchControlRole role, int pointerId)
        {
            var wasOwner = _ownership.IsOwner(role, pointerId);
            _ownership.Release(role, pointerId);
            if (wasOwner && role == TouchControlRole.Movement)
            {
                _rawMovement = Vector2.zero;
                _postDeadZoneMovement = Vector2.zero;
                _movement = Vector2.zero;
            }

            RebuildActiveIds();
        }

        public void SetMovement(Vector2 movement)
        {
            SetMovement(movement, movement);
        }

        public void SetMovement(Vector2 rawMovement, Vector2 postDeadZoneMovement)
        {
            _rawMovement = Vector2.ClampMagnitude(rawMovement, 1f);
            _postDeadZoneMovement = Vector2.ClampMagnitude(postDeadZoneMovement, 1f);
            var multiplier = _runtimeProfile.MovementSensitivityMultiplier > 0f
                ? _runtimeProfile.MovementSensitivityMultiplier
                : 1f;
            _movement = Vector2.ClampMagnitude(
                _postDeadZoneMovement * multiplier,
                1f);
        }

        public void SetMovementVisualState(Vector2 origin, Vector2 knob)
        {
            _movementOrigin = origin;
            _movementKnob = knob;
        }

        public void AddLookDelta(Vector2 delta)
        {
            _lookDelta += delta;
        }

        public void PressJump()
        {
            PressJump(TouchJumpSource.FixedButton);
        }

        public void PressJump(TouchJumpSource source)
        {
            if (_jumpPressed && _lastJumpSource != source)
            {
                _lastJumpSource = TouchJumpSource.Multiple;
            }
            else
            {
                _lastJumpSource = source;
            }

            _jumpPressed = true;
        }

        public void BeginMovementGesture(
            int pointerId,
            Vector2 localPosition,
            float time)
        {
            if (!_runtimeProfile.EnableRightFlick || !IsOwner(TouchControlRole.Movement, pointerId))
            {
                return;
            }

            _flickDetector.Configure(EnsureLayout().FlickSettings);
            _flickDetector.Begin(localPosition, time);
            _flickResult = _flickDetector.LastResult;
        }

        public void UpdateMovementGesture(
            int pointerId,
            Vector2 localPosition,
            float time,
            float referenceLength)
        {
            if (!_runtimeProfile.EnableRightFlick || !IsOwner(TouchControlRole.Movement, pointerId))
            {
                return;
            }

            EnsureLayout();
            _flickResult = _flickDetector.Update(localPosition, time, referenceLength);
            if (!_flickResult.Triggered)
            {
                return;
            }

            PressJump(TouchJumpSource.RightFlick);

        }

        public void EndMovementGesture(int pointerId)
        {
            if (IsOwner(TouchControlRole.Movement, pointerId))
            {
                _flickDetector.End();
                _flickResult = _flickDetector.LastResult;
            }
        }

        public void BeginLookGesture(int pointerId, Vector2 screenPosition, float time)
        {
            if (!_runtimeProfile.EnableRightTap || !IsOwner(TouchControlRole.Look, pointerId))
            {
                return;
            }

            _lookGesture.Begin(pointerId, screenPosition, time);
        }

        public bool UpdateLookGesture(
            int pointerId,
            Vector2 screenPosition,
            float time,
            float referenceLength)
        {
            if (!IsOwner(TouchControlRole.Look, pointerId))
            {
                return false;
            }

            if (!_runtimeProfile.EnableRightTap)
            {
                return true;
            }

            var wasCamera = _lookGesture.IsCameraActive;
            var state = _lookGesture.Update(
                pointerId,
                screenPosition,
                time,
                referenceLength,
                CreateTapDragSettings());
            return state == TapDragGestureState.Camera;
        }

        public void EndLookGesture(
            int pointerId,
            Vector2 screenPosition,
            float time,
            float referenceLength)
        {
            if (!_runtimeProfile.EnableRightTap || !IsOwner(TouchControlRole.Look, pointerId))
            {
                return;
            }

            var state = _lookGesture.Release(
                pointerId,
                screenPosition,
                time,
                referenceLength,
                CreateTapDragSettings());
            var snapshot = _lookGesture.Snapshot;
            _lastTapDuration = snapshot.Elapsed;
            _lastTapDisplacement = snapshot.TotalDisplacement;
            _lastTapPeakVelocity = snapshot.PeakVelocity;
            if (state == TapDragGestureState.Tap)
            {
                PressJump(TouchJumpSource.RightTap);
            }
        }

        public void CancelLookGesture(int pointerId)
        {
            _lookGesture.Cancel(pointerId);
        }

        public void PressRestart()
        {
            _restartPressed = true;
        }

        public void PressSwitchProfile()
        {
            _switchProfilePressed = true;
        }

        public void PressToggleCameraEffects()
        {
            _toggleCameraEffectsPressed = true;
        }

        public void PressToggleDiagnostics()
        {
            _toggleDiagnosticsPressed = true;
        }

        public Vector2 ConsumeLookDelta()
        {
            var multiplier = _runtimeProfile.CameraSensitivityMultiplier > 0f
                ? _runtimeProfile.CameraSensitivityMultiplier
                : 1f;
            var value = _lookDelta * multiplier;
            _lookDelta = Vector2.zero;
            return value;
        }

        public bool ConsumeJumpPressed()
        {
            var value = _jumpPressed;
            _jumpPressed = false;
            return value;
        }

        public bool ConsumeRestartPressed()
        {
            var value = _restartPressed;
            _restartPressed = false;
            return value;
        }

        public bool ConsumeSwitchProfilePressed()
        {
            var value = _switchProfilePressed;
            _switchProfilePressed = false;
            return value;
        }

        public bool ConsumeToggleCameraEffectsPressed()
        {
            var value = _toggleCameraEffectsPressed;
            _toggleCameraEffectsPressed = false;
            return value;
        }

        public bool ConsumeToggleDiagnosticsPressed()
        {
            var value = _toggleDiagnosticsPressed;
            _toggleDiagnosticsPressed = false;
            return value;
        }

        public void ResetState()
        {
            _ownership.Reset();
            _activeTouchIds.Clear();
            _rawMovement = Vector2.zero;
            _postDeadZoneMovement = Vector2.zero;
            _movement = Vector2.zero;
            _lookDelta = Vector2.zero;
            _movementOrigin = Vector2.zero;
            _movementKnob = Vector2.zero;
            _lookGesture.Reset();
            _flickDetector.End();
            _flickResult = _flickDetector.LastResult;
            _jumpPressed = false;
            _lastJumpSource = TouchJumpSource.None;
            _lastTapDuration = -1f;
            _lastTapDisplacement = -1f;
            _lastTapPeakVelocity = -1f;
            _restartPressed = false;
            _switchProfilePressed = false;
            _toggleCameraEffectsPressed = false;
            _toggleDiagnosticsPressed = false;
            _joystick?.ResetVisual();
            _look?.ResetVisual();
            _jump?.ResetVisual();
        }

        public void SetZonesVisible(bool visible)
        {
            SetZonesVisible(visible, save: true);
        }

        public void SetZonesVisible(bool visible, bool save)
        {
            _zonesVisible = visible;
            _joystick?.SetZoneVisible(visible);
            _look?.SetZoneVisible(visible);
            _jump?.SetZoneVisible(visible);
            if (save)
            {
                PlayerPrefs.SetInt(TouchZonesPreferenceKey, visible ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        private void OnDisable()
        {
            ResetState();
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                ResetState();
            }
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused)
            {
                ResetState();
            }
        }

        private void ApplyRuntimeProfile()
        {
            var layout = EnsureLayout();
            _runtimeProfile = layout.CreateRuntimeProfile(
                _controlProfile,
                _jumpMode,
                _cameraSensitivity,
                _movementSensitivity);
            _joystick?.ApplyProfile(_runtimeProfile);
            _look?.ApplyProfile(_runtimeProfile);
            _jump?.ApplyProfile(_runtimeProfile);
            SetMovement(_rawMovement, _postDeadZoneMovement);
            _lookGesture.Reset();
        }

        private void PersistCameraSensitivity()
        {
            if (_settings == null)
            {
                return;
            }

            _settings.Current.lookSensitivity = NormalizedFromPreset(_cameraSensitivity);
            _settings.Save();
        }

        private void PersistControlProfile()
        {
            StorePreferredControlProfile(_controlProfile);
        }

        private void PersistMovementSensitivity()
        {
            StorePreferredMovementSensitivity(_movementSensitivity);
        }

        private static TouchSensitivityPreset PresetFromNormalized(float value)
        {
            if (value <= 0.25f)
            {
                return TouchSensitivityPreset.Low;
            }

            if (value <= 0.55f)
            {
                return TouchSensitivityPreset.Medium;
            }

            if (value <= 0.82f)
            {
                return TouchSensitivityPreset.Fast;
            }

            return TouchSensitivityPreset.VeryFast;
        }

        private static float NormalizedFromPreset(TouchSensitivityPreset preset)
        {
            switch (preset)
            {
                case TouchSensitivityPreset.Low:
                    return 0.15f;
                case TouchSensitivityPreset.Medium:
                    return 0.45f;
                case TouchSensitivityPreset.Fast:
                    return 0.7f;
                case TouchSensitivityPreset.VeryFast:
                    return 0.95f;
                default:
                    return 0.7f;
            }
        }

        public static TouchControlProfileKind LoadPreferredControlProfile(
            TouchControlProfileKind fallback)
        {
            var hasStoredProfile = PlayerPrefs.HasKey(ControlProfilePreferenceKey);
            var value = PlayerPrefs.GetInt(ControlProfilePreferenceKey, (int)fallback);
            var resolved = Enum.IsDefined(typeof(TouchControlProfileKind), value)
                ? (TouchControlProfileKind)value
                : fallback;
            var preferenceVersion = PlayerPrefs.GetInt(ControlProfilePreferenceVersionKey, 0);
            if (preferenceVersion < CurrentControlProfilePreferenceVersion)
            {
                if (hasStoredProfile
                    && resolved != fallback
                    && fallback == TouchControlProfileKind.LeftMoveRightLookTapJump)
                {
                    resolved = fallback;
                }

                PlayerPrefs.SetInt(ControlProfilePreferenceKey, (int)resolved);
                PlayerPrefs.SetInt(
                    ControlProfilePreferenceVersionKey,
                    CurrentControlProfilePreferenceVersion);
                PlayerPrefs.Save();
            }

            return resolved;
        }

        public static TouchMovementSensitivityPreset LoadPreferredMovementSensitivity(
            TouchMovementSensitivityPreset fallback)
        {
            var value = PlayerPrefs.GetInt(MovementSensitivityPreferenceKey, (int)fallback);
            return Enum.IsDefined(typeof(TouchMovementSensitivityPreset), value)
                ? (TouchMovementSensitivityPreset)value
                : fallback;
        }

        public static void StorePreferredControlProfile(TouchControlProfileKind controlProfile)
        {
            PlayerPrefs.SetInt(ControlProfilePreferenceKey, (int)controlProfile);
            PlayerPrefs.SetInt(
                ControlProfilePreferenceVersionKey,
                CurrentControlProfilePreferenceVersion);
            PlayerPrefs.Save();
        }

        public static void StorePreferredMovementSensitivity(
            TouchMovementSensitivityPreset movementSensitivity)
        {
            PlayerPrefs.SetInt(MovementSensitivityPreferenceKey, (int)movementSensitivity);
            PlayerPrefs.Save();
        }

        private TouchControlLayout EnsureLayout()
        {
            if (_layout != null)
            {
                return _layout;
            }

            _layout = TouchControlLayout.CreateRuntimeDefault();
            _controlProfile = _layout.DefaultControlProfile;
            _jumpMode = _layout.DefaultJumpMode;
            _cameraSensitivity = _layout.DefaultCameraSensitivity;
            _movementSensitivity = _layout.DefaultMovementSensitivity;
            _runtimeProfile = _layout.CreateRuntimeProfile(
                _controlProfile,
                _jumpMode,
                _cameraSensitivity,
                _movementSensitivity);
            _flickDetector.Configure(_layout.FlickSettings);
            return _layout;
        }

        private TapDragGestureSettings CreateTapDragSettings()
        {
            var layout = EnsureLayout();
            return new TapDragGestureSettings(
                layout.TapMaxDuration,
                layout.TapMovementTolerance,
                layout.DragActivationDistance);
        }

        private TouchJumpMode ResolveJumpModeForControlProfile(
            TouchControlProfileKind controlProfile)
        {
            switch (controlProfile)
            {
                case TouchControlProfileKind.FlowSteerAutoBalanced:
                case TouchControlProfileKind.FlowSteerAutoDirect:
                case TouchControlProfileKind.FlowSteerAutoFlow:
                    return TouchJumpMode.RightTap;
                case TouchControlProfileKind.LegacyManual:
                    return TouchJumpMode.RightTap;
                case TouchControlProfileKind.LeftMoveRightLookTapJump:
                    return TouchJumpMode.RightTap;
                case TouchControlProfileKind.LeftMoveRightLookFixedJump:
                    return TouchJumpMode.FixedButton;
                case TouchControlProfileKind.LeftMoveRightLookTapAndButton:
                    return TouchJumpMode.RightTapAndFixedButton;
                case TouchControlProfileKind.LegacyCenteredFlick:
                    return TouchJumpMode.LegacyRightFlick;
                case TouchControlProfileKind.LegacyCenteredFlickAndTap:
                    return TouchJumpMode.LegacyRightFlickAndTap;
                case TouchControlProfileKind.LegacyAutoSteer:
                case TouchControlProfileKind.LegacyStandardReversed:
                    return TouchJumpMode.FixedButton;
                default:
                    return _layout.DefaultJumpMode;
            }
        }

        private void CycleJumpModeVariant()
        {
            switch (_controlProfile)
            {
                case TouchControlProfileKind.LeftMoveRightLookTapJump:
                    _controlProfile = TouchControlProfileKind.LeftMoveRightLookFixedJump;
                    _jumpMode = TouchJumpMode.FixedButton;
                    break;
                case TouchControlProfileKind.LeftMoveRightLookFixedJump:
                    _controlProfile = TouchControlProfileKind.LeftMoveRightLookTapAndButton;
                    _jumpMode = TouchJumpMode.RightTapAndFixedButton;
                    break;
                case TouchControlProfileKind.LeftMoveRightLookTapAndButton:
                    _controlProfile = TouchControlProfileKind.LeftMoveRightLookTapJump;
                    _jumpMode = TouchJumpMode.RightTap;
                    break;
                case TouchControlProfileKind.LegacyCenteredFlick:
                    _controlProfile = TouchControlProfileKind.LegacyCenteredFlickAndTap;
                    _jumpMode = TouchJumpMode.LegacyRightFlickAndTap;
                    break;
                case TouchControlProfileKind.LegacyCenteredFlickAndTap:
                    _controlProfile = TouchControlProfileKind.LegacyCenteredFlick;
                    _jumpMode = TouchJumpMode.LegacyRightFlick;
                    break;
                case TouchControlProfileKind.FlowSteerAutoBalanced:
                case TouchControlProfileKind.FlowSteerAutoDirect:
                case TouchControlProfileKind.FlowSteerAutoFlow:
                    _jumpMode = TouchJumpMode.RightTap;
                    break;
                default:
                    _jumpMode = TouchControlRuntimeProfile.Next(_jumpMode);
                    break;
            }
        }

        private void RebuildActiveIds()
        {
            _activeTouchIds.Clear();
            AddOwner(TouchControlRole.Movement);
            AddOwner(TouchControlRole.Look);
            AddOwner(TouchControlRole.Jump);
        }

        private void AddOwner(TouchControlRole role)
        {
            var owner = _ownership.GetOwner(role);
            if (owner != TouchOwnershipRegistry.UnassignedPointerId)
            {
                _activeTouchIds.Add(owner);
            }
        }

    }
}
