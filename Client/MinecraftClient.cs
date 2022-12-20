using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using AdvancedBot.client.Crypto;
using AdvancedBot.client.Handler;
using AdvancedBot.client.Map;
using AdvancedBot.client.Packets;
using AdvancedBot.client.PathFinding;
using System.Threading.Tasks;
using System.Net;
using Ionic.Zlib;
using System.Linq;
using System.Collections.Concurrent;
using AdvancedBot.client.Bypassing;
using AdvancedBot.Json;
using AdvancedBot.Plugins;

namespace AdvancedBot.client
{
    /*
     * 1.12.1: http://wiki.vg/index.php?title=Protocol&oldid=13325
     * 1.8:    http://wiki.vg/index.php?title=Protocol&oldid=7368, http://wiki.vg/index.php?title=Entities&oldid=6366
     * 1.7:    http://wiki.vg/index.php?title=Protocol&oldid=5486
     */
    public enum ClientVersion : int
    {
        v1_5_2  = 999900152,
        v1_7    = 4,
        v1_7_10 = 5,

        v1_8    = 47,

        v1_9   = 107,
       /* v1_9_1 = 108,
        v1_9_2 = 109,
        v1_9_3 = 110,

        v1_10 = 210,

        v1_11 = 315,
        v1_11_1 = 316,

        v1_12 = 335,*/
        v1_12_1 = 338,
        v1_12_2 = 340,

        Unknown = -1
    }
    public enum ConnectionState
    {
        Disconnected,
        Connecting,
        Connected
    }
    public class MinecraftClient : IDisposable
    {
        public static ClientVersion ParseVersion(string ver)
        {
            if (!Enum.TryParse("v" + ver.Replace('.', '_'), out ClientVersion cver))
                cver = ClientVersion.Unknown;

            return cver;
        }

        public static bool AutoReconnect = false;
        public static bool Knockback = true;

        public static bool MultiPing = false;
        public bool autoLogin = false;
        public string IP;
        public string RealIP;
        public ushort Port;

        public List<ChatMessage> JSONMessage = new List<ChatMessage>();
        public Proxy ConProxy;
        public string Email;
        public string Username;
        public string Password;
        public string realServer;
        public string CmdRegister = "/register abc123 abc123";
        public string CmdLogin = "/login abc123";

        public event Action OnTick;

        private ServerBypassBase bypasser;

        /// <summary>
        /// used by the handling system in v1.5.2
        /// </summary>
        private ConcurrentQueue<IPacket> pktQueue;

        // used on newer versions
        private PacketStream ps;
        public PacketStream Stream => ps;

        public CommandManagerNew CmdManager;

        public PathGuide CurrentPath = null;
        public PlayerManager PlayerManager = new PlayerManager();
        public Entity Player;

        public int PlayerID;
        public sbyte Dimension;

        public int Gamemode;

        public LoginResponse LoginResp = null;

        public World World;

        private bool canLoop = false;

        private Thread updateThread;
        private Timer ticker;

        private int keepAliveTicks = 0;
        
        public ConnectionState ConnState { get; private set; } = ConnectionState.Disconnected;

        private int hslot;
        public int HotbarSlot
        {
            get { return hslot; }
            set {
                if (value < 0 || value > 8) throw new ArgumentOutOfRangeException();
                if (IsBeingTicked())
                    SendPacket(new PacketHeldItemChange((short)value));
                hslot = value;
            }
        }

        public bool LimitChunks = false;
        public const int CHUNK_LIMIT = (2 * 16) * (2 * 16); //pow(n * 16, 2)

        public Inventory OpenWindow = null;

        public bool chatJsonUpdate = true;
        public Inventory Inventory = new Inventory(45);
        public ItemStack ItemInHand
        {
            get { return Inventory.Slots[36 + (hslot > 8 ? 8 : hslot < 0 ? 0 : hslot)]; }
        }

        public ClientVersion Version = ClientVersion.v1_7;
        public bool MapAndPhysics = true;
        public bool SendPing = false;

        private static Regex EMAIL_REGEX = new Regex(@".+@.+\..+", RegexOptions.Compiled);

