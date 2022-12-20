using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedBot.client.Packets
{
    public class PacketTeleportConfirm : IPacket
    {
        public int TeleportID;

        public PacketTeleportConfirm(int id)
        {
            TeleportID = id;
        }

        public void WritePacket(WriteBuffer s, MinecraftClient client)
        {
            s.WriteVarInt(0x00);
            s.WriteVarInt(TeleportID);
        }
    }
}
