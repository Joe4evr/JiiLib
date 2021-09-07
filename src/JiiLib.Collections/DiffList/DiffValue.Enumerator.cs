using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace JiiLib.Collections.DiffList
{
    public sealed partial class DiffValue
    {
        /// <summary>
        /// 
        /// </summary>
        public Enumerator GetEnumerator() => new(this);

        /// <summary>
        /// 
        /// </summary>
        public struct Enumerator
        {
            private readonly DiffValue _dv;
            private ImmutableArray<string>.Enumerator _enum;
            private string? _current;

            /// <summary>
            /// 
            /// </summary>
            public string Current => _current!;

            internal Enumerator(DiffValue dv)
                : this()
            {
                _enum = dv._values.GetEnumerator();
                _dv = dv;
                _current = null;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns>
            /// </returns>
#if NET5_0_OR_GREATER
            [MemberNotNullWhen(true, nameof(Current))]
#endif
            public bool MoveNext()
            {
                return _dv switch
                {
                    { IsSingleValue: true }
                        when Interlocked.CompareExchange(ref _current, _dv.Value, null) is null => true,
                    { IsSingleValue: false } when _enum.MoveNext()
                        => (_current = _enum.Current) is { },
                    _ => false
                };
            }
        }
    }
}
