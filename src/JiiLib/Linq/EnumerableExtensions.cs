using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace JiiLib.Linq
{
    public static class EnumerableExtensions
    {
        //Method for randomizing lists using a Fisher-Yates shuffle.
        //Taken from http://stackoverflow.com/questions/273313/
        /// <summary>
        ///     Perform a Fisher-Yates shuffle on a collection implementing <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <param name="source">
        ///     The list to shuffle.
        /// </param>
        /// <remarks>
        ///     Adapted from http://stackoverflow.com/questions/273313/.
        /// </remarks>
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            var provider =
#if DOTNET5_4
                RandomNumberGenerator.Create();
#else
                new RNGCryptoServiceProvider();
#endif
            var buffer = source.ToList();
            int n = buffer.Count;
            while (n > 1)
            {
                byte[] box = new byte[(n / Byte.MaxValue) + 1];
                int boxSum;
                do
                {
                    provider.GetBytes(box);
                    boxSum = box.Sum(b => b);
                }
                while (!(boxSum < n * ((Byte.MaxValue * box.Length) / n)));
                int k = (boxSum % n);
                n--;
                T value = buffer[k];
                buffer[k] = buffer[n];
                buffer[n] = value;
            }

            return buffer;
        }

        /// <summary>
        ///     Indicates whether a <see cref="IEnumerable{string}"/> contains a given string case-invariantly.
        /// </summary>
        /// <param name="haystack">
        ///     The collection of strings.
        /// </param>
        /// <param name="needle">
        ///     The string to find.
        /// </param>
        /// <returns>
        ///     True if at least one item is case-invariantly equal to the provided string, otherwise false.
        /// </returns>
        public static bool ContainsIgnoreCase(this IEnumerable<string> haystack, string needle)
            => haystack.Any(s => s.ToLowerInvariant() == needle.ToLowerInvariant());

        /// <summary>
        ///     Repeat a sequence of items by a specified amount.
        /// </summary>
        /// <param name="sequence">
        ///     An <see cref="IEnumerable{T}"/> to be repeated.
        /// </param>
        /// <param name="amount">
        ///     The amount of times to repeat the sequence.
        /// </param>
        /// <returns>
        ///     A sequence of items repeated x times.
        /// </returns>
        public static IEnumerable<TResult> RepeatSeq<TResult>(this IEnumerable<TResult> sequence, int amount)
        {
            var result = sequence;
            for (int i = 0; i < amount; i++)
            {
                result = result.Concat(sequence);
            }
            return result;
        }

    }
}
