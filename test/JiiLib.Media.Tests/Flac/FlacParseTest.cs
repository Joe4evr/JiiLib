using System;
using System.IO;
using JiiLib.Media.Metadata.Flac;
using Xunit;

namespace JiiLib.Media.Tests.Flac
{
    public sealed class FlacParseTest
    {
        [Theory]
        [InlineData(@"Flac/Kibou ni Tsuite.flac")]
        public void BasicReadTag(string fileName)
        {
            var fileInfo = new FileInfo(fileName);

            Assert.True(fileInfo.Exists);

            var mediaFile = MediaFile.Parse(fileInfo);

            Assert.NotNull(mediaFile);
            Assert.IsType<FlacFile>(mediaFile);

            var tags = MediaTag.GetTagFromFile(mediaFile);

            Assert.NotNull(tags);
            Assert.IsType<FlacTag>(tags);

            Assert.Equal(expected: @"Kibou ni Tsuite", actual: tags.Title);
            Assert.Equal(expected: @"NO NAME", actual: tags.Artist);
            Assert.Equal(expected: 2012, actual: tags.Year);
            Assert.Equal(expected: @"J-POP", actual: tags.Genre);
            Assert.Equal(expected: @"Kibou ni Tsuite", actual: tags.Album);
            Assert.Equal(expected: @"NO NAME", actual: tags.AlbumArtist);
            Assert.Equal(expected: 1, actual: tags.TrackNumber);
            Assert.Equal(expected: 6, actual: tags.TotalTracks);
            Assert.Equal(expected: 1, actual: tags.DiscNumber);
            Assert.Equal(expected: 1, actual: tags.TotalDiscs);
            Assert.Equal(expected: @"Space", actual: tags.Comment);
        }
    }
}
