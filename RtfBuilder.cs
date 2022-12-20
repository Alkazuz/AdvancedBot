using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Diagnostics;

namespace AdvancedBot
{
    public class RtfBuilder //based on https://www.codeproject.com/Articles/30902/RichText-Builder-StringBuilder-for-RTF
    {
        public float FontSize = 10f;
        private List<Color> colorTable = new List<Color>();

        private FontStyle fontStyle;
        private Color currentColor = SystemColors.WindowText;

        private char[] buf = new char[4096];
        private int bufLen = 4096;
        private int bufPos = 0;

        public static RtfBuilder Instance = new RtfBuilder();
        private RtfBuilder()
        {
            colorTable.Add(currentColor);
        }

        public void SetFontStyle(FontStyle style)
        {
            if (fontStyle != style)
            {
                if (style == FontStyle.Regular)
                {
                    if ((fontStyle & FontStyle.Bold) == FontStyle.Bold) AppendCopy("\\b0 ");
                    if ((fontStyle & FontStyle.Italic) == FontStyle.Italic) AppendCopy("\\i0 ");
                    if ((fontStyle & FontStyle.Underline) == FontStyle.Underline) AppendCopy("\\ulnone ");
                    if ((fontStyle & FontStyle.Strikeout) == FontStyle.Strikeout) AppendCopy("\\strike0 ");
                }
                else
                {
                    if ((style & FontStyle.Bold) == FontStyle.Bold) AppendCopy("\\b ");
                    if ((style & FontStyle.Italic) == FontStyle.Italic) AppendCopy("\\i ");
                    if ((style & FontStyle.Underline) == FontStyle.Underline) AppendCopy("\\ul ");
                    if ((style & FontStyle.Strikeout) == FontStyle.Strikeout) AppendCopy("\\strike ");
                }
                fontStyle = style;
            }
        }
        public void SetTextColor(Color color, bool force = false)
        {
            if (force || currentColor != color)
            {
                if (bufPos + 16 > bufLen) ExpandBuffer();
                AppendCopy("\\cf");
                AppendInt(ColorIndex(color));
                buf[bufPos++] = ' ';
                currentColor = color;
            }
        }
        private int ColorIndex(Color c)
        {
            int idx = colorTable.IndexOf(c);
            if (idx == -1)
            {
                colorTable.Add(c);
                return colorTable.Count;
            }
            return idx + 1;
        }
        public void Append(char ch)
        {
            if (bufPos + 8 > bufLen) ExpandBuffer();
            if (ch > 0xFF)
            {
                buf[bufPos++] = '\\';
                buf[bufPos++] = 'u';
                AppendInt((int)ch);
                buf[bufPos++] = '?';
            }
            else if (ch == '{' || ch == '}' || ch == '\\')
            {
                buf[bufPos++] = '\\';
                buf[bufPos++] = ch;
            }
            else if (ch == '\n')
            {
                AppendCopy("\\line ");
            }
            else
            {
                buf[bufPos++] = ch;
            }
        }
        public void Append(string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                Append(str[i]);
            }
        }
        public void AppendLine(string s)
        {
            Append(s);
            AppendCopy("\\line ");
        }
        public void AppendLink(string display, string url)
        {
            // \ul\cf2{{\field{\*\fldinst{HYPERLINK "https://abc.com"}}{\fldrslt{Test}}}}
            AppendCopy("\\ul{{\\field{\\*\\fldinst{HYPERLINK \"");
            AppendCopy(url.Replace("\"", "\\u34?"));
            AppendCopy("\"}}{\\fldrslt{");
            Append(display);
            AppendCopy("}}}}\\ulnone ");
        }

        //Copy the string to the dest buffer without escaping
        private void AppendCopy(string s)
        {
            int len = s.Length;
            if (bufPos + len >= bufLen) ExpandBuffer();
            for (int i = 0; i < len; i++)
            {
                buf[bufPos + i] = s[i];
            }
            bufPos += len;
        }
        private void ExpandBuffer()
        {
            char[] newBuf = new char[buf.Length * 2];
            Debug.WriteLine("Expanding RTF builder buffer to " + newBuf.Length + " characters");

            Buffer.BlockCopy(buf, 0, newBuf, 0, buf.Length * 2);
            buf = newBuf;
            bufLen = newBuf.Length;
        }

        public string ToRTF()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{\\rtf1\\ansi\\ansicpg1252\\deff0\\deflang3081");
            sb.Append("{\\fonttbl{{\f0\fswiss\fprq2\fcharset0 MS Reference Sans Serif;}}}\n");
            sb.Append("{\\colortbl ;");
            foreach (Color item in colorTable)
            {
                sb.AppendFormat("\\red{0}\\green{1}\\blue{2};", item.R, item.G, item.B);
            }

            sb.Append("}\n\\viewkind4\\uc1\\pard\\plain\\f0");

            sb.AppendFormat("\\fs{0} \n", (int)(FontSize * 2));

            sb.Append(buf, 0, bufPos);
            sb.Append("}");

            bufPos = 0;

            return sb.ToString();
        }

        private void AppendInt(int num)
        {
            int dc = num == 0 ? 1 : (int)Math.Log10(num);
            if (bufPos + dc >= bufLen) ExpandBuffer();
            for (int i = 0; num > 0; num /= 10, i++)
            {
                buf[bufPos + (dc - i)] = (char)('0' + (num % 10));
            }
            bufPos += dc + 1;
        }
    }
}
