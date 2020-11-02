﻿using System;
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
        public static KeyedDiffList<TKey> CreateWithEntries<TKey>(
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
                    new Dictionary<TKey, DiffValue>(values, equalityComparer)),
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
                    new Dictionary<TKey, DiffValue>(equalityComparer)),
                newEntries: new Dictionary<TKey, DiffValue>(values, equalityComparer),
                comparison: comparison);
        }
    }
}
