using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedBot.client.Packets
{
    public class PacketUpdate : IPacket
    {
        public bool OnGround;
        public PacketUpdate(bool g)
        {
            OnGround = g;
        }

        public void WritePacket(WriteBuffer s, MinecraftClient client)
        {
            int nId = 0x03;
            if (client.Version <= ClientVersion.v1_8 || !client.Handler.WritePacket(ref nId, this, s)) {
                s.WriteVarInt(nId);
                s.WriteBoolean(OnGround);
            }
        }
    }
}