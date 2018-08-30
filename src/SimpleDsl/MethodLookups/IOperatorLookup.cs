using System.Linq.Expressions;

namespace JiiLib.SimpleDsl
{
    internal interface IOperatorLookup
    {
        Expression GetContainsExpression(Expression lhs, Expression rhs);
        Expression GetLessThanExpression(Expression lhs, Expression rhs);
        Expression GetGreaterThanExpression(Expression lhs, Expression rhs);
        Expression GetIsEqualExpression(Expression lhs, Expression rhs);
    }
}
