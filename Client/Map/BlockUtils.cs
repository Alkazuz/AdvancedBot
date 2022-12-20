using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace AdvancedBot.client.Map
{
    public class BlockUtils
    {
        public static void AddAABBsToList(World w, int x, int y, int z, List<AABB> list)
        {
            byte id = w.GetBlock(x, y, z);
            byte data = w.GetData(x, y, z);

            if (id != Blocks.snow_layer && id != Blocks.stone_pressure_plate &&
                id != Blocks.wooden_pressure_plate && id != Blocks.light_weighted_pressure_plate &&
                id != Blocks.heavy_weighted_pressure_plate && !Blocks.IsSolid(id)) return;

            AABB result = null;

            switch (id)
            {
                case Blocks.stone_slab:
                case Blocks.wooden_slab:
                case Blocks.stone_slab2:
                case Blocks.purpur_slab:
                    if ((data & 8) != 0) //top?
                        result = new AABB(0.0, 0.5, 0.0, 1.0, 1.0, 1.0);
                    else
                        result = new AABB(0.0, 0.0, 0.0, 1.0, 0.5, 1.0);
                    break;
                case Blocks.bed: result = new AABB(0.0, 0.0, 0.0, 1.0, 0.5625, 1.0); break;
                case Blocks.wooden_door:
                case Blocks.iron_door:
                case Blocks.acacia_door:
                case Blocks.birch_door:
                case Blocks.dark_oak_door:
                case Blocks.jungle_door:
                case Blocks.spruce_door:
                    {
                        const double thick = 0.1875;

                        AABB aabb = null;
                        byte flags = GetFullDoorMeta(w, x, y, z);
                        int facing = flags & 3;
                        bool isOpen = (flags & 4) != 0;
                        bool isHinge = (flags & 16) != 0;

                        if (facing == 0) {
                            if (isOpen)
                                aabb = isHinge ? new AABB(0.0, 0.0, 1.0 - thick, 1.0, 1.0, 1.0) : new AABB(0.0, 0.0, 0.0, 1.0, 1.0, thick);
                            else
                                aabb = new AABB(0.0, 0.0, 0.0, thick, 1.0, 1.0);
                        } else if (facing == 1) {
                            if (isOpen)
                                aabb = isHinge ? new AABB(0.0, 0.0, 0.0, thick, 1.0, 1.0) : new AABB(1.0 - thick, 0.0, 0.0, 1.0, 1.0, 1.0);
                            else
                                aabb = new AABB(0.0, 0.0, 0.0, 1.0, 1.0, thick);
                        } else if (facing == 2) {
                            if (isOpen)
                                aabb = isHinge ? new AABB(0.0, 0.0, 0.0, 1.0, 1.0, thick) : new AABB(0.0, 0.0, 1.0 - thick, 1.0, 1.0, 1.0);
                            else
                                aabb = new AABB(1.0 - thick, 0.0, 0.0, 1.0, 1.0, 1.0);
                        } else if (facing == 3) {
                            if (isOpen)
                                aabb = isHinge ? new AABB(1.0 - thick, 0.0, 0.0, 1.0, 1.0, 1.0) : new AABB(0.0, 0.0, 0.0, thick, 1.0, 1.0);
                            else
                                aabb = new AABB(0.0, 0.0, 1.0 - thick, 1.0, 1.0, 1.0);
                        }
                        result = aabb;
                    }
                    break;
                case Blocks.ladder:
                    {
                        double thick = w.Owner.Version >= ClientVersion.v1_9 ? 0.1875 : 0.125;
                        switch (data)
                        {
                            case 2: result = new AABB(0.0, 0.0, 1.0 - thick, 1.0, 1.0, 1.0); break;
                            case 3: result = new AABB(0.0, 0.0, 0.0, 1.0, 1.0, thick); break;
                            case 4: result = new AABB(1.0 - thick, 0.0, 0.0, 1.0, 1.0, 1.0); break;
                            case 5: result = new AABB(0.0, 0.0, 0.0, thick, 1.0, 1.0); break;
                            default: result = null; break;
                        }
                    }
                    break;
                case Blocks.snow_layer: result = new AABB(0.0, 0.0, 0.0, 1.0, (data & 7) * 0.125, 1.0); break;
                case Blocks.soul_sand: result = new AABB(0.0, 0.0, 0.0, 1.0, 0.875, 1.0); break;
                case Blocks.cake:
                    {
                        double length = 0.0625;
                        double width = (1 + data * 2) / 16.0;
                        result = new AABB(width, 0.0, length, 1.0 - length, 0.5, 1.0 - length);
                    }
                    break;
                case Blocks.unpowered_repeater:
                case Blocks.powered_repeater:
                case Blocks.unpowered_comparator:
                case Blocks.powered_comparator:
                    result = new AABB(0.0, 0.0, 0.0, 1.0, 0.125, 1.0);
                    break;
                case Blocks.trapdoor:
                case Blocks.iron_trapdoor:
                    {
                        double thick = 0.1875;
                        AABB aabb;
                        if ((data & 8) != 0)
                            aabb = new AABB(0.0, 1.0 - thick, 0.0, 1.0, 1.0, 1.0);
                        else
                            aabb = new AABB(0.0, 0.0, 0.0, 1.0, thick, 1.0);

                        if ((data & 4) != 0) {
                            int d = data & 3;
                            if (d == 0) aabb = new AABB(0.0, 0.0, 1.0 - thick, 1.0, 1.0, 1.0);
                            if (d == 1) aabb = new AABB(0.0, 0.0, 0.0, 1.0, 1.0, thick);
                            if (d == 2) aabb = new AABB(1.0 - thick, 0.0, 0.0, 1.0, 1.0, 1.0);
                            if (d == 3) aabb = new AABB(0.0, 0.0, 0.0, thick, 1.0, 1.0);
                        }
                        result = aabb;
                    }
                    break;
                case Blocks.fence_gate:
                case Blocks.acacia_fence_gate:
                case Blocks.birch_fence_gate:
                case Blocks.dark_oak_fence_gate:
                case Blocks.jungle_fence_gate:
                case Blocks.spruce_fence_gate:
                    {
                        if (IsFenceGateOpen(data)) result = null;
                        result = (data != 2 && data != 0 ?
                                     new AABB(0.375, 0.0, 0.0, 0.625, 1.5, 1.0) :
                                     new AABB(0.0, 0.0, 0.375, 1.0, 1.5, 0.625));
                    }
                    break;
                case Blocks.waterlily: result = new AABB(0.0, 0.0, 0.0, 1.0, 0.015625, 1.0); break;
                case Blocks.fence:
                case Blocks.acacia_fence:
                case Blocks.birch_fence:
                case Blocks.dark_oak_fence:
                case Blocks.jungle_fence:
                case Blocks.spruce_fence:
                case Blocks.nether_brick_fence:
                    {
                        double minX = 0.375;
                        double maxX = 0.625;
                        double minZ = 0.375;
                        double maxZ = 0.625;

                        if (CanFenceConnect(w, x, y, z - 1)) minZ = 0.0;
                        if (CanFenceConnect(w, x, y, z + 1)) maxZ = 1.0;
                        if (CanFenceConnect(w, x - 1, y, z)) minX = 0.0;
                        if (CanFenceConnect(w, x + 1, y, z)) maxX = 1.0;

                        result = new AABB(minX, 0.0, minZ, maxX, 1.5, maxZ);
                    }
                    break;
                case Blocks.cobblestone_wall:
                    {
                        bool connectNorth = CanWallConnect(w, x, y, z - 1);
                        bool connectSouth = CanWallConnect(w, x, y, z + 1);
                        bool connectWest = CanWallConnect(w, x - 1, y, z);
                        bool connectEast = CanWallConnect(w, x + 1, y, z);
                        double minX = 0.25;
                        double maxX = 0.75;
                        double minZ = 0.25;
                        double maxZ = 0.75;

                        if (connectNorth) minZ = 0.0;
                        if (connectSouth) maxZ = 1.0;
                        if (connectWest) minX = 0.0;
                        if (connectEast) maxX = 1.0;

                        if (connectNorth && connectSouth && !connectWest && !connectEast)
                        {
                            minX = 0.3125;
                            maxX = 0.6875;
                        } else if (!connectNorth && !connectSouth && connectWest && connectEast) {
                            minZ = 0.3125;
                            maxZ = 0.6875;
                        }

                        result = new AABB(minX, 0, minZ, maxX, 1.5, maxZ);
                    }
                    break;
                case Blocks.flower_pot:
                    {
                        double height = 0.375;
                        double width = height / 2.0;
                        result = new AABB(0.5 - width, 0.0, 0.5 - width, 0.5 + width, height, 0.5 + width);
                    }
                    break;
                case Blocks.skull:
                    switch (data & 7)
                    {
                        case 1:
                        default:
                            result = new AABB(0.25, 0.0, 0.25, 0.75, 0.5, 0.75); break;
                        case 2: result = new AABB(0.25, 0.25, 0.5, 0.75, 0.75, 1.0); break;
                        case 3: result = new AABB(0.25, 0.25, 0.0, 0.75, 0.75, 0.5); break;
                        case 4: result = new AABB(0.5, 0.25, 0.25, 1.0, 0.75, 0.75); break;
                        case 5: result = new AABB(0.0, 0.25, 0.25, 0.5, 0.75, 0.75); break;
                    }
                    break;
                case Blocks.anvil:
                    int j = data & 3;
                    if (j != 3 && j != 1)
                        result = new AABB(0.125, 0.0, 0.0, 0.875, 1.0, 1.0);
                    else
                        result = new AABB(0.0, 0.0, 0.125, 1.0, 1.0, 0.875);
                    break;
                case Blocks.carpet: result = new AABB(0.0, 0.0, 0.0, 1.0, 0.0625, 1.0); break;
                case Blocks.acacia_stairs:
                case Blocks.birch_stairs:
                case Blocks.brick_stairs:
                case Blocks.dark_oak_stairs:
                case Blocks.jungle_stairs:
                case Blocks.nether_brick_stairs:
                case Blocks.oak_stairs:
                case Blocks.quartz_stairs:
                case Blocks.sandstone_stairs:
                case Blocks.spruce_stairs:
                case Blocks.stone_brick_stairs:
                case Blocks.stone_stairs:
                    float ys = 0.5F;

                    AABB step1 = null;
                    AABB step2 = null;

                    if ((data & 4) != 0) //is top?
                    {
                        step1 = new AABB(0.0F, 0.5F, 0.0F, 1.0F, 1.0F, 1.0F);
                        ys = 0F;
                    } else
                        step1 = new AABB(0.0F, 0.0F, 0.0F, 1.0F, 0.5F, 1.0F);

                    int side = data & 3;

                    if (side == 0) step2 = new AABB(0.5F, ys, 0F, 1F, ys + 0.5F, 1F);
                    else if (side == 1) step2 = new AABB(0F, ys, 0F, 0.5F, ys + 0.5F, 1F);
                    else if (side == 2) step2 = new AABB(0F, ys, 0.5F, 1F, ys + 0.5F, 1F);
                    else if (side == 3) step2 = new AABB(0F, ys, 0F, 1F, ys + 0.5F, 0.5F);

                    step1.Move(x, y, z);
                    step2.Move(x, y, z);
                    list.Add(step1);
                    list.Add(step2);
                    break;
                case Blocks.brewing_stand:
                    list.Add(new AABB(0.4375F, 0.0F, 0.4375F, 0.5625F, 0.875F, 0.5625F).MoveAndGet(x, y, z));
                    list.Add(new AABB(0.0F, 0.0F, 0.0F, 1.0F, 0.125F, 1.0F).MoveAndGet(x, y, z));
                    break;
                case Blocks.daylight_detector:
                case Blocks.daylight_detector_inverted:
                    result = new AABB(0.0F, 0.0F, 0.0F, 1.0F, 0.375F, 1.0F); break;
                case Blocks.chest:
                case Blocks.trapped_chest:
                    if (w.GetBlock(x, y, z - 1) == id)
                        result = new AABB(0.0625F, 0.0F, 0.0F, 0.9375F, 0.875F, 0.9375F);
                    else if (w.GetBlock(x, y, z + 1) == id)
                        result = new AABB(0.0625F, 0.0F, 0.0625F, 0.9375F, 0.875F, 1.0F);
                    else if (w.GetBlock(x - 1, y, z) == id)
                        result = new AABB(0.0F, 0.0F, 0.0625F, 0.9375F, 0.875F, 0.9375F);
                    else if (w.GetBlock(x + 1, y, z) == id)
                        result = new AABB(0.0625F, 0.0F, 0.0625F, 1.0F, 0.875F, 0.9375F);
                    else
                        result = new AABB(0.0625F, 0.0F, 0.0625F, 0.9375F, 0.875F, 0.9375F);
                    break;
                case Blocks.ender_chest: result = new AABB(0.0625F, 0.0F, 0.0625F, 0.9375F, 0.875F, 0.9375F); break;
                default: result = new AABB(0.0, 0.0, 0.0, 1.0, 1.0, 1.0); break;
            }
            if(result != null)
                list.Add(result.MoveAndGet(x, y, z));
        }

        public static bool CanFenceConnect(World w, int x, int y, int z)
        {
            byte b = w.GetBlock(x, y, z);
            return (b != Blocks.fence || b != 113) && b != 107 ? (Blocks.IsSolid(b) ? b != 103 : false) : true;
        }
        public static bool CanWallConnect(World w, int x, int y, int z)
        {
            byte b = w.GetBlock(x, y, z);
            return b != Blocks.cobblestone_wall && b != Blocks.fence_gate ? (Blocks.IsSolid(b) ? (b != Blocks.melon_block && b != Blocks.pumpkin) : false) : true;
        }
        public static bool IsFenceGateOpen(int data)
        {
            return (data & 4) != 0;
        }
        public static byte GetFullDoorMeta(World w, int x, int y, int z)
        {
            int d = w.GetData(x, y, z);
            bool isTop = (d & 8) != 0;
            int bottom;
            int top;

            if (isTop) {
                bottom = w.GetData(x, y - 1, z);
                top = d;
            } else {
                bottom = d;
                top = w.GetData(x, y + 1, z);
            }

            bool isHinge = (top & 1) != 0;
            return (byte)(bottom & 7 | 
                          (isTop ? 8 : 0) | 
                          (isHinge ? 16 : 0));
        }

        public static bool IsStairs(int id)
        {
            return id == Blocks.acacia_stairs ||
                   id == Blocks.birch_stairs ||
                   id == Blocks.brick_stairs ||
                   id == Blocks.dark_oak_stairs ||
                   id == Blocks.jungle_stairs ||
                   id == Blocks.nether_brick_stairs ||
                   id == Blocks.oak_stairs ||
                   id == Blocks.quartz_stairs ||
                   id == Blocks.sandstone_stairs ||
                   id == Blocks.spruce_stairs ||
                   id == Blocks.stone_brick_stairs ||
                   id == Blocks.stone_stairs;
        }
        public static float GetFluidHeightPercent(int data)
        {
            if (data >= 8)
                data = 0;
            return (float)(data + 1) / 9F;
        }
        public static char GetPortalDirection(int data)
        {
            switch (data & 3) {
                case 0: return 'Z';
                case 1: return 'X';
                case 2: return 'Z';
                default: return 'V';
            }
        }
    }
}
