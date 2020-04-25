using System;
using JiiLib.Media.Metadata.Flac;
using JiiLib.Media.Metadata.Mp3;
//using JiiLib.Media.Metadata.Ogg;

namespace JiiLib.Media
{
    /// <summary>
    /// 
    /// </summary>
    public interface IMediaTag
    {
        /// <summary>
        ///     Value of the Title tag
        /// </summary>
        string? Title { get; }

        /// <summary>
        ///     Value of the Artist tag
        /// </summary>
        string? Artist { get; }

        /// <summary>
        ///     Value of the Year tag
        /// </summary>
        int? Year { get; }

        /// <summary>
        ///     Value of the Genre tag
        /// </summary>
        string? Genre { get; }

        /// <summary>
        ///     Value of the Album tag
        /// </summary>
        string? Album { get; }

        /// <summary>
        ///     Value of the AlbumArtist tag
        /// </summary>
        string? AlbumArtist { get; }

        /// <summary>
        ///     Value of the TrackNumber tag
        /// </summary>
        int? TrackNumber { get; }

        /// <summary>
        ///     Value of the TotalTracks tag
        /// </summary>
        int? TotalTracks { get; }

        /// <summary>
        ///     Value of the DiscNumber tag
        /// </summary>
        int? DiscNumber { get; }

        /// <summary>
        ///     Value of the TotalDiscs tag
        /// </summary>
        int? TotalDiscs { get; }

        /// <summary>
        ///     Value of the Comment tag
        /// </summary>
        string? Comment { get; }

        //internal IMediaTag() { }

        public static IMediaTag Parse(string path)
        {
            return GetTagFromFile(MediaFile.Parse(path));
        }

        public static IMediaTag GetTagFromFile(MediaFile file)
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
    public interface IMediaTag<TFile> : IMediaTag
        where TFile : MediaFile
    {
        TFile? File { get; }

        //public static implicit operator Wrapper<MediaTag<TFile>>()
        //public abstract void WriteTo(TFile file);
    }
}
