using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Drawing.Drawing2D;

namespace AdvancedBot
{
    public partial class About : Form
    {
        CPoint[] stars = new CPoint[512];
        Random rand = new Random();

        Rectangle drawImgRect;
        SolidBrush brushGray = new SolidBrush(Color.FromArgb(230, 230, 230));
        Font arial10bold = new Font("Arial", 10, FontStyle.Bold);
        int j = 0;

        Bitmap logo;

        string[] txtLns = new string[] { "AdvancedBot "+Program.AppVersion + " data: " + Program.GetBuildDate().ToString("dd/MM/yyyy"), 
                                         "Desenvolvido por: DarkSkeleton & Alkazuz"};

        public About()
        {
            InitializeComponent();

            int w = ClientSize.Width;
            int h = ClientSize.Height;
            for (int i = 0; i < stars.Length; i++)
                stars[i] = new CPoint(rand.Next(w), rand.Next(h));

            DoubleBuffered = true;

            logo = new Bitmap(1, 1);// AdvancedBot.Properties.Resources.logo;

            drawImgRect = new Rectangle((w / 2) - (logo.Width / 2), (h / 2) - (logo.Height / 2), 201, 72);
            FormClosing += (s, e) => logo.Dispose();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;

            g.Clear(Color.Black);

            int w = ClientSize.Width;
            int h = ClientSize.Height;

            int w2 = w / 2;
            int h2 = h / 2;
            for (int i = 0; i < stars.Length; i++)
            {
                CPoint pt = stars[i];

                if (pt.X < 0 || pt.Y < 0 || pt.X > w || pt.Y > h)
                {
                    do {
                        pt.X = rand.Next(w);
                        pt.Y = rand.Next(h);
                    } while ((int)pt.X == w2 && (int)pt.Y == h2);
                } else {
                    float x1 = pt.X;
                    float y1 = pt.Y;
                    pt.X += (pt.X > w2 ? 1 : -1) * Math.Abs(w2 - pt.X) * 0.07f;
                    pt.Y += (pt.Y > h2 ? 1 : -1) * Math.Abs(h2 - pt.Y) * 0.07f;

                    using (LinearGradientBrush br = new LinearGradientBrush(new PointF(x1, y1), new PointF(pt.X, pt.Y), Color.Black, Color.White))
                    {
                        br.WrapMode = WrapMode.TileFlipX;

                        using (Pen pen = new Pen(br))
                            g.DrawLine(pen, x1, y1, pt.X, pt.Y);
                    }
                }
            }
            
            for (int i = 0; i < w; i++)
            {
                using (SolidBrush br = new SolidBrush(HSL((w - i + j) / 360.0)))
                {
                    g.FillRectangle(br, i, 0, 1, 1);
                    g.FillRectangle(br, i, h - 1, 1, 1);
                }
            }
            for (int i = 0; i < txtLns.Length;i++)
            {
                string txt = txtLns[i];
                int txtW = (int)g.MeasureString(txt, arial10bold).Width;
                g.DrawString(txt, arial10bold, brushGray, new Point((w / 2) - (txtW / 2), (h-(16*txtLns.Length)) + (i * 15)));
            }
            g.DrawImage(logo, drawImgRect);
            j = (j + 16) % 360;
        }
        private static Color HSL(double hue)
        {
            double div = (Math.Abs(hue % 1) * 6);
            int ascending = (int)((div % 1) * 255);
            int descending = 255 - ascending;

            switch ((int)div)
            {
                case 0: return Color.FromArgb(255, 255, ascending, 0);
                case 1: return Color.FromArgb(255, descending, 255, 0);
                case 2: return Color.FromArgb(255, 0, 255, ascending);
                case 3: return Color.FromArgb(255, 0, descending, 255);
                case 4: return Color.FromArgb(255, ascending, 0, 255);
                default: return Color.FromArgb(255, 255, 0, descending);
            }
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            Invalidate();
        }

        private class CPoint
        {
            public float X, Y;
            public CPoint(float x, float y)
            {
                X = x;
                Y = y;
            }
        }

        private void About_Load(object sender, EventArgs e)
        {

        }
    }
}
