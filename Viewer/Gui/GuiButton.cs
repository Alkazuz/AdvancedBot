using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AdvancedBot.Viewer.Gui
{
    public class GuiButton : GuiControl
    {
        public EventHandler OnClick;

        public string Text;
        public int Width, Height;

        private bool isMouseDownOver = false;

        public GuiButton(int x, int y, int w, int h, string txt)
        {
            X = x;
            Y = y;
            Width = w;
            Height = h;
            Text = txt;
        }

        public override void Draw(Font font, int cx, int cy)
        {
            bool curOver = GuiUtils.PointInRect(X, Y, Width, Height, cx, cy);;

            float c = isMouseDownOver ? 0.6f : curOver ? 0.55f : 0.5f;
            GL.glColor4f(c, c, c+0.02f, 0.8f);
            GuiUtils.DrawRectangle(X, Y, Width, Height);

            if (Text != null) {
                GL.glEnable(GL.GL_TEXTURE_2D);
                font.Draw(Text, X, Y, Width, Height, TextPosition.CenterAll);
                GL.glDisable(GL.GL_TEXTURE_2D);
            }
        }


        public override void MouseDown(int cx, int cy, MouseButtons btn)
        {
            isMouseDownOver = GuiUtils.PointInRect(X, Y, Width, Height, cx, cy);
        }
        public override void MouseUp(int cx, int cy, MouseButtons btn)
        {
            if (GuiUtils.PointInRect(X, Y, Width, Height, cx, cy)) {
                OnClick?.Invoke(this, null);
            }
            isMouseDownOver = false;
        }

        public override Rectangle GetBounds(Font fnt)
        {
            return new Rectangle(X, Y, Width, Height);
        }
    }
}
