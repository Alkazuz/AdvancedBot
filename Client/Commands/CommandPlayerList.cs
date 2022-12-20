using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedBot.client.Commands
{
    public class CommandPlayerList : CommandBase
    {
        public CommandPlayerList(MinecraftClient cli)
            : base(cli, "PlayerList", "Lista os players que estão no servidor (do TAB list).", "playerlist", "players")
        {
        }
        public override CommandResult Run(string alias, string[] args)
        {
            var vals = Client.PlayerManager.UUID2Nick.Values;
            Client.PrintToChat(string.Format("§aPlayers ({0}): §f{1}\n", vals.Count, string.Join(", ", vals)));
            return CommandResult.Success;
        }
    }
}
