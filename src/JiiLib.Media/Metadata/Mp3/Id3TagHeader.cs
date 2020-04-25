using System;

namespace JiiLib.Media.Metadata.Mp3
{
    public readonly struct Id3Header
    {
        /// <summary>
        ///     The Major version of this ID3v2 tag
        /// </summary>
        public byte MajorVersion { get; }

        /// <summary>
        ///     The Minor version of this ID3v2 tag
        /// </summary>
        public byte MinorVersion { get; }

        /// <summary>
        ///     The flags that are set on this ID3v2 tag
        /// </summary>
        public Id3TagHeaderFlags Flags { get; }

        /// <summary>
        ///     The total size of the entire ID3v2 tag, excluding the header.
        ///     This is represented as the actual size converted from the "synchsafe" format.
        /// </summary>
        public int Size { get; }
        
        /// <summary>
        ///     Instantiates an object that represents an ID3v2 header.
        /// </summary>
        /// <param name="major">
        ///     The major verson.
        /// </param>
        /// <param name="minor">
        ///     The minor version.
        /// </param>
        /// <param name="flags">
        ///     The flags that should be set.
        /// </param>
        /// <param name="size">
        ///     The total size of the tag, as the raw bytes.
        /// </param>
        internal Id3Header(byte major, byte minor, Id3TagHeaderFlags flags, int size)
        {
            if ((flags & Id3TagHeaderFlags.Unsynchronisation) == Id3TagHeaderFlags.Unsynchronisation)
                throw new NotSupportedException("Unsynchronized tags are not supported by this library (yet) (maybe).");

            MajorVersion = major;
            MinorVersion = minor;
            Flags = flags;
            Size = size;
        }

        ///// <summary>
        /////     Creates an array of bytes that represents the header.
        ///// </summary>
        ///// <returns>
        /////     An array of bytes that represents the header.
        ///// </returns>
        //internal byte[] ToRawHeader()
        //    => new byte[] { 0x49, 0x44, 0x33, MajorVersion, MinorVersion, (byte)Flags }
        //        .Concat(Size).ToArray();
    }
}
