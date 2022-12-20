using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace AdvancedBot.client.Bypassing
{
    public class WorldCraftBP
    {
        public static List<int> GetSlotsToClick(MinecraftClient cli)
        {
            Inventory inv = cli.OpenWindow;
            if (inv == null || !inv.Title.StartsWith("Clique n")) return null;

            List<int> slots = new List<int>();

            String title = Utils.StripColorCodes(inv.Title);
            Debug.WriteLine(title);
            string itemName = title.Substring(title.IndexOf(':') + 2).ToLower();
            int id = getID(itemName);

            Debug.WriteLine(itemName + ":" + getID(itemName));
            for (int i = 0; i < inv.NumSlots; i++) {
                ItemStack item = inv.Slots[i];
                if (item == null) continue;

                if (item.ID == id) {
                    slots.Add(i);
                    break;
                }
            }
            return slots;
        }

        private static int getID(String name)
        {
            switch (name) {
                case "fornalha": return 61;
                case "bancada de trabalho": return 58;
                case "areia": return 12;
                case "abóbora": return 86;
                case "mesa de encantamentos": return 116;
                case "pedregulho": return 4;
                case "vidro": return 20;
                case "lã": return 35;
                case "jukebox": return 84;
                case "bigorna": return 145;
                case "pedra do fim": return 121;
                case "fardo de ferro": return 42;
                case "bloco de ferro": return 42;
                case "bloco de ouro": return 41;
                case "bloco de grama": return 2;
                case "bloco de esmeralda": return 133;
                case "rocha matriz": return 7;
                case "baú": return 54;
                case "dinamite": return 46;
                case "bloco de quartzo": return 155;
                case "pedra": return 1;
                case "fardo de feno": return 170;
                case "estante": return 47;
                case "tijolos": return 45;
                case "bloco de lápis-lazúli": return 22;
                case "terra": return 3;
                case "baú do fim": return 130;
                case "pedra luminosa": return 89;
                default: return -1;
            }
        }
    }
}
