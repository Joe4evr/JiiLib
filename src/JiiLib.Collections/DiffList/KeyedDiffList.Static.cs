using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using JiiLib.Collections.DiffList;

namespace JiiLib.Collections
{
    /// <summary>
    ///     Convenience methods to create a
    ///     <see cref="KeyedDiffList{TKey}"/>.
    /// </summary>
    public static class KeyedDiffList
    {
        /// <summary>
        ///     Create a new <see cref="KeyedDiffList{TKey}"/> with a set
        ///     of Old and New entry values without needing to create
        ///     an empty instance and shifting the values to a second instance.
        /// </summary>
        /// <param name="oldValues">
        ///     The Old values to pre-fill in.
        /// </param>
        /// <param name="newValues">
        ///     The New values to pre-fill in.
        /// </param>
        /// <param name="equalityComparer">
        ///     An optional equality comparer to pass
        ///     into the <see cref="KeyedDiffList{TKey}"/>.
        /// </param>
        /// <param name="comparison">
        ///     An optional Comparison function to pass
        ///     into the <see cref="KeyedDiffList{TKey}"/>.
        /// </param>
        /// <returns>
        ///     A <see cref="KeyedDiffList{TKey}"/> with the Old
        ///     entries pre-filled to the provided <paramref name="oldValues"/>
        ///     and the New entries pre-filled to <paramref name="newValues"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="oldValues"/> or <paramref name="newValues"/> is <see langword="null"/>.
        /// </exception>
        public static KeyedDiffList<TKey> CreateWithEntries<TKey>(
            IEnumerable<KeyValuePair<TKey, DiffValue>> oldValues,
            IEnumerable<KeyValuePair<TKey, DiffValue>> newValues,
            IEqualityComparer<TKey>? equalityComparer = null,
            Comparison<TKey>? comparison = null)
            where TKey : notnull
        {
            if (oldValues is null)
                throw new ArgumentNullException(paramName: nameof(oldValues));
            if (newValues is null)
                throw new ArgumentNullException(paramName: nameof(newValues));

            equalityComparer ??= EqualityComparer<TKey>.Default;

            var oldEntries = new Dictionary<TKey, DiffValue>(oldValues, equalityComparer);
            oldEntries.TrimExcess();
            return new KeyedDiffList<TKey>(
                oldEntries: new ReadOnlyDictionary<TKey, DiffValue>(oldEntries),
                newEntries: new Dictionary<TKey, DiffValue>(newValues, equalityComparer),
                comparison: comparison);
        }

        /// <summary>
        ///     Create a new <see cref="KeyedDiffList{TKey}"/> with a set
        ///     of Old and New entry values without needing to create
        ///     an empty instance and shifting the values to a second instance.
        /// </summary>
        /// <param name="values">
        ///     The values to pre-fill in.
        /// </param>
        /// <param name="equalityComparer">
        ///     An optional equality comparer to pass
        ///     into the <see cref="KeyedDiffList{TKey}"/>.
        /// </param>
        /// <param name="comparison">
        ///     An optional Comparison function to pass
        ///     into the <see cref="KeyedDiffList{TKey}"/>.
        /// </param>
        /// <returns>
        ///     A <see cref="KeyedDiffList{TKey}"/> with the Old and New
        ///     entries pre-filled to the provided <paramref name="values"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="values"/> is <see langword="null"/>.
        /// </exception>
        public static KeyedDiffList<TKey> CreateWithDualEntries<TKey>(
            IEnumerable<KeyValuePair<TKey, DiffValue>> values,
            IEqualityComparer<TKey>? equalityComparer = null,
            Comparison<TKey>? comparison = null)
            where TKey : notnull
        {
            if (values is null)
                throw new ArgumentNullException(paramName: nameof(values));

            equalityComparer ??= EqualityComparer<TKey>.Default;

            var oldEntries = new Dictionary<TKey, DiffValue>(values, equalityComparer);
            oldEntries.TrimExcess();
            return new KeyedDiffList<TKey>(
                oldEntries: new ReadOnlyDictionary<TKey, DiffValue>(oldEntries),
                newEntries: new Dictionary<TKey, DiffValue>(values, equalityComparer),
                comparison: comparison);
        }

        /// <summary>
        ///     Create a new <see cref="KeyedDiffList{TKey}"/> with a set
        ///     of New entry values without needing to create
        ///     an empty instance and adding values manually.
        /// </summary>
        /// <param name="values">
        ///     The values to pre-fill in.
        /// </param>
        /// <param name="equalityComparer">
        ///     An optional equality comparer to pass
        ///     into the <see cref="KeyedDiffList{TKey}"/>.
        /// </param>
        /// <param name="comparison">
        ///     An optional Comparison function to pass
        ///     into the <see cref="KeyedDiffList{TKey}"/>.
        /// </param>
        /// <returns>
        ///     A <see cref="KeyedDiffList{TKey}"/> with the Old
        ///     entries pre-filled to the provided <paramref name="values"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="values"/> is <see langword="null"/>.
        /// </exception>
        public static KeyedDiffList<TKey> CreateWithNewEntries<TKey>(
            IEnumerable<KeyValuePair<TKey, DiffValue>> values,
            IEqualityComparer<TKey>? equalityComparer = null,
            Comparison<TKey>? comparison = null)
            where TKey : notnull
        {
            if (values is null)
                throw new ArgumentNullException(paramName: nameof(values));

            equalityComparer ??= EqualityComparer<TKey>.Default;

            return new KeyedDiffList<TKey>(
                oldEntries: new ReadOnlyDictionary<TKey, DiffValue>(
                    new Dictionary<TKey, DiffValue>(capacity: 0, equalityComparer)),
                newEntries: new Dictionary<TKey, DiffValue>(values, equalityComparer),
                comparison: comparison);
        }
    }
}
