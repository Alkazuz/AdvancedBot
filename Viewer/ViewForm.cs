using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using AdvancedBot.client;
using AdvancedBot.client.Map;
using AdvancedBot.client.Packets;
using AdvancedBot.Viewer.Gui;
using AdvancedBot.Viewer.Gui.ItemRenderer;
using AdvancedBot.Viewer.Model;

namespace AdvancedBot.Viewer
{
    public partial class ViewForm : Form
    {
        public static ViewForm OpenForm = null;

        public MinecraftClient Client;
        public ViewForm(MinecraftClient client)
        {
            InitializeComponent();
            Client = client;
            x = tickX = client.Player.PosX;
            y = tickY = client.Player.PosY;
            z = tickZ = client.Player.PosZ;
            OpenForm = this;
        }

        private WGL wgl;
        private Font font;

        private float pitch, yaw = 180;

        private double tickX, tickY, tickZ;
        private double oldX, oldY, oldZ;
        public double x, y = 20, z;
        private double mx, my, mz;

        private bool ShowDebugInfo = Debugger.IsAttached;
        private bool ShowChat = true;
        private bool ControlPlayer = false;
        private bool ControlAllBots = false;

        private Queue<Action> glDispatchQueue = new Queue<Action>();
        public void InvokeOnGLThread(Action func)
        {
            lock (glDispatchQueue) {
                glDispatchQueue.Enqueue(func);
            }
        }

        private GuiBase _openGui;
        public GuiBase OpenGUI
        {
            get => _openGui;
            set {
                _openGui = value;
                if (value == null) {
                    Cursor = invisibleCursor;
                } else {
                    Cursor = Cursors.Default;
                }
            }
        }

        public bool Fly = false;

        private HitResult hit;
        private int mouseClickDelay;

        public WorldRenderer WorldRenderer;
        public TextureManager TexManager = new TextureManager();

        private bool continueLoop = true;
        private Thread glThread;

        private double realFps;

        private Cursor invisibleCursor;

        private void ViewContext_Load(object sender, EventArgs e)
        {
            TextureManager.LoadTextureMap();
            this.Icon = Program.FrmMain.Icon;

            Client.World.OnUpdate += WorldUpdate;
            Client.UseExternalTickSource = true;
            ClientSize = new Size((int)(854 / 1.2), (int)(480 / 1.2));

            glThread = new Thread(MainLoop);
            glThread.Name = "Viewer main loop";
            glThread.Start();

            using (Bitmap bmp = new Bitmap(16, 16)) {
                invisibleCursor = new Cursor(bmp.GetHicon());
            }
            Cursor = invisibleCursor;
        }

        protected override void OnPaintBackground(PaintEventArgs e) { }

        private void MainLoop()
        {
            bool cleaned = false;

            try {
                wgl = WGL.Create(this);
                GL.glEnable(GL.GL_DEPTH_TEST);
                GL.glEnable(GL.GL_CULL_FACE);
                GL.glDepthFunc(GL.GL_LEQUAL);
                GL.glShadeModel(GL.GL_SMOOTH);

                GL.glHint(GL.GL_PERSPECTIVE_CORRECTION_HINT, GL.GL_NICEST);
                //a7dbf9

                const int BGC = 0xa7dbf9;
                GL.glClearColor((BGC >> 16 & 0xFF) / 255.0f, (BGC >> 8 & 0xFF) / 255.0f, (BGC & 0xFF) / 255.0f, 0f);

                font = new Font(TexManager);
                WorldRenderer = new WorldRenderer(Client.World);
                ItemRendererBase.Initialize();

                if (!GL.IsVBOAvailable) {
                    ViewerConfig.UseVBO = false;
                }

                Stopwatch sw = Stopwatch.StartNew();
                long lastFrame = Stopwatch.GetTimestamp();
                double FreqMs = 1000.0 / Stopwatch.Frequency;
                double FreqTick = FreqMs / 50.0;
                double a = 0;

                while (continueLoop) {
                    sw.Restart();

                    if (a > 100.0) a -= (int)(a - 100.0);

                    for (; a > 1.0; a -= 1.0) {
                        Tick();
                    }
                    x = oldX + (tickX - oldX) * a;
                    y = oldY + (tickY - oldY) * a;
                    z = oldZ + (tickZ - oldZ) * a;

                    Render();
                    int delay = (int)((1000.0 / ViewerConfig.MaxFps) - sw.ElapsedMilliseconds);

                    if (delay > 1) {
                        Thread.Sleep(delay);
                    }
                    long now = Stopwatch.GetTimestamp();
                    long delta = now - lastFrame;
                    double fps = 1000.0 / (delta * FreqMs);
                    realFps = (realFps * 7 + fps) / 8;

                    a += delta * FreqTick;

                    lastFrame = now;
                }

                cleaned = true;
                CleanUp();
                Debug.WriteLine("Shutdown");
            } catch (Exception e) {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Ocorreu um erro.");
                sb.AppendLine("--------");
                sb.AppendFormat("OpenGL: '{0}', VBO: {1}, Avail: {2}\n", GL.Version, ViewerConfig.UseVBO, GL.IsVBOAvailable);
                sb.Append("MissingFunc: [");
                foreach (FieldInfo field in typeof(GL).GetFields(BindingFlags.Static | BindingFlags.Public)) {
                    var attrib = field.GetCustomAttribute<GL.GLFuncAttribute>();
                    if (attrib != null && field.GetValue(null) == null) {
                        sb.Append(attrib.Name + ", ");
                    }
                }
                sb.Append("]\n");
                sb.AppendLine(e.ToString());
                sb.AppendLine("--------");

                MessageBox.Show(sb.ToString(), "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (!cleaned) {
                    try {
                        Debug.WriteLine("Shutdown at error");
                        CleanUp();
                    } catch { }
                }
                Close();
            }
        }
        private void CleanUp()
        {
            WorldRenderer.CleanUp();
            WorldRenderer = null;
            TexManager.DeleteTextures();
            ItemRendererBase.CleanUp();
            if (ChunkRenderer.modelRenderer != null) {
                ChunkRenderer.modelRenderer.Dispose();
                ChunkRenderer.modelRenderer = null;
            }
            wgl.Destroy();
        }

