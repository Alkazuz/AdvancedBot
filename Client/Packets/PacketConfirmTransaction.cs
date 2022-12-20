using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedBot.client.Packets
{
    public class PacketConfirmTransaction : IPacket
    {
        public byte WindowID;
        public short ActionNumber;
        public bool Accepted;

        public PacketConfirmTransaction(byte winId, short actionNum, bool accepted)
        {
            WindowID = winId;
            ActionNumber = actionNum;
            Accepted = accepted;
        }

        public void WritePacket(WriteBuffer s, MinecraftClient client)
        {
            int nId = 0x0F; 
            if (client.Version <= ClientVersion.v1_8 || !client.Handler.WritePacket(ref nId, this, s)) {
                s.WriteVarInt(nId);
                s.WriteByte(WindowID);
                s.WriteShort(ActionNumber);
                s.WriteBoolean(Accepted);
            }
        }
    }
}
