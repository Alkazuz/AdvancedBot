using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedBot.client.Commands
{
    public class CommandDropAll : CommandBase
    {
        private int slot = -1;
        private int ticks = 0;
        int[] excList = new int[0];

        public CommandDropAll(MinecraftClient cli)
            : base(cli, "DropAll", "Dropa todos os items do inventário do bot.", "dropall")
        {
            SetParams("[blacklist de slots (separada por virgulas)]");
        }
        public override CommandResult Run(string alias, string[] args)
        {
            excList = new int[0];
            if (args.Length > 0) {
                try {
                    excList = args[0].Split(',').Select(n => int.Parse(n)).ToArray();
                } catch {
                    Client.PrintToChat("§eIgnorando lista de exclusão: Erro de sintaxe.");
                }
            }
            ticks = 0;
            slot = 5;
            return CommandResult.Success;
        }
        public override void Tick()
        {
            if (slot != -1 && ticks++ % 3 == 0) {
                Inventory pinv = Client.Inventory;
                for (; slot < 45; slot++) {
                    if (pinv.Slots[slot] != null && !excList.Contains(slot)) {
                        pinv.DropItem(Client, slot);
                        slot++;
                        break;
                    }
                }
                if (slot == 45) {
                    slot = -1;
                }
            }
        }
    }
}
