using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedBot.client.Packets
{
    public class PacketClientSettings : IPacket
    {
        public string Locate;
        public byte ViewDistance, ChatFlags;
        public bool ChatColors;
        public byte Difficulty;
        public bool ShowCape;

        public PacketClientSettings(byte viewDist)
        {
            Locate = "pt_BR";
            ViewDistance = viewDist;
            ChatColors = true;
            Difficulty = 1;
            ChatFlags = 0x00;
            ShowCape = true;
        }
        public void WritePacket(WriteBuffer s, MinecraftClient client)
        {
            int nId = 0x15;
            if (client.Version <= ClientVersion.v1_8 || !client.Handler.WritePacket(ref nId, this, s)) {
                s.WriteVarInt(nId);
                if (client.Version == ClientVersion.v1_8) {
                    s.WriteString(Locate);
                    s.WriteByte(ViewDistance);
                    s.WriteByte(ChatFlags);
                    s.WriteBoolean(ChatColors);
                    s.WriteByte(0x7F);//all flags
                } else {
                    s.WriteString(Locate);
                    s.WriteByte(ViewDistance);
                    s.WriteByte(ChatFlags);
                    s.WriteBoolean(ChatColors);
                    s.WriteByte(Difficulty);
                    s.WriteBoolean(ShowCape);
                }
            }
        }
    }
}
