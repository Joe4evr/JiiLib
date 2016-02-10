using System;
using System.Collections.Generic;
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
    }
}