        private void Render()
        {
            //tickX = x = 156; tickY = y = 69.62; tickZ = z = 135.03;
            //yaw = 81.6f; pitch = 12.6f;

            int w = ClientSize.Width;
            int h = ClientSize.Height;

            Stopwatch sw = Stopwatch.StartNew();

            GL.glViewport(0, 0, w, h);
            GL.glClear(GL.GL_COLOR_BUFFER_BIT | GL.GL_DEPTH_BUFFER_BIT);

            GL.glDepthMask(false);
            GuiUtils.DrawGradientRect(0, 0, w, h, 0xFF << 24 | 0x5eace5, 0xFF << 24 | 0xaadcff);
            GL.glDepthMask(true);

            GL.glMatrixMode(GL.GL_PROJECTION);
            GL.glLoadIdentity();
            GL.gluPerspective(80f, w / (float)h, 0.05f, 1000f);
            GL.glMatrixMode(GL.GL_MODELVIEW);
            GL.glLoadIdentity();

            GL.glRotatef(pitch, 1, 0, 0);
            GL.glRotatef(yaw, 0, 1, 0);

            if (ViewerConfig.UseTexture) {
                GL.glEnable(GL.GL_TEXTURE_2D);

                GL.glEnable(GL.GL_BLEND);
                GL.glBlendFunc(GL.GL_SRC_ALPHA, GL.GL_ONE_MINUS_SRC_ALPHA);

                GL.glEnable(GL.GL_ALPHA_TEST);
                GL.glAlphaFunc(GL.GL_GREATER, 1.0f / 255f);

                if (ChunkRenderer.modelRenderer != null) {
                    GL.glBindTexture(GL.GL_TEXTURE_2D, ChunkRenderer.modelRenderer.TexManager.TextureID);
                } else {
                    GL.glBindTexture(GL.GL_TEXTURE_2D, TexManager.Get("debug_textures_blocks", true));
                }
            }
            GL.glEnable(GL.GL_CULL_FACE);

            WorldRenderer.Render(x, y, z);
            if (ViewerConfig.RenderSigns) {
                WorldRenderer.RenderSigns(font);
            }

            if (ViewerConfig.UseTexture) {
                GL.glDisable(GL.GL_TEXTURE_2D);
            } else {
                GL.glEnable(GL.GL_BLEND);
                GL.glBlendFunc(GL.GL_SRC_ALPHA, GL.GL_ONE_MINUS_SRC_ALPHA);
            }

            GL.glEnable(GL.GL_LINE_SMOOTH);
            GL.glLineWidth(2f);

            if (Client.CurrentPath != null) {
                Vec3i[] p = Client.CurrentPath.Points;
                GL.glBegin(GL.GL_LINE_STRIP);
                GL.glColor3f(0.1f, 0.1f, 0.7f);
                foreach (Vec3i pt in p) {
                    GL.glVertex3d(-x + (pt.X + 0.5f), -y + (pt.Y + 0.1f), -z + (pt.Z + 0.5f));
                }
                GL.glEnd();
                GL.glColor3f(1, 1, 1);
            }

            DrawMinerBlockESP();
            if (!ControlPlayer) {
                Entity p = Client.Player;

                //if(pChar == null) {
                //    pChar = new PlayerChar();
                //}

                //GL.glEnable(GL.GL_TEXTURE_2D);
                //GL.glBindTexture(GL.GL_TEXTURE_2D, TexManager.Get("steve"));

                ////p.Yaw = (float)(DateTime.UtcNow.Ticks / 100000.0 % 360);

                //pChar.Render(new PlayerChar.CharInfo() {
                //    X = -x + p.PosX,
                //    Y = -y + p.PosY - 0.22,
                //    Z = -z + p.PosZ,
                //    Yaw = p.Yaw,
                //    Pitch = p.Pitch,
                //    Run = (float)Math.Sqrt(p.MotionX*p.MotionX+ p.MotionZ * p.MotionZ)
                //});
                //GL.glDisable(GL.GL_TEXTURE_2D);


                AABB bb = Client.Player.AABB;
                GL.glColor3f(1, 0, 0);
                DrawBox(bb.MinX, bb.MinY, bb.MinZ, bb.MaxX, bb.MaxY, bb.MaxZ);

                /*GL.glPolygonMode(GL.GL_FRONT_AND_BACK, GL.GL_LINE);
                GL.glLineWidth(4);
                pChar.Render();
                GL.glPolygonMode(GL.GL_FRONT_AND_BACK, GL.GL_FILL);*/


                GL.glColor3f(0.0f, 0.8f, 0.0f);
                Vec3d l = Client.Player.CalculateLookVector(p.Yaw - 180, p.Pitch).Mul(0.8);
                GL.glBegin(GL.GL_LINE_STRIP);
                GL.glVertex3d(-x + p.PosX, -y + p.PosY, -z + p.PosZ);
                GL.glVertex3d(-x + (p.PosX + l.X), -y + (p.PosY + l.Y), -z + (p.PosZ + l.Z));
                GL.glEnd();
            }

            Vec3d start = new Vec3d(x, y, z);
            Vec3d end = Client.Player.CalculateLookVector(yaw, pitch).Mul(ControlPlayer ? 6 : 128).Add(start);
            hit = Client.World.RayCast(new Vec3d(x, y, z), end, true, !ControlPlayer);

            MPPlayer collidedEntity = null;
            HitResult phit = GetPlayerOverHit(start, end);
            if (phit != null && (hit == null || (Utils.DistToSq(x, y, z, hit.X + 0.5, hit.Y + 0.5, hit.Z + 0.5) > Utils.DistToSq(x, y, z, phit.PointedEntity.X, phit.PointedEntity.Y, phit.PointedEntity.Z)))) {
                hit = phit;
                collidedEntity = phit.PointedEntity;
            }

            var players = Client.PlayerManager.Players.Values;
            foreach (MPPlayer p in players) {
                if (p == collidedEntity) {
                    GL.glColor3f(1, 1, 0);
                } else {
                    GL.glColor3f(0, 0.4f, 0);
                }
                DrawBox(p.X - 0.3, p.Y, p.Z - 0.3, p.X + 0.3, p.Y + 1.8, p.Z + 0.3);
            }

            if (hit != null && hit.PointedEntity == null) {
                GL.glDisable(GL.GL_ALPHA_TEST);
                GL.glDisable(GL.GL_TEXTURE_2D);
                GL.glColor4f(0, 0, 0, 0.4f);
                GL.glLineWidth(2);
                DrawBox(hit.X - 0.003, hit.Y - 0.003, hit.Z - 0.003, hit.X + 1.003, hit.Y + 1.003, hit.Z + 1.003);
                GL.glLineWidth(1);
                GL.glEnable(GL.GL_ALPHA_TEST);
            }
            //var cur = PointToClient(Cursor.Position);
            //var mouseVec = CalculateMouseHitVec(cur.X, cur.Y);
            //var mouseHit = Client.World.RayCast(new Vec3d(x, y, z),
            //                                    mouseVec.Mul(128).Add(x, y, z), true, true);
            //if(mouseHit != null) {
            //    GL.glColor4f(1, 0, 0, 0.4f);
            //    GL.glLineWidth(2);

            //    DrawBox(mouseHit.X - 0.003, mouseHit.Y - 0.003, mouseHit.Z - 0.003, 
            //            mouseHit.X + 1.003, mouseHit.Y + 1.003, mouseHit.Z + 1.003);
            //    GL.glLineWidth(1);
            //}

            GL.glEnable(GL.GL_TEXTURE_2D);
            GL.glDisable(GL.GL_CULL_FACE);

            foreach (MPPlayer p in players) {
                GL.glPushMatrix();
                GL.glTranslated(-x + p.X, -y + (p.Y + 2.15), -z + p.Z);

                GL.glRotatef(180 - yaw, 0, 1, 0);

                GL.glScalef(-0.024f, -0.024f, -0.024f);

                string nick = Client.PlayerManager.GetNick(p);
                font.Draw(nick, -(font.Measure(nick) / 2), 0, true);
                GL.glPopMatrix();
            }

            GL.glEnable(GL.GL_CULL_FACE);
            GL.glDisable(GL.GL_TEXTURE_2D);
            GL.glColor4f(1, 1, 1, 1);

            GL.glMatrixMode(GL.GL_PROJECTION);
            GL.glLoadIdentity();
            GL.glOrtho(0.0, w, h, 0.0, -300.0, 300.0);
            GL.glMatrixMode(GL.GL_MODELVIEW);
            GL.glLoadIdentity();

            DrawGui();
            GL.glEnable(GL.GL_TEXTURE_2D);
            DrawChatHistory(w, h);

            sw.Stop();
            double elMs = sw.Elapsed.TotalMilliseconds;

            StringBuilder sinf = new StringBuilder();
            AppendInfo(sinf);
            if (ShowDebugInfo) {
                sinf.AppendFormat("\nRender: {0:0.00}ms (FPS: {1:0.00})\n", elMs, realFps);
                sinf.AppendFormat("Players: {0}\n", Client.PlayerManager.UUID2Nick.Count);
            }
            font.Draw(sinf.ToString(), 2, 2, false);

            int mY = 0;

            if (!ControlPlayer) {
                font.Draw("§cFreecam", 2, 2 + (mY++ * 10), w, h, TextPosition.Right);
            }
            if (Client.CmdManager.GetCommand("killaura").IsToggled) {
                font.Draw("§cKillaura", 2, 2 + (mY++ * 10), w, h, TextPosition.Right);
            }
            if (Fly) {
                font.Draw("§cFly", 2, 2 + (mY++ * 10), w, h, TextPosition.Right);
            }
            if (ControlAllBots) {
                font.Draw("§cControlando todos os bots", 2, 2 + (mY++ * 10), w, h, TextPosition.Right);
            }

            //Hotbar items
            if (ControlPlayer && OpenGUI == null) {
                int xs = (w / 2) - (int)((9.0 / 2) * 32);
                int ys = h - 35;

                for (int i = 0; i < 9; i++) {
                    GL.glDisable(GL.GL_TEXTURE_2D);
                    GL.glLineWidth(2);
                    float col = Client.HotbarSlot == i ? 0.8f : 0.25f;
                    GL.glColor4f(col, col, col, 0.6f);
                    GuiUtils.DrawRectangle(xs, ys, 32, 32, GL.GL_LINE_LOOP);
                    GL.glColor4f(1, 1, 1, 1f);
                    GL.glLineWidth(1);
                    GL.glEnable(GL.GL_TEXTURE_2D);
                    GuiInventory.DrawItem(TexManager, font, Client.Inventory.Slots[36 + i], xs, ys, 2);
                    xs += 32;
                }
            }
            GL.glDisable(GL.GL_TEXTURE_2D);

            if (OpenGUI == null) {
                DrawCrossbar(w, h);
            }

            GL.glDisable(GL.GL_BLEND);

            GL.glMatrixMode(GL.GL_PROJECTION);
            GL.glMatrixMode(GL.GL_MODELVIEW);

            wgl.Flush();
        }
        private void DrawGui()
        {
            if (OpenGUI != null) {
                int w = ClientSize.Width;
                int h = ClientSize.Height;
                Point cur = PointToClient(Cursor.Position);

                OpenGUI.Draw(font, cur.X, cur.Y, w, h);
            }
        }
        private void DrawChatHistory(int w, int h)
        {
            if (OpenGUI is GuiChatInput) {
                h -= 20;
            } else if (OpenGUI != null) {
                return;
            }

            h -= 12;
            List<string> messages = Client.ChatMessages;
            lock (messages) {
                int c = messages.Count;
                const int HIS_SIZE = 6;
                for (int i = 0, l = Math.Min(c, HIS_SIZE); i < l; i++) {
                    string msg = messages[c - 1 - i];
                    font.Draw(msg, 2, h - (i * 12), false);
                }
            }
        }
        private void DrawCrossbar(int w, int h)
        {
            GL.glDisable(GL.GL_LINE_SMOOTH);
            GL.glLineWidth(2f);

            int xm = w / 2;
            int ym = h / 2;

            GL.glBegin(GL.GL_LINES);
            GL.glColor4f(0.5f, 0.5f, 0.5f, 0.8f);

            GL.glVertex2i(xm - 8, ym);
            GL.glVertex2i(xm + 8, ym);

            GL.glVertex2i(xm, ym - 8);
            GL.glVertex2i(xm, ym + 8);

            GL.glEnd();
        }

