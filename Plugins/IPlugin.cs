using AdvancedBot.client;
using System.Reflection;

namespace AdvancedBot.Plugins
{
    [Obfuscation(Exclude = true)]
    public interface IPlugin
    {
        void Unload();

        void Tick();

        void onClientConnect(MinecraftClient client);

        void OnReceivePacket(ReadBuffer pkt, MinecraftClient client);

        void OnSendPacket(IPacket packet, MinecraftClient client);

        void onReceiveChat(string chat, byte pos, MinecraftClient client);

        void onSendChat(string chat, MinecraftClient client);
    }
}
