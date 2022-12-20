using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedBot.client.Packets
{
    public class PacketClientStatus : IPacket
    {
        public byte ActionID;

        public PacketClientStatus(byte id)
        {
            ActionID = id;
        }
        public void WritePacket(WriteBuffer s, MinecraftClient client)
        {
            int nId = 0x16;
            if (client.Version <= ClientVersion.v1_8 || !client.Handler.WritePacket(ref nId, this, s)) {
                s.WriteVarInt(nId);
                s.WriteSByte((sbyte)ActionID);
            }
        }
    }
}
