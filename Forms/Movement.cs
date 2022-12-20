using AdvancedBot.client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AdvancedBot.Forms
{
    public partial class Movement : Form
    {
        public List<MinecraftClient> Clients;
        public Movement(List<MinecraftClient> Clients)
        {
            this.Clients = Clients;
            InitializeComponent();
        }

        public Movement()
        {
            this.Clients = Program.FrmMain.Clients;
            InitializeComponent();
        }

        private void Movement_Load(object sender, EventArgs e)
        {
            this.Icon = Program.FrmMain.Icon;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            foreach(MinecraftClient clients in Clients)
            {
                for (int i = 0; i < numericUpDown1.Value; i++)
                {
                    clients.Player.MoveQueue.Enqueue(AdvancedBot.client.Movement.Forward);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            foreach (MinecraftClient clients in Clients)
            {
                for (int i = 0; i < numericUpDown1.Value; i++)
                {
                    clients.Player.MoveQueue.Enqueue(AdvancedBot.client.Movement.Back);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            foreach (MinecraftClient clients in Clients)
            {
                for (int i = 0; i < numericUpDown1.Value; i++)
                {
                    clients.Player.MoveQueue.Enqueue(AdvancedBot.client.Movement.Right);
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            foreach (MinecraftClient clients in Clients)
            {
                for (int i = 0; i < numericUpDown1.Value; i++)
                {
                    clients.Player.MoveQueue.Enqueue(AdvancedBot.client.Movement.Left);
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            foreach (MinecraftClient clients in Clients)
            {
                for (int i = 0; i < 1; i++)
                {
                    clients.Player.MoveQueue.Enqueue(AdvancedBot.client.Movement.Jump);
                }
            }
        }
    }
}
