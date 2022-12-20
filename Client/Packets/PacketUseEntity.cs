using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedBot.client.Packets
{
    public class PacketUseEntity : IPacket
    {
        public int EntityID;
        public byte MouseButton;

        public PacketUseEntity(int eID, bool attack)
        {
            EntityID = eID;
            MouseButton = (byte)(attack ? 1 : 0);
        }

        public void WritePacket(WriteBuffer s, MinecraftClient client)
        {
            int nId = 0x02;

            if (client.Version <= ClientVersion.v1_8 || !client.Handler.WritePacket(ref nId, this, s)) {
                s.WriteVarInt(nId);
                if (client.Version >= ClientVersion.v1_8) {
                    s.WriteVarInt(EntityID);
                    s.WriteVarInt(MouseButton);
                } else {
                    s.WriteInt(EntityID);
                    s.WriteByte(MouseButton);
                }
            }
        }
    }
}
