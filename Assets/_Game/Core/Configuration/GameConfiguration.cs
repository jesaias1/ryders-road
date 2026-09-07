using System;
using System.Collections.Generic;
using Avoidance.Core.FeatureFlags;
using UnityEngine;

namespace Avoidance.Core.Configuration
{
    [CreateAssetMenu(menuName = "RYDERS BLOCK/Project Configuration", fileName = "FoundationGameConfiguration")]
    public sealed class GameConfiguration : ScriptableObject
    {
        [SerializeField] private string _gameTitle = "RYDER'S ROAD";
        [SerializeField] private string _gameVersion = "0.5.0";
        [SerializeField] private string _buildVersion = "0.7.9-module003-first-finished-level";
        [SerializeField] private string _initialScene = "ModuleSelector";
        [SerializeField] private bool _developerTelemetryEnabled;
        [SerializeField] private List<FeatureFlagDefinition> _featureFlags = new List<FeatureFlagDefinition>();

        public string GameTitle => _gameTitle;
        public string GameVersion => _gameVersion;
        public string BuildVersion => _buildVersion;
        public string InitialScene => _initialScene;
        public bool DeveloperTelemetryEnabled => _developerTelemetryEnabled;
        public IReadOnlyList<FeatureFlagDefinition> FeatureFlags => _featureFlags;

        public static GameConfiguration CreateRuntimeDefault()
        {
            var configuration = CreateInstance<GameConfiguration>();
            configuration.hideFlags = HideFlags.DontSave;
            configuration._featureFlags = FeatureFlagDefinition.FoundationDefaults();
            return configuration;
        }
    }
}
