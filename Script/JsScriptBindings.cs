using AdvancedBot.client;
using AdvancedBot.client.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

//[assembly: Obfuscation(Exclude = false, Feature = "namespace('AdvancedBot.Script'):-rename")]

namespace AdvancedBot.Script
{
    public class JsMinecraftClient
    {
        private MinecraftClient client;

        public JsPlayer Player { get; }
        public JsWorld World { get; }
        public string Username => client.Username;

        public JsInventory Inventory
        {
            get {
                return new JsInventory(client, client.Inventory);
            }
        }
        public JsInventory OpenWindow
        {
            get {
                var inv = client.OpenWindow;
                return inv == null ? null : new JsInventory(client, inv);
            }
        }
        public int SelectedHotbarSlot {
            get => client.HotbarSlot;
            set => client.HotbarSlot = value;
        }
        public JsItemStack ItemInHand
        {
            get {
                var item = client.ItemInHand;
                return item == null ? null : new JsItemStack(item);
            }
        }

        public JsMinecraftClient(MinecraftClient cli)
        {
            client = cli;
            Player = new JsPlayer(cli.Player);
            World = new JsWorld(cli.World);
        }

        public void BreakBlock(JsHitResult hit)
        {
            client.BreakBlock(new HitResult(hit.X, hit.Y, hit.Z, hit.Face));
        }
        public void PlaceBlock(JsHitResult hit)
        {
            client.PlaceCurrentBlock(new HitResult(hit.X, hit.Y, hit.Z, hit.Face));
        }
        public int FindHotbarItem(int itemId)
        {
            return client.SlotOfHotbarItem(itemId);
        }
        public int FindItem(int itemId, bool allowHotbar)
        {
            return client.SlotOfItem(itemId, allowHotbar);
        }
    }
    public class JsPlayer
    {
        private Entity p;
        internal JsPlayer(Entity e)
        {
            p = e;
        }

        public double PosX => p.PosX;
        public double PosY => p.AABB.MinY;
        public double PosZ => p.PosZ;

        public double MotionX
        {
            get => p.MotionX;
            set => p.MotionX = value;
        }
        public double MotionY
        {
            get => p.MotionY;
            set => p.MotionY = value;
        }
        public double MotionZ
        {
            get => p.MotionZ;
            set => p.MotionZ = value;
        }

        public float Yaw
        {
            get => p.Yaw;
            set => p.Yaw = value;
        }
        public float Pitch
        {
            get => p.Pitch;
            set => p.Pitch = value;
        }

        public bool OnGround
        {
            get => p.OnGround;
            set => p.OnGround = value;
        }
        public bool IsCollidedHorizontally
        {
            get => p.IsCollidedHorizontally;
            set => p.IsCollidedHorizontally = value;
        }
        public bool IsCollidedVertically
        {
            get => p.IsCollidedVertically;
            set => IsCollidedVertically = value;
        }
        public bool IsSprinting
        {
            get => p.IsSprinting;
            set => p.IsSprinting = value;
        }

        public void SetPosition(double x, double y, double z)
        {
            p.SetPosition(x, y, z);
        }
        public int GetPotionAmplifier(int id)
        {
            if(p.ActivePotions.TryGetValue((byte)id, out byte amp)) {
                return amp;
            }
            return -1;
        }

        public void EnqueueMovement(string flags)
        {
            Movement mf = Movement.None;
            for(int i = 0; i < flags.Length; i++) {
                switch (Char.ToUpper(flags[i])) {
                    case 'F': mf |= Movement.Forward; break;
                    case 'B': mf |= Movement.Back; break;
                    case 'L': mf |= Movement.Left; break;
                    case 'R': mf |= Movement.Right; break;
                    case 'J': mf |= Movement.Jump; break;
                }
            }
            if(mf != Movement.None) {
                p.MoveQueue.Enqueue(mf);
            }
        }

        public void LookToBlock(int x, int y, int z, bool randomize)
        {
            p.LookToBlock(x, y, z, randomize);
        }

        public JsHitResult RayCastBlocks(double radius)
        {
            var hit = p.RayCastBlocks(radius);
            return hit == null ? null : new JsHitResult(hit);
        }
    }
    public class JsWorld
    {
        private World w;
        internal JsWorld(World world)
        {
            w = world;
        }

        public int GetBlockId(int x, int y, int z)
        {
            return w.GetBlock(x, y, z);
        }
        public int GetBlockData(int x, int y, int z)
        {
            return w.GetData(x, y, z);
        }

        public string[] GetSignText(int x, int y, int z)
        {
            return w.GetSignText(x, y, z);
        }
    }
    public class JsInventory
    {
        private MinecraftClient cli;
        private Inventory inv;

        internal JsInventory(MinecraftClient client, Inventory inv)
        {
            cli = client;
            this.inv = inv;
        }

        public int Slots => inv.NumSlots;
        public string Title => inv.Title;

        public JsItemStack GetItemAt(int slot)
        {
            if(slot >= 0 && slot <= inv.NumSlots) {
                ItemStack stack = inv.Slots[slot];
                return stack == null ? null : new JsItemStack(stack);
            }
            return null;
        }

        public void Click(int slot, bool isLeftButton)
        {
            inv.Click(cli, (short)slot, inv == cli.Inventory && cli.OpenWindow != null, isLeftButton);
        }
        public void DropItem(int slot)
        {
            inv.DropItem(cli, slot);
        }
    }
    public class JsItemStack
    {
        private ItemStack stack;

        public int ID => stack.ID;
        public short Metadata => stack.Metadata;
        public int Count => stack.Count;

        internal JsItemStack(ItemStack stack)
        {
            this.stack = stack;
        }

        public string GetLore()
        {
            return stack.GetLore();
        }
        public string GetDisplayName()
        {
            return stack.GetDisplayName();
        }
        public int GetEnchantmentLevel(int enchId)
        {
            return stack.GetEnchantmentLevel(enchId);
        }
    }
    public class JsHitResult
    {
        public int X, Y, Z;
        public int Face;

        internal JsHitResult(HitResult hit)
        {
            X = hit.X;
            Y = hit.Y;
            Z = hit.Z;
            Face = hit.Face;
        }
    }
}
