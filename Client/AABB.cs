using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdvancedBot.client.Map;

namespace AdvancedBot.client
{
    public class AABB
    {
        public double MinX;
        public double MinY;
        public double MinZ;
        public double MaxX;
        public double MaxY;
        public double MaxZ;

        public AABB(double minX, double minY, double minZ, double maxX, double maxY, double maxZ)
        {
            MinX = minX;
            MinY = minY;
            MinZ = minZ;
            MaxX = maxX;
            MaxY = maxY;
            MaxZ = maxZ;
        }
        public AABB(double playerX, double playerY, double playerZ)
        {
            const double width = 0.3;
            const double height = 1.8;

            MinX = playerX - width;
            MinY = playerY;
            MinZ = playerZ - width;
            MaxX = playerX + width;
            MaxY = playerY + height;
            MaxZ = playerZ + width;
        }

        public AABB Expand(double xa, double ya, double za)
        {
            double minX = MinX;
            double minY = MinY;
            double minZ = MinZ;
            double maxX = MaxX;
            double maxY = MaxY;
            double maxZ = MaxZ;

            if (xa < 0.0) minX += xa;
            if (xa > 0.0) maxX += xa;
            if (ya < 0.0) minY += ya;
            if (ya > 0.0) maxY += ya;
            if (za < 0.0) minZ += za;
            if (za > 0.0) maxZ += za;

            return new AABB(minX, minY, minZ, maxX, maxY, maxZ);
        }
        public AABB Grow(double xa, double ya, double za)
        {
            return new AABB(MinX - xa, MinY - ya, MinZ - za,
                            MaxX + xa, MaxY + ya, MaxZ + za);
        }
        public void Move(double xa, double ya, double za)
        {
            MinX += xa;
            MinY += ya;
            MinZ += za;
            MaxX += xa;
            MaxY += ya;
            MaxZ += za;
        }
        public AABB MoveAndGet(double xa, double ya, double za)
        {
            MinX += xa;
            MinY += ya;
            MinZ += za;
            MaxX += xa;
            MaxY += ya;
            MaxZ += za;
            return this;
        }
        public AABB MoveClone(double xa, double ya, double za)
        {
            return new AABB(MinX + xa, MinY + ya, MinZ + za, MaxX + xa, MaxY + ya, MaxZ + za);
        }
        public void Set(double minX, double minY, double minZ, double maxX, double maxY, double maxZ)
        {
            MinX = minX;
            MaxX = maxX;
            MinY = minY;
            MaxY = maxY;
            MinZ = minZ;
            MaxZ = maxZ;
        }

        public double ClipXCollide(AABB c, double xa)
        {
            if (c.MaxY <= MinY || c.MinY >= MaxY) return xa;
            if (c.MaxZ <= MinZ || c.MinZ >= MaxZ) return xa;

            if (xa > 0.0 && c.MaxX <= MinX) {
                double max = MinX - c.MaxX;
                if (max < xa)
                    xa = max;
            }
            if (xa < 0.0 && c.MinX >= MaxX) {
                double max = MaxX - c.MinX;
                if (max > xa)
                    xa = max;
            }
            return xa;
        }
        public double ClipYCollide(AABB c, double ya)
        {
            if (c.MaxX <= MinX || c.MinX >= MaxX) return ya;
            if (c.MaxZ <= MinZ || c.MinZ >= MaxZ) return ya;

            if (ya > 0.0 && c.MaxY <= MinY) {
                double max = MinY - c.MaxY;
                if (max < ya)
                    ya = max;
            }
            if (ya < 0.0 && c.MinY >= MaxY) {
                double max = MaxY - c.MinY;
                if (max > ya)
                    ya = max;
            }
            return ya;
        }
        public double ClipZCollide(AABB c, double za)
        {
            if (c.MaxX <= MinX || c.MinX >= MaxX) return za;
            if (c.MaxY <= MinY || c.MinY >= MaxY) return za;

            if (za > 0.0 && c.MaxZ <= MinZ) {
                double max = MinZ - c.MaxZ;
                if (max < za)
                    za = max;
            }
            if (za < 0.0 && c.MinZ >= MaxZ) {
                double max = MaxZ - c.MinZ;
                if (max > za)
                    za = max;
            }
            return za;
        }

