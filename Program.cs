using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AdvancedBot.client;
using AdvancedBot.client.Map;
using AdvancedBot.client.NBT;
using AdvancedBot.Plugins;
using AdvancedBot.Viewer;

namespace AdvancedBot
{
    public static class Program
    {
        public const string AppVersion = "v2.7.5.1";

        public static PluginManager pluginManager;
        public static Main FrmMain;
        public static CompoundTag Config;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
      {
            if (Debugger.IsAttached) {
                new TestServer().Run();
            }

            AppDomain.CurrentDomain.UnhandledException += (s, e) => {
                Program.CreateErrLog((Exception)e.ExceptionObject, e.IsTerminating ? "crash" : "err");
            };
            Application.ThreadException += (s, e) => {
                Program.CreateErrLog(e.Exception, "threadexception");
            };
            
            int pa = 0;
            for (int i = 0, l = Math.Max(1, Environment.ProcessorCount - 1); i < l; i++)
                pa |= (1 << i);
            Process.GetCurrentProcess().ProcessorAffinity = new IntPtr(pa);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            LoadConf();
            FrmMain = new Main();
            

            //LaunchViewerTest();
            pluginManager = new PluginManager();
            //PacketLogger.Init();
            Application.Run(FrmMain);
            //Application.Run(new MacroEditor());
        }
        
#region Extensions
        public static bool ParseIP(this string addr, out string ip, ref ushort port)
        {
            int sidx = addr.IndexOf(':');

            if (sidx != -1) {
                ip = addr.Substring(0, sidx);
                if (!ushort.TryParse(addr.Substring(sidx + 1), out port)) {
                    return false;
                }
            } else {
                ip = addr;
            }
            return true;
        }
        public static void SplitColon(this string s, out string a, ref string b)
        {
            int i = s.IndexOf(':');

            if (i != -1) {
                a = s.Substring(0, i);
                b = s.Substring(i + 1);
            } else {
                a = s;
            }
        }
        public static string[] Lines(this string str)
        {
            return str.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static void SetExplorerTheme(this ListView lv)
        {
            typeof(ListView).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(lv, true, null);
            SetWindowTheme(lv.Handle, "explorer", null);
        }

        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);

        public static string HttpGet(this string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Proxy = null;
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            request.KeepAlive = true;
            request.ReadWriteTimeout = 10000;
            request.Timeout = 10000;
            using (WebResponse resp = request.GetResponse()) {
                using (StreamReader sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8)) {
                    return sr.ReadToEnd();
                }
            }
        }
#endregion

        private static DateTime appDate = new DateTime(0);
        public static DateTime GetBuildDate()
        {
            //https://stackoverflow.com/a/44511677

            if (appDate.Ticks != 0) return appDate;

            IntPtr hInstance = Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().ManifestModule);
            int timestamp = Marshal.ReadInt32(hInstance, Marshal.ReadInt32(hInstance, 60) + 8);

            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return appDate = TimeZoneInfo.ConvertTimeFromUtc(epoch.AddSeconds(timestamp), TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"));
        }

        public static MinecraftClient getBot(String nick)
        {
            foreach(MinecraftClient c in FrmMain.Clients)
            {
                if (c.Username.EqualsIgnoreCase(nick))
                {
                    return c;
                }
            }
            return null;
        }

        public static void SaveConf()
        {
            Config.AddBoolean("Knockback", MinecraftClient.Knockback);
            Config.AddBoolean("ViewerTextures", ViewerConfig.UseTexture);
            Config.AddBoolean("ViewerRenderSignText", ViewerConfig.RenderSigns);
            Config.AddBoolean("AutoReconnect", MinecraftClient.AutoReconnect);
            Config.AddBoolean("ViewerUseVBO", ViewerConfig.UseVBO);
            Config.AddBoolean("ViewerUseMipmap", ViewerConfig.UseMipMap);
            Config.AddInt("ViewerFpsLimit", ViewerConfig.MaxFps);
            Config.AddInt("ChatMaxLines", MinecraftClient.MaximumChatLines);

            try {
                NbtIO.Write(Config, "conf.dat");
            } catch { Debug.WriteLine("Error while writing new config file."); }
        }
        public static void LoadConf()
        {
            try {
                Config = NbtIO.Read("conf.dat");
            } catch {
                Config = new CompoundTag(); 
                Debug.WriteLine("Error while loading config file.");
            }
            MinecraftClient.Knockback = Config.GetBoolOrTrue("Knockback");
            ViewerConfig.UseTexture = Config.GetBoolOrTrue("ViewerTextures");
            ViewerConfig.RenderSigns = Config.GetBoolean("ViewerRenderSignText");
            MinecraftClient.AutoReconnect = Config.GetBoolean("AutoReconnect");
            ViewerConfig.UseVBO = Config.GetBoolOrTrue("ViewerUseVBO");
            ViewerConfig.UseMipMap = Config.GetBoolOrTrue("ViewerUseMipmap");
            ViewerConfig.MaxFps = Config.GetIntOrDefault("ViewerFpsLimit", 40);
            MinecraftClient.MaximumChatLines = Config.GetIntOrDefault("ChatMaxLines", 150);
        }

#region Config
        public static Tag GetConfigEntry(string path)
        {
            CompoundTag tag = Config;
            int n = 0;
            for (int pos = 0; (pos = path.IndexOf('.', n)) != -1; ) {
                string key = path.Substring(n, pos - n);
                tag = tag.GetCompound(key);
                n = pos + 1;
            }
            return tag.Get(path.Substring(n));
        }
        public static bool ConfigEntryExists(string path)
        {
            CompoundTag tag = Config;
            int n = 0;
            for (int pos = 0; (pos = path.IndexOf('.', n)) != -1; ) {
                string key = path.Substring(n, pos - n);
                tag = tag.GetCompound(key);
                n = pos + 1;
            }
            return tag.Contains(path.Substring(n));
        }
        public static void SetConfigEntry(string path, Tag val)
        {
            CompoundTag tag = Config;
            int n = 0;
            for (int pos = 0; (pos = path.IndexOf('.', n)) != -1; ) {
                string key = path.Substring(n, pos - n);
                CompoundTag entry = tag.GetCompound(key);
                if (entry.IsEmpty()) {
                    tag.AddCompound(key, entry);
                }
                tag = entry;
                
                n = pos + 1;
            }
            tag.Add(path.Substring(n), val);
        }

