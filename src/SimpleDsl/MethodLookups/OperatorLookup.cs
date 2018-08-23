using System;
using System.Linq.Expressions;

namespace JiiLib.SimpleDsl
{
    public abstract class OperatorLookup<T> : IOperatorLookup
    {
        public abstract (BlockExpression, MethodCallExpression) GetContainsExpression(Expression lhs, Expression rhs);
        public abstract (BlockExpression, MethodCallExpression) GetLessThanExpression(Expression lhs, Expression rhs);
        public abstract (BlockExpression, MethodCallExpression) GetGreaterThanExpression(Expression lhs, Expression rhs);
        public abstract (BlockExpression, MethodCallExpression) GetIsEqualExpression(Expression lhs, Expression rhs);
    }
}
