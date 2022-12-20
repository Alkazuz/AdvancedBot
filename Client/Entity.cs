using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using AdvancedBot.client.Map;
using AdvancedBot.Properties;
using Newtonsoft.Json.Linq;

namespace AdvancedBot.client
{
    public class Entity
    {
        public AABB AABB;
        //PosY = AABB.MinY + 1.62
        public double PosX, PosY, PosZ;
        
        public int BlockX { get { return Utils.Floor(PosX); } }
        public int BlockY { get { return Utils.Floor(AABB.MinY); } }
        public int BlockZ { get { return Utils.Floor(PosZ); } }

        public double MotionX, MotionY, MotionZ;
        public bool OnGround;
        
        public float Yaw, Pitch;
        public bool IsCollidedVertically;
        public bool IsCollidedHorizontally;

        public double OldX, OldY, OldZ;
        public float OldYaw, OldPitch;
        private int posTicks = 0;

        public int PortalCentralizeTicks = 0;

        //public bool JumpOnCollide = true;

        private const double width = 0.3;
        private const double height = 1.8;

        private double stepHeight = 0.5;

        public readonly World World;
        public bool IsSprinting = false;
        public bool WasSprinting = false;

        internal bool LockMoveQueue = false; //used by viewer

        public bool IsPositionChanged;
        public bool IsRotationChanged;

        public Dictionary<byte, byte> ActivePotions = new Dictionary<byte, byte>();

        public Entity(MinecraftClient c)
        {
            World = c.World;
            SetPosition(0, 0, 0);
        }

        public Queue<Movement> MoveQueue = new Queue<Movement>();
        private Movement DequeueMove()
        {
            if (LockMoveQueue) Monitor.Enter(MoveQueue);
            try {
                return MoveQueue.Count > 0 ? MoveQueue.Dequeue() : Movement.None;
            } finally {
                if (LockMoveQueue) Monitor.Exit(MoveQueue);
            }
        }
        
        public void Tick()
        {
            OldX = PosX;
            OldY = PosY;
            OldZ = PosZ;

            double xa = 0.0;
            double za = 0.0;

            Movement mov = DequeueMove();
            
            if (mov != Movement.None) {
                if ((mov & Movement.Forward) != 0) za++;
                if ((mov & Movement.Back)    != 0) za--;
                if ((mov & Movement.Left)    != 0) xa++;
                if ((mov & Movement.Right)   != 0) xa--;

                if ((mov & Movement.Jump)    != 0) {
                    if (IsInWater() || IsInLava()) {
                        MotionY += 0.04;
                    } else if (OnGround) {
                        MotionY = 0.42;
                        if (ActivePotions.TryGetValue(8, out byte amp))
                            MotionY += (amp + 1) * 0.1;

                        if (IsSprinting) {
                            double xRot = Yaw * (Math.PI / 180.0);
                            MotionX -= Math.Sin(xRot) * 0.2;
                            MotionZ += Math.Cos(xRot) * 0.2;
                        }
                    }
                }
            }

            if (Math.Abs(MotionX) < 0.005) MotionX = 0.0;
            if (Math.Abs(MotionY) < 0.005) MotionY = 0.0;
            if (Math.Abs(MotionZ) < 0.005) MotionZ = 0.0;

            MoveWithHeading(xa * 0.98, za * 0.98);

            double dx = OldX - PosX;
            double dy = OldY - PosY;
            double dz = OldZ - PosZ;
            if (dx * dx + dy * dy + dz * dz > 0.0009 || posTicks++ >= 20) {
                posTicks = 0;
                IsPositionChanged = true;
            } else {
                IsPositionChanged = false;
            }
            IsRotationChanged = Yaw != OldYaw || Pitch != OldPitch;
            OldYaw = Yaw;
            OldPitch = Pitch;
        }
        public double JumpMoveSpeed = 0.02;
        public double GetMoveSpeed()
        {
            double speed = 0.1;

            if (IsSprinting) speed *= 1.3;

            byte spot;
            if (ActivePotions.TryGetValue(1, out spot)) speed *= 1.0 + (0.2 * (spot + 1));//speed
            if (ActivePotions.TryGetValue(2, out spot)) speed *= 1.0 + (-0.15 * (spot + 1));//slowdown

            return speed < 0 ? 0 : speed;
        }

        private bool IsOffsetPositionInLiquid(double x, double y, double z)
        {
            AABB bb = AABB.MoveClone(x, y, z);
            return World.GetCollisionBoxes(bb).Count == 0 && !World.IsAnyBlockInBB(bb, 8, 9, 10, 11);
        }

