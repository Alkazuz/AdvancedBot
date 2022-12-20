using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedBot.client.Map
{
    public class Chunk
    {
        public ChunkSection[] Sections = new ChunkSection[16];

        public readonly int X, Z;

        public Chunk(int x, int z)
        {
            X = x;
            Z = z;
        }

        public byte GetBlock(int x, int y, int z)
        {
            ChunkSection s = Sections[y >> 4];
            return s == null ? (byte)0 : s.Blocks[(y & 0xF) << 8 | z << 4 | x];
        }
        public void SetBlock(int x, int y, int z, byte id)
        {
            ChunkSection s = Sections[y >> 4];
            if(s == null) {
                Sections[y >> 4] = (s = new ChunkSection());
            }
            s.Blocks[(y & 0xF) << 8 | z << 4 | x] = id;
        }

        public byte GetData(int x, int y, int z)
        {
            ChunkSection s = Sections[y >> 4];
            if (s == null) return 0;

            int i = (y & 0xF) << 8 | z << 4 | x;
            byte b = s.Metadata[i / 2];
            return (byte)(i % 2 == 0 ? b & 0x0F : (b >> 4) & 0x0F);
        }
        public void SetData(int x, int y, int z, byte data)
        {
            ChunkSection s = Sections[y >> 4];
            if (s == null) {
                Sections[y >> 4] = (s = new ChunkSection());
            }

            int i = (y & 0xF) << 8 | z << 4 | x;
            byte current = s.Metadata[i / 2];

            s.Metadata[i / 2] = (byte)(i % 2 == 0 ? (current & 0xF0) | (data & 0xF) :
                                                    (data << 4) | (current & 0xF));
        }
    }
    public class ChunkSection
    {
        public byte[] Blocks;
        public byte[] Metadata;

        public ChunkSection()
        {
            Blocks = new byte[4096];
            Metadata = new byte[2048];
        }
    }
}
