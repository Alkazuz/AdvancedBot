using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedBot.client.Map
{
    public class LinearBlockGetter
    {
        public World world;

        private Chunk chunk;
        private int chunkX, chunkZ = int.MaxValue;

        public LinearBlockGetter(World w)
        {
            world = w;
        }

        public int GetBlock(int x, int y, int z)
        {
            if (y >= 0 && y < 256) {
                int cx = x >> 4;
                int cz = z >> 4;

                if (chunkX != cx || chunkZ != cz) {
                    chunk = world.GetChunk(cx, cz);
                    chunkX = cx;
                    chunkZ = cz;
                }

                if (chunk != null) {
                    return chunk.GetBlock(x & 0xF, y, z & 0xF);
                }
            }
            return 0;
        }
    }
}
