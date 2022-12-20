using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdvancedBot.client.PathFinding;

namespace AdvancedBot.client.Map
{
    public class World
    {
        public MinecraftClient Owner;
        public World(MinecraftClient c)
        {
            Owner = c;
        }

        public delegate void UpdateEventDelegate(Vec3i pos, WorldUpdateEventType type);
        public event UpdateEventDelegate OnUpdate;

        public ConcurrentDictionary<long, Chunk> Chunks = new ConcurrentDictionary<long, Chunk>(2, 523, ChunkPosComparer.Instance);
        public ConcurrentDictionary<Vec3i, string[]> Signs = new ConcurrentDictionary<Vec3i, string[]>();

        public void SetBlockAndData(int x, int y, int z, byte id, byte data)
        {
            if (x >= -30000000 && z >= -30000000 && x < 30000000 && z < 30000000 && y >= 0 && y < 256) {
                Chunk c = GetChunk(x >> 4, z >> 4);

                if (c != null) {
                    if (c.Sections[y >> 4] == null) {
                        c.Sections[y >> 4] = new ChunkSection();
                    }

                    c.SetBlock(x & 0xF, y, z & 0xF, id);
                    c.SetData(x & 0xF, y, z & 0xF, data);
                    OnUpdate?.Invoke(new Vec3i(x, y, z), WorldUpdateEventType.Block);
                }
            }
        }

        public byte GetBlock(int x, int y, int z)
        {
            if (x >= -30000000 && z >= -30000000 && x < 30000000 && z < 30000000 && y >= 0 && y < 256) {
                Chunk c = GetChunk(x >> 4, z >> 4);
                if (c != null) {
                    return c.GetBlock(x & 0xF, y, z & 0xF);
                } else {
                    return 0;
                }
            }
            return 0;
        }
        public void GetBlockAndData(int x, int y, int z, out byte id, out byte data)
        {
            id = data = 0;
            if (x >= -30000000 && z >= -30000000 && x < 30000000 && z < 30000000 && y >= 0 && y < 256) {
                Chunk c = GetChunk(x >> 4, z >> 4);
                if (c != null) {
                    id = c.GetBlock(x & 0xF, y, z & 0xF);
                    data = c.GetData(x & 0xF, y, z & 0xF);
                }
            }
        }

        public void SetBlock(int x, int y, int z, byte id)
        {
            if (x >= -30000000 && z >= -30000000 && x < 30000000 && z < 30000000 && y >= 0 && y < 256) {
                Chunk c = GetChunk(x >> 4, z >> 4);
                if (c != null) {
                    c.SetBlock(x & 0xF, y, z & 0xF, id);
                    OnUpdate?.Invoke(new Vec3i(x, y, z), WorldUpdateEventType.Block);
                }
            }
        }

        public byte GetData(int x, int y, int z)
        {
            if (x >= -30000000 && z >= -30000000 && x < 30000000 && z < 30000000 && y >= 0 && y < 256) {
                Chunk c = GetChunk(x >> 4, z >> 4);
                if (c != null)
                    return c.GetData(x & 0xF, y, z & 0xF);
                else
                    return 0;
            }
            return 0;
        }
        public void SetData(int x, int y, int z, byte data)
        {
            if (x >= -30000000 && z >= -30000000 && x < 30000000 && z < 30000000 && y >= 0 && y < 256) {
                Chunk c = GetChunk(x >> 4, z >> 4);
                if (c != null)
                    c.SetData(x & 0xF, y, z & 0xF, data);
            }
        }

