using System;

namespace JiiLib.Media.Metadata.Flac
{
    public enum FlacMetadataBlockType : byte
    {
        StreamInfo = 0,
        Padding = 1,
        Application = 2,
        SeekTable = 3,
        VorbisComment = 4,
        CueSheet = 5,
        Picture = 6,
        // remaining values reserved
        Invalid = 127
    }
}
