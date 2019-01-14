using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using MyUtil;

namespace MyUtil
{
    public class Robot_Serial : Robot_base
    {
        private SerialPort serialPort = new SerialPort();

        public Robot_Serial()
        {
            serialPort.DataReceived += SerialPort_DataReceived;
            DEFAULT_COMMAND_TIMEOUT = 1000;
            MAX_WAIT_MS = 1000;
        }

        private void SerialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            System.IO.Ports.SerialPort sp = sender as System.IO.Ports.SerialPort;

            if (sp == null) return;

            int bytesToRead = sp.BytesToRead;
            byte[] tempBuffer = new byte[bytesToRead];

            sp.Read(tempBuffer, 0, bytesToRead);

            AddRxData(tempBuffer);
            CallOnDataReceived(this, tempBuffer);
        }


        public override bool isConnected
        {
            get
            {
                return serialPort.IsOpen;
            }
        }

        public bool Connect(string portName, int baudRate = 115200, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One)
        {
            RobotConnParm parm = new RobotConnParm()
            {
                mode = RobotConnMode.bufferMode,
                portName = portName,
                baudRate = baudRate,
                parity = parity,
                dataBits = dataBits,
                stopBits = stopBits
            };
            return Connect(parm);
        }

        public override bool Connect(RobotConnParm parm)
        {
            bool flag = false;

            if (isConnected)
            {
                UpdateInfo(string.Format("Already connected to {0}", connTarget), UTIL.InfoType.alert);
                return false;
            }

            serialPort.PortName = parm.portName;
            serialPort.BaudRate = parm.baudRate;
            serialPort.Parity = parm.parity;
            serialPort.DataBits = parm.dataBits;
            serialPort.StopBits = parm.stopBits;

            connTarget = String.Format("{0}-{1},{2},{3},{4}",
                                       parm.portName, parm.baudRate, parm.parity, parm.dataBits, parm.stopBits);

            try
            {
                CallOnConnect(this, eventStatus.before);
                serialPort.Open();
                CallOnConnect(this, eventStatus.after);
                if (isConnected)
                {
                    serialPort.DiscardInBuffer();
                    serialPort.DiscardOutBuffer();
                    UpdateInfo(string.Format("Port {0} connected - 115200, N, 8, 1", serialPort.PortName));
                    UTIL.WriteRegistry(UTIL.KEY.LAST_CONNECTION_SERIAL, parm.portName);
                    flag = true;
                }
                else
                {
                    UpdateInfo(string.Format("Fail connecting to Port {0} - 115200, N, 8, 1", serialPort.PortName), UTIL.InfoType.error);
                }
            }
            catch (Exception ex)
            {
                UpdateInfo("Error: " + ex.Message, UTIL.InfoType.error);
            }
            if (!flag) connTarget = "{" + connTarget + "} - failed";

            return flag;
        }

        public override bool Disconnect()
        {
            if (!isConnected)
            {
                UpdateInfo(string.Format("Port {0} not yet connected", serialPort.PortName), UTIL.InfoType.alert);
                return true;
            }

            UpdateInfo();

            CallOnDisconnect(this, eventStatus.before);
            serialPort.Close();
            CallOnDisconnect(this, eventStatus.after);

            // Still connecting, seems some error here
            if (isConnected)
            {
                UpdateInfo(string.Format("Fail to disconnect Port {0}", serialPort.PortName), UTIL.InfoType.error);
                return false;
            }
            connTarget = null;
            UpdateInfo(string.Format("Port {0} disconnected", serialPort.PortName));
            return true;
        }

        // Force close without any message
        public override void Close() {
            if (isConnected)
            {
                CallOnClose(this, eventStatus.before);
                serialPort.Close();
                CallOnClose(this, eventStatus.after);
            }
        }

        public string LastConnection
        {
            get
            {
                return (string)UTIL.ReadRegistry(UTIL.KEY.LAST_CONNECTION_SERIAL);
            }
        }

        public string[] GetPortNames()
        {
            return SerialPort.GetPortNames();
        }

        public void SetSerialPorts(ComboBox comboPorts, string defaultPort = null, int excludePort = 65535)
        {
            if (defaultPort == null) defaultPort = LastConnection;
            // comboPorts.ItemsSource = SerialPort.GetPortNames();
            string[] ports = SerialPort.GetPortNames();
            comboPorts.Items.Clear();
            for (int i = 0; i < ports.Length; i++)
            {
                string port = ports[i];
                if (port.StartsWith("COM"))
                {
                    try
                    {
                        int portNum = int.Parse(port.Replace("COM", ""));
                        // Try to exculde virtual ports
                        if (portNum >= excludePort) port = "";
                    }
                    catch
                    {
                        port = "";
                    }
                }
                if (port != "") comboPorts.Items.Add(port);
            }

            if (comboPorts.Items.Count > 0)
            {
                if ((defaultPort == null) || (defaultPort.Trim() == ""))
                {
                    comboPorts.SelectedIndex = 0;
                }
                else
                {
                    defaultPort = defaultPort.Trim();
                    comboPorts.SelectedIndex = comboPorts.Items.IndexOf(defaultPort);
                    if (comboPorts.SelectedIndex < 0) comboPorts.SelectedIndex = 0;
                }
                comboPorts.IsEnabled = true;
            }
            else
            {
                comboPorts.IsEnabled = false;
            }
        }

        public override void Send(byte[] data, int offset, int count)
        {
            serialPort.Write(data, offset, count);
        }

    }
}
