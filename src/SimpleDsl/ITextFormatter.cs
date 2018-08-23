using System;

namespace JiiLib.SimpleDsl
{
    /// <summary>
    ///     Represents a contract for formatting text.
    /// </summary>
    public interface ITextFormats
    {
        /// <summary>
        ///     
        /// </summary>
        string BoldOpen { get; }

        /// <summary>
        ///     
        /// </summary>
        string BoldClose { get; }

        /// <summary>
        ///     
        /// </summary>
        string ItalicOpen { get; }

        /// <summary>
        ///     
        /// </summary>
        string ItalicClose { get; }

        /// <summary>
        ///     
        /// </summary>
        string UnderlineOpen { get; }

        /// <summary>
        ///     
        /// </summary>
        string UnderlineClose { get; }
    }

    /// <summary>
    ///     An implementation of <see cref="ITextFormats"/> to use markdown formatting.
    /// </summary>
    public sealed class MarkdownFormatter : ITextFormats
    {
        /// <summary>
        ///     The <see cref="MarkdownFormatter"/> instance.
        /// </summary>
        public static ITextFormats Instance { get; } = new MarkdownFormatter();

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

        private MarkdownFormatter() { }
    }

    /// <summary>
    ///     An implementation of <see cref="ITextFormats"/> to use HTML formatting.
    /// </summary>
    public sealed class HtmlFormatter : ITextFormats
    {
        /// <summary>
        ///     The <see cref="HtmlFormatter"/> instance.
        /// </summary>
        public static ITextFormats Instance { get; } = new HtmlFormatter();

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

        private HtmlFormatter() { }
    }
}
