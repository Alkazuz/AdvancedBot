using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdvancedBot.client.Map;
using AdvancedBot.client;
using System.Drawing;
using AdvancedBot.Viewer.Model;
using System.IO.Compression;
using System.IO;
using System.Diagnostics;

namespace AdvancedBot.Viewer
{
    public class ChunkRenderer
    {
        private static readonly Dictionary<int, int> id2color = new Dictionary<int, int>();
        private static readonly Dictionary<int, int> woolColors = new Dictionary<int, int>();
        private static readonly Dictionary<int, int> clayColors = new Dictionary<int, int>();

        static ChunkRenderer()
        {
            id2color.Add(1, 0x7D7D7D);
            id2color.Add(2, 0x82C144);
            id2color.Add(3, 0x866043);
            id2color.Add(4, 0x7A7A7A);
            id2color.Add(5, 0xC69E5B);
            id2color.Add(7, 0x535353);
            id2color.Add(8, 0x3046F4);
            id2color.Add(9, 0x2E43F4);
            id2color.Add(10, 0xCF5B14);
            id2color.Add(11, 0xD8681A);
            id2color.Add(12, 0xDBD3A0);
            id2color.Add(13, 0x7E7C7A);
            id2color.Add(14, 0x8F8B7C);
            id2color.Add(15, 0x87827E);
            id2color.Add(16, 0x737373);
            id2color.Add(17, 0x665131);
            id2color.Add(18, 0x47B516);
            id2color.Add(19, 0xB6B639);
            id2color.Add(20, 0x3C4243);
            id2color.Add(21, 0x667086);
            id2color.Add(22, 0x264389);
            id2color.Add(25, 0x644332);
            id2color.Add(27, 0x584830);
            id2color.Add(28, 0x493D36);
            id2color.Add(30, 0x5A5A5A);
            id2color.Add(31, 0x69C143);
            id2color.Add(32, 0x271908);
            id2color.Add(37, 0x0D1300);
            id2color.Add(38, 0x120B01);
            id2color.Add(39, 0x0E0A08);
            id2color.Add(40, 0x190707);
            id2color.Add(41, 0xF9EC4E);
            id2color.Add(42, 0xDBDBDB);
            id2color.Add(45, 0x926356);
            id2color.Add(47, 0x6B5839);
            id2color.Add(48, 0x677967);
            id2color.Add(49, 0x14121D);
            id2color.Add(50, 0x0A0804);
            id2color.Add(52, 0x10181D);
            id2color.Add(56, 0x818C8F);
            id2color.Add(57, 0x61DBD5);
            id2color.Add(65, 0x44351D);
            id2color.Add(66, 0x443D31);
            id2color.Add(69, 0x080605);
            id2color.Add(73, 0x846B6B);
            id2color.Add(74, 0x846B6B);
            id2color.Add(75, 0x070402);
            id2color.Add(76, 0x110704);
            id2color.Add(78, 0xEFFBFB);
            id2color.Add(79, 0x7DADFF);
            id2color.Add(80, 0xEFFBFB);
            id2color.Add(82, 0x9EA4B0);
            id2color.Add(83, 0x516A37);
            id2color.Add(87, 0x6F3634);
            id2color.Add(88, 0x544033);
            id2color.Add(89, 0x8F7645);
            id2color.Add(90, 0x590CC0);
            id2color.Add(93, 0x979393);
            id2color.Add(94, 0xA09393);
            id2color.Add(95, 0x3C4243);
            id2color.Add(96, 0x6C5027);
            id2color.Add(98, 0x7A7A7A);
            id2color.Add(106, 0x3C7C3C);
            id2color.Add(111, 0x448444);
            id2color.Add(112, 0x2C161A);
            id2color.Add(117, 0x3A3025);
            id2color.Add(121, 0xDDDFA5);
            id2color.Add(122, 0x0C090F);
            id2color.Add(123, 0x462B1A);
            id2color.Add(124, 0x775937);
            id2color.Add(129, 0x6D8074);
            id2color.Add(131, 0x181714);
            id2color.Add(132, 0x0A0A0A);
            id2color.Add(133, 0x51D975);
            id2color.Add(137, 0xB2896F);
            id2color.Add(138, 0x74DDD7);
            id2color.Add(140, 0x170C0A);
            id2color.Add(149, 0x9C9695);
            id2color.Add(150, 0xA59594);
            id2color.Add(152, 0xAB1B09);
            id2color.Add(153, 0x7D544F);
            id2color.Add(157, 0x4D3D2E);
            id2color.Add(160, 0x3C4243);
            id2color.Add(172, 0x965C42);
            id2color.Add(173, 0x121212);
            id2color.Add(174, 0xA5C2F5);

            woolColors.Add(0, 0xE9ECEC);
            woolColors.Add(1, 0xF07613);
            woolColors.Add(2, 0xBD44B3);
            woolColors.Add(3, 0x3AAFD9);
            woolColors.Add(4, 0xF8C627);
            woolColors.Add(5, 0x70B919);
            woolColors.Add(6, 0xED8DAC);
            woolColors.Add(7, 0x3E4447);
            woolColors.Add(8, 0x8E8E86);
            woolColors.Add(9, 0x158991);
            woolColors.Add(10, 0x792AAC);
            woolColors.Add(11, 0x35399D);
            woolColors.Add(12, 0x724728);
            woolColors.Add(13, 0x546D1B);
            woolColors.Add(14, 0xA12722);
            woolColors.Add(15, 0x141519);

            clayColors.Add(0, 0xD1B2A1);
            clayColors.Add(1, 0xA15325);
            clayColors.Add(2, 0x95586C);
            clayColors.Add(3, 0x716C89);
            clayColors.Add(4, 0xBA8523);
            clayColors.Add(5, 0x677534);
            clayColors.Add(6, 0xA14E4E);
            clayColors.Add(7, 0x392A23);
            clayColors.Add(8, 0x876A61);
            clayColors.Add(9, 0x565B5B);
            clayColors.Add(10, 0x764656);
            clayColors.Add(11, 0x4A3B5B);
            clayColors.Add(12, 0x4D3323);
            clayColors.Add(13, 0x4C532A);
            clayColors.Add(14, 0x8F3D2E);
            clayColors.Add(15, 0x251610);
        }

        public int ListId = -1;
        public VBO Vbo;