        private void DrawMinerBlockESP()
        {
            if (AutoMiner.DiggingBlocks.Count != 0) {
                GL.glDisable(GL.GL_DEPTH_TEST);
                GL.glDepthMask(false);
                foreach (var key in AutoMiner.DiggingBlocks.Keys) {
                    int x = (int)(key >> 38);
                    int y = (int)((key >> 26) & 0xFFF);
                    int z = (int)(key << 38 >> 38);
                    int color = ((Utils.XorShift32(x) * 1832086) ^ Utils.XorShift32(y) * 3040100) ^ Utils.XorShift32(z);

                    GL.glColor3f((color >> 16 & 0xFF) / 255.0f, (color >> 8 & 0xFF) / 255.0f, (color & 0xFF) / 255.0f);
                    DrawBox(x, y, z, x + 1, y + 1, z + 1);
                }
                GL.glDepthMask(true);
                GL.glEnable(GL.GL_DEPTH_TEST);
            }
            var areaMiner = Client.CmdManager.GetCommand<client.Commands.CommandAreaMiner>()?.Miner;
            if (areaMiner != null && areaMiner.IsMining) {
                var min = areaMiner.Min;
                var max = areaMiner.Max;
                GL.glDisable(GL.GL_DEPTH_TEST);
                GL.glColor3f(1f, 1f, 1f);
                DrawBox(min.X, min.Y, min.Z, max.X + 1, max.Y + 1, max.Z + 1);

                GL.glColor3f(0f, 0f, 1f);
                DrawBox(min.X, min.Y, min.Z, min.X + 1, min.Y + 1, min.Z + 1);
                GL.glColor3f(1f, 0f, 0f);
                DrawBox(max.X, max.Y, max.Z, max.X + 1, max.Y + 1, max.Z + 1);
                GL.glEnable(GL.GL_DEPTH_TEST);
            }
        }

