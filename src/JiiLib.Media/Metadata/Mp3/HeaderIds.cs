using System;
using System.Collections.Generic;

namespace JiiLib.Media.Metadata.Mp3
{
    internal static class HeaderIds
    {
        /// <summary>
        ///     Attached picture
        /// </summary>
        internal const string APIC = "APIC";

        /// <summary>
        ///     Comments
        /// </summary>
        internal const string COMM = "COMM";

        /// <summary>
        ///     Commercial frame
        /// </summary>
        internal const string COMR = "COMR";

        /// <summary>
        ///     General encapsulated object
        /// </summary>
        internal const string GEOB = "GEOB";

        /// <summary>
        ///     Group identification registration
        /// </summary>
        internal const string GRID = "GRID";

        /// <summary>
        ///     Involved people list
        /// </summary>
        internal const string IPLS = "IPLS";

        /// <summary>
        ///     Linked information
        /// </summary>
        internal const string LINK = "LINK";

        /// <summary>
        ///     Music CD identifier
        /// </summary>
        internal const string MCDI = "MCDI";

        /// <summary>
        ///     Ownership frame
        /// </summary>
        internal const string OWNE = "OWNE";

        /// <summary>
        ///     Private frame
        /// </summary>
        internal const string PRIV = "PRIV";

        /// <summary>
        ///     Play counter
        /// </summary>
        internal const string PCNT = "PCNT";

        /// <summary>
        ///     Popularimeter
        /// </summary>
        internal const string POPM = "POPM";

        /// <summary>
        ///     Recommended buffer size
        /// </summary>
        internal const string RBUF = "RBUF";

        /// <summary>
        ///     Relative volume adjustment
        /// </summary>
        internal const string RVA2 = "RVA2";

        /// <summary>
        ///     Relative volume adjustment
        /// </summary>
        internal const string RVAD = "RVAD";

        /// <summary>
        ///     Reverb
        /// </summary>
        internal const string RVRB = "RVRB";

        /// <summary>
        ///   Album/Movie/Show title
        /// </summary>
        internal const string TALB = "TALB";

        /// <summary>
        ///   BPM (beats per minute)
        /// </summary>
        internal const string TBPM = "TBPM";

        /// <summary>
        ///   Composer
        /// </summary>
        internal const string TCOM = "TCOM";

        /// <summary>
        ///   Content type
        /// </summary>
        internal const string TCON = "TCON";

        /// <summary>
        ///   Copyright message
        /// </summary>
        internal const string TCOP = "TCOP";

        /// <summary>
        ///   Date
        /// </summary>
        internal const string TDAT = "TDAT";

        /// <summary>
        ///   Playlist delay
        /// </summary>
        internal const string TDLY = "TDLY";

        /// <summary>
        ///   Encoded by
        /// </summary>
        internal const string TENC = "TENC";

        /// <summary>
        ///   Lyricist/Text writer
        /// </summary>
        internal const string TEXT = "TEXT";

        /// <summary>
        ///   File type
        /// </summary>
        internal const string TFLT = "TFLT";

        /// <summary>
        ///     Time
        /// </summary>
        internal const string TIME = "TIME";

        /// <summary>
        ///   Content group description
        /// </summary>
        internal const string TIT1 = "TIT1";

        /// <summary>
        ///   Title/songname/content description
        /// </summary>
        internal const string TIT2 = "TIT2";

        /// <summary>
        ///   Subtitle/Description refinement
        /// </summary>
        internal const string TIT3 = "TIT3";

        /// <summary>
        ///   Initial key
        /// </summary>
        internal const string TKEY = "TKEY";

        /// <summary>
        ///   Language(s)
        /// </summary>
        internal const string TLAN = "TLAN";

        /// <summary>
        ///   Length
        /// </summary>
        internal const string TLEN = "TLEN";

        /// <summary>
        ///   Media type
        /// </summary>
        internal const string TMED = "TMED";

        /// <summary>
        ///   Original album/movie/show title
        /// </summary>
        internal const string TOAL = "TOAL";

        /// <summary>
        ///    Original filename
        /// </summary>
        internal const string TOFN = "TOFN";

        /// <summary>
        ///     Original lyricist(s)/text writer(s)
        /// </summary>
        internal const string TOLY = "TOLY";

        /// <summary>
        ///    Original artist(s)/performer(s)
        /// </summary>
        internal const string TOPE = "TOPE";

        /// <summary>
        ///     Original release year
        /// </summary>
        internal const string TORY = "TORY";

        /// <summary>
        ///   File owner/licensee
        /// </summary>
        internal const string TOWN = "TOWN";

        /// <summary>
        ///   Lead performer(s)/Soloist(s)
        /// </summary>
        internal const string TPE1 = "TPE1";