        public ProtocolHandler Handler;
        public MinecraftClient( string ip, ushort port, string userOrEmail, string password, Proxy proxy)
        {
            IP = ip;
            Port = port;
            if (EMAIL_REGEX.IsMatch(userOrEmail))
            {
                Email = userOrEmail;
            }
            else
            {
                Username = userOrEmail;
            }
            Password = password;
            ConProxy = proxy;
            CmdManager = new CommandManagerNew(this);
            World = new World(this);
        }
        public MinecraftClient(string r,string ip, ushort port, string userOrEmail, string password, Proxy proxy)
        {
            realServer = r;
            IP = ip;
            Port = port;
            if (EMAIL_REGEX.IsMatch(userOrEmail)) {
                Email = userOrEmail;
            } else {
                Username = userOrEmail;
            }
            Password = password;
            ConProxy = proxy;
            CmdManager = new CommandManagerNew(this);
            World = new World(this);
        }
        
        public void StartClient()
        {
            try {

                foreach (IPlugin plugin in Program.pluginManager.plugins.Values)
                {
                    plugin.onClientConnect(this);
                }

                if (ticker == null) {
                    ticker = new Timer(TimerTick, null, 0, 50);
                }
                canLoop = false;
                if (ConnState != ConnectionState.Disconnected) {
                    try {
                        if (Handler != null && Version == ClientVersion.v1_5_2) {
                            ((Handler_v152)Handler).TcpConn.Close();
                        } else {
                            ps.Dispose();
                        }
                    } catch { }
                }
                ConnState = ConnectionState.Connecting;

                if (updateThread != null && updateThread.IsAlive) {
                    try {
                        updateThread.Join(1000);
                        updateThread.Abort();
                    } catch { }
                }
                updateThread = null;

                canLoop = true;
                PlayerManager.Clear();
                authmeCounter = 0;

                Handler = ProtocolHandler.Create(Version, this);
                if (Handler == null) {
                    throw new NotImplementedException("Esta versão ainda não foi implementada!");
                }
                
                if (Handler.HandlerVersion == ClientVersion.v1_5_2) {
                    updateThread = new Thread(Connect_v15) {
                        Name = string.Format("MC152 {0}", Username)
                    };
                    updateThread.Start();
                } else {
                    Connect();
                }
            } catch (Exception ex) {
                PrintToChat("Não foi possível conectar-se ao servidor: " + ex.ToString());
            }
        }

        private void Connect_v15()
        {
            try {
                if (Email != null && LoginResp == null) {
                    LoginResp = SessionUtils.Login(Email, Password, ConProxy);
                    if (LoginResp.Error)
                        throw new IOException("Não foi possivel efetuar a autenticação");
                    Username = LoginResp.Username;
                }

                World.Clear();
                PlayerManager.Clear();
                Player = new Entity(this);

                Handler_v152 h = (Handler_v152)Handler;

                if (SendPing) h.Ping(IP, Port, ConProxy);

                TcpClient c = ConProxy == null ? new TcpClient(IP, Port) : ConProxy.Connect(IP, Port);

                c.ReceiveTimeout = 30000;
                c.SendTimeout = 30000;

                pktQueue = new ConcurrentQueue<IPacket>();

                handshakeState = 1;

                h.TcpConn = c;
                if (h.ConnectAndHandshake()) {
                    keepAliveTicks = 0;
                    try {
                        WriteBuffer wb = new WriteBuffer();
                        int itr;
                        while (h.TcpConn.Connected && canLoop) {
                            for (itr = 0; h.NetStream.DataAvailable && canLoop && itr < 128; itr++) {
                                if (!h.ReadAndHandlePacket()) {
                                    ConnState = ConnectionState.Disconnected;
                                    return;
                                }
                            }
                            if (keepAliveTicks > 30 * 25) throw new IOException("Conexão perdida: timeout");
                            
                            while(pktQueue.Count > 0) {
                                if(pktQueue.TryDequeue(out IPacket p)) {
                                    p.WritePacket(wb, this);
                                    wb.Reset();
                                }
                            }

                            Thread.Sleep(10);
                        }
                        ConnState = ConnectionState.Disconnected;
                        if (canLoop) {
                            throw new IOException("Conexão perdida");
                        }
                    } catch (Exception ex) {
                        World.Clear();
                        ConnState = ConnectionState.Disconnected;
                        if (!(ex is ThreadAbortException)) {
                            Program.CreateErrLog(ex, "clienterror");
                            PrintToChat("Erro no loop principal: \n" + ex.ToString());
                        }
                    }
                }
            } catch (Exception ex) {
                PrintToChat("Não foi possível conectar-se ao servidor: " + ex.ToString());
                ConnState = ConnectionState.Disconnected;
            }
        }