        private void DrawBox(double minX, double minY, double minZ, double maxX, double maxY, double maxZ)
        {
            minX -= x;
            minY -= y;
            minZ -= z;
            maxX -= x;
            maxY -= y;
            maxZ -= z;

            GL.glBegin(GL.GL_LINE_STRIP);
            GL.glVertex3d(minX, minY, minZ);
            GL.glVertex3d(maxX, minY, minZ);
            GL.glVertex3d(maxX, minY, maxZ);
            GL.glVertex3d(minX, minY, maxZ);
            GL.glVertex3d(minX, minY, minZ);
            GL.glVertex3d(minX, maxY, minZ);
            GL.glVertex3d(maxX, maxY, minZ);
            GL.glVertex3d(maxX, maxY, maxZ);
            GL.glVertex3d(minX, maxY, maxZ);
            GL.glVertex3d(minX, maxY, minZ);
            GL.glVertex3d(minX, maxY, maxZ);
            GL.glVertex3d(minX, minY, maxZ);
            GL.glVertex3d(maxX, minY, maxZ);
            GL.glVertex3d(maxX, maxY, maxZ);
            GL.glVertex3d(maxX, maxY, minZ);
            GL.glVertex3d(maxX, minY, minZ);
            GL.glEnd();
        }

        private void AppendInfo(StringBuilder sb)
        {
            sb.AppendFormat("Cam pos: {0:0.00} {1:0.00} {2:0.00} rot: {3:0.0} {4:0.0}\n", x, y, z, Utils.WrapAngleTo360(yaw - 180), pitch);

            if (hit != null && hit.PointedEntity != null) {
                sb.AppendFormat("Hit: {0}§f (ID: {1})", Client.PlayerManager.GetNick(hit.PointedEntity), hit.PointedEntity.EntityID);
            } else if (hit != null) {
                int id = Client.World.GetBlock(hit.X, hit.Y, hit.Z);
                int data = Client.World.GetData(hit.X, hit.Y, hit.Z);

                sb.AppendFormat("Hit: {4}:{5} (Pos: {0} {1} {2})", hit.X, hit.Y, hit.Z, hit.Face, Blocks.GetName(id), data);

                if ((id == Blocks.standing_sign || id == Blocks.wall_sign) &&
                    Client.World.Signs.TryGetValue(new Vec3i(hit.X, hit.Y, hit.Z), out var txt)) {
                    sb.Append("\nSign text: \"" + string.Join("§f\", \"", txt) + "§f\"");
                }
            }
        }

