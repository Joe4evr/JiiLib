using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace JiiLib.SimpleDsl
{
    internal sealed class EnumerableOperatorLookup<T> : OperatorLookup<IEnumerable<T>>
    {
        private static readonly MethodInfo _linqAny;
        private static readonly ParameterExpression _elementExpr;
        private static readonly IOperatorLookup _baseLookup;

        static EnumerableOperatorLookup()
        {
            var elemType = typeof(T);
            _linqAny = InfoCache.LinqAny.MakeGenericMethod(elemType);
            _elementExpr = Expression.Parameter(elemType);
            _baseLookup = QueryLookups.GetLookup(elemType);
        }

        public override (BlockExpression, MethodCallExpression) GetContainsExpression(Expression lhs, Expression rhs)
        {
            var (b, m) = _baseLookup.GetIsEqualExpression(_elementExpr, rhs);
            return (InfoCache.EmptyBlock, Expression.Call(
                _linqAny,
                lhs,
                Expression.Lambda<Func<T, bool>>(
                    b.Update(b.Variables, b.Expressions.Append(m)),
                    _elementExpr)));
        }

        public override (BlockExpression, MethodCallExpression) GetGreaterThanExpression(Expression lhs, Expression rhs)
            => throw new InvalidOperationException("Greater/Less Than operations not supported on collections.");
        public override (BlockExpression, MethodCallExpression) GetLessThanExpression(Expression lhs, Expression rhs)
            => throw new InvalidOperationException("Greater/Less Than operations not supported on collections.");
        public override (BlockExpression, MethodCallExpression) GetIsEqualExpression(Expression lhs, Expression rhs)
            => throw new InvalidOperationException("Equals operations not supported on collections.");
    }
}
