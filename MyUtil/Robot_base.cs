using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyUtil
{

    public enum RobotConnMode
    {
        eventMode, bufferMode
    }

    public class RobotConnParm
    {
        public RobotConnMode mode;
        
        // For serial connection
        public string portName;
        public int baudRate;
        public Parity parity;
        public int dataBits;
        public StopBits stopBits;

        // For scoket connection
        public IPAddress address;
        public string hostName;
        public int port;
    }

    public abstract class Robot_base
    {
        protected long DEFAULT_COMMAND_TIMEOUT = 1000;    // Default 10ms, according to spec, servo return at 400us, add some overhead for ESP8266 handler
        protected long MAX_WAIT_MS = 1000;             // Default 1s, for any command, it should not wait more than 1s

        public long DefaultCommandTimeout {
            get { return DEFAULT_COMMAND_TIMEOUT;  }
        }

        protected UTIL.DelegateUpdateInfo updateInfo;
        protected void UpdateInfo(string msg = "", UTIL.InfoType iType = UTIL.InfoType.message, bool async = false)
        {
            updateInfo?.Invoke(msg, iType, async);
        }
        public void InitialObject(UTIL.DelegateUpdateInfo fxUpdateInfo)
        {
            this.updateInfo = fxUpdateInfo;
        }

        public enum eventStatus
        {
            before, after, any
        }

        public delegate void RobotEventHandler<TEventArgs>(object sender, Robot_base connection, TEventArgs e, eventStatus status);
        public delegate void RobotEventHandler(object sender, Robot_base connection, eventStatus status);

        public event RobotEventHandler OnConnect;
        public event RobotEventHandler OnDisconnect;
        public event RobotEventHandler OnClose;
        public event RobotEventHandler<byte[]> OnDataSend;
        public event RobotEventHandler<byte[]> OnDataReceived;

        private List<byte> rxBuffer = new List<byte>();
        private Semaphore rxToken = new Semaphore(initialCount: 1, maximumCount: 1);

        public void ClearRxBuffer()
        {
            rxToken.WaitOne();
            rxBuffer.Clear();
            rxToken.Release();
        }

        protected void CallOnConnect(Robot_base client, eventStatus status = eventStatus.any)
        {
            OnConnect?.Invoke(this, client, status);
        }
        protected void CallOnDisconnect(Robot_base client, eventStatus status = eventStatus.any)
        {
            OnDisconnect?.Invoke(this, client, status);
        }
        protected void CallOnClose(Robot_base client, eventStatus status = eventStatus.any)
        {
            OnClose?.Invoke(this, client, status);
        }
        protected void CallOnDataReceived(Robot_base connection, byte[] data, eventStatus status = eventStatus.any)
        {
            OnDataReceived?.Invoke(this, connection, data, status);
        }
        protected void CallOnDataSend(Robot_base connection, byte[] data, eventStatus status = eventStatus.any)
        {
            OnDataSend?.Invoke(this, connection, data, status);
        }

        protected string connTarget = null;
        public string target { get { return connTarget; } }
        public abstract bool isConnected { get; }
        public abstract bool Connect(RobotConnParm parm);
        public abstract bool Disconnect();
        public abstract void Close();

        public virtual int Available
        {
            get { return rxBuffer.Count; }
        }

        protected void AddRxData(byte data)
        {
            rxToken.WaitOne();
            rxBuffer.Add(data);
            rxToken.Release();
        }

        protected void AddRxData(byte[] data)
        {
            rxToken.WaitOne();
            rxBuffer.AddRange(data);
            rxToken.Release();
        }

        public virtual byte Read()
        {
            if (Available == 0) return 0;
            rxToken.WaitOne();
            byte data = rxBuffer[0];
            rxBuffer.RemoveAt(0);
            rxToken.Release();
            return data;
        }

        public virtual byte[] ReadAll(bool clearAfterRead = true)
        {
            rxToken.WaitOne();
            byte[] data = rxBuffer.ToArray();
            if (clearAfterRead) rxBuffer.Clear();
            rxToken.Release();
            return data;
        }

        public abstract void Send(byte[] data, int offset, int count);

        public virtual long WaitForData(long minBytes, long maxMs, out long cmdEndTicks)
        {
            cmdEndTicks = 0;
            // Wait for at least 1 bytes
            if (minBytes < 1) minBytes = 1;
            // at least wait for 1 ms, but not more than 10s
            if (maxMs < 1) maxMs = 1;
            if (maxMs > MAX_WAIT_MS) maxMs = MAX_WAIT_MS;

            long startTicks = DateTime.Now.Ticks;
            long endTicks = DateTime.Now.Ticks + maxMs * TimeSpan.TicksPerMillisecond;
            while (DateTime.Now.Ticks < endTicks)
            {
                // Always give extra 1ms to retrieve data
                if (rxBuffer.Count > 0) System.Threading.Thread.Sleep(1);

                if (rxBuffer.Count >= minBytes)
                {
                    cmdEndTicks = DateTime.Now.Ticks;
                    break;
                }
                System.Threading.Thread.Sleep(1);
            }
            return rxBuffer.Count;
        }

    }
}
