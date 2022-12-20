using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using AdvancedBot.client;
using Newtonsoft.Json.Linq;
using AdvancedBot.client.Map;
using System.Diagnostics;
using AdvancedBot.client.NBT;
using System.Collections.Concurrent;

namespace AdvancedBot
{
    public class TestServer
    {
        private Thread loopThread;

        private bool listen;
        TcpListener listener;
        private ConcurrentDictionary<int, Client> clientss = new ConcurrentDictionary<int, Client>();
        private int ci = 0;

        public World world = new World(null);

        public void Run()
        {
            listen = true;
            Listen();

            loopThread = new Thread(Loop);
            loopThread.IsBackground = true;
            loopThread.Start();
            
            loopThread.Name = "TSV Loop Thread";

            int width = 16;
            int depth = 16;

            for (int x = 0; x < width; x++) {
                for (int z = 0; z < depth; z++) {
                    //Debug.WriteLine(noiseAt(x, z));
                    Chunk ch = new Chunk(x - (width / 2), z - (depth / 2));
                    ch.Sections[0] = new ChunkSection();
                    ch.Sections[1] = new ChunkSection();

                    for (int bx = 0; bx < 16; bx++) {
                        for (int bz = 0; bz < 16; bz++) {
                            int height = 6;
                            ch.SetBlock(bx, 0, bz, Blocks.bedrock);
                            for (int y = 0; y < height; y++)
                                ch.SetBlock(bx, 1 + y, bz, (byte)(y < height - 3 ? Blocks.stone : Blocks.dirt));
                            ch.SetBlock(bx, height + 1, bz, Blocks.grass);

                            int wx = (ch.X << 4) + bx;
                            int wz = (ch.Z << 4) + bz;

                            if(!(wx > -128 && wz > -128 && wx < 127 && wz < 127)) {
                                ch.SetBlock(bx, height + 3, bz, Blocks.barrier);
                            }
                        }
                    }

                    world.SetChunk(ch.X, ch.Z, ch);
                }
            }

            Random rng = new Random();
            int[] ores = new int[] { Blocks.stone, Blocks.diamond_ore, Blocks.iron_ore, Blocks.gold_ore, Blocks.coal_ore, Blocks.lapis_ore };
            for (int x = 0; x < 32; x++) {
                for (int z = 0; z < 32; z++) {
                    for (int y = 0; y < 12; y++) {
                        world.SetBlock(4 + x, 8 + y, 4 + z, (byte)ores[rng.Next(ores.Length)]);
                    }
                }
            }

            GenPortal(-10, 8, -10);

            void GenPortal(int px, int py, int pz) {
                for (int x = 0; x < 5; x++) {
                    for (int y = 0; y < 6; y++) {
                        if ((x == 0 || x == 4) || (y == 0 || y == 5)) {
                            world.SetBlock(px + x, py + y, pz, Blocks.obsidian);
                        } else {
                            world.SetBlock(px + x, py + y, pz, Blocks.portal);
                        }
                    }
                }
            }
        }
        public void Stop()
        {
            listen = false;
            if (listener != null) {
                listener.Stop();
            }
            try {
                loopThread.Join(500);
                loopThread.Abort();
            } catch { }
        }

        private void Loop()
        {
            Stopwatch sw = Stopwatch.StartNew();
            List<int> rem = new List<int>();
            while (listen) {
                sw.Restart();

                foreach (var kv in clientss) {
                    if (!kv.Value.Update()) {
                        rem.Add(kv.Key);
                    }
                }
                foreach(var key in rem) {
                    clientss.TryRemove(key, out _);
                }
                rem.Clear();

                int delta = 50 - (int)sw.ElapsedMilliseconds;
                if (delta > 0) {
                    Thread.Sleep(delta);
                }
            }
        }
        private async void Listen()
        {
            listener = new TcpListener(IPAddress.Loopback, 25564);
            listener.Start();
            Debug.WriteLine("Test server is running at port 25564");

            while (listen) {
                Socket cli = await listener.AcceptSocketAsync();
                cli.ReceiveTimeout = 1000;
                cli.SendTimeout = 1000;

                int key = Interlocked.Increment(ref ci);
                clientss.TryAdd(key, new Client(this, key, cli));
            }
        }
        public void SendPacketForAllPlayers(WriteBuffer wb)
        {
            foreach (Client cli in clientss.Values) {
                if (cli.stream.Connected) {
                    cli.stream.SendPacket(wb);
                }
            }
        }

        private class Client
        {
            public PacketStream stream;
            public TestServer sv;
            int ticks;
            private int connState = 0;
            public string playerNick;
            public HashSet<string> spawnedPlayers = new HashSet<string>();
            public int entityId;

