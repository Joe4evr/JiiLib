using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JiiLib
{
    public static class StringBuilderExt
    {
        public static StringBuilder AppendWhen(this StringBuilder builder, Func<bool> predicate, Func<StringBuilder, StringBuilder> fn)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            if (fn == null) throw new ArgumentNullException(nameof(fn));

            return predicate() ? fn(builder) : builder;
        }

        public static StringBuilder AppendSequence<T>(this StringBuilder builder, IEnumerable<T> seq, Func<StringBuilder, T, StringBuilder> fn)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (seq == null) throw new ArgumentNullException(nameof(seq));
            if (fn == null) throw new ArgumentNullException(nameof(fn));

            return seq.Aggregate(builder, fn);
        }
    }
}
