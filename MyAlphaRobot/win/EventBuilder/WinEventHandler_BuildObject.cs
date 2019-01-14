using CefSharp;
using CefSharp.Wpf;
using Microsoft.Win32;
using MyUtil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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


        private void SaveEvents()
        {
           string xml = "";
            browser.EvaluateScriptAsync("getXml();").ContinueWith(x =>
            {
                JavascriptResponse response = x.Result;
                if (response.Success)
                {

                    xml = response.Result.ToString();
                    BuildObject(xml);
                }
                else
                {
                    MessageBox.Show("Fail getting data from Blockly");
               }
            });
        }

        public static string RemoveXmlNamespace(string xmlDocument)
        {
            return Regex.Replace(xmlDocument, "xmlns=\".*?\"", "");
        }

        private StringBuilder sbObjInfo;

        private void BuildObject(string xmlStr)
        {
            sbObjInfo = new StringBuilder();
            xmlStr = RemoveXmlNamespace(xmlStr);

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(xmlStr);

            string nodePath;
            nodePath = string.Format("//block[@type=\"{0}\"]", BLOCKLY.HAT.BUSY);
            XmlNodeList busyNode = xml.SelectNodes(nodePath);
            if (busyNode.Count == 0)
            {
                MessageBox.Show("動作執行中事件遺失了");
                return;
            }
            if (busyNode.Count > 1)
            {
                MessageBox.Show("超過一個動作執行中事件");
                return;
            }

            nodePath = string.Format("//block[@type=\"{0}\"]", BLOCKLY.HAT.IDLE);
            XmlNodeList idleNode = xml.SelectNodes(nodePath);
            if (idleNode.Count == 0)
            {
                MessageBox.Show("閒置事件遺失了");
                return;
            }
            if (idleNode.Count > 1)
            {
                MessageBox.Show("超過一個閒置事件");
                return;
            }

            RobotHandler idle = new RobotHandler(idleNode[0], BLOCKLY.HAT.IDLE, 0, 0);
            RobotHandler busy = new RobotHandler(busyNode[0], BLOCKLY.HAT.BUSY, 400, 0);

            ParentCommand(CONST.COMMAND.StartSystemWork, false);
            bool success = UpdateRobotEvents(idle, (byte)RobotHandler.MODE.idle) &&
                           UpdateRobotEvents(busy, (byte)RobotHandler.MODE.busy);
            ParentCommand(CONST.COMMAND.EndSystemWork, false);

            if (success)
            {
                MessageBox.Show("事件處理設定已更新, 請重啟機械人.");
            }
            return;
        }

        private bool UpdateRobotEvents(RobotHandler rh, byte mode)
        {
            byte count = (byte)rh.events.Count;
            if (!UBT.V2_SaveEventHeader(mode, count, (byte)RobotHandler.ACTION.before)) return false;
            try
            {
                byte[] data = rh.ToBytes();
                byte startIdx = 0;
                while (startIdx < count)
                {
                    byte batchCount = (byte)(count - startIdx);
                    if (batchCount > CONST.ED.BATCH_SIZE) batchCount = CONST.ED.BATCH_SIZE;
                    if (!UBT.V2_SaveEventData(mode, startIdx, batchCount, data)) return false;
                    startIdx += batchCount;
                }
            }
            catch
            {
                MessageBox.Show("事件設定不正常, 請小心檢查一下.");
                return false;
            }
            if (!UBT.V2_SaveEventHeader(mode, count, (byte)RobotHandler.ACTION.after)) return false;
            return true;
        }


    }
}
