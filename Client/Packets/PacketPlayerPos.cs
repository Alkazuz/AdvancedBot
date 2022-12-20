using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedBot.client.Packets
{
    public class PacketPlayerPos : IPacket
    {
        public double X, Y, Z;
        public double FeetY;
        public bool OnGround;

        public PacketPlayerPos(double x, double feetY, double y, double z, bool g)
        {
            X = x;
            Y = y;
            FeetY = feetY;
            Z = z;
            OnGround = g;
        }
        public void WritePacket(WriteBuffer s, MinecraftClient client)
        {
            int nId = 0x04;
            if (client.Version <= ClientVersion.v1_8 || !client.Handler.WritePacket(ref nId, this, s)) {
                s.WriteVarInt(nId);
                s.WriteDouble(X);
                s.WriteDouble(FeetY);
                if (client.Version < ClientVersion.v1_8) s.WriteDouble(Y);
                s.WriteDouble(Z);
                s.WriteBoolean(OnGround);
            }
        }
    }
}
