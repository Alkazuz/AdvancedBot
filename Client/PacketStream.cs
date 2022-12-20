using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AdvancedBot.client.Crypto;
using Ionic.Zlib;

namespace AdvancedBot.client
{
    public class PacketStream : IDisposable
    {
        private Socket socket;
        private SocketAsyncEventArgs recv;
        private SocketAsyncEventArgs send;

        private const int BUFFER_SIZE = 8192;

        public event Action<ReadBuffer> OnPacketAvailable;
        public event Action<Exception> OnError;
        public event Action OnClose;

        public bool Connected = true;
        private bool Disposed = false;
        private ConcurrentQueue<ArraySegment<byte>> sendQueue = new ConcurrentQueue<ArraySegment<byte>>();
        private int isSending = 0;

        private ICryptoTransform aesEnc, aesDec;
        private byte[] encBuf, decBuf;

        private int dataPos = 0;
        private int pktLength;
        private byte[] data;

        public int CompressionThreshold = -1;

        public PacketStream(Socket sock, bool startReceiving = true)
        {
            socket = sock;

            recv = new SocketAsyncEventArgs();
            recv.SetBuffer(data = new byte[BUFFER_SIZE], 0, BUFFER_SIZE);
            recv.Completed += ReceiveCompleted;

            send = new SocketAsyncEventArgs();
            send.Completed += SendCompleted;

            if (startReceiving) {
                BeginReceive();
            }
        }
        public void StartReceiving()
        {
            BeginReceive();
        }

        private void ReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success) {
                OnException(new SocketException((int)e.SocketError));
            } else if (e.LastOperation == SocketAsyncOperation.Receive) {
                try {
                    int read = e.BytesTransferred;
                    if (read == 0) {
                        throw new EndOfStreamException("Server closed the connection unexpectedly");
                    }
                    if (aesDec != null) {
                        EnsureBufferSize(ref decBuf, read);
                        int ofs = dataPos;
                        aesDec.TransformBlock(data, ofs, read, decBuf, 0);
                        Buffer.BlockCopy(decBuf, 0, data, ofs, read);
                    }
                    dataPos += read;

                    int bpos = 0;
                    while (TryReadVarInt(data, ref bpos, dataPos, out pktLength)) {
                        int totalPktLen = pktLength + bpos;
                        if (pktLength > 2097152) {
                            throw new Exception("Tried to read a packet larger than 2MiB.");
                        }

                        if (data.Length < pktLength) ExpandBuffer(pktLength + 64);
                        if (dataPos >= totalPktLen) {
                            Statistics.IncrementRead(totalPktLen);
                            ReadBuffer pkt = null;
                            if (CompressionThreshold >= 0) {
                                int uLength = ReadVarInt(ref bpos);

                                if (uLength == 0) {
                                    pkt = new ReadBuffer(CopyBuffer(bpos, pktLength - 1), 0, true);
                                } else {
                                    byte[] decomp = Utils.Inflate(data, bpos, pktLength - Utils.VarIntSize(uLength), uLength);
                                    //byte[] decomp = ZlibStream.UncompressBuffer(CopyBuffer(bpos, pktLength - Utils.VarIntSize(uLength)));

                                    pkt = new ReadBuffer(decomp, 0, true);
                                }
                            } else {
                                pkt = new ReadBuffer(CopyBuffer(bpos, pktLength), 0, true);
                            }
                            OnPacketAvailable?.Invoke(pkt);
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
                        ExpandBuffer(data.Length * 2);
                        len = data.Length - dataPos;
                    }
                    recv.SetBuffer(dataPos, len);
                } catch (Exception ex) {
                    OnException(ex);
                    return;
                }
                BeginReceive();
            }
        }
        private void ExpandBuffer(int n)
        {
            byte[] buf = new byte[n];
            Buffer.BlockCopy(data, 0, buf, 0, data.Length);
            data = buf;
            recv.SetBuffer(data, 0, data.Length);
        }
        private byte[] CopyBuffer(int offset, int count)
        {
            byte[] b = new byte[count];
            Buffer.BlockCopy(data, offset, b, 0, count);
            return b;
        }

        private void BeginReceive()
        {
            try {
                if (!socket.ReceiveAsync(recv)) {
                    ReceiveCompleted(socket, recv);
                }
            } catch (Exception ex) {
                OnException(ex);
            }
        }

