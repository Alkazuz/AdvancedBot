using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Imaging;
using System.Drawing;
using System.Runtime.InteropServices;

namespace AdvancedBot.Viewer
{
    public class Font
    {
        private const string CHAR_MAP = "\u00c0\u00c1\u00c2\u00c8\u00ca\u00cb\u00cd\u00d3\u00d4\u00d5\u00da\u00df\u00e3\u00f5\u011f\u0130\u0131\u0152\u0153\u015e\u015f\u0174\u0175\u017e\u0207\u0000\u0000\u0000\u0000\u0000\u0000\u0000 !\"#$%&\'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~\u0000\u00c7\u00fc\u00e9\u00e2\u00e4\u00e0\u00e5\u00e7\u00ea\u00eb\u00e8\u00ef\u00ee\u00ec\u00c4\u00c5\u00c9\u00e6\u00c6\u00f4\u00f6\u00f2\u00fb\u00f9\u00ff\u00d6\u00dc\u00f8\u00a3\u00d8\u00d7\u0192\u00e1\u00ed\u00f3\u00fa\u00f1\u00d1\u00aa\u00ba\u00bf\u00ae\u00ac\u00bd\u00bc\u00a1\u00ab\u00bb\u2591\u2592\u2593\u2502\u2524\u2561\u2562\u2556\u2555\u2563\u2551\u2557\u255d\u255c\u255b\u2510\u2514\u2534\u252c\u251c\u2500\u253c\u255e\u255f\u255a\u2554\u2569\u2566\u2560\u2550\u256c\u2567\u2568\u2564\u2565\u2559\u2558\u2552\u2553\u256b\u256a\u2518\u250c\u2588\u2584\u258c\u2590\u2580\u03b1\u03b2\u0393\u03c0\u03a3\u03c3\u03bc\u03c4\u03a6\u0398\u03a9\u03b4\u221e\u2205\u2208\u2229\u2261\u00b1\u2265\u2264\u2320\u2321\u00f7\u2248\u00b0\u2219\u00b7\u221a\u207f\u00b2\u25a0\u0000";
        private static readonly int CHAR_UNKNOWN = CHAR_MAP.IndexOf('?');

        private readonly int tex;
        private byte[] charWidths = new byte[65536];

        public Font(TextureManager texMngr)
        {
            int w;
            int[] rawPixels;
            using (Bitmap bmp = Properties.Resources.ascii) {
                w = bmp.Width;
                BitmapData data = bmp.LockBits(new Rectangle(0, 0, w, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                rawPixels = new int[w * bmp.Height];
                Marshal.Copy(data.Scan0, rawPixels, 0, rawPixels.Length);

                tex = texMngr.CreateTexture(data);
                bmp.UnlockBits(data);
            }

            for (int i = 0; i < CHAR_MAP.Length; i++) {
                char ch = CHAR_MAP[i];

                int xt = i % 16;
                int yt = i / 16;
                int x = 0;
                for (bool emptyColumn = false; x < 8 && !emptyColumn; x++) {
                    int xPixel = xt * 8 + x;
                    emptyColumn = true;
                    for (int y = 0; y < 8 && emptyColumn; ++y) {
                        int yPixel = (yt * 8 + y);
                        int pixel = rawPixels[(w * yPixel) + xPixel] >> 24 & 0xFF;
                        if (pixel != 0)
                            emptyColumn = false;
                    }
                }
                if (ch == 32)
                    x = 4;
                charWidths[ch] = (byte)x;
            }
        }
        public void Draw(string text, int x, int y, bool is3D)
        {
            GL.glBindTexture(GL.GL_TEXTURE_2D, tex);

            double z = is3D ? 0.1 : 100;
            if (is3D) {
                DoDraw(text, x + 1, y + 1, true);
                GL.glTranslated(0, 0, z);
                DoDraw(text, x, y, false);
            } else {
                GL.glTranslated(0, 0, z);
                DoDraw(text, x + 1, y + 1, true);
                DoDraw(text, x, y, false);
            }
            GL.glTranslated(0, 0, -z);
        }
        public void Draw(string text, int x, int y, int w, int h, TextPosition pos)
        {
            int tw = pos == TextPosition.Left ? 0 : Measure(text);
            switch(pos) {
                case TextPosition.Left: Draw(text, x, y, false); break;
                case TextPosition.Right: Draw(text, w - tw - x, y, false); break;
                default: {
                    if((pos & TextPosition.CenterHorizontal) != 0) {
                        x = x + (w - tw) / 2;
                    }
                    if((pos & TextPosition.CenterVertical) != 0) {
                        y = y + (h - 10) / 2;
                    }

                    Draw(text, x, y, false);
                    break;
                }
            }
        }
        private void DoDraw(string text, int x, int y, bool dark)
        {
            int xo = x;

            if (dark) {
                GL.glColor3f(0.25f, 0.25f, 0.25f);
            } else {
                GL.glColor3f(1.00f, 1.00f, 1.00f);
            }

            for (int i = 0; i < text.Length; i++) {
                char c = (char)(text[i] & 0xFF);

                if (c == '§' && i + 1 < text.Length) {
                    int col = "0123456789abcdef".IndexOf(text[++i]);
                    if (col != -1) {
                        int br = (col >> 3 & 0x01) * 0x55;
                        int r = (col >> 2 & 0x01) * 0xAA + br;
                        int g = (col >> 1 & 0x01) * 0xAA + br;
                        int b = (col & 0x01) * 0xAA + br;

                        if (col == 6) {
                            r += 0x55;
                        }

                        if (dark) {
                            r /= 4;
                            g /= 4;
                            b /= 4;
                        }
                        GL.glColor3f(r / 255f, g / 255f, b / 255f);
                    }
                    continue;
                }
                if (c == '\r') {
                    continue;
                } else if (c == '\n') {
                    y += 9;
                    x = xo;
                    continue;
                }

                int ch = CHAR_MAP.IndexOf(c);
                if (ch == -1) {
                    ch = CHAR_UNKNOWN;
                }

                float ix = ch % 16 * 8;
                float iy = ch / 16 * 8;

                int width = charWidths[c];
                if (c != ' ') {
                    GL.glBegin(GL.GL_TRIANGLE_STRIP);
                    GL.glTexCoord2f(ix / 128f, iy / 128f);
                    GL.glVertex2f(x, y);
                    GL.glTexCoord2f(ix / 128f, (iy + 8f) / 128f);
                    GL.glVertex2f(x, y + 8f);
                    GL.glTexCoord2f((ix + width - 1f) / 128f, iy / 128f);
                    GL.glVertex2f(x + width - 1f, y);
                    GL.glTexCoord2f((ix + width - 1f) / 128f, (iy + 8f) / 128f);
                    GL.glVertex2f(x + width - 1f, y + 8f);
                    GL.glEnd();
                }

                x += width;
            }
        }

        public int Measure(string text)
        {
            return Measure(text, 0, text.Length);
        }
        public int Measure(string text, int offset, int len)
        {
            int width = 0;
            for (int i = 0; i < len; i++) {
                int c = text[i];
                if (c == '§' && i + 1 < len) {
                    i++;
                } else {
                    width += charWidths[c];
                }
            }
            return width;
        }
    }
    public enum TextPosition
    {
        Left             = 0x00,
        CenterHorizontal = 0x01,
        Right            = 0x02,
        CenterVertical   = 0x04,

        CenterAll        = CenterHorizontal | CenterVertical
    }
}
