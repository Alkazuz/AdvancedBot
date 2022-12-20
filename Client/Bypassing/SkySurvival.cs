using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace AdvancedBot.client.Bypassing
{
    public class SkySurvival
    {
        private const int IGNORE_METADATA = 0x100;
        private const int MATCH_SINGLE    = 0x001;
        private const int MATCH_ANY       = 0x002;
        private const int MATCH_ALL       = 0x004;
        private static Dictionary<string, int[]> ItemNames = new Dictionary<string, int[]>()
        {
            {"livro",       new int[] { MATCH_SINGLE|IGNORE_METADATA, Items.book << 4 }},
            {"batata",      new int[] { MATCH_SINGLE|IGNORE_METADATA, Items.potato << 4 }},
            {"biscoito",    new int[] { MATCH_SINGLE|IGNORE_METADATA, Items.cookie << 4 }},
            {"cenoura",     new int[] { MATCH_SINGLE|IGNORE_METADATA, Items.carrot << 4 }},
            {"maçã",        new int[] { MATCH_SINGLE|IGNORE_METADATA, Items.apple << 4 }},
            {"cabeça",      new int[] { MATCH_SINGLE|IGNORE_METADATA, Items.skull << 4 }},
            {"cabeças",     new int[] { MATCH_ALL|IGNORE_METADATA, Items.skull << 4}},
            {"peixe",       new int[] { MATCH_SINGLE|IGNORE_METADATA, Items.fish << 4 }},
            {"picareta",    AnyItem(MATCH_ANY|IGNORE_METADATA, true, "_pickaxe")},
            {"machado",     AnyItem(MATCH_ANY|IGNORE_METADATA, true, "_axe")},
            {"pá",          AnyItem(MATCH_ANY|IGNORE_METADATA, true, "_shovel") },
            {"enchada",     AnyItem(MATCH_ANY|IGNORE_METADATA, true, "_hoe") },
            {"comidas",     Append(AnyItem(MATCH_ALL|IGNORE_METADATA, false, "cooked"), Items.apple << 4, Items.cookie << 4) },
            {"ferramentas", AnyItem(MATCH_ALL|IGNORE_METADATA, true, "_axe", "_shovel", "_hoe", "_pickaxe") },
            {"peixes",      new int[] { MATCH_ALL|IGNORE_METADATA, Items.fish << 4}}
        };

        private static int[] AnyItem(int mode, bool ends, params string[] q) {
            List<int> items = new List<int>();
            items.Add(mode);
            foreach (FieldInfo f in typeof(Items).GetFields())
                if (StartsOrEndsWithAny(ends, f.Name, q))
                    items.Add((int)f.GetValue(null) << 4);
            return items.ToArray();
        }
        private static bool StartsOrEndsWithAny(bool ends, string str, string[] with){
            foreach(string w in with)
                if(ends ? str.EndsWith(w) : str.StartsWith(w)) return true;
            return false;
        }
        private static int[] Append(int[] a, params int[] b)
        {
            int[] n = new int[a.Length + b.Length];
            Array.Copy(a, 0, n, 0, a.Length);
            Array.Copy(b, 0, n, a.Length, b.Length);
            return n;
        }

        public static List<int> GetSlotsToClick(MinecraftClient client)
        {
            Inventory inv = client.OpenWindow;
            if (inv == null || !inv.Title.StartsWith("Clique n")) return null;

            List<int> slots = new List<int>();

            string title = inv.Title;

            int[] names;
            int idx = title.LastIndexOf(' ') + 1;
            string key = title.Substring(idx, title.Length - idx - 1);
            if (ItemNames.TryGetValue(key, out names)) {
                int flags = names[0];
                for (int i = 0; i < inv.NumSlots; i++) {
                    if (Match(flags, inv.Slots[i], names)) {
                        slots.Add(i);
                        if ((flags & MATCH_SINGLE) != 0) break;
                    }
                }
            }
            return slots;
        }
        private static bool Match(int flags, ItemStack item, int[] items)
        {
            if (item == null) return false;

            for (int i = 1; i < items.Length; i++) {
                int id = items[i] >> 4;
                int meta = items[i] & 0xF;
                if (item.ID == id) 
                    return (flags & IGNORE_METADATA) == 0 ? item.Metadata == meta : true;
            }
            return false;
        }
    }
}
