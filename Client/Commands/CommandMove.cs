using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedBot.client.Commands
{
    public class CommandMove : CommandBase
    {
        public CommandMove(MinecraftClient cli)
            : base(cli, "Move", "Faz o bot se mover por um tempo determinado. (As direções podem ser combinadas com o caractere '|')", "move")
        {
            SetParams("<direções jump,forward,back,left,right>", "[duração em ticks]");
        }
        public override CommandResult Run(string alias, string[] args)
        {
            if (args.Length < 1) return CommandResult.MissingArgs;

            int dur;
            if (args.Length >= 2 && int.TryParse(args[1], out dur)) {
                if (dur <= 0 || dur > 100) {
                    Client.PrintToChat("§cA duração prescisa ser entre 1 e 100.");
                    return CommandResult.ErrorSilent;
                }
            } else {
                dur = 1;
            }
            Movement mov = Movement.None;
            foreach (string flag in args[0].Split('|', ' ')) {
                switch ((char)(flag[0] | 0x20)) {
                    case 'j': mov |= Movement.Jump; break;
                    case 'f': mov |= Movement.Forward; break;
                    case 'b': mov |= Movement.Back; break;
                    case 'l': mov |= Movement.Left; break;
                    case 'r': mov |= Movement.Right; break;
                }
            }
            for (int i = 0; i < dur; i++) {
                Player.MoveQueue.Enqueue(mov);
            }

            Client.PrintToChat(string.Format("§aMovendo nas direções: {0} por {1:0.0} segundo(s).", mov.ToString(), dur / 20.0));
            return CommandResult.Success;
        }
    }
}
