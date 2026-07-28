using System.IO;
using Avoidance.Core.Configuration;
using Avoidance.Core.FeatureFlags;
using Avoidance.Core.Services;
using Avoidance.Diagnostics;
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
        private ServiceContainer _services;
        private ISettingsService _settings;
        private ISaveService _save;
        private GameConfiguration _configuration;
        private bool _initialized;

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
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;

            _configuration = Resources.Load<GameConfiguration>("FoundationGameConfiguration")
                ?? GameConfiguration.CreateRuntimeDefault();
            _services = ComposeServices(_configuration);
            GameServices.Publish(_services);

            _settings = _services.Get<ISettingsService>();
            _settings.Load();
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
                    _configuration);
            }

            _initialized = true;
        }

        public static ServiceContainer ComposeServices(GameConfiguration configuration)
        {
            var services = new ServiceContainer();
            var settings = new PlayerPrefsSettingsService();
            var fileStore = new LocalSaveFileStore(
                Path.Combine(Application.persistentDataPath, "Saves"));
            var save = new JsonSaveService(fileStore, configuration.GameVersion);
            save.RegisterMigration(new SaveV1ToV2Migration());
            var flags = new FeatureFlagService(
                configuration.FeatureFlags,
                new PlayerPrefsFeatureFlagStore(),
                Debug.isDebugBuild || Application.isEditor);

            services.Register<ISettingsService>(settings);
            services.Register<ISaveService>(save);
            services.Register<IFeatureFlagService>(flags);
            services.Register<IDiagnosticsService>(new DiagnosticsService());
            services.Register<ILevelLoader>(new UnitySceneLevelLoader());
            return services;
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused && _initialized)
            {
                _settings.Save();
                _save.Save();
            }
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused && _initialized)
            {
                _settings.Save();
                _save.Save();
            }
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
