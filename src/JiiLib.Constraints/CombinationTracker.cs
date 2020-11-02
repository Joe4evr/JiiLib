using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace JiiLib.Constraints
{
    internal struct CombinationTracker
    {
        private Dictionary<(string, string), (Location, string, string)>? _tracker;

        public void AddCombination(string first, string second,
            Location location, string typeParamName, string parentName)
        {
            _tracker ??= new Dictionary<(string, string), (Location, string, string)>();
            if (_tracker.ContainsKey((second, first)))
                return;

            _tracker.Add((first, second), (location, typeParamName, parentName));
        }

        public IReadOnlyDictionary<(string first, string second), (Location location, string typeParamName, string parentName)> GetConflicts()
            => _tracker ?? new Dictionary<(string, string), (Location, string, string)>();
    }
}
