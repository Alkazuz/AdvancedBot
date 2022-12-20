using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedBot.client.Commands
{
    public class CommandClearChat : CommandBase
    {
        public CommandClearChat(MinecraftClient cli)
            : base(cli, "ClearChat", "Limpa o chat do bot.", "clearchat")
        {
        }
        public override CommandResult Run(string alias, string[] args)
        {
            lock (Client.ChatMessages) {
                Client.ChatMessages.Clear();
                Client.ChatChanged = true;
            }
            return CommandResult.Success;
        }
    }
}
