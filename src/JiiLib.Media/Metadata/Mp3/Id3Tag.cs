using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JiiLib.Media.Internal;

namespace JiiLib.Media.Metadata.Mp3
{
    /// <summary>
    ///     Represents an ID3 tag, compliant with version 2.3.
    /// </summary>
    public sealed partial class Id3Tag
    {
        public Id3Header Header { get; }
        public Dictionary<string, Id3Frame> Frames { get; }

        /// <summary>
        ///     Instantiates a new <see cref="Id3Tag"/> object to
        ///     represent an MP3 file's ID3 tags.
        /// </summary>
        /// <param name="file">
        ///     The file to read.
        /// </param>
        /// <param name="backCompat">
        ///     Specify whether or not to set this instance
        ///     to ID3v2.3 standards. Defaults to false.
        /// </param>
        public Id3Tag(Mp3File file)
        {
            Header = ReadHeader(file);
            Frames = (Header.Size > 0)
                ? ReadFrames(file, Header)
                : new Dictionary<string, Id3Frame>();
        }

        private Id3Tag()
        {
            //_backCompat = backCompat;
            Frames = new Dictionary<string, Id3Frame>();
        }

        /// <summary>
        ///     Create an <see cref="Id3Tag"/> tag from an existing tag and
        ///     copies the most common properties from it to this instance.
        /// </summary>
        /// <param name="tag">
        ///     An existing <see cref="MediaTag{TFile}"/>.
        /// </param>
        /// <param name="backCompat">
        ///     Specify whether or not to set this instance
        ///     to ID3v2.3 standards. Defaults to false.
        /// </param>
        /// <returns>
        ///     An <see cref="Id3Tag"/> tag with several properties set.
        /// </returns>
        public static Id3Tag FromTag(MediaTag tag)
            => new Id3Tag()
                {
                    Title = tag.Title,
                    Artist = tag.Artist,
                    Year = tag.Year,
                    Genre = tag.Genre,
                    Album = tag.Album,
                    AlbumArtist = tag.AlbumArtist,
                    TrackNumber = tag.TrackNumber,
                    TotalTracks = tag.TotalTracks,
                    DiscNumber = tag.DiscNumber,
                    TotalDiscs = tag.TotalDiscs,
                    Comment = tag.Comment
                };

        // private static readonly byte[] _stringPrefix = new byte[] { 0x00 };
        // private static readonly byte[] _stringPostfix = new byte[] { 0x00 };
        // private static readonly Encoding _latin1 = Encoding.GetEncoding("iso-8859-1");

        //private static byte[] EncodeString(string value)
        //{

        //}

        ///// <summary>
        /////     Write the ID3v2 tag to an <see cref="Mp3File"/>.
        ///// </summary>
        ///// <param name="file">
        /////     The MP3 file to write to.
        ///// </param>
        ///// <remarks>
        /////     If the "backCompat" flag when creating this instance
        /////     was set to "true", will write an ID3v2.3 tag.
        ///// </remarks>
        //public override void WriteTo(Mp3File file)
        //{
        //    if (file == null) throw new ArgumentNullException(nameof(file));

        //    int totalSize = 0;
        //    var utf8 = Encoding.UTF8;
        //    var rawTags = new List<byte[]>();
        //    foreach (var frame in Frames)
        //    {
        //        var chars = frame.FrameHeader.ToCharArray();
        //        var head = new byte[4];
        //        int i, j;
        //        bool b;
        //        utf8.GetEncoder().Convert(chars, 0, 4, head, 0, 4, true, out i, out j, out b);
        //        var b4 = (byte)(frame.RawContent.Length & 127);
        //        var b3 = (byte)((frame.RawContent.Length >> 7) & 127);
        //        var b2 = (byte)((frame.RawContent.Length >> 14) & 127);
        //        var b1 = (byte)((frame.RawContent.Length >> 21) & 127);
        //        var sz = new byte[4] { b1, b2, b3, b4 };
        //        var fullTag = head.Concat(sz).Concat(frame.Flags).Concat(frame.RawContent).ToArray();
        //        rawTags.Add(fullTag);
        //        totalSize += fullTag.Length;
        //    }

        //    var h4 = (byte)(totalSize & 127);
        //    var h3 = (byte)((totalSize >> 7) & 127);
        //    var h2 = (byte)((totalSize >> 14) & 127);
        //    var h1 = (byte)((totalSize >> 21) & 127);
        //    var header = Header ?? ReadHeader(file);
        //    header = new Id3v2Header(
        //        (byte)(_backCompat ? 3 : 4),
        //        0,
        //        (header == null ? Id3v2HeaderFlags.None : header.Flags),
        //        new byte[4] { h1, h2, h3, h4 });

        //    var temp = File.ReadAllBytes(file.Path);

        //    using var fs = new FileStream(file.Path, FileMode.Truncate, FileAccess.Write);
        //    var rawHead = header.ToRawHeader();
        //    fs.Write(rawHead, 0, rawHead.Length);
        //    foreach (var item in rawTags)
        //    {
        //        fs.Write(item, 0, item.Length);
        //    }
        //    fs.Write(temp, 0, temp.Length);
        //}

        private static Id3Header ReadHeader(Mp3File file)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));

            using var fs = file.File.OpenRead();
            using var reader = new BinaryReader(fs);

            Span<byte> head = stackalloc byte[3];
            reader.Read(head);

            ReadOnlySpan<byte> id3 = stackalloc byte[] { 0x49, 0x44, 0x33 }; //"ID3"
            if (!id3.SequenceEqual(head))
                throw new InvalidOperationException("Data not an ID3 tag.");

            byte major = reader.ReadByte();
            byte minor = reader.ReadByte();
            var flags = (Id3TagHeaderFlags)reader.ReadByte();
            int size = reader.ReadSynchInt();
            return new Id3Header(major, minor, flags, size);
        }

        private static Dictionary<string, Id3Frame> ReadFrames(Mp3File file, Id3Header header)
        {
            var frames = new Dictionary<string, Id3Frame>();
            var buffer = new byte[header.Size];

            using (var fs = file.File.OpenRead())
            {
                fs.Position = 10;
                fs.Read(buffer);
            }

            using (var ms = new MemoryStream(buffer, writable: false))
            using (var reader = new BinaryReader(ms))
            {
                while (ms.Position < ms.Length)
                {
                    var headerId = new string(reader.ReadChars(4));
                    var frameSize = reader.ReadSynchInt();
                    var flags = (Id3FrameFlags)reader.ReadUInt16();

                    byte[] content = reader.ReadBytes(frameSize);

                    if (HeaderIds.FrameDict.ContainsKey(headerId))
                    {
                        frames[headerId] = Id3Frame.FromContent(
                            Id3FrameId.Create(headerId), content, flags);
                    }
                }
            }

            return frames;
        }
    }

    public sealed class Mp3File : MediaFile
    {
        public Mp3File(FileInfo fileInfo) : base(fileInfo) { }
        public Mp3File(string path) : base(path) { }
    }
}
