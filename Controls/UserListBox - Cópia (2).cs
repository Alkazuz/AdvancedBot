using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Drawing.Text;
using AdvancedBot.Client;
using System.Windows.Forms.VisualStyles;
using System.Drawing.Drawing2D;

namespace AdvancedBot.Controls
{
    public class UserListBox : ScrollableControl
    {
        public UserListBox()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true);
        }
        public new Color DefaultBackColor { get { return Color.White; } }
        public List<MinecraftClient> Items = new List<MinecraftClient>();
        public event EventHandler OnSelectedItemChanged;

        private VisualStyleRenderer ItemSelectedRenderer;
        private const int ITEM_HEIGHT = 13;
        private int searchIndex = -1, searchCount = 0;

        public object Selected;
        public int SelectedIndex
        {
            get {
                if (Selected is ManyItems) {
                    return 0;
                } else if (Selected != null) {
                    for (int i = 0; i < Items.Count; i++) {
                        if (Items[i] == Selected) {
                            return i + (Items.Count > 1 ? 1 : 0);
                        }
                    }
                }
                return -1;
            }
        }
        public class ManyItems { };

        private Point mouseDownPos = new Point(-999, -999);

        const string HEAVY_BALLOT_X   = "✘";
        const string HEAVY_CHECK_MARK = "✔";
        const string HEAVY_ASTERISK   = "✱";
        const string CLOCKWISE_CIRCLE_ARROW = "↻";
        static readonly string ALL_ICONS = HEAVY_BALLOT_X + HEAVY_CHECK_MARK + HEAVY_ASTERISK + CLOCKWISE_CIRCLE_ARROW;

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;

            g.Clear(BackColor);

            int w = ClientRectangle.Width;
            int h = ClientRectangle.Height;

            int io = Items.Count > 1 ? 1 : 0;

            AutoScrollMinSize = new Size(0, ((Items.Count + io) * ITEM_HEIGHT) + 2);
            g.TranslateTransform(0, AutoScrollPosition.Y);

            var font = Font;

            int iconWidth = (int)g.MeasureString(ALL_ICONS, font).Width / ALL_ICONS.Length;

            for (int i = Math.Max(0, -(AutoScrollPosition.Y / ITEM_HEIGHT) - 1), ul = Math.Min(Items.Count, i + (h / ITEM_HEIGHT) + 3) + io; i < ul; i++) {
                Rectangle itemRect = new Rectangle(1, (i * ITEM_HEIGHT) + 1, w - 1, ITEM_HEIGHT);

                string text;

                string icon = null;
                Brush iconColor = null;
                bool isSelected = false;
                
                if(io == 1 && i == 0) {
                    text = "Todos";
                    isSelected = Selected is ManyItems;

                    icon = HEAVY_ASTERISK;
                    iconColor = Brushes.DodgerBlue;
                } else {
                    var cli = Items[i - io];
                    text = cli.Username ?? cli.Email;

                    switch(cli.ConnState) {
                        case ConnectionState.Disconnected: icon = HEAVY_BALLOT_X; iconColor = Brushes.Red; break;
                        case ConnectionState.Connecting:   icon = CLOCKWISE_CIRCLE_ARROW; iconColor = Brushes.Gray; break;
                        case ConnectionState.Connected:    icon = HEAVY_CHECK_MARK; iconColor = Brushes.Green; break;
                    }
                    isSelected = Selected == cli;
                }
                if(isSelected) {
                    if(Application.RenderWithVisualStyles) {
                        if(ItemSelectedRenderer == null) {
                            ItemSelectedRenderer = new VisualStyleRenderer("Explorer::ListView", 1, 3);
                        }
                        ItemSelectedRenderer.DrawBackground(g, itemRect);
                    } else {
                        g.FillRectangle(SystemBrushes.ActiveCaption, itemRect);
                        using (Pen pen = new Pen(Color.Black)) {
                            pen.DashStyle = DashStyle.Dot;
                            g.DrawRectangle(pen, itemRect);
                        }
                    }
                }
                g.TextRenderingHint = TextRenderingHint.AntiAlias;

                if(icon == CLOCKWISE_CIRCLE_ARROW) {
                    var state = g.Save();
                    var size = g.MeasureString(CLOCKWISE_CIRCLE_ARROW, font);
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    g.TranslateTransform(itemRect.X + (size.Width / 2) + 2, itemRect.Y + (size.Height / 2));
                    g.RotateTransform(Utils.CurrentTimeMillis() / 3f);
                    g.DrawString(CLOCKWISE_CIRCLE_ARROW, font, Brushes.Gray, -(size.Width / 2), -(size.Height / 2) + 1);
                    g.Restore(state);
                } else {
                    g.DrawString(icon, font, iconColor, itemRect.X, itemRect.Y + 0.5f);
                }

                itemRect.X += iconWidth;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                g.DrawString(text, font, Brushes.Black, itemRect);
            }
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left) {
                Focus();
                SetSelected(e.Y);
            }
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if(e.Button == MouseButtons.Left) {
                SetSelected(e.Y);
            }
        }
        
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            int n = e.KeyCode == Keys.Up ? -1 : e.KeyCode == Keys.Down ? 1 : 0;
            if (n != 0) {
                SetSelected((SelectedIndex + n) * ITEM_HEIGHT + AutoScrollPosition.Y);
            }
        }
        protected override bool IsInputKey(Keys keyData)
        {
            return base.IsInputKey(keyData) || (keyData == Keys.Up || keyData == Keys.Down);
        }
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            if (e.KeyChar <= 0x20 || e.KeyChar >= 0x7F)
                return;
            char cLow = Char.ToLower(e.KeyChar);

            int matchCount = 0, matchFirst = 0;

            int io = Items.Count > 1 ? 1 : 0;
            for (int i = 0; i < Items.Count + io; i++) {
                string txt;
                if (io == 1 && i == 0) {
                    txt = "Todos";
                } else {
                    var c = Items[i - io];
                    txt = c.Username ?? c.Email;
                }
                if (Char.ToLower(txt[0]) == cLow && matchCount++ == 0) {
                    matchFirst = i;
                }
            }
            int idx = matchCount > 0 ? matchFirst + (searchCount++ % matchCount) : -1;
            if (searchIndex != idx) {
                Selected = idx == 0 && io == 1 ? (object)new ManyItems() : Items[idx - io];
                AutoScrollPosition = new Point(0, (idx * ITEM_HEIGHT) - (Height / 2));
                Invalidate();
            }
        }
        private void SetSelected(int y)
        {
            int io = Items.Count > 1 ? 1 : 0;
            int i = (y - AutoScrollPosition.Y) / ITEM_HEIGHT;

            bool hasChanged = false;
            if (io == 1 && i == 0) {
                hasChanged = Selected != (Selected = new ManyItems());
            } else if (i >= 0 && i < Items.Count + io) {
                hasChanged = Selected != (Selected = Items[i - io]);
            }
            if(hasChanged) {
                Invalidate();
                OnSelectedItemChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_VSCROLL || m.Msg == WM_MOUSEWHEEL)
                Invalidate();
            base.WndProc(ref m);
        }
        protected override CreateParams CreateParams {
            get {
                CreateParams cp = base.CreateParams;
                cp.Style |= 0x00800000; // WS_BORDER
                return cp;
            }
        }
        [DllImport("user32.dll")] private static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")] private static extern ushort GetKeyState(Keys nVirtKey);
        private const int WM_VSCROLL    = 0x0115,
                          WM_MOUSEWHEEL = 0x020A,
                          SB_PAGEBOTTOM = 0x0007,
                          SB_LINEUP     = 0,
                          SB_LINEDOWN   = 1;
    }
}
