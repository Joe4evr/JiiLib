using System;
using System.Collections.Generic;
using System.Linq;

namespace JiiLib.SimpleDsl
{
    /// <summary>
    ///     
    /// </summary>
    /// <typeparam name="T">
    ///     The element type that is queried against.
    /// </typeparam>
    public sealed class QueryParseResult<T>
    {
        private static readonly Func<T, bool> _defaultFilter = (_ => true);
        private static readonly Func<IEnumerable<T>, IOrderedEnumerable<T>> _defaultOrder = (__ => __.OrderBy(_ => 0));
        private static readonly Func<T, string> _defaultSelector = (_ => _.ToString());

        internal QueryParseResult(
            Func<T, bool> predicate,
            Func<IEnumerable<T>, IOrderedEnumerable<T>> order,
            int skipAmount,
            int takeAmount,
            Func<T, string> selector)
        {
            Predicate = predicate ?? _defaultFilter;
            Order = order ?? _defaultOrder;
            SkipAmount = skipAmount;
            TakeAmount = takeAmount;
            Selector = selector ?? _defaultSelector;
        }

        /// <summary>
        ///     The complete parsed predicate.
        /// </summary>
        public Func<T, bool> Predicate { get; }

        /// <summary>
        ///     A function that orders the collection as desired.
        /// </summary>
        public Func<IEnumerable<T>, IOrderedEnumerable<T>> Order { get; }

        /// <summary>
        ///     The amount of items that are skipped.
        /// </summary>
        public int SkipAmount { get; }

        /// <summary>
        ///     The amount of items that are taken.
        /// </summary>
        public int TakeAmount { get; }

        /// <summary>
        ///     The selector from <typeparamref name="T"/> to a <see cref="String"/>.
        /// </summary>
        public Func<T, string> Selector { get; }

        /// <summary>
        ///     Applies all the functions to the given collection.
        /// </summary>
        /// <param name="items">
        ///     The collection to apply the query to.
        /// </param>
        public IEnumerable<string> Apply(IEnumerable<T> items)
            => Order(items.Where(Predicate))
                .Skip(SkipAmount)
                .Take(TakeAmount)
                .Select(Selector);
    }
}
