using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdvancedBot.client.Packets;

namespace AdvancedBot.client.Commands
{
    public class CommandUseEntity : CommandBase
    {
        public CommandUseEntity(MinecraftClient cli)
            : base(cli, "UseEntity", "Ataca ou clica o player especificado.", "useentity")
        {
            SetParams("<nick ou @any>", "[modo (atacar = 0, clicar = 1)]");
        }
        public override CommandResult Run(string alias, string[] args)
        {
            Entity me = Client.Player;
            bool attack = (args.Length > 1 && args[1][0] == '1') ? false : true;

            if (args.Length < 1 || args[0] == "@any") {
                double bestDist = double.MaxValue;
                MPPlayer bestPlayer = null;
                foreach (MPPlayer p in Client.PlayerManager.Players.Values) {
                    double pDist = Utils.DistTo(me.PosX, me.AABB.MinY, me.PosZ, p.X, p.Y, p.Z);
                    if (pDist <= 4.0 && pDist < bestDist && !IsBot(Client.PlayerManager.GetNick(p)) && me.CanSeePlayer(p)) {
                        bestDist = pDist;
                        bestPlayer = p;
                    }
                }
                if (bestPlayer != null) {
                    me.LookTo(bestPlayer.X, bestPlayer.Y + 1.62, bestPlayer.Z);
                    Client.SendPacket(new PacketSwingArm(Client.PlayerID)); //send swing
                    Client.SendPacket(new PacketUseEntity(bestPlayer.EntityID, attack));
                }
                return CommandResult.Success;
            }

            MPPlayer player = Client.PlayerManager.GetPlayerByNick(args[0]);
            if (player == null) {
                Client.PrintToChat("§cPlayer não encontrado");
                return CommandResult.ErrorSilent;
            }
            if (!me.CanSeePlayer(player) || Utils.DistToSq(me.PosX, me.AABB.MinY, me.PosZ, player.X, player.Y, player.Z) > 6 * 6) {
                Client.PrintToChat("§cO player está muito longe ou existe algum bloco entre o bot e ele.");
                return CommandResult.ErrorSilent;
            }

            me.LookTo(player.X, player.Y + 1.62, player.Z);

            Client.SendPacket(new PacketSwingArm(Client.PlayerID)); //send swing
            Client.SendPacket(new PacketUseEntity(player.EntityID, attack));

            return CommandResult.Success;
        }

        private bool IsBot(string nick)
        {
            List<MinecraftClient> clients = Program.FrmMain.Clients;
            for (int i = 0; i < clients.Count; i++) {
                MinecraftClient cli = clients[i];
                if (cli.IsBeingTicked() && cli.Username != null && cli.Username.Equals(nick)) return true;
            }
            return false;
        }
    }
}
