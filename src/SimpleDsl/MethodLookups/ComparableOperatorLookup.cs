using System;
using System.Linq.Expressions;
using System.Reflection;

namespace JiiLib.SimpleDsl
{
    internal sealed class ComparableOperatorLookup<T> : OperatorLookup<T>
        where T : IComparable<T>
    {
        private static readonly MethodInfo _compareTo = typeof(IComparable<T>).GetMethod(nameof(IComparable<T>.CompareTo));

        public override (BlockExpression, Expression) GetLessThanExpression(Expression lhs, Expression rhs)
        {
            var intermVarExpr = Expression.Variable(InfoCache.IntType);
            return (CreateCompareBlock(intermVarExpr, lhs, rhs),
                //interimVar.Equals(-1);
                Expression.Call(intermVarExpr, InfoCache.IntEquals, InfoCache.IntNegOneExpr));
        }
        public override (BlockExpression, Expression) GetGreaterThanExpression(Expression lhs, Expression rhs)
        {
            var intermVarExpr = Expression.Variable(InfoCache.IntType);
            return (CreateCompareBlock(intermVarExpr, lhs, rhs),
                //interimVar.Equals(1);
                Expression.Call(intermVarExpr, InfoCache.IntEquals, InfoCache.IntOneExpr));
        }
        public override (BlockExpression, Expression) GetIsEqualExpression(Expression lhs, Expression rhs)
        {
            var intermVarExpr = Expression.Variable(InfoCache.IntType);
            return (CreateCompareBlock(intermVarExpr, lhs, rhs),
                //interimVar.Equals(0);
                Expression.Call(intermVarExpr, InfoCache.IntEquals, InfoCache.IntZeroExpr));
        }

        public override (BlockExpression, Expression) GetContainsExpression(Expression lhs, Expression rhs)
            => throw new InvalidOperationException($"Contains operations not supported on type '{lhs.Type}'.");

        private static BlockExpression CreateCompareBlock(ParameterExpression varExpr, Expression lhs, Expression rhs)
            => Expression.Block(
                variables: new[] { varExpr },
                expressions: new Expression[]
                {
                    //int interimVar = lhs.CompareTo(rhs);
                    Expression.Assign(varExpr, Expression.Call(lhs, _compareTo, rhs))
                });
    }
}
