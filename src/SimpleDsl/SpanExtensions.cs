using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace JiiLib.SimpleDsl
{
    internal static class SpanExtensions
    {
        [DebuggerStepThrough]
        public static ReadOnlySpan<char> SliceUntil(this ReadOnlySpan<char> span, char delimeter, out ReadOnlySpan<char> remainder)
        {
            var ret = span;
            for (int i = 0; i < span.Length; i++)
            {
                if (span[i] == delimeter && i > 0)
                {
                    int remStart = i + 1; //skip the delimeter
                    remainder = span.Slice(remStart, span.Length - remStart).Trim();
                    return span.Slice(0, i).Trim();
                }
            }
            remainder = ReadOnlySpan<char>.Empty;
            return ret;
        }

        [DebuggerStepThrough]
        public static string Materialize(this ReadOnlySpan<char> span)
            => new string(span.ToArray());

        [DebuggerStepThrough]
        public static ReadOnlySpan<char> TrimBraces(this ReadOnlySpan<char> span)
            => span.TrimStart(_startChars.AsSpan()).TrimEnd(_endChars.AsSpan());

        [DebuggerStepThrough]
        public static int FindMatchingBrace(this ReadOnlySpan<char> span)
        {
            var braces = new Stack<char>();

            for (int i = 0; i < span.Length; i++)
            {
                var current = span[i];
                var c = (braces.Count > 0)
                    ? braces.Peek()
                    : default;

                if (c == current)
                {
                    braces.Pop();
                    if (braces.Count == 0)
                        return i;
                }
                else if (CharAliasMap.TryGetValue(current, out var match))
                {
                    braces.Push(match);
                }
            }

            throw new InvalidOperationException();
        }

        // Output of a gist provided by https://gist.github.com/ufcpp
        // https://gist.github.com/ufcpp/5b2cf9a9bf7d0b8743714a0b88f7edc5
        internal static readonly IReadOnlyDictionary<char, char> CharAliasMap
            = new Dictionary<char, char> {
                    {'\"', '\"' },
                    {'[', ']' },
                    {'{', '}' },
                    {'«', '»' },
                    {'‘', '’' },
                    {'“', '”' },
                    {'„', '‟' },
                    {'‹', '›' },
                    {'‚', '‛' },
                    {'《', '》' },
                    {'〈', '〉' },
                    {'「', '」' },
                    {'『', '』' },
                    {'〝', '〞' },
                    {'﹁', '﹂' },
                    {'﹃', '﹄' },
                    {'＂', '＂' },
                    {'＇', '＇' },
                    {'｢', '｣' },
                    {'(', ')' },
                    {'༺', '༻' },
                    {'༼', '༽' },
                    {'᚛', '᚜' },
                    {'⁅', '⁆' },
                    {'⌈', '⌉' },
                    {'⌊', '⌋' },
                    {'❨', '❩' },
                    {'❪', '❫' },
                    {'❬', '❭' },
                    {'❮', '❯' },
                    {'❰', '❱' },
                    {'❲', '❳' },
                    {'❴', '❵' },
                    {'⟅', '⟆' },
                    {'⟦', '⟧' },
                    {'⟨', '⟩' },
                    {'⟪', '⟫' },
                    {'⟬', '⟭' },
                    {'⟮', '⟯' },
                    {'⦃', '⦄' },
                    {'⦅', '⦆' },
                    {'⦇', '⦈' },
                    {'⦉', '⦊' },
                    {'⦋', '⦌' },
                    {'⦍', '⦎' },
                    {'⦏', '⦐' },
                    {'⦑', '⦒' },
                    {'⦓', '⦔' },
                    {'⦕', '⦖' },
                    {'⦗', '⦘' },
                    {'⧘', '⧙' },
                    {'⧚', '⧛' },
                    {'⧼', '⧽' },
                    {'⸂', '⸃' },
                    {'⸄', '⸅' },
                    {'⸉', '⸊' },
                    {'⸌', '⸍' },
                    {'⸜', '⸝' },
                    {'⸠', '⸡' },
                    {'⸢', '⸣' },
                    {'⸤', '⸥' },
                    {'⸦', '⸧' },
                    {'⸨', '⸩' },
                    {'【', '】'},
                    {'〔', '〕' },
                    {'〖', '〗' },
                    {'〘', '〙' },
                    {'〚', '〛' }
                };

        private static readonly string _startChars = new string(CharAliasMap.Keys.ToArray());
        private static readonly string _endChars = new string(CharAliasMap.Values.ToArray());
    }
}
