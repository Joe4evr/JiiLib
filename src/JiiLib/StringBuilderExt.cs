using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JiiLib
{
    public static class StringBuilderExt
    {
        /// <summary>
        /// Appends a <see cref="string"/> to a <see cref="StringBuilder"/> instance only if a condition is met.
        /// </summary>
        /// <param name="builder">A <see cref="StringBuilder"/> instanc</param>
        /// <param name="predicate">The condition to be met.</param>
        /// <param name="fn">The function to apply if <see cref="predicate"/> is true.</param>
        /// <returns>A <see cref="StringBuilder"/> instance with the specified
        /// <see cref="string"/> appended if <see cref="predicate"/> was true,
        /// or the unchanged <see cref="StringBuilder"/> instance otherwise.</returns>
        public static StringBuilder AppendWhen(this StringBuilder builder, Func<bool> predicate, Func<StringBuilder, StringBuilder> fn)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            if (fn == null) throw new ArgumentNullException(nameof(fn));

            return predicate() ? fn(builder) : builder;
        }

        /// <summary>
        /// Appends each element of an <see cref="IEnumerable{T}"/> to a <see cref="StringBuilder"/> instance.
        /// </summary>
        /// <param name="builder">A <see cref="StringBuilder"/> instance</param>
        /// <param name="seq">The sequence to append.</param>
        /// <param name="fn">The function to apply to each element of the sequence.</param>
        /// <returns>An instance of <see cref="StringBuilder"/> with all elements of <see cref="seq"/>appended.</returns>
        public static StringBuilder AppendSequence<T>(this StringBuilder builder, IEnumerable<T> seq, Func<StringBuilder, T, StringBuilder> fn)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (seq == null) throw new ArgumentNullException(nameof(seq));
            if (fn == null) throw new ArgumentNullException(nameof(fn));

            return seq.Aggregate(builder, fn);
        }
    }
}
