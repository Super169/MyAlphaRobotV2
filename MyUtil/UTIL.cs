using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyUtil
{
    public static partial class UTIL
    {
        public enum InfoType
        {
            message, alert, error
        };

        public delegate void DelegateUpdateInfo(string msg = "", UTIL.InfoType iType = UTIL.InfoType.message, bool async = false);
        public delegate void DelegateCommandHandler(string command, object parm);
        public delegate void DelegateUIMethod(Object parm);

        #region "Registry related"


        public static class KEY
        {
            private static string _AppName = "";
            public static string APP_PATH = "Software\\Super169\\";
            public const string BASE_PATH = "Software\\Super169\\";

            public const string LAST_CONNECTION_SERIAL = "Last Serial Port";
            public const string LAST_CONNECTION_IP = "Last Network IP";
            public const string LAST_CONNECTION_PORT = "Last Network Port";
            public const string SERVO_VERSION = "Servo Version";

            public static string AppName
            {
                set
                {
                    _AppName = value;
                    APP_PATH = BASE_PATH + value;
                }
            }

        }

        public static bool WriteRegistry(string key, object value)
        {
            bool success = false;
            try
            {
                RegistryView platformView = (Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32);
                RegistryKey registryBase = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, platformView);
                if (registryBase == null) return false;
                RegistryKey registryEntry = registryBase.CreateSubKey(KEY.APP_PATH);
                if (registryEntry != null)
                {
                    registryEntry.SetValue(key, value);
                    success = true;
                    registryEntry.Close();
                }
                registryBase.Close();
            }
            catch (Exception)
            {
            }
            return success;
        }

        public static object ReadRegistry(string key)
        {
            object value = null;
            try
            {
                RegistryView platformView = (Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32);
                RegistryKey registryBase = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, platformView);
                if (registryBase == null) return null;
                RegistryKey registryEntry = registryBase.OpenSubKey(KEY.APP_PATH);
                if (registryEntry != null)
                {
                    value = registryEntry.GetValue(key);
                    registryEntry.Close();
                }
                registryBase.Close();

            }
            catch (Exception)
            {
                value = null;
            }
            return value;
        }

        #endregion "Registry related"

        #region "UBT CheckSum calaction"


        public static byte UBTCheckSum(byte[] data, int startIdx = 0)
        {
            int sum = 0;
            for (int i = 2; i < 8; i++)
            {
                sum += data[startIdx + i];
            }
            sum %= 256;
            return (byte)sum;
        }

        public static byte UBTCheckSum(List<byte> data, int startIdx = 0)
        {
            int sum = 0;
            for (int i = 2; i < 8; i++)
            {
                sum += data[startIdx + i];
            }
            sum %= 256;
            return (byte)sum;
        }


        public static byte CalUBTCheckSum(byte[] data)
        {
            int sum = 0;
            for (int i = 2; i < 8; i++)
            {
                sum += data[i];
            }
            sum %= 256;
            return (byte)sum;
        }

        public static byte CalCBCheckSum(byte[] data)
        {
            // A9 9A {len} {cmd} {sum} ED - minimum 6 bytes
            if (data.Length < 6) return 0;
            int dataLen = data.Length - 4;
            if (data[2] != dataLen) return 0;

            int sum = 0;
            int endPos = dataLen + 2;
            for (int i = 2; i < endPos; i++)
            {
                sum += data[i];
            }
            sum %= 256;
            return (byte)sum;
        }

        #endregion "UBT CheckSum calaction"

        public static int GetInputInteger(string data)
        {
            return Convert.ToInt32(data, 10);
        }

        public static byte GetInputByte(string data)
        {
            if (data.EndsWith("."))
            {
                data = data.Substring(0, data.Length - 1);
                return (byte) GetInputInteger(data);
            }
            return (byte)Convert.ToInt32(data, 16);
        }

        public static string GetByteString(byte[] data, string separator = " ")
        {
            string output = BitConverter.ToString(data);
            return output.Replace("-", separator);
        }

    }
}