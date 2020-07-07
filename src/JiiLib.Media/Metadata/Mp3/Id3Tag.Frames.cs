using System;
using System.Collections.Generic;
using System.Linq;
using JiiLib.Media.Internal;

namespace JiiLib.Media.Metadata.Mp3
{
    public sealed partial class Id3Tag : IMediaTag<Mp3File>
    {
        public string? Title
        {
            get => GetStringFrame(HeaderIds.TIT2);
            private set => SetStringFrame(HeaderIds.TIT2, value);
        }
        public string? Artist
        {
            get => GetStringFrame(HeaderIds.TPE1);
            private set => SetStringFrame(HeaderIds.TPE1, value);
        }
        public int? Year
        {
            get => GetSingleIntFrame(HeaderIds.TYER);
            private set => SetSingleIntFrame(HeaderIds.TYER, value);
        }
        public string? Genre
        {
            get => GetStringFrame(HeaderIds.TCON);
            private set => SetStringFrame(HeaderIds.TCON, value);
        }
        public string? Album
        {
            get => GetStringFrame(HeaderIds.TALB);
            private set => SetStringFrame(HeaderIds.TALB, value);
        }
        public string? AlbumArtist
        {
            get => GetStringFrame(HeaderIds.TPE2);
            private set => SetStringFrame(HeaderIds.TPE2, value);
        }
        public int? TrackNumber
        {
            get => GetMultiIntFrame(HeaderIds.TRCK)?[0];
            private set => SetMultiIntFrame(HeaderIds.TRCK, new Optional<int?>(value), default);
        }
        public int? TotalTracks
        {
            get => GetMultiIntFrame(HeaderIds.TRCK)?[1];
            private set => SetMultiIntFrame(HeaderIds.TRCK, default, new Optional<int?>(value));
        }
        public int? DiscNumber
        {
            get => GetMultiIntFrame(HeaderIds.TPOS)?[0];
            private set => SetMultiIntFrame(HeaderIds.TPOS, new Optional<int?>(value), default);
        }
        public int? TotalDiscs
        {
            get => GetMultiIntFrame(HeaderIds.TPOS)?[1];
            private set => SetMultiIntFrame(HeaderIds.TPOS, default, new Optional<int?>(value));
        }
        public string? Comment
        {
            get => GetStringFrame(HeaderIds.COMM);
            private set => SetStringFrame(HeaderIds.COMM, value);
        }

        private string? GetStringFrame(string frameId)
            => _frames.GetValueOrDefault(frameId)?.AsString();
        private int? GetSingleIntFrame(string frameId)
            => _frames.GetValueOrDefault(frameId)?.AsSingleInt();
        private int?[]? GetMultiIntFrame(string frameId)
            => _frames.GetValueOrDefault(frameId)?.AsMultiInt();

        private void SetStringFrame(string frameId, string? value)
        {
            if (value is null)
            {
                _frames.Remove(frameId);
                return;
            }

            _frames[frameId] = Id3Frame.FromString(Id3FrameId.Create(frameId), value);
        }
        private void SetSingleIntFrame(string frameId, int? value)
        {
            if (value is null)
            {
                _frames.Remove(frameId);
                return;
            }

            SetStringFrame(frameId, value.Value.ToString());
        }
        private void SetMultiIntFrame(string frameId, params Optional<int?>[] values)
        {
            if (values.All(v => !v.IsSpecified(out _)))
            {
                _frames.Remove(frameId);
                return;
            }

            string result;
            if (_frames.TryGetValue(frameId, out var frame))
            {
                var parts = frame.AsString().Split('/');
                if (parts.Length == values.Length)
                {
                    // direct replace elements of 'parts'
                    for (int i = 0; i < values.Length; i++)
                    {
                        if (values[i].IsSpecified(out var val))
                            parts[i] = ToString(val) ?? String.Empty;
                    }

                    result = String.Join('/', parts);
                }
                else
                {
                    var copy = new string[Math.Max(parts.Length, values.Length)];
                    for (int i = 0; i < copy.Length; i++)
                    {
                        if (i < parts.Length)
                        {
                            copy[i] = parts[i];
                        }
                        if (i < values.Length)
                        {
                            if (values[i].IsSpecified(out var val))
                                copy[i] = ToString(val);
                        }
                    }

                    result = String.Join('/', copy);
                }
            }
            else
            {
                result = String.Join('/', values.Select(opt => opt.IsSpecified(out var v) ? ToString(v) : String.Empty));
            }

            _frames[frameId] = Id3Frame.FromString(Id3FrameId.Create(frameId), result);

            static string ToString(int? i)
                => i?.ToString() ?? String.Empty;
        }
    }
}
