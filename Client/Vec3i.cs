using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdvancedBot.client.PathFinding;

namespace AdvancedBot.client
{
    public class Vec3i
    {
        public static Vec3i Empty => new Vec3i(0, 0, 0);

        public int X, Y, Z;

        public Vec3i(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double DistanceSquared(Vec3i location)
        {
            return ((X - location.X) * (X - location.X))
                 + ((Y - location.Y) * (Y - location.Y))
                 + ((Z - location.Z) * (Z - location.Z));
        }
        public double Distance(Vec3i location)
        {
            return Math.Sqrt(DistanceSquared(location));
        }

        public Vec3i Add(int x, int y, int z)
        {
            return new Vec3i(X + x, Y + y, Z + z);
        }

        public static bool operator ==(Vec3i loc1, Vec3i loc2)
        {
            return (object)loc1 == null ? (object)loc2 == null : loc1.Equals(loc2);
        }
        public static bool operator !=(Vec3i loc1, Vec3i loc2)
        {
            return !((object)loc1 == null ? (object)loc2 == null : loc1.Equals(loc2));
        }

        public override bool Equals(object obj)
        {
            if (obj is Vec3i) {
                Vec3i ot = (Vec3i)obj;
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
                hash = hash * 31 + X;
                hash = hash * 31 + Y;
                hash = hash * 31 + Z;
                return hash;
            }
        }


        public override string ToString()
        {
            return String.Format("vec3(X:{0} Y:{1} Z:{2})", X, Y, Z);
        }
    }
}
