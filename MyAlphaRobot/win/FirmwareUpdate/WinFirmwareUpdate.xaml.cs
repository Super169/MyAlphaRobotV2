using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
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
    /// Interaction logic for WinFirmwareUpdate.xaml
    /// </summary>
    public partial class WinFirmwareUpdate : Window
    {
        const string UNKNOEN_VERSION = "??.??.??.??";
        string urlFirmware = "";
        string fwVersion = "";
        string fwFileName = "";
        string fwURL = "";

        bool componentReady = false;

        public WinFirmwareUpdate()
        {
            InitializeComponent();
            componentReady = true;
            InitObjects();
        }

        private void OnWindowClosing(object sender, CancelEventArgs e)
        {
            if ((TBurn != null) && (TBurn.ThreadState != System.Threading.ThreadState.Stopped))
            {
                if (MessageBox.Show("可能会导致固件损坏, 确定要离开?", "烧录进行中", MessageBoxButton.YesNo) == MessageBoxResult.No)
                {
                    e.Cancel = true;
                    return;
                }
                TBurn.Abort();
            }
            if (serialPort.IsOpen)
            {
                serialPort.Close();
            }
        }

        #region "UI handling - Info, Output, Log, Status, Progress"

        private void UpdateInfo(string msg = "", MyUtil.UTIL.InfoType iType = MyUtil.UTIL.InfoType.message, bool async = false)
        {
            MyUtil.UI.UpdateInfo(statusBar, statusInfoTextBlock, msg, iType, async);
        }

        private void AppendOutput(string text, bool async = false)
        {
            MyUtil.UI.AppendTextbox(txtOutput, text + "\n");
        }

        private void AppendLog(string text, bool async = false)
        {
            text = DateTime.Now.ToString("hh:mm:ss.fff ") + text + "\n";
            MyUtil.UI.AppendTextbox(txtLog, text);
        }

        // This function must be executed in UI Thread
        private void UISetStatus(Object parm)
        {
            if (!componentReady) return;
            if (string.IsNullOrWhiteSpace((string)lblVersion.Content)) lblVersion.Content = UNKNOEN_VERSION;
            btnCloudDownload.IsEnabled = ((string)lblVersion.Content != UNKNOEN_VERSION);
            btnCloudBurn.IsEnabled = cboPorts.HasItems && btnCloudDownload.IsEnabled;
            btnBurn.IsEnabled = cboPorts.HasItems && !string.IsNullOrWhiteSpace(txtFirmware.Text);
        }

        // Being called from any thread
        private void SetStatus()
        {
            if (!componentReady) return;
            MyUtil.UI.RunInUIThread(UISetStatus);
        }

        // This function must be executed in UI Thread
        private void UIUpdateProgress(Object parm)
        {
            int value = (int)parm;
            pbProgress.Value = value;
            lblProgress.Content = string.Format("{0}%", value);
        }

        private void UpdateProgress(int value, bool async = false)
        {
            MyUtil.UI.RunInUIThread(UIUpdateProgress, value, async);
        }

        private void UISetActionGrid(object parm)
        {
            gridAction.IsEnabled = (bool) parm;
        }

        private void SetActionGrid(bool enabled, bool async = false)
        {
            MyUtil.UI.RunInUIThread(UISetActionGrid, enabled, async);
        }

        #endregion

        private bool BurnInProgress()
        {
            return ((TBurn != null) && (TBurn.ThreadState != System.Threading.ThreadState.Stopped));
        }


        private void InitObjects()
        {
            this.Closing += OnWindowClosing;
            InitializeSerial();
            FindSerial((string)MyUtil.UTIL.ReadRegistry(MyUtil.UTIL.KEY.LAST_CONNECTION_SERIAL));
            cboVersion.SelectedIndex = (RCVersion.IsBeta() ? 1 : 0);
            SetFwInfo();
            SetStatus();
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            UpdateInfo();
            FindSerial((string)cboPorts.SelectedValue);
            SetStatus();
        }

        private void cboVersion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateInfo();
            SetFwInfo();
            SetStatus();
        }

        private void btnCheckVersion_Click(object sender, RoutedEventArgs e)
        {
            UpdateInfo();
            Util.CheckFirmware();
            SetFwInfo();
            SetStatus();
        }

        private void SetFwInfo()
        {
            if (!componentReady) return;  // prevent error if called during initialization
            fwVersion = "";
            if (SYSTEM.firmwareChecked)
            {
                if (cboVersion.SelectedIndex == 0)
                {
                    fwVersion = SYSTEM.firmwareRelease;
                    urlFirmware = CONST.DISTRIBUTION.FIRMWARE.RELEASE.PATH;
                }
                else
                {
                    fwVersion = SYSTEM.firmwareBeta;
                    urlFirmware = CONST.DISTRIBUTION.FIRMWARE.BETA.PATH;
                }
            }
            fwFileName = string.Format("firmware.{0}.bin", fwVersion);
            fwURL = urlFirmware + fwVersion.Substring(0, fwVersion.LastIndexOf(".") + 1).Replace(".", "/") + fwFileName;
            lblVersion.Content = (string.IsNullOrWhiteSpace(fwVersion) ? UNKNOEN_VERSION : fwVersion);
        }

        private void btnCloudDownload_Click(object sender, RoutedEventArgs e)
        {
            UpdateInfo();
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() != CommonFileDialogResult.Ok) return;
            if (string.IsNullOrWhiteSpace(dialog.FileName)) return;
            string target = System.IO.Path.Combine(dialog.FileName, fwFileName);
            AppendLog(string.Format("下載{0}固件版本: {1}", (cboVersion.SelectedIndex == 0 ? "發佈版" : "測試版"), fwVersion));
            AppendLog(string.Format("下截地址: {0}", fwURL));
            AppendLog(string.Format("本地儲存: {0}", target));
            try
            {
                WebClient client = new WebClient();
                client.DownloadFile(fwURL, target);
                txtFirmware.Text = target;
            }
            catch (Exception ex)
            {
                AppendLog(string.Format("下載失敗: {0}", ex.Message));
                return;
            }
            AppendLog("下載完成");
            SetStatus();
        }

        private void btnCloudBurn_Click(object sender, RoutedEventArgs e)
        {
            UpdateInfo();
            UpdateProgress(0);
            AppendLog(string.Format("下載{0}固件版本 {1} 直接進行燒錄", (cboVersion.SelectedIndex == 0 ? "發佈版" : "測試版"), fwVersion));
            AppendLog(string.Format("下截地址: {0}", fwURL));
            try
            {
                WebClient client = new WebClient();
                firmware = client.DownloadData(fwURL);
            }
            catch (Exception ex)
            {
                AppendLog(string.Format("下載失敗: {0}", ex.Message));
            }
            AppendLog(string.Format("下載完成, {0:n0} bytes 下載了", firmware.Length));
            FlashFirmware();

        }

        private void btnLoadROM_Click(object sender, RoutedEventArgs e)
        {
            UpdateInfo();
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "固件档|*.bin";
            if (openFileDialog.ShowDialog() == true)
            {
                String fileName = openFileDialog.FileName.Trim();
                txtFirmware.Text = fileName;
            }
            SetStatus();
        }

        private void btnBurn_Click(object sender, RoutedEventArgs e)
        {
            UpdateInfo();
            UpdateProgress(0);
            if (txtFirmware.Text.Trim() == "")
            {
                MessageBox.Show("請先選擇固件");
                return;
            }

            filename = txtFirmware.Text.Trim();
            if (filename == "")
            {
                MessageBox.Show("請先選取要燒錄的固件檔");
                return;
            }

            try
            {
                firmware = File.ReadAllBytes(filename);
            }
            catch (Exception ex)
            {
                AppendLog(string.Format("讀取固件檔案 {0} 出錯: {1}", filename, ex.Message));
                return;
            }
            AppendLog(string.Format("準備燒固件 {0}", filename));
            FlashFirmware();
        }

        private void btnQuit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void txtLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtLog.ScrollToEnd();
        }
    }
}
