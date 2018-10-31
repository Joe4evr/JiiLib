using System;
using System.Linq.Expressions;

namespace JiiLib.SimpleDsl
{
    internal struct OrderByExpression<T>
    {
        public Expression<Func<T, int>> Expression { get; }

        public bool IsDescending { get; }

        public OrderByExpression(Expression<Func<T, int>> expression, bool isDescending)
        {
            Expression = expression;
            IsDescending = isDescending;
        }

        public OrderByFunc<T> Compile()
            => new OrderByFunc<T>(Expression.Compile(), IsDescending);
    }

    internal struct OrderByFunc<T>
    {
        public Func<T, int> Function { get; }

        public bool IsDescending { get; }

        public OrderByFunc(Func<T, int> function, bool isDescending)
        {
            Function = function;
            IsDescending = isDescending;
        }
    }
}
