using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Globalization;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace AdvancedBot.client
{
    static class ChatParser
    {
        public static string ParseJson(string json)
        {
            StringBuilder sb = new StringBuilder();
            ParseComponent(JToken.Parse(json), sb);
            return sb.ToString();
        }
        public static string ParseChat(JToken data)
        {
            StringBuilder sb = new StringBuilder();
            ParseComponent(data, sb);
            return sb.ToString();
        }
        private static void ParseComponent(JToken data, StringBuilder sb)
        {
            switch (data.Type) {
                case JTokenType.Object:
                    JToken tmp = null;
                    if ((tmp = data["color"]) != null) {
                        sb.Append(GetCodeFromName(tmp.AsStr()));
                    }
                    if (data["bold"].AsBool()) sb.Append("§l");
                    if (data["italic"].AsBool()) sb.Append("§o");
                    if (data["underlined"].AsBool()) sb.Append("§n");
                    if (data["strikethrough"].AsBool()) sb.Append("§m");
                    //  if (data["obfuscated"].AsBool())    sb.Append("§k");

                    if ((tmp = data["text"]) != null) {
                        ParseComponent(tmp, sb);
                    } else if ((tmp = data["translate"]) != null) {
                        List<string> usingData = new List<string>();
                        if ((tmp = data["with"]) != null) {
                            StringBuilder wsb = new StringBuilder();
                            foreach (JToken item in tmp.AsJArr()) {
                                ParseComponent(item, wsb);
                                usingData.Add(wsb.ToString());
                                wsb.Clear();
                            }
                        }
                        //TranslateString(data["translate"].AsStr(), usingData, sb);
                        Translate(data["translate"].AsStr(), usingData, sb);
                    }

                    if ((tmp = data["extra"]) != null) {
                        foreach (JToken item in tmp.AsJArr()) {
                            ParseComponent(item, sb);
                            sb.Append("§r");
                        }
                    }
                    break;
                case JTokenType.Array:
                    foreach (JToken item in data.AsJArr())
                        ParseComponent(item, sb);
                    break;
                case JTokenType.String: sb.Append(data.AsStr()); break;
            }
        }
        
        public static string GetCodeFromName(string name)
        {
            switch (name.ToLower()) {
                case "black":         return "§0";
                case "dark_blue":     return "§1";
                case "dark_green":    return "§2";
                case "dark_aqua":     return "§3";
                case "dark_red":      return "§4";
                case "dark_purple":   return "§5";
                case "gold":          return "§6";
                case "gray":          return "§7";
                case "dark_gray":     return "§8";
                case "blue":          return "§9";
                case "green":         return "§a";
                case "aqua":          return "§b";
                case "red":           return "§c";
                case "light_purple":  return "§d";
                case "yellow":        return "§e";
                case "white":         return "§f";
                case "obfuscated":    return "§k"; 
                case "bold":          return "§l"; 
                case "strikethrough": return "§m"; 
                case "underline":     return "§n";
                case "italic":        return "§o";
                case "reset":         return "§r";
                default: return "";
            }
        }

        private static Dictionary<string, string> TranslationRules = new Dictionary<string, string>();

        static ChatParser()
        {
            //Small default dictionnary of translation rules
            TranslationRules["chat.type.admin"] = "[%s: %s]";
            TranslationRules["chat.type.announcement"] = "§d[%s] %s";
            TranslationRules["chat.type.emote"] = " * %s %s";
            TranslationRules["chat.type.text"] = "<%s> %s";
            TranslationRules["multiplayer.player.joined"] = "§e%s joined the game.";
            TranslationRules["multiplayer.player.left"] = "§e%s left the game.";
            TranslationRules["commands.message.display.incoming"] = "§7%s whispers to you: %s";
            TranslationRules["commands.message.display.outgoing"] = "§7You whisper to %s: %s";

            if (!Directory.Exists("lang"))
                Directory.CreateDirectory("lang");

            string file = "lang\\pt_BR.lang";
            if (!File.Exists(file)) {
                try {
                    JObject assets = JObject.Parse(DownloadString("https://s3.amazonaws.com/Minecraft.Download/indexes/1.12.json"));
                    string hash = assets["objects"]["minecraft/lang/pt_br.lang"]["hash"].AsStr();

                    File.WriteAllText(file, DownloadString("https://resources.download.minecraft.net/" + hash.Substring(0, 2) + "/" + hash));
                } catch {
                    string dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\.minecraft\assets\objects\5b\5b933ea21cd6e7bccefb023df4f6efc742468845";
                    if (File.Exists(dir)) {
                        file = dir;
                    }
                }
            }

            if (File.Exists(file)) {
                string[] translations = File.ReadAllLines(file);
                foreach (string line in translations) {
                    if (line.Length > 0) {
                        string[] splitted = line.Split('=');
                        if (splitted.Length == 2) {
                            TranslationRules[splitted[0]] = splitted[1];
                        }
                    }
                }
            }
        }

        private static Regex FMT_PATTERN = new Regex("%(?:(\\d+)\\$)?([A-Za-z%]|$)", RegexOptions.Compiled);
        private static void Translate(string rulename, List<string> args, StringBuilder sb)
        {
            try {
                string rule;// = TranslationRules[rulename];
                if (!TranslationRules.TryGetValue(rulename, out rule)) {
                    rule = rulename;
                }

                int argIdx = 0, patternPos = 0;

                int l;
                for (Match match; (match = FMT_PATTERN.Match(rule, patternPos)).Success; patternPos = l) {
                    int idx = match.Index;
                    int len = match.Length;
                    l = idx + len;
                    if (idx > patternPos) {
                        sb.Append(rule, patternPos, idx - patternPos);
                    }

                    string fmt = match.Groups[2].Value;
                    if (match.Value == "%%") {
                        sb.Append('%');
                    } else {
                        if (fmt != "s")
                            throw new Exception("Unsupported format: \'" + match.Value + "\'");

                        string sa = match.Groups[1].Value;
                        int arg = sa.Length != 0 ? int.Parse(sa) - 1 : argIdx++;

                        if (arg < args.Count) {
                            sb.Append(args[arg]);
                        }
                    }
                }

                if (patternPos < rule.Length) {
                    sb.Append(rule, patternPos, rule.Length - patternPos);
                }
            } catch {
                sb.Append("[" + rulename + "], ");
                sb.Append(string.Join(" ", args));
            }
        }
        
        private static string DownloadString(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.Proxy = null;
            using (WebResponse response = request.GetResponse()) {
                using (StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8)) {
                    return sr.ReadToEnd();
                }
            }
        }
    }
}
