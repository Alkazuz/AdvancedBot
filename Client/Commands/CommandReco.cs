using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedBot.client.Commands
{
    public class CommandReco : CommandBase
    {
        public CommandReco(MinecraftClient cli)
            : base(cli, "Reconnect", "Tenta reconectar o bot no servidor.", "reco", "reconnect")
        {
            SetParams("[IP:porta]");
        }
        private static Dictionary<string, long> SrvCache = new Dictionary<string, long>();

        public override CommandResult Run(string alias, string[] args)
        {
            if (args.Length > 0) {
                string ip;
                ushort port = 25565;
                if (!args[0].ParseIP(out ip, ref port)) {
                    Client.PrintToChat("§cO endereço especificado está em um formato inválido.");
                    return CommandResult.ErrorSilent;
                }
                string ipPort = ip + ":" + port;
                long queryTime = 0, now = Utils.GetTimestamp();

                SrvCache.RemoveAll(a => now - a > 30000);
                if (!SrvCache.TryGetValue(ipPort, out queryTime)) {
                    SrvResolver.ResolveIP(ref ip, ref port);
                    SrvCache[ipPort] = now;
                }
                Client.IP = ip;
                Client.Port = port;
            }
            Client.StartClient();
            return CommandResult.Success; 
        }
    }
}
