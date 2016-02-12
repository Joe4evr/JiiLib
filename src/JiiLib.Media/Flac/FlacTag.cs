using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JiiLib.Media.Vorbis;

namespace JiiLib.Media.Flac
{
    public class FlacTag : VorbisComment<Flac>
    {


        /// <summary>
        /// Create an <see cref="FlacTag"/> tag from an existing tag.
        /// </summary>
        /// <typeparam name="TFile"></typeparam>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static FlacTag FromTag<TFile>(Tag<TFile> tag) where TFile : MediaFile
        {
            return new FlacTag
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
    }

    public class Flac : VorbisFile { }
}
