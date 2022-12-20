using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Net.Security;
using System.Text.RegularExpressions;
using AdvancedBot.client;
using AdvancedBot.client.Packets;

namespace AdvancedBot.ProxyChecker
{
    public class ProxyUtils
    {

        private static readonly Regex HTML_TAG_REGEX = new Regex(@"<\/?[^>]+>", RegexOptions.Compiled);
        private static readonly Regex IPv4_REGEX = new Regex(@"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){2,3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$", RegexOptions.Compiled);
        public static HashSet<ProxyInfo> Parse(string raw)
        {
            string[] splitted = HTML_TAG_REGEX.Replace(raw, " ").Split(new char[] { '\t', '\n', ' ', ':', '"', '(', ')', ',', '<', '>', '@', '#' }, StringSplitOptions.RemoveEmptyEntries);

            HashSet<ProxyInfo> proxies = new HashSet<ProxyInfo>();
            for (int i = 0; i < splitted.Length - 1; i++) {
                string ip = splitted[i];
                if (IPv4_REGEX.IsMatch(ip)) {
                    if (ushort.TryParse(splitted[i + 1], out ushort port))
                        proxies.Add(new ProxyInfo(0, ip, port, -1));
                }
            }

            return proxies;
        }

        private static int[] CommonHttpPorts = new int[] { 80, 443, 3128, 3129, 8000, 8080, 8081 };
        public static bool CheckProxy(ProxyInfo p)
        {
            ProxyType[] types = CommonHttpPorts.Contains(p.Port) ? new ProxyType[] { ProxyType.HTTP, ProxyType.Socks5, ProxyType.Socks4 } :
                                                                   new ProxyType[] { ProxyType.Socks5, ProxyType.Socks4, ProxyType.HTTP };

            foreach (ProxyType type in types) {
                //Debug.WriteLine("BEGIN CHECK " + type + " " + p.IP + ":" + p.Port);
                try {
                    TcpClient c = null;
                    switch (type) {
                        case ProxyType.HTTP: c = Proxy.CreateHttp(p.IP, p.Port, p.TestIP, p.TestPort, out p.Ping); break;
                        case ProxyType.Socks4: c = Proxy.CreateSocks4(p.IP, p.Port, p.TestIP, p.TestPort, out p.Ping); break;
                        case ProxyType.Socks5: c = Proxy.CreateSocks5(p.IP, p.Port, p.TestIP, p.TestPort, out p.Ping); break;
                    }
                    p.Type = type;
                    bool ok = p.LoginVer != -1 ? TryLogin(c, p.TestIP, p.TestPort, p.LoginVer) :
                              (p.TestPort == 80 || p.TestPort == 443 ? TryHttpRequest(c, p.TestIP, p.TestPort) :
                                                                       TryPing(c, p.TestIP, p.TestPort, 315, ref p.Ping));
                    c.Close();

                    return ok;
                } catch (Exception e) {
                    if (e is SocketException se) {
                        if (se.SocketErrorCode == SocketError.ConnectionRefused)
                            return false;
                    }
                }
            }

            return false;
        }

        // v1.8 ID|Version|ID
        private static int[][] PacketIDs = new int[][] {
            new int[] { 0x40, 318, 0x1B },
            new int[] { 0x40, 80,  0x1A },
            new int[] { 0x40, 67,  0x19 },
            new int[] { 0x40, 4,   0x40 },

            new int[] { 0x01, 332, 0x23 },
            new int[] { 0x01, 318, 0x24 },
            new int[] { 0x01, 86,  0x23 },
            new int[] { 0x01, 67,  0x24 },
            new int[] { 0x01, 4,   0x01 },
        };
        private static bool TryLogin(TcpClient cli, string ip, ushort port, int protocol)
        {
            try {
                MinecraftStream ms = new MinecraftStream(cli);
                ms.SendPacket(new PacketHandshake(protocol, ip, port, 2), null);
                ms.SendPacket(new PacketLoginStart(NickGenerator.PseudoNick()), null);
                
                for (int i = 0, s = 0; i < 16; i++) {
                    ReadBuffer pkt = ms.ReadPacket();
                    switch (pkt.ID | (s << 8)) {
                        case 0x00: Debug.WriteLine("Kick: " + pkt.ReadString()); return false; // disconnect
                        case 0x01: return true;  // encryption request
                        case 0x02: s = 1; break; // login success
                        case 0x03: ms.CompressionThreshold = pkt.ReadVarInt(); break;//compression threshold
                        default:
                            int[] idData = PacketIDs.FirstOrDefault(a => a[1] <= protocol && a[2] == pkt.ID);
                            if (idData != null && idData[0] == 0x40) {
                                Debug.WriteLine("Kick post connect: " + pkt.ReadString());
                                return false;
                            } else if(idData != null && idData[0] == 0x01) {
                                return true;
                            }
                            break;
                    }
                }
                return false;
            } catch { }
            return false;
        }
        private static bool TryPing(TcpClient cli, string ip, ushort port, int protocol, ref int ping)
        {
            try {
                MinecraftStream ms = new MinecraftStream(cli);
                ms.SendPacket(new PacketHandshake(protocol, ip, port, 1), null);
                WriteBuffer don = new WriteBuffer();
                don.WriteVarInt(0x00);
                ms.SendPacket(don);
                
                int len = ms.ReadVarInt();
                //length must be > 0 && < 2MiB and ID must be 0.
                if ((len <= 0 || len > 2097152) || ms.ReadVarInt() != 0x00) {
                    return false;
                }
                string s = Encoding.UTF8.GetString(ms.ReadByteArray(ms.ReadVarInt()));
                Debug.WriteLine("Proxy checker ping response: " + s);
                if (!(s.Length > 0 && s.StartsWith("{") && s.EndsWith("}"))) {
                    return false;
                }
                
                don.Reset();
                don.WriteVarInt(0x01);
                long time = Stopwatch.GetTimestamp();
                don.WriteLong(time);
                ms.SendPacket(don);

                len = ms.ReadVarInt();
                if ((len <= 0 || len > 2097152) || ms.ReadVarInt() != 0x01) {
                    return false;
                }
                var diff = Stopwatch.GetTimestamp() - time;
                ping = (int)(diff / (Stopwatch.Frequency / 1000));
                return true;
            } catch { }
            return false;
        }
        private static bool TryHttpRequest(TcpClient cli, string ip, ushort port)
        {
            try {
                Stream s = cli.GetStream();

                if (port == 443) {
                    SslStream ssl = new SslStream(s);
                    ssl.AuthenticateAsClient(ip);
                    s = ssl;
                }
                byte[] buf = Encoding.ASCII.GetBytes(string.Format("GET / HTTP/1.1\r\nHost: {0}\r\n\r\n", ip));
                s.Write(buf, 0, buf.Length);
                buf = new byte[256];

                if (s.Read(buf, 0, buf.Length) > 5) { //HTTP/1.1 200 OK
                    string resp = Encoding.ASCII.GetString(buf, 0, buf.Length);
                    Debug.WriteLine("Checker HTTP response: " + resp);
                    return Regex.IsMatch(resp, @"HTTP\/\d.\d (\d+) ([^\r\n]+)\r\n");
                }
            } catch { }
            return false;
        }
    }
}
