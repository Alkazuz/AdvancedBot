using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AdvancedBot.client;
using AdvancedBot.client.NBT;
using AdvancedBot.Viewer.Character;

namespace AdvancedBot.Viewer.Gui.ItemRenderer
{
    public class BannerRenderer : ItemRendererBase
    {
        private Dictionary<string, Texture> textures = new Dictionary<string, Texture>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, FastBitmap> patternTextures = new Dictionary<string, FastBitmap>();
        private DateTime lastCacheCleanup = DateTime.UtcNow;

        private static readonly IReadOnlyList<Cube> BannerModel = new List<Cube>() {
            new Cube(0,  0, 64, 64).AddBox(-10f,  0f, -2f, 20, 40, 1), //Slate
            new Cube(44, 0, 64, 64).AddBox(-1f,   0f, -1f,  2, 42, 2), //Stand
            new Cube(0, 42, 64, 64).AddBox(-10f,  0f, -1f, 20,  2, 2)  //Top
        };

        public BannerRenderer()
        {
            using (var zip = new ZipArchive(new MemoryStream(Properties.Resources.BannerPatterns))) {
                foreach (var entry in zip.Entries) {
                    //MS says that the stream must be keept open as long as the Bitmap lifetime.
                    using (var stream = entry.Open()) {
                        byte[] buf = new byte[entry.Length];
                        for (int rem = buf.Length; rem > 0;) {
                            rem -= stream.Read(buf, buf.Length - rem, rem);
                        }
                        string name = Path.GetFileNameWithoutExtension(entry.Name);
                        patternTextures[name] = new FastBitmap(new Bitmap(new MemoryStream(buf)), true, true);
                    }
                }
            }
        }

        public override void Render(ItemStack stack, int x, int y)
        {
            var tag = stack.NBTData?.GetCompound("BlockEntityTag") ?? new CompoundTag();

            var texId = GetPatternTextureId(tag);

            GL.glEnable(GL.GL_TEXTURE_2D);
            GL.glBindTexture(GL.GL_TEXTURE_2D, texId);

            DrawBannerOnGUI(x, y, 2);

            var frm = ViewForm.OpenForm;
            var cur = frm.PointToClient(Cursor.Position);
            if (GuiUtils.PointInRect(x, y, 32, 32, cur.X, cur.Y) && InputHelper.IsKeyDown(Keys.LControlKey) && frm.HasFocus) {
                const int SIZE = 96;

                //most top item
                GL.glTranslated(0, 0, 250);

                int sx = cur.X + 12;
                int sy = cur.Y + 2;
                //background
                GL.glDisable(GL.GL_TEXTURE_2D);
                GL.glColor4f(1, 1, 1, 1);
                GuiUtils.DrawRectangle(sx, sy, SIZE, SIZE);
                GL.glEnable(GL.GL_TEXTURE_2D);

                GuiUtils.DrawTexturedRect(sx, sy, SIZE, SIZE, 42, 42, 0, 0, 64, 64);

                GL.glTranslated(0, 0, -250);
            }
        }
        private void DrawBannerOnGUI(int x, int y, float scale)
        {
            GL.glPushMatrix();
            GL.glTranslated(x + 8 * scale, y + 2 * scale, 16);
            GL.glScalef(scale, -scale, 1);
            GL.glRotatef(210f, 1, 0, 0);
            GL.glRotatef(32, 0, 1, 0);

            const float S = 0.35f;
            GL.glScalef(S, S, S);
            foreach (var cube in BannerModel) {
                cube.RenderNoTransform();
            }

            GL.glColor3f(1f, 1f, 1f);
            GL.glPopMatrix();
        }

        public override void Tick()
        {
            var now = DateTime.UtcNow;
            if ((now - lastCacheCleanup).TotalSeconds >= 5) {
                var toRemove = new List<string>();
                foreach (var kv in textures) {
                    var tex = kv.Value; 
                    if (tex.Cacheable && (now - tex.LastUsed).TotalSeconds >= 15) {
                        GL.glDeleteTexture(tex.Id);
                        toRemove.Add(kv.Key);
                    }
                }
                foreach (var key in toRemove) {
                    textures.Remove(key);
                }
                
                lastCacheCleanup = now;
            }
        }
        public override void Dispose()
        {
            foreach (var tex in textures.Values) {
                GL.glDeleteTexture(tex.Id);
            }
            foreach (var tex in patternTextures.Values) {
                tex.Dispose();
            }
            textures.Clear();
            patternTextures.Clear();
        }

