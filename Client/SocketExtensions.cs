using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AdvancedBot.client
{
    public static class SocketExtensions
    {
        public static Task ConnectAsync(this Socket sock, string host, int port)
        {
            return Task.Factory.FromAsync(sock.BeginConnect, sock.EndConnect, host, port, null);
        }
        public static Task<int> ReceiveAsync(this Socket sock, byte[] buf, int offset = 0, int length = -1)
        {
            if (length == -1) {
                length = buf.Length;
            }
            return Task.Factory.FromAsync(sock.BeginReceive, sock.EndReceive, buf, offset, length, null);
        }
        public static Task SendAsync(this Socket sock, byte[] buf, int offset = 0, int length = -1)
        {
            if (length == -1) {
                length = buf.Length;
            }
            return Task.Factory.FromAsync(sock.BeginSend, sock.EndSend, buf, offset, length, null);
        }

        public static IAsyncResult BeginReceive(this Socket sock, byte[] buffer, int offset, int length, AsyncCallback ac, object state)
        {
            return sock.BeginReceive(buffer, offset, length, SocketFlags.None, ac, state);
        }
        public static IAsyncResult BeginSend(this Socket sock, byte[] buffer, int offset, int length, AsyncCallback ac, object state)
        {
            return sock.BeginSend(buffer, offset, length, SocketFlags.None, ac, state);
        }
    }
}
