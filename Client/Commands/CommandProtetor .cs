using AdvancedBot.client.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedBot.client.Commands
{
    public class CommandProtetor : CommandBase
    {

        private string query = null;
        private int lastAttack = 0;
        private int speed;
        private MPPlayer following;
        Vec3d lastFollowPos = new Vec3d(0, -555, 0);
        public CommandProtetor (MinecraftClient cli)
            : base(cli, "Protetor", "Proteger jogadores na lista usando killaura em jogadores não listadosm pode seguir determinado jogador automaticamente", "protetor")
        {
            ToggleText = "§6Protetor {0}";
        }
        public override CommandResult Run(string alias, string[] args)
        {
            Toggle(new string[0]);
            if (IsToggled) {
                if (Program.Config.GetBoolean("ProtetorArmor"))
                {
                    SelectBestArmor();
                }
                if (Program.Config.GetBoolean("ProtetorEspada"))
                {
                    SelectBestTool();
                }
                lastFollowPos = new Vec3d(0, -555, 0);
                speed = Program.Config.GetInt("ProtetorCPS");
                following = Client.PlayerManager.GetPlayerByNick(Program.Config.GetString("ProtetorFollowNick"));
                if (following == null && Program.Config.GetBoolean("ProtetorFollow"))
                {
                    Client.PrintToChat("§cNão foi possível encontrar o jogador " + Program.Config.GetString("ProtetorFollowNick") + " para seguir");
                    return CommandResult.ErrorSilent;
                }
                else
                {
                    if (Program.Config.GetBoolean("ProtetorFollow")) {
                    Client.CmdManager.GetCommand("retard").IsToggled = false;
                    Client.PrintToChat("§aSeguindo o player '" + Client.PlayerManager.GetNick(following) + "'");
                    return CommandResult.Success;
                    }
                }
            }
            return CommandResult.Success;
        }
        private static Random PRNG = new Random();
        public override void Tick()
        {
            if (!IsToggled) return;
            Entity me = Client.Player;
            MPPlayer bestPlayer = null;
            if (lastAttack++ >= speed)
            {
                double bestDist = double.MaxValue;
                foreach (MPPlayer p in Client.PlayerManager.Players.Values)
                {
                    if (protect(Client.PlayerManager.GetNick(p))) continue;
                    if (Program.getBot(Client.PlayerManager.GetNick(p)) != null) continue;
                    double pDist = Utils.DistTo(me.PosX, me.AABB.MinY, me.PosZ, p.X, p.Y, p.Z);
                    if (pDist <= 4.0 &&
                        (query == null || query == "*" ? true : Client.PlayerManager.GetNick(p).EqualsIgnoreCase(query)) &&
                        (bestPlayer == null || pDist < bestDist) &&
                        me.CanSeePlayer(p))
                    {
                        bestDist = pDist;
                        bestPlayer = p;
                    }
                }
                if (bestPlayer != null)
                {
                    me.LookTo(bestPlayer.X + RPosInc(), bestPlayer.Y + 1.62 + RPosInc(), bestPlayer.Z + RPosInc());

                    Client.SendPacket(new PacketSwingArm(Client.PlayerID)); //send swing
                    Client.SendPacket(new PacketUseEntity(bestPlayer.EntityID, true));
                }
                lastAttack = 0;
            }
            if (following != null && Program.Config.GetBoolean("ProtetorFollow"))
            {
                Entity p = Player;
                double dist = Utils.DistTo(p.PosX, p.AABB.MinY, p.PosZ, following.X, following.Y, following.Z);
                bool exists = Client.PlayerManager.PlayerExists(following.EntityID);
                if (!exists || dist > 80)
                {
                    Client.CurrentPath = null;
                    following = null;
                    return;
                }

                p.LookTo(following.X, following.Y + 1.62, following.Z);
                double lastDist = Utils.DistTo(following.X, following.Y, following.Z,
                                               lastFollowPos.X, lastFollowPos.Y, lastFollowPos.Z);
                if (lastDist >= 2.5)
                {
                    Client.SetPath(Utils.Floor(following.X), Utils.Floor(following.Y), Utils.Floor(following.Z));
                    lastFollowPos = new Vec3d(following.X, following.Y, following.Z);
                }
            }
        }
        public static Boolean protect(String d)
        {
            foreach (String nicks in Protetor.Protetor.protect)
            {
                String nick = nicks.Replace("|","");
                if (d.EqualsIgnoreCase(nick)) return true;
            }
            return false;
        }
        private static double RPosInc()
        {
            return (PRNG.NextDouble() - 0.5) * 0.2;
        }
        private void SelectBestTool()
        {
            int bestSlot = -1;
            for (int i = 0; i < 9; i++)
            {
                ItemStack item = Client.Inventory.Slots[36 + i];

                if (item != null)
                {
                    if(item.ID == 276)
                    {
                        bestSlot = i; break;
                    }
                    if (item.ID == 267)
                    {
                        bestSlot = i; break;
                    }
                    if (item.ID == 272)
                    {
                        bestSlot = i; break;
                    }
                    if (item.ID == 276)
                    {
                        bestSlot = i;break;
                    }
                    
                }
            }
            if (bestSlot != -1)
            {
                Client.HotbarSlot = bestSlot;
            }
        }

        private void SelectBestArmor()
        {
            int[] bootIds;
            int[] pantIds;
            int[] chestIds;
            int[] helmIds;
            int prevSlot;

            bootIds = new int[] { 313, 309, 305, 317, 301 };
            pantIds = new int[] { 312, 308, 304, 316, 300 };
            chestIds = new int[] { 311, 307, 303, 315, 299 };
            helmIds = new int[] { 310, 306, 302, 314, 298 };

            Boolean boots = Client.Inventory.Slots[5] != null;
            Boolean pants = Client.Inventory.Slots[6] != null;
            Boolean shirt = Client.Inventory.Slots[7] != null;
            Boolean helm = Client.Inventory.Slots[8] != null;
            if (!boots)
            {
                this.equip(bootIds);
            }
            if (!pants)
            {
                this.equip(pantIds);
            }
            if (!shirt)
            {
                this.equip(chestIds);
            }
            if (!helm)
            {
                this.equip(helmIds);
            }
        }

        private void equip( int[] ids)
        {
            Boolean hot = false;
            Boolean inv = false;
            int slot = -1;
            foreach(int id in ids)
            {
                int invSlot = this.getSlotOfInvItem(id);
                if (invSlot != -1)
                {
                    inv = true;
                    slot = invSlot;
                    break;
                }
                int newSlot = this.getSlotOfHotbarItem(id);
                if (newSlot != -1)
                {
                    hot = true;
                    slot = newSlot;
                    break;
                }
            }
            if (slot != -1 && inv)
            {
                    Client.Inventory.Click(Client, (short)slot, Client.OpenWindow != null, true);
            }
            else if (slot != -1 && hot)
            {
                Client.HotbarSlot = slot;
                Client.LeftClickItem();
            }
        }

        private int getSlotOfHotbarItem(int itemId)
        {
            for (int i = 0; i < 9; ++i)
            {
                ItemStack item = Client.Inventory.Slots[36 + i];
                if (item != null && item.ID == itemId)
                {
                    return i;
                }
            }
            return -1;
        }
        private int getSlotOfInvItem(int itemId)
        {
            for (int i = 9; i < 36; ++i)
            {
                ItemStack item = Client.Inventory.Slots[i];
                if (item != null && item.ID == itemId)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