        public bool Dirty = false;

        public bool Built = false;
        public int Vertices = 0;
        public readonly int X, Y, Z;
        public readonly World World;

        public ChunkRenderer(World w, int x, int y, int z)
        {
            World = w;
            X = x;
            Y = y;
            Z = z;
        }

        public void Rebuild()
        {
            Tessellator t = Tessellator.Instance;
            if (!ViewerConfig.UseVBO) {
                if (ListId == -1) {
                    ListId = GL.glGenLists(1);
                }

                GL.glNewList(ListId, GL.GL_COMPILE);

                t.Begin(GL.GL_QUADS);
                TessellateOld(t);
                t.End();
                
                GL.glEndList();
            } else {
                if (false&&ViewerConfig.UseResPack) {
                    t.Begin(GL.GL_TRIANGLES);
                    TessellateNew(t);
                } else {
                    t.Begin(GL.GL_QUADS);
                    TessellateOld(t);
                }
                t.EndVBO(ref Vbo);
            }
            Built = true;
            Dirty = false;
            Vertices = t.VertCount;
        }
        public void Render()
        {
            if (Built && Vertices > 0) {
                if (ViewerConfig.UseVBO && Vbo != null) {
                    Vbo.Render();
                } else if(ListId != -1) {
                    GL.glCallList(ListId);
                }
            }
        }
        public void Delete()
        {
            if (Built) {
                if (Vbo != null) {
                    Vbo.Dispose();
                    Vbo = null;
                }
                if (ListId != -1) {
                    GL.glDeleteLists(ListId, 1);
                    ListId = -1;
                }
                Built = false;
            }
        }


        public static ModelRenderer modelRenderer;

        private void TessellateNew(Tessellator t)
        {
            int cx = X * 16;
            int cy = Y * 16;
            int cz = Z * 16;

            Chunk c = World.GetChunk(X, Z);

            ChunkSection sec = c?.Sections[Y];

            if (modelRenderer == null) {
                var zip = ZipFile.OpenRead(@"resources\Default.zip");
                modelRenderer = new ModelRenderer(zip);
            }

            if (sec != null) {
                bool hasWater = false;

                for (int by = 0; by < 16; by++) {
                    for (int bz = 0; bz < 16; bz++) {
                        for (int bx = 0; bx < 16; bx++) {
                            int id = sec.Blocks[by << 8 | bz << 4 | bx];

                            if (id != Blocks.air && id != Blocks.barrier) {
                                if (Blocks.IsTransparent(id)) {
                                    hasWater = true;
                                } else {
                                    RenderModel(t, cx + bx, cy + by, cz + bz, id);
                                }
                            }
                        }
                    }
                }

                if (!hasWater) return;

                for (int by = 0; by < 16; by++) {
                    for (int bz = 0; bz < 16; bz++) {
                        for (int bx = 0; bx < 16; bx++) {
                            int id = sec.Blocks[by << 8 | bz << 4 | bx];

                            if (id != Blocks.air && id != Blocks.barrier && Blocks.IsTransparent(id)) {
                                RenderModel(t, cx + bx, cy + by, cz + bz, id);
                            }
                        }
                    }
                }
            }
        }
        private void RenderModel(Tessellator t, int x, int y, int z, int id)
        {
            if (Blocks.IsLiquid(id)) {
                modelRenderer.RenderLiquid(t, x, y, z, World);
            } else {
                modelRenderer.Render(t, x, y, z, World);
            }
        }

        public double DistToSq(double x, double y, double z)
        {
            double dx = (X * 16 + 8) - x;
            double dy = (Y * 16 + 8) - y;
            double dz = (Z * 16 + 8) - z;
            return dx * dx + dy * dy + dz * dz;
        }
        public double DistTo(double x, double y, double z)
        {
            return Math.Sqrt(DistToSq(x, y, z));
        }
        public bool InFrustum(Frustum f)
        {
            return f.CubeInFrustum(X * 16, Y * 16, Z * 16,
                                   X * 16 + 16, Y * 16 + 16, Z * 16 + 16);
        }

        public override int GetHashCode()
        {
            return WorldRenderer.PosKey(X, Y, Z).GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj is ChunkRenderer o) {
                return o.X == X && o.Y == Y && o.Z == Z && o.World == World;
            }
            return base.Equals(obj);
        }

        #region Old renderer
        private void TessellateOld(Tessellator t)
        {
            Chunk c = World.GetChunk(X, Z);

            ChunkSection sec = c?.Sections[Y];
            if (sec != null) {
                TessellatePass(t, sec, 0); //solid
                if (ViewerConfig.UseTexture) {
                    TessellatePass(t, sec, 1); //translucent
                }
            }
        }
        private void TessellatePass(Tessellator t, ChunkSection sec, int pass)
        {
            int cx = X * 16;
            int cy = Y * 16;
            int cz = Z * 16;
            bool tex = ViewerConfig.UseTexture;

            for (int by = 0; by < 16; by++) {
                for (int bz = 0; bz < 16; bz++) {
                    for (int bx = 0; bx < 16; bx++) {
                        int idx = by << 8 | bz << 4 | bx;
                        int id = sec.Blocks[idx];
                        if (id != Blocks.air && id != Blocks.barrier) {
                            byte b = sec.Metadata[idx / 2];
                            int data = (idx & 1) == 0 ? b & 0x0F : (b >> 4) & 0x0F;

                            if (tex) {
                                switch (id) {
                                    case Blocks.tallgrass:
                                    case Blocks.red_flower:
                                    case Blocks.yellow_flower:
                                    case Blocks.double_plant:
                                    case Blocks.reeds:
                                    case Blocks.web:
                                    case Blocks.wheat:
                                    case Blocks.carrots:
                                    case Blocks.potatoes:
                                    case Blocks.red_mushroom:
                                    case Blocks.brown_mushroom:
                                    case Blocks.deadbush:
                                    case Blocks.nether_wart:
                                    case Blocks.fire:
                                    case Blocks.sapling:
                                    case Blocks.melon_stem:
                                    case Blocks.pumpkin_stem:
                                        if (pass == 1) {
                                            RenderCrossedCube(t, cx + bx, cy + by, cz + bz, id, data);
                                        }
                                        break;
                                    default:
                                        if (BlockUtils.IsStairs(id)) {
                                            RenderStairs(t, cx + bx, cy + by, cz + bz, id, data);
                                        } else if (GetFenceWoodID(id) != -1) {
                                            RenderFence(t, cx + bx, cy + by, cz + bz, id, data);
                                        } else {
                                            bool tr = Blocks.IsTransparent(id);
                                            if (pass == 0 ? !tr : tr) {
                                                RenderBlockWithTex(t, cx + bx, cy + by, cz + bz, id, data);
                                            }
                                        }
                                        break;
                                }
                            } else {
                                RenderCube(t, cx + bx, cy + by, cz + bz, id, data);
                            }
                        }
                    }
                }
            }
        }

