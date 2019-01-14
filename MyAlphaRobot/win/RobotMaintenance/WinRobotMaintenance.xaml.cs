using Microsoft.Win32;
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
    /// Interaction logic for WinRobotMaintenance.xaml
    /// </summary>
    public partial class WinRobotMaintenance : Window
    {

        public WinRobotMaintenance()
        {
            InitializeComponent();
            InitControls();
        }

        private void InitControls()
        {
            ucRobotMaintenance.SetConfig(SYSTEM.configObject);
        }

        private void btnLoadImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = SYSTEM.configPath;
            //            openFileDialog.Filter = "JPG|*.jpg|PNG|*.png|AL (*.*)|*.*";
            openFileDialog.Filter = "Image file (JPG, PNG, BMP)|*.jpg;*.png;*.bmp|AL (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                ucRobotMaintenance.ChangeImage(openFileDialog.FileName.Trim());
            }
        }

        private void btnLoadConfig_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = SYSTEM.configPath;
            openFileDialog.Filter = CONST.ROBOT_CONFIG_FILTER;
            if (openFileDialog.ShowDialog() == true)
            {
                string fileName = openFileDialog.FileName.Trim();
                ConfigObject co = MyUtil.FILE.RestoreDataFile<ConfigObject>(fileName);
                if (!Object.ReferenceEquals(co, null))
                {
                    ucRobotMaintenance.SetConfig(co);
                }
            }
        }

        private void btnSaveConfig_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = SYSTEM.configPath;
            saveFileDialog.Filter = CONST.ROBOT_CONFIG_FILTER;
            saveFileDialog.FileName = CONST.DEFAULT_CONFIG.ROBOT_CONFIG_FILE;
            if (saveFileDialog.ShowDialog() == true)
            {
                string fileName = saveFileDialog.FileName.Trim();
                ConfigObject co = ucRobotMaintenance.GetConfigObject();
                MyUtil.FILE.SaveDataFile(co, fileName);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
