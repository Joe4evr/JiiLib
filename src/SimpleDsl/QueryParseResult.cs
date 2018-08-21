using System;
using System.Collections.Generic;
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
    public sealed class QueryParseResult<T>
    {
        private static readonly Func<T, bool> _defaultFilter = (_ => true);
        private static readonly Func<IEnumerable<T>, IOrderedEnumerable<T>> _defaultOrder = (__ => __.OrderBy(_ => 0));
        private static readonly Func<T, string> _defaultSelector = (_ => _.ToString());

        internal QueryParseResult(
            IReadOnlyDictionary<string, Expression> vars,
            Func<T, bool> predicate,
            Func<IEnumerable<T>, IOrderedEnumerable<T>> order,
            int skipAmount,
            int takeAmount,
            Func<T, string> selector)
        {
            InlineVars = vars;
            Predicate = predicate ?? _defaultFilter;
            Order = order ?? _defaultOrder;
            SkipAmount = skipAmount;
            TakeAmount = takeAmount;
            Selector = selector ?? _defaultSelector;
        }

        /// <summary>
        ///     The inline variables that have been declared for this query.
        /// </summary>
        public IReadOnlyDictionary<string, Expression> InlineVars { get; }

        /// <summary>
        ///     The complete parsed predicate.
        ///     The default function keeps everything.
        /// </summary>
        public Func<T, bool> Predicate { get; }

        /// <summary>
        ///     A function that orders the collection as desired.
        ///     The default function leaves the collection as-is.
        /// </summary>
        public Func<IEnumerable<T>, IOrderedEnumerable<T>> Order { get; }

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

            return Order(items.Where(Predicate))
                .Skip(SkipAmount)
                .Take(TakeAmount)
                .Select(Selector);
        }
    }
}
