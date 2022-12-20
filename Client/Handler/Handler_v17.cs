using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdvancedBot.client.Map;
using Ionic.Zlib;
using AdvancedBot.client.Packets;
using System.Threading;
using AdvancedBot.client.NBT;
using System.Diagnostics;

namespace AdvancedBot.client.Handler
{
    public class Handler_v17  : ProtocolHandler
    {
        public Handler_v17(MinecraftClient mc) : base(mc) { }

        public override ClientVersion HandlerVersion { get { return ClientVersion.v1_7; } }

        public override bool HandlePacket(ReadBuffer pkt)
        {
            switch (pkt.ID) {
                case 0x00: //keep alive
                    Client.SendPacket(new PacketKeepAlive(pkt.ReadInt()));
                    Client.ResetKeepAlive();
                    break;
                case 0x01: {//join game
                        int playerId = pkt.ReadInt();
                        int gm = pkt.ReadByte(); //gamemode
                        sbyte dimension = pkt.ReadSByte();

                        Client.HandlePacketJoinGame(playerId, dimension, gm);
                    }
                    break;
                case 0x02: //chat
                    Client.HandlePacketChat(ChatParser.ParseJson(pkt.ReadString()), 0);
                    break;
                case 0x06: //update health
                    float health = pkt.ReadFloat();

                    if (health <= 0) {
                        Player.MotionX = Player.MotionY = Player.MotionZ = 0;
                        Thread.Sleep(300);
                        Client.SendPacket(new PacketClientStatus(0));
                    }
                    break;
                case 0x07: {//respawn
                        sbyte dim = (sbyte)pkt.ReadInt();
                        int difficulty = pkt.ReadByte();
                        int gm = pkt.ReadByte();
                        Client.HandlePacketRespawn(dim, gm);
                    }
                    break;
                case 0x08: { //pos and look
                        double x = pkt.ReadDouble();
                        double y = pkt.ReadDouble();
                        double z = pkt.ReadDouble();
                        Player.Yaw = pkt.ReadFloat();
                        Player.Pitch = pkt.ReadFloat();
                        Player.OnGround = pkt.ReadBoolean();
                        Player.SetPosition(x, y - 1.62, z);

                        Player.MotionX = 0.0;
                        Player.MotionY = 0.0;
                        Player.MotionZ = 0.0;

                        Client.SendPacket(new PacketPosAndLook(Player.PosX, Player.AABB.MinY, Player.PosY, Player.PosZ, Player.Yaw, Player.Pitch, false));
                    }
                    break;
                case 0x09: Client.SetHotbarSlot(pkt.ReadByte()); break;
                case 0x0C: { //spawn player 
                        int entityId = pkt.ReadVarInt();
                        string strUuid = pkt.ReadString();
                        string name = pkt.ReadString();
                        double x = pkt.ReadInt() / 32.0;
                        double y = pkt.ReadInt() / 32.0;
                        double z = pkt.ReadInt() / 32.0;
                        float yaw = (float)(pkt.ReadSByte() * 360) / 256f;
                        float pitch = (float)(pkt.ReadSByte() * 360) / 256f;
                        
                        UUID uuid = string.IsNullOrEmpty(strUuid) ? UUID.NameUUID("OfflinePlayer:" + name) : UUID.Parse(strUuid);
                        
                        MPPlayer player = new MPPlayer(entityId, uuid);
                        player.SetPos(x, y, z);
                        player.SetRotation(yaw, pitch);

                        Client.PlayerManager.Players[entityId] = player;
                        Client.PlayerManager.UUID2Nick[uuid] = name;
                    }
                    break;
                case 0x12: {//entity velocity
                        int entityId = pkt.ReadInt();
                        double velX = pkt.ReadShort() / 8000.0;
                        double velY = pkt.ReadShort() / 8000.0;
                        double velZ = pkt.ReadShort() / 8000.0;
                        if (entityId == Client.PlayerID && MinecraftClient.Knockback) {
                            Player.MotionX += velX;
                            Player.MotionY += velY;
                            Player.MotionZ += velZ;
                        }
                    }
                    break;
                case 0x13: { //destroy entities
                        int count = pkt.ReadByte();
                        for (int i = 0; i < count; i++)
                            Client.PlayerManager.Players.TryRemove(pkt.ReadInt(), out _);
                    }
                    break;
                case 0x15: { //entity relative move
                        int entityId = pkt.ReadInt();
                        double dx = pkt.ReadSByte() / 32.0;
                        double dy = pkt.ReadSByte() / 32.0;
                        double dz = pkt.ReadSByte() / 32.0;
                        if (Client.PlayerManager.Players.TryGetValue(entityId, out MPPlayer player)) {
                            player.X += dx;
                            player.Y += dy;
                            player.Z += dz;
                        }
                    }
                    break;
                case 0x16: { //entity look
                        int entityId = pkt.ReadInt();
                        float yaw = (pkt.ReadSByte() * 360) / 256f;
                        float pitch = (pkt.ReadSByte() * 360) / 256f;
                        if (Client.PlayerManager.Players.TryGetValue(entityId, out MPPlayer player))
                            player.SetRotation(yaw, pitch);
                    }
                    break;
                case 0x17: {// entity look and relative move
                        int entityId = pkt.ReadInt();

                        double dx = pkt.ReadSByte() / 32.0;
                        double dy = pkt.ReadSByte() / 32.0;
                        double dz = pkt.ReadSByte() / 32.0;

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
                case 0x18: { //entity teleport
                        int entityId = pkt.ReadInt();

                        double x = pkt.ReadInt() / 32.0;
                        double y = pkt.ReadInt() / 32.0;
                        double z = pkt.ReadInt() / 32.0;
                        float yaw = (pkt.ReadSByte() * 360) / 256f;
                        float pitch = (pkt.ReadSByte() * 360) / 256f;
                        if (Client.PlayerManager.Players.TryGetValue(entityId, out MPPlayer player)) {
                            player.SetPos(x, y, z);
                            player.SetRotation(yaw, pitch);
                        }
                    }
                    break;
                case 0x19: { //entity head look
                        int entityId = pkt.ReadInt();
                        float yaw = (pkt.ReadSByte() * 360) / 256f;
                        if (Client.PlayerManager.Players.TryGetValue(entityId, out MPPlayer player))
                            player.Yaw = yaw;
                    }
                    break;
                case 0x1D: {
                        int entityId = pkt.ReadInt();
                        byte effectId = pkt.ReadByte();
                        byte amp = pkt.ReadByte();
                        if (entityId == Client.PlayerID)
                            Player.ActivePotions[effectId] = amp;
                    }
                    break;
                case 0x1E: {
                        int entityId = pkt.ReadInt();
                        byte effectId = pkt.ReadByte();
                        if (entityId == Client.PlayerID)
                            Player.ActivePotions.Remove(effectId);
                    }
                    break;
                case 0x21: if (Client.MapAndPhysics) P21ReadChunkData(pkt); break;
                case 0x22: { //multi block change
                        if (!Client.MapAndPhysics) break;

                        int chunkX = pkt.ReadInt();
                        int chunkZ = pkt.ReadInt();
                        ushort count = pkt.ReadUShort();
                        int dataSize = pkt.ReadInt();

                        Chunk chunk = World.GetChunk(chunkX, chunkZ);
                        if (chunk == null) {
                            chunk = new Chunk(chunkX, chunkZ);
                            World.SetChunk(chunkX, chunkZ, chunk);
                        }
                        for (int i = 0; i < count; i++) {
                            ushort coord = pkt.ReadUShort();
                            ushort bdata = pkt.ReadUShort();

                            int blockId = bdata >> 4 & 0xFFF;
                            int blockData = bdata & 0xF;

                            int x = coord >> 12 & 0xF;
                            int z = coord >> 8 & 0xF;
                            int y = coord & 0xFF;
                            
                            chunk.SetBlock(x, y, z, (byte)blockId);
                            chunk.SetData(x, y, z, (byte)blockData);
                        }
                        World.FireChunkUpdate(chunkX, chunkZ);
                    }
                    break;
                case 0x23: { //block change
                        if (!Client.MapAndPhysics) break;

                        int x = pkt.ReadInt();
                        byte y = pkt.ReadByte();
                        int z = pkt.ReadInt();
                        int blockId = pkt.ReadVarInt();
                        byte data = pkt.ReadByte();

                        World.SetBlockAndData(x, y, z, (byte)blockId, data);
                    }
                    break;
                case 0x26: if (Client.MapAndPhysics) P26ReadMapChunkBulk(pkt); break;
                case 0x27: { //explosion
                        if (!Client.MapAndPhysics) break;
                        
                        float x = pkt.ReadFloat();
                        float y = pkt.ReadFloat();
                        float z = pkt.ReadFloat();
                        pkt.ReadFloat(); //radius
                        int count = pkt.ReadInt();
                        for (int i = 0; i < count; i++) {
                            int blockX = pkt.ReadSByte() + (int)x;
                            int blockY = pkt.ReadSByte() + (int)y;
                            int blockZ = pkt.ReadSByte() + (int)z;
                            World.SetBlockAndData(blockX, blockY, blockZ, 0, 0);
                        }
                        Player.MotionX += pkt.ReadFloat();
                        Player.MotionY += pkt.ReadFloat();
                        Player.MotionZ += pkt.ReadFloat();
                    }
                    break;
                case 0x2B: {//change game state
                        if (pkt.ReadByte() == 3) {
                            Client.Gamemode = (int)pkt.ReadFloat();
                        }
                    }
                    break;
                case 0x2D: {//open window
                        Inventory inv = new Inventory {
                            WindowID = pkt.ReadByte(),
                            Type = (InventoryType)pkt.ReadByte(),
                            Title = pkt.ReadString(),
                            NumSlots = pkt.ReadByte(),
                            UseTitle = pkt.ReadBoolean()
                        };
                        inv.Slots = new ItemStack[inv.NumSlots];
                        if (inv.Type == InventoryType.Horse)
                            inv.EntityID = pkt.ReadInt();

                        Client.OpenWindow = inv;
                        Inventory.ClickedItem = null;
                    }
                    break;
                case 0x2E://close window
                    byte invId = pkt.ReadByte();
                    if (Client.OpenWindow != null && Client.OpenWindow.WindowID == invId)
                        Client.OpenWindow = null;
                    Inventory.ClickedItem = null;
                    break;
                case 0x2F: { //set slot
                        byte winID = pkt.ReadByte();
                        int slot = pkt.ReadShort();
                        Inventory openWindow = Client.OpenWindow;
                        
                        if (winID == 0) {
                            Client.Inventory.SetItem(slot, ReadItemStack(pkt));
                        } else if (openWindow != null && openWindow.WindowID == winID) {
                            if (slot >= openWindow.NumSlots) {
                                Client.Inventory.SetItem((slot - openWindow.NumSlots) + 9, ReadItemStack(pkt));
                            } else {
                                openWindow.SetItem(slot, ReadItemStack(pkt));
                            }
                        }
                    }
                    break;
                case 0x30: { //window items
                        byte winID = pkt.ReadByte();
                        int count = pkt.ReadShort();

                        Inventory openWindow = Client.OpenWindow;
                        if (winID == 0) {
                            for (int i = 0; i < count; i++) {
                                Client.Inventory.SetItem(i, ReadItemStack(pkt));
                            }
                        } else if (openWindow != null && openWindow.WindowID == winID) {
                            for (int i = 0; i < count; i++) {
                                ItemStack stack = ReadItemStack(pkt);
                                if (i >= openWindow.NumSlots) {
                                    Client.Inventory.SetItem((i - openWindow.NumSlots) + 9, stack);
                                } else {
                                    openWindow.SetItem(i, stack);
                                }
                            }
                        }
                    }
                    break;
                case 0x32: {//confirm transaction
                        byte winID = pkt.ReadByte();
                        short actionNum = pkt.ReadShort();
                        bool accepted = pkt.ReadBoolean();
                        if (!accepted)
                            Client.SendPacket(new PacketConfirmTransaction(winID, actionNum, true));
                    }
                    break;
                case 0x33: {//update sign
                        if (!Client.MapAndPhysics) break;

                        int x = pkt.ReadInt();
                        short y = pkt.ReadShort();
                        int z = pkt.ReadInt();
                        string[] lines = new string[4];
                        for (int i = 0; i < 4; i++)
                            lines[i] = pkt.ReadString();
                        List<Vec3i> rem = new List<Vec3i>();
                        foreach (Vec3i v in World.Signs.Keys) {
                            int px = Utils.Floor(Player.PosX);
                            int pz = Utils.Floor(Player.PosZ);
                            int dx = v.X - px;
                            int dz = v.Z - pz;
                            if (Math.Sqrt(dx * dx + dz * dz) > 256)
                                rem.Add(v);
                        }
                        foreach (Vec3i key in rem)
                            World.Signs.TryRemove(key, out _);

                        if (x >= -30000000 && z >= -30000000 && x < 30000000 && z < 30000000 && y >= 0 && y < 256)
                            World.Signs[new Vec3i(x, y, z)] = lines;
                    }
                    break;
                case 0x38: { //player list
                        string username = pkt.ReadString();
                        bool online = pkt.ReadBoolean();
                        short ping = pkt.ReadShort();

                        if (online) {
                            Client.PlayerManager.UUID2Nick[UUID.NameUUID("OfflinePlayer:" + username)] = username;
                        } else {
                            Client.PlayerManager.UUID2Nick.TryRemove(UUID.NameUUID("OfflinePlayer:" + username), out _);
                        }
                    }
                    break;
                case 0x40: //disconnect
                    Client.HandlePacketDisconnect(ChatParser.ParseJson(pkt.ReadString()));
                    return false;
            }
            return true;
        }

        private void P21ReadChunkData(ReadBuffer pkt)
        {
            int x = pkt.ReadInt();
            int z = pkt.ReadInt();
            bool loadChunk = pkt.ReadBoolean();

            ushort pMask = pkt.ReadUShort();
            ushort addMask = pkt.ReadUShort();
            int dataSize = pkt.ReadInt();

            byte[] buffer = ZlibStream.UncompressBuffer(pkt.ReadByteArray(dataSize));

            if (loadChunk && pMask == 0)
                World.SetChunk(x, z, null);
            else
                ReadChunk(x, z, buffer, 0, loadChunk, Client.Dimension == 0, pMask);
        }
        private void P26ReadMapChunkBulk(ReadBuffer pkt)
        {
            short count = pkt.ReadShort();
            int dataSize = pkt.ReadInt();
            bool isOverworld = pkt.ReadBoolean();

            byte[] buffer = ZlibStream.UncompressBuffer(pkt.ReadByteArray(dataSize));

            int offset = 0;

            for (int i = 0; i < count; i++) {
                int x = pkt.ReadInt();
                int z = pkt.ReadInt();
                ushort mask = pkt.ReadUShort();
                ushort addMask = pkt.ReadUShort();

                int sectionCount = Utils.HammingWeight32(mask);
                int addCount = Utils.HammingWeight32(addMask);
                /*
                for (int j = 0; j < 16; j++) {
                    sectionCount += (mask >> j) & 0x01;
                    addCount += (addMask >> j) & 0x01;
                }*/

                int length = 2048 * 4 * sectionCount + 256;
                length += 2048 * addCount;

                if (isOverworld)
                    length += 2048 * sectionCount;

                //byte[] rawData = new byte[length];
                //Buffer.BlockCopy(decomp, offset, rawData, 0, length);

                ReadChunk(x, z, buffer, offset, true, isOverworld, mask);
                offset += length;
            }
        }

        private void ReadChunk(int x, int z, byte[] buffer, int offset, bool fullChunk, bool skyLight, ushort pMask)
        {
            if (!Client.MapAndPhysics) return;

            if (Client.LimitChunks) {
                int dx = (x * 16 + 8) - Utils.Floor(Player.PosX);
                int dz = (z * 16 + 8) - Utils.Floor(Player.PosZ);
                if (dx * dx + dz * dz > MinecraftClient.CHUNK_LIMIT)
                    return;
            }
            Chunk c = fullChunk ? new Chunk(x, z) : World.GetChunk(x, z);
            if (c == null) {
                c = new Chunk(x, z);
            }
            int idx = 0;
            for (int y = 0; y < 16; y++) {
                if ((pMask & (1 << y)) != 0) {
                    ChunkSection s = new ChunkSection();
                    idx += Copy(buffer, offset+idx, s.Blocks, 0, 4096);
                    c.Sections[y] = s;
                }
            }
            for (int y = 0; y < 16; y++)
                if ((pMask & (1 << y)) != 0)
                    idx += Copy(buffer, offset+idx, c.Sections[y].Metadata, 0, 2048);

            World.SetChunk(x, z, c);
        }
        private static int Copy(byte[] src, int srcIdx, byte[] dst, int dstIdx, int length)
        {
            Buffer.BlockCopy(src, srcIdx, dst, dstIdx, length);
            return length;
        }

        public static ItemStack ReadItemStack(ReadBuffer pkt)
        {
            short id = pkt.ReadShort();
            if (id == -1)
                return null;

            ItemStack stack = new ItemStack(id) {
                Count = pkt.ReadByte(),
                Metadata = pkt.ReadShort()
            };

            short nbtLength = pkt.ReadShort();
            if (nbtLength < 0) return stack;
            stack.NBTData = NbtIO.Decompress(pkt.ReadByteArray(nbtLength));

            return stack;
        }
        public static void WriteItemStack(WriteBuffer buf, ItemStack stack)
        {
            if (stack == null) {
                buf.WriteShort(-1);
            } else {
                buf.WriteShort(stack.ID);
                buf.WriteByte(stack.Count);
                buf.WriteShort(stack.Metadata);

                if (stack.NBTData == null) {
                    buf.WriteShort(-1);
                } else {
                    byte[] tag = NbtIO.Compress(stack.NBTData);
                    buf.WriteShort((short)tag.Length);
                    buf.WriteByteArray(tag);
                }
            }
        }

        public override bool WritePacket(ref int v18id, IPacket pkt, WriteBuffer buf)
        {
            return false;
        }
    }
}
