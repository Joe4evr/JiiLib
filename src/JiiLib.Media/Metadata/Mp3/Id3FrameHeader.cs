using System;

namespace JiiLib.Media.Metadata.Mp3
{
    public readonly struct Id3FrameHeader
    {
        public Id3FrameId FrameId { get; }
        public int FrameSize { get; }
        public Id3FrameFlags Flags { get; }

        //internal bool IsTextHeader => HeaderIds.Text.Contains(FrameId.IdString);
        //internal bool IsNumericHeader => HeaderIds.Numeric.Contains(FrameId.IdString);

        internal Id3FrameHeader(Id3FrameId frameId, int frameSize, Id3FrameFlags flags)
        {
            FrameId = frameId;
            FrameSize = frameSize;
            Flags = flags;
        }
    }
}
