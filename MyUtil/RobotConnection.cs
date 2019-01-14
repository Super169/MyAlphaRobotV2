using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MyUtil
{
    public class RobotConnection
    {
        public long cmdStartTicks, cmdEndTicks;
        protected UTIL.DelegateUpdateInfo updateInfo;
        protected void UpdateInfo(string msg = "", UTIL.InfoType iType = UTIL.InfoType.message, bool async = false)
        {
            updateInfo?.Invoke(msg, iType, async);
        }

        public enum connMode
        {
            Serial, Network
        }

        private connMode mode;
        private Robot_base robot;

        public connMode currMode
        {
            get { return mode;  }
        }

        Robot_Serial serial = new Robot_Serial();
        Robot_Net netClient = new Robot_Net();

        public void InitObject(UTIL.DelegateUpdateInfo fxUpdateInfo)
        {
            this.updateInfo = fxUpdateInfo;
            serial.InitialObject(fxUpdateInfo);
            netClient.InitialObject(fxUpdateInfo);
        }

        public bool isConnected
        {
            get
            {
                return (robot == null ? false : robot.isConnected);
            }
        }

        public int Available
        {
            get
            {
                return (robot == null ? 0 : robot.Available);
            }
        }

        private bool AlreadyConnected()
        {
            if (isConnected)
            {
                UpdateInfo(string.Format("Already connected to {0}", robot.target), UTIL.InfoType.alert);
                return true;
            }
            return false;
        }

        // Cannot use default as net connection has (string, int) already
        public bool Connect(string portName)
        {
            return Connect(portName, 115200, Parity.None, 8, StopBits.One);
        }

        public bool Connect(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
        {
            if (AlreadyConnected()) return false;

            mode = connMode.Serial;
            bool success = serial.Connect(portName, baudRate, parity, dataBits, stopBits);
            robot = (success ? serial : null);
            return success;
        }

        public bool Connect(IPAddress address, int port)
        {
            if (AlreadyConnected()) return false;

            mode = connMode.Network;
            bool success = netClient.Connect(address, port);
            robot = (success ? netClient : null);
            return success;
        }

        public bool Connect(string hostName, int port)
        {
            if (AlreadyConnected()) return false;

            mode = connMode.Network;
            bool success = netClient.Connect(hostName, port);
            robot = (success ? netClient : null);
            return success;
        }

        public bool Disconnect()
        {
            if (robot == null) return true;
            bool success = robot.Disconnect();
            if (success) robot = null;
            return success;
        }

        public void Close()
        {
            if (robot == null) return;
            robot.Close();
            robot = null;
        }

        public byte Read()
        {
            if (robot == null) return 0;
            return robot.Read();
        }

        public byte[] PeekAll()
        {
            if (robot == null) return (new byte[0]);
            return robot.ReadAll(false);
        }

        public byte[] ReadAll()
        {
            if (robot == null) return (new byte[0]);
            return robot.ReadAll();
        }

        public void ClearRxBuffer()
        {
            if (robot == null) return;
            robot.ClearRxBuffer();
        }

        public bool SendCommand(byte[] command, int count, long minBytes, long maxMs = -1)
        {
            if (maxMs == -1) maxMs = robot.DefaultCommandTimeout;
            if (robot == null) return false;
            robot.ClearRxBuffer();
            cmdEndTicks = 0;
            cmdStartTicks = DateTime.Now.Ticks;
            robot.Send(command, 0, count);
            if (minBytes > 0) robot.WaitForData(minBytes, maxMs, out cmdEndTicks);
            return (robot.Available >= minBytes);
        }

        public void SetSerialPorts(ComboBox comboPorts, string defaultPort = null)
        {
            serial.SetSerialPorts(comboPorts, defaultPort,250);
        }

        public void SetNetConnection(TextBox txtIP, TextBox txtPort)
        {
            netClient.SetNetConnection(txtIP, txtPort);
        }
    }
}
