using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jint;
using AdvancedBot.client;
using System.Reflection;
using System.IO;
using Jint.Native;
using System.Diagnostics;
using System.Threading;

namespace AdvancedBot.Script
{
    public class JsScriptContext : IScriptContext
    {
        public Engine JsEngine;
        private ABContext ab;

        public JsScriptContext(MinecraftClient cli, string[] args) : base(cli)
        {
            JsEngine = new Engine(c => c.AllowClr());

            ab = new ABContext(cli, JsEngine) {
                Args = args
            };

            JsEngine.SetValue("AdvancedBot", ab);
            JsEngine.SetValue("importAsm", new Action<string>((name) => {
                try {
                    Assembly asm = File.Exists(name) ?
                                   Assembly.LoadFile(name) :
                                   AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == name);
                    if (asm != null) {
                        JsEngine.Options.AllowClr(asm);
                    }
                } catch { }
            }));
            
        }
        public override void Execute()
        {
            JsEngine.Execute(Source);
        }
        int numErr = 0;
        public override void Tick()
        {
            var onTick = ab.OnTick;
            if (onTick != null && onTick.IsObject() && onTick is ICallable ic) {
                try {
                    ic.Call(JsValue.Undefined, new JsValue[0]);
                } catch (Exception ex) {
                    if (numErr++ == 0) {
                        Client.PrintToChat("§cErro ao invocar a função OnTick: \n\n" + ex.ToString());
                    }
                }
            }
            ab.PollTimeout();
        }
        public override void Dispose()
        {
            base.Dispose();
        }

        public class ABContext
        {
            private Engine engine;
            public ABContext(MinecraftClient cli, Engine e)
            {
                client = cli;
                Caller = new JsMinecraftClient(cli);
                engine = e;
            }

            public JsValue OnTick = null;
            public JsMinecraftClient Caller;
            public string[] Args;
            private MinecraftClient client;

            public JsMinecraftClient[] GetClients()
            {
                List<MinecraftClient> clients = Program.FrmMain.Clients;
                JsMinecraftClient[] arr = new JsMinecraftClient[clients.Count];
                for (int i = 0; i < Math.Min(arr.Length, clients.Count); i++) {
                    arr[i] = new JsMinecraftClient(clients[i]);
                }
                return arr;// JsValue.FromObject(engine, arr);
            }
            public void Log(params string[] parts)
            {
                client.PrintToChat(string.Concat(parts));
            }
            public void SendMessage(params string[] parts)
            {
                client.SendMessage(string.Concat(parts));
            }

            private List<Tuple<ICallable, int, long>> timeoutList = new List<Tuple<ICallable, int, long>>();
            public void SetTimeout(JsValue func, int delay)
            {
                if(func is ICallable ic) {
                    lock(timeoutList) {
                        timeoutList.Add(new Tuple<ICallable, int, long>(ic, delay, Utils.GetTimestamp()));
                    }
                }
            }

            internal void PollTimeout()
            {
                lock (timeoutList) {
                    var now = Utils.GetTimestamp();
                    for (var i = 0; i < timeoutList.Count; i++) {
                        var to = timeoutList[i];
                        if (now - to.Item3 > to.Item2) {
                            try {
                                to.Item1.Call(JsValue.Undefined, new JsValue[0]);
                            } catch { }
                            timeoutList.RemoveAt(i--);
                        }
                    }
                }
            }
        }
    }
}
