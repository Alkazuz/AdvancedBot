using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Ionic.Zlib;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using AdvancedBot.Viewer.Gui;

namespace AdvancedBot.Viewer
{
    public class TextureManager
    {
        private static Dictionary<ushort, Rectangle> textureMap = new Dictionary<ushort, Rectangle>();
        private static Dictionary<int, ItemInfo> itemMap = new Dictionary<int, ItemInfo>();
        private struct ItemInfo
        {
            public int TextureX, TextureY;
            public int TextureWidth, TextureHeight;
            public string ItemName;

            public ItemInfo(int x, int y, int w, int h, string name)
            {
                TextureX = x;
                TextureY = y;
                TextureWidth = w;
                TextureHeight = h;
                ItemName = name;
            }
        }

        //private static readonly Rectangle MISSING_BLOCK_TEX = new Rectangle(400, 288, 16, 16);
        private static readonly Rectangle MISSING_BLOCK_TEX = new Rectangle(64, 224, 16, 16);
        private static readonly Rectangle MISSING_ITEM_TEX = new Rectangle(48, 144, 16, 16);
        public static void LoadTextureMap()
        {
            if (textureMap.Count == 0)
            {
                string[] mapLines = Encoding.UTF8.GetString(ZlibStream.UncompressBuffer(AdvancedBot.Properties.Resources.texturemap_csv)).Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                //blockName,blockId,blockData,textureX,textureY,textureWidth,textureHeight,blockFace
                for (int i = 1; i < mapLines.Length; i++)
                {
                    string[] parts = mapLines[i].Split(',');
                    string name = parts[0];
                    int blockId = int.Parse(parts[1]);
                    int blockData = int.Parse(parts[2]);
                    int tx = int.Parse(parts[3]);
                    int ty = int.Parse(parts[4]);
                    int tw = int.Parse(parts[5]);
                    int th = int.Parse(parts[6]);
                    int face = int.Parse(parts[7]);

                    int key = blockId << 7 | (blockData & 0xF) << 3 | (face & 0x7);
                    textureMap[(ushort)key] = new Rectangle(tx, ty, tw, th);
                }
                /*string[] mapLines = Encoding.UTF8.GetString(ZlibStream.UncompressBuffer(Properties.Resources.blocks_csv)).Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 1; i < mapLines.Length; i++)
                {
                    string[] parts = mapLines[i].Split(',');

                    //resourceName,blockId,blockData,face,uMin,vMin,uMax,vMax
                    string resname = parts[0]; 
                    int blockId = int.Parse(parts[1]);
                    int blockData = int.Parse(parts[2]);
                    int face = int.Parse(parts[3]);

                    int uMin = (int)Math.Round(float.Parse(parts[4], System.Globalization.CultureInfo.InvariantCulture) * 512);
                    int vMin = (int)Math.Round(float.Parse(parts[5], System.Globalization.CultureInfo.InvariantCulture) * 512);
                    int uMax = (int)Math.Round(float.Parse(parts[6], System.Globalization.CultureInfo.InvariantCulture) * 512);
                    int vMax = (int)Math.Round(float.Parse(parts[7], System.Globalization.CultureInfo.InvariantCulture) * 512);
                    int key = blockId << 7 | (blockData & 0xF) << 3 | (face & 0x7);
                    textureMap[(ushort)key] = new Rectangle(uMin, vMin, uMax - uMin, vMax - vMin);
                }*/
            }
            if (itemMap.Count == 0)
            {
                string[] mapLines = Encoding.UTF8.GetString(ZlibStream.UncompressBuffer(AdvancedBot.Properties.Resources.itemmap_csv)).Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                //itemUnlocalizedName,itemId,itemDamage,textureX,textureY,textureWidth,textureHeight
                for (int i = 1; i < mapLines.Length; i++)
                {
                    string[] parts = mapLines[i].Split(',');
                    string name = parts[0];
                    int itemId = int.Parse(parts[1]);
                    int itemDamage = int.Parse(parts[2]);
                    int tx = int.Parse(parts[3]);
                    int ty = int.Parse(parts[4]);
                    int tw = int.Parse(parts[5]);
                    int th = int.Parse(parts[6]);

                    itemMap[itemId << 16 | itemDamage] = new ItemInfo(tx, ty, tw, th, name);
                }
                //itemId,itemDamage,textureX,textureY
                mapLines = AdvancedBot.Properties.Resources.item_blocks_textures.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 1; i < mapLines.Length; i++)
                {
                    string[] parts = mapLines[i].Split(',');
                    int itemId = int.Parse(parts[0]);
                    int itemDamage = int.Parse(parts[1]);
                    int tx = int.Parse(parts[2]);
                    int ty = int.Parse(parts[3]);
                    itemMap[itemId << 16 | itemDamage] = new ItemInfo(tx, ty, 32, 32, null);
                }
            }
        }

