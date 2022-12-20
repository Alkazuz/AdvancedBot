using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using AdvancedBot.client;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections;

namespace AdvancedBot
{
    public partial class MinerOptions : Form
    {
        public MinerOptions()
        {
            InitializeComponent();
            Icon = Program.FrmMain.Icon;
            foreach (Block block in Blocks.GetBlocks()) {
                cbBlock.Items.Add(block.ID + ": " + block.Name);
            }

            listView1.ListViewItemSorter = new LVComparer();

            int[] blocks = Program.Config.GetIntArray("MinerBlocks");
            foreach (int block in blocks) {
                int p = block >> 16 & 0xFFFF;
                int id = block & 0xFFFF;
                string bname = id + " (" + Blocks.GetName(id) + ")";
                listView1.Items.Add(new ListViewItem(new string[] { p.ToString(), bname }));
            }
            listView1.Sort();

            nudPriority.Value = blocks.Length + 1;


            cbStopInvFull.Checked = Program.Config.GetBoolOrTrue("MinerStopInvFull");
            nudMinerRadius.Value = Program.Config.GetIntOrDefault("MinerRadius", 8);

            cbExec.Checked = !string.IsNullOrEmpty(Program.Config.GetString("MinerCmdsInvFull"));
            if (cbExec.Checked) {
                tbCmds.Text = Program.Config.GetString("MinerCmdsInvFull");
            }
            cbAutoTool.Checked = Program.Config.GetBoolOrTrue("MinerSelectBestTool");

            listView1.SetExplorerTheme();
        }

        private void btnAddBlock_Click(object sender, EventArgs e)
        {
            try {
                int id = int.Parse(cbBlock.Text.Substring(0, cbBlock.Text.IndexOf(':')));
                int p = (int)nudPriority.Value;

                string bname = id + " (" + Blocks.GetName(id) + ")";
                int i = 0;
                foreach (ListViewItem item in listView1.Items) {
                    if (item.SubItems[1].Text == bname) {
                        item.Selected = true;
                        listView1.Select();
                        listView1.EnsureVisible(i);
                        return;
                    }
                    i++;
                }
                listView1.Items.Add(new ListViewItem(new string[] { p.ToString(), bname }));
                listView1.Sort();

                nudPriority.Value = listView1.Items.Count + 1;
            } catch {
                MessageBox.Show("Ocorreu um erro. Verifique os campos e tente novamente.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelSel_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems) {
                listView1.Items.Remove(item);
            }
        }

        private void listView1_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            int tmp;
            e.CancelEdit = !int.TryParse(e.Label, out tmp);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            int[] blocks = new int[listView1.Items.Count];
            int i = 0;
            foreach (ListViewItem block in listView1.Items) {
                int p = int.Parse(block.Text);

                string bText = block.SubItems[1].Text;
                int id = int.Parse(bText.Substring(0, bText.IndexOf(' ')));

                blocks[i++] = p << 16 | id;
            }
            Program.Config.AddIntArray("MinerBlocks", blocks);
            Program.Config.AddBoolean("MinerStopInvFull", cbStopInvFull.Checked);
            Program.Config.AddString("MinerCmdsInvFull", cbExec.Checked ? tbCmds.Text : "");
            Program.Config.AddInt("MinerRadius", (int)nudMinerRadius.Value);
            Program.Config.AddBoolean("MinerSelectBestTool", cbAutoTool.Checked);

            Close();
        }

        private class LVComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                ListViewItem a = (ListViewItem)x;
                ListViewItem b = (ListViewItem)y;
                if(int.TryParse(a.Text, out int ai) && int.TryParse(b.Text, out int bi)) {
                    return ai.CompareTo(bi);
                } else {
                    return 1;
                }
            }
        }
        private void tbCmds_TextChanged(object sender, EventArgs e)
        {
            cbExec.Checked = true;
        }

        private void listView1_MouseDown(object sender, MouseEventArgs e)
        {
            ListViewItem item = listView1.GetItemAt(e.X, e.Y);
            if (item != null) {
                item.BeginEdit();
            }
        }

        private void btnUpPriority_Click(object sender, EventArgs e)
        {
            AddPriority(-1);
        }
        private void btnDownPriority_Click(object sender, EventArgs e)
        {
            AddPriority(1);
        }
        private void AddPriority(int n)
        {
            listView1.BeginUpdate();
            foreach (ListViewItem item in listView1.SelectedItems) {
                int p = int.Parse(item.SubItems[0].Text);
                item.SubItems[0].Text = Math.Max(1, p + n).ToString();
            }
            listView1.Sort();
            listView1.EndUpdate();
        }

        private void MinerOptions_Load(object sender, EventArgs e)
        {
            Translation.setup(this);
        }
    }
}
