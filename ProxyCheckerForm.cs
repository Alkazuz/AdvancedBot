using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using AdvancedBot.client;
using AdvancedBot.client.Packets;
using AdvancedBot.ProxyChecker;
using Newtonsoft.Json.Linq;

namespace AdvancedBot
{
    public partial class ProxyCheckerForm : Form
    {
        public ProxyCheckerForm()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            lvProxies.ContextMenuStrip = cms1;
            this.Icon = Program.FrmMain.Icon;
        }
        private ProxyForm p_form = null;
        public ProxyCheckerForm(ProxyForm pForm)
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            lvProxies.ContextMenuStrip = cms1;
            this.Icon = Program.FrmMain.Icon;
            p_form = pForm;

            FormClosing += (s, e) => {
                if (proxies.Count >= 0) {
                    if (p_form != null) {
                        proxies.Sort((a, b) => a.Ping.CompareTo(b.Ping));
                        p_form.displayedProxies.AddRange(proxies.Where(a => a.Type != (ProxyType)0));
                        p_form.UpdateListView();
                    }
                }
                try {
                    if (chkQueue != null) chkQueue.Dispose();
                } catch { }
            };

            lvProxies.SetExplorerTheme();
            lvProxies.FullRowSelect = true;


            cbServer.SelectedIndex = 0;
            cbServer.Items.Add("Outro (" + Program.FrmMain.frmStart.svTb.Text + ")");

