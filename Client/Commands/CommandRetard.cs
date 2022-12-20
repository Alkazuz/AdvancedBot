using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace AdvancedBot.client.Commands
{
    public class CommandRetard : CommandBase
    {
        public CommandRetard(MinecraftClient cli)
            : base(cli, "Retard", "Faz o bot se mover aleatoriamente.", "retard")
        {
            ToggleText = "§6Retard {0}";
        }
        public override CommandResult Run(string alias, string[] args)
        {
            Toggle(args);
            return CommandResult.Success;
        }
        private static Random PRNG = new Random();
        public override void Tick()
        {
            if (IsToggled) {
                double time = Environment.TickCount / 300.0;
                Player.Yaw = (float)(Math.Cos(time) * 180.0);
                Player.Pitch = (float)(Math.Sin(time) * 90.0);

                switch (PRNG.Next(6)) {
                    case 0: Player.MoveQueue.Enqueue(Movement.Jump); break;
                    case 1: Player.MoveQueue.Enqueue(Movement.Forward); break;
                    case 2: Player.MoveQueue.Enqueue(Movement.Back); break;
                    case 3: Player.MoveQueue.Enqueue(Movement.Left); break;
                    case 4: Player.MoveQueue.Enqueue(Movement.Right); break;
                    case 5: /* do nothing */ break;
                }
            }
        }
    }
}
