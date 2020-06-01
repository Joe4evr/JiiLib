using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

namespace JiiLib.Components.Dynamic
{
    public sealed class Node
    {
        public string? Tag { get; }
        public string? Contents { get; }

        public bool? IsSelfClosing { get; }

        public IReadOnlyDictionary<string, string?> Attributes { get; }

        public IReadOnlyCollection<Node> SubNodes { get; }

        private bool _isVisited = false;

        public bool ShouldRender()
        {
            if (!_isVisited)
            {
                _isVisited = true;
                return true;
            }
            return false;
        }

        [DebuggerStepThrough]
        internal Node(string? tag, string? contents, bool? selfClosing = null,
            IReadOnlyDictionary<string, string?>? attributes = null,
            IReadOnlyCollection<Node>? subNodes = null)
        {
            if (tag != null && !selfClosing.HasValue)
                throw new ArgumentException($"'{nameof(selfClosing)}' must have a value if '{nameof(tag)}' has a value");

            Tag = tag;
            Contents = contents;
            IsSelfClosing = selfClosing;
            Attributes = (attributes is null)
                ? _emptyAttrs
                : ImmutableDictionary.CreateRange(attributes);
            SubNodes = subNodes ?? Array.Empty<Node>();
        }

        private static readonly IReadOnlyDictionary<string, string?> _emptyAttrs
            = ImmutableDictionary<string, string?>.Empty;
    }
}
