using System;

namespace JiiLib.Media.Internal
{
    internal readonly struct Optional<T>
    {
        private readonly bool _isSpecified;
        private readonly T _value;

        public Optional(T value)
        {
            _isSpecified = true;
            _value = value;
        }

        public bool IsSpecified(out T value)
        {
            value = _value;
            return _isSpecified;
        }
    }
}
