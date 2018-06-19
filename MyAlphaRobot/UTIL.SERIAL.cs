using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MyAlphaRobot
{
    public static partial class UTIL
    {
        public class SERIAL
        {
            public static int FindPorts(string defaultPort, ComboBox cbo)
            {
                cbo.ItemsSource = SerialPort.GetPortNames();
                if (cbo.Items.Count > 0)
                {
                    if (defaultPort == null)
                    {
                        cbo.SelectedIndex = 0;
                    }
                    else
                    {
                        cbo.SelectedIndex = cbo.Items.IndexOf(defaultPort);
                        if (cbo.SelectedIndex < 0) cbo.SelectedIndex = 0;

                    }
                    cbo.IsEnabled = true;
                }
                else
                {
                    cbo.IsEnabled = false;
                }
                return cbo.Items.Count;
            }

            public static bool Connect(SerialPort serialPort, String portName)
            {
                bool flag = false;

                serialPort.PortName = portName;
                serialPort.BaudRate = 115200;
                serialPort.Parity = Parity.None;
                serialPort.DataBits = 8;
                serialPort.StopBits = StopBits.One;

                try
                {
                    serialPort.Open();
                    serialPort.DiscardInBuffer();
                    serialPort.DiscardOutBuffer();
                    flag = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                }
                return flag;
            }
        }
    }

}
