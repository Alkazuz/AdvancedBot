using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ionic.Zlib;

namespace AdvancedBot.client.NBT
{
    public class NbtIO
    {
        public static CompoundTag ReadCompressed(Stream input)
        {
            using (var dis = new DataInput(new GZipStream(input, CompressionMode.Decompress, true))) {
                return Read(dis);
            }
        }
        public static void WriteCompressed(CompoundTag tag, Stream output)
        {
            using (var dos = new DataOutput(new GZipStream(output, CompressionMode.Compress, true))) {
                Write(tag, dos);
            }
        }

        public static CompoundTag Decompress(byte[] buffer)
        {
            using (var dis = new DataInput(GZipStream.UncompressBuffer(buffer))) {
                return Read(dis);
            }
        }
        public static byte[] Compress(CompoundTag tag)
        {
            using (var mem = new MemoryStream()) {
                using (var dos = new DataOutput(new GZipStream(mem, CompressionMode.Compress, true))) {
                    Write(tag, dos);
                    return mem.ToArray();
                }
            }
        }

        public static void Write(CompoundTag tag, string file)
        {
            using (var dos = new DataOutput(new FileStream(file, FileMode.Create, FileAccess.Write))) {
                Write(tag, dos);
            }
        }
        public static CompoundTag Read(string file)
        {
            using (var dis = new DataInput(new FileStream(file, FileMode.Open, FileAccess.Read))) {
                return Read(dis);
            }
        }
        public static CompoundTag Read(DataInput dis)
        {
            Tag tag = Tag.ReadNamedTag(dis);
            if (tag is CompoundTag ctag) {
                return ctag;
            }
            throw new IOException("Root tag must be a named compound tag");
        }
        public static void Write(CompoundTag tag, DataOutput dos)
        {
            Tag.WriteNamedTag(tag, dos);
        }
    }
}
