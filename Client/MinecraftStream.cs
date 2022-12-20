using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using AdvancedBot.client.Crypto;
using AdvancedBot;
using Ionic.Zlib;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AdvancedBot.client
{
    public class MinecraftStream : IDisposable
    {
        public NetworkStream BaseStream;
        public TcpClient BaseClient;

        public bool Encrypted = false;
        private AesStream aesStream;

        public MinecraftStream(TcpClient client)
        {
            BaseClient = client;
            BaseStream = client.GetStream();
        }
        public void StartEncryption(byte[] key)
        {
            aesStream = new AesStream(BaseStream, key);
            Encrypted = true;
        }

        public static int GetVarIntLength(int val)
        {
            return (val & 0xFFFFFF80) == 0 ? 1 :
                   (val & 0xFFFFC000) == 0 ? 2 :
                   (val & 0xFFE00000) == 0 ? 3 :
                   (val & 0xF0000000) == 0 ? 4 : 5;
        }
        public static byte[] GetVarInt(int val)
        {
            byte[] b = new byte[GetVarIntLength(val)];
            uint u = (uint)val;
            int i = 0;
            for (; u > 0x7F; u >>= 7) {
                b[i++] = (byte)((u & 0x7F) | 0x80);
            }
            b[i] = (byte)u;
            return b;
        }

        public int ReadVarInt()
        {
            int result = 0;
            int shift = 0;
            byte b;

            do {
                b = ReadByte();
                result |= (b & 0x7F) << (shift++ * 7);

                if (shift > 5)
                    throw new OverflowException("VarInt is too big");
            } while ((b & 0x80) != 0);

            return result;
        }
        public byte ReadByte()
        {
            int b = Encrypted ? aesStream.ReadByte() : BaseStream.ReadByte();
            if (b == -1) throw new EndOfStreamException("EOF while reading byte.");
            return (byte)b;
        }
        public byte[] ReadByteArray(int length)
        {
            byte[] buffer = new byte[length];

            for (int remain = length; remain > 0; ) {
                int r = Encrypted ? aesStream.Read(buffer, length - remain, remain) :
                                    BaseStream.Read(buffer, length - remain, remain);
                if (r == 0) throw new EndOfStreamException("EOF while reading byte array.");
                remain -= r;
            }
            return buffer;
        }

        public void Dispose()
        {
            BaseClient.Close();
            BaseStream.Dispose();
        }
        
        public int CompressionThreshold = -1;
        
        public ReadBuffer ReadPacket()
        {
            int length = ReadVarInt();
            if (length > 2097152) throw new Exception("Tried to read a packet larger than 2MiB.");

            Statistics.IncrementRead(length + 1);

            if (CompressionThreshold >= 0) {
                int uLength = ReadVarInt();

                if (uLength > 2097152) throw new Exception("Tried to inflate a packet larger than 2MiB.");
                if (uLength == 0) { //uncompressed
                    return new ReadBuffer(ReadByteArray(length - 1), 0, true);
                } else {
                    byte[] decomp = ZlibStream.UncompressBuffer(ReadByteArray(length - GetVarIntLength(uLength)));
                    return new ReadBuffer(decomp, 0, true);
                }
            } else {
                return new ReadBuffer(ReadByteArray(length), 0, true);
            }
        }

        public void SendPacket(WriteBuffer packet)
        {
            byte[] pktBytes = packet.GetBytes();
            byte[] final;

            if (CompressionThreshold < 0) {
                byte[] varInt = GetVarInt(pktBytes.Length);
                final = new byte[varInt.Length + pktBytes.Length];

                Buffer.BlockCopy(varInt, 0, final, 0, varInt.Length);
                Buffer.BlockCopy(pktBytes, 0, final, varInt.Length, pktBytes.Length);
            } else {
                int uLength = pktBytes.Length;
                
                if (uLength < CompressionThreshold) //send uncompressed
                {
                    byte[] pLenVI = GetVarInt(uLength + 1);

                    final = new byte[uLength + pLenVI.Length + 1];

                    Buffer.BlockCopy(pLenVI, 0, final, 0, pLenVI.Length);
                    final[pLenVI.Length] = 0;
                    Buffer.BlockCopy(pktBytes, 0, final, pLenVI.Length + 1, pktBytes.Length);
                } else {
                    byte[] cmpPkt = Utils.Deflate(pktBytes, pktBytes.Length, CompressionLevel.BestSpeed);
                    byte[] uLenVI = GetVarInt(pktBytes.Length);

                    int pLen = uLenVI.Length + cmpPkt.Length;
                    byte[] pLenVI = GetVarInt(pLen);

                    final = new byte[pLen + pLenVI.Length];

                    Buffer.BlockCopy(pLenVI, 0, final, 0, pLenVI.Length);
                    Buffer.BlockCopy(uLenVI, 0, final, pLenVI.Length, uLenVI.Length);
                    Buffer.BlockCopy(cmpPkt, 0, final, pLenVI.Length + uLenVI.Length, cmpPkt.Length);
                }
            }
            WriteToStream(final);
        }
        public void SendPacket(IPacket packet, MinecraftClient cli)
        {
            WriteBuffer don = new WriteBuffer();
            packet.WritePacket(don, cli);
            SendPacket(don);
        }
        public void WriteToStream(byte[] data)
        {
            Statistics.IncrementSent(data.Length);
            if (Encrypted)
                aesStream.Write(data, 0, data.Length);
            else
                BaseStream.Write(data, 0, data.Length);
        }
    }
}
