using AdvancedBot.client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AdvancedBot.Plugins
{
    public class PluginManager
    {
        public PluginManager()
        {
        }

        public void Init()
        {
            Program.FrmMain.DebugConsole($"Loading plugins...");
            if (Directory.Exists("Plugins\\"))
            {

                try
                {
                    string directoryName = System.IO.Directory.GetCurrentDirectory();
                    foreach (string str in Directory.GetFiles("Plugins", "*.dll"))
                    {
                        try
                        {
                            Assembly assembly = Assembly.Load(File.ReadAllBytes(directoryName + "\\" + str));

                            foreach (Type type in assembly.GetTypes())
                            {
                                var myType = typeof(IPlugin);
                                if (myType == null) throw new Exception("Type of assembly cannot be null");
                                if (type.GetInterfaces().Contains(typeof(IPlugin)))
                                {
                                    IPlugin value = (IPlugin)Activator.CreateInstance(type);
                                    lock (this.plugins)
                                    {
                                        this.plugins[assembly] = value;
                                        Program.FrmMain.DebugConsole($"Loaded Plugin {directoryName + "\\" + str}");
                                        break;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {

                            Program.FrmMain.DebugConsole($"Falied to load Plugin {directoryName + "\\" + str}");
                            Program.FrmMain.DebugConsole($"{ex.ToString()}");
                            Program.CreateErrLog(ex, "pluginerr");
                        }
                    }
                }

                catch (Exception ex) { Program.CreateErrLog(ex, "pluginerr"); }
            }
            else
            {
                Directory.CreateDirectory("Plugins\\");
            }
        }

        internal void Unload()
        {
            lock (this.plugins)
            {
                foreach (IPlugin plugin in this.plugins.Values)
                {
                    plugin.Unload();
                }
            }
        }

        // Token: 0x060002DA RID: 730 RVA: 0x00018000 File Offset: 0x00016200
        internal void Tick()
        {
            lock (this.plugins)
            {
                foreach (IPlugin plugin in this.plugins.Values)
                {
                    plugin.Tick();
                }
            }
        }

        internal bool DoCommand(MinecraftClient sender, string fullCmd)
        {
            int num = fullCmd.IndexOf(' ');
            string key = (num == -1) ? fullCmd : fullCmd.Substring(0, num);
            string[] args = (num == -1) ? new string[0] : fullCmd.Substring(num + 1).Split(new char[]
            {
                ' '
            });
            PluginManager.CommandDelegate commandDelegate;
            return this.registeredCommands.TryGetValue(key, out commandDelegate) && commandDelegate(sender, args);
        }

        public void RegisterCommand(string name, PluginManager.CommandDelegate cmd)
        {
            this.registeredCommands.Add(name, cmd);
        }

        public static PluginManager Instance = new PluginManager();

        public Dictionary<string, PluginManager.CommandDelegate> registeredCommands = new Dictionary<string, PluginManager.CommandDelegate>(StringComparer.InvariantCultureIgnoreCase);

        public Dictionary<Assembly, IPlugin> plugins = new Dictionary<Assembly, IPlugin>();

        public delegate bool CommandDelegate(MinecraftClient sender, string[] args);
    }
}
