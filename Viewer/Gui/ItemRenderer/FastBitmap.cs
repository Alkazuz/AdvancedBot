using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedBot.Viewer.Gui.ItemRenderer
{
    public unsafe class FastBitmap : IDisposable
    {
        public Bitmap Bitmap { get; }
        public int Width { get; }
        public int Height { get; }

        public bool IsLocked { get; private set; }
        public Pixel* Pixels { get; private set; }
        public int Stride { get; private set; }

        private BitmapData data;

        private bool disposeBitmap;

        public FastBitmap(Bitmap bmp, bool lockPixels = true, bool disposeBmp = false)
        {
            Bitmap = bmp;
            Width = bmp.Width;
            Height = bmp.Height;
            disposeBitmap = disposeBmp;
            if (lockPixels) {
                Lock();
            }
        }
        public void Lock()
        {
            if (!IsLocked) {
                data = Bitmap.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                Pixels = (Pixel*)data.Scan0;
                Stride = data.Stride / 4;
                IsLocked = true;
            }
        }
        public void Unlock()
        {
            if (IsLocked) {
                Pixels = null;
                Stride = 0;
                Bitmap.UnlockBits(data);
                IsLocked = false;
            }
        }

        public Pixel GetPixel(int x, int y)
        {
            return Pixels[x + y * Stride];
        }
        public void SetPixel(int x, int y, Pixel color)
        {
            Pixels[x + y * Stride] = color;
        }

        public Pixel GetPixelSafe(int x, int y)
        {
            if (x >= 0 && y >= 0 && x < Width && y < Height) {
                return Pixels[x + y * Stride];
            }
            return new Pixel();
        }
        public void SetPixelSafe(int x, int y, Pixel color)
        {
            if (x >= 0 && y >= 0 && x < Width && y < Height) {
                Pixels[x + y * Stride] = color;
            }
        }

        public int CreateTexture(bool linear = false)
        {
            bool locked = IsLocked;

            if (!locked) Lock();
            try {
                int tex = GL.glCreateTexture();

                GL.glBindTexture(GL.GL_TEXTURE_2D, tex);
                GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_S, GL.GL_REPEAT);
                GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_T, GL.GL_REPEAT);

                int filter = linear ? GL.GL_LINEAR : GL.GL_NEAREST;
                GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MAG_FILTER, filter);
                GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MIN_FILTER, filter);
                GL.glTexImage2D(GL.GL_TEXTURE_2D, 0, GL.GL_RGBA8, Width, Height, 0, GL.GL_BGRA, GL.GL_UNSIGNED_BYTE, (IntPtr)Pixels);

                return tex;
            } finally {
                if (!locked) Unlock();
            }
        }

        public Bitmap Clone()
        {
            bool locked = IsLocked;
            if (!locked) Lock();
            try {
                var bmp = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
                int bytes = Width * Stride * 4;
                using (var fbmp = new FastBitmap(bmp, true, false)) {
                    Buffer.MemoryCopy(Pixels, fbmp.Pixels, bytes, bytes);
                }
                return bmp;
            } finally {
                if (!locked) Unlock();
            }
        }

        public void Dispose()
        {
            Unlock();
            if (disposeBitmap) {
                Bitmap.Dispose();
            }
        }
    }
    public struct Pixel
    {
        public byte B, G, R, A;

        public Pixel(int argb)
        {
            A = (byte)(argb >> 24);
            R = (byte)(argb >> 16);
            G = (byte)(argb >> 8);
            B = (byte)(argb >> 0);
        }
        public Pixel(byte a, byte r, byte g, byte b)
        {
            A = a;
            R = r;
            G = g;
            B = b;
        }
        public Pixel(int a, int r, int g, int b)
        {
            A = (byte)(a < 0 ? 0 : a > 255 ? 255 : a);
            R = (byte)(r < 0 ? 0 : r > 255 ? 255 : r);
            G = (byte)(g < 0 ? 0 : g > 255 ? 255 : g);
            B = (byte)(b < 0 ? 0 : b > 255 ? 255 : b);
        }

        public static implicit operator Pixel(int argb) => new Pixel(argb);
        public static implicit operator int(Pixel p) => p.A << 24 | p.R << 16 | p.G << 8 | p.B;
    }
}
