using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AdvancedBot
{
    public partial class SetPrefixForm : Form
    {
        public SetPrefixForm()
        {
            InitializeComponent();
            Icon = Program.FrmMain.Icon;
            tbPrefix.Text = Program.Config.GetString("NickGenPrefix");
            checkBox1.Checked = Program.Config.GetBoolean("NickGenPass");
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            Program.Config.AddString("NickGenPrefix", tbPrefix.Text);
            Program.Config.AddBoolean("NickGenPass", checkBox1.Checked);
            Close();
        }

        private void SetPrefixForm_Load(object sender, EventArgs e)
        {
            Translation.setup(this);
        }
    }
}
