using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace JiiLib.Collections.DiffList
{
    public sealed partial class KeyedDiffList<TKey>
    {
        /// <summary>
        ///     Provides the enumerator for a
        ///     <see cref="KeyedDiffList{TKey}"/>.
        /// </summary>
        [StructLayout(LayoutKind.Auto)]
        public struct Enumerator : IEnumerator<DiffValuePair<TKey>>
        {
            /// <inheritdoc cref="IEnumerator{T}.Current" />
            public DiffValuePair<TKey> Current { get; private set; }

            private readonly KeyedDiffList<TKey> _list;
            private readonly int _listVersion;

            private SortedSet<TKey>.Enumerator _keys;

            internal Enumerator(KeyedDiffList<TKey> list)
                : this()
            {
                _list = list;
                _listVersion = list._version;

                var comp = (list._comparison is null)
                    ? Comparer<TKey>.Default
                    : Comparer<TKey>.Create(list._comparison);
                _keys = new SortedSet<TKey>(list._oldEntries.Keys.Concat(list._newEntries.Keys), comp)
                    .GetEnumerator();
            }

            /// <inheritdoc cref="IEnumerator.MoveNext" />
            public bool MoveNext()
            {
                VersionCheck();

                if (_keys.MoveNext())
                {
                    var key = _keys.Current;
                    var old = _list._oldEntries.GetValueOrDefault(key);
                    var @new = _list._newEntries.GetValueOrDefault(key);

                    //VersionCheck();

                    Current = new DiffValuePair<TKey>(key, old, @new);
                    return true;
                }

                return false;
            }

            private void VersionCheck()
            {
                if (_list._version > _listVersion)
                    throw new InvalidOperationException("KeyedDiffList modified during enumeration.");
            }

            /// <summary>
            ///     No-op.
            /// </summary>
            public void Dispose() { }

            /// <inheritdoc cref="IEnumerator.Current" />
            object IEnumerator.Current => Current;
            /// <inheritdoc cref="IEnumerator.Reset" />
            void IEnumerator.Reset() => throw new NotImplementedException();
        }
    }
}
