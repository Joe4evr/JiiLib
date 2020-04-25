using System;
using System.IO;

namespace JiiLib.Media.Internal
{
    internal static class Extensions
    {
        public static int GetCount<T>(this ReadOnlySpan<T> span, T item)
            where T : IEquatable<T>
        {
            int counter = 0;
            for (int i = 0; i < span.Length; i++)
            {
                if (span[i].Equals(item))
                    counter++;
            }

            return counter;
        }

        public static int GetCount<T>(this Span<T> span, T item)
            where T : IEquatable<T>
            => GetCount((ReadOnlySpan<T>)span, item);


        public static int ReadSynchInt(this BinaryReader reader)
        {
            Span<byte> buffer = stackalloc byte[4];
            reader.Read(buffer);

            int result = 0;
            result |= buffer[0] << 21;
            result |= buffer[1] << 14;
            result |= buffer[2] << 7;
            result |= buffer[3];

            return result;
        }

        /// <summary>
        ///     ¯\_(ツ)_/¯
        /// </summary>
        public static int ReadInt24(this BinaryReader reader)
        {
            Span<byte> buffer = stackalloc byte[3];
            reader.Read(buffer);

            int result = 0;
            result |= buffer[0] << 16;
            result |= buffer[1] << 8;
            result |= buffer[2];

            return result;
        }
    }
}