            for (int i = 250; i <= 4000; i += 250) {
                tsmiRemoveProxyPing.DropDownItems.Add(i + "ms");
            }
            tsmiRemoveProxyPing.DropDownItemClicked += (s, e) => {
                string pingstr = e.ClickedItem.Text;
                int pingMax = int.Parse(pingstr.Substring(0, pingstr.Length - 2));

                lvProxies.BeginUpdate();
                for (int i = 0; i < lvProxies.Items.Count; i++) {
                    pingstr = lvProxies.Items[i].SubItems[2].Text;
                    if (pingstr != "---" && int.Parse(pingstr.Substring(0, pingstr.Length - 2)) > pingMax) {
                        lvProxies.Items.RemoveAt(i--);
                    }
                }
                lvProxies.EndUpdate();
                proxies.RemoveAll(a => a.Ping > pingMax);
            };
        }

        List<ProxyInfo> proxies = new List<ProxyInfo>();

        private ProxyCheckQueue chkQueue;
        private void btnStart_Click(object sender, EventArgs e)
        {
            HashSet<ProxyInfo> proxyList = ProxyUtils.Parse(rtfProxies.Text);

            if (proxyList.Count > 0)
                percentageProgressBar1.Maximum = proxyList.Count;
            else {
                this.Close();
                return;
            }
            rtfProxies.Visible = false;
            lvProxies.Visible = true;
            percentageProgressBar1.Visible = true;
            btnStart.Enabled = false;
            cbServer.Enabled = false;
            label3.Visible = false;
            nudTimeout.Visible = false;
            
            new Thread(() => {
                string host = FirstOnlineServer();

                if (host == null) {
                    MessageBox.Show("Não foi encontrado nenhum servidor para testes!", "ERRO", MessageBoxButtons.OK);
                    return;
                }
                int hi = host.IndexOf(':');
                string tip = host.Substring(0, hi);
                ushort tport = ushort.Parse(host.Substring(hi + 1));
                Debug.WriteLine(proxyList.Count + "||" + host);

                SetModeRBsVisible(false);

                bool login = tport != 80 && tport != 443 && rbLogin.Checked;
                int loginVer = 47;
                if (login) {
                    try {
                        using (MinecraftStream ms = new MinecraftStream(new TcpClient(tip, tport))) {
                            ms.SendPacket(new PacketHandshake(47, tip, tport, 1), null);
                            WriteBuffer don = new WriteBuffer();
                            don.WriteVarInt(0x00);
                            ms.SendPacket(don);

                            ReadBuffer rb = ms.ReadPacket();
                            if (rb.ID != 0x00) throw new Exception("Ping response ID != 0x00");
                            JObject response = JObject.Parse(rb.ReadString());
                            loginVer = response["version"]["protocol"].AsInt();
                        }
                    } catch(Exception ex) {
                        MessageBox.Show("Erro ao identificar a versão do servidor. Usando o modo de checagem 'Ping'.\n\n" + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        login = false;
                    }
                }
                Debug.WriteLine(loginVer);

                const int MAX_THREADS = 64;
                chkQueue = new ProxyCheckQueue();
                foreach (ProxyInfo p in proxyList) {
                    if (IsDisposed) break;
                    p.SetTestAddr(tip, tport);
                    p.LoginVer = login ? loginVer : -1;
                    chkQueue.Add(p);
                    while ((chkQueue.QueueSize >= MAX_THREADS && chkQueue.FreeThreadCount < 1) && !IsDisposed)
                        Thread.Sleep(100);
                }
            }).Start();
        }

        private string FirstOnlineServer()
        {
            string cbText = cbServer.Text;

            string addr = cbText.EqualsIgnoreCase("Nenhum") ? "survival.redesky.com:25565" : cbText.Split('(', ')')[1];

            if (true/*cbText.StartsWith("Outro (")*/) {
                string ip;
                ushort port = 25565;
                if (!addr.ParseIP(out ip, ref port)) {
                    MessageBox.Show("O endereço especificado está em um formato inválido!", "ERRO", MessageBoxButtons.OK);
                    addr = null;
                }
                SrvResolver.ResolveIP(ref ip, ref port);
                addr = ip + ":" + port;
            }

            return addr;
        }

        bool clearTextOnClick = true;
        private void rtfProxies_Click(object sender, EventArgs e)
        {
            if (clearTextOnClick) {
                rtfProxies.Text = "";
                clearTextOnClick = false;
            }
        }

        private void tsmiCopys4_Click(object sender, EventArgs e)
        {
            CopyToClipboard(ProxyType.Socks4);
        }
        private void tsmiCopys5_Click(object sender, EventArgs e)
        {
            CopyToClipboard(ProxyType.Socks5);
        }
        private void tsmiCopyhttp_Click(object sender, EventArgs e)
        {
            CopyToClipboard(ProxyType.HTTP);
        }
        private void tsmiRemoveInvalid_Click(object sender, EventArgs e)
        {
            lvProxies.BeginUpdate();
            for (int i = 0; i < lvProxies.Items.Count; i++) {
                if (lvProxies.Items[i].SubItems[1].Text == "---")
                    lvProxies.Items.RemoveAt(i--);
            }
            lvProxies.EndUpdate();
        }

        private void CopyToClipboard(ProxyType t)
        {
            StringBuilder sb = new StringBuilder();
            foreach (ProxyInfo i in proxies)
                if (i.Type == t)
                    sb.AppendLine(i.IP + ":" + i.Port);
            Clipboard.SetDataObject(sb.ToString(), true);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (chkQueue != null) {
                List<ProxyInfo> proxies = chkQueue.GetFinished((int)nudTimeout.Value * 1000);

                this.proxies.AddRange(proxies);

                percentageProgressBar1.Value += proxies.Count;
                foreach (ProxyInfo p in proxies) {
                    if (p.Type == 0)
                        lvProxies.Items.Add(new ListViewItem(new string[] { p.IP + ":" + p.Port, "---", "---", "" }));
                    else {
                        
                        lvProxies.Items.Add(new ListViewItem(new string[] { p.IP + ":" + p.Port, p.Type.ToString(), p.Ping + "ms", "" }));
                    }
                }
            }
        }

        private void tsmiRemoveSel_Click(object sender, EventArgs e)
        {
            lvProxies.BeginUpdate();
            for (int i = 0; i < lvProxies.Items.Count; i++) {
                if (lvProxies.Items[i].Selected)
                    lvProxies.Items.RemoveAt(i--);
            }
            lvProxies.EndUpdate();
        }

        private void cbServer_SelectedIndexChanged(object sender, EventArgs e)
        {
            string cbText = cbServer.Text;
            if (cbText.EqualsIgnoreCase("Nenhum")) return;

            string addr = cbText.Split('(', ')')[1];
            if (addr.EndsWith(":80") || addr.EndsWith(":443")) {
                SetModeRBsVisible(false);
            } else {
                SetModeRBsVisible(true);
            }
        }
        private void SetModeRBsVisible(bool vis)
        {
            label2.Visible = vis;
            rbPing.Visible = vis;
            rbLogin.Visible = vis;
        }

        /*private void tsmiGen_Click(object sender, EventArgs e)
        {
            string[] p = new []{ "https://dc-proxybot.hyplex.com.br/proxygen.php?type=s4&count=50",
                                 "https://dc-proxybot.hyplex.com.br/proxygen.php?type=s5&count=50",
                                 "https://dc-proxybot.hyplex.com.br/proxygen.php?type=ssl&count=50" };
            HashSet<ProxyInfo> proxies = new HashSet<ProxyInfo>();
            for(int i = 0; i < p.Length; i++) {
                foreach (ProxyInfo proxy in ProxyUtils.Parse(p[i].HttpGet())) {
                    proxies.Add(proxy);
                }
            }
            rtfProxies.AppendText(string.Join("\n", proxies.Select(proxy => proxy.IP + ":" + proxy.Port)));
        }*/

        private void tsmiFilter_Click(object sender, EventArgs e)
        {
            new FilterCountryForm(this).AddFromList(rtfProxies.Text).Show();
        }

        private void ProxyCheckerForm_Load(object sender, EventArgs e)
        {
            Translation.setup(this);
        }
    }
}
