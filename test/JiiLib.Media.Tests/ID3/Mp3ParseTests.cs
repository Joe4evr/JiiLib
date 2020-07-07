using System;
using System.IO;
using JiiLib.Media.Metadata.Mp3;
using Xunit;

namespace JiiLib.Media.Tests.Mp3
{
    public sealed class Mp3ParseTests
    {
        [Theory]
        [InlineData(@"ID3/ROCK OVER JAPAN.mp3")]
        public void BasicReadTag(string fileName)
        {
            var fileInfo = new FileInfo(fileName);

            Assert.True(fileInfo.Exists);

            Assert.True(MediaFile.TryParse(fileInfo, out var mediaFile));
            Assert.NotNull(mediaFile);
            Assert.IsType<Mp3File>(mediaFile);

            Assert.True(MediaTag.TryReadTagFromFile(mediaFile, out var tags));
            Assert.NotNull(tags);
            Assert.IsType<Id3Tag>(tags);

            var header = ((Id3Tag)tags).Header;

            Assert.Equal(expected: 3, actual: header.MajorVersion);
            Assert.Equal(expected: 0, actual: header.MinorVersion);
            Assert.Equal(expected: Id3TagHeaderFlags.None, actual: header.Flags);
            Assert.Equal(expected: 70646, actual: header.Size);

            Assert.Equal(expected: @"ROCK OVER JAPAN", actual: tags.Title);
            Assert.Equal(expected: @"Triple H", actual: tags.Artist);
            Assert.Equal(expected: 2011, actual: tags.Year);
            Assert.Equal(expected: @"Rock", actual: tags.Genre);
            Assert.Equal(expected: @"HHH", actual: tags.Album);
            Assert.Equal(expected: @"Triple H", actual: tags.AlbumArtist);
            Assert.Equal(expected: 1, actual: tags.TrackNumber);
            Assert.Equal(expected: 10, actual: tags.TotalTracks);
            Assert.Equal(expected: 1, actual: tags.DiscNumber);
            Assert.Equal(expected: 1, actual: tags.TotalDiscs);
            Assert.Equal(expected: @"KICA-3168", actual: tags.Comment);
        }
    }
}