        private const float C1 = 1.0f, C2 = 0.8f, C3 = 0.6f;

        private void RenderCube(Tessellator t, int x, int y, int z, int id, int data)
        {
            const float FC1 = 0.0f, 
                        FC2 = 0.2f, 
                        FC3 = 0.4f;
            float r = 1;
            float g = 1;
            float b = 1;
            int color = 0;

            bool wool = id == Blocks.wool || id == Blocks.carpet;
            bool clay = id == Blocks.stained_hardened_clay;
            
            if (wool ? woolColors.TryGetValue(data, out color) : 
                clay ? clayColors.TryGetValue(data, out color) :
                       id2color.TryGetValue(id, out color)) 
            {
                r = (color >> 16 & 0xFF) / 255f;
                g = (color >> 8 & 0xFF) / 255f;
                b = (color & 0xFF) / 255f;
            }

            double minX = x & 0xF;
            double maxX = minX + 1;
            double minY = y;
            double maxY = y + (id == Blocks.carpet ? 0.0625 : 1);
            double minZ = z & 0xF;
            double maxZ = minZ + 1;
            if (ShouldRender(x, y - 1, z)) {
                t.Color(r - FC1, g - FC1, b - FC1);

                t.Vertex(minX, minY, maxZ);
                t.Vertex(minX, minY, minZ);
                t.Vertex(maxX, minY, minZ);
                t.Vertex(maxX, minY, maxZ);
            }
            if (ShouldRender(x, y + 1, z)) {
                t.Color(r - FC1, g - FC1, b - FC1);

                t.Vertex(maxX, maxY, maxZ);
                t.Vertex(maxX, maxY, minZ);
                t.Vertex(minX, maxY, minZ);
                t.Vertex(minX, maxY, maxZ);
            }
            if (ShouldRender(x, y, z - 1)) {
                t.Color(r - FC2, g - FC2, b - FC2);

                t.Vertex(minX, maxY, minZ);
                t.Vertex(maxX, maxY, minZ);
                t.Vertex(maxX, minY, minZ);
                t.Vertex(minX, minY, minZ);
            }
            if (ShouldRender(x, y, z + 1)) {
                t.Color(r - FC2, g - FC2, b - FC2);

                t.Vertex(minX, maxY, maxZ);
                t.Vertex(minX, minY, maxZ);
                t.Vertex(maxX, minY, maxZ);
                t.Vertex(maxX, maxY, maxZ);
            }
            if (ShouldRender(x - 1, y, z)) {
                t.Color(r - FC3, g - FC3, b - FC3);

                t.Vertex(minX, maxY, maxZ);
                t.Vertex(minX, maxY, minZ);
                t.Vertex(minX, minY, minZ);
                t.Vertex(minX, minY, maxZ);
            }
            if (ShouldRender(x + 1, y, z)) {
                t.Color(r - FC3, g - FC3, b - FC3);

                t.Vertex(maxX, minY, maxZ);
                t.Vertex(maxX, minY, minZ);
                t.Vertex(maxX, maxY, minZ);
                t.Vertex(maxX, maxY, maxZ);
            }
        }
        private bool ShouldRender(int x, int y, int z)
        {
            int id = World.GetBlock(x, y, z);
            return id == Blocks.air || id == Blocks.carpet;
        }

        private const float TEX_WIDTH = 512;
        private const float TEX_HEIGHT = 256;

        private static float[] pineLeafColor  = new float[] { 0.38f, 0.60f, 0.38f };
        private static float[] birchLeafColor = new float[] { 0.50f, 0.65f, 0.33f };
        private static float[] oakLeafColor   = new float[] { 0.28f, 0.71f, 0.09f };
        private static float[] grassColor     = new float[] { 0.51f, 0.76f, 0.27f };

        private void RenderBlockWithTex(Tessellator t, int x, int y, int z, int id, int data)
        {
            AABB bb = GetBlockAABB(World, x, y, z, id, data);
            if (bb == null) bb = new AABB(0, 0, 0, 1, 1, 1);

            if (bb.MaxY > 1) bb.MaxY = 1;

            if (id >= 8 && id <= 11 && World.GetBlock(x, y + 1, z) != id)
                bb.MaxY = 1 - BlockUtils.GetFluidHeightPercent(data);

            if (ShouldRenderTex(x, y - 1, z, id)) {
                t.Color(C1, C1, C1);
                RenderFaceWithTexture(t, x, y, z, id, data, bb, 0);
            }
            if (ShouldRenderTex(x, y + 1, z, id)) {
                t.Color(C1, C1, C1);
                RenderFaceWithTexture(t, x, y, z, id, data, bb, 1);
            }
            if (ShouldRenderTex(x, y, z - 1, id)) {
                t.Color(C2, C2, C2);
                RenderFaceWithTexture(t, x, y, z, id, data, bb, 2);
            }
            if (ShouldRenderTex(x, y, z + 1, id)) {
                t.Color(C2, C2, C2);
                RenderFaceWithTexture(t, x, y, z, id, data, bb, 3);
            }
            if (ShouldRenderTex(x - 1, y, z, id)) {
                t.Color(C3, C3, C3);
                RenderFaceWithTexture(t, x, y, z, id, data, bb, 4);
            }
            if (ShouldRenderTex(x + 1, y, z, id)) {
                t.Color(C3, C3, C3);
                RenderFaceWithTexture(t, x, y, z, id, data, bb, 5);
            }
        }

