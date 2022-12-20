using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdvancedBot.client.Map;

namespace AdvancedBot.client.Packets
{
    public class PacketPlayerDigging : IPacket
    {
        public DiggingStatus Status;
        public int X, Z;
        public byte Y;
        public byte Face;

        public PacketPlayerDigging(DiggingStatus s)
        {
            Status = s;
            X = 0; Y = 0; Z = 0;
            Face = 0;
        }
        public PacketPlayerDigging(DiggingStatus s, int x, byte y, int z, byte fc)
        {
            Status = s;
            X = x;
            Y = y;
            Z = z;
            Face = fc;
        }
        public PacketPlayerDigging(DiggingStatus s, HitResult hit)
        {
            Status = s;
            X = hit.X;
            Y = (byte)hit.Y;
            Z = hit.Z;
            Face = (byte)hit.Face;
        }

        public void WritePacket(WriteBuffer s, MinecraftClient client)
        {
            int nId = 0x07;
            if (client.Version <= ClientVersion.v1_8 || !client.Handler.WritePacket(ref nId, this, s)) {
                s.WriteVarInt(nId);
                s.WriteByte((byte)Status);

                if (client.Version >= ClientVersion.v1_8) {
                    s.WriteLocation(new Vec3i(X, Y, Z));
                } else {
                    s.WriteInt(X);
                    s.WriteByte(Y);
                    s.WriteInt(Z);
                }
                s.WriteByte(Face);
            }
        }
    }
    public enum DiggingStatus : byte
    {
        StartedDigging = 0,
        CancelledDigging = 1,
        FinishedDigging = 2,
        DropItemStack = 3,
        DropItem = 4,
        FinishUse = 5
    }
}
