using System;
using System.Linq.Expressions;
using System.Reflection;

namespace JiiLib.SimpleDsl
{
    internal sealed class ComparableOperatorLookup<T> : OperatorLookup<T>
        where T : IComparable<T>
    {
        private static readonly MethodInfo _compare = typeof(ComparableOperatorLookup<T>).GetMethod(nameof(ComparableOperatorLookup<T>.Compare));

        public override Expression GetLessThanExpression(Expression lhs, Expression rhs)
            => CreateCompareBlock(lhs, rhs, InfoCache.IntNegOneExpr);
        public override Expression GetGreaterThanExpression(Expression lhs, Expression rhs)
            => CreateCompareBlock(lhs, rhs, InfoCache.IntOneExpr);
        public override Expression GetIsEqualExpression(Expression lhs, Expression rhs)
            => CreateCompareBlock(lhs, rhs, InfoCache.IntZeroExpr);

        public override Expression GetContainsExpression(Expression lhs, Expression rhs)
            => throw new InvalidOperationException($"Contains operations not supported on type '{lhs.Type}'.");

        private static BlockExpression CreateCompareBlock(
            Expression lhs,
            Expression rhs,
            ConstantExpression resultCheck)
        {
            var varExpr = Expression.Variable(InfoCache.IntType, "compareResult");
            var vars = (lhs is ParameterExpression paramLhs)
                ? new[] { varExpr, paramLhs }
                : new[] { varExpr };
            //var vars = new[] { varExpr };

            return Expression.Block(
                variables: vars,
                expressions: new Expression[]
                {
                    Expression.Assign(varExpr,
                        Expression.Call(_compare, lhs, rhs)),
                    //compareResult.Equals(resultCheck);
                    Expression.Call(varExpr, InfoCache.IntEquals, resultCheck)
                });
        }

        public static int Compare(T lhs, T rhs)
            => lhs.CompareTo(rhs);
    }
}
