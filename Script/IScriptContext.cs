using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdvancedBot.client;

namespace AdvancedBot.Script
{
    public abstract class IScriptContext : IDisposable
    {
        public MinecraftClient Client { get; }
        public string Source { get; set; }

        public IScriptContext(MinecraftClient cli)
        {
            Client = cli;
        }
        public abstract void Execute();
        public virtual void Tick() { }

        public virtual void Dispose() { }
    }
}
