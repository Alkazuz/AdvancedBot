using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace AdvancedBot.Viewer.Gui
{
    public interface IGuiControl
    {
        void Render(Font fnt, int cx, int cy);
        void MouseMove(int cx, int cy);
        void MouseDown(int cx, int cy);
        void MouseUp(int cx, int cy);

        Rectangle GetBounds(Font fnt);
    }
}