        private static Vec3d CalculateMouseHitVec(int x, int y)
        {
            int[] viewport = new int[4];
            double[] modelMatrix = new double[16];
            double[] projMatrix = new double[16];
            GL.glGetIntegerv(GL.GL_VIEWPORT, viewport);
            GL.glGetDoublev(GL.GL_MODELVIEW_MATRIX, modelMatrix);
            GL.glGetDoublev(GL.GL_PROJECTION_MATRIX, projMatrix);

            double objX, objY, objZ;
            GL.gluUnProject(x, viewport[3] - y, 1.0, modelMatrix, projMatrix, viewport, out objX, out objY, out objZ);

            return new Vec3d(objX, objY, objZ);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            continueLoop = false;
            Client.Player.LockMoveQueue = false;

            Client.UseExternalTickSource = false;
            Client.World.OnUpdate -= WorldUpdate;

            Client.Player.JumpMoveSpeed = 0.02;
            try {
                glThread.Join(1000);
            } catch { }
            invisibleCursor.Dispose();
            Debug.WriteLine("Viewer form closing");

            OpenForm = null;
            base.OnFormClosing(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            OpenGUI?.MouseDown(e.X, e.Y, e.Button);
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (ContainsFocus && OpenGUI == null) {
                int w2 = ClientSize.Width / 2;
                int h2 = ClientSize.Height / 2;

                yaw += (e.X - w2) * 0.04f;
                pitch -= (h2 - e.Y) * 0.055f;

                pitch = pitch < -90 ? -90 : pitch > 90 ? 90 : pitch;
                yaw %= 360;
            } else if (OpenGUI != null) {
                OpenGUI.MouseMove(e.X, e.Y, e.Button);
            }
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            OpenGUI?.MouseUp(e.X, e.Y, e.Button);
            mouseClickDelay = 0;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            OpenGUI?.KeyDown(e);
        }
        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            switch (e.KeyCode) {
                case Keys.Escape:
                    if (OpenGUI != null) {
                        if (Client.OpenWindow != null && OpenGUI is GuiInventory) {
                            Client.SendPacket(new PacketCloseWindow(Client.OpenWindow.WindowID));
                            Client.OpenWindow = null;
                            Inventory.ClickedItem = null;
                        }
                        OpenGUI = null;
                    } else {
                        OpenGUI = new GuiOptions(WorldRenderer);
                    }
                    break;
                default: {
                    if (OpenGUI == null) {
                        HandleKeyUp(e);
                    } else {
                        OpenGUI.KeyUp(e);
                    }
                    break;
                }
            }
        }
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            OpenGUI?.KeyPress(e.KeyChar);
        }

