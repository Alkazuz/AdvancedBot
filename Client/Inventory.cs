using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdvancedBot.client.Packets;
using AdvancedBot.client.NBT;

namespace AdvancedBot.client
{
    public class Inventory
    {
        public byte WindowID;
        public InventoryType Type;
        public string Title;
        public byte NumSlots;
        public bool UseTitle;
        public int EntityID;

        public short TransactionId = 0;

        public ItemStack[] Slots;

        public static ItemStack ClickedItem = null;

        public Inventory() { }
        public Inventory(int slotCount)
        {
            Slots = new ItemStack[slotCount];
            NumSlots = (byte)slotCount;
        }
        public void SetItem(int slot, ItemStack item)
        {
            if (slot >= 0 && slot < NumSlots) {
                Slots[slot] = item;
            }
        }

        public static InventoryType GetType(string name)
        {
            switch (name)
            {
                case "minecraft:chest":            return InventoryType.Chest;
                case "minecraft:crafting_table":   return InventoryType.Workbench;
                case "minecraft:furnace":          return InventoryType.Furnace;
                case "minecraft:dispenser":        return InventoryType.Dispenser;
                case "minecraft:enchanting_table": return InventoryType.EnchantmentTable;
                case "minecraft:brewing_stand":    return InventoryType.BrewingStand;
                case "minecraft:villager":         return InventoryType.NPCTrade;
                case "minecraft:beacon":           return InventoryType.Beacon;
                case "minecraft:anvil":            return InventoryType.Anvil;
                case "minecraft:hopper":           return InventoryType.Hopper;
                case "minecraft:dropper":          return InventoryType.Dropper;
                case "EntityHorse":                return InventoryType.Horse;
                default: return (InventoryType)0xFF;
            }
        }

        //TODO: Cleanup
        //isChestOpen = Is clicking on player's inventory and another inventory is open
        public void Click(MinecraftClient c, short slot, bool isChestOpen, bool leftClick = true)
        {
            ItemStack clickedItem = null;
            if (ClickedItem == null) { //take
                if (leftClick) {
                    ClickedItem = Slots[slot];
                    clickedItem = Slots[slot];
                    Slots[slot] = null;
                } else {
                    ItemStack stack = Slots[slot];
                    if (stack != null) {
                        clickedItem = stack.Copy();
                        int half = (stack.Count + 1) / 2;
                        ClickedItem = new ItemStack(stack.ID, stack.Metadata, (byte)half, stack.NBTData);

                        stack.Count = (byte)Math.Max(0, stack.Count - half);
                    }
                }
            } else {
                if (Slots[slot] == null) { //put
                    if (leftClick) {
                        Slots[slot] = ClickedItem;
                        ClickedItem = null;
                    } else if(ClickedItem != null) {
                        ItemStack stack = ClickedItem.Copy();
                        stack.Count = 1;
                        Slots[slot] = stack;
                        if (--ClickedItem.Count == 0) {
                            ClickedItem = null;
                        }
                    }
                } else if (Slots[slot].IsSameItem(ClickedItem)) {//join
                    ItemStack a = Slots[slot];
                    clickedItem = new ItemStack(a.ID, a.Metadata, a.Count, a.NBTData);

                    int cnt = a.Count + (leftClick ? ClickedItem.Count : 1);
                    int remain = leftClick ? cnt - 64 : ClickedItem.Count - 1;

                    if (cnt < 64) {
                        if (leftClick) {
                            ClickedItem = null;
                        } else if (remain <= 0) {
                            ClickedItem = null;
                        } else {
                            ClickedItem.Count = (byte)remain;
                        }
                        a.Count = (byte)cnt;
                    } else {
                        if (remain <= 0)
                            ClickedItem = null;
                        else
                            ClickedItem.Count = (byte)remain;
                        a.Count = (byte)(cnt > 64 ? 64 : cnt);
                    }
                } else { //switch
                    clickedItem = Slots[slot];
                    Slots[slot] = ClickedItem;
                    ClickedItem = clickedItem;
                }
            }
            if (isChestOpen)
                slot -= 9;

            int slotOffset = this == c.Inventory && c.OpenWindow != null ? c.OpenWindow.NumSlots : 0;
            System.Diagnostics.Debug.WriteLine("Inv click: " + (c.OpenWindow != null ? c.OpenWindow.WindowID : WindowID) + " " + (slot + slotOffset));
            c.SendPacket(new PacketClickWindow(c.OpenWindow != null ? c.OpenWindow.WindowID : WindowID, (short)(slot + slotOffset), (byte)(leftClick?0:1), ++TransactionId, 0, clickedItem));
        }

        public void DropItem(MinecraftClient q, int slot)
        {
            if (Slots[slot] != null) {
                q.SendPacket(new PacketClickWindow(WindowID, (short)slot, 1, ++TransactionId, 4, null));
                Slots[slot] = null;
            }
        }
    }

    public enum InventoryType : byte
    {
        Chest = 0,
        Workbench = 1,
        Furnace = 2,
        Dispenser = 3,
        EnchantmentTable = 4,
        BrewingStand = 5,
        NPCTrade = 6,
        Beacon = 7,
        Anvil = 8,
        Hopper = 9,
        Dropper = 10,
        Horse = 11
    }
}