        public static T GetConfigEntry<T>(string path)
        {
            throw new NotImplementedException();
        }
        public static void SetConfigEntry<T>(string path, T val)
        {
            throw new NotImplementedException();
        }
#endregion

        private static object errLogLock = new object();
        private static string lastErrStr;
        public static void CreateErrLog(Exception ex, string type)
        {
            lock (errLogLock) {
                var stacktrace = ex.ToString();

                if (stacktrace == lastErrStr) {
                    return;
                }
                lastErrStr = stacktrace;

                var rawName = string.Format("errlogs\\{0}_{1:dd-MM-yyyy_HH.mm.ss}", type, DateTime.Now);

                int i = 0;
                if (!Directory.Exists("errlogs"))
                    Directory.CreateDirectory("errlogs");

                for (; File.Exists(rawName + "_" + i + ".txt"); i++) ;

                File.WriteAllText(rawName + "_" + i + ".txt",
                                  "--------AdvancedBot " + Program.AppVersion + " built on: " + GetBuildDate() +
                                  "--------\r\nStacktrace: \r\n\r\n" + stacktrace);
            }
        }
        
        private static void LaunchViewerTest()
        {
            MinecraftClient cli = new MinecraftClient("localhost", 25564, "ViewerTest", "123", null);
            cli.World = new World(cli);
            cli.Player = new Entity(cli);

            Random rng = new Random();
            int seed = 1234;

            float cubicNoise(float x, float y)
            {
                //https://github.com/jobtalle/CubicNoise/blob/master/c%23/CubicNoise.cs
                const int RND_A = 134775813;
                const int RND_B = 1103515245;

                const int octave = 12;

                int xi = (int)Math.Floor(x / octave);
                float lerpx = x / octave - xi;
                int yi = (int)Math.Floor(y / octave);
                float lerpy = y / octave - yi;

                float[] samples = new float[4];
                for (int i = 0; i < 4; ++i) {
                    samples[i] = interpolate(randomize(seed, xi - 1, yi - 1 + i),
                                             randomize(seed, xi,     yi - 1 + i),
                                             randomize(seed, xi + 1, yi - 1 + i),
                                             randomize(seed, xi + 2, yi - 1 + i), 
                                             lerpx);
                }
                float sample = interpolate(samples[0], samples[1], samples[2], samples[3], lerpy) * 0.5f + 0.25f;
                return Math.Max(0.0f, Math.Min(1.0f, sample * 0.5f + 0.5f));

                float randomize(int iSeed, int iX, int iY) {
                    return (float)((((iX ^ iY) * RND_A) ^ (iSeed + iX)) * (((RND_B * iX) << 16) ^ (RND_B * iY) - RND_A)) / int.MaxValue;
                }
                float interpolate(float a, float b, float c, float d, float t) {
                    float p = (d - c) - (a - b);
                    return t * (t * (t * p + ((a - b) - p)) + (c - a)) + b;
                }
            }

            int width = 10;
            int depth = 10;
            
            for (int x = 0; x < width; x++) {
                for (int z = 0; z < depth; z++) {
                    Chunk ch = new Chunk(x - (width / 2), z - (depth / 2));
                    int trees = 0;
                    for (int bx = 0; bx < 16; bx++) {
                        for (int bz = 0; bz < 16; bz++) {
                            float px = (x * 16.0f + bx);
                            float pz = (z * 16.0f + bz);

                            float h = cubicNoise(px, pz) * 16.0f;

                            px /= width * 16f;
                            pz /= depth * 16f;
                            float pxc = Math.Abs(px - 0.5f) * 4;
                            float pyc = Math.Abs(pz - 0.5f) * 4;

                            h = (4 - (float)Math.Sqrt(pxc * pxc + pyc * pyc)) * 16 + h;
                            //h = (float)Math.Sqrt(pxc * pxc + pyc * pyc) * 16 + h;

                            int height = (int)h;

                            for (int y = 0; y < height; y++) {
                                ch.SetBlock(bx, y, bz, (byte)(y < height - 6 ? Blocks.stone : Blocks.dirt));
                            }
                            ch.SetBlock(bx, 0, bz, Blocks.bedrock);
                            if (height < 40) {
                                for (int y = height; y <= 40; y++)
                                    ch.SetBlock(bx, y, bz, Blocks.water);
                                continue;
                            }
                            ch.SetBlock(bx, height, bz, Blocks.grass);

                            if (rng.Next(30) == 0) {
                                ch.SetBlock(bx, height + 1, bz, Blocks.tallgrass);
                                ch.SetData(bx, height + 1, bz, 1);
                            } else if (rng.Next(200) == 0 && bx > 3 && bz > 3 && bx < 13 && bz < 13 && trees++ < 5) {
                                int ys = height + 1;
                                int th = 5 + rng.Next(3);
                                for (int kz = -3; kz <= 3; kz++) {
                                    for (int kx = -3; kx <= 3; kx++) {
                                        for (int by = 0; by < th; by++) {
                                            int ky = by - 4;
                                            if (by >= 3 && Math.Sqrt(kx * kx + ky * ky + kz * kz) < 3 && rng.Next(10) != 0) {
                                                ch.SetBlock(bx + kx, ys + by, bz + kz, Blocks.leaves);
                                            }
                                            if(by < th - 1) ch.SetBlock(bx, ys + by, bz, Blocks.log);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    cli.World.SetChunk(ch.X, ch.Z, ch);
                }
            }

            cli.World.SetBlockAndData(0, 75, 0, Blocks.wall_sign, 2);
            cli.World.Signs.TryAdd(new Vec3i(0, 75, 0), new string[] {
                "Test", "Abcdefghiklmn", "jopqrstuvwxyz", "123456789"
            });
            for (int i = 0; i < 9; i++) {
                cli.Inventory.Slots[36 + i] = new ItemStack(Items.apple, 0, (byte)(1 + i));
            }
            cli.Inventory.Slots[36] = new ItemStack(Items.diamond_pickaxe, 0, 1);
            cli.Inventory.Slots[37] = new ItemStack(Items.iron_pickaxe, 0, 1);
            cli.Inventory.Slots[38] = new ItemStack(Items.wooden_pickaxe, 0, 1);
            cli.Inventory.Slots[39] = new ItemStack(Blocks.glass, 0, 1);
            cli.Inventory.Slots[40] = new ItemStack(Items.skull, 3, 64);

            string[] tags = {
                "H4sIAMPhglwA/+NiYGBm4PTITEl1y0lML2ZgYLDnYuBzyslPznbNK8ksqQxJTOdk4AhILClJLcor5gIqYOVgYIfyGZiSSpgZWJ3zc/KLGEAASYo5qaQYSY4fWY4ppxiXNqYi3FJJ+SgGMjOwOCUWp4LZDAAGItelyQAAAA==",
                "H4sIAMPhglwA/+NiYGBm4PTITEl1y0lML2ZgYLDnYuBzyslPznbNK8ksqQxJTOdk4AhILClJLcor5gIqYOVgYIfyGZhyipkZWJ3zc/KLGEAAWSojA0mKH0UqCbeuItxSSfkoBjIzsDglFqeC2QwAGk8eEcgAAAA=",
                "H4sIAMPhglwA/+NiYGBm4PTITEl1y0lML2ZgYLDnYuBzyslPznbNK8ksqQxJTOdk4AhILClJLcor5gIqYOZgYIfyGZhKipkZWJ3zc/KLGEAAWSoZt1RSPpIUP9AFLE6JxalgNgMAAYErr5IAAAA=",
                "H4sIAMPhglwA/+NiYGBm4PTITEl1y0lML2ZgYLDnYuBzyslPznbNK8ksqQxJTOdk4AhILClJLcor5gIqYOFgYIfyGZhKipkZWJ3zc/KLGEAAWSoJpxRzSg4ebflIUvxA17E4JRangtkMAKr90WquAAAA"
            };
            for (int i = 0; i < 4; i++) {
                var tag = NbtIO.ReadCompressed(new MemoryStream(Convert.FromBase64String(tags[i])));
                cli.Inventory.Slots[31 + i] = new ItemStack(Items.banner, 3, 64, tag);
            }
            cli.Inventory.Slots[30] = new ItemStack(Items.skull, 3, 64);

            cli.Version = ClientVersion.v1_8;
            cli.Player.SetPosition(0, 80, 0);

            ViewForm vf = new ViewForm(cli);
            
            Control.CheckForIllegalCrossThreadCalls = false;
            Application.Run(vf);
        }
    }
}