        private async void Connect()
        {
            try {
                World.Clear();
                PlayerManager.Clear();
                Player = new Entity(this);

                bypasser = ServerBypassBase.NewInstance(this);

                if (Email != null && LoginResp == null) {
                    LoginResp = await SessionUtils.LoginAsync(Email, Password, ConProxy).ConfigureAwait(false);
                    if (LoginResp.Error) {
                        throw new IOException("Não foi possivel efetuar a autenticação");
                    }
                    Username = LoginResp.Username;
                }

                if (SendPing) await Ping().ConfigureAwait(false);

                Socket sock;
                if(ConProxy == null) {
                    sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    await Task.Factory.FromAsync(sock.BeginConnect, sock.EndConnect, IP, (int)Port, null).ConfigureAwait(false);
                } else {
                    sock = await ConProxy.ConnectAsync(IP, Port).ConfigureAwait(false);
                }
                sock.ReceiveTimeout = 30000;
                sock.SendTimeout = 30000;

                ps = new PacketStream(sock);

                SendPacket(new PacketHandshake((int)Version, IP, Port, 2));
                SendPacket(new PacketLoginStart(Username));

                handshakeState = 0;
                keepAliveTicks = 0;
                handshakeStart = Utils.GetTimestamp();

                ps.OnPacketAvailable += HandlePacket;
                ps.OnError += (err) => {
                    Disconnect("Erro de rede: \n" + err.ToString());
                };

                keepAliveTicks = 0;
                handshakeStart = Utils.GetTimestamp();
            } catch (Exception ex) {
                PrintToChat("Não foi possível conectar-se ao servidor: " + ex.ToString());
                ConnState = ConnectionState.Disconnected;
            }
        }

        private int handshakeState;
        long handshakeStart, connectedTime;
        private void HandlePacket(ReadBuffer pkt)
        {
            //if (pkt.ID != 0x21 && pkt.ID != 0x26) {
            //    PacketLogger.Log(pkt);
            //}
            foreach (IPlugin plugin in Program.pluginManager.plugins.Values)
            {

                plugin.OnReceivePacket(pkt, this);
                pkt.Reset();
            }
            if (handshakeState == 0) {
                switch (pkt.ID) { 
                    case 0x00: //Disconnect
                        HandlePacketDisconnect(ChatParser.ParseJson(pkt.ReadString()));
                        return;
                    case 0x01: //Encryption request
                        if (LoginResp == null) {
                            throw new Exception("O servidor não suporta o modo offline!");
                        }
                        string serverID = pkt.ReadString();
                        byte[] serverKey = pkt.ReadByteArray(pkt.ReadVarInt());
                        byte[] token = pkt.ReadByteArray(pkt.ReadVarInt());

                        using (RSACryptoServiceProvider rsa = CryptoUtils.DecodeRSAPublicKey(serverKey)) {
                            byte[] secretKey = CryptoUtils.GenerateAESPrivateKey();
                            SessionUtils.CheckSession(LoginResp.UUID, LoginResp.AccessToken, CryptoUtils.GetServerHash(serverID, serverKey, secretKey), ConProxy/*null */);

                            byte[] sharedSecret = rsa.Encrypt(secretKey, false);
                            byte[] verifyToken = rsa.Encrypt(token, false);

                            SendPacket(new PacketEncryptionResponse(sharedSecret, verifyToken));

                            ps.InitEncryption(secretKey);
                        }
                        break;
                    case 0x02: //Login success
                        pkt.ReadString();
                        pkt.ReadString();
                        handshakeState = 1;
                        connectedTime = Utils.GetTimestamp();
                        break;
                    case 0x03: //Set compression
                        ps.CompressionThreshold = pkt.ReadVarInt();
                        break;
                    default:
                        ps.Dispose();
                        throw new InvalidDataException("O servidor enviou um pacote desconhecido.\n (connStatus=0) ID: 0x" + pkt.ID.ToString("X"));
                }
            } else {
                if (!Handler.HandlePacket(pkt)) {
                    ConnState = ConnectionState.Disconnected;
                }
                if (bypasser != null) {
                    pkt.Reset();
                    bypasser.HandlePacket(pkt);
                }
            }
        }

