using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedBot.Viewer.Model
{
    public class ModelTextureManager : IDisposable
    {
        private Dictionary<string, Rectangle> map = new Dictionary<string, Rectangle>(StringComparer.OrdinalIgnoreCase);
        private ZipArchive zip;

        public int TextureID { get; private set; } = 0;
        private Bitmap bmp;
        private Graphics g;

        public int Width { get; }
        public int Height { get; }

        private int texSize;

        public ModelTextureManager(ZipArchive zip)
        {
            this.zip = zip;

            using (var stream = zip.GetEntry("assets/minecraft/textures/blocks/stone.png").Open())
            using (Bitmap stone = new Bitmap(stream)) {
                if (stone.Width != stone.Height) {
                    throw new ArgumentException("Texture aspect ratio must be 1:1");
                }
                texSize = stone.Width;
            }
            int numTex = zip.Entries.Count(a => a.FullName.StartsWith(@"assets/minecraft/textures/blocks/") &&
                                           a.FullName.EndsWith(".png")) + 1;

            int needed = (int)(Math.Sqrt(numTex) + 0.5) * 16;
            int pow = (int)(Math.Pow(2, Math.Ceiling(Math.Log(needed, 2))));
            bmp = new Bitmap(pow, pow);
            g = Graphics.FromImage(bmp);

            Width = bmp.Width;
            Height = bmp.Height;
        }

        public Rectangle GetTexture(string name)
        {
            EnsureTextureCreated();

            if (!map.TryGetValue(name, out var rect)) {
                var entry = zip.GetEntry($"assets/minecraft/textures/{name}.png");
                if (entry == null) {
                    return map["missingno"];
                }
                using (var stream = entry.Open())
                using (var tex = new Bitmap(stream)) {
                    int pos = map.Count;

                    int wMod = Width / texSize;
                    int posX = (pos % wMod) * texSize;
                    int posY = (pos / wMod) * texSize;

                    if (tex.Width != tex.Height) {
                        g.DrawImage(tex, new Rectangle(posX, posY, texSize, texSize), new Rectangle(0, 0, texSize, texSize), GraphicsUnit.Pixel);
                    } else {
                        g.DrawImage(tex, posX, posY, texSize, texSize);
                    }

                    rect = new Rectangle(posX, posY, texSize, texSize);

                    UpdateTexture(posX, posY, texSize, texSize);
                }
                map[name] = rect;
            }
            return rect;
        }

        private void EnsureTextureCreated()
        {
            if (TextureID == 0) {
                TextureID = GL.glCreateTexture();

                GL.glBindTexture(GL.GL_TEXTURE_2D, TextureID);
                GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_S, GL.GL_REPEAT);
                GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_T, GL.GL_REPEAT);

                GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MAG_FILTER, GL.GL_NEAREST);
                GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MIN_FILTER, GL.GL_NEAREST);
                
                int hs = texSize / 2;
                g.FillRectangle(Brushes.Fuchsia, 0,  0, hs, hs);
                g.FillRectangle(Brushes.Black,   hs, 0, hs, hs);
                g.FillRectangle(Brushes.Black,   0,  hs, hs, hs);
                g.FillRectangle(Brushes.Fuchsia, hs, hs, hs, hs);
                map["missingno"] = new Rectangle(0, 0, texSize, texSize);

                BitmapData data = bmp.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                try {
                    GL.glTexImage2D(GL.GL_TEXTURE_2D, 0, GL.GL_RGBA8, data.Width, data.Height, 0, GL.GL_BGRA, GL.GL_UNSIGNED_BYTE, data.Scan0);
                } finally {
                    bmp.UnlockBits(data);
                }
            }
        }
        private void UpdateTexture(int x, int y, int w, int h)
        {
            BitmapData data = bmp.LockBits(new Rectangle(x, y, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try {
                GL.glBindTexture(GL.GL_TEXTURE_2D, TextureID);
                GL.glPixelStorei(GL.GL_UNPACK_ROW_LENGTH, data.Stride / 4);
                GL.glTexSubImage2D(GL.GL_TEXTURE_2D, 0, x, y, w, h, GL.GL_BGRA, GL.GL_UNSIGNED_BYTE, data.Scan0);
            } finally {
                GL.glPixelStorei(GL.GL_UNPACK_ROW_LENGTH, 0);
                bmp.UnlockBits(data);
            }
            //bmp.Save("model_blocks.png");
        }

        public void Dispose()
        {
            if (TextureID != 0) {
                GL.glDeleteTextures(1, new[] { TextureID });
                TextureID = 0;
            }
            g.Dispose();
            bmp.Dispose();
        }
    }
}
