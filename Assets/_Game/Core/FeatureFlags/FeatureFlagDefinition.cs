using System;
using System.Collections.Generic;

namespace Avoidance.Core.FeatureFlags
{
    [Serializable]
    public sealed class FeatureFlagDefinition
    {
        public string _id;
        public string _displayName;
        public bool _enabledByDefault;
        public bool _allowedInRelease;

        public string Id => _id;
        public string DisplayName => _displayName;
        public bool EnabledByDefault => _enabledByDefault;
        public bool AllowedInRelease => _allowedInRelease;

        public static List<FeatureFlagDefinition> FoundationDefaults()
        {
            return new List<FeatureFlagDefinition>
            {
                Create("assisted-camera", "Assisted Camera"),
                Create("ghost-chase", "Ghost Chase"),
                Create("fatal-zone", "Fatal Zone"),
                Create("landing-assistance", "Landing Assistance"),
                Create("new-movement-model", "New Movement Model"),
                Create("experimental-ui", "Experimental UI"),
                Create("power-up-prototypes", "Power-up Prototypes")
            };
        }

        private static FeatureFlagDefinition Create(string id, string name)
        {
            return new FeatureFlagDefinition
            {
                _id = id,
                _displayName = name,
                _enabledByDefault = false,
                _allowedInRelease = false
            };
        }
    }
}
