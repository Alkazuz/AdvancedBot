using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdvancedBot.client;

namespace AdvancedBot.ProxyChecker
{
    public class ProxyInfo
    {
        public string IP, TestIP;
        public ushort Port, TestPort;
        public ProxyType Type;
        public int Ping;
        public int LoginVer = -1;

        public ProxyInfo(ProxyType t, string ip, ushort port, int ping)
        {
            Type = t;
            IP = ip;
            Port = port;
            Ping = ping;
        }

        public ProxyInfo SetTestAddr(string ip, ushort port)
        {
            TestIP = ip;
            TestPort = port;
            return this;
        }
        public override bool Equals(object obj)
        {
            if (obj is ProxyInfo) {
                ProxyInfo p = (ProxyInfo)obj;
                return p.IP == IP && p.Port == Port;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return (Port * 397) ^ IP.GetHashCode();
        }
    }
}
