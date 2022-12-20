using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Reflection;

namespace AdvancedBot
{
    public partial class Ranking : Form
    {
        public Ranking()
        {
            InitializeComponent();
            listView1.SetExplorerTheme();
            listView1.FullRowSelect = true;
        }

        private void Ranking_Load(object sender, EventArgs e)
        {
            UpdateRanking();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            UpdateRanking();
        }
        private void UpdateRanking()
        {
            try {
                WebClient client = new WebClient();
                client.DownloadStringCompleted += (s, e) => {
                    listView1.Items.Clear();
                    foreach (string a in e.Result.Lines()) {
                        listView1.Items.Add(new ListViewItem(a.Split('|')));
                    }
                    listView1.AutoResizeColumn(0, ColumnHeaderAutoResizeStyle.ColumnContent);
                };
                client.DownloadStringAsync(new Uri("https://dc-proxybot.hyplex.com.br/ranking.php"));
            } catch { }
        }

        private void tsmiCopy_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0) {
                Clipboard.SetDataObject(listView1.SelectedItems[0].SubItems[0].Text, true);
            }
        }
    }
}
