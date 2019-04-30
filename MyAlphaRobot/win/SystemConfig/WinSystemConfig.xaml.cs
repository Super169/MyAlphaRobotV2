using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for WinSystemConfig.xaml
    /// </summary>
    public partial class WinSystemConfig : Window
    {
        public WinSystemConfig()
        {
            InitializeComponent();
            InitObject();
        }

        private void InitObject()
        {
            cboRobotConfig.Items.Clear();
            string[] files = Directory.GetFiles(SYSTEM.configPath, "*.jaz", SearchOption.TopDirectoryOnly);
            foreach (string file in files)
            {
                cboRobotConfig.Items.Add(System.IO.Path.GetFileName(file));
            }
            if (cboRobotConfig.Items.Contains(SYSTEM.sc.robotConfigFile))
            {
                cboRobotConfig.SelectedValue = SYSTEM.sc.robotConfigFile;
            }
            txtBlocklyPath.Text = SYSTEM.sc.blocklyPath;
            cbxAutoCheckVersion.IsChecked = SYSTEM.sc.autoCheckVersion;
            cbxAutoCheckFirmware.IsChecked = SYSTEM.sc.autoCheckFirmware;
            txtWaitRebootSec.Text = SYSTEM.sc.waitRebootSec.ToString();
            cbxDisableBatteryUpdate.IsChecked = SYSTEM.sc.disableBatteryUpdate;
            cbxDisableMpuUpdate.IsChecked = SYSTEM.sc.disableMpuUpdate;
            cbxDeveloperMode.IsChecked = SYSTEM.sc.developerMode;
            cboFirmware.SelectedIndex = (int)SYSTEM.sc.firmwareType;
        }

        private void btnBlockly_Click(object sender, RoutedEventArgs e)
        {
            string blocklyPath = txtBlocklyPath.Text;
            if (Util.GetBlocklyPath(ref blocklyPath))
            {
                txtBlocklyPath.Text = blocklyPath;
            }

        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidInteger(txtWaitRebootSec, 1, 99, "等待启动时间")) return;
            try
            {
                SYSTEM.sc.robotConfigFile = (string)cboRobotConfig.SelectedValue;
                SYSTEM.sc.blocklyPath = txtBlocklyPath.Text;
                SYSTEM.sc.autoCheckVersion = (cbxAutoCheckVersion.IsChecked == true);
                SYSTEM.sc.autoCheckFirmware = (cbxAutoCheckFirmware.IsChecked == true);
                SYSTEM.sc.waitRebootSec = byte.Parse(txtWaitRebootSec.Text);
                SYSTEM.sc.developerMode = (cbxDeveloperMode.IsChecked == true);
                SYSTEM.sc.disableBatteryUpdate = (cbxDisableBatteryUpdate.IsChecked == true);
                SYSTEM.sc.disableMpuUpdate = (cbxDisableMpuUpdate.IsChecked == true);
                SYSTEM.sc.firmwareType = (SystemConfig.FIRMWARE)cboFirmware.SelectedIndex;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "输入资料错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Util.SaveSystemConfig();
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void tb_PreviewInteger(object sender, TextCompositionEventArgs e)
        {
            MyUtil.UTIL.INPUT.PreviewInteger(ref e);
        }

        static protected bool ValidInteger(TextBox tb, int min, int max, string fieldName)
        {
            int value;
            bool valid = int.TryParse(tb.Text, out value);
            if ((value >= min) && (value <= max)) return true;
            string msg = String.Format("{0} 的数值 '{3}' 不正确\n\n请输入 {1} 至 {2} 之间的数值.", fieldName, min, max, tb.Text);
            MessageBox.Show(msg, "输入错误", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        private void btnResetRobot_Click(object sender, RoutedEventArgs e)
        {
            string defaultRobot = MyUtil.FILE.AppFilePath(CONST.CONFIG_FOLDER, CONST.DEFAULT_CONFIG.ROBOT_CONFIG_FILE);
            if (MyUtil.UI.MessageConfirm(string.Format("是否重置回 Alpha 1 的模型?\n{0} 會被刪除.", defaultRobot)))
            {
                System.IO.File.Delete(defaultRobot);
                System.Threading.Thread.Sleep(500);
                cboRobotConfig.Text = "";
                if (System.IO.File.Exists(defaultRobot))
                {
                    MessageBox.Show(String.Format("删除档案失败, 请检查是否可以删去 {0}", defaultRobot));
                }
            }
        }


    }
}
