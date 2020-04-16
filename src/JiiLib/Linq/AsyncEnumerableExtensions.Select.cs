using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JiiLib.Linq
{
    public static partial class AsyncEnumerableExtensions
    {
        public static IAsyncEnumerable<TResult> SelectAsync<T, TResult>(
            this IAsyncEnumerable<T> source,
            Func<T, TResult> selector)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (selector is null)
                throw new ArgumentNullException(nameof(selector));

            return SelectAsyncCore(source, (item, _, sel) => Task.FromResult(sel(item)), selector);
        }

        public static IAsyncEnumerable<TResult> SelectAsync<T, TResult>(
            this IAsyncEnumerable<T> source,
            Func<T, int, TResult> selector)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (selector is null)
                throw new ArgumentNullException(nameof(selector));

            return SelectAsyncCore(source, (item, index, sel) => Task.FromResult(sel(item, index)), selector);
        }

        public static IAsyncEnumerable<TResult> SelectAsync<T, TResult>(
            this IAsyncEnumerable<T> source,
            Func<T, Task<TResult>> selector)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (selector is null)
                throw new ArgumentNullException(nameof(selector));

            return SelectAsyncCore(source, (item, _, sel) => sel(item), selector);
        }

        public static IAsyncEnumerable<TResult> SelectAsync<T, TResult>(
            this IAsyncEnumerable<T> source,
            Func<T, int, Task<TResult>> selector)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (selector is null)
                throw new ArgumentNullException(nameof(selector));

            return SelectAsyncCore(source, (item, index, sel) => sel(item, index), selector);
        }

        private static async IAsyncEnumerable<TResult> SelectAsyncCore<T, TState, TResult>(
            this IAsyncEnumerable<T> source,
            Func<T, int, TState, Task<TResult>> selector,
            TState state)
        {
            int index = -1;
            await foreach (var item in source.ConfigureAwait(false))
            {
                checked { index++; };
                yield return await selector(item, index, state).ConfigureAwait(false);
            }
        }
    }
}
