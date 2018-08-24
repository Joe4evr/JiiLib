using System;
using System.Linq.Expressions;
using System.Reflection;

namespace JiiLib.SimpleDsl
{
    internal sealed class NullableOperatorLookup<T> : OperatorLookup<T?>
        where T : struct
    {
        private static readonly IOperatorLookup _baseLookup;
        private static readonly PropertyInfo _hasValue;
        private static readonly PropertyInfo _value;

        static NullableOperatorLookup()
        {
            var elemType = typeof(T);
            var nullType = typeof(T?);
            _baseLookup = QueryLookups.GetLookup(elemType);
            _hasValue = nullType.GetProperty(nameof(Nullable<T>.HasValue));
            _value = nullType.GetProperty(nameof(Nullable<T>.Value));
        }

        public override (BlockExpression, Expression) GetContainsExpression(Expression lhs, Expression rhs)
        {
            if (rhs.Type.IsNullableStruct(out _))
                return (EmptyBlock, InvertHasValue(lhs));

            return _baseLookup.GetContainsExpression(Expression.Property(lhs, _value), rhs);
        }

        public override (BlockExpression, Expression) GetGreaterThanExpression(Expression lhs, Expression rhs)
        {
            if (rhs.Type.IsNullableStruct(out _))
                return (EmptyBlock, InvertHasValue(lhs));

            return _baseLookup.GetGreaterThanExpression(Expression.Property(lhs, _value), rhs);
        }

        public override (BlockExpression, Expression) GetIsEqualExpression(Expression lhs, Expression rhs)
        {
            if (rhs.Type.IsNullableStruct(out _))
                return (EmptyBlock, InvertHasValue(lhs));

            return _baseLookup.GetIsEqualExpression(Expression.Property(lhs, _value), rhs);
        }

        public override (BlockExpression, Expression) GetLessThanExpression(Expression lhs, Expression rhs)
        {
            if (rhs.Type.IsNullableStruct(out _))
                return (EmptyBlock, InvertHasValue(lhs));

            return _baseLookup.GetLessThanExpression(Expression.Property(lhs, _value), rhs);
        }

        private static Expression InvertHasValue(Expression lhs)
            => Expression.IsFalse(Expression.Property(lhs, _hasValue));
    }
}
