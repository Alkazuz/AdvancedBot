using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdvancedBot.client.Packets;
using System.Threading;
using AdvancedBot.client.Map;
using System.Diagnostics;
using Newtonsoft.Json;
using AdvancedBot.Json;

namespace AdvancedBot.client.Handler
{
    public class Handler_v18 : ProtocolHandler
    {
        public Handler_v18(MinecraftClient mc) : base(mc) { }

        public override ClientVersion HandlerVersion { get { return ClientVersion.v1_8; } }

        public override bool HandlePacket(ReadBuffer pkt)
        {
            try {
            switch (pkt.ID) {
                
                case 0x00: //keep alive
                    Client.SendPacket(new PacketKeepAlive(pkt.ReadVarInt()));
                Client.ResetKeepAlive();
                break;
                case 0x01: {//join game
                    int playerID = pkt.ReadInt();
                    int gm = pkt.ReadByte(); //gamemode
                    sbyte dimension = pkt.ReadSByte();
                    //packet.ReadByteArray(2); //difficulty, max Players
                    //packet.ReadString(); //Level type
                    Client.HandlePacketJoinGame(playerID, dimension, gm);
                }
                break;
                case 0x02: //chat
                    string c = pkt.ReadString();
                    string chat = ChatParser.ParseJson(c);
                    byte chatPos = pkt.ReadByte();
                        
                        Client.HandlePacketChat(chat, chatPos);
                        if (chatPos == 0)
                        {
                            ChatMessage message = ChatDeserializer.deserialize(c);
                            if(message.GetWith().Count > 0)
                            {
                                Client.JSONMessage.Add(message);
                                Client.chatJsonUpdate = true;
                            }
                            
                        }
                            
                        break;
                case 0x06: //update health
                    float health = pkt.ReadFloat();
                //packet.ReadVarInt();
                //packet.ReadFloat();

                if (health <= 0)
                {
                    Player.MotionX = Player.MotionY = Player.MotionZ = 0;
                    Client.SendPacket(new PacketClientStatus(0));
                }
                break;
                case 0x07: {//respawn

                    sbyte dim = (sbyte)pkt.ReadInt();
                    int difficulty = pkt.ReadByte();
                    int gm = pkt.ReadByte();
                    Client.HandlePacketRespawn(dim, gm);
                    //packet.ReadByteArray(2);//difficulty + gm
                    //packet.ReadString();
                }
                break;
                case 0x08: { //pos and look
                    double x = pkt.ReadDouble();
                    double y = pkt.ReadDouble();
                    double z = pkt.ReadDouble();
                    float yaw = pkt.ReadFloat();
                    float pitch = pkt.ReadFloat();
                    byte flags = pkt.ReadByte();

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

                    Client.SendPacket(new PacketPosAndLook(Player.PosX, Player.AABB.MinY, Player.PosY, Player.PosZ, Player.Yaw, Player.Pitch, false));
                }
                break;
                case 0x09: Client.SetHotbarSlot(pkt.ReadByte()); break;
                case 0x0C: { //spawn player
                    int entityID = pkt.ReadVarInt();
                    UUID uuid = new UUID(pkt.ReadLong(), pkt.ReadLong());
                    double x = pkt.ReadInt() / 32.0;
                    double y = pkt.ReadInt() / 32.0;
                    double z = pkt.ReadInt() / 32.0;
                    float yaw = (pkt.ReadSByte() * 360) / 256f;
                    float pitch = (pkt.ReadSByte() * 360) / 256f;

                    MPPlayer player = new MPPlayer(entityID, uuid);
                    player.SetPos(x, y, z);
                    player.SetRotation(yaw, pitch);

                    Client.PlayerManager.Players[entityID] = player;

                    //var metadata = ReadEntityMetadata(pkt);
                    //var displayName = metadata[2] as string;
                    //Debug.WriteLine("MD DisplayName {0} '{1}'", entityID, displayName ?? "null");
                }
                break;
                case 0x12: { //entity velocity
                    int entityId = pkt.ReadVarInt();
                    double velX = pkt.ReadShort() / 8000.0;
                    double velY = pkt.ReadShort() / 8000.0;
                    double velZ = pkt.ReadShort() / 8000.0;

                    if (entityId == Client.PlayerID && MinecraftClient.Knockback)
                    {
                        Player.MotionX += velX;
                        Player.MotionY += velY;
                        Player.MotionZ += velZ;
                    }
                }
                break;
                case 0x13: {//destroy entities
                    int count = pkt.ReadVarInt();
                    for (int i = 0; i < count; i++)
                        Client.PlayerManager.Players.TryRemove(pkt.ReadVarInt(), out _);
                }
                break;
                case 0x15: { //entity relative move
                    int entityId = pkt.ReadVarInt();
                    double dx = pkt.ReadSByte() / 32.0;
                    double dy = pkt.ReadSByte() / 32.0;
                    double dz = pkt.ReadSByte() / 32.0;

                    if (Client.PlayerManager.Players.TryGetValue(entityId, out MPPlayer player))
                    {
                        player.X += dx;
                        player.Y += dy;
                        player.Z += dz;
                    }
                }
                break;
                case 0x16: { //entity look
                    int entityId = pkt.ReadVarInt();
                    float yaw = (pkt.ReadSByte() * 360) / 256f;
                    float pitch = (pkt.ReadSByte() * 360) / 256f;
                    if (Client.PlayerManager.Players.TryGetValue(entityId, out MPPlayer player))
                    {
                        player.SetRotation(yaw, pitch);
                        Client.SendPacket(new PacketPosAndLook(Player.PosX, Player.AABB.MinY, Player.PosY, Player.PosZ, Player.Yaw, Player.Pitch, false));
                    }
                }
                break;
                case 0x17: {// entity look and relative move
                    int entityId = pkt.ReadVarInt();

                    double dx = pkt.ReadSByte() / 32.0;
                    double dy = pkt.ReadSByte() / 32.0;
                    double dz = pkt.ReadSByte() / 32.0;

                    float yaw = (pkt.ReadSByte() * 360) / 256f;
                    float pitch = (pkt.ReadSByte() * 360) / 256f;
                    if (Client.PlayerManager.Players.TryGetValue(entityId, out MPPlayer player))
                    {
                        player.X += dx;
                        player.Y += dy;
                        player.Z += dz;
                        player.SetRotation(yaw, pitch);
                        Client.SendPacket(new PacketPosAndLook(Player.PosX, Player.AABB.MinY, Player.PosY, Player.PosZ, Player.Yaw, Player.Pitch, false));
                    }
                }
                break;
                case 0x18: { //entity teleport
                    int entityId = pkt.ReadVarInt();

                    double x = pkt.ReadInt() / 32.0;
                    double y = pkt.ReadInt() / 32.0;
                    double z = pkt.ReadInt() / 32.0;
                    float yaw = (pkt.ReadSByte() * 360) / 256f;
                    float pitch = (pkt.ReadSByte() * 360) / 256f;
                    if (Client.PlayerManager.Players.TryGetValue(entityId, out MPPlayer player))
                    {
                        player.SetPos(x, y, z);
                        player.SetRotation(yaw, pitch);
                    }
                }
                break;
                case 0x19: { //entity head look
                    int entityId = pkt.ReadVarInt();
                    float yaw = (pkt.ReadSByte() * 360) / 256f;
                    if (Client.PlayerManager.Players.TryGetValue(entityId, out MPPlayer player))
                        player.Yaw = yaw;
                }
                break;
                case 0x1D: {
                    int entityId = pkt.ReadVarInt();
                    byte effectId = pkt.ReadByte();
                    byte amp = pkt.ReadByte();
                    /* int duration = packet.ReadVarInt();
                     bool hideParticles = packet.ReadBoolean();*/
                    if (entityId == Client.PlayerID)
                        Player.ActivePotions[effectId] = amp;
                }
                break;
                case 0x1E: {
                    int entityId = pkt.ReadVarInt();
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
                    int count = pkt.ReadVarInt();

                    Chunk chunk = World.GetChunk(chunkX, chunkZ);
                    if (chunk == null)
                    {
                        chunk = new Chunk(chunkX, chunkZ);
                        World.SetChunk(chunkX, chunkZ, chunk);
                    }
                    for (int i = 0; i < count; i++)
                    {
                        ushort coord = pkt.ReadUShort();
                        int bdata = pkt.ReadVarInt();

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
                    Vec3i pos = pkt.ReadLocation();

                    int bdata = pkt.ReadVarInt();

                    int blockId = bdata >> 4 & 0xFFF;
                    int blockData = bdata & 0xF;

                    World.SetBlockAndData(pos.X, pos.Y, pos.Z, (byte)blockId, (byte)blockData);
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
                    for (int i = 0; i < count; i++)
                    {
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
                    if (pkt.ReadByte() == 3)
                    {
                        Client.Gamemode = (int)pkt.ReadFloat();
                    }
                }
                break;
                case 0x2D: {//open window

                    Inventory inv = new Inventory
                    {
                        WindowID = pkt.ReadByte(),
                        Type = Inventory.GetType(pkt.ReadString()),
                        Title = ChatParser.ParseJson(pkt.ReadString()),
                        NumSlots = pkt.ReadByte()
                    };

                    inv.Slots = new ItemStack[inv.NumSlots];

                    if (inv.Type == InventoryType.Horse)
                        inv.EntityID = pkt.ReadInt();
                    Client.OpenWindow = inv;
                    Inventory.ClickedItem = null;
                }
                break;
                case 0x2E: //close window
                    byte invId = pkt.ReadByte();
                if (Client.OpenWindow != null && Client.OpenWindow.WindowID == invId)
                    Client.OpenWindow = null;
                Inventory.ClickedItem = null;
                break;
                case 0x2F: { //set slot
                    byte winID = pkt.ReadByte();
                    int slot = pkt.ReadShort();
                    Inventory openWindow = Client.OpenWindow;

                    if (winID == 0)
                    {
                        Client.Inventory.SetItem(slot, pkt.ReadItemStack());
                    }
                    else if (openWindow != null && openWindow.WindowID == winID)
                    {
                        if (slot >= openWindow.NumSlots)
                        {
                            Client.Inventory.SetItem((slot - openWindow.NumSlots) + 9, pkt.ReadItemStack());
                        }
                        else
                        {
                            openWindow.SetItem(slot, pkt.ReadItemStack());
                        }
                    }
                }
                break;
                case 0x30: { //window items
                    byte winID = pkt.ReadByte();
                    int count = pkt.ReadShort();

                    Inventory openWindow = Client.OpenWindow;
                    if (winID == 0)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            Client.Inventory.SetItem(i, pkt.ReadItemStack());
                        }
                    }
                    else if (openWindow != null && openWindow.WindowID == winID)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            ItemStack stack = pkt.ReadItemStack();
                            if (i >= openWindow.NumSlots)
                            {
                                Client.Inventory.SetItem((i - openWindow.NumSlots) + 9, stack);
                            }
                            else
                            {
                                openWindow.SetItem(i, stack);
                            }
                        }
                    }
                }
                break;
                case 0x32: { //confirm transaction
                    byte winID = pkt.ReadByte();
                    short actionNum = pkt.ReadShort();
                    bool accepted = pkt.ReadBoolean();
                    Debug.WriteLine("Confirm Transaction: " + winID + " " + actionNum + " " + accepted);
                    if (!accepted)
                    {
                        Client.SendPacket(new PacketConfirmTransaction(winID, actionNum, true));
                        Inventory.ClickedItem = null;
                    }
                }
                break;
                case 0x33: { //update sign
                    if (!Client.MapAndPhysics) break;

                    Vec3i position = pkt.ReadLocation();
                    string[] lines = new string[4];
                    for (int i = 0; i < 4; i++)
                        lines[i] = ChatParser.ParseJson(pkt.ReadString());

                    List<Vec3i> rem = new List<Vec3i>();
                    foreach (Vec3i v in World.Signs.Keys)
                    {
                        int px = Utils.Floor(Player.PosX);
                        int pz = Utils.Floor(Player.PosZ);
                        int dx = v.X - px;
                        int dz = v.Z - pz;
                        if (dx * dx + dz * dz > 300 * 300)
                            rem.Add(v);
                    }
                    foreach (Vec3i key in rem)
                        World.Signs.TryRemove(key, out _);

                    World.Signs[position] = lines;
                }
                break;
                case 0x38: ReadPlayerList(pkt); break;
                case 0x40: //disconnect
                    string reason = ChatParser.ParseJson(pkt.ReadString());
                Client.HandlePacketDisconnect(reason);
                return false;
                case 0x45: { //title
                    int action = pkt.ReadVarInt();
                    if (action == 1)
                    {

                    }
                    if (action == 0 || action == 1)
                    {//title or subtitle
                        Client.HandlePacketChat(ChatParser.ParseJson(pkt.ReadString()), 0);
                    }
                    break;
                }
                case 0x46: {
                    Client.Stream.CompressionThreshold = pkt.ReadVarInt();
                    break;
                }
            
            }
            }
            catch (Exception ex) { }
            return true;
        }

        private void P21ReadChunkData(ReadBuffer pkt)
        {
            int x = pkt.ReadInt();
            int z = pkt.ReadInt();
            bool loadChunk = pkt.ReadBoolean();
            ushort pMask = pkt.ReadUShort();
            byte[] data = pkt.ReadByteArray(pkt.ReadVarInt());

            if (loadChunk && pMask == 0) {
                World.SetChunk(x, z, null);
            } else {
                ReadChunk(x, z, data, loadChunk, Client.Dimension == 0, pMask);
            }
        }
        private void P26ReadMapChunkBulk(ReadBuffer pkt)
        {
            bool skyLight = pkt.ReadBoolean();
            int columnCount = pkt.ReadVarInt();

            Vec3i[] chunks = new Vec3i[columnCount];
            for (int i = 0; i < columnCount; i++) {
                int x = pkt.ReadInt();
                int z = pkt.ReadInt();
                ushort mask = pkt.ReadUShort();
                chunks[i] = new Vec3i(x, mask, z);
            }
            for (int i = 0; i < columnCount; i++) {
                Vec3i chunk = chunks[i];
                byte[] bytes = pkt.ReadByteArray(CalcChunkSize(Utils.HammingWeight32(chunk.Y), skyLight, true));
                ReadChunk(chunk.X, chunk.Z, bytes, true, skyLight, (ushort)chunk.Y);
            }
        }
        private void ReadChunk(int x, int z, byte[] data, bool fullChunk, bool skyLight, ushort pMask)
        {
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
                    for (int i = 0; i < 4096; i++) {
                        int block = data[idx++] | (data[idx++] << 8);

                        s.Blocks[i] = (byte)(block >> 4 & 0xFF);

                        int meta = block & 0xF;
                        s.Metadata[i / 2] |= (byte)(i % 2 == 0 ? meta : (meta << 4));
                    }
                    c.Sections[y] = s;
                }
            }

            World.SetChunk(x, z, c);
        }
        private static int CalcChunkSize(int sectionCount, bool hasSkyLight, bool hasBiomes)
        {
            //                     blocks  blight
            return sectionCount * (8192 + 2048 + (hasSkyLight ? 2048 : 0)) + (hasBiomes ? 256 : 0);
        }

        private void ReadPlayerList(ReadBuffer pkt)
        {
            PlayerManager playerMngr = Client.PlayerManager;
            int action = pkt.ReadVarInt();
            int count = pkt.ReadVarInt();

            //var actions = new string[] { "ADD_PLAYER", "UPDATE_GAMEMODE", "UPDATE_PING", "UPDATE_DISPLAY", "REMOVE_PLAYER" };
            //Debug.WriteLine("Packet PlayerList action={0} count={1}", actions[action], count);

            for (int i = 0; i < count; i++) {
                UUID uuid = pkt.ReadUUID();
                switch (action) {
                    case 0: //add player
                        string nick = pkt.ReadString();

                        int propertyCount = pkt.ReadVarInt();
                        for (int j = 0; j < propertyCount; ++j) {
                            pkt.ReadString(); //prop name
                            pkt.ReadString(); //prop value

                            if (pkt.ReadBoolean())
                                pkt.ReadString(); //prop signature
                        }

                        pkt.ReadVarInt();//gamemode
                        pkt.ReadVarInt();//ping

                        if (pkt.ReadBoolean()) //has display name
                            pkt.ReadString();//display name
                        playerMngr.UUID2Nick[uuid] = nick;
                        break;
                    case 1: //update gamemode
                        pkt.ReadVarInt();//new gamemode
                        break;
                    case 2: //update ping
                        pkt.ReadVarInt();//new ping
                        break;
                    case 3: //update display name
                        if (pkt.ReadBoolean()) {
                            pkt.ReadString();
                        }
                        break;
                    case 4://remove player
                        playerMngr.UUID2Nick.TryRemove(uuid, out _);
                        break;
                }
            }
        }

        private static object[] ReadEntityMetadata(ReadBuffer buf)
        {
            var array = new object[0x1F];
            for (byte b; (b = buf.ReadByte()) != 0x7F;) {
                int index = b & 0x1F;
                int type = b >> 5;

                switch (type) {
                    case 0: array[index] = buf.ReadByte(); break;
                    case 1: array[index] = buf.ReadShort(); break;
                    case 2: array[index] = buf.ReadInt(); break;
                    case 3: array[index] = buf.ReadFloat(); break;
                    case 4: array[index] = buf.ReadString(); break;
                    case 5: array[index] = buf.ReadItemStack(); break;
                    case 6: buf.Skip(4 * 3); break; /* vector   (3 integers) */
                    case 7: buf.Skip(4 * 3); break; /* rotation (3 floats) */
                }
            }
            return array;
        }

        public override bool WritePacket(ref int v18id, IPacket pkt, WriteBuffer buf)
        {
            return false;
        }
    }
}
