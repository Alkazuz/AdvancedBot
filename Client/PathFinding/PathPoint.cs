using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedBot.client.PathFinding
{
    public class PathPoint
    {
        public readonly int X;
        public readonly int Y;
        public readonly int Z;

        private readonly int hash;
        public int index = -1;
        public float TotalPathDistance;
        public float DistanceToNext;
        public float DistanceToTarget;
        public PathPoint Previous;
        public bool IsFirst;

        public PathPoint(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
            hash = MakeHash(x, y, z);
        }

        public static int MakeHash(int x, int y, int z)
        {
            return y & 0xFF |
                  (x & 0x7FFF) << 8 |
                  (z & 0x7FFF) << 24 | 
                  (x < 0 ? int.MinValue : 0) | 
                  (z < 0 ? 0x8000 : 0);
        }
        public float DistanceEuclidean(PathPoint point)
        {
            int x = point.X - X;
            int y = point.Y - Y;
            int z = point.Z - Z;
            return (float)Math.Sqrt(x * x + y * y + z * z);
        }
        public float DistanceManhattan(PathPoint point)
        {
            int x = point.X - X;
            int y = (point.Y - Y) * 2;
            int z = point.Z - Z;
            return Math.Abs(x) + Math.Abs(y) + Math.Abs(z);
        }

        public override bool Equals(object other)
        {
            if (!(other is PathPoint))
                return false;
            else
            {
                PathPoint pt = (PathPoint)other;
                return hash == pt.hash && 
                       X == pt.X && 
                       Y == pt.Y && 
                       Z == pt.Z;
            }
        }

        public override int GetHashCode()
        {
            return hash;
        }

        public bool IsAssigned()
        {
            return index >= 0;
        }

        public override string ToString()
        {
            return X + ", " + Y + ", " + Z;
        }
    }
}