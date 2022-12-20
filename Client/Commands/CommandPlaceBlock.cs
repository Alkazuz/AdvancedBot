using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdvancedBot.client.Map;

namespace AdvancedBot.client.Commands
{
    public class CommandPlaceBlock : CommandBase
    {
        public CommandPlaceBlock(MinecraftClient cli)
            : base(cli, "PlaceBlock", "Olha para a coordenada especificada e coloca o bloco que está selecionado na hotbar.", "placeblock")
        {
            SetParams("<x>", "<y>", "<z>", "[r=posição relativa]");
        }
        public override CommandResult Run(string alias, string[] args)
        {
            if (args.Length >= 3) {
                int x = int.Parse(args[0]);
                int y = int.Parse(args[1]);
                int z = int.Parse(args[2]);
                if (args.Length >= 4 && args[3] == "r") {
                    x += Utils.Floor(Player.PosX);
                    y += Utils.Floor(Player.AABB.MinY);
                    z += Utils.Floor(Player.PosZ);
                }
                Player.LookToBlock(x, y, z, true);
            } else {
                return CommandResult.MissingArgs;
            }
            if (Client.ItemInHand == null || Client.ItemInHand.ID >= 256) {
                Client.PrintToChat("§cO bot não tem nenhum bloco selecionado na hotbar!");
                return CommandResult.ErrorSilent;
            }
            HitResult hit = Player.RayCastBlocks(6);
            if (hit == null) {
                Client.PrintToChat("§cO bot não está olhando para nenhum bloco!");
                return CommandResult.ErrorSilent;
            }
            Client.PlaceCurrentBlock(hit);
            Client.PrintToChat(string.Format("§aBloco colocado em {0} {1} {2}.", hit.X, hit.Y, hit.Z));
            return CommandResult.Success;
        }
    }
}
