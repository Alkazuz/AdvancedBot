using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AdvancedBot.client;
using AdvancedBot.client.Packets;
using Newtonsoft.Json.Linq;

namespace AdvancedBot.Controls
{
    public class PingButton : CheckBox
    {
        const string HEAVY_BALLOT_X = "✘";
        const string HEAVY_CHECK_MARK = "✔";
        const string CLOCKWISE_CIRCLE_ARROW = "↻";
        const string EXCLAMATION_MARK = "!";
        const string QUESTION_MARK = "?";
        
        private int state = 1;
        PingResult query = null;
        Timer timer = new Timer();
        Timer tbTimer = new Timer();

        PingToolTip toolTipForm = new PingToolTip();
        private TextBox tb;

        public PingButton(TextBox tb)
        {
            this.tb = tb;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            
            int h = tb.Height - 2;
            Size = new Size(h + 1, h);
            
            SendMessage(tb.Handle, EM_SETMARGINS, (IntPtr)EC_RIGHTMARGIN, (IntPtr)(h << 16));

            UseVisualStyleBackColor = true;
            Appearance = Appearance.Button;

            Checked = Program.Config.GetBoolOrTrue("PingButtonChecked");
            
            timer.Tick += (s, e) => Invalidate();
            timer.Interval = 40;

            SetupDelayedTextHandler();

            RePing();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.TextRenderingHint = TextRenderingHint.AntiAlias;

            int w = ClientSize.Width;
            int h = ClientSize.Height;
            
            var font = Font;

            string icon = null;
            Brush iconColor = null;

            switch (state) {
                case 1: icon = CLOCKWISE_CIRCLE_ARROW; iconColor = Brushes.Gray; break;
                case 2: icon = HEAVY_CHECK_MARK; iconColor = Brushes.Green; break;
                case 3: icon = HEAVY_BALLOT_X; iconColor = Brushes.Red; break;
            }
            if(!Checked) {
                icon = QUESTION_MARK; iconColor = Brushes.Black;
            }

            var size = g.MeasureString(icon, font);
            Rectangle itemRect = new Rectangle((int)(w - size.Width) / 2, (int)(h - size.Height) / 2, w, h);
            if (icon == CLOCKWISE_CIRCLE_ARROW) {
                itemRect.X -= 2;
                var state = g.Save();
                g.TranslateTransform(itemRect.X + (size.Width / 2) + 2, itemRect.Y + (size.Height / 2));
                g.RotateTransform((float)(DateTime.UtcNow.TimeOfDay.TotalMilliseconds / 3f));
                g.DrawString(CLOCKWISE_CIRCLE_ARROW, font, iconColor, -(size.Width / 2), -(size.Height / 2) + 1);
                g.Restore(state);
            } else {
                g.DrawString(icon, font, iconColor, itemRect.X + 1, itemRect.Y + 1);
            }
        }
        protected override void OnMouseMove(MouseEventArgs mevent)
        {
            base.OnMouseMove(mevent);
            var cur = PointToScreen(mevent.Location);
            if (Checked && query != null && !toolTipForm.IsShown) {
                toolTipForm.Location = new Point(cur.X + 1, cur.Y + 1);
                toolTipForm.Query = query;
                toolTipForm.Show();
            } else if(toolTipForm.IsShown) {
                toolTipForm.Location = new Point(cur.X + 1, cur.Y + 1);
            }
        }
        protected override void OnMouseLeave(EventArgs eventargs)
        {
            base.OnMouseLeave(eventargs);
            if (toolTipForm.IsShown) {
                toolTipForm.Hide();
            }
        }
        protected override void OnCheckedChanged(EventArgs e)
        {
            base.OnCheckedChanged(e);
            
            if (Checked) {
                RePing();
            } else if (toolTipForm.IsShown) {
                toolTipForm.Hide();
                query?.Dispose();
                query = null;
            }
            Program.Config.AddBoolean("PingButtonChecked", Checked);
        }

        private async void RePing()
        {
            if (!Checked) return;

            try {
                string ip;
                ushort port = 25565;
                state = 1;
                timer.Start();

                query = null;
                if (tb.Text.ParseIP(out ip, ref port)) {
                    query = await PingAsync(ip, port);
                }
            } catch {
            }
            state = query == null ? 3 : 2;
            timer.Stop();
            Invalidate();
        }

        private void SetupDelayedTextHandler()
        {
            long lastChanged = Utils.GetTimestamp();
            bool handled = true;

            void OnTextChanged(object sender, EventArgs e) {
                lastChanged = Utils.GetTimestamp();
                handled = false;
            }

            tb.TextChanged += OnTextChanged;
            tbTimer.Interval = 50;
            tbTimer.Tick += (s, e) => {
                if(!handled && Utils.GetTimestamp() - lastChanged > 500) {
                    RePing();
                    handled = true;
                }
            };
            tbTimer.Disposed += (s, e) => tb.TextChanged -= OnTextChanged;
            tbTimer.Start();
        }