        public FastBitmap ComposePattern(CompoundTag tag)
        {
            var patterns = tag.GetList("Patterns");
            int baseColor = GetDyeColor(tag.GetInt("Base"));

            var bannerBase = patternTextures["banner_base"];

            int w = bannerBase.Width;
            int h = bannerBase.Height;

            var composed = new FastBitmap(bannerBase.Clone(), true, true);

            for (int i = 0; i < Math.Min(16, patterns.Count); i++) {
                var pat = (CompoundTag)patterns[i];

                var color = new Pixel(GetDyeColor(pat.GetInt("Color")));

                if (!Patterns.TryGetValue(pat.GetString("Pattern"), out var patName)) {
                    continue;
                }

                var pattern = patternTextures[patName];

                for (int y = 0; y < h; y++) {
                    for (int x = 0; x < w; x++) {
                        Pixel c1 = pattern.GetPixel(x, y);

                        if (c1.A != 0) {
                            int a = c1.R << 24;
                            int rgb = MultiplyColors(bannerBase.GetPixel(x, y), color) & 0xFFFFFF;
                            composed.SetPixel(x, y, AlphaBlend(composed.GetPixel(x, y), a | rgb));
                        }
                    }
                }
            }

            return composed;
        }

        private int GetPatternTextureId(CompoundTag tag)
        {
            var key = GetPatternKey(tag);
            if (textures.TryGetValue(key, out Texture tex)) {
                tex.LastUsed = DateTime.UtcNow;
                return tex.Id;
            }

            using (FastBitmap bmp = ComposePattern(tag)) {
                textures[key] = tex = new Texture(bmp.CreateTexture(), true);
            }
            return tex.Id;
        }
        private string GetPatternKey(CompoundTag tag)
        {
            var sb = new StringBuilder(64);
            sb.Append(tag.GetInt("Base"));

            var patterns = tag.GetList("Patterns");
            for (int i = 0; i < Math.Min(16, patterns.Count); i++) {
                var pat = (CompoundTag)patterns[i];

                sb.Append(pat.GetString("Pattern"));
                sb.Append(pat.GetInt("Color"));
            }
            return sb.ToString();
        }

        private static int GetDyeColor(int damage)
        {
            switch (damage) {
                case 15: return 0xFFFFFF; //White
                case 14: return 0xD87F33; //Orange
                case 13: return 0xB24CD8; //Magenta
                case 12: return 0x6699D8; //Light blue
                case 11: return 0xE5E533; //Yellow
                case 10: return 0x7FCC19; //Lime
                case 9: return 0xF27FA5; //Pink
                case 8: return 0x4C4C4C; //Gray
                case 7: return 0x999999; //Silver
                case 6: return 0x4C7F99; //Cyan
                case 5: return 0x7F3FB2; //Purple
                case 4: return 0x334CB2; //Blue
                case 3: return 0x664C33; //Brown
                case 2: return 0x667F33; //Green
                case 1: return 0x993333; //Red
                default:
                case 0: return 0x000000; //Black
            }
        }

        private static Pixel AlphaBlend(Pixel bg, Pixel fg)
        {
            int fgA = fg.A;
            int fgNA = 255 - fgA;
            int r = (fg.R * fgA + bg.R * fgNA) >> 8;
            int g = (fg.G * fgA + bg.G * fgNA) >> 8;
            int b = (fg.B * fgA + bg.B * fgNA) >> 8;

            return new Pixel((byte)bg.A, (byte)r, (byte)g, (byte)b);
        }
        private static int MultiplyColors(Pixel c1, Pixel c2)
        {
            int r = (int)(c1.R * (float)c2.R / 255f);
            int g = (int)(c1.G * (float)c2.G / 255f);
            int b = (int)(c1.B * (float)c2.B / 255f);
            return c1.A << 24 | r << 16 | g << 8 | b;
        }

        private static readonly IReadOnlyDictionary<string, string> Patterns = new Dictionary<string, string>() {
            { "b", "base" },
            { "bl", "square_bottom_left" },
            { "br", "square_bottom_right" },
            { "tl", "square_top_left" },
            { "tr", "square_top_right" },
            { "bs", "stripe_bottom" },
            { "ts", "stripe_top" },
            { "ls", "stripe_left" },
            { "rs", "stripe_right" },
            { "cs", "stripe_center" },
            { "ms", "stripe_middle" },
            { "drs", "stripe_downright" },
            { "dls", "stripe_downleft" },
            { "ss", "small_stripes" },
            { "cr", "cross" },
            { "sc", "straight_cross" },
            { "bt", "triangle_bottom" },
            { "tt", "triangle_top" },
            { "bts", "triangles_bottom" },
            { "tts", "triangles_top" },
            { "ld", "diagonal_left" },
            { "rd", "diagonal_up_right" },
            { "lud", "diagonal_up_left" },
            { "rud", "diagonal_right" },
            { "mc", "circle" },
            { "mr", "rhombus" },
            { "vh", "half_vertical" },
            { "hh", "half_horizontal" },
            { "vhr", "half_vertical_right" },
            { "hhb", "half_horizontal_bottom" },
            { "bo", "border" },
            { "cbo", "curly_border" },
            { "cre", "creeper" },
            { "gra", "gradient" },
            { "gru", "gradient_up" },
            { "bri", "bricks" },
            { "sku", "skull" },
            { "flo", "flower" },
            { "moj", "mojang" }
        };
        
        private class Texture
        {
            public bool Cacheable { get; }
            public DateTime LastUsed { get; set; }
            public int Id { get; }

            public Texture(int id, bool cacheable)
            {
                Cacheable = cacheable;
                LastUsed = DateTime.UtcNow;
                Id = id;
            }
        }
    }
}