        private void HandleKeyUp(KeyEventArgs e)
        {
            switch (e.KeyCode) {

                case Keys.E: OpenGUI = new GuiInventory(this); break;
                case Keys.T: OpenGUI = new GuiChatInput(this); break;
                case Keys.F7: ShowChat = !ShowChat; break;
                case Keys.D1: Client.HotbarSlot = 0; break;
                case Keys.D2: Client.HotbarSlot = 1; break;
                case Keys.D3: Client.HotbarSlot = 2; break;
                case Keys.D4: Client.HotbarSlot = 3; break;
                case Keys.D5: Client.HotbarSlot = 4; break;
                case Keys.D6: Client.HotbarSlot = 5; break;
                case Keys.D7: Client.HotbarSlot = 6; break;
                case Keys.D8: Client.HotbarSlot = 7; break;
                case Keys.D9: Client.HotbarSlot = 8; break;
                case Keys.R: var cmd = Client.CmdManager.GetCommand("killaura"); cmd.IsToggled = !cmd.IsToggled; break;
                case Keys.F4: ShowDebugInfo = !ShowDebugInfo; break;
                case Keys.F5: WorldRenderer.SetAllDirty(); break;
                case Keys.F6:
                    ControlPlayer = !ControlPlayer;
                    Client.Player.LockMoveQueue = ControlPlayer;
                    break;
                case Keys.F8:
                    ControlPlayer = true;
                    ControlAllBots = !ControlAllBots;

                    var player = Client.Player;
                    foreach (var client in Program.FrmMain.Clients) {
                        var cp = client.Player;
                        var dist = Utils.DistTo(cp.PosX, cp.PosY, cp.PosZ, player.PosX, player.PosY, player.PosZ);
                        if (dist < 6.0 && ControlAllBots) {
                            client.SetPath(player.BlockX, player.BlockY, player.BlockZ);
                        }
                        Client.Player.LockMoveQueue = ControlAllBots;
                    }
                    break;
                case Keys.Q: Client.Inventory.DropItem(Client, 36 + Client.HotbarSlot); break;
                case Keys.F:
                    Fly = !Fly;
                    Client.Player.JumpMoveSpeed = 0.02;
                    break;
            }
        }

