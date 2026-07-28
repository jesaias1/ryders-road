using System.Collections.Generic;
using Avoidance.Core.Services;

namespace Avoidance.Diagnostics
{
    public sealed class DiagnosticsService : IDiagnosticsService
    {
        private readonly SortedDictionary<string, string> _values =
            new SortedDictionary<string, string>();

        public IReadOnlyDictionary<string, string> Values => _values;

        public void SetValue(string key, string value)
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                _values[key] = value ?? string.Empty;
            }
        }

        public bool RemoveValue(string key) => _values.Remove(key);
    }
}
