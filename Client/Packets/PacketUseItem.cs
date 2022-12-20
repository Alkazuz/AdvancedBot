using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedBot.client.Packets
{
    public class PacketUseItem : IPacket
    {
        public void WritePacket(WriteBuffer s, MinecraftClient client)
        {
            s.WriteVarInt(0x1D);
            s.WriteVarInt(0);//hand
        }
    }
}
