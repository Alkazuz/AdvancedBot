using System;
using System.Windows.Forms;

namespace AdvancedBot.Forms
{
    public partial class LoginSettings : Form
    {
        public LoginSettings()
        {
            InitializeComponent();
        }

        private void LoginSettings_Load(object sender, EventArgs e)
        {
            Translation.setup(this);
            txtLogin.Text = Program.Config.GetStringOrDefault("StartLogin", txtLogin.Text);
            txtRegister.Text = Program.Config.GetStringOrDefault("StartRegister", txtRegister.Text);
        }

        private void btnSendInput_Click(object sender, EventArgs e)
        {
            Program.Config.AddString("StartLogin", txtLogin.Text);
            Program.Config.AddString("StartRegister", txtRegister.Text);
            Program.SaveConf();
            Close();
        }
    }
}
