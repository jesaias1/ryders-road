using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Avoidance.Core.Utilities
{
    [Serializable]
    public struct StableId : IEquatable<StableId>
    {
        private static readonly Regex Pattern =
            new Regex("^[a-z][a-z0-9]*(?:[.-][a-z0-9]+)*$", RegexOptions.Compiled);

        public StableId(string value)
        {
            if (!IsValid(value))
            {
                throw new ArgumentException($"Invalid stable ID: '{value}'.", nameof(value));
            }

            _value = value;
        }

        [SerializeField] private string _value;

        public string Value => _value;

        public static bool IsValid(string value)
        {
            return !string.IsNullOrWhiteSpace(value)
                && value.Length <= 64
                && Pattern.IsMatch(value);
        }

        public bool Equals(StableId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object obj) => obj is StableId other && Equals(other);
        public override int GetHashCode() => Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        public override string ToString() => Value ?? string.Empty;
    }
}
