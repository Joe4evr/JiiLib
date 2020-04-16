using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JiiLib.Linq
{
    public static partial class AsyncEnumerableExtensions
    {
        public static async Task<T> SingleAsync<T>(this IAsyncEnumerable<T> source)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            await using var e = source.GetAsyncEnumerator();
            if (!(await e.MoveNextAsync().ConfigureAwait(false)))
                throw new InvalidOperationException(message: "Source was empty.");

            var item = e.Current;
            if (await e.MoveNextAsync().ConfigureAwait(false))
                throw new InvalidOperationException(message: "Source had more than one element.");

            return item;
        }

        public static async Task<T> SingleAsync<T, TState>(
            IAsyncEnumerable<T> source,
            Func<T, TState, Task<bool>> predicate,
            TState state)
        {
            await using var e = source.GetAsyncEnumerator();
            if (!(await e.MoveNextAsync()))
                throw new InvalidOperationException(message: "Source was empty.");

            bool assigned = false;
            T item = default!;
            while (await e.MoveNextAsync().ConfigureAwait(false))
            {
                if (await predicate(e.Current, state).ConfigureAwait(false))
                {
                    item = assigned
                        ? throw new InvalidOperationException(message: "Source had more than one element matching the predicate.")
                        : e.Current;
                }
            }

            return assigned ? item
                : throw new InvalidOperationException(message: "Source had more no elements matching the predicate.");
        }

        public static async Task<T> SingleOrDefault<T>(this IAsyncEnumerable<T> source)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            await using var e = source.GetAsyncEnumerator();
            if (!(await e.MoveNextAsync().ConfigureAwait(false)))
                return default!;

            var item = e.Current;
            if (await e.MoveNextAsync().ConfigureAwait(false))
                throw new InvalidOperationException(message: "Source had more than one element.");

            return item;
        }

        public static async Task<T> SingleOrDefaultAsync<T, TState>(
            IAsyncEnumerable<T> source,
            Func<T, TState, Task<bool>> predicate,
            TState state)
        {
            await using var e = source.GetAsyncEnumerator();
            if (!(await e.MoveNextAsync()))
                return default!;

            bool assigned = false;
            T item = default!;
            while (await e.MoveNextAsync().ConfigureAwait(false))
            {
                if (await predicate(e.Current, state).ConfigureAwait(false))
                {
                    item = assigned
                        ? throw new InvalidOperationException(message: "Source had more than one element matching the predicate.")
                        : e.Current;
                }
            }

            return item;
        }
    }
}
