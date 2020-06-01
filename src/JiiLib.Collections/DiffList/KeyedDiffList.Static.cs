using System;
using System.Collections.Generic;

namespace JiiLib.Collections
{
    public static class KeyedDiffList
    {
        /// <summary>
        ///     Create a new <see cref="KeyedDiffList{TKey}"/> with a set
        ///     of Old entry values without needing to create an empty instance
        ///     and shifting the values to yet another instance.
        /// </summary>
        /// <param name="values">
        ///     The values to pre-fill in.
        /// </param>
        /// <returns>
        ///     A <see cref="KeyedDiffList{TKey}"/> with the Old
        ///     entries pre-filled to the provided <paramref name="values"/>.
        /// </returns>
        public static KeyedDiffList<TKey> CreateWithEntries<TKey>(
            IEnumerable<KeyValuePair<TKey, DiffValue>> values,
            Comparison<TKey>? comparison = null)
            where TKey : notnull
        {
            if (values is null)
                throw new ArgumentNullException(nameof(values));

            return new KeyedDiffList<TKey>(
                oldEntries: new Dictionary<TKey, DiffValue>(values),
                newEntries: new Dictionary<TKey, DiffValue>(values),
                comparison: comparison);
        }
    }
}
