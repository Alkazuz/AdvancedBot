using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdvancedBot.client.Packets;
using System.Diagnostics;

namespace AdvancedBot.client.Commands
{
    public class CommandGive : CommandBase
    {
        public CommandGive(MinecraftClient cli)
            : base(cli, "Give", "Coloca o item especificado no inventário (se o player estiver no modo criativo).", "give")
        {
            SetParams("<id>", "[metadata]", "[quantidade]", "[slot (hotbar: 36-44)]");
        }

        public override CommandResult Run(string alias, string[] args)
        {
            if (Client.Gamemode != 1) {
                Client.PrintToChat("§cVocê só pode usar esse comando no modo criativo.");
                return CommandResult.ErrorSilent;
            }
            if (args.Length < 1) return CommandResult.MissingArgs;

            Item item = null;

            int numId;
            if (int.TryParse(args[0], out numId)) {
                if (numId < 256) {
                    Block block = Blocks.GetBlock(numId);
                    item = new Item(block.ID, block.StackSize, block.Name, block.DisplayName);
                } else {
                    item = Items.GetItemInfo(numId);
                }
            } else {
                Block block = Blocks.GetBlockByName(args[0]);
                if (block != null) {
                    item = new Item(block.ID, block.StackSize, block.Name, block.DisplayName);
                } else {
                    item = Items.GetItemByName(args[0]);
                }
            }

            if (item == null) {
                Client.PrintToChat("§aItem não encontrado.");
                return CommandResult.ErrorSilent;
            }
            //"<id>", "[metadata]", "[quantidade]", "[slot]"
            int data  = args.Length > 1 ? int.Parse(args[1]) : 0;
            int count = args.Length > 2 ? int.Parse(args[2]) : 0;
            int slot  = args.Length > 3 ? int.Parse(args[3]) : 36;

            if (slot < 0 || slot > 45) {
                Client.PrintToChat("§cO slot prescisa ser entre 0 e 44.");
                return CommandResult.ErrorSilent;
            }
            count = Utils.Clamp(count, 1, item.StackSize);
            
            Client.SendPacket(new PacketCreativeInvAction((short)slot, new ItemStack((short)item.ID, (short)data, (byte)count)));
            Client.PrintToChat(string.Format("§aColocado o item '{0}' no slot {1}.", item.Name, slot));
            return CommandResult.Success;
        }
    }
}
