using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JiiLib.Linq
{
    public static partial class AsyncEnumerableExtensions
    {
        public static async Task<bool> AnyAsync<T>(this IAsyncEnumerable<T> source)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            await using var e = source.GetAsyncEnumerator();
            return await e.MoveNextAsync().ConfigureAwait(false);
        }

        public static async Task<bool> AnyAsync<T, TState>(
            IAsyncEnumerable<T> source,
            Func<T, TState, Task<bool>> predicate,
            TState state)
        {
            await foreach (var item in source.ConfigureAwait(false))
            {
                if (await predicate(item, state).ConfigureAwait(false))
                    return true;
            }

            return false;
        }
    }
}