        public void SetPosition(double x, double y, double z)
        {
            PosX = x;
            PosY = y + 1.62;
            PosZ = z;
            AABB = new AABB(x - width, y, z - width,
                            x + width, y + height, z + width);
        }

        //void EntityLivingBase.moveEntityWithHeading(float strafe, float forward)
        private void MoveWithHeading(double strafe, double forward)
        {
            if (HandleWaterMovement()) {
                double oldY = PosY;
                MoveRelative(strafe, forward, 0.02);
                Move(MotionX, MotionY, MotionZ);
                MotionX *= 0.8;
                MotionY *= 0.8;
                MotionZ *= 0.8;
                MotionY -= 0.02;
                
                if (IsCollidedHorizontally && IsOffsetPositionInLiquid(MotionX, MotionY + 1.0 - PosY + oldY, MotionZ))
                    MotionY = 0.3;
            } else if (IsInLava()) {
                double oldY = PosY;
                MoveRelative(strafe, forward, 0.02);
                Move(MotionX, MotionY, MotionZ);
                MotionX *= 0.5;
                MotionY *= 0.5;
                MotionZ *= 0.5;
                MotionY -= 0.02;

                if (IsCollidedHorizontally && IsOffsetPositionInLiquid(MotionX, MotionY + 0.6 - PosY + oldY, MotionZ))
                    MotionY = 0.3;
            } else {
                double sl = 0.91;

                if (OnGround) {
                    byte id = World.GetBlock(Utils.Floor(PosX), Utils.Floor(AABB.MinY) - 1, Utils.Floor(PosZ));
                    sl = (id == Blocks.ice || id == Blocks.packed_ice ? 0.98 : 0.6) * 0.91;
                }

                double speed = OnGround ? GetMoveSpeed() * (0.16277136 / (sl * sl * sl)) :
                                         JumpMoveSpeed;
                MoveRelative(strafe, forward, speed);
                sl = 0.91;

                if (OnGround) {
                    byte id = World.GetBlock(Utils.Floor(PosX), Utils.Floor(AABB.MinY) - 1, Utils.Floor(PosZ));
                    sl = (id == Blocks.ice || id == Blocks.packed_ice ? 0.98 : 0.6) * 0.91;
                }

                if (IsOnLadder()) {
                    MotionX = Utils.Clamp(MotionX, -0.15, 0.15);
                    MotionZ = Utils.Clamp(MotionZ, -0.15, 0.15);
                    
                    if (MotionY < -0.15)
                        MotionY = -0.15;
                    
                    /*if (IsSneaking() && MotionY < 0.0)
                        MotionY = 0.0;*/
                }

                Move(MotionX, MotionY, MotionZ);

                if (IsCollidedHorizontally && IsOnLadder())
                    MotionY = 0.2;

                MotionY -= 0.08;

                MotionY *= 0.98;
                MotionX *= sl;
                MotionZ *= sl;
            }
        }

