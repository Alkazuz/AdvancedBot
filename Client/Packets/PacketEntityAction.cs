using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedBot.client.Packets
{
    public class PacketEntityAction : IPacket
    {
        public int EntityID;
        public byte ActionID;
        public int JumpBoost;

        public PacketEntityAction(int eID, byte action, int jumpBoost)
        {
            EntityID = eID;
            ActionID = action;
            JumpBoost = jumpBoost;
        }

        public void WritePacket(WriteBuffer s, MinecraftClient client)
        {
            int nId = 0x0B;
            if (client.Version <= ClientVersion.v1_8 || !client.Handler.WritePacket(ref nId, this, s)) {
                s.WriteVarInt(nId);
                if (client.Version >= ClientVersion.v1_8) {
                    s.WriteVarInt(EntityID);
                    s.WriteVarInt(ActionID);
                    s.WriteVarInt(JumpBoost);
                } else {
                    s.WriteInt(EntityID);
                    s.WriteByte((byte)(ActionID + 1));
                    s.WriteInt(JumpBoost);
                }
            }
        }
    }
}
