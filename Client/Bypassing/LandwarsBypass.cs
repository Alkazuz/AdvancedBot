using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedBot.client.Bypassing
{
    class LandwarsBypass
    {
        private MinecraftClient client;

        public LandwarsBypass(MinecraftClient mc)
        {
            this.client = mc;
        }

        public List<int> bypass()
        {
            Inventory inv = client.OpenWindow;
            if (inv == null || !inv.Title.StartsWith("Captcha")) return null;

            List<int> slots = new List<int>();
            ItemStack iten = null;
            for (int i = 0; i < inv.NumSlots; i++)
            {
                ItemStack item = inv.Slots[i];
                if (item == null) continue;

                if (item.HasDisplayName() && Utils.StripColorCodes(item.GetDisplayName()).EqualsIgnoreCase("sistema de proteção"))
                {
                    iten = item;
                    break;
                }
            }
            String resolve = Utils.StripColorCodes(iten.GetLore()).Split(new[] { "Quanto é " }, StringSplitOptions.None)[1].Trim();
            Debug.WriteLine(resolve);
            int result = resolveString(resolve);
            Debug.WriteLine(result);
            for (int i = 0; i < inv.NumSlots; i++)
            {
                ItemStack item = inv.Slots[i];
                if (item == null) continue;

                if (item.HasDisplayName() && Utils.StripColorCodes(item.GetDisplayName()).EqualsIgnoreCase("a resposta é:"))
                {
                    if (Utils.StripColorCodes(item.GetLore()).Contains(Convert.ToString(result)))
                    {
                        slots.Add(i);
                    }
                }
            }
            return slots;
        }

        public int resolveString(String str)
        {
            int n1 = Convert.ToInt32(str.Split(new char[] { ' ' })[0]);
            int n2 = Convert.ToInt32(str.Split(new char[] { ' ' })[2]);
            string symbol = str.Split(new char[] { ' ' })[1];
            switch (symbol.ToLower())
            {
                case "+": return n1 + n2;
                case "-": return n1 - n2;
                case "*": return n1 * n2;
                case "x": return n1 * n2;
                case "/": return n1 / n2;
            }
            return -1;
        }

    }

}