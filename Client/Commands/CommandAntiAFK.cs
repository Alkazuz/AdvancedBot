using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedBot.client.Commands
{
    public class CommandAntiAFK  : CommandBase
    {
        private int delay = -1;
        private long lastJump = -1;
        public CommandAntiAFK(MinecraftClient cli)
            : base(cli, "AntiAFK", "Pula de tempo em tempo. O delay padrão é 5000ms.", "antiafk")
        {
            SetParams("[delay]");
            ToggleText = "§6AntiAFK {0}";
        }
        public override CommandResult Run(string alias, string[] args)
        {
            Toggle(new string[0]);
            if (IsToggled) {
                lastJump = Utils.GetTimestamp();
                if (args.Length > 0 && int.TryParse(args[0], out delay)) {
                    if (delay <= 0) {
                        Client.PrintToChat("§cO delay prescisa ser maior que 0ms");
                        return CommandResult.ErrorSilent;
                    }
                } else {
                    delay = 5000;
                }
            }
            return CommandResult.Success;
        }
        public override void Tick()
        {
            if (IsToggled) {
                long now = Utils.GetTimestamp();
                if (now - lastJump > delay) {
                    Player.MoveQueue.Enqueue(Movement.Jump);
                    lastJump = now;
                }
            }
        }
    }
}
