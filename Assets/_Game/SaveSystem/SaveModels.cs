using System;
using Avoidance.Core.Services;

namespace Avoidance.SaveSystem
{
    [Serializable]
    public sealed class SaveDocument
    {
        public int schemaVersion = SaveSchema.CurrentVersion;
        public string gameVersion = "0.0.1";
        public GameSettings settings = new GameSettings();
        public ProgressionData progression = new ProgressionData();
    }

    [Serializable]
    public sealed class ProgressionData
    {
        // Intentionally empty in Phase 0. Fields arrive with their owning features.
    }

    public static class SaveSchema
    {
        public const int CurrentVersion = 2;
    }
}
