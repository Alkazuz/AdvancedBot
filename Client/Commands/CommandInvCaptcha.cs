using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedBot.client.Commands
{
    public class CommandInvCaptcha : CommandBase
    {
        List<int> slots = null;
        private int slotIndex = -1;
        private int tick = 0;
        private Inventory inv;

        public CommandInvCaptcha(MinecraftClient cli)
            : base(cli, "InvCaptcha", "Burla o captcha de um servidor", "captcha", "invcaptcha")
        {
            SetParams("<nome-do-sv (skysurvival, worldcraft, tintaantibot, mastercraft, redeslow, partyredesky, landwars, *)>");
        }
        public override CommandResult Run(string alias, string[] args)
        {
            if (args.Length < 1) return CommandResult.MissingArgs;
            switch (args[0].ToLower()) {
                case "skysurvival": slots = Bypassing.SkySurvival.GetSlotsToClick(Client); break;
                case "mastercraft": slots = new Bypassing.MasterCraftBypass(Client).bypass(); break;
                case "partyredesky": slots = new Bypassing.RedeSkyByPass(Client).bypass(); break;
                case "redeslow": slots = new Bypassing.RedeSlowBypass(Client).bypass(); break;
                case "worldcraft":  slots = Bypassing.WorldCraftBP.GetSlotsToClick(Client); break;
                case "landwars": slots = new Bypassing.LandwarsBypass(Client).bypass(); break;
                case "tintaantibot": slots = Bypassing.HealpBypass.GetSlotsToClick(Client); break;
                case "*": slots = new Bypassing.AllBypass(Client).bypass(); break;
                default:
                    Client.PrintToChat("§cBypass não encontrado.");
                    return CommandResult.ErrorSilent;
            }
            if (slots == null) {
                Client.PrintToChat("§cInventário inválido.");
                return CommandResult.ErrorSilent;
            }
            inv = Client.OpenWindow;
            slotIndex = 0;
            return CommandResult.Success;
        }
        public override void Tick()
        {
            if (slots != null && slotIndex >= 0 && tick++ % 21 == 0) { //1050ms
                try
                {
                    Inventory.ClickedItem = null;
                    inv.Click(Client, (short)slots[slotIndex++], false);

                    if (slotIndex >= slots.Count)
                    {
                        slots = null;
                        slotIndex = -1;
                        tick = 0;
                    }
                }
                catch (Exception ed) { }
            }
        }
    }
}
