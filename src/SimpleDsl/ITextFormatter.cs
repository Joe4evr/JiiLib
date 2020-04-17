using System;

namespace JiiLib.SimpleDsl
{
    /// <summary>
    ///     Represents a contract for formatting text.
    /// </summary>
    public interface ITextFormats
    {
        /// <summary>
        ///     The <see cref="HtmlFormatter"/> instance.
        /// </summary>
        public static ITextFormats Html { get; } = new HtmlFormatter();

        /// <summary>
        ///     The <see cref="MarkdownFormatter"/> instance.
        /// </summary>
        public static ITextFormats Markdown { get; } = new MarkdownFormatter();

        /// <summary>
        ///     String that defines the start of a bold section of text.
        /// </summary>
        string BoldOpen { get; }

        /// <summary>
        ///     String that defines the end of a bold section of text.
        /// </summary>
        string BoldClose { get; }

        /// <summary>
        ///     String that defines the start of a italic section of text.
        /// </summary>
        string ItalicOpen { get; }

        /// <summary>
        ///     String that defines the end of a italic section of text.
        /// </summary>
        string ItalicClose { get; }

        /// <summary>
        ///     String that defines the start of a underlined section of text.
        /// </summary>
        string UnderlineOpen { get; }

        /// <summary>
        ///     String that defines the end of a underlined section of text.
        /// </summary>
        string UnderlineClose { get; }

        /// <summary>
        ///     An implementation of <see cref="ITextFormats"/> to use markdown formatting.
        /// </summary>
        private sealed class MarkdownFormatter : ITextFormats
        {
            /// <inheritdoc />
            string ITextFormats.BoldOpen { get; } = "**";
            /// <inheritdoc />
            string ITextFormats.BoldClose { get; } = "**";
            /// <inheritdoc />
            string ITextFormats.ItalicOpen { get; } = "*";
            /// <inheritdoc />
            string ITextFormats.ItalicClose { get; } = "*";
            /// <inheritdoc />
            string ITextFormats.UnderlineOpen { get; } = "__";
            /// <inheritdoc />
            string ITextFormats.UnderlineClose { get; } = "__";
        }

        /// <summary>
        ///     An implementation of <see cref="ITextFormats"/> to use HTML formatting.
        /// </summary>
        private sealed class HtmlFormatter : ITextFormats
        {
            /// <inheritdoc />
            string ITextFormats.BoldOpen { get; } = "<b>";
            /// <inheritdoc />
            string ITextFormats.BoldClose { get; } = "</b>";
            /// <inheritdoc />
            string ITextFormats.ItalicOpen { get; } = "<i>";
            /// <inheritdoc />
            string ITextFormats.ItalicClose { get; } = "</i>";
            /// <inheritdoc />
            string ITextFormats.UnderlineOpen { get; } = "<u>";
            /// <inheritdoc />
            string ITextFormats.UnderlineClose { get; } = "</u>";
        }
    }
}
