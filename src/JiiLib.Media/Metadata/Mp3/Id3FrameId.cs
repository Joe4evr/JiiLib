using System;

namespace JiiLib.Media.Metadata.Mp3
{
    public readonly struct Id3FrameId
    {
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
    }
}
