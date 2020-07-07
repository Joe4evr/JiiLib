//using System;
//using System.IO;
//using System.Linq;
//using JiiLib.Media.Internal;
//using JiiLib.Media.Metadata.Vorbis;

//namespace JiiLib.Media.Metadata.Ogg
//{
//    public sealed class OggTag : VorbisComment<OggFile>
//    {
//        public OggTag(OggFile file)
//            : base(file)
//        {
//            ReadComments(file);
//        }

//        private OggTag()
//            : base(null) { }

//        /// <summary>
//        ///     Creates a <see cref="OggTag"/> tag from an existing tag.
//        /// </summary>
//        /// <param name="tag"></param>
//        /// <returns></returns>
//        public static OggTag FromTag(IMediaTag tag)
//        {
//            return new OggTag
//            {
//                Title = tag.Title,
//                Artist = tag.Artist,
//                Year = tag.Year,
//                Genre = tag.Genre,
//                Album = tag.Album,
//                AlbumArtist = tag.AlbumArtist,
//                TrackNumber = tag.TrackNumber,
//                TotalTracks = tag.TotalTracks,
//                DiscNumber = tag.DiscNumber,
//                TotalDiscs = tag.TotalDiscs,
//                Comment = tag.Comment
//            };
//        }

//        //public override void WriteTo(OggFile file)
//        //{
//        //    throw new NotImplementedException();
//        //}

//        private void ReadComments(OggFile file)
//        {

//            static byte[] GetDataBlock(OggFile file)
//            {
//                using var fs = file.FileInfo.OpenRead();
//                using var reader = new BinaryReader(fs);

//                Span<byte> head = stackalloc byte[3];
//                reader.Read(head);

//                ReadOnlySpan<byte> ogg = stackalloc byte[] { 0x4F, 0x67, 0x67 }; //"Ogg"
//                if (!ogg.SequenceEqual(head))
//                    throw new InvalidDataException("Data not an Ogg tag.");

//                //int tags;
//                //Span<byte> headField = stackalloc byte[7];
//                //reader.Read(headField);
//                //int packetType = headField[0];
//                //Span<byte> idHeader = stackalloc byte[23];
//                //reader.Read(idHeader);
//                //Span<byte> setupHeader = stackalloc byte[44];
//                //reader.Read(setupHeader);

//                //var commentsLength = setupHeader.LastIndexOf(0xFF);
//                //var comments = new byte[commentsLength];
//                //fs.Read(comments, 0, commentsLength);
//            }

//            //var headBuffer = new byte[28];



//            //using (var ms = new MemoryStream(comments, writable: false))
//            //{
//            //    var first = true;
//            //    ms.Read(new byte[7], 0, 7);
//            //    while (ms.Position < ms.Length)
//            //    {
//            //        var h = new byte[4];
//            //        ms.Read(h, 0, 4);
//            //        int l = (h[3] << 24) + (h[2] << 16) + (h[1] << 8) + h[0];

//            //        var tag = new byte[l];
//            //        ms.Read(tag, 0, l);
//            //        var s = new String(enc.GetChars(tag));

//            //        var t = s.Split('=');
//            //        if (t.Length == 2)
//            //        {
//            //            AssignFields(t[0], t[1]);
//            //        }

//            //        if (first)
//            //        {
//            //            ms.Read(new byte[4], 0, 4);
//            //            first = false;
//            //        }
//            //    }
//            //}
//        }
//    }
//}
