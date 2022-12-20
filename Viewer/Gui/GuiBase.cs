using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AdvancedBot.Viewer.Gui
{
    public abstract class GuiBase
    {
        public List<GuiControl> Child { get; protected set; } = new List<GuiControl>();

        public bool Visible { get; protected set; } = true;

        protected bool DrawBackground { get; set; } = true;

        public virtual void MouseDown(int cx, int cy, MouseButtons btn)
        {
            foreach(var control in Child) {
                control.MouseDown(cx, cy, btn);
            }
        }
        public virtual void MouseMove(int cx, int cy, MouseButtons btn)
        {
            foreach (var control in Child) {
                control.MouseMove(cx, cy, btn);
            }
        }
        public virtual void MouseUp(int cx, int cy, MouseButtons btn)
        {
            foreach (var control in Child) {
                control.MouseUp(cx, cy, btn);
            }
        }

        public virtual void KeyPress(char chr)
        {
            foreach (var control in Child) {
                control.KeyPress(chr);
            }
        }
        public virtual void KeyDown(KeyEventArgs e) { }
        public virtual void KeyUp(KeyEventArgs e) { }
        public virtual void Draw(Font font, int cx, int cy, int w, int h)
        {
            GL.glDisable(GL.GL_TEXTURE_2D);
            if (DrawBackground) {
                GL.glColor4f(0f, 0f, 0f, 0.5f);
                GuiUtils.DrawRectangle(0, 0, w, h, GL.GL_QUADS);
                GL.glColor4f(1f, 1f, 1f, 1f);
            }
            foreach (var control in Child) {
                control.Draw(font, cx, cy);
            }
        }

        public GuiControl GetControl(int id)
        {
            return Child.FirstOrDefault(a => a.ID == id);
        }
    }
}
