using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MyAlphaRobot
{
    public partial class MainWindow : Window
    {
        private void FindPorts(string defaultPort)
        {
            portsComboBox.ItemsSource = SerialPort.GetPortNames();
            if (portsComboBox.Items.Count > 0)
            {
                UpdateInfo(String.Format("{0} ports detected", portsComboBox.Items.Count));
                if (defaultPort == null)
                {
                    portsComboBox.SelectedIndex = 0;
                }
                else
                {
                    portsComboBox.SelectedIndex = portsComboBox.Items.IndexOf(defaultPort);
                    if (portsComboBox.SelectedIndex < 0) portsComboBox.SelectedIndex = 0;

                }
                portsComboBox.IsEnabled = true;
                connectButton.IsEnabled = true;
            }
            else
            {
                portsComboBox.IsEnabled = false;
                connectButton.IsEnabled = false;
            }
        }
    }
}
