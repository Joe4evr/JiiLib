using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace JiiLib.Media.Metadata.Mp3
{
    public sealed class Mp3File : MediaFile
    {
        public Mp3File(FileInfo fileInfo) : base(fileInfo) { }
        public Mp3File(string path) : base(path) { }

        internal static bool Create(FileInfo file, [NotNullWhen(true)] out MediaFile mediaFile)
        {
            mediaFile = new Mp3File(file);
            return true;
        }
    }
}
