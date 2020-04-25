using System;
using System.Collections.Generic;
using System.Linq;
using JiiLib.Media.Internal;

namespace JiiLib.Media.Metadata.Mp3
{
    public sealed partial class Id3Tag : MediaTag<Mp3File>
    {
        public override string Title
        {
            get => GetStringFrame(HeaderIds.TIT2);
            protected set => SetStringFrame(HeaderIds.TIT2, value);
        }
        public override string Artist
        {
            get => GetStringFrame(HeaderIds.TPE1);
            protected set => SetStringFrame(HeaderIds.TPE1, value);
        }
        public override int? Year
        {
            get => GetSingleIntFrame(HeaderIds.TYER);
            protected set => SetSingleIntFrame(HeaderIds.TYER, value);
        }
        public override string Genre
        {
            get => GetStringFrame(HeaderIds.TCON);
            protected set => SetStringFrame(HeaderIds.TCON, value);
        }
        public override string Album
        {
            get => GetStringFrame(HeaderIds.TALB);
            protected set => SetStringFrame(HeaderIds.TALB, value);
        }
        public override string AlbumArtist
        {
            get => GetStringFrame(HeaderIds.TPE2);
            protected set => SetStringFrame(HeaderIds.TPE2, value);
        }
        public override int? TrackNumber
        {
            get => GetMultiIntFrame(HeaderIds.TRCK)[0];
            protected set => SetMultiIntFrame(HeaderIds.TRCK, new Optional<int?>(value), default);
        }
        public override int? TotalTracks
        {
            get => GetMultiIntFrame(HeaderIds.TRCK)[1];
            protected set => SetMultiIntFrame(HeaderIds.TRCK, default, new Optional<int?>(value));
        }
        public override int? DiscNumber
        {
            get => GetMultiIntFrame(HeaderIds.TPOS)[0];
            protected set => SetMultiIntFrame(HeaderIds.TPOS, new Optional<int?>(value), default);
        }
        public override int? TotalDiscs
        {
            get => GetMultiIntFrame(HeaderIds.TPOS)[1];
            protected set => SetMultiIntFrame(HeaderIds.TPOS, default, new Optional<int?>(value));
        }
        public override string Comment
        {
            get => GetStringFrame(HeaderIds.COMM);
            protected set => SetStringFrame(HeaderIds.COMM, value);
        }

        private string GetStringFrame(string frameId)
            => Frames.GetValueOrDefault(frameId)?.AsString();
        private int? GetSingleIntFrame(string frameId)
            => Frames.GetValueOrDefault(frameId)?.AsSingleInt();
        private int?[] GetMultiIntFrame(string frameId)
            => Frames.GetValueOrDefault(frameId)?.AsMultiInt();

        private void SetStringFrame(string frameId, string value)
        {
            if (value is null)
            {
                Frames.Remove(frameId);
                return;
            }

            Frames[frameId] = Id3Frame.FromString(Id3FrameId.Create(frameId), value);
        }
        private void SetSingleIntFrame(string frameId, int? value)
        {
            if (value is null)
            {
                Frames.Remove(frameId);
                return;
            }

            SetStringFrame(frameId, value.Value.ToString());
        }
        private void SetMultiIntFrame(string frameId, params Optional<int?>[] values)
        {
            if (values.All(v => !v.IsSpecified(out _)))
            {
                Frames.Remove(frameId);
                return;
            }

            string result;
            if (Frames.TryGetValue(frameId, out var frame))
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

            Frames[frameId] = Id3Frame.FromString(Id3FrameId.Create(frameId), result);

            static string ToString(int? i)
                => i?.ToString() ?? String.Empty;
        }
    }
}