        public bool ChunkExists(int x, int z)
        {
            return Chunks.ContainsKey(ChunkPosKey(x, z));
        }
        public void SetChunk(int x, int z, Chunk c)
        {
            long cPos = ChunkPosKey(x, z);
            if (c != null) {
                Chunks[cPos] = c;
            } else {
                Chunks.TryRemove(cPos, out _);
            }
            OnUpdate?.Invoke(new Vec3i(x, 0, z), WorldUpdateEventType.Chunk);
        }
        public Chunk GetChunk(int x, int z)
        {
            Chunks.TryGetValue(ChunkPosKey(x, z), out Chunk c);
            return c;
        }
        public void FireChunkUpdate(int x, int z)
        {
            OnUpdate?.Invoke(new Vec3i(x, 0, z), WorldUpdateEventType.Chunk);
        }

        public void Clear()
        {
            OnUpdate?.Invoke(Vec3i.Empty, WorldUpdateEventType.World);
            Chunks.Clear();
            Signs.Clear();
        }

        public List<AABB> GetCollisionBoxes(AABB aabb)
        {
            List<AABB> boxes = new List<AABB>();
            int minX = Utils.Floor(aabb.MinX);
            int maxX = Utils.Floor(aabb.MaxX + 1.0);
            int minY = Utils.Floor(aabb.MinY);
            int maxY = Utils.Floor(aabb.MaxY + 1.0);
            int minZ = Utils.Floor(aabb.MinZ);
            int maxZ = Utils.Floor(aabb.MaxZ + 1.0);
            for (int x = minX; x < maxX; x++) {
                for (int z = minZ; z < maxZ; z++) {
                    for (int y = minY - 1; y < maxY; y++) {
                        if (IsSolid(x, y, z)) {
                            BlockUtils.AddAABBsToList(this, x, y, z, boxes);
                        }
                    }
                }
            }
            return boxes;
        }
        public bool IsAnyBlockInBB(AABB aabb, params byte[] ids)
        {
            int minX = Utils.Floor(aabb.MinX);
            int maxX = Utils.Floor(aabb.MaxX + 1.0);
            int minY = Utils.Floor(aabb.MinY);
            int maxY = Utils.Floor(aabb.MaxY + 1.0);
            int minZ = Utils.Floor(aabb.MinZ);
            int maxZ = Utils.Floor(aabb.MaxZ + 1.0);

            for (int x = minX; x < maxX; x++) {
                for (int y = minY; y < maxY; y++) {
                    for (int z = minZ; z < maxZ; z++) {
                        byte id = GetBlock(x, y, z);
                        if (Array.IndexOf(ids, id) >= 0)
                            return true;
                    }
                }
            }

            return false;
        }
        public bool IsSolid(int x, int y, int z)
        {
            return Blocks.IsSolid(GetBlock(x, y, z));
        }
        public string[] GetSignText(int x, int y, int z)
        {
            return Signs.TryGetValue(new Vec3i(x, y, z), out var text) ? text : null;
        }

        private int GetEffectiveFlowDecay(int x, int y, int z)
        {
            byte id = GetBlock(x, y, z);
            byte data = GetData(x, y, z);

            if (id != 8 && id != 9)
                return -1;
            if (data >= 8)
                data = 0;
            return data;
        }
        public Vec3d GetWaterFlowVector(int x, int y, int z)
        {
            Vec3d vec = new Vec3d(0, 0, 0);
            int effectiveDecay = GetEffectiveFlowDecay(x, y, z);
            for (int i = 0; i < 4; i++) {
                int xa = x;
                int ya = y;
                int za = z;

                if (i == 0) xa--;
                if (i == 1) za--;
                if (i == 2) xa++;
                if (i == 3) za++;

                int d = GetEffectiveFlowDecay(xa, ya, za);
                if (d < 0) {
                    if (IsSolid(xa, ya, za))
                        continue;

                    d = GetEffectiveFlowDecay(xa, ya - 1, za);
                    if (d >= 0) {
                        int j2 = d - (effectiveDecay - 8);
                        vec.Add((xa - x) * j2, (ya - y) * j2, (za - z) * j2);
                    }
                    continue;
                }
                if (d >= 0) {
                    int k2 = d - effectiveDecay;
                    vec.Add((xa - x) * k2, (ya - y) * k2, (za - z) * k2);
                }
            }

            vec.Normalize();
            return vec;
        }

