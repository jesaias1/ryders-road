using System.Collections.Generic;
using System.Linq;
using Avoidance.Core.Configuration;
using Avoidance.Core.FeatureFlags;
using Avoidance.Core.Services;
using Avoidance.SaveSystem;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Avoidance.Diagnostics
{
    [DisallowMultipleComponent]
    public sealed class DiagnosticsOverlay : MonoBehaviour
    {
        private const int FpsWindow = 120;
        private readonly Queue<float> _frameRates = new Queue<float>(FpsWindow);
        private IDiagnosticsService _diagnostics;
        private IFeatureFlagService _featureFlags;
        private ISaveService _saveService;
        private GameConfiguration _configuration;
        private GUIStyle _style;
        private bool _visible = true;
        private float _currentFps;
        private float _averageFps;
        private float _minimumFps;

        public void Initialize(
            IDiagnosticsService diagnostics,
            IFeatureFlagService featureFlags,
            ISaveService saveService,
            GameConfiguration configuration)
        {
            _diagnostics = diagnostics;
            _featureFlags = featureFlags;
            _saveService = saveService;
            _configuration = configuration;
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame)
            {
                _visible = !_visible;
            }

            var unscaledDelta = Time.unscaledDeltaTime;
            if (unscaledDelta <= 0f)
            {
                return;
            }

            _currentFps = 1f / unscaledDelta;
            _frameRates.Enqueue(_currentFps);
            while (_frameRates.Count > FpsWindow)
            {
                _frameRates.Dequeue();
            }

            _averageFps = _frameRates.Average();
            _minimumFps = _frameRates.Min();
        }

        private void OnGUI()
        {
            if (!_visible || (!Debug.isDebugBuild && !Application.isEditor))
            {
                return;
            }

            _style ??= new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = Mathf.Clamp(Screen.height / 48, 14, 24),
                normal = { textColor = Color.white },
                padding = new RectOffset(14, 14, 12, 12)
            };

            var safe = Screen.safeArea;
            var activeFlags = _featureFlags == null
                ? "unavailable"
                : string.Join(", ", _featureFlags.Definitions
                    .Where(flag => _featureFlags.IsEnabled(flag.Id))
                    .Select(flag => flag.Id)
                    .DefaultIfEmpty("none"));
            var text =
                $"Game: {_configuration?.GameVersion ?? Application.version}\n" +
                $"Build: {_configuration?.BuildVersion ?? "unknown"}\n" +
                $"Unity: {Application.unityVersion}\n" +
                $"Scene: {SceneManager.GetActiveScene().name}\n" +
                $"Resolution: {Screen.width} x {Screen.height}\n" +
                $"Safe area: {safe.x:0},{safe.y:0} {safe.width:0}x{safe.height:0}\n" +
                $"Orientation: {Screen.orientation}\n" +
                $"FPS: {_currentFps:0} / avg {_averageFps:0.0} / min {_minimumFps:0.0}\n" +
                $"Feature flags: {activeFlags}\n" +
                $"Save schema: {_saveService?.SchemaVersion.ToString() ?? "unavailable"}";

            if (_diagnostics != null)
            {
                foreach (var pair in _diagnostics.Values)
                {
                    text += $"\n{pair.Key}: {pair.Value}";
                }
            }

            var width = Mathf.Min(Screen.width * 0.62f, 780f);
            var content = new GUIContent(text);
            var height = _style.CalcHeight(content, width);
            var topInset = Screen.height - safe.yMax;
            GUI.Box(
                new Rect(safe.xMin + 16f, topInset + 16f, width, height),
                content,
                _style);
        }
    }
}
