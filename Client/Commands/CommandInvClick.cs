using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedBot.client.Commands
{
    public class CommandInvClick : CommandBase
    {
        public CommandInvClick(MinecraftClient cli)
            : base(cli, "InvClick", "Clica em um item do inventário aberto no slot especificado.", "invclick")
        {
            SetParams("<slot>", "[botão (0=esquerdo, 1=direito)]");
        }
        public override CommandResult Run(string alias, string[] args)
        {
            if (args.Length < 1) return CommandResult.MissingArgs;

            bool left = args.Length > 1 && args[1][0] != '1';

            Inventory inv = Client.OpenWindow;
            if (inv == null) {
                Client.PrintToChat("§cNenhum inventário aberto...");
                return CommandResult.ErrorSilent;
            }
            int islot = int.Parse(args[0]);
            if (islot >= 1 && islot <= inv.NumSlots) {
                inv.Click(Client, (short)(islot - 1), false, left);
                return CommandResult.Success;
            } else {
                Client.PrintToChat("§cA posição do slot precisa ser entre 1 e " + inv.NumSlots + ".");
                return CommandResult.ErrorSilent;
            }
        }
    }
}
