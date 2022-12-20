using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdvancedBot.Script;
using System.IO;
using Esprima;
using Jint.Runtime;

namespace AdvancedBot.client.Commands
{
    //TODO: cleanup
    public class CommandScript : CommandBase
    {
        public CommandScript(MinecraftClient cli)
            : base(cli, "Macro", "Executa um macro/script.", "macro", "script")
        {
            SetParams("<operação (list: lista todos os macros, stop: para de executar o script, run: executa o script)>", "<nome do macro (se a operação não for 'list' ou 'stop')>");
            RunAsync = true; //??
        }
        IScriptContext ctx;
        public override CommandResult Run(string alias, string[] args)
        {
            if (args.Length < 1) {
                return CommandResult.MissingArgs;
            }
            string op = args[0].ToLower();

            switch(args[0].ToLower()) {
                case "list":
                    int count = 0;
                    StringBuilder sb = new StringBuilder();
                    if (Directory.Exists("macros\\")) {
                        foreach (string f in Directory.GetFiles("macros\\")) {
                            sb.AppendFormat(" - {0}\n", Path.GetFileName(f));
                            ++count;
                        }
                    }
                    Client.PrintToChat(string.Format("§6Macros ({0}):§f\n{1}", count, sb.ToString()));
                    break;
                case "stop":
                    ctx?.Dispose();
                    ctx = null;
                    break;
                case "run":
                    if (args.Length < 2) return CommandResult.MissingArgs;

                    string filename = "macros\\" + args[1];

                    bool js = filename.EndsWith(".js");
                    if (!js && !args[1].EndsWith(".txt")) {
                        filename += ".txt";
                    }
                    if (!File.Exists(filename)) {
                        Client.PrintToChat("§cO arquivo especificado não existe");
                        return CommandResult.ErrorSilent;
                    }
                    ctx = js ? (IScriptContext)new JsScriptContext(Client, args.Skip(2).ToArray()) :
                               new ScriptContext(Client);
                    ctx.Source = File.ReadAllText(filename);
                    Exec();
                    break;
                default: Client.PrintToChat("§cOperação inválida."); break;
            }
            return CommandResult.Success;
        }
        public override void Tick()
        {
            ctx?.Tick();
        }
        private void Exec()
        {
            try {
                ctx.Execute();
            } catch (Exception ex) {
                string ln = ex is ParserException pex ? $"(linha {pex.LineNumber} coluna {pex.Column})" : 
                            ex is JavaScriptException jex ? $"(linha {jex.LineNumber} coluna {jex.Column})" : "";
                Client.PrintToChat($"§cOcorreu um erro ao executar o script {ln}\n{ex.Message}");
            }
        }
    }
}
