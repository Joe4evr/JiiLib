using System;

namespace JiiLib.Components
{
    /// <summary>
    ///     Provides the style classes for
    ///     a <see cref="CollapsableBox"/>.
    /// </summary>
    public interface ICollapsableBoxStyle
    {
        /// <summary>
        ///     The class for the outer box.
        /// </summary>
        string OuterBoxClass { get; }

        /// <summary>
        ///     The class for the title.
        /// </summary>
        string TitleClass { get; }

        /// <summary>
        ///     The class for the content box.
        /// </summary>
        string ContentClass { get; }

        internal static ICollapsableBoxStyle Default { get; }
            = new DefaultStyle();

        private sealed class DefaultStyle : ICollapsableBoxStyle
        {
            public string OuterBoxClass { get; } = "w-50";
            public string TitleClass { get; } = "p-2";
            public string ContentClass { get; } = "pr-3";
        }
    }
}
