using System;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using AdvancedBot.client;

namespace AdvancedBot
{
    public partial class AccountChecker : Form
    {
        public AccountChecker()
        {
            InitializeComponent();
            listView1.SetExplorerTheme();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(richTextBox1.Text))
            {
                MessageBox.Show("Você precisa adicionar pelo menos uma conta", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            listView1.Items.Clear();
            new Thread(() =>
            {
                btnStart.Enabled = false;
                richTextBox1.Enabled = false;
                ProxyList pl = Program.FrmMain.Proxies;
                foreach (String acc in richTextBox1.Lines)
                {
                    String email = "";
                    String pass = "";
                    if (acc.Contains(":")) {
                        email = acc.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries)[0];
                        pass = acc.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries)[1];
                    } else {
                        continue;
                    }
                    try
                    {
                        Proxy proxy = pl.NextProxy();
                        LoginResponse login = SessionUtils.Login(email, pass, proxy);
                        if (login.Error) {
                            var listViewItem = new ListViewItem(acc);
                            listViewItem.SubItems.Add("Fail");
                            listView1.Items.Add(listViewItem);
                        } else {
                            var listViewItem = new ListViewItem(acc);
                            listViewItem.SubItems.Add("OK");
                            listView1.Items.Add(listViewItem);
                        }
                        int size = getRichSize();
                        percentageProgressBar1.Value = (100 * listView1.Items.Count) / size;
                        Thread.Sleep(2000);
                    }
                    catch (Exception)
                    {
                        var listViewItem = new ListViewItem(acc);
                        listViewItem.SubItems.Add("Fail");
                        listView1.Items.Add(listViewItem);
                    }
                    
                    Thread.Sleep(1000);
                }
                btnStart.Enabled = true;
                richTextBox1.Enabled = true;
            }).Start();
            
        }

        public int getRichSize()
        {
            int i = 0;
            foreach (String acc in richTextBox1.Lines) {
                if (acc.Contains(":"))
                    i++;
            }
            return i;
        }

        private void AccountChecker_Load(object sender, EventArgs e)
        {
            percentageProgressBar1.Show();
            listView1.ContextMenuStrip = contextMenuStrip1;
            Icon = Program.FrmMain.Icon;
        }

        private void copiarFuncionandoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            foreach (ListViewItem item in listView1.Items) {
                if(item.SubItems[1].Text.EqualsIgnoreCase("OK"))
                    sb.AppendLine(item.Text);
            }
            Clipboard.SetDataObject(sb.ToString(), true);
        }

        private void copiarOfflinesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            foreach (ListViewItem item in listView1.Items) {
                if (item.SubItems[1].Text.EqualsIgnoreCase("Fail"))
                    sb.AppendLine(item.Text);
            }
            Clipboard.SetDataObject(sb.ToString(), true);
        }
    }
}
