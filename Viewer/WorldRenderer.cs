using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using AdvancedBot.client.Map;
using AdvancedBot.client;

namespace AdvancedBot.Viewer
{
    public class WorldRenderer
    {
        private World world;

        private Dictionary<long, ChunkRenderer> chunks = new Dictionary<long, ChunkRenderer>();
        private List<ChunkRenderer> dirtyChunks = new List<ChunkRenderer>();

        private DateTime lastTick = DateTime.UtcNow;

        private bool delAllDirty = false;
        private World newWorld;

        public WorldRenderer(World w)
        {
            world = w;
        }

        public const int MAX_CHUNK_DIST = 16;
        const int MAX_CHUNK_UPDATES_PER_TICK = 6;
        
        private double camX, camY, camZ;

        public void Tick(double x, double y, double z)
        {
            camX = x; camY = y; camZ = z;

            if (newWorld != null) {
                foreach (ChunkRenderer c in chunks.Values) {
                    c.Delete();
                }

                chunks.Clear();
                dirtyChunks.Clear();
                
                world = newWorld;
                newWorld = null;
            }
            if (delAllDirty) {
                foreach (ChunkRenderer renderer in dirtyChunks) {
                    renderer.Delete();
                }
                delAllDirty = false;
            }
            

            if ((DateTime.UtcNow - lastTick).TotalMilliseconds > 600) {
                lastTick = DateTime.UtcNow;
                if (dirtyChunks.Count > 4) {
                    dirtyChunks.RemoveAll(c => c == null);
                    dirtyChunks.Sort(ChunkComparer);
                }
                DeleteFarChunks(x, y, z);
            }
            int l = Math.Min(dirtyChunks.Count, MAX_CHUNK_UPDATES_PER_TICK);
            for (int i = 0; i < l; i++) {
                dirtyChunks[i].Rebuild();
            }
            dirtyChunks.RemoveRange(0, l);

            int cX = FloorDiv((int)x, 16);
            int cZ = FloorDiv((int)z, 16);

            int d = ViewerConfig.RenderDist;
            for (int cx = cX - d; cx <= cX + d; cx++) {
                for (int cz = cZ - d; cz <= cZ + d; cz++) {
                    for (int cy = 0; cy < 16; cy++) {
                        if (!chunks.ContainsKey(PosKey(cx, cy, cz)) && world.ChunkExists(cx, cz)) {
                            chunks[PosKey(cx, cy, cz)] = new ChunkRenderer(world, cx, cy, cz);
                        }
                    }
                }
            }
        }

        public int Render(double x, double y, double z)
        {
            camX = x; camY = y; camZ = z;
            var frustum = Frustum.CalculateAndGet(-x, -y, -z);

            int viewDist = ViewerConfig.RenderDist * 16;

            int nvert = 0;

            foreach (ChunkRenderer chunk in chunks.Values.OrderByDescending(a => a.DistToSq(x, y, z))) {
                double dist = chunk.DistTo(x, y, z);
                if (dist < viewDist && chunk.InFrustum(frustum)) {
                    if (!chunk.Built && !dirtyChunks.Contains(chunk)) {
                        dirtyChunks.Add(chunk);
                    } else if(chunk.Vertices > 0) {
                        GL.glPushMatrix();
                        GL.glTranslated(chunk.X * 16 - x, -y, chunk.Z * 16 - z);
                        chunk.Render();
                        nvert += chunk.Vertices;
                        GL.glPopMatrix();
                    }
                }
            }

            return nvert;
        }
        public void RenderSigns(Font fnt)
        {
            var frustum = Frustum.Instance;

            foreach (KeyValuePair<Vec3i, string[]> sign in world.Signs) {
                Vec3i pos = sign.Key;
                world.GetBlockAndData(pos.X, pos.Y, pos.Z, out byte id, out byte data);

                if ((/*id != Blocks.standing_sign && */id != Blocks.wall_sign) ||
                    Utils.DistToSq(pos.X + 0.5, pos.Y + 0.5, pos.Z + 0.5, camX, camY, camZ) > 64 * 64 ||
                    sign.Value.All(a => string.IsNullOrEmpty(a)) ||
                    !frustum.CubeInFrustum(pos.X, pos.Y, pos.Z, pos.X + 1, pos.Y + 1, pos.Z + 1)) continue;

                GL.glPushMatrix();
                GL.glTranslated(-camX + (pos.X + 0.5), -camY + (pos.Y + 0.75), -camZ + (pos.Z + 0.5));

                if (id == Blocks.standing_sign) {
                    GL.glRotatef(-((data * 360f) / 16f), 0, 1, 0);
                } else {
                    float ang = data == 2 ? 180 :
                                data == 4 ? 90 :
                                data == 5 ? -90 : 0;

                    GL.glRotatef(-ang, 0, 1, 0);
                    GL.glTranslated(0.0, 0.0/*-0.3125*/, -0.43);
                }

                GL.glScalef(0.0105f, -0.0105f, 0.0105f);

                string[] lns = sign.Value;
                for (int i = 0, l = lns.Length; i < l; i++) {
                    string txt = lns[i];
                    fnt.Draw(txt, -(fnt.Measure(txt) / 2), i * 10, true);
                }
                GL.glPopMatrix();
            }
        }

