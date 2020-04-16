using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace JiiLib.Linq
{
    public static partial class AsyncEnumerableExtensions
    {
        public static async Task<T> FirstAsync<T>(this IAsyncEnumerable<T> source)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            await using var e = source.GetAsyncEnumerator();
            return (await e.MoveNextAsync().ConfigureAwait(false))
                ? e.Current
                : throw new InvalidOperationException(message: "Source was empty.");
        }

        public static async Task<T> FirstAsync<T, TState>(
            IAsyncEnumerable<T> source,
            Func<T, TState, Task<bool>> predicate,
            TState state)
        {
            await foreach (var item in source.ConfigureAwait(false))
            {
                if (await predicate(item, state).ConfigureAwait(false))
                    return item;
            }

            throw new InvalidOperationException(message: "Source was empty.");
        }

        public static async Task<T> FirstOrDefault<T>(this IAsyncEnumerable<T> source)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            await using var e = source.GetAsyncEnumerator();
            return (await e.MoveNextAsync().ConfigureAwait(false))
                ? e.Current
                : default!;
        }

        public static async Task<T> FirstOrDefaultAsync<T, TState>(
            IAsyncEnumerable<T> source,
            Func<T, TState, Task<bool>> predicate,
            TState state)
        {
            await foreach (var item in source.ConfigureAwait(false))
            {
                if (await predicate(item, state).ConfigureAwait(false))
                    return item;
            }

            return default!;
        }
    }
}
