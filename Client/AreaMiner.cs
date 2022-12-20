using AdvancedBot.client.Map;
using AdvancedBot.client.Packets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedBot.client
{
    public class AreaMiner
    {
        public MinecraftClient Client;
        public Entity Player { get { return Client.Player; } }
        public World World { get { return Client.World; } }

        public bool IsMining = false;

        public Vec3i Min { get; set; }
        public Vec3i Max { get; set; }

        public AreaMiner(MinecraftClient c)
        {
            Client = c;
        }

        private MiningStatus status = MiningStatus.Finished;

        private HitResult digPos = new HitResult(0, -1, 0, -1);
        private long digStart = Utils.GetTimestamp();
        private float digSum = 0.0f;

        private Vec3i currentBlock = new Vec3i(0, -1, 0);

        private static ConcurrentDictionary<long, byte> DiggingBlocks => AutoMiner.DiggingBlocks;
        private static ConcurrentDictionary<long, ushort> digTries = new ConcurrentDictionary<long, ushort>();

        private Queue<TimedMsg> timedMsgs = new Queue<TimedMsg>();
        private class TimedMsg
        {
            public string Msg;
            public int DelayMs;

            public TimedMsg(string msg, int delay)
            {
                Msg = msg;
                DelayMs = delay;
            }
        }

        public void StartMining()
        {
            IsMining = true;
        }
        public void StopMining()
        {
            Player.MoveQueue.Clear();
            IsMining = false;
            status = MiningStatus.Finished;

            //clear to prevent 'deadlocked' blocks
            DiggingBlocks.Clear();
            digTries.Clear();
        }

        public void Tick()
        {
            if (timedMsgs.Count > 0) {
                for (TimedMsg tm; timedMsgs.Count > 0 && ((tm = timedMsgs.Peek()).DelayMs -= 50) <= 5;) {
                    //Debug.WriteLine(tm.DelayMs + " | " + " | " + (DateTime.Now - tm.dt).TotalMilliseconds);
                    timedMsgs.Dequeue();
                    Client.SendMessage(tm.Msg);
                }
            }
            if (!IsMining) return;

            string s = Program.Config.GetString("MinerCmdsInvFull");
            bool stop = Program.Config.GetBoolean("MinerStopInvFull");
            if (stop || !string.IsNullOrEmpty(s)) {
                int nFree = 0;
                for (int i = 9; i < 45; i++) {
                    ItemStack stack = Client.Inventory.Slots[i];
                    if (stack == null) nFree++;
                }
                if (nFree == 0) {
                    Client.PrintToChat("§cParando o minerador porque o inventário está cheio.");
                    if (stop) StopMining();

                    int prevDelay = -1;
                    foreach (string ln in s.Lines()) {
                        if (ln.StartsWith("wait(") && ln.EndsWith(")")) {
                            if (!int.TryParse(ln.Substring(5, ln.Length - 6), out prevDelay))
                                prevDelay = -1;
                        } else {
                            timedMsgs.Enqueue(new TimedMsg(ln, prevDelay));
                            prevDelay = -1;
                        }
                        //Client.SendMessage(ln);
                    }
                    return;
                }
            }
            if (status == MiningStatus.Breaking && Utils.GetTimestamp() - digStart > 15000) {
                Debug.WriteLine("Digging timeout");
                Client.SendPacket(new PacketPlayerDigging(DiggingStatus.CancelledDigging, digPos));
                EndDestroyBlock();
                status = MiningStatus.Finished;
            } else if (status == MiningStatus.Breaking) {
                Player.LookToBlock(digPos.X, digPos.Y, digPos.Z, false);
                Client.SendPacket(new PacketSwingArm(Client.PlayerID));
                digSum += DiggingHelper.StrengthVsBlock(Client, World.GetBlock(digPos.X, digPos.Y, digPos.Z));
                if (digSum >= 1.0f) { //done?
                    Client.SendPacket(new PacketPlayerDigging(DiggingStatus.FinishedDigging, digPos));
                    digSum = 0.0f;
                    EndDestroyBlock();
                    status = MiningStatus.Finished;
                    return;
                }
            }

            if (status == MiningStatus.Finished) {
                //break any block the player collides with
                int pMinY = (int)Player.AABB.MinY;
                foreach (AABB bb in GetCollisionBoxes()) {
                    int bx = (int)bb.MinX;
                    int by = (int)bb.MinY;
                    int bz = (int)bb.MinZ;
                    if (by >= pMinY && by <= pMinY + 1) {
                        int id = World.GetBlock(bx, by, bz);
                        if (id == Blocks.bedrock || IsDigging(bx, by, bz) || !IsInRange(bx, by, bz)) {
                            continue;
                        }
                        Player.LookToBlock(bx, by, bz, false);
                        BreakHitBlock();
                        return;
                    }
                }

                SearchNearestBlock();
            }
        }

        private void SelectBestTool(int blockId)
        {
            float bestSpeed = 1.0f;
            int bestSlot = -1;

            Block block = Blocks.GetBlock(blockId);
            for (int i = 0; i < 9; i++) {
                ItemStack item = Client.Inventory.Slots[36 + i];

                if (item != null) {
                    float speed = DiggingHelper.ToolStrengthVsBlock(item, block);
                    if (!DiggingHelper.CanHarvestBlock(item, block))
                        speed *= 0.2f;

                    if (speed > bestSpeed) {
                        bestSpeed = speed;
                        bestSlot = i;
                    }
                }
            }
            if (bestSlot != -1) {
                Client.HotbarSlot = bestSlot;
            }
        }

        private bool BreakHitBlock()
        {
            HitResult hit = Player.RayCastBlocks(6);

            if (hit != null) {
                BreakBlock(hit);
                return true;
            }
            return false;
        }

        private void BreakBlock(HitResult hit)
        {
            if (Program.Config.GetBoolOrTrue("MinerSelectBestTool")) {
                SelectBestTool(World.GetBlock(hit.X, hit.Y, hit.Z));
            }
            // Debug.WriteLine("Start digging: " + hit.X + " " + hit.Y + " " + hit.Z);
            SetDigging(hit.X, hit.Y, hit.Z, true, false);
            Client.SendPacket(new PacketSwingArm(Client.PlayerID));
            Client.SendPacket(new PacketPlayerDigging(DiggingStatus.StartedDigging, hit));
            digPos = hit;
            status = MiningStatus.Breaking;
            digStart = Utils.GetTimestamp();
            digSum = 0f;
        }

        private void EndDestroyBlock()
        {
            SetDigging(digPos.X, digPos.Y, digPos.Z, false, true);
            if (currentBlock.Y != -1) {
                SetDigging(currentBlock.X, currentBlock.Y, currentBlock.Z, false, false);
                currentBlock.Y = -1;
            }
        }

        // Predicts blocks the player will collide with on the next tick
        private IEnumerable<AABB> GetCollisionBoxes()
        {
            double sl = 0.91;
            if (Player.OnGround) {
                byte id = World.GetBlock(Utils.Floor(Player.PosX), Utils.Floor(Player.AABB.MinY) - 1, Utils.Floor(Player.PosZ));
                sl = (id == Blocks.ice || id == Blocks.packed_ice ? 0.98 : 0.6) * 0.91;
            }

            double speed = Player.OnGround ? Player.GetMoveSpeed() * (0.16277136 / (sl * sl * sl)) : 0.02;

            double xa = 0, za = 1;
            double dist = xa * xa + za * za;

            double mX = 0, mZ = 0;
            if (dist >= 0.0001) {
                dist = Math.Sqrt(dist);

                if (dist < 1.0)
                    dist = 1.0;

                dist = speed / dist;
                xa *= dist;
                za *= dist;
                double sin = Math.Sin(Player.Yaw * Math.PI / 180.0);
                double cos = Math.Cos(Player.Yaw * Math.PI / 180.0);
                mX = xa * cos - za * sin;
                mZ = za * cos + xa * sin;
            }
            AABB pBB = Player.AABB.Expand(mX, 0, mZ);
            List<AABB> bbs = World.GetCollisionBoxes(pBB);
            return bbs.Where(bb => pBB.Intersects(bb));
        }

        private void SearchNearestBlock()
        {
            int px = Utils.Floor(Player.PosX);
            int py = Utils.Floor(Player.AABB.MinY);
            int pz = Utils.Floor(Player.PosZ);
            
            BlockInfo best = null;
            double smallestCost = double.PositiveInfinity;

            var lbg = new LinearBlockGetter(World);
            const int radius = 6;

            int xMin = Math.Max(Min.X, px - radius);
            int zMin = Math.Max(Min.Z, pz - radius);
            int xMax = Math.Min(Max.X, px + radius);
            int zMax = Math.Min(Max.Z, pz + radius);

            int yMin = Math.Max(Min.Y, py - 1);
            int yMax = Math.Min(Max.Y, py + 4);

            for (int y = yMax; y >= yMin; y--) {
                for (int z = zMin; z <= zMax; z++) {
                    for (int x = xMin; x <= xMax; x++) {
                        int id = lbg.GetBlock(x, y, z);
                        if (id == Blocks.air || id == Blocks.bedrock) continue; //dont spend time with non minable blocks
                        if (!IsDigging(x, y, z)) {
                            int dx = x - px;
                            int dy = y - py;
                            int dz = z - pz;
                            double cost = Math.Sqrt(dx * dx + dy * dy + dz * dz);
                            if (y == py - 1) {
                                cost = (cost + 1.5) * 16.0;
                            }
                            if (cost < smallestCost) { //is this the best block so far?
                                smallestCost = cost;
                                best = new BlockInfo(x, y, z, id, cost);
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < 8; i++) {
                Player.MoveQueue.Enqueue(Movement.Forward);
            }
            if (best == null) {
                //Debug.WriteLine("Not found any block.");
                return;
            }

            Player.LookToBlock(best.X, best.Y, best.Z, false);
            HitResult hit = Player.RayCastBlocks(6);

            if (hit != null) {
                currentBlock = new Vec3i(best.X, best.Y, best.Z);
                SetDigging(best.X, best.Y, best.Z, true, false);
                Player.LookToBlock(hit.X, hit.Y, hit.Z, false);
                BreakBlock(hit);
            }
        }

        private static void SetDigging(int x, int y, int z, bool dig, bool incrTries)
        {
            long key = (x & 0x3FFFFFFL) << 38 | (y & 0xFFFL) << 26 | (z & 0x3FFFFFFL);
            if (dig) {
                DiggingBlocks[key] = 1;
            } else {
                DiggingBlocks.TryRemove(key, out _);
                if (incrTries) {
                    digTries.AddOrUpdate(key, 1, (k, oldVal) => (ushort)(oldVal + 1));
                }
            }
        }

        // Checks if the block at specified coordinate is being mined by another bot
        private static bool IsDigging(int x, int y, int z)
        {
            long key = (x & 0x3FFFFFFL) << 38 | (y & 0xFFFL) << 26 | (z & 0x3FFFFFFL);
            return DiggingBlocks.ContainsKey(key) || (digTries.TryGetValue(key, out var tries) && tries > 4);
        }
        private static int GetPriority(int[] ores, int id)
        {
            for (int i = 0; i < ores.Length; i++) {
                if ((ores[i] & 0xFFFF) == id) {
                    return ores[i] >> 16 & 0xFFFF;
                }
            }
            return -1;
        }

        private bool IsInRange(int x, int y, int z)
        {
            return (x > Min.X && y > Min.Y && z > Min.Z) &&
                   (x < Max.X && y < Max.Y && z < Max.Z);
        }

        private enum MiningStatus
        {
            Breaking, Finished
        }
        private class BlockInfo
        {
            public int X, Y, Z, ID;
            public double Cost;
            public BlockInfo(int x, int y, int z, int id, double pr)
            {
                X = x;
                Y = y;
                Z = z;
                ID = id;
                Cost = pr;
            }
        }
    }
}
