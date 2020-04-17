using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;

namespace JiiLib.SimpleDsl
{
    /// <summary>
    ///     Contains the resulting functions that were parsed out of the user query,
    ///     or the default of a function if that clause went unspecified.
    /// </summary>
    /// <typeparam name="T">
    ///     The element type that is queried against.
    /// </typeparam>
    public sealed class EnumerableQueryParseResult<T>
        where T : notnull
    {
        private static readonly Func<T, bool> _defaultFilter = (_ => true);
        private static readonly Func<T, int> _defaultOrder = (_ => 0);
        private static readonly Func<T, string> _defaultSelector = (_ => _.ToString()!);

        private readonly IReadOnlyCollection<OrderByFunc<T>> _orderFuncs;

        internal EnumerableQueryParseResult(
            IReadOnlyDictionary<string, Expression<Func<T, string>>>? vars,
            Func<T, bool>? predicate,
            IReadOnlyCollection<OrderByFunc<T>> orderFuncs,
            int skipAmount,
            int takeAmount,
            Func<T, string>? selector)
        {
            InlineVars = vars ?? ImmutableDictionary<string, Expression<Func<T, string>>>.Empty;
            Predicate = predicate ?? _defaultFilter;
            _orderFuncs = orderFuncs;
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
        public Func<T, bool> Predicate { get; }

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

        private IOrderedEnumerable<T> Order(IEnumerable<T> items)
        {
            var result = items.OrderBy(_defaultOrder);

            foreach (var func in _orderFuncs)
                result = (func.IsDescending)
                    ? result.ThenByDescending(func.Function)
                    : result.ThenBy(func.Function);

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
        public IEnumerable<string> Apply(IEnumerable<T> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

#if DEBUG
            var filtered = items.Where(Predicate).ToArray();
            var ordered = Order(filtered).ToArray();
            var skipped = ordered.Skip(SkipAmount).ToArray();
            var taken = skipped.Take(TakeAmount).ToArray();
            var selection = taken.Select(Selector).ToArray();
            return selection;
#else
            return Order(items.Where(Predicate))
                .Skip(SkipAmount)
                .Take(TakeAmount)
                .Select(Selector);
#endif
        }
    }
}
