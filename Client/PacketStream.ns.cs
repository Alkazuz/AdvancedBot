using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using AdvancedBot.client.Crypto;
using Ionic.Zlib;

namespace AdvancedBot.client
{
    public class PacketStream_ : IDisposable
    {
        public Stream DataStream;
        public bool Connected = true;

        private int dataPos = 0;
        private int pktLength;
        private byte[] data;

        public event Action<ReadBuffer> OnPacketAvailable;
        public event Action<Exception> OnError;

        public int CompressionThreshold = -1;

        private ICryptoTransform aesEnc, aesDec;
        private byte[] encBuf, decBuf;

        public PacketStream_(Stream s)
        {
            DataStream = s;
            data = new byte[8192];

            s.BeginRead(data, 0, data.Length, ReadCallback, s);
        }
        private void ReadCallback(IAsyncResult result)
        {
            try {
                int read = ((Stream)result.AsyncState).EndRead(result);

                if (read == 0) {
                    throw new EndOfStreamException("PacketStream.ReadCallback");
                }
                if (aesDec != null) {
                    if (decBuf.Length < read) ExpandBuffer(ref decBuf, read + 64);
                    int ofs = dataPos;
                    int transformed = aesDec.TransformBlock(data, ofs, read, decBuf, 0);
                    if (transformed != read) {
                        throw new Exception("Unexpected: AesDecryptor.TransformBlock() != BytesRead");
                    }
                    Buffer.BlockCopy(decBuf, 0, data, ofs, read);
                }
                dataPos += read;

                int bpos = 0;
                while (IsVarIntComplete(ref bpos, dataPos, out pktLength)) {
                    int totalPktLen = pktLength + bpos;
                    if (pktLength > 2097152) {
                        throw new Exception("Tried to read a packet larger than 2MiB.");
                    }

                    if (data.Length < pktLength) ExpandBuffer(ref data, pktLength + 32);
                    if (dataPos >= totalPktLen) {
                        Statistics.IncrementRead(totalPktLen);
                        ReadBuffer pkt = null;
                        if (CompressionThreshold >= 0) {
                            int uLength = ReadVarInt(ref bpos);

                            if (uLength == 0) {
                                pkt = new ReadBuffer(CopyBuffer(bpos, pktLength - 1), 0, true);
                            } else {
                                int n = GetVarIntLength(uLength);
                                byte[] decomp = ZlibStream.UncompressBuffer(CopyBuffer(bpos, pktLength - GetVarIntLength(uLength)));
                                pkt = new ReadBuffer(decomp, 0, true);
                            }
                        } else {
                            pkt = new ReadBuffer(CopyBuffer(bpos, pktLength), 0, true);
                        }
                        Action<ReadBuffer> packetEvent = OnPacketAvailable;
                        if (packetEvent != null) {
                            packetEvent(pkt);
                        }
                        //Debug.WriteLine(string.Format("id=0x{0:X2} len={1} totalLen={2}, dataPos={3}", pkt.ID, pkt.Length, totalPktLen, dataPos));

                        Buffer.BlockCopy(data, totalPktLen, data, 0, data.Length - totalPktLen);
                        dataPos -= totalPktLen;
                        bpos = 0;
                    } else {
                        break;
                    }
                }
                int len = data.Length - dataPos;
                if (len < 256) {
                    ExpandBuffer(ref data, data.Length * 2);
                }
                if (Connected) {
                    Stream s = DataStream;
                    s.BeginRead(data, dataPos, len, ReadCallback, s);
                }
            } catch (Exception e) {
                OnException(e);
            }
        }

