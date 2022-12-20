using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdvancedBot.client.Map;

namespace AdvancedBot.client.PathFinding
{
    public class PathFinder
    {
        private World worldMap;
        private Path path = new Path();
        private Dictionary<int, PathPoint> pointMap = new Dictionary<int, PathPoint>();
        private PathPoint[] pathOptions = new PathPoint[32];

        private bool isWoddenDoorAllowed;
        private bool isMovementBlockAllowed;
        private bool isPathingInWater;
        private bool canEntityDrown;

        public PathFinder(World world, bool allowDoors, bool movementBlocked, bool canSwim, bool canDrown)
        {
            worldMap = world;
            isWoddenDoorAllowed = allowDoors;
            isMovementBlockAllowed = movementBlocked;
            isPathingInWater = canSwim;
            canEntityDrown = canDrown;
        }

        public PathPoint[] CreatePathTo(Entity entity, double x, double y, double z, float radius)
        {
            path.Clear();
            pointMap.Clear();
            bool var9 = isPathingInWater;

            int groundY = Utils.Floor(entity.AABB.MinY + 0.5D);

            if (canEntityDrown && entity.IsInWater()) {
                groundY = (int)entity.AABB.MinY;

                for (int id = worldMap.GetBlock(Utils.Floor(entity.PosX), groundY, Utils.Floor(entity.PosZ));
                    id == Blocks.flowing_water || id == Blocks.water; id = worldMap.GetBlock(Utils.Floor(entity.PosX), groundY, Utils.Floor(entity.PosZ))) {
                    ++groundY;
                }

                var9 = isPathingInWater;
                isPathingInWater = false;
            } else {
                groundY = Utils.Floor(entity.AABB.MinY + 0.5D);
            }

            PathPoint start = OpenPoint(Utils.Floor(entity.AABB.MinX), groundY, Utils.Floor(entity.AABB.MinZ));
            PathPoint end = OpenPoint(Utils.Floor(x), Utils.Floor(y), Utils.Floor(z));
            PathPoint[] points = FindPath(entity, start, end, radius);
            isPathingInWater = var9;
            return points;
        }
        private PathPoint[] FindPath(Entity entity, PathPoint origin, PathPoint dest, float radius)
        {
            origin.TotalPathDistance = 0.0F;
            origin.DistanceToNext = origin.DistanceManhattan(dest);
            origin.DistanceToTarget = origin.DistanceToNext;
            path.Clear();
            path.AddPoint(origin);
            PathPoint closestEnd = origin;

            for (int itr = 0; !path.IsPathEmpty() && itr < 512; itr++) {
                PathPoint current = path.Dequeue();

                if (current.Equals(dest))
                    return ToArray(origin, dest);

                if (current.DistanceManhattan(dest) < closestEnd.DistanceManhattan(dest))
                    closestEnd = current;

                current.IsFirst = true;

                int optionCount = FindOptions(entity, current, dest, radius);
                for (int i = 0; i < optionCount; ++i) {
                    PathPoint option = pathOptions[i];
                    float optDist = current.TotalPathDistance + current.DistanceManhattan(option);

                    if (!option.IsAssigned() || optDist < option.TotalPathDistance) {
                        option.Previous = current;
                        option.TotalPathDistance = optDist;
                        option.DistanceToNext = option.DistanceManhattan(dest);

                        if (option.IsAssigned()) {
                            path.ChangeDistance(option, option.TotalPathDistance + option.DistanceToNext);
                        } else {
                            option.DistanceToTarget = option.TotalPathDistance + option.DistanceToNext;
                            path.AddPoint(option);
                        }
                    }
                }
            }

            if (closestEnd == origin) {
                return null;
            } else {
                return ToArray(origin, closestEnd);
            }
        }

        private int FindOptions(Entity entity, PathPoint current, PathPoint target, float maxDist)
        {
            int var7 = 0;

            if (GetNodeType(entity, current.X, current.Y + 1, current.Z) == NodeType.Open) {
                var7 = 1;
            }

            PathPoint south = GetSafePoint(entity, current.X, current.Y, current.Z + 1, var7);
            PathPoint west = GetSafePoint(entity, current.X - 1, current.Y, current.Z, var7);
            PathPoint east = GetSafePoint(entity, current.X + 1, current.Y, current.Z, var7);
            PathPoint north = GetSafePoint(entity, current.X, current.Y, current.Z - 1, var7);

            int opt = 0;
            if (south != null && !south.IsFirst && south.DistanceEuclidean(target) < maxDist) pathOptions[opt++] = south;
            if (west != null && !west.IsFirst && west.DistanceEuclidean(target) < maxDist) pathOptions[opt++] = west;
            if (east != null && !east.IsFirst && east.DistanceEuclidean(target) < maxDist) pathOptions[opt++] = east;
            if (north != null && !north.IsFirst && north.DistanceEuclidean(target) < maxDist) pathOptions[opt++] = north;

            return opt;
        }

