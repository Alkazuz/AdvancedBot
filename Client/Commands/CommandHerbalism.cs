using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdvancedBot.client.Map;

namespace AdvancedBot.client.Commands
{
    public class CommandHerbalism : CommandBase
    {
        public CommandHerbalism(MinecraftClient cli)
            : base(cli, "Herbalismo", "'Macro' de herbalismo.", "herbalismo")
        {
            ToggleText = "§6Herbalismo {0}";
        }
        public override CommandResult Run(string alias, string[] args)
        {
            Toggle(args);
            return CommandResult.Success;
        }
        public override void Tick()
        {
            if (IsToggled) {
                int px = Utils.Floor(Player.PosX);
                int py = Utils.Floor(Player.AABB.MinY);
                int pz = Utils.Floor(Player.PosZ);

                Player.LookToBlock(px, py, pz, false);

                HitResult hit = Player.RayCastBlocks(6);
                if (hit == null) {
                    Client.PrintToChat("§6HERBALISMO: §cNão foi possível encontrar o bloco.");
                    IsToggled = false;
                    return;
                }

                int blockId = Client.World.GetBlock(hit.X, hit.Y, hit.Z);
                if (hit.Y == py - 1) {
                    if (Client.ItemInHand == null || Client.ItemInHand.ID != Items.reeds) {
                        for (int i = 0; i < 9; i++) {
                            ItemStack stack = Client.GetHotbarItem(i);
                            if (stack != null && stack.ID == Items.reeds) {
                                Client.HotbarSlot = i;
                                break;
                            }
                        }
                    }

                    if (Client.ItemInHand != null && Client.ItemInHand.ID == Items.reeds) {
                        Client.PlaceCurrentBlock(hit);
                    }
                } else if (blockId == Blocks.reeds) {
                    Client.BreakBlock(hit);
                    if (hit.Y != py && Client.World.GetBlock(hit.X, hit.Y - 1, hit.Z) == Blocks.reeds) {
                        Client.World.SetBlockAndData(hit.X, hit.Y, hit.Z, 0, 0);
                        hit = Player.RayCastBlocks(6);
                        Client.BreakBlock(hit);
                    }
                }
            }
        }
    }
}
