using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdvancedBot.client.Packets;

namespace AdvancedBot.client.Commands
{
    public class CommandTwerk : CommandBase
    {
        private bool sneaking = false;
        private int ticks = 0;
        public CommandTwerk(MinecraftClient cli)
            : base(cli, "Twerk", "", "twerk")
        {
            ToggleText = "§6Twerk {0}";
        }
        public override CommandResult Run(string alias, string[] args)
        {
            Toggle(args);
            if (!IsToggled && sneaking) {
                sneaking = false;
                Client.SendPacket(new PacketEntityAction(Client.PlayerID, 1, 0));
            }
            ticks = 0;
            return CommandResult.Success;
        }
        public override void Tick()
        {
            if (IsToggled && ticks++ >= 3) {
                sneaking = !sneaking;
                Client.SendPacket(new PacketEntityAction(Client.PlayerID, (byte)(sneaking ? 0 : 1), 0));
            }
        }
    }
}
