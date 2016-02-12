using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace JiiLib.Media.Mp3
{
    /// <summary>
    /// 
    /// </summary>
    public class Id3v2 : Tag<Mp3>
    {
        public IList<Id3Frame> Frames { get; }

        #region Ctors
        public Id3v2()
        {
            Frames = ReadFrames();
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
                Frames.Add(new Id3Frame(Constants.TIT2, value));
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
                Frames.Add(new Id3Frame(Constants.TPE1, value));
            }
        }

        /// <summary>
        /// Value of the Year field
        /// </summary>
        public override int Year
        {
            get
            {
                return Int32.Parse(Frames.FirstOrDefault(f => f.FrameHeader == Constants.TDRC)?.FrameContent
                    ?? Frames.FirstOrDefault(f => f.FrameHeader == "TDAT")?.FrameContent
                    ?? Frames.FirstOrDefault(f => f.FrameHeader == "TIME")?.FrameContent //check for obsolete fields
                    ?? Frames.FirstOrDefault(f => f.FrameHeader == "TRDA")?.FrameContent
                    ?? Frames.FirstOrDefault(f => f.FrameHeader == "TYER")?.FrameContent ?? "0");
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
                Frames.Add(new Id3Frame(Constants.TDRC, value.ToString()));
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
                Frames.Add(new Id3Frame(Constants.TALB, value));
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
                Frames.Add(new Id3Frame(Constants.TPE2, value));
            }
        }

        /// <summary>
        /// Value of the TrackNumber field
        /// </summary>
        public override int TrackNumber
        {
            get
            {
                return Int32.Parse(Frames.FirstOrDefault(f => f.FrameHeader == Constants.TRCK)?
                    .FrameContent.Split('/')[0] ?? "0");
            }
            protected set
            {
                var tmp = Frames.FirstOrDefault(f => f.FrameHeader == Constants.TRCK);
                var res = "0";
                if (tmp != null)
                {
                    res = tmp.FrameContent.Split('/')[1];
                    Frames.Remove(tmp);
                }
                Frames.Add(new Id3Frame(Constants.TRCK, $"{value}/{res}"));
            }
        }

        /// <summary>
        /// Value of the TotalTracks field
        /// </summary>
        public override int TotalTracks
        {
            get
            {
                return Int32.Parse(Frames.FirstOrDefault(f => f.FrameHeader == Constants.TRCK)?
                    .FrameContent.Split('/')[1] ?? "0");
            }
            protected set
            {
                var tmp = Frames.FirstOrDefault(f => f.FrameHeader == Constants.TRCK);
                var res = "0";
                if (tmp != null)
                {
                    res = tmp.FrameContent.Split('/')[0];
                    Frames.Remove(tmp);
                }
                Frames.Add(new Id3Frame(Constants.TRCK, $"{res}/{value}"));
            }
        }

        /// <summary>
        /// Value of the DiscNumber field
        /// </summary>
        public override int DiscNumber
        {
            get
            {
                return Int32.Parse(Frames.FirstOrDefault(f => f.FrameHeader == Constants.TPOS)?
                    .FrameContent.Split('/')[0] ?? "0");
            }
            protected set
            {
                var tmp = Frames.FirstOrDefault(f => f.FrameHeader == Constants.TPOS);
                var res = "0";
                if (tmp != null)
                {
                    res = tmp.FrameContent.Split('/')[1];
                    Frames.Remove(tmp);
                }
                Frames.Add(new Id3Frame(Constants.TPOS, $"{value}/{res}"));
            }
        }

        /// <summary>
        /// Value of the TotalDiscs field
        /// </summary>
        public override int TotalDiscs
        {
            get
            {
                return Int32.Parse(Frames.FirstOrDefault(f => f.FrameHeader == Constants.TPOS)?
                    .FrameContent.Split('/')[1] ?? "0");
            }
            protected set
            {
                var tmp = Frames.FirstOrDefault(f => f.FrameHeader == Constants.TPOS);
                var res = "0";
                if (tmp != null)
                {
                    res = tmp.FrameContent.Split('/')[0];
                    Frames.Remove(tmp);
                }
                Frames.Add(new Id3Frame(Constants.TPOS, $"{res}/{value}"));
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
                Frames.Add(new Id3Frame(Constants.COMM, value));
            }
        }
        #endregion

        #region Private Methods
        private static IList<Id3Frame> ReadFrames()
        {
            return new List<Id3Frame>();
        }
        #endregion
    }

    public class Mp3 : MediaFile { }
}
