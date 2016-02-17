using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace JiiLib
{
    public static class Extensions
    {
        //Method for randomizing lists using a Fisher-Yates shuffle.
        //Taken from http://stackoverflow.com/questions/273313/
        public static void Shuffle<T>(this IList<T> list)
        {
            var provider =
#if NET46
                new RNGCryptoServiceProvider();
#else
                RandomNumberGenerator.Create();
#endif
            
            int n = list.Count;
            while (n > 1)
            {
                byte[] box = new byte[1];
                do provider.GetBytes(box);
                while (!(box[0] < n * (Byte.MaxValue / n)));
                int k = (box[0] % n);
                n--;
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static bool ContainsIgnoreCase(this IEnumerable<string> haystack, string needle)
            => haystack.Any(s => s.ToLowerInvariant() == needle.ToLowerInvariant());

        public static bool ContainsIgnoreCase(this string haystack, string needle)
            => haystack.ToLowerInvariant().Contains(needle.ToLowerInvariant());

        public static bool StartsWith<T>(this T[] array, T[] values) where T : struct
        {
            bool result = true;
            for (int i = 0; i < values.Length; i++)
            {
                result = (!array[i].Equals(values[i])) ? false : result;
            }

            return result;
        }
        
        public static char[] ToCharArray(this byte[] array)
        {
            return array.Cast<char>().ToArray();
        }

        /*
        public static (string d, string h, string m) SingularOrPlural(this TimeSpan ts)
            => (ts.Days == 1 ? "day" : "days",
                ts.Hours == 1 ? "hour" : "hours",
                ts.Minutes == 1 ? "minute" : "minutes");
        */
    }
}
