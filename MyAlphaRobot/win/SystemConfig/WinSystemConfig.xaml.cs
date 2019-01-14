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
            foreach(string file in files)
            {
                cboRobotConfig.Items.Add(System.IO.Path.GetFileName(file));
            }
            if (cboRobotConfig.Items.Contains(SYSTEM.sc.robotConfigFile)) { 
                cboRobotConfig.SelectedValue = SYSTEM.sc.robotConfigFile;
            }
            txtBlocklyPath.Text = SYSTEM.sc.blocklyPath;
            cbxAutoCheckVersion.IsChecked = SYSTEM.sc.autoCheckVersion;
            cbxAutoCheckFirmware.IsChecked = SYSTEM.sc.autoCheckFirmware;
            cbxDisableBatteryUpdate.IsChecked = SYSTEM.sc.disableBatteryUpdate;
            cbxDisableMpuUpdate.IsChecked = SYSTEM.sc.disableMpuUpdate;
            cbxDeveloperMode.IsChecked = SYSTEM.sc.developerMode;
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
            SYSTEM.sc.robotConfigFile = (string) cboRobotConfig.SelectedValue;
            SYSTEM.sc.blocklyPath = txtBlocklyPath.Text ;
            SYSTEM.sc.autoCheckVersion = (cbxAutoCheckVersion.IsChecked == true);
            SYSTEM.sc.autoCheckFirmware = (cbxAutoCheckFirmware.IsChecked == true) ;
            SYSTEM.sc.developerMode = (cbxDeveloperMode.IsChecked == true);
            SYSTEM.sc.disableBatteryUpdate = (cbxDisableBatteryUpdate.IsChecked == true);
            SYSTEM.sc.disableMpuUpdate = (cbxDisableMpuUpdate.IsChecked == true);


            Util.SaveSystemConfig();
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
