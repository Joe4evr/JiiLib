using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JiiLib.Media.Metadata.Mp3
{
    public class Id3v2Header
    {
        /// <summary>
        /// The Major version of this ID3v2 tag
        /// </summary>
        public byte MajorVersion { get; }

        /// <summary>
        /// The Minor version of this ID3v2 tag
        /// </summary>
        public byte MinorVersion { get; }


        /// <summary>
        /// The flags that are set on this ID3v2 tag
        /// </summary>
        public Id3v2HeaderFlags Flags { get; }

        /// <summary>
        /// The total size of the entire ID3v2 tag, excluding the header. This is represented as the actual bytes in ID3v2's "synchsafe" format.
        /// For the actual size, use <see cref="IntSize"/>.
        /// </summary>
        internal byte[] Size { get; }

        /// <summary>
        /// The total size of the entire ID3v2 tag, excluding the header. This is represented as the actual size converted from the "synchsafe" format.
        /// </summary>
        public int IntSize => (Size[0] << 21) + (Size[1] << 14) + (Size[2] << 7) + Size[3];
        
        /// <summary>
        /// Instantiates an object that represents an ID3v2 header.
        /// </summary>
        /// <param name="major">The major verson</param>
        /// <param name="minor">The minor version</param>
        /// <param name="flags">The flags that should be set</param>
        /// <param name="size">The total size of the tag, as the raw bytes</param>
        internal Id3v2Header(byte major, byte minor, Id3v2HeaderFlags flags, byte[] size)
        {
            if (size.Length != 4) throw new ArgumentOutOfRangeException(nameof(size), "Argument has to be of length 4.");

            MajorVersion = major;
            MinorVersion = minor;
            Flags = flags;
            Size = size;
        }

        /// <summary>
        /// Creates an array of bytes that represents the header.
        /// </summary>
        /// <returns>An array of bytes that represents the header.</returns>
        internal byte[] ToRawHeader()
            => new byte[] { 0x49, 0x44, 0x33, MajorVersion, MinorVersion, (byte)Flags }.Concat(Size).ToArray();
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
