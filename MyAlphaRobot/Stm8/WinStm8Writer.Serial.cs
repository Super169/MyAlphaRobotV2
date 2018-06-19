using System;
using System.Collections.Generic;
using System.IO.Ports;
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
    /// Interaction logic for WinStm8Writer.xaml
    /// </summary>
    public partial class WinStm8Writer : Window
    {
        SerialPort serialPort = new SerialPort();
        List<Byte> receiveBuffer = new List<byte>();

        public void InitializeSerial()
        {
            serialPort.DataReceived += SerialPort_DataReceived;
        }

        private void SerialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            System.IO.Ports.SerialPort sp = sender as System.IO.Ports.SerialPort;

            if (sp == null) return;

            int bytesToRead = sp.BytesToRead;
            byte[] tempBuffer = new byte[bytesToRead];

            sp.Read(tempBuffer, 0, bytesToRead);

            receiveBuffer.AddRange(tempBuffer);
        }

        private void SerialSendByte(byte data, bool updateProgress = false)
        {
            SerialSendByte(new Byte[] { data }, updateProgress);
        }

        private void SerialSendByte(byte[] data, bool updateProgress = false)
        {
            if ((data == null) || (data.Length == 0)) return;
            serialPort.Write(data, 0, data.Length);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sb.Append(String.Format(" {0:X2}", data[i]));
            }
            UpdateLog(sb.ToString(), updateProgress);
        }
    }
}
