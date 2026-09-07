using System.Collections.Generic;
using Avoidance.Core.Services;

namespace Avoidance.Diagnostics
{
    public sealed class DiagnosticsService : IDiagnosticsService
    {
        private readonly SortedDictionary<string, string> _values =
            new SortedDictionary<string, string>();

        public IReadOnlyDictionary<string, string> Values => _values;
        public DiagnosticsDisplayMode DisplayMode { get; private set; }

        public DiagnosticsService(
            DiagnosticsDisplayMode initialMode = DiagnosticsDisplayMode.Normal)
        {
            DisplayMode = initialMode;
        }

        public void SetValue(string key, string value)
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                _values[key] = value ?? string.Empty;
            }
        }

        public bool RemoveValue(string key) => _values.Remove(key);

        public void SetDisplayMode(DiagnosticsDisplayMode mode)
        {
            DisplayMode = mode;
        }

        public void CycleDisplayMode()
        {
            DisplayMode = DisplayMode switch
            {
                DiagnosticsDisplayMode.Hidden => DiagnosticsDisplayMode.Normal,
                DiagnosticsDisplayMode.Normal => DiagnosticsDisplayMode.Full,
                _ => DiagnosticsDisplayMode.Hidden
            };
        }
    }
}