        public static Rectangle GetBlockTexture(int id, int meta, int face)
        {
            Rectangle rect;
            if (!textureMap.TryGetValue((ushort)(id << 7 | (meta & 0xF) << 3 | (face & 0x7)), out rect) &&
                !textureMap.TryGetValue((ushort)(id << 7 | (face & 0x7)), out rect) &&
                !textureMap.TryGetValue((ushort)(id << 7 | (meta & 0xF) << 3), out rect))
                return MISSING_BLOCK_TEX;

            return rect;
        }
        public static Rectangle GetItemTexture(int id, int meta)
        {
            int key = id << 16 | meta;
            ItemInfo info;
            if (itemMap.TryGetValue(key, out info))
                return new Rectangle(info.TextureX, info.TextureY, info.TextureWidth, info.TextureHeight);
            else if (itemMap.TryGetValue(id << 16, out info))
                return new Rectangle(info.TextureX, info.TextureY, info.TextureWidth, info.TextureHeight);
            return MISSING_ITEM_TEX;
        }

        private Dictionary<string, int> textures = new Dictionary<string, int>();

        public int Get(string name, bool canUseMipmap = false)
        {
            int tex = 0;
            if (!textures.TryGetValue(name, out tex)) {
                tex = CreateTexture((Bitmap)Properties.Resources.ResourceManager.GetObject(name), canUseMipmap && ViewerConfig.UseMipMap);
                textures[name] = tex;
            }
            return tex;
        }
        public void DeleteTextures()
        {
            int[] tex = textures.Values.ToArray();
            GL.glDeleteTextures(tex.Length, tex);
            textures.Clear();
        }
        public bool DeleteTexture(string name)
        {
            if (textures.TryGetValue(name, out int tex)) {
                GL.glDeleteTexture(tex);
                textures.Remove(name);
                return true;
            }
            return false;
        }