        //void Entity.moveEntity(double x, double y, double z)
        private void Move(double xa, double ya, double za)
        {
            if (IsOnWeb()) {
                xa *= 0.25;
                ya *= 0.05;
                za *= 0.25;
                MotionX = 0;
                MotionY = 0;
                MotionZ = 0;
            }

            double xaOrg = xa;
            double yaOrg = ya;
            double zaOrg = za;
            AABB aabbOrg = AABB.Copy();
            List<AABB> AABBs = World.GetCollisionBoxes(AABB.Expand(xa, ya, za));

            foreach (AABB bb in AABBs) ya = bb.ClipYCollide(AABB, ya); AABB.Move(0.0, ya, 0.0);

            bool ground = OnGround || yaOrg != ya && yaOrg < 0.0;

            foreach (AABB bb in AABBs) xa = bb.ClipXCollide(AABB, xa); AABB.Move(xa, 0.0, 0.0);
            foreach (AABB bb in AABBs) za = bb.ClipZCollide(AABB, za); AABB.Move(0.0, 0.0, za);

            if (ground && (xaOrg != xa || zaOrg != za)) {
                double xa_ = xa;
                double ya_ = ya;
                double za_ = za;
                xa = xaOrg;
                ya = stepHeight;
                za = zaOrg;
                AABB aabb_ = AABB.Copy();
                AABB = aabbOrg.Copy();
                AABBs = World.GetCollisionBoxes(AABB.Expand(xaOrg, ya, zaOrg));

                //for (int i = 0; i < AABBs.Count; i++) ya = AABBs[i].ClipYCollide(AABB, ya); AABB.Move(0.0, ya, 0.0);
                //for (int i = 0; i < AABBs.Count; i++) xa = AABBs[i].ClipXCollide(AABB, xa); AABB.Move(xa, 0.0, 0.0);
                //for (int i = 0; i < AABBs.Count; i++) za = AABBs[i].ClipZCollide(AABB, za); AABB.Move(0.0, 0.0, za);

                foreach (AABB bb in AABBs) ya = bb.ClipYCollide(AABB, ya); AABB.Move(0.0, ya, 0.0);
                foreach (AABB bb in AABBs) xa = bb.ClipXCollide(AABB, xa); AABB.Move(xa, 0.0, 0.0);
                foreach (AABB bb in AABBs) za = bb.ClipZCollide(AABB, za); AABB.Move(0.0, 0.0, za);

                ya = -stepHeight;

                for (int i = 0; i < AABBs.Count; i++)
                    ya = AABBs[i].ClipYCollide(AABB, ya);
                AABB.Move(0.0, ya, 0.0);

                if (xa_ * xa_ + za_ * za_ >= xa * xa + za * za) {
                    xa = xa_;
                    ya = ya_;
                    za = za_;
                    AABB = aabb_.Copy();
                }
            }

            IsCollidedHorizontally = xaOrg != xa || zaOrg != za;
            IsCollidedVertically = yaOrg != ya;
            OnGround = yaOrg != ya && yaOrg < 0.0;

            if (xaOrg != xa) MotionX = 0;
            if (yaOrg != ya) MotionY = 0;
            if (zaOrg != za) MotionZ = 0;

            PosX = (AABB.MinX + AABB.MaxX) / 2.0;
            PosY = AABB.MinY + 1.62;
            PosZ = (AABB.MinZ + AABB.MaxZ) / 2.0;
        }
        public void MoveRelative(double xa, double za, double speed)
        {
            double dist = xa * xa + za * za;

            if (dist >= 0.0001) {
                dist = Math.Sqrt(dist);

                if (dist < 1.0)
                    dist = 1.0;

                dist = speed / dist;
                xa *= dist;
                za *= dist;
                double sin = Math.Sin(Yaw * Math.PI / 180.0);
                double cos = Math.Cos(Yaw * Math.PI / 180.0);
                MotionX += xa * cos - za * sin;
                MotionZ += za * cos + xa * sin;
            }
        }

        public bool IsOnLadder()
        {
            int x = Utils.Floor(PosX);
            int y = Utils.Floor(AABB.MinY);
            int z = Utils.Floor(PosZ);
            byte block = World.GetBlock(x, y, z);
            return block == Blocks.ladder || block == Blocks.vine;
        }
        public bool IsInLava()
        {
            return World.IsAnyBlockInBB(AABB.Grow(-0.1, -0.4, -0.1), Blocks.lava, Blocks.flowing_lava);
        }
        public bool IsInWater()
        {
            return World.IsAnyBlockInBB(AABB.Grow(-0.1, -0.4, -0.1), Blocks.water, Blocks.flowing_water);
        }
        public bool IsOnPortal()
        {
            return World.IsAnyBlockInBB(AABB, Blocks.portal);
        }
        public IEnumerable<Tuple<Vec3i, byte>> GetCollidingBlocks(params int[] ids)
        {
            var bb = AABB;
            int minX = Utils.Floor(bb.MinX);
            int maxX = Utils.Floor(bb.MaxX + 1.0);
            int minY = Utils.Floor(bb.MinY);
            int maxY = Utils.Floor(bb.MaxY + 1.0);
            int minZ = Utils.Floor(bb.MinZ);
            int maxZ = Utils.Floor(bb.MaxZ + 1.0);

            for (int x = minX; x < maxX; x++) {
                for (int y = minY; y < maxY; y++) {
                    for (int z = minZ; z < maxZ; z++) {
                        byte id = World.GetBlock(x, y, z);
                        if (Array.IndexOf(ids, id) >= 0)
                            yield return new Tuple<Vec3i, byte>(new Vec3i(x, y, z), id);
                    }
                }
            }
        }
        public bool IsOnWeb()
        {
            int minX = Utils.Floor(AABB.MinX);
            int maxX = Utils.Floor(AABB.MaxX);
            int minY = Utils.Floor(AABB.MinY);
            int maxY = Utils.Floor(AABB.MaxY);
            int minZ = Utils.Floor(AABB.MinZ);
            int maxZ = Utils.Floor(AABB.MaxZ);

            for (int x = minX; x <= maxX; x++) {
                for (int y = minY; y <= maxY; y++) {
                    for (int z = minZ; z <= maxZ; z++) {
                        byte id = World.GetBlock(x, y, z);
                        if (id == Blocks.web)
                            return true;
                    }
                }
            }
            return false;
        }
        private bool HandleWaterMovement()
        {
            AABB aabb = AABB.Grow(0, -0.4, 0);

            int minX = Utils.Floor(aabb.MinX);
            int maxX = Utils.Floor(aabb.MaxX + 1.0);
            int minY = Utils.Floor(aabb.MinY);
            int maxY = Utils.Floor(aabb.MaxY + 1.0);
            int minZ = Utils.Floor(aabb.MinZ);
            int maxZ = Utils.Floor(aabb.MaxZ + 1.0);
            Vec3d v = new Vec3d(0.0, 0.0, 0.0);
            bool inWater = false;

            World w = World;
            for (int x = minX; x < maxX; x++) {
                for (int y = minY; y < maxY; y++) {
                    for (int z = minZ; z < maxZ; z++) {
                        byte b = w.GetBlock(x, y, z);
                        if (b != 8 && b != 9) continue;

                        double d1 = (y + 1) - BlockUtils.GetFluidHeightPercent(w.GetData(x, y, z));
                        if (maxY >= d1) {
                            v += w.GetWaterFlowVector(x, y, z);
                            inWater = true;
                        }
                    }
                }
            }

            if (v.Length() > 0.0) {
                v.Normalize();
                double d = 0.014;
                MotionX += v.X * d;
                MotionY += v.Y * d;
                MotionZ += v.Z * d;
            }
            return inWater;
        }

