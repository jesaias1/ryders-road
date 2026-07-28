using System;
using System.Collections.Generic;
using Avoidance.Core.FeatureFlags;
using UnityEngine;

namespace Avoidance.Core.Configuration
{
    [CreateAssetMenu(menuName = "Avoidance/Foundation/Game Configuration", fileName = "FoundationGameConfiguration")]
    public sealed class GameConfiguration : ScriptableObject
    {
        [SerializeField] private string _gameVersion = "0.0.1";
        [SerializeField] private string _buildVersion = "0.0.1-foundation";
        [SerializeField] private string _initialScene = "FoundationTest";
        [SerializeField] private List<FeatureFlagDefinition> _featureFlags = new List<FeatureFlagDefinition>();

        public string GameVersion => _gameVersion;
        public string BuildVersion => _buildVersion;
        public string InitialScene => _initialScene;
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
