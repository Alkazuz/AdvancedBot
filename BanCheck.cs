using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AdvancedBot.client;
using System.Threading;
using AdvancedBot.client.Packets;
using System.Net.Sockets;
using System.Diagnostics;

namespace AdvancedBot
{
    public partial class BanCheck : Form
    {
        public BanCheck()
        {
            InitializeComponent();
        }

        private void BanCheck_Load(object sender, EventArgs e)
        {
            Icon = Program.FrmMain.Icon;

            foreach (ClientVersion val in Enum.GetValues(typeof(ClientVersion)))
                if (val != ClientVersion.Unknown && val != ClientVersion.v1_5_2)
                    cbVersion.Items.Add(val.ToString().Replace('_', '.'));
            
            int n = Program.Config.GetInt("PreferredVersion") - 1;
            if (n >= 0) {
                cbVersion.SelectedIndex = n;
            }
            tbSvAddr.Text = Program.Config.GetString("LastServer");
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbSvAddr.Text)) {
                MessageBox.Show("O IP do servidor não pode estar vazio.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (string.IsNullOrWhiteSpace(rtbAccounts.Text)) {
                MessageBox.Show("Você prescisa inserir pelo menos uma conta.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string ip = "";
            ushort port = 25565;

            if (!tbSvAddr.Text.ParseIP(out ip, ref port)) {
                MessageBox.Show("O endereço do servidor especificado está em um formato inválido", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            SrvResolver.ResolveIP(ref ip, ref port);

            rtbAccounts.Visible = false;
            lvResult.Visible = true;
            lvResult.BringToFront();

            lvResult.SetExplorerTheme();
            lvResult.FullRowSelect = true;
            lvResult.ShowItemToolTips = true;

            btnStart.Enabled = false;
            
            ProxyList pl = Program.FrmMain.Proxies;
            pl.ResetIndexes();

            ClientVersion ver = MinecraftClient.ParseVersion(((string)cbVersion.SelectedItem).Substring(1));
            Debug.WriteLine(ver);

            bool ping = cbSendPing.Checked;
            string[] nicks = rtbAccounts.Lines.Where(a => !string.IsNullOrWhiteSpace(a)).ToArray();
            progress.Maximum = nicks.Length;

            new Thread(() => {
                foreach (string nick in nicks) {
                    if (IsDisposed) return;
                    Proxy proxy = pl.NextProxy();

                    if (ping) {
                        Ping(ip, port, ver, proxy);
                    }
                    lvResult.Items.Add(new ListViewItem(new string[] { nick, CheckBan(nick, ip, port, ver, proxy).Replace('\n', '|') }));
                    progress.Value++;

                    Thread.Sleep(1000);
                }
            }).Start();
        }

        bool cleaned = false;
        private void rtbAccounts_Enter(object sender, EventArgs e)
        {
            if (!cleaned) {
                rtbAccounts.Text = "";
                cleaned = true;
            }
        }


        private static bool Ping(string ip, ushort port, ClientVersion ver, Proxy proxy = null)
        {
            try {
                int count = MinecraftClient.MultiPing ? 4 : 1;
                for (int i = 0; i < count; i++) {
                    TcpClient c = proxy == null ? new TcpClient(ip, port) : proxy.Connect(ip, port);

                    c.ReceiveTimeout = 5000;
                    c.SendTimeout = 5000;

                    using (MinecraftStream ms = new MinecraftStream(c)) {
                        ms.SendPacket(new PacketHandshake((int)ver, ip, port, 1), null);
                        WriteBuffer don = new WriteBuffer();
                        don.WriteVarInt(0x00);
                        ms.SendPacket(don);

                        int len = ms.ReadVarInt();
                        if ((len <= 0 || len > 2097152) || ms.ReadVarInt() != 0x00) {
                            return false;
                        }
                        string info = Encoding.UTF8.GetString(ms.ReadByteArray(ms.ReadVarInt()));
                    }

                    if (count != 1)
                        Thread.Sleep(1000);
                }
            } catch {
                return false;
            }
            return true;
        }
        private static string CheckBan(string nick, string ip, ushort port, ClientVersion ver, Proxy proxy = null)
        {
            try {
                /*
                if (email != null && loginResp == null) {
                    loginResp = SessionUtils.Login(email, password, proxy);
                    if (loginResp.Error)
                        throw new IOException("Não foi possivel efetuar a autenticação");
                    nick = loginResp.Username;
                }*/

                TcpClient c = proxy == null ? new TcpClient(ip, port) : proxy.Connect(ip, port);

                c.ReceiveTimeout = 5000;
                c.SendTimeout = 5000;

                MinecraftStream mstream = new MinecraftStream(c);

                mstream.SendPacket(new PacketHandshake((int)ver, ip, port, 2), null);
                mstream.SendPacket(new PacketLoginStart(nick), null);

                int conStatus = 0;
                for (int i = 0; conStatus == 0 && i < 64; i++) {
                    ReadBuffer packet = mstream.ReadPacket();

                    if (conStatus == 0) {
                        switch (packet.ID) {
                            case 0x00: //Disconnect
                                string reason = ChatParser.ParseJson(packet.ReadString());
                                mstream.Dispose();
                                return "Kick: " + Utils.StripColorCodes(reason);
                            case 0x01: //Encryption request
                                string serverID = packet.ReadString();
                                byte[] serverKey = packet.ReadByteArray(packet.ReadVarInt());
                                byte[] token = packet.ReadByteArray(packet.ReadVarInt());

                                mstream.Dispose();
                                return "Autenticação não é suportada.";
                            case 0x02: //Login success
                                packet.ReadString();
                                packet.ReadString();
                                conStatus = 1;
                                // mstream.Dispose();
                                // return "Não banido";
                                break;
                            case 0x03: //Set compression
                                mstream.CompressionThreshold = packet.ReadVarInt();
                                break;
                            default: return "O servidor enviou um pacote desconhecido.";
                        }
                    } else {
                        if (ver == ClientVersion.v1_8 && packet.ID == 0x46) {
                            mstream.CompressionThreshold = packet.ReadVarInt();
                        } else if(packet.ID == GetJoinGameID(ver)) {
                            mstream.Dispose();
                            return "Não banido";
                        }
                    }
                }

                if (conStatus == 1) {
                    return "Provavelmente não banido";
                } else {
                    return "Por algum motivo, a conexão não foi completada.";
                }
            } catch {
                return "Não foi possível conectar-se ao servidor.";
            }
        }

        private static int GetJoinGameID(ClientVersion ver)
        {
            switch (ver) {
                case ClientVersion.v1_7: return 0x01;
                case ClientVersion.v1_7_10: return 0x01;
                case ClientVersion.v1_8: return 0x01;
                case ClientVersion.v1_9: return 0x23;
                /*case McVersion.v1_9_2: return 0x23;
                case McVersion.v1_10: return 0x23;
                case McVersion.v1_11: return 0x23;
                case McVersion.v1_11_2: return 0x23;
                case McVersion.v1_12: return 0x23;*/
                case ClientVersion.v1_12_1: return 0x23;
                /*case McVersion.v1_12_2: return 0x23;
                case McVersion.v1_13: return 0x25;
                case McVersion.v1_13_1: return 0x25;*/
                default: return -1;
            }
        }
    }
}