        private async Task Ping()
        {
            try {
                int count = MultiPing ? 4 : 1;
                for (int i = 0; i < count; i++) {
                    if (count != 1) {
                        PrintToChat($"§fEnviando ping {i + 1} de {count}...");
                    }
                    if(!await OnePing().ConfigureAwait(false)) {
                        PrintToChat($"Erro ao enviar o ping #{i + 1}");
                    }

                    if (count != 1) {
                        await Task.Delay(1000).ConfigureAwait(false);
                    }
                }
            } catch {
                PrintToChat("§cNão foi possivel enviar o ping");
            }
        }
        private async Task<bool> OnePing()
        {
            Socket sock;
            if (ConProxy == null) {
                sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                await Task.Factory.FromAsync(sock.BeginConnect, sock.EndConnect, IP, (int)Port, null).ConfigureAwait(false);
                sock.ReceiveTimeout = 5000;
                sock.SendTimeout = 5000;
            } else {
                sock = await ConProxy.ConnectAsync(IP, Port).SetTimeout(10000).ConfigureAwait(false);
            }

            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            using (PacketStream ps = new PacketStream(sock)) {

                ps.OnPacketAvailable += (pkt) => {
                    tcs.SetResult(pkt.ID == 0x00 && pkt.ReadString().EndsWith("}"));
                };

                WriteBuffer don = new WriteBuffer();
                new PacketHandshake((int)Version, IP, Port, 1).WritePacket(don, this);
                ps.SendPacket(don);
                don.Reset();
                don.WriteVarInt(0x00);
                ps.SendPacket(don);

                return await tcs.Task.SetTimeout(15000).ConfigureAwait(false);
            }
        }

        public void SendPacket(IPacket pkt)
        {

            foreach (IPlugin plugin in Program.pluginManager.plugins.Values)
            {
                plugin.OnSendPacket(pkt, this);
            }

            if (Version == ClientVersion.v1_5_2) {
                pktQueue.Enqueue(pkt);
                return;
            }
            if (ps == null || !ps.Connected) {
                Debug.WriteLine("Tried to send an packet while disconnected");
                return;
            }
            ps.SendPacket(pkt, this);
        }

        public void Disconnect(string errMsg)
        {
            World.Clear();
            ConnState = ConnectionState.Disconnected;
            PrintToChat(errMsg);
            if (ps != null && ps.Connected) {
                //ps.Disconnect();
                ps.Dispose();
                ps = null;
            }
        }

        #region Server packet handlers
        internal void ResetKeepAlive()
        {
            keepAliveTicks = 0;
        }
        internal void HandlePacketJoinGame(int playerId, sbyte dimension, int gm)
        {
            PlayerID = playerId;
            Dimension = dimension;
            Gamemode = gm;

            World.Clear();
            PlayerManager.Clear();
            Player.ActivePotions.Clear();
            ConnState = ConnectionState.Connected;

            SendPacket(new PacketClientSettings(8));

            WriteBuffer brand = new WriteBuffer();
            brand.WriteString("vanilla");
            SendPacket(new PacketPluginMessage("MC|Brand", brand.GetBytes()));
            HotbarSlot = 0;
        }

        private int authmeCounter = 0;
        private string randomEmail = NickGenerator.RandomNick(16, 1).Replace('\n', 'a') + "@gmail.com";
        private int numEmptyMsgs = 0;
        internal void HandlePacketChat(string chat, byte pos)
        {
            if (pos != 2) {
                string stripped = Utils.StripColorCodes(chat);
                bool empty = string.IsNullOrWhiteSpace(stripped);
                numEmptyMsgs = empty ? numEmptyMsgs + 1 : 0;

                if (numEmptyMsgs < 3) {
                    PrintToChat(chat);
                    
                    int r = Utils.GetBits(authmeCounter, 0, 4);
                    int l = Utils.GetBits(authmeCounter, 4, 4);
                    
                    if (autoLogin && r < 2 && stripped.ContainsIgnoreCase(CmdRegister.Substring(0, CmdRegister.IndexOf(' ')))) {
                        var task = Task.Run(async () => {
                            Thread.Sleep(new Random().Next(3000, 5000));
                            SendMessage(CmdRegister.Replace("@email", randomEmail).Replace("@pass", Password));
                            Utils.SetBits(ref authmeCounter, r + 1, 0, 4);
                        });
                        
                    } else if (autoLogin && l < 2 && stripped.ContainsIgnoreCase(CmdLogin.Substring(0, CmdLogin.IndexOf(' ')))) {
                        var task = Task.Run(async () => {
                            Thread.Sleep(new Random().Next(3000, 5000));
                            SendMessage(CmdLogin.Replace("@email", randomEmail).Replace("@pass", Password));
                            Utils.SetBits(ref authmeCounter, l + 1, 4, 4);
                        });
                        
                    }
                }
            }
        }
        internal void HandlePacketRespawn(sbyte dimension, int gm)
        {
            Dimension = dimension;
            Gamemode = gm;

            World.Clear();
            //PlayerManager.Clear();
            Player.ActivePotions.Clear();
        }

