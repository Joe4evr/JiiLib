using System;
using JiiLib.Media.Metadata.Flac;
using JiiLib.Media.Metadata.Mp3;
//using JiiLib.Media.Metadata.Ogg;

namespace JiiLib.Media
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class MediaTag
    {
        /// <summary>
        ///     Value of the Title tag
        /// </summary>
        public abstract string Title { get; protected set; }

        /// <summary>
        ///     Value of the Artist tag
        /// </summary>
        public abstract string Artist { get; protected set; }

        /// <summary>
        ///     Value of the Year tag
        /// </summary>
        public abstract int? Year { get; protected set; }

        /// <summary>
        ///     Value of the Genre tag
        /// </summary>
        public abstract string Genre { get; protected set; }

        /// <summary>
        ///     Value of the Album tag
        /// </summary>
        public abstract string Album { get; protected set; }

        /// <summary>
        ///     Value of the AlbumArtist tag
        /// </summary>
        public abstract string AlbumArtist { get; protected set; }

        /// <summary>
        ///     Value of the TrackNumber tag
        /// </summary>
        public abstract int? TrackNumber { get; protected set; }

        /// <summary>
        ///     Value of the TotalTracks tag
        /// </summary>
        public abstract int? TotalTracks { get; protected set; }

        /// <summary>
        ///     Value of the DiscNumber tag
        /// </summary>
        public abstract int? DiscNumber { get; protected set; }

        /// <summary>
        ///     Value of the TotalDiscs tag
        /// </summary>
        public abstract int? TotalDiscs { get; protected set; }

        /// <summary>
        ///     Value of the Comment tag
        /// </summary>
        public abstract string Comment { get; protected set; }

        internal MediaTag() { }

        public static MediaTag Parse(string path)
        {
            return GetTagFromFile(MediaFile.Parse(path));
        }

        public static MediaTag GetTagFromFile(MediaFile file)
        {
            if (file is null) throw new ArgumentNullException(nameof(file));

            return file switch
            {
                Mp3File mp3 => new Id3Tag(mp3),
                //OggFile ogg => new OggTag(ogg),
                FlacFile flac => new FlacTag(flac),
                _ => throw new NotSupportedException("Unsupported file given")
            };
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TFile">
    /// </typeparam>
    public abstract class MediaTag<TFile> : MediaTag
        where TFile : MediaFile
    {
        internal MediaTag() { }

        //public static implicit operator Wrapper<MediaTag<TFile>>()
        //public abstract void WriteTo(TFile file);
    }
}
