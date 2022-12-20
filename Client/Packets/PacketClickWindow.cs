using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdvancedBot.client.Handler;

namespace AdvancedBot.client.Packets
{
    public class PacketClickWindow : IPacket
    {
        public byte WindowID, Button, Mode;
        public short Slot, ActionNumber;
        public ItemStack ClickedItem;

        public PacketClickWindow(byte winId, short slot, byte button, short actionNum, byte mode, ItemStack item)
        {
            WindowID = winId;
            Slot = slot;
            Button = button;
            ActionNumber = actionNum;
            Mode = mode;
            ClickedItem = item;
        }
        public void WritePacket(WriteBuffer s, MinecraftClient client)
        {
            int nId = 0x0E;
            if (client.Version <= ClientVersion.v1_8 || !client.Handler.WritePacket(ref nId, this, s)) {
                s.WriteVarInt(nId);
                s.WriteByte(WindowID);
                s.WriteShort(Slot);
                s.WriteByte(Button);
                s.WriteShort(ActionNumber);
                s.WriteByte(Mode);
                if (client.Version >= ClientVersion.v1_8)
                    s.WriteItemStack(ClickedItem);
                else
                    Handler_v17.WriteItemStack(s, ClickedItem);
            }
        }
    }
}