        private async Task<PingResult> PingAsync(string ip, ushort port)
        {
            Debug.WriteLine("PingAsync");
            await Task.Run(() => SrvResolver.ResolveIP(ref ip, ref port));

            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await Task.Factory.FromAsync(sock.BeginConnect, sock.EndConnect, ip, (int)port, null).ConfigureAwait(false);
            sock.ReceiveTimeout = 5000;
            sock.SendTimeout = 5000;

            using (var ps = new PacketStream(sock)) {
                WriteBuffer wb = new WriteBuffer();
                new PacketHandshake(315, ip, port, 1).WritePacket(wb, null); /**/ ps.SendPacket(wb);
                wb.Reset(); wb.WriteVarInt(0x00); /**/ ps.SendPacket(wb);

                var pkt = await ReadPacketAsync(ps);

                var resp = new PingResult(JObject.Parse(pkt.ReadString()));

                long sent = Stopwatch.GetTimestamp();
                wb.Reset(); wb.WriteVarInt(0x01); wb.WriteLong(sent); /**/ ps.SendPacket(wb);

                pkt = await ReadPacketAsync(ps);
                if(pkt.ReadLong() != sent) {
                    //throw an exception?
                    Debug.WriteLine("Pong packet timestamp is invalid");
                }
                resp.Ping = (int)((Stopwatch.GetTimestamp() - sent) / (Stopwatch.Frequency / 1000));
                return resp;
            }
        }
        private async Task<ReadBuffer> ReadPacketAsync(PacketStream ps)
        {
            TaskCompletionSource<ReadBuffer> tcs = new TaskCompletionSource<ReadBuffer>();
            void SetResult(ReadBuffer rb) {
                tcs.SetResult(rb);
            }
            ps.OnPacketAvailable += SetResult;
            try {
                return await tcs.Task.SetTimeout(3000);
            } finally {
                ps.OnPacketAvailable -= SetResult;
            }
        }
        private class PingResult : IDisposable
        {
            public Bitmap Icon;

            public string Version;
            public int MaxPlayers;
            public int OnlinePlayers;
            public string Description;

            public int Ping;

            public PingResult() { }
            public PingResult(JObject j)
            {
                JToken tmp = j["version"];

                Version = $"{tmp["name"].AsStr()} ({tmp["protocol"].AsInt()})";

                tmp = j["players"];

                MaxPlayers = tmp["max"].AsInt();
                OnlinePlayers = tmp["online"].AsInt();
                
                Description = ChatParser.ParseChat(j["description"]);

                if((tmp = j["favicon"]) != null) {
                    //data:image/png;base64,
                    using (MemoryStream mem = new MemoryStream(Convert.FromBase64String(tmp.AsStr().Substring(22)))) {
                        Icon = new Bitmap(mem);
                    }
                }
            }

            public void Dispose()
            {
                Icon?.Dispose();
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            toolTipForm.Dispose();
            query?.Dispose();
            timer.Dispose();
            tbTimer.Dispose();
        }

