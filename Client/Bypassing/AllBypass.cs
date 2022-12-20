using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AdvancedBot.client.Bypassing
{
    class AllBypass
    {
        private MinecraftClient client;

        public AllBypass(MinecraftClient mc)
        {
            this.client = mc;
        }

        public List<int> bypass()
        {
            Inventory inv = client.OpenWindow;
            if (inv == null) return null;

            List<int> slots = new List<int>();

            String title = Utils.StripColorCodes(inv.Title);
            string itemName = "";
            title = new String(title.Where(c => Char.IsLetter(c) || c.Equals(' ')).ToArray());
            Debug.WriteLine("title: "+title);
            for (int i = 0; i < title.Split(new char[] { ' ' }).Length; i++)
            {
                if (getItemID(title.Split(new char[] { ' ' })[i].Replace(".","")) != null)
                {
                    itemName = title.Split(new char[] { ' ' })[i].Replace(".", "");
                }
            }
            
                string sid = getItemID(itemName);
                if (sid == null) return null;
                int id = 0;
                short data = 0;
                if (sid.Contains(","))
                {
                    for (int i = 0; i < title.Split(new char[] { ',' }).Length; i++)
                    {
                         id = 0;
                         data = 0;
                        if (sid.Contains(":"))
                        {
                            id = Convert.ToInt32(sid.Split(new char[] { ':' })[0]);
                            data = (short)Convert.ToInt32(sid.Split(new char[] { ':' })[1]);
                        }
                        else
                        {
                            id = Convert.ToInt32(sid);
                        }
                        for (int iv = 0; iv < inv.NumSlots; i++)
                        {
                            ItemStack item = inv.Slots[iv];
                            if (item == null) continue;
                            if (item.ID == id && item.Metadata == data)
                            {
                                slots.Add(iv);

                            }
                        }
                    }
                    return slots;
                }
                 id = 0;
                 data = 0;
                if (sid.Contains(":"))
                {
                    id = Convert.ToInt32(sid.Split(new char[] { ':' })[0]);
                    data = (short)Convert.ToInt32(sid.Split(new char[] { ':' })[1]);
                }
                else
                {
                    id = Convert.ToInt32(sid);
                }
                for (int i = 0; i < inv.NumSlots; i++)
                {
                    ItemStack item = inv.Slots[i];
                    if (item == null) continue;
                    if (item.ID == id && item.Metadata == data)
                    {
                        slots.Add(i);

                    }
                }

            return slots;
        }

        private static string getItemID(String name)
        {

            switch (name.ToLower())
            {
                case "comidas": return "260,282,297,319,320,322,322:1,349,349:1,349:2,349:3,350,350:1,354,357,360,363,364,365,366,367,391,392,393,394,396,398,400,411,412,413,423,424,432,434,436";
                case "comida": return "260,282,297,319,320,322,322:1,349,349:1,349:2,349:3,350,350:1,354,357,360,363,364,365,366,367,391,392,393,394,396,398,400,411,412,413,423,424,432,434,436";
                case "spawner": return "52,52:50,52:51,52:52,52:53,52:54,52:55,52:56,52:57,52:58,52:59,52:60,52:61,52:62,52:63,52:64,52:65,52:66,52:90,52:91,52:92,52:93,52:94,52:95,52:96,52:97,52:98,52:99,52:100";
                case "spawners": return "52,52:50,52:51,52:52,52:53,52:54,52:55,52:56,52:57,52:58,52:59,52:60,52:61,52:62,52:63,52:64,52:65,52:66,52:90,52:91,52:92,52:93,52:94,52:95,52:96,52:97,52:98,52:99,52:100";
                case "gerador": return "52,52:50,52:51,52:52,52:53,52:54,52:55,52:56,52:57,52:58,52:59,52:60,52:61,52:62,52:63,52:64,52:65,52:66,52:90,52:91,52:92,52:93,52:94,52:95,52:96,52:97,52:98,52:99,52:100";
                case "geradores": return "52,52:50,52:51,52:52,52:53,52:54,52:55,52:56,52:57,52:58,52:59,52:60,52:61,52:62,52:63,52:64,52:65,52:66,52:90,52:91,52:92,52:93,52:94,52:95,52:96,52:97,52:98,52:99,52:100";
                case "ferramentas": return "256,257,258,259,261,269,270,271,273,274,275,277,278,279,284,285,286,290,291,292,293,294,345,346,347,358,359,395,398,420,449,450,452";
                case "ferramenta":  return "256,257,258,259,261,269,270,271,273,274,275,277,278,279,284,285,286,290,291,292,293,294,345,346,347,358,359,395,398,420,449,450,452";
                case "decorativos": return "5,22,24,25,31,32,35,37,38,41,42,43,44,45,48,53,55,57,58,78,79,80,81,90,91,95,98,99,100,101,102,106,107,108,109,110,111,112,113,114,115,116,117,119,120,121,133,134,135,136,139,140,155,156,159,160,161,162,163,164,165,166,168,169,171,172,173,174,175,176,177,178,179,181,182,183,184,185,186,187,198,199,200,201,202,203,204,205,206,208,209,210,211,212,213,214,215,216,235,236,237,238,239,240,241,242,243,244,245,246,247,248,249,250,251,252,321,389,390,397,405,416,425,427,428,429,430,431";
                case "decorativo": return "5,22,24,25,31,32,35,37,38,41,42,43,44,45,48,53,55,57,58,78,79,80,81,90,91,95,98,99,100,101,102,106,107,108,109,110,111,112,113,114,115,116,117,119,120,121,133,134,135,136,139,140,155,156,159,160,161,162,163,164,165,166,168,169,171,172,173,174,175,176,177,178,179,181,182,183,184,185,186,187,198,199,200,201,202,203,204,205,206,208,209,210,211,212,213,214,215,216,235,236,237,238,239,240,241,242,243,244,245,246,247,248,249,250,251,252,321,389,390,397,405,416,425,427,428,429,430,431"; ;
                case "armadura": return "298,299,300,301,302,303,304,305,306,307,308,309,310,311,312,313,314,315,316,317,417,418,419,442,443";
                case "armaduras": return "298,299,300,301,302,303,304,305,306,307,308,309,310,311,312,313,314,315,316,317,417,418,419,442,443";
                case "minerio":
                    return "14,15,16,21,56,73,74,129";
                        case "minerios":
                    return "14,15,16,21,56,73,74,129";
                case "minério":
                    return "14,15,16,21,56,73,74,129";
                case "minérios":
                    return "14,15,16,21,56,73,74,129";
                case "arco": return "261";
                case "osso": return "352";
                case "maça": return "260,322,322:1";
                case "maçã": return "260,322,322:1";
                case "biscoito": return "257";
                case "cenoura": return "391";
                case "batata": return "392";
                case "peixe": return "349";
                case "peixe-palhaço": return "392:2";
                case "salmão cru": return "349:1";
                case "salmao cru": return "349:1";

                case "espada": return "267,268,272,276,283";
                case "enxada": return "292,290,291,293,294";
                case "pá": return "256,284,278,273,269";
                case "pa": return "256,284,278,273,269";
                case "machado": return "258,271,275,279,286";
                case "picareta": return "278,146,285,274,270";

                case "capacete": return "298,302,306,310,314";
                case "peitoral": return "315,311,307,303,299";
                case "calça": return "300,304,308,312,316";

                case "cama": return "26";
                case "teia": return "30";
                case "fornalha": return "61";
                case "neve": return "78,80";
                case "gelo": return "79";
                case "cana": return "83";
                case "cacto": return "81";
                case "arvore": return "60";
                case "cerca": return "85";
                case "abóbora": return "86";
                case "abobora": return "86";
                case "folha": return "161,161:1";
                case "folhas": return "161,161:1";
                case "funil": return "154";
                case "porta": return "64";
                case "placa": return "63";
                case "dinamite": return "46";
                case "tnt": return "46";

                case "baú": return "54,146";
                case "enderchest": return "130";
                case "bau": return "54,146";

                case "flor": return "37,38,38:1,38:2,38:3,38:4,38:5,38:6,38:7,38:8";

                case "livro": return "340";
                case "cabeça": return "397";
                case "cabeca": return "397";
            }
            return null;
        }

    }
}
