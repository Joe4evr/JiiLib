using System;
using System.Linq.Expressions;

namespace JiiLib.SimpleDsl
{
    public sealed partial class QueryInterpreter<T>
    {
        private enum Operator
        {
            Contains,
            NotContains,
            LessThan,
            LessThanOrEqual,
            GreaterThan,
            GreaterThanOrEqual,
            IsEqual,
            NotEqual
        }

        private static Operator ParseOperator(ReadOnlySpan<char> opSpan)
        {
            if (opSpan.SequenceEqual(InfoCache.Contains.AsSpan()))
                return Operator.Contains;
            if (opSpan.SequenceEqual(InfoCache.NotContains.AsSpan()))
                return Operator.NotContains;
            else if (opSpan.SequenceEqual(InfoCache.LessThan.AsSpan()))
                return Operator.LessThan;
            else if (opSpan.SequenceEqual(InfoCache.LessThanOrEqual.AsSpan()))
                return Operator.LessThanOrEqual;
            else if (opSpan.SequenceEqual(InfoCache.GreaterThan.AsSpan()))
                return Operator.GreaterThan;
            else if (opSpan.SequenceEqual(InfoCache.GreaterThanOrEqual.AsSpan()))
                return Operator.GreaterThanOrEqual;
            else if (opSpan.SequenceEqual(InfoCache.IsEqual.AsSpan()))
                return Operator.IsEqual;
            else if (opSpan.SequenceEqual(InfoCache.IsNotEqual.AsSpan()))
                return Operator.NotEqual;
            else
                throw new InvalidOperationException("Unrecognized operator");
        }

        private static (Expression, bool) CreateOperatorExpression(Expression lhs, Operator op, Expression rhs)
        {
            var lookup = QueryLookups.GetLookup(lhs.Type);

            switch (op)
            {
                case Operator.Contains:
                    return (lookup.GetContainsExpression(lhs, rhs), true);
                case Operator.NotContains:
                    return (lookup.GetContainsExpression(lhs, rhs), false);
                case Operator.LessThan:
                    return (lookup.GetLessThanExpression(lhs, rhs), true);
                case Operator.LessThanOrEqual:
                    return (lookup.GetGreaterThanExpression(lhs, rhs), false);
                case Operator.GreaterThan:
                    return (lookup.GetGreaterThanExpression(lhs, rhs), true);
                case Operator.GreaterThanOrEqual:
                    return (lookup.GetLessThanExpression(lhs, rhs), false);
                case Operator.IsEqual:
                    return (lookup.GetIsEqualExpression(lhs, rhs), true);
                case Operator.NotEqual:
                    return (lookup.GetIsEqualExpression(lhs, rhs), false);
                default:
                    throw new InvalidOperationException("Unrecognized operator");
            }
        }
    }
}
