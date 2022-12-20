using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdvancedBot.client.Map;
using AdvancedBot.client.Packets;
using System.Threading;
using System.Diagnostics;

namespace AdvancedBot.client.Handler
{
    //http://wiki.vg/index.php?title=Pre-release_protocol&oldid=7268
    //TODO fix IPacket classes 
    public class Handler_v19 : Handler_v18
    {
        public Handler_v19(MinecraftClient mc) : base(mc) { }
        public override ClientVersion HandlerVersion { get { return ClientVersion.v1_9; } }

        public override bool HandlePacket(ReadBuffer pkt)
        {
            switch (pkt.ID) {
                case 0x1F: pkt.ID = 0x00; base.HandlePacket(pkt); break; //Keep-alive
                case 0x23: pkt.ID = 0x01; base.HandlePacket(pkt); break; //Join game
                case 0x0F: pkt.ID = 0x02; base.HandlePacket(pkt); break; //Chat message
                case 0x3E: pkt.ID = 0x06; base.HandlePacket(pkt); break; //Update health
                case 0x33: pkt.ID = 0x07; base.HandlePacket(pkt); break; //Respawn
                case 0x37: Client.SetHotbarSlot(pkt.ReadByte()); break;  //Held item change
                case 0x3B: pkt.ID = 0x12; base.HandlePacket(pkt); break; //Entity velocity
                case 0x30: pkt.ID = 0x13; base.HandlePacket(pkt); break; //Destroy entities
                case 0x4C: pkt.ID = 0x1D; base.HandlePacket(pkt); break; //Effect
                case 0x31: pkt.ID = 0x1E; base.HandlePacket(pkt); break; //Remove effect
                case 0x10: pkt.ID = 0x22; base.HandlePacket(pkt); break; //Multi block change
                case 0x0B: pkt.ID = 0x23; base.HandlePacket(pkt); break; //Block change
                case 0x1C: pkt.ID = 0x27; base.HandlePacket(pkt); break; //Explosion
                case 0x1E: pkt.ID = 0x2B; base.HandlePacket(pkt); break; //Change game state
                case 0x13: pkt.ID = 0x2D; base.HandlePacket(pkt); break; //Open window
                case 0x12: pkt.ID = 0x2E; base.HandlePacket(pkt); break; //Close window
                case 0x16: pkt.ID = 0x2F; base.HandlePacket(pkt); break; //Set slot
                case 0x14: pkt.ID = 0x30; base.HandlePacket(pkt); break; //Window items
                case 0x11: pkt.ID = 0x32; base.HandlePacket(pkt); break; //Confirm transaction
                case 0x46: pkt.ID = 0x33; base.HandlePacket(pkt); break; //Update sign
                case 0x2D: pkt.ID = 0x38; base.HandlePacket(pkt); break; //Player list
                case 0x45: pkt.ID = 0x45; base.HandlePacket(pkt); break; //Title
                ///////////Changed packets////////////
                case 0x05: { //spawn player
                        int entityID = pkt.ReadVarInt();
                        UUID uuid = new UUID(pkt.ReadLong(), pkt.ReadLong());
                        double x = pkt.ReadDouble();
                        double y = pkt.ReadDouble();
                        double z = pkt.ReadDouble();
                        float yaw = (float)(pkt.ReadSByte() * 360) / 256f;
                        float pitch = (float)(pkt.ReadSByte() * 360) / 256f;

                        MPPlayer player = new MPPlayer(entityID, uuid);
                        player.SetPos(x, y, z);
                        player.SetRotation(yaw, pitch);

                        Client.PlayerManager.Players[entityID] = player;
                    }
                    break;
                case 0x25: { //entity relative move
                        int entityId = pkt.ReadVarInt();
                        double dx = pkt.ReadShort() / 4096.0;
                        double dy = pkt.ReadShort() / 4096.0;
                        double dz = pkt.ReadShort() / 4096.0;
                        if (Client.PlayerManager.Players.TryGetValue(entityId, out MPPlayer player)) {
                            player.X += dx;
                            player.Y += dy;
                            player.Z += dz;
                        }
                    }
                    break;
                case 0x27: { //entity look
                        int entityId = pkt.ReadVarInt();
                        float yaw = (pkt.ReadSByte() * 360) / 256f;
                        float pitch = (pkt.ReadSByte() * 360) / 256f;
                        if (Client.PlayerManager.Players.TryGetValue(entityId, out MPPlayer player))
                            player.SetRotation(yaw, pitch);
                    }
                    break;
                case 0x26: {// entity look and relative move
                        int entityId = pkt.ReadVarInt();

                        double dx = pkt.ReadShort() / 4096.0;
                        double dy = pkt.ReadShort() / 4096.0;
                        double dz = pkt.ReadShort() / 4096.0;

                        float yaw = (pkt.ReadSByte() * 360) / 256f;
                        float pitch = (pkt.ReadSByte() * 360) / 256f;
                        if (Client.PlayerManager.Players.TryGetValue(entityId, out MPPlayer player)) {
                            player.X += dx;
                            player.Y += dy;
                            player.Z += dz;
                            player.SetRotation(yaw, pitch);
                        }
                    }
                    break;
                case 0x4A: { //entity teleport
                        int entityId = pkt.ReadVarInt();

                        double x = pkt.ReadDouble();
                        double y = pkt.ReadDouble();
                        double z = pkt.ReadDouble();
                        float yaw = (pkt.ReadSByte() * 360) / 256f;
                        float pitch = (pkt.ReadSByte() * 360) / 256f;
                        if (Client.PlayerManager.Players.TryGetValue(entityId, out MPPlayer player)) {
                            player.SetPos(x, y, z);
                            player.SetRotation(yaw, pitch);
                        }
                    }
                    break;
                case 0x34: { //entity head look
                        int entityId = pkt.ReadVarInt();
                        float yaw = (pkt.ReadSByte() * 360) / 256f;
                        if (Client.PlayerManager.Players.TryGetValue(entityId, out MPPlayer player))
                            player.Yaw = yaw;
                    }
                    break;
                case 0x20: if (Client.MapAndPhysics) P20ReadChunkData(pkt); break;
                case 0x1D: { //Unload chunk
                        if (!Client.MapAndPhysics) break;

                        int chunkX = pkt.ReadInt();
                        int chunkZ = pkt.ReadInt();
                        World.SetChunk(chunkX, chunkZ, null);
                    }
                    break;
                case 0x2E: { //Position
                        double x = pkt.ReadDouble();
                        double y = pkt.ReadDouble();
                        double z = pkt.ReadDouble();
                        float yaw = pkt.ReadFloat();
                        float pitch = pkt.ReadFloat();
                        byte flags = pkt.ReadByte();
                        int teleportId = pkt.ReadVarInt();

                        const int FLAG_X = 0x01;
                        const int FLAG_Y = 0x02;
                        const int FLAG_Z = 0x04;
                        const int FLAG_Y_ROT = 0x08;
                        const int FLAG_X_ROT = 0x10;

                        if ((flags & FLAG_X) != 0) x += Player.PosX;
                        else Player.MotionX = 0.0;

                        if ((flags & FLAG_Y) != 0) y += Player.AABB.MinY;
                        else Player.MotionY = 0.0;

                        if ((flags & FLAG_Z) != 0) z += Player.PosZ;
                        else Player.MotionZ = 0.0;

                        if ((flags & FLAG_X_ROT) != 0) pitch += Player.Pitch;
                        if ((flags & FLAG_Y_ROT) != 0) yaw += Player.Yaw;

                        Player.SetPosition(x, y, z);
                        Player.Yaw = yaw % 360.0f;
                        Player.Pitch = pitch % 360.0f;

                        Client.SendPacket(new PacketTeleportConfirm(teleportId));
                    }
                    break;
                case 0x1A: //disconnect
                    string reason = ChatParser.ParseJson(pkt.ReadString());
                    Client.HandlePacketDisconnect(reason);
                    return false;
            }
            return true;
        }