        public static int CreateTexture(Bitmap bmp, bool mipmap)
        {
            int[] t = new int[1];
            GL.glGenTextures(1, t);
            int tex = t[0];

            GL.glBindTexture(GL.GL_TEXTURE_2D, tex);
            GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_S, GL.GL_CLAMP);
            GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_T, GL.GL_CLAMP);
            GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MAG_FILTER, GL.GL_NEAREST);

            if (mipmap) {
                GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MIN_FILTER, GL.GL_NEAREST_MIPMAP_LINEAR);
                GL.glTexParameterf(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MIN_LOD, 0.0f);
                GL.glTexParameterf(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MAX_LOD, 4.0f);
                GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MAX_LEVEL, 4);
                //GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_GENERATE_MIPMAP, GL.GL_TRUE);
            } else {
                GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MIN_FILTER, GL.GL_NEAREST);
                //GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_GENERATE_MIPMAP, GL.GL_FALSE);
            }

            int w = bmp.Width;
            int h = bmp.Height;

            BitmapData data = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            GL.glTexImage2D(GL.GL_TEXTURE_2D, 0, GL.GL_RGBA8, w, h, 0, GL.GL_BGRA, GL.GL_UNSIGNED_BYTE, data.Scan0);

            if (mipmap) {
                UploadMipMaps(data);
            }

            bmp.UnlockBits(data);

            bmp.Dispose();
            return tex;
        }
        private static unsafe void UploadMipMaps(BitmapData data)
        {
            int w = data.Width;
            int h = data.Height;
            int[] pixels = new int[w * h];
            Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);

            for (int i = 1; i <= 4; i++) {
                int mipW = w / 2;
                int mipH = h / 2;

                int[] buf = new int[mipW * mipH];
                for (int mipY = 0; mipY < mipH; mipY++) {
                    for (int mipX = 0; mipX < mipW; mipX++) {
                        int tx = mipX * 2;
                        int ty = mipY * 2;
                        int p1 = pixels[(tx + 0) + (ty + 0) * w];
                        int p2 = pixels[(tx + 1) + (ty + 0) * w];
                        int p3 = pixels[(tx + 1) + (ty + 1) * w];
                        int p4 = pixels[(tx + 0) + (ty + 1) * w];
                        buf[mipX + mipY * mipW] = AlphaBlend(p1, p2, p3, p4);
                    }
                }

                fixed (int* pBuf = buf) {
                    GL.glTexImage2D(GL.GL_TEXTURE_2D, i, GL.GL_RGBA8, mipW, mipH, 0, GL.GL_BGRA, GL.GL_UNSIGNED_BYTE, (IntPtr)pBuf);
                }
                pixels = buf;
                w = mipW;
                h = mipH;
            }
        }

        private static int AlphaBlend(int c1, int c2, int c3, int c4)
        {
            return AlphaBlend(AlphaBlend(c1, c2), AlphaBlend(c3, c4));
        }
        private static int AlphaBlend(int c1, int c2)
        {
            //optifine.Mipmaps
            int a1 = c1 >> 24 & 0xFF;
            int a2 = c2 >> 24 & 0xFF;
            int ax = (a1 + a2) / 2;

            if (a1 == 0 && a2 == 0) {
                a1 = 1;
                a2 = 1;
            } else {
                if (a1 == 0) {
                    c1 = c2;
                    ax /= 2;
                }
                if (a2 == 0) {
                    c2 = c1;
                    ax /= 2;
                }
            }

            int r1 = (c1 >> 16 & 0xFF) * a1;
            int g1 = (c1 >> 8 & 0xFF) * a1;
            int b1 = (c1 & 0xFF) * a1;
            int r2 = (c2 >> 16 & 0xFF) * a2;
            int g2 = (c2 >> 8 & 0xFF) * a2;
            int b2 = (c2 & 0xFF) * a2;
            int rx = (r1 + r2) / (a1 + a2);
            int gx = (g1 + g2) / (a1 + a2);
            int bx = (b1 + b2) / (a1 + a2);
            return ax << 24 | rx << 16 | gx << 8 | bx;
        }

        public int CreateTexture(BitmapData data, bool linearInterp = false)
        {
            int[] t = new int[1];
            GL.glGenTextures(1, t);
            int tex = t[0];

            GL.glBindTexture(GL.GL_TEXTURE_2D, tex);
            GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_S, GL.GL_REPEAT);
            GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_T, GL.GL_REPEAT);

            int filter = linearInterp ? GL.GL_LINEAR : GL.GL_NEAREST;
            GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MAG_FILTER, filter);
            GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MIN_FILTER, filter);
            GL.glTexImage2D(GL.GL_TEXTURE_2D, 0, GL.GL_RGBA8, data.Width, data.Height, 0, GL.GL_BGRA, GL.GL_UNSIGNED_BYTE, data.Scan0);

            return tex;
        }
    }
}
