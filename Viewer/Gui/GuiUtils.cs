using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedBot.Viewer.Gui
{
    public class GuiUtils
    {
        public const int RGBA_FULL_ALPHA = 0xFF << 24;

        public static void DrawTooltip(string text, int x, int y, Font fnt, int width, int height)
        {
            string[] textLines = text.Split('\n');
            if (textLines.Length > 0) {
                GL.glPushMatrix();

                x /= 2;
                y /= 2;
                GL.glScalef(2, 2, 1);
                GL.glTranslated(0, 0, 100);

                int w = 0;

                foreach (string ln in textLines) {
                    int sw = fnt.Measure(ln);
                    if (sw > w)
                        w = sw;
                }

                int posX = x + 12;
                int posY = y - 12;
                int h = 8;

                if (textLines.Length > 1)
                    h += 2 + (textLines.Length - 1) * 10;

                if (posX + w > width)
                    posX -= 28 + w;

                if (posY + h + 6 > height)
                    posY = height - h - 6;

                GL.glDisable(GL.GL_TEXTURE_2D);
                int bgColor = unchecked((int)0xF0100010);
                DrawGradientRect(posX - 3, posY - 4, posX + w + 3, posY - 3, bgColor, bgColor);
                DrawGradientRect(posX - 3, posY + h + 3, posX + w + 3, posY + h + 4, bgColor, bgColor);
                DrawGradientRect(posX - 3, posY - 3, posX + w + 3, posY + h + 3, bgColor, bgColor);
                DrawGradientRect(posX - 4, posY - 3, posX - 3, posY + h + 3, bgColor, bgColor);
                DrawGradientRect(posX + w + 3, posY - 3, posX + w + 4, posY + h + 3, bgColor, bgColor);
                int lnColor1 = 0x505000FF;
                int lnColor2 = (lnColor1 & 0xFEFEFE) >> 1 | lnColor1 & unchecked((int)0xFF000000);
                DrawGradientRect(posX - 3, posY - 3 + 1, posX - 3 + 1, posY + h + 3 - 1, lnColor1, lnColor2);
                DrawGradientRect(posX + w + 2, posY - 3 + 1, posX + w + 3, posY + h + 3 - 1, lnColor1, lnColor2);
                DrawGradientRect(posX - 3, posY - 3, posX + w + 3, posY - 3 + 1, lnColor1, lnColor1);
                DrawGradientRect(posX - 3, posY + h + 2, posX + w + 3, posY + h + 3, lnColor2, lnColor2);

                GL.glEnable(GL.GL_TEXTURE_2D);
                for (int i = 0; i < textLines.Length; i++) {
                    fnt.Draw(textLines[i], posX, posY, false);

                    if (i == 0)
                        posY += 2;

                    posY += 10;
                }
                GL.glDisable(GL.GL_TEXTURE_2D);
                GL.glPopMatrix();
            }
        }

        public static void DrawGradientRect(int left, int top, int right, int bottom, int startColor, int endColor)
        {
            float sa = (startColor >> 24 & 0xFF) / 255.0f;
            float sr = (startColor >> 16 & 0xFF) / 255.0f;
            float sg = (startColor >> 8 & 0xFF) / 255.0f;
            float sb = (startColor & 0xFF) / 255.0f;
            float ea = (endColor >> 24 & 0xFF) / 255.0f;
            float er = (endColor >> 16 & 0xFF) / 255.0f;
            float eg = (endColor >> 8 & 0xFF) / 255.0f;
            float ab = (endColor & 0xFF) / 255.0f;

            GL.glBegin(GL.GL_QUADS);
            GL.glColor4f(sr, sg, sb, sa);
            GL.glVertex2i(right, top);
            GL.glVertex2i(left, top);
            GL.glColor4f(er, eg, ab, ea);
            GL.glVertex2i(left, bottom);
            GL.glVertex2i(right, bottom);
            GL.glEnd();
        }
        public static void DrawRectangle(int x, int y, int w, int h, int mode = GL.GL_QUADS)
        {
            GL.glBegin(mode);
            GL.glVertex2i(x - (mode == GL.GL_LINE_LOOP ? 1 : 0), y);
            GL.glVertex2i(x, y + h);
            GL.glVertex2i(x + w, y + h);
            GL.glVertex2i(x + w, y);
            GL.glEnd();
        }

        public static void DrawTexturedRect(int x, int y, int w, int h, int texSrcW, int texSrcH, int texX, int texY, int texW, int texH)
        {
            float u0 = texX / (float)texW;
            float u1 = (texX + texSrcW) / (float)texW;
            float v0 = texY / (float)texH;
            float v1 = (texY + texSrcH) / (float)texH;

            GL.glBegin(GL.GL_QUADS);
            GL.glTexCoord2f(u0, v1); GL.glVertex2f(x, y + h);
            GL.glTexCoord2f(u1, v1); GL.glVertex2f(x + w, y + h);
            GL.glTexCoord2f(u1, v0); GL.glVertex2f(x + w, y);
            GL.glTexCoord2f(u0, v0); GL.glVertex2f(x, y);
            GL.glEnd();
        }

        public static bool PointInRect(int rx, int ry, int rw, int rh, int px, int py)
        {
            return (px >= rx && px <= rx + rw) &&
                   (py >= ry && py <= ry + rh);
        }
    }
}
