using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AdvancedBot.client
{
    public class Proxy
    {
        public string IP;
        public ushort Port;
        public ProxyType Type;

        public Proxy(string ip, ushort port, ProxyType type)
        {
            IP = ip;
            Port = port;
            Type = type;
        }

        private static readonly Regex HTTP_STATUS_REGEX = new Regex(@"HTTP\/\d.\d (\d+) ([a-zA-Z ]+)");

        #region Old blocking connectors
        public TcpClient Connect(string dstIp, ushort dstPort)
        {
            int tmp;
            switch (Type) {
                case ProxyType.HTTP: return CreateHttp(IP, Port, dstIp, dstPort, out tmp);
                case ProxyType.Socks4: return CreateSocks4(IP, Port, dstIp, dstPort, out tmp);
                case ProxyType.Socks5: return CreateSocks5(IP, Port, dstIp, dstPort, out tmp);
            }
            return null;
        }
        public static TcpClient CreateSocks4(string ip, ushort port, string dstIp, ushort dstPort, out int ping)
        {
            Stopwatch sw = Stopwatch.StartNew();
            TcpClient c = new TcpClient(ip, port);
            sw.Stop();
            ping = (int)sw.ElapsedMilliseconds;

            c.ReceiveTimeout = 5000;
            c.SendTimeout = 5000;
            NetworkStream s = c.GetStream();

            byte[] req = new byte[9];
            req[0] = 0x04; //VN
            req[1] = 0x01; //CD

            //DSTPORT
            req[2] = (byte)(dstPort >> 8);
            req[3] = (byte)dstPort;

            Array.Copy(IPv4Bytes(dstIp), 0, req, 4, 4); //DSTIP

            s.Write(req, 0, req.Length);
            /* 90: request granted
             * 91: request rejected or failed
             * 92: request rejected becasue SOCKS server cannot connect to identd on the client
             * 93: request rejected because the client program and identd report different user-ids
             */
            for (int i = 0; i < 8; i++) {
                int b = s.ReadByte();
                if (i == 1 && b != 90)
                    throw new Exception("Connection to proxy " + ip + ":" + port + " rejected or failed, c=" + b);
                else if (b == -1)
                    throw new EndOfStreamException("EOF while reading SOCKS4 response");
            }
            return c;
        }
        public static TcpClient CreateSocks5(string ip, ushort port, string dstIp, ushort dstPort, out int ping)
        {
            //https://github.com/bentonstark/starksoft-aspen/blob/master/Starksoft.Aspen/Proxy/Socks5ProxyClient.cs
            //https://en.wikipedia.org/wiki/SOCKS#SOCKS5

            Stopwatch sw = Stopwatch.StartNew();
            TcpClient c = new TcpClient(ip, port);
            sw.Stop();
            ping = (int)sw.ElapsedMilliseconds;

            c.ReceiveTimeout = 5000;
            c.SendTimeout = 5000;
            NetworkStream s = c.GetStream();

            s.Write(new byte[] { 0x05, //version
                                 1,    //num of auth methods
                                 0x00  //no auth
                               }, 0, 3);

            s.ReadByte();
            if (s.ReadByte() == 0xFF)
                throw new Exception("Connection to proxy " + ip + ":" + port + " failed, NO_ACCEPTABLE_AUTH_METHODS");

            byte[] address;
            byte ipType = 0x03;

            IPAddress ad;
            if (IPAddress.TryParse(dstIp, out ad)) {
                address = ad.GetAddressBytes();
                ipType = (byte)(ad.AddressFamily == AddressFamily.InterNetwork ? 0x01 :
                              ad.AddressFamily == AddressFamily.InterNetworkV6 ? 0x04 : 0xFF);
                if (ipType == 0xFF)
                    throw new Exception("Unsupported address type: " + ad.AddressFamily);
            } else {
                address = new byte[dstIp.Length + 1];
                address[0] = (byte)dstIp.Length;
                Encoding.ASCII.GetBytes(dstIp).CopyTo(address, 1);
            }

            byte[] req = new byte[6 + address.Length];

            req[0] = 0x05; //version
            req[1] = 0x01; //command
            req[3] = ipType;
            Array.Copy(address, 0, req, 4, address.Length);

            req[address.Length + 4] = (byte)(dstPort >> 8);
            req[address.Length + 5] = (byte)dstPort;

            s.Write(req, 0, req.Length);

            s.ReadByte(); //version
            int b = s.ReadByte(); //status
            if (b != 0)
                throw new Exception("Connection to proxy " + ip + ":" + port + " failed, status code: " + b);

            s.ReadByte();
            int t = s.ReadByte();//type
            int l = t == 0x01 ? 4 : t == 0x03 ? s.ReadByte() : 16;
            for (int i = 0; i < l + 2; i++)
                s.ReadByte();

            return c;
        }
        public static TcpClient CreateHttp(string ip, ushort port, string dstIp, ushort dstPort, out int ping)//http & https
        {
            Stopwatch sw = Stopwatch.StartNew();
            TcpClient c = new TcpClient(ip, port);
            sw.Stop();
            ping = (int)sw.ElapsedMilliseconds;

            c.ReceiveTimeout = 5000;
            c.SendTimeout = 5000;
            NetworkStream s = c.GetStream();

            byte[] req = Encoding.ASCII.GetBytes(string.Format("CONNECT {0}:{1} HTTP/1.1\r\nHost: {0}\r\n\r\n", dstIp, dstPort));
            s.Write(req, 0, req.Length);

            Match m = HTTP_STATUS_REGEX.Match(ReadHeader(s));
            if (int.Parse(m.Groups[1].Value) / 100 != 2) //2xx
                throw new Exception("Connection to proxy " + ip + ":" + port + " failed, status=" + m.ToString());

            for (string hdr; (hdr = ReadHeader(s)).Length != 0;) {
                if (hdr.ToLower().Contains("content-length")) {
                    int clen = int.Parse(hdr.Substring(hdr.IndexOf(':') + 2), NumberStyles.Integer);

                    if (clen > 0) {
                        throw new Exception("HTTP proxy " + ip + ":" + port + " has sent a 'Content-Length' header.");
                    }
                }
            }

            return c;
        }

        private static string ReadHeader(Stream s)
        {
            StringBuilder sb = new StringBuilder(32);

            for (int i = 0; i <= 1024; i++) {
                int c = s.ReadByte();
                if (c == '\r') {
                    if (s.ReadByte() != '\n') throw new InvalidDataException("Expected LF after CR when reading header");
                    break;
                } else if (c == -1) throw new EndOfStreamException();
                else if (i == 1024) throw new InvalidDataException("Header too long");
                sb.Append((char)c);
            }
            return sb.ToString();
        }
        private static byte[] IPv4Bytes(string ip)
        {
            IPAddress ad = null;
            if (!IPAddress.TryParse(ip, out ad)) {
                ad = Dns.GetHostEntry(ip).AddressList.First(a => a.AddressFamily == AddressFamily.InterNetwork);
            }
            return ad.GetAddressBytes();
        }
        #endregion

        public async Task<Socket> ConnectAsync(string dstHost, ushort dstPort)
        {
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) {
                ReceiveTimeout = 5000,
                SendTimeout = 5000
            };
            await sock.ConnectAsync(IP, Port);
            switch (Type) {
                case ProxyType.Socks4: await ConnectS4Async(sock, dstHost, dstPort).ConfigureAwait(false); break;
                case ProxyType.Socks5: await ConnectS5Async(sock, dstHost, dstPort).ConfigureAwait(false); break;
                case ProxyType.HTTP: await ConnectHttpAsync(sock, dstHost, dstPort).ConfigureAwait(false); break;
                default: throw new ArgumentException("Invalid proxy type");
            }
            return sock;
        }
        private async Task ConnectS4Async(Socket sock, string dstHost, ushort dstPort)
        {
            byte[] addrBytes;
            if (IPAddress.TryParse(dstHost, out IPAddress addr)) {
                addrBytes = addr.GetAddressBytes();
            } else {
                var entries = await Dns.GetHostEntryAsync(dstHost).ConfigureAwait(false);
                addrBytes = entries.AddressList.First(a => a.AddressFamily == AddressFamily.InterNetwork).GetAddressBytes();
            }
            byte[] req = new byte[9];
            req[0] = 0x04; //VN
            req[1] = 0x01; //CD

            //DSTPORT
            req[2] = (byte)(dstPort >> 8);
            req[3] = (byte)dstPort;

            //DSTIP
            Array.Copy(addrBytes, 0, req, 4, 4);

            await sock.SendAsync(req).ConfigureAwait(false);

            byte[] buf = new byte[8];
            int bytes = await sock.ReceiveAsync(buf).ConfigureAwait(false);

            byte status = buf[1];
            if (bytes != 8) {
                throw new ProxyConnectException($"SOCKS4 server response: expected 8 bytes, received {bytes}");
            } else if (status != 90) {
                throw new ProxyConnectException($"SOCKS4 connection rejected or failed, status code={status}");
            }
        }

        private static string[] S5Errors = new string[] {
            "request granted",
            "general failure",
            "connection not allowed by ruleset",
            "network unreachable",
            "host unreachable",
            "connection refused by destination host",
            "TTL expired",
            "protocol error",
            "address type not supported"
        };
        private async Task ConnectS5Async(Socket sock, string dstHost, ushort dstPort)
        {
            await sock.SendAsync(new byte[] { 0x05, //version
                                              1,    //num of auth methods
                                              0x00  //no auth
                                          }).ConfigureAwait(false);
            byte[] buf = new byte[256];
            int bytes = await sock.ReceiveAsync(buf, 0, 2).ConfigureAwait(false);

            if (bytes != 2) {
                throw new ProxyConnectException($"SOCKS5 server auth choice: expected 2 bytes, got {bytes}");
            } else if (buf[1] == 0xFF) {
                throw new ProxyConnectException("SOCKS5 server auth choice: No acceptable methods");
            }

            byte[] address;
            byte addrType = 0x03;

            if (IPAddress.TryParse(dstHost, out IPAddress ad)) {
                address = ad.GetAddressBytes();
                addrType = (byte)(ad.AddressFamily == AddressFamily.InterNetwork ? 0x01 :
                                  ad.AddressFamily == AddressFamily.InterNetworkV6 ? 0x04 : 0xFF);
            } else {
                address = new byte[dstHost.Length + 1];
                address[0] = (byte)dstHost.Length;
                Encoding.ASCII.GetBytes(dstHost).CopyTo(address, 1);
            }

            byte[] req = new byte[6 + address.Length];
            req[0] = 0x05; //version
            req[1] = 0x01; //command
            req[3] = addrType;
            Array.Copy(address, 0, req, 4, address.Length);

            req[address.Length + 4] = (byte)(dstPort >> 8);
            req[address.Length + 5] = (byte)dstPort;
            await sock.SendAsync(req).ConfigureAwait(false);
            bytes = await sock.ReceiveAsync(buf, 0, 256).ConfigureAwait(false);

            byte status = buf[1];
            if (bytes < 6) {
                throw new ProxyConnectException($"SOCKS5 server response: expected +6 bytes, got {bytes}");
            } else if (status != 0x00) {
                throw new ProxyConnectException($"SOCKS5 connection failed: 0x{status:X2} {S5Errors[Math.Min(S5Errors.Length - 1, status)]}");
            }
        }
        private async Task ConnectHttpAsync(Socket sock, string dstHost, ushort dstPort)
        {
            byte[] req = Encoding.ASCII.GetBytes(string.Format("CONNECT {0}:{1} HTTP/1.1\r\nHost: {0}:{1}\r\n\r\n", dstHost, dstPort));
            byte[] buf = new byte[1024];

            await sock.SendAsync(req).ConfigureAwait(false);
            int bytes = await sock.ReceiveAsync(buf).ConfigureAwait(false);
            if (bytes == 0) {
                throw new ProxyConnectException("End of stream");
            }

            int pos = 0;
            Match m = HTTP_STATUS_REGEX.Match(ReadHeader(buf, ref pos));
            if (int.Parse(m.Groups[1].Value) / 100 != 2) {//2xx
                throw new ProxyConnectException($"HTTP conection failed, status code={m.Value}");
            } else {
                for (int i = 0; i < 200; i++) {
                    string hdr = ReadHeader(buf, ref pos);
                    if (hdr == null) { //missing data..
                        throw new NotImplementedException("HTTP async connect->Missing data on buffer.");
                    } else if (hdr.Length == 0) { //final header
                        break;
                    }
                    if (hdr.ToLower().Contains("content-length")) {
                        int clen = int.Parse(hdr.Substring(hdr.IndexOf(':') + 2), NumberStyles.Integer);
                        if (clen > 0) {
                            throw new ProxyConnectException("Server response contains a body.");
                        }
                    }
                }
            }
            Debug.WriteLine("ConnectHTTPAsync trailing bytes: " + (bytes - pos));
        }

        private static string ReadHeader(byte[] buf, ref int offset)
        {
            int start = offset;
            for (; offset < buf.Length; offset++) {
                if (buf[offset] == '\r') {
                    if (buf[offset + 1] != '\n') break;
                    offset += 2;
                    return Encoding.ASCII.GetString(buf, start, (offset - 2) - start);
                }
            }
            return null;
        }

        public override string ToString()
        {
            return string.Format("Proxy[{0} {1}:{2}]", Type, IP, Port);
        }
    }
    public class ProxyConnectException : Exception
    {
        public ProxyConnectException(string msg) : base(msg)
        {
        }
    }
    public enum ProxyType
    {
        Socks4 = 4,
        Socks5 = 5,
        HTTP   = 2,
    }
}