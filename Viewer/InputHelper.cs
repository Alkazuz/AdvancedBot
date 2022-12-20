using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AdvancedBot.Viewer
{
    public class InputHelper
    {
        private const int VK_LBUTTON = 0x01;
        private const int VK_RBUTTON = 0x02;

        [DllImport("user32.dll")] private static extern short GetKeyState(int nVirtKey);
        public static bool IsKeyDown(Keys key)
        {
            return (GetKeyState((int)key) & 0x100) != 0;
        }
        public static MouseButtons GetMouseState()
        {
            if ((GetKeyState(VK_LBUTTON) & 0x100) != 0) return MouseButtons.Left;
            if ((GetKeyState(VK_RBUTTON) & 0x100) != 0) return MouseButtons.Right;
            return MouseButtons.None;
        }
    }
}
