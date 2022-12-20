using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using AdvancedBot.client;
using Newtonsoft.Json.Linq;

namespace AdvancedBot.client
{
    public class Item
    {
        public int ID, StackSize;
        public string DisplayName, Name;
        public Dictionary<int, string> Variations = new Dictionary<int, string>();

        public Item() { }
        public Item(int id, int stackSize, string name, string dn)
        {
            ID = id;
            StackSize = stackSize;
            Name = name;
            DisplayName = dn;
        }
    }
    public class Items
    {
        private static Dictionary<int, Item> items = new Dictionary<int, Item>();

        static Items()
        {
            try {
                JArray ja = JArray.Parse(Encoding.UTF8.GetString(Properties.Resources.items));
                foreach (JObject obj in ja) {
                    Item item = new Item();

                    item.ID = obj["id"].AsInt();
                    item.DisplayName = obj["displayName"].AsStr();
                    item.Name = obj["name"].AsStr();
                    item.StackSize = obj["stackSize"].AsInt();
                    JToken tmp;
                    if ((tmp = obj["variations"]) != null) {
                        foreach (JObject varia in tmp) {
                            item.Variations[varia["metadata"].AsInt()] = varia["displayName"].AsStr();
                        }
                    }

                    items[item.ID] = item;
                }
            } catch (Exception e) {
                Debug.WriteLine("Error while loading items.json: \n\n" + e.ToString());
            }
        }
        public static Item GetItemInfo(int id)
        {
            Item it;
            items.TryGetValue(id, out it);
            return it;
        }
        public static Item GetItemByName(string name)
        {
            foreach (Item i in items.Values)
                if (i.Name.EqualsIgnoreCase(name))
                    return i;
            return null;
        }
        public static string GetName(int id)
        {
            Item i;
            if (items.TryGetValue(id, out i)) {
                return i.Name;
            }
            return "unknown";
        }
        public static string GetDisplayName(int id, int data)
        {
            if (id < 256) {
                Block bl = Blocks.GetBlock(id);
                return bl.Variations[data & 0xF] ?? bl.DisplayName;
            } else {
                Item it = GetItemInfo(id);
                string dn;
                return it.Variations.TryGetValue(data & 0xFFFF, out dn) ? dn : it.DisplayName;
            }
        }

