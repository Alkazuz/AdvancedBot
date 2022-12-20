using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AdvancedBot.client;
using AdvancedBot.client.NBT;
using AdvancedBot.Controls;
using AdvancedBot.ProxyChecker;
using AdvancedBot.Protetor;
using AdvancedBot.Viewer;
using Newtonsoft.Json.Linq;
using System.Net;
using AdvancedBot.ClientSocket;
using Microsoft.Win32.SafeHandles;
using AdvancedBot.Json;
using AdvancedBot.Plugins;

namespace AdvancedBot
{
#pragma warning disable IDE1006 // Naming Styles
    public partial class Main : Form
    {
        public Main()
        {

            InitializeComponent();
            Translation.setup(this);
        }

        public List<MinecraftClient> Clients = new List<MinecraftClient>();

        private List<string> prevSentMsgs = new List<string>();
        int prevIdx = 0;

        private Color[] FormattingColors = new Color[16];

        public bool FrmClosed = false;
        private AboutNew aboutForm;
        public Start frmStart;

        public ProxyList Proxies = new ProxyList();

        public static string GetUsername()
        {
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE Name = 'javaw.exe'")) {
                foreach (ManagementBaseObject obj in searcher.Get()) {
                    string cmd = (string)obj["CommandLine"];
                    obj.Dispose();

                    string username = Regex.Match(cmd, "--username ([^ ]+)").Groups[1].Value;
                    if (username.Length > 0) {
                        return username;
                    }
                }
            }
            return "desconhecido";
        }

        protected override void OnClosing(CancelEventArgs e)
        {

            foreach (IPlugin plugin in Program.pluginManager.plugins.Values)
            {
                plugin.Unload();
            }

            FrmClosed = true;
            Program.SaveConf();

            try {
                for (int i = 0; i < Clients.Count; i++) {
                    string us = Clients[i].Username;
                    Clients[i].Dispose();
                    Clients.RemoveAt(i--);
                    lbUsers.Refresh();
                }
            } catch { }

            Clients.Clear();
            GC.Collect();

            try {
                foreach (Form f in Application.OpenForms.Cast<Form>().ToArray()) {
                    if (!(f is Main || f is Start)) {
                        f.Close();
                    } else {
                        f.Hide();
                    }
                }
            } catch { }
            
            base.OnClosing(e);
            Environment.Exit(0);
        }
        public static void CreateConsole()
        {
            AllocConsole();

            IntPtr stdHandle = GetStdHandle(STD_OUTPUT_HANDLE);
            SafeFileHandle safeFileHandle = new SafeFileHandle(stdHandle, true);
            FileStream fileStream = new FileStream(safeFileHandle, FileAccess.Write);
            Encoding encoding = System.Text.Encoding.GetEncoding(MY_CODE_PAGE);
            StreamWriter standardOutput = new StreamWriter(fileStream, encoding);
            standardOutput.AutoFlush = true;
            Console.SetOut(standardOutput);
        }

