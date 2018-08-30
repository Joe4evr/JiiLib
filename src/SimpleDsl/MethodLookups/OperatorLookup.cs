using System;
using System.Linq.Expressions;

namespace JiiLib.SimpleDsl
{
    /// <summary>
    ///     
    /// </summary>
    /// <typeparam name="T">
    ///     
    /// </typeparam>
    public abstract class OperatorLookup<T> : IOperatorLookup
    {
        /// <summary>
        ///     A cached empty <see cref="BlockExpression"/> to use if no additional block is needed.
        /// </summary>
        protected static BlockExpression EmptyBlock { get; } = InfoCache.EmptyBlock;

        /// <summary>
        ///     
        /// </summary>
        /// <param name="lhs">
        ///     The left-hand side expression.
        /// </param>
        /// <param name="rhs">
        ///     The right-hand side expression.
        /// </param>
        /// <returns>
        ///     A <see cref="BlockExpression"/> that holds intermediate expressions or is empty,
        ///     and a <see cref="MethodCallExpression"/> that performs the Contains operation.
        /// </returns>
        public abstract Expression GetContainsExpression(Expression lhs, Expression rhs);

        /// <summary>
        ///     
        /// </summary>
        /// <param name="lhs">
        ///     The left-hand side expression.
        /// </param>
        /// <param name="rhs">
        ///     The right-hand side expression.
        /// </param>
        /// <returns>
        ///     A <see cref="BlockExpression"/> that holds intermediate expressions or is empty,
        ///     and a <see cref="MethodCallExpression"/> that performs the LessThan operation.
        /// </returns>
        public abstract Expression GetLessThanExpression(Expression lhs, Expression rhs);

        /// <summary>
        ///     
        /// </summary>
        /// <param name="lhs">
        ///     The left-hand side expression.
        /// </param>
        /// <param name="rhs">
        ///     The right-hand side expression.
        /// </param>
        /// <returns>
        ///     A <see cref="BlockExpression"/> that holds intermediate expressions or is empty,
        ///     and a <see cref="MethodCallExpression"/> that performs the GreaterThan operation.
        /// </returns>
        public abstract Expression GetGreaterThanExpression(Expression lhs, Expression rhs);

        /// <summary>
        ///     
        /// </summary>
        /// <param name="lhs">
        ///     The left-hand side expression.
        /// </param>
        /// <param name="rhs">
        ///     The right-hand side expression.
        /// </param>
        /// <returns>
        ///     A <see cref="BlockExpression"/> that holds intermediate expressions or is empty,
        ///     and a <see cref="MethodCallExpression"/> that performs the IsEqual operation.
        /// </returns>
        public abstract Expression GetIsEqualExpression(Expression lhs, Expression rhs);
    }
}
