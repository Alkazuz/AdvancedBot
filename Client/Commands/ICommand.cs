using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdvancedBot.Client.Map;
using System.Diagnostics;

namespace AdvancedBot.Client.Commands
{
    public abstract class ICommand
    {
        public string[] Aliases;

        public string DisplayName;
        public string[] Parameters = new string[0];
        public string Description;

        public MinecraftClient Client;
        public Entity Player { get { return Client.Player; } }
        public World World { get { return Client.World; } }

        public bool IsToggled;

        public bool RunAsync = false;

        public string ToggleText = null;
        public bool CanToggle { get { return ToggleText != null; } }

        public ICommand(MinecraftClient cli, string dn, string desc, params string[] aliases)
        {
            Client = cli;
            DisplayName = dn;
            Description = desc;
            Aliases = aliases;
        }
        protected void SetParams(params string[] pars)
        {
            Parameters = pars;
        }
        protected void Toggle(string[] args)
        {
            if (CanToggle) {
                IsToggled = args.Length > 0 ? (args[0] == "1" || args[0].EqualsIgnoreCase("on")) : !IsToggled;
                PrintToggleMsg(ToggleText, IsToggled);
            }
        }
        protected void PrintToggleMsg(string txt, bool toggled)
        {
            Client.PrintToChat(string.Format(txt, toggled ? "§aON" : "§cOFF"));
        }

        public abstract CommandResult Run(string alias, string[] args);
        public virtual void Tick() { }
    }
    public enum CommandResult
    {
        Success, MissingArgs, Error, ErrorSilent
    }
}
