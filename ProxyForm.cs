using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using AdvancedBot.client;
using AdvancedBot.ProxyChecker;
using System.IO;
using System.Diagnostics;

#pragma warning disable IDE1006
namespace AdvancedBot
{
    public partial class ProxyForm : Form
    {
        public ProxyForm()
        {
            InitializeComponent();
        }
        public List<ProxyInfo> displayedProxies = new List<ProxyInfo>();
        private void ProxyForm_Load(object sender, EventArgs e)
        {
            Translation.setup(this);
            this.Icon = Program.FrmMain.Icon;
            lvProxies.SetExplorerTheme();
            lvProxies.FullRowSelect = true;

            foreach (ProxyInfo p in Program.FrmMain.Proxies) {
                displayedProxies.Add(p);
            }
            UpdateListView();
        }
        public void UpdateListView()
        {
            lvProxies.BeginUpdate();
            lvProxies.Items.Clear();
            var sorter = lvProxies.ListViewItemSorter;
            lvProxies.ListViewItemSorter = null;
            foreach (ProxyInfo p in displayedProxies) {
                ListViewItem lvi = new ListViewItem(new string[] { string.Format("{0}:{1}", p.IP, p.Port), p.Type.ToString(), "" });
                lvi.Tag = p;
                lvProxies.Items.Add(lvi);
            }
            lvProxies.ListViewItemSorter = sorter;
            lvProxies.EndUpdate();
            Text = $"Lista de proxies ({displayedProxies.Count})";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try {
                ProxyList list = Program.FrmMain.Proxies;
                list.Clear();
                list.AddRange(displayedProxies.OrderBy(p => p.Ping).Distinct());
                this.Close();
            } catch (Exception ex) {
                MessageBox.Show("Ocorreu um erro:\n\n" + ex.ToString(), "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void btnChecker_Click(object sender, EventArgs e)
        {
            new ProxyCheckerForm(this).Show();
        }
        
        private void tsmiAdd_Click(object sender, EventArgs e)
        {
            ShowAddForm("Adicionar proxies");
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            bool result = base.ProcessCmdKey(ref msg, keyData);
            if (keyData.HasFlag(Keys.V) && keyData.HasFlag(Keys.Control) && addFrm != null) {
                addFrm.Controls["tbProxies"].Text = Clipboard.GetText();
            }
            return result;
        }

        private void tsmiRemove_Click(object sender, EventArgs e)
        {
            lvProxies.BeginUpdate();
            for (int i = 0; i < lvProxies.Items.Count; i++) {
                ListViewItem lvi = lvProxies.Items[i];
                if (lvi.Selected) {
                    lvProxies.Items.RemoveAt(i--);
                    displayedProxies.Remove((ProxyInfo)lvi.Tag);
                }
            }
            lvProxies.EndUpdate();
            Text = $"Lista de proxies ({displayedProxies.Count})";
        }
        private void tsmiCopy_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            foreach (ListViewItem item in lvProxies.Items) {
                if (item.Selected) {
                    sb.AppendLine(item.SubItems[0].Text);
                }
            }
            Clipboard.SetDataObject(sb.ToString(), true);
        }

        private Form addFrm = null;
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
        }
        private void ShowAddForm(string title)
        {
            if (addFrm != null) {
                addFrm.Focus();
                return;
            }
            Form frm = new Form {
                StartPosition = FormStartPosition.CenterScreen,
                Size = new Size(335, 180),
                MaximizeBox = false,
                Text = title,
                Icon = Program.FrmMain.Icon
            };
            Size cs = frm.ClientSize;

            Button btn = new Button();
            btn.Text = "OK";
            btn.Location = new Point(cs.Width - btn.Width - 11, cs.Height - btn.Height - 3);

            TextBox tb = new TextBox();
            tb.Location = new Point(12, 12);
            tb.Multiline = true;
            tb.Name = "tbProxies";
            tb.MaxLength = 1024 * 1024; //1MiB or ~50K proxies
            tb.ScrollBars = ScrollBars.Vertical;
            btn.Focus();

            tb.Size = new Size(cs.Width - 24, cs.Height - 18 - btn.Height);
            
            frm.Controls.Add(tb);
            frm.Controls.Add(btn);

            btn.Click += (s, e) => frm.Close();

            int x = 12;

            List<RadioButton> rBtns = new List<RadioButton>();
            foreach (ProxyType type in new[] { ProxyType.Socks4, ProxyType.Socks5, ProxyType.HTTP }) {
                int rbIndex = frm.Controls.Count - 2;
                RadioButton rb = new RadioButton();
                rb.Text = type.ToString();
                rb.Checked = x == 12;

                rb.Location = new Point(x, cs.Height - 5 - rb.Height);
                rb.Size = new Size(TextRenderer.MeasureText(type.ToString(), rb.Font).Width + 18, 30);
                rb.Tag = type;
                x += rb.Width;
                frm.Controls.Add(rb);
                rBtns.Add(rb);
            }

            frm.FormClosing += (s, e) => {
                ProxyType type = (ProxyType)rBtns.First(b => b.Checked).Tag;

                foreach (ProxyInfo p in ProxyUtils.Parse(tb.Text)) {
                    p.Type = type;
                    p.Ping = 3000;
                    displayedProxies.Add(p);
                }
                UpdateListView();
                addFrm = null;
            };
            (addFrm = frm).Show();
        }

        private void lvProxies_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A && e.Modifiers == Keys.Control) {
                lvProxies.BeginUpdate();
                foreach (ListViewItem item in lvProxies.Items) {
                    item.Selected = true;
                }
                lvProxies.EndUpdate();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            saveMenu.Show(PointToScreen(btnSave.Location), ToolStripDropDownDirection.AboveRight);
        }

