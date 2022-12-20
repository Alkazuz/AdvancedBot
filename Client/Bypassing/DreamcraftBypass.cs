using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Threading;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace AdvancedBot.client.Bypassing
{
    public class DreamcraftBypass : ServerBypassBase
    {
        public override ClientVersion Version => ClientVersion.v1_8;

        public static DreamcraftCaptchaForm CaptchaForm;
        private static bool isFormInitializing = false;
        public static Queue<CaptchaSolveRequest> Captchas = new Queue<CaptchaSolveRequest>();

        public class CaptchaSolveRequest
        {
            public Bitmap Image;
            public MinecraftClient Client;
            public DreamcraftBypass Bypasser;
        }

        public DreamcraftBypass(MinecraftClient client) : base(client)
        {
            client.OnTick += OnTick;
        }
        private int[] mapPixels;
        private long LastDataReceived;

        public override bool HandlePacket(ReadBuffer rb)
        {
            switch(rb.ID) {
                case 0x34: {
                    int mapId = rb.ReadVarInt();
                    int scale = rb.ReadSByte();
                    int iconCount = rb.ReadVarInt();
                    rb.Skip(iconCount * 3);//skip the icons

                    byte columns = rb.ReadByte();
                    if(columns > 0) {
                        int rows = rb.ReadByte();
                        int minX = rb.ReadByte();
                        int minY = rb.ReadByte();
                        byte[] pix = rb.ReadByteArray(rb.ReadVarInt());

                        LoadMap(minX, minY, columns, rows, pix);
                    }
                    return true;
                }
                case 0x02: { //chat
                    string chat = Utils.StripColorCodes(ChatParser.ParseJson(rb.ReadString()));
                    if(chat.ContainsIgnoreCase("Tudo certo, parece que você é um humano")) {
                        Client.OnTick -= OnTick;
                        IsFinished = true;
                    }
                    return true;
                }
                case 0x40: {
                    Client.OnTick -= OnTick;
                    IsFinished = true;
                    return false;
                }
                default: return false;
            }
        }
        private unsafe void LoadMap(int minX, int minY, int width, int height, byte[] pix)
        {
            LastDataReceived = Utils.GetTimestamp();
            Debug.WriteLine($"LoadMap x={minX} y={minY} w={width} h={height}");
            if (minX < 0 || minY < 0 || (minX + width) > 128 || (minY + height) > 128) {
                throw new ArgumentException();
            }

            if(mapPixels == null) {
                mapPixels = new int[128 * 128];
            }

            int[] mapPix = mapPixels;
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {

                    int dstIdx = (minX + x) + (minY + y) * 128;

                    int color = pix[x + y * width];
                    if (color / 4 == 0) {
                        mapPix[dstIdx] = (dstIdx + dstIdx / 128 & 0x01) * 8 + 16 << 24;
                    } else {
                        mapPix[dstIdx] = MapColor.GetVariant(color & 0x03, MapColor.Colors[color / 4]);
                    }
                }
            }

            //using (Bitmap bmp = new Bitmap(128, 128)) {
            //    BitmapData data = bmp.LockBits(new Rectangle(0, 0, 128, 128), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            //    Marshal.Copy(mapPix, 0, data.Scan0, mapPix.Length);
            //    bmp.UnlockBits(data);
            //    bmp.Save("map.png");
            //}
        }

        private bool isWaiting = false;
        private bool wasPrevConnected = false;
        private void OnTick()
        {
            bool connected = Client.IsBeingTicked();
            if(connected) {
                wasPrevConnected = true;
            }
            if(!connected && wasPrevConnected) {
                IsFinished = true;
                Client.OnTick -= OnTick;
                return;
            }

            if (LastDataReceived != 0 && Utils.GetTimestamp() - LastDataReceived > 1000 && !isWaiting) {
                isWaiting = true;
                Process();
            }
        }
        private void Process()
        {
            Bitmap treated = TreatMapImage();
            lock (Captchas) {
                Captchas.Enqueue(new CaptchaSolveRequest() {
                    Bypasser = this,
                    Image = treated,
                    Client = Client
                });
                if (CaptchaForm == null && !isFormInitializing) {
                    isFormInitializing = true;
                    Program.FrmMain.Invoke(new Action(() => {
                        CaptchaForm = new DreamcraftCaptchaForm();
                        CaptchaForm.Show();
                        isFormInitializing = false;
                    }));
                }
            }
        }

        public void SetCaptchaText(string text)
        {
            Client.SendMessage(text);
            isWaiting = false;
            LastDataReceived = Utils.GetTimestamp() + 2500;
        }
        private unsafe Bitmap TreatMapImage()
        {
            int AH = 78;
            int AW = 128;
            int AY = 128 - AH;

            Bitmap bmp = new Bitmap(AW, AH);
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, AW, AH), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            int* dst = (int*)data.Scan0;
            int[] src = mapPixels;
            int[] buf = new int[AW * AH];

            for (int y = 0; y < AH; y++) {
                for (int x = 0; x < AW - 1; x++) {
                    if ((x + y) % 2 == 1) {
                        buf[x + y * AW] = src[(x + 1) + (AY + y) * 128];
                    } else {
                        buf[x + y * AW] = src[x + (AY + y) * 128];
                    }
                }
            }
            GaussianBlur(buf, dst, AW, AH);

            bmp.UnlockBits(data);

            return bmp;
        }

        private static unsafe void GaussianBlur(int[] src, int* dst, int w, int h)
        {
            int[] Gauss = {
                1, 2, 1,
                2, 4, 2,
                1, 2, 1
            };
            
            for (int y = 1; y < h - 1; y++) {
                for (int x = 1; x < w - 1; x++) {
                    int p0 = src[x + y * w];

                    int r = p0 >> 16 & 0xFF;
                    int g = p0 >> 8 & 0xFF;
                    int b = p0 >> 0 & 0xFF;
                    int samples = 1;

                    for (int yo = -1; yo <= 1; yo++) {
                        for (int xo = -1; xo <= 1; xo++) {
                            int p1 = src[(x + xo) + (y + yo) * w];

                            int R = p1 >> 16 & 0xFF;
                            int G = p1 >> 8 & 0xFF;
                            int B = p1 >> 0 & 0xFF;
                            if (IsNonWhite(R, G, B)) {
                                int ga = Gauss[(xo + 1) + (yo + 1) * 3];

                                r += R * ga;
                                g += G * ga;
                                b += B * ga;
                                samples += ga;
                            }
                        }
                    }
                    dst[x + y * w] = 0xFF << 24 | (r / samples) << 16 | (g / samples) << 8 | (b / samples);
                }
            }
        }
        private static bool IsNonWhite(int r, int g, int b)
        {
            //0.299 * r + 0.587 * g + 0.114 * b
            int Y = (77 * r + 150 * g + 29 * b) >> 8;
            return Y < 245;
        }

        private struct Pixel
        {
            public byte B, G, R, A;

            public Pixel(int r, int g, int b)
            {
                R = (byte)(r < 0 ? 0 : r > 255 ? 255 : r);
                G = (byte)(g < 0 ? 0 : g > 255 ? 255 : g);
                B = (byte)(b < 0 ? 0 : b > 255 ? 255 : b);
                A = 255;
            }
        }

        private class MapColor
        {
            public static int[] Colors;

            static MapColor()
            {
                //foreach (Match match in Regex.Matches(str, @"MapColor (.+) = new MapColor\((\d+), (\d+)\)")) {
                //    var index = match.Groups[2].Value;

                //    int rgb = int.Parse(match.Groups[3].Value);
                //    Debug.WriteLine($"Colors[{index}] = 0xFF{rgb:X6}; //{match.Groups[1].Value}");
                //}

                Colors = new int[64];
                Colors[0] = 0x000000; //airColor
                Colors[1] = 0x7FB238; //grassColor
                Colors[2] = 0xF7E9A3; //sandColor
                Colors[3] = 0xA7A7A7; //clothColor
                Colors[4] = 0xFF0000; //tntColor
                Colors[5] = 0xA0A0FF; //iceColor
                Colors[6] = 0xA7A7A7; //ironColor
                Colors[7] = 0x007C00; //foliageColor
                Colors[8] = 0xFFFFFF; //snowColor
                Colors[9] = 0xA4A8B8; //clayColor
                Colors[10] = 0xB76A2F; //dirtColor
                Colors[11] = 0x707070; //stoneColor
                Colors[12] = 0x4040FF; //waterColor
                Colors[13] = 0x685332; //woodColor
                Colors[14] = 0xFFFCF5; //quartzColor
                Colors[15] = 0xD87F33; //adobeColor
                Colors[16] = 0xB24CD8; //magentaColor
                Colors[17] = 0x6699D8; //lightBlueColor
                Colors[18] = 0xE5E533; //yellowColor
                Colors[19] = 0x7FCC19; //limeColor
                Colors[20] = 0xF27FA5; //pinkColor
                Colors[21] = 0x4C4C4C; //grayColor
                Colors[22] = 0x999999; //silverColor
                Colors[23] = 0x4C7F99; //cyanColor
                Colors[24] = 0x7F3FB2; //purpleColor
                Colors[25] = 0x334CB2; //blueColor
                Colors[26] = 0x664C33; //brownColor
                Colors[27] = 0x667F33; //greenColor
                Colors[28] = 0x993333; //redColor
                Colors[29] = 0x191919; //blackColor
                Colors[30] = 0xFAEE4D; //goldColor
                Colors[31] = 0x5CDBD5; //diamondColor
                Colors[32] = 0x4A80FF; //lapisColor
                Colors[33] = 0x00D93A; //emeraldColor
                Colors[34] = 0x15141F; //obsidianColor
                Colors[35] = 0x700200; //netherrackColor
            }

            public static int GetVariant(int variant, int color)
            {
                int br = 220;

                if (variant == 3) br = 135;
                else if (variant == 2) br = 255;
                else if (variant == 1) br = 220;
                else if (variant == 0) br = 180;

                int R = (color >> 16 & 0xFF) * br / 0xFF;
                int G = (color >> 8 & 0xFF) * br / 0xFF;
                int B = (color & 0xFF) * br / 0xFF;
                return 0xFF << 24 | R << 16 | G << 8 | B;
            }
        }
    }
}
