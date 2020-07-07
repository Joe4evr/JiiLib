using System;

namespace JiiLib.Media.Metadata.Mp3
{
    /// <summary>
    ///     Represents the id of an ID3 frame.
    /// </summary>
    public readonly struct Id3FrameId : IEquatable<Id3FrameId>
    {
        /// <summary>
        ///     The string value of this frame ID.
        /// </summary>
        public string IdString { get; }

        private Id3FrameId(string value)
        {
            IdString = value;
        }

        internal static Id3FrameId Create(string value)
        {
            if (!HeaderIds.FrameDict.ContainsKey(value))
                throw new ArgumentException(message: "Unknown frame ID value");

            return new Id3FrameId(value);
        }

        public bool Equals(Id3FrameId other)
            => String.Equals(IdString, other.IdString);

        public override bool Equals(object obj)
            => obj is Id3FrameId frameId && Equals(frameId);

        public override int GetHashCode()
            => IdString?.GetHashCode() ?? 0;

        public static bool operator ==(Id3FrameId left, Id3FrameId right)
            => left.Equals(right);

        public static bool operator !=(Id3FrameId left, Id3FrameId right)
            => !(left == right);

        public static bool operator ==(Id3FrameId left, string right)
            => String.Equals(left.IdString, right);

        public static bool operator !=(Id3FrameId left, string right)
            => !(left == right);

        public static bool operator ==(string left, Id3FrameId right)
            => String.Equals(left, right.IdString);

        public static bool operator !=(string left, Id3FrameId right)
            => !(left == right);
    }
}
