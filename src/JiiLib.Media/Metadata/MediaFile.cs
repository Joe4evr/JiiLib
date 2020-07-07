using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using JiiLib.Media.Internal;
using JiiLib.Media.Metadata.Flac;
using JiiLib.Media.Metadata.Mp3;
//using JiiLib.Media.Metadata.Ogg;

namespace JiiLib.Media
{
    public abstract class MediaFile
    {
        public FileInfo FileInfo { get; }

        protected MediaFile(FileInfo file)
        {
            FileInfo = file ?? throw new ArgumentNullException(nameof(file));
        }

        protected MediaFile(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (!System.IO.File.Exists(path)) throw new ArgumentException("File does not exist or was an invalid path.", nameof(path));

            FileInfo = new FileInfo(path);
        }

        public static bool TryParse(FileInfo file, [NotNullWhen(true)] out MediaFile mediaFile)
            => (file is null || !file.Exists)
                ? Extensions.FalseOut(out mediaFile)
                : file.Extension.ToLowerInvariant() switch
                {
                    ".mp3" => Mp3File.Create(file, out mediaFile),
                    //".ogg" => new OggFile(file),
                    ".flac" => FlacFile.Create(file, out mediaFile),
                    _ => Extensions.FalseOut(out mediaFile)
                };

        public static bool TryParse(string path, [NotNullWhen(true)] out MediaFile mediaFile)
            => (path is null || !File.Exists(path))
                ? Extensions.FalseOut(out mediaFile)
                : TryParse(new FileInfo(path), out mediaFile);
    }
}
