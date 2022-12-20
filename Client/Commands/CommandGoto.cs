using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedBot.client.Commands
{
    public class CommandGoto : CommandBase
    {
        public CommandGoto(MinecraftClient cli)
            : base(cli, "Goto", "Tenta ir até o bloco especificado pelas coordenadas.", "goto")
        {
            SetParams("<x>", "<y>", "<z>");
        }

        public override CommandResult Run(string alias, string[] args)
        {
            if (args.Length < 3) return CommandResult.MissingArgs;

            int x = int.Parse(args[0]);
            int y = int.Parse(args[1]);
            int z = int.Parse(args[2]);

            Client.SetPath(x, y, z, string.Format("§aNão foi possivel calcular o path até o bloco {0} {1} {2}.", x, y, z));
            return CommandResult.Success;
        }
    }
}
