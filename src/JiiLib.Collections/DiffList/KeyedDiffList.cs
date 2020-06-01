using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace JiiLib.Collections
{
    /// <summary>
    ///     Represents two lists of values that can
    ///     be compared for changes.
    /// </summary>
    /// <typeparam name="TKey">
    ///     The key type.
    /// </typeparam>
    public sealed class KeyedDiffList<TKey>
        where TKey : notnull//, IEquatable<TKey>
    {
        private readonly Dictionary<TKey, DiffValue> _oldEntries;
        private readonly Dictionary<TKey, DiffValue> _newEntries;
        private readonly Comparison<TKey>? _comparison;

        /// <summary>
        ///     Initializes a new <see cref="KeyedDiffList{TKey}"/>
        ///     with empty entries.
        /// </summary>
        /// <param name="comparison">
        ///     An optional comparison function to sort
        ///     the keys in a specific way.
        ///     
        ///     If not provided, a default comparison
        ///     will be used.
        /// </param>
        public KeyedDiffList(Comparison<TKey>? comparison = null)
            : this(new Dictionary<TKey, DiffValue>(), new Dictionary<TKey, DiffValue>(), comparison)
        {
        }

        internal KeyedDiffList(
            Dictionary<TKey, DiffValue> oldEntries,
            Dictionary<TKey, DiffValue> newEntries,
            Comparison<TKey>? comparison = null)
        {
            _oldEntries = oldEntries;
            _newEntries = newEntries;
            _comparison = comparison;
        }

        /// <summary>
        ///     Add or replace a value in the New entries.
        /// </summary>
        /// <param name="key">
        ///     Key of the entry.
        /// </param>
        /// <param name="value">
        ///     Value to add or replace.
        /// </param>
        /// <returns>
        ///     This instance to allow chaining calls.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="key"/> or <paramref name="value"/> was <see langword="null"/>.
        /// </exception>
        public KeyedDiffList<TKey> SetEntry(TKey key, string value)
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key));
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            _newEntries[key] = new DiffValue(value);
            return this;
        }

        /// <summary>
        ///     Add or replace a set of values in the New entries.
        /// </summary>
        /// <param name="key">
        ///     Key of the entry.
        /// </param>
        /// <param name="values">
        ///     Values to add or replace.
        /// </param>
        /// <returns>
        ///     This instance to allow chaining calls.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="key"/> or <paramref name="values"/> was <see langword="null"/>.
        /// </exception>
        public KeyedDiffList<TKey> SetEntry(TKey key, IEnumerable<string> values)
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key));
            if (values is null)
                throw new ArgumentNullException(nameof(values));

            _newEntries[key] = new DiffValue(values);
            return this;
        }

        /// <summary>
        ///     Add a value to an existing entry.
        /// </summary>
        /// <param name="key">
        ///     Key of the entry.
        /// </param>
        /// <param name="value">
        ///     Value to add.
        /// </param>
        /// <returns>
        ///     This instance to allow chaining calls.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="key"/> or <paramref name="value"/> was <see langword="null"/>.
        /// </exception>
        /// <exception cref="KeyNotFoundException">
        ///     <paramref name="key"/> did not exist in the set of New entries.
        /// </exception>
        public KeyedDiffList<TKey> AddTo(TKey key, string value)
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key));
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            var dv = _newEntries[key];
            _newEntries[key] = dv.Add(value);
            return this;
        }

        /// <summary>
        ///     Add a set of values to an existing entry.
        /// </summary>
        /// <param name="key">
        ///     Key of the entry.
        /// </param>
        /// <param name="values">
        ///     Values to add.
        /// </param>
        /// <returns>
        ///     This instance to allow chaining calls.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="key"/> was <see langword="null"/>.
        /// </exception>
        /// <exception cref="KeyNotFoundException">
        ///     <paramref name="key"/> or <paramref name="values"/> did not exist in the set of New entries.
        /// </exception>
        public KeyedDiffList<TKey> AddTo(TKey key, IEnumerable<string> values)
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key));
            if (values is null)
                throw new ArgumentNullException(nameof(values));

            var dv = _newEntries[key];
            _newEntries[key] = dv.Add(values);
            return this;
        }

        /// <summary>
        ///     Remove an Entry from the set of New entries.
        /// </summary>
        /// <param name="key">
        ///     Key of the entry.
        /// </param>
        /// <returns>
        ///     This instance to allow chaining calls.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="key"/> was <see langword="null"/>.
        /// </exception>
        public KeyedDiffList<TKey> RemoveEntry(TKey key)
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key));

            _newEntries.Remove(key);
            return this;
        }

        /// <summary>
        ///     Remove a value from an entry in the set of New entries.
        /// </summary>
        /// <param name="key">
        ///     Key of the entry.
        /// </param>
        /// <param name="value">
        ///     Value to add.
        /// </param>
        /// <returns>
        ///     This instance to allow chaining calls.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="key"/> or <paramref name="value"/> was <see langword="null"/>.
        /// </exception>
        /// <exception cref="KeyNotFoundException">
        ///     <paramref name="key"/> did not exist in the set of New entries.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     The entry had only a single value.
        /// </exception>
        public KeyedDiffList<TKey> RemoveFrom(TKey key, string value)
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key));
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            var dv = _newEntries[key];
            _newEntries[key] = dv.Remove(value);
            return this;
        }

        /// <summary>
        ///     Creates a new <see cref="KeyedDiffList{TKey}"/> where the New entries
        ///     of the current instance become the Old entries of the new instance.
        /// </summary>
        /// <returns>
        ///     A new <see cref="KeyedDiffList{TKey}"/>.
        /// </returns>
        public KeyedDiffList<TKey> Shift() => new KeyedDiffList<TKey>(
            new Dictionary<TKey, DiffValue>(_newEntries),
            new Dictionary<TKey, DiffValue>(_newEntries), _comparison);

        /// <summary>
        ///     Gets the enumerator for this list.
        /// </summary>
        public Enumerator GetEnumerator() => new Enumerator(this);

        public struct Enumerator
        {
            public (TKey Key, DiffValue? Old, DiffValue? New) Current { get; private set; }

            private readonly KeyedDiffList<TKey> _diff;
            private SortedSet<TKey>.Enumerator _keys;

            public Enumerator(KeyedDiffList<TKey> diff)
                : this()
            {
                _diff = diff;

                var comp = (diff._comparison is null)
                    ? Comparer<TKey>.Default
                    : Comparer<TKey>.Create(diff._comparison);
                _keys = new SortedSet<TKey>(diff._oldEntries.Keys.Concat(diff._newEntries.Keys), comp)
                    .GetEnumerator();
            }

            public bool MoveNext()
            {
                if (_keys.MoveNext())
                {
                    var key = _keys.Current;
                    var old = _diff._oldEntries.GetValueOrDefault(key);
                    var @new = _diff._newEntries.GetValueOrDefault(key);
                    Current = (key, old, @new);
                    return true;
                }

                return false;
            }
        }
    }
}