        /**
         * Returns a point that the entity can safely move to
         */
        private PathPoint GetSafePoint(Entity entity, int x, int y, int z, int par6)
        {
            PathPoint point = null;
            NodeType type = GetNodeType(entity, x, y, z);

            if (type == NodeType._2) {
                return OpenPoint(x, y, z);
            } else {
                if (type == NodeType.Open) point = OpenPoint(x, y, z);

                if (point == null && par6 > 0 && type != NodeType.Fence && type != NodeType.Trapdoor && GetNodeType(entity, x, y + par6, z) == NodeType.Open) {
                    point = OpenPoint(x, y + par6, z);
                    y += par6;
                }

                if (point != null) {
                    int fallHeight = 0;

                    while (y > 0) {
                        if (y == 1) return null;

                        type = GetNodeType(entity, x, y - 1, z);

                        if (isPathingInWater && type == NodeType.Water)
                            return null;

                        if (type != NodeType.Open)
                            break;

                        if (fallHeight++ >= 3)
                            return null;

                        --y;

                        if (y > 0) point = OpenPoint(x, y, z);
                    }

                    if (type == NodeType.Lava) return null;
                }

                return point;
            }
        }

        /**
         * Returns a mapped point or creates and adds one
         */
        private PathPoint OpenPoint(int x, int y, int z)
        {
            int hash = PathPoint.MakeHash(x, y, z);

            if (!pointMap.TryGetValue(hash, out PathPoint pt)) {
                pt = new PathPoint(x, y, z);
                pointMap.Add(hash, pt);
            }

            return pt;
        }

        /**
         * Checks if an entity collides with blocks at a position. 
         * Returns:
         * 
         * 2 if otherwise clear except for open trapdoor or water(if not avoiding)
         * 1 if clear
         * 0 for colliding with any solid block,
         * -1 for water(if avoiding water) but otherwise clear, 
         * -2 for lava, 
         * -3 for fence, 
         * -4 for closed trapdoor
         */
        private enum NodeType
        {
            _2,
            Open,
            Blocked,
            Water,
            Lava,
            Fence,
            Trapdoor
        }
        private NodeType GetNodeType(Entity entity, int posX, int posY, int posZ)
        {
            bool trapdoor = false;
            const int SIZE_X = 1;
            const int SIZE_Y = 2;
            const int SIZE_Z = 1;

            for (int x = posX; x < posX + SIZE_X; x++) {
                for (int y = posY; y < posY + SIZE_Y; y++) {
                    for (int z = posZ; z < posZ + SIZE_Z; z++) {
                        int id = entity.World.GetBlock(x, y, z);

                        if (id == Blocks.portal || id == Blocks.vine) return NodeType.Open;
                        if (id != 0) {
                            if (id == Blocks.trapdoor)
                                trapdoor = true;
                            else if (id != Blocks.flowing_water && id != Blocks.water) {
                                if (!isWoddenDoorAllowed && id == Blocks.wooden_door)
                                    return NodeType.Blocked;
                            } else {
                                if (isPathingInWater)
                                    return NodeType.Water;

                                trapdoor = true;
                            }

                            if (id != Blocks.web && (!isMovementBlockAllowed || id != Blocks.wooden_door)) {
                                if (id == Blocks.fence || id == Blocks.nether_brick_fence ||
                                    id == Blocks.fence_gate || id == Blocks.cobblestone_wall)
                                    return NodeType.Fence;

                                if (id == Blocks.trapdoor)
                                    return NodeType.Trapdoor;

                                if (id != Blocks.lava && id != Blocks.flowing_lava)
                                    return NodeType.Blocked;

                                if (!entity.IsInLava())
                                    return NodeType.Lava;
                            }
                        }
                    }
                }
            }

            return trapdoor ? NodeType._2 : NodeType.Open;
        }

        private PathPoint[] ToArray(PathPoint start, PathPoint end)
        {
            int count = 1;
            PathPoint point;

            for (point = end; point.Previous != null; point = point.Previous) {
                ++count;
            }

            PathPoint[] path = new PathPoint[count];
            point = end;

            --count;
            for (path[count] = end; point.Previous != null; path[count] = point) {
                point = point.Previous;
                --count;
            }

            return path;
        }
    }
}