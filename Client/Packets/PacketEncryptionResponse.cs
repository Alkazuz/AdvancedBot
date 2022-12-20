using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedBot.client.Packets
{
    public class PacketEncryptionResponse : IPacket
    {
        public byte[] SharedSecret;
        public byte[] VerifyToken;

        public PacketEncryptionResponse(byte[] sharedSecret, byte[] verifyToken)
        {
            SharedSecret = sharedSecret;
            VerifyToken = verifyToken;
        }

        public void WritePacket(WriteBuffer s, MinecraftClient client)
        {
            s.WriteVarInt(0x01);
            if (client.Version < ClientVersion.v1_8) {
                s.WriteShort((short)SharedSecret.Length);
                s.WriteByteArray(SharedSecret);
                s.WriteShort((short)VerifyToken.Length);
                s.WriteByteArray(VerifyToken);
            } else {
                s.WriteVarInt(SharedSecret.Length);
                s.WriteByteArray(SharedSecret);
                s.WriteVarInt(VerifyToken.Length);
                s.WriteByteArray(VerifyToken);
            }
        }
    }
}
