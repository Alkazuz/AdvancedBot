using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using AdvancedBot.client.Crypto;
using AdvancedBot.client.Map;
using AdvancedBot.client.NBT;
using AdvancedBot.client.Packets;
using Ionic.Zlib;

namespace AdvancedBot.client.Handler
{
    public class Handler_v152 : ProtocolHandler
    {
        private TcpClient _tcp;
        public NetworkStream NetStream;
        private Stream stream;

        public TcpClient TcpConn
        {
            get { return _tcp; }
            set {
                _tcp = value;
                NetStream = _tcp.GetStream();
                stream = NetStream;
            }
        }

        public Handler_v152(MinecraftClient mc) : base(mc) { }
        public override ClientVersion HandlerVersion { get { return ClientVersion.v1_5_2; } }

        public override bool HandlePacket(ReadBuffer pkt)
        {
            return true;
        }
        private int prevPkt = 0;
        private bool HandlePacket0(byte id)
        {
            //Debug.WriteLine("0x" + id.ToString("X2"));
            switch (id) {
                case 0x00: //Keep Alive
                    Client.SendPacket(new PacketKeepAlive(ReadInt()));
                    Client.ResetKeepAlive();
                    break;
                case 0x01: { //Join game
                        int playerId = ReadInt();
                        SkipString(); //Level type
                        int gm = ReadByte(); //Game mode
                        sbyte dimension = ReadSByte();
                        SkipBytes(3); //Difficulty, world height, max players
                        Client.HandlePacketJoinGame(playerId, dimension, gm);
                    }
                    break;
                case 0x02: ReadByte(); SkipString(); SkipString(); SkipBytes(4); break; //Handshake
                case 0x03: Client.HandlePacketChat(ReadString(), 0); break; //Chat Message
                case 0x04: SkipBytes(16); break; //Time Update
                case 0x05: SkipBytes(6); ReadItemStack(); break; //Entity Equipment
                case 0x06: SkipBytes(12); break; //Spawn Position
                case 0x07: SkipBytes(9); break; //Use Entity
                case 0x08: //Update Health
                    short health = ReadShort();
                    SkipBytes(6);//food+food saturation
                    if (health <= 0) {
                        Player.MotionX = Player.MotionY = Player.MotionZ = 0;
                        Thread.Sleep(300);
                        Client.SendPacket(new PacketClientStatus(0));
                    }
                    break;
                case 0x09: {//Respawn
                        sbyte dim = (sbyte)ReadInt();
                        int difficulty = ReadByte();
                        int gm = ReadByte();
                        Client.HandlePacketRespawn(dim, gm);
                        SkipBytes(2);
                        SkipString();
                    }
                    break; 
                case 0x0A: SkipBytes(1); break; //Player
                case 0x0B: { //Player Position
                    double x = ReadDouble();
                    double y = ReadDouble();
                    SkipBytes(8);//y+1.62
                    double z = ReadDouble();
                    Player.OnGround = ReadBoolean();
                    Player.SetPosition(x, y, z);

                    Player.MotionX = 0.0;
                    Player.MotionY = 0.0;
                    Player.MotionZ = 0.0;
                    break;
                }
                case 0x0C: SkipBytes(9); break; //Player Look
                case 0x0D: { //Player Position and Look
                    double x = ReadDouble();
                    double y = ReadDouble();
                    SkipBytes(8);//y+1.62
                    double z = ReadDouble();
                    Player.Yaw = ReadFloat();
                    Player.Pitch = ReadFloat();
                    Player.OnGround = ReadBoolean();
                    Player.SetPosition(x, y, z);

                    Player.MotionX = 0.0;
                    Player.MotionY = 0.0;
                    Player.MotionZ = 0.0;

                    Client.SendPacket(new PacketPosAndLook(Player.PosX, Player.AABB.MinY, Player.PosY, Player.PosZ, Player.Yaw, Player.Pitch, false));
                    break;
                }
                case 0x0E: SkipBytes(11); break; //Player Digging
                case 0x0F: SkipBytes(10); ReadItemStack(); SkipBytes(3); break; //Player Block Placement
                case 0x10: Client.SetHotbarSlot(ReadShort()); break; //Held Item Change
                case 0x11: SkipBytes(14); break; //Use Bed
                case 0x12: SkipBytes(5); break; //Animation
                case 0x13: SkipBytes(5); break; //Entity Action
                case 0x14: { //Spawn Named Entity
                        int entityId = ReadInt();
                        string username = ReadString();
                        double x = ReadInt() / 32.0;
                        double y = ReadInt() / 32.0;
                        double z = ReadInt() / 32.0;
                        float yaw = (float)(ReadSByte() * 360) / 256f;
                        float pitch = (float)(ReadSByte() * 360) / 256f;

                        ReadShort();//current item
                        SkipMetadata();

                        UUID uuid = UUID.NameUUID("OfflinePlayer:" + username);

                        MPPlayer player = new MPPlayer(entityId, uuid);
                        player.SetPos(x, y, z);
                        player.SetRotation(yaw, pitch);
                        
                        Client.PlayerManager.Players[entityId] = player;
                        Client.PlayerManager.UUID2Nick[uuid] = username;
                    }
                    break;
                case 0x16: SkipBytes(8); break; //Collect Item
                case 0x17: SkipBytes(19); if(ReadInt() != 0) SkipBytes(6); break; //Spawn Object/Vehicle
                case 0x18: SkipBytes(26); SkipMetadata(); break; //Spawn Mob
                case 0x19: SkipBytes(4); SkipString(); SkipBytes(16); break; //Spawn Painting
                case 0x1A: SkipBytes(18); break; //Spawn Experience Orb
                case 0x1C: { //Entity Velocity
                        int entityId = ReadInt();
                        double velX = ReadShort() / 8000.0;
                        double velY = ReadShort() / 8000.0;
                        double velZ = ReadShort() / 8000.0;
                        if (entityId == Client.PlayerID && MinecraftClient.Knockback) {
                            Player.MotionX += velX;
                            Player.MotionY += velY;
                            Player.MotionZ += velZ;
                        }
                    }
                    break;
                case 0x1D: { //Destroy Entity
                        int count = ReadByte();
                        for (int i = 0; i < count; i++)
                            Client.PlayerManager.Players.TryRemove(ReadInt(), out _);
                    }
                    break;
                case 0x1E: SkipBytes(4); break; //Entity
                case 0x1F: {//Entity Relative Move
                        int entityId = ReadInt();
                        double dx = ReadSByte() / 32.0;
                        double dy = ReadSByte() / 32.0;
                        double dz = ReadSByte() / 32.0;

                        if (Client.PlayerManager.Players.TryGetValue(entityId, out MPPlayer player)) {
                            player.X += dx;
                            player.Y += dy;
                            player.Z += dz;
                        }
                    }
                    break;
                case 0x20: { //Entity Look
                        int entityId = ReadInt();
                        float yaw = (ReadSByte() * 360) / 256f;
                        float pitch = (ReadSByte() * 360) / 256f;
                        if (Client.PlayerManager.Players.TryGetValue(entityId, out MPPlayer player))
                            player.SetRotation(yaw, pitch);
                    }
                    break;
                case 0x21: { //Entity Look and Relative Move
                        int entityId = ReadInt();

                        double dx = ReadSByte() / 32.0;
                        double dy = ReadSByte() / 32.0;
                        double dz = ReadSByte() / 32.0;

                        float yaw = (ReadSByte() * 360) / 256f;
                        float pitch = (ReadSByte() * 360) / 256f;
                        if (Client.PlayerManager.Players.TryGetValue(entityId, out MPPlayer player)) {
                            player.X += dx;
                            player.Y += dy;
                            player.Z += dz;
                            player.SetRotation(yaw, pitch);
                        }
                    }
                    break;
                case 0x22: { //Entity Teleport
                        int entityId = ReadInt();

                        double x = ReadInt() / 32.0;
                        double y = ReadInt() / 32.0;
                        double z = ReadInt() / 32.0;
                        float yaw = (ReadSByte() * 360) / 256f;
                        float pitch = (ReadSByte() * 360) / 256f;
                        if (Client.PlayerManager.Players.TryGetValue(entityId, out MPPlayer player)) {
                            player.SetPos(x, y, z);
                            player.SetRotation(yaw, pitch);
                        }
                    }
                    break;
                case 0x23: { //Entity Head Look
                        int entityId = ReadInt();
                        float yaw = (ReadSByte() * 360) / 256f;
                        if (Client.PlayerManager.Players.TryGetValue(entityId, out MPPlayer player)) {
                            player.Yaw = yaw;
                        }
                    }
                    break;
                case 0x26: SkipBytes(5); break; //Entity Status
                case 0x27: SkipBytes(8); break; //Attach Entity
                case 0x28: SkipBytes(4); SkipMetadata(); break; //Entity Metadata
                case 0x29: SkipBytes(8); break; //Entity Effect
                case 0x2A: SkipBytes(5); break; //Remove Entity Effect
                case 0x2B: SkipBytes(8); break; //Set Experience
                case 0x33: HandleChunkPacket(); break; //Chunk Data
                case 0x34: { //Multi block change
                        int chunkX = ReadInt();
                        int chunkZ = ReadInt();
                        int count = (ushort)ReadShort();
                        int dataSize = ReadInt();

                        if(!Client.MapAndPhysics) {
                            SkipBytes(count * 4);
                            break;
                        }
                        Chunk chunk = World.GetChunk(chunkX, chunkZ);
                        if (chunk == null) {
                            chunk = new Chunk(chunkX, chunkZ);
                            World.SetChunk(chunkX, chunkZ, chunk);
                        }
                        for (int i = 0; i < count; i++) {
                            ushort coord = (ushort)ReadShort();
                            ushort bdata = (ushort)ReadShort();

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
                case 0x35: { //Block change
                        int x = ReadInt();
                        int y = ReadByte();
                        int z = ReadInt();
                        int blockId = ReadShort();
                        byte data = ReadByte();
                        if (Client.MapAndPhysics) {
                            World.SetBlockAndData(x, y, z, (byte)blockId, data);
                        }
                    }
                    break;
                case 0x36: SkipBytes(14); break; //Block Action
                case 0x37: SkipBytes(17); break; //Block Break Animation
                case 0x38: HandleMultiChunkPacket(); break; //Map Chunk Bulk
                case 0x3C: { //Explosion
                        int x = (int)ReadDouble();
                        int y = (int)ReadDouble();
                        int z = (int)ReadDouble();
                        ReadFloat(); //radius
                        int count = ReadInt();
                        for (int i = 0; i < count; i++) {
                            int blockX = ReadSByte() + x;
                            int blockY = ReadSByte() + y;
                            int blockZ = ReadSByte() + z;
                            if (Client.MapAndPhysics) {
                                World.SetBlockAndData(blockX, blockY, blockZ, 0, 0);
                            }
                        }
                        Player.MotionX += ReadFloat();
                        Player.MotionY += ReadFloat();
                        Player.MotionZ += ReadFloat();
                    }
                    break;
                case 0x3D: SkipBytes(18); break; //Sound Or Particle Effect
                case 0x3E: SkipString(); SkipBytes(17); break; //Named Sound Effect
                case 0x3F: SkipString(); SkipBytes(32); break; //Particle
                case 0x46: { //Change Game State
                        byte reason = ReadByte();
                        byte value = ReadByte();
                        if (reason == 3) Client.Gamemode = value;
                    } break;
                case 0x47: SkipBytes(17); break; //Spawn Global Entity
                case 0xD0: ReadByte(); SkipString(); break; //Display Scoreboard
                case 0xD1: {//Teams
                        SkipString(); //Internal Name
                        byte mode = ReadByte();

                        if (mode == 0 || mode == 2) {
                            SkipString(); //Display Name
                            SkipString(); //Prefix
                            SkipString(); //Suffix
                            ReadByte(); //Friendly Fire
                        }
                        if (mode == 0 || mode == 3 || mode == 4) {
                            short count = ReadShort();
                            for (int i = 0; i < count; i++) {
                                SkipString(); //Players
                            }
                        }
                    }
                    break;
                case 0x64: { //Open Window
                        Inventory inv = new Inventory();
                        inv.WindowID = ReadByte();
                        inv.Type = (InventoryType)ReadByte();
                        inv.Title = ReadString();
                        inv.NumSlots = ReadByte();
                        inv.Slots = new ItemStack[inv.NumSlots];
                        inv.UseTitle = ReadBoolean();
                        
                        Client.OpenWindow = inv;
                        Inventory.ClickedItem = null;
                    }
                    break;
                case 0x65: { //Close Window
                        byte invId = ReadByte();
                        if (Client.OpenWindow != null && Client.OpenWindow.WindowID == invId)
                            Client.OpenWindow = null;
                        Inventory.ClickedItem = null;
                    }
                    break;
                case 0x66: SkipBytes(7); ReadItemStack(); break; //Click Window
                case 0x67: { //Set Slot
                        byte winID = ReadByte();
                        int slot = ReadShort();
                        ItemStack stack = ReadItemStack();
                        Inventory openWindow = Client.OpenWindow;

                        if (winID == 0) {
                            Client.Inventory.SetItem(slot, stack);
                        } else if (openWindow != null && openWindow.WindowID == winID) {
                            if (slot >= openWindow.NumSlots) {
                                Client.Inventory.SetItem((slot - openWindow.NumSlots) + 9, stack);
                            } else {
                                openWindow.SetItem(slot, stack);
                            }
                        }
                    }
                    break;
                case 0x68: { //Set Window Items
                        byte winID = ReadByte();
                        int count = ReadShort();
                        ItemStack[] items = new ItemStack[count];
                        for (int i = 0; i < count; i++) {
                            items[i] = ReadItemStack();
                        }

                        Inventory openWindow = Client.OpenWindow;
                        if (winID == 0) {
                            for (int i = 0; i < count; i++) {
                                Client.Inventory.SetItem(i, items[i]);
                            }
                        } else if (openWindow != null && openWindow.WindowID == winID) {
                            for (int i = 0; i < count; i++) {
                                if (i >= openWindow.NumSlots) {
                                    Client.Inventory.SetItem((i - openWindow.NumSlots) + 9, items[i]);
                                } else {
                                    openWindow.SetItem(i, items[i]);
                                }
                            }
                        }
                    }
                    break;
                case 0x69: SkipBytes(5); break; //Update Window Property
                case 0x6A: {//Confirm Transaction
                        byte winID = ReadByte();
                        short actionNum = ReadShort();
                        bool accepted = ReadBoolean();
                        if (!accepted)
                            Client.SendPacket(new PacketConfirmTransaction(winID, actionNum, true));
                    }
                    break;
                case 0x6B: SkipBytes(2); ReadItemStack(); break; //Creative Inventory Action
                case 0x6C: SkipBytes(2); break; //Enchant Item
                case 0x82: { //Update Sign
                        int x = ReadInt();
                        short y = ReadShort();
                        int z = ReadInt();
                        string[] lines = new string[4];
                        for (int i = 0; i < 4; i++) {
                            lines[i] = ReadString();
                        }

                        List<Vec3i> rem = new List<Vec3i>();
                        foreach (Vec3i v in World.Signs.Keys) {
                            int px = Utils.Floor(Player.PosX);
                            int pz = Utils.Floor(Player.PosZ);
                            int dx = v.X - px;
                            int dz = v.Z - pz;
                            if (Math.Sqrt(dx * dx + dz * dz) > 256)
                                rem.Add(v);
                        }
                        foreach (Vec3i key in rem) {
                            World.Signs.TryRemove(key, out _);
                        }
                        if (x >= -30000000 && z >= -30000000 && x < 30000000 && z < 30000000 && y >= 0 && y < 256) {
                            World.Signs[new Vec3i(x, y, z)] = lines;
                        }
                    }
                    break;
                case 0x83: SkipBytes(4); SkipBytes(ReadShort()); break; //Item Data
                case 0x84: SkipBytes(11); SkipBytes(ReadShort()); break; //Update Tile Entity
                case 0xC8: SkipBytes(5); break; //Increment Statistic
                case 0xC9: ReadString(); SkipBytes(3); break; //Player List Item
                case 0xCA: SkipBytes(3); break; //Player Abilities
                case 0xCB: SkipString(); break; //Tab-complete
                case 0xCC: SkipString(); SkipBytes(4); break; //Client Settings
                case 0xCD: SkipBytes(1); break; //Client Statuses
                case 0xCE: SkipString(); SkipString(); ReadByte(); break; //Scoreboard Objective
                case 0xCF: SkipString(); ReadByte(); SkipString(); SkipBytes(4); break; //Update Score
                case 0xFA: ReadString(); SkipBytes(ReadShort()); break; //Plugin Message
                case 0xFE: SkipBytes(1); break; //Server List Ping
                case 0xFF: //Kick
                    Client.HandlePacketDisconnect(ReadString());
                    return false;
                default:
                    Debug.WriteLine("Unknown packet 0x{0:X2}, prev=0x{1:X2}", id, prevPkt);
                    break;
            }
            Statistics.IncrementRead(1);
            prevPkt = id;
            return true;
        }

        public bool ReadAndHandlePacket()
        {
            return HandlePacket0(ReadByte());
        }

        public override bool WritePacket(ref int v18id, IPacket pkt, WriteBuffer buf)
        {
            switch (v18id) {
                case 0x00:
                    buf.WriteByte(0x00);
                    buf.WriteInt(((PacketKeepAlive)pkt).KeepAliveID);
                    break;
                case 0x01:
                    buf.WriteByte(0x03);
                    WriteString(buf, ((PacketChatMessage)pkt).Message);
                    break;
                case 0x02: {
                        PacketUseEntity p = (PacketUseEntity)pkt;
                        buf.WriteByte(0x07);
                        buf.WriteInt(Client.PlayerID);
                        buf.WriteInt(p.EntityID);
                        buf.WriteByte(p.MouseButton);
                    }
                    break;
                case 0x03:
                    buf.WriteByte(0x0A);
                    buf.WriteBoolean(((PacketUpdate)pkt).OnGround);
                    break;
                case 0x04: {
                        PacketPlayerPos p = (PacketPlayerPos)pkt;
                        buf.WriteByte(0x0B);
                        buf.WriteDouble(p.X);
                        buf.WriteDouble(p.FeetY);
                        buf.WriteDouble(p.Y);
                        buf.WriteDouble(p.Z);
                        buf.WriteBoolean(p.OnGround);
                    }
                    break;
                case 0x05: {
                        PacketPlayerLook p = (PacketPlayerLook)pkt;
                        buf.WriteByte(0x0C);
                        buf.WriteFloat(p.Yaw);
                        buf.WriteFloat(p.Pitch);
                        buf.WriteBoolean(p.OnGround);
                    }
                    break;
                case 0x06: {
                        PacketPosAndLook p = (PacketPosAndLook)pkt;
                        buf.WriteByte(0x0D);
                        buf.WriteDouble(p.X);
                        buf.WriteDouble(p.FeetY);
                        buf.WriteDouble(p.Y);
                        buf.WriteDouble(p.Z);
                        buf.WriteFloat(p.Yaw);
                        buf.WriteFloat(p.Pitch);
                        buf.WriteBoolean(p.OnGround);
                    }
                    break;
                case 0x07: {
                        PacketPlayerDigging p = (PacketPlayerDigging)pkt;
                        buf.WriteByte(0x0E);
                        buf.WriteByte((byte)p.Status);
                        buf.WriteInt(p.X);
                        buf.WriteByte(p.Y);
                        buf.WriteInt(p.Z);
                        buf.WriteByte(p.Face);
                    }
                    break;
                case 0x08: {
                        PacketBlockPlace p = (PacketBlockPlace)pkt;
                        buf.WriteByte(0x0F);
                        buf.WriteInt(p.X);
                        buf.WriteByte((byte)p.Y);
                        buf.WriteInt(p.Z);
                        buf.WriteByte(p.Direction);
                        Handler_v17.WriteItemStack(buf, p.Item);
                        buf.WriteByte(p.CursorX);
                        buf.WriteByte(p.CursorY);
                        buf.WriteByte(p.CursorZ);
                    }
                    break;
                case 0x09: {
                        buf.WriteByte(0x10);
                        buf.WriteShort(((PacketHeldItemChange)pkt).Slot);
                    }
                    break;
                case 0x0A: {
                        buf.WriteByte(0x12);
                        buf.WriteInt(((PacketSwingArm)pkt).EntityID);
                        buf.WriteByte(1);
                    }
                    break;
                case 0x0B: {
                        PacketEntityAction p = (PacketEntityAction)pkt;
                        buf.WriteByte(0x13);
                        buf.WriteInt(p.EntityID);
                        buf.WriteByte((byte)(p.ActionID + 1));
                    }
                    break;
                case 0x0D: {
                        buf.WriteByte(0x65);
                        buf.WriteByte(((PacketCloseWindow)pkt).WindowID);
                    }
                    break;
                case 0x0E: {
                        PacketClickWindow p = (PacketClickWindow)pkt;
                        buf.WriteByte(0x66);
                        buf.WriteByte(p.WindowID);
                        buf.WriteShort(p.Slot);
                        buf.WriteByte(p.Button);
                        buf.WriteShort(p.ActionNumber);
                        buf.WriteByte(p.Mode);
                        Handler_v17.WriteItemStack(buf, p.ClickedItem);
                    }
                    break;
                case 0x0F: {
                        PacketConfirmTransaction p = (PacketConfirmTransaction)pkt;
                        buf.WriteByte(0x6A);
                        buf.WriteByte(p.WindowID);
                        buf.WriteShort(p.ActionNumber);
                        buf.WriteBoolean(p.Accepted);
                    }
                    break;
                case 0x10: {
                        PacketCreativeInvAction p = (PacketCreativeInvAction)pkt;
                        buf.WriteByte(0x6B);
                        buf.WriteShort(p.Slot);
                        Handler_v17.WriteItemStack(buf, p.Item);
                    }
                    break;
                case 0x15: {
                        PacketClientSettings p = (PacketClientSettings)pkt;
                        buf.WriteByte(0xCC);
                        WriteString(buf, p.Locate);
                        buf.WriteByte(p.ViewDistance);
                        buf.WriteByte((byte)(p.ChatFlags | (0x10)));
                        buf.WriteByte(p.Difficulty);
                        buf.WriteBoolean(true);
                    }
                    break;
                case 0x16: {
                        buf.WriteByte(0xCD);
                        buf.WriteByte(1);
                    }
                    break;
                case 0x17: {
                        PacketPluginMessage p = (PacketPluginMessage)pkt;
                        buf.WriteByte(0xFA);
                        WriteString(buf, p.Channel);
                        buf.WriteShort((short)p.Data.Length);
                        buf.WriteByteArray(p.Data);
                    }
                    break;
                default:
                    Client.PrintToChat("§6AVISO: §fTentativa de enviar um pacote desconhecido para o servidor. ID=0x" + v18id.ToString("X2") + " CN=" + pkt.GetType().Name);
                    break;
            }

            /*if (v18id < 0x03 || v18id > 0x06) {
                Debug.WriteLine("0x{0:X2} {1}|1.5: {2}", v18id, pkt.GetType().FullName, BitConverter.ToString(buf.GetBytes()).Replace('-', ' '));
            }*/
            if (buf.Length != 0) {
                SendPacket(buf);
            }
            return true;
        }

        public bool ConnectAndHandshake()
        {
            /*  Client connects to server
                C->S 0x02 handshake
                S->C 0xFD encryption request - server sends its server id string, public key, and 4 random bytes
                Client generates symmetric key (shared secret)
                Client authenticates via session.minecraft.net.
                Client encrypts these 4 bytes with the servers public key.
                C->S 0xFC encryption response - client encrypts shared secret with server's public key and sends along with encrypted 4 bytes
                Server checks that the encrypted bytes match
                Server decrypts shared secret with its private key
                Server checks player authenticity via session.minecraft.net
                S->C 0xFC encryption response - empty payload meaning two zero length byte arrays and two zero shorts
                Server enables AES/CFB8 stream encryption
                Client enables AES/CFB8 stream encryption
                C->S 0xCD - Payload of 0 (byte)
                S->C 0x01 login
                see Protocol FAQ to get information about what happens next.*/

            WriteBuffer wb = new WriteBuffer();
            wb.WriteByte(0x02); //ID
            wb.WriteByte(61); //Version number
            WriteString(wb, Client.Username);
            WriteString(wb, Client.IP);
            wb.WriteInt(Client.Port);
            SendPacket(wb);

            for (int pktNo = 0; pktNo < 128; pktNo++) {
                byte id = ReadByte();
                if (id == 0xFD) {
                    string serverId = ReadString();
                    byte[] pubKey = ReadByteArray(ReadShort());
                    byte[] verifyToken = ReadByteArray(ReadShort());

                    using (RSACryptoServiceProvider rsa = CryptoUtils.DecodeRSAPublicKey(pubKey)) {
                        byte[] secretKey = CryptoUtils.GenerateAESPrivateKey();

                        if (serverId != "-") {
                            if (Client.LoginResp == null) {
                                TcpConn.Close();
                                throw new Exception("O servidor só aceita conexões autenticadas.");
                            }
                            SessionUtils.CheckSession(Client.LoginResp.UUID, Client.LoginResp.AccessToken, CryptoUtils.GetServerHash(serverId, pubKey, secretKey), Client.ConProxy);
                        }

                        byte[] encKey = rsa.Encrypt(secretKey, false);
                        byte[] encToken = rsa.Encrypt(verifyToken, false);

                        wb.Reset();
                        wb.WriteByte(0xFC);
                        wb.WriteShort((short)encKey.Length);
                        wb.WriteByteArray(encKey);
                        wb.WriteShort((short)encToken.Length);
                        wb.WriteByteArray(encToken);
                        SendPacket(wb);

                        if (ReadByte() == 0xFC) {
                            ReadByteArray(4);

                            stream = new AesStream(NetStream, secretKey);

                            wb.Reset();
                            wb.WriteByte(0xCD);
                            wb.WriteByte(0); //initial respawn
                            SendPacket(wb);

                            return true;
                        } else {
                            throw new Exception("O servidor não respondeu o pacote de encriptação.");
                        }
                    }
                } else {
                    HandlePacket0(id);
                }
            }
            throw new Exception("O servidor não completou a conexão.");
        }
        public void Ping(string ip, ushort port, Proxy proxy)
        {
            try {
                int count = MinecraftClient.MultiPing ? 4 : 1;
                for (int i = 0; i < count; i++) {
                    if (count != 1) {
                        Client.PrintToChat(string.Format("§fEnviando ping {0} de {1}...", i + 1, count));
                    }
                    TcpClient c = proxy == null ? new TcpClient(ip, port) : proxy.Connect(ip, port);

                    c.ReceiveTimeout = 5000;
                    c.SendTimeout = 5000;

                    using (NetworkStream s = c.GetStream()) {
                        s.Write(new byte[] { 0xFE, 1 }, 0, 2);


                        int id = s.ReadByte() | s.ReadByte() << 8;
                        int len = s.ReadByte() | s.ReadByte() << 8;

                        byte[] buf = new byte[len * 2];
                        s.Read(buf, 0, buf.Length);
                        string str = Encoding.Unicode.GetString(buf);
                        if (id != 0xFF || !str.StartsWith("§")) {
                            Client.PrintToChat("§6AVISO: §fO servidor respondeu o ping incorretamente.");
                        }

                        //string[] fields = str.StartsWith("§") ? str.Split('\0') : str.Split('§');
                    }

                    if (count != 1)
                        Thread.Sleep(1000);
                }
            } catch {
                Client.PrintToChat("§cNão foi possivel enviar o ping");
            }
        }

        private void HandleChunkPacket()
        {
            int x = ReadInt();
            int z = ReadInt();
            bool loadChunk = ReadByte() != 0;

            ushort pMask = (ushort)ReadShort();
            ushort addMask = (ushort)ReadShort();
            int dataSize = ReadInt();

            byte[] buffer = ZlibStream.UncompressBuffer(ReadByteArray(dataSize));

            if (loadChunk && pMask == 0)
                World.SetChunk(x, z, null);
            else
                ReadChunk(x, z, buffer, 0, loadChunk, Client.Dimension == 0, pMask);
        }
        private void HandleMultiChunkPacket()
        {
            short count = ReadShort();
            int dataSize = ReadInt();
            bool isOverworld = ReadByte() != 0;

            byte[] buffer = ZlibStream.UncompressBuffer(ReadByteArray(dataSize));

            int offset = 0;

            for (int i = 0; i < count; i++) {
                int x = ReadInt();
                int z = ReadInt();
                ushort mask = (ushort)ReadShort();
                ushort addMask = (ushort)ReadShort();

                int sectionCount = Utils.HammingWeight32(mask);
                int addCount = Utils.HammingWeight32(addMask);

                int length = 2048 * 4 * sectionCount + 256;
                length += 2048 * addCount;

                if (isOverworld)
                    length += 2048 * sectionCount;

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
                    idx += Copy(buffer, offset + idx, s.Blocks, 0, 4096);
                    c.Sections[y] = s;
                }
            }
            for (int y = 0; y < 16; y++)
                if ((pMask & (1 << y)) != 0)
                    idx += Copy(buffer, offset + idx, c.Sections[y].Metadata, 0, 2048);

            World.SetChunk(x, z, c);
        }
        private static int Copy(byte[] src, int srcIdx, byte[] dst, int dstIdx, int length)
        {
            Buffer.BlockCopy(src, srcIdx, dst, dstIdx, length);
            return length;
        }

        #region IO
        private byte ReadByte()
        {
            int n = stream.ReadByte();
            if (n == -1) throw new EndOfStreamException("EOF while reading byte.");
            return (byte)n;
        }
        private sbyte ReadSByte()
        {
            int n = stream.ReadByte();
            if (n == -1) throw new EndOfStreamException("EOF while reading byte.");
            return (sbyte)n;
        }
        private bool ReadBoolean()
        {
            return ReadByte() != 0;
        }
        private byte[] ReadByteArray(int length)
        {
            byte[] buffer = new byte[length];

            for (int remain = length; remain > 0; ) {
                int r = stream.Read(buffer, length - remain, remain);
                if (r == 0) throw new EndOfStreamException("EOF while reading byte array.");
                remain -= r;
            }
            Statistics.IncrementReadBytesOnly(length);
            return buffer;
        }
        private short ReadShort()
        {
            return (short)(ReadByte() << 8 | ReadByte());
        }
        private int ReadInt()
        {
            byte[] b = ReadByteArray(4);
            return b[0] << 24 | b[1] << 16 | b[2] << 8 | b[3];
        }
        public long ReadLong()
        {
            byte[] b = ReadByteArray(8);
            return (long)b[0] << 56 |
                   (long)b[1] << 48 |
                   (long)b[2] << 40 |
                   (long)b[3] << 32 |
                   (long)b[4] << 24 |
                   (long)b[5] << 16 |
                   (long)b[6] << 8 |
                   (long)b[7];
        }
        public unsafe float ReadFloat()
        {
            int n = ReadInt();
            return *(float*)&n;
        }
        public unsafe double ReadDouble()
        {
            long n = ReadLong();
            return *(double*)&n;
        }
        private string ReadString()
        {
            ushort len = (ushort)ReadShort();
            return Encoding.BigEndianUnicode.GetString(ReadByteArray(len * 2));
        }
        private void WriteString(WriteBuffer buf, string s)
        {
            buf.WriteUShort((ushort)s.Length);
            buf.WriteByteArray(Encoding.BigEndianUnicode.GetBytes(s));
        }

        private byte[] SKIP_BUFFER = new byte[4096];
        private void SkipBytes(int count)
        {
            for (int remain = count; remain > 0; ) {
                int r = stream.Read(SKIP_BUFFER, 0, Math.Min(4096, remain));
                if (r == 0) throw new EndOfStreamException("EOF while skipping byte array.");
                remain -= r;
            }
            Statistics.IncrementReadBytesOnly(count);
        }
        private void SkipString()
        {
            SkipBytes((ushort)ReadShort() * 2);
        }

        private void SkipMetadata()
        {
            for(byte id; (id = ReadByte()) != 0x7F; ) {
                int index = id & 0x1F;
                int type = id >> 5;
                switch (type) {
                    case 0: SkipBytes(1); break;        //Byte
                    case 1: SkipBytes(2); break;        //Short
                    case 2: SkipBytes(4); break;        //Int
                    case 3: SkipBytes(4); break;        //Float
                    case 4: ReadString(); break;        //String
                    case 5: ReadItemStack(); break;     //Slot
                    case 6: SkipBytes(12); break;       //Vector (3 Int)
                }
            }
        }

        private ItemStack ReadItemStack()
        {
            short id = ReadShort();
            if (id == -1)
                return null;

            ItemStack stack = new ItemStack(id) {
                Count = ReadByte(),
                Metadata = ReadShort()
            };

            short nbtLength = ReadShort();
            if (nbtLength > 0) {
                stack.NBTData = NbtIO.Decompress(ReadByteArray(nbtLength));
            }
            return stack;
        }

        private void SendPacket(WriteBuffer buf)
        {
            Statistics.IncrementSent(buf.Length);
            stream.Write(buf.GetBuffer(), 0, buf.Length);
        }
        #endregion
    }
}