        private void tsmiS4_Click(object sender, EventArgs e)   { SaveProxiesByType(ProxyType.Socks4); }
        private void tsmiS5_Click(object sender, EventArgs e)   { SaveProxiesByType(ProxyType.Socks5); }
        private void tsmiHttp_Click(object sender, EventArgs e) { SaveProxiesByType(ProxyType.HTTP); }
        private void tsmiAll_Click(object sender, EventArgs e)  { SaveProxiesByType(0); }

        private void SaveProxiesByType(ProxyType type)
        {
            using (SaveFileDialog sfd = new SaveFileDialog()) {
                sfd.Filter = "Text files|*.txt";
                if (sfd.ShowDialog() == DialogResult.OK) {
                    if (type == 0) {
                        string filename = sfd.FileName.Substring(0, sfd.FileName.LastIndexOf('.'));
                        foreach (ProxyType pType in Enum.GetValues(typeof(ProxyType))) {
                            SaveCore(string.Format("{0}.{1}.txt", filename, pType), pType);
                        }
                    } else {
                        SaveCore(sfd.FileName, type);
                    }
                }
            }
        }
        private void SaveCore(string filename, ProxyType type)
        {
            using (StreamWriter sw = new StreamWriter(filename, false, Encoding.UTF8)) {
                foreach (ProxyInfo p in displayedProxies.Where(a => a.Type == type)) {
                    sw.WriteLine(p.IP + ":" + p.Port);
                }
            }
        }

        private void tsmiSaveCsv_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog()) {
                sfd.Filter = "Comma-separated values files|*.csv";
                if (sfd.ShowDialog() == DialogResult.OK) {
                    using (StreamWriter sw = new StreamWriter(sfd.FileName, false, Encoding.UTF8)) {
                        sw.WriteLine("ip,port,type,ping");
                        foreach(var p in displayedProxies.OrderBy(p => p.Ping)) {
                            sw.WriteLine($"{p.IP},{p.Port},{p.Type.ToString().ToLower()},{p.Ping}");
                        }
                    }
                }
            }
        }

        private void tsmiLoadCsv_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog()) {
                ofd.Filter = "Comma-separated values files|*.csv";
                if (ofd.ShowDialog() == DialogResult.OK) {
                    using (StreamReader sw = new StreamReader(ofd.FileName, Encoding.UTF8)) {
                        sw.ReadLine();
                        for(string ln; (ln = sw.ReadLine()) != null;) {
                            string[] vals = ln.Split(',');

                            ProxyInfo p = new ProxyInfo((ProxyType)Enum.Parse(typeof(ProxyType), vals[2], true),
                                                        vals[0], ushort.Parse(vals[1]),
                                                        int.Parse(vals[3]));
                            displayedProxies.Add(p);
                        }
                    }
                    UpdateListView();
                }
            }
        }

        private void tsmiSelectS4_Click(object sender, EventArgs e)
        {
            SelectAllOfType(ProxyType.Socks4);
        }
        private void tsmiSelectS5_Click(object sender, EventArgs e)
        {
            SelectAllOfType(ProxyType.Socks5);
        }
        private void tsmiSelectHTTP_Click(object sender, EventArgs e)
        {
            SelectAllOfType(ProxyType.HTTP);
        }
        private void SelectAllOfType(ProxyType type) {
            lvProxies.BeginUpdate();
            foreach (ListViewItem item in lvProxies.Items) {
                if ((item.Tag as ProxyInfo).Type == type) {
                    item.Selected = true;
                }
            }
            lvProxies.EndUpdate();
        }
    }
}
