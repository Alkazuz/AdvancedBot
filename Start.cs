using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AdvancedBot.client;
using AdvancedBot.ClientSocket;
using AdvancedBot.Controls;
using AdvancedBot.Forms;

namespace AdvancedBot
{
#pragma warning disable IDE1006
    public partial class Start : Form
    {
        public static Start Instance;
        public bool hideOnClose = false;
        public Start()
        {
            //Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
            InitializeComponent();
            Translation.setup(this);
            cbVersion.Items.Add("1.5.2");
            foreach (ClientVersion val in Enum.GetValues(typeof(ClientVersion)))
                if (val != ClientVersion.Unknown && val != ClientVersion.v1_5_2)
                    cbVersion.Items.Add(val.ToString().Substring(1).Replace('_', '.'));
            
            cbVersion.SelectedIndex = Program.Config.GetInt("PreferredVersion");
            svTb.Text = Program.Config.GetString("LastServer");

            if (Program.Config.Contains("LastUsers"))
                rtbAccounts.Text = Program.Config.GetString("LastUsers");

            cbDoPing.Checked = Program.Config.GetBoolean("SendPing");
            MinecraftClient.MultiPing = Program.Config.GetBoolean("SendMultiPing");
            nudConnLimit.Value = Program.Config.GetInt("ConnectionLimit");


            Instance = this;

            var pb = new PingButton(svTb);
            pb.Location = new Point(svTb.ClientSize.Width - pb.Width + 1, -1);
            svTb.Controls.Add(pb);
            
        }
        private async void btnOk_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(svTb.Text))
            {
                MessageBox.Show(this, Translation.getStringKey("Others.error2"), Translation.getStringKey("Others.error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (string.IsNullOrWhiteSpace(rtbAccounts.Text))
            {
                MessageBox.Show(this, Translation.getStringKey("Others.error3"), Translation.getStringKey("Others.error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Program.Config.AddInt("PreferredVersion", cbVersion.SelectedIndex);
            Program.Config.AddString("LastServer", svTb.Text);
            Program.Config.AddBoolean("SendPing", cbDoPing.Checked);

            try {
                string ip = "";
                ushort port = 25565;

                if (!svTb.Text.ParseIP(out ip, ref port))
                {
                    MessageBox.Show(this, Translation.getStringKey("Others.error4"), Translation.getStringKey("Others.error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                string ipRaw = ip + ":" + port;
                string ipReal = ip;

                if (ip != "localhost")
                {
                    Debug.WriteLine("Original IP: " + ip + ":" + port);
                    SrvResolver.ResolveIP(ref ip, ref port);
                    Debug.WriteLine("SRV record: " + ip + ":" + port);

                    if (WebConnection.check(DoGetHostAddresses(ip) + ":" + port))
                    {
                        MessageBox.Show(this, Translation.getStringKey("Others.error9"), Translation.getStringKey("Others.error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                }
                ProxyList pl = Program.FrmMain.Proxies;
                if (!hideOnClose) {
                    pl.ResetIndexes();
                }

                ClientVersion ver = MinecraftClient.ParseVersion((string)cbVersion.SelectedItem);
                bool phys = cbPhysics.Checked;
                bool ping = cbDoPing.Checked;
                bool limitChunks = cbLimitChunks.Checked;
                
                var task = Task.Run(async () => {
                    var clients = Program.FrmMain.Clients;
                    bool nickConflit = false;

                    int limit = Program.Config.GetIntOrDefault("ConnectionLimit", 0);
                    foreach (string ln in rtbAccounts.Lines.Distinct()) {
                        if (!string.IsNullOrWhiteSpace(ln)) {
                            string nick;
                            string pass = "abc123";

                            ln.SplitColon(out nick, ref pass);
                            if (Program.FrmMain.GetClient(nick) != null) {
                                nickConflit = true;
                                continue;
                            }

                            if (limit != 0 && clients.Count(a => a.IsBeingTicked()) >= limit)
                                break;

                            Proxy proxy = pl.NextProxy();

                            MinecraftClient client = new MinecraftClient(svTb.Text, ip, port, nick, pass, proxy) {
                                Version = ver,
                                autoLogin = cbAutoLogin.Checked,
                                MapAndPhysics = phys,
                                SendPing = ping,
                                LimitChunks = limitChunks,
                                CmdRegister = Program.Config.GetStringOrDefault("StartRegister", "/register @pass @pass"),
                                CmdLogin = Program.Config.GetStringOrDefault("StartLogin", "/login @pass"),
                                RealIP = ipReal
                            };
                            Program.FrmMain.Clients.Add(client);

                            client.StartClient();
                            await Task.Delay((int)nudDelay.Value).ConfigureAwait(false);
                        }
                    }
                    if (nickConflit)
                        MessageBox.Show("Algumas contas não foram adicionadas pois já existem.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                    StringBuilder accs = new StringBuilder();
                    //foreach (MinecraftClient mc in Program.FrmMain.Clients)
                    for (int i = 0; i < clients.Count; i++) {
                        MinecraftClient mc = clients[i];
                        accs.AppendLine((mc.Email ?? mc.Username) + ":" + mc.Password);
                    }
                    Program.Config.AddString("LastUsers", accs.ToString());
                });

                Program.FrmMain.groupBox1.Enabled = true;
                Program.FrmMain.groupBox2.Enabled = true;
                Program.FrmMain.tsSpammer.Enabled = true;
                this.Hide();

                await task;
            } catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

        public static String DoGetHostAddresses(string hostname)
        {

            IPAddress[] ips;

            ips = Dns.GetHostAddresses(hostname);

            Console.WriteLine("GetHostAddresses({0}) returns:", hostname);

            foreach (IPAddress ip in ips)
            {
                return ip.ToString();
            }
            return "localhost";
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            if (!closeAll) { e.Cancel = true; return; }

            if (hideOnClose) {
                Hide();
                e.Cancel = true;
            } else {
                if (Program.FrmMain != null) Program.FrmMain.Close();
            }

            base.OnClosing(e);
        }

        private void btnProxy_Click(object sender, EventArgs e)
        {
            ProxyForm f = new ProxyForm();
            f.Show();
        }

        private void gen5Random_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 5; i++)
            {
                rtbAccounts.AppendText((NickGenerator.RandomNick(14, 1) + (Program.Config.GetBoolean("NickGenPass") ? ":" + NickGenerator.RandomNick(8, 1) : "")) +"\n");
            }
        }
        private void gen10Random_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 5; i++)
            {
                rtbAccounts.AppendText((NickGenerator.RandomNick(14, 1) + (Program.Config.GetBoolean("NickGenPass") ? ":" + NickGenerator.RandomNick(8, 1) : ""))+"\n");

            }
        }
        private void gen5Pseudo_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 5; i++) {
                sb.AppendLine(NickGenerator.PseudoNick() + (Program.Config.GetBoolean("NickGenPass") ? ":" + NickGenerator.RandomNick(8, 1) : ""));
            }
            rtbAccounts.AppendText(sb.ToString() );
        }


        public bool closeAll = true;
        private void Start_Load(object sender, EventArgs e)
        {
            if (!WebConnection.computer.isAllowed())
            {
                WebConnection.Controls(this, false);
            }
            else
            {
                WebConnection.Controls(this, true);
            }
            Translation.setup(this);
            if (!Program.Config.GetBoolean("ChangelogOpen")) {
                Changelog logs = new Changelog();
                logs.Show();
                logs.Focus();
                Program.Config.AddBoolean("ChangelogOpen", true);
            }
        }

        private void btnLoadState_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog {
                Filter = "Arquivo de estado do AdvancedBot|*.state"
            };
            if (ofd.ShowDialog() == DialogResult.OK) {
                if (!Program.FrmMain.LoadState(ofd.FileName, (int)nudDelay.Value))
                    MessageBox.Show("Arquivo inválido!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else {
                    hideOnClose = true;
                    this.Hide();

                    Program.FrmMain.groupBox1.Enabled = true;
                    Program.FrmMain.groupBox2.Enabled = true;
                    Program.FrmMain.tsSpammer.Enabled = true;
                }
            }
        }

        private void btnChkAccs_Click(object sender, EventArgs e)
        {
            new AccountChecker().Show();
        }

        private void cbDoPing_CheckedChanged(object sender, EventArgs e)
        {
            if (Instance != null && cbDoPing.Checked) {
                DialogResult dp = MessageBox.Show("Deseja enviar mais de um Ping?", "Ping bypass", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                MinecraftClient.MultiPing = dp == DialogResult.Yes;

                Program.Config.AddBoolean("SendMultiPing", MinecraftClient.MultiPing);
            }
        }

        private void definirPrefixoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new SetPrefixForm().Show();
        }

        private void gen5Prefix_Click(object sender, EventArgs e)
        {
            int nbNicks = rtbAccounts.Lines.Count(a => !string.IsNullOrWhiteSpace(a));
            StringBuilder sb = new StringBuilder();
            string prefix = Program.Config.GetString("NickGenPrefix");
            for (int i = 0; i < 5; i++) {
                sb.AppendLine(prefix + NickGenerator.RandomNick(3, 1) + (Program.Config.GetBoolean("NickGenPass") ? ":" + NickGenerator.RandomNick(8, 1) : ""));
            }
            rtbAccounts.AppendText(sb.ToString());
        }

        private int seqCounter = (int)(Utils.GetTimestamp() / 5 % 210000L);
        private void gen5PrefixSeq_Click(object sender, EventArgs e)
        {
            int nbNicks = rtbAccounts.Lines.Count(a => !string.IsNullOrWhiteSpace(a));
            StringBuilder sb = new StringBuilder();
            string prefix = Program.Config.GetString("NickGenPrefix");

            for (int i = 0; i < 5; i++) {
                sb.AppendLine(prefix + NickGenerator.Sequential(seqCounter+i) + (Program.Config.GetBoolean("NickGenPass") ? ":" + NickGenerator.RandomNick(8, 1) : ""));
            }
            seqCounter += 5;
            rtbAccounts.AppendText(sb.ToString());
        }

        private void rtbMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            gen5Prefix.Text = string.Format("Gerar 5 nicks com o prefixo '{0}'", Program.Config.GetString("NickGenPrefix"));
            gen5PrefixSeq.Text = string.Format("Gerar 5 nicks com o prefixo '{0}' sequencial", Program.Config.GetString("NickGenPrefix"));
        }

        private void nudConnLimit_ValueChanged(object sender, EventArgs e)
        {
            Program.Config.AddInt("ConnectionLimit", (int)nudConnLimit.Value);
        }

        private void rtbAccounts_TextChanged(object sender, EventArgs e)
        {
            gbAccounts.Text = $"Contas ({rtbAccounts.Lines.Distinct().Count(a => !string.IsNullOrWhiteSpace(a))})";
        }
        public static LoginSettings frmLogin;
        private void CbAutoLogin_CheckedChanged(object sender, EventArgs e)
        {
            Program.Config.AddBoolean("AutoLogin", cbAutoLogin.Checked);
            Program.SaveConf();
            Debug.WriteLine(Program.Config.GetBoolOrTrue("AutoLogin"));
            if (cbAutoLogin.Checked)
            {
                if (frmLogin != null && !frmLogin.IsDisposed) return;
                frmLogin = new LoginSettings();
                frmLogin.Show();
            }
        }

        private void CbLimitChunks_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
