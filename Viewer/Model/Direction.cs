using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdvancedBot.client;

namespace AdvancedBot.Viewer.Model
{
    public enum Direction
    {
        Down  = 0,
        Up    = 1,
        North = 2,
        South = 3,
        West  = 4,
        East  = 5,
        Invalid = -1
    }
    public static class DirectionEx
    {
        public static readonly Direction[] Values = new Direction[] {
            Direction.Down,
            Direction.Up,
            Direction.North,
            Direction.South,
            Direction.West,
            Direction.East
        };

        public static Direction FromString(string str)
        {
            switch (str) {
                case "down":  return Direction.Down;
                case "up":    return Direction.Up;
                case "north": return Direction.North;
                case "south": return Direction.South;
                case "west":  return Direction.West;
                case "east":  return Direction.East;
            }
            return Direction.Invalid;
        }
        public static Direction Opposite(this Direction dir)
        {
            switch (dir) {
                case Direction.Down:  return Direction.Up;
                case Direction.Up:    return Direction.Down;
                case Direction.North: return Direction.South;
                case Direction.South: return Direction.North;
                case Direction.West:  return Direction.East;
                case Direction.East:  return Direction.West;
            }
            return Direction.Invalid;
        }
        public static Vec3i Offset(this Direction dir)
        {
            switch (dir) {
                case Direction.Down:  return new Vec3i(0, -1, 0);
                case Direction.Up:    return new Vec3i(0, 1, 0);
                case Direction.North: return new Vec3i(0, 0, -1);
                case Direction.South: return new Vec3i(0, 0, 1);
                case Direction.West:  return new Vec3i(-1, 0, 0);
                case Direction.East:  return new Vec3i(1, 0, 0);
            }
            return new Vec3i(0, 0, 0);
        }

        public static string Name(this Direction dir)
        {
            switch (dir) {
                case Direction.Down: return "down";
                case Direction.Up: return "up";
                case Direction.North: return "north";
                case Direction.South: return "south";
                case Direction.West: return "west";
                case Direction.East: return "east";
            }
            return "invalid";
        }
    }
}
