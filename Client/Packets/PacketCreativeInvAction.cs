using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdvancedBot.client.Handler;

namespace AdvancedBot.client.Packets
{
    public class PacketCreativeInvAction : IPacket
    {
        public short Slot;
        public ItemStack Item;
        public PacketCreativeInvAction(short slot, ItemStack item)
        {
            Slot = slot;
            Item = item;
        }
        public void WritePacket(WriteBuffer s, MinecraftClient client)
        {
            int nId = 0x10;
            if (client.Version <= ClientVersion.v1_8 || !client.Handler.WritePacket(ref nId, this, s)) {
                s.WriteVarInt(nId);
                s.WriteShort(Slot);
                if (client.Version >= ClientVersion.v1_8)
                    s.WriteItemStack(Item);
                else
                    Handler_v17.WriteItemStack(s, Item);
            }
        }
    }
}
