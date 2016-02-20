using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JiiLib;

namespace JiiLib.Media.Metadata.Mp3
{
    /// <summary>
    /// Represents an ID3v2 tag, compliant with version 4.0.
    /// </summary>
    public class Id3v2 : Tag<Mp3File>
    {
        public Id3v2Header Header { get; }
        public IList<Id3Frame> Frames { get; }


        #region Ctors
        public Id3v2(Mp3File file)
        {
            Header = ReadHeader(file);
            Frames = (Header != null) ? ReadFrames(file, Header) : null;
        }

        private Id3v2()
        {
            Frames = new List<Id3Frame>();
        }

        /// <summary>
        /// Create an <see cref="Id3v2"/> tag from an existing tag.
        /// </summary>
        /// <typeparam name="TFile"></typeparam>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static Id3v2 FromTag<TFile>(Tag<TFile> tag) where TFile : MediaFile
        {
            return new Id3v2()
            {
                Title = tag.Title,
                Artist = tag.Artist,
                Year = tag.Year,
                // Genre = tag.Genre,
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

        #region Overridden Members
        /// <summary>
        /// Value of the Title field
        /// </summary>
        public override string Title
        {
            get { return Frames.FirstOrDefault(f => f.FrameHeader == Constants.TIT2)?.AsString(); }
            protected set
            {
                var tmp = Frames.FirstOrDefault(f => f.FrameHeader == Constants.TIT2);
                if (tmp != null)
                {
                    Frames.Remove(tmp);
                }
                if (value != null)
                {
                    var v = new byte[] { 0x01, 0xFF, 0xFE }.Concat(Encoding.Unicode.GetBytes(value)).Concat(new byte[] { 0x00, 0x00 }).ToArray();
                    Frames.Add(new Id3Frame(Constants.TIT2, v, tmp?.Flags ?? new byte[2]));
                }
            }
        }

        /// <summary>
        /// Value of the Artist field
        /// </summary>
        public override string Artist
        {
            get { return Frames.FirstOrDefault(f => f.FrameHeader == Constants.TPE1)?.AsString(); }
            protected set
            {
                var tmp = Frames.FirstOrDefault(f => f.FrameHeader == Constants.TPE1);
                if (tmp != null)
                {
                    Frames.Remove(tmp);
                }
                if (value != null)
                {
                    var v = new byte[] { 0x01, 0xFF, 0xFE }.Concat(Encoding.Unicode.GetBytes(value)).Concat(new byte[] { 0x00, 0x00 }).ToArray();
                    Frames.Add(new Id3Frame(Constants.TPE1, v, tmp?.Flags ?? new byte[2]));
                }
            }
        }

        /// <summary>
        /// Value of the Year field
        /// </summary>
        public override int? Year
        {
            get
            {
                int i;
                return Int32.TryParse(Frames.FirstOrDefault(f => f.FrameHeader == Constants.TDRC)?.AsString()
                    ?? Frames.FirstOrDefault(f => f.FrameHeader == "TDAT")?.AsString()
                    ?? Frames.FirstOrDefault(f => f.FrameHeader == "TIME")?.AsString() //check for obsolete fields
                    ?? Frames.FirstOrDefault(f => f.FrameHeader == "TRDA")?.AsString()
                    ?? Frames.FirstOrDefault(f => f.FrameHeader == "TYER")?.AsString(), out i) ? i : (int?)null;
            }
            protected set
            {
                var tmp = Frames.FirstOrDefault(f => f.FrameHeader == Constants.TDRC)
                    ?? Frames.FirstOrDefault(f => f.FrameHeader == "TDAT")
                    ?? Frames.FirstOrDefault(f => f.FrameHeader == "TIME") //check for obsolete fields
                    ?? Frames.FirstOrDefault(f => f.FrameHeader == "TRDA")
                    ?? Frames.FirstOrDefault(f => f.FrameHeader == "TYER");
                if (tmp != null)
                {
                    Frames.Remove(tmp);
                }
                if (value.HasValue)
                {
                    var v = new byte[] { 0x01, 0xFF, 0xFE }.Concat(Encoding.Unicode.GetBytes(value.ToString())).Concat(new byte[] { 0x00, 0x00 }).ToArray();
                    Frames.Add(new Id3Frame(Constants.TDRC, v, tmp?.Flags ?? new byte[2]));
                }
            }
        }

        /// <summary>
        /// Value of the Genre field
        /// </summary>
        public override string Genre
        {
            get
            {
                throw new NotImplementedException();
            }

            protected set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Value of the Album field
        /// </summary>
        public override string Album
        {
            get { return Frames.FirstOrDefault(f => f.FrameHeader == Constants.TALB)?.AsString(); }
            protected set
            {
                var tmp = Frames.FirstOrDefault(f => f.FrameHeader == Constants.TALB);
                if (tmp != null)
                {
                    Frames.Remove(tmp);
                }
                if (value != null)
                {
                    var v = new byte[] { 0x01, 0xFF, 0xFE }.Concat(Encoding.Unicode.GetBytes(value)).Concat(new byte[] { 0x00, 0x00 }).ToArray();
                    Frames.Add(new Id3Frame(Constants.TALB, v, tmp?.Flags ?? new byte[2]));
                }
            }
        }

        /// <summary>
        /// Value of the AlbumArtist field
        /// </summary>
        public override string AlbumArtist
        {
            get { return Frames.FirstOrDefault(f => f.FrameHeader == Constants.TPE2)?.AsString(); }
            protected set
            {
                var tmp = Frames.FirstOrDefault(f => f.FrameHeader == Constants.TPE2);
                if (tmp != null)
                {
                    Frames.Remove(tmp);
                }
                if (value != null)
                {
                    var v = new byte[] { 0x01, 0xFF, 0xFE }.Concat(Encoding.Unicode.GetBytes(value)).Concat(new byte[] { 0x00, 0x00 }).ToArray();
                    Frames.Add(new Id3Frame(Constants.TPE2, v, tmp?.Flags ?? new byte[2]));
                }
            }
        }

        /// <summary>
        /// Value of the TrackNumber field
        /// </summary>
        public override int? TrackNumber
        {
            get
            {
                int i;
                return Int32.TryParse(Frames.FirstOrDefault(f => f.FrameHeader == Constants.TRCK)?
                    .AsString()?.Split('/')?[0], out i) ? i : (int?)null;
            }
            protected set
            {
                var tmp = Frames.FirstOrDefault(f => f.FrameHeader == Constants.TRCK);
                string res = null;
                if (tmp != null)
                {
                    res = tmp.AsString().Split('/')[1];
                    Frames.Remove(tmp);
                }
                if (value.HasValue)
                {
                    var v = new byte[] { 0x01, 0xFF, 0xFE }.Concat(Encoding.Unicode.GetBytes($"{value}/{res}")).Concat(new byte[] { 0x00, 0x00 }).ToArray();
                    Frames.Add(new Id3Frame(Constants.TRCK, v, tmp?.Flags ?? new byte[2]));
                }
            }
        }

        /// <summary>
        /// Value of the TotalTracks field
        /// </summary>
        public override int? TotalTracks
        {
            get
            {
                int i;
                return Int32.TryParse((Frames.FirstOrDefault(f => f.FrameHeader == Constants.TRCK)?
                    .AsString()?.Split('/')?[1]), out i) ? i : (int?)null;
            }
            protected set
            {
                var tmp = Frames.FirstOrDefault(f => f.FrameHeader == Constants.TRCK);
                string res = null;
                if (tmp != null)
                {
                    res = tmp.AsString().Split('/')[0];
                    Frames.Remove(tmp);
                }
                if (value.HasValue)
                {
                    var v = new byte[] { 0x01, 0xFF, 0xFE }.Concat(Encoding.Unicode.GetBytes($"{res}/{value}")).Concat(new byte[] { 0x00, 0x00 }).ToArray();
                    Frames.Add(new Id3Frame(Constants.TRCK, v, tmp?.Flags ?? new byte[2]));
                }
            }
        }

        /// <summary>
        /// Value of the DiscNumber field
        /// </summary>
        public override int? DiscNumber
        {
            get
            {
                int i;
                return Int32.TryParse(Frames.FirstOrDefault(f => f.FrameHeader == Constants.TPOS)?
                    .AsString()?.Split('/')?[0], out i) ? i : (int?)null;
            }
            protected set
            {
                var tmp = Frames.FirstOrDefault(f => f.FrameHeader == Constants.TPOS);
                string res = null;
                if (tmp != null)
                {
                    res = tmp.AsString().Split('/')[1];
                    Frames.Remove(tmp);
                }
                if (value.HasValue)
                {
                    var v = new byte[] { 0x01, 0xFF, 0xFE }.Concat(Encoding.Unicode.GetBytes($"{value}/{res}")).Concat(new byte[] { 0x00, 0x00 }).ToArray();
                    Frames.Add(new Id3Frame(Constants.TPOS, v, tmp?.Flags ?? new byte[2]));
                }
            }
        }

        /// <summary>
        /// Value of the TotalDiscs field
        /// </summary>
        public override int? TotalDiscs
        {
            get
            {
                int i;
                return Int32.TryParse(Frames.FirstOrDefault(f => f.FrameHeader == Constants.TPOS)?
                    .AsString()?.Split('/')?[1], out i) ? i : (int?)null;
            }
            protected set
            {
                var tmp = Frames.FirstOrDefault(f => f.FrameHeader == Constants.TPOS);
                string res = null;
                if (tmp != null)
                {
                    res = tmp.AsString().Split('/')[0];
                    Frames.Remove(tmp);
                }
                if (value.HasValue)
                {
                    var v = new byte[] { 0x01, 0xFF, 0xFE }.Concat(Encoding.Unicode.GetBytes($"{res}/{value}")).Concat(new byte[] { 0x00, 0x00 }).ToArray();
                    Frames.Add(new Id3Frame(Constants.TPOS, v, tmp?.Flags ?? new byte[2]));
                }
            }
        }

        /// <summary>
        /// Value of the Comment field
        /// </summary>
        public override string Comment
        {
            get { return Frames.FirstOrDefault(f => f.FrameHeader == Constants.COMM)?.AsString(); }
            protected set
            {
                var tmp = Frames.FirstOrDefault(f => f.FrameHeader == Constants.COMM);
                if (tmp != null)
                {
                    Frames.Remove(tmp);
                }
                if (value != null)
                {
                    var v = new byte[] { 0x01, 0xFF, 0xFE }.Concat(Encoding.Unicode.GetBytes(value)).Concat(new byte[] { 0x00, 0x00 }).ToArray();
                    Frames.Add(new Id3Frame(Constants.COMM, v, tmp?.Flags ?? new byte[2]));
                }
            }
        }

        public override void WriteTo(Mp3File file)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));

            int totalSize = 0;
            var utf8 = Encoding.UTF8;
            var rawTags = new List<byte[]>();
            foreach (var frame in Frames)
            {
                var chars = frame.FrameHeader.ToCharArray();
                var head = new byte[4];
                int i, j;
                bool b;
                utf8.GetEncoder().Convert(chars, 0, 4, head, 0, 4, true, out i, out j, out b);
                var b4 = (byte)(frame.RawContent.Length & 127);
                var b3 = (byte)((frame.RawContent.Length >> 7) & 127);
                var b2 = (byte)((frame.RawContent.Length >> 14) & 127);
                var b1 = (byte)((frame.RawContent.Length >> 21) & 127);
                var sz = new byte[4] { b1, b2, b3, b4 };
                var fullTag = head.Concat(sz).Concat(frame.Flags).Concat(frame.RawContent).ToArray();
                rawTags.Add(fullTag);
                totalSize += fullTag.Length;
            }
            
            var h4 = (byte)(totalSize & 127);
            var h3 = (byte)((totalSize >> 7) & 127);
            var h2 = (byte)((totalSize >> 14) & 127);
            var h1 = (byte)((totalSize >> 21) & 127);
            var header = Header ?? ReadHeader(file);
            header = new Id3v2Header(4, 0, ( header == null ? Id3v2HeaderFlags.None : header.Flags), new byte[4] { h1, h2, h3, h4 });

            var temp = File.ReadAllBytes(file.Path);
            using (FileStream fs = new FileStream(file.Path, FileMode.Truncate, FileAccess.Write))
            {
                var rawHead = header.ToRawHeader();
                fs.Write(rawHead, 0, rawHead.Length);
                foreach (var item in rawTags)
                {
                    fs.Write(item, 0, item.Length);
                }
                fs.Write(temp, 0, temp.Length);
            }
        }
        #endregion

