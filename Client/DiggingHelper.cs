using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdvancedBot.client.Map;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace AdvancedBot.client
{
    //https://github.com/PrismarineJS/minecraft-data/tree/master/data/pc/1.11

    /* wiki.vg/How_to_Write_a_Client.html + MPC source
     * 
     * sum = 0
     * every tick, sum += StrengthVsBlock
     * when sum >= 1.0, block is broken
     */
    public class DiggingHelper
    {
        private class Tool
        {
            public string materialName;
            public int toolID;
            public float strength;
            public Tool(string name, int id, float str)
            {
                materialName = name;
                toolID = id;
                strength = str;
            }
        }
        private static List<Tool> tools = new List<Tool>();
        static DiggingHelper()
        {
            try {
                JObject jo = JObject.Parse(Encoding.UTF8.GetString(Properties.Resources.materials));
                foreach (JProperty prop in jo.Properties()) {
                    foreach (JProperty mat in prop.Value.ToObject<JObject>().Properties()) {
                        tools.Add(new Tool(prop.Name, int.Parse(mat.Name), mat.Value.ToObject<float>()));
                    }
                }
            } catch (Exception e) {
                Debug.WriteLine("Error while loading materials.json: \n\n" + e.ToString());
            }
        }

        public static float StrengthVsBlock(MinecraftClient c, Vec3i pos)
        {
            return StrengthVsBlock(c, c.World.GetBlock(pos.X, pos.Y, pos.Z));
        }
        public static float StrengthVsBlock(MinecraftClient c, int id)
        {
            Block block = Blocks.GetBlock(id);
            float hardness = block.Hardness;
            if (hardness < 0.0f) return 0.0f;

            return !CanHarvestBlock(c.ItemInHand, block) ? PlayerStrengthVsBlock(c, block) / hardness / 100.0f : 
                                                           PlayerStrengthVsBlock(c, block) / hardness / 30.0f;
        }
        public static bool CanHarvestBlock(ItemStack item, Block block)
        {
            if (block.HarvestTools.Count == 0/*block.Material.isToolNotRequired()*/)
                return true;
            else {
                //ItemStack var2 = getStackInSlot(currentItem);
                //return var2 != null ? var2.canHarvestBlock(block) : false;

                return item != null ? block.HarvestTools.Contains(item.ID) : false;
            }
        }
        public static float ToolStrengthVsBlock(ItemStack tool, Block block)
        {
            float str = 1.0f;

            //ItemStack currentItem = c.Inventory.Slots[36 + c.HotbarSlot];
            if (tool != null) {
                //str *= currentItem.GetStrVsBlock(block);
                foreach (Tool mat in tools) {
                    if (mat.toolID == tool.ID && mat.materialName == block.Material) {
                        str *= mat.strength;
                        break;
                    }
                }
            }

            return str;
        }
        private static float PlayerStrengthVsBlock(MinecraftClient c, Block block)
        {
            ItemStack currentItem = c.ItemInHand;

            float mul = ToolStrengthVsBlock(currentItem, block);

            if (mul > 1.0f) {
                //int effMod = EnchantmentHelper.getEfficiencyModifier(this);
                const int ENCHANTMENT_EFFICIENCY = 32;
                
                int effMod = currentItem == null ? 0 : currentItem.GetEnchantmentLevel(ENCHANTMENT_EFFICIENCY);

                if (effMod > 0 && currentItem != null)
                    mul += effMod * effMod + 1;
            }
            
            const byte HASTE = 3;
            const byte MINING_FATIGUE = 4;

            byte amplifier;
            Entity e = c.Player;
            if (e.ActivePotions.TryGetValue(HASTE, out amplifier))
                mul *= 1.0f + (amplifier + 1) * 0.2f;

            if (e.ActivePotions.TryGetValue(MINING_FATIGUE, out amplifier)) {
                float slownessMul = 1.0F;

                switch (amplifier) {
                    case 0: slownessMul = 0.3f; break;
                    case 1: slownessMul = 0.09f; break;
                    case 2: slownessMul = 0.0027f; break;
                    case 3: 
                    default: slownessMul = 0.00081f; break;
                }

                mul *= slownessMul;
            }

            if (e.IsUnderWater()/* && !EnchantmentHelper.getAquaAffinityModifier(this)*/)
                mul /= 5.0f;

            if (!e.OnGround)
                mul /= 5.0f;

            return mul;
        }
    }
}