        [DllImport("kernel32.dll",
            EntryPoint = "GetStdHandle",
            SetLastError = true,
            CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr GetStdHandle(int nStdHandle);
        [DllImport("kernel32.dll",
            EntryPoint = "AllocConsole",
            SetLastError = true,
            CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        private static extern int AllocConsole();
        private const int STD_OUTPUT_HANDLE = -11;
        private const int MY_CODE_PAGE = 437;

        public void DebugConsole(String text)
        {
            Console.WriteLine(text);
        }

        private void Main_Load(object sender, EventArgs e)
        {
           
            //webBrowser1.ScriptErrorsSuppressed = true;
            //VerifyVersion(webBrowser1);
            AllocConsole();
            IntPtr stdHandle = GetStdHandle(STD_OUTPUT_HANDLE);
            SafeFileHandle safeFileHandle = new SafeFileHandle(stdHandle, true);
            FileStream fileStream = new FileStream(safeFileHandle, FileAccess.Write);
            Encoding encoding = System.Text.Encoding.GetEncoding(MY_CODE_PAGE);
            StreamWriter standardOutput = new StreamWriter(fileStream, encoding);
            standardOutput.AutoFlush = true;
            Console.SetOut(standardOutput);
            
            try
            {
                lbUsers.Items = Clients;
                lbUsers.OnSelectedItemChanged += lbUsers_SelectedItemChanged;

                Control.CheckForIllegalCrossThreadCalls = false;
                this.Text = "AdvancedBot " + Program.AppVersion;

                for (int i = 0; i < 16; i++) {
                    int br = (i >> 3 & 0x01) * 0x55;
                    int r = (i >> 2 & 0x01) * 0xAA + br;
                    int g = (i >> 1 & 0x01) * 0xAA + br;
                    int b = (i & 0x01) * 0xAA + br;

                    if (i == 6)
                        r += 0x55;
                    FormattingColors[i] = Color.FromArgb(r, g, b);
                }

                if (Program.Config.Contains("ChatBackColor"))
                {
                    rtbChat.BackColor = Color.FromArgb(Program.Config.GetInt("ChatBackColor"));
                    tabPage2.BackColor = Color.FromArgb(Program.Config.GetInt("ChatBackColor"));
                }
                    
                CompoundTag fontTag;
                if (!(fontTag = Program.Config.GetCompound("ChatFont")).IsEmpty()) {
                    rtbChat.Font = new System.Drawing.Font(fontTag.GetString("Name"), 
                                                           fontTag.GetFloat("SizeInPoints"));
                    RtfBuilder.Instance.FontSize = fontTag.GetFloat("SizeInPoints");
                }

                tsKnockback.Checked = MinecraftClient.Knockback;
                tsAutoReco.Checked = MinecraftClient.AutoReconnect;

                lbUsers.MouseUp += (s, ev) => {
                    var sel = lbUsers.Selected;
                    if (sel != null) {
                        tsView.Enabled = sel is MinecraftClient cli && (cli.IsBeingTicked() && cli.MapAndPhysics);
                        tsRemove.Enabled = true;
                       
                    } else {
                        tsView.Enabled = false;
                        tsRemove.Enabled = lbUsers.SelectedItems.Count > 0;
                    }
                    selecionadosToolStripMenuItem.Enabled = tsView.Enabled;
                };

                tbChatInput.PreviewKeyDown += (s, ev) => {
                    if (ev.KeyCode == Keys.Tab)
                        ev.IsInputKey = true;
                };
                tbChatInput.TextChanged += (s, ev) => {
                    if (!isTabChanged) {
                        suggestIdx = 0;
                        tabSuggestions.Clear();
                    }
                    isTabChanged = false;
                };

                foreach(int n in new [] { 50, 100, 150, 250, 500 }) {
                    tsmiHistorySize.DropDownItems.Add(new ToolStripMenuItem(n.ToString()) {
                        Checked = MinecraftClient.MaximumChatLines == n
                    });
                }
                tsmiHistorySize.DropDownItemClicked += (s, ev) => {
                    ToolStripMenuItem item = (ToolStripMenuItem)ev.ClickedItem;
                    MinecraftClient.MaximumChatLines = int.Parse(item.Text);
                    foreach (ToolStripMenuItem i in tsmiHistorySize.DropDownItems) {
                        i.Checked = false;
                    }
                    item.Checked = true;
                };
            } catch (Exception ex) {
                MessageBox.Show("Ocorreu um erro durante a inicialização de alguns componentes. O funcionamento do programa pode estar comprometido.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Program.CreateErrLog(ex, "errinit");
            }
            WebConnection.connect();
        }
        bool isFirstPaint = true;
        protected override unsafe void OnPaint(PaintEventArgs e)
        {
            if (isFirstPaint) {
                isFirstPaint = false;
                frmStart = new Start();
                frmStart.Show();
                frmStart.Icon = Icon;

                groupBox1.Enabled = false;
                groupBox2.Enabled = false;
            }
            base.OnPaint(e);
        }

        public MinecraftClient GetClient(string name)
        {
            for (int i = 0; i < Clients.Count; i++) {
                MinecraftClient cli = Clients[i];
                if ((cli.Username != null && cli.Username.EqualsIgnoreCase(name)) || (cli.Email != null && cli.Email.EqualsIgnoreCase(name))) {
                    return cli;
                }
            }
            return null;
        }

        private void tbChatInput_KeyPress(object sender, KeyPressEventArgs e)
        {
            var sel = lbUsers.Selected;
            var selItems = lbUsers.SelectedItems;
            if (e.KeyChar == 0x0D && (sel != null || selItems.Count > 1) && !string.IsNullOrWhiteSpace(tbChatInput.Text)) //Enter
            {
                if (prevSentMsgs.Count == 0 || prevSentMsgs.Last() != tbChatInput.Text)
                    prevSentMsgs.Add(tbChatInput.Text);

                if (prevSentMsgs.Count > 64)
                    prevSentMsgs.RemoveRange(0, prevSentMsgs.Count - 64);
                prevIdx = prevSentMsgs.Count;

                if(selItems.Count > 1) {
                    SendMessageFor(selItems);
                } else if (sel is UserListBox.ManyItems) {
                    SendMessageForAllBots();
                } else {
                    SendMessageFor(sel as MinecraftClient);
                }
            }
        }
        private void tbChatInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down) {
                prevIdx += (e.KeyCode == Keys.Down ? 1 : -1);
                prevIdx = prevIdx < 0 ? 0 : prevIdx >= prevSentMsgs.Count ? prevSentMsgs.Count - 1 : prevIdx;

                if (prevIdx >= 0 && prevIdx < prevSentMsgs.Count) {
                    tbChatInput.Text = prevSentMsgs[prevIdx];
                    tbChatInput.SelectionStart = tbChatInput.Text.Length;
                }

                e.Handled = true;
            } else if (e.KeyCode == Keys.Escape) {
                prevIdx = prevSentMsgs.Count;
                tbChatInput.Text = "";
                e.Handled = true;
            }
        }

        int suggestIdx = 0;
        string txtOrg = null;
        bool isTabChanged = false;
        List<string> tabSuggestions = new List<string>();
        private void tbChatInput_KeyUp(object sender, KeyEventArgs ev)
        {
            var sel = lbUsers.Selected;
            if (ev.KeyCode == Keys.Tab && sel != null) {
                MinecraftClient c = sel as MinecraftClient;
                if (c == null) {
                    foreach (MinecraftClient cli in Clients) {
                        if (cli.IsBeingTicked()) {
                            c = cli;
                            break;
                        }
                    }
                }

                if (c != null) {
                    if (suggestIdx == 0) {
                        txtOrg = tbChatInput.Text;

                        string initFilter = txtOrg.Substring(txtOrg.LastIndexOf(' ') + 1);

                        txtOrg = txtOrg.Substring(0, txtOrg.Length - initFilter.Length);

                        tabSuggestions.Clear();

                        if (txtOrg.Equals("/login ") || txtOrg.StartsWith("/register ")) {
                            tabSuggestions.Add(c.Password);
                        } else if (initFilter.StartsWith("$")) {
                            foreach (string cmd in c.CmdManager.Commands.Select(a => a.Aliases[0])) {
                                if (cmd.StartsWith(initFilter.Substring(1)))
                                    tabSuggestions.Add("$" + cmd);
                            }
                        } else {
                            foreach (string nick in c.PlayerManager.UUID2Nick.Values) {
                                if (initFilter.Length == 0 || nick.StartsWith(initFilter))
                                    tabSuggestions.Add(nick);
                            }
                        }
                    }
                    //Debug.WriteLine(string.Join(", ", tabSuggestions));
                    if (tabSuggestions.Count > 0) {
                        isTabChanged = true;
                        tbChatInput.Text = txtOrg + tabSuggestions[suggestIdx++ % tabSuggestions.Count];
                        tbChatInput.SelectionStart = tbChatInput.Text.Length;
                    }
                }
            }
        }

        private void btnSendInput_Click(object sender, EventArgs e)
        {
            var sel = lbUsers.Selected;
            var selItems = lbUsers.SelectedItems;
            if (!string.IsNullOrWhiteSpace(tbChatInput.Text) && (sel != null || selItems.Count > 1)) {
                if (selItems.Count > 1) {
                    SendMessageFor(selItems);
                } else if (sel is UserListBox.ManyItems) {
                    SendMessageForAllBots();
                } else {
                    SendMessageFor(sel as MinecraftClient);
                }
            }
        }

        private void SendMessageForAllBots()
        {
            var text = tbChatInput.Text;
            for (int i = 0; i < Clients.Count; i++) {
                Clients[i].SendMessage(text);
            }

            tbChatInput.Text = "";
        }
        private void SendMessageFor(IEnumerable<MinecraftClient> clients)
        {
            string text = tbChatInput.Text;

            MinecraftClient first = null;
            int count = 0;
            foreach(var client in clients) {
                client.SendMessage(text);

                if (first == null) {
                    first = client;
                }
                ++count;
            }
            tbChatInput.Text = "";
        }
        private void SendMessageFor(MinecraftClient c)
        {
            if (c != null) {
                c.SendMessage(tbChatInput.Text);
            }
            tbChatInput.Text = "";
        }
        private void FormatChatToRTF2(string chat, RtfBuilder sb)
        {
            Color color = Color.White;
            FontStyle style = FontStyle.Regular;

            sb.SetFontStyle(style);
            sb.SetTextColor(color, true);
            for (int i = 0, l = chat.Length; i < l; i++)
            {
                if (chat[i] == '§' && i + 1 < l)
                {
                    char fmtCode = chat[++i];
                    int col = "0123456789abcdef".IndexOf(fmtCode);
                    if (col != -1)
                    {
                        color = FormattingColors[col];
                        sb.SetTextColor(color);
                    }
                }
                else
                {
                    sb.Append(chat[i]);
                }
            }
        }
        private void FormatChatToRTF(string chat, RtfBuilder sb)
        {
            Color color = Color.White;
            FontStyle style = FontStyle.Regular;

            sb.SetFontStyle(style);
            sb.SetTextColor(color, true);
            for (int i = 0, l = chat.Length; i < l; i++) {
                if (chat[i] == '§' && i + 1 < l) {
                    char fmtCode = chat[++i];
                    int col = "0123456789abcdef".IndexOf(fmtCode);
                    if (col != -1) {
                        color = FormattingColors[col];
                        sb.SetTextColor(color);
                    } else {
                        switch (fmtCode) {
                            case 'l': style |= FontStyle.Bold; break;
                            case 'm': style |= FontStyle.Strikeout; break;
                            case 'n': style |= FontStyle.Underline; break;
                            case 'o': style |= FontStyle.Italic; break;
                            case 'r': color = Color.White; style = FontStyle.Regular; sb.SetTextColor(color); break;
                        }
                        sb.SetTextColor(color);
                        sb.SetFontStyle(style);
                    }
                } else {
                    sb.Append(chat[i]);
                }
            }
        }

        private void lbUsers_SelectedItemChanged(object sender, EventArgs e)
        {
            if (lbUsers.Selected is MinecraftClient cli) {
                SetChat(cli, true);
                SendMessage(rtbChat.Handle, WM_VSCROLL, (IntPtr)SB_PAGEBOTTOM, IntPtr.Zero);
            }
        }
        private RichTextBox createMessage(String message)
        {
            RichTextBox LB = new RichTextBox();
            LB.Cursor = Cursors.Hand;
            LB.ReadOnly = true;
            LB.BackColor = tabPage2.BackColor;
            LB.BorderStyle = BorderStyle.None;
            LB.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            return LB;
        }
        private void SetChat(MinecraftClient cli, bool force)
        {
            if (cli == null) {
                rtbChat.Text = "";
                return;
            }


            try {
                if (force || cli.chatJsonUpdate)
                {
                    cli.chatJsonUpdate = false;
                    //Debug.WriteLine("Quantidade de controls " + tabPage2.Controls.Count);
                    IntPtr hwnd2 = tabPage2.Handle;

                    ScrollInfo sf2 = new ScrollInfo();
                    sf2.cbSize = Marshal.SizeOf(sf2);
                    sf2.fMask = SIF_PAGE | SIF_POS | SIF_RANGE | SIF_TRACKPOS;
                    GetScrollInfo(hwnd2, SB_VERT, ref sf2);

                    SendMessage(hwnd2, WM_SETREDRAW, (IntPtr)0, IntPtr.Zero);

                    if ((sf2.nMax - sf2.nPage) - sf2.nTrackPos <= 5)
                    {
                        SendMessage(hwnd2, WM_VSCROLL, (IntPtr)SB_PAGEBOTTOM, IntPtr.Zero);
                    }
                    else
                    {
                        sf2.fMask = SIF_POS;
                        SetScrollInfo(hwnd2, SB_VERT, ref sf2, true);

                        SendMessage(hwnd2, WM_VSCROLL, (IntPtr)(SB_THUMBPOSITION | (sf2.nPos << 16)), IntPtr.Zero);
                    }
                    SendMessage(hwnd2, WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);
                    tabPage2.Invalidate();
                    
                }

                if (!force && !cli.ChatChanged) return;

                
                RtfBuilder sb = RtfBuilder.Instance;
                lock (cli.ChatMessages) {
                    foreach (string msg in cli.ChatMessages) {
                        if (!string.IsNullOrWhiteSpace(msg)) {
                            FormatChatToRTF(msg, sb);
                            sb.Append('\n');
                        }
                    }
                }
                IntPtr hwnd = rtbChat.Handle;

                ScrollInfo sf = new ScrollInfo();
                sf.cbSize = Marshal.SizeOf(sf);
                sf.fMask = SIF_PAGE | SIF_POS | SIF_RANGE | SIF_TRACKPOS;
                GetScrollInfo(hwnd, SB_VERT, ref sf);

                SendMessage(hwnd, WM_SETREDRAW, (IntPtr)0, IntPtr.Zero);
                rtbChat.Rtf = sb.ToRTF();
                
                if ((sf.nMax - sf.nPage) - sf.nTrackPos <= 5) {
                    SendMessage(hwnd, WM_VSCROLL, (IntPtr)SB_PAGEBOTTOM, IntPtr.Zero);
                } else {
                    sf.fMask = SIF_POS;
                    SetScrollInfo(hwnd, SB_VERT, ref sf, true);

                    SendMessage(hwnd, WM_VSCROLL, (IntPtr)(SB_THUMBPOSITION | (sf.nPos << 16)), IntPtr.Zero);
                }
                SendMessage(hwnd, WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);
                rtbChat.Invalidate();

                cli.ChatChanged = false;
            } catch { }
        }
        //http://stackoverflow.com/questions/8535102/inconsistent-results-with-richtextbox-scrolltocaret
        [DllImport("user32.dll")] private static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")] private static extern bool GetScrollInfo(IntPtr hwnd, int fnBar, ref ScrollInfo lpsi);
        [DllImport("user32.dll")] private static extern int SetScrollInfo(IntPtr hwnd, int fnBar, ref ScrollInfo lpsi, bool fRedraw);
        private const int SIF_RANGE        = 0x0001;
        private const int SIF_PAGE         = 0x0002;
        private const int SIF_POS          = 0x0004;
        private const int SIF_TRACKPOS     = 0x0010;
        private const int WM_VSCROLL       = 0x115;
        private const int WM_SETREDRAW     = 0x0B;
        private const int SB_VERT          = 0x01;
        private const int SB_THUMBPOSITION = 0x04;
        private const int SB_PAGEBOTTOM    = 0x07;
        private struct ScrollInfo
        {
            public int cbSize;
            public int fMask;
            public int nMin;
            public int nMax;
            public int nPage;
            public int nPos;
            public int nTrackPos;
        }


        private void removerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sel = lbUsers.Selected;
            if (sel is UserListBox.ManyItems) {
                for (int i = 0; i < Clients.Count; i++) {
                    Clients[i].Dispose();
                }
                Clients.Clear();
                lbUsers.Items.Clear();
            } else if(sel is MinecraftClient cli) {
                int i = Clients.IndexOf(cli);
                if (i >= 0) {
                    Clients[i].Dispose();
                    Clients.RemoveAt(i);
                }
            } else if(lbUsers.SelectedItems.Count != 0) {
                foreach(var selCli in lbUsers.SelectedItems) {
                    selCli.Dispose();
                    Clients.Remove(selCli);
                }
            }
        }
        private void tsKnockback_Click(object sender, EventArgs e)
        {
            MinecraftClient.Knockback = tsKnockback.Checked;
        }

        private Spammer spammer = null;
        private void tsSpammer_Click(object sender, EventArgs e)
        {
            if (spammer != null) return;
            spammer = new Spammer();
            spammer.FormClosing += (s, ev) => spammer = null;
            spammer.Show();
        }

        private void chatUpdater_Tick(object sender, EventArgs e)
        {
            var sel = lbUsers.Selected;
            if (sel is MinecraftClient cli) {
                SetChat(cli, false);
            } else if(sel is UserListBox.ManyItems || lbUsers.SelectedItems.Count > 1) {
                RtfBuilder sb = RtfBuilder.Instance;
                sb.SetTextColor(Color.White, true);
                sb.SetFontStyle(FontStyle.Regular);

                int conn = Clients.Count(a => a.IsBeingTicked());
                foreach (char ch in $"Selecione um bot para ver o chat.\nBots conectados: {conn} de {Clients.Count}") {
                    sb.Append(ch);
                }
                rtbChat.Rtf = sb.ToRTF();
            }
            lbUsers.Invalidate();
        }

        private Statistics dbg = null;
        private void tsDebug_Click(object sender, EventArgs e)
        {
            if (dbg == null) {
                dbg = new Statistics();
                dbg.Show();
                dbg.FormClosing += (s, ev) => dbg = null;
            }
        }

        private void tsAdd_Click(object sender, EventArgs e)
        {
            Start s = Start.Instance;
            s.rtbAccounts.Text = "";
            //s.svTb.Enabled = false;
            s.hideOnClose = true;
            s.Show();
        }

        public void SaveState(string filename)
        {
            CompoundTag tag = new CompoundTag();

            ListTag<CompoundTag> proxies = new ListTag<CompoundTag>();
            ListTag<CompoundTag> clients = new ListTag<CompoundTag>();

            foreach (ProxyInfo p in Proxies) {
                CompoundTag ptag = new CompoundTag();

                ptag.AddString("IP", p.IP);
                ptag.AddShort("Port", (short)p.Port);
                ptag.AddByte("Type", (byte)p.Type);

                proxies.AddTag(ptag);
            }

            foreach (MinecraftClient c in Clients) {
                CompoundTag ctag = new CompoundTag();

                ctag.AddString("Username", c.Username);
                ctag.AddString("Password", c.Password);
                ctag.AddString("ServerIP", c.IP);
                ctag.AddShort("ServerPort", (short)c.Port);
                ctag.AddInt("ProtocolVersion", (int)c.Version);

                if (c.ConProxy != null) {
                    Proxy p = c.ConProxy;

                    CompoundTag ptag = new CompoundTag();

                    ptag.AddString("IP", p.IP);
                    ptag.AddShort("Port", (short)p.Port);
                    ptag.AddByte("Type", (byte)p.Type);

                    ctag.Add("Proxy", ptag);
                }

                clients.AddTag(ctag);
            }

            tag.Add("Proxies", proxies);
            tag.Add("Clients", clients);

            NbtIO.Write(tag, filename);
        }
        public bool LoadState(string filename, int delay)
        {
            try {
                CompoundTag tag = NbtIO.Read(filename);

                foreach (CompoundTag ptag in tag.GetList("Proxies")) {
                    ProxyInfo p = new ProxyInfo((ProxyType)ptag.GetByte("Type"), ptag.GetString("IP"), (ushort)ptag.GetShort("Port"), -123);
                    Proxies.Add(p);
                }
                new Thread(() => {
                    foreach (CompoundTag ctag in tag.GetList("Clients")) {
                        string name = ctag.GetString("Username");
                        string pass = ctag.GetString("Password");
                        string ip = ctag.GetString("ServerIP");
                        ushort port = (ushort)ctag.GetShort("ServerPort");
                        ClientVersion ver = (ClientVersion)ctag.GetInt("ProtocolVersion");
                        Proxy p = null;

                        if (ctag.Contains("Proxy")) {
                            CompoundTag ptag = ctag.GetCompound("Proxy");
                            p = new Proxy(ptag.GetString("IP"), (ushort)ptag.GetShort("Port"), (ProxyType)ptag.GetByte("Type"));
                        }

                        MinecraftClient cl = new MinecraftClient(ip, port, name, pass, p) {
                            Version = ver,
                            SendPing = frmStart.cbDoPing.Checked
                        };

                        Clients.Add(cl);

                        cl.StartClient();
                        Thread.Sleep(delay);
                    }
                }).Start();
                return true;
            } catch (Exception) {
                return false;
            }
        }

        private void tsView_Click(object sender, EventArgs e)
        {
            var sel = lbUsers.Selected;
            if (sel is MinecraftClient client) {
                if (client.IsBeingTicked()) {
                    if (ViewForm.OpenForm != null) {
                        ViewForm.OpenForm.SetClient(client);
                    } else {
                        new ViewForm(client).Show();
                    }
                }
            }
        }

        private void tsSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog {
                Filter = "Arquivo de estado do AdvancedBot|*.state"
            };
            if (sfd.ShowDialog() == DialogResult.OK)
                SaveState(sfd.FileName);
        }

        private void tsRemDiscon_Click(object sender, EventArgs e)
        {
            List<MinecraftClient> toRemove = new List<MinecraftClient>();
            for (int i = 0; i < Clients.Count; i++)
                if (Clients[i].ConnState != ConnectionState.Connected)
                    toRemove.Add(Clients[i]);

            foreach (MinecraftClient mClient in toRemove) {
                mClient.Dispose();
                Clients.Remove(mClient);
            }
        }

        private void rtbOptCopy_Click(object sender, EventArgs e)
        {
            rtbChat.Copy();
        }

        private void rtbOptChangeBkColor_Click(object sender, EventArgs e)
        {
            using (ColorDialog cd = new ColorDialog()) {
                cd.FullOpen = true;
                cd.AnyColor = true;
                cd.CustomColors = new int[] { ColorTranslator.ToOle(Color.Gainsboro) };
                cd.Color = rtbChat.BackColor;
                if (cd.ShowDialog() == DialogResult.OK) {
                    Color c = cd.Color;

                    rtbChat.BackColor = c;
                    tabPage2.BackColor = c;
                    Program.Config.AddInt("ChatBackColor", c.ToArgb());
                }
            }
        }
        private void rtbOptChatChangeFont_Click(object sender, EventArgs e)
        {
            using (FontDialog fd = new FontDialog()) {
                fd.Font = rtbChat.Font;
                fd.ShowEffects = false;

                if (fd.ShowDialog() == DialogResult.OK) {
                    var fnt = fd.Font;
                    rtbChat.Font = fnt;
                    RtfBuilder.Instance.FontSize = fnt.SizeInPoints;
                    
                    CompoundTag tag = new CompoundTag();
                    tag.AddString("Name", fnt.Name);
                    tag.AddFloat("SizeInPoints", fnt.SizeInPoints);
                    Program.Config.AddCompound("ChatFont", tag);

                    lbUsers_SelectedItemChanged(lbUsers, new EventArgs());
                }
            }
        }

        private void testadorDeACCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AccountChecker frm = new AccountChecker();
            frm.Show();
        }