        #region Private Methods
        private static Id3v2Header ReadHeader(Mp3File file)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));

            var head = new byte[10];
            byte major;
            byte minor;
            byte[] size;
            Id3v2HeaderFlags flags;
            using (FileStream fs = new FileStream(file.Path, FileMode.Open, FileAccess.Read))
            {
                fs.Read(head, 0, 10);
            }

            if (!head.StartsWith(new byte[] { 0x49, 0x44, 0x33 })) //"ID3"
            {
                return null;
            }

            major = head[3];
            minor = head[4];
            //flags = Enum.Parse(typeof(Id3v2HeaderFlags), ((int)buff[5]).ToString());
            Enum.TryParse(((int)head[5]).ToString(), out flags);
            size = new byte[4] { head[6], head[7], head[8], head[9] };
            return new Id3v2Header(major, minor, flags, size);
        }

        private static IList<Id3Frame> ReadFrames(Mp3File file, Id3v2Header header)
        {
            var frames = new List<Id3Frame>();
            var buffer = new byte[header.IntSize];

            using (FileStream fs = new FileStream(file.Path, FileMode.Open, FileAccess.Read))
            {
                fs.Position = 10;
                fs.Read(buffer, 0, header.IntSize);
            }

            using (MemoryStream ms = new MemoryStream(buffer, writable: false))
            {
                var frameHead = new byte[10];

                while (ms.Position < ms.Length)
                {
                    ms.Read(frameHead, 0, 10);
                    string desc = new String(new char[] { (char)frameHead[0], (char)frameHead[1], (char)frameHead[2], (char)frameHead[3], });
                    int size = (frameHead[4] << 21) + (frameHead[5] << 14) + (frameHead[6] << 7) + frameHead[7];
                    byte[] flags = new byte[2] { frameHead[8], frameHead[9] };

                    byte[] content = new byte[size];
                    ms.Read(content, 0, size);
                    if (Id3Frame.frameDict.Keys.Contains(desc))
                    {
                        // Console.WriteLine($"Found frame: {desc} - {content}");
                        frames.Add(new Id3Frame(desc, content, flags));
                    }
                }
            }

            return frames;
        }
        #endregion
    }

    public class Mp3File : MediaFile
    {
        public Mp3File(string path) : base(path) { }
    }
}
