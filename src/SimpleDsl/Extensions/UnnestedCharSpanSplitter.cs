using System;
using System.Diagnostics;

namespace JiiLib.SimpleDsl
{
    internal ref struct UnnestedCharSpanSplitter
    {
        private readonly char _separator;
        public ReadOnlySpan<char> Span { get; private set; }

        [DebuggerStepThrough]
        public UnnestedCharSpanSplitter(ReadOnlySpan<char> span, char separator)
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
                if (current == '\\') //skip a backslash-escaped char
                {
                    i += 1;
                    continue;
                }

                if (Extensions.CharAliasMap.TryGetValue(current, out var match))
                {
                    i = ret.FindMatchingBrace(i);
                    continue;
                }

                if (current == _separator && i > 0)
                {
                    int remStart = i + 1; //skip the delimeter
                    Span = ret.Slice(remStart).Trim();
                    result = ret.Slice(0, i).Trim();
                    return true;
                }
            }

            Span = ReadOnlySpan<char>.Empty;
            result = ret;
            return true;
        }
    }
}
