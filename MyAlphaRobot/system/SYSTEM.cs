using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyAlphaRobot
{
    public static class SYSTEM
    {
        public static MainWindow main { get; set; }
        public static string configPath { get; set; }
        public static string systemConfigFile { get; set; }

        public static SystemConfig sc { get; set; }

        public static bool firmwareChecked { get; set; }
        public static string firmwareBeta { get; set; }
        public static string firmwareRelease { get; set; }
        public static string firmwareHailzd { get; set; }

        public static ConfigObject configObject { get; set; }


    }
}