        public HitResult RayCast(Vec3d start, Vec3d end, bool stopOnNonAir, bool allowWater)
        {
            int x2 = Utils.Floor(end.X);
            int y2 = Utils.Floor(end.Y);
            int z2 = Utils.Floor(end.Z);
            int x1 = Utils.Floor(start.X);
            int y1 = Utils.Floor(start.Y);
            int z1 = Utils.Floor(start.Z);
            int face = 0;
            for (int i = 0; i < 256; i++) {
                if (Double.IsNaN(start.X) || Double.IsNaN(start.Y) || Double.IsNaN(start.Z))
                    return null;
                if (x1 == x2 && y1 == y2 && z1 == z2)
                    return null;

                int id = GetBlock(x1, y1, z1);
                bool water = id >= 8 && id <= 11;
                if (allowWater && water ? true : stopOnNonAir ? id != 0 && !water : Blocks.IsSolid(id))
                    return new HitResult(x1, y1, z1, face);

                double cx = x2 > x1 ? x1 + 1 : x2 < x1 ? x1 : 999;
                double cy = y2 > y1 ? y1 + 1 : y2 < y1 ? y1 : 999;
                double cz = z2 > z1 ? z1 + 1 : z2 < z1 ? z1 : 999;

                double dx = end.X - start.X;
                double dy = end.Y - start.Y;
                double dz = end.Z - start.Z;

                double tx = cx != 999 ? (cx - start.X) / dx : 999;
                double ty = cy != 999 ? (cy - start.Y) / dy : 999;
                double tz = cz != 999 ? (cz - start.Z) / dz : 999;

                if (tx < ty && tx < tz) {
                    face = x2 > x1 ? 4 : 5;

                    start.X = cx;
                    start.Y += dy * tx;
                    start.Z += dz * tx;
                } else if (ty < tz) {
                    face = y2 > y1 ? 0 : 1;

                    start.X += dx * ty;
                    start.Y = cy;
                    start.Z += dz * ty;
                } else {
                    face = z2 > z1 ? 2 : 3;

                    start.X += dx * tz;
                    start.Y += dy * tz;
                    start.Z = cz;
                }

                x1 = Utils.Floor(start.X) - (face == 5 ? 1 : 0);
                y1 = Utils.Floor(start.Y) - (face == 1 ? 1 : 0);
                z1 = Utils.Floor(start.Z) - (face == 3 ? 1 : 0);
            }

            return null;
        }

        public static long ChunkPosKey(int x, int z)
        {
            return x & 0xFFFFFFFFL | (z & 0xFFFFFFFFL) << 32;
        }
        private class ChunkPosComparer : IEqualityComparer<long>
        {
            public static readonly ChunkPosComparer Instance = new ChunkPosComparer();

            public bool Equals(long x, long y)
            {
                return x == y;
            }
            public int GetHashCode(long k)
            {
                return (int)(k ^ k >> 27);
            }
        }
    }
    public class SignTile
    {
        public Vec3i Coords;
        public string[] Lines;
        public SignTile(Vec3i c, string[] lns)
        {
            Coords = c;
            Lines = lns;
        }
    }
    public class HitResult
    {
        public int X, Y, Z, Face;
        public Vec3d HitVector;

        public MPPlayer PointedEntity = null;

        public HitResult(int x, int y, int z, int f)
        {
            X = x;
            Y = y;
            Z = z;
            Face = f;
        }
        public HitResult(Vec3d hit, int f)
        {
            HitVector = hit;
            Face = f;
        }
    }
    public enum WorldUpdateEventType
    {
        /// <summary>
        /// Block has changed
        /// </summary>
        Block,
        /// <summary>
        /// Chunk has changed
        /// </summary>
        Chunk,
        /// <summary>
        /// Entire world has changed
        /// </summary>
        World
    }
}
