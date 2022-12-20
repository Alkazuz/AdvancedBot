using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;

namespace AdvancedBot.Viewer.Gui
{
    public class GuiCheckBox : GuiControl
    {
        public event EventHandler OnCheckChanged;
        public bool IsChecked = true;

        private bool isMouseDownOverBox = false;

        public string Text;

        public GuiCheckBox(int x, int y, string txt)
        {
            X = x;
            Y = y;
            Text = txt;
        }

        const int BOX_W = 18;
        const int BOX_H = 18;
        private static readonly float[] CHECK_MARK = {
            0.0684f, 0.4784f,
            0.3986f, 0.8154f,
            0.9226f, 0.0841f
        };

        public override void Draw(Font fnt, int cx, int cy)
        {
            GL.glDisable(GL.GL_LINE_SMOOTH);
            GL.glColor3f(1f, 1f, 1f);
            GuiUtils.DrawRectangle(X, Y, BOX_W, BOX_H, GL.GL_LINE_LOOP);
            if (IsChecked) {
                GL.glColor4f(0f, 0.6f, 0.0f, 1.0f);
                GL.glLineWidth(2);
                GL.glEnable(GL.GL_LINE_SMOOTH);
                GL.glHint(GL.GL_LINE_SMOOTH_HINT, GL.GL_NICEST);
                
                GL.glBegin(GL.GL_LINE_STRIP);
                for (int i = 0; i < CHECK_MARK.Length; i += 2) {
                    const float CW = 14, CH = 13;
                    float x = 1 + CHECK_MARK[i + 0] * CW;
                    float y = 2 + CHECK_MARK[i + 1] * CH;

                    GL.glVertex2f(X + x, Y + y);
                }
                GL.glEnd();

                GL.glDisable(GL.GL_LINE_SMOOTH);
                GL.glLineWidth(1);
            }
            if (GuiUtils.PointInRect(X, Y, BOX_W, BOX_H, cx, cy)) {
                GL.glColor4f(1f, 1f, 1f, isMouseDownOverBox ? 0.5f : 0.3f);
                GuiUtils.DrawRectangle(X, Y, BOX_W, BOX_H);
            }
            if (Text != null) {
                GL.glEnable(GL.GL_TEXTURE_2D);
                fnt.Draw(Text, X + BOX_W + 4, Y + (BOX_H / 4), false);
                GL.glDisable(GL.GL_TEXTURE_2D);
            }
        }
        
        public override void MouseDown(int cx, int cy, MouseButtons btn)
        {
            isMouseDownOverBox = GuiUtils.PointInRect(X, Y, BOX_W, BOX_H, cx, cy);
        }
        public override void MouseUp(int cx, int cy, MouseButtons btn)
        {
            if (GuiUtils.PointInRect(X, Y, BOX_W, BOX_H, cx, cy)) {
                IsChecked = !IsChecked;
                OnCheckChanged?.Invoke(this, null);
            }
            isMouseDownOverBox = false;
        }


        public override Rectangle GetBounds(Font fnt)
        {
            return new Rectangle(X, Y, BOX_W + fnt.Measure(Text), BOX_H);
        }
    }
}