        private void DeleteFarChunks(double x, double y, double z)
        {
            List<long> r = null;

            const int BUFFER_LIMIT = 2 * 16;
            int maxDist = (ViewerConfig.RenderDist * 16) + BUFFER_LIMIT;
            foreach (ChunkRenderer c in chunks.Values) {
                if (c.DistTo(x, y, z) > maxDist) {
                    c.Delete();
                    if (r == null) {
                        r = new List<long>();
                    }
                    r.Add(PosKey(c.X, c.Y, c.Z));
                }
            }
            if (r != null) {
                foreach (long key in r) {
                    chunks.Remove(key);
                }
            }
        }
        private int ChunkComparer(ChunkRenderer a, ChunkRenderer b)
        {
            double aDist = a.DistTo(camX, camY, camZ);
            double bDist = b.DistTo(camX, camY, camZ);

            var frustum = Frustum.Instance;
            if (!a.InFrustum(frustum)) aDist += 512;
            if (!b.InFrustum(frustum)) bDist += 512;

            return aDist.CompareTo(bDist);
        }

        public void SetAllDirty(bool deleteAll = false)
        {
            dirtyChunks.Clear();
            dirtyChunks.AddRange(chunks.Values);
            delAllDirty = deleteAll;
        }
        public void SetWorld(World w)
        {
            newWorld = w;
        }

        public void BlockChange(int x, int y, int z)
        {
            SetDirty(x - 1, y - 1, z - 1, x + 1, y + 1, z + 1);
        }
        public void ChunkChange(int x, int z)
        {
            for (int y = 0; y < 16; y++) {
                if (chunks.TryGetValue(PosKey(x, y, z), out ChunkRenderer c)) {
                    if (!dirtyChunks.Contains(c)) {
                        dirtyChunks.Add(c);
                    }
                }
            }
        }
        private void SetDirty(int minX, int minY, int minZ, int maxX, int maxY, int maxZ)
        {
            minX = FloorDiv(minX, 16);
            maxX = FloorDiv(maxX, 16);
            minY = FloorDiv(minY, 16);
            maxY = FloorDiv(maxY, 16);
            minZ = FloorDiv(minZ, 16);
            maxZ = FloorDiv(maxZ, 16);

            for (int x = minX; x <= maxX; x++) {
                for (int y = minY; y <= maxY; y++) {
                    for (int z = minZ; z <= maxZ; z++) {
                        if (chunks.TryGetValue(PosKey(x, y, z), out var c)) {
                            if (!dirtyChunks.Contains(c)) {
                                dirtyChunks.Add(c);
                            }
                        }
                    }
                }
            }
        }
        public void CleanUp()
        {
            foreach (KeyValuePair<long, ChunkRenderer> chunk in chunks) {
                chunk.Value.Delete();
            }
            chunks.Clear();
        }

        public static long PosKey(int x, int y, int z)
        {
            return (x & 0xFFFFFFL) << 40 |
                   (y & 0xFFFFL) << 24 |
                   (z & 0xFFFFFFL);
        }
        private static int FloorDiv(int x, int y)
        {
            return (int)Math.Floor(x / (double)y);
        }
    }
}
