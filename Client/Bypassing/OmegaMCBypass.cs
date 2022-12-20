using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedBot.client.Bypassing
{
    public class OmegaMCBypass : ServerBypassBase
    {
        public override ClientVersion Version => ClientVersion.v1_8;
        public OmegaMCBypass(MinecraftClient cli) : base(cli)
        {
            cli.OnTick += OnTick;
        }
        private void OnTick()
        {
            if(Client.OpenWindow != null && Client.OpenWindow.Title.ContainsIgnoreCase("captcha")) {
                int slot = GetSlotToClick();
                if(slot == -1) {
                    Client.PrintToChat("§cNão foi possível burlar o captcha.");
                } else {
                    Client.OpenWindow.Click(Client, (short)slot, false);
                    Client.PrintToChat("§aO captcha foi burlado!");
                }

                Client.OnTick -= OnTick;
                IsFinished = true;
            }
        }

        private int GetSlotToClick()
        {
            var inv = Client.OpenWindow;
            for (int i = 0; i < inv.NumSlots; i++) {
                ItemStack stack = inv.Slots[i];
                if(stack != null) {
                    string lore = Utils.StripColorCodes(stack.GetLore());
                    if (lore.Contains("Clique aqui")) {
                        return i;
                    }
                }
            }
            return -1;
        }
    }
}
