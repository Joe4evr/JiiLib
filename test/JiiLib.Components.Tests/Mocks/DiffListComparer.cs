using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JiiLib.Collections.DiffList;

namespace JiiLib.Components.Tests
{
    internal sealed class DiffListComparer<TKey> : IEqualityComparer<KeyedDiffList<TKey>?>
        where TKey : notnull
    {
        public static DiffListComparer<TKey> Instance { get; } = new DiffListComparer<TKey>();


        public bool Equals(KeyedDiffList<TKey>? x, KeyedDiffList<TKey>? y)
        {
            return (x, y) switch
            {
                (null, null) => true,
                (_, null) => false,
                (null, _) => false,
                var (l, r) => EqualsCore(l, r)
            };

            static bool EqualsCore(KeyedDiffList<TKey> left, KeyedDiffList<TKey> right)
            {
                if (ReferenceEquals(left, right))
                {
                    return true;
                }

                if ((left.OldEntriesCount != right.OldEntriesCount)
                    || (left.NewEntriesCount != right.NewEntriesCount))
                {
                    return false;
                }


                var joined = left.Join(inner: right,
                    outerKeySelector: dv => dv.Key,
                    innerKeySelector: dv => dv.Key,
                    resultSelector: (l, r) => (l, r));

                foreach (var (lpair, rpair) in joined)
                {
                    if ((lpair.OldValue != rpair.OldValue)
                        || (lpair.NewValue != rpair.NewValue))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public int GetHashCode([DisallowNull] KeyedDiffList<TKey> obj)
        {
            var hash = new HashCode();

            foreach (var (key, ov, nv) in obj)
            {
                hash.Add(key);
                hash.Add(ov);
                hash.Add(nv);
            }

            return hash.ToHashCode();
        }
    }
}
