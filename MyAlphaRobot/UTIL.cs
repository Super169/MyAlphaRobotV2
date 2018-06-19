using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace MyAlphaRobot
{
    public static partial class UTIL
    {
        public static class KEY
        {
            public const string APP_PATH = "Software\\Super169\\MyAlphaRobot";
            public const string LAST_CONNECTION = "Last Connection";
            public const string LAST_LAYOUT = "Last Layout";
            public const string SERVO_VERSION = "Servo Version";
        }

        public delegate void DelegateUpdateInfo(string msg = "", UTIL.InfoType iType = UTIL.InfoType.message, bool async = false);

        public enum InfoType
        {
            message, alert, error
        };

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

        public static bool getByte(string data, out byte value)
        {
            return getByte(data, out value, false, 0);
        }

        public static bool getByte(string data, out byte value, byte emptyValue)
        {
            return getByte(data, out value, true, emptyValue);
        }

        public static bool getByte(string data, out byte value, bool allowEmpty, byte emptyValue)
        {
            data = data.Trim();
            value = emptyValue;
            if ((data == "") || (data == null))
            {
                return allowEmpty;
            }
            int iTemp;
            if (!int.TryParse(data, out iTemp)) return false;
            if ((iTemp < 0) || (iTemp > 255)) return false;
            value = (byte)iTemp;
            return true;
        }

        public static byte GetInputByte(string data)
        {
            if (data.StartsWith("'"))
            {
                char ch = data[1];
                return (byte)Convert.ToInt32(ch);
            }
            if (data.EndsWith("."))
            {
                data = data.Substring(0, data.Length - 1);
                return (byte)Convert.ToInt32(data, 10);
            }
            return (byte)Convert.ToInt32(data, 16);
        }

        public static T Clone<T>(this T source)
        {
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }
            T data;
            try
            {
                string serializedObject = new JavaScriptSerializer().Serialize(source);
                data = new JavaScriptSerializer().Deserialize<T>(serializedObject);
            }
            catch
            {
                return default(T);
            }
            return data;
        }

        public static UInt16 getUInt16(byte[] data, byte offset)
        {
            if (data.Length < offset + 2) return 0;
            return (UInt16)(data[offset] << 8 | data[offset + 1]);
        }

        public static UInt16 getUInt16(List<byte> data, byte offset)
        {
            if (data.Count < offset + 2) return 0;
            return (UInt16)(data[offset] << 8 | data[offset + 1]);
        }

    }
}