        private const int EM_SETMARGINS = 0x00D3;
        private const int EC_RIGHTMARGIN = 2;
        [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        private class PingToolTip : Form
        {
            public PingResult Query;
            public bool IsShown { get; private set; }
            
            public PingToolTip()
            {
                SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
                FormBorderStyle = FormBorderStyle.None;
                ShowInTaskbar = false;
                ClientSize = new Size(400, 71);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                int w = ClientSize.Width;
                int h = ClientSize.Height;
                g.Clear(Color.FromArgb(0xFF << 24 | 0xFFFFF0));
                g.DrawRectangle(Pens.Black, 0, 0, w - 1, h - 1);

                if (Query != null) {
                    g.FillRectangle(Brushes.White, 3, 3, 64, 64);
                    g.DrawRectangle(Pens.Gray, 3, 3, 64, 64);
                    if (Query.Icon != null) {
                        g.DrawImage(Query.Icon, new Rectangle(3, 3, 64, 64));
                    }

                    int p = Query.Ping;
                    int bars = p < 150 ? 4 :
                               p < 300 ? 3 :
                               p < 600 ? 2 :
                               p < 1000 ? 1 : 0;

                    for (int i = 0; i < 5; i++) {
                        int bx = i * 3;
                        int bh = i * 2;
                        g.FillRectangle(i <= bars ? Brushes.Green : Brushes.LightGray, bx + 71, 11 - bh, 2, bh);
                    }
                    using (Font font = new Font("Courier New", 8f)) {
                        g.DrawString(p + "ms", font, Brushes.Black, 85, 0);
                        int x1 = (int)g.MeasureString(p + "ms", font).Width + 85;

                        string players = $"{Query.OnlinePlayers}/{Query.MaxPlayers}";

                        int x2 = w - (int)g.MeasureString(players, font).Width - 3;
                        g.DrawString(players, font, Brushes.Black, x2, 0);

                        using (StringFormat sf = new StringFormat()) {
                            sf.Trimming = StringTrimming.EllipsisCharacter;
                            var cr = new Rectangle(0, 0, x2 - x1, 16);
                            int sw = (int)g.MeasureString(Query.Version, font, cr.Width).Width;
                            cr.X = x1 + (cr.Width - sw) / 2;
                            g.DrawString(Query.Version, font, Brushes.Black, cr, sf);
                        }
                    }

                    int y = 15;
                    foreach (var line in Query.Description.Lines()) {
                        DrawLine(g, line, 70, y);
                        y += 10;
                    }
                }
            }

            public new void Show()
            {
                //https://stackoverflow.com/a/23008105 
                ShowWindow(Handle, SW_SHOWNOACTIVATE);
                SetWindowPos(Handle, (IntPtr)(-1), Left, Top, Width, Height, SWP_NOACTIVATE);
                IsShown = true;
            }
            public new void Hide()
            {
                base.Hide();
                IsShown = false;
            }

            private const int SW_SHOWNOACTIVATE = 4;
            private const int SWP_NOACTIVATE = 0x0010;
            [DllImport("user32.dll")] private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);
            [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

            private void DrawLine(Graphics g, string s, int x, int y)
            {
                int n = 0;
                using (FormatStyle style = new FormatStyle()) {
                    style.Padding = (int)(g.MeasureString("a", style.TextFont).Width +
                                          g.MeasureString("b", style.TextFont).Width -
                                          g.MeasureString("ab", style.TextFont).Width);

                    for (int idx; (idx = s.IndexOf('§', n)) != -1;) {
                        DrawPart(g, style, s, n, idx, ref x, y);
                        n = idx + 2;
                    }
                    DrawPart(g, style, s, n, s.Length, ref x, y);
                }
            }
            private void DrawPart(Graphics g, FormatStyle style, string s, int start, int end, ref int x, int y)
            {
                int len = end - start;
                if (len > 0) {
                    string part = s.Substring(start, end - start);

                    if (start >= 2) {
                        style.Update(s[start - 1]);
                    }
                    SolidBrush br = style.FillBrush;
                    br.Color = style.DarkColor;

                    g.DrawString(part, style.TextFont, br, x + 1, y + 1);
                    br.Color = style.Color;
                    g.DrawString(part, style.TextFont, br, x, y);
                    x += (int)g.MeasureString(part, style.TextFont, 65536, style.Format).Width - style.Padding;
                } else if (start >= 2) {
                    style.Update(s[start - 1]);
                }
            }
            private class FormatStyle : IDisposable
            {
                private static Color[] Colors = new Color[32];
                public FontStyle Style = FontStyle.Regular;
                public StringFormat Format = new StringFormat() {
                    FormatFlags = StringFormatFlags.MeasureTrailingSpaces
                };
                public SolidBrush FillBrush = new SolidBrush(Colors[0xF]);
                public Font TextFont = new Font("Courier New", 8f, FontStyle.Regular);

                private int colorIndex = 0xF;
                public Color Color { get { return Colors[colorIndex]; } }
                public Color DarkColor { get { return Colors[colorIndex + 0x10]; } }
                public int Padding;

                public void Update(char c)
                {
                    if (c >= '0' && c <= '9') {
                        colorIndex = c - '0';
                    } else if (c >= 'a' && c <= 'f') {
                        colorIndex = c - 'a' + 10;
                    } else {
                        switch (c) {
                            case 'l': Style |= FontStyle.Bold; break;
                            case 'm': Style |= FontStyle.Strikeout; break;
                            case 'n': Style |= FontStyle.Underline; break;
                            case 'o': Style |= FontStyle.Italic; break;
                            case 'r': colorIndex = 0xF; Style = FontStyle.Regular; break;
                        }
                    }
                }
                public void Dispose()
                {
                    FillBrush.Dispose();
                    TextFont.Dispose();
                    Format.Dispose();
                }
                static FormatStyle()
                {
                    for (int i = 0; i < 32; i++) {
                        int br = (i >> 3 & 0x01) * 0x55;
                        int r = (i >> 2 & 0x01) * 0xAA + br;
                        int g = (i >> 1 & 0x01) * 0xAA + br;
                        int b = (i & 0x01) * 0xAA + br;

                        if (i == 6)
                            r += 0x55;
                        if (i > 0xF) {
                            r /= 4;
                            g /= 4;
                            b /= 4;
                        }
                        Colors[i] = Color.FromArgb(r, g, b);
                    }
                }
            }
        }
    }
}
