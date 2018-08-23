using System;

namespace JiiLib.SimpleDsl
{
    /// <summary>
    ///     Represents a contract for formatting text.
    /// </summary>
    public interface ITextFormats
    {
        string BoldOpen { get; }
        string BoldClose { get; }
        string ItalicOpen { get; }
        string ItalicClose { get; }
        string UnderlineOpen { get; }
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

        string ITextFormats.BoldOpen { get; } = "**";
        string ITextFormats.BoldClose { get; } = "**";
        string ITextFormats.ItalicOpen { get; } = "*";
        string ITextFormats.ItalicClose { get; } = "*";
        string ITextFormats.UnderlineOpen { get; } = "__";
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

        string ITextFormats.BoldOpen { get; } = "<b>";
        string ITextFormats.BoldClose { get; } = "</b>";
        string ITextFormats.ItalicOpen { get; } = "<i>";
        string ITextFormats.ItalicClose { get; } = "</i>";
        string ITextFormats.UnderlineOpen { get; } = "<u>";
        string ITextFormats.UnderlineClose { get; } = "</u>";

        private HtmlFormatter() { }
    }
}
