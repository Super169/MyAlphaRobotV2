using CefSharp;
using CefSharp.Wpf;
using Microsoft.Win32;
using MyUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MyAlphaRobot
{
    /// <summary>
    /// Interaction logic for WinEventHandler.xaml
    /// </summary>
    public partial class WinEventHandler : Window
    {
        UTIL.DelegateUpdateInfo updateInfo;
        protected void UpdateInfo(string msg = "", MyUtil.UTIL.InfoType iType = MyUtil.UTIL.InfoType.message, bool async = false)
        {
            updateInfo?.Invoke(msg, iType, async);
        }

        UTIL.DelegateCommandHandler commandHandler;
        protected void ParentCommand(string command, object parm)
        {
            commandHandler?.Invoke(command, parm);
        }

        UBTController UBT;
        public ChromiumWebBrowser browser;
        bool requestReset = false;

        public WinEventHandler(UTIL.DelegateUpdateInfo fxUpdateInfo,
                               UTIL.DelegateCommandHandler fxCommandHandler)
        {
            updateInfo = fxUpdateInfo;
            commandHandler = fxCommandHandler;
            InitializeComponent();
            InitConfig();
            InitBrowser();
        }

        private void InitBrowser()
        {
            browser = new ChromiumWebBrowser();
            gridBrowser.Children.Add(browser);
            browser.LoadingStateChanged += OnLoadingStateChanged;
        }

        private void OnLoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            // Due to problem of Blockly in handling empty block XML using <block ,, />
            // Just reset after load complete, only need to do once
            if (!e.IsLoading)
            {
                if (requestReset)
                {
                    requestReset = false;
                    browser.ExecuteScriptAsync("loadWorkspace", new object[] { robotWorkspaceXml });
                }
            }
        }

        public bool InitObject(UBTController UBT)
        {
            this.UBT = UBT;
            return InitBlockly();
        }

        private void btnBackup_Click(object sender, RoutedEventArgs e)
        {
            UpdateInfo();
            BackupEvents();
        }

        private void btnRestore_Click(object sender, RoutedEventArgs e)
        {
            UpdateInfo();
            RestoreEvents();
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            UpdateInfo();
            browser.ExecuteScriptAsync("loadWorkspace", new object[] { robotWorkspaceXml });
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            UpdateInfo();
            browser.ExecuteScriptAsync("loadWorkspace", new object[] { emptyWorkspaceXml });
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            UpdateInfo();
            if (!UI.MessageConfirm("儲存事件處理設定?")) return;
            SaveEvents();

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            UpdateInfo();
            this.Close();                
        }


        private void BackupEvents()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = SYSTEM.configPath;
            saveFileDialog.Filter = CONST.BLOCKLY.EVENT_FILE_FILTER;
            saveFileDialog.FileName = CONST.BLOCKLY.DEFAULT_EVENT_FILE;
            if (saveFileDialog.ShowDialog() != true) return;
            string fileName = saveFileDialog.FileName.Trim();

            string xml = "";
            browser.EvaluateScriptAsync("getXml();").ContinueWith(x =>
            {
                JavascriptResponse response = x.Result;
                if (response.Success)
                {

                    xml = response.Result.ToString();
                    BackupXml(fileName, xml);
                }
                else
                {
                    MessageBox.Show("Fail getting data from Blockly");
                }
            });
        }

        private void BackupXml(string fileName, string xml)
        {
            MyUtil.FILE.SaveDataFile(xml, fileName, true);
        }

        private void RestoreEvents()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = SYSTEM.configPath;
            openFileDialog.Filter = CONST.BLOCKLY.EVENT_FILE_FILTER;
            openFileDialog.FileName = CONST.BLOCKLY.DEFAULT_EVENT_FILE;
            if (openFileDialog.ShowDialog() != true) return;

            string fileName = openFileDialog.FileName.Trim();
            string xml = "";
            if (!MyUtil.FILE.RestoreDataFile(out xml, fileName, true))
            {
                return;
            }
            browser.ExecuteScriptAsync("loadWorkspace", new object[] { xml });
        }
    }
}
