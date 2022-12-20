using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;

namespace AdvancedBot.client.Bypassing
{
    public class HealpBypass
    {

        private static int getItemID(String name) {
            
                switch (name.ToLower()) {
                    case "arco": return 261;
                    case "maça": return 260;
                    case "peixe": return 349;
                    case "flor": return 37;
                    case "maçã": return 260;
                     case "espada": return 267;
                 case "baú": return 54;
                case "biscoito": return 257;
                case "cenoura": return 391;
                case "batata": return 392;
                case "enxada": return 292;
                case "pá": return 56;
                case "pa": return 56;
                case "picareta": return 257;
                case "machado": return 258;
                case "livro": return 340;
                case "cabeça": return 397;
                case "arvore": return 6;
                case "enderchest": return 130;
                case "fornalha": return 61;
            }
                return 1;
         }

            public static List<int> GetSlotsToClick(MinecraftClient cli)
            {
                Inventory inv = cli.OpenWindow;
                if (inv == null || !inv.Title.StartsWith("Clique no(a)")) return null;

                List<int> slots = new List<int>();

                String title = Utils.StripColorCodes(inv.Title);
                Debug.WriteLine(title);
                string itemName = title.Split(new char[] { ' ' })[2];
                int id = getItemID(itemName);

                Debug.WriteLine(itemName + ":" + getItemID(itemName));
                for (int i = 0; i < inv.NumSlots; i++)
                {
                    ItemStack item = inv.Slots[i];
                    if (item == null) continue;

                    if (item.ID == id)
                    {
                        slots.Add(i);
                        break;
                    }
                }
                return slots;
            }
        }
}
