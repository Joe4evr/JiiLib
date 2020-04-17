using System;
using System.Linq.Expressions;
using System.Reflection;

namespace JiiLib.SimpleDsl
{
    internal sealed class StringOperatorLookup : OperatorLookup<string>
    {
        public static StringOperatorLookup Instance { get; } = new StringOperatorLookup();

        private static readonly MethodInfo _contains = typeof(StringOperatorLookup).GetMethod(nameof(Contains))!;

        private StringOperatorLookup() { }

        public override Expression GetContainsExpression(Expression lhs, Expression rhs)
            //Contains(lhs, rhs);
            => Expression.Call(_contains, lhs, rhs);
        public override Expression GetIsEqualExpression(Expression lhs, Expression rhs)
            //String.Equals(lhs, rhs, StringComparison.OrdinalIgnoreCase);
            => Expression.Call(InfoCache.StrEquals, lhs, rhs, InfoCache.StrCompsExpr);

        public override Expression GetGreaterThanExpression(Expression lhs, Expression rhs)
            => throw new InvalidOperationException("Greater/Less Than operations not supported on strings.");
        public override Expression GetLessThanExpression(Expression lhs, Expression rhs)
            => throw new InvalidOperationException("Greater/Less Than operations not supported on strings.");

        public static bool Contains(string source, string sub)
            => source?.Contains(sub, StringComparison.OrdinalIgnoreCase) ?? false;
    }
}
