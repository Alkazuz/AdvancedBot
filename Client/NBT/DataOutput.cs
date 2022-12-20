using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AdvancedBot.client.NBT
{
    public class DataOutput : IDisposable
    {
        private Stream stream;
        
        public DataOutput()
        {
            stream = new MemoryStream();
        }
        public DataOutput(Stream s)
        {
            stream = s;
        }

        public void WriteByte(byte b)
        {
            stream.WriteByte(b);
        }
        public void WriteByteArray(byte[] b)
        {
            stream.Write(b, 0, b.Length);
        }
        public void WriteUTF(string s)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(s);
            WriteShort((short)(ushort)bytes.Length);
            WriteByteArray(bytes);
        }
        public void WriteShort(short s)
        {
            WriteByte((byte)(s >> 8));
            WriteByte((byte)s);
        }
        public void WriteInt(int i)
        {
            WriteByte((byte)(i >> 24));
            WriteByte((byte)(i >> 16));
            WriteByte((byte)(i >> 8));
            WriteByte((byte)i);
        }
        public void WriteLong(long l)
        {
            WriteByte((byte)(l >> 56));
            WriteByte((byte)(l >> 48));
            WriteByte((byte)(l >> 40));
            WriteByte((byte)(l >> 32));
            WriteByte((byte)(l >> 24));
            WriteByte((byte)(l >> 16));
            WriteByte((byte)(l >> 8));
            WriteByte((byte)l);
        }
        public unsafe void WriteFloat(float f)
        {
            WriteInt(*(int*)&f);
        }
        public unsafe void WriteDouble(double d)
        {
            WriteLong(*(long*)&d);
        }

        public byte[] ToArray()
        {
            if(stream is MemoryStream mem) {
                return mem.ToArray();
            }
            throw new NotSupportedException();
        }

        public void Close()
        {
            stream.Close();
        }
        public void Dispose()
        {
            stream.Dispose();
        }
    }
}
