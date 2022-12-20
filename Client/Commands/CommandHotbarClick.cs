using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedBot.client.Commands
{
    public class CommandHotbarClick : CommandBase
    {
        public CommandHotbarClick(MinecraftClient cli)
            : base(cli, "HotbarClick", "Clica no item da hotbar no slot especificado com o botão direito.", "hotbarclick")
        {
            SetParams("<slot>");
        }
        public override CommandResult Run(string alias, string[] args)
        {
            if (args.Length < 1) return CommandResult.MissingArgs;

            int slot = int.Parse(args[0]);
            if (slot >= 1 && slot <= 9) {
                Client.HotbarSlot = slot - 1;

                Client.LeftClickItem();

                return CommandResult.Success;
            } else {
                Client.PrintToChat("§cA posição do slot precisa ser entre 1 e 9.");
                return CommandResult.ErrorSilent;
            }
        }
    }
}
