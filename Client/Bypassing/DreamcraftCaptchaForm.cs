using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Request = AdvancedBot.client.Bypassing.DreamcraftBypass.CaptchaSolveRequest;

namespace AdvancedBot.client.Bypassing
{
    public partial class DreamcraftCaptchaForm : Form
    {
        private Request Current = null;
        private object drawSync = new object();

        public DreamcraftCaptchaForm()
        {
            InitializeComponent();
            Icon = Program.FrmMain.Icon;
            timer1.Enabled = true;
            DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;

            lock (drawSync) {
                if (Current != null) {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.DrawImage(Current.Image, new Rectangle(12, 12, 256, 156));
                }
            }
            g.DrawRectangle(Pens.Gray, new Rectangle(12, 12, 256, 156));
        }
        private void tbText_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r') {
                e.Handled = true;
                btnOK.PerformClick();
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if(Current != null) {
                Current.Bypasser.SetCaptchaText(tbText.Text);
            }
            tbText.Text = "";
            TryGetNextCaptcha();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (Current == null) {
                TryGetNextCaptcha();
            }
            string user = Current == null ? "" : ("Atual: " + Current.Client.Username + " ");
            lblInfo.Text = $"{user}Restante: {DreamcraftBypass.Captchas.Count}";
        }
        private void TryGetNextCaptcha()
        {
            Queue<Request> queue = DreamcraftBypass.Captchas;
            lock (queue) {
                lock (drawSync) {
                    if (Current != null) {
                        Current.Image.Dispose();
                        Current = null;
                        Invalidate();
                    }
                    if (queue.Count > 0) {
                        Current = queue.Dequeue();
                        Invalidate();
                    }
                }
            }
        }
    }
}
