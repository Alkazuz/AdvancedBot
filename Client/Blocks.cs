using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using AdvancedBot.client.Map;
using System.IO;
using Ionic.Zlib;
using Newtonsoft.Json;

namespace AdvancedBot.client
{
    public class Blocks
    {
        public const int air = 0;
        public const int stone = 1;
        public const int grass = 2;
        public const int dirt = 3;
        public const int cobblestone = 4;
        public const int planks = 5;
        public const int sapling = 6;
        public const int bedrock = 7;
        public const int flowing_water = 8;
        public const int water = 9;
        public const int flowing_lava = 10;
        public const int lava = 11;
        public const int sand = 12;
        public const int gravel = 13;
        public const int gold_ore = 14;
        public const int iron_ore = 15;
        public const int coal_ore = 16;
        public const int log = 17;
        public const int leaves = 18;
        public const int sponge = 19;
        public const int glass = 20;
        public const int lapis_ore = 21;
        public const int lapis_block = 22;
        public const int dispenser = 23;
        public const int sandstone = 24;
        public const int noteblock = 25;
        public const int bed = 26;
        public const int golden_rail = 27;
        public const int detector_rail = 28;
        public const int sticky_piston = 29;
        public const int web = 30;
        public const int tallgrass = 31;
        public const int deadbush = 32;
        public const int piston = 33;
        public const int piston_head = 34;
        public const int wool = 35;
        public const int piston_extension = 36;
        public const int yellow_flower = 37;
        public const int red_flower = 38;
        public const int brown_mushroom = 39;
        public const int red_mushroom = 40;
        public const int gold_block = 41;
        public const int iron_block = 42;
        public const int double_stone_slab = 43;
        public const int stone_slab = 44;
        public const int brick_block = 45;
        public const int tnt = 46;
        public const int bookshelf = 47;
        public const int mossy_cobblestone = 48;
        public const int obsidian = 49;
        public const int torch = 50;
        public const int fire = 51;
        public const int mob_spawner = 52;
        public const int oak_stairs = 53;
        public const int chest = 54;
        public const int redstone_wire = 55;
        public const int diamond_ore = 56;
        public const int diamond_block = 57;
        public const int crafting_table = 58;
        public const int wheat = 59;
        public const int farmland = 60;
        public const int furnace = 61;
        public const int lit_furnace = 62;
        public const int standing_sign = 63;
        public const int wooden_door = 64;
        public const int ladder = 65;
        public const int rail = 66;
        public const int stone_stairs = 67;
        public const int wall_sign = 68;
        public const int lever = 69;
        public const int stone_pressure_plate = 70;
        public const int iron_door = 71;
        public const int wooden_pressure_plate = 72;
        public const int redstone_ore = 73;
        public const int lit_redstone_ore = 74;
        public const int unlit_redstone_torch = 75;
        public const int redstone_torch = 76;
        public const int stone_button = 77;
        public const int snow_layer = 78;
        public const int ice = 79;
        public const int snow = 80;
        public const int cactus = 81;
        public const int clay = 82;
        public const int reeds = 83;
        public const int jukebox = 84;
        public const int fence = 85;
        public const int pumpkin = 86;
        public const int netherrack = 87;
        public const int soul_sand = 88;
        public const int glowstone = 89;
        public const int portal = 90;
        public const int lit_pumpkin = 91;
        public const int cake = 92;
        public const int unpowered_repeater = 93;
        public const int powered_repeater = 94;
        public const int stained_glass = 95;
        public const int trapdoor = 96;
        public const int monster_egg = 97;
        public const int stonebrick = 98;
        public const int brown_mushroom_block = 99;
        public const int red_mushroom_block = 100;
        public const int iron_bars = 101;
        public const int glass_pane = 102;
        public const int melon_block = 103;
        public const int pumpkin_stem = 104;
        public const int melon_stem = 105;
        public const int vine = 106;
        public const int fence_gate = 107;
        public const int brick_stairs = 108;
        public const int stone_brick_stairs = 109;
        public const int mycelium = 110;
        public const int waterlily = 111;
        public const int nether_brick = 112;
        public const int nether_brick_fence = 113;
        public const int nether_brick_stairs = 114;
        public const int nether_wart = 115;
        public const int enchanting_table = 116;
        public const int brewing_stand = 117;
        public const int cauldron = 118;
        public const int end_portal = 119;
        public const int end_portal_frame = 120;
        public const int end_stone = 121;
        public const int dragon_egg = 122;
        public const int redstone_lamp = 123;
        public const int lit_redstone_lamp = 124;
        public const int double_wooden_slab = 125;
        public const int wooden_slab = 126;
        public const int cocoa = 127;
        public const int sandstone_stairs = 128;
        public const int emerald_ore = 129;
        public const int ender_chest = 130;
        public const int tripwire_hook = 131;
        public const int tripwire = 132;
        public const int emerald_block = 133;
        public const int spruce_stairs = 134;
        public const int birch_stairs = 135;
        public const int jungle_stairs = 136;
        public const int command_block = 137;
        public const int beacon = 138;
        public const int cobblestone_wall = 139;
        public const int flower_pot = 140;
        public const int carrots = 141;
        public const int potatoes = 142;
        public const int wooden_button = 143;
        public const int skull = 144;
        public const int anvil = 145;
        public const int trapped_chest = 146;
        public const int light_weighted_pressure_plate = 147;
        public const int heavy_weighted_pressure_plate = 148;
        public const int unpowered_comparator = 149;
        public const int powered_comparator = 150;
        public const int daylight_detector = 151;
        public const int redstone_block = 152;
        public const int quartz_ore = 153;
        public const int hopper = 154;
        public const int quartz_block = 155;
        public const int quartz_stairs = 156;
        public const int activator_rail = 157;
        public const int dropper = 158;
        public const int stained_hardened_clay = 159;
        public const int stained_glass_pane = 160;
        public const int leaves2 = 161;
        public const int log2 = 162;
        public const int acacia_stairs = 163;
        public const int dark_oak_stairs = 164;
        public const int slime = 165;
        public const int barrier = 166;
        public const int iron_trapdoor = 167;
        public const int prismarine = 168;
        public const int sea_lantern = 169;
        public const int hay_block = 170;
        public const int carpet = 171;
        public const int hardened_clay = 172;
        public const int coal_block = 173;
        public const int packed_ice = 174;
        public const int double_plant = 175;
        public const int standing_banner = 176;
        public const int wall_banner = 177;
        public const int daylight_detector_inverted = 178;
        public const int red_sandstone = 179;
        public const int red_sandstone_stairs = 180;
        public const int double_stone_slab2 = 181;
        public const int stone_slab2 = 182;
        public const int spruce_fence_gate = 183;
        public const int birch_fence_gate = 184;
        public const int jungle_fence_gate = 185;
        public const int dark_oak_fence_gate = 186;
        public const int acacia_fence_gate = 187;
        public const int spruce_fence = 188;
        public const int birch_fence = 189;
        public const int jungle_fence = 190;
        public const int dark_oak_fence = 191;
        public const int acacia_fence = 192;
        public const int spruce_door = 193;
        public const int birch_door = 194;
        public const int jungle_door = 195;
        public const int acacia_door = 196;
        public const int dark_oak_door = 197;
        public const int end_rod = 198;
        public const int chorus_plant = 199;
        public const int chorus_flower = 200;
        public const int purpur_block = 201;
        public const int purpur_pillar = 202;
        public const int purpur_stairs = 203;
        public const int purpur_double_slab = 204;
        public const int purpur_slab = 205;
        public const int end_bricks = 206;
        public const int beetroots = 207;
        public const int grass_path = 208;
        public const int end_gateway = 209;
        public const int repeating_command_block = 210;
        public const int chain_command_block = 211;
        public const int frosted_ice = 212;
        public const int magma = 213;
        public const int nether_wart_block = 214;
        public const int red_nether_brick = 215;
        public const int bone_block = 216;
        public const int structure_void = 217;
        public const int observer = 218;
        public const int white_shulker_box = 219;
        public const int orange_shulker_box = 220;
        public const int magenta_shulker_box = 221;
        public const int light_blue_shulker_box = 222;
        public const int yellow_shulker_box = 223;
        public const int lime_shulker_box = 224;
        public const int pink_shulker_box = 225;
        public const int gray_shulker_box = 226;
        public const int light_gray_shulker_box = 227;
        public const int cyan_shulker_box = 228;
        public const int purple_shulker_box = 229;
        public const int blue_shulker_box = 230;
        public const int brown_shulker_box = 231;
        public const int green_shulker_box = 232;
        public const int red_shulker_box = 233;
        public const int black_shulker_box = 234;
        public const int structure_block = 255;

