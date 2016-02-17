using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JiiLib;

namespace JiiLib.Media.Mp3
{
    /// <summary>
    /// 
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

        #region Overridden Members
        /// <summary>
        /// Value of the Title field
        /// </summary>
        public override string Title
        {
            get { return Frames.FirstOrDefault(f => f.FrameHeader == Constants.TIT2)?.FrameContent; }
            protected set
            {
                var tmp = Frames.FirstOrDefault(f => f.FrameHeader == Constants.TIT2);
                if (tmp != null)
                {
                    Frames.Remove(tmp);
                }
                if (value != null)
                {
                    Frames.Add(new Id3Frame(Constants.TIT2, value, tmp.Flags));
                }
            }
        }

        /// <summary>
        /// Value of the Artist field
        /// </summary>
        public override string Artist
        {
            get { return Frames.FirstOrDefault(f => f.FrameHeader == Constants.TPE1)?.FrameContent; }
            protected set
            {
                var tmp = Frames.FirstOrDefault(f => f.FrameHeader == Constants.TPE1);
                if (tmp != null)
                {
                    Frames.Remove(tmp);
                }
                if (value != null)
                {
                    Frames.Add(new Id3Frame(Constants.TPE1, value, tmp.Flags));
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
                return Int32.TryParse(Frames.FirstOrDefault(f => f.FrameHeader == Constants.TDRC)?.FrameContent
                    ?? Frames.FirstOrDefault(f => f.FrameHeader == "TDAT")?.FrameContent
                    ?? Frames.FirstOrDefault(f => f.FrameHeader == "TIME")?.FrameContent //check for obsolete fields
                    ?? Frames.FirstOrDefault(f => f.FrameHeader == "TRDA")?.FrameContent
                    ?? Frames.FirstOrDefault(f => f.FrameHeader == "TYER")?.FrameContent, out i) ? i : (int?)null;
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
                    Frames.Add(new Id3Frame(Constants.TDRC, value.ToString(), tmp.Flags));
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
            get { return Frames.FirstOrDefault(f => f.FrameHeader == Constants.TALB)?.FrameContent; }
            protected set
            {
                var tmp = Frames.FirstOrDefault(f => f.FrameHeader == Constants.TALB);
                if (tmp != null)
                {
                    Frames.Remove(tmp);
                }
                if (value != null)
                {
                    Frames.Add(new Id3Frame(Constants.TALB, value, tmp.Flags));
                }
            }
        }

        /// <summary>
        /// Value of the AlbumArtist field
        /// </summary>
        public override string AlbumArtist
        {
            get { return Frames.FirstOrDefault(f => f.FrameHeader == Constants.TPE2)?.FrameContent; }
            protected set
            {
                var tmp = Frames.FirstOrDefault(f => f.FrameHeader == Constants.TPE2);
                if (tmp != null)
                {
                    Frames.Remove(tmp);
                }
                if (value != null)
                {
                    Frames.Add(new Id3Frame(Constants.TPE2, value, tmp.Flags));
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
                    .FrameContent?.Split('/')?[0], out i) ? i : (int?)null;
            }
            protected set
            {
                var tmp = Frames.FirstOrDefault(f => f.FrameHeader == Constants.TRCK);
                string res = null;
                if (tmp != null)
                {
                    res = tmp.FrameContent.Split('/')[1];
                    Frames.Remove(tmp);
                }
                if (value.HasValue || res.Length > 0)
                {
                    Frames.Add(new Id3Frame(Constants.TRCK, $"{value}/{res}", tmp.Flags));
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
                return Int32.TryParse(Frames.FirstOrDefault(f => f.FrameHeader == Constants.TRCK)?
                    .FrameContent?.Split('/')?[1], out i) ? i : (int?)null;
            }
            protected set
            {
                var tmp = Frames.FirstOrDefault(f => f.FrameHeader == Constants.TRCK);
                string res = null;
                if (tmp != null)
                {
                    res = tmp.FrameContent.Split('/')[0];
                    Frames.Remove(tmp);
                }
                if (value.HasValue || res.Length > 0)
                {
                    Frames.Add(new Id3Frame(Constants.TRCK, $"{res}/{value}", tmp.Flags));
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
                    .FrameContent?.Split('/')?[0], out i) ? i : (int?)null;
            }
            protected set
            {
                var tmp = Frames.FirstOrDefault(f => f.FrameHeader == Constants.TPOS);
                string res = null;
                if (tmp != null)
                {
                    res = tmp.FrameContent.Split('/')[1];
                    Frames.Remove(tmp);
                }
                if (value.HasValue || res.Length > 0)
                {
                    Frames.Add(new Id3Frame(Constants.TPOS, $"{value}/{res}", tmp.Flags));
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
                    .FrameContent?.Split('/')?[1], out i) ? i : (int?)null;
            }
            protected set
            {
                var tmp = Frames.FirstOrDefault(f => f.FrameHeader == Constants.TPOS);
                string res = null;
                if (tmp != null)
                {
                    res = tmp.FrameContent.Split('/')[0];
                    Frames.Remove(tmp);
                }
                if (value.HasValue || res.Length > 0)
                {
                    Frames.Add(new Id3Frame(Constants.TPOS, $"{res}/{value}", tmp.Flags));
                }
            }
        }

        /// <summary>
        /// Value of the Comment field
        /// </summary>
        public override string Comment
        {
            get { return Frames.FirstOrDefault(f => f.FrameHeader == Constants.COMM)?.FrameContent; }
            protected set
            {
                var tmp = Frames.FirstOrDefault(f => f.FrameHeader == Constants.COMM);
                if (tmp != null)
                {
                    Frames.Remove(tmp);
                }
                if (value != null)
                {
                    Frames.Add(new Id3Frame(Constants.COMM, value, tmp.Flags));
                }
            }
        }

        public override void WriteTo(Mp3File file)
        {
            throw new NotImplementedException();
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
                    int size = ((frameHead[4] << 24) / 2) + ((frameHead[5] << 16) / 2) + ((frameHead[6] << 8) / 2) + frameHead[7];
                    byte[] flags = new byte[2] { frameHead[8], frameHead[9] };

                    byte[] temp = new byte[size];
                    ms.Read(temp, 0, size);
                    string content;
                    if (Id3Frame.frameDict.Keys.Contains(desc))
                    {
                        if (desc.StartsWith("T"))
                        {
                            Encoding enc;
                            switch (temp[0])
                            {
                                case 0:
                                    enc = new ASCIIEncoding();
                                    content = new String(enc.GetChars(temp.Skip(1).ToArray()).Skip(1).TakeWhile(c => c != 0x00).ToArray());
                                    break;
                                case 1:
                                    enc = new UnicodeEncoding(bigEndian: false, byteOrderMark: true);
                                    if (temp[1] != 0xFF && temp[2] != 0xFE) continue;
                                    content = new String(enc.GetChars(temp.Skip(1).ToArray()).Skip(1).TakeWhile(c => c != 0x0000).ToArray());
                                    break;
                                case 2:
                                    enc = new UnicodeEncoding(bigEndian: true, byteOrderMark: false);
                                    if (temp[1] != 0xFE && temp[2] != 0xFF) continue;
                                    content = new String(enc.GetChars(temp.Skip(1).ToArray()).Skip(1).TakeWhile(c => c != 0x0000).ToArray());
                                    break;
                                case 3:
                                    enc = new UTF8Encoding();
                                    content = new String(enc.GetChars(temp.Skip(1).ToArray()).Skip(1).TakeWhile(c => c != 0x00).ToArray());
                                    break;
                                default:
                                    throw new InvalidOperationException();
                            }
                            // Console.WriteLine($"Found frame: {desc} - {content}");
                            frames.Add(new Id3Frame(desc, content, flags));
                        }
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