        private void tsAutoReco_Click(object sender, EventArgs e)
        {
            MinecraftClient.AutoReconnect = tsAutoReco.Checked;
        }

        private void conectarDesconectadosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for(int i = 0; i < Clients.Count; i++) {
                MinecraftClient cli = Clients[i];
                if (cli.ConnState != ConnectionState.Connected)
                    cli.StartClient();
            }
        }

        private void mineradorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new MinerOptions().Show();
        }
        private void checadorDeBanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new BanCheck().Show();
        }

        private void scriptsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new MacroEditor().Show();
        }

        private void desconectarTodosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < Clients.Count; i++) {
                MinecraftClient mClient = Clients[i];

                mClient.Dispose();
                Clients.RemoveAt(i--);
                lbUsers.Refresh();
            }
        }
        
        private async void CheckForUpdate()
        {
            try {
                string[] d = (await Utils.GetStringAsync("https://dc-proxybot.hyplex.com.br/version2.txt")).Lines();

                if (d[0] != Program.AppVersion) {
                    Program.Config.AddBoolean("ChangelogOpen", false);
                    Program.SaveConf();

                    if (File.Exists("AdvancedBot.Updater.exe") && (d.Length < 3 || d[2] != "manual")) {
                        Process.Start("AdvancedBot.Updater.exe", "https://dc-proxybot.hyplex.com.br/downloads/AdvancedBot_" + d[0] + ".zip AdvancedBot.exe");
                    } else {
                        DialogResult dialogResult = MessageBox.Show("Uma nova atualização foi encontrada: " + d[0] + "\nDeseja baixa-la agora?", "Atualização", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                        if (dialogResult == DialogResult.Yes) {
                            Process.Start("https://dc-proxybot.hyplex.com.br/downloads/AdvancedBot_" + d[0] + ".rar");
                        }
                    }
                    Environment.Exit(0);
                    return;
                }
                if (d[1] != "y") {
                    MessageBox.Show("Este programa foi desativado.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                }
                return;
            } catch (Exception ex) {
                MessageBox.Show("Não foi possível procurar por atualizações.\n" + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }
        }

        private void tsAbout_Click(object sender, EventArgs e)
        {
            if (aboutForm != null && !aboutForm.IsDisposed) return;
            aboutForm = new AboutNew();
            aboutForm.Show();
        }

        private void tsChangelog_Click(object sender, EventArgs e)
        {
            new Changelog().Show();
        }

        private void movimentarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Forms.Movement frm = new Forms.Movement();
            frm.Show();
        }

        private async void discordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try {
                var result = await Utils.GetStringAsync("https://discordapp.com/api/guilds/432265286771539988/widget.json");
                var invite = JObject.Parse(result)["instant_invite"].AsStr();
                Process.Start(invite);
            } catch {

            }
        }

        private void protetorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Protetor.Protetor().Show();   
        }

        private void movimentarToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            new Forms.Movement(lbUsers.SelectedItems.ToList()).Show();
        }

        private void label1_Click(object sender, EventArgs e)
        {
            //label1.Text = "{0} usuários usando o AdvancedBot neste momento".Replace("{0}", "" + Program.Client.getUsersOnline());
        }

        private void tClientCount_Tick(object sender, EventArgs e)
        {
            //label1.Text = "{0} usuários usando o AdvancedBot neste momento".Replace("{0}", "" + Program.Client.getUsersOnline());
        }

        private async void PictureBox1_Click(object sender, EventArgs e)
        {
            try
            {
                String link = await WebConnection.DownloadString("https://advanced-bot.tk/api/v2/discord.php");
                Process.Start(link);
            }
            catch (Exception ex)
            {

            }
        }

        private void ChatOptions_Opening(object sender, CancelEventArgs e)
        {

        }
    }
}