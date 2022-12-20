using System;
using System.Diagnostics;
using System.IO;

namespace AdvancedBot.ClientSocket.Packets
{
    class UpdateHandler
    {
        private Boolean latest;
        public UpdateHandler(Boolean t)
        {
            this.latest = t;
        }

        public Boolean check()
        {
            if (latest)
            {
                // new Update
                if (!File.Exists("AdvancedBot.Updater2.exe"))
                {
                    System.Net.WebClient WC = new System.Net.WebClient();
                    WC.Headers.Add("user-agent", "Only a test!");
                    var O = WC.DownloadString("https://advanced-bot.tk/up.txt");
                    System.IO.File.WriteAllBytes("AdvancedBot.Updater2.exe", Convert.FromBase64String(O));
                }
                Program.Config.AddBoolean("ChangelogOpen", false);
                Program.SaveConf();
                WebConnection.computer.SetAllowed(false);
                Process.Start("AdvancedBot.Updater2.exe", WebConnection.computer.getHWID() + " AdvancedBot.exe");
                Environment.Exit(0);
                return false;
            }
            return true;
        }
    }
}
