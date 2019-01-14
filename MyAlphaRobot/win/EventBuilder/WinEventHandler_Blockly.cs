using CefSharp;
using CefSharp.Wpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace MyAlphaRobot
{

    public partial class WinEventHandler : Window
    {
        const string blocklyCheckFile = "blockly_compressed.js";
        const string workFilePath = "_www";

        private string blocklyPath = "";
        private string blocklyUrl = "";
        private string emptyWorkspaceXml;
        private string robotWorkspaceXml;

        #region "Blockly Utilitiies"

        private bool CheckBlocklyPath()
        {
            blocklyPath = SYSTEM.sc.blocklyPath;
            if (!Util.IsBlocklyPath(blocklyPath))
            {
                MessageBox.Show("系統找不到 Blockly 相關檔案.\n請指出已安裝的 Blockly 檔案位置", "Blockly 設定");

                if (!Util.GetBlocklyPath(ref blocklyPath))
                {
                    MessageBox.Show("找不到 Blockly 檔案, 請先下載 Google Blockly.", "啟用 Blockly 失敗");
                    return false;
                }
                SYSTEM.sc.blocklyPath = blocklyPath;
                Util.SaveSystemConfig();
            }
            blocklyUrl = "file:///" + blocklyPath.Replace("\\", "/");
            return true;
        }

        private string WorkFile(string file)
        {
            return MyUtil.FILE.AppFilePath(workFilePath, file);
        }
        private string GetBlocklyFiles(string fileName)
        {
            /*
            fileName = WorkFile(fileName);
            if (!File.Exists(fileName)) return "";
            return System.IO.File.ReadAllText(WorkFile(fileName));
            */
            string data = "";
            string ns = this.GetType().Namespace;
            string resName = string.Format("{0}.{1}.{2}", ns, CONST.BLOCKLY.RESOURCE, fileName);
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resName))
            using (StreamReader reader = new StreamReader(stream))
            {
                data = reader.ReadToEnd();
            }
            return data;
        }

        private bool ReplaceBlockly(string defaultWorkspace = null)
        {

            string html, customBlocksJs, toolboxXml;

            /*
            string htmlPath = WorkFile("EventBuilder.html");
            if (!System.IO.File.Exists(htmlPath))
            {
                MessageBox.Show("System error: missing source html");
                return false;
            }

            html = System.IO.File.ReadAllText(htmlPath);
            customBlocksJs = System.IO.File.ReadAllText(WorkFile("custom_blocks.js"));
            toolboxXml = System.IO.File.ReadAllText(WorkFile("toolbox.xml"));
            emptyWorkspaceXml = System.IO.File.ReadAllText(WorkFile("workspace.xml"));
            */

            string WorkHTML = WorkFile("FinalBlockly.html");
            if (System.IO.File.Exists(WorkHTML))
            {
                System.IO.File.Delete(WorkHTML);
            }

            html = GetBlocklyFiles(CONST.BLOCKLY.HTML);
            customBlocksJs = GetBlocklyFiles(CONST.BLOCKLY.CUSTOM_BLOCKS);
            toolboxXml = GetBlocklyFiles(CONST.BLOCKLY.TOOLBOX);
            emptyWorkspaceXml = GetBlocklyFiles(CONST.BLOCKLY.WORKSPCE);


            robotWorkspaceXml = "";
            if ((defaultWorkspace != null) && (defaultWorkspace != ""))
            {
                robotWorkspaceXml = defaultWorkspace;
            }
            html = html.Replace("<blocklyUrl>", blocklyUrl);
            html = html.Replace("/* custom_blocks.js */", customBlocksJs);
            html = html.Replace("<!-- toolbox.xml -->", toolboxXml);
            html = html.Replace("<!-- workspace.xml -->", (robotWorkspaceXml == "" ? emptyWorkspaceXml : robotWorkspaceXml));

            System.IO.File.WriteAllText(WorkHTML, html);
            WorkHTML = "file:///" + WorkHTML.Replace("\\", "/");

            requestReset = true;
            browser.Address = WorkHTML;

            return true;
        }

        private string XmlToString(XmlDocument xml)
        {
            string xmlString = "";
            using (StringWriter sw = new StringWriter())
            using (XmlWriter xw = XmlWriter.Create(sw))
            {
                xml.WriteTo(xw);
                xw.Flush();
                xmlString = sw.GetStringBuilder().ToString();
            }
            return xmlString;
        }

        #endregion


        private bool InitBlockly()
        {
            if (!CheckBlocklyPath()) return false;
            if (!InitWorkspace()) return false;
            return true;
        }

        private bool InitWorkspace()
        {
            //TODO: just test using sample data
            string robotWorkSpace;
            if (!GetRobotSetting(out robotWorkSpace)) return false;
            if (!ReplaceBlockly(robotWorkSpace)) return false;


            return true;
        }

        private bool GetRobotSetting(out string robotWorkSpace)
        {
            byte[] dataIdle, dataBusy;
            robotWorkSpace = "";
            if (!GetRobotEvent(0, out dataIdle)) return false;
            if (!GetRobotEvent(1, out dataBusy)) return false;

            XmlDocument root = new XmlDocument();
            XmlElement parent = root.CreateElement(string.Empty, "xml", string.Empty);
            parent.SetAttribute("id", "workspaceBlocks");
            parent.SetAttribute("style", "display:none");
            root.AppendChild(parent);

            RobotHandler rhIdle = new RobotHandler(dataIdle, BLOCKLY.HAT.IDLE, 0, 0);
            RobotHandler rhBusy = new RobotHandler(dataBusy, BLOCKLY.HAT.BUSY, 400, 0);

            parent.AppendChild(rhIdle.ToXml(root));
            parent.AppendChild(rhBusy.ToXml(root));

            robotWorkSpace = XmlToString(root);
            return true;
        }

        private bool GetRobotEvent(byte mode, out byte[] data)
        {
            byte[] header = UBT.V2_GetEventHeader(mode);
            if (header == null)
            {
                data = null;
                return false;
            }
            int evtCount = header[CONST.EH.OFFSET.COUNT];
            data = new byte[evtCount * BLOCKLY.EVENT.SIZE];
            byte startIdx = 0;
            while (startIdx < evtCount)
            {
                byte[] result = UBT.V2_GetEventData(mode, startIdx);
                if (result == null) return false;
                byte count = result[CONST.ED.OFFSET.COUNT];
                if (count > CONST.ED.BATCH_SIZE) return false;  // Abnormal case
                if (startIdx + count > evtCount) return false;
                int startPos = startIdx * BLOCKLY.EVENT.SIZE;
                int bytes = count * BLOCKLY.EVENT.SIZE;
                for (int i = 0; i < bytes; i++)
                {
                    data[startPos + i] = result[CONST.ED.OFFSET.DATA + i];
                }
                startIdx += count;
            }
            return true;

        }




    }
}