        public static bool IsSolid(int id)
        {
            switch (id) {
                case air:
                case sapling:
                case flowing_water:
                case water:
                case flowing_lava:
                case lava:
                case golden_rail:
                case detector_rail:
                case web:
                case tallgrass:
                case deadbush:
                case yellow_flower:
                case red_flower:
                case torch:
                case fire:
                case wheat:
                case standing_sign:
                case rail:
                case wall_sign:
                case lever:
                case stone_pressure_plate:
                case wooden_pressure_plate:
                case unlit_redstone_torch:
                case redstone_torch:
                case stone_button:
                case reeds:
                case portal:
                case cake:
                case redstone_wire:
                case pumpkin_stem:
                case melon_stem:
                case vine:
                case nether_wart:
                case end_portal:
                case tripwire_hook:
                case tripwire:
                case carrots:
                case potatoes:
                case wooden_button:
                case light_weighted_pressure_plate:
                case heavy_weighted_pressure_plate:
                case activator_rail:
                case double_plant:
                case red_mushroom:
                case brown_mushroom:
                case standing_banner:
                case wall_banner:
                case beetroots:
                case end_gateway:
                    return false;
                default: return true;
            }
        }

        //private static Dictionary<int, Block> blocks = new Dictionary<int, Block>();
        private static Block[] blocks = new Block[256];
        private static Dictionary<string, Block> blocksByName = new Dictionary<string, Block>(StringComparer.OrdinalIgnoreCase);
        static Blocks()
        {
            try {
                JArray ja = JArray.Parse(Encoding.UTF8.GetString(Properties.Resources.blocks));
                foreach (JObject obj in ja) {
                    Block block = new Block {
                        ID = obj["id"].AsInt(),
                        DisplayName = obj["displayName"].AsStr(),
                        Name = obj["name"].AsStr(),
                        Hardness = obj["hardness"].Type == JTokenType.Null ? -1f : obj["hardness"].AsFloat(),
                        Diggable = obj["diggable"].AsBool(),
                        Transparent = obj["transparent"].AsBool(),
                        Solid = !obj["boundingBox"].AsStr().Equals("empty"),
                        StackSize = obj["stackSize"].AsInt()
                    };
                    JToken tmp;
                    if ((tmp = obj["harvestTools"]) != null) {
                        foreach (JProperty prop in tmp)
                            if (prop.Value.AsBool())
                                block.HarvestTools.Add(int.Parse(prop.Name));
                    }
                    if ((tmp = obj["variations"]) != null) {
                        foreach (JObject vari in tmp) {
                            block.Variations[vari["metadata"].AsInt()] = vari["displayName"].AsStr();
                        }
                    }
                    if ((tmp = obj["material"]) != null)
                        block.Material = tmp.AsStr();

                    //Debug.WriteLine("public const int " + name + " = " + ID +";");

                    blocks[obj["id"].AsInt()] = block;
                    blocksByName[block.Name] = block;
                }
            } catch (Exception e) {
                Debug.WriteLine("Error while loading blocks.json: \n\n" + e.ToString());
            }
        }

