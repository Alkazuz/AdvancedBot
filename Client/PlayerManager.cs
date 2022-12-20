using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace AdvancedBot.client
{
    public class PlayerManager
    {
        public ConcurrentDictionary<UUID, string> UUID2Nick = new ConcurrentDictionary<UUID, string>();
        public ConcurrentDictionary<int, MPPlayer> Players = new ConcurrentDictionary<int, MPPlayer>();

        public string GetNick(MPPlayer player)
        {
            if (UUID2Nick.TryGetValue(player.Uuid, out string nick))
                return nick;

            return "";
        }
        public MPPlayer GetPlayerByNick(string nick)
        {
            foreach (var kv in Players) {
                var p = kv.Value;
                if (UUID2Nick.TryGetValue(p.Uuid, out string pn) && pn.EqualsIgnoreCase(nick)) {
                    return p;
                }
            }
            return null;
        }
        public bool PlayerExists(int id)
        {
            return Players.ContainsKey(id);
        }
        public void Clear()
        {
            Players.Clear();
            UUID2Nick.Clear();
        }
    }
    public class MPPlayer
    {
        public int EntityID;
        public UUID Uuid;
        public double X, Y, Z;
        public float Yaw, Pitch;

        public MPPlayer(int eId, UUID uuid)
        {
            EntityID = eId;
            Uuid = uuid;
        }

        public void SetPos(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public void SetRotation(float yaw, float pitch)
        {
            Yaw = yaw;
            Pitch = pitch;
        }
    }
}
