using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using JiiLib.Media.Internal;
using JiiLib.Media.Metadata.Flac;
using JiiLib.Media.Metadata.Mp3;
//using JiiLib.Media.Metadata.Ogg;

namespace JiiLib.Media
{
    public static class MediaTag
    {
        public static bool TryParse(string path, [NotNullWhen(true)] out IMediaTag mediaTag)
            => (!MediaFile.TryParse(path, out var mediaFile))
                ? Extensions.FalseOut(out mediaTag)
                : TryReadTagFromFile(mediaFile, out mediaTag);

        public static bool TryParse(FileInfo file, [NotNullWhen(true)] out IMediaTag mediaTag)
            => (!MediaFile.TryParse(file, out var mediaFile))
                ? Extensions.FalseOut(out mediaTag)
                : TryReadTagFromFile(mediaFile, out mediaTag);

        public static bool TryReadTagFromFile(MediaFile file, [NotNullWhen(true)] out IMediaTag mediaTag)
            => (file is null)
                ? Extensions.FalseOut(out mediaTag)
                : file switch
                {
                    Mp3File mp3 => Id3Tag.Create(mp3, out mediaTag),
                    //OggFile ogg => new OggTag(ogg),
                    FlacFile flac => FlacTag.Create(flac, out mediaTag),
                    _ => Extensions.FalseOut(out mediaTag)
                };
    }
}
