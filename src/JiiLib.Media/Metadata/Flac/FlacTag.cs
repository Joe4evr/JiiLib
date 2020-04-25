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
        {
            ReadComments(file);
        }

        private FlacTag() { }

        private string _reference;

        /// <summary>
        ///     Creates a <see cref="FlacTag"/> tag from an existing tag.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static FlacTag FromTag(MediaTag tag)
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
            using var ms = new MemoryStream(dataBlock, writable: false);
            using var reader = new BinaryReader(ms);

            while (ms.Position < ms.Length)
            {
                int l = reader.ReadInt32();

                Span<byte> tag = (l <= 64)
                    ? stackalloc byte[l]
                    : new byte[l];

                reader.Read(tag);
                var s = Enc.GetString(tag);

                if (s.StartsWith("reference", StringComparison.OrdinalIgnoreCase))
                {
                    _reference = s;
                    reader.ReadBytes(4); //need to advance 4 extra bytes for some reason ¯\_(ツ)_/¯
                    continue;
                }

                var tagParts = s.Split('=');
                if (tagParts.Length == 2)
                {
                    AssignFields(tagParts[0], tagParts[1]);
                }
            }

            static byte[] GetDataBlock(FlacFile file)
            {
                using var fs = file.File.OpenRead();
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
                    var blockType = blh % 128;
                    lastBlock = blh >= 128;
                    var length = reader.ReadInt24();
                    var metadataBlock = new byte[length];
                    reader.Read(metadataBlock);

                    if (blockType == 4)
                        return metadataBlock;
                }
                while (!lastBlock);

                return Array.Empty<byte>();
            }
        }
    }

    public sealed class FlacFile : VorbisFile
    {
        public FlacFile(FileInfo fileInfo) : base(fileInfo) { }
        public FlacFile(string path) : base(path) { }
    }
}
