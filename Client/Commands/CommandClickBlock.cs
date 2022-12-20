using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdvancedBot.client.Packets;
using AdvancedBot.client.Map;

namespace AdvancedBot.client.Commands
{
    public class CommandClickBlock : CommandBase
    {
        public CommandClickBlock(MinecraftClient cli)
            : base(cli, "ClickBlock", "Clica em um bloco na coordenada e com o botão especificado.", "clickblock")
        {
            SetParams("<x>", "<y>", "<z>", "<botão (0=esquerdo, 1=direito)>");
        }
        public override CommandResult Run(string alias, string[] args)
        {
            if (args.Length < 3) return CommandResult.MissingArgs;

            int x = int.Parse(args[0]);
            int y = int.Parse(args[1]);
            int z = int.Parse(args[2]);
            bool leftClick = args[3] == "0";

            Entity p = Client.Player;
            if (Utils.DistTo(p.PosX, p.PosY, p.PosZ, x, y, z) > 5.5) {
                Client.PrintToChat("§cO bot está muito longe do bloco. Use $goto <x> <y> <z> para ir até lá.");
                return CommandResult.ErrorSilent;
            }
            p.LookToBlock(x, y, z, true);

            HitResult hit = Client.World.RayCast(new Vec3d(p.PosX, p.PosY, p.PosZ), new Vec3d(x + 0.5, y + 0.5, z + 0.5), false, true);
            int face = hit == null ? 1 : hit.Face;
            if (leftClick) {
                Client.SendPacket(new PacketPlayerDigging(DiggingStatus.StartedDigging, x, (byte)y, z, (byte)face));
                Client.SendPacket(new PacketPlayerDigging(DiggingStatus.CancelledDigging, x, (byte)y, z, (byte)face));
            } else {
                if (hit == null)
                    hit = new HitResult(x, y, z, face);
                Client.SendPacket(new PacketBlockPlace(hit, Client.ItemInHand));
            }
            return CommandResult.Success;
        }
    }
}
