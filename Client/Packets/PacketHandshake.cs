using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedBot.client.Packets
{
    public class PacketHandshake : IPacket
    {
        public int ProtocolVersion;
        public string ServerIP;
        public ushort ServerPort;
        public int NextState;

        public PacketHandshake(int pVersion, string ip, ushort port, int nState)
        {
            ProtocolVersion = pVersion;
            ServerIP = ip;
            ServerPort = port;
            NextState = nState;
        }

        public void WritePacket(WriteBuffer s, MinecraftClient client)
        {
            s.WriteVarInt(0x00);
            s.WriteVarInt(ProtocolVersion);
            s.WriteString(ServerIP);
            s.WriteUShort(ServerPort);
            s.WriteVarInt(NextState);
        }
    }
}
