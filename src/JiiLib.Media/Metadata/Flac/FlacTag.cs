using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JiiLib.Media.Internal;
using JiiLib.Media.Metadata.Vorbis;

namespace JiiLib.Media.Metadata.Flac
{
    public sealed class FlacTag : VorbisComment<FlacFile>
    {
        public FlacTag(FlacFile file)
            : base(file)
        {
            ReadComments(file);
        }

        private FlacTag()
            : base(null) { }

        private string? _reference;
        private int _tags;

        /// <summary>
        ///     Creates a <see cref="FlacTag"/> tag from an existing tag.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static FlacTag FromTag(IMediaTag tag)
        {
            return new FlacTag
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
        }

        //public override void WriteTo(FlacFile file)
        //{
        //    throw new NotImplementedException();
        //}

        private void ReadComments(FlacFile file)
        {
            var dataBlock = GetDataBlock(file);
            if (dataBlock.Length == 0)
                throw new ArgumentException("No VorbisComment metadata block found.");


            using var ms = new MemoryStream(dataBlock, writable: false);
            using var reader = new BinaryReader(ms);

            while (ms.Position < ms.Length)
            {
                int length = reader.ReadInt32();

                Span<byte> tag = (length <= 64)
                    ? stackalloc byte[length]
                    : new byte[length];

                reader.Read(tag);
                var tagString = Enc.GetString(tag);

                if (tagString.StartsWith("reference", StringComparison.OrdinalIgnoreCase))
                {
                    _reference = tagString;
                    _tags = reader.ReadInt32();
                    continue;
                }

                var tagParts = tagString.Split('=');
                if (tagParts.Length == 2)
                {
                    AssignFields(tagParts[0], tagParts[1]);
                }
            }

            static byte[] GetDataBlock(FlacFile file)
            {
                using var fs = file.FileInfo.OpenRead();
                using var reader = new BinaryReader(fs);

                Span<byte> head = stackalloc byte[4];
                reader.Read(head);

                ReadOnlySpan<byte> flac = stackalloc byte[] { 0x66, 0x4C, 0x61, 0x43 }; //"fLaC"
                if (!flac.SequenceEqual(head))
                    throw new InvalidDataException("Data not a Flac tag.");

                bool lastBlock = false;
                do
                {
                    var blh = reader.ReadByte();
                    var blockType = (FlacMetadataBlockType)(blh % 128);
                    lastBlock = blh >= 128; // highest bit set = last block
                    var length = reader.ReadInt24();
                    var metadataBlock = new byte[length];
                    reader.Read(metadataBlock);

                    if (blockType == FlacMetadataBlockType.VorbisComment)
                        return metadataBlock;
                }
                while (!lastBlock);

                return Array.Empty<byte>(); // ¯\_(ツ)_/¯
            }
        }
    }
}
