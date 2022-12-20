using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedBot.client
{
    public class PacketLogger
    {
        private static StreamWriter sw;
        private static StringBuilder sb;
        private static int counter = 0;

        public static void Init()
        {
            sb = new StringBuilder();
            sw = new StreamWriter("packet_log.txt", false, Encoding.Default);
            sw.AutoFlush = true;
        }
        public static void Log(ReadBuffer rb)
        {
            if (counter == 0) {
                Debug.WriteLine("PacketLogger is enabled");
            }
            sb.Clear();
            sb.AppendFormat("#{2:0000} Packet ID=0x{0:X2} Len={1:0.00} KiB\n", rb.ID, rb.Length / 1024.0, ++counter);
            HexUtil.DoAppendPrettyHexDump(sb, rb, 0, Math.Min(4096, rb.Length));

            sb.Append('\n');
            sw.Write(sb.ToString());
        }

        //https://github.com/Azure/DotNetty/blob/580bde0415a50e570eb9a40b08142d28569ec798/src/DotNetty.Buffers/ByteBufferUtil.cs
        static class HexUtil
        {
            static readonly char[] HexdumpTable = new char[256 * 4];
            static readonly string Newline = "\n";
            static readonly string[] Byte2Hex = new string[256];
            static readonly string[] HexPadding = new string[16];
            static readonly string[] BytePadding = new string[16];
            static readonly char[] Byte2Char = new char[256];

            static HexUtil()
            {
                char[] digits = "0123456789abcdef".ToCharArray();
                for (int i = 0; i < 256; i++) {
                    HexdumpTable[i << 1] = digits[(int)((uint)i >> 4 & 0x0F)];
                    HexdumpTable[(i << 1) + 1] = digits[i & 0x0F];
                }

                // Generate the lookup table for byte-to-hex-dump conversion
                for (int i = 0; i < Byte2Hex.Length; i++) {
                    Byte2Hex[i] = " " + i.ToString("X2");
                }

                // Generate the lookup table for hex dump paddings
                for (int i = 0; i < HexPadding.Length; i++) {
                    int padding = HexPadding.Length - i;
                    var buf = new StringBuilder(padding * 3);
                    for (int j = 0; j < padding; j++) {
                        buf.Append("   ");
                    }
                    HexPadding[i] = buf.ToString();
                }

                // Generate the lookup table for byte dump paddings
                for (int i = 0; i < BytePadding.Length; i++) {
                    int padding = BytePadding.Length - i;
                    var buf = new StringBuilder(padding);
                    for (int j = 0; j < padding; j++) {
                        buf.Append(' ');
                    }
                    BytePadding[i] = buf.ToString();
                }

                // Generate the lookup table for byte-to-char conversion
                for (int i = 0; i < Byte2Char.Length; i++) {
                    if (i <= 0x1f || i >= 0x7f) {
                        Byte2Char[i] = '.';
                    } else {
                        Byte2Char[i] = (char)i;
                    }
                }
            }

            public static string DoPrettyHexDump(ReadBuffer buffer, int offset, int length)
            {
                if (length == 0) {
                    return string.Empty;
                } else {
                    int rows = length / 16 + (length % 15 == 0 ? 0 : 1) + 4;
                    var buf = new StringBuilder(rows * 80);
                    DoAppendPrettyHexDump(buf, buffer, offset, length);
                    return buf.ToString();
                }
            }

            public static void DoAppendPrettyHexDump(StringBuilder dump, ReadBuffer buf, int offset, int length)
            {
                if (length == 0) {
                    return;
                }
                dump.Append(
                    "         +-------------------------------------------------+" +
                    Newline + "         |  0  1  2  3  4  5  6  7  8  9  a  b  c  d  e  f |" +
                    Newline + "+--------+-------------------------------------------------+----------------+");

                int startIndex = offset;
                int fullRows = (int)((uint)length >> 4);
                int remainder = length & 0xF;

                // Dump the rows which have 16 bytes.
                for (int row = 0; row < fullRows; row++) {
                    int rowStartIndex = (row << 4) + startIndex;

                    // Per-row prefix.
                    AppendHexDumpRowPrefix(dump, row, rowStartIndex);

                    // Hex dump
                    int rowEndIndex = rowStartIndex + 16;
                    for (int j = rowStartIndex; j < rowEndIndex; j++) {
                        dump.Append(Byte2Hex[buf.GetByte(j)]);
                    }
                    dump.Append(" |");

                    // ASCII dump
                    for (int j = rowStartIndex; j < rowEndIndex; j++) {
                        dump.Append(Byte2Char[buf.GetByte(j)]);
                    }
                    dump.Append('|');
                }

                // Dump the last row which has less than 16 bytes.
                if (remainder != 0) {
                    int rowStartIndex = (fullRows << 4) + startIndex;
                    AppendHexDumpRowPrefix(dump, fullRows, rowStartIndex);

                    // Hex dump
                    int rowEndIndex = rowStartIndex + remainder;
                    for (int j = rowStartIndex; j < rowEndIndex; j++) {
                        dump.Append(Byte2Hex[buf.GetByte(j)]);
                    }
                    dump.Append(HexPadding[remainder]);
                    dump.Append(" |");

                    // Ascii dump
                    for (int j = rowStartIndex; j < rowEndIndex; j++) {
                        dump.Append(Byte2Char[buf.GetByte(j)]);
                    }
                    dump.Append(BytePadding[remainder]);
                    dump.Append('|');
                }

                dump.Append(Newline + "+--------+-------------------------------------------------+----------------+");
            }

            static void AppendHexDumpRowPrefix(StringBuilder dump, int row, int rowStartIndex)
            {
                dump.Append('\n');
                dump.Append('|');
                dump.Append((row << 4).ToString("X8"));
                dump.Append('|');
            }
        }
    }
}
