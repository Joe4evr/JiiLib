using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace JiiLib.SimpleDsl
{
    internal static class Extensions
    {
        [DebuggerStepThrough]
        internal static ReadOnlySpan<char> SliceUntilFirstUnnested(this ReadOnlySpan<char> span, char delimeter, out ReadOnlySpan<char> remainder)
        {
            var ret = span;
            for (int i = 0; i < span.Length; i++)
            {
                char current = span[i];
                if (CharAliasMap.TryGetValue(current, out var match))
                {
                    i = span.FindMatchingBrace(i);
                    continue;
                }

                if (current == delimeter && i > 0)
                {
                    int remStart = i + 1; //skip the delimeter
                    remainder = span.Slice(remStart).Trim();
                    return span.Slice(0, i).Trim();
                }
            }
            remainder = ReadOnlySpan<char>.Empty;
            return ret;
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
                    return span.Slice(i, j - i).Trim();
            }
            return span;
        }

        [DebuggerStepThrough]
        internal static int FindMatchingBrace(this ReadOnlySpan<char> span, int startIdx = 0)
        {
            var braces = new Stack<char>();

            for (int i = startIdx; i < span.Length; i++)
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

            throw new InvalidOperationException($"No matching brace found for '{braces.Peek()}'");
        }

        [DebuggerStepThrough]
        internal static ReadOnlySpan<char> VerifyOpenChar(this ReadOnlySpan<char> span, char c, string name)
        {
            if (span[0] != c)
                throw new InvalidOperationException($"Must use '{c}' after '{name}'.");

            return span;
        }

        [DebuggerStepThrough]
        internal static void AddInlineVar(this Dictionary<string, Expression> vars, string identifier, Expression invocation)
        {
            if (identifier != null)
            {
                if (!vars.TryAdd(identifier, invocation))
                    throw new InvalidOperationException($"Inline variable identifier '{identifier}' is already used.");
            }
        }

        [DebuggerStepThrough]
        internal static bool ParseIsDescending(ref this ReadOnlySpan<char> span)
        {
            if (span.IndexOf(':') >= 0)
            {
                var prefix = span.SliceUntilFirstUnnested(':', out span);
                for (int i = 0; i < prefix.Length; i++)
                {
                    var cur = prefix[i];
                    if (cur == 'd')
                        return true;
                }
            }
            return false;
        }

        // Output of a gist provided by https://gist.github.com/ufcpp
        // https://gist.github.com/ufcpp/5b2cf9a9bf7d0b8743714a0b88f7edc5
        private static readonly IReadOnlyDictionary<char, char> CharAliasMap
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

        [DebuggerStepThrough]
        internal static bool IsCollectionType(this Type type, out Type elementType)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == InfoCache.IEnumOpenType)
            {
                elementType = type.GetGenericArguments()[0];
                return true;
            }
            elementType = type;
            return false;
        }

        [DebuggerStepThrough]
        internal static bool IsNullableStruct(this Type type, out Type elementType)
        {
            if (type.IsValueType)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == InfoCache.NullableOpenType)
                {
                    elementType = type.GetGenericArguments()[0];
                    return true;
                }
            }
            elementType = type;
            return false;
        }

        [DebuggerStepThrough]
        internal static Expression Stringify(this Expression objExpr)
            => (objExpr.Type == InfoCache.StrType)
                ? objExpr
                //-> obj.ToString();
                : Expression.Call(objExpr, InfoCache.ObjToString);
    }
}
