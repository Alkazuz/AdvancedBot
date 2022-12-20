using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedBot.client.Bypassing
{
    class RedeSlowBypass
    {
        private MinecraftClient client;

        public RedeSlowBypass(MinecraftClient mc)
        {
            this.client = mc;
        }

        public List<int> bypass()
        {
            Inventory inv = client.OpenWindow;
            if (inv == null || !inv.Title.StartsWith("Clique no(a) ")) return null;

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
                    
                }
            }
            return slots;
        }

        private static int getItemID(String name)
        {

            switch (name.ToLower())
            {
                case "arco": return 261;
                case "maça": return 260;
                case "maçã": return 260;
                case "espada": return 267;
                case "baú": return 54;
                case "peixe": return 349;
                case "flor": return 37;
                case "machado": return 258;
                case "osso": return 352;
                case "capacete": return 310;
                case "livro": return 340;
                case "ovo": return 344;
            }
            return 1;
        }
        
    }
}
