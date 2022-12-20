using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedBot.client.Packets
{
    public class PacketPosAndLook : IPacket
    {
        public double X, Y, Z;
        public double FeetY;
        public float Yaw, Pitch;
        public bool OnGround;

        public PacketPosAndLook(double x, double feetY, double y, double z, float yaw, float pitch, bool g)
        {
            X = x;
            Y = y;
            FeetY = feetY;
            Z = z;
            Yaw = yaw;
            Pitch = pitch;
            OnGround = g;
        }

        public void WritePacket(WriteBuffer s, MinecraftClient client)
        {
            int nId = 0x06;

            if (client.Version <= ClientVersion.v1_8 || !client.Handler.WritePacket(ref nId, this, s)) {
                s.WriteVarInt(nId);
                s.WriteDouble(X);
                s.WriteDouble(FeetY);
                if (client.Version < ClientVersion.v1_8) s.WriteDouble(Y);
                s.WriteDouble(Z);
                s.WriteFloat(Yaw);
                s.WriteFloat(Pitch);
                s.WriteBoolean(OnGround);
            }
        }
    }
}
