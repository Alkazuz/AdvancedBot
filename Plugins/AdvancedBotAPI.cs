using AdvancedBot.client;
using AdvancedBot.client.NBT;
using System.Collections.Generic;

namespace AdvancedBot.Plugins
{

    public class AdvancedBotAPI
    {
        public static List<MinecraftClient> getAllBots()
        {
            return Program.FrmMain.Clients;
        }

        public static Main GetMain()
        {
            return Program.FrmMain;
        }

        public static CompoundTag GetConfig()
        {
            return Program.Config;
        }

        public static PluginManager GetPluginManager()
        {
            return Program.pluginManager;
        }
    }
}
