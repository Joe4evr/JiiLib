using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;

namespace JiiLib.SimpleDsl
{
    public sealed class QueryableQueryParseResult<T>
        where T : notnull
    {
        private static readonly Expression<Func<T, bool>> _defaultFilter = (_ => true);
        private static readonly Expression<Func<T, int>> _defaultOrder = (_ => 0);
        private static readonly Func<T, string> _defaultSelector = (_ => _.ToString()!);

        private readonly IReadOnlyCollection<OrderByExpression<T>> _orderExprs;

        internal QueryableQueryParseResult(
            IReadOnlyDictionary<string, Expression<Func<T, string>>>? vars,
            Expression<Func<T, bool>>? predicate,
            IReadOnlyCollection<OrderByExpression<T>> orderExprs,
            int skipAmount,
            int takeAmount,
            Func<T, string>? selector)
        {
            InlineVars = vars ?? ImmutableDictionary<string, Expression<Func<T, string>>>.Empty;
            Predicate = predicate ?? _defaultFilter;
            _orderExprs = orderExprs;
            SkipAmount = skipAmount;
            TakeAmount = takeAmount;
            Selector = selector ?? _defaultSelector;
        }

        /// <summary>
        ///     The inline variables that have been declared for this query.
        /// </summary>
        public IReadOnlyDictionary<string, Expression<Func<T, string>>> InlineVars { get; }

        /// <summary>
        ///     The complete parsed predicate.
        ///     The default function keeps everything.
        /// </summary>
        public Expression<Func<T, bool>> Predicate { get; }

        /// <summary>
        ///     The amount of items that are skipped. The default value is 0.
        /// </summary>
        public int SkipAmount { get; }

        /// <summary>
        ///     The amount of items that are taken. The default value is 10.
        /// </summary>
        public int TakeAmount { get; }

        /// <summary>
        ///     The selector from <typeparamref name="T"/> to a <see cref="String"/>.
        ///     The default function calls <see cref="Object.ToString"/>.
        /// </summary>
        public Func<T, string> Selector { get; }

        private IOrderedQueryable<T> Order(IQueryable<T> items)
        {
            var result = items.OrderBy(_defaultOrder);

            foreach (var expr in _orderExprs)
                result = (expr.IsDescending)
                    ? result.ThenByDescending(expr.Expression)
                    : result.ThenBy(expr.Expression);

            return result;
        }

        /// <summary>
        ///     Applies all the functions to the given collection.
        /// </summary>
        /// <param name="items">
        ///     The collection to apply the query to.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="items"/> was <see langword="null"/>.
        /// </exception>
        public IEnumerable<string> Apply(IQueryable<T> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

#if DEBUG
            var filtered = items.Where(Predicate);
            var ordered = Order(filtered);
            var skipped = ordered.Skip(SkipAmount);
            var taken = skipped.Take(TakeAmount);
            var list = taken.ToList();
            var selection = list.Select(Selector);
            return selection;
#else
            return Order(items.Where(Predicate))
                .Skip(SkipAmount)
                .Take(TakeAmount)
                .ToList()
                .Select(Selector);
#endif
        }

        internal EnumerableQueryParseResult<T> Compile()
            => new EnumerableQueryParseResult<T>(
                InlineVars,
                Predicate.Compile(),
                _orderExprs.Select(e => e.Compile()).ToImmutableArray(),
                SkipAmount,
                TakeAmount,
                Selector);
    }
}