        private ConcurrentQueue<DirtyEvent> dirtyQueue = new ConcurrentQueue<DirtyEvent>();
        private void WorldUpdate(Vec3i pos, WorldUpdateEventType type)
        {
            dirtyQueue.Enqueue(new DirtyEvent(pos, type));
        }

        private void Tick()
        {
            Client.Tick();
            if (ControlPlayer && Fly && HasFocus) {
                double speed = ViewerConfig.FlySpeed;
                Client.Player.MotionX = 0.0;
                Client.Player.MotionY = 0.0;
                Client.Player.MotionZ = 0.0;

                Client.Player.JumpMoveSpeed = speed / 3.0;

                if (IsKeyDown(Keys.Space)) Client.Player.MotionY += speed / 4.0;
                if (IsKeyDown(Keys.LShiftKey)) Client.Player.MotionY -= speed / 4.0;
            }

            if (WorldRenderer == null) {
                return;
            }
            ItemRendererBase.NotifyTick();

            for (int i = 0; i < 1024 && dirtyQueue.TryDequeue(out var ev); i++) {
                switch (ev.Type) {
                    case WorldUpdateEventType.Block: WorldRenderer.BlockChange(ev.X, ev.Y, ev.Z); break;
                    case WorldUpdateEventType.Chunk: WorldRenderer.ChunkChange(ev.X, ev.Z); break;
                    case WorldUpdateEventType.World: WorldRenderer.SetAllDirty(); break;
                }
            }
            WorldRenderer.Tick(x, y, z);
            lock (glDispatchQueue) {
                while (glDispatchQueue.Count > 0) {
                    glDispatchQueue.Dequeue().Invoke();
                }
            }

            bool input = OpenGUI == null;

            oldX = tickX;
            oldY = tickY;
            oldZ = tickZ;

            if (ControlPlayer) {
                MovePlayer(input);
            } else {
                MoveCamera(input);
            }

            if (input && HasFocus) {
                Cursor.Position = PointToScreen(new Point(ClientSize.Width / 2, ClientSize.Height / 2));
            }
        }
        private void MovePlayer(bool hasInput)
        {
            if (!hasInput) {
                tickX = Client.Player.PosX;
                tickY = Client.Player.PosY;
                tickZ = Client.Player.PosZ;
            } else if (hasInput) {
                if (--mouseClickDelay < 0 && hit != null) {
                    var btn = InputHelper.GetMouseState();

                    if (hit.PointedEntity != null && btn != MouseButtons.None) {
                        Client.SendPacket(new PacketSwingArm(Client.PlayerID));
                        Client.SendPacket(new PacketUseEntity(hit.PointedEntity.EntityID, btn == MouseButtons.Left));
                    } else {
                        if (btn == MouseButtons.Right) {
                            Client.PlaceCurrentBlock(hit, true);
                            mouseClickDelay = 4;
                        } else if (btn == MouseButtons.Left) {
                            Client.BreakBlock(hit);
                            mouseClickDelay = 4;
                        }
                    }
                }

                Movement mf = Movement.None;

                if (IsKeyDown(Keys.W)) mf |= Movement.Forward;
                if (IsKeyDown(Keys.S)) mf |= Movement.Back;
                if (IsKeyDown(Keys.A)) mf |= Movement.Left;
                if (IsKeyDown(Keys.D)) mf |= Movement.Right;
                if (IsKeyDown(Keys.Space)) mf |= Movement.Jump;

                bool sprint = IsKeyDown(Keys.ControlKey);

                Entity p = Client.Player;

                if (ControlAllBots) {
                    foreach (var bot in Program.FrmMain.Clients) {
                        var bp = bot.Player;
                        MovePlayer(bp, mf, sprint);
                        if (bp != p && Utils.DistTo(bp.PosX, bp.PosY, bp.PosZ, p.PosX, p.PosY, p.PosZ) < 0.28) {
                            bp.SetPosition(p.PosX, p.AABB.MinY, p.PosZ);
                            bp.MotionX = p.MotionX;
                            bp.MotionY = p.MotionY;
                            bp.MotionZ = p.MotionZ;
                        }
                    }
                } else {
                    MovePlayer(p, mf, sprint);
                }

                tickX = p.PosX;
                tickY = p.PosY;
                tickZ = p.PosZ;
            }
        }
        private void MovePlayer(Entity p, Movement mf, bool sprint)
        {
            p.IsSprinting = sprint;

            lock (p.MoveQueue) {
                p.MoveQueue.Clear();
                if (mf != Movement.None) {
                    p.MoveQueue.Enqueue(mf);
                }
            }

            p.Yaw = yaw - 180;
            p.Pitch = pitch;
        }
        private void MoveCamera(bool hasInput)
        {
            if (hasInput) {
                float xa = 0, za = 0;

                if (IsKeyDown(Keys.W)) za--;
                if (IsKeyDown(Keys.S)) za++;
                if (IsKeyDown(Keys.A)) xa--;
                if (IsKeyDown(Keys.D)) xa++;

                const double SPEED = 0.20;
                if (xa != 0 || za != 0) {
                    double sin = Math.Sin(yaw * Math.PI / 180.0);
                    double cos = Math.Cos(yaw * Math.PI / 180.0);

                    mx += (xa * cos - za * sin) * SPEED;
                    mz += (za * cos + xa * sin) * SPEED;
                }
                if (IsKeyDown(Keys.Space)) my += SPEED;
                if (IsKeyDown(Keys.LShiftKey)) my -= SPEED;

                if (IsKeyDown(Keys.G)) {
                    tickX = Client.Player.PosX;
                    tickY = Client.Player.PosY;
                    tickZ = Client.Player.PosZ;
                    mx = 0;
                    my = 0;
                    mz = 0;
                }
            }

            tickX += mx;
            tickY += my;
            tickZ += mz;

            mx *= 0.8;
            my *= 0.8;
            mz *= 0.8;
        }

