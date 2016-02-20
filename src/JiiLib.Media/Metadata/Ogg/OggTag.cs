using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JiiLib.Media.Metadata.Vorbis;

namespace JiiLib.Media.Metadata.Ogg
{
    public class OggTag : VorbisComment<OggFile>
    {
        public OggTag(OggFile file)
        {
            ReadComments(file);
        }

        private OggTag()
        {
        }
        /// <summary>
        /// Create an <see cref="OggTag"/> tag from an existing tag.
        /// </summary>
        /// <typeparam name="TFile"></typeparam>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static OggTag FromTag<TFile>(Tag<TFile> tag) where TFile : MediaFile
        {
            return new OggTag
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

        public override void WriteTo(OggFile file)
        {
            throw new NotImplementedException();
        }

        private void ReadComments(OggFile file)
        {
            var headBuffer = new byte[28];

            using (FileStream fs = new FileStream(file.Path, FileMode.Open, FileAccess.Read))
            {
                fs.Read(headBuffer, 0, 28);
                if (!headBuffer.StartsWith(new byte[] { 0x4F, 0x67, 0x67 })) //"Ogg"
                {
                    throw new InvalidDataException("File was not in a correct format.");
                }

                //int tags;
                var headField = new byte[7];
                fs.Read(headField, 0, 7);
                int packetType = headField[0];
                var idHeader = new byte[23];
                fs.Read(idHeader, 0, 23);
                var setupHeader = new byte[44];
                fs.Read(setupHeader, 0, 44);
                var commentsLength = (setupHeader.TakeWhile(c => c != 0xFF).Last()) - 1;
                var comments = new byte[commentsLength];
                fs.Read(comments, 0, commentsLength);
                
                using (MemoryStream ms = new MemoryStream(comments, writable: false))
                {
                    var first = true;
                    ms.Read(new byte[7], 0, 7);
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
    }

    public class OggFile : VorbisFile
    {
        public OggFile(string path) : base(path) { }
    }
}