        public bool Intersects(AABB c)
        {
            return c.MaxX > MinX && c.MinX < MaxX &&
                   c.MaxY > MinY && c.MinY < MaxY &&
                   c.MaxZ > MinZ && c.MinZ < MaxZ;
        }

        public override bool Equals(object obj)
        {
            if (obj != null && obj is AABB)
            {
                AABB aabb = (AABB)obj;
                return aabb.MinX == MinX &&
                       aabb.MinY == MinY &&
                       aabb.MinZ == MinZ &&
                       aabb.MaxX == MaxX &&
                       aabb.MaxY == MaxY &&
                       aabb.MaxZ == MaxZ;
            }
            return false;
        }
        public unsafe override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + MinX.GetHashCode();
                hash = hash * 31 + MinY.GetHashCode();
                hash = hash * 31 + MinZ.GetHashCode();
                hash = hash * 31 + MaxX.GetHashCode();
                hash = hash * 31 + MaxY.GetHashCode();
                hash = hash * 31 + MaxZ.GetHashCode();
                return hash;
            }
        }
        public override string ToString()
        {
            return string.Format("AABB[{0:0.00} {1:0.00} {2:0.00} -> {3:0.00} {4:0.00} {5:0.00}]", MinX, MinY, MinZ, MaxX, MaxY, MaxZ);
        }

        private bool isVecInYZ(Vec3d vec)
        {
            return vec == null ? false : vec.Y >= MinY && vec.Y <= MaxY && vec.Z >= MinZ && vec.Z <= MaxZ;
        }
        private bool isVecInXZ(Vec3d vec)
        {
            return vec == null ? false : vec.X >= MinX && vec.X <= MaxX && vec.Z >= MinZ && vec.Z <= MaxZ;
        }
        private bool isVecInXY(Vec3d vec)
        {
            return vec == null ? false : vec.X >= MinX && vec.X <= MaxX && vec.Y >= MinY && vec.Y <= MaxY;
        }
        public HitResult CalculateIntercept(Vec3d start, Vec3d end)
        {
            Vec3d xiMin = start.LerpWithX(end, MinX);
            Vec3d xiMax = start.LerpWithX(end, MaxX);
            Vec3d yiMin = start.LerpWithY(end, MinY);
            Vec3d yiMax = start.LerpWithY(end, MaxY);
            Vec3d ziMin = start.LerpWithZ(end, MinZ);
            Vec3d ziMax = start.LerpWithZ(end, MaxZ);

            if (!isVecInYZ(xiMin)) xiMin = null;
            if (!isVecInYZ(xiMax)) xiMax = null;
            if (!isVecInXZ(yiMin)) yiMin = null;
            if (!isVecInXZ(yiMax)) yiMax = null;
            if (!isVecInXY(ziMin)) ziMin = null;
            if (!isVecInXY(ziMax)) ziMax = null;

            Vec3d result = null;

            if (xiMin != null) result = xiMin;

            if (xiMax != null && (result == null || start.DistanceSq(xiMax) < start.DistanceSq(result))) result = xiMax;
            if (yiMin != null && (result == null || start.DistanceSq(yiMin) < start.DistanceSq(result))) result = yiMin;
            if (yiMax != null && (result == null || start.DistanceSq(yiMax) < start.DistanceSq(result))) result = yiMax;
            if (ziMin != null && (result == null || start.DistanceSq(ziMin) < start.DistanceSq(result))) result = ziMin;
            if (ziMax != null && (result == null || start.DistanceSq(ziMax) < start.DistanceSq(result))) result = ziMax;

            if (result == null)
                return null;
            else {
                byte face = 0;

                if (result == yiMin) face = 0;
                if (result == yiMax) face = 1;
                if (result == ziMin) face = 2;
                if (result == ziMax) face = 3;
                if (result == xiMin) face = 4;
                if (result == xiMax) face = 5;

                return new HitResult(result, face);
            }
        }
        
        public AABB Copy()
        {
            return new AABB(MinX, MinY, MinZ, MaxX, MaxY, MaxZ);
        }
    }
}
