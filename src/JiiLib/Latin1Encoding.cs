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

        public override string WebName => @"ISO-8859-1";

#if !DOTNET5_4
        public override int CodePage => 28591;
#endif
        public override int GetByteCount(char[] chars, int index, int count) => count;

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

        public override int GetCharCount(byte[] bytes, int index, int count) => count;
        
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

        public override int GetMaxByteCount(int charCount) => charCount;

        public override int GetMaxCharCount(int byteCount) => byteCount;
    }
}
