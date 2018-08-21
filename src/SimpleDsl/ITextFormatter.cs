using System;

namespace JiiLib.SimpleDsl
{
    /// <summary>
    ///     Represents a contract for formatting text.
    /// </summary>
    public interface ITextFormatter
    {
        /// <summary>
        ///     Formats a string to be displayed a certain way.
        /// </summary>
        /// <param name="value">
        ///     The input string.
        /// </param>
        /// <param name="formats">
        ///     The desired formats.
        /// </param>
        /// <returns>
        ///     A string that will be displayed in the specified format on the target environment.
        /// </returns>
        string FormatString(string value, FormatModifiers formats);
    }

    /// <summary>
    ///     An implementation of <see cref="ITextFormatter"/> to use markdown formatting.
    /// </summary>
    public sealed class MarkdownFormatter : ITextFormatter
    {
        public static ITextFormatter Instance { get; } = new MarkdownFormatter();

        private MarkdownFormatter() { }

        /// <inheritdoc />
        string ITextFormatter.FormatString(string value, FormatModifiers formats)
        {
            if ((formats & FormatModifiers.Bold) == FormatModifiers.Bold)
                value = $"**{value}**";

            if ((formats & FormatModifiers.Italic) == FormatModifiers.Italic)
                value = $"*{value}*";

            if ((formats & FormatModifiers.Underline) == FormatModifiers.Underline)
                value = $"__{value}__";

            return value;
        }
    }
}
