using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyAlphaRobot
{
    public static class RCVersion
    {
        public static class TargetVersion
        {
            public const byte major = 2;
            public const byte minor = 1;
            public const byte build = 99;
            public const byte fix = 10;

            public static long GetSeq()
            {
                return major * 1000000 + minor * 10000 + build * 100 + fix;
            }
        }

        public static bool ready = false;
        public static byte major = 0;
        public static byte minor = 0;
        public static byte build = 0;
        public static byte fix = 0;

        public static bool IsOutdated()
        {
            if (!ready) return true;
            long currSeq = major * 1000000 + minor * 10000 + build * 100 + fix;
            return (currSeq < TargetVersion.GetSeq());
        }

        public static new string ToString()
        {
            if (!ready) return "";
            string version = string.Format("{0}.{1}.{2}", major, minor, build);
            if (fix > 0) version += string.Format("  [Fix {0}]", fix);
            if (IsOutdated()) version += " *";
            return version;
        }

        public static string ToCode()
        {
            if (!ready) return "";
            return string.Format("{0}.{1}.{2}.{3}", major, minor, build, fix);
        }

        public static bool IsBeta()
        {
            if (!ready) return false;
            return (build == 99);
        }
    }



}
