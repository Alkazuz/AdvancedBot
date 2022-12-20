using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedBot.client.Commands
{
    public class CommandHelp : CommandBase
    {
        public CommandHelp(MinecraftClient cli) :
            base(cli, "Ajuda", "Mostra os comandos disponíveis.", "help", "?")
        {
            SetParams("[nome do comando]");
        }
        public override CommandResult Run(string alias, string[] args)
        {
            IEnumerable<CommandBase> _enum;
            if (args.Length > 0) {
                string q = args[0];
                _enum = Client.CmdManager.Commands.Where(a => a.DisplayName.ContainsIgnoreCase(q) ||
                                                               a.Aliases.Any(b => b.ContainsIgnoreCase(q)));           
            } else {
                _enum = Client.CmdManager.Commands;
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(args.Length > 0 ? "§fComandos encontrados: " : "§fComandos registrados: ");
            foreach (CommandBase cmd in _enum.OrderBy(a => a.DisplayName)) {
                sb.AppendFormat(" - §f{0}, {1}: §6${2} {3}§r\n", cmd.DisplayName, cmd.Description, cmd.Aliases[0], string.Join(" ", cmd.Parameters));
            }
            Client.PrintToChat(sb.ToString());
            return CommandResult.Success;
        }
    }
}