        public static Block GetBlockByName(string name)
        {
            return blocksByName.TryGetValue(name, out Block b) ? b : null;
        }
        public static Block GetBlock(int id)
        {
            return id >= 0 && id < 256 ? blocks[id] : null;
        }
        public static IEnumerable<Block> GetBlocks()
        {
            return blocks.Where(a => a != null);
        }
        public static bool IsSolid2(int id)
        {
            Block b = GetBlock(id);
            return b != null && b.Solid;
        }
        public static bool IsTransparent(int id)
        {
            Block b = GetBlock(id);
            return b != null && b.Transparent;
        }
        public static float GetHardness(int id)
        {
            Block b = GetBlock(id);
            return b == null ? -1 : b.Hardness;
        }
        public static bool CanToolHarvestBlock(int toolId, int blockId)
        {
            Block b = GetBlock(blockId);
            return b == null ? false : b.HarvestTools.Contains(toolId);
        }
        public static string GetDisplayName(int id)
        {
            Block b = GetBlock(id);
            return b == null ? "Unknown" : b.DisplayName;
        }
        public static string GetName(int id)
        {
            Block b = GetBlock(id);
            return b == null ? "unknown" : b.Name;
        }
        public static bool CanHarvest(int id)
        {
            Block b = GetBlock(id);
            return b == null ? false : b.Diggable;
        }
        public static bool IsLiquid(int id)
        {
            return id >= 8 && id <= 11;
        }
    }
    public class Block
    {
        public int ID;
        public string Name, DisplayName, Material;
        public bool Diggable, Transparent, Solid;
        public float Hardness;
        public List<int> HarvestTools = new List<int>();
        public string[] Variations = new string[16];
        public int StackSize;

