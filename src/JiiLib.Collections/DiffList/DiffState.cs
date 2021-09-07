using System;

namespace JiiLib.Collections.DiffList
{
    /// <summary>
    ///     Indicates the state that two instances
    ///     of <see cref="DiffValue"/> differ in.
    /// </summary>
    public enum DiffState
    {
        /// <summary>
        ///     The value did not exist in either Old or New entries.
        /// </summary>
        NonExistent = 0,

        /// <summary>
        ///     The value exists only in the New entries.
        /// </summary>
        New,

        /// <summary>
        ///     The value exists in both and has
        ///     changed between Old and New entries.
        /// </summary>
        Changed,

        /// <summary>
        ///     The value exists in both and has <em><b>not</b></em>
        ///     changed between Old and New entries.
        /// </summary>
        Unchanged,

        /// <summary>
        ///     The value exists only in the Old entries.
        /// </summary>
        Removed
    }
}
