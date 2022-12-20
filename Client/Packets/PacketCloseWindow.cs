using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedBot.client.Packets
{
    public class PacketCloseWindow : IPacket
    {
        public byte WindowID;
        public PacketCloseWindow(byte winId)
        {
            WindowID = winId;
        }
        public void WritePacket(WriteBuffer s, MinecraftClient client)
        {
            int nId = 0x0D; 
            if (client.Version <= ClientVersion.v1_8 || !client.Handler.WritePacket(ref nId, this, s)) {
                s.WriteVarInt(nId);
                s.WriteByte(WindowID);
            }
        }
    }
}
