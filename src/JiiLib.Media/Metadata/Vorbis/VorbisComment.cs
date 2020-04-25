using System;
using System.Collections.Generic;
using System.Text;

namespace JiiLib.Media.Metadata.Vorbis
{
    public abstract class VorbisComment<TFile> : IMediaTag<TFile>
        where TFile : VorbisFile
    {
        protected static readonly Encoding Enc = Encoding.UTF8;

        public TFile? File { get; }

        protected VorbisComment(TFile? file)
        {
            File = file;
        }

        protected IList<string> Artists { get; private set; } = new List<string>();

        public string? Title { get; protected set; }

        public string? Artist
        {
            get => String.Join(", ", Artists);
            protected set
            {
                if (value is null)
                {
                    Artists = new List<string>();
                    return;
                }

                Artists = new List<string> { value };
            }
        }

        public int? Year { get; protected set; }

        public string? Genre { get; protected set; }

        public string? Album { get; protected set; }

        public string? AlbumArtist { get; protected set; }

        public int? TrackNumber { get; protected set; }

        public int? TotalTracks { get; protected set; }

        public int? DiscNumber { get; protected set; }

        public int? TotalDiscs { get; protected set; }

        public string? Comment { get; protected set; }

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
            switch (field.ToUpperInvariant())
            {
                case "TITLE":
                    Title = value;
                    break;
                case "ARTIST":
                    Artists.Add(value);
                    break;
                case "DATE":
                    Year = TryParseInt(value);
                    break;
                case "GENRE":
                    Genre = value;
                    break;
                case "ALBUM":
                    Album = value;
                    break;
                case "ALBUMARTIST":
                    AlbumArtist = value;
                    break;
                case "TRACKNUMBER":
                    var tracks = TryParseMultiInt(value);
                    TrackNumber = tracks[0];
                    if (tracks.Length >= 2)
                        TotalTracks = tracks[1];
                    break;
                case "TRACKTOTAL":
                case "TOTALTRACKS":
                    TotalTracks = TryParseInt(value);
                    break;
                case "DISCNUMBER":
                    var discs = TryParseMultiInt(value);
                    DiscNumber = discs[0];
                    if (discs.Length >= 2)
                        TotalDiscs = discs[1];
                    break;
                case "TOTALDISCS":
                case "DISCTOTAL":
                    TotalDiscs = TryParseInt(value);
                    break;
                case "COMMENT":
                    Comment = value;
                    break;
                default:
                    break;
            }

            static int? TryParseInt(string s)
                => Int32.TryParse(s, out var i) ? i : (int?)null;

            static int?[] TryParseMultiInt(string s)
            {
                var parts = s.Split('/');
                var vals = new int?[parts.Length];
                for (int i = 0; i < parts.Length; i++)
                    vals[i] = TryParseInt(parts[i]);

                return vals;
            }
        }
    }
}