            public double x, y, z;

            private static int entityIdCounter = 0;
            public int NextEntityID()
            {
                return Interlocked.Increment(ref entityIdCounter);
            }

            public Client(TestServer server, int key, Socket c)
            {
                stream = new PacketStream(c, false);
                sv = server;

                stream.OnPacketAvailable += HandlePacket;
                stream.OnError += (ex) => {
                    stream.Dispose();
                    server.clientss.TryRemove(key, out _);
                };
                stream.StartReceiving();
            }
            private void HandlePacket(ReadBuffer pkt)
            {
                WriteBuffer wb = new WriteBuffer();
                if (connState != 2) {
                    if (connState++ == 1) {
                        playerNick = pkt.ReadString();

                        wb.Reset();
                        wb.WriteVarInt(0x03);
                        wb.WriteVarInt(256);
                        stream.SendPacket(wb);
                        stream.CompressionThreshold = 256;

                        wb.Reset();
                        wb.WriteVarInt(0x02);
                        wb.WriteString(UUID.NameUUID("OfflinePlayer:" + playerNick).ToString());
                        wb.WriteString(playerNick);
                        stream.SendPacket(wb);

                        InitGame();
                    }
                } else {
                    switch (pkt.ID) {
                        case 0x01:
                            wb.Reset();
                            wb.WriteVarInt(0x02);
                            string msg = pkt.ReadString();
                            JObject json = new JObject(new JProperty("text", "<" + playerNick + "> " + msg));
                            wb.WriteString(json.ToString());
                            wb.WriteByte(0);
                            sv.SendPacketForAllPlayers(wb);
                            break;
                        case 0x04:
                            x = pkt.ReadDouble();
                            y = pkt.ReadDouble();
                            z = pkt.ReadDouble();
                            break;
                        case 0x06:
                            x = pkt.ReadDouble();
                            y = pkt.ReadDouble();
                            z = pkt.ReadDouble();
                            break;
                        case 0x07:
                            byte status = pkt.ReadByte();
                            if (status == 2) {
                                Vec3i dpos = pkt.ReadLocation();
                                sv.world.SetBlock(dpos.X, dpos.Y, dpos.Z, (byte)0);

                                wb.Reset();
                                wb.WriteVarInt(0x23);
                                wb.WriteLocation(dpos);
                                wb.WriteVarInt(0x0);

                                sv.SendPacketForAllPlayers(wb);
                            }
                            break;
                        case 0x08:
                            Vec3i pos = pkt.ReadLocation();
                            byte dir = pkt.ReadByte();
                            ItemStack item = pkt.ReadItemStack();
                            if (item != null && item.ID < 256) {
                                int n = (dir % 2) == 0 ? -1 : 1;
                                switch (dir / 2) {
                                    case 0: pos.Y += n; break;
                                    case 1: pos.Z += n; break;
                                    case 2: pos.X += n; break;
                                }
                                sv.world.SetBlock(pos.X, pos.Y, pos.Z, (byte)item.ID);

                                wb.Reset();
                                wb.WriteVarInt(0x23);
                                wb.WriteLocation(pos);
                                wb.WriteVarInt(item.ID << 4 | 0x0);

                                sv.SendPacketForAllPlayers(wb);
                            }
                            break;
                    }
                }
            }
            public bool Update()
            {
                try {
                    if(connState == 2) {
                        WriteBuffer wb = new WriteBuffer();
                        if (ticks++ % 20 == 0) {
                            wb.Reset();
                            wb.WriteVarInt(0x00);
                            wb.WriteVarInt(Environment.TickCount);
                            stream.SendPacket(wb);
                        }
                        SpawnPlayers(wb);
                    }
                } catch {
                    return false;
                }
                return true;
            }
            private void SpawnPlayers(WriteBuffer b)
            {
                foreach (Client c in sv.clientss.Values) {
                    if (c != this && c.connState == 2 && !spawnedPlayers.Contains(c.playerNick)) {
                        UUID uuid = UUID.NameUUID("OfflinePlayer:" + c.playerNick);
                        b.Reset();
                        b.WriteVarInt(0x38);
                        b.WriteVarInt(0);
                        b.WriteVarInt(1);

                        b.WriteUUID(uuid);
                        b.WriteString(c.playerNick);
                        b.WriteVarInt(0);
                        b.WriteVarInt(0);
                        b.WriteVarInt(0);
                        b.WriteBoolean(false);
                        stream.SendPacket(b);

                        b.Reset();
                        b.WriteVarInt(0x0C);
                        b.WriteVarInt(c.entityId);
                        b.WriteUUID(uuid);
                        b.WriteInt((int)(c.x * 32));
                        b.WriteInt((int)(c.y * 32));
                        b.WriteInt((int)(c.z * 32));
                        b.WriteByte(0);
                        b.WriteByte(0);
                        b.WriteShort(0);
                        b.WriteByte(0x7F);
                        stream.SendPacket(b);

                        spawnedPlayers.Add(c.playerNick);
                    } else {
                        b.Reset();
                        b.WriteVarInt(0x18);
                        b.WriteVarInt(c.entityId);
                        b.WriteInt((int)(c.x * 32));
                        b.WriteInt((int)(c.y * 32));
                        b.WriteInt((int)(c.z * 32));
                        b.WriteByte(0);
                        b.WriteByte(0);
                        b.WriteByte(0);
                        stream.SendPacket(b);
                    }
                }
            }
            private void InitGame()
            {
                WriteBuffer wb = new WriteBuffer();

                //Join game
                wb.WriteVarInt(0x01);
                wb.WriteInt(entityId = NextEntityID());
                wb.WriteByte(1);//gm
                wb.WriteSByte(0);//dim
                wb.WriteByte(0);//difficulty
                wb.WriteByte(100);
                wb.WriteString("default");
                wb.WriteBoolean(false);
                stream.SendPacket(wb);

                byte[] cBuf = new byte[CalcChunkSize(16, true, true)];
                foreach (Chunk c in sv.world.Chunks.Values) {
                    wb.Reset();
                    wb.WriteVarInt(0x21);
                    wb.WriteInt(c.X);
                    wb.WriteInt(c.Z);
                    wb.WriteBoolean(true);
                    int mask = 0;
                    for (int i = 0; i < 16; i++) {
                        if (c.Sections[i] != null) {
                            mask |= 1 << i;
                        }
                    }
                    wb.WriteUShort((ushort)mask);

                    int len = FillChunkBuffer(cBuf, c);

                    wb.WriteVarInt(len);
                    wb.WriteByteArray(cBuf, len);

                    stream.SendPacket(wb);
                }

                wb.Reset();
                wb.WriteVarInt(0x08);
                wb.WriteDouble(0);
                wb.WriteDouble(16);
                wb.WriteDouble(0);
                wb.WriteFloat(0);
                wb.WriteFloat(0);
                wb.WriteByte(0);
                stream.SendPacket(wb);

                SetItem(wb, 0, new ItemStack(Blocks.planks, 0, 4));
                SetItem(wb, 2, new ItemStack(Items.bow, 0, 4));
                SetItem(wb, 3, new ItemStack(Items.arrow, 0, 64));

                ItemStack dp = new ItemStack(Items.diamond_pickaxe);
                dp.NBTData = new CompoundTag();

                ListTag<Tag> enchList = new ListTag<Tag>();

                enchList.AddTag(new CompoundTag().Add("id", new ShortTag("", 32))
                                                 .Add("lvl", new ShortTag("", 5)));
                dp.NBTData.Add("ench", enchList);
                SetItem(wb, 1, dp);
            }


