using System;

namespace JiiLib.SimpleDsl
{
    /// <summary>
    ///     Specifies how the output text is desired to be formatted.
    /// </summary>
    [Flags]
    public enum FormatModifiers
    {
        /// <summary>
        ///     No formatting is applied.
        /// </summary>
        None = 0,

        /// <summary>
        ///     Bold formatting is applied.
        /// </summary>
        Bold = 1,

        /// <summary>
        ///     Italic formatting is applied.
        /// </summary>
        Italic = 1 << 1,

        /// <summary>
        ///     Underline formatting is applied.
        /// </summary>
        Underline = 1 << 2
    }
}
