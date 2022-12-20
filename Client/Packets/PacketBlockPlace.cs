using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdvancedBot.client.Map;
using AdvancedBot.client.Handler;

namespace AdvancedBot.client.Packets
{
    public class PacketBlockPlace : IPacket
    {
        public int X, Y, Z;
        public byte Direction;
        public ItemStack Item;
        public byte CursorX, CursorY, CursorZ;

        public PacketBlockPlace(HitResult hit, ItemStack s)
        {
            X = hit.X;
            Y = hit.Y;
            Z = hit.Z;
            Direction = (byte)hit.Face;
            Item = s;
            CursorX = 8;
            CursorY = 8;
            CursorZ = 8;
        }
        public PacketBlockPlace(ItemStack i)
        {
            Item = i;
            X = -1;
            Z = -1;
            Y = -1;
            Direction = 255;
            CursorX = 0; 
            CursorY = 0;
            CursorZ = 0;
        }

        public void WritePacket(WriteBuffer s, MinecraftClient client)
        {
            int nId = 0x08;
            if (client.Version <= ClientVersion.v1_8 || !client.Handler.WritePacket(ref nId, this, s)) {
                s.WriteVarInt(nId);
                if (client.Version == ClientVersion.v1_8) {
                    s.WriteLocation(new Vec3i(X, Y, Z));
                    s.WriteByte(Direction);
                    s.WriteItemStack(Item);
                } else {
                    s.WriteInt(X);
                    s.WriteByte((byte)Y);
                    s.WriteInt(Z);
                    s.WriteByte(Direction);
                    Handler_v17.WriteItemStack(s, Item);
                }
                s.WriteByte(CursorX);
                s.WriteByte(CursorY);
                s.WriteByte(CursorZ);
            }
        }
    }
}
