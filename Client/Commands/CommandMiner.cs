using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedBot.client.Commands
{
    public class CommandMiner : CommandBase
    {
        private AutoMiner miner = null;
        public CommandMiner(MinecraftClient cli)
            : base(cli, "Miner", "Minera automaticamente. Configurações: Opções->Minerador...", "miner")
        {
            ToggleText = "§6Minerador {0}";
        }
        public override CommandResult Run(string alias, string[] args)
        {
            Toggle(args);
            if (miner == null) {
                miner = new AutoMiner(Client);
            }

            if (IsToggled) {
                miner.StartMining();
            } else {
                miner.StopMining();
            }
            return CommandResult.Success;
        }
        public override void Tick()
        {
            miner?.Tick();
        }
    }
}
