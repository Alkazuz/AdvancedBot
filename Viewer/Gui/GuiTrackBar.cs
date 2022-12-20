using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace AdvancedBot.Viewer.Gui
{
    public class GuiTrackBar : GuiControl
    {
        public int ThumbWidth = 8;
        public int ThumbHeight = 12;

        public int RailHeight = 6;

        public int Width;

        public string TextFormat;
        public float Maximum;

        public float Value;

        public event EventHandler OnValueChanged;

        public GuiTrackBar(int x, int y, int w, string txtFmt, float max)
        {
            X = x;
            Y = y;
            Width = w;

            TextFormat = txtFmt;
            Maximum = max;
        }

        private bool isMouseDownOverThumb = false;
        public override void Draw(Font fnt, int cx, int cy)
        {
            GL.glEnable(GL.GL_LINE_SMOOTH);

            GL.glColor4f(0.4f, 0.4f, 0.4f, 1f);
            GuiUtils.DrawRectangle(X, Y, Width, RailHeight);

            Rectangle thumb = GetThumbRectangle();
            GuiUtils.DrawGradientRect(thumb.X, thumb.Y, thumb.X + thumb.Width, thumb.Y + thumb.Height, 0xD0D0D0 | GuiUtils.RGBA_FULL_ALPHA, 
                                                                                                       0xAAAAAA | GuiUtils.RGBA_FULL_ALPHA);
            GuiUtils.DrawRectangle(thumb.X, thumb.Y, thumb.Width, thumb.Height, GL.GL_LINE_LOOP);

            GL.glDisable(GL.GL_LINE_SMOOTH);


            GL.glEnable(GL.GL_TEXTURE_2D);
            fnt.Draw(string.Format(TextFormat, Value * Maximum), X + Width + 6, Y, false);
            GL.glDisable(GL.GL_TEXTURE_2D);
        }

        public override void MouseDown(int cx, int cy, MouseButtons btn)
        {
            if (GetThumbRectangle().Contains(cx, cy)) {
                MouseMove(cx, cy, btn);
                isMouseDownOverThumb = true;
            }
        }
        public override void MouseUp(int cx, int cy, MouseButtons btn)
        {
            isMouseDownOverThumb = false;
        }
        public override void MouseMove(int cx, int cy, MouseButtons btn)
        {
            if (isMouseDownOverThumb) {
                Value = Math.Max(0, Math.Min(1, (cx - X) / (float)Width));
                OnValueChanged?.Invoke(this, null);
            }
        }

        private Rectangle GetThumbRectangle()
        {
            int v = (int)(Value * Width);
            return new Rectangle(X + (v - (ThumbWidth / 2)), Y - ((ThumbHeight-RailHeight) / 2), ThumbWidth, ThumbHeight);
        }

        public override Rectangle GetBounds(Font fnt)
        {
            return new Rectangle(X, Y, Width + 6 + fnt.Measure(string.Format(TextFormat, Maximum)), ThumbHeight + 4);
        }
    }
}
