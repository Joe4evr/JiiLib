using System;
using System.Collections.Immutable;

namespace JiiLib.Collections.DiffList
{
    public sealed partial class DiffValue
    {
        /// <summary>
        /// 
        /// </summary>
        public Enumerator GetEnumerator() => new Enumerator(this);

        /// <summary>
        /// 
        /// </summary>
        public struct Enumerator
        {
            private readonly DiffValue _dv;
            private ImmutableArray<string>.Enumerator _enum;

            /// <summary>
            /// 
            /// </summary>
            public string Current { get; private set; }

            internal Enumerator(DiffValue dv)
                : this()
            {
                if (dv is { IsSingleValue: false })
                {
                    _enum = dv.Values.GetEnumerator();
                }

                _dv = dv;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns>
            /// </returns>
            public bool MoveNext()
            {
                if (_dv is { IsSingleValue: true }
                    && Current is null)
                {
                    Current = _dv.Value;
                    return true;
                }
                else if (_dv is { IsSingleValue: false }
                    && _enum.MoveNext())
                {
                    Current = _enum.Current;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
