using System;

namespace JiiLib.Media.Metadata.Mp3
{
    /// <summary>
    ///     Represents the header of an ID3 frame.
    /// </summary>
    public readonly struct Id3FrameHeader
    {
        /// <summary>
        ///     The ID for the corresponding frame.
        /// </summary>
        public Id3FrameId FrameId { get; }

        /// <summary>
        ///     The size of the corresponding frame.
        /// </summary>
        public int FrameSize { get; }

        /// <summary>
        ///     The flags that were set on the corresponding frame.
        /// </summary>
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
