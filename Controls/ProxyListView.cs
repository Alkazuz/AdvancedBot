using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using MaxMind.MaxMindDb;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Collections;

namespace AdvancedBot.Controls
{
    public class ProxyListView : ListView
    {
        private int countryColumn = -1;
        private Bitmap flagSprite;
        private Dictionary<string, Rectangle> flags = new Dictionary<string, Rectangle>(StringComparer.OrdinalIgnoreCase);
        private Rectangle RECT_UNKNOWN = new Rectangle(-1, -1, -1, -1);
        private MaxMindDbReader mmdb;

        public ProxyListView()
        {
            OwnerDraw = true;
            flagSprite = Properties.Resources.flag_sprite;
            foreach (string entry in Properties.Resources.flag_map.Lines()) {
                string[] vals = entry.Split(',');
                flags[vals[0]] = new Rectangle(int.Parse(vals[1]), int.Parse(vals[2]), 16, 16);
            }
            const string MMDB_FILE = @"GeoLite2-Country\GeoLite2-Country.mmdb";
            if (File.Exists(MMDB_FILE)) {
                mmdb = new MaxMindDbReader(MMDB_FILE);
            }
        }

        protected override void OnDrawColumnHeader(DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
            base.OnDrawColumnHeader(e);
        }
        protected override void OnDrawSubItem(DrawListViewSubItemEventArgs e)
        {
            if (countryColumn == -1) {
                for (int i = 0; i < Columns.Count; i++) {
                    if (Columns[i].Text.EndsWith(" ")) {
                        countryColumn = i;
                        break;
                    }
                }
            }
            if (e.ColumnIndex == countryColumn) {
                if (!(e.SubItem.Tag is Rectangle)) {
                    e.SubItem.Text = "Desconhecido";
                    e.SubItem.Tag = RECT_UNKNOWN;

                    if (mmdb != null) {
                        string ipAddr = e.Item.SubItems[0].Text;
                        ipAddr = ipAddr.Substring(0, ipAddr.IndexOf(':'));
                        JToken entry = mmdb.Find(ipAddr);
                        if (entry != null) {
                            JToken country = entry["country"] ?? entry["registered_country"];
                            if (country != null) {
                                e.SubItem.Text = country["names"]["pt-BR"].Value<string>();
                                string iso = country["iso_code"].Value<string>();
                                Rectangle rect;
                                if (flags.TryGetValue(iso, out rect))
                                    e.SubItem.Tag = rect;
                            }
                        }
                    }
                }
                
                Rectangle bounds = e.Bounds;
                bounds.X += 2;
                bounds.Width -= 2;

                Rectangle fRect = (Rectangle)e.SubItem.Tag;
                if (fRect.Width > 0) {
                    e.Graphics.DrawImage(flagSprite, new Rectangle(bounds.X, bounds.Y, 16, 16), fRect, GraphicsUnit.Pixel);
                    bounds.X += 16;
                    bounds.Width -= 16;
                }
                TextFormatFlags fmtFlags = TextFormatFlags.Left | 
                                           TextFormatFlags.VerticalCenter | 
                                           TextFormatFlags.WordEllipsis;
                TextRenderer.DrawText(e.Graphics, e.SubItem.Text, Font, bounds, e.SubItem.ForeColor, fmtFlags);
            } else {
                e.DrawDefault = true;
            }
            base.OnDrawSubItem(e);
        }

        protected override void OnColumnClick(ColumnClickEventArgs e)
        {
            base.OnColumnClick(e);
            ColumnHeader col = Columns[e.Column];

            SortOrder order = SortOrder.Ascending;
            if (col.Tag != null && (SortOrder)col.Tag == SortOrder.Ascending) {
                order = SortOrder.Descending;
            }
            col.Tag = order;
            ListViewItemSorter = new ProxyComparer(Columns[e.Column]);
            SetSortIcon(this, e.Column, order);
        }

        private class ProxyComparer : IComparer
        {
            public ColumnHeader column;
            private SortOrder order;
            public ProxyComparer(ColumnHeader c)
            {
                column = c;
                order = (SortOrder)c.Tag;
            }
            public int Compare(object xo, object yo)
            {
                ListViewItem x = (ListViewItem)xo;
                ListViewItem y = (ListViewItem)yo;
                int r;

                if (column.Text == "Ping") {//ping
                    string px = x.SubItems[2].Text;
                    string py = y.SubItems[2].Text;
                    if (px == "---") px = "999999ms";
                    if (py == "---") py = "999999ms";
                    r = int.Parse(px.Substring(0,px.Length-2)).CompareTo(
                        int.Parse(py.Substring(0,py.Length-2)));
                } else {
                    r = x.SubItems[column.Index].Text.CompareTo(
                        y.SubItems[column.Index].Text);
                }
                return order == SortOrder.Descending ? -r : r;
            }
        }

        //https://stackoverflow.com/a/254139
        [StructLayout(LayoutKind.Sequential)]
        private struct HDITEM
        {
            public Mask mask;
            public int cxy;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pszText;
            public IntPtr hbm;
            public int cchTextMax;
            public Format fmt;
            public IntPtr lParam;
            // _WIN32_IE >= 0x0300 
            public int iImage;
            public int iOrder;
            // _WIN32_IE >= 0x0500
            public uint type;
            public IntPtr pvFilter;
            // _WIN32_WINNT >= 0x0600
            public uint state;

            [Flags]
            public enum Mask
            {
                Format = 0x4,       // HDI_FORMAT
            }

            [Flags]
            public enum Format
            {
                SortDown = 0x200,   // HDF_SORTDOWN
                SortUp = 0x400,     // HDF_SORTUP
            }
        }

        private const int LVM_FIRST = 0x1000;
        private const int LVM_GETHEADER = LVM_FIRST + 31;

        private const int HDM_FIRST = 0x1200;
        private const int HDM_GETITEM = HDM_FIRST + 11;
        private const int HDM_SETITEM = HDM_FIRST + 12;

        [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, ref HDITEM lParam);

        private static void SetSortIcon(ListView lv, int columnIndex, SortOrder order)
        {
            IntPtr columnHeader = SendMessage(lv.Handle, LVM_GETHEADER, IntPtr.Zero, IntPtr.Zero);
            for (int columnNumber = 0; columnNumber < lv.Columns.Count; columnNumber++) {
                IntPtr columnPtr = new IntPtr(columnNumber);
                HDITEM item = new HDITEM {
                    mask = HDITEM.Mask.Format
                };

                if (SendMessage(columnHeader, HDM_GETITEM, columnPtr, ref item) == IntPtr.Zero) {
                    throw new Win32Exception();
                }

                if (order != SortOrder.None && columnNumber == columnIndex) {
                    switch (order) {
                        case SortOrder.Ascending:
                            item.fmt &= ~HDITEM.Format.SortDown;
                            item.fmt |= HDITEM.Format.SortUp;
                            break;
                        case SortOrder.Descending:
                            item.fmt &= ~HDITEM.Format.SortUp;
                            item.fmt |= HDITEM.Format.SortDown;
                            break;
                    }
                } else {
                    item.fmt &= ~HDITEM.Format.SortDown & ~HDITEM.Format.SortUp;
                }

                if (SendMessage(columnHeader, HDM_SETITEM, columnPtr, ref item) == IntPtr.Zero) {
                    throw new Win32Exception();
                }
            }
        }
    }
}
