using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace AdvancedBot.Controls
{
    public class PercentageProgressBar : ProgressBar
    {
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == 0x0F) { //WM_PAINT
                using (Graphics g = CreateGraphics()) {
                    TextRenderer.DrawText(g, (Value / (float)Maximum).ToString("#0.00%"), Font, new Rectangle(0, 0, Width, Height), Color.Black);
                }
            }
        }
    }
}
