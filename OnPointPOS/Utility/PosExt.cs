using System.IO;
using System.Text;

namespace POSSUM
{
    public static class PosExt
    {
        public static void PrintLogo(this BinaryWriter bw)
        {
            bw.Write(AsciiControlChars.FileSeparator);
            bw.Write((byte)112);
            bw.Write((byte)1);
            // bw.Write((byte)0);
            bw.Write((byte)48);
        }

        public static void Enlarged(this BinaryWriter bw, string text)
        {
            bw.Write(AsciiControlChars.Escape);
            bw.Write((byte)33);
            bw.Write((byte)8);
            Encoding iso = Encoding.GetEncoding("ISO-8859-1");
            Encoding utf8 = Encoding.UTF8;
            byte[] utfBytes = utf8.GetBytes(text);
            byte[] isoBytes = Encoding.Convert(utf8, iso, utfBytes);
            bw.Write(isoBytes);
            bw.Write(AsciiControlChars.Newline);
        }

        public static void High(this BinaryWriter bw, string text)
        {
            bw.Write(AsciiControlChars.Escape);
            bw.Write((byte)33);
            bw.Write((byte)16);
            if (Defaults.Language == CurrentLanguage.Arabic)
            {
                var utfEncoding = Encoding.GetEncoding("CP_1256");
                byte[] utfbytes = utfEncoding.GetBytes(text);
                string msg = utfEncoding.GetString(utfbytes);
                bw.Write(utfbytes);
            }
            else
            {
                Encoding iso = Encoding.GetEncoding("ISO-8859-1");
                Encoding utf8 = Encoding.UTF8;
                byte[] utfBytes = utf8.GetBytes(text);
                byte[] isoBytes = Encoding.Convert(utf8, iso, utfBytes);
                bw.Write(isoBytes);
            }
            //  bw.Write(Encoding.ASCII.GetBytes(text)); //Width,enlarged
            bw.Write(AsciiControlChars.Newline);
        }

        public static void LargeText(this BinaryWriter bw, string text)
        {
            bw.Write(AsciiControlChars.Escape);
            bw.Write((byte)33);
            bw.Write((byte)48);
            Encoding iso = Encoding.GetEncoding("ISO-8859-1");
            Encoding utf8 = Encoding.UTF8;
            byte[] utfBytes = utf8.GetBytes(text);
            byte[] isoBytes = Encoding.Convert(utf8, iso, utfBytes);
            bw.Write(isoBytes);
            bw.Write(AsciiControlChars.Newline);
        }

        public static void LargeTextCenter(this BinaryWriter bw, string text)
        {
            bw.Write(AsciiControlChars.Escape);
            bw.Write((byte)33);
            bw.Write((byte)48);
            bw.Write(AsciiControlChars.Escape);
            bw.Write((byte)97);
            bw.Write('1');
            Encoding iso = Encoding.GetEncoding("ISO-8859-1");
            Encoding utf8 = Encoding.UTF8;
            byte[] utfBytes = utf8.GetBytes(text);
            byte[] isoBytes = Encoding.Convert(utf8, iso, utfBytes);
            string msg = iso.GetString(isoBytes);
            bw.Write(isoBytes);
            bw.Write(AsciiControlChars.Newline);
            bw.Write(AsciiControlChars.Escape);
            bw.Write((byte)97);

        }

        public static void FeedLines(this BinaryWriter bw, int lines)
        {
            bw.Write(AsciiControlChars.Newline);
            if (lines > 0)
            {
                bw.Write(AsciiControlChars.Escape);
                bw.Write('d');
                bw.Write((byte)lines - 1);
            }
        }

        public static void FeedLine(this BinaryWriter bw)
        {
            FeedLines(bw, 1);
        }

        public static void Finish(this BinaryWriter bw)
        {
        }

        public static void NormalFont(this BinaryWriter bw, string text, bool line = true)
        {
            bw.Write(AsciiControlChars.Escape);
            bw.Write((byte)33);
            bw.Write((byte)0);
            bw.Write(AsciiControlChars.Escape);
            bw.Write((byte)97);
            bw.Write('0');

            Encoding iso = Encoding.GetEncoding("ISO-8859-1");
            Encoding utf8 = Encoding.UTF8;
            byte[] utfBytes = utf8.GetBytes(text);
            byte[] isoBytes = Encoding.Convert(utf8, iso, utfBytes);
            bw.Write(isoBytes);

            if (line)
            {
                bw.Write(AsciiControlChars.Newline);
            }
        }