        private static void RenderAllFaces(Tessellator t, int x, int y, int z, int id, int data, AABB bounds)
        {
            t.Color(C1, C1, C1); RenderFaceWithTexture(t, x, y, z, id, data, bounds, 0);
            t.Color(C1, C1, C1); RenderFaceWithTexture(t, x, y, z, id, data, bounds, 1);
            t.Color(C2, C2, C2); RenderFaceWithTexture(t, x, y, z, id, data, bounds, 2);
            t.Color(C2, C2, C2); RenderFaceWithTexture(t, x, y, z, id, data, bounds, 3);
            t.Color(C3, C3, C3); RenderFaceWithTexture(t, x, y, z, id, data, bounds, 4);
            t.Color(C3, C3, C3); RenderFaceWithTexture(t, x, y, z, id, data, bounds, 5);
        }
        
        private static void RenderStairs(Tessellator t, int x, int y, int z, int id, int data)
        {
            double yStart = 0.5;
            double yMax = 1.0;

            AABB aabb1 = null;
            AABB aabb2 = null;

            bool isUpside = (data & 4) != 0;
            if (isUpside) {
                aabb1 = new AABB(0.0, 0.5, 0.0, 1.0, 1.0, 1.0);
                yStart = 0;
                yMax = 0.5;
            } else
                aabb1 = new AABB(0.0, 0.0, 0.0, 1.0, 0.5, 1.0);

            int dir = data & 3;

            if (dir == 0)      aabb2 = new AABB(0.5, yStart, 0.0, 1.0, yMax, 1.0);
            else if (dir == 1) aabb2 = new AABB(0.0, yStart, 0.0, 0.5, yMax, 1.0);
            else if (dir == 2) aabb2 = new AABB(0.0, yStart, 0.5, 1.0, yMax, 1.0);
            else if (dir == 3) aabb2 = new AABB(0.0, yStart, 0.0, 1.0, yMax, 0.5);

            for (int i = 0; i < 12; i++)
            {
                if ((isUpside && i == 7) || (!isUpside && i == 6)) continue;

                int face = i % 6;
                switch (face / 2)
                {
                    case 0: t.Color(C1, C1, C1); break;
                    case 1: t.Color(C2, C2, C2); break;
                    case 2: t.Color(C3, C3, C3); break;
                }
                RenderFaceWithTexture(t, x, y, z, id, data, i > 5 ? aabb2 : aabb1, face);
            }
        }
        private void RenderFence(Tessellator t, int x, int y, int z, int id, int data)
        {
            double xw = 0.375;
            double zw = 0.625;

            RenderAllFaces(t, x, y, z, id, data, new AABB(xw, 0, xw, zw, 1, zw));

            bool conWE = BlockUtils.CanFenceConnect(World, x - 1, y, z) || BlockUtils.CanFenceConnect(World, x + 1, y, z);
            bool conNS = BlockUtils.CanFenceConnect(World, x, y, z - 1) || BlockUtils.CanFenceConnect(World, x, y, z + 1);

            bool conWest = BlockUtils.CanFenceConnect(World, x - 1, y, z);
            bool conEast = BlockUtils.CanFenceConnect(World, x + 1, y, z);
            bool conNorth = BlockUtils.CanFenceConnect(World, x, y, z - 1);
            bool conSouth = BlockUtils.CanFenceConnect(World, x, y, z + 1);

            if (!conWE && !conNS) conWE = true;

            xw = 0.4375;
            zw = 0.5625;

            double minY = 0.75;
            double maxY = 0.9375;
            double minX = conWest ? 0.0 : xw;
            double minZ = conEast ? 1.0 : zw;
            double maxX = conNorth ? 0.0 : xw;
            double maxZ = conSouth ? 1.0 : zw;

            if (conWE) RenderAllFaces(t, x, y, z, id, data, new AABB(minX, minY, xw, minZ, maxY, zw));
            if (conNS) RenderAllFaces(t, x, y, z, id, data, new AABB(xw, minY, maxX, zw, maxY, maxZ));

            minY = 0.375;
            maxY = 0.5625;

            if (conWE) RenderAllFaces(t, x, y, z, id, data, new AABB(minX, minY, xw, minZ, maxY, zw));
            if (conNS) RenderAllFaces(t, x, y, z, id, data, new AABB(xw, minY, maxX, zw, maxY, maxZ));
        }

