using AdvancedBot.client.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedBot.client.Handler
{
    public class Handler_v1222 : Handler_v1221
    {
        public Handler_v1222(MinecraftClient mc) : base(mc) { }
        public override ClientVersion HandlerVersion { get { return ClientVersion.v1_12_2; } }

        public override bool HandlePacket(ReadBuffer pkt)
        {
            if(pkt.ID == 0x1F) { //keep alive
                Client.SendPacket(new PacketKeepAlive_v1222() {
                    Timestamp = pkt.ReadLong()
                });
                Client.ResetKeepAlive();
                return true;
            }
            return base.HandlePacket(pkt);
        }

        private class PacketKeepAlive_v1222 : IPacket
        {
            public long Timestamp;
            public void WritePacket(WriteBuffer s, MinecraftClient client)
            {
                s.WriteVarInt(0x0B);
                s.WriteLong(Timestamp);
            }
        }
    }
}
