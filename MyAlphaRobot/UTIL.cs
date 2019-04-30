using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace MyAlphaRobot
{
    public static partial class Util
    {

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

        public static void CheckFirmware()
        {
            SYSTEM.firmwareRelease = MyUtil.WEB.GetTextFile(CONST.DISTRIBUTION.FIRMWARE.RELEASE.VERSION);
            SYSTEM.firmwareBeta = MyUtil.WEB.GetTextFile(CONST.DISTRIBUTION.FIRMWARE.BETA.VERSION);
            SYSTEM.firmwareHailzd = MyUtil.WEB.GetTextFile(CONST.DISTRIBUTION.FIRMWARE.HAILZD.VERSION);
            // mark as checked if any of them is available
            SYSTEM.firmwareChecked = !(string.IsNullOrWhiteSpace(SYSTEM.firmwareBeta) || 
                                       string.IsNullOrWhiteSpace(SYSTEM.firmwareRelease) ||
                                       string.IsNullOrWhiteSpace(SYSTEM.firmwareHailzd));
        }

        public static string LatestVersion()
        {
            if (!SYSTEM.firmwareChecked) CheckFirmware();
            string version = SYSTEM.firmwareRelease; // use release version by default
            switch (SYSTEM.sc.firmwareType)
            {
                case SystemConfig.FIRMWARE.beta:
                    version = SYSTEM.firmwareBeta;
                    break;

                case SystemConfig.FIRMWARE.hailzd:
                    version = SYSTEM.firmwareHailzd;
                    break;

            }
            return version;
        }

        public static bool IsLatest(string version)
        {
            return (version.Equals(LatestVersion()));
        }

        public static void SaveSystemConfig()
        {
            MyUtil.FILE.SaveDataFile(SYSTEM.sc, SYSTEM.systemConfigFile, CONST.SYSTEM_CONFIG_ZIP);
        }

        public static bool IsBlocklyPath(string path)
        {
            return (File.Exists(System.IO.Path.Combine(path, CONST.BLOCKLY.CHECK_FILE)));
        }

        public static bool GetBlocklyPath(ref string blocklyPath)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (!string.IsNullOrWhiteSpace(blocklyPath))
            {
                ofd.InitialDirectory = blocklyPath;
            }
            ofd.Filter = "Blockly File | " + CONST.BLOCKLY.CHECK_FILE;
            if (ofd.ShowDialog() == true)
            {
                blocklyPath = System.IO.Path.GetDirectoryName(ofd.FileName);
                if (IsBlocklyPath(blocklyPath))
                {
                    Util.SaveSystemConfig();
                    return true;
                }
            }
            return false;
        }
    }
}
