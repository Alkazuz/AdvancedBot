using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedBot.client.Packets
{
    public class PacketSwingArm : IPacket
    {
        public int EntityID;

        public PacketSwingArm(int eID)
        {
            EntityID = eID;
        }

        public void WritePacket(WriteBuffer s, MinecraftClient client)
        {
            int nId = 0x0A;
            if (client.Version <= ClientVersion.v1_8 || !client.Handler.WritePacket(ref nId, this, s)) {
                s.WriteVarInt(nId);
                if (client.Version < ClientVersion.v1_8) {
                    s.WriteInt(EntityID);
                    s.WriteByte(1);
                }
            }
        }
    }
}
