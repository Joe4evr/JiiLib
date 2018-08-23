using System;
using System.Linq.Expressions;
using System.Reflection;

namespace JiiLib.SimpleDsl
{
    internal sealed class ComparableOperatorLookup<T> : OperatorLookup<T>
        where T : IComparable<T>
    {
        private static readonly MethodInfo _compareTo = typeof(IComparable<T>).GetMethod(nameof(IComparable<T>.CompareTo));

        public override (BlockExpression, MethodCallExpression) GetLessThanExpression(Expression lhs, Expression rhs)
        {
            var intermVarExpr = Expression.Variable(InfoCache.IntType);
            return (Expression.Block(
                variables: new[] { intermVarExpr },
                expressions: new Expression[]
                {
                    Expression.Assign(intermVarExpr, Expression.Call(lhs, _compareTo, rhs))
                }),
                Expression.Call(intermVarExpr, InfoCache.IntEquals, InfoCache.IntNegOneExpr));
        }
        public override (BlockExpression, MethodCallExpression) GetGreaterThanExpression(Expression lhs, Expression rhs)
        {
            var intermVarExpr = Expression.Variable(InfoCache.IntType);
            return (Expression.Block(
                variables: new[] { intermVarExpr },
                expressions: new Expression[]
                {
                    Expression.Assign(intermVarExpr, Expression.Call(lhs, _compareTo, rhs)),
                }),
                Expression.Call(intermVarExpr, InfoCache.IntEquals, InfoCache.IntOneExpr));
        }
        public override (BlockExpression, MethodCallExpression) GetIsEqualExpression(Expression lhs, Expression rhs)
        {
            var intermVarExpr = Expression.Variable(InfoCache.IntType);
            return (Expression.Block(
                variables: new[] { intermVarExpr },
                expressions: new Expression[]
                {
                    Expression.Assign(intermVarExpr, Expression.Call(lhs, _compareTo, rhs))
                }),
                Expression.Call(intermVarExpr, InfoCache.IntEquals, InfoCache.IntZeroExpr));
        }

        public override (BlockExpression, MethodCallExpression) GetContainsExpression(Expression lhs, Expression rhs)
            => throw new InvalidOperationException($"Contains operations not supported on type '{lhs.Type}'.");
    }
}
