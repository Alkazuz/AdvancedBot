using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedBot.client.Commands
{
    public class CommandProxy : CommandBase
    {
        public CommandProxy(MinecraftClient cli)
            : base(cli, "GetProxy", "Mostra a proxy que o bot está usando.", "proxy", "getproxy")
        {
        }
        public override CommandResult Run(string alias, string[] args)
        {
            if (Client.ConProxy == null) {
                Client.PrintToChat("§aO bot não está usando nenhuma proxy!");
            } else {
                Proxy p = Client.ConProxy;
                Client.PrintToChat(string.Format("§aProxy do bot: {0}:{1} tipo: {2}", p.IP, p.Port, p.Type));
            }
            return CommandResult.Success;
        }
    }
}