        private void SendCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success) {
                OnException(new SocketException((int)e.SocketError));
            } else if (e.LastOperation == SocketAsyncOperation.Send) {
                ProcessQueuedPackets();
            }
        }
        public void Send(byte[] data, int offset = 0, int len = -1)
        {
            sendQueue.Enqueue(new ArraySegment<byte>(data, offset, len == -1 ? data.Length : len));
            if (Interlocked.CompareExchange(ref isSending, 1, 0) == 0) {
                ProcessQueuedPackets();
            }
        }
        private void ProcessQueuedPackets()
        {
            if (sendQueue.TryDequeue(out ArraySegment<byte> pkt)) {
                try {
                    if (aesEnc != null) {
                        int len = pkt.Count;
                        EnsureBufferSize(ref encBuf, len);
                        aesEnc.TransformBlock(pkt.Array, pkt.Offset, len, encBuf, 0);
                        send.SetBuffer(encBuf, 0, len);
                    } else {
                        send.SetBuffer(pkt.Array, pkt.Offset, pkt.Count);
                    }
                    if (!socket.SendAsync(send)) {
                        SendCompleted(socket, recv);
                    }
                } catch (Exception ex) {
                    OnException(ex);
                }
            } else {
                Interlocked.Exchange(ref isSending, 0);
            }
        }

        private bool TryReadVarInt(byte[] buf, ref int pos, int len, out int result)
        {
            int n = Math.Min(6, len);

            result = 0;
            for (int i = 0; i < n; i++) {
                byte b = buf[pos++];
                result |= (b & 0x7F) << i * 7;
                if (i >= 5) {
                    throw new Exception("VarInt is too big");
                } else if ((b & 0x80) == 0x00) {
                    return true;
                }
            }
            return false;
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

        private static void EnsureBufferSize(ref byte[] buf, int size)
        {
            if (buf.Length < size) {
                buf = new byte[size];
            }
        }

        public void SendPacket(IPacket pkt, MinecraftClient cli)
        {
            WriteBuffer wb = new WriteBuffer();
            pkt.WritePacket(wb, cli);
            SendPacket(wb);
        }
        public void SendPacket(WriteBuffer wb)
        {
            byte[] pkt = BuildPacket(wb, out int len, CompressionThreshold);
            Send(pkt, 0, len);
            Statistics.IncrementSent(len);
        }
        private static byte[] BuildPacket(WriteBuffer wb, out int len, int ct)
        {
            byte[] final;

            if (ct < 0) {
                final = new byte[wb.Length + 5];

                int pos = 0;
                Utils.PutVarInt(final, ref pos, wb.Length);
                wb.CopyTo(final, pos);
                len = pos + wb.Length;
            } else {
                int uLength = wb.Length;
                int pos = 0;

                if (uLength < ct) {
                    final = new byte[uLength + 6];
                    Utils.PutVarInt(final, ref pos, uLength + 1);
                    final[pos++] = 0;
                    wb.CopyTo(final, pos);
                    len = pos + uLength;
                } else {
                    byte[] cmpPkt = Utils.Deflate(wb.GetBuffer(), uLength, CompressionLevel.Level1);

                    final = new byte[cmpPkt.Length + 10];
                    Utils.PutVarInt(final, ref pos, cmpPkt.Length + MinecraftStream.GetVarIntLength(uLength));
                    Utils.PutVarInt(final, ref pos, uLength);
                    Buffer.BlockCopy(cmpPkt, 0, final, pos, cmpPkt.Length);
                    len = pos + cmpPkt.Length;
                }
            }
            return final;
        }      

        public void InitEncryption(byte[] aesKey)
        {
            RijndaelManaged aes = CryptoUtils.GenerateAES(aesKey);

            encBuf = new byte[4096];
            decBuf = new byte[BUFFER_SIZE];
            aesDec = aes.CreateDecryptor();
            aesEnc = aes.CreateEncryptor();
        }

        private void OnException(Exception ex)
        {
            if (!(ex is ObjectDisposedException)) {
                OnError?.Invoke(ex);
                Disconnect(true);
            }
        }
        public void Disconnect(bool isInternalCall = false)
        {
            if (isInternalCall && Connected) {
                OnClose?.Invoke();
            }
            Connected = false;
            if (!Disposed) {
                try { socket.Shutdown(SocketShutdown.Both); } catch { }
            }
        }
        public void Dispose()
        {
            Connected = false;
            if (!Disposed) {
                socket.Dispose();
                recv.Dispose();
                send.Dispose();
            }
            Disposed = true;
        }
    }
}