        public const int iron_shovel = 256;
        public const int iron_pickaxe = 257;
        public const int iron_axe = 258;
        public const int flint_and_steel = 259;
        public const int apple = 260;
        public const int bow = 261;
        public const int arrow = 262;
        public const int coal = 263;
        public const int diamond = 264;
        public const int iron_ingot = 265;
        public const int gold_ingot = 266;
        public const int iron_sword = 267;
        public const int wooden_sword = 268;
        public const int wooden_shovel = 269;
        public const int wooden_pickaxe = 270;
        public const int wooden_axe = 271;
        public const int stone_sword = 272;
        public const int stone_shovel = 273;
        public const int stone_pickaxe = 274;
        public const int stone_axe = 275;
        public const int diamond_sword = 276;
        public const int diamond_shovel = 277;
        public const int diamond_pickaxe = 278;
        public const int diamond_axe = 279;
        public const int stick = 280;
        public const int bowl = 281;
        public const int mushroom_stew = 282;
        public const int golden_sword = 283;
        public const int golden_shovel = 284;
        public const int golden_pickaxe = 285;
        public const int golden_axe = 286;
        public const int @string = 287;
        public const int feather = 288;
        public const int gunpowder = 289;
        public const int wooden_hoe = 290;
        public const int stone_hoe = 291;
        public const int iron_hoe = 292;
        public const int diamond_hoe = 293;
        public const int golden_hoe = 294;
        public const int wheat_seeds = 295;
        public const int wheat = 296;
        public const int bread = 297;
        public const int leather_helmet = 298;
        public const int leather_chestplate = 299;
        public const int leather_leggings = 300;
        public const int leather_boots = 301;
        public const int chainmail_helmet = 302;
        public const int chainmail_chestplate = 303;
        public const int chainmail_leggings = 304;
        public const int chainmail_boots = 305;
        public const int iron_helmet = 306;
        public const int iron_chestplate = 307;
        public const int iron_leggings = 308;
        public const int iron_boots = 309;
        public const int diamond_helmet = 310;
        public const int diamond_chestplate = 311;
        public const int diamond_leggings = 312;
        public const int diamond_boots = 313;
        public const int golden_helmet = 314;
        public const int golden_chestplate = 315;
        public const int golden_leggings = 316;
        public const int golden_boots = 317;
        public const int flint = 318;
        public const int porkchop = 319;
        public const int cooked_porkchop = 320;
        public const int painting = 321;
        public const int golden_apple = 322;
        public const int sign = 323;
        public const int wooden_door = 324;
        public const int bucket = 325;
        public const int water_bucket = 326;
        public const int lava_bucket = 327;
        public const int minecart = 328;
        public const int saddle = 329;
        public const int iron_door = 330;
        public const int redstone = 331;
        public const int snowball = 332;
        public const int boat = 333;
        public const int leather = 334;
        public const int milk_bucket = 335;
        public const int brick = 336;
        public const int clay_ball = 337;
        public const int reeds = 338;
        public const int paper = 339;
        public const int book = 340;
        public const int slime_ball = 341;
        public const int chest_minecart = 342;
        public const int furnace_minecart = 343;
        public const int egg = 344;
        public const int compass = 345;
        public const int fishing_rod = 346;
        public const int clock = 347;
        public const int glowstone_dust = 348;
        public const int fish = 349;
        public const int cooked_fish = 350;
        public const int dye = 351;
        public const int bone = 352;
        public const int sugar = 353;
        public const int cake = 354;
        public const int bed = 355;
        public const int repeater = 356;
        public const int cookie = 357;
        public const int filled_map = 358;
        public const int shears = 359;
        public const int melon = 360;
        public const int pumpkin_seeds = 361;
        public const int melon_seeds = 362;
        public const int beef = 363;
        public const int cooked_beef = 364;
        public const int chicken = 365;
        public const int cooked_chicken = 366;
        public const int rotten_flesh = 367;
        public const int ender_pearl = 368;
        public const int blaze_rod = 369;
        public const int ghast_tear = 370;
        public const int gold_nugget = 371;
        public const int nether_wart = 372;
        public const int potion = 373;
        public const int glass_bottle = 374;
        public const int spider_eye = 375;
        public const int fermented_spider_eye = 376;
        public const int blaze_powder = 377;
        public const int magma_cream = 378;
        public const int brewing_stand = 379;
        public const int cauldron = 380;
        public const int ender_eye = 381;
        public const int speckled_melon = 382;
        public const int spawn_egg = 383;
        public const int experience_bottle = 384;
        public const int fire_charge = 385;
        public const int writable_book = 386;
        public const int written_book = 387;
        public const int emerald = 388;
        public const int item_frame = 389;
        public const int flower_pot = 390;
        public const int carrot = 391;
        public const int potato = 392;
        public const int baked_potato = 393;
        public const int poisonous_potato = 394;
        public const int map = 395;
        public const int golden_carrot = 396;
        public const int skull = 397;
        public const int carrot_on_a_stick = 398;
        public const int nether_star = 399;
        public const int pumpkin_pie = 400;
        public const int fireworks = 401;
        public const int firework_charge = 402;
        public const int enchanted_book = 403;
        public const int comparator = 404;
        public const int netherbrick = 405;
        public const int quartz = 406;
        public const int tnt_minecart = 407;
        public const int hopper_minecart = 408;
        public const int prismarine_shard = 409;
        public const int prismarine_crystals = 410;
        public const int rabbit = 411;
        public const int cooked_rabbit = 412;
        public const int rabbit_stew = 413;
        public const int rabbit_foot = 414;
        public const int rabbit_hide = 415;
        public const int armor_stand = 416;
        public const int iron_horse_armor = 417;
        public const int golden_horse_armor = 418;
        public const int diamond_horse_armor = 419;
        public const int lead = 420;
        public const int name_tag = 421;
        public const int command_block_minecart = 422;
        public const int mutton = 423;
        public const int cooked_mutton = 424;
        public const int banner = 425;
        public const int end_crystal = 426;
        public const int spruce_door = 427;
        public const int birch_door = 428;
        public const int jungle_door = 429;
        public const int acacia_door = 430;
        public const int dark_oak_door = 431;
        public const int chorus_fruit = 432;
        public const int chorus_fruit_popped = 433;
        public const int beetroot = 434;
        public const int beetroot_seeds = 435;
        public const int beetroot_soup = 436;
        public const int dragon_breath = 437;
        public const int splash_potion = 438;
        public const int spectral_arrow = 439;
        public const int tipped_arrow = 440;
        public const int lingering_potion = 441;
        public const int shield = 442;
        public const int elytra = 443;
        public const int spruce_boat = 444;
        public const int birch_boat = 445;
        public const int jungle_boat = 446;
        public const int acacia_boat = 447;
        public const int dark_oak_boat = 448;
        public const int totem_of_undying = 449;
        public const int shulker_shell = 450;
        public const int iron_nugget = 452;
        public const int record_13 = 2256;
        public const int record_cat = 2257;
        public const int record_blocks = 2258;
        public const int record_chirp = 2259;
        public const int record_far = 2260;
        public const int record_mall = 2261;
        public const int record_mellohi = 2262;
        public const int record_stal = 2263;
        public const int record_strad = 2264;
        public const int record_ward = 2265;
        public const int record_11 = 2266;
        public const int record_wait = 2267;
    }
}