        private static void RenderCrossedCube(Tessellator t, int x, int y, int z, int id, int data)
        {
            if (id == Blocks.tallgrass || (id == Blocks.double_plant && (data & 0x7) / 2 == 1))
                t.Color(grassColor[0], grassColor[1], grassColor[2]);
            else if (id == Blocks.melon_stem || id == Blocks.pumpkin_stem)
                t.Color(0.75f, 0.81f, 0.09f);
            else
                t.Color(1.0f, 1.0f, 1.0f);

            Rectangle tex = TextureManager.GetBlockTexture(id, data, 2);
            int tx = tex.X;
            int ty = tex.Y;

            float u0 = tx / TEX_WIDTH;
            float u1 = (tx + tex.Width) / TEX_WIDTH;
            float v0 = ty / TEX_HEIGHT;
            float v1 = (ty + tex.Height) / TEX_HEIGHT;

            const int rots = 2;

            x &= 0xF;
            z &= 0xF;
            for (int r = 0; r < rots; r++)
            {
                double xa = Math.Sin(r * Math.PI / rots + (Math.PI / 4)) * 0.7;
                double za = Math.Cos(r * Math.PI / rots + (Math.PI / 4)) * 0.7;
                double x0 = x + 0.5 - xa;
                double x1 = x + 0.5 + xa;
                double y0 = y + 0.0;
                double y1 = y + 1.0;
                double z0 = z + 0.5 - za;
                double z1 = z + 0.5 + za;

                t.TexCoord(u1, v0);
                t.Vertex(x0, y1, z0);
                t.TexCoord(u0, v0);
                t.Vertex(x1, y1, z1);
                t.TexCoord(u0, v1);
                t.Vertex(x1, y0, z1);
                t.TexCoord(u1, v1);
                t.Vertex(x0, y0, z0);

                t.TexCoord(u1, v0);
                t.Vertex(x1, y1, z1);
                t.TexCoord(u0, v0);
                t.Vertex(x0, y1, z0);
                t.TexCoord(u0, v1);
                t.Vertex(x0, y0, z0);
                t.TexCoord(u1, v1);
                t.Vertex(x1, y0, z1);
            }
        }
        private static void RenderFaceWithTexture(Tessellator t, int x, int y, int z, int id, int data, AABB bb, int face)
        {
            int fid = 0;
            if ((fid = GetFenceWoodID(id)) != -1) {
                id = fid >> 4;
                data = fid & 0xF;
            }

            if (id == Blocks.log) data &= 3;
            else if (id == Blocks.wooden_slab || id == Blocks.stone_slab) data &= 7;
            
            Rectangle tex = TextureManager.GetBlockTexture(id, data, face);
            int tx = tex.X;//tex % 16 * 16;
            int ty = tex.Y;//tex / 16 * 16;

            x &= 0xF;
            z &= 0xF;
            double minX = x + bb.MinX;
            double maxX = x + bb.MaxX;
            double minY = y + bb.MinY;
            double maxY = y + bb.MaxY;
            double minZ = z + bb.MinZ;
            double maxZ = z + bb.MaxZ;

            if (id == Blocks.leaves) {
                switch (data & 0x3) {
                    case 1: t.Color(pineLeafColor[0], pineLeafColor[1], pineLeafColor[2]); break;
                    case 2: t.Color(birchLeafColor[0], birchLeafColor[1], birchLeafColor[2]); break;
                    default: t.Color(oakLeafColor[0], oakLeafColor[1], oakLeafColor[2]); break;
                }
            }
            else if(id == Blocks.leaves2)
                t.Color(oakLeafColor[0], oakLeafColor[1], oakLeafColor[2]);
            else if (id == Blocks.redstone_wire)
            {
                float power = data / 15.0F;
                float r = power * 0.6F + 0.4F;

                if (data == 0) r = 0.3F;

                float g = power * power * 0.7F - 0.5F;
                float b = power * power * 0.6F - 0.7F;

                if (g < 0.0F) g = 0.0F;
                if (b < 0.0F) b = 0.0F;

                t.Color(r, g, b);
            }
            else if ((id == Blocks.grass && face == 1) || id == Blocks.vine)
                t.Color(grassColor[0], grassColor[1], grassColor[2]);
            
            if (face == 0) {
                float u0 = (tx + (float)(bb.MinX * 16)) / TEX_WIDTH;
                float u2 = (tx + (float)(bb.MaxX * 16)) / TEX_WIDTH;
                float v0 = (ty + (float)(bb.MinZ * 16)) / TEX_HEIGHT;
                float v2 = (ty + (float)(bb.MaxZ * 16)) / TEX_HEIGHT;

                double tmp = minY;
                if(id == Blocks.cauldron)
                    tmp = y + 0.125;

                t.TexCoord(u0, v2);
                t.Vertex(minX, tmp, maxZ);
                t.TexCoord(u0, v0);
                t.Vertex(minX, tmp, minZ);
                t.TexCoord(u2, v0);
                t.Vertex(maxX, tmp, minZ);
                t.TexCoord(u2, v2);
                t.Vertex(maxX, tmp, maxZ);
            }
            if (face == 1) {
                float u0 = (tx + (float)(bb.MinX * 16)) / TEX_WIDTH;
                float u2 = (tx + (float)(bb.MaxX * 16)) / TEX_WIDTH;
                float v0 = (ty + (float)(bb.MinZ * 16)) / TEX_HEIGHT;
                float v2 = (ty + (float)(bb.MaxZ * 16)) / TEX_HEIGHT;

                double tmp = maxY;
                if (id == Blocks.enchanting_table || id == Blocks.end_portal_frame)
                    tmp = y + 0.75;

                t.TexCoord(u2, v2);
                t.Vertex(maxX, tmp, maxZ);
                t.TexCoord(u2, v0);
                t.Vertex(maxX, tmp, minZ);
                t.TexCoord(u0, v0);
                t.Vertex(minX, tmp, minZ);
                t.TexCoord(u0, v2);
                t.Vertex(minX, tmp, maxZ);
            }
            if (face == 2) {
                float u0 = (tx + (float)(bb.MinX * 16)) / TEX_WIDTH;
                float u2 = (tx + (float)(bb.MaxX * 16)) / TEX_WIDTH;
                float v0 = (ty + (float)(bb.MinY * 16)) / TEX_HEIGHT;
                float v2 = (ty + (float)(bb.MaxY * 16)) / TEX_HEIGHT;
                t.TexCoord(u2, v0);
                t.Vertex(minX, maxY, minZ);
                t.TexCoord(u0, v0);
                t.Vertex(maxX, maxY, minZ);
                t.TexCoord(u0, v2);
                t.Vertex(maxX, minY, minZ);
                t.TexCoord(u2, v2);
                t.Vertex(minX, minY, minZ);
            }
            if (face == 3) {
                float u0 = (tx + (float)(bb.MinX * 16)) / TEX_WIDTH;
                float u2 = (tx + (float)(bb.MaxX * 16)) / TEX_WIDTH;
                float v0 = (ty + (float)(bb.MinY * 16)) / TEX_HEIGHT;
                float v2 = (ty + (float)(bb.MaxY * 16)) / TEX_HEIGHT;
                t.TexCoord(u0, v0);
                t.Vertex(minX, maxY, maxZ);
                t.TexCoord(u0, v2);
                t.Vertex(minX, minY, maxZ);
                t.TexCoord(u2, v2);
                t.Vertex(maxX, minY, maxZ);
                t.TexCoord(u2, v0);
                t.Vertex(maxX, maxY, maxZ);
            }
            if (face == 4) {
                float u0 = (tx + (float)(bb.MinZ * 16)) / TEX_WIDTH;
                float u2 = (tx + (float)(bb.MaxZ * 16)) / TEX_WIDTH;
                float v0 = (ty + (float)(bb.MinY * 16)) / TEX_HEIGHT;
                float v2 = (ty + (float)(bb.MaxY * 16)) / TEX_HEIGHT;
                t.TexCoord(u2, v0);
                t.Vertex(minX, maxY, maxZ);
                t.TexCoord(u0, v0);
                t.Vertex(minX, maxY, minZ);
                t.TexCoord(u0, v2);
                t.Vertex(minX, minY, minZ);
                t.TexCoord(u2, v2);
                t.Vertex(minX, minY, maxZ);
            }
            if (face == 5) {
                float u0 = (tx + (float)(bb.MinZ * 16)) / TEX_WIDTH;
                float u2 = (tx + (float)(bb.MaxZ * 16)) / TEX_WIDTH;
                float v0 = (ty + (float)(bb.MinY * 16)) / TEX_HEIGHT;
                float v2 = (ty + (float)(bb.MaxY * 16)) / TEX_HEIGHT;
                t.TexCoord(u0, v2);
                t.Vertex(maxX, minY, maxZ);
                t.TexCoord(u2, v2);
                t.Vertex(maxX, minY, minZ);
                t.TexCoord(u2, v0);
                t.Vertex(maxX, maxY, minZ);
                t.TexCoord(u0, v0);
                t.Vertex(maxX, maxY, maxZ);
            }
        }

