using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JiiLib.Media.Metadata.Mp3
{
    public class Id3v2Header
    {
        public byte MajorVersion { get; }
        public byte MinorVersion { get; }
        public Id3v2HeaderFlags Flags { get; }
        public byte[] Size { get; }
        public int IntSize
            => (Size[0] << 21) + (Size[1] << 14) + (Size[2] << 7) + Size[3];
        
        public Id3v2Header(byte major, byte minor, Id3v2HeaderFlags flags, byte[] size)
        {
            if (size.Length != 4) throw new ArgumentOutOfRangeException(nameof(size), "Argument has to be of length 4.");

            MajorVersion = major;
            MinorVersion = minor;
            Flags = flags;
            Size = size;
        }
    }

    [Flags]
    public enum Id3v2HeaderFlags
    {
        Unsynchronisation = 0x80,
        Extended          = 0x40,
        Experimental      = 0x20,
        Footer            = 0x10,
        None              = 0x00
    }
}
