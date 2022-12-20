﻿using System;
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

namespace AdvancedBot
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
        public class AllItems { };

        private Point mouseDownPos = new Point(-999, -999);
        private bool isSelecting = false;
        private int selStart = -1;
        private int selEnd = -1;
        public int SelectionLength
        {
            get {
                if (selStart == -1)
                    return 0;

                int c = Items.Count;
                if (c > 1) c += 1;
                int rEnd = Math.Min(c, Math.Max(selStart, selEnd));
                int rStart = Math.Min(c, Math.Min(selStart, selEnd));
                return (rEnd - rStart) + 1;
            }
        }
        public int SelectionStart
        {
            get { return Math.Min(selStart, selEnd); }
            set { selStart = value; }
        }
        public int SelectionEnd
        {
            get { return Math.Max(selStart, selEnd); }
            set { selEnd = value; }
        }

        const string HEAVY_BALLOT_X = "✘";
        const string HEAVY_CHECK_MARK = "✔";
        const string HEAVY_ASTERISK = "✱";
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;

            g.Clear(BackColor);

            int w = ClientRectangle.Width;
            int h = ClientRectangle.Height;

            AutoScrollMinSize = new Size(0, (Items.Count * ITEM_HEIGHT) + 2);
            g.TranslateTransform(0, AutoScrollPosition.Y);

            var font = Font;

            int io = Items.Count > 1 ? -1 : 0;
            for (int i = Math.Max(0, -(AutoScrollPosition.Y / ITEM_HEIGHT) - 1), ul = Math.Min(Items.Count, i + (h / ITEM_HEIGHT) + 3) + Math.Abs(io); i < ul; i++) {
                Rectangle itemRect = new Rectangle(1, (i * ITEM_HEIGHT) + 1, w - 1, ITEM_HEIGHT);

                string text;

                string icon = null;
                Brush iconColor = null;
                bool isSelected = false;
                
                if(io == -1 && i == 0) {
                    text = "Todos";
                    isSelected = Selected is AllItems;

                    icon = HEAVY_ASTERISK;
                    iconColor = Brushes.DodgerBlue;
                } else {
                    var cli = Items[i + io];
                    text = cli.Username ?? cli.Email;
                    bool con = cli.IsBeingTicked();

                    icon = con ? HEAVY_CHECK_MARK : HEAVY_BALLOT_X;
                    iconColor = con ? Brushes.Green : Brushes.Red;
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
                g.DrawString(icon, font, iconColor, itemRect.X, itemRect.Y + 1);
                g.TextRenderingHint = TextRenderingHint.SystemDefault;
                itemRect.X += (int)g.MeasureString(icon, font).Width - 4;
                g.DrawString(text, font, Brushes.Black, itemRect);
            }
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left) {
                SetSelected(e.Y);
                ushort ks = GetKeyState(Keys.ShiftKey);
                if ((ks & 0x100) != 0) {
                    selEnd = IndexOfItemAt(e.Y);
                } else {
                    selStart = selEnd = IndexOfItemAt(e.Y);
                }
                if (selStart >= Proxies.Count) selStart = selEnd = -1;

                mouseDownPos = new Point(e.X, e.Y - AutoScrollPosition.Y - 26);
                if (SelectionChanged != null) SelectionChanged(this, EventArgs.Empty);
                Invalidate();
            }
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if(e.Button == MouseButtons.Left) {
                SetSelected(e.Y);
            }
        }
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            if (e.KeyChar <= 0x20 || e.KeyChar >= 0x7F)
                return;
            char cLow = Char.ToLower(e.KeyChar);

            int matchCount = 0, matchFirst = 0;


            int io = Items.Count > 1 ? -1 : 0;
            for (int i = 0; i < Items.Count + Math.Abs(io); i++) {
                string txt;
                if (io == -1 && i == 0) {
                    txt = "Todos";
                } else {
                    var c = Items[i + io];
                    txt = c.Username ?? c.Email;
                }
                if (Char.ToLower(txt[0]) == cLow && matchCount++ == 0) {
                    matchFirst = i;
                }
            }
            int idx = matchCount > 0 ? matchFirst + (searchCount++ % matchCount) : -1;
            if (searchIndex != idx) {
                Selected = idx == 0 && io == -1 ? (object)new AllItems() : Items[idx + io];
                AutoScrollPosition = new Point(0, (idx * 13) - (Height / 2));
                Invalidate();
            }
        }
        private void SetSelected(int y)
        {
            int io = Items.Count > 1 ? -1 : 0;
            int i = (y - AutoScrollPosition.Y) / ITEM_HEIGHT;

            bool hasChanged = false;
            if (io == -1 && i == 0) {
                hasChanged = Selected != (Selected = new AllItems());
            } else if (i >= 0 && i <= Items.Count) {
                hasChanged = Selected != (Selected = Items[i + io]);
            }
            if(hasChanged) {
                Invalidate();
                OnSelectedItemChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            Focus();
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
