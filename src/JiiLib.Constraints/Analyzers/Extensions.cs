using System;
using System.Collections.Generic;
using System.Linq;

namespace JiiLib.Constraints
{
    internal static class Extensions
    {
        public static IEnumerable<(T1, T2)> ZipT<T1, T2>(this IEnumerable<T1> source, IEnumerable<T2> second)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (second is null) throw new ArgumentNullException(nameof(second));

            var e1 = source.GetEnumerator();
            var e2 = second.GetEnumerator();

            while (e1.MoveNext() & e2.MoveNext())
            {
                yield return (e1.Current, e2.Current);
            }
        }

        public static bool None<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (predicate is null) throw new ArgumentNullException(nameof(predicate));

            return source.All(item => !predicate(item));
        }
    }
}
