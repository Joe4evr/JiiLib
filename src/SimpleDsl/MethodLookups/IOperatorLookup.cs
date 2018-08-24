using System.Linq.Expressions;

namespace JiiLib.SimpleDsl
{
    internal interface IOperatorLookup
    {
        (BlockExpression, Expression) GetContainsExpression(Expression lhs, Expression rhs);
        (BlockExpression, Expression) GetLessThanExpression(Expression lhs, Expression rhs);
        (BlockExpression, Expression) GetGreaterThanExpression(Expression lhs, Expression rhs);
        (BlockExpression, Expression) GetIsEqualExpression(Expression lhs, Expression rhs);
    }
}
