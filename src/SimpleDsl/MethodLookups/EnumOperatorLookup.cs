using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace JiiLib.SimpleDsl
{
    internal sealed class EnumOperatorLookup<T> : OperatorLookup<T>
        where T : struct, Enum
    {
        private static readonly ConstantExpression _comparer = Expression.Constant(EqualityComparer<T>.Default);
        private static readonly MethodInfo _equals = typeof(IEqualityComparer<T>).GetMethod(nameof(IEqualityComparer<T>.Equals));

        public override (BlockExpression, Expression) GetIsEqualExpression(Expression lhs, Expression rhs)
            => (EmptyBlock, Expression.Call(_comparer, _equals, lhs, rhs));

        public override (BlockExpression, Expression) GetContainsExpression(Expression lhs, Expression rhs)
            => throw new InvalidOperationException("Contains Than operations not supported on enums.");
        public override (BlockExpression, Expression) GetGreaterThanExpression(Expression lhs, Expression rhs)
            => throw new InvalidOperationException("Greater/Less Than operations not supported on enums.");
        public override (BlockExpression, Expression) GetLessThanExpression(Expression lhs, Expression rhs)
            => throw new InvalidOperationException("Greater/Less Than operations not supported on enums.");
    }
}
