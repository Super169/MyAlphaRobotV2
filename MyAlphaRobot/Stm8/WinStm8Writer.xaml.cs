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
        byte[] enterTTL = { 0xA9, 0x9A, 0x04, 0x07, 0x00, 0x01, 0x0C, 0xED };
        byte[] exitTTL = { 0xA9, 0x9A, 0x01, 0x06, 0x09 };

        private void UpdateInfo(string msg = "", MyUtil.UTIL.InfoType iType = MyUtil.UTIL.InfoType.message, bool async = false)
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
                case MyUtil.UTIL.InfoType.message:
                    statusBar.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x7A, 0xCC));
                    break;
                case MyUtil.UTIL.InfoType.alert:
                    statusBar.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xCA, 0x51, 0x00));
                    break;
                case MyUtil.UTIL.InfoType.error:
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
            if (serialPort.IsOpen)
            {
                serialPort.Write(exitTTL, 0, exitTTL.Length);
                serialPort.Close();
            }
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
                serialPort.Write(exitTTL, 0, exitTTL.Length);
                serialPort.Close();
                if (serialPort.IsOpen)
                {
                    UpdateInfo("Fail to close connection", MyUtil.UTIL.InfoType.error);
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
                    UpdateInfo("No port selected", MyUtil.UTIL.InfoType.error);
                    return;
                }
                if (Util.SERIAL.Connect(serialPort, portName))
                {
                    UpdateInfo(String.Format("Connected to {0} in 115200, N, 8, 1", portName));
                }
            }
            if (serialPort.IsOpen)
            {
                // Send command to enter USB-TTL mode, and clear receive buffer
                // A9 9A 04 07 00 01 0C ED
                enterTTL[5] = (byte)cboGPIO.SelectedIndex;
                enterTTL[6] = (byte) (0x0B + enterTTL[5]);
                serialPort.Write(enterTTL, 0, enterTTL.Length);
                Thread.Sleep(1000);
                receiveBuffer.Clear();
            }
            SetStatus();
        }

        private void FindSerial(string defaultPort)
        {
            int nPorts = Util.SERIAL.FindPorts((string)cboPorts.SelectedValue, cboPorts, 250);
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
                    UpdateInfo(String.Format("Fail reading {0}", fileName), MyUtil.UTIL.InfoType.error);
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
