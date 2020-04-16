using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JiiLib.Linq
{
    public static partial class AsyncEnumerableExtensions
    {
        public static IAsyncEnumerable<T> AsAsyncEnumerable<T>(this IEnumerable<T> sequence)
            => new AsyncSyncWrapper<T>(sequence);

        private sealed class AsyncSyncWrapper<T> : IAsyncEnumerable<T>
        {
            private readonly IEnumerable<T> _wrapped;

            public AsyncSyncWrapper(IEnumerable<T> wrapped)
            {
                _wrapped = wrapped;
            }

            public Enumerator GetAsyncEnumerator(CancellationToken cancellationToken = default)
                => new Enumerator(_wrapped, cancellationToken);

            IAsyncEnumerator<T> IAsyncEnumerable<T>.GetAsyncEnumerator(CancellationToken cancellationToken)
                => GetAsyncEnumerator(cancellationToken);

            public struct Enumerator : IAsyncEnumerator<T>
            {
                private readonly IEnumerator<T> _enumerator;
                private readonly CancellationToken _cancellationToken;
                public T Current { readonly get; private set; }

                public Enumerator(IEnumerable<T> wrapped, CancellationToken cancellationToken = default)
                    : this()
                {
                    _enumerator = wrapped.GetEnumerator();
                    _cancellationToken = cancellationToken;
                }

                public ValueTask<bool> MoveNextAsync()
                {
                    _cancellationToken.ThrowIfCancellationRequested();

                    if (_enumerator.MoveNext())
                    {
                        Current = _enumerator.Current;
                        return new ValueTask<bool>(true);
                    }

                    return new ValueTask<bool>(false);
                }

                public ValueTask DisposeAsync() => new ValueTask();
            }
        }
    }
}
