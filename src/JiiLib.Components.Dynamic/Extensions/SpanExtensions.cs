using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace JiiLib.Components.Dynamic
{
    internal static class SpanExtensions
    {
        [DebuggerStepThrough]
        internal static ReadOnlySpan<char> SliceToFirstUnnested(this ReadOnlySpan<char> span, char delimeter, out ReadOnlySpan<char> remainder)
        {
            var splitter = span.CreateSplitter(delimeter);
            splitter.TryMoveNext(out var result);
            remainder = splitter.Span;
            return result;
        }

        internal static ReadOnlySpan<char> SliceToFirstUnnestedWhitespace(this ReadOnlySpan<char> span, out ReadOnlySpan<char> remainder)
        {
            var splitter = span.CreateWhitespaceSplitter();
            splitter.TryMoveNext(out var result);
            remainder = splitter.Span;
            return result;
        }

        [DebuggerStepThrough]
        internal static UnnestedCharSpanSplitter CreateSplitter(this ReadOnlySpan<char> span, char separator)
            => new UnnestedCharSpanSplitter(span, separator);

        internal static UnnestedCharSpanSplitter CreateWhitespaceSplitter(this ReadOnlySpan<char> span)
            => new UnnestedCharSpanSplitter(span, c => Char.IsWhiteSpace(c));

        [DebuggerStepThrough]
        internal static bool ContainsUnnested(this ReadOnlySpan<char> span, char needle, out ReadOnlySpan<char> remainder)
        {
            var splitter = span.CreateSplitter(needle);
            splitter.TryMoveNext(out remainder);
            return splitter.Span.Length > 0;
        }

        [DebuggerStepThrough]
        internal static string Materialize(this ReadOnlySpan<char> span)
            => new string(span);

        [DebuggerStepThrough]
        internal static ReadOnlySpan<char> TrimBraces(this ReadOnlySpan<char> span)
        {
            for (var (i, j) = (0, span.Length); i < span.Length; (i, j) = (i + 1, j - 1))
            {
                if (j <= i)
                    break;

                var ci = span[i];
                var cj = span[j - 1];
                if (!(CharAliasMap.TryGetValue(ci, out var e) && cj == e))
                    return span[i..j].Trim();
            }
            return span;
        }

        [DebuggerStepThrough]
        internal static int FindMatchingBrace(this ReadOnlySpan<char> span, int startIdx = 0)
        {
            var braces = new Stack<(char, int)>();

            for (int i = startIdx; i < span.Length; i++)
            {
                var current = span[i];
                if (current == '\\') //skip a backslash-escaped char
                {
                    i += 1;
                    continue;
                }

                var (c, _) = (braces.Count > 0)
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
                    braces.Push((match, i));
                }
            }

            var (ch, idx) = braces.Peek();
            throw new InvalidOperationException($"No matching brace found for '{ch}' at:\n{span.Materialize()}\n{new string('-', idx)}^");
        }

        [DebuggerStepThrough]
        internal static ReadOnlySpan<char> VerifyOpenChar(this ReadOnlySpan<char> span, char c, string funcName)
        {
            if (span[0] != c)
                throw new InvalidOperationException($"Must use '{c}' after '{funcName}'.");

            return span;
        }

        //[DebuggerStepThrough]
        //internal static void AddInlineVar(this Dictionary<string, InlineVar> vars, string identifier, ParameterExpression parent, Expression invocation)
        //{
        //    if (!String.IsNullOrWhiteSpace(identifier))
        //    {
        //        if (!vars.TryAdd(identifier, new InlineVar(parent, invocation)))
        //            throw new InvalidOperationException($"Inline variable identifier '{identifier}' is already used.");
        //    }
        //}

        [DebuggerStepThrough]
        internal static bool ParseIsDescending(ref this ReadOnlySpan<char> span)
        {
            if (span.IndexOf(':') >= 0)
            {
                var prefix = span.SliceToFirstUnnested(':', out span);
                return (prefix.Length == 1 && prefix[0] == 'd');

                //for (int i = 0; i < prefix.Length; i++)
                //{
                //    var cur = prefix[i];
                //    if (cur == 'd')
                //        return true;
                //}
            }
            return false;
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


        //[DebuggerStepThrough]
        //internal static IEnumerable<(T item, int index)> AsIndexed<T>(this IList<T> items)
        //{
        //    for (int i = 0; i < items.Count; i++)
        //        yield return (items[i], i);
        //}

        //[DebuggerStepThrough]
        //internal static IEnumerable<TResult> NoCapSelect<TKey, TSource, TValue, TResult>(
        //    this IReadOnlyDictionary<TKey, Func<TSource, TValue>> source,
        //    TSource obj,
        //    Func<TKey, TValue, TResult> selector)
        //    where TKey : notnull
        //{
        //    //foreach (var (key, value) in source)
        //    //    yield return selector(key, value(obj));
        //    return source.Select(kvp => selector(kvp.Key, kvp.Value(obj)));
        //}
    }
}
