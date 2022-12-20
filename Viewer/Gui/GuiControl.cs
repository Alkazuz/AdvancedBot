using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace AdvancedBot.Viewer.Gui
{
    public abstract class GuiControl
    {
        private static int counter = 0;

        public bool Visible { get; set; } = true;

        public int ID { get; set; } = ++counter;

        public int X { get; set; }
        public int Y { get; set; }

        public abstract void Draw(Font font, int cx, int cy);
        public virtual void MouseMove(int cx, int cy, MouseButtons btn) { }
        public virtual void MouseDown(int cx, int cy, MouseButtons btn) { }
        public virtual void MouseUp(int cx, int cy, MouseButtons btn) { }
        public virtual void KeyPress(char chr) { }
        public abstract Rectangle GetBounds(Font fnt);
    }
}
