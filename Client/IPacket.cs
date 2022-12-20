using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedBot.client
{
    public interface IPacket
    {
        void WritePacket(WriteBuffer s, MinecraftClient client);
    }
}
