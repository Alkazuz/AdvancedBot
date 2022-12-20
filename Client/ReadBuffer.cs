using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using AdvancedBot.client.NBT;

namespace AdvancedBot.client
{
    public class ReadBuffer
    {
        private byte[] _buff;
        private int _bufPos;
        public ReadBuffer(byte[] data, int offset, bool readId)
        {
            _buff = data;
            _bufPos = offset;

            ID = readId ? ReadVarInt() : 0;
            Length = data.Length - _bufPos;
        }
        public void Reset()
        {
            _bufPos = _buff.Length - Length;
        }

        public int ID { get; set; }
        public int Length { get; set; }
        public int Remaining { get { return _buff.Length - _bufPos; } }

        public byte[] Data { get { return _buff; } set { _buff = value; } }
        public int Position { get { return _bufPos; } set { _bufPos = value; } }
        
        public int ReadVarInt()
        {
            int result = 0;
            int shift = 0;
            byte b;

            do {
                b = _buff[_bufPos++];
                result |= (b & 0x7F) << shift++ * 7;

                if (shift > 5)
                    throw new OverflowException("VarInt is too big");
            } while ((b & 0x80) != 0);

            return result;
        }
        public byte ReadByte() { return _buff[_bufPos++]; }
        public sbyte ReadSByte() { return (sbyte)_buff[_bufPos++]; }
        public ushort ReadUShort()
        {
            return (ushort)(_buff[_bufPos++] << 8 | _buff[_bufPos++]);
        }
        public short ReadShort()
        {
            return (short)(_buff[_bufPos++] << 8 | _buff[_bufPos++]);
        }
        public int ReadInt()
        {
            int n = _buff[_bufPos] << 24 |
                    _buff[_bufPos + 1] << 16 |
                    _buff[_bufPos + 2] << 8 |
                    _buff[_bufPos + 3];

            _bufPos += 4;
            return n;
        }
        public long ReadLong()
        {
            long n = (long)_buff[_bufPos + 0] << 56 |
                     (long)_buff[_bufPos + 1] << 48 |
                     (long)_buff[_bufPos + 2] << 40 |
                     (long)_buff[_bufPos + 3] << 32 |
                     (long)_buff[_bufPos + 4] << 24 |
                     (long)_buff[_bufPos + 5] << 16 |
                     (long)_buff[_bufPos + 6] << 8 |
                     (long)_buff[_bufPos + 7];
            _bufPos += 8;
            return n;
        }
        public unsafe float ReadFloat()
        {
            int n = ReadInt();
            return *(float*)&n; //((FloatPtr)AddressOf(n))[0]
        }
        public unsafe double ReadDouble()
        {
            long n = ReadLong();
            return *(double*)&n;
        }
        public bool ReadBoolean() { return _buff[_bufPos++] != 0; }
        public byte[] ReadByteArray(int len)
        {
            if (len > Remaining)
                throw new Exception("Tried to read more data than available.");

            byte[] buf = new byte[len];
            Buffer.BlockCopy(_buff, _bufPos, buf, 0, len);
            _bufPos += len;
            return buf;
        }
        public string ReadString()
        {
            int len = ReadVarInt();
            string str = Encoding.UTF8.GetString(_buff, _bufPos, len);
            _bufPos += len;
            return str;
        }

        public byte GetByte(int index)
        {
            return _buff[_buff.Length - Length + index];
        }

        /// <summary>
        /// Skips <b>n</b> bytes 
        /// </summary>
        /// <param name="count">Number of bytes to skip</param>
        /// <returns>Returns true if the number of bytes to skip is less or equal to remaining data on buffer</returns>
        public bool Skip(int count)
        {
            int d = Math.Min(count, Remaining);
            _bufPos += d;
            return d == count;
        }

        public Vec3i ReadLocation()
        {
            long val = ReadLong();
            int x = (int)(val >> 38);
            int y = (int)((val >> 26) & 0xFFF);
            int z = (int)(val << 38 >> 38);
            return new Vec3i(x, y, z);
        }
        public CompoundTag ReadNBT()
        {
            if (ReadByte() == 0)
                return null;
            else {
                using (MemoryStream mem = new MemoryStream(_buff)) {
                    mem.Position = _bufPos - 1;
                    CompoundTag tag = NbtIO.Read(new DataInput(mem));
                    _bufPos = (int)mem.Position;
                    return tag;
                }
            }
        }
        public ItemStack ReadItemStack()
        {
            short id = ReadShort();

            if (id >= 0) {
                byte count = ReadByte();
                short metadata = ReadShort();
                ItemStack stack = new ItemStack(id, metadata, count) {
                    NBTData = ReadNBT()
                };
                return stack;
            }
            return null;
        }
        public UUID ReadUUID() { return new UUID(ReadLong(), ReadLong()); }
    }
}
