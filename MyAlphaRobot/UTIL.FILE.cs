using Ionic.Zlib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace MyAlphaRobot
{
    public static partial class UTIL
    {
        public static class FILE
        {
            public static bool AppendToFile(string msg, string fileName)
            {
                bool success = true;
                StreamWriter file = null;
                try
                {
                    file = File.AppendText(fileName);
                    file.WriteLine(msg);
                }
                catch
                {
                    success = false;
                }
                finally
                {
                    if (file != null) file.Close();
                }
                return success;
            }

            public static string GetConfigFilePath()
            {
                string filePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Config");
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                    if (!Directory.Exists(filePath)) filePath = Directory.GetCurrentDirectory();
                }
                return filePath;
            }

            public static string GetConfigFileFullName(string fileName)
            {
                return System.IO.Path.Combine(GetConfigFilePath(), fileName);
            }

            public static bool SaveConfig(object data, string fileName, bool zip = true)
            {
                return SaveDataFile(data, GetConfigFileFullName(fileName), zip);
            }

            public static bool SaveConfig(string data, string fileName, bool zip = true)
            {
                return SaveDataFile(data, GetConfigFileFullName(fileName), zip);
            }

            public static bool RestoreConfig(out string js, string fileName, bool zip = true)
            {
                return RestoreDataFile(out js, GetConfigFileFullName(fileName), zip);
            }

            public static T RestoreConfig<T>(string fileName, bool zip = true)
            {
                return RestoreDataFile<T>(GetConfigFileFullName(fileName), zip);
            }

            public static bool SaveDataFile(object data, string fileName, bool zip = true)
            {
                string js;
                try
                {
                    js = new JavaScriptSerializer().Serialize(data);
                }
                catch
                {
                    return false;
                }
                return SaveDataFile(js, fileName, zip);
            }

            public static bool SaveDataFile(string data, string fileName, bool zip = true)
            {

                if (zip) return SaveJaz(data, fileName);
                return SaveTextFile(data, fileName);
            }



            public static bool RestoreDataFile(out string js, string fileName, bool zip = true)
            {
                if (zip) return RestoreJaz(out js, fileName);
                return RestoreTextFile(out js, fileName);
            }

            public static T RestoreDataFile<T>(string fileName, bool zip = true)
            {
                T data;
                string js;
                if (!RestoreDataFile(out js, fileName, zip)) return default(T);
                try
                {
                    data = new JavaScriptSerializer().Deserialize<T>(js);

                }
                catch
                {
                    return default(T);
                }
                return data;
            }


            #region "File in Text format"

            public static bool SaveTextFile(string data, string fileName)
            {
                try
                {
                    File.WriteAllText(fileName, data);
                } catch
                {
                    return false;
                }
                return true;
            }

            public static bool RestoreTextFile(out string data, string fileName)
            {
                data = "";
                if (!File.Exists(fileName)) return false;
                try
                {
                    data = File.ReadAllText(fileName);
                }
                catch
                {
                    return false;
                }
                return true;
            }

            #endregion

            #region "File in Zip Format"

            public static bool SaveJaz(string data, string fileName)
            {
                try
                {
                    System.IO.MemoryStream msSinkCompressed = new System.IO.MemoryStream();
                    ZlibStream zOut = new ZlibStream(msSinkCompressed, CompressionMode.Compress, CompressionLevel.BestCompression, true);
                    CopyStream(StringToMemoryStream(data), zOut);
                    zOut.Close();
                    FileStream file = new FileStream(fileName, FileMode.Create, System.IO.FileAccess.Write);
                    msSinkCompressed.WriteTo(file);
                    file.Close();
                    return true;
                }
                catch { }
                return false;
            }

            public static bool RestoreJaz(out string data, string fileName)
            {
                try
                {
                    MemoryStream msSinkCompressed = new MemoryStream(File.ReadAllBytes(fileName));
                    msSinkCompressed.Seek(0, System.IO.SeekOrigin.Begin);
                    MemoryStream msSinkDecompressed = new System.IO.MemoryStream();
                    ZlibStream zOut = new ZlibStream(msSinkDecompressed, CompressionMode.Decompress, true);
                    CopyStream(msSinkCompressed, zOut);
                    data = MemoryStreamToString(msSinkDecompressed);
                    return true;
                }
                catch
                {
                    data = null;
                    return false;
                }
            }

            static System.IO.MemoryStream StringToMemoryStream(string s)
            {
                byte[] a = System.Text.Encoding.UTF8.GetBytes(s);
                return new System.IO.MemoryStream(a);
            }

            static String MemoryStreamToString(System.IO.MemoryStream ms)
            {
                byte[] ByteArray = ms.ToArray();
                return System.Text.Encoding.UTF8.GetString(ByteArray);
            }

            static void CopyStream(System.IO.Stream src, System.IO.Stream dest)
            {
                byte[] buffer = new byte[1024];
                int len = src.Read(buffer, 0, buffer.Length);
                while (len > 0)
                {
                    dest.Write(buffer, 0, len);
                    len = src.Read(buffer, 0, buffer.Length);
                }
                dest.Flush();
            }

            public static void WriteToBinaryFile<T>(string filePath, T objectToWrite, bool append = false)
            {
                using (Stream stream = File.Open(filePath, append ? FileMode.Append : FileMode.Create))
                {
                    var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    binaryFormatter.Serialize(stream, objectToWrite);
                }
            }

            public static T ReadFromBinaryFile<T>(string filePath)
            {
                using (Stream stream = File.Open(filePath, FileMode.Open))
                {
                    var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    return (T)binaryFormatter.Deserialize(stream);
                }
            }
        }

        #endregion
    }
}