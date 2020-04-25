using System;
using System.IO;
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

        public static MediaFile Parse(FileInfo file)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));

            return file.Extension.ToLowerInvariant() switch
            {
                ".mp3" => new Mp3File(file),
                //".ogg" => new OggFile(file),
                ".flac" => new FlacFile(file),
                _ => throw new NotSupportedException("Unsupported file given")
            };
        }

        public static MediaFile Parse(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (!System.IO.File.Exists(path)) throw new ArgumentException("File does not exist or was an invalid path.", nameof(path));

            return Parse(new FileInfo(path));
        }
    }
}
