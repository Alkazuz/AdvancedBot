using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedBot.client.Packets
{
    public class PacketPluginMessage : IPacket
    {
        public string Channel;
        public byte[] Data;

        public PacketPluginMessage(string ch, byte[] dat)
        {
            Channel = ch;
            Data = dat;
        }

        public void WritePacket(WriteBuffer s, MinecraftClient client)
        {
            int nId = 0x17;
            if (client.Version <= ClientVersion.v1_8 || !client.Handler.WritePacket(ref nId, this, s)) {
                s.WriteVarInt(nId);
                s.WriteString(Channel);
                if (client.Version < ClientVersion.v1_8)
                    s.WriteShort((short)Data.Length);
                s.WriteByteArray(Data);
            }
        }
    }
}
