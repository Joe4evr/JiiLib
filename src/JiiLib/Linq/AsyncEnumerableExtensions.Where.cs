using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JiiLib.Linq
{
    public static partial class AsyncEnumerableExtensions
    {
        public static IAsyncEnumerable<T> WhereAsync<T, TState>(
            this IAsyncEnumerable<T> source,
            Func<T, TState, Task<bool>> predicate,
            TState state)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            return WhereAsyncCore(source, predicate, state);
        }

        public static IAsyncEnumerable<T> WhereAsync<T, TState>(
            this IAsyncEnumerable<T> source,
            Func<T, TState, bool> predicate,
            TState state)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            return WhereAsyncCore(source, (item, tup) => Task.FromResult(tup.predicate(item, tup.state)), (predicate, state));
        }

        private static async IAsyncEnumerable<T> WhereAsyncCore<T, TState>(
            this IAsyncEnumerable<T> sequence,
            Func<T, TState, Task<bool>> predicate,
            TState state)
        {
            await foreach (var item in sequence.ConfigureAwait(false))
            {
                if (await predicate(item, state).ConfigureAwait(false))
                    yield return item;
            }
        }
    }
}
