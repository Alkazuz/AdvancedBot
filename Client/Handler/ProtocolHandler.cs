using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdvancedBot.client.Map;
using System.Reflection;

namespace AdvancedBot.client.Handler
{
    public abstract class ProtocolHandler
    {
        public MinecraftClient Client;
        
        public World World { get { return Client.World; } }
        public Entity Player { get { return Client.Player; } }

        public ProtocolHandler(MinecraftClient mc)
        {
            Client = mc;
        }

        public abstract ClientVersion HandlerVersion { get; }

        /// <summary>
        /// Returns false if the packet is PacketDisconnect
        /// </summary>
        public abstract bool HandlePacket(ReadBuffer pkt);

        /// <summary>
        /// NOTE: Only called on 1.9+
        /// Returns true if the packet was written to buf by this method.
        /// </summary>
        public abstract bool WritePacket(ref int v18id, IPacket pkt, WriteBuffer buf);

        public static ProtocolHandler Create(ClientVersion ver, MinecraftClient cli)
        {
            switch (ver) {
                case ClientVersion.v1_5_2:  return new Handler_v152(cli);
                case ClientVersion.v1_7:
                case ClientVersion.v1_7_10: return new Handler_v17(cli);
                case ClientVersion.v1_8:    return new Handler_v18(cli);
                case ClientVersion.v1_9:    return new Handler_v19(cli);
                case ClientVersion.v1_12_1: return new Handler_v1221(cli);
                case ClientVersion.v1_12_2: return new Handler_v1222(cli);
                default: return null;
            }
        }
    }
}
