using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdvancedBot.client.NBT;

namespace AdvancedBot.client
{
    public class ItemStack
    {
        public short ID = -1;
        public byte Count = 1;
        public short Metadata = 0;
        public CompoundTag NBTData = null;

        public ItemStack() { }
        public ItemStack(short id)
        {
            ID = id;
        }
        public ItemStack(short id, short data)
        {
            ID = id;
            Metadata = data;
        }
        public ItemStack(short id, short data, byte count)
        {
            ID = id;
            Metadata = data;
            Count = count;
        }
        public ItemStack(short id, short data, CompoundTag nbtdata)
        {
            ID = id;
            Metadata = data;
            NBTData = nbtdata;
        }
        public ItemStack(short id, short data, byte count, CompoundTag nbtdata)
        {
            ID = id;
            Metadata = data;
            Count = count;
            NBTData = nbtdata;
        }
        public int GetEnchantmentLevel(int id)
        {
            if (NBTData != null) {
                foreach (CompoundTag tag in NBTData.GetList("ench")) {
                    if (tag.GetShort("id") == id)
                        return tag.GetShort("lvl");
                }
            }
            return 0;
        }
        public string GetEnchantments()
        {
            if (NBTData != null && NBTData.Contains("ench")) {
                StringBuilder sb = new StringBuilder();
                foreach (CompoundTag tag in NBTData.GetList("ench")) {
                    int id  = tag.GetShort("id");
                    int lvl = tag.GetShort("lvl");
                    string name;
                    string lvlS = lvl >= 0 && lvl < 10 ? RomanNums1T10[lvl] : lvl.ToString();
                    if (EnchantmentNames.TryGetValue(id, out name)) {
                        sb.AppendFormat("{0} {1}\n", name, lvlS);
                    } else {
                        sb.AppendFormat("(ID: {0}) {1}\n", id, lvlS);
                    }
                }
                return sb.Length == 0 ? null : sb.Remove(sb.Length - 1, 1).ToString();
            }
            return null;
        }
        public bool HasEnchantments()
        {
            return NBTData != null && NBTData.Contains("ench");
        }
        public bool HasDisplayName()
        {
            return NBTData == null ? false : !NBTData.Contains("display") ? false : NBTData.GetCompound("display").Contains("Name");
        }
        public string GetDisplayName()
        {
            return HasDisplayName() ? NBTData.GetCompound("display").GetString("Name") : Items.GetDisplayName(ID, Metadata);
        }

        public string GetLore()
        {
            if (NBTData != null && NBTData.Contains("display"))
            {
                ListTag<Tag> lore = NBTData.GetCompound("display").GetList("Lore");

                StringBuilder sb = new StringBuilder();
                foreach (Tag tag in lore)
                    sb.AppendLine(((StringTag)tag).Data);
                return sb.ToString();
            }
            return null;
        }
        public void SetDisplayName(string name)
        {
            if (NBTData == null)
                NBTData = new CompoundTag();

            if (!NBTData.Contains("display"))
                NBTData.AddCompound("display", new CompoundTag());

            NBTData.GetCompound("display").AddString("Name", name);
        }

        public bool IsSameItem(ItemStack other)
        {
            return other.ID == ID &&
                   other.Metadata == Metadata &&
                   (other.NBTData == null && NBTData == null ? true :
                   other.NBTData != null && NBTData != null ? other.NBTData.Equals(other.NBTData) : false);
        }
        /// <summary>
        /// This method does not copy the NBT tag
        /// </summary>
        public ItemStack Copy()
        {
            return new ItemStack(ID, Metadata, Count, NBTData);
        }

        private static string[] RomanNums1T10 = "I|II|III|IV|V|VI|VII|VIII|IX|X".Split('|');
        private static Dictionary<int, string> EnchantmentNames = new Dictionary<int, string>() {
            { 0, "Protection" }, 
            { 1, "Fire Protection" }, 
            { 2, "Feather Falling" }, 
            { 3, "Blast Protection" }, 
            { 4, "Projectile Protection" }, 
            { 5, "Respiration" }, 
            { 6, "Aqua Affinity" }, 
            { 7, "Thorns" }, 
            { 8, "Depth Strider" }, 
            { 9, "Frost Walker" }, 
            { 10, "Curse of Binding" }, 
            { 16, "Sharpness" }, 
            { 17, "Smite" }, 
            { 18, "Bane of Arthropods" }, 
            { 19, "Knockback" }, 
            { 20, "Fire Aspect" }, 
            { 21, "Looting" }, 
            { 22, "Sweeping Edge" }, 
            { 32, "Efficiency" }, 
            { 33, "Silk Touch" }, 
            { 34, "Unbreaking" }, 
            { 35, "Fortune" }, 
            { 48, "Power" }, 
            { 49, "Punch" }, 
            { 50, "Flame" }, 
            { 51, "Infinity" }, 
            { 61, "Luck of the Sea" }, 
            { 62, "Lure" }, 
            { 70, "Mending" }, 
            { 71, "Curse of Vanishing" }
        };
    }
}