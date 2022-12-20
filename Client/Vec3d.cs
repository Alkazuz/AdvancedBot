using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedBot.client
{
    public class Vec3d
    {
        public double X, Y, Z;

        public Vec3d(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double DistanceSq(Vec3d location)
        {
            double dx = X - location.X;
            double dy = Y - location.Y;
            double dz = Z - location.Z;
            return dx * dx + dy * dy + dz * dz;
        }
        public double Distance(Vec3d location)
        {
            return Math.Sqrt(DistanceSq(location));
        }

        public double Length()
        {
            return Math.Sqrt(X * X + Y * Y + Z * Z);
        }
        public void Normalize()
        {
            double d = Math.Sqrt(X * X + Y * Y + Z * Z);
            if (d < 0.0001D)
            {
                X = 0;
                Y = 0;
                Z = 0;
            } else {
                X /= d;
                Y /= d;
                Z /= d;
            }
        }
        public Vec3d Add(double xa, double ya, double za)
        {
            X += xa;
            Y += ya;
            Z += za;
            return this;
        }
        public Vec3d Add(Vec3d v)
        {
            X += v.X;
            Y += v.Y;
            Z += v.Z;
            return this;
        }
        public Vec3d Mul(double m)
        {
            X *= m;
            Y *= m;
            Z *= m;
            return this;
        }

        public static bool operator ==(Vec3d loc1, Vec3d loc2)
        {
            return (object)loc1 == null ? (object)loc2 == null : loc1.Equals(loc2);
        }
        public static bool operator !=(Vec3d loc1, Vec3d loc2)
        {
            return !((object)loc1 == null ? (object)loc2 == null : loc1.Equals(loc2));
        }
        public static Vec3d operator +(Vec3d loc1, Vec3d loc2)
        {
            return new Vec3d(loc1.X + loc2.X, loc1.Y + loc2.Y, loc1.Z + loc2.Z);
        }
        public static Vec3d operator -(Vec3d loc1, Vec3d loc2)
        {
            return new Vec3d(loc1.X - loc2.X, loc1.Y - loc2.Y, loc1.Z - loc2.Z);
        }

        public Vec3d LerpWithX(Vec3d vec, double x)
        {
            double dx = vec.X - X;
            double dy = vec.Y - Y;
            double dz = vec.Z - Z;
            if (dx * dx < 0.0000001)
                return null;

            double n = (x - X) / dx;
            return n < 0.0 || n > 1.0 ? null : new Vec3d(X + dx * n, Y + dy * n, Z + dz * n);
        }
        public Vec3d LerpWithY(Vec3d vec, double y)
        {
            double dx = vec.X - X;
            double dy = vec.Y - Y;
            double dz = vec.Z - Z;
            if (dy * dy < 0.0000001)
                return null;

            double n = (y - Y) / dy;
            return n < 0.0 || n > 1.0 ? null : new Vec3d(X + dx * n, Y + dy * n, Z + dz * n);
        }
        public Vec3d LerpWithZ(Vec3d vec, double x)
        {
            double dx = vec.X - X;
            double dy = vec.Y - Y;
            double dz = vec.Z - Z;
            if (dz * dz < 0.0000001)
                return null;

            double n = (x - Z) / dz;
            return n < 0.0 || n > 1.0 ? null : new Vec3d(X + dx * n, Y + dy * n, Z + dz * n);
        }

        public override bool Equals(object obj)
        {
            if (obj is Vec3d ot) {
                return X == ot.X &&
                       Y == ot.Y &&
                       Z == ot.Z;
            }
            return false;
        }
        public override int GetHashCode()
        {
            unchecked {
                int hash = 17;
                hash = hash * 31 + X.GetHashCode();
                hash = hash * 31 + Y.GetHashCode();
                hash = hash * 31 + Z.GetHashCode();
                return hash;
            }
        }
        public override string ToString()
        {
            return String.Format("vec3(X:{0:0.00} Y:{1:0.00} Z:{2:0.00})", X, Y, Z);
        }
    }
}