        private HitResult GetPlayerOverHit(Vec3d start, Vec3d end)
        {
            HitResult hit = null;

            double bestDist = 0;

            PlayerManager pm = Client.PlayerManager;
            foreach (MPPlayer entity in pm.Players.Values) {
                AABB aabb = new AABB(entity.X, entity.Y, entity.Z);
                HitResult intercept = aabb.CalculateIntercept(start, end);
                if (intercept != null) {
                    double dist = start.Distance(intercept.HitVector);

                    if (dist < bestDist || bestDist == 0) {
                        hit = intercept;
                        hit.PointedEntity = entity;
                        bestDist = dist;
                    }
                }
            }
            return hit;
        }


        public void SetClient(MinecraftClient cli)
        {
            Client.World.OnUpdate -= WorldUpdate;
            Client.UseExternalTickSource = false;

            Client = cli;

            Client.World.OnUpdate += WorldUpdate;
            Client.UseExternalTickSource = true;

            WorldRenderer.SetWorld(cli.World);

            x = tickX = cli.Player.PosX;
            y = tickY = cli.Player.PosY;
            z = tickZ = cli.Player.PosZ;
            Fly = false;
            if (OpenGUI is GuiInventory) {
                OpenGUI = new GuiOptions(WorldRenderer);
            }
        }

        public bool HasFocus { get; private set; } //this.Focused returns false if accessed from other thread
        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            HasFocus = true;
        }
        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            HasFocus = false;
            if (OpenGUI == null) {
                OpenGUI = new GuiOptions(WorldRenderer);
            }
        }
        private bool IsKeyDown(Keys key)
        {
            return HasFocus && InputHelper.IsKeyDown(key);
        }

        private struct DirtyEvent
        {
            public WorldUpdateEventType Type;
            public int X;
            public int Y;
            public int Z;

            public DirtyEvent(Vec3i pos, WorldUpdateEventType type)
            {
                X = pos.X;
                Y = pos.Y;
                Z = pos.Z;
                Type = type;
            }
        }
    }
}