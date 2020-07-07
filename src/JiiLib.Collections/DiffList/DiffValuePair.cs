using System;

namespace JiiLib.Collections.DiffList
{
    /// <summary>
    ///     Correlates a pair of <see cref="DiffValue"/>s
    ///     along with the key they belong to.
    /// </summary>
    public readonly struct DiffValuePair<TKey>
        where TKey : notnull
    {
        /// <summary>
        ///     The key that this pair of values belong to.
        /// </summary>
        public TKey Key { get; }

        /// <summary>
        ///     The Old entry value.
        /// </summary>
        public DiffValue? OldValue { get; }

        /// <summary>
        ///     The New entry value.
        /// </summary>
        public DiffValue? NewValue { get; }

        internal DiffValuePair(TKey key, DiffValue? oldValue, DiffValue? newValue)
        {
            Key = key;
            OldValue = oldValue;
            NewValue = newValue;
        }

        ///// <summary>
        /////     Short-cut method for <see cref="DiffValue.GetDiffState(DiffValue?, DiffValue?)"/>
        ///// </summary>
        ///// <returns>
        /////     The <see cref="DiffState"/> of the two values.
        ///// </returns>
        //private DiffState GetDiffState() => DiffValue.GetDiffState(OldValue, NewValue);

        /// <summary>
        ///     Deconstructs this instance to a tuple.
        /// </summary>
        public void Deconstruct(out TKey key, out DiffValue? oldValue, out DiffValue? newValue)
        {
            key = Key;
            oldValue = OldValue;
            newValue = NewValue;
        }

        /// <summary>
        ///     Deconstructs this instance to a tuple
        ///     <em>with</em> a pre-calculated <see cref="DiffState"/>.
        /// </summary>
        public void Deconstruct(out TKey key, out DiffValue? oldValue, out DiffValue? newValue, out DiffState diffState)
        {
            key = Key;
            oldValue = OldValue;
            newValue = NewValue;
            diffState = DiffValue.GetDiffState(oldValue, newValue);
        }
    }
}
