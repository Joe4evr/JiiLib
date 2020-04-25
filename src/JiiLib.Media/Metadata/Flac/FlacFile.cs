using System.IO;
using JiiLib.Media.Metadata.Vorbis;

namespace JiiLib.Media.Metadata.Flac
{
    public sealed class FlacFile : VorbisFile
    {
        public FlacFile(FileInfo fileInfo) : base(fileInfo) { }
        public FlacFile(string path) : base(path) { }
    }
}