        public bool CanSeePlayer(MPPlayer p)
        {
            Vec3d start = new Vec3d(PosX, PosY, PosZ);
            Vec3d end = new Vec3d(p.X, p.Y + 1.62, p.Z);

            return World.RayCast(start, end, false, true) == null;
        }

        private Random RNG = new Random();
        public void LookToBlock(int x, int y, int z, bool randomize)
        {
            if (randomize) {
                const double F = 0.2;
                const double MID = 0.5 - (F / 2.0);
                double a = MID + (RNG.NextDouble() * F);
                double b = MID + (RNG.NextDouble() * F);
                double c = MID + (RNG.NextDouble() * F);
                LookTo(x + a, y + b, z + c);
            } else {
                LookTo(x + 0.5, y + 0.5, z + 0.5);
            }
        }
        public void LookTo(double x, double y, double z)
        {
            double dx = x - PosX;
            double dz = z - PosZ;
            double dy = y - PosY;

            double rotX = (Math.Atan2(dz, dx) * 180.0 / Math.PI) - 90.0;
            double rotY = -(Math.Atan2(dy, Math.Sqrt(dx * dx + dz * dz)) * 180.0 / Math.PI);
            
            Yaw = (float)rotX;
            Pitch = (float)rotY;
        }

        public double GetDistanceToBlock(int x2, int y2, int z2)
        {
            double x = PosX - (x2 + 0.5);
            double y = AABB.MinY - (y2 + 0.5);
            double z = PosZ - (z2 + 0.5);
            return Math.Sqrt(x * x + y * y + z * z);
        }

        public Vec3d GetLookVector()
        {
            return CalculateLookVector(Yaw - 180.0f, Pitch);
        }

        public bool IsUnderWater()
        {
            int id = World.GetBlock(Utils.Floor(PosX), Utils.Floor(PosY), Utils.Floor(PosZ));
            return id == Blocks.water || id == Blocks.flowing_water;
        }

        public Vec3d CalculateLookVector(float yaw, float pitch)
        {
            double rx = (yaw - 90) * Math.PI / 180.0;
            double ry = -pitch * Math.PI / 180.0;
            double ycos = Math.Cos(ry);

            double dx = ycos * Math.Cos(rx);
            double dy = Math.Sin(ry);
            double dz = ycos * Math.Sin(rx);
            return new Vec3d(dx, dy, dz);
        }

        public HitResult RayCastBlocks(double radius)
        {
            Vec3d lVec = GetLookVector().Mul(radius);
            return World.RayCast(new Vec3d(PosX, PosY, PosZ),
                                 new Vec3d(PosX, PosY, PosZ).Add(lVec), true, false);
        }
    }
    [Flags]
    public enum Movement : byte
    {
        Jump    = 0x01,
        Forward = 0x02,
        Back    = 0x04,
        Left    = 0x08,
        Right   = 0x10,
        None    = 0
    }
}