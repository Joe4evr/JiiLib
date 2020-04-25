using System;
using System.Diagnostics;

namespace JiiLib.Media.Internal
{
    internal ref struct SpanCharSplitter
    {
        public ReadOnlySpan<char> Span { get; private set; }
        private readonly char _separator;

        [DebuggerStepThrough]
        public SpanCharSplitter(ReadOnlySpan<char> span, char separator)
        {
            Span = span;
            _separator = separator;
        }

        [DebuggerStepThrough]
        public bool TryMoveNext(out ReadOnlySpan<char> result)
        {
            var ret = Span;
            if (ret.Length == 0)
            {
                result = ReadOnlySpan<char>.Empty;
                return false;
            }

            for (int i = 0; i < ret.Length; i++)
            {
                char current = ret[i];
                if (current == _separator && i > 0)
                {
                    int remStart = i + 1; //skip the delimeter
                    Span = ret[remStart..];
                    result = ret[0..i];
                    return true;
                }
            }

            Span = ReadOnlySpan<char>.Empty;
            result = ret;
            return true;
        }
    }
}