        protected void P20ReadChunkData(ReadBuffer pkt)
        {
            int x = pkt.ReadInt();
            int z = pkt.ReadInt();
            bool full = pkt.ReadBoolean();
            int mask = pkt.ReadVarInt();

            if (Client.LimitChunks) {
                int dx = (x * 16 + 8) - Utils.Floor(Player.PosX);
                int dz = (z * 16 + 8) - Utils.Floor(Player.PosZ);
                if (dx * dx + dz * dz > MinecraftClient.CHUNK_LIMIT)
                    return;
            }

            Chunk chunk = full ? new Chunk(x, z) : World.GetChunk(x, z);
            if (chunk == null) {
                chunk = new Chunk(x, z);
            }

            int dataLen = pkt.ReadVarInt();
            DecodeChunk(chunk, full, Client.Dimension == 0, mask, pkt);
            //ReadChunkColumn(chunk, full, mask, new ReadBuffer(pkt.ReadByteArray(pkt.ReadVarInt()), false));
            World.SetChunk(x, z, chunk);

         /* int blockEntityCount = pkt.ReadVarInt();
            for (int i = 0; i < blockEntityCount; i++) {
                CompoundTag tag = pkt.ReadNBT();
            }*/
        }
        public static void DecodeChunk(Chunk chunk, bool full, bool overworld, int mask, ReadBuffer pkt)
        {
            for (int y = 0; y < 16; y++) {
                if ((mask & 1 << y) != 0) {
                    ChunkSection section = new ChunkSection();

                    int bits = pkt.ReadByte();
                    bool usePalette = bits <= 8;

                    int paletteLen = pkt.ReadVarInt();
                    int[] palette = new int[paletteLen];
                    for (int i = 0; i < paletteLen; i++)
                        palette[i] = pkt.ReadVarInt();

                    ulong maxEntryValue = (1ul << bits) - 1ul;
                    ulong[] data = new ulong[pkt.ReadVarInt()];
                    for (int i = 0, l = data.Length; i < l; i++)
                        data[i] = (ulong)pkt.ReadLong();

                    for (int i = 0; i < 4096; i++) {
                        int bitIndex = i * bits;
                        int startLong = bitIndex / 64;
                        int startOffset = bitIndex % 64;
                        int endLong = ((i + 1) * bits - 1) / 64;

                        int block;
                        if (startLong == endLong)
                            block = (int)(data[startLong] >> startOffset & maxEntryValue);
                        else {
                            int endOffset = 64 - startOffset;
                            block = (int)((data[startLong] >> startOffset | data[endLong] << endOffset) & maxEntryValue);
                        }
                        if (usePalette) {
                            block = palette[block];
                        }

                        int meta = block & 0xF;
                        section.Metadata[i / 2] |= (byte)(i % 2 == 0 ? meta : (meta << 4));
                        section.Blocks[i] = (byte)(block >> 4);
                    }

                    chunk.Sections[y] = section;
                    pkt.Skip(2048); //pkt.ReadByteArray(2048); //blocklight

                    if (overworld) {
                        pkt.Skip(2048); //pkt.ReadByteArray(2048); //skylight
                    }
                } else if (full && chunk.Sections[y] != null) {
                    chunk.Sections[y] = null;
                }
            }
        }

