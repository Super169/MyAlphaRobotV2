using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml;

namespace MyUtil
{
    public static partial class XML
    {
        public static bool GetSingleNode(XmlNode root, string xpath, out XmlNode node  )
        {
            XmlNodeList nodes = root.SelectNodes(xpath);
            if (nodes.Count == 1)
            {
                node = nodes[0];
                return true;
            }
            node = null;
            return false;
        }
    }
}
