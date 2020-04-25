using System;

namespace JiiLib.Media.Metadata.Mp3
{
    [Flags]
    public enum Id3TagHeaderFlags : byte
    {
        None              = 0x00,
        Footer            = 0x10,
        Experimental      = 0x20,
        Extended          = 0x40,
        Unsynchronisation = 0x80
    }
}
