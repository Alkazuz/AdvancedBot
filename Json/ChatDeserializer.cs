using AdvancedBot.client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;

namespace AdvancedBot.Json
{
    class ChatDeserializer
    {
        public static ChatMessage deserialize(String json)
        {
            ChatMessage message = new ChatMessage();
            try
            {
                JToken data = JToken.Parse(json);
                if (data.Type == JTokenType.Object)
                {
                    With with = new With();
                    with.setText(ChatParser.ParseChat(data));
                    Debug.WriteLine(ChatParser.ParseChat(data));
                    if (data["clickEvent"] != null)
                    {
                       
                        ClickEvent clickEvent = new ClickEvent();
                        if (data["clickEvent"]["action"] != null)
                        {
                            clickEvent.setAction(data["clickEvent"]["action"].AsStr());
                        }
                        if (data["clickEvent"]["value"] != null)
                        {
                            clickEvent.setValue(data["clickEvent"]["value"].AsStr());
                        }
                        with.setClickEvent(clickEvent);
                    }

                    if (data["clickEvent"] == null)
                    {
                        data = data["extra"];
                    }
                    else
                    {
                        message.GetWith().Add(with);
                    }
                        
                }
                if (data.Type == JTokenType.Array)
                {
                    foreach (JToken obj2 in data)
                    {
                        With with = new With();

                        with.setText(ChatParser.ParseChat(obj2));
                        Debug.WriteLine(ChatParser.ParseChat(obj2));
                        if (obj2["clickEvent"] != null)
                        {
                            ClickEvent clickEvent = new ClickEvent();
                            if (obj2["clickEvent"]["action"] != null)
                            {
                                clickEvent.setAction(obj2["clickEvent"]["action"].AsStr());
                            }
                            if (obj2["clickEvent"]["value"] != null)
                            {
                                clickEvent.setValue(obj2["clickEvent"]["value"].AsStr());
                            }
                            with.setClickEvent(clickEvent);
                        }
                        
                        message.GetWith().Add(with);
                    }
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString());}
                return message;
        }
        public static string GetCodeFromName(string name)
        {
            switch (name.ToLower())
            {
                case "black": return "§0";
                case "dark_blue": return "§1";
                case "dark_green": return "§2";
                case "dark_aqua": return "§3";
                case "dark_red": return "§4";
                case "dark_purple": return "§5";
                case "gold": return "§6";
                case "gray": return "§7";
                case "dark_gray": return "§8";
                case "blue": return "§9";
                case "green": return "§a";
                case "aqua": return "§b";
                case "red": return "§c";
                case "light_purple": return "§d";
                case "yellow": return "§e";
                case "white": return "§f";
                case "obfuscated": return "§k";
                case "bold": return "§l";
                case "strikethrough": return "§m";
                case "underline": return "§n";
                case "italic": return "§o";
                case "reset": return "§r";
                default: return "";
            }
        }

    }
}
