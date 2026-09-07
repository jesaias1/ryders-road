using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avoidance.Core.Configuration;
using Avoidance.Core.FeatureFlags;
using Avoidance.Core.Services;
using Avoidance.SaveSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Avoidance.Diagnostics
{
    [DisallowMultipleComponent]
    public sealed class DiagnosticsOverlay : MonoBehaviour
    {
        private const string PlayerFacingTitle = "RYDER'S ROAD";
        private const int FpsWindow = 120;
        private readonly Queue<float> _frameRates = new Queue<float>(FpsWindow);
        private IDiagnosticsService _diagnostics;
        private IFeatureFlagService _featureFlags;
        private ISaveService _saveService;
        private IPlatformDisplayService _displayService;
        private GameConfiguration _configuration;
        private GUIStyle _normalStyle;
        private GUIStyle _fullStyle;
        private Texture2D _background;
        private readonly StringBuilder _text = new StringBuilder(1024);
        private Vector2 _scroll;
        private float _currentFps;
        private float _averageFps;
        private float _minimumFps;

        public void Initialize(
            IDiagnosticsService diagnostics,
            IFeatureFlagService featureFlags,
            ISaveService saveService,
            IPlatformDisplayService displayService,
            GameConfiguration configuration)
        {
            _diagnostics = diagnostics;
            _featureFlags = featureFlags;
            _saveService = saveService;
            _displayService = displayService;
            _configuration = configuration;
        }

        private void Update()
        {
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
            var mode = _diagnostics?.DisplayMode ?? DiagnosticsDisplayMode.Normal;
            if (mode == DiagnosticsDisplayMode.Hidden
                || (!Debug.isDebugBuild && !Application.isEditor))
            {
                return;
            }

            EnsureStyles();

            var safe = Screen.safeArea;
            var topInset = Screen.height - safe.yMax;
            if (mode == DiagnosticsDisplayMode.Normal)
            {
                DrawNormal(safe, topInset);
                return;
            }

            DrawFull(safe, topInset);
        }

        private void DrawNormal(Rect safe, float topInset)
        {
            _text.Clear();
            _text.Append(_configuration?.GameTitle ?? PlayerFacingTitle);
            _text.Append(' ');
            _text.AppendLine(_configuration?.BuildVersion ?? "unknown");
            _text.Append("FPS ");
            _text.Append(_currentFps.ToString("0"));
            _text.Append("  SPD ");
            _text.Append(GetDiagnostic("Horizontal speed", "0.00"));
            _text.Append("  ");
            _text.AppendLine(GetDiagnostic("Movement profile", "Default"));
            _text.Append("Ground ");
            _text.Append(GetDiagnostic("Grounded", "False"));

            var width = Mathf.Min(Screen.width * 0.36f, 420f);
            var content = new GUIContent(_text.ToString());
            var height = _normalStyle.CalcHeight(content, width);
            GUI.Box(
                new Rect(safe.xMin + 12f, topInset + 12f, width, height),
                content,
                _normalStyle);
        }

        private void DrawFull(Rect safe, float topInset)
        {
            var activeFlags = _featureFlags == null
                ? "unavailable"
                : string.Join(", ", _featureFlags.Definitions
                    .Where(flag => _featureFlags.IsEnabled(flag.Id))
                    .Select(flag => flag.Id)
                    .DefaultIfEmpty("none"));

            _text.Clear();
            _text.AppendLine($"Game: {_configuration?.GameTitle ?? Application.productName}");
            _text.AppendLine($"Version: {_configuration?.GameVersion ?? Application.version}");
            _text.AppendLine($"Build: {_configuration?.BuildVersion ?? "unknown"}");
            _text.AppendLine($"Unity: {Application.unityVersion}");
            _text.AppendLine($"Scene: {SceneManager.GetActiveScene().name}");
            _text.AppendLine($"Resolution: {Screen.width} x {Screen.height}");
            _text.AppendLine($"Safe area: {safe.x:0},{safe.y:0} {safe.width:0}x{safe.height:0}");
            _text.AppendLine($"Orientation: {Screen.orientation}");
            _text.AppendLine($"Fullscreen: {Screen.fullScreen}");
            _text.AppendLine($"Display: {_displayService?.DisplaySummary ?? "unavailable"}");
            _text.AppendLine($"Immersive requested: {_displayService?.ImmersiveRequested.ToString() ?? "unavailable"}");
            _text.AppendLine($"App focus: {_displayService?.HasFocus.ToString() ?? "unavailable"}");
            _text.AppendLine($"Android API: {_displayService?.AndroidApiLevel.ToString() ?? "unavailable"}");
            _text.AppendLine($"FPS: {_currentFps:0} / avg {_averageFps:0.0} / min {_minimumFps:0.0}");
            _text.AppendLine($"Feature flags: {activeFlags}");
            _text.AppendLine($"Save schema: {_saveService?.SchemaVersion.ToString() ?? "unavailable"}");

            if (_diagnostics != null)
            {
                foreach (var pair in _diagnostics.Values)
                {
                    _text.AppendLine($"{pair.Key}: {pair.Value}");
                }
            }

            var width = Mathf.Min(Screen.width * 0.52f, 760f);
            var outer = new Rect(
                safe.xMin + 12f,
                topInset + 12f,
                width,
                Mathf.Min(safe.height * 0.72f, 720f));
            GUI.Box(outer, GUIContent.none, _fullStyle);

            var inner = new Rect(outer.x + 12f, outer.y + 12f, outer.width - 24f, outer.height - 24f);
            var content = new GUIContent(_text.ToString());
            var contentHeight = _fullStyle.CalcHeight(content, inner.width - 18f);
            _scroll = GUI.BeginScrollView(
                inner,
                _scroll,
                new Rect(0f, 0f, inner.width - 18f, contentHeight));
            GUI.Label(
                new Rect(0f, 0f, inner.width - 18f, contentHeight),
                content,
                _fullStyle);
            GUI.EndScrollView();
        }

        private string GetDiagnostic(string key, string fallback)
        {
            if (_diagnostics != null && _diagnostics.Values.TryGetValue(key, out var value))
            {
                return value;
            }

            return fallback;
        }

        private void EnsureStyles()
        {
            if (_background == null)
            {
                _background = new Texture2D(1, 1)
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                _background.SetPixel(0, 0, new Color(0.015f, 0.022f, 0.04f, 0.72f));
                _background.Apply();
            }

            _normalStyle ??= new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = Mathf.Clamp(Screen.height / 64, 12, 18),
                normal = { textColor = Color.white, background = _background },
                padding = new RectOffset(10, 10, 8, 8),
                wordWrap = true
            };

            _fullStyle ??= new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = Mathf.Clamp(Screen.height / 58, 13, 19),
                normal = { textColor = Color.white, background = _background },
                padding = new RectOffset(10, 10, 8, 8),
                wordWrap = true
            };
        }
    }
}