        private bool IsVarIntComplete(ref int pos, int len, out int result)
        {
            int n = Math.Min(6, len);

            result = 0;
            for (int i = 0; i < n; i++) {
                byte b = data[pos++];
                result |= (b & 0x7F) << i * 7;
                if (i >= 5) {
                    throw new Exception("VarInt is too big");
                } else if ((b & 0x80) == 0x00) {
                    return true;
                }
            }
            result = -1;
            return false;
        }
        private static int GetVarIntLength(int val)
        {
            return (val & -0x80) == 0 ? 1 :
                   ((val & -0x4000) == 0 ? 2 :
                   ((val & -0x200000) == 0 ? 3 :
                   ((val & -0x10000000) == 0 ? 4 : 5)));
        }
        private int ReadVarInt(ref int pos)
        {
            int result = 0;
            int shift = 0;
            byte b;

            do {
                b = data[pos++];
                result |= (b & 0x7F) << shift++ * 7;

                if (shift > 5)
                    throw new OverflowException("VarInt is too big");
            } while ((b & 0x80) != 0);

            return result;
        }
        private static void PutVarInt(byte[] buf, ref int pos, int val)
        {
            uint u = (uint)val;
            for (; u > 0x7F; u >>= 7) {
                buf[pos++] = (byte)((u & 0x7F) | 0x80);
            }
            buf[pos++] = (byte)u;
        }
        private byte[] CopyBuffer(int offset, int count)
        {
            byte[] b = new byte[count];
            Buffer.BlockCopy(data, offset, b, 0, count);
            return b;
        }
        private void ExpandBuffer(ref byte[] buf, int n)
        {
            byte[] b = new byte[n];
            Buffer.BlockCopy(buf, 0, b, 0, buf.Length);
            buf = b;
        }

        public void SendPacket(WriteBuffer packet)
        {
            byte[] final;
            int len;

            if (CompressionThreshold < 0) {
                final = new byte[packet.Length + 5];

                int pos = 0;
                PutVarInt(final, ref pos, packet.Length);
                packet.CopyTo(final, pos);
                len = pos + packet.Length;
            } else {
                int uLength = packet.Length;
                int pos = 0;

                if (uLength < CompressionThreshold) {
                    final = new byte[uLength + 6];
                    PutVarInt(final, ref pos, uLength + 1);
                    final[pos++] = 0;
                    packet.CopyTo(final, pos);
                    len = pos + uLength;
                } else {
                    byte[] cmpPkt = Utils.Deflate(packet.GetBuffer(), uLength, CompressionLevel.BestSpeed);

                    final = new byte[cmpPkt.Length + 10];
                    PutVarInt(final, ref pos, cmpPkt.Length + GetVarIntLength(uLength));
                    PutVarInt(final, ref pos, uLength);
                    Buffer.BlockCopy(cmpPkt, 0, final, pos, cmpPkt.Length);
                    len = pos + cmpPkt.Length;
                }
            }
            Write(final, len);
        }
        public void SendPacket(IPacket packet, MinecraftClient cli)
        {
            WriteBuffer don = new WriteBuffer();
            packet.WritePacket(don, cli);
            SendPacket(don);
        }

        public void Write(byte[] b, int len)
        {
            if (!Connected) return;

            Statistics.IncrementSent(len);
            Stream s = DataStream;
            try {
                if (aesEnc != null) {
                    if (encBuf.Length < len) ExpandBuffer(ref encBuf, len + 64);
                    int transformed = aesEnc.TransformBlock(b, 0, len, encBuf, 0);
                    if (transformed != len) {
                        throw new Exception("Unexpected: AesEncryptor.TransformBlock() != Count");
                    }
                    s.BeginWrite(encBuf, 0, len, WriteCallback, s);
                } else {
                    s.BeginWrite(b, 0, len, WriteCallback, s);
                }
            } catch (Exception e) {
                OnException(e);
            }
        }
        private void WriteCallback(IAsyncResult result)
        {
            try {
                ((Stream)result.AsyncState).EndWrite(result);
            } catch (Exception e) {
                OnException(e);
            }
        }
        private void OnException(Exception e)
        {
            Connected = false;
            OnError?.Invoke(e);
        }

        public void InitEncryption(byte[] aesKey)
        {
            RijndaelManaged aes = CryptoUtils.GenerateAES(aesKey);

            encBuf = new byte[4096];
            decBuf = new byte[4096];
            aesDec = aes.CreateDecryptor();
            aesEnc = aes.CreateEncryptor();
        }

        public void Close()
        {
            Connected = false;
            DataStream.Close();
        }
        public void Dispose()
        {
            Close();
            DataStream.Dispose();
        }
    }
}
