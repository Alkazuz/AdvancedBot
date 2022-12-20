using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using AdvancedBot.client.NBT;

namespace AdvancedBot.client
{
    public class WriteBuffer
    {
        private byte[] _buff = new byte[16];
        private int _bufPos = 0, bufSize = 16;

        public int Length { get { return _bufPos; } }

        public void CopyTo(byte[] buf, int offset)
        {
            Buffer.BlockCopy(_buff, 0, buf, offset, _bufPos);
        }
        public byte[] GetBuffer()
        {
            return _buff;
        }
        public byte[] GetBytes()
        {
            int len = Length;
            byte[] buf = new byte[len];
            Buffer.BlockCopy(_buff, 0, buf, 0, len);
            return buf;
        }
        public void Reset()
        {
            _bufPos = 0;
        }

        private void EnsureSize(int n)
        {
            if (_bufPos + n > bufSize) {
                bufSize = (bufSize * 2) + n;
                byte[] newBuf = new byte[bufSize];
                Buffer.BlockCopy(_buff, 0, newBuf, 0, _buff.Length);
                _buff = newBuf;
            }
        }

        public void WriteVarInt(int val)
        {
            EnsureSize(5);
            uint u = (uint)val;
            for (; u > 0x7F; u >>= 7) {
                _buff[_bufPos++] = (byte)((u & 0x7F) | 0x80);
            }
            _buff[_bufPos++] = (byte)u;
        }
        public void WriteByte(byte val)
        {
            EnsureSize(1);
            _buff[_bufPos++] = val;
        }
        public void WriteSByte(sbyte val)
        {
            EnsureSize(1);
            _buff[_bufPos++] = (byte)val;
        }
        public void WriteUShort(ushort val)
        {
            EnsureSize(2);
            _buff[_bufPos++] = (byte)(val >> 8);
            _buff[_bufPos++] = (byte)val;
        }
        public void WriteShort(short val)
        {
            EnsureSize(2);
            _buff[_bufPos++] = (byte)(val >> 8);
            _buff[_bufPos++] = (byte)val;
        }
        public void WriteInt(int val)
        {
            EnsureSize(4);
            _buff[_bufPos] = (byte)(val >> 24);
            _buff[_bufPos + 1] = (byte)(val >> 16);
            _buff[_bufPos + 2] = (byte)(val >> 8);
            _buff[_bufPos + 3] = (byte)val;
            _bufPos += 4;
        }
        public void WriteLong(long val)
        {
            EnsureSize(8);
            _buff[_bufPos] = (byte)(val >> 56);
            _buff[_bufPos + 1] = (byte)(val >> 48);
            _buff[_bufPos + 2] = (byte)(val >> 40);
            _buff[_bufPos + 3] = (byte)(val >> 32);
            _buff[_bufPos + 4] = (byte)(val >> 24);
            _buff[_bufPos + 5] = (byte)(val >> 16);
            _buff[_bufPos + 6] = (byte)(val >> 8);
            _buff[_bufPos + 7] = (byte)val;
            _bufPos += 8;
        }
        public unsafe void WriteFloat(float val)
        {
            WriteInt(*(int*)&val);
        }
        public unsafe void WriteDouble(double val)
        {
            WriteLong(*(long*)&val);
        }
        public void WriteBoolean(bool val)
        {
            EnsureSize(1);
            _buff[_bufPos++] = (byte)(val ? 1 : 0);
        }
        public void WriteByteArray(byte[] val)
        {
            int len = val.Length;
            EnsureSize(len);
            if (len < 16) {
                while (len-- > 0)
                    _buff[_bufPos + len] = val[len];
            } else {
                Buffer.BlockCopy(val, 0, _buff, _bufPos, len);
            }
            _bufPos += val.Length;
        }
        public void WriteByteArray(byte[] val, int count)
        {
            int len = count;
            EnsureSize(len);
            if (len < 16) {
                while (len-- > 0)
                    _buff[_bufPos + len] = val[len];
            } else {
                Buffer.BlockCopy(val, 0, _buff, _bufPos, len);
            }
            _bufPos += count;
        }
        public void WriteString(string val)
        {
            byte[] utf8 = Encoding.UTF8.GetBytes(val);
            WriteVarInt(utf8.Length);
            WriteByteArray(utf8);
        }

        public void WriteLocation(Vec3i pos)
        {
            WriteLong(((pos.X & 0x3FFFFFFL) << 38) | ((pos.Y & 0xFFFL) << 26) | (pos.Z & 0x3FFFFFFL));
        }
        public void WriteNBT(CompoundTag tag)
        {
            if (tag == null)
                WriteByte(0);
            else {
                using (MemoryStream mem = new MemoryStream()) {
                    NbtIO.Write(tag, new DataOutput(mem));
                    WriteByteArray(mem.ToArray());
                }
            }
        }
        public void WriteItemStack(ItemStack stack)
        {
            if (stack == null)
                WriteShort(-1);
            else {
                WriteShort(stack.ID);
                WriteByte(stack.Count);
                WriteShort(stack.Metadata);

                WriteNBT(stack.NBTData);
            }
        }
        public void WriteUUID(UUID uuid)
        {
            WriteLong(uuid.HiBits);
            WriteLong(uuid.LoBits);
        }
    }
}
