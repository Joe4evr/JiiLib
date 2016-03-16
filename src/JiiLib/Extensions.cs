using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace JiiLib
{
    /// <summary>
    /// Extension methods for various useful operations.
    /// </summary>
    public static class Extensions
    {
        //Method for randomizing lists using a Fisher-Yates shuffle.
        //Taken from http://stackoverflow.com/questions/273313/
        /// <summary>
        /// Perform a Fisher-Yates shuffle on a collection implementing <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <param name="source">The list to shuffle.</param>
        /// <remarks>Taken from http://stackoverflow.com/questions/273313/. </remarks>
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
                byte[] box = new byte[1];
                do provider.GetBytes(box);
                while (!(box[0] < n * (Byte.MaxValue / n)));
                int k = (box[0] % n);
                n--;
                T value = buffer[k];
                buffer[k] = buffer[n];
                buffer[n] = value;
            }

            return buffer;
        }

        /// <summary>
        /// Indicates whether a <see cref="IEnumerable{string}"/> contains a given string case-invariantly.
        /// </summary>
        /// <param name="haystack">The collection of strings.</param>
        /// <param name="needle">The string to find.</param>
        /// <returns>True if at least one item is case-invariantly equal to the provided string, otherwise false.</returns>
        public static bool ContainsIgnoreCase(this IEnumerable<string> haystack, string needle)
            => haystack.Any(s => s.ToLowerInvariant() == needle.ToLowerInvariant());

        /// <summary>
        /// Indicates whether a <see cref="string"/> contains a given substring case-invariantly.
        /// </summary>
        /// <param name="haystack">The string to search in.</param>
        /// <param name="needle">The substring to find.</param>
        /// <returns>True if the string case-invariantly contains the provided substring, otherwise false.</returns>
        public static bool ContainsIgnoreCase(this string haystack, string needle)
            => haystack.ToLowerInvariant().Contains(needle.ToLowerInvariant());

        /// <summary>
        /// Indicates whether an array starts with the same items as a provided array.
        /// </summary>
        /// <param name="array">Array to check.</param>
        /// <param name="values">Array to check against.</param>
        /// <returns>True if the array starts with the same values in the same order as the provided array, otherwise false.</returns>
        public static bool StartsWith<T>(this T[] array, T[] values) where T : struct
        {
            bool result = true;
            for (int i = 0; i < values.Length; i++)
            {
                result = (!array[i].Equals(values[i])) ? false : result;
            }

            return result;
        }

        /// <summary>
        /// Repeat a sequence of items by a specified amount.
        /// </summary>
        /// <param name="sequence">A <see cref="IEnumerable{T}"/> to berepeated.</param>
        /// <param name="amount">The amount of times to repeat the sequence.</param>
        /// <returns>A sequence of items repeated x times.</returns>
        public static IEnumerable<TResult> RepeatSeq<TResult>(this IEnumerable<TResult> sequence, int amount)
        {
            var result = sequence;
            for (int i = 0; i < amount; i++)
            {
                result = result.Concat(sequence);
            }
            return result;
        }

#if !DOTNET5_4
        public static string DeJis(string input)
        {
            var shjis = Encoding.GetEncoding(20290);
            var jisBytes = shjis.GetBytes(input);
            var output = Encoding.Unicode.GetString(jisBytes, 0, input.Length);
            return output;
        }

        //static void SomeVoidReturningMethod()
        //{
        //    null;
        //}
#endif

        /*
        public static (string d, string h, string m) SingularOrPlural(this TimeSpan ts)
            => (ts.Days == 1 ? "day" : "days",
                ts.Hours == 1 ? "hour" : "hours",
                ts.Minutes == 1 ? "minute" : "minutes");
        */
    }
}
