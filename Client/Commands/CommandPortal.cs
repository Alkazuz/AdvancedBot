using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using AdvancedBot.client.PathFinding;
using Newtonsoft.Json.Linq;
using AdvancedBot.Properties;
using AdvancedBot.client.Map;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;

namespace AdvancedBot.client.Commands
{
    public class CommandPortal : CommandBase
    {
        public CommandPortal(MinecraftClient cli)
            : base(cli, "Portal", "Entra em um portal (CraftLandia)", "portal")
        {
            SetParams("<nome do mg>");
            RunAsync = true;
        }
        public override CommandResult Run(string alias, string[] args)
        {
            if (args.Length < 1) return CommandResult.MissingArgs;

            Vec3i portal = FindPortal(Player, args[0].ToLower());
            if (portal == null) {
                Client.PrintToChat("§cNão foi possível encontrar o portal.");
                return CommandResult.ErrorSilent;
            }
            Client.CmdManager.GetCommand("retard").IsToggled = false;
            Client.SetPath(portal.X, portal.Y, portal.Z, "§cNão foi possível encontrar o caminho até o portal.");
            return CommandResult.Success;
        }
        private static Dictionary<string, List<Vec3i>> portals = new Dictionary<string, List<Vec3i>>();

        public static Vec3i FindPortal(Entity p, string name)
        {
            if (name == "**") return BruteForcePortalFinder(p);

            if (portals.Count == 0) {
                portals["*"] = new List<Vec3i>();

                JArray plist = null;
                if(File.Exists("cl_portals.json")) {
                    try {
                        plist = JArray.Parse(File.ReadAllText("cl_portals.json"));
                    } catch { }
                }
                if(plist == null) {
                    plist = JArray.Parse(Encoding.UTF8.GetString(Resources.portals));
                    File.WriteAllText("cl_portals.json", plist.ToString(Formatting.Indented));
                }

                foreach (JObject portal in plist) {
                    string pname = (string)portal["name"];
                    List<Vec3i> poss = new List<Vec3i>();

                    foreach (JObject pos in (JArray)portal["positions"]) {
                        int x = (int)pos["x"];
                        int y = (int)pos["y"];
                        int z = (int)pos["z"];
                        poss.Add(new Vec3i(x, y, z));
                    }
                    portals[pname] = poss;
                    portals["*"].AddRange(poss);
                }
            }
            List<Vec3i> positions;
            if (portals.TryGetValue(name.ToLower(), out positions)) {
                double bestDist = double.MaxValue;
                Vec3i bestPortal = null;
                foreach (Vec3i pos in positions) {
                    double dist = Utils.DistToSq(pos.X + 0.5, pos.Y + 0.5, pos.Z + 0.5, p.PosX, p.PosY, p.PosZ);
                    if (dist < bestDist) {
                        bestDist = dist;
                        bestPortal = pos;
                    }
                }
                return bestPortal;
            }
            return null;
        }
        public static Vec3i BruteForcePortalFinder(Entity p)
        {
            const int CHUNK_RADIUS_HOR = 6;
            const int RADIUS_VER = 16;

            int px = Utils.Floor(p.PosX) / 16;
            int py = Utils.Floor(p.PosY);
            int pz = Utils.Floor(p.PosZ) / 16;
            
            for (int cz = -CHUNK_RADIUS_HOR; cz <= CHUNK_RADIUS_HOR; cz++) {
                for (int cx = -CHUNK_RADIUS_HOR; cx <= CHUNK_RADIUS_HOR; cx++) {
                    Chunk chunk = p.World.GetChunk(px + cx, pz + cz);
                    if (chunk != null) {
                        for (int y = Math.Max(1, py - RADIUS_VER), ye = Math.Min(254, py + RADIUS_VER); y < ye; y++) {
                            for (int z = 0; z < 16; z++) {
                                for (int x = 0; x < 16; x++) {
                                    if (chunk.GetBlock(x, y, z) == Blocks.portal)
                                        return new Vec3i((px + cx) * 16 + x, y, (pz + cz) * 16 + z);
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }
    }
}
