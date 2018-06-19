using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MyAlphaRobot
{
    /// <summary>
    /// Interaction logic for WinStm8Writer.xaml
    /// </summary>
    public partial class WinStm8Writer : Window
    {

        private void UpdateInfo(string msg = "", UTIL.InfoType iType = UTIL.InfoType.message, bool async = false)
        {
            if (Dispatcher.FromThread(Thread.CurrentThread) == null)
            {
                if (async)
                {
                    Application.Current.Dispatcher.BeginInvoke(
                      System.Windows.Threading.DispatcherPriority.Normal,
                      (Action)(() => UpdateInfo(msg, iType, async)));
                    return;
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(
                      System.Windows.Threading.DispatcherPriority.Normal,
                      (Action)(() => UpdateInfo(msg, iType, async)));
                    return;
                }
            }
            // Update UI is allowed here
            switch (iType)
            {
                case UTIL.InfoType.message:
                    statusBar.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x7A, 0xCC));
                    break;
                case UTIL.InfoType.alert:
                    statusBar.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xCA, 0x51, 0x00));
                    break;
                case UTIL.InfoType.error:
                    statusBar.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x00, 0x00));
                    break;

            }
            statusInfoTextBlock.Text = msg;
        }

        public WinStm8Writer()
        {
            InitializeComponent();
            InitializeSerial();
            FindSerial("");
            this.Closing += WinStm8Writer_Closing;
        }

        private void WinStm8Writer_Closing(object sender, CancelEventArgs e)
        {
            if ((TBurn != null) && (TBurn.ThreadState != ThreadState.Stopped))
            {
                if (MessageBox.Show("可能會導致固件損壞, 確定要離開?", "燒錄進行中", MessageBoxButton.YesNo) == MessageBoxResult.No)
                {
                    e.Cancel = true;
                    return;
                }
                TBurn.Abort();
            }
            if (serialPort.IsOpen) serialPort.Close();
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            FindSerial((string)cboPorts.SelectedValue);
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            UpdateInfo();
            if (serialPort.IsOpen)
            {
                serialPort.Close();
                if (serialPort.IsOpen)
                {
                    UpdateInfo("Fail to close connection", UTIL.InfoType.error);
                }
                else
                {
                    UpdateInfo("Connection closed");
                }
            }
            else
            {
                string portName = (string)cboPorts.SelectedValue;
                if (portName == "")
                {
                    UpdateInfo("No port selected", UTIL.InfoType.error);
                    return;
                }
                if (UTIL.SERIAL.Connect(serialPort, portName))
                {
                    UpdateInfo(String.Format("Connected to {0} in 115200, N, 8, 1", portName));
                }
            }
            SetStatus();
        }

        private void FindSerial(string defaultPort)
        {
            int nPorts = UTIL.SERIAL.FindPorts((string)cboPorts.SelectedValue, cboPorts);
            UpdateInfo(String.Format("{0} serial port{1} found", (nPorts > 0 ? nPorts.ToString() : "No"), (nPorts > 1 ? "s" : "")));
            SetStatus();
        }

        private void SetStatus()
        {
            cboPorts.IsEnabled = !serialPort.IsOpen;
            btnRefresh.IsEnabled = !serialPort.IsOpen;
            btnConnect.Content = (serialPort.IsOpen ? "断开" : "联机");
            btnConnect.IsEnabled = (cboPorts.Items.Count > 0);
            btnBurn.IsEnabled = (serialPort.IsOpen && hexFileReady);
        }


        private void btnLoadFile_Click(object sender, RoutedEventArgs e)
        {
            UpdateInfo();
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "hex files|*.hex";
            if (openFileDialog.ShowDialog() == true)
            {
                String fileName = openFileDialog.FileName.Trim();
                hexFileReady = ReadHexFile(fileName);
                if (hexFileReady)
                {
                    txtFileName.Text = fileName;
                }
                else
                {
                    UpdateInfo(String.Format("Fail reading {0}", fileName), UTIL.InfoType.error);
                }
                SetStatus();
            }
        }

        private bool ReadHexFile(string fileName)
        {
            if (!File.Exists(fileName)) return false;
            StreamReader file = new StreamReader(fileName);
            string line;

            List<string> inData = new List<string>();

            while ((line = file.ReadLine()) != null)
            {
                inData.Add(line);
            }
            file.Close();
            if (inData.Count > 0)
            {
                sendData.ReadFile(inData);
            }
            int rowSend = sendData.DataRowCount;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < rowSend; i++)
            {
                sb.AppendLine(inData[i]);
            }
            sb = new StringBuilder();
            for (int i = rowSend; i < inData.Count; i++)
            {
                sb.AppendLine(inData[i]);
            }
            return true;
        }

        private void btnCloseClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
