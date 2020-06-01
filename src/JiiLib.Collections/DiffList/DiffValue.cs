using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace JiiLib.Collections
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public sealed class DiffValue
    {
        private static readonly IEqualityComparer<string> _comparer = StringComparer.OrdinalIgnoreCase;

        /// <summary>
        ///     Indicates if this instance contains only a single value.
        /// </summary>
        public bool IsSingleValue => Values.Length == 0;

        /// <summary>
        ///     The single value this instance tracks
        ///     if <see cref="IsSingleValue"/> returns <see langword="true"/>.
        /// </summary>
        public string Value { get; } = String.Empty;

        /// <summary>
        ///     The set of values this instance tracks
        ///     if <see cref="IsSingleValue"/> returns <see langword="false"/>.
        /// </summary>
        public ImmutableArray<string> Values { get; } = ImmutableArray<string>.Empty;

        /// <summary>
        ///     Initializes a new <see cref="DiffValue"/>
        ///     with a single value.
        /// </summary>
        /// <param name="value">
        ///     The value to track.
        /// </param>
        public DiffValue(string value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        ///     Initializes a new <see cref="DiffValue"/>
        ///     with a set of values.
        /// </summary>
        /// <param name="values">
        ///     The values to track.
        /// </param>
        public DiffValue(IEnumerable<string> values)
        {
            Values = values.ToImmutableArray();
        }

        /// <summary>
        ///     Creates a new <see cref="DiffValue"/> with the provided
        ///     <paramref name="value"/> appended.
        /// </summary>
        /// <param name="value">
        ///     The value to add.
        /// </param>
        public DiffValue Add(string value)
            => IsSingleValue
                ? new DiffValue(new[] { Value, value })
                : new DiffValue(Values.Append(value));

        /// <summary>
        ///     Creates a new <see cref="DiffValue"/> with the provided
        ///     <paramref name="values"/> appended.
        /// </summary>
        /// <param name="values">
        ///     The values to add.
        /// </param>
        public DiffValue Add(IEnumerable<string> values)
            => IsSingleValue
                ? new DiffValue(new[] { Value }.Concat(values))
                : new DiffValue(Values.Concat(values));

        /// <summary>
        ///     Creates a new <see cref="DiffValue"/> with the provided
        ///     <paramref name="value"/> removed.
        /// </summary>
        /// <param name="value">
        ///     The value to remove.
        /// </param>
        /// <exception cref="InvalidOperationException">
        ///     This instance only carries a single value.
        /// </exception>
        public DiffValue Remove(string value)
        {
            if (IsSingleValue)
                throw new InvalidOperationException("Cannot remove from a single-value instance");

            if (!Values.Contains(value, _comparer))
                return this;

            var builder = Values.ToBuilder();
            builder.RemoveAt(builder.IndexOf(value, 0, builder.Count, _comparer));

            return new DiffValue(builder);
        }

        /// <summary>
        ///     Creates a new <see cref="DiffValue"/> with the provided
        ///     <paramref name="values"/> removed.
        /// </summary>
        /// <param name="values">
        ///     The values to remove.
        /// </param>
        /// <exception cref="InvalidOperationException">
        ///     This instance only carries a single value
        ///     - OR -
        ///     All the values from this instance were taken.
        /// </exception>
        public DiffValue Remove(IEnumerable<string> values)
        {
            if (IsSingleValue)
                throw new InvalidOperationException("Cannot remove from a single-value instance");

            var builder = Values.ToBuilder();
            foreach (var val in values)
            {
                int index = builder.IndexOf(val, 0, builder.Count, _comparer);
                if (index < 0)
                    continue;

                builder.RemoveAt(index);

                if (builder.Count == 0)
                    throw new InvalidOperationException("Cannot remove all values from an instance.");
            }

            return new DiffValue(builder);
        }

        /// <summary>
        ///     Compares two <see cref="DiffValue"/>s and
        ///     determines the state they differ in.
        /// </summary>
        /// <param name="oldValue">
        ///     The old value.
        /// </param>
        /// <param name="newValue">
        ///     The new value.
        /// </param>
        public static DiffState GetDiffState(DiffValue? oldValue, DiffValue? newValue)
        {
            return (oldValue, newValue) switch
            {
                (null, null) => DiffState.NonExistent,
                (null, _)    => DiffState.New,
                (_, null)    => DiffState.Removed,

                var (o, n) => (o.IsSingleValue, n.IsSingleValue) switch
                {
                    (true, true) => (_comparer.Equals(o.Value, n.Value)
                        ? DiffState.Unchanged : DiffState.Changed),

                    (false, false) when o.Values.SequenceEqual(n.Values)
                        => DiffState.Unchanged,

                    _ => DiffState.Changed
                }
            };
        }

        private DiffValue(ImmutableArray<string>.Builder builder)
        {
            if (builder.Count > 1)
                Values = builder.ToImmutable();
            else
                Value = builder[0];
        }

        private string DebuggerDisplay => IsSingleValue
            ? $"Single: ({Value})"
            : $"Multi: [Count = {Values.Length}]";
    }
}
