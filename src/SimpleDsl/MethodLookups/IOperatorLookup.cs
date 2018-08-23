using System.Linq.Expressions;

namespace JiiLib.SimpleDsl
{
    internal interface IOperatorLookup
    {
        (BlockExpression, MethodCallExpression) GetContainsExpression(Expression lhs, Expression rhs);
        (BlockExpression, MethodCallExpression) GetLessThanExpression(Expression lhs, Expression rhs);
        (BlockExpression, MethodCallExpression) GetGreaterThanExpression(Expression lhs, Expression rhs);
        (BlockExpression, MethodCallExpression) GetIsEqualExpression(Expression lhs, Expression rhs);
    }
}
