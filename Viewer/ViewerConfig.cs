using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedBot.Viewer
{
    public class ViewerConfig
    {
        public static int MaxFps = 40;
        public static int RenderDist = 6;
        public static bool UseVBO = true;
        public static bool UseResPack = true;
        public static bool RenderSigns = true;
        public static bool UseTexture = true;
        public static bool UseMipMap = true;
        public static double FlySpeed = 2.0;

        static ViewerConfig()
        {
            //Config.Deserialize(typeof(ViewerConfig), "Viewer");
        }
    }
}