        private bool ShouldRenderTex(int x, int y, int z, int cid)
        {
            int id = World.GetBlock(x, y, z);
            if ((id >= 8 && id <= 11) && cid != id) return true;

            if (BlockUtils.IsStairs(id)) return true;
            return ShouldRenderBlock(id);
        }
        private static bool ShouldRenderBlock(int id)
        {
            switch (id)
            {
                case Blocks.air:
                case Blocks.carpet:
                case Blocks.ladder:
                case Blocks.glass:
                case Blocks.wooden_door:
                case Blocks.iron_door:
                case Blocks.torch:
                case Blocks.fence:
                case Blocks.acacia_fence:
                case Blocks.birch_fence:
                case Blocks.dark_oak_fence:
                case Blocks.jungle_fence:
                case Blocks.spruce_fence:
                case Blocks.nether_brick_fence:
                case Blocks.fence_gate:
                case Blocks.acacia_fence_gate:
                case Blocks.birch_fence_gate:
                case Blocks.dark_oak_fence_gate:
                case Blocks.jungle_fence_gate:
                case Blocks.spruce_fence_gate:
                case Blocks.leaves:
                case Blocks.leaves2:
                case Blocks.sapling:
                case Blocks.tallgrass:
                case Blocks.red_flower:
                case Blocks.yellow_flower:
                case Blocks.double_plant:
                case Blocks.vine:
                case Blocks.stone_slab:
                case Blocks.wooden_slab:
                case Blocks.cobblestone_wall:
                case Blocks.wall_sign:
                case Blocks.standing_sign:
                case Blocks.reeds:
                case Blocks.nether_wart:
                case Blocks.web:
                case Blocks.soul_sand:
                case Blocks.redstone_wire:
                case Blocks.skull:
                case Blocks.stone_button:
                case Blocks.wooden_button:
                case Blocks.trapdoor:
                case Blocks.flower_pot:
                case Blocks.wheat:
                case Blocks.carrots:
                case Blocks.potatoes:
                case Blocks.glass_pane:
                case Blocks.stained_glass_pane:
                case Blocks.stained_glass:
                case Blocks.iron_bars:
                case Blocks.rail:
                case Blocks.golden_rail:
                case Blocks.detector_rail:
                case Blocks.activator_rail:
                case Blocks.redstone_torch:
                case Blocks.bed:
                case Blocks.red_mushroom:
                case Blocks.brown_mushroom:
                case Blocks.deadbush:
                case Blocks.lever:
                case Blocks.fire:
                case Blocks.heavy_weighted_pressure_plate:
                case Blocks.light_weighted_pressure_plate:
                case Blocks.stone_pressure_plate:
                case Blocks.wooden_pressure_plate:
                case Blocks.snow_layer:
                case Blocks.barrier:
                case Blocks.mob_spawner:
                case Blocks.slime:
                    return true;
                default: return false;
            }
        }