        public override bool WritePacket(ref int v18id, IPacket pkt, WriteBuffer buf)
        {
            switch (v18id) {
                case 0x00: v18id = 0x0B; return false; //Keep-alive
                case 0x01: v18id = 0x02; return false; //Chat message
                case 0x03: v18id = 0x0F; return false; //Player update
                case 0x04: v18id = 0x0C; return false; //Player pos
                case 0x05: v18id = 0x0E; return false; //Player look
                case 0x06: v18id = 0x0D; return false; //Player pos and look
                case 0x09: v18id = 0x17; return false; //Held item change
                case 0x0B: v18id = 0x14; return false; //Entity action - “Open inventory” is now sent via the Client Status packet.
                case 0x0D: v18id = 0x08; return false; //Close window
                case 0x0E: v18id = 0x07; return false; //Click window
                case 0x0F: v18id = 0x05; return false; //Confirm transaction
                case 0x10: v18id = 0x18; return false; //Creative inv action
                case 0x17: v18id = 0x09; return false; //Plugin msg
                case 0x02: {
                        PacketUseEntity p = (PacketUseEntity)pkt;
                        buf.WriteVarInt(0x0A);
                        buf.WriteVarInt(p.EntityID);
                        buf.WriteVarInt(p.MouseButton);
                        if (p.MouseButton == 2) {
                            /*buf.WriteFloat(p.TargetX);
                            buf.WriteFloat(p.TargetY);
                            buf.WriteFloat(p.TargetZ);*/
                        }
                        if (p.MouseButton == 0 || p.MouseButton == 2) {
                            buf.WriteVarInt(0);//hand
                        }
                    }
                    return true;
                case 0x07: {
                        PacketPlayerDigging p = (PacketPlayerDigging)pkt;
                        buf.WriteVarInt(0x13);
                        buf.WriteByte((byte)p.Status);
                        buf.WriteLocation(new Vec3i(p.X, p.Y, p.Z));
                        buf.WriteByte(p.Face);
                    }
                    return true;
                case 0x16: {
                        PacketClientStatus p = (PacketClientStatus)pkt;
                        buf.WriteVarInt(0x03);
                        buf.WriteVarInt(p.ActionID);
                    }
                    return true;
                case 0x15: {
                        PacketClientSettings p = (PacketClientSettings)pkt;
                        buf.WriteVarInt(0x04);
                        buf.WriteString(p.Locate);
                        buf.WriteByte(p.ViewDistance);
                        buf.WriteVarInt(p.ChatFlags);
                        buf.WriteBoolean(p.ChatColors);
                        buf.WriteByte(0x7F);//skin parts, all flags
                        buf.WriteVarInt(0);//hand
                    }
                    return true;
                case 0x0A: { //Animation->SwingArm
                        buf.WriteVarInt(0x1A);
                        buf.WriteVarInt(0);//hand
                    }
                    return true;
                case 0x08: {
                        PacketBlockPlace p = (PacketBlockPlace)pkt;
                        buf.WriteVarInt(0x1C);
                        buf.WriteLocation(new Vec3i(p.X, p.Y, p.Z));
                        buf.WriteVarInt(p.Direction);
                        buf.WriteVarInt(0);//hand
                        buf.WriteByte(p.CursorX);
                        buf.WriteByte(p.CursorY);
                        buf.WriteByte(p.CursorZ);
                    }
                    return true;
                default: return false;
            }
        }
    }
}
