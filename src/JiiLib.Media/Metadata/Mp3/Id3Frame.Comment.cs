using System;
using System.Text;

namespace JiiLib.Media.Metadata.Mp3
{
    public partial class Id3Frame
    {
        private sealed class Id3CommentFrame : Id3Frame
        {
            internal Id3CommentFrame(Id3FrameHeader header, byte[] content)
                : base(header, content) { }

            private protected override Encoding GetEncodingAndDataRange(out Range contentRange)
            {
                switch (_content[0])
                {
                    case 0:
                        contentRange = GetZeroesIndex(_content, 1)..^1;
                        return GetLatin1();

                    case 1:
                        contentRange = (GetZeroesIndex(_content, 2) + 3)..^2;
                        return Encoding.Unicode;

                    case 2:
                        contentRange = (GetZeroesIndex(_content, 2) + 3)..^2;
                        return Encoding.BigEndianUnicode;

                    case 3:
                        contentRange = GetZeroesIndex(_content, 1)..^1;
                        return Encoding.UTF8;

                    default:
                        throw new InvalidOperationException("Could not determine frame's encoding.");
                }

                static int GetZeroesIndex(ReadOnlySpan<byte> span, int amount)
                {
                    var count = 0;
                    for (int i = 4; i < span.Length; i++)
                    {
                        if (span[i] == 0x0)
                            count++;
                        else if (count > 0)
                            count = 0;

                        if (count == amount)
                            return i;
                    }

                    throw new InvalidOperationException();
                }
            }
        }
    }
}
