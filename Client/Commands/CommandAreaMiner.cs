using AdvancedBot.client.PathFinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedBot.client.Commands
{
    public class CommandAreaMiner : CommandBase
    {
        public AreaMiner Miner { get; private set; } = null;
        private bool waitingPath = false;

        public CommandAreaMiner(MinecraftClient cli)
            : base(cli, "AreaMiner", "Minera automaticamente uma àrea especifica.", "areaminer")
        {
            SetParams("<x1>", "<y1>", "<z1>", "<x2>", "<y2>", "<z2>");
            ToggleText = "§6Minerador {0}";
        }
        public override CommandResult Run(string alias, string[] args)
        {
            if (!IsToggled && args.Length < 6) return CommandResult.MissingArgs;

            Toggle(args.Length > 6 ? new string[] { args[6] } : new string[0]);

            if (IsToggled) {
                int x1 = int.Parse(args[0]);
                int y1 = int.Parse(args[1]);
                int z1 = int.Parse(args[2]);
                int x2 = int.Parse(args[3]);
                int y2 = int.Parse(args[4]);
                int z2 = int.Parse(args[5]);

                if (x1 > x2) Swap(ref x1, ref x2);
                if (y1 > y2) Swap(ref y1, ref y2);
                if (z1 > z2) Swap(ref z1, ref z2);

                if (Miner == null) {
                    Miner = new AreaMiner(Client);
                }
                int px = Player.BlockX;
                int py = Player.BlockY;
                int pz = Player.BlockZ;
                if (!(px > x1 && py > y1 && pz > z1 && px < x2 && py < y2 && pz < y2)) {
                    int cx = x2, cy = y2, cz = z2;
                    double distMin = Utils.DistToSq(px, py, pz, x1, y1, z1);
                    double distMax = Utils.DistToSq(px, py, pz, x2, y2, z2);

                    if (distMin < distMax) {
                        cx = x1; cy = y1; cz = z1;
                    }
                    PathGuide.CreateAsync(Player, cx, cy, cz).ContinueWith((task) => {
                        PathGuide path = task.Result;
                        if (path == null) {
                            Miner.StartMining();
                            return;
                        }
                        Client.CurrentPath = path;
                        waitingPath = true;
                    });
                }

                Miner.Min = new Vec3i(x1, y1, z1);
                Miner.Max = new Vec3i(x2, y2, z2);
            } else {
                Miner.StopMining();
            }
            return CommandResult.Success;
        }

        private void Swap(ref int a, ref int b)
        {
            int tmp = b;
            b = a;
            a = tmp;
        }
        public override void Tick()
        {
            if(waitingPath && (Client.CurrentPath == null || Client.CurrentPath.Finished())) {
                Miner.StartMining();
                waitingPath = false;
            }
            Miner?.Tick();
        }
    }
}
