using System.IO;
using JiiLib.Media.Metadata.Vorbis;

namespace JiiLib.Media.Metadata.Ogg
{
    public sealed class OggFile : VorbisFile
    {
        public OggFile(FileInfo fileInfo) : base(fileInfo) { }
        public OggFile(string path) : base(path) { }
    }
}
