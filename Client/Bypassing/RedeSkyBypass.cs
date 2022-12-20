using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedBot.client.Bypassing
{
    class RedeSkyByPass
    {
        private MinecraftClient client;

        public RedeSkyByPass(MinecraftClient mc)
        {
            this.client = mc;
        }

        private static string[] comidas = new string[] {"257","260","391","392","349","392:2","349:1"};
        private static string[] ferramentas = new string[] { "292", "256", "257", "258"};
        private static string[] itensd = new string[] { "340", "397:3", "397" };

        public List<int> bypass()
        {
            Inventory inv = client.OpenWindow;
            if (inv == null || !inv.Title.StartsWith("Clique n")) return null;

            List<int> slots = new List<int>();
            Debug.WriteLine("c");
            String title = Utils.StripColorCodes(inv.Title);
            Debug.WriteLine(title);
            string itemName = title.Split(new char[] { ' ' })[2].Replace(".","");
            if(itemName.EqualsIgnoreCase("comidas") 
                || itemName.EqualsIgnoreCase("ferramentas")
                || itemName.EqualsIgnoreCase("itens"))
            {
                if (itemName.EqualsIgnoreCase("ferramentas"))
                {
                    for (int i = 0; i < inv.NumSlots; i++)
                    {
                        foreach (string itens in ferramentas)
                        {
                            int id = 0;
                            short data = 0;
                            if (itens.Contains(":"))
                            {
                                id = Convert.ToInt32(itens.Split(new char[] { ':' })[0]);
                                data = (short)Convert.ToInt32(itens.Split(new char[] { ':' })[1]);
                            }
                            else
                            {
                                id = Convert.ToInt32(itens);
                            }
                            ItemStack item = inv.Slots[i];
                            if (item == null) continue;
                            if (item.ID == id && item.Metadata == data)
                            {
                                slots.Add(i);

                            }
                        }
                    }
                }
                if (itemName.EqualsIgnoreCase("itens"))
                {
                    for (int i = 0; i < inv.NumSlots; i++)
                    {
                        foreach (string itens in itensd)
                        {
                            int id = 0;
                            short data = 0;
                            if (itens.Contains(":"))
                            {
                                id = Convert.ToInt32(itens.Split(new char[] { ':' })[0]);
                                data = (short)Convert.ToInt32(itens.Split(new char[] { ':' })[1]);
                            }
                            else
                            {
                                id = Convert.ToInt32(itens);
                            }
                            ItemStack item = inv.Slots[i];
                            if (item == null) continue;
                            if (item.ID == id && item.Metadata == data)
                            {
                                slots.Add(i);

                            }
                        }
                    }
                }
                if (itemName.EqualsIgnoreCase("comidas"))
                {
                    for (int i = 0; i < inv.NumSlots; i++)
                    {
                        foreach(string itens in comidas)
                        {
                            int id = 0;
                            short data = 0;
                            if (itens.Contains(":"))
                            {
                                id = Convert.ToInt32(itens.Split(new char[] { ':' })[0]);
                                data = (short)Convert.ToInt32(itens.Split(new char[] { ':' })[1]);
                            }
                            else
                            {
                                id = Convert.ToInt32(itens);
                            }
                            ItemStack item = inv.Slots[i];
                            if (item == null) continue;
                            if (item.ID == id && item.Metadata == data)
                            {
                                slots.Add(i);

                            }
                        }
                    }
                }
            }
            else {
                string sid = getItemID(itemName);
                if (sid == null) return null;
                int id = 0;
                short data = 0;
                if (sid.Contains(":"))
                {
                    id = Convert.ToInt32(sid.Split(new char[] { ':' })[0]);
                    data = (short) Convert.ToInt32(sid.Split(new char[] { ':' })[1]);
                }
                else
                {
                    id = Convert.ToInt32(sid);
                }

                Debug.WriteLine(itemName + ":" + getItemID(itemName));
                for (int i = 0; i < inv.NumSlots; i++)
                {
                    ItemStack item = inv.Slots[i];
                    if (item == null) continue;
                    if (item.ID == id && item.Metadata == data)
                    {
                        slots.Add(i);

                    }
                }
            }
            
            return slots;
        }

        private static string getItemID(String name)
        {

            switch (name.ToLower())
            {
                case "arco": return "261";
                case "maça": return "260";
                case "maçã": return "260";
                case "espada": return "267";
                case "baú": return "54";
                case "flor": return "37";
                case "biscoito": return "257";
                case "cenoura": return "391";
                case "batata": return "392";
                case "peixe": return "349";
                case "peixe-palhaço": return "392:2";
                case "salmão cru": return "349:1";
                case "enxada": return "292";
                case "pá": return "256";
                case "picareta": return "257";
                case "machado": return "258";
                case "livro": return "340";
                case "cabeça": return "397";
            }
            return null;
        }
        
    }
}