        /// <summary>
        ///   Band/orchestra/accompaniment
        /// </summary>
        internal const string TPE2 = "TPE2";

        /// <summary>
        ///   Conductor/performer refinement
        /// </summary>
        internal const string TPE3 = "TPE3";

        /// <summary>
        ///   Interpreted, remixed, or otherwise modified by
        /// </summary>
        internal const string TPE4 = "TPE4";

        /// <summary>
        ///   Part of a set
        /// </summary>
        internal const string TPOS = "TPOS";

        /// <summary>
        ///     Publisher
        /// </summary>
        internal const string TPUB = "TPUB";

        /// <summary>
        ///     Track number/Position in set
        /// </summary>
        internal const string TRCK = "TRCK";

        /// <summary>
        ///     Recording dates
        /// </summary>
        internal const string TRDA = "TRDA";

        /// <summary>
        ///     Internet radio station name
        /// </summary>
        internal const string TRSN = "TRSN";

        /// <summary>
        ///     Internet radio station owner
        /// </summary>
        internal const string TRSO = "TRSO";

        /// <summary>
        ///     Size
        /// </summary>
        internal const string TSIZ = "TSIZ";

        /// <summary>
        ///     ISRC (international standard recording code)
        /// </summary>
        internal const string TSRC = "TSRC";

        /// <summary>
        ///    Software/Hardware and settings used for encoding
        /// </summary>
        internal const string TSSE = "TSSE";

        /// <summary>
        ///     Year
        /// </summary>
        internal const string TYER = "TYER";

        /// <summary>
        ///     User defined text information frame
        /// </summary>
        internal const string TXXX = "TXXX";

        /// <summary>
        ///     Unique file identifier
        /// </summary>
        internal const string UFID = "UFID";

        /// <summary>
        ///     Terms of use
        /// </summary>
        internal const string USER = "USER";

        /// <summary>
        ///     Unsychronized lyric/text transcription
        /// </summary>
        internal const string USLT = "USLT";

        /// <summary>
        ///     Commercial information
        /// </summary>
        internal const string WCOM = "WCOM";

        /// <summary>
        ///     Copyright/Legal information
        /// </summary>
        internal const string WCOP = "WCOP";

        /// <summary>
        ///     Official audio file webpage
        /// </summary>
        internal const string WOAF = "WOAF";

        /// <summary>
        ///     Official artist/performer webpage
        /// </summary>
        internal const string WOAR = "WOAR";

        /// <summary>
        ///     Official audio source webpage
        /// </summary>
        internal const string WOAS = "WOAS";

        /// <summary>
        ///     Official internet radio station homepage
        /// </summary>
        internal const string WORS = "WORS";

        /// <summary>
        ///     Payment
        /// </summary>
        internal const string WPAY = "WPAY";

        /// <summary>
        ///     Publishers official webpage
        /// </summary>
        internal const string WPUB = "WPUB";

        /// <summary>
        ///     User defined URL link frame
        /// </summary>
        internal const string WXXX = "WXXX";

        // Unsupported
        ///// <summary>
        /////     Audio encryption
        ///// </summary>
        //internal const string AENC = "AENC";
        //internal const string ENCR = "ENCR";
        //internal const string ETCO = "ETCO";
        //internal const string EQUA = "EQUA";

        ///// <summary>
        /////     MPEG location lookup table
        ///// </summary>
        //internal const string MLLT = "MLLT";

        ///// <summary>
        /////   Position synchronisation frame
        ///// </summary>
        //internal const string POSS = "POSS";

        ///// <summary>
        /////     
        ///// </summary>
        //internal const string SEEK = "SEEK";

        ///// <summary>
        /////   
        ///// </summary>
        //internal const string SIGN = "SIGN";

        ///// <summary>
        /////   
        ///// </summary>
        //internal const string SYLT = "SYLT";

        ///// <summary>
        /////   
        ///// </summary>
        //internal const string SYTC = "SYTC";

        ///// <summary>
        /////   
        ///// </summary>
        //internal const string TIPL = "TIPL";

        ///// <summary>
        /////   
        ///// </summary>
        //internal const string TMCL = "TMCL";

        ///// <summary>
        /////   
        ///// </summary>
        //internal const string TMOO = "TMOO";

        ///// <summary>
        /////     
        ///// </summary>
        //internal const string TPRO = "TPRO";

        ///// <summary>
        /////   
        ///// </summary>
        //internal const string TSOA = "TSOA";

        ///// <summary>
        /////   
        ///// </summary>
        //internal const string TSOP = "TSOP";

        ///// <summary>
        /////   
        ///// </summary>
        //internal const string TSOT = "TSOT";

