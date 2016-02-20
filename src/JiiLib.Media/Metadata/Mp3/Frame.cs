﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JiiLib.Media.Metadata.Mp3
{
    public class Id3Frame
    {
        public string FrameHeader { get; }
        internal byte[] RawContent { get; }
        public byte[] Flags { get; }
        public string HeaderDefinition { get; }

        internal Id3Frame(string header, byte[] content, byte[] flags)
        {
            if (header == null) throw new ArgumentNullException(nameof(header));
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (flags == null) throw new ArgumentNullException(nameof(flags));
            if (flags.Length != 2) throw new ArgumentOutOfRangeException(nameof(flags), "Argument has to be of length 2.");
            if (!frameDict.Keys.Contains(header)) throw new ArgumentException("Not a valid frame header.", nameof(header));

            FrameHeader = header;
            RawContent = content;
            Flags = flags;
            HeaderDefinition = frameDict.Single(kv => kv.Key == FrameHeader).Value;
        }
        
        /// <summary>
        /// If the content of this tag is a Text field, returns the content as a <see cref="string"/>, otherwise returns null.
        /// </summary>
        /// <returns>This tag's field if it's text, otherwise null.</returns>
        internal string AsString()
        {
            if (!FrameHeader.StartsWith("T")) return null;

            string content;
            Encoding enc;
            switch (RawContent[0])
            {
                case 0:
                    try
                    {
                        enc = Encoding.GetEncoding("iso-8859-1");
                    }
                    catch (Exception)
                    {
                        enc = new Latin1Encoding(); //fallback in case Latin-1 isn't available
                    }
                    content = enc.GetString(RawContent, 1, RawContent.Length - 2);
                    break;
                case 1:
                    enc = Encoding.Unicode;
                    if (RawContent[1] != 0xFF && RawContent[2] != 0xFE) return null;
                    content = enc.GetString(RawContent, 3, RawContent.Length - 5);
                    break;
                case 2:
                    enc = new UnicodeEncoding(bigEndian: true, byteOrderMark: false);
                    //if (FrameContent[1] != 0xFE && FrameContent[2] != 0xFF) return null;
                    content = enc.GetString(RawContent, 1, RawContent.Length - 3);
                    break;
                case 3:
                    enc = Encoding.UTF8;
                    content = enc.GetString(RawContent, 1, RawContent.Length - 2);
                    break;
                default:
                    throw new InvalidOperationException();
            }
            return content;
        }

        internal static Dictionary<string, string> frameDict = new Dictionary<string, string>
        {
            { Constants.AENC, "Audio encryption" },
            { Constants.APIC, "Attached picture" },
            { Constants.ASPI, "Audio seek point index" },
            { Constants.COMM, "Comments" },
            { Constants.COMR, "Commercial frame" },
            { Constants.ENCR, "Encryption method registration" },
            { Constants.EQU2, "Equalisation (2)" },
            { Constants.ETCO, "Event timing codes" },
            { Constants.GEOB, "General encapsulated object" },
            { Constants.GRID, "Group identification registration" },
            { Constants.LINK, "Linked information" },
            { Constants.MCDI, "Music CD identifier" },
            { Constants.MLLT, "MPEG location lookup table" },
            { Constants.OWNE, "Ownership frame" },
            { Constants.PRIV, "Private frame" },
            { Constants.PCNT, "Play counter" },
            { Constants.POPM, "Popularimeter" },
            { Constants.POSS, "Position synchronisation frame" },
            { Constants.RBUF, "Recommended buffer size" },
            { Constants.RVA2, "Relative volume adjustment (2)" },
            { Constants.RVRB, "Reverb" },
            { Constants.SEEK, "Seek frame" },
            { Constants.SIGN, "Signature frame" },
            { Constants.SYLT, "Synchronised lyric/text" },
            { Constants.SYTC, "Synchronised tempo codes" },
            { Constants.TALB, "Album/Movie/Show title" },
            { Constants.TBPM, "BPM (beats per minute)" },
            { Constants.TCOM, "Composer" },
            { Constants.TCON, "Content type" },
            { Constants.TCOP, "Copyright message" },
            { Constants.TDEN, "Encoding time" },
            { Constants.TDLY, "Playlist delay" },
            { Constants.TDOR, "Original release time" },
            { Constants.TDRC, "Recording time" },
            { Constants.TDRL, "Release time" },
            { Constants.TDTG, "Tagging time" },
            { Constants.TENC, "Encoded by" },
            { Constants.TEXT, "Lyricist/Text writer" },
            { Constants.TFLT, "File type" },
            { Constants.TIPL, "Involved people list" },
            { Constants.TIT1, "Content group description" },
            { Constants.TIT2, "Title/songname/content description" },
            { Constants.TIT3, "Subtitle/Description refinement" },
            { Constants.TKEY, "Initial key" },
            { Constants.TLAN, "Language(s)" },
            { Constants.TLEN, "Length" },
            { Constants.TMCL, "Musician credits list" },
            { Constants.TMED, "Media type" },
            { Constants.TMOO, "Mood" },
            { Constants.TOAL, "Original album/movie/show title" },
            { Constants.TOFN, "Original filename" },
            { Constants.TOLY, "Original lyricist(s)/text writer(s)" },
            { Constants.TOPE, "Original artist(s)/performer(s)" },
            { Constants.TOWN, "File owner/licensee" },
            { Constants.TPE1, "Lead performer(s)/Soloist(s)" },
            { Constants.TPE2, "Band/orchestra/accompaniment" },
            { Constants.TPE3, "Conductor/performer refinement" },
            { Constants.TPE4, "Interpreted, remixed, or otherwise modified by" },
            { Constants.TPOS, "Part of a set" },
            { Constants.TPRO, "Produced notice" },
            { Constants.TPUB, "Publisher" },
            { Constants.TRCK, "Track number/Position in set" },
            { Constants.TRSN, "Internet radio station name" },
            { Constants.TRSO, "Internet radio station owner" },
            { Constants.TSOA, "Album sort order" },
            { Constants.TSOP, "Performer sort order" },
            { Constants.TSOT, "Title sort order" },
            { Constants.TSRC, "ISRC (international standard recording code)" },
            { Constants.TSSE, "Software/Hardware and settings used for encoding" },
            { Constants.TSST, "Set subtitle" },
            { Constants.TXXX, "User defined text information frame" },
            { Constants.UFID, "Unique file identifier" },
            { Constants.USER, "Terms of use" },
            { Constants.USLT, "Unsynchronised lyric/text transcription" },
            { Constants.WCOM, "Commercial information" },
            { Constants.WCOP, "Copyright/Legal information" },
            { Constants.WOAF, "Official audio file webpage" },
            { Constants.WOAR, "Official artist/performer webpage" },
            { Constants.WOAS, "Official audio source webpage" },
            { Constants.WORS, "Official Internet radio station homepage" },
            { Constants.WPAY, "Payment" },
            { Constants.WPUB, "Publishers official webpage" },
            { Constants.WXXX, "User defined URL link frame" }
        };
    }
}
