using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedBot.client.Packets
{
    public class PacketKeepAlive : IPacket
    {
        public int KeepAliveID;

        public PacketKeepAlive(int id)
        {
            KeepAliveID = id;
        }

        public void WritePacket(WriteBuffer s, MinecraftClient client)
        {
            int nId = 0x00;
            if (client.Version <= ClientVersion.v1_8 || !client.Handler.WritePacket(ref nId, this, s)) {
                s.WriteVarInt(nId);
                if (client.Version >= ClientVersion.v1_8)
                    s.WriteVarInt(KeepAliveID);
                else
                    s.WriteInt(KeepAliveID);
            }
        }
    }
}
