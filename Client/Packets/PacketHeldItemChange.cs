using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedBot.client.Packets
{
    public class PacketHeldItemChange : IPacket
    {
        public short Slot;

        public PacketHeldItemChange(short s)
        {
            Slot = s;
        }
        public void WritePacket(WriteBuffer s, MinecraftClient client)
        {
            int nId = 0x09; 
            if (client.Version <= ClientVersion.v1_8 || !client.Handler.WritePacket(ref nId, this, s)) {
                s.WriteVarInt(nId);
                s.WriteShort(Slot);
            }
        }
    }
}
