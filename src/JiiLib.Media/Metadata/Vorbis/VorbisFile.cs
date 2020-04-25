using System.IO;

namespace JiiLib.Media.Metadata.Vorbis
{
    public abstract class VorbisFile : MediaFile
    {
        protected VorbisFile(FileInfo fileInfo) : base(fileInfo) { }
        protected VorbisFile(string path) : base(path) { }
    }
}
