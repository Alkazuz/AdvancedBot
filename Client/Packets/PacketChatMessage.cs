using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedBot.client.Packets
{
    public class PacketChatMessage : IPacket
    {
        public string Message;

        public PacketChatMessage(string msg)
        {
            Message = msg.Length > 100 ? msg.Substring(0, 99) : msg;
        }

        public void WritePacket(WriteBuffer s, MinecraftClient client)
        {
            int nId = 0x01;
            if (client.Version <= ClientVersion.v1_8 || !client.Handler.WritePacket(ref nId, this, s)) {
                s.WriteVarInt(nId);
                s.WriteString(Message);
            }
        }
    }
}
