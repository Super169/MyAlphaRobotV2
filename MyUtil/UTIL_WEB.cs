using Ionic.Zlib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

namespace MyUtil
{
    public static class WEB
    {
        public static string GetTextFile(string url)
        {
            string data = "";
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream());

                data = reader.ReadToEnd();
            } catch {  }
            return data;
        }

    }
}
