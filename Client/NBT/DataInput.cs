using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AdvancedBot.client.NBT
{
    public class DataInput : IDisposable
    {
        private Stream stream;
        
        public DataInput(Stream s)
        {
            stream = s;
        }

        public DataInput(byte[] bytes)
        {
            stream = new MemoryStream(bytes);
        }

        public byte ReadByte()
        {
            int i = stream.ReadByte();
            if (i == -1) throw new EndOfStreamException();
            return (byte)i;
        }
        public void ReadByteArray(byte[] buffer)
        {
            int length = buffer.Length;

            for (int remain = length; remain > 0;) {
                int r = stream.Read(buffer, length - remain, remain);
                if (r == 0) throw new EndOfStreamException();
                remain -= r;
            }
        }
        public byte[] ReadByteArray(int length)
        {
            byte[] buf = new byte[length];
            ReadByteArray(buf);
            return buf;
        }
        public string ReadUTF()
        {
            return Encoding.UTF8.GetString(ReadByteArray((ushort)ReadShort()));
        }
        public short ReadShort()
        {
            return (short)(ReadByte() << 8 | ReadByte());
        }
        public int ReadInt()
        {
            return (ReadByte() << 24 | 
                    ReadByte() << 16 | 
                    ReadByte() << 8 | 
                    ReadByte());
        }
        public long ReadLong()
        {
            return (long)ReadByte() << 56 |
                   (long)ReadByte() << 48 |
                   (long)ReadByte() << 40 |
                   (long)ReadByte() << 32 |
                   (long)ReadByte() << 24 |
                   (long)ReadByte() << 16 |
                   (long)ReadByte() << 8 |
                   (long)ReadByte();
        }
        public unsafe float ReadFloat()
        {
            int i = ReadInt();
            return *(float*)&i;
        }
        public unsafe double ReadDouble()
        {
            long l = ReadLong();
            return *(double*)&l;
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
