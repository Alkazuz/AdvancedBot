using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdvancedBot.client.Packets;
using System.Diagnostics;

namespace AdvancedBot.client.Commands
{
    public class CommandUseBow : CommandBase
    {
        private MPPlayer target;
        private int bowTicks = -1;
        private LookInterpolator lint;

        public CommandUseBow(MinecraftClient cli)
            : base(cli, "UseBow", "Usa um arco em um player.", "usebow")
        {
            SetParams("<nick ou * para o mais proximo>", "[1=continua mesmo se o bot nao tiver flechas no inv.]");
        }
        public override CommandResult Run(string alias, string[] args)
        {
            if (args.Length < 1) return CommandResult.MissingArgs;

            if (args[0] == "*") {
                double bestDist = double.MaxValue;
                foreach (MPPlayer p in Client.PlayerManager.Players.Values) {
                    double pDist = Utils.DistTo(Player.PosX, Player.AABB.MinY, Player.PosZ, p.X, p.Y, p.Z);
                    if ((target == null || pDist < bestDist) && Player.CanSeePlayer(p)) {
                        bestDist = pDist;
                        target = p;
                    }
                }
                if (target != null) Client.PrintToChat(string.Format("§aO alvo será o player '{0}'.", Client.PlayerManager.GetNick(target)));
            } else {
                target = Client.PlayerManager.GetPlayerByNick(args[0]);
            }
            if (target == null) {
                Client.PrintToChat("§cNão foi possível encontrar esse player");
                return CommandResult.ErrorSilent;
            }
            int bowSlot = Client.SlotOfHotbarItem(Items.bow);
            if (bowSlot == -1) {
                Client.PrintToChat("§cO bot não tem nenhum arco na hotbar.");
                return CommandResult.ErrorSilent;
            } else {
                Client.HotbarSlot = bowSlot;
            }
            if (Client.SlotOfItem(Items.arrow, true) == -1 && (args.Length < 2 || args[1][0] != '1')) {
                Client.PrintToChat("§cO bot não tem nenhuma flecha no inventário.");
                return CommandResult.ErrorSilent;
            }

            bowTicks = 0;
            return CommandResult.Success;
        }
        public override void Tick()
        {
            if (target != null && bowTicks >= 0) {
                if (bowTicks++ == 0) {
                    Client.LeftClickItem();
                    
                    CalculateTrajectory(out float yaw, out float pitch);
                    lint = new LookInterpolator(Player, yaw, pitch, 0.75f);
                } else if (bowTicks >= 32) {
                    Client.SendPacket(new PacketPlayerDigging(DiggingStatus.FinishUse));
                    Client.PrintToChat("§aFlecha disparada.");

                    target = null;
                    bowTicks = -1;
                    lint = null;
                } else if (lint != null && lint.Finished) {
                    CalculateTrajectory(out Player.Yaw, out Player.Pitch);
                } else if (lint != null) {
                    lint.InterpolateToPlayer();
                }
            }
        }
        private void CalculateTrajectory(out float yaw, out float pitch)
        {
            const int MAX_DUR = 30;
            double vel = MAX_DUR / 20.0;
            vel = (vel * vel + vel * 2.0) / 3.0;

            vel = Utils.Clamp(vel, 0.1, 1.0);

            double dx = target.X - Player.PosX;
            double dy = target.Y + 1.62 - Player.PosY;
            double dz = target.Z - Player.PosZ;

            double dist = Math.Sqrt(dx * dx + dz * dz);
            yaw = (float)(Math.Atan2(dz, dx) * 180.0 / Math.PI) - 90.0f;

            double g = 0.006;

            double vel2 = vel * vel;
            double dist2 = dist * dist;
            double traj = Math.Atan((vel2 - Math.Sqrt(Math.Pow(vel, 4) - (g * (g * dist2 + 2.0 * dy * vel2)))) / (g * dist));
            
            if (!double.IsNaN(traj)) {
                pitch = (float)-(traj * 180.0 / Math.PI);
            } else {
                Debug.WriteLine("traj == NaN; This should never happen.");
                pitch = 0.0f;
            }
        }
    }
}
