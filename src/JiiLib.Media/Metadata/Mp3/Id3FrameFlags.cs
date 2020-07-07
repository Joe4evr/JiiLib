using System;

namespace JiiLib.Media.Metadata.Mp3
{
    /// <summary>
    ///     S
    /// </summary>
    [Flags]
    public enum Id3FrameFlags : ushort
    {
        None = 0,

        //DiscardTagIfAltered = 1 << 15,
        //DiscardFileIfAltered = 1 << 14,
        //ReadOnly = 1 << 13,

        //ZlibCompressed = 1 << 7,
        //Encrypted = 1 << 6,
        //Grouped = 1 << 5
    }
}
