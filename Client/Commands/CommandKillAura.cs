using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdvancedBot.client.Packets;
using System.Diagnostics;

namespace AdvancedBot.client.Commands
{
    public class CommandKillAura : CommandBase
    {
        private string query = null;
        private int lastAttack = 0;
        private int speed;

        public CommandKillAura(MinecraftClient cli)
            : base(cli, "KillAura", "Ataca o player mais próximo ou o especificado pelo argumento.", "killaura", "ka")
        {
            SetParams("[nick ou * para todos]", "[velocidade, padrão=7]");
            ToggleText = "§6KillAura {0}";
        }
        public override CommandResult Run(string alias, string[] args)
        {
            if (args.Length > 0 && IsToggled) {
                query = args[0];
                speed = 7;
                if (args.Length > 1) {
                    speed = int.Parse(args[1]);
                    if (speed < 1 || speed > 200) {
                        speed = Utils.Clamp(speed, 1, 200);
                        Client.PrintToChat("§eA velocidade foi limitada para " + speed);
                    }
                }
                Client.PrintToChat("§aConfigurações atualizadas.");
            } else {
                Toggle(new string[0]);
            }
            return CommandResult.Success;
        }
        private static Random PRNG = new Random();
        public override void Tick()
        {
            if (IsToggled) {
                Entity me = Client.Player;

                MPPlayer bestPlayer = null;
                if (lastAttack++ >= speed) {
                    double bestDist = double.MaxValue;
                    foreach (MPPlayer p in Client.PlayerManager.Players.Values) {
                        double pDist = Utils.DistTo(me.PosX, me.AABB.MinY, me.PosZ, p.X, p.Y, p.Z);
                        if (pDist <= 4.0 && (bestPlayer == null || pDist < bestDist) && Matches(p) && me.CanSeePlayer(p)) {
                            bestDist = pDist;
                            bestPlayer = p;
                        }
                    }
                    if (bestPlayer != null) {
                        me.LookTo(bestPlayer.X + RPosInc(), bestPlayer.Y + 1.62 + RPosInc(), bestPlayer.Z + RPosInc());

                        //Debug.WriteLine(Client.Username + " attacks " + Client.PlayerManager.GetNick(bestPlayer));
                        Client.SendPacket(new PacketSwingArm(Client.PlayerID)); //send swing
                        Client.SendPacket(new PacketUseEntity(bestPlayer.EntityID, true));
                    }
                    lastAttack = 0;
                }
            }
        }
        private bool Matches(MPPlayer player)
        {
            string nick = Client.PlayerManager.GetNick(player);

            return (query == null || query == "*") ? !IsBot(nick) : nick.EqualsIgnoreCase(query);
        }
        private bool IsBot(string nick)
        {
            foreach (var bot in Program.FrmMain.Clients) {
                if (bot.Username.EqualsIgnoreCase(nick)) {
                    return true;
                }
            }
            return false;
        }
        private static double RPosInc()
        {
            return (PRNG.NextDouble() - 0.5) * 0.2;
        }
    }
}
