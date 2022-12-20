using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AdvancedBot
{
    public partial class AboutNew : Form
    {
        private Bitmap cmdBlock;
        private Font fontBig;
        private Font fontSmall;
        public AboutNew()
        {
            InitializeComponent();
            Icon = Program.FrmMain.Icon;

            cmdBlock = Properties.Resources.cmdBlock;

            fontBig = new Font("Segoe UI", 16f, FontStyle.Regular);
            fontSmall = new Font("Segoe UI", 10f, FontStyle.Regular);
            FormClosing += (s, e) => {
                fontBig.Dispose();
                fontSmall.Dispose();
                cmdBlock.Dispose();
            };
            p = new int[256];
            Random rng = new Random();
            for (int i = 0; i < 256; i++) {
                p[i] = rng.Next(256);
            }
            // To remove the need for index wrapping, double the permutation table length
            perm = new int[512];
            for (int i = 0; i < 512; i++) {
                perm[i] = p[i & 255];
            }
            DoubleBuffered = true;
        }

        private bool overLink = false;
        protected override unsafe void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;

            int w = ClientSize.Width / 2;
            int h = ClientSize.Height / 2;
            using (Bitmap bmp = new Bitmap(w, h)) {
                BitmapData data = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);

                double time = Environment.TickCount / 2000.0;
                int* pixels = (int*)data.Scan0;

                double tb = time * 0.35;

                double bx = (Noise3d(0, 0, tb) * 0.75 + 0.5) * w;
                double by = (Noise3d(1, 1, tb) * 0.75 + 0.5) * h;

                for (int y = 0; y < h; y++) {
                    for (int x = 0; x < w; x++) {
                        double scale = 0.045;

                        double dx = bx - x;
                        double dy = by - y;
                        scale = Math.Min(0.1, Math.Max(((Math.Sqrt(dx * dx + dy * dy) / 50) + 0.25) * scale, 0.02));
                        int n = (int)Math.Abs((Noise3d(x * scale, y * scale, time)) * 16) + 240;
                        pixels[x + y * w] = 0xFF << 24 | n << 16 | n << 8 | n;
                    }
                }
                bmp.UnlockBits(data);
                g.DrawImage(bmp, ClientRectangle);
            }
            g.InterpolationMode = InterpolationMode.Bilinear;
            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            g.DrawImage(cmdBlock, new Rectangle(16, 32, 64, 64));

            SizeF titleSize = g.MeasureString("AdvancedBot", fontBig);
            g.DrawString("AdvancedBot", fontBig, Brushes.Black, 88, 36);
            g.DrawString(Program.AppVersion, fontSmall, Brushes.Gray, 86 + titleSize.Width, 45);
            g.DrawString("Compilado em: " + Program.GetBuildDate(), Font, Brushes.Gray, 90, 64);

            g.DrawString("Desenvolvido por DarkSkeleton & Alkazuz", fontSmall, Brushes.Black, 16, 96);
            g.DrawString("Duvidas (Discord): Cron#9613", fontSmall, Brushes.Black, 16, 110);

            g.DrawString("Nosso canal: ", fontSmall, Brushes.Black, 16, 130);
            float urlX1 = 16 + g.MeasureString("Nosso canal: ", fontSmall).Width;
            float urlX2 = urlX1 + g.MeasureString("youtube.com/caçadoresdebancdb", fontSmall).Width - 2;
            g.DrawString("youtube.com/caçadoresdebancdb", fontSmall, Brushes.DodgerBlue, urlX1, 130);
            g.DrawLine(Pens.DodgerBlue, urlX1, 130 + 16, urlX2, 130 + 16);

            var cur = PointToClient(Cursor.Position);
            if (cur.X >= urlX1 && cur.X <= urlX2 && cur.Y >= 130 && cur.Y <= 130 + 16) {
                if (!overLink) {
                    Cursor = Cursors.Hand;
                    overLink = true;
                }
            } else if(overLink) {
                Cursor = Cursors.Default;
                overLink = false;
            }

            g.DrawString("Este programa é distribuido gratuitamente,", fontSmall, Brushes.Black, 16, 150);
            g.DrawString("Comércio deste programa não será tolerado.", fontSmall, Brushes.Black, 16, 165);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if(e.Button == MouseButtons.Left && overLink) {
                Process.Start("https://youtube.com/caçadoresdebancdb");
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Invalidate();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }


        //https://gamedev.stackexchange.com/a/20943
        int[][] grad3 = {new []{1,1,0},new []{-1,1,0},new []{1,-1,0},new []{-1,-1,0},
                         new []{1,0,1},new []{-1,0,1},new []{1,0,-1},new []{-1,0,-1},
                         new []{0,1,1},new []{0,-1,1},new []{0,1,-1},new []{0,-1,-1}};
        int[] p, perm;
        private double Dot(int[] g, double x, double y, double z)
        {
            return g[0]*x + g[1]*y + g[2]*z;
        }

        private void AboutNew_Load(object sender, EventArgs e)
        {
            var task = Task.Run(async () => {
                try
                {
                    var FN = System.IO.Path.GetTempPath() + @"\AdvancedBot-MUSIC.mp3";
                    if (!System.IO.File.Exists(FN))
                    {
                        System.Net.WebClient WC = new System.Net.WebClient();
                        WC.DownloadFile("https://dc-proxybot.hyplex.com.br/downloads/AdvancedBot-MUSIC.mp3", FN);
                    }
                    axWindowsMediaPlayer1.settings.setMode("Loop", true);
                    axWindowsMediaPlayer1.URL = FN;
                    axWindowsMediaPlayer1.Ctlcontrols.play();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            });

        }

        private void axWindowsMediaPlayer1_Enter(object sender, EventArgs e)
        {

        }

        private double Noise3d(double xin, double yin, double zin)
        {
            double n0, n1, n2, n3; // Noise contributions from the four corners
            // Skew the input space to determine which simplex cell we're in
            var F3 = 1.0 / 3.0;
            var s = (xin + yin + zin) * F3; // Very nice and simple skew factor for 3D
            var i = Math.Floor(xin + s);
            var j = Math.Floor(yin + s);
            var k = Math.Floor(zin + s);
            var G3 = 1.0 / 6.0; // Very nice and simple unskew factor, too
            var t = (i + j + k) * G3;
            var X0 = i - t; // Unskew the cell origin back to (x,y,z) space
            var Y0 = j - t;
            var Z0 = k - t;
            var x0 = xin - X0; // The x,y,z distances from the cell origin
            var y0 = yin - Y0;
            var z0 = zin - Z0;
            // For the 3D case, the simplex shape is a slightly irregular tetrahedron.
            // Determine which simplex we are in.
            int i1, j1, k1; // Offsets for second corner of simplex in (i,j,k) coords
            int i2, j2, k2; // Offsets for third corner of simplex in (i,j,k) coords
            if (x0 >= y0) {
                if (y0 >= z0) { i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 1; k2 = 0; } // X Y Z order
                  else if (x0 >= z0) { i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 0; k2 = 1; } // X Z Y order
                  else { i1 = 0; j1 = 0; k1 = 1; i2 = 1; j2 = 0; k2 = 1; } // Z X Y order
            } else { // x0<y0
                if (y0 < z0) { i1 = 0; j1 = 0; k1 = 1; i2 = 0; j2 = 1; k2 = 1; } // Z Y X order
                else if (x0 < z0) { i1 = 0; j1 = 1; k1 = 0; i2 = 0; j2 = 1; k2 = 1; } // Y Z X order
                else { i1 = 0; j1 = 1; k1 = 0; i2 = 1; j2 = 1; k2 = 0; } // Y X Z order
            }
            // A step of (1,0,0) in (i,j,k) means a step of (1-c,-c,-c) in (x,y,z),
            // a step of (0,1,0) in (i,j,k) means a step of (-c,1-c,-c) in (x,y,z), and
            // a step of (0,0,1) in (i,j,k) means a step of (-c,-c,1-c) in (x,y,z), where
            // c = 1/6.
            var x1 = x0 - i1 + G3; // Offsets for second corner in (x,y,z) coords
            var y1 = y0 - j1 + G3;
            var z1 = z0 - k1 + G3;
            var x2 = x0 - i2 + 2.0 * G3; // Offsets for third corner in (x,y,z) coords
            var y2 = y0 - j2 + 2.0 * G3;
            var z2 = z0 - k2 + 2.0 * G3;
            var x3 = x0 - 1.0 + 3.0 * G3; // Offsets for last corner in (x,y,z) coords
            var y3 = y0 - 1.0 + 3.0 * G3;
            var z3 = z0 - 1.0 + 3.0 * G3;
            // Work out the hashed gradient indices of the four simplex corners
            var ii = (int)i & 255;
            var jj = (int)j & 255;
            var kk = (int)k & 255;
            var gi0 = perm[ii + perm[jj + perm[kk]]] % 12;
            var gi1 = perm[ii + i1 + perm[jj + j1 + perm[kk + k1]]] % 12;
            var gi2 = perm[ii + i2 + perm[jj + j2 + perm[kk + k2]]] % 12;
            var gi3 = perm[ii + 1 + perm[jj + 1 + perm[kk + 1]]] % 12;
            // Calculate the contribution from the four corners
            var t0 = 0.6 - x0 * x0 - y0 * y0 - z0 * z0;
            if (t0 < 0) n0 = 0.0;
            else {
                t0 *= t0;
                n0 = t0 * t0 * Dot(grad3[gi0], x0, y0, z0);
            }
            var t1 = 0.6 - x1 * x1 - y1 * y1 - z1 * z1;
            if (t1 < 0) n1 = 0.0;
            else {
                t1 *= t1;
                n1 = t1 * t1 * Dot(grad3[gi1], x1, y1, z1);
            }
            var t2 = 0.6 - x2 * x2 - y2 * y2 - z2 * z2;
            if (t2 < 0) n2 = 0.0;
            else {
                t2 *= t2;
                n2 = t2 * t2 * Dot(grad3[gi2], x2, y2, z2);
            }
            var t3 = 0.6 - x3 * x3 - y3 * y3 - z3 * z3;
            if (t3 < 0) n3 = 0.0;
            else {
                t3 *= t3;
                n3 = t3 * t3 * Dot(grad3[gi3], x3, y3, z3);
            }
            // Add contributions from each corner to get the final noise value.
            // The result is scaled to stay just inside [-1,1]
            return 32.0 * (n0 + n1 + n2 + n3);
        }
    }
}
