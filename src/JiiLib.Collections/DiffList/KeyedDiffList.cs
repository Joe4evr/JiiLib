using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace JiiLib.Collections.DiffList
{
    /// <summary>
    ///     Represents two lists of values that can
    ///     be compared for changes.
    /// </summary>
    /// <typeparam name="TKey">
    ///     The key type.
    /// </typeparam>
    /// 
    public sealed partial class KeyedDiffList<TKey> : IEnumerable<DiffValuePair<TKey>>
        where TKey : notnull
    {
        private readonly ReadOnlyDictionary<TKey, DiffValue> _oldEntries;
        private readonly Dictionary<TKey, DiffValue> _newEntries;
        private readonly Comparison<TKey>? _comparison;

        private int _version = 0;

        /// <summary>
        ///     Initializes a new <see cref="KeyedDiffList{TKey}"/>
        ///     with empty entries.
        /// </summary>
        /// <param name="equalityComparer">
        ///     An optional equality comparer to use for comparing keys.
        ///     If not provided, a default comparer will be used.
        /// </param>
        /// <param name="comparison">
        ///     An optional comparison function to sort
        ///     the keys in a specific way during enumeration.
        ///     If not provided, a default comparison will be used.
        /// </param>
        public KeyedDiffList(
            IEqualityComparer<TKey>? equalityComparer = null,
            Comparison<TKey>? comparison = null)
            : this(new ReadOnlyDictionary<TKey, DiffValue>(
                    new Dictionary<TKey, DiffValue>(equalityComparer ??= EqualityComparer<TKey>.Default)),
                  new Dictionary<TKey, DiffValue>(equalityComparer),
                  comparison)
        {
        }

        internal KeyedDiffList(
            ReadOnlyDictionary<TKey, DiffValue> oldEntries,
            Dictionary<TKey, DiffValue> newEntries,
            Comparison<TKey>? comparison = null)
        {
            _oldEntries = oldEntries;
            _newEntries = newEntries;
            _comparison = comparison;
        }

        /// <summary>
        ///     Gets the amount of Old entries currently stored.
        /// </summary>
        public int OldEntriesCount => _oldEntries.Count;

        /// <summary>
        ///     Gets the amount of New entries currently stored.
        /// </summary>
        public int NewEntriesCount => _newEntries.Count;

        /// <summary>
        ///     Gets the value-pair at a specified key.
        /// </summary>
        /// <param name="key">
        ///     Key of the entries.
        /// </param>
        /// <returns>
        ///     A <see cref="DiffValuePair{TKey}"/> that
        ///     represents the values at the specified key.
        /// </returns>
        /// <remarks>
        ///     If neither Old nor New entries contained a value
        ///     at the specified key, this property will
        ///     return a <see cref="DiffValuePair{TKey}"/>
        ///     where <see cref="DiffValuePair{TKey}.OldValue"/>
        ///     and <see cref="DiffValuePair{TKey}.NewValue"/>
        ///     are <see langword="null"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="key"/> was <see langword="null"/>.
        /// </exception>
        public DiffValuePair<TKey> this[TKey key]
        {
            get
            {
                if (key is null)
                    throw new ArgumentNullException(nameof(key));

                var old = _oldEntries.GetValueOrDefault(key);
                var @new = _newEntries.GetValueOrDefault(key);
                return new DiffValuePair<TKey>(key, old, @new);
            }
        }

        /// <summary>
        ///     Determines whether a specified key exists
        ///     in at least one of the Old and New entries.
        /// </summary>
        /// <param name="key">
        ///     The key to locate.
        /// </param>
        /// <returns>
        ///     <see langword="true"/> if either Old or New
        ///     entries contains an element that has the
        ///     specified key; otherwise, <see langword="false"/>.
        /// </returns>
        public bool ContainsKey(TKey key)
        {
            return _oldEntries.ContainsKey(key) || _newEntries.ContainsKey(key);
        }

        /// <summary>
        ///     Create or replace a single value in the New entries.
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

            Interlocked.Increment(ref _version);
            return this;
        }

        /// <summary>
        ///     Create or replace a set of values in the New entries.
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

            Interlocked.Increment(ref _version);
            return this;
        }

        /// <summary>
        ///     Add a single value to an existing entry.
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

            Interlocked.Increment(ref _version);
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
        ///     <paramref name="key"/> or <paramref name="values"/> was <see langword="null"/>.
        /// </exception>
        /// <exception cref="KeyNotFoundException">
        ///     <paramref name="key"/> did not exist in the set of New entries.
        /// </exception>
        public KeyedDiffList<TKey> AddTo(TKey key, IEnumerable<string> values)
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key));
            if (values is null)
                throw new ArgumentNullException(nameof(values));

            var dv = _newEntries[key];
            _newEntries[key] = dv.Add(values);

            Interlocked.Increment(ref _version);
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

            Interlocked.Increment(ref _version);
            return this;
        }

        /// <summary>
        ///     Remove a single value from an entry in the set of New entries.
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

            Interlocked.Increment(ref _version);
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
            new ReadOnlyDictionary<TKey, DiffValue>(
                new Dictionary<TKey, DiffValue>(_newEntries)),
            new Dictionary<TKey, DiffValue>(_newEntries), _comparison);

        /// <summary>
        ///     Gets the enumerator for this list.
        /// </summary>
        public Enumerator GetEnumerator() => new Enumerator(this);

        ///// <inheritdoc cref="IEnumerable{T}.GetEnumerator" />
        IEnumerator<DiffValuePair<TKey>> IEnumerable<DiffValuePair<TKey>>.GetEnumerator() => GetEnumerator();
        ///// <inheritdoc cref="IEnumerable.GetEnumerator" />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