        public bool ShouldCullAgainst()
        {
            return !Transparent && Solid;
        }

        public override bool Equals(object obj)
        {
            return obj is Block b && b.ID == ID;
        }
        public override int GetHashCode()
        {
            return ID;
        }
    }
    public class BlockState
    {
        public Block Block;
        public int Data;
        public string Variant;

        public int BlockID => Block.ID;
        public int StateID => Block.ID << 4 | Data;

        public Dictionary<string, string> Properties = new Dictionary<string, string>();

        private static BlockState[] states = new BlockState[0x1000];
        private static BlockState[] defaultStates = new BlockState[0x100];
        static BlockState()
        {
            JArray jarr = JArray.Parse(Encoding.UTF8.GetString(AdvancedBot.Properties.Resources.states));
            foreach(var obj in jarr) {
                int id = obj["state_id"].AsInt();
                BlockState state = new BlockState {
                    Block = Blocks.GetBlock(id >> 4),
                    Data = id & 0xF,
                    Variant = obj["variant"].AsStrOr(obj["name"].AsStr())
                };

                foreach (var prop in obj["properties"]) {
                    state.Properties[prop["name"].AsStr()] = prop["value"].AsStr();
                }

                states[id] = state;
                if(obj["default"].AsBool()) {
                    defaultStates[id >> 4] = state;
                }
            }
            for (int i = 0; i < 256; i++) {
                if(defaultStates[i] == null) {
                    defaultStates[i] = Enumerable.Range(0, 16).Select(a => FromID(i << 4 | a)).FirstOrDefault(a => a != null);
                }
            }
        }
        public static BlockState FromID(int id)
        {
            return states[id & 0xFFF] ?? GetDefaultState(id >> 4);
        }
        public static BlockState GetDefaultState(int blockId)
        {
            return blockId >= 0 && blockId < 256 ? defaultStates[blockId] : null;
        }

        public override string ToString()
        {
            return $"BlockState[ID={Block.ID}, Variant={Variant}, Properties='{string.Join(",", Properties.Select(a => a.Key + "=" + a.Value))}']";
        }
    }
}
