using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JiiLib.Media;

namespace JiiLib.Media.Metadata.Vorbis
{
    public abstract class VorbisComment<TFile> : Tag<TFile> where TFile : VorbisFile
    {
        #region protected/private fields
        protected static readonly Encoding enc = Encoding.UTF8;
        protected IList<string> _artists;
        #endregion

        public VorbisComment()
        {
            _artists = new List<string>();
        }

        public override string Title { get; protected set; }
        
        public override string Artist
        {
            get { return String.Join(", ", _artists); }
            protected set { _artists = new List<string> { value }; }
        }

        public override int? Year { get; protected set; }

        public override string Genre { get; protected set; }

        public override string Album { get; protected set; }

        public override string AlbumArtist { get; protected set; }

        public override int? TrackNumber { get; protected set; }

        public override int? TotalTracks { get; protected set; }

        public override int? DiscNumber { get; protected set; }

        public override int? TotalDiscs { get; protected set; }

        public override string Comment { get; protected set; }

        //protected void ReadComments(VorbisFile file, byte[] targetHeader)
        //{
        //    var headBuffer = new byte[targetHeader.Length];

        //    using (FileStream fs = new FileStream(file.Path, FileMode.Open, FileAccess.Read))
        //    {
        //        fs.Read(headBuffer, 0, targetHeader.Length);
        //        if (!headBuffer.StartsWith(targetHeader)) //"fLaC"
        //        {
        //            throw new InvalidDataException("File was not in a correct format.");
        //        }

        //        var first = true;
        //        var artists = new List<string>();
        //        bool lastBlock = false;
        //        while (!lastBlock)
        //        {
        //            var blh = (byte)fs.ReadByte();
        //            var blockType = blh % 128;
        //            var len = new byte[3];
        //            lastBlock = blh >= 128;
        //            fs.Read(len, 0, 3);
        //            var length = (len[0] << 16) + (len[1] << 8) + len[2];
        //            var metadataBlock = new byte[length];
        //            fs.Read(metadataBlock, 0, length);

        //            if (blockType == 4)
        //            {
        //                using (MemoryStream ms = new MemoryStream(metadataBlock, writable: false))
        //                {
        //                    while (ms.Position < ms.Length)
        //                    {
        //                        var h = new byte[4];
        //                        ms.Read(h, 0, 4);
        //                        int l = h[0];

        //                        var tag = new byte[l];
        //                        ms.Read(tag, 0, l);
        //                        var s = new String(enc.GetChars(tag));

        //                        var t = s.Split('=');
        //                        switch (t[0])
        //                        {
        //                            case "TITLE":
        //                                this.Title = t[1];
        //                                break;
        //                            case "ARTIST":
        //                                artists.Add(t[1]);
        //                                break;
        //                            case "DATE":
        //                                this.Year = Int32.Parse(t[1]);
        //                                break;
        //                            case "GENRE":
        //                                this.Genre = t[1];
        //                                break;
        //                            case "ALBUM":
        //                                this.Album = t[1];
        //                                break;
        //                            case "ALBUMARTIST":
        //                                this.AlbumArtist = t[1];
        //                                break;
        //                            case "TRACKNUMBER":
        //                                this.TrackNumber = Int32.Parse(t[1]);
        //                                break;
        //                            case "TOTALTRACKS":
        //                                this.TotalTracks = Int32.Parse(t[1]);
        //                                break;
        //                            case "DISCNUMBER":
        //                                this.DiscNumber = Int32.Parse(t[1]);
        //                                break;
        //                            case "TOTALDISCS":
        //                                this.TotalDiscs = Int32.Parse(t[1]);
        //                                break;
        //                            case "COMMENT":
        //                                this.Comment = t[1];
        //                                break;
        //                            default:
        //                                break;
        //                        }
        //                        if (first)
        //                        {
        //                            ms.Read(new byte[4], 0, 4);
        //                            first = false;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        _artists = artists;
        //    }
        //}
        protected void AssignFields(string field, string value)
        {
            switch (field)
            {
                case "TITLE":
                case "title":
                    this.Title = value;
                    break;
                case "ARTIST":
                case "artist":
                    this.Artist = value;
                    break;
                case "DATE":
                case "date":
                    this.Year = Int32.Parse(value);
                    break;
                case "GENRE":
                case "genre":
                    this.Genre = value;
                    break;
                case "ALBUM":
                case "album":
                    this.Album = value;
                    break;
                case "ALBUMARTIST":
                case "albumartist":
                    this.AlbumArtist = value;
                    break;
                case "TRACKNUMBER":
                case "tracknumber":
                    this.TrackNumber = Int32.Parse(value);
                    break;
                case "TRACKTOTAL":
                case "tracktotal":
                case "TOTALTRACKS":
                case "totaltracks":
                    this.TotalTracks = Int32.Parse(value);
                    break;
                case "DISCNUMBER":
                case "discnumber":
                    this.DiscNumber = Int32.Parse(value);
                    break;
                case "TOTALDISCS":
                case "totaldiscs":
                    this.TotalDiscs = Int32.Parse(value);
                    break;
                case "COMMENT":
                case "comment":
                    this.Comment = value;
                    break;
                default:
                    break;
            }
        }
    }

    public abstract class VorbisFile : MediaFile
    {
        public VorbisFile(string path) : base(path) { }
    }
}
