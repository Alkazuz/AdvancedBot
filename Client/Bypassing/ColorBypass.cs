using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AdvancedBot.client.Bypassing
{
    public class ColorBypass : ServerBypassBase
    {
        public override ClientVersion Version => ClientVersion.v1_8;

        private static Regex CAPTCHA_REGEX = new Regex(@"clique na cor (.+)\s?", RegexOptions.Compiled);

        public ColorBypass(MinecraftClient cli) : base(cli) { }

        private string color = null;
        public override bool HandlePacket(ReadBuffer rb)
        {
            if (rb.ID == 0x02 && !IsFinished)
            {
                string json = rb.ReadString();
                Debug.WriteLine(json);
                if (json.Contains(", clique na cor "))
                {
                    Match m = CAPTCHA_REGEX.Match(Utils.StripColorCodes(ChatParser.ParseJson(json)));
                    color = m.Groups[1].Value;
                }
                if (color != null && json.Contains("clickEvent"))
                {
                    var obj = JObject.Parse(json);
                    var clickEvent = GetClickEvent(obj, color);
                    if (clickEvent != null)
                    {
                        Client.PrintToChat("§a[ColorBypass]: O captcha foi burlado.");
                        Client.SendMessage(clickEvent);
                        IsFinished = true;
                    }
                    else
                    {
                        Client.PrintToChat("§c[ColorBypass]: Não foi possivel burlar o captcha.");
                    }
                }
                return true;
            }
            return false;
            string GetClickEvent(JToken obj, string text)
            {
                if (obj == null) return null;

                if (obj.Type == JTokenType.Object && obj["clickEvent"] != null)
                {
                    bool match = Utils.StripColorCodes(obj["text"].AsStr()).ContainsIgnoreCase(text);
                    return match ? obj["clickEvent"]["value"].AsStr()
                                 : null;
                }
                else if (obj.Type == JTokenType.Object)
                {
                    return GetClickEvent(obj["extra"], text);
                }
                else if (obj.Type == JTokenType.Array)
                {
                    foreach (var item in obj)
                    {
                        var evt = GetClickEvent(item, text);
                        if (evt != null) return evt;
                    }
                }
                return null;
            }
        }
    }
}
