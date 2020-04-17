using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace JiiLib.SimpleDsl
{
    [Flags]
    internal enum FormatModifiers
    {
        None = 0,
        Bold = 1,
        Italic = 1 << 1,
        Underline = 1 << 2
    }
    internal enum Operator
    {
        Contains,
        NotContains,
        LessThan,
        LessThanOrEqual,
        GreaterThan,
        GreaterThanOrEqual,
        IsEqual,
        NotEqual
    }

    internal static class DslHelpers
    {
        internal static Expression GenerateEqualityExpression(
            bool isTrue,
            Expression target,
            MethodInfo method,
            params Expression[] args)
        {
            var call = (method.IsStatic)
                ? Expression.Call(method, args)
                : Expression.Call(target, method, args);

            return (isTrue) ? Expression.IsTrue(call) : Expression.IsFalse(call);
        }

        internal static FormatModifiers ParseFormatModifiers(ref ReadOnlySpan<char> slice)
        {
            var fmt = FormatModifiers.None;
            if (slice.IndexOf(':') >= 0)
            {
                var prefix = slice.SliceToFirstUnnested(':', out slice);

                for (int i = 0; i < prefix.Length; i++)
                {
                    var cur = prefix[i];
                    fmt = cur switch
                    {
                        'b' => fmt |= FormatModifiers.Bold,
                        'i' => fmt |= FormatModifiers.Italic,
                        'u' => fmt |= FormatModifiers.Underline,
                        _ => throw new InvalidOperationException($"Unknown format modifier '{cur}'."),
                    };
                }
            }
            return fmt;
        }

        internal static Operator ParseOperator(ReadOnlySpan<char> opSpan)
        {
            if (opSpan.SequenceEqual(InfoCache.Contains.AsSpan()))
                return Operator.Contains;
            else if (opSpan.SequenceEqual(InfoCache.NotContains.AsSpan()))
                return Operator.NotContains;
            else if (opSpan.SequenceEqual(InfoCache.LessThan.AsSpan()))
                return Operator.LessThan;
            else if (opSpan.SequenceEqual(InfoCache.LessThanOrEqual.AsSpan()))
                return Operator.LessThanOrEqual;
            else if (opSpan.SequenceEqual(InfoCache.GreaterThan.AsSpan()))
                return Operator.GreaterThan;
            else if (opSpan.SequenceEqual(InfoCache.GreaterThanOrEqual.AsSpan()))
                return Operator.GreaterThanOrEqual;
            else if (opSpan.SequenceEqual(InfoCache.IsEqual.AsSpan()))
                return Operator.IsEqual;
            else if (opSpan.SequenceEqual(InfoCache.IsNotEqual.AsSpan()))
                return Operator.NotEqual;
            else
                throw new InvalidOperationException("Unrecognized operator");
        }

        internal static (Expression expr, bool isTruthy) CreateOperatorExpression(Expression lhs, Operator op, Expression rhs)
        {
            var lookup = QueryLookups.GetLookup(lhs.Type);

            return op switch
            {
                Operator.Contains           => (lookup.GetContainsExpression(lhs, rhs), true),
                Operator.NotContains        => (lookup.GetContainsExpression(lhs, rhs), false),
                Operator.LessThan           => (lookup.GetLessThanExpression(lhs, rhs), true),
                Operator.LessThanOrEqual    => (lookup.GetGreaterThanExpression(lhs, rhs), false),
                Operator.GreaterThan        => (lookup.GetGreaterThanExpression(lhs, rhs), true),
                Operator.GreaterThanOrEqual => (lookup.GetLessThanExpression(lhs, rhs), false),
                Operator.IsEqual            => (lookup.GetIsEqualExpression(lhs, rhs), true),
                Operator.NotEqual           => (lookup.GetIsEqualExpression(lhs, rhs), false),
                _ => throw new InvalidOperationException("Unrecognized operator"),
            };
        }

        [DebuggerStepThrough]
        internal static MemberExpression PropertyAccess(this ParameterExpression parameter, PropertyInfo property)
            => Expression.Property(parameter, property);

        internal static bool IsNegatedBool(ref ReadOnlySpan<char> span)
        {
            var res = span[0] == '!';
            span = (res) ? span.Slice(1).Trim() : span;
            return res;
        }
    }
}
