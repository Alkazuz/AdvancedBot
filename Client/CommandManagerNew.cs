using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using AdvancedBot.client.Commands;
using System.Threading.Tasks;
using System.Reflection;

namespace AdvancedBot.client
{
    public class CommandManagerNew
    {
        public MinecraftClient Client;
        public List<CommandBase> Commands = new List<CommandBase>();

        public CommandManagerNew(MinecraftClient c)
        {
            Client = c;

            Commands.Add(new CommandHelp(c));
            Commands.Add(new CommandMove(c));
            Commands.Add(new CommandPortal(c));
            Commands.Add(new CommandRetard(c));
            Commands.Add(new CommandReco(c));
            Commands.Add(new CommandFollow(c));
            Commands.Add(new CommandKillAura(c));
            Commands.Add(new CommandTwerk(c));
            Commands.Add(new CommandProtetor(c));
            Commands.Add(new CommandPlayerList(c));
            Commands.Add(new CommandGive(c));
            Commands.Add(new CommandGoto(c));
            Commands.Add(new CommandUseEntity(c));
            Commands.Add(new CommandHotbarClick(c));
            Commands.Add(new CommandInvClick(c));
            Commands.Add(new CommandDropAll(c));
            Commands.Add(new CommandClickBlock(c));
            Commands.Add(new CommandMiner(c));
            Commands.Add(new CommandClearChat(c));
            Commands.Add(new CommandAntiAFK(c));
            Commands.Add(new CommandCrash(c));
            Commands.Add(new CommandCrashNew(c));
            Commands.Add(new CommandPlaceBlock(c));
            Commands.Add(new CommandInvCaptcha(c));
            Commands.Add(new CommandHerbalism(c));
            Commands.Add(new CommandUseBow(c));
            Commands.Add(new CommandProxy(c));
            Commands.Add(new CommandBreakBlock(c));
            Commands.Add(new CommandScript(c));

            Commands.Add(new CommandAreaMiner(c));
            /*var mods = Assembly.GetCallingAssembly().GetModules(false);
            foreach (var type in mods.SelectMany(a => a.GetTypes().Where(b => b.IsSubclassOf(typeof(ICommand))))) {
                Commands.Add((ICommand)Activator.CreateInstance(type, c));
            }*/
        }
        public void RunCommand(string cmdstr, bool printErrors)
        {
            int i = cmdstr.IndexOf(' ');
            string name;
            string[] args;
            if (i == -1) {
                name = cmdstr.Substring(1);
                args = new string[0];
            } else {
                name = cmdstr.Substring(1, i - 1);
                args = cmdstr.Substring(i + 1).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            }
            CommandBase cmd = GetCommand(name);
            if (cmd == null && printErrors) {
                Client.PrintToChat("§cComando não encontrado. Use $help para ver os comandos disponíveis.");
                return;
            }
            try {
                if (cmd.RunAsync) {
                    Task.Run(() => HandleCommandResult(cmd.Run(name, args)));
                } else {
                    HandleCommandResult(cmd.Run(name, args));
                }
            } catch {
                if (!printErrors) return;
                HandleCommandResult(CommandResult.Error);
            }
        }
        private void HandleCommandResult(CommandResult result)
        {
            switch (result) {
                case CommandResult.Error: Client.PrintToChat("§cOcorreu um erro ao executar o comando."); break;
                case CommandResult.MissingArgs: Client.PrintToChat("§cSintaxe incorreta. Use $help <nome do comando> para ver os parametros."); break;
            }
        }
        public void Tick()
        {
            foreach (CommandBase cmd in Commands) {
                cmd.Tick();
            }
        }
        public CommandBase GetCommand(string alias)
        {
            foreach (CommandBase cmd in Commands) {
                foreach (string cmdAlias in cmd.Aliases) {
                    if (alias.EqualsIgnoreCase(cmdAlias)) {
                        return cmd;
                    }
                }
            }
            return null;
        }
        public TCommand GetCommand<TCommand>() where TCommand : CommandBase
        {
            return Commands.OfType<TCommand>().FirstOrDefault();
        }
    }
}