        private int kickTicks = -1;
        internal void HandlePacketDisconnect(string reason)
        {
            Disconnect("Kick: " + reason);
            kickTicks = 0;
        }

        internal void SetHotbarSlot(int slot)
        {
            hslot = slot;
        }
        #endregion

        public bool IsBeingTicked()
        {
            return ConnState == ConnectionState.Connected;
        }

        public async void SetPath(int x, int y, int z, string errMsg = null)
        {
            CurrentPath = await PathGuide.CreateAsync(Player, x, y, z).ConfigureAwait(false);
            if (CurrentPath == null && errMsg != null) {
                PrintToChat(errMsg);
            }
        }

        internal bool UseExternalTickSource = false;
        private long lastExternalTick = 0;
        private void TimerTick(object timer)
        {
            if (!UseExternalTickSource || Utils.GetTimestamp() - lastExternalTick > 1000) {
                Tick();
            }
        }
        internal void Tick()
        {
            if (UseExternalTickSource) {
                lastExternalTick = Utils.GetTimestamp();
            }

            foreach (IPlugin plugin in Program.pluginManager.plugins.Values)
            {
                plugin.Tick();
            }

            OnTick?.Invoke();
            if(bypasser != null && bypasser.IsFinished) {
                bypasser = null;
            }
            if (ConnState == ConnectionState.Connected) {
                if (keepAliveTicks++ > 30 * 25) {
                    Disconnect("Conexão perdida: timeout");
                    return;
                }
                if (CurrentPath != null) {
                    CurrentPath.Tick();
                    if (CurrentPath.Finished())
                        CurrentPath = null;
                }
                CmdManager.Tick();

                if (MapAndPhysics) {
                    Player.Tick();

                    if (Player.WasSprinting != Player.IsSprinting) {
                        SendPacket(new PacketEntityAction(PlayerID, (byte)(Player.IsSprinting ? 3 : 4), 0));
                        Player.WasSprinting = Player.IsSprinting;
                    }
                    if (!World.ChunkExists(Player.BlockX >> 4, Player.BlockZ >> 4)) {
                        Player.MotionY = Player.AABB.MinY > 0 ? -0.1 : 0.0;
                    }
                    if (Player.PortalCentralizeTicks++ > 5 * 20 && Player.IsOnPortal() && Player.OnGround) {
                        var portal = Player.GetCollidingBlocks(Blocks.portal).OrderByDescending(a => a.Item1.Y).FirstOrDefault()?.Item1;
                        if (portal != null) {
                            double px = portal.X + 0.5;
                            double py = portal.Y + 0.5;
                            double pz = portal.Z + 0.5;
                            double d = Utils.DistTo(Player.PosX, 0, Player.PosZ, px, 0, pz);
                            if (d > 0.4) {
                                Player.LookTo(px, py, pz);
                                int ticks = (int)Math.Round(d / 0.21); //~4.31 / 20
                                //Debug.WriteLine(d + ": " + ticks);
                                for (int i = 0; i < ticks; i++) {
                                    Player.MoveQueue.Enqueue(Movement.Forward);
                                }
                                Player.PortalCentralizeTicks = 0;
                            }
                        }
                    }
                    bool rot = Player.IsRotationChanged;
                    bool pos = Player.IsPositionChanged;
                    
                    if (pos && rot) {
                        SendPacket(new PacketPosAndLook(Player.PosX, Player.AABB.MinY, Player.PosY, Player.PosZ, Player.Yaw, Player.Pitch, Player.OnGround));
                    } else if (pos) {
                        SendPacket(new PacketPlayerPos(Player.PosX, Player.AABB.MinY, Player.PosY, Player.PosZ, Player.OnGround));
                    } else if (rot) {
                        SendPacket(new PacketPlayerLook(Player.Yaw, Player.Pitch, Player.OnGround));
                    } else {
                        SendPacket(new PacketUpdate(Player.OnGround));
                    }
                }
            } else if (AutoReconnect && kickTicks != -1 && kickTicks++ > 20) {
                kickTicks = -1;
                StartClient();
            } else if (handshakeState == 0 && handshakeStart != 0 && Utils.GetTimestamp() - handshakeStart > 20000) {
                Disconnect("Por algum motivo, a conexão não foi completada.");
                handshakeStart = 0;
            } else if(ConnState == ConnectionState.Connecting && handshakeState == 1 && Utils.GetTimestamp() - connectedTime > 20000) {
                Disconnect("Por algum motivo, a conexão não foi completada.");
                connectedTime = 0;
            }
        }

