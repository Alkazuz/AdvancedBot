using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedBot.client.Bypassing
{
    using BypassRegistryEntry = Tuple<string, ClientVersion, Func<MinecraftClient, ServerBypassBase>>;
    public abstract class ServerBypassBase
    {
        public MinecraftClient Client { get; private set; }
        
        public abstract ClientVersion Version { get; }
        public bool IsFinished { get; protected set; } = false;

        public ServerBypassBase(MinecraftClient cli)
        {
            Client = cli;
        }

        public virtual bool HandlePacket(ReadBuffer rb)
        {
            return false;
        }
        
        private static List<BypassRegistryEntry> registry = new List<BypassRegistryEntry>();
        public static void RegisterBypass(string ip, ClientVersion v, Type type)
        {
            //https://stackoverflow.com/a/6882881

            var parTypes = new[] { typeof(MinecraftClient) };
            var parsExpr = parTypes.Select(Expression.Parameter).ToArray();

            var newExpr = Expression.New(type.GetConstructor(parTypes), parsExpr);
            var newLambda = Expression.Lambda<Func<MinecraftClient, ServerBypassBase>>(newExpr, parsExpr).Compile();

            registry.Add(new BypassRegistryEntry(ip, v, newLambda));
        }
        public static ServerBypassBase NewInstance(MinecraftClient cli)
        {
            var tuple = registry.FirstOrDefault(a => a.Item2 == cli.Version && a.Item1.EqualsIgnoreCase(cli.RealIP));
            return tuple?.Item3(cli) ?? null;
        }

        static ServerBypassBase() {
            RegisterBypass("adrserver.com.br", ClientVersion.v1_8, typeof(ADRBypass));
            RegisterBypass("jogar.mcnextup.com", ClientVersion.v1_8, typeof(ColorBypass));
            RegisterBypass("imperamc.batt.host", ClientVersion.v1_8, typeof(ColorBypass));
            RegisterBypass("mc.playdreamcraft.com.br", ClientVersion.v1_8, typeof(DreamcraftBypass));
            RegisterBypass("jogar.omegamc.net", ClientVersion.v1_8, typeof(OmegaMCBypass));
            RegisterBypass("skyminigames.com", ClientVersion.v1_8, typeof(ADRBypass));
            RegisterBypass("jogar.rede-union.com", ClientVersion.v1_8, typeof(TintaBypass));
            // RegisterBypass("redebiomes.minesv.net", ClientVersion.v1_8, typeof(ColorBypass));
            /*
[
  {
    "typename": "click_color",
    "ip": "***IP ADDRESS***",
    "protocol": 47,
  }
]
*/
            try {
                var reg = JArray.Parse(File.ReadAllText("bypass_registry.json"));
                foreach (var entry in reg) {
                    Type type = null;
                    switch(entry["typename"].AsStr()) {
                        case "click_color": type = typeof(ColorBypass); break;
                        case "tinta2": type = typeof(TintaBypass); break;
                        case "adr": type = typeof(ADRBypass); break;
                        case "worldcraft": type = typeof(WorldCraftBP); break;
                        case "dreamcraft": type = typeof(DreamcraftBypass); break;
                        default: continue;
                    }
                    RegisterBypass(entry["ip"].AsStr(), (ClientVersion)entry["protocol"].AsIntOr(47), type);
                }
            } catch { }
        }
    }
}
