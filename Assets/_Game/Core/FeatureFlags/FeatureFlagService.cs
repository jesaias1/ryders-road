using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Avoidance.Core.FeatureFlags
{
    public interface IFeatureFlagStore
    {
        bool TryRead(string id, out bool enabled);
        void Write(string id, bool enabled);
    }

    public interface IFeatureFlagService
    {
        IReadOnlyList<FeatureFlagDefinition> Definitions { get; }
        bool IsEnabled(string id);
        bool TrySetEnabled(string id, bool enabled);
    }

    public sealed class FeatureFlagService : IFeatureFlagService
    {
        private readonly Dictionary<string, FeatureFlagDefinition> _definitions;
        private readonly IFeatureFlagStore _store;
        private readonly bool _isDevelopment;

        public FeatureFlagService(
            IEnumerable<FeatureFlagDefinition> definitions,
            IFeatureFlagStore store,
            bool isDevelopment)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _isDevelopment = isDevelopment;
            _definitions = (definitions ?? Enumerable.Empty<FeatureFlagDefinition>())
                .Where(definition => definition != null && !string.IsNullOrWhiteSpace(definition.Id))
                .GroupBy(definition => definition.Id, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
            Definitions = _definitions.Values.OrderBy(definition => definition.Id).ToArray();
        }

        public IReadOnlyList<FeatureFlagDefinition> Definitions { get; }

        public bool IsEnabled(string id)
        {
            if (!_definitions.TryGetValue(id, out var definition))
            {
                return false;
            }

            if (!_isDevelopment && !definition.AllowedInRelease)
            {
                return false;
            }

            return _store.TryRead(id, out var enabled) ? enabled : definition.EnabledByDefault;
        }

        public bool TrySetEnabled(string id, bool enabled)
        {
            if (!_isDevelopment || !_definitions.ContainsKey(id))
            {
                return false;
            }

            _store.Write(id, enabled);
            return true;
        }
    }

    public sealed class PlayerPrefsFeatureFlagStore : IFeatureFlagStore
    {
        private const string Prefix = "feature-flag.";

        public bool TryRead(string id, out bool enabled)
        {
            var key = Prefix + id;
            if (!PlayerPrefs.HasKey(key))
            {
                enabled = false;
                return false;
            }

            enabled = PlayerPrefs.GetInt(key) != 0;
            return true;
        }

        public void Write(string id, bool enabled)
        {
            PlayerPrefs.SetInt(Prefix + id, enabled ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