        public List<string> ChatMessages = new List<string>();
        public bool ChatChanged = false;
        public static int MaximumChatLines = 150;
        public void PrintToChat(string msg)
        {
            lock (ChatMessages) {
                int cnt = ChatMessages.Count;
                if (cnt > MaximumChatLines) {
                    ChatMessages.RemoveRange(0, cnt - MaximumChatLines);
                }
                ChatMessages.Add(msg);
                ChatChanged = true;
            }
        }

        public void SendMessage(string msg, Boolean f)
        {
            if (msg.Length <= 0) return;

            foreach (IPlugin plugin in Program.pluginManager.plugins.Values)
            {
                plugin.onSendChat(msg, this);
            }

            try {
                if (msg[0] == '$') {
                    CmdManager.RunCommand(msg, f);
                } else {
                    SendPacket(new PacketChatMessage(msg.Length > 100 ? msg.Substring(0, 99) : msg));
                }
            } catch { }
        }
        public void SendMessage(string msg)
        {
            if (msg.Length <= 0) return;
            try {
                if (msg[0] == '$') {
                    CmdManager.RunCommand(msg, true);
                } else {
                    SendPacket(new PacketChatMessage(msg.Length > 100 ? msg.Substring(0, 99) : msg));
                }
            } catch { }
        }

        #region Utility methods
        public bool PlaceCurrentBlock(HitResult hit, bool force = false)
        {
            if(ItemInHand == null && !force) {
                return false;
            }
            int id = ItemInHand == null ? -1 : ItemInHand.ID;
            bool specialBlock = id == Items.bucket || id == Items.water_bucket || id == Items.lava_bucket || id == Items.reeds || id == Items.flint_and_steel;

            SendPacket(new PacketSwingArm(PlayerID));
            SendPacket(new PacketBlockPlace(hit, ItemInHand));

            if (specialBlock) {
                SendPacket(new PacketBlockPlace(ItemInHand));
            }
            return true;
        }
        public void BreakBlock(HitResult hit)
        {
            SendPacket(new PacketSwingArm(PlayerID));
            SendPacket(new PacketPlayerDigging(DiggingStatus.StartedDigging, hit.X, (byte)hit.Y, hit.Z, (byte)hit.Face));
            SendPacket(new PacketPlayerDigging(DiggingStatus.FinishedDigging, hit.X, (byte)hit.Y, hit.Z, (byte)hit.Face));
        }
        public void LeftClickItem()
        {
            if (Version > ClientVersion.v1_8)
                SendPacket(new PacketUseItem());
            else
                SendPacket(new PacketBlockPlace(ItemInHand));
        }
       
        public ItemStack GetHotbarItem(int n)
        {
            return Inventory.Slots[36 + n];
        }
        public int SlotOfHotbarItem(int itemId)
        {
            for (int slot = 0; slot < 9; slot++) {
                ItemStack item = Inventory.Slots[36 + slot];
                if (item != null && item.ID == itemId) {
                    return slot;
                }
            }
            return -1;
        }
        public int SlotOfItem(int itemId, bool allowHotbar)
        {
            int n = allowHotbar ? 45 : 36;
            for (int slot = 9; slot < n; slot++) {
                ItemStack item = Inventory.Slots[slot];
                if (item != null && item.ID == itemId) {
                    return slot;
                }
            }
            return -1;
        }
        #endregion

        public void Dispose()
        {
            canLoop = false;
            World.Clear();

            if(ticker != null) {
                ticker.Dispose();
                ticker = null;
            }

            if (ps != null) {
                ps.Dispose();
                ps = null;
            }
            try {
                if (updateThread != null && updateThread.IsAlive) {
                    updateThread.Join(1000);
                    updateThread.Abort();
                }
                updateThread = null;
            } catch { }
        }
    }
}
