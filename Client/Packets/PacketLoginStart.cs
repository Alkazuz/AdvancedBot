using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedBot.client.Packets
{
    public class PacketLoginStart : IPacket
    {
        public string Username;

        public PacketLoginStart(string u)
        {
            Username = u;
        }

        public void WritePacket(WriteBuffer s, MinecraftClient client)
        {
            s.WriteVarInt(0x00);
            s.WriteString(Username);
        }
    }
}
