using System;

namespace JiiLib.Components
{
    /// <summary>
    ///     Provides the style classes for
    ///     a <see cref="DiffTable"/>.
    /// </summary>
    public interface IDiffTableStyle
    {
        /// <summary>
        ///     The class for the table.
        /// </summary>
        string TableClass { get; }

        /// <summary>
        ///     The class for the title.
        /// </summary>
        string TitleClass { get; }

        /// <summary>
        ///     The class for the table content.
        /// </summary>
        string? TableContentClass { get; }

        /// <summary>
        ///     The class for the Key column.
        /// </summary>
        string? ItemKeyClass { get; }

        /// <summary>
        ///     The class for the Old entries column.
        /// </summary>
        string? OldEntryClass { get; }

        /// <summary>
        ///     The class for the New entries column.
        /// </summary>
        string? NewEntryClass { get; }

        /// <summary>
        ///     The class for a <see cref="Diff.DiffValueCell"/>.
        /// </summary>
        string? DiffValueClass { get; }


        internal static IDiffTableStyle Default { get; }
            = new DefaultStyle();

        private sealed class DefaultStyle : IDiffTableStyle
        {
            public string TableClass { get; } = "table-bordered border-dark rounded";
            public string TitleClass { get; } = "border-bottom";
            public string? TableContentClass { get; }
            public string? ItemKeyClass { get; }
            public string? OldEntryClass { get; }
            public string? NewEntryClass { get; }
            public string? DiffValueClass { get; }
        }
    }
}
