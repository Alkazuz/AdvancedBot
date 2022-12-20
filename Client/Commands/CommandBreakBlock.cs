using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdvancedBot.client.Map;
using AdvancedBot.client.Packets;

namespace AdvancedBot.client.Commands
{
    public class CommandBreakBlock : CommandBase
    {
        private Vec3i digPos = null;
        private int digFace;
        private long digStart = Utils.GetTimestamp();
        private float digSum = 0.0f;
        public CommandBreakBlock(MinecraftClient cli)
            : base(cli, "BreakBlock", "Quebra o bloco na coordenada especificada.", "breakblock")
        {
            SetParams("<x>", "<y>", "<z>", "[Opções (separe com espaços) ncp: quebra como um player real, at: auto tool, rt: quebra qualquer bloco que estiver na frente, rp=posição relativa]");
        }
        public override CommandResult Run(string alias, string[] args)
        {
            OptionFlags flags = OptionFlags.None;

            if (args.Length < 3) {
                return CommandResult.MissingArgs;
            }
            for (int i = 3; i < args.Length; i++) {
                foreach (OptionFlags of in Enum.GetValues(typeof(OptionFlags))) {
                    string name = Enum.GetName(typeof(OptionFlags), of);
                    if (name.Where(c => Char.IsUpper(c)).SequenceEqual(args[i].Select(c => char.ToUpper(c)))) {
                        flags |= of;
                    }
                }
            }
            int x = int.Parse(args[0]);
            int y = int.Parse(args[1]);
            int z = int.Parse(args[2]);

            if ((flags & OptionFlags.RelativePos) != 0) {
                x += Utils.Floor(Player.PosX);
                y += Utils.Floor(Player.AABB.MinY);
                z += Utils.Floor(Player.PosZ);
            }
            Player.LookToBlock(x, y, z, true);
            HitResult hit = Player.RayCastBlocks(6);

            if ((flags & OptionFlags.RayTrace) == 0 && hit != null && (hit.X != x || hit.Y != y || hit.Z != z)) {
                Client.PrintToChat("§cO bloco não está visível. Use a opção 'rt' para ignorar isso.");
                return CommandResult.ErrorSilent;
            } else if (hit != null) {
                x = hit.X;
                y = hit.Y;
                z = hit.Z;
            } else {
                Client.PrintToChat("§cO bloco está muito longe.");
                return CommandResult.ErrorSilent;
            }
            int id = World.GetBlock(x, y, z);
            if (id == Blocks.air) {
                Client.PrintToChat("§cNão existe nenhum bloco na coordenada especificada.");
                return CommandResult.ErrorSilent;
            }
            if ((flags & OptionFlags.AutoTool) != 0) {
                SelectBestTool(id);
            }

            if ((flags & OptionFlags.NoCheatPlus) != 0) {
                Client.SendPacket(new PacketSwingArm(Client.PlayerID));
                Client.SendPacket(new PacketPlayerDigging(DiggingStatus.StartedDigging, hit.X, (byte)hit.Y, hit.Z, (byte)hit.Face));

                digFace = hit.Face;
                digStart = Utils.GetTimestamp();
                
                digPos = new Vec3i(hit.X, hit.Y, hit.Z);
            } else {
                Client.BreakBlock(hit);
            }

            return CommandResult.Success;
        }
        public override void Tick()
        {
            if (digPos != null) {
                if (Utils.GetTimestamp() - digStart > 15000) {
                    StopDestroyBlock(DiggingStatus.CancelledDigging);
                    return;
                }
                Client.SendPacket(new PacketSwingArm(Client.PlayerID));
                digSum += DiggingHelper.StrengthVsBlock(Client, digPos);
                if (digSum >= 1.0f) {
                    StopDestroyBlock(DiggingStatus.FinishedDigging);
                }
            }
        }
        private void StopDestroyBlock(DiggingStatus ds)
        {
            Client.SendPacket(new PacketPlayerDigging(ds, digPos.X, (byte)digPos.Y, digPos.Z, (byte)digFace));
            digSum = 0.0f;
            digPos = null;
        }

        private void SelectBestTool(int blockId)
        {
            float bestSpeed = 1.0f;
            int bestSlot = -1;

            Block block = Blocks.GetBlock(blockId);
            for (int i = 0; i < 9; i++) {
                ItemStack item = Client.Inventory.Slots[36 + i];

                if (item != null) {
                    float speed = DiggingHelper.ToolStrengthVsBlock(item, block);
                    if (!DiggingHelper.CanHarvestBlock(item, block))
                        speed *= 0.2f;

                    if (speed > bestSpeed) {
                        bestSpeed = speed;
                        bestSlot = i;
                    }
                }
            }
            if (bestSlot != -1) {
                Client.HotbarSlot = bestSlot;
            }
        }
        [Flags]
        private enum OptionFlags
        {
            None = 0x00,
            NoCheatPlus = 0x01,
            AutoTool = 0x02,
            RayTrace = 0x04,
            RelativePos = 0x08
        }
    }
}