        ///// <summary>
        /////   
        ///// </summary>
        //internal const string TSST = "TSST";

        // v2.4 tags
        //internal const string ASPI = "ASPI";
        //internal const string EQU2 = "EQU2";

        //internal const string TDEN = "TDEN";
        //internal const string TDOR = "TDOR";
        //internal const string TDRC = "TDRC";
        //internal const string TDRL = "TDRL";
        //internal const string TDTG = "TDTG";

        internal static Dictionary<string, string> FrameDict { get; } = new Dictionary<string, string>
        {
            //[AENC] = "Audio encryption",
            [APIC] = "Attached picture",
            //[ASPI] = "Audio seek point index",
            [COMM] = "Comments",
            [COMR] = "Commercial frame",
            //[ENCR] = "Encryption method registration",
            //[EQU2] = "Equalisation (2)",
            //[EQUA] = "Equalization",
            //[ETCO] = "Event timing codes",
            [GEOB] = "General encapsulated object",
            [GRID] = "Group identification registration",
            [IPLS] = "Involved people list",
            [LINK] = "Linked information",
            [MCDI] = "Music CD identifier",
            //[MLLT] = "MPEG location lookup table",
            [OWNE] = "Ownership frame",
            [PRIV] = "Private frame",
            [PCNT] = "Play counter",
            [POPM] = "Popularimeter",
            //[POSS] = "Position synchronisation frame",
            [RBUF] = "Recommended buffer size",
            [RVA2] = "Relative volume adjustment (2)",
            [RVAD] = "Relative volume adjustment",
            [RVRB] = "Reverb",
            //[SEEK] = "Seek frame",
            //[SIGN] = "Signature frame",
            //[SYLT] = "Synchronised lyric/text",
            //[SYTC] = "Synchronised tempo codes",
            [TALB] = "Album/Movie/Show title",
            [TBPM] = "BPM (beats per minute)",
            [TCOM] = "Composer",
            [TCON] = "Content type",
            [TCOP] = "Copyright message",
            [TDAT] = "Date",
            //[TDEN] = "Encoding time",
            [TDLY] = "Playlist delay",
            //[TDOR] = "Original release time",
            //[TDRC] = "Recording time",
            //[TDRL] = "Release time",
            //[TDTG] = "Tagging time",
            [TENC] = "Encoded by",
            [TEXT] = "Lyricist/Text writer",
            [TFLT] = "File type",
            [TIME] = "Time",
            //{ TIPL, "Involved people list",
            [TIT1] = "Content group description",
            [TIT2] = "Title/songname/content description",
            [TIT3] = "Subtitle/Description refinement",
            [TKEY] = "Initial key",
            [TLAN] = "Language(s)",
            [TLEN] = "Length",
            //[TMCL] = "Musician credits list",
            [TMED] = "Media type",
            //[TMOO] = "Mood",
            [TOAL] = "Original album/movie/show title",
            [TOFN] = "Original filename",
            [TOLY] = "Original lyricist(s)/text writer(s)",
            [TOPE] = "Original artist(s)/performer(s)",
            [TORY] = "Original release year",
            [TOWN] = "File owner/licensee",
            [TPE1] = "Lead performer(s)/Soloist(s)",
            [TPE2] = "Band/orchestra/accompaniment",
            [TPE3] = "Conductor/performer refinement",
            [TPE4] = "Interpreted, remixed, or otherwise modified by",
            [TPOS] = "Part of a set",
            //[TPRO] = "Produced notice",
            [TPUB] = "Publisher",
            [TRCK] = "Track number/Position in set",
            [TRDA] = "Recording dates",
            [TRSN] = "Internet radio station name",
            [TRSO] = "Internet radio station owner",
            [TSIZ] = "Size",
            //[TSOA] = "Album sort order",
            //[TSOP] = "Performer sort order",
            //[TSOT] = "Title sort order",
            [TSRC] = "ISRC (international standard recording code)",
            [TSSE] = "Software/Hardware and settings used for encoding",
            //[TSST] = "Set subtitle",
            [TYER] = "Year",
            [TXXX] = "User defined text information frame",
            [UFID] = "Unique file identifier",
            [USER] = "Terms of use",
            [USLT] = "Unsynchronised lyric/text transcription",
            [WCOM] = "Commercial information",
            [WCOP] = "Copyright/Legal information",
            [WOAF] = "Official audio file webpage",
            [WOAR] = "Official artist/performer webpage",
            [WOAS] = "Official audio source webpage",
            [WORS] = "Official Internet radio station homepage",
            [WPAY] = "Payment",
            [WPUB] = "Publishers official webpage",
            [WXXX] = "User defined URL link frame"
        };
    }
}