            private void SetItem(WriteBuffer wb, int slot, ItemStack item)
            {
                wb.Reset();
                wb.WriteVarInt(0x2F);
                wb.WriteByte(0);
                wb.WriteShort((short)(36 + slot));
                wb.WriteItemStack(item);
                stream.SendPacket(wb);
            }
            private static int FillChunkBuffer(byte[] buf, Chunk c)
            {
                int idx = 0;
                int secCount = 0;
                for (int y = 0; y < 16; y++) {
                    ChunkSection sec;
                    if ((sec = c.Sections[y]) != null) {
                        for (int i = 0; i < 4096; i++) {
                            int block = sec.Blocks[i] << 4;
                            byte meta = sec.Metadata[i / 2];
                            block |= i % 2 == 0 ? meta & 0x0F : (meta >> 4) & 0x0F;

                            buf[idx++] = (byte)block;
                            buf[idx++] = (byte)(block >> 8);
                        }
                        secCount++;
                    }
                }
                for (int j = 0; j < secCount; j++) {
                    for (int i = 0; i < 4096; i++) {//2048+2048=blocklight+skylight
                        buf[idx + i] = 0xFF;
                    }
                    idx += 4096;
                }
                for (int i = 0; i < 256; i++) { //biomes
                    buf[idx + i] = 1;//Plains
                }
                idx += 256;

                return idx;
            }
            private static int CalcChunkSize(int sectionCount, bool hasSkyLight, bool hasBiomes)
            {
                return sectionCount * (8192 + 2048 + (hasSkyLight ? 2048 : 0)) + (hasBiomes ? 256 : 0);
            }
        }
    }
}
