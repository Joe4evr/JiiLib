using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JiiLib.Media.Metadata
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TFile"></typeparam>
    public abstract class Tag<TFile> where TFile : MediaFile
    {
        public abstract string Title { get; protected set; }
        public abstract string Artist { get; protected set; }
        public abstract int? Year { get; protected set; }
        public abstract string Genre { get; protected set; }
        public abstract string Album { get; protected set; }
        public abstract string AlbumArtist { get; protected set; }
        public abstract int? TrackNumber { get; protected set; }
        public abstract int? TotalTracks { get; protected set; }
        public abstract int? DiscNumber { get; protected set; }
        public abstract int? TotalDiscs { get; protected set; }
        public abstract string Comment { get; protected set; }

        //public static implicit operator Wrapper<Tag<TFile>>()
        public abstract void WriteTo(TFile file);
    }

    public abstract class MediaFile
    {
        public string Path { get; }
        public MediaFile(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            Path = path;
        }
    }
}
