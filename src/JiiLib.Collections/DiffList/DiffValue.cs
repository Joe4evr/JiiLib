using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace JiiLib.Collections.DiffList
{
    /// <summary>
    ///     Represents one or more <see cref="String"/>
    ///     values that can be tracked for changes.
    /// </summary>
    /// <remarks>
    ///     <note type="info">
    ///         All strings are compared for equality using
    ///         <see cref="StringComparer.OrdinalIgnoreCase"/>.
    ///     </note>
    /// </remarks>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public sealed partial class DiffValue : IEquatable<DiffValue>
    {
        private static readonly IEqualityComparer<string> _comparer = StringComparer.OrdinalIgnoreCase;

        /// <summary>
        ///     Indicates if this instance contains only a single value.
        /// </summary>
        public bool IsSingleValue => _values.Length == 0;


        /// <summary>
        ///     The single value this instance tracks
        ///     if <see cref="IsSingleValue"/> returns <see langword="true"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///     <see cref="IsSingleValue"/> was <see langword="false"/>.
        /// </exception>
        public string Value => IsSingleValue ? _value
            : throw new InvalidOperationException("DiffValue instance was not single-value.");

        /// <summary>
        ///     The set of values this instance tracks
        ///     if <see cref="IsSingleValue"/> returns <see langword="false"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///     <see cref="IsSingleValue"/> was <see langword="true"/>.
        /// </exception>
        public ImmutableArray<string> Values => !IsSingleValue ? _values
            : throw new InvalidOperationException("DiffValue instance was not multi-value.");

        private readonly string _value = String.Empty;
        private readonly ImmutableArray<string> _values = ImmutableArray<string>.Empty;

        /// <summary>
        ///     Initializes a new <see cref="DiffValue"/>
        ///     with a single value.
        /// </summary>
        /// <param name="value">
        ///     The value to track.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="value"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="value"/> is the empty string.
        /// </exception>
        public DiffValue(string value)
        {
            if (value is null)
                throw new ArgumentNullException(paramName: nameof(value));
            if (value == String.Empty)
                throw new ArgumentException(paramName: nameof(value),
                    message: "Value may not be empty string.");

            _value = value;
        }

        /// <summary>
        ///     Initializes a new <see cref="DiffValue"/>
        ///     with a set of values.
        /// </summary>
        /// <param name="values">
        ///     The values to track.
        /// </param>
        /// <remarks>
        ///     <note type="info">
        ///         This constructor will filter out
        ///         <see langword="null"/>s and empty strings.
        ///     </note>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="values"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="values"/> did not contain
        ///     any values after filtering.
        /// </exception>
        public DiffValue(IEnumerable<string> values)
        {
            if (values is null)
                throw new ArgumentNullException(paramName: nameof(values));

            var vs = values.Where(v => !String.IsNullOrEmpty(v))
                .ToImmutableArray();

            if (vs.Length == 0)
                throw new ArgumentException(paramName: nameof(values),
                    message: "Values may not be empty.");
            else if (vs.Length == 1)
                _value = vs[0];
            else
                _values = vs;
        }

        /// <summary>
        ///     Creates a new <see cref="DiffValue"/> with the provided
        ///     <paramref name="value"/> appended.
        /// </summary>
        /// <param name="value">
        ///     The value to add.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="value"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="value"/> is the empty string.
        /// </exception>
        public DiffValue Add(string value)
        {
            if (String.IsNullOrEmpty(value))
                return this;

            return IsSingleValue
                ? new DiffValue(new[] { Value, value })
                : new DiffValue(Values.Append(value));
        }

        /// <summary>
        ///     Creates a new <see cref="DiffValue"/> with the provided
        ///     <paramref name="values"/> appended.
        /// </summary>
        /// <param name="values">
        ///     The values to add.
        /// </param>
        /// <remarks>
        ///     <para>
        ///         This method will filter out
        ///         <see langword="null"/>s and empty strings.
        ///     </para>
        ///     <para>
        ///         
        ///     </para>
        /// </remarks>
        public DiffValue Add(IEnumerable<string> values)
        {
            if (values is null)
                throw new ArgumentNullException(paramName: nameof(values));

            var vs = values.Where(v => !String.IsNullOrEmpty(v))
                .ToArray();

            if (vs.Length == 0)
                return this;
            else 
                return IsSingleValue
                    ? new DiffValue(new[] { Value }.Concat(vs))
                    : new DiffValue(Values.Concat(vs));
        }

        /// <summary>
        ///     Creates a new <see cref="DiffValue"/> with the provided
        ///     <paramref name="value"/> removed.
        /// </summary>
        /// <param name="value">
        ///     The value to remove.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="value"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     This instance only carries a single value.
        /// </exception>
        public DiffValue Remove(string value)
        {
            if (value is null)
                throw new ArgumentNullException(paramName: nameof(value));

            if (IsSingleValue)
                throw new InvalidOperationException("Cannot remove from a single-value DiffValue instance.");

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
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="values"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     This instance only carries a single value.
        ///     <br/>-OR-<br/>
        ///     All the values from this instance were removed.
        /// </exception>
        public DiffValue Remove(IEnumerable<string> values)
        {
            if (values is null)
                throw new ArgumentNullException(paramName: nameof(values));

            if (IsSingleValue)
                throw new InvalidOperationException("Cannot remove from a single-value DiffValue instance.");

            var builder = Values.ToBuilder();
            foreach (var val in values)
            {
                int index = builder.IndexOf(val, 0, builder.Count, _comparer);
                if (index < 0)
                    continue;

                builder.RemoveAt(index);

                if (builder.Count == 0)
                    throw new InvalidOperationException("Cannot remove all values from a DiffValue instance.");
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
        /// <returns>
        ///     The <see cref="DiffState"/> of the two values.
        /// </returns>
        public static DiffState GetDiffState(DiffValue? oldValue, DiffValue? newValue)
        {
            return (oldValue, newValue) switch
            {
                (null, null) => DiffState.NonExistent,
                (null, _)    => DiffState.New,
                (_, null)    => DiffState.Removed,
                var (o, n) => EqualsCore(o, n) ? DiffState.Unchanged : DiffState.Changed
            };
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj switch
            {
                DiffValue dv => EqualsCore(this, dv),
                _ => false
            };
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            var hash = new HashCode();

            foreach (var val in this)
            {
                hash.Add(val, _comparer);
            }

            return hash.ToHashCode();
        }

        /// <inheritdoc/>
        public bool Equals(DiffValue? other)
        {
            return other switch
            {
                null => false,
                _ => EqualsCore(this, other)
            };
        }

        private static bool Equals(DiffValue? left, DiffValue? right)
        {
            return (left, right) switch
            {
                (null, null) => true,
                (null, _) or (_, null) => false,
                var (l, r) => EqualsCore(l, r)
            };
        }

        private static bool EqualsCore(DiffValue left, DiffValue right)
        {
            return ReferenceEquals(left, right)
                || ((left.IsSingleValue, right.IsSingleValue) switch
                    {
                        (true, true) => _comparer.Equals(left.Value, right.Value),
                        (false, false) => left.Values.SequenceEqual(right.Values, _comparer),
                        _ => false
                    });
        }

        private DiffValue(ImmutableArray<string>.Builder builder)
        {
            if (builder.Count == 1)
                _value = builder[0];
            else
                _values = builder.ToImmutable();
        }

        private string DebuggerDisplay() => IsSingleValue
            ? $"Single: ({Value})"
            : $"Multi: [Count = {Values.Length}]";

        /// <summary>
        ///     Compares two <see cref="DiffValue"/>s for equality.
        /// </summary>
        /// <param name="left">
        ///     The left value.
        /// </param>
        /// <param name="right">
        ///     The right value.
        /// </param>
        /// <returns>
        ///     <see langword="true"/> if the two values are
        ///     the same, or structurally equal;
        ///     otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator ==(DiffValue? left, DiffValue? right) => Equals(left, right);

        /// <summary>
        ///     Compares two <see cref="DiffValue"/>s for inequality.
        /// </summary>
        /// <param name="left">
        ///     The left value.
        /// </param>
        /// <param name="right">
        ///     The right value.
        /// </param>
        /// <returns>
        ///     <see langword="true"/> if the two
        ///     values are structurally unequal;
        ///     otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator !=(DiffValue? left, DiffValue? right) => !Equals(left, right);
    }
}
