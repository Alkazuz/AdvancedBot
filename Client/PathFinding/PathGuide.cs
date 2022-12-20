using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedBot.client.PathFinding
{
    public class PathGuide
    {
        private Vec3i[] pathPoints;
        private Entity player;

        private int pathIndex = 0;

        private PathGuide(Vec3i g) { GoalPos = g; }

        public Vec3i[] Points
        {
            get { return pathPoints; }
        }

        public readonly Vec3i GoalPos;
        public static async Task<PathGuide> CreateAsync(Entity p, int x, int y, int z)
        {
            PathGuide pg = new PathGuide(new Vec3i(x, y, z));

            const int MAX_DISTANCE = 80;
            if (Utils.DistTo(Utils.Floor(p.PosX), Utils.Floor(p.AABB.MinY), Utils.Floor(p.PosZ), x, y, z) > MAX_DISTANCE + 8)
                return null;
            var points = await Task.Run(() => new PathFinder(p.World, true, false, true, false).CreatePathTo(p, x + 0.5, y + 0.5, z + 0.5, MAX_DISTANCE));

            if (points != null) {
                Vec3i[] varr = new Vec3i[points.Length];
                for (int i = 0; i < points.Length; i++) {
                    PathPoint pt = points[i];
                    varr[i] = new Vec3i(pt.X, pt.Y, pt.Z);
                }
                pg.pathPoints = varr;
                pg.pathIndex = 1;
            }
            pg.player = p;

            return pg.pathPoints == null ? null : pg;
        }

        public void Tick()
        {
            if (Finished()) {
                return;
            }
            if (player.IsCollidedHorizontally && player.OnGround) {
                player.MotionY = 0.42;
            }

            Vec3i current = pathPoints[pathIndex];
            move(current.X, current.Z);

            double dist = Utils.DistTo(current.X + 0.5, 0, current.Z + 0.5, player.PosX, 0, player.PosZ);

            if (dist <= 0.8) {
                pathIndex++;
            } else if(dist > 256) {
                pathIndex = pathPoints.Length;
            } else if (dist >= 2.5) {
                pathIndex = getClosestIndex(player.PosX, player.PosY, player.PosZ);
            }
        }

        private static readonly double RAD90 = 90 * (Math.PI / 180.0);
        private static readonly double SQRT2 = Math.Sqrt(2.0);
        private void move(int x, int z)
        {
            double sl = 0.91;
            if (player.OnGround) {
                int id = player.World.GetBlock(Utils.Floor(player.PosX),
                                               Utils.Floor(player.AABB.MinY) - 1,
                                               Utils.Floor(player.PosZ));
                sl = (id == Blocks.ice || id == Blocks.packed_ice ? 0.98 : 0.6) * 0.91;
            }

            double ang = Math.Atan2((z + 0.5) - player.PosZ, (x + 0.5) - player.PosX) - RAD90;
            double m = (player.OnGround ? player.GetMoveSpeed() * (0.16277136 / (sl * sl * sl)) : 0.02) / SQRT2;

            player.MotionX -= m * Math.Sin(ang);
            player.MotionZ += m * Math.Cos(ang);
        }

        public bool Finished()
        {
            return pathPoints == null ? true : pathIndex >= pathPoints.Length;
        }

        private int getClosestIndex(double x, double y, double z)
        {
            double bestDist = double.MaxValue;
            int bestIndex = 0;
            for (int i = 0; i < pathPoints.Length; i++) {
                Vec3i pt = pathPoints[i];
                double dist = Utils.DistTo(x, y, z, pt.X + 0.5, pt.Y, pt.Z + 0.5);
                if (dist < bestDist) {
                    bestDist = dist;
                    bestIndex = i;
                }
            }
            return bestIndex;
        }
    }
}