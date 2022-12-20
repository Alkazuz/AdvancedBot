using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedBot.client.Packets
{
    public class PacketPlayerLook : IPacket
    {
        public float Yaw, Pitch;
        public bool OnGround;

        public PacketPlayerLook(float yaw, float pitch, bool g)
        {
            Yaw = yaw;
            Pitch = pitch;
            OnGround = g;
        }

        public void WritePacket(WriteBuffer s, MinecraftClient client)
        {
            int nId = 0x05;

            if (client.Version <= ClientVersion.v1_8 || !client.Handler.WritePacket(ref nId, this, s)) {
                s.WriteVarInt(nId);
                s.WriteFloat(Yaw);
                s.WriteFloat(Pitch);
                s.WriteBoolean(OnGround);
            }
        }
    }
}
