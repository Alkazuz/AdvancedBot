using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdvancedBot.client.Packets;
using Newtonsoft.Json.Linq;

namespace AdvancedBot.client.Bypassing
{
    public class ADRBypass : ServerBypassBase
    {
        private static Dictionary<int, JArray> Recipes = new Dictionary<int, JArray>();
        static ADRBypass()
        {
            var jo = JObject.Parse(Encoding.UTF8.GetString(Properties.Resources.recipes));
            foreach (var kv in jo) {
                Recipes[int.Parse(kv.Key)] = (JArray)kv.Value;
            }
        }

        public override ClientVersion Version => ClientVersion.v1_8;

        private ConcurrentDictionary<int, ItemFrame> itemFrames = new ConcurrentDictionary<int, ItemFrame>();

        public ADRBypass(MinecraftClient cli) : base(cli)
        {
            cli.OnTick += OnTick;
        }

        public override bool HandlePacket(ReadBuffer rb)
        {
            switch (rb.ID) {
                case 0x0E: { //spawn object
                    int entityId = rb.ReadVarInt();
                    if (rb.ReadByte() == 71) {
                        double x = rb.ReadInt() / 32.0;
                        double y = rb.ReadInt() / 32.0;
                        double z = rb.ReadInt() / 32.0;
                        rb.Skip(2); //pitch, yaw
                        int data = rb.ReadInt();
                        itemFrames[entityId] = new ItemFrame(entityId, x, y, z, data);
                    }
                    return true;
                }
                case 0x1C: { //entity metadata
                    int entityId = rb.ReadVarInt();
                    if (itemFrames.TryGetValue(entityId, out var frame)) {
                        for (byte item; (item = rb.ReadByte()) != 0x7F;) {
                            int index = item & 0x1F;
                            int metaType = item >> 5;

                            switch (metaType) {
                                case 0: rb.ReadByte(); break;
                                case 1: rb.ReadShort(); break;
                                case 2: rb.ReadInt(); break;
                                case 3: rb.ReadFloat(); break;
                                case 4: rb.ReadString(); break;
                                case 5:
                                    var stack = rb.ReadItemStack();
                                    if (index == 8) {
                                        frame.DisplayedItem = stack;
                                    }
                                    break;
                                case 6:
                                case 7: rb.Skip(12); break;
                            }
                        }
                    }
                    return true;
                }
                case 0x07: {//respawn
                    started = 0;
                    return true;
                }
                default: return false;
            }
        }

        private int hasWalked = -1;
        private int craftDelay = -1;
        private long started = Utils.GetTimestamp();
        private void OnTick()
        {
            try {
                if (hasWalked != -1 && hasWalked++ < 31) return;
                if (craftDelay != -1 && craftDelay++ < 20) return;

                if (itemFrames.Count < 9) return;
                var frames = itemFrames.Values.ToArray();

                var cpo = new int[][] { //craft pattern offsets
                    new [] { -1,  1 }, new [] { 0,  1 }, new [] { 1,  1 },
                    new [] { -1,  0 }, new [] { 0,  0 }, new [] { 1,  0 },
                    new [] { -1, -1 }, new [] { 0, -1 }, new [] { 1, -1 }
                };

                ItemFrame center = frames.First(f => cpo.All(po => GetNeighbor(po[0], po[1], f) != null));
                ItemFrame result = frames.First(f => cpo.All(po => GetNeighbor(po[0], po[1], f) != center));

                if (result.DisplayedItem == null) return; //ainda não logado...

                var pattern = new ItemFrame[9];
                for (int i = 0; i < 9; i++) {
                    var ofs = cpo[i];
                    pattern[i] = GetNeighbor(ofs[0], ofs[1], center);
                }
                //for (int i = 0; i < 3; i++) {
                //    for (int j = 0; j < 3; j++) {
                //        var item = pattern[j + i * 3].DisplayedItem;
                //        Debug.Write((item==null?new string(' ', 18):item?.GetDisplayName().PadLeft(18))+"|");
                //    }
                //    Debug.Write("\n");
                //}
                //Debug.WriteLine(">>> " + result.DisplayedItem?.GetDisplayName());

                ItemStack inHand = Client.Inventory.Slots[40];
                var missing = FindMissingItem(pattern, inHand, result.DisplayedItem);
                if (missing == null) {
                    Client.PrintToChat($"§cADRBypass: O captcha contém uma receita inválida ({Items.GetDisplayName(result.DisplayedItem.ID, 0)})");
                    Client.OnTick -= OnTick;
                    IsFinished = true;
                    return;
                } else {
                    var ifram = missing.Item2;
                    Client.Player.LookToBlock(ifram.X, ifram.Y, ifram.Z, false);
                    if (hasWalked == -1) {
                        hasWalked = 0;
                        for (int i = 0; i < 30; i++) {
                            Client.Player.MoveQueue.Enqueue(Movement.Forward);
                        }
                        return;
                    }
                    Client.HotbarSlot = 5;
                    ifram.DisplayedItem = inHand;
                    Client.SendPacket(new PacketUseEntity(ifram.EntityID, false));
                    craftDelay = 0;

                    if (Utils.GetTimestamp() - started > 20000) {
                        Client.PrintToChat("§aADRBypass: O captcha foi burlado.");

                        Client.OnTick -= OnTick;
                        IsFinished = true;
                    }
                }

                ItemFrame GetNeighbor(int xo, int yo, ItemFrame origin)
                {
                    //dir = S-W-N-E
                    Vec3i ofs;
                    switch (origin.Direction) {
                        case 0: ofs = new Vec3i(-1, 1, 0); break; //south 0,0,1
                        case 2: ofs = new Vec3i(1, 1, 0); break; //north 0,0,-1
                        case 1: ofs = new Vec3i(0, 1, -1); break; //west -1,0,0
                        case 3: ofs = new Vec3i(0, 1, 1); break; //east 1,0,0
                        default: throw new Exception("Invalid direction");
                    }
                    ofs = new Vec3i(origin.X + ofs.X * xo,
                                    origin.Y + ofs.Y * yo,
                                    origin.Z + ofs.Z * xo);
                    return frames.FirstOrDefault(a => a.X == ofs.X &&
                                                      a.Y == ofs.Y &&
                                                      a.Z == ofs.Z);
                }
            } catch (Exception ex) {
                Client.PrintToChat("§4ADRBypass: Erro ao tentar burlar o captcha.\n\n" + ex.ToString());

                Client.OnTick -= OnTick;
                IsFinished = true;
            }
        }

