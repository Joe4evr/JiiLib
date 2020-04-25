using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using JiiLib.Media.Internal;

namespace JiiLib.Media.Metadata.Mp3
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public partial class Id3Frame
    {
        public Id3FrameHeader Header { get; }

        private readonly byte[] _content;
        public string HeaderDefinition { get; }

        private Id3Frame(Id3FrameHeader header, byte[] content)
        {
            var idStr = header.FrameId.IdString ?? throw new ArgumentException(message: "Invalid Frame ID");
            _content = content ?? throw new ArgumentNullException(nameof(content));

            Header = header;
            HeaderDefinition = HeaderIds.FrameDict.GetValueOrDefault(idStr, String.Empty);
        }

        /// <summary>
        ///     If the content of this tag is a Text field,
        ///     returns the content as a <see cref="String"/>.
        /// </summary>
        /// <returns>
        ///     This tag's field if it's text, otherwise throws.
        /// </returns>
        internal string AsString()
        {
            //if (!Header.IsTextHeader) throw new InvalidOperationException();

            var enc = GetEncodingAndDataRange(out var range);
            return enc.GetString(_content.AsSpan()[range]);
        }

        internal int? AsSingleInt()
        {
            //if (!Header.IsNumericHeader) throw new InvalidOperationException();

            var enc = GetEncodingAndDataRange(out var range);
            var span = _content.AsSpan()[range];

            Span<char> charspan = stackalloc char[enc.GetCharCount(span)];
            enc.GetChars(span, charspan);

            return Int32.TryParse(charspan, out var i) ? i : (int?)null;
        }
        internal int?[] AsMultiInt()
        {
            var enc = GetEncodingAndDataRange(out var range);
            var span = _content.AsSpan()[range];

            Span<char> charspan = new char[enc.GetCharCount(span)];
            enc.GetChars(span, charspan);

            var parts = new int?[charspan.GetCount('/') + 1];
            var splitter = new SpanCharSplitter(charspan, '/');

            var counter = 0;
            while (splitter.TryMoveNext(out var piece))
            {
                parts[counter] = Int32.TryParse(piece, out var i) ? i : (int?)null;
                counter++;
            }

            return parts;
        }

        private protected virtual Encoding GetEncodingAndDataRange(out Range contentRange)
        {
            switch (_content[0])
            {
                case 0:
                    contentRange = 1..^1;
                    return GetLatin1();

                case 1 when (_content[1] == 0xFF && _content[2] == 0xFE):
                    contentRange = 3..^2;
                    return Encoding.Unicode;

                case 2 when (_content[1] == 0xFE && _content[2] == 0xFF):
                    contentRange = 3..^2;
                    return Encoding.BigEndianUnicode;

                case 3:
                    contentRange = 1..^1;
                    return Encoding.UTF8;

                default:
                    throw new InvalidOperationException("Could not determine frame's encoding.");
            }
        }

        internal static Id3Frame FromContent(Id3FrameId frameId, byte[] content, Id3FrameFlags flags = Id3FrameFlags.None)
        {
            var header = new Id3FrameHeader(frameId, content.Length, flags);
            return (frameId.IdString == HeaderIds.COMM)
                ? new Id3CommentFrame(header, content)
                : new Id3Frame(header, content);
        }

        internal static Id3Frame FromString(Id3FrameId frameId, string value, Id3FrameFlags flags = Id3FrameFlags.None)
        {
            var enc = GetLatin1();
            var byteCount = enc.GetByteCount(value);
            var encoded = new byte[byteCount + 2];
            enc.GetBytes(value, encoded.AsSpan()[1..^1]);

            return FromContent(frameId, encoded, flags);
        }

        private static Encoding GetLatin1()
        {
            try
            {
                return Encoding.GetEncoding("iso-8859-1");
            }
            catch (Exception)
            {
                return Latin1EncodingFallback.Instance; // fallback in case Latin-1 isn't available
            }
        }

        private string DebuggerDisplay => Header.FrameId.IdString;
    }
}
