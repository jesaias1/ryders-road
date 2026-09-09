using System;
using System.Collections.Generic;

namespace Avoidance.Gameplay.Levels
{
    public enum CampaignTrialMode { None, Accepted, FlowManual, FlowLanding, PreviousFlow, Foundation }

    // Session-only evaluation on real modules. Never a Campaign preference or save record.
    public static class CampaignFlowTrial
    {
        private static readonly Dictionary<string, double> BestTimes = new Dictionary<string, double>();
        public static CampaignTrialMode Mode { get; private set; }
        public static bool Active => Mode != CampaignTrialMode.None;
        public static bool UsesCandidate => Mode == CampaignTrialMode.Foundation || Mode == CampaignTrialMode.FlowManual || Mode == CampaignTrialMode.FlowLanding || Mode == CampaignTrialMode.PreviousFlow;
        public static string MovementResource => Mode == CampaignTrialMode.Foundation ? "Training/Movement_Foundation" : !UsesCandidate ? "MovementProfiles/Movement_Default"
            : Mode == CampaignTrialMode.PreviousFlow ? "Training/Movement_Mastery" : "Training/Movement_RealRoute";
        public static string Label => Mode == CampaignTrialMode.Foundation ? "FOUNDATION / HOLD JUMP + LOOK" : Mode == CampaignTrialMode.Accepted ? "ACCEPTED CONTROLS"
            : Mode == CampaignTrialMode.PreviousFlow ? "PREVIOUS FLOW 0.9.9"
            : Mode == CampaignTrialMode.FlowManual ? "ROUTE FLOW / MANUAL VIEW" : "ROUTE FLOW / LANDING VIEW";

        public static void Launch(string moduleId, CampaignTrialMode mode)
        {
            if (!ModuleSelectionState.IsCampaignModule(moduleId) || mode == CampaignTrialMode.None)
                throw new ArgumentException("A trial requires a Campaign road and comparison mode.");
            ModuleSelectionState.Select(moduleId, true);
            Mode = mode;
        }

        public static void Clear() { Mode = CampaignTrialMode.None; }
        private static string Key(ModuleDefinition module) => module.StableModuleId + ":" + module.ContentVersion + ":" + Mode;
        public static double Best(ModuleDefinition module) => BestTimes.TryGetValue(Key(module), out var time) ? time : 0d;
        public static void Complete(ModuleDefinition module, double seconds)
        {
            if (!Active || seconds <= 0d) return;
            var best = Best(module);
            if (best <= 0d || seconds < best) BestTimes[Key(module)] = seconds;
        }
    }
}
