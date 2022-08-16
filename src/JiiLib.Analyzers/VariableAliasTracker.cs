using System;
using System.Diagnostics.CodeAnalysis;

namespace JiiLib.Analyzers;

internal readonly struct VariableAliasTracker
{
    private readonly Dictionary<string, string> _aliases = new(StringComparer.Ordinal);
    private readonly HashSet<string> _filter;

    public VariableAliasTracker(IEnumerable<string> filter)
        : this()
    {
        _filter = new(filter, StringComparer.Ordinal);
    }

    public int FilterCount => _filter.Count;

    public void SetAlias(string alias, string referred) => _aliases[alias] = referred;

    public bool Contains(string item, [NotNullWhen(returnValue: true)] out string? found)
    {
        if (_aliases.TryGetValue(item, out var referred))
        {
            return Contains(referred, out found);
        }

        found = item;
        return _filter.Contains(item);
    }
}
