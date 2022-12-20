using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Forms.VisualStyles;
using System.IO;
using System.Diagnostics;
using System.Drawing.Drawing2D;

namespace AdvancedBot.Controls
{
    public class CountryListBox : ScrollableControl
    {
        public List<Item> Items = new List<Item>();
        private Dictionary<string, Item> itemSet = new Dictionary<string, Item>();

        private Bitmap flagSprite;
        private Dictionary<string, Rectangle> flags = new Dictionary<string, Rectangle>(StringComparer.OrdinalIgnoreCase);
        private Rectangle RECT_UNKNOWN = new Rectangle(-1, -1, -1, -1);

        public event EventHandler OnCheckChange;

        private int searchIndex = -1, searchCount = 0;

        public CountryListBox()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);

            Items.Add(new Item("_A", "Todos"));
            BackColor = Color.White;

            flagSprite = Properties.Resources.flag_sprite;
            foreach (string entry in Properties.Resources.flag_map.Lines()) {
                string[] vals = entry.Split(',');
                flags[vals[0]] = new Rectangle(int.Parse(vals[1]), int.Parse(vals[2]), 16, 16);
            }
        }
        public class Item : IComparable<Item>
        {
            public bool Checked = true;
            public string CountryCode;
            public string CountryName;
            public int Count = 1;

            public Item(string cc, string cn)
            {
                CountryCode = cc;
                CountryName = cn;
            }

            public int CompareTo(Item other)
            {
                if (other == null)
                    return 0;
                return CountryName.CompareTo(other.CountryName);
            }
        }

        public const string CODE_UNKNOWN = "_?";
        private bool hasUnknown = false;
        public void Add(string cc, string name)
        {
            if (!itemSet.TryGetValue(cc, out Item item)) {
                item = new Item(cc, name);
                item.Checked = Program.Config.GetCompound("CountryListBox").GetBoolOrTrue(cc);
                itemSet.Add(cc, item);

                if (cc == CODE_UNKNOWN) {
                    Items.Insert(1, item);
                    hasUnknown = true;
                } else {
                    Items.Add(item);
                }
                int n = hasUnknown ? 2 : 1;
                Items.Sort(n, Items.Count - n, null);
            } else {
                item.Count++;
            }
        }
        public void Clear()
        {
            hasUnknown = false;
            itemSet.Clear();
            Items.Clear();
            Items.Add(new Item("_A", "Todos"));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            AutoScrollMinSize = new Size(0, ((Items.Count + 1) / 2 * 13) + 3);

            Graphics g = e.Graphics;

            int w = ClientRectangle.Width;
            int h = ClientRectangle.Height;

            g.TranslateTransform(0, AutoScrollPosition.Y);
            StringFormat sf = new StringFormat();
            sf.Trimming = StringTrimming.EllipsisCharacter;

            for (int i = Math.Max(0, -(AutoScrollPosition.Y / 13) - 1), ul = Math.Min(Items.Count, (i + (h / 13) + 3) * 2); i < ul; i++) {
                Rectangle realRect = new Rectangle(1 + (i % 2 * (w / 2)), (i / 2 * 13) + 1, w / 2 - 1, 13);
                Rectangle itemRect = realRect;

                Item it = Items[i];

                if (Application.RenderWithVisualStyles) {
                    CheckBoxState state = it.Checked ? CheckBoxState.CheckedNormal : CheckBoxState.UncheckedNormal;
                    if (it.CountryCode == "_A") {
                        int f = 0; //bit 1: not checked, 2: checked
                        it.Count = 0;
                        for (int j = 1; j < Items.Count; j++) {
                            var item = Items[j];
                            f |= item.Checked ? 0x2 : 0x1;
                            it.Count += item.Checked ? item.Count : 0;
                        }
                        state = f == 0x3 ? CheckBoxState.MixedNormal :
                                f == 0x2 ? CheckBoxState.CheckedNormal : CheckBoxState.UncheckedNormal;
                    }

                    CheckBoxRenderer.DrawCheckBox(e.Graphics, itemRect.Location, state);
                    Shrink(ref itemRect, CheckBoxRenderer.GetGlyphSize(e.Graphics, state).Width);
                } else {
                    ControlPaint.DrawCheckBox(e.Graphics, itemRect.X, itemRect.Y, 13, 13, it.Checked ? ButtonState.Checked : ButtonState.Normal);
                    Shrink(ref itemRect, 13);
                }
                if (flags.TryGetValue(it.CountryCode, out Rectangle fr)) {
                    g.DrawImage(flagSprite, itemRect.X + 2, itemRect.Y - 2, fr, GraphicsUnit.Pixel);
                    Shrink(ref itemRect, 18);
                }
                
                g.DrawString($"{it.CountryName} ({it.Count})", Font, Brushes.Black, itemRect, sf);

                if (searchIndex == i) {
                    using (Pen pen = new Pen(Color.Black)) {
                        pen.DashStyle = DashStyle.Dot;
                        g.DrawRectangle(pen, realRect);
                    }
                }
            }
            sf.Dispose();

            void Shrink(ref Rectangle rect, int aw) {
                rect.X += aw;
                rect.Width -= aw;
            }
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (searchIndex != -1) {
                searchIndex = -1;
                Invalidate();
            }
        }
        private int IndexOfItem(int x, int y)
        {
            int iy = (y - AutoScrollPosition.Y) / 13;
            int ix = x / (ClientSize.Width / 2) % 2;
            return ix + iy * 2;
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            ToggleItem(IndexOfItem(e.X, e.Y), false);
        }
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            if (e.Button == MouseButtons.Left)
                ToggleItem(IndexOfItem(e.X, e.Y), true);
        }
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            if (e.KeyChar == '\r' && searchIndex != -1) {
                ToggleItem(searchIndex, false);
            }

            if (e.KeyChar <= 0x20 || e.KeyChar >= 0x7F)
                return;
            char cLow = Char.ToLower(e.KeyChar);

            int matchCount = 0, matchFirst = 0;
            for (int i = hasUnknown ? 2 : 1; i < Items.Count; i++) {
                if (Char.ToLower(Items[i].CountryName[0]) == cLow) {
                    if (matchCount++ == 0)
                        matchFirst = i;
                }
            }
            int idx = matchCount > 0 ? matchFirst + (searchCount++ % matchCount) : -1;
            if (searchIndex != idx) {
                searchIndex = idx;
                AutoScrollPosition = new Point(0, ((idx + 1) / 2 * 13) - (Height / 2));
                Invalidate();
            }
        }
        public void ToggleItem(int index, bool toggleOthers)
        {
            if (index >= 0 && index < Items.Count) {
                Item it = Items[index];
                if (it.CountryName == "Todos") {
                    it.Checked = !it.Checked;
                    foreach (Item itt in Items)
                        itt.Checked = it.Checked;
                } else {
                    it.Checked = !it.Checked;
                    if (toggleOthers) {
                        foreach (Item itt in Items)
                            itt.Checked = it.Checked;
                    }
                }
                OnCheckChange?.Invoke(this, EventArgs.Empty);
                Invalidate();
            }
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_VSCROLL = 0x0115,
                      WM_MOUSEWHEEL = 0x020A;
            if (m.Msg == WM_VSCROLL || m.Msg == WM_MOUSEWHEEL)
                Invalidate();
            base.WndProc(ref m);
        }
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            Focus();
        }
        protected override CreateParams CreateParams {
            get {
                CreateParams cp = base.CreateParams;
                cp.Style |= 0x00800000; // WS_BORDER
                return cp;
            }
        }
    }
}
