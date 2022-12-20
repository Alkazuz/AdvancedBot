using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AdvancedBot.Client.Handler
{
    public class HandlerGenerator
    {
        private static HashSet<string> RequiredPackets_C = new HashSet<string>() {
            "keep_alive",           //0x00
            "login",                //0x01 join game
            "chat",                 //0x02
            "update_health",        //0x06
            "respawn",              //0x07
            "position",             //0x08
            "held_item_slot",       //0x09
            "named_entity_spawn",   //0x0C spawn player
            "entity_velocity",      //0x12
            "entity_destroy",       //0x13
            "rel_entity_move",      //0x15
            "entity_look",          //0x16
            "entity_move_look",     //0x17
            "entity_teleport",      //0x18
            "entity_head_rotation", //0x19
            "entity_effect",        //0x1D
            "remove_entity_effect", //0x1E
            "map_chunk",            //0x21
            "multi_block_change",   //0x22
            "block_change",         //0x23
            "explosion",            //0x27
            "game_state_change",    //0x2B
            "open_window",          //0x2D
            "close_window",         //0x2E
            "set_slot",             //0x2F
            "window_items",         //0x30
            "transaction",          //0x32 confirm transaction
            "update_sign",          //0x33
            "player_info",          //0x38 player list
            "kick_disconnect",      //0x40
            "title",                //0x45
        };

        public static void Generate(JObject json)
        {

        }

        private static void GenPacket(JArray packet)
        {

        }


        public class PrettyPrinter
        {
            private StringBuilder sb = new StringBuilder();
            private int iLevel = 0;

            public void AppendLine(string content)
            {
                Indent();
                sb.AppendFormat("{0}{1}\r\n", content);
            }
            public void AppendFormat(string format, params object[] args)
            {
                Indent();
                sb.AppendFormat(format, args);
            }
            public void Append(string content)
            {
                sb.Append(content);
            }

            public void Begin(string name)
            {
                Indent();
                sb.Append(name + " {\r\n");
                iLevel++;
            }
            public void End()
            {
                iLevel--;
                Indent();
                sb.Append("}\r\n");
            }
            private void Indent() => sb.Append(' ', iLevel * 4);

            public override string ToString()
            {
                return sb.ToString();
            }
        }
    }
}
