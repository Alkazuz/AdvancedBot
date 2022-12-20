using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WMPLib;

namespace AdvancedBot
{
    class SurroundingClass
    {
        SurroundingClass()
        {
            Player = new WMPLib.WindowsMediaPlayer();
        }

        public static WMPLib.WindowsMediaPlayer _Player;

        public static WMPLib.WindowsMediaPlayer Player
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _Player;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_Player != null)
                {
                }

                _Player = value;
                if (_Player != null)
                {
                }
            }
        }

        public static void Play()
        {
            try
            {
                var FN = System.IO.Path.GetTempPath() + @"\AdvancedBot-MUSIC.MP3";
                if (!System.IO.File.Exists(FN))
                {
                    System.Net.WebClient WC = new System.Net.WebClient();
                    var O = WC.DownloadString("https://pastebin.com/raw/34Gqdu7K");
                    try
                    {
                        System.IO.File.WriteAllBytes(FN, Convert.FromBase64String(O));
                    }
                    catch(Exception ed)
                    {
                        Debug.WriteLine(ed.ToString());
                    }
                   
                }
                Player.settings.setMode("Loop", true);
                Player.URL = FN;
                Player.enabled = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
    }
}
