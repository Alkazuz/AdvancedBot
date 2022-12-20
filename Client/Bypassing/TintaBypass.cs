using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace AdvancedBot.client.Bypassing
{
    public class TintaBypass : ServerBypassBase
    {
        public override ClientVersion Version => ClientVersion.v1_8;
        public TintaBypass(MinecraftClient cli) : base(cli) { }

        private static Regex COLOR_REGEX = new Regex(@"clique na cor ([a-zA-Z]+)\s?", RegexOptions.Compiled);
        private static Regex CAPTCHA_REGEX = new Regex(@"'(\d+)'\s*para\s*confirmar!", RegexOptions.Compiled);

        private string color = null;
        private long lifeStart;
        public override bool HandlePacket(ReadBuffer rb)
        {
            if (rb.ID == 0x02 && !IsFinished) {
                string json = rb.ReadString();

                string stripped = Utils.StripColorCodes(ChatParser.ParseJson(json));
                Debug.WriteLine(stripped + "  " + json);

                if (stripped.Contains(", clique na cor ")) {
                    Match m = COLOR_REGEX.Match(stripped);
                    color = m.Groups[1].Value;
                    lifeStart = Utils.GetTimestamp();
                }
                if (color != null && json.Contains("clickEvent")) {
                    var obj = JObject.Parse(json);
                    var clickEvent = GetClickEvent(obj, color);
                    if (clickEvent != null) {
                        Client.PrintToChat($"§a[TintaBypass]: Clicando na cor {color}!");
                        Client.SendMessage(clickEvent);
                    } else {
                        Client.PrintToChat("§c[TintaBypass]: Não foi possivel clicar na cor.");
                    }
                }
                if(stripped.Contains("para confirmar")) {
                    Match m = CAPTCHA_REGEX.Match(stripped);
                    if(m.Success) {
                        Client.PrintToChat("§a[TintaBypass]: O captcha foi burlado!");
                        Client.SendMessage(m.Groups[1].Value);
                    } else {
                        Client.PrintToChat("§c[TintaBypass]: Não foi possivel burlar o captcha.");
                    }
                }

                return true;
            }
            if(lifeStart != 0 && Utils.GetTimestamp() - lifeStart > 25000) { //25s de vida para este objeto
                IsFinished = true;
            }
            return false;
            string GetClickEvent(JToken obj, string text)
            {
                if (obj == null) return null;

                if (obj.Type == JTokenType.Object && obj["clickEvent"] != null) {
                    bool match = Utils.StripColorCodes(obj["text"].AsStr()).ContainsIgnoreCase(text);
                    return match ? obj["clickEvent"]["value"].AsStr()
                                 : null;
                } else if (obj.Type == JTokenType.Object) {
                    return GetClickEvent(obj["extra"], text);
                } else if (obj.Type == JTokenType.Array) {
                    foreach (var item in obj) {
                        var evt = GetClickEvent(item, text);
                        if (evt != null) return evt;
                    }
                }
                return null;
            }
        }
    }
}