        private Tuple<int, ItemFrame> FindMissingItem(ItemFrame[] pattern, ItemStack item, ItemStack result)
        {
            if (Recipes.TryGetValue(result.ID, out var recipe)) {
                foreach (var r in recipe) {
                    JToken tmp;
                    if ((tmp = r["inShape"]) != null) {
                        var jarr = tmp as JArray;
                        int rw = jarr.Max(a => a.Count());
                        int rh = jarr.Count;
                        for (int rx = 0; rx <= 3 - rw; rx++) {
                            for (int ry = 0; ry <= 3 - rh; ry++) {
                                var i = GetMissingIndexShaped(jarr, rx, ry, rw, rh);
                                if (i != null) {
                                    return i;
                                }
                            }
                        }
                    } else if ((tmp = r["ingredients"]) != null) {
                        var ingr = tmp.FirstOrDefault(a => !pattern.Any(pat => pat != null && Compare(pat, a)));
                        GetIngredientID(ingr, out int id, out _);
                        return new Tuple<int, ItemFrame>(id, pattern.First(a => a.DisplayedItem == null));
                    } else {
                        Debug.WriteLine($"Invalid recipe, ID={result.ID}");
                    }
                }
            }
            return null;
            /////////////////////local methods/////////////////////
            bool Compare(ItemFrame frame, JToken jt)
            {
                var stack = frame.DisplayedItem;
                if (stack == null) {
                    return jt == null;
                } else {
                    GetIngredientID(jt, out int id, out int data);
                    return id == stack.ID && (data == -1 || data == stack.Metadata);
                }
            }
            Tuple<int, ItemFrame> GetMissingIndexShaped(JArray shape, int rx, int ry, int rw, int rh)
            {
                bool has = false;
                Tuple<int, ItemFrame> lNull = null;
                for (int x = 0; x < rw; x++) {
                    for (int y = 0; y < rh; y++) {

                        int id, data;
                        GetIngredientID(shape[y][x], out id, out data);
                        ItemStack placedItem = pattern[(rx + x) + (ry + y) * 3].DisplayedItem;

                        has |= placedItem != null && id != -1;
                        if (placedItem == null && id != -1 && id == item.ID) {
                            lNull = new Tuple<int, ItemFrame>(id, pattern[(rx + x) + (ry + y) * 3]);
                        }
                    }
                }
                return has ? lNull : null;
            }
            void GetIngredientID(JToken jt, out int id, out int data)
            {
                data = -1;
                if (jt.Type == JTokenType.Integer) {
                    id = jt.AsInt();
                } else if (jt.Type == JTokenType.Object) {
                    id = jt["id"].AsInt();
                    data = jt["metadata"].AsInt();
                } else if (jt.Type == JTokenType.Array) {
                    id = jt[0].AsInt();
                    data = jt[1].AsInt();
                } else {
                    if (jt.Type != JTokenType.Null) {
                        Debug.WriteLine($"Don't know how to compare JToken of type {jt.Type} with item");
                    }
                    id = data = -1;
                }
            }
        }

        private class ItemFrame
        {
            public int EntityID;
            public int X, Y, Z;
            public ItemStack DisplayedItem;
            public int Direction;

            public ItemFrame(int id, double x, double y, double z, int data)
            {
                EntityID = id;
                X = Utils.Floor(x);
                Y = Utils.Floor(y);
                Z = Utils.Floor(z);
                Direction = Math.Abs(data % 4);
            }
            public override string ToString()
            {
                return $"Pos=[{X} {Y} {Z}] Dir={Direction}";
            }
        }
    }
}
