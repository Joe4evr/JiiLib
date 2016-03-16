using System;
using System.Text;

namespace JiiLib
{
    /// <summary>
    /// Represents an ISO-8895-1 encoding of Unicode characters.
    /// </summary>
    /// <remarks>Taken from http://stackoverflow.com/a/24618827, slightly modified to use C# 6 features.</remarks>
    public class Latin1Encoding : Encoding
    {
        private readonly string m_specialCharset = (char)0xA0 + @"¡¢£¤¥¦§¨©ª«¬­®¯°±²³´µ¶·¸¹º»¼½¾¿ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþÿ";

        /// <summary>
        /// Gets the name registered with the Internet Assigned Numbers Authority(IANA) for the current encoding.
        /// </summary>
        public override string WebName => @"ISO-8859-1";

#if !DOTNET5_4
        /// <summary>
        /// Gets the code page identifier of the current Encoding.
        /// </summary>
        public override int CodePage => 28591;

#endif
        /// <summary>
        /// Calculates the number of bytes produced by encoding a set of characters from the specified character array.
        /// </summary>
        /// <param name="chars">The character array containing the set of characters to encode./param>
        /// <param name="index">The index of the first character to encode.</param>
        /// <param name="count">The number of characters to encode.</param>
        /// <returns>The number of bytes produced by encoding the specified characters.</returns>
        public override int GetByteCount(char[] chars, int index, int count) => count;

        /// <summary>
        /// Encodes a set of characters from the specified character array into the specified byte array.
        /// </summary>
        /// <param name="chars">The character array containing the set of characters to encode.</param>
        /// <param name="charIndex">The index of the first character to encode.</param>
        /// <param name="charCount">The number of characters to encode.</param>
        /// <param name="bytes">The byte array to contain the resulting sequence of bytes.</param>
        /// <param name="byteIndex">The index at which to start writing the resulting sequence of bytes.</param>
        /// <returns>The actual number of bytes written into bytes.</returns>
        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            if (chars == null) throw new ArgumentNullException(nameof(chars), @"null array");
            if (bytes == null) throw new ArgumentNullException(nameof(bytes), @"null array");
            if (charIndex < 0) throw new ArgumentOutOfRangeException(nameof(charIndex));
            if (charCount < 0) throw new ArgumentOutOfRangeException(nameof(charCount));
            if (chars.Length - charIndex < charCount) throw new ArgumentOutOfRangeException(nameof(chars));
            if (byteIndex < 0 || byteIndex > bytes.Length) throw new ArgumentOutOfRangeException(nameof(byteIndex));

            for (int i = 0; i < charCount; i++)
            {
                char ch = chars[charIndex + i];
                int chVal = ch;
                bytes[byteIndex + i] = chVal < 160 ? (byte)ch : (chVal <= byte.MaxValue ? (byte)m_specialCharset[chVal - 160] : (byte)63);
            }

            return charCount;
        }

        /// <summary>
        /// Calculates the number of characters produced by decoding a sequence of bytes from the specified byte array.
        /// </summary>
        /// <param name="bytes">The byte array containing the sequence of bytes to decode.</param>
        /// <param name="index">The index of the first byte to decode.</param>
        /// <param name="count">The number of bytes to decode.</param>
        /// <returns>The number of characters produced by decoding the specified sequence of bytes.</returns>
        public override int GetCharCount(byte[] bytes, int index, int count) => count;

        /// <summary>
        /// Decodes a sequence of bytes from the specified byte array into the specified character array.
        /// </summary>
        /// <param name="bytes">The byte array containing the sequence of bytes to decode. </param>
        /// <param name="byteIndex">The index of the first byte to decode.</param>
        /// <param name="byteCount">The number of bytes to decode.</param>
        /// <param name="chars">The character array to contain the resulting set of characters.</param>
        /// <param name="charIndex">The index at which to start writing the resulting set of characters.</param>
        /// <returns>The actual number of characters written into chars.</returns>
        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            if (chars == null) throw new ArgumentNullException(nameof(chars), @"null array");
            if (bytes == null) throw new ArgumentNullException(nameof(bytes), @"null array");
            if (byteIndex < 0) throw new ArgumentOutOfRangeException(nameof(byteIndex));
            if (byteCount < 0) throw new ArgumentOutOfRangeException(nameof(byteCount));
            if (bytes.Length - byteIndex < byteCount) throw new ArgumentOutOfRangeException(nameof(bytes));
            if (charIndex < 0 || charIndex > chars.Length) throw new ArgumentOutOfRangeException(nameof(charIndex));

            for (int i = 0; i < byteCount; ++i)
            {
                byte b = bytes[byteIndex + i];
                chars[charIndex + i] = b < 160 ? (char)b : m_specialCharset[b - 160];
            }

            return byteCount;
        }

        /// <summary>
        /// Calculates the maximum number of bytes produced by encoding the specified number of characters.
        /// </summary>
        /// <param name="charCount">The number of characters to encode. </param>
        /// <returns>The maximum number of bytes produced by encoding the specified number of characters.</returns>
        public override int GetMaxByteCount(int charCount) => charCount;

        /// <summary>
        /// Calculates the maximum number of characters produced by decoding the specified number of bytes.
        /// </summary>
        /// <param name="byteCount">The number of bytes to decode.</param>
        /// <returns>The maximum number of characters produced by decoding the specified number of bytes.</returns>
        public override int GetMaxCharCount(int byteCount) => byteCount;
    }
}
