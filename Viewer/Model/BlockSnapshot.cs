using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdvancedBot.Client;
using AdvancedBot.Client.Map;

namespace AdvancedBot.Viewer.Model
{
    public class BlockSnapshot
    {
        const int W = 20, H = 20, D = 20;
        private ushort[] blocks;
        private int xo, yo, zo;

        private BlockSnapshot()
        {
            blocks = new ushort[W * H * D];
        }

        public static BlockSnapshot Capture(World w, int x, int y, int z)
        {
            var bs = new BlockSnapshot();
            bs.xo = x;
            bs.yo = y;
            bs.zo = z;

            int cx1 = (int)Math.Floor(x / 16.0);
            int cy1 = (int)Math.Floor(y / 16.0);
            int cz1 = (int)Math.Floor(z / 16.0);
            int cx2 = (int)Math.Ceiling((x + W) / 16.0);
            int cy2 = (int)Math.Ceiling((y + H) / 16.0);
            int cz2 = (int)Math.Ceiling((z + D) / 16.0);

            for (int cx = cx1; cx < cx2; cx++) {
                for (int cz = cz1; cz < cz2; cz++) {
                    var chunk = w.GetChunk(cx, cz);
                    if (chunk == null) continue;

                    int x1 = Math.Max(0, x - cx << 4);
                    int x2 = Math.Min(16, x + W - cx << 4);
                    int z1 = Math.Max(0, z -     cz << 4);
                    int z2 = Math.Min(16, z + D - cz << 4);

                    for (int cy = cy1; cy < cy2; cy++) {
                        ChunkSection cs;
                        if ((cy < 0 || cy > 15) || (cs = chunk.Sections[cy]) == null) {
                            continue;
                        }
                        int y1 = y     - cy << 4;
                        int y2 = y + H - cy << 4;

                        int wx = cx << 4;
                        int wy = cy << 4;
                        int wz = cz << 4;

                        for (int yy = y1; yy < y2; yy++) {
                            for (int zz = z1; zz < z2; zz++) {
                                for (int xx = x1; xx < x2; xx++) {
                                    int i = (y & 0xF) << 8 | z << 4 | x;
                                    byte meta = cs.Metadata[i / 2];
                                    meta = (byte)(i % 2 == 0 ? meta & 0x0F : (meta >> 4) & 0x0F);

                                    bs.SetBlockData(xx + wx, yy + wy, zz + wy, cs.Blocks[i], meta);
                                }
                            }
                        }
                    }
                }
            }

            return bs;
        }
        public int GetBlockData(int x, int y, int z)
        {
            x -= xo;
            y -= yo;
            z -= zo;

            if (x >= 0 && y >= 0 && z >= 0 && x < W && y < H && z < D) {
                return blocks[x + z * W + y * W * D];
            } else {
                return 0;
            }
        }
        public int GetBlock(int x, int y, int z)
        {
            return GetBlockData(x, y, z) >> 4;
        }
        public int GetData(int x, int y, int z)
        {
            return GetBlockData(x, y, z) & 0xF;
        }
        public BlockState GetBlockState(int x, int y, int z)
        {
            return BlockState.FromID(GetBlockData(x, y, z));
        }

        public void SetBlockData(int x, int y, int z, int id, int data)
        {
            x -= xo;
            y -= yo;
            z -= zo;

            if (x >= 0 && y >= 0 && z >= 0 && x < W && y < H && z < D) {
                blocks[x + z * W + y * W * D] = (ushort)(id << 4 | data);
            }
        }
    }
}
