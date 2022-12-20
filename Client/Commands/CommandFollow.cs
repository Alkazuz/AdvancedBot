using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using AdvancedBot.client.PathFinding;

namespace AdvancedBot.client.Commands
{
    public class CommandFollow : CommandBase
    {
        private MPPlayer following;
        Vec3d lastFollowPos = new Vec3d(0, -555, 0);
        public CommandFollow(MinecraftClient cli)
            : base(cli, "Follow", "Segue um player. (para parar, use o comando sem argumentos)", "follow")
        {
            SetParams("<nick>");
        }
        public override CommandResult Run(string alias, string[] args)
        {
            if (following == null && args.Length == 0)
                return CommandResult.MissingArgs;

            if (args.Length == 0) {
                Client.PrintToChat("§aParando de seguir o player '" + Client.PlayerManager.GetNick(following) + "'");
                Client.CurrentPath = null;
                following = null;
                return CommandResult.Success;
            }
            lastFollowPos = new Vec3d(0, -555, 0);
            following = Client.PlayerManager.GetPlayerByNick(args[0]);
            if (following == null) {
                Client.PrintToChat("§cNão foi possível encontrar esse player");
                return CommandResult.ErrorSilent;
            } else {
                Client.CmdManager.GetCommand("retard").IsToggled = false;
                Client.PrintToChat("§aSeguindo o player '" + Client.PlayerManager.GetNick(following) + "'");
                return CommandResult.Success;
            }
        }
        public override void Tick()
        {
            if (following != null) {
                Entity p = Player;
                double dist = Utils.DistTo(p.PosX, p.AABB.MinY, p.PosZ, following.X, following.Y, following.Z);
                bool exists = Client.PlayerManager.PlayerExists(following.EntityID);
                if (!exists || dist > 80) {
                    Client.PrintToChat("§cO player está muito distante");
                    Client.CurrentPath = null;
                    following = null;
                    return;
                }

                p.LookTo(following.X, following.Y + 1.62, following.Z);
                double lastDist = Utils.DistTo(following.X, following.Y, following.Z,
                                               lastFollowPos.X, lastFollowPos.Y, lastFollowPos.Z);
                if (lastDist >= 2.5) {
                    Client.SetPath(Utils.Floor(following.X), Utils.Floor(following.Y), Utils.Floor(following.Z));
                    lastFollowPos = new Vec3d(following.X, following.Y, following.Z);
                }
            }
        }
    }
}
