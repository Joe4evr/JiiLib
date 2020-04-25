//using System;
//using System.IO;
//using JiiLib.Media.Metadata.Ogg;
//using Xunit;

//namespace JiiLib.Media.Tests.Ogg
//{
//    public sealed class OggParseTests
//    {
//        [Theory]
//        [InlineData(@"Ogg/Riders on the Storm.ogg")]
//        public void BasicReadTag(string fileName)
//        {
//            var fileInfo = new FileInfo(fileName);

//            Assert.True(fileInfo.Exists);

//            var mediaFile = MediaFile.Parse(fileInfo);

//            Assert.NotNull(mediaFile);
//            Assert.IsType<OggFile>(mediaFile);

//            var tags = MediaTag.GetTagFromFile(mediaFile);

//            Assert.NotNull(tags);
//            Assert.IsType<OggTag>(tags);

//            Assert.Equal(expected: @"Riders on the Storm", actual: tags.Title);
//            Assert.Equal(expected: @"The Doors", actual: tags.Artist);
//            Assert.Equal(expected: 1971, actual: tags.Year);
//            Assert.Equal(expected: @"Rock", actual: tags.Genre);
//            Assert.Equal(expected: @"L.A. Woman", actual: tags.Album);
//            Assert.Equal(expected: @"The Doors", actual: tags.AlbumArtist);
//            Assert.Equal(expected: 5, actual: tags.TrackNumber);
//            Assert.Equal(expected: 5, actual: tags.TotalTracks);
//            Assert.Equal(expected: 2, actual: tags.DiscNumber);
//            Assert.Equal(expected: 2, actual: tags.TotalDiscs);
//            Assert.Equal(expected: @"Space", actual: tags.Comment);
//        }
//    }
//}
