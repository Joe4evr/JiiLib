using System;

namespace JiiLib.Media
{
    /// <summary>
    /// 
    /// </summary>
    public interface IMediaTag
    {
        /// <summary>
        ///     Info about the file associated with this tag.
        /// </summary>
        MediaFile? File { get; }

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
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TFile">
    /// </typeparam>
    public interface IMediaTag<TFile> : IMediaTag
        where TFile : MediaFile
    {
        /// <summary>
        ///     Info about the file associated with this tag.
        /// </summary>
        new TFile? File { get; }

        MediaFile? IMediaTag.File => File;
    }
}
