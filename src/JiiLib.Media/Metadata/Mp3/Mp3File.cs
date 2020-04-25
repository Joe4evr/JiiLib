using System.IO;

namespace JiiLib.Media.Metadata.Mp3
{
    public sealed class Mp3File : MediaFile
    {
        public Mp3File(FileInfo fileInfo) : base(fileInfo) { }
        public Mp3File(string path) : base(path) { }
    }
}