        private static int GetFenceWoodID(int id)
        {
            switch (id)
            {
                case Blocks.fence: return Blocks.planks << 4;
                case Blocks.acacia_fence: return Blocks.planks << 4 | 4;
                case Blocks.birch_fence: return Blocks.planks << 4 | 2;
                case Blocks.dark_oak_fence: return Blocks.planks << 4 | 5;
                case Blocks.jungle_fence: return Blocks.planks << 4 | 3;
                case Blocks.spruce_fence: return Blocks.planks << 4 | 1;
                case Blocks.nether_brick_fence: return Blocks.nether_brick << 4;
                default: return -1;
            }
        }
        private static AABB GetBlockAABB(World w, int x, int y, int z, int id, int data)
        {
            AABB result = null;
            //byte id = w.GetBlock(x, y, z);
            //byte data = w.GetData(x, y, z);

            switch (id)
            {
                case Blocks.stone_slab:
                case Blocks.wooden_slab:
                case Blocks.stone_slab2:
                case Blocks.purpur_slab:
                    if ((data & 8) != 0) //top?
                        result = new AABB(0.0, 0.5, 0.0, 1.0, 1.0, 1.0);
                    else
                        result = new AABB(0.0, 0.0, 0.0, 1.0, 0.5, 1.0);
                    break;
                case Blocks.bed: result = new AABB(0.0, 0.0, 0.0, 1.0, 0.5625, 1.0); break;
                case Blocks.wooden_door:
                case Blocks.iron_door:
                case Blocks.acacia_door:
                case Blocks.birch_door:
                case Blocks.dark_oak_door:
                case Blocks.jungle_door:
                case Blocks.spruce_door:
                    {
                        const double thick = 0.1875;

                        AABB aabb = null;
                        byte flags = BlockUtils.GetFullDoorMeta(w, x, y, z);
                        int facing = flags & 3;
                        bool isOpen = (flags & 4) != 0;
                        bool isHinge = (flags & 16) != 0;

                        if (facing == 0) {
                            if (isOpen)
                                aabb = isHinge ? new AABB(0.0, 0.0, 1.0 - thick, 1.0, 1.0, 1.0) : new AABB(0.0, 0.0, 0.0, 1.0, 1.0, thick);
                            else
                                aabb = new AABB(0.0, 0.0, 0.0, thick, 1.0, 1.0);
                        } else if (facing == 1) {
                            if (isOpen)
                                aabb = isHinge ? new AABB(0.0, 0.0, 0.0, thick, 1.0, 1.0) : new AABB(1.0 - thick, 0.0, 0.0, 1.0, 1.0, 1.0);
                            else
                                aabb = new AABB(0.0, 0.0, 0.0, 1.0, 1.0, thick);
                        } else if (facing == 2) {
                            if (isOpen)
                                aabb = isHinge ? new AABB(0.0, 0.0, 0.0, 1.0, 1.0, thick) : new AABB(0.0, 0.0, 1.0 - thick, 1.0, 1.0, 1.0);
                            else
                                aabb = new AABB(1.0 - thick, 0.0, 0.0, 1.0, 1.0, 1.0);
                        } else if (facing == 3) {
                            if (isOpen)
                                aabb = isHinge ? new AABB(1.0 - thick, 0.0, 0.0, 1.0, 1.0, 1.0) : new AABB(0.0, 0.0, 0.0, thick, 1.0, 1.0);
                            else
                                aabb = new AABB(0.0, 0.0, 1.0 - thick, 1.0, 1.0, 1.0);
                        }
                        result = aabb;
                    }
                    break;
                case Blocks.ladder:
                    {
                        const double thick = 0.125;

                        switch (data)
                        {
                            case 2: result = new AABB(0.0, 0.0, 1.0 - thick, 1.0, 1.0, 1.0); break;
                            case 3: result = new AABB(0.0, 0.0, 0.0, 1.0, 1.0, thick); break;
                            case 4: result = new AABB(1.0 - thick, 0.0, 0.0, 1.0, 1.0, 1.0); break;
                            case 5: result = new AABB(0.0, 0.0, 0.0, thick, 1.0, 1.0); break;
                            default: result = null; break;
                        }
                    }
                    break;
                case Blocks.vine:
                    {
                        float minX = 1.0F;
                        float minY = 1.0F;
                        float minZ = 1.0F;
                        float maxX = 0.0F;
                        float maxY = 0.0F;
                        float maxZ = 0.0F;

                        if ((data & 2) != 0)
                        {
                            maxX = Math.Max(maxX, 0.0625F);
                            minX = 0.0F;
                            minY = 0.0F;
                            maxY = 1.0F;
                            minZ = 0.0F;
                            maxZ = 1.0F;
                        }

                        if ((data & 8) != 0)
                        {
                            minX = Math.Min(minX, 0.9375F);
                            maxX = 1.0F;
                            minY = 0.0F;
                            maxY = 1.0F;
                            minZ = 0.0F;
                            maxZ = 1.0F;
                        }

                        if ((data & 4) != 0)
                        {
                            maxZ = Math.Max(maxZ, 0.0625F);
                            minZ = 0.0F;
                            minX = 0.0F;
                            maxX = 1.0F;
                            minY = 0.0F;
                            maxY = 1.0F;
                        }

                        if ((data & 1) != 0)
                        {
                            minZ = Math.Min(minZ, 0.9375F);
                            maxZ = 1.0F;
                            minX = 0.0F;
                            maxX = 1.0F;
                            minY = 0.0F;
                            maxY = 1.0F;
                        }
                        result = new AABB(minX, minY, minZ, maxX, maxY, maxZ);
                    }
                    break;
                case Blocks.snow_layer: result = new AABB(0.0, 0.0, 0.0, 1.0, (data & 7) * 0.125, 1.0); break;
                case Blocks.soul_sand:  result = new AABB(0.0, 0.0, 0.0, 1.0, 0.875, 1.0); break;
                case Blocks.cake:
                    const double length = 0.0625;
                    result = new AABB((1 + data * 2) / 16.0, 0.0, length, 1.0 - length, 0.5, 1.0 - length);
                    break;
                case Blocks.powered_repeater:
                case Blocks.unpowered_repeater:
                case Blocks.powered_comparator:
                case Blocks.unpowered_comparator:
                    result = new AABB(0.0, 0.0, 0.0, 1.0, 0.125, 1.0);
                    break;
                case Blocks.trapdoor:
                case Blocks.iron_trapdoor:
                    {
                        double height = 0.1875;
                        AABB aabb;
                        if ((data & 8) != 0)
                            aabb = new AABB(0.0, 1.0 - height, 0.0, 1.0, 1.0, 1.0);
                        else
                            aabb = new AABB(0.0, 0.0, 0.0, 1.0, height, 1.0);

                        if ((data & 4) != 0)
                        {
                            int d = data & 3;
                            if (d == 0) aabb = new AABB(0.0, 0.0, 1.0 - height, 1.0, 1.0, 1.0);
                            if (d == 1) aabb = new AABB(0.0, 0.0, 0.0, 1.0, 1.0, height);
                            if (d == 2) aabb = new AABB(1.0 - height, 0.0, 0.0, 1.0, 1.0, 1.0);
                            if (d == 3) aabb = new AABB(0.0, 0.0, 0.0, height, 1.0, 1.0);
                        }
                        result = aabb;
                    }
                    break;
                case Blocks.fence_gate:
                    if (BlockUtils.IsFenceGateOpen(data)) result = null;
                    result = (data != 2 && data != 0 ? new AABB(0.375, 0.0, 0.0, 0.625, 1.5, 1.0) :
                                                       new AABB(0.0, 0.0, 0.375, 1.0, 1.5, 0.625));
                    break;
                case Blocks.waterlily: result = new AABB(0.0, 0.0, 0.0, 1.0, 0.015625, 1.0); break;
                case Blocks.fence:
                case Blocks.acacia_fence:
                case Blocks.birch_fence:
                case Blocks.dark_oak_fence:
                case Blocks.jungle_fence:
                case Blocks.spruce_fence:
                case Blocks.nether_brick_fence:
                    {
                        double minX = 0.375;
                        double maxX = 0.625;
                        double minZ = 0.375;
                        double maxZ = 0.625;

                        if (BlockUtils.CanFenceConnect(w, x, y, z - 1)) minZ = 0.0;
                        if (BlockUtils.CanFenceConnect(w, x, y, z + 1)) maxZ = 1.0;
                        if (BlockUtils.CanFenceConnect(w, x - 1, y, z)) minX = 0.0;
                        if (BlockUtils.CanFenceConnect(w, x + 1, y, z)) maxX = 1.0;

                        result = new AABB(minX, 0.0, minZ, maxX, 1.5, maxZ);
                    }
                    break;
                case Blocks.cobblestone_wall:
                    {
                        bool connectNorth = BlockUtils.CanWallConnect(w, x, y, z - 1);
                        bool connectSouth = BlockUtils.CanWallConnect(w, x, y, z + 1);
                        bool connectWest = BlockUtils.CanWallConnect(w, x - 1, y, z);
                        bool connectEast = BlockUtils.CanWallConnect(w, x + 1, y, z);
                        double minX = 0.25;
                        double maxX = 0.75;
                        double minZ = 0.25;
                        double maxZ = 0.75;

                        if (connectNorth) minZ = 0.0;
                        if (connectSouth) maxZ = 1.0;
                        if (connectWest) minX = 0.0;
                        if (connectEast) maxX = 1.0;

                        if (connectNorth && connectSouth && !connectWest && !connectEast) {
                            minX = 0.3125;
                            maxX = 0.6875;
                        } else if (!connectNorth && !connectSouth && connectWest && connectEast) {
                            minZ = 0.3125;
                            maxZ = 0.6875;
                        }

                        result = new AABB(minX, 0, minZ, maxX, 1.5, maxZ);
                    }
                    break;
                case Blocks.flower_pot:
                    double width = 0.375 / 2.0;
                    result = new AABB(0.5 - width, 0.0, 0.5 - width, 0.5 + width, 0.375, 0.5 + width);
                    break;
                case Blocks.skull:
                    switch (data & 7)
                    {
                        case 1:
                        default:
                            result = new AABB(0.25, 0.0, 0.25, 0.75, 0.5, 0.75); break;
                        case 2: result = new AABB(0.25, 0.25, 0.5, 0.75, 0.75, 1.0); break;
                        case 3: result = new AABB(0.25, 0.25, 0.0, 0.75, 0.75, 0.5); break;
                        case 4: result = new AABB(0.5, 0.25, 0.25, 1.0, 0.75, 0.75); break;
                        case 5: result = new AABB(0.0, 0.25, 0.25, 0.5, 0.75, 0.75); break;
                    }
                    break;
                case Blocks.anvil:
                    int j = data & 3;
                    if (j != 3 && j != 1)
                        result = new AABB(0.125, 0.0, 0.0, 0.875, 1.0, 1.0);
                    else
                        result = new AABB(0.0, 0.0, 0.125, 1.0, 1.0, 0.875);
                    break;
                case Blocks.carpet: result = new AABB(0.0, 0.0, 0.0, 1.0, 0.0625, 1.0); break;
                case Blocks.daylight_detector_inverted:
                case Blocks.daylight_detector: result = new AABB(0.0F, 0.0F, 0.0F, 1.0F, 0.375F, 1.0F); break;
                case Blocks.chest:
                case Blocks.trapped_chest:
                    if (w.GetBlock(x, y, z - 1) == id)
                        result = new AABB(0.0625F, 0.0F, 0.0F, 0.9375F, 0.875F, 0.9375F);
                    else if (w.GetBlock(x, y, z + 1) == id)
                        result = new AABB(0.0625F, 0.0F, 0.0625F, 0.9375F, 0.875F, 1.0F);
                    else if (w.GetBlock(x - 1, y, z) == id)
                        result = new AABB(0.0F, 0.0F, 0.0625F, 0.9375F, 0.875F, 0.9375F);
                    else if (w.GetBlock(x + 1, y, z) == id)
                        result = new AABB(0.0625F, 0.0F, 0.0625F, 1.0F, 0.875F, 0.9375F);
                    else
                        result = new AABB(0.0625F, 0.0F, 0.0625F, 0.9375F, 0.875F, 0.9375F);
                    break;
                case Blocks.ender_chest: result = new AABB(0.0625F, 0.0F, 0.0625F, 0.9375F, 0.875F, 0.9375F); break;
                case Blocks.wall_sign:
                    if (data == 2) result = new AABB(0.0, 0.28125, 1.0F - 0.0625, 1.0, 0.78125, 1.0F);
                    else if (data == 3) result = new AABB(0.0, 0.28125, 0.0F, 1.0, 0.78125, 0.0625);
                    else if (data == 4) result = new AABB(1.0F - 0.0625, 0.28125, 0.0, 1.0F, 0.78125, 1.0);
                    else if (data == 5) result = new AABB(0.0F, 0.28125, 0.0, 0.0625, 0.78125, 1.0);
                    else result = new AABB(0.0, 0.0, 0.0, 1.0, 1.0, 1.0);
                    break;
                case Blocks.redstone_wire:
                case Blocks.rail:
                case Blocks.golden_rail:
                case Blocks.detector_rail:
                case Blocks.activator_rail:
                    result = new AABB(0.0, 0.0, 0.0, 1.0, 0.0625, 1.0);
                    break;
                case Blocks.wooden_button:
                case Blocks.stone_button:
                    int var2 = data & 7;
                    float var4 = 0.375F;
                    float var5 = 0.625F;
                    float var6 = 0.1875F;
                    float var7 = 0.125F;

                    if ((data & 8) > 0) var7 = 0.0625F;

                    if (var2 == 1) result = new AABB(0.0F, var4, 0.5F - var6, var7, var5, 0.5F + var6);
                    else if (var2 == 2) result = new AABB(1.0F - var7, var4, 0.5F - var6, 1.0F, var5, 0.5F + var6);
                    else if (var2 == 3) result = new AABB(0.5F - var6, var4, 0.0F, 0.5F + var6, var5, var7);
                    else if (var2 == 4) result = new AABB(0.5F - var6, var4, 1.0F - var7, 0.5F + var6, var5, 1.0F);
                    break;
                case Blocks.torch:
                case Blocks.redstone_torch:
                    const double t = 1 / 16.0;
                    result = new AABB(0.5 - t, 0.0, 0.5 - t, 0.5 + t, 1.0, 0.5 + t);
                    break;
                case Blocks.heavy_weighted_pressure_plate:
                case Blocks.light_weighted_pressure_plate:
                case Blocks.stone_pressure_plate:
                case Blocks.wooden_pressure_plate:
                    {
                        const double thick = 1 / 16.0;
                        result = new AABB(thick, 0.0, thick, 1.0 - thick, thick, 1.0 - thick);
                    }
                    break;
                case Blocks.cactus:
                    {
                        const double thick = 1 / 16.0;
                        return new AABB(thick, 0.0, thick, 1.0 - thick, 1.0 - thick, 1.0 - thick);
                    }
                default: result = new AABB(0.0, 0.0, 0.0, 1.0, 1.0, 1.0); break;
            }
            return result;
        }
        #endregion
    }
}