        public static void NormalFontBold(this BinaryWriter bw, string text, bool line = true)
        {
            bw.Write(AsciiControlChars.Escape);
            bw.Write((byte)33);
            bw.Write((byte)0);
            bw.Write(AsciiControlChars.Escape);
            bw.Write((byte)71);
            bw.Write('1');

            Encoding iso = Encoding.GetEncoding("ISO-8859-1");
            Encoding utf8 = Encoding.UTF8;
            byte[] utfBytes = utf8.GetBytes(text);
            byte[] isoBytes = Encoding.Convert(utf8, iso, utfBytes);
            bw.Write(isoBytes);

            if (line)
                bw.Write(AsciiControlChars.Newline);

            bw.Write(AsciiControlChars.Escape);
            bw.Write((byte)71);
            bw.Write('0');
        }

        public static void NormalFontCenter(this BinaryWriter bw, string text, bool line = true)
        {
            bw.Write(AsciiControlChars.Escape);
            bw.Write((byte)33);
            bw.Write((byte)0);
            bw.Write(AsciiControlChars.Escape);
            bw.Write((byte)97);
            bw.Write('1');
            Encoding iso = Encoding.GetEncoding("ISO-8859-1");
            Encoding utf8 = Encoding.UTF8;
            byte[] utfBytes = utf8.GetBytes(text);
            byte[] isoBytes = Encoding.Convert(utf8, iso, utfBytes);
            string msg = iso.GetString(isoBytes);
            bw.Write(isoBytes);

            if (line)
                bw.Write(AsciiControlChars.Newline);
        }

        public static void NormalFontBoldCenter(this BinaryWriter bw, string text, bool line = true)
        {
            bw.Write(AsciiControlChars.Escape);
            bw.Write((byte)33);
            bw.Write((byte)0);
            bw.Write(AsciiControlChars.Escape);
            bw.Write((byte)71);
            bw.Write('1');
            Encoding iso = Encoding.GetEncoding("ISO-8859-1");
            Encoding utf8 = Encoding.UTF8;
            byte[] utfBytes = utf8.GetBytes(text);
            byte[] isoBytes = Encoding.Convert(utf8, iso, utfBytes);
            string msg = iso.GetString(isoBytes);
            bw.Write(isoBytes);

            if (line)
                bw.Write(AsciiControlChars.Newline);
            bw.Write(AsciiControlChars.Escape);
            bw.Write((byte)71);
            bw.Write('0');
        }

        /*
27 33 0     ESC ! NUL    Master style: pica                              ESC/P
27 33 1     ESC ! SOH    Master style: elite                             ESC/P
27 33 2     ESC ! STX    Master style: proportional                      ESC/P
27 33 4     ESC ! EOT    Master style: condensed                         ESC/P
27 33 8     ESC ! BS     Master style: emphasised                        ESC/P
27 33 16    ESC ! DLE    Master style: enhanced (double-strike)          ESC/P
27 33 32    ESC ! SP     Master style: enlarged (double-width)           ESC/P
27 33 64    ESC ! @      Master style: italic                            ESC/P
27 33 128   ESC ! ---    Master style: underline                         ESC/P
                     Above values can be added for combined styles.

        bw.Write(AsciiControlChars.Escape);
        bw.Write((byte)33);
        bw.Write((byte)0);
        bw.Write("test"); //Default, Pica
        bw.Write(AsciiControlChars.Newline);

        bw.Write(AsciiControlChars.Escape);
        bw.Write((byte)33);
        bw.Write((byte)4);
        bw.Write("test"); //condensed
        bw.Write(AsciiControlChars.Newline);
        bw.Write(AsciiControlChars.Escape);
        bw.Write((byte)33);
        bw.Write((byte)8);
        bw.Write("test"); //emphasised
        bw.Write(AsciiControlChars.Newline);
        bw.Write(AsciiControlChars.Escape);
        bw.Write((byte)33);
        bw.Write((byte)16);
        bw.Write("test"); //Height,enhanced
        bw.Write(AsciiControlChars.Newline);
        bw.Write(AsciiControlChars.Escape);
        bw.Write((byte)33);
        bw.Write((byte)32);
        bw.Write("test"); //Width,enlarged
        bw.Write(AsciiControlChars.Newline);
        bw.Write(AsciiControlChars.Escape);
        bw.Write((byte)33);
        bw.Write((byte)48);
        bw.Write("test");   //WxH
        bw.Write(AsciiControlChars.Newline);
             */
    }
}