using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JiiLib.Media.Metadata.Vorbis;

namespace JiiLib.Media.Metadata.Flac
{
    public class FlacTag : VorbisComment<FlacFile>
    {
        #region ctors
        public FlacTag(FlacFile file)
        {
            ReadComments(file);
        }

        private FlacTag()
        {
        }

        /// <summary>
        /// Create an <see cref="FlacTag"/> tag from an existing tag.
        /// </summary>
        /// <typeparam name="TFile"></typeparam>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static FlacTag FromTag<TFile>(Tag<TFile> tag) where TFile : MediaFile
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
        #endregion

        public override void WriteTo(FlacFile file)
        {
            throw new NotImplementedException();
        }

        private void ReadComments(FlacFile file)
        {
            var headBuffer = new byte[4];

            using (FileStream fs = new FileStream(file.Path, FileMode.Open, FileAccess.Read))
            {
                fs.Read(headBuffer, 0, 4);
                if (!headBuffer.StartsWith(new byte[] { 0x66, 0x4C, 0x61, 0x43 })) //"fLaC"
                {
                    throw new InvalidDataException("File was not in a correct format.");
                }

                var artists = new List<string>();
                bool lastBlock = false;
                while (!lastBlock)
                {
                    var blh = (byte)fs.ReadByte();
                    var blockType = blh % 128;
                    var len = new byte[3];
                    lastBlock = blh >= 128;
                    fs.Read(len, 0, 3);
                    var length = (len[0] << 16) + (len[1] << 8) + len[2];
                    var metadataBlock = new byte[length];
                    fs.Read(metadataBlock, 0, length);

                    if (blockType == 4)
                    {
                        using (MemoryStream ms = new MemoryStream(metadataBlock, writable: false))
                        {
                            var first = true;
                            while (ms.Position < ms.Length)
                            {
                                var h = new byte[4];
                                ms.Read(h, 0, 4);
                                int l = (h[3] << 24) + (h[2] << 16) + (h[1] << 8) + h[0];

                                var tag = new byte[l];
                                ms.Read(tag, 0, l);
                                var s = new String(enc.GetChars(tag));

                                var t = s.Split('=');
                                if (t.Length == 2)
                                {
                                    AssignFields(t[0], t[1]);
                                }

                                if (first)
                                {
                                    ms.Read(new byte[4], 0, 4);
                                    first = false;
                                }
                            }
                        }
                    }
                }
                _artists = artists;
            }
        }
    }

    public class FlacFile : VorbisFile
    {
        public FlacFile(string path) : base(path) { }
    }
}
