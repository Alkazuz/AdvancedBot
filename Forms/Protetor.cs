using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AdvancedBot.Protetor
{
    public partial class Protetor : Form
    {
        public static HashSet<String> protect = new HashSet<String>();
        public Protetor()
        {
            InitializeComponent();
        }

        private void cbFollow_CheckedChanged(object sender, EventArgs e)
        {
            textBox1.Enabled = cbFollow.Checked;
        }

        private void Protetor_Load(object sender, EventArgs e)
        {
            Translation.setup(this);
            this.Icon = Program.FrmMain.Icon;
            if (protect.Count > 0)
            {
                richTextBox1.Clear();
            }
                foreach (String nicks in protect)
                {
                    richTextBox1.AppendText(nicks+"\n");
                }
            cbFollow.Checked = Program.Config.GetBoolean("ProtetorFollow");
            checkBox1.Checked = Program.Config.GetBoolean("ProtetorArmor");
            checkBox2.Checked = Program.Config.GetBoolean("ProtetorEspada");
            textBox1.Text = Program.Config.GetString("ProtetorFollowNick");
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            protect.Clear();
            foreach (String line in richTextBox1.Lines)
            {
                protect.Add(line);
            }
            Program.Config.AddBoolean("ProtetorFollow", cbFollow.Checked);
            Program.Config.AddBoolean("ProtetorArmor", checkBox1.Checked);
            Program.Config.AddInt("ProtetorCPS", trackBar1.Value);
            Program.Config.AddBoolean("ProtetorEspada", checkBox2.Checked);
            Program.Config.AddString("ProtetorFollowNick", textBox1.Text);
            this.Close();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            label2.Text = trackBar1.Value + " CPS";
        }
    }
}
