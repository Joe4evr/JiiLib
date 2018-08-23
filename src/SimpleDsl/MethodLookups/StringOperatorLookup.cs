using System;
using System.Linq.Expressions;

namespace JiiLib.SimpleDsl
{
    internal sealed class StringOperatorLookup : OperatorLookup<string>
    {
        public static StringOperatorLookup Instance { get; } = new StringOperatorLookup();
        private StringOperatorLookup() { }

        public override (BlockExpression, MethodCallExpression) GetContainsExpression(Expression lhs, Expression rhs)
            => (EmptyBlock, Expression.Call(lhs, InfoCache.StrContains, rhs, InfoCache.StrCompsExpr));
        public override (BlockExpression, MethodCallExpression) GetIsEqualExpression(Expression lhs, Expression rhs)
            => (EmptyBlock, Expression.Call(lhs, InfoCache.StrEquals, rhs, InfoCache.StrCompsExpr));

        public override (BlockExpression, MethodCallExpression) GetGreaterThanExpression(Expression lhs, Expression rhs)
            => throw new InvalidOperationException("Greater/Less Than operations not supported on strings.");
        public override (BlockExpression, MethodCallExpression) GetLessThanExpression(Expression lhs, Expression rhs)
            => throw new InvalidOperationException("Greater/Less Than operations not supported on strings.");
    }
}
