namespace Avoidance.Gameplay.Levels
{
    public static class ModuleSelectionState
    {
        public const string ModuleRunnerSceneName = "ModuleRunner";
        public const string ModuleSelectorSceneName = "ModuleSelector";
        public const string MovementLabSceneName = "MovementLab";
        public const string DefaultModuleId = "module.001.first-steps";
        public const string SpiralModuleId = "module.004.the-spiral";
        private static readonly string[] CampaignStableModuleIds =
        {
            "module.001.first-steps",
            "module.002.moving-parts",
            "module.003.flow-error"
        };

        public static string SelectedModuleId { get; private set; } = DefaultModuleId;
        public static bool DevelopmentOverride { get; private set; }

        public static string[] GetCampaignModuleIds()
        {
            return (string[])CampaignStableModuleIds.Clone();
        }

        public static string GetNextCampaignModuleId(string stableModuleId)
        {
            var index = System.Array.IndexOf(CampaignStableModuleIds, stableModuleId);
            return index >= 0 && index + 1 < CampaignStableModuleIds.Length
                ? CampaignStableModuleIds[index + 1] : null;
        }

        public static bool IsCampaignModule(string stableModuleId)
        {
            return System.Array.IndexOf(CampaignStableModuleIds, stableModuleId) >= 0;
        }

        public static bool IsSpiralModule(string stableModuleId)
        {
            return string.Equals(stableModuleId, SpiralModuleId, System.StringComparison.Ordinal);
        }

        public static void Select(string stableModuleId, bool developmentOverride = false)
        {
            SelectedModuleId = string.IsNullOrWhiteSpace(stableModuleId)
                ? DefaultModuleId
                : stableModuleId;
            DevelopmentOverride = developmentOverride;
        }
    }
}
