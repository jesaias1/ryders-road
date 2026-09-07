using Avoidance.Core.Configuration;
using Avoidance.Core.FeatureFlags;
using Avoidance.Core.Services;
using Avoidance.Core.Utilities;
using Avoidance.Diagnostics;
using Avoidance.Gameplay.Checkpoints;
using Avoidance.Gameplay.Levels;
using Avoidance.SaveSystem;
using Avoidance.SaveSystem.Migrations;
using UnityEngine;

namespace Avoidance.Bootstrap
{
    [DefaultExecutionOrder(-10000)]
    [DisallowMultipleComponent]
    public sealed class GameBootstrap : MonoBehaviour
    {
        private static GameBootstrap _instance;
        private const float ImmersiveReapplyIntervalSeconds = 0.5f;
        private const int ImmersiveStartupReapplyCount = 14;
        private ServiceContainer _services;
        private ISettingsService _settings;
        private ISaveService _save;
        private IPlatformDisplayService _display;
        private GameConfiguration _configuration;
        private bool _initialized;
        private float _immersiveReapplyTimer;
        private int _immersiveReapplyRemaining;

        public bool IsInitialized => _initialized;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
            StartCoroutine(_services.Get<ILevelLoader>().LoadAsync(_configuration.InitialScene));
        }

        public void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            Application.targetFrameRate = 60;
            Screen.orientation = ScreenOrientation.AutoRotation;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;

            _configuration = Resources.Load<GameConfiguration>("FoundationGameConfiguration")
                ?? GameConfiguration.CreateRuntimeDefault();
            _display = new AndroidDisplayService();
            _display.RequestImmersiveMode();
            ScheduleImmersiveReapply();
            _services = ComposeServices(_configuration, _display);
            GameServices.Publish(_services);

            _settings = _services.Get<ISettingsService>();
            _settings.Load();
            AudioListener.volume = Mathf.Clamp01(_settings.Current.masterVolume);
            _save = _services.Get<ISaveService>();
            var loadResult = _save.Initialize();
            if (!loadResult.Success)
            {
                Debug.LogError(loadResult.Message);
            }
            else
            {
                Debug.Log(loadResult.Message);
            }

            if (Debug.isDebugBuild || Application.isEditor)
            {
                var overlay = gameObject.AddComponent<DiagnosticsOverlay>();
                overlay.Initialize(
                    _services.Get<IDiagnosticsService>(),
                    _services.Get<IFeatureFlagService>(),
                    _save,
                    _display,
                    _configuration);
            }

            _initialized = true;
        }

        private void Update()
        {
            ReapplyImmersiveModeIfNeeded();
        }

        public static ServiceContainer ComposeServices(
            GameConfiguration configuration,
            IPlatformDisplayService displayService = null)
        {
            var services = new ServiceContainer();
            var settings = new PlayerPrefsSettingsService();
            var saveDirectory = SavePathMigrationUtility.ResolveCurrentSaveDirectory();
            SavePathMigrationUtility.CopyLegacySaveIfCurrentMissing(
                saveDirectory,
                SavePathMigrationUtility.CandidateLegacySaveDirectories(saveDirectory));
            var fileStore = new LocalSaveFileStore(saveDirectory);
            var save = new JsonSaveService(fileStore, configuration.GameVersion);
            save.RegisterMigration(new SaveV1ToV2Migration());
            save.RegisterMigration(new SaveV2ToV3Migration());
            save.RegisterMigration(new SaveV3ToV4Migration());
            var flags = new FeatureFlagService(
                configuration.FeatureFlags,
                new PlayerPrefsFeatureFlagStore(),
                Debug.isDebugBuild || Application.isEditor);

            services.Register<ISettingsService>(settings);
            services.Register<ISaveService>(save);
            services.Register<IFeatureFlagService>(flags);
            services.Register<IDiagnosticsService>(new DiagnosticsService(
                configuration != null && configuration.DeveloperTelemetryEnabled
                    ? DiagnosticsDisplayMode.Normal
                    : DiagnosticsDisplayMode.Hidden));
            services.Register<IPlatformDisplayService>(displayService ?? new AndroidDisplayService());
            services.Register<ILevelLoader>(new UnitySceneLevelLoader());
            services.Register<ICheckpointService>(new CheckpointService());
            return services;
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused && _initialized)
            {
                _settings.Save();
                _save.Save();
            }
            else if (!paused)
            {
                _display?.SetFocusState(true);
                _display?.RequestImmersiveMode();
                ScheduleImmersiveReapply();
            }
        }

        private void OnApplicationFocus(bool focused)
        {
            _display?.SetFocusState(focused);

            if (!focused && _initialized)
            {
                _settings.Save();
                _save.Save();
            }
            else if (focused)
            {
                _display?.RequestImmersiveMode();
                ScheduleImmersiveReapply();
            }
        }

        private void ScheduleImmersiveReapply()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            _immersiveReapplyRemaining = ImmersiveStartupReapplyCount;
            _immersiveReapplyTimer = 0f;
#endif
        }

        private void ReapplyImmersiveModeIfNeeded()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (_display == null
                || !_display.HasFocus
                || _immersiveReapplyRemaining <= 0)
            {
                return;
            }

            _immersiveReapplyTimer -= Time.unscaledDeltaTime;
            if (_immersiveReapplyTimer > 0f)
            {
                return;
            }

            _display.RequestImmersiveMode();
            _immersiveReapplyRemaining--;
            _immersiveReapplyTimer = ImmersiveReapplyIntervalSeconds;
#endif
        }

        private void OnDestroy()
        {
            if (_instance != this)
            {
                return;
            }

            if (_initialized)
            {
                _settings.Save();
                _save.Save();
                GameServices.Clear(_services);
            }

            _instance = null;
        }
    }
}